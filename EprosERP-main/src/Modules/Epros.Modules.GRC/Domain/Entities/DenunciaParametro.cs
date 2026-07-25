using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-DEN — Parametro (grc_den_parametro). Parametrizacao do canal por tenant: SLA,
    /// anonimato, categorias e regras de acesso. Fiel a
    /// EF_13_GRC_INVESTIGACOES_E_DENUNCIAS_V1 (secoes 9 e 11.7).
    /// </summary>
    public class DenunciaParametro : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string ValorJson { get; private set; } = string.Empty;
        public bool Ativo { get; private set; }
        public DateTime DataAlteracao { get; private set; }

        protected DenunciaParametro() { } // EF Core

        public DenunciaParametro(
            string chave,
            string valorJson,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<DenunciaParametro>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do parâmetro é obrigatória.")
            );

            Chave = chave;
            ValorJson = valorJson ?? string.Empty;
            Ativo = true;
            DataAlteracao = DateTime.UtcNow;
        }

        public void Alterar(string valorJson, bool ativo, string usuario)
        {
            ValorJson = valorJson ?? string.Empty;
            Ativo = ativo;
            DataAlteracao = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }
    }
}
