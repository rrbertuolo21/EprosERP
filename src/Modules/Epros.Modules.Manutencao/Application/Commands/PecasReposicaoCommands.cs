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
    // ===== Criar registro de pecas =====
    public record CriarRegistroPecaReposicaoCommand(
        string Codigo,
        string Descricao,
        Guid ResponsavelId,
        Guid? OrdemServicoId,
        Guid? PlanoPreventivoId) : ICommand;

    public class CriarRegistroPecaReposicaoCommandValidator : AbstractValidator<CriarRegistroPecaReposicaoCommand>
    {
        public CriarRegistroPecaReposicaoCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().MaximumLength(30);
            RuleFor(c => c.Descricao).NotEmpty().MaximumLength(500);
            RuleFor(c => c.ResponsavelId).NotEmpty();
        }
    }

    public class CriarRegistroPecaReposicaoCommandHandler : ICommandHandler<CriarRegistroPecaReposicaoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarRegistroPecaReposicaoCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarRegistroPecaReposicaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var existe = await _context.RegistrosPecaReposicao.AnyAsync(r => r.Codigo == request.Codigo, cancellationToken);
            if (existe)
                return CommandResult.Falha("Ja existe registro de pecas com este codigo para a empresa/tenant.");

            var registro = new RegistroPecaReposicao(request.Codigo, request.Descricao, request.ResponsavelId, request.OrdemServicoId, request.PlanoPreventivoId, tenantId, usuario);
            if (!registro.IsValid)
                return CommandResult.Falha(registro.Notifications.Select(n => n.Message));

            _context.RegistrosPecaReposicao.Add(registro);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Registro de pecas de reposicao criado.", new { registro.Id });
        }
    }

    // ===== Adicionar item de peca =====
    public record AdicionarItemPecaCommand(
        Guid RegistroId,
        Guid ProdutoId,
        int Sequencia,
        decimal Quantidade,
        Guid? OrdemServicoId,
        Guid? GradeId,
        decimal ValorUnitario,
        decimal TaxaDesconto,
        ETipoSaidaItem? TipoSaida) : ICommand;

    public class AdicionarItemPecaCommandHandler : ICommandHandler<AdicionarItemPecaCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarItemPecaCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarItemPecaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var registro = await _context.RegistrosPecaReposicao
                .Include(r => r.Itens)
                .FirstOrDefaultAsync(r => r.Id == request.RegistroId, cancellationToken);
            if (registro == null)
                return CommandResult.Falha("Registro de pecas nao encontrado.");

            var item = new ItemPecaReposicao(registro.Id, request.ProdutoId, request.Sequencia, request.Quantidade,
                request.OrdemServicoId, request.GradeId, request.ValorUnitario, request.TaxaDesconto, request.TipoSaida, tenantId, usuario);
            registro.AdicionarItem(item, usuario);
            if (!registro.IsValid)
                return CommandResult.Falha(registro.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Item de peca adicionado.", new { item.Id });
        }
    }

    // ===== Definir politica de reposicao =====
    public record DefinirPoliticaReposicaoCommand(
        Guid ProdutoId,
        decimal? EstoqueMinimo,
        decimal? EstoqueMaximo,
        decimal? PontoPedido,
        int? LeadTimeDias,
        string? Criticidade,
        Guid? GradeId,
        Guid? LocalEstoqueId) : ICommand;

    public class DefinirPoliticaReposicaoCommandHandler : ICommandHandler<DefinirPoliticaReposicaoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DefinirPoliticaReposicaoCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DefinirPoliticaReposicaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var politica = new PoliticaReposicao(request.ProdutoId, request.EstoqueMinimo, request.EstoqueMaximo, request.PontoPedido,
                request.LeadTimeDias, request.Criticidade, request.GradeId, request.LocalEstoqueId, tenantId, usuario);
            if (!politica.IsValid)
                return CommandResult.Falha(politica.Notifications.Select(n => n.Message));

            _context.PoliticasReposicao.Add(politica);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Politica de reposicao definida.", new { politica.Id });
        }
    }
}
