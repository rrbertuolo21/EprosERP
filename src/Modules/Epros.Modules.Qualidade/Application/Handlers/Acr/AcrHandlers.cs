using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Qualidade.Application.Commands.Acr;
using Epros.Modules.Qualidade.Domain.Entities;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Modules.Qualidade.Domain.Services.Aql;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Qualidade.Application.Handlers.Acr
{
    public class AdicionarItemAcrCommandHandler : ICommandHandler<AdicionarItemAcrCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public AdicionarItemAcrCommandHandler(ContextQualidade ctx, ITenantProvider tenant, ICurrentUser user)
        { _ctx = ctx; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(AdicionarItemAcrCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";
            var analise = await _ctx.AcrAnalises.FirstOrDefaultAsync(a => a.Id == r.AnaliseId, ct);
            if (analise is null) return CommandResult.Falha("Analise de aceite/rejeicao nao encontrada.", block: true);
            if (analise.Status == EStatusRegistroQualidade.Encerrado)
                return CommandResult.Falha("Analise encerrada nao aceita novos itens.", block: true);

            var item = new AcrItem(r.AnaliseId, r.Sequencia, r.QuantidadeAnalisada, r.UnidadeMedida, r.ItemParcial,
                r.ProdutoId, r.CodigoItem, r.NomeItem, r.Lote, r.QuantidadeIntegral, r.Ncm, r.Cfop, r.ValorUnitario,
                r.Observacao, tenantId, usuario);
            if (!item.IsValid) return CommandResult.Falha(item.Notifications.Select(n => n.Message));

            _ctx.AcrItens.Add(item);
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Item adicionado a analise.", new { item.Id, item.Sequencia });
        }
    }

    public class SubmeterAcrCommandHandler : ICommandHandler<SubmeterAcrCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ICurrentUser _user;
        public SubmeterAcrCommandHandler(ContextQualidade ctx, ICurrentUser user) { _ctx = ctx; _user = user; }

        public async Task<CommandResult> Handle(SubmeterAcrCommand r, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            var analise = await _ctx.AcrAnalises.FirstOrDefaultAsync(a => a.Id == r.AnaliseId, ct);
            if (analise is null) return CommandResult.Falha("Analise nao encontrada.", block: true);

            var itens = await _ctx.AcrItens.Where(i => i.AnaliseId == r.AnaliseId).ToListAsync(ct);
            foreach (var i in itens) analise.AdicionarItem(i);

            analise.Submeter(usuario);
            if (!analise.IsValid) return CommandResult.Falha(analise.Notifications.Select(n => n.Message));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Analise submetida.", new { analise.Id, Status = analise.Status.ToString() });
        }
    }

    public class DecidirAnaliseAcrCommandHandler : ICommandHandler<DecidirAnaliseAcrCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public DecidirAnaliseAcrCommandHandler(ContextQualidade ctx, ITenantProvider tenant, ICurrentUser user)
        { _ctx = ctx; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(DecidirAnaliseAcrCommand r, CancellationToken ct)
        {
            var analise = await _ctx.AcrAnalises.FirstOrDefaultAsync(a => a.Id == r.AnaliseId, ct);
            if (analise is null) return CommandResult.Falha("Analise nao encontrada.", block: true);

            return await AcrDecisao.RegistrarAsync(_ctx, _tenant.GetTenantId(), _user.GetUserId() ?? "system",
                analise, r.Resultado, r.MotivoId, r.Severidade, r.QuantidadeAfetada, r.Justificativa,
                r.AprovadoPor, r.StatusFiscalIgnorado, r.Lote, null, ct);
        }
    }

    public class AvaliarAcrPorAmostragemCommandHandler : ICommandHandler<AvaliarAcrPorAmostragemCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        private readonly MotorAql _motor;
        public AvaliarAcrPorAmostragemCommandHandler(ContextQualidade ctx, ITenantProvider tenant, ICurrentUser user, MotorAql motor)
        { _ctx = ctx; _tenant = tenant; _user = user; _motor = motor; }

        public async Task<CommandResult> Handle(AvaliarAcrPorAmostragemCommand r, CancellationToken ct)
        {
            var analise = await _ctx.AcrAnalises.FirstOrDefaultAsync(a => a.Id == r.AnaliseId, ct);
            if (analise is null) return CommandResult.Falha("Analise nao encontrada.", block: true);

            var plano = _motor.CalcularPlano(r.TamanhoLote, r.Nivel, r.Aql, r.Severidade);
            var decisao = plano.Decidir(r.Defeituosos);
            var resultado = decisao switch
            {
                EDecisaoLote.Rejeita => EResultadoAcr.Rejeitado,
                _ => EResultadoAcr.Aceito // Aceita e AceitaERetornaNormal -> lote aceito
            };

            var criterio = new
            {
                LetraCodigo = plano.LetraCodigo.ToString(),
                plano.TamanhoAmostra, plano.NumeroAceitacao, plano.NumeroRejeicao,
                r.Defeituosos, plano.InspecaoTotal, Decisao = decisao.ToString()
            };

            return await AcrDecisao.RegistrarAsync(_ctx, _tenant.GetTenantId(), _user.GetUserId() ?? "system",
                analise, resultado, r.MotivoId, r.SeveridadeMotivo, r.Defeituosos, JsonSerializer.Serialize(criterio),
                r.AprovadoPor, false, r.Lote, criterio, ct);
        }
    }

    /// <summary>Fluxo comum de decisao ACR: grava resultado, emite projecoes e eventos Outbox.</summary>
    internal static class AcrDecisao
    {
        public static async Task<CommandResult> RegistrarAsync(
            ContextQualidade ctx, string tenantId, string usuario, AcrAnalise analise, EResultadoAcr resultado,
            Guid? motivoId, string? severidade, decimal? quantidadeAfetada, string? justificativa, Guid? aprovadoPor,
            bool statusFiscalIgnorado, string? lote, object? criterioAmostragem, CancellationToken ct)
        {
            var res = new AcrResultado(analise.Id, resultado, motivoId, severidade, quantidadeAfetada, justificativa,
                aprovadoPor, statusFiscalIgnorado, tenantId, usuario);
            if (!res.IsValid) return CommandResult.Falha(res.Notifications.Select(n => n.Message));
            ctx.AcrResultados.Add(res);

            // Projecao + evento de estoque (intencao). Qualidade solicita; nao movimenta saldo (D3/D21).
            var (tipoEvento, eventoNome) = MapearEstoque(resultado);
            var qtd = quantidadeAfetada ?? 0m;
            var eventoEstoque = new AcrEventoEstoque(res.Id, tipoEvento, qtd, lote, analise.LocalId, null, tenantId, usuario);
            ctx.AcrEventosEstoque.Add(eventoEstoque);
            ctx.OutboxMessages.Add(new OutboxMessage(tenantId, eventoNome, JsonSerializer.Serialize(new
            {
                analiseId = analise.Id, resultadoId = res.Id, tipo = tipoEvento.ToString(),
                lote, quantidade = qtd, resultado = resultado.ToString(), tenantId
            })));

            // Gatilho de NCR (D13, default seguro): rejeicao SUGERE NCR (projecao + evento), nao cria a revelia.
            if (resultado == EResultadoAcr.Rejeitado || resultado == EResultadoAcr.Quarentena)
            {
                var eventoNcr = new AcrEventoNcr(res.Id, EGatilhoNcrAcr.Severidade, tenantId, usuario);
                ctx.AcrEventosNcr.Add(eventoNcr);
                ctx.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Qualidade.AcrNcrSolicitada,
                    JsonSerializer.Serialize(new
                    {
                        analiseId = analise.Id, resultadoId = res.Id, motivoId, severidade,
                        gatilho = EGatilhoNcrAcr.Severidade.ToString(), tenantId
                    })));
            }

            // Intencao de devolucao fiscal — ⚠️ valida-contador (D15): sinaliza, nao monta a NF.
            if (resultado == EResultadoAcr.Rejeitado)
            {
                ctx.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Qualidade.AcrDevolucaoSolicitada,
                    JsonSerializer.Serialize(new
                    {
                        analiseId = analise.Id, resultadoId = res.Id, lote, quantidade = qtd,
                        documentoFiscalId = analise.DocumentoFiscalId, tenantId,
                        aviso = "valida-contador: CFOP/NCM/chave = modulo Fiscal + contador"
                    })));
            }

            analise.Encerrar(usuario);
            await ctx.SaveChangesAsync(ct);

            return CommandResult.Ok("Decisao de aceite/rejeicao registrada.", new
            {
                AnaliseId = analise.Id,
                Resultado = resultado.ToString(),
                ResultadoId = res.Id,
                EventoEstoque = eventoNome,
                GerouSugestaoNcr = resultado == EResultadoAcr.Rejeitado || resultado == EResultadoAcr.Quarentena,
                Amostragem = criterioAmostragem
            });
        }

        private static (ETipoEventoEstoqueAcr, string) MapearEstoque(EResultadoAcr resultado) => resultado switch
        {
            EResultadoAcr.Aceito => (ETipoEventoEstoqueAcr.Liberar, CatalogoEventosIntegracao.Qualidade.AcrLoteLiberado),
            EResultadoAcr.AceitoComDesvio => (ETipoEventoEstoqueAcr.Liberar, CatalogoEventosIntegracao.Qualidade.AcrLoteLiberado),
            EResultadoAcr.Quarentena => (ETipoEventoEstoqueAcr.Quarentena, CatalogoEventosIntegracao.Qualidade.AcrLoteQuarentena),
            _ => (ETipoEventoEstoqueAcr.Bloquear, CatalogoEventosIntegracao.Qualidade.AcrLoteBloqueado) // Rejeitado/Reinspecao
        };
    }
}
