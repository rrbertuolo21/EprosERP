using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class ComunicacaoSuperAdmin : EntidadeSaaSBase
    {
        public List<string> BusinessIds { get; private set; } = new();
        public List<string> Canais { get; private set; } = new();
        public string Assunto { get; private set; } = string.Empty;
        public string Mensagem { get; private set; } = string.Empty;
        public Guid EnviadoPor { get; private set; }
        public DateTime EnviadoEm { get; private set; }
        public string Status { get; private set; } = string.Empty; // Pendente, Sucesso, Falha

        protected ComunicacaoSuperAdmin() { } // EF Core

        public ComunicacaoSuperAdmin(
            List<string> businessIds, 
            string assunto, 
            string mensagem, 
            Guid enviadoPor, 
            string status,
            string tenantId, 
            string criadoPor,
            List<string>? canais = null) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ComunicacaoSuperAdmin>()
                .Requires()
                .IsNotNull(businessIds, nameof(BusinessIds), "A lista de destinatários é obrigatória")
                .IsGreaterThan(businessIds.Count, 0, nameof(BusinessIds), "Pelo menos um destinatário deve ser selecionado")
                .IsNotNullOrEmpty(assunto, nameof(Assunto), "O assunto da comunicação é obrigatório")
                .IsNotNullOrEmpty(mensagem, nameof(Mensagem), "A mensagem da comunicação é obrigatória")
            );

            BusinessIds = businessIds;
            Assunto = assunto;
            Mensagem = mensagem;
            EnviadoPor = enviadoPor;
            EnviadoEm = DateTime.UtcNow;
            Status = status;
            Canais = canais ?? new List<string> { "Email" };
        }

        public void AtualizarStatus(string status, string alteradoPor)
        {
            AddNotifications(new Contract<ComunicacaoSuperAdmin>()
                .Requires()
                .IsNotNullOrEmpty(status, nameof(Status), "O status de envio é obrigatório")
            );

            if (status != null && status != "Sucesso" && status != "Falha")
            {
                AddNotification(nameof(Status), "O status de envio deve ser Sucesso ou Falha");
            }

            if (IsValid)
            {
                Status = status ?? string.Empty;
                MarcarAlterado(alteradoPor);
            }
        }
    }
}
