using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Manutencao.Domain.Entities;
using Epros.Modules.Manutencao.Domain.Enums;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Manutencao.Application.Commands
{
    // ===== Abrir OS =====
    public record AbrirOrdemServicoCommand(
        EPerfilOrdem PerfilOrdem,
        int TipoPessoa,
        Guid PessoaId,
        DateTime Data,
        bool Garantia,
        Guid? TipoAtendimentoId,
        Guid? TipoEquipamentoId,
        Guid? MarcaId,
        Guid? ColaboradorId,
        string? Numero) : ICommand;

    public class AbrirOrdemServicoCommandValidator : AbstractValidator<AbrirOrdemServicoCommand>
    {
        public AbrirOrdemServicoCommandValidator()
        {
            RuleFor(c => c.PessoaId).NotEmpty();
            RuleFor(c => c.ColaboradorId)
                .NotEmpty()
                .When(c => c.PerfilOrdem == EPerfilOrdem.Campo)
                .WithMessage("No perfil Campo o colaborador responsavel e obrigatorio.");
        }
    }

    public class AbrirOrdemServicoCommandHandler : ICommandHandler<AbrirOrdemServicoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AbrirOrdemServicoCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AbrirOrdemServicoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var os = new OrdemServico(request.PerfilOrdem, request.TipoPessoa, request.PessoaId, request.Data, request.Garantia,
                request.TipoAtendimentoId, request.TipoEquipamentoId, request.MarcaId, request.ColaboradorId, request.Numero, tenantId, usuario);
            if (!os.IsValid)
                return CommandResult.Falha(os.Notifications.Select(n => n.Message));

            _context.OrdensServico.Add(os);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Ordem de servico aberta com sucesso.", new { os.Id });
        }
    }

    // ===== Adicionar item a OS =====
    public record AdicionarItemOrdemServicoCommand(
        Guid OrdemServicoId,
        Guid? ProdutoId,
        ETipoItemOrdemServico? Tipo,
        string? Complemento,
        decimal Quantidade,
        decimal ValorUnitario,
        decimal TaxaDesconto,
        ETipoSaidaItem? TipoSaida,
        Guid? GradeId) : ICommand;

    public class AdicionarItemOrdemServicoCommandHandler : ICommandHandler<AdicionarItemOrdemServicoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarItemOrdemServicoCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarItemOrdemServicoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var os = await _context.OrdensServico
                .Include(o => o.Itens)
                .FirstOrDefaultAsync(o => o.Id == request.OrdemServicoId, cancellationToken);
            if (os == null)
                return CommandResult.Falha("Ordem de servico nao encontrada.");

            var item = new OrdemServicoItem(os.Id, request.ProdutoId, request.Tipo, request.Complemento, request.Quantidade,
                request.ValorUnitario, request.TaxaDesconto, request.TipoSaida, request.GradeId, tenantId, usuario);
            os.AdicionarItem(item, usuario);
            if (!os.IsValid)
                return CommandResult.Falha(os.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Item adicionado a OS.", new { item.Id, item.ValorTotal });
        }
    }

    // ===== Transicionar status da OS =====
    public record TransicionarStatusOrdemServicoCommand(Guid OrdemServicoId, EStatusOrdemServico NovoStatus) : ICommand;

    public class TransicionarStatusOrdemServicoCommandHandler : ICommandHandler<TransicionarStatusOrdemServicoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ICurrentUser _currentUser;

        public TransicionarStatusOrdemServicoCommandHandler(ContextManutencao context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(TransicionarStatusOrdemServicoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var os = await _context.OrdensServico.FirstOrDefaultAsync(o => o.Id == request.OrdemServicoId, cancellationToken);
            if (os == null)
                return CommandResult.Falha("Ordem de servico nao encontrada.");

            os.TransicionarStatus(request.NovoStatus, usuario);
            if (!os.IsValid)
                return CommandResult.Falha(os.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Status da OS atualizado.", new { os.Id, Status = os.StatusCodigo.ToString() });
        }
    }
}
