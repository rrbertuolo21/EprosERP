using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Services
{
    /// <summary>
    /// Conjunto de capacidades efetivas de um usuário numa empresa (nomes em minúsculas, "recurso:acao").
    /// <see cref="Concedidas"/> = capacidades vindas de papéis (na empresa corrente) + grants diretos.
    /// <see cref="Negadas"/> = deny direto (REG-041) — SOBREPÕE o grant.
    /// </summary>
    public sealed class CapacidadesEfetivas
    {
        public HashSet<string> Concedidas { get; }
        public HashSet<string> Negadas { get; }

        public CapacidadesEfetivas(HashSet<string> concedidas, HashSet<string> negadas)
        {
            Concedidas = concedidas;
            Negadas = negadas;
        }

        /// <summary>Capacidade utilizável = concedida E não negada (deny vence). É o teste de VISIBILIDADE do menu
        /// e a condição de AUTORIZAÇÃO do AbacFilter — mesma fonte, garante o invariante "visível ⇔ autoriza".</summary>
        public bool PodeUsar(string capacidade)
            => !string.IsNullOrWhiteSpace(capacidade)
               && Concedidas.Contains(capacidade)
               && !Negadas.Contains(capacidade);
    }

    /// <summary>
    /// 1.10 (PERMISSOES_DE_MENU) — FONTE ÚNICA das capacidades efetivas do RBAC unificado (1.09). Extraída do
    /// <c>AbacFilter</c> para que o GATE (AbacFilter) e a PROJEÇÃO DE MENU (GET /menu) computem exatamente o
    /// mesmo conjunto — fecha LC-1/LC-2 (item visível ⇔ endpoint autoriza; REG-002/REG-080/§8.2/CA-007).
    ///
    /// Regra (idêntica ao AbacFilter): capacidades = união dos papéis do usuário NA EMPRESA CORRENTE
    /// (papel com EmpresaId nulo vale p/ todas) + GRANT direto; DENY direto sobrepõe (REG-040/041).
    ///
    /// REG-070/item 6 (cache por request): a instância é registrada Scoped (vive por request, junto do
    /// DbContext) e memoiza por (usuário, empresa) — o AbacFilter e o menu não refazem a query N vezes no
    /// mesmo request. TTL efetivo = duração do request (mesma postura curta do InquilinoSaaSMiddleware).
    /// </summary>
    public interface ICapacidadesEfetivasService
    {
        Task<CapacidadesEfetivas> ObterAsync(Guid usuarioId, Guid? empresaCorrente, CancellationToken ct = default);
    }

    public sealed class CapacidadesEfetivasService : ICapacidadesEfetivasService
    {
        private readonly ContextGestaoClientes _context;
        private readonly ConcurrentDictionary<string, CapacidadesEfetivas> _cacheRequest = new();

        public CapacidadesEfetivasService(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<CapacidadesEfetivas> ObterAsync(Guid usuarioId, Guid? empresaCorrente, CancellationToken ct = default)
        {
            var chave = $"{usuarioId:N}:{empresaCorrente?.ToString("N") ?? "-"}";
            if (_cacheRequest.TryGetValue(chave, out var cache))
                return cache;

            // DENY direto (REG-041) — nomes das capacidades negadas ao usuário.
            var negadasIds = await _context.UsuariosCapacidades
                .Where(uc => uc.UsuarioId == usuarioId && !uc.Granted)
                .Select(uc => uc.CapacidadeId)
                .ToListAsync(ct);

            var negadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (negadasIds.Count > 0)
            {
                var nomesNegados = await _context.Capacidades
                    .IgnoreQueryFilters()
                    .Where(c => negadasIds.Contains(c.Id) && c.DeletadoEm == null)
                    .Select(c => c.Name)
                    .ToListAsync(ct);
                foreach (var n in nomesNegados) negadas.Add(n);
            }

            // Capacidades dos papéis do usuário na empresa corrente (papel EmpresaId nulo = todas).
            var papelIds = await _context.UsuariosPapeis
                .Where(up => up.UsuarioId == usuarioId && (up.EmpresaId == null || up.EmpresaId == empresaCorrente))
                .Select(up => up.PapelId)
                .ToListAsync(ct);

            var capIds = new HashSet<Guid>();
            if (papelIds.Count > 0)
            {
                var capsDoPapel = await _context.PapeisCapacidades
                    .IgnoreQueryFilters()
                    .Where(pc => papelIds.Contains(pc.PapelId) && pc.DeletadoEm == null)
                    .Select(pc => pc.CapacidadeId)
                    .ToListAsync(ct);
                foreach (var id in capsDoPapel) capIds.Add(id);
            }

            // GRANT direto (REG-040).
            var grantsIds = await _context.UsuariosCapacidades
                .Where(uc => uc.UsuarioId == usuarioId && uc.Granted)
                .Select(uc => uc.CapacidadeId)
                .ToListAsync(ct);
            foreach (var id in grantsIds) capIds.Add(id);

            var concedidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (capIds.Count > 0)
            {
                var nomesEfetivos = await _context.Capacidades
                    .IgnoreQueryFilters()
                    .Where(c => capIds.Contains(c.Id) && c.DeletadoEm == null)
                    .Select(c => c.Name)
                    .ToListAsync(ct);
                foreach (var n in nomesEfetivos) concedidas.Add(n);
            }

            var resultado = new CapacidadesEfetivas(concedidas, negadas);
            _cacheRequest[chave] = resultado;
            return resultado;
        }
    }
}
