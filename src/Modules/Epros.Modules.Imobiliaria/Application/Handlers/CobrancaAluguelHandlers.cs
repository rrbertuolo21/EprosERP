using System;
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
    /// Gera a cobranca do aluguel por competencia e publica o evento consumido pelo CONTAS_RECEBER.
    /// Idempotente: uma cobranca por (locacao+competencia+tipo). A segunda chamada retorna a existente.
    /// </summary>
    public class GerarCobrancaAluguelCommandHandler : ICommandHandler<GerarCobrancaAluguelCommand>
    {
        private readonly ContextImobiliaria _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public GerarCobrancaAluguelCommandHandler(ContextImobiliaria context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(GerarCobrancaAluguelCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var locacao = await _context.Locacoes.FirstOrDefaultAsync(l => l.Id == request.LocacaoId, cancellationToken);
            if (locacao is null)
                return CommandResult.Falha("Locacao nao encontrada.");
            if (locacao.Status != EStatusLocacao.Vigente)
                return CommandResult.Falha("Apenas locacoes vigentes geram cobranca de aluguel. (NF-01)", block: true);

            var competencia = new DateTime(request.Ano, request.Mes, 1);

            // Idempotencia (T2/NF-01): nao duplicar a cobranca da mesma competencia/tipo.
            var existente = await _context.CobrancasAluguel.FirstOrDefaultAsync(
                c => c.LocacaoId == request.LocacaoId && c.Competencia == competencia && c.Tipo == request.Tipo,
                cancellationToken);
            if (existente is not null)
                return CommandResult.Ok("Cobranca ja existente para a competencia (idempotente).",
                    new { CobrancaId = existente.Id, Reaproveitada = true });

            var valor = request.ValorOverride ?? locacao.Valor;
            var cobranca = new CobrancaAluguel(locacao.Id, competencia, request.Tipo, valor, locacao.Vencimento, tenantId, usuario);
            if (!cobranca.IsValid)
                return CommandResult.Falha(cobranca.Notifications.Select(n => n.Message));

            _context.CobrancasAluguel.Add(cobranca);

            // Evento consumido pelo CONTAS_RECEBER — a baixa/titulo vivem la (NF-01). Nao recriamos o recebivel.
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Imobiliaria.AluguelCobrancaGerada,
                JsonSerializer.Serialize(new
                {
                    cobrancaId = cobranca.Id,
                    locacaoId = locacao.Id,
                    imovelId = locacao.ImovelId,
                    competencia = competencia.ToString("yyyy-MM"),
                    tipo = request.Tipo.ToString(),
                    valor = cobranca.Valor,
                    vencimentoDia = cobranca.Vencimento,
                    tenantId
                })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Cobranca de aluguel gerada com sucesso!",
                new { CobrancaId = cobranca.Id, Competencia = competencia.ToString("yyyy-MM"), cobranca.Valor });
        }
    }

    public class RefletirBaixaAluguelCommandHandler : ICommandHandler<RefletirBaixaAluguelCommand>
    {
        private readonly ContextImobiliaria _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RefletirBaixaAluguelCommandHandler(ContextImobiliaria context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(RefletirBaixaAluguelCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var cobranca = await _context.CobrancasAluguel.FirstOrDefaultAsync(c => c.Id == request.CobrancaId, cancellationToken);
            if (cobranca is null)
                return CommandResult.Falha("Cobranca nao encontrada.");

            cobranca.RefletirBaixa(request.ValorPago, request.ReceberRef, request.DataBaixa ?? DateTime.UtcNow, usuario);
            if (!cobranca.IsValid)
                return CommandResult.Falha(cobranca.Notifications.Select(n => n.Message));

            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Imobiliaria.AluguelBaixaRefletida,
                JsonSerializer.Serialize(new
                {
                    cobrancaId = cobranca.Id,
                    locacaoId = cobranca.LocacaoId,
                    valorPago = cobranca.ValorPago,
                    status = cobranca.Status.ToString(),
                    receberRef = cobranca.ReceberRef,
                    tenantId
                })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Baixa refletida com sucesso!",
                new { CobrancaId = cobranca.Id, Status = cobranca.Status.ToString(), cobranca.ValorPago });
        }
    }

    public class EstornarBaixaAluguelCommandHandler : ICommandHandler<EstornarBaixaAluguelCommand>
    {
        private readonly ContextImobiliaria _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public EstornarBaixaAluguelCommandHandler(ContextImobiliaria context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(EstornarBaixaAluguelCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var cobranca = await _context.CobrancasAluguel.FirstOrDefaultAsync(c => c.Id == request.CobrancaId, cancellationToken);
            if (cobranca is null)
                return CommandResult.Falha("Cobranca nao encontrada.");

            cobranca.RefletirEstorno(usuario);
            if (!cobranca.IsValid)
                return CommandResult.Falha(cobranca.Notifications.Select(n => n.Message));

            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Imobiliaria.AluguelBaixaEstornada,
                JsonSerializer.Serialize(new { cobrancaId = cobranca.Id, locacaoId = cobranca.LocacaoId, tenantId })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Baixa estornada com sucesso!", new { CobrancaId = cobranca.Id, Status = cobranca.Status.ToString() });
        }
    }

    /// <summary>
    /// Emite o recibo de uma cobranca baixada. Numero atomico pelo servico central de numeracao (T9);
    /// idempotente por cobranca (uma cobranca → um recibo). Publica imo.recibo.emitido.
    /// </summary>
    public class EmitirReciboAluguelCommandHandler : ICommandHandler<EmitirReciboAluguelCommand>
    {
        private const string TipoDocumentoRecibo = "imo_recibo_aluguel";

        private readonly ContextImobiliaria _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly INumeracaoService _numeracao;

        public EmitirReciboAluguelCommandHandler(
            ContextImobiliaria context, ITenantProvider tenantProvider, ICurrentUser currentUser, INumeracaoService numeracao)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; _numeracao = numeracao; }

        public async Task<CommandResult> Handle(EmitirReciboAluguelCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var cobranca = await _context.CobrancasAluguel.FirstOrDefaultAsync(c => c.Id == request.CobrancaId, cancellationToken);
            if (cobranca is null)
                return CommandResult.Falha("Cobranca nao encontrada.");
            if (cobranca.Status != EStatusCobrancaAluguel.Pago && cobranca.Status != EStatusCobrancaAluguel.Parcial)
                return CommandResult.Falha("Recibo so pode ser emitido para cobranca com baixa (Paga/Parcial). (NF-05)", block: true);

            // Idempotencia: uma cobranca gera no maximo um recibo.
            var reciboExistente = await _context.RecibosAluguel.FirstOrDefaultAsync(r => r.CobrancaId == cobranca.Id, cancellationToken);
            if (reciboExistente is not null)
                return CommandResult.Ok("Recibo ja emitido para a cobranca (idempotente).",
                    new { ReciboId = reciboExistente.Id, reciboExistente.Numero, Reaproveitado = true });

            var numero = await _numeracao.ProximoNumeroAsync(TipoDocumentoRecibo, 1, cancellationToken);
            var recibo = new ReciboAluguel(cobranca.Id, cobranca.LocacaoId, numero, cobranca.ValorPago, tenantId, usuario);
            if (!recibo.IsValid)
                return CommandResult.Falha(recibo.Notifications.Select(n => n.Message));

            _context.RecibosAluguel.Add(recibo);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Imobiliaria.ReciboEmitido,
                JsonSerializer.Serialize(new { reciboId = recibo.Id, cobrancaId = cobranca.Id, locacaoId = cobranca.LocacaoId, numero, valor = recibo.Valor, tenantId })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Recibo emitido com sucesso!", new { ReciboId = recibo.Id, recibo.Numero, recibo.Valor });
        }
    }
}
