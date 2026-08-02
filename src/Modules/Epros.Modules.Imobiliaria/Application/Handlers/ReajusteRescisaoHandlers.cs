using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Imobiliaria.Application.Commands;
using Epros.Modules.Imobiliaria.Domain.Entities;
using Epros.Modules.Imobiliaria.Domain.Enums;
using Epros.Modules.Imobiliaria.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Imobiliaria.Application.Handlers
{
    /// <summary>
    /// Aplica um reajuste ratificado (ID7/NF-02). Registra o historico e reflete o novo valor no
    /// contrato. valida-contador: o indice/percentual/arredondamento sao definidos pelo contador;
    /// aqui persiste-se o valor ratificado sem inventar o calculo.
    /// </summary>
    public class AplicarReajusteCommandHandler : ICommandHandler<AplicarReajusteCommand>
    {
        private readonly ContextImobiliaria _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AplicarReajusteCommandHandler(ContextImobiliaria context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AplicarReajusteCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var locacao = await _context.Locacoes.FirstOrDefaultAsync(l => l.Id == request.LocacaoId, cancellationToken);
            if (locacao is null)
                return CommandResult.Falha("Locacao nao encontrada.");

            var valorAnterior = locacao.Valor;

            // valida-contador (NF-02): novo valor ratificado; indice/percentual apenas registrados.
            locacao.AplicarNovoValor(request.NovoValor, usuario);
            if (!locacao.IsValid)
                return CommandResult.Falha(locacao.Notifications.Select(n => n.Message));

            var reajuste = new LocacaoReajuste(
                locacao.Id, request.Indice, request.DataBase ?? System.DateTime.UtcNow,
                valorAnterior, request.NovoValor, request.PercentualAplicado, tenantId, usuario);
            if (!reajuste.IsValid)
                return CommandResult.Falha(reajuste.Notifications.Select(n => n.Message));

            _context.LocacaoReajustes.Add(reajuste);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Imobiliaria.LocacaoReajustada,
                JsonSerializer.Serialize(new { locacaoId = locacao.Id, valorAnterior, valorNovo = request.NovoValor, indice = request.Indice, tenantId })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Reajuste aplicado com sucesso!",
                new { LocacaoId = locacao.Id, ValorAnterior = valorAnterior, ValorNovo = locacao.Valor });
        }
    }

    /// <summary>
    /// Rescinde a locacao (ID7): registra a rescisao, encerra a locacao e libera o imovel, numa
    /// unica transacao. Multa proporcional = valida-contador (NF-02).
    /// </summary>
    public class RescindirLocacaoCommandHandler : ICommandHandler<RescindirLocacaoCommand>
    {
        private readonly ContextImobiliaria _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RescindirLocacaoCommandHandler(ContextImobiliaria context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(RescindirLocacaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var locacao = await _context.Locacoes.FirstOrDefaultAsync(l => l.Id == request.LocacaoId, cancellationToken);
            if (locacao is null)
                return CommandResult.Falha("Locacao nao encontrada.");
            if (locacao.Status != EStatusLocacao.Vigente)
                return CommandResult.Falha("Apenas locacoes vigentes podem ser rescindidas.", block: true);

            if (await _context.LocacaoRescisoes.AnyAsync(r => r.LocacaoId == request.LocacaoId, cancellationToken))
                return CommandResult.Falha("Locacao ja possui rescisao registrada.", block: true);

            var rescisao = new LocacaoRescisao(
                locacao.Id, request.Motivo, request.DataRescisao, request.AvisoPrevioDias,
                request.MultaProporcional, request.VistoriaSaidaId, tenantId, usuario);
            if (!rescisao.IsValid)
                return CommandResult.Falha(rescisao.Notifications.Select(n => n.Message));

            locacao.Encerrar(usuario);
            if (!locacao.IsValid)
                return CommandResult.Falha(locacao.Notifications.Select(n => n.Message));

            if (locacao.ImovelId.HasValue)
            {
                var imovel = await _context.Imoveis.FirstOrDefaultAsync(i => i.Id == locacao.ImovelId.Value, cancellationToken);
                imovel?.LiberarLocacao(usuario);
            }

            _context.LocacaoRescisoes.Add(rescisao);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Imobiliaria.LocacaoRescindida,
                JsonSerializer.Serialize(new { locacaoId = locacao.Id, imovelId = locacao.ImovelId, motivo = request.Motivo, multa = request.MultaProporcional, tenantId })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Locacao rescindida com sucesso!",
                new { LocacaoId = locacao.Id, RescisaoId = rescisao.Id, Status = locacao.Status.ToString() });
        }
    }
}
