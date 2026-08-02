using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Ajuste de eliminação intercompany de um grupo por período (EF FIN-CON §10.7 con_eliminacao_intercompany).
    /// Empresas origem/destino cross-module por Guid FK.
    /// </summary>
    public class EliminacaoIntercompany : EntidadeSaaSBase
    {
        public Guid GrupoConsolidacaoId { get; private set; }
        public string Periodo { get; private set; } = string.Empty;
        public Guid? EmpresaOrigemId { get; private set; }
        public Guid? EmpresaDestinoId { get; private set; }
        public decimal? Valor { get; private set; }
        public string? Descricao { get; private set; }
        public string? Estado { get; private set; }
        public DateTime? AplicadaEm { get; private set; }

        public const string EstadoPendente = "Pendente";
        public const string EstadoAplicada = "Aplicada";

        /// <summary>True quando a eliminação já foi aplicada ao balancete consolidado pelo motor.</summary>
        public bool Aplicada => string.Equals(Estado, EstadoAplicada, StringComparison.OrdinalIgnoreCase);

        protected EliminacaoIntercompany() { } // EF Core

        public EliminacaoIntercompany(Guid grupoConsolidacaoId, string periodo, Guid? empresaOrigemId, Guid? empresaDestinoId,
            decimal? valor, string? descricao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            GrupoConsolidacaoId = grupoConsolidacaoId;
            Periodo = periodo;
            EmpresaOrigemId = empresaOrigemId;
            EmpresaDestinoId = empresaDestinoId;
            Valor = valor;
            Descricao = descricao;
            Estado = EstadoPendente;
            Validar();
        }

        /// <summary>Marca a eliminação como aplicada ao balancete consolidado (idempotência do motor).</summary>
        public void MarcarAplicada(string usuario)
        {
            Estado = EstadoAplicada;
            AplicadaEm = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<EliminacaoIntercompany>()
                .Requires()
                .IsNotEmpty(GrupoConsolidacaoId, nameof(GrupoConsolidacaoId), "O grupo de consolidação é obrigatório [Origem: EliminacaoIntercompany]")
                .IsNotNullOrEmpty(Periodo, nameof(Periodo), "O período é obrigatório [Origem: EliminacaoIntercompany]"));
        }
    }
}
