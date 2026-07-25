using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class NewsletterSubscriber : EntidadeSaaSBase, IHardDeletable
    {
        public string Email { get; private set; } = string.Empty;
        public bool Ativo { get; private set; } = true;
        public bool ConsentimentoLGPD { get; private set; }
        public string TermosVersao { get; private set; } = string.Empty;
        public string IpRegistro { get; private set; } = string.Empty;
        public DateTime DataConsentimento { get; private set; }
        public DateTime? DesativadoEm { get; private set; }
        public Guid TokenDescadastro { get; private set; }

        protected NewsletterSubscriber() { } // EF Core

        // Construtor principal com conformidade LGPD
        public NewsletterSubscriber(
            string email,
            bool consentimentoLGPD,
            string termosVersao,
            string ipRegistro,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<NewsletterSubscriber>()
                .Requires()
                .IsNotNullOrEmpty(email, nameof(Email), "O e-mail para inscrição é obrigatório")
                .IsEmail(email, nameof(Email), "O formato do e-mail é inválido")
                .IsTrue(consentimentoLGPD, nameof(ConsentimentoLGPD), "O consentimento com os termos de privacidade é obrigatório para inscrição na newsletter.")
                .IsNotNullOrEmpty(termosVersao, nameof(TermosVersao), "A versão dos termos é obrigatória.")
                .IsNotNullOrEmpty(ipRegistro, nameof(IpRegistro), "O IP de registro é obrigatório.")
            );

            Email = email;
            ConsentimentoLGPD = consentimentoLGPD;
            TermosVersao = termosVersao;
            IpRegistro = ipRegistro;
            DataConsentimento = DateTime.UtcNow;
            TokenDescadastro = Guid.NewGuid();
            Ativo = true;
        }

        // Construtor legado para compatibilidade com testes existentes
        public NewsletterSubscriber(string email, string tenantId, string criadoPor)
            : this(email, true, "v1.0-legacy", "127.0.0.1", tenantId, criadoPor)
        {
        }

        public void CancelarInscricao(string alteradoPor)
        {
            Ativo = false;
            DesativadoEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void ReativarInscricao(string alteradoPor)
        {
            Ativo = true;
            DesativadoEm = null;
            MarcarAlterado(alteradoPor);
        }
    }
}
