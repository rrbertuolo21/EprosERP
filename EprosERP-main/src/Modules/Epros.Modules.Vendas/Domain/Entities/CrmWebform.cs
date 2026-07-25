using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>Formulário de captação web (crm_webform). Fonte: EF §10.16. Regras CRM-018..CRM-024.</summary>
    public class CrmWebform : EntidadeSaaSBase
    {
        public string IdentificadorUnico { get; private set; } = string.Empty;
        public string Titulo { get; private set; } = string.Empty;
        public string EstruturaJson { get; private set; } = string.Empty;
        public string? CssCustomizado { get; private set; }
        public string? MensagemAgradecimento { get; private set; }
        public string? TextoBotao { get; private set; }
        public Guid? CampanhaId { get; private set; }
        public string? LeadTituloPadrao { get; private set; }
        public string? LeadStatusPadrao { get; private set; }
        public Guid? LeadOrigemId { get; private set; }
        public bool UsarCaptcha { get; private set; }
        public bool NotificarAdministrador { get; private set; }
        public int ContadorSubmissoes { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected CrmWebform() { }

        public CrmWebform(
            string identificadorUnico,
            string titulo,
            string estruturaJson,
            string? cssCustomizado,
            string? mensagemAgradecimento,
            string? textoBotao,
            Guid? campanhaId,
            string? leadTituloPadrao,
            string? leadStatusPadrao,
            Guid? leadOrigemId,
            bool usarCaptcha,
            bool notificarAdministrador,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            IdentificadorUnico = identificadorUnico;
            Titulo = titulo;
            EstruturaJson = estruturaJson;
            CssCustomizado = cssCustomizado;
            MensagemAgradecimento = mensagemAgradecimento;
            TextoBotao = textoBotao;
            CampanhaId = campanhaId;
            LeadTituloPadrao = leadTituloPadrao;
            LeadStatusPadrao = leadStatusPadrao;
            LeadOrigemId = leadOrigemId;
            UsarCaptcha = usarCaptcha;
            NotificarAdministrador = notificarAdministrador;
            ContadorSubmissoes = 0;
            Ativo = true;

            AddNotifications(new Contract<CrmWebform>()
                .Requires()
                .IsNotNullOrEmpty(identificadorUnico, nameof(IdentificadorUnico), "O webform deve possuir identificador único. [Origem: CrmWebform]")  // CRM-018
                .IsNotNullOrEmpty(titulo, nameof(Titulo), "O webform deve possuir título. [Origem: CrmWebform]")
                .IsNotNullOrEmpty(estruturaJson, nameof(EstruturaJson), "O webform deve possuir estrutura de campos. [Origem: CrmWebform]"));
        }

        /// <summary>CRM-021: incrementa contador de submissão quando o envio é aceito.</summary>
        public void RegistrarSubmissao(string alteradoPor)
        {
            ContadorSubmissoes++;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Responsável pré-atribuído ao lead gerado pelo webform (crm_webform_responsavel). Fonte: EF §10.17.</summary>
    public class CrmWebformResponsavel : EntidadeSaaSBase
    {
        public Guid WebformId { get; private set; }
        public Guid UsuarioId { get; private set; }

        protected CrmWebformResponsavel() { }

        public CrmWebformResponsavel(Guid webformId, Guid usuarioId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            WebformId = webformId;
            UsuarioId = usuarioId;
            AddNotifications(new Contract<CrmWebformResponsavel>()
                .Requires()
                .AreNotEquals(webformId, Guid.Empty, nameof(WebformId), "O webform é obrigatório. [Origem: CrmWebformResponsavel]")
                .AreNotEquals(usuarioId, Guid.Empty, nameof(UsuarioId), "O usuário é obrigatório. [Origem: CrmWebformResponsavel]"));
        }
    }
}
