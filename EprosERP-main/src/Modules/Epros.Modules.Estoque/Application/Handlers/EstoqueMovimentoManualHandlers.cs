using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Services;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>
    /// Cria e APLICA um movimento manual de estoque orquestrando, em transação única (MVM-029):
    /// validação de obrigatórios (MVM-001..005) → fato gerador (MVM-006) →
    /// entrada: ficha de entrada + saldo + custo médio (MVM-007/016/018) /
    /// saída: validação de saldo (MVM-009) + seleção PEPS/UEPS/médio + ficha de saída + baixa (MVM-008/013/014/015/017).
    /// </summary>
    public class CriarEstoqueMovimentoManualCommandHandler : ICommandHandler<CriarEstoqueMovimentoManualCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarEstoqueMovimentoManualCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarEstoqueMovimentoManualCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            // MVM-001..005: validações obrigatórias no domínio.
            var movimento = new EstoqueMovimentoManual(request.ProdutoId, request.TipoEstoque, request.TipoMovimento, request.QuantidadeMovimentada, request.ValorUnitario, tenantId, criadoPor, request.Motivo);
            if (!movimento.IsValid)
                return CommandResult.Falha(movimento.Notifications.Select(n => n.Message), "Dados do movimento manual são inválidos.");

            _context.EstoqueMovimentosManuais.Add(movimento);

            // MVM-006: fato gerador.
            var fato = new FatoGeradorEstoque(null, null, movimento.Id, EOrigemFatoGeradorEstoque.MovimentoManual, tenantId, criadoPor);
            _context.FatosGeradoresEstoque.Add(fato);

            var custeioPadrao = await _context.EstoqueProdutos
                .Where(e => e.EmpresaId == request.EmpresaId && e.ProdutoId == request.ProdutoId)
                .Select(e => (ETipoCusteioEstoque?)e.TipoCusteioEstoque)
                .FirstOrDefaultAsync(cancellationToken) ?? ETipoCusteioEstoque.CustoMedio;

            var motor = new MotorMovimentacaoEstoque(_context, tenantId, criadoPor);

            ResultadoMovimentacao resultado = request.TipoMovimento == ETipoMovimento.Entrada
                ? await motor.AplicarEntradaAsync(request.EmpresaId, request.ProdutoId, request.TipoEstoque, request.QuantidadeMovimentada, request.ValorUnitario, fato.Id, request.LocalId, request.Lote, request.DataValidade, custeioPadrao, cancellationToken)
                : await motor.AplicarSaidaAsync(request.EmpresaId, request.ProdutoId, request.QuantidadeMovimentada, fato.Id, request.LocalId, cancellationToken);

            if (!resultado.Sucesso)
                return CommandResult.Falha(resultado.Erro ?? "Falha ao aplicar o movimento.");

            movimento.Aplicar(criadoPor);

            RegistrarHistorico(_context, "EstoqueMovimentoManual", movimento.Id, "movimento_aplicado", "Rascunho", "Aplicado", request.Motivo, criadoPor, tenantId);
            PublicarEvento(_context, tenantId, "est.mvm.movimento_aplicado", new { movimento.Id, movimento.ProdutoId, movimento.TipoMovimento, movimento.QuantidadeMovimentada, fatoId = fato.Id });

            // MVM-029: gravação em transação única.
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Movimento manual de estoque aplicado com sucesso!", new { movimento.Id, fatoId = fato.Id });
        }

        internal static void RegistrarHistorico(ContextEstoque context, string entidade, Guid entidadeId, string evento, string? anterior, string? nova, string? motivo, string usuario, string tenantId)
        {
            context.HistoricosEstoque.Add(new HistoricoEstoque(entidade, entidadeId, evento, anterior, nova, motivo, usuario, tenantId, usuario));
        }

        internal static void PublicarEvento(ContextEstoque context, string tenantId, string eventType, object payload)
        {
            context.OutboxMessages.Add(new OutboxMessage(tenantId, eventType, JsonSerializer.Serialize(payload)));
        }
    }

    /// <summary>Atualiza os dados de um movimento manual ainda não aplicado.</summary>
    public class AtualizarEstoqueMovimentoManualCommandHandler : ICommandHandler<AtualizarEstoqueMovimentoManualCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarEstoqueMovimentoManualCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarEstoqueMovimentoManualCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var movimento = await _context.EstoqueMovimentosManuais.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            if (movimento == null)
                return CommandResult.Falha("Movimento manual de estoque não encontrado.");

            // MVM-030: movimento já aplicado não pode ser alterado in-place — exige estorno.
            if (movimento.Situacao == EStatusMovimentoEstoque.Aplicado)
                return CommandResult.Falha("Movimento já aplicado não pode ser alterado. Utilize o estorno controlado (MVM-030).");

            movimento.Alterar(request.ProdutoId, request.TipoEstoque, request.TipoMovimento, request.QuantidadeMovimentada, request.ValorUnitario, usuario, request.Motivo);
            if (!movimento.IsValid)
                return CommandResult.Falha(movimento.Notifications.Select(n => n.Message), "Dados do movimento manual são inválidos.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Movimento manual de estoque atualizado com sucesso!");
        }
    }

    /// <summary>Exclui (soft delete) um movimento manual ainda não aplicado.</summary>
    public class DeletarEstoqueMovimentoManualCommandHandler : ICommandHandler<DeletarEstoqueMovimentoManualCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public DeletarEstoqueMovimentoManualCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarEstoqueMovimentoManualCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var movimento = await _context.EstoqueMovimentosManuais.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            if (movimento == null)
                return CommandResult.Falha("Movimento manual de estoque não encontrado.");

            // MVM-030: movimento aplicado não pode ser excluído — exige estorno.
            if (movimento.Situacao == EStatusMovimentoEstoque.Aplicado)
                return CommandResult.Falha("Movimento já aplicado não pode ser excluído. Utilize o estorno controlado (MVM-030).");

            movimento.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Movimento manual de estoque excluído com sucesso!");
        }
    }

    /// <summary>
    /// Estorno controlado de movimento já aplicado (MVM-030). Gera movimento compensatório inverso
    /// (entrada↔saída) via motor, revertendo saldo/ficha/custo, e marca o original como Estornado.
    /// </summary>
    public class EstornarEstoqueMovimentoManualCommandHandler : ICommandHandler<EstornarEstoqueMovimentoManualCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public EstornarEstoqueMovimentoManualCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(EstornarEstoqueMovimentoManualCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (string.IsNullOrWhiteSpace(request.Motivo))
                return CommandResult.Falha("O motivo do estorno é obrigatório (MVM-030).");

            var movimento = await _context.EstoqueMovimentosManuais.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            if (movimento == null)
                return CommandResult.Falha("Movimento manual de estoque não encontrado.");

            if (movimento.Situacao != EStatusMovimentoEstoque.Aplicado)
                return CommandResult.Falha("Somente movimentos aplicados podem ser estornados.");

            // Fato gerador compensatório.
            var fato = new FatoGeradorEstoque(null, null, movimento.Id, EOrigemFatoGeradorEstoque.MovimentoManual, tenantId, usuario, referenciaExterna: $"Estorno do movimento {movimento.Id}");
            _context.FatosGeradoresEstoque.Add(fato);

            var custeioPadrao = await _context.EstoqueProdutos
                .Where(e => e.EmpresaId == request.EmpresaId && e.ProdutoId == movimento.ProdutoId)
                .Select(e => (ETipoCusteioEstoque?)e.TipoCusteioEstoque)
                .FirstOrDefaultAsync(cancellationToken) ?? ETipoCusteioEstoque.CustoMedio;

            var motor = new MotorMovimentacaoEstoque(_context, tenantId, usuario);

            // Movimento compensatório inverso (EF §11: efeito revertido por movimento compensatório).
            ResultadoMovimentacao resultado = movimento.TipoMovimento == ETipoMovimento.Entrada
                ? await motor.AplicarSaidaAsync(request.EmpresaId, movimento.ProdutoId, movimento.QuantidadeMovimentada, fato.Id, null, cancellationToken)
                : await motor.AplicarEntradaAsync(request.EmpresaId, movimento.ProdutoId, movimento.TipoEstoque, movimento.QuantidadeMovimentada, movimento.ValorUnitario, fato.Id, null, null, null, custeioPadrao, cancellationToken);

            if (!resultado.Sucesso)
                return CommandResult.Falha(resultado.Erro ?? "Falha ao estornar o movimento.");

            movimento.Estornar(usuario, request.Motivo);

            CriarEstoqueMovimentoManualCommandHandler.RegistrarHistorico(_context, "EstoqueMovimentoManual", movimento.Id, "movimento_estornado", "Aplicado", "Estornado", request.Motivo, usuario, tenantId);
            CriarEstoqueMovimentoManualCommandHandler.PublicarEvento(_context, tenantId, "est.mvm.movimento_estornado", new { movimento.Id, movimento.ProdutoId, request.Motivo, usuario });

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Movimento manual de estoque estornado com sucesso!", new { movimento.Id });
        }
    }
}
