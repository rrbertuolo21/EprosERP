using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Services
{
    /// <summary>Resultado de uma operação do motor de movimentação de estoque.</summary>
    public sealed class ResultadoMovimentacao
    {
        public bool Sucesso { get; private set; }
        public string? Erro { get; private set; }
        private ResultadoMovimentacao(bool sucesso, string? erro) { Sucesso = sucesso; Erro = erro; }
        public static ResultadoMovimentacao Ok() => new(true, null);
        public static ResultadoMovimentacao Falha(string erro) => new(false, erro);
    }

    /// <summary>
    /// Motor de valorização e ficha de estoque (kardex). Orquestra, em uma unidade de trabalho,
    /// fato gerador → ficha (entrada/saída) → saldo agregado → custo médio, aplicando o custeio
    /// PEPS/UEPS/CustoMédio configurado no produto.
    /// Implementa MVM-006/007/008/009/013/014/015/016/017/018 da EF Movimentação Manual e Ajustes.
    /// Não chama SaveChanges: o handler chamador controla a transação única (MVM-029).
    /// </summary>
    public class MotorMovimentacaoEstoque
    {
        private readonly ContextEstoque _context;
        private readonly string _tenantId;
        private readonly string _usuario;

        /// <summary>
        /// D1/D2 (ESTOQUE EST01): empresa "não segregada" — bucket default do tenant usado pelos fluxos que
        /// ainda não carregam empresa (compra, venda, produção, manutenção, qualidade). A chave de saldo é
        /// (EmpresaId+ProdutoId); a granularidade real por empresa+local+lote/série entra na fatia D2.
        /// </summary>
        public static readonly Guid EmpresaPadrao = Guid.Empty;

        public MotorMovimentacaoEstoque(ContextEstoque context, string tenantId, string usuario)
        {
            _context = context;
            _tenantId = tenantId;
            _usuario = usuario;
        }

        /// <summary>
        /// Mantém o ESPELHO denormalizado do produto (Produto.SaldoEstoque/CustoMedio) alinhado ao saldo
        /// verdadeiro do kardex. É o ÚNICO ponto que grava esses campos (D1). Usa IgnoreQueryFilters porque
        /// ProdutoId é GUID único global — não há risco de vazamento entre tenants.
        /// </summary>
        private async Task SincronizarEspelhoProdutoAsync(EstoqueProduto saldo, CancellationToken ct)
        {
            // FindAsync resolve pelo identity map (inclui entidades Added ainda não salvas, ex.: produto
            // recém-criado a partir da NF) antes de consultar o banco; chave é GUID único global.
            var produto = await _context.Produtos.FindAsync(new object[] { saldo.ProdutoId }, ct);
            produto?.SincronizarSaldoDenormalizado(saldo.QuantidadeSaldoEstoque, saldo.ValorCustoMedio, _usuario);
        }

        /// <summary>
        /// D2 — localiza (ou cria) a linha de saldo no GRÃO FINO (Empresa+Produto+Local+Lote+Série). Dimensões
        /// não informadas caem no bucket determinístico (Local = Guid.Empty, Lote/Série = "") para manter a
        /// unicidade sob PostgreSQL. Vive em paralelo ao agregado — o agregado continua a verdade da suíte.
        /// </summary>
        private async Task<EstoqueSaldoLocal> ObterOuCriarGrainAsync(
            Guid empresaId, Guid produtoId, Guid localBucket, string codigoLote, string numeroSerie,
            DateTime? dataValidade, CancellationToken ct)
        {
            var grain = await _context.EstoqueSaldosLocais.FirstOrDefaultAsync(
                s => s.EmpresaId == empresaId && s.ProdutoId == produtoId && s.LocalId == localBucket
                     && s.CodigoLote == codigoLote && s.NumeroSerie == numeroSerie, ct);

            if (grain == null)
            {
                grain = new EstoqueSaldoLocal(empresaId, produtoId, localBucket, codigoLote, numeroSerie, dataValidade, _tenantId, _usuario);
                _context.EstoqueSaldosLocais.Add(grain);
            }
            return grain;
        }

        /// <summary>
        /// D2 — espelha a ENTRADA no grão fino, na MESMA transação do agregado. Aditivo: nunca afeta o saldo
        /// agregado que a suíte valida; apenas materializa a posição física por local/lote/série.
        /// </summary>
        private async Task EspelharEntradaGrainAsync(
            Guid empresaId, Guid produtoId, Guid? localId, string? lote, string? numeroSerie,
            decimal quantidade, decimal valorUnitario, DateTime? dataValidade, CancellationToken ct)
        {
            var grain = await ObterOuCriarGrainAsync(
                empresaId, produtoId, localId ?? Guid.Empty,
                EstoqueSaldoLocal.Normalizar(lote), EstoqueSaldoLocal.Normalizar(numeroSerie), dataValidade, ct);
            grain.Creditar(quantidade, valorUnitario * quantidade, dataValidade, _usuario);
        }

        /// <summary>
        /// D2 — espelha a SAÍDA no grão fino: consome as posições do local (as informadas, senão o bucket
        /// padrão) do mais antigo para o mais novo (PEPS por posição; FEFO real por validade entra na fatia 3),
        /// valorizando pelo custo médio vigente do agregado. A QUANTIDADE sempre reconcilia com o agregado
        /// (baixa-se exatamente `quantidade`); eventual lacuna de backfill do grão é aplicada ao bucket base
        /// do local, podendo ficar negativa — condição transitória documentada até o backfill completo.
        /// </summary>
        private async Task EspelharSaidaGrainAsync(
            Guid empresaId, Guid produtoId, Guid? localId, decimal quantidade, decimal custoMedioSnapshot, CancellationToken ct)
        {
            var localBucket = localId ?? Guid.Empty;

            var linhas = await _context.EstoqueSaldosLocais
                .Where(s => s.EmpresaId == empresaId && s.ProdutoId == produtoId && s.LocalId == localBucket && s.QuantidadeSaldo > 0m)
                .OrderBy(s => s.CriadoEm).ThenBy(s => s.SyncVersion)
                .ToListAsync(ct);

            var restante = quantidade;
            foreach (var linha in linhas)
            {
                if (restante <= 0m) break;
                var consumir = Math.Min(linha.QuantidadeSaldo, restante);
                linha.Debitar(consumir, consumir * custoMedioSnapshot, _usuario);
                restante -= consumir;
            }

            // Lacuna de backfill (grão ainda não materializado para esse produto/local): aplica o restante no
            // bucket base do local para que a soma do grão continue reconciliando com o delta do agregado.
            if (restante > 0m)
            {
                var baseLinha = await ObterOuCriarGrainAsync(empresaId, produtoId, localBucket, string.Empty, string.Empty, null, ct);
                baseLinha.Debitar(restante, restante * custoMedioSnapshot, _usuario);
            }
        }

        /// <summary>Localiza (ou cria) o saldo agregado do produto na empresa. MVM-010: unicidade.</summary>
        private async Task<EstoqueProduto> ObterOuCriarSaldoAsync(Guid empresaId, Guid produtoId, ETipoCusteioEstoque custeioPadrao, CancellationToken ct)
        {
            var saldo = await _context.EstoqueProdutos
                .FirstOrDefaultAsync(e => e.EmpresaId == empresaId && e.ProdutoId == produtoId, ct);

            if (saldo == null)
            {
                saldo = new EstoqueProduto(empresaId, produtoId, 0m, 0m, 0m, 0m, 0m, custeioPadrao, _tenantId, _usuario);
                _context.EstoqueProdutos.Add(saldo);
            }
            return saldo;
        }

        /// <summary>
        /// Aplica uma ENTRADA: cria ficha de entrada, soma saldo agregado e recalcula custo médio.
        /// MVM-007, MVM-016, MVM-018.
        /// </summary>
        public async Task<ResultadoMovimentacao> AplicarEntradaAsync(
            Guid empresaId, Guid produtoId, ETipoEstoque tipoEstoque, decimal quantidade, decimal valorUnitario,
            Guid fatoGeradorId, Guid? localId, string? lote, DateTime? dataValidade, ETipoCusteioEstoque custeioPadrao,
            CancellationToken ct)
        {
            if (quantidade <= 0)
                return ResultadoMovimentacao.Falha("A quantidade de entrada deve ser maior que zero.");

            var saldo = await ObterOuCriarSaldoAsync(empresaId, produtoId, custeioPadrao, ct);

            var ficha = new ProdutoFichaEstoqueEntrada(empresaId, produtoId, fatoGeradorId, tipoEstoque, quantidade, valorUnitario, _tenantId, _usuario, localId, lote, dataValidade);
            _context.ProdutoFichaEstoqueEntradas.Add(ficha);

            // D4/D13: custo médio ponderado MÓVEL — cada entrada recalcula o custo médio; entrada sobre
            // saldo zero assume o custo desta entrada (ValorSaldo/Quantidade). Nunca divide por zero.
            saldo.SomarQuantidadeSaldoEstoque(quantidade);
            saldo.AtualizarValorSaldo(saldo.ValorSaldo + (valorUnitario * quantidade));
            saldo.AtualizarValorCustoMedio();

            // D2: espelha a entrada no grão fino (Local+Lote/Série), na mesma transação. Aditivo.
            await EspelharEntradaGrainAsync(empresaId, produtoId, localId, lote, null, quantidade, valorUnitario, dataValidade, ct);

            await SincronizarEspelhoProdutoAsync(saldo, ct);

            return ResultadoMovimentacao.Ok();
        }

        /// <summary>
        /// Aplica uma SAÍDA: valida saldo (MVM-009), seleciona fichas de entrada por custeio
        /// (PEPS/UEPS/CustoMédio), gera fichas de saída vinculadas (MVM-008), baixa o saldo e recalcula custo.
        /// MVM-013/014/015/017.
        /// </summary>
        public async Task<ResultadoMovimentacao> AplicarSaidaAsync(
            Guid empresaId, Guid produtoId, decimal quantidade, Guid fatoGeradorId, Guid? localId, CancellationToken ct)
        {
            if (quantidade <= 0)
                return ResultadoMovimentacao.Falha("A quantidade de saída deve ser maior que zero.");

            // D8: política de estoque negativo do produto (default false = bloqueia). FindAsync resolve pelo
            // identity map (inclui Added) antes do banco; chave é GUID único global.
            var produto = await _context.Produtos.FindAsync(new object[] { produtoId }, ct);
            var permiteNegativo = produto?.PermiteEstoqueNegativo ?? false;

            var saldo = await _context.EstoqueProdutos
                .FirstOrDefaultAsync(e => e.EmpresaId == empresaId && e.ProdutoId == produtoId, ct);

            // MVM-012: produto sem saldo. D8: só é permitido "furar" para negativo se o produto autorizar.
            if (saldo == null)
            {
                if (!permiteNegativo)
                    return ResultadoMovimentacao.Falha("Saldo insuficiente: produto sem saldo de estoque e sem permissão de estoque negativo.");
                saldo = new EstoqueProduto(empresaId, produtoId, 0m, 0m, 0m, 0m, 0m, ETipoCusteioEstoque.CustoMedio, _tenantId, _usuario);
                _context.EstoqueProdutos.Add(saldo);
            }

            // MVM-009 / VAL-MVM-008 + D8: saldo insuficiente bloqueia por padrão; libera se permite_estoque_negativo.
            if (saldo.QuantidadeSaldoEstoque < quantidade && !permiteNegativo)
                return ResultadoMovimentacao.Falha("Saldo insuficiente para a saída solicitada.");

            // Fichas de entrada com saldo remanescente, ordenadas pelo custeio do produto.
            var fichasQuery = _context.ProdutoFichaEstoqueEntradas
                .Where(f => f.EmpresaId == empresaId && f.ProdutoId == produtoId && f.QuantidadeSaldo > 0m);

            fichasQuery = saldo.TipoCusteioEstoque == ETipoCusteioEstoque.UEPS
                ? fichasQuery.OrderByDescending(f => f.CriadoEm).ThenByDescending(f => f.SyncVersion)
                : fichasQuery.OrderBy(f => f.CriadoEm).ThenBy(f => f.SyncVersion); // PEPS e CustoMédio consomem camada mais antiga primeiro

            var fichas = await fichasQuery.ToListAsync(ct);

            var restante = quantidade;
            var custoMedioSnapshot = saldo.ValorCustoMedio;
            decimal valorBaixadoFichas = 0m;

            foreach (var ficha in fichas)
            {
                if (restante <= 0m) break;

                var consumir = Math.Min(ficha.QuantidadeSaldo, restante);

                var fichaSaida = new ProdutoFichaEstoqueSaida(empresaId, produtoId, fatoGeradorId, ficha.Id, consumir, ficha.ValorUnitario, custoMedioSnapshot, _tenantId, _usuario, localId);
                _context.ProdutoFichaEstoqueSaidas.Add(fichaSaida);

                ficha.AtualizarQuantidadeSaldo(consumir);

                valorBaixadoFichas += ficha.ValorUnitario * consumir;
                restante -= consumir;
            }

            // D8: quando o produto permite negativo e as fichas não cobrem tudo, a saída prossegue; o restante
            // não coberto por camadas de entrada é valorizado pelo custo médio vigente (sem ficha órfã).
            // TODO (D2/rastreabilidade): registrar a camada negativa quando houver granularidade local+lote/série.
            if (restante > 0m && !permiteNegativo)
                return ResultadoMovimentacao.Falha("Saldo em fichas insuficiente para a saída (kardex divergente do saldo agregado).");

            // D4: custeio da SAÍDA.
            // - Custo médio (default, valida-contador): baixa pelo custo médio vigente → a média não muda na saída.
            // - PEPS/UEPS (ganchos p/ fatia futura): baixa pelo custo real das camadas consumidas.
            // TODO (D15, valida-contador): contabilização do custo é DIFERIDA ao módulo Financeiro/Contábil;
            //       o kardex apenas registra o custo factual (snapshot em ProdutoFichaEstoqueSaida.ValorCustoMedio).
            decimal valorBaixado = saldo.TipoCusteioEstoque == ETipoCusteioEstoque.CustoMedio
                ? quantidade * custoMedioSnapshot
                : valorBaixadoFichas + (restante > 0m ? restante * custoMedioSnapshot : 0m);

            saldo.DiminuirQuantidadeSaldoEstoque(quantidade);
            saldo.AtualizarValorSaldo(saldo.ValorSaldo - valorBaixado);
            if (saldo.QuantidadeSaldoEstoque <= 0m) saldo.AtualizarValorSaldo(0m); // saldo zerado/negativo → valor zero (custo médio preservado por D13)
            saldo.AtualizarValorCustoMedio();

            // D2: espelha a saída no grão fino (consome posições do local), na mesma transação. Aditivo.
            await EspelharSaidaGrainAsync(empresaId, produtoId, localId, quantidade, custoMedioSnapshot, ct);

            await SincronizarEspelhoProdutoAsync(saldo, ct);

            return ResultadoMovimentacao.Ok();
        }
    }
}
