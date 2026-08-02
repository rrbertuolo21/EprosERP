using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GRC.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GRC.Application.Services
{
    /// <summary>
    /// D-SOD-03 — implementação do avaliador preventivo de SoD consumido pelo caminho de concessão RBAC.
    /// Mesma lógica do <c>AvaliarConcessaoSoDCommandHandler</c>: dado o conjunto de funções efetivas
    /// (atuais + novas), detecta conflitos entre as regras ATIVAS e bloqueia quando a regra é de modo
    /// "Bloqueia". Emite o evento de catálogo (bloqueada/avaliada) para trilha/alerta.
    ///
    /// Isso torna o bloqueio SoD EFETIVO em runtime: a concessão de papel (GestaoClientes) chama esta
    /// porta ANTES de gravar; se bloqueado, a concessão é negada.
    /// </summary>
    public class SoDAvaliadorConcessaoService : ISoDAvaliadorConcessao
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;

        public SoDAvaliadorConcessaoService(ContextGRC context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<SoDResultadoConcessao> AvaliarConcessaoAsync(
            Guid? usuarioId,
            IEnumerable<Guid> funcoesAtuais,
            IEnumerable<Guid> funcoesNovas,
            CancellationToken cancellationToken = default)
        {
            var novas = (funcoesNovas ?? Enumerable.Empty<Guid>()).ToHashSet();
            var efetivas = (funcoesAtuais ?? Enumerable.Empty<Guid>()).Concat(novas).ToHashSet();

            // Sem funções novas não há o que avaliar (evita custo em concessões fora do escopo SoD).
            if (novas.Count == 0)
                return SoDResultadoConcessao.Liberado;

            var regrasAtivas = await _context.RegrasSoD.Where(r => r.Status == "Ativo").ToListAsync(cancellationToken);

            var conflitos = regrasAtivas
                .Where(r => efetivas.Contains(r.FuncaoAId) && efetivas.Contains(r.FuncaoBId))
                .Where(r => novas.Contains(r.FuncaoAId) || novas.Contains(r.FuncaoBId))
                .ToList();

            // Sem conflito → não há efeito colateral (nenhum evento).
            if (conflitos.Count == 0)
                return SoDResultadoConcessao.Liberado;

            var bloqueados = conflitos.Where(r => r.BloqueiaConcessao()).ToList();

            var tenantId = _tenantProvider.GetTenantId();
            var eventType = bloqueados.Any()
                ? CatalogoEventosIntegracao.Grc.SodConcessaoBloqueada
                : CatalogoEventosIntegracao.Grc.SodConcessaoAvaliada;

            var payload = JsonSerializer.Serialize(new
            {
                UsuarioId = usuarioId,
                Alvo = "Usuario",
                Decisao = bloqueados.Any() ? "Bloqueado" : "PermitidoComExcecao",
                RegrasEmConflito = conflitos.Select(c => c.Id),
                RegrasBloqueantes = bloqueados.Select(c => c.Id),
                Origem = "ConcessaoRBAC"
            });
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, eventType, payload));
            await _context.SaveChangesAsync(cancellationToken);

            return new SoDResultadoConcessao
            {
                Bloqueado = bloqueados.Any(),
                TemConflito = true,
                RegrasBloqueantes = bloqueados.Select(c => c.Id).ToList(),
                RegrasEmConflito = conflitos.Select(c => c.Id).ToList()
            };
        }
    }
}
