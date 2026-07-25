using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-ESC — Parâmetro de sequenciamento por tenant (prd_esc_parametro). ESC-REG-023.</summary>
    public class EscParametro : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string? Valor { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected EscParametro() { } // EF Core

        public EscParametro(string chave, string tenantId, string criadoPor, string? valor = null, bool ativo = true)
            : base(tenantId, criadoPor)
        {
            Chave = chave;
            Valor = valor;
            Ativo = ativo;

            AddNotifications(new Contract<EscParametro>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do parâmetro é obrigatória [Origem: EscParametro]. (ESC-REG-023)")
            );
        }

        public void Alterar(string? valor, bool ativo, string alteradoPor)
        {
            Valor = valor;
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
        }
    }
}
