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

        public MotorMovimentacaoEstoque(ContextEstoque context, string tenantId, string usuario)
        {
            _context = context;
            _tenantId = tenantId;
            _usuario = usuario;
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

            saldo.SomarQuantidadeSaldoEstoque(quantidade);
            saldo.AtualizarValorSaldo(saldo.ValorSaldo + (valorUnitario * quantidade));
            saldo.AtualizarValorCustoMedio();

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

            var saldo = await _context.EstoqueProdutos
                .FirstOrDefaultAsync(e => e.EmpresaId == empresaId && e.ProdutoId == produtoId, ct);

            // MVM-012: produto sem saldo.
            if (saldo == null)
                return ResultadoMovimentacao.Falha("Produto sem cadastro de saldo de estoque (não localizado).");

            // MVM-009 / VAL-MVM-008: saldo insuficiente.
            if (saldo.QuantidadeSaldoEstoque < quantidade)
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
            decimal valorBaixado = 0m;

            foreach (var ficha in fichas)
            {
                if (restante <= 0m) break;

                var consumir = Math.Min(ficha.QuantidadeSaldo, restante);

                var fichaSaida = new ProdutoFichaEstoqueSaida(empresaId, produtoId, fatoGeradorId, ficha.Id, consumir, ficha.ValorUnitario, custoMedioSnapshot, _tenantId, _usuario, localId);
                _context.ProdutoFichaEstoqueSaidas.Add(fichaSaida);

                ficha.AtualizarQuantidadeSaldo(consumir);

                valorBaixado += ficha.ValorUnitario * consumir;
                restante -= consumir;
            }

            if (restante > 0m)
                return ResultadoMovimentacao.Falha("Saldo em fichas insuficiente para a saída (kardex divergente do saldo agregado).");

            saldo.DiminuirQuantidadeSaldoEstoque(quantidade);
            saldo.AtualizarValorSaldo(saldo.ValorSaldo - valorBaixado);
            if (saldo.ValorSaldo < 0m) saldo.AtualizarValorSaldo(0m);
            saldo.AtualizarValorCustoMedio();

            return ResultadoMovimentacao.Ok();
        }
    }
}
