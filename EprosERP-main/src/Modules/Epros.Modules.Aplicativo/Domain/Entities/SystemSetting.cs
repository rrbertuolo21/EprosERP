using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class SystemSetting : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string Valor { get; private set; } = string.Empty;
        public string Escopo { get; private set; } = string.Empty; // global, landlord, tenant, gateway, install
        public bool EhSegredo { get; private set; }

        protected SystemSetting() { } // EF Core

        public SystemSetting(string chave, string valor, string escopo, bool ehSegredo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<SystemSetting>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave da configuração é obrigatória")
                .IsNotNullOrEmpty(escopo, nameof(Escopo), "O escopo da configuração é obrigatório")
            );

            if (escopo != null && escopo != "global" && escopo != "landlord" && escopo != "tenant" && escopo != "gateway" && escopo != "install")
            {
                AddNotification(nameof(Escopo), "O escopo da configuração é inválido");
            }

            Chave = chave ?? string.Empty;
            Valor = valor ?? string.Empty;
            Escopo = escopo ?? string.Empty;
            EhSegredo = ehSegredo;
        }

        public void AtualizarValor(string novoValor, string alteradoPor)
        {
            if (IsValid)
            {
                Valor = novoValor;
                MarcarAlterado(alteradoPor);
            }
        }
    }
}
