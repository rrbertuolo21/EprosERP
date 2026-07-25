using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.ESG.Application.Commands;
using Epros.Modules.ESG.Domain.Entities;
using Epros.Modules.ESG.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.ESG.Application.Handlers
{
    public class ImportarDevolucaoCommandHandler : ICommandHandler<ImportarDevolucaoCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ImportarDevolucaoCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ImportarDevolucaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // RN-ECO: evitar XML/documento duplicado quando chave existir.
            if (!string.IsNullOrWhiteSpace(request.ChaveNfEntrada))
            {
                var duplicada = await _context.DevolucoesEco.AnyAsync(d => d.ChaveNfEntrada == request.ChaveNfEntrada, cancellationToken);
                if (duplicada)
                    return CommandResult.Falha($"Ja existe devolucao com a chave de entrada {request.ChaveNfEntrada}.");
            }

            var devolucao = new DevolucaoEco(
                request.ContactId, request.NaturezaId, request.ValorIntegral, request.ValorDevolvido, request.Motivo,
                request.Observacao, request.Estado, request.DevolucaoParcial, request.ChaveNfEntrada, request.NumeroNf,
                request.ValorFrete, request.ValorDesconto, request.BusinessId, request.LocationId, request.Tipo,
                tenantId, usuario);

            if (!devolucao.IsValid)
                return CommandResult.Falha(devolucao.Notifications.Select(n => n.Message));

            _context.DevolucoesEco.Add(devolucao);

            if (request.Itens != null)
            {
                foreach (var i in request.Itens)
                {
                    var item = new DevolucaoItemEco(devolucao.Id, i.Codigo, i.Nome, i.Ncm, i.Cfop, i.ValorUnitario, i.Quantidade, i.ItemParcial, i.UnidadeMedida, tenantId, usuario);
                    if (!item.IsValid)
                        return CommandResult.Falha(item.Notifications.Select(n => n.Message));
                    _context.DevolucoesItensEco.Add(item);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Devolucao importada!", new { devolucao.Id, Itens = request.Itens?.Count ?? 0 });
        }
    }

    public class CriarFluxoCircularCommandHandler : ICommandHandler<CriarFluxoCircularCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarFluxoCircularCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarFluxoCircularCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var jaExiste = await _context.FluxosCirculares.AnyAsync(f => f.Codigo == request.Codigo, cancellationToken);
            if (jaExiste)
                return CommandResult.Falha($"Ja existe fluxo circular com codigo {request.Codigo}.");

            var fluxo = new FluxoCircular(request.Codigo, request.Descricao, request.Tipo, request.DevolucaoId, request.ResponsavelId, tenantId, usuario);
            if (!fluxo.IsValid)
                return CommandResult.Falha(fluxo.Notifications.Select(n => n.Message));

            _context.FluxosCirculares.Add(fluxo);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Fluxo circular criado!", new { fluxo.Id });
        }
    }

    public class RegistrarTriagemCommandHandler : ICommandHandler<RegistrarTriagemCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarTriagemCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarTriagemCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var fluxo = await _context.FluxosCirculares.AnyAsync(f => f.Id == request.FluxoId, cancellationToken);
            if (!fluxo)
                return CommandResult.Falha("Fluxo circular nao encontrado.");

            var triagem = new TriagemEco(request.FluxoId, request.ItemDevolucaoId, request.QuantidadeRecebida, request.Unidade, request.Condicao, request.DestinoProposto, request.Motivo, request.ResponsavelId, tenantId, usuario);
            if (!triagem.IsValid)
                return CommandResult.Falha(triagem.Notifications.Select(n => n.Message));

            _context.TriagensEco.Add(triagem);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Triagem registrada!", new { triagem.Id });
        }
    }

    public class RegistrarDestinoCommandHandler : ICommandHandler<RegistrarDestinoCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarDestinoCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarDestinoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var triagem = await _context.TriagensEco.FirstOrDefaultAsync(t => t.Id == request.TriagemId, cancellationToken);
            if (triagem == null)
                return CommandResult.Falha("Triagem nao encontrada.");

            // RN-ECO: destinos somados nao podem exceder a quantidade recebida na triagem.
            var jaDestinado = await _context.DestinosEco.Where(d => d.TriagemId == request.TriagemId).SumAsync(d => (decimal?)d.Quantidade, cancellationToken) ?? 0m;
            if (jaDestinado + request.Quantidade > triagem.QuantidadeRecebida)
                return CommandResult.Falha("A soma dos destinos excede a quantidade recebida na triagem.");

            var destino = new DestinoEco(request.TriagemId, request.TipoDestino, request.Quantidade, request.Unidade, request.DataExecucao, request.ResponsavelId, request.EvidenciaArquivoId, request.Observacao, tenantId, usuario);
            if (!destino.IsValid)
                return CommandResult.Falha(destino.Notifications.Select(n => n.Message));

            _context.DestinosEco.Add(destino);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Destino registrado!", new { destino.Id });
        }
    }

    public class DefinirMetaCircularCommandHandler : ICommandHandler<DefinirMetaCircularCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DefinirMetaCircularCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DefinirMetaCircularCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var fluxo = await _context.FluxosCirculares.AnyAsync(f => f.Id == request.FluxoId, cancellationToken);
            if (!fluxo)
                return CommandResult.Falha("Fluxo circular nao encontrado.");

            var meta = new MetaCircular(request.FluxoId, request.TipoIndicador, request.PeriodoInicio, request.PeriodoFim, request.ValorMeta, request.Unidade, request.Formula, request.ResponsavelId, tenantId, usuario);
            if (!meta.IsValid)
                return CommandResult.Falha(meta.Notifications.Select(n => n.Message));

            _context.MetasCirculares.Add(meta);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Meta circular definida!", new { meta.Id });
        }
    }

    public class RegistrarMedicaoCircularCommandHandler : ICommandHandler<RegistrarMedicaoCircularCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarMedicaoCircularCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarMedicaoCircularCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var fluxo = await _context.FluxosCirculares.AnyAsync(f => f.Id == request.FluxoId, cancellationToken);
            if (!fluxo)
                return CommandResult.Falha("Fluxo circular nao encontrado.");

            var medicao = new MedicaoCircular(request.FluxoId, request.TipoIndicador, request.Periodo, request.Numerador, request.Denominador, request.Unidade, request.Fonte, tenantId, usuario);
            if (!medicao.IsValid)
                return CommandResult.Falha(medicao.Notifications.Select(n => n.Message));

            _context.MedicoesCirculares.Add(medicao);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Medicao registrada!", new { medicao.Id, medicao.Valor });
        }
    }
}
