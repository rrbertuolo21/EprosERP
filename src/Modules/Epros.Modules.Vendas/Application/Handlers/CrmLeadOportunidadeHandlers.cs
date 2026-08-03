using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Handlers
{
    public class CriarCrmLeadCommandHandler : ICommandHandler<CriarCrmLeadCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCrmLeadCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCrmLeadCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var lead = new CrmLead(
                request.PipelineId, request.EtapaId, request.Nome, request.PrimeiroNome, request.Sobrenome,
                request.Email, request.Telefone, request.Assunto, request.ValorEstimado, request.Status,
                request.OrigemId, request.Notas, request.CriadoPorUsuarioId, request.ResponsavelUsuarioId,
                request.CampanhaId, request.EnderecoIpCaptacao, tenantId, usuario);
            if (!lead.IsValid) return CommandResult.Falha(lead.Notifications.Select(n => n.Message), "Dados do lead inválidos.");
            _context.CrmLeads.Add(lead);
            // T-09/EC-05: efetiva a escrita de histórico (antes nunca gravado).
            _context.CrmHistoricos.Add(new CrmHistorico("Lead", lead.Id, "Criacao", null, null, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Lead criado com sucesso!", new { lead.Id, Status = lead.Status.ToString() });
        }
    }

    public class MoverCrmLeadEtapaCommandHandler : ICommandHandler<MoverCrmLeadEtapaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public MoverCrmLeadEtapaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(MoverCrmLeadEtapaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var lead = await _context.CrmLeads.FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == request.LeadId, cancellationToken);
            if (lead == null) return CommandResult.Falha("Lead não encontrado.");
            lead.MoverEtapa(request.EtapaId, request.PosicaoKanban, usuario);
            _context.CrmHistoricos.Add(new CrmHistorico("Lead", lead.Id, "MoverEtapa", null, null, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Lead movido de etapa.", new { lead.Id, Status = lead.Status.ToString() });
        }
    }

    public class ConverterCrmLeadCommandHandler : ICommandHandler<ConverterCrmLeadCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ConverterCrmLeadCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ConverterCrmLeadCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var lead = await _context.CrmLeads.FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == request.LeadId, cancellationToken);
            if (lead == null) return CommandResult.Falha("Lead não encontrado.");

            // CRM-011: verificação de duplicidade desconsidera leads já convertidos.
            if (lead.Convertido) return CommandResult.Falha("Lead já convertido.");

            Guid? oportunidadeId = null;
            // CRM-014: conversão pode criar oportunidade quando aplicável.
            if (request.CriarOportunidade)
            {
                if (lead.PipelineId == null || lead.EtapaId == null)
                    return CommandResult.Falha("Lead sem pipeline/etapa não pode gerar oportunidade automaticamente.");

                var oportunidade = new CrmOportunidade(
                    lead.PipelineId.Value, lead.EtapaId.Value,
                    string.IsNullOrWhiteSpace(request.NomeOportunidade) ? lead.Nome : request.NomeOportunidade!,
                    request.ValorOportunidade ?? lead.ValorEstimado,
                    lead.OrigemId, request.ClienteId, lead.Id, lead.Notas, null,
                    lead.CriadoPorUsuarioId, tenantId, usuario);
                if (!oportunidade.IsValid) return CommandResult.Falha(oportunidade.Notifications.Select(n => n.Message), "Não foi possível criar a oportunidade.");
                _context.CrmOportunidades.Add(oportunidade);
                oportunidadeId = oportunidade.Id;
            }

            // CRM-009: marca o lead como convertido e grava vínculos gerados.
            lead.Converter(request.ClienteId, request.ContatoId, oportunidadeId, usuario);
            _context.CrmHistoricos.Add(new CrmHistorico("Lead", lead.Id, "Conversao", null, null, null, null, tenantId, usuario));
            if (oportunidadeId.HasValue)
                _context.CrmHistoricos.Add(new CrmHistorico("Oportunidade", oportunidadeId.Value, "Criacao", null, null, null, "Criada por conversão de lead.", tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Lead convertido com sucesso!", new { lead.Id, OportunidadeId = oportunidadeId });
        }
    }

    public class ArquivarCrmLeadCommandHandler : ICommandHandler<ArquivarCrmLeadCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ArquivarCrmLeadCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ArquivarCrmLeadCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var lead = await _context.CrmLeads.FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == request.LeadId, cancellationToken);
            if (lead == null) return CommandResult.Falha("Lead não encontrado.");
            lead.Arquivar(usuario);
            _context.CrmHistoricos.Add(new CrmHistorico("Lead", lead.Id, "Arquivamento", null, null, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Lead arquivado.", new { lead.Id });
        }
    }

    public class CriarCrmOportunidadeCommandHandler : ICommandHandler<CriarCrmOportunidadeCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCrmOportunidadeCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCrmOportunidadeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var oportunidade = new CrmOportunidade(
                request.PipelineId, request.EtapaId, request.Nome, request.Valor, request.OrigemId,
                request.ClientePrincipalId, request.LeadOrigemId, request.Notas, request.DataFechamentoPrevista,
                request.CriadoPorUsuarioId, tenantId, usuario);
            if (!oportunidade.IsValid) return CommandResult.Falha(oportunidade.Notifications.Select(n => n.Message), "Dados da oportunidade inválidos.");
            _context.CrmOportunidades.Add(oportunidade);
            _context.CrmHistoricos.Add(new CrmHistorico("Oportunidade", oportunidade.Id, "Criacao", null, null, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Oportunidade criada com sucesso!", new { oportunidade.Id, Status = oportunidade.Status.ToString() });
        }
    }

    public class MoverCrmOportunidadeEtapaCommandHandler : ICommandHandler<MoverCrmOportunidadeEtapaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public MoverCrmOportunidadeEtapaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(MoverCrmOportunidadeEtapaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var op = await _context.CrmOportunidades.FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == request.OportunidadeId, cancellationToken);
            if (op == null) return CommandResult.Falha("Oportunidade não encontrada.");
            op.MoverEtapa(request.EtapaId, request.PosicaoKanban, usuario);
            _context.CrmHistoricos.Add(new CrmHistorico("Oportunidade", op.Id, "MoverEtapa", null, null, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Oportunidade movida de etapa.", new { op.Id });
        }
    }

    public class GanharCrmOportunidadeCommandHandler : ICommandHandler<GanharCrmOportunidadeCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public GanharCrmOportunidadeCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(GanharCrmOportunidadeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var op = await _context.CrmOportunidades.FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == request.OportunidadeId, cancellationToken);
            if (op == null) return CommandResult.Falha("Oportunidade não encontrada.");
            op.Ganhar(usuario);
            _context.CrmHistoricos.Add(new CrmHistorico("Oportunidade", op.Id, "Ganho", null, null, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            // CRM-026 / EF §6.6: oportunidade ganha pode acionar proposta/pedido/venda (integração futura via Outbox).
            return CommandResult.Ok("Oportunidade marcada como ganha.", new { op.Id, Status = op.Status.ToString() });
        }
    }

    public class PerderCrmOportunidadeCommandHandler : ICommandHandler<PerderCrmOportunidadeCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public PerderCrmOportunidadeCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(PerderCrmOportunidadeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var op = await _context.CrmOportunidades.FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == request.OportunidadeId, cancellationToken);
            if (op == null) return CommandResult.Falha("Oportunidade não encontrada.");
            op.Perder(request.MotivoPerda, usuario);
            _context.CrmHistoricos.Add(new CrmHistorico("Oportunidade", op.Id, "Perda", null, null, null, request.MotivoPerda, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Oportunidade marcada como perdida.", new { op.Id, Status = op.Status.ToString() });
        }
    }
}
