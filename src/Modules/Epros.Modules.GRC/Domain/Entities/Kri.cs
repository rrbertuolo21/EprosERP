using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-RIS — KRI (grc_ris_kri). Indicador-chave de risco monitorado, com limites de alerta.
    /// Fiel a EF_13_GRC_GESTAO_DE_RISCOS_CORPORATIVOS_V1 (secao 10.2).
    /// </summary>
    public class Kri : EntidadeSaaSBase
    {
        public Guid RiscoId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string? Unidade { get; private set; }
        public decimal? LimiteAlerta { get; private set; }
        public decimal? LimiteCritico { get; private set; }
        public string Status { get; private set; } = "Ativo"; // Ativo, Inativo

        protected Kri() { } // EF Core

        public Kri(
            Guid riscoId,
            string nome,
            string? unidade,
            decimal? limiteAlerta,
            decimal? limiteCritico,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Kri>()
                .Requires()
                .IsTrue(riscoId != Guid.Empty, nameof(RiscoId), "O risco do KRI e obrigatorio.")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do KRI e obrigatorio.")
            );

            RiscoId = riscoId;
            Nome = nome;
            Unidade = unidade;
            LimiteAlerta = limiteAlerta;
            LimiteCritico = limiteCritico;
            Status = "Ativo";
        }

        public void Inativar(string usuario)
        {
            Status = "Inativo";
            MarcarAlterado(usuario);
        }
    }
}
