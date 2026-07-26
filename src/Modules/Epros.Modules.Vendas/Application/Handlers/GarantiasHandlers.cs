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
    public class CriarGarantiaPoliticaCommandHandler : ICommandHandler<CriarGarantiaPoliticaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarGarantiaPoliticaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarGarantiaPoliticaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var politica = new GarantiaPolitica(request.Nome, request.Descricao, request.Duracao, request.TipoDuracao, tenantId, usuario);
            if (!politica.IsValid) return CommandResult.Falha(politica.Notifications.Select(n => n.Message), "Dados da política de garantia inválidos.");
            _context.GarantiaPoliticas.Add(politica);
            _context.GarantiaHistoricos.Add(new GarantiaHistorico(EGarantiaEntidadeTipo.Politica, politica.Id, EGarantiaEvento.Criacao, null, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Política de garantia criada com sucesso!", new { politica.Id });
        }
    }

    public class AtualizarGarantiaPoliticaCommandHandler : ICommandHandler<AtualizarGarantiaPoliticaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtualizarGarantiaPoliticaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarGarantiaPoliticaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var politica = await _context.GarantiaPoliticas.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.Id, cancellationToken);
            if (politica == null) return CommandResult.Falha("Política de garantia não encontrada.");
            politica.Alterar(request.Nome, request.Descricao, request.Duracao, request.TipoDuracao, usuario);
            if (!politica.IsValid) return CommandResult.Falha(politica.Notifications.Select(n => n.Message), "Dados da política de garantia inválidos.");
            _context.GarantiaHistoricos.Add(new GarantiaHistorico(EGarantiaEntidadeTipo.Politica, politica.Id, EGarantiaEvento.Edicao, null, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Política de garantia atualizada.", new { politica.Id });
        }
    }

    public class InativarGarantiaPoliticaCommandHandler : ICommandHandler<InativarGarantiaPoliticaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public InativarGarantiaPoliticaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(InativarGarantiaPoliticaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var politica = await _context.GarantiaPoliticas.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.Id, cancellationToken);
            if (politica == null) return CommandResult.Falha("Política de garantia não encontrada.");
            politica.Inativar(usuario);
            _context.GarantiaHistoricos.Add(new GarantiaHistorico(EGarantiaEntidadeTipo.Politica, politica.Id, EGarantiaEvento.Inativacao, null, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Política de garantia inativada.", new { politica.Id });
        }
    }

    public class AplicarGarantiaCoberturaCommandHandler : ICommandHandler<AplicarGarantiaCoberturaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AplicarGarantiaCoberturaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AplicarGarantiaCoberturaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var politica = await _context.GarantiaPoliticas.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.GarantiaPoliticaId, cancellationToken);
            if (politica == null) return CommandResult.Falha("Política de garantia não encontrada.");

            // GAR-016/GAR-017: vencimento calculado a partir da duração da política e da data de origem.
            var cobertura = new GarantiaCobertura(
                request.GarantiaPoliticaId, request.VendaId, request.VendaItemId, request.ProdutoId, request.ClienteId,
                request.NumeroSerieLote, request.DataOrigem, request.Observacao, politica.Duracao, politica.TipoDuracao, tenantId, usuario);
            if (!cobertura.IsValid) return CommandResult.Falha(cobertura.Notifications.Select(n => n.Message), "Dados da cobertura inválidos.");
            _context.GarantiaCoberturas.Add(cobertura);
            _context.GarantiaHistoricos.Add(new GarantiaHistorico(EGarantiaEntidadeTipo.Cobertura, cobertura.Id, EGarantiaEvento.Aplicacao, null, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Garantia aplicada.", new { cobertura.Id, Situacao = cobertura.Situacao.ToString(), cobertura.DataVencimento });
        }
    }
}
