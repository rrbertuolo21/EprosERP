using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Services
{
    /// <summary>Resultado de uma operação do serviço de separação WMS.</summary>
    public sealed class ResultadoSeparacao
    {
        public bool Sucesso { get; }
        public string? Erro { get; }
        public WmsTarefaSeparacao? Tarefa { get; }
        private ResultadoSeparacao(bool sucesso, string? erro, WmsTarefaSeparacao? tarefa) { Sucesso = sucesso; Erro = erro; Tarefa = tarefa; }
        public static ResultadoSeparacao Ok(WmsTarefaSeparacao t) => new(true, null, t);
        public static ResultadoSeparacao Falha(string erro) => new(false, erro, null);
    }

    /// <summary>
    /// D5 (WMS operacional) — orquestra a tarefa de separação MOVENDO o grão fino (EstoqueSaldoLocal),
    /// na mesma transação (não chama SaveChanges; o chamador controla a UoW). Movimento INTERNO de armazém:
    /// muda apenas a posição física; o saldo AGREGADO do produto na empresa permanece constante — por isso
    /// não toca EstoqueProduto e não regride a suíte.
    ///
    /// - Criar: valida saldo DISPONÍVEL (saldo − reservado) na posição de origem e RESERVA a quantidade.
    /// - Conferir: libera a reserva, BAIXA a posição de origem e, se houver destino, CREDITA a posição de destino
    ///   (mesmo produto/lote/série) — a soma do grão do produto não muda.
    /// - Cancelar: libera a reserva.
    /// </summary>
    public class WmsSeparacaoService
    {
        private readonly ContextEstoque _context;
        private readonly string _tenantId;
        private readonly string _usuario;

        public WmsSeparacaoService(ContextEstoque context, string tenantId, string usuario)
        {
            _context = context;
            _tenantId = tenantId;
            _usuario = usuario;
        }

        public async Task<ResultadoSeparacao> CriarTarefaAsync(
            Guid armazemId, Guid empresaId, Guid produtoId, Guid localOrigemId, Guid? localDestinoId,
            string? codigoLote, string? numeroSerie, decimal quantidade, string? observacao, CancellationToken ct)
        {
            var lote = EstoqueSaldoLocal.Normalizar(codigoLote);
            var serie = EstoqueSaldoLocal.Normalizar(numeroSerie);

            var origem = await _context.EstoqueSaldosLocais.FirstOrDefaultAsync(
                s => s.EmpresaId == empresaId && s.ProdutoId == produtoId && s.LocalId == localOrigemId
                     && s.CodigoLote == lote && s.NumeroSerie == serie, ct);

            if (origem == null || origem.QuantidadeDisponivel() < quantidade)
                return ResultadoSeparacao.Falha("Saldo disponível insuficiente na posição de origem para a separação.");

            var tarefa = new WmsTarefaSeparacao(armazemId, empresaId, produtoId, localOrigemId, localDestinoId, lote, serie, quantidade, observacao, _tenantId, _usuario);
            if (!tarefa.IsValid)
                return ResultadoSeparacao.Falha(string.Join("; ", System.Linq.Enumerable.Select(tarefa.Notifications, n => n.Message)));

            origem.SomarReservado(quantidade, _usuario); // trava o saldo até a conferência
            _context.WmsTarefasSeparacao.Add(tarefa);
            return ResultadoSeparacao.Ok(tarefa);
        }

        public async Task<ResultadoSeparacao> ConferirTarefaAsync(Guid tarefaId, decimal quantidadeConferida, CancellationToken ct)
        {
            var tarefa = await _context.WmsTarefasSeparacao.FirstOrDefaultAsync(t => t.Id == tarefaId, ct);
            if (tarefa == null)
                return ResultadoSeparacao.Falha("Tarefa de separação não encontrada.");

            var origem = await _context.EstoqueSaldosLocais.FirstOrDefaultAsync(
                s => s.EmpresaId == tarefa.EmpresaId && s.ProdutoId == tarefa.ProdutoId && s.LocalId == tarefa.LocalOrigemId
                     && s.CodigoLote == tarefa.CodigoLote && s.NumeroSerie == tarefa.NumeroSerie, ct);
            if (origem == null)
                return ResultadoSeparacao.Falha("Posição de origem não encontrada para conferência.");

            // A conferência não pode exceder o solicitado (regra na entidade); libera a reserva integral do
            // solicitado e baixa a origem pela quantidade conferida, valorizando pelo custo médio da posição.
            var custoUnit = origem.ValorCustoMedio;
            origem.DiminuirReservado(tarefa.QuantidadeSolicitada, _usuario);
            origem.Debitar(quantidadeConferida, quantidadeConferida * custoUnit, _usuario);

            // Transferência interna: credita a posição de destino (mesmo produto/lote/série), mantendo a soma.
            if (tarefa.LocalDestinoId.HasValue)
            {
                var destino = await _context.EstoqueSaldosLocais.FirstOrDefaultAsync(
                    s => s.EmpresaId == tarefa.EmpresaId && s.ProdutoId == tarefa.ProdutoId && s.LocalId == tarefa.LocalDestinoId.Value
                         && s.CodigoLote == tarefa.CodigoLote && s.NumeroSerie == tarefa.NumeroSerie, ct);
                if (destino == null)
                {
                    destino = new EstoqueSaldoLocal(tarefa.EmpresaId, tarefa.ProdutoId, tarefa.LocalDestinoId.Value, tarefa.CodigoLote, tarefa.NumeroSerie, origem.DataValidade, _tenantId, _usuario);
                    _context.EstoqueSaldosLocais.Add(destino);
                }
                destino.Creditar(quantidadeConferida, quantidadeConferida * custoUnit, origem.DataValidade, _usuario);
            }

            tarefa.MarcarConferida(quantidadeConferida, _usuario);
            return ResultadoSeparacao.Ok(tarefa);
        }

        public async Task<ResultadoSeparacao> CancelarTarefaAsync(Guid tarefaId, CancellationToken ct)
        {
            var tarefa = await _context.WmsTarefasSeparacao.FirstOrDefaultAsync(t => t.Id == tarefaId, ct);
            if (tarefa == null)
                return ResultadoSeparacao.Falha("Tarefa de separação não encontrada.");

            var origem = await _context.EstoqueSaldosLocais.FirstOrDefaultAsync(
                s => s.EmpresaId == tarefa.EmpresaId && s.ProdutoId == tarefa.ProdutoId && s.LocalId == tarefa.LocalOrigemId
                     && s.CodigoLote == tarefa.CodigoLote && s.NumeroSerie == tarefa.NumeroSerie, ct);
            origem?.DiminuirReservado(tarefa.QuantidadeSolicitada, _usuario); // devolve a reserva

            tarefa.Cancelar(_usuario);
            return ResultadoSeparacao.Ok(tarefa);
        }
    }
}
