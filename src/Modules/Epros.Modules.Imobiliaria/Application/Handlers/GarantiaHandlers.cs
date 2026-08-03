using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Imobiliaria.Application.Commands;
using Epros.Modules.Imobiliaria.Domain.Entities;
using Epros.Modules.Imobiliaria.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Imobiliaria.Application.Handlers
{
    public class AdicionarGarantiaCommandHandler : ICommandHandler<AdicionarGarantiaCommand>
    {
        private readonly ContextImobiliaria _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarGarantiaCommandHandler(ContextImobiliaria context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AdicionarGarantiaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (!await _context.Locacoes.AnyAsync(l => l.Id == request.LocacaoId, cancellationToken))
                return CommandResult.Falha("Locacao nao encontrada.");

            var garantia = new LocacaoGarantia(
                request.LocacaoId, request.Tipo, request.ValorLimite, request.VigenciaInicio, request.VigenciaFim,
                request.Descricao, request.FiadorPessoaId, null, tenantId, usuario);
            if (!garantia.IsValid)
                return CommandResult.Falha(garantia.Notifications.Select(n => n.Message));

            _context.LocacaoGarantias.Add(garantia);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Imobiliaria.GarantiaRegistrada,
                JsonSerializer.Serialize(new { garantiaId = garantia.Id, locacaoId = garantia.LocacaoId, tipo = garantia.Tipo.ToString(), tenantId })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Garantia registrada com sucesso!", new { GarantiaId = garantia.Id });
        }
    }

    public class SubstituirGarantiaCommandHandler : ICommandHandler<SubstituirGarantiaCommand>
    {
        private readonly ContextImobiliaria _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public SubstituirGarantiaCommandHandler(ContextImobiliaria context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(SubstituirGarantiaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var anterior = await _context.LocacaoGarantias.FirstOrDefaultAsync(g => g.Id == request.GarantiaAnteriorId, cancellationToken);
            if (anterior is null)
                return CommandResult.Falha("Garantia anterior nao encontrada.");

            anterior.MarcarSubstituida(usuario);
            if (!anterior.IsValid)
                return CommandResult.Falha(anterior.Notifications.Select(n => n.Message));

            var nova = new LocacaoGarantia(
                anterior.LocacaoId, request.Tipo, request.ValorLimite, request.VigenciaInicio, request.VigenciaFim,
                request.Descricao, request.FiadorPessoaId, anterior.Id, tenantId, usuario);
            if (!nova.IsValid)
                return CommandResult.Falha(nova.Notifications.Select(n => n.Message));

            _context.LocacaoGarantias.Add(nova);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Garantia substituida com sucesso!",
                new { GarantiaAnteriorId = anterior.Id, GarantiaNovaId = nova.Id });
        }
    }

    public class LiberarGarantiaCommandHandler : ICommandHandler<LiberarGarantiaCommand>
    {
        private readonly ContextImobiliaria _context;
        private readonly ICurrentUser _currentUser;

        public LiberarGarantiaCommandHandler(ContextImobiliaria context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(LiberarGarantiaCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var garantia = await _context.LocacaoGarantias.FirstOrDefaultAsync(g => g.Id == request.GarantiaId, cancellationToken);
            if (garantia is null)
                return CommandResult.Falha("Garantia nao encontrada.");

            // NF-04: tratamento financeiro da caucao (retencao/correcao/devolucao) permanece off ate contador.
            garantia.Liberar(usuario);
            if (!garantia.IsValid)
                return CommandResult.Falha(garantia.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Garantia liberada com sucesso!", new { GarantiaId = garantia.Id, Status = garantia.Status.ToString() });
        }
    }
}
