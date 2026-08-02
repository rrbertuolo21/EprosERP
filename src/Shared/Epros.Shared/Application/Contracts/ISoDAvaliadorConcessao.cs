using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Epros.Shared.Application.Contracts
{
    /// <summary>
    /// GRC / D-SOD-03 — porta de avaliação preventiva de Segregação de Funções (SoD) para o caminho de
    /// concessão de acesso (RBAC). Implementada pelo módulo GRC (dono das regras SoD) e consumida pela
    /// Plataforma/GestaoClientes no momento em que um papel/capacidade é concedido, tornando o bloqueio
    /// SoD EFETIVO em runtime (antes o handler de bloqueio existia mas não tinha caller — P0).
    ///
    /// Abstração no kernel compartilhado para evitar dependência de projeto GestaoClientes → GRC.
    /// </summary>
    public interface ISoDAvaliadorConcessao
    {
        Task<SoDResultadoConcessao> AvaliarConcessaoAsync(
            Guid? usuarioId,
            IEnumerable<Guid> funcoesAtuais,
            IEnumerable<Guid> funcoesNovas,
            CancellationToken cancellationToken = default);
    }

    /// <summary>Resultado da avaliação SoD de uma concessão.</summary>
    public sealed class SoDResultadoConcessao
    {
        /// <summary>true quando ao menos uma regra com ModoTratamento=Bloqueia foi violada → concessão negada.</summary>
        public bool Bloqueado { get; init; }

        /// <summary>true quando há conflito (bloqueante ou não) — permite exceção/alerta.</summary>
        public bool TemConflito { get; init; }

        public IReadOnlyList<Guid> RegrasBloqueantes { get; init; } = Array.Empty<Guid>();
        public IReadOnlyList<Guid> RegrasEmConflito { get; init; } = Array.Empty<Guid>();

        public static readonly SoDResultadoConcessao Liberado = new();
    }
}
