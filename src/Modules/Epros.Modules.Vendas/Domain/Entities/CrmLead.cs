using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Lead comercial (crm_lead). Fonte: EF §10.5. Regras CRM-005..CRM-012.
    /// Lead sem status informado inicia como Novo (CRM-006). O cadastro mestre de pessoa/cliente
    /// permanece em Cadastros Base; aqui só o contexto comercial (EF §2/§9).
    /// </summary>
    public class CrmLead : EntidadeSaaSBase
    {
        public Guid? PipelineId { get; private set; }
        public Guid? EtapaId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string? PrimeiroNome { get; private set; }
        public string? Sobrenome { get; private set; }
        public string? Email { get; private set; }
        public string? Telefone { get; private set; }
        public string? Assunto { get; private set; }
        public decimal? ValorEstimado { get; private set; }
        public ECrmLeadStatus Status { get; private set; } = ECrmLeadStatus.Novo;
        public Guid? OrigemId { get; private set; }
        public string? FontesJson { get; private set; }
        public string? ProdutosJson { get; private set; }
        public string? EtiquetasJson { get; private set; }
        public string? Notas { get; private set; }
        public int? PosicaoKanban { get; private set; }
        public bool Convertido { get; private set; }
        public bool Ativo { get; private set; } = true;
        public ECrmEstadoArquivo EstadoArquivo { get; private set; } = ECrmEstadoArquivo.Ativo;
        public string? Visibilidade { get; private set; }
        public Guid? ClienteId { get; private set; }
        public Guid? ContatoId { get; private set; }
        public Guid? OportunidadeId { get; private set; }
        public Guid? CampanhaId { get; private set; }
        public string? EnderecoIpCaptacao { get; private set; }
        public string? ImagemCapa { get; private set; }
        public string? CamposCustomizadosJson { get; private set; }
        public Guid CriadoPorUsuarioId { get; private set; }
        public Guid? ResponsavelUsuarioId { get; private set; }

        protected CrmLead() { }

        public CrmLead(
            Guid? pipelineId,
            Guid? etapaId,
            string nome,
            string? primeiroNome,
            string? sobrenome,
            string? email,
            string? telefone,
            string? assunto,
            decimal? valorEstimado,
            ECrmLeadStatus? status,
            Guid? origemId,
            string? notas,
            Guid criadoPorUsuarioId,
            Guid? responsavelUsuarioId,
            Guid? campanhaId,
            string? enderecoIpCaptacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PipelineId = pipelineId;
            EtapaId = etapaId;
            Nome = nome;
            PrimeiroNome = primeiroNome;
            Sobrenome = sobrenome;
            Email = email;
            Telefone = telefone;
            Assunto = assunto;
            ValorEstimado = valorEstimado;
            Status = status ?? ECrmLeadStatus.Novo; // CRM-006
            OrigemId = origemId;
            Notas = notas;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            ResponsavelUsuarioId = responsavelUsuarioId;
            CampanhaId = campanhaId;
            EnderecoIpCaptacao = enderecoIpCaptacao;
            Convertido = false;
            Ativo = true;
            EstadoArquivo = ECrmEstadoArquivo.Ativo;

            AddNotifications(new Contract<CrmLead>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O lead deve possuir nome. [Origem: CrmLead]"));
        }

        public void Alterar(
            Guid? pipelineId,
            Guid? etapaId,
            string nome,
            string? email,
            string? telefone,
            string? assunto,
            decimal? valorEstimado,
            Guid? origemId,
            string? notas,
            Guid? responsavelUsuarioId,
            string alteradoPor)
        {
            PipelineId = pipelineId;
            EtapaId = etapaId;
            Nome = nome;
            Email = email;
            Telefone = telefone;
            Assunto = assunto;
            ValorEstimado = valorEstimado;
            OrigemId = origemId;
            Notas = notas;
            ResponsavelUsuarioId = responsavelUsuarioId;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>CRM-007: move o lead entre etapas atualizando ordem no kanban.</summary>
        public void MoverEtapa(Guid etapaId, int? posicaoKanban, string alteradoPor)
        {
            EtapaId = etapaId;
            PosicaoKanban = posicaoKanban;
            if (Status == ECrmLeadStatus.Novo) Status = ECrmLeadStatus.EmQualificacao;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>CRM-009: marca o lead como convertido e grava os vínculos gerados.</summary>
        public void Converter(Guid? clienteId, Guid? contatoId, Guid? oportunidadeId, string alteradoPor)
        {
            Convertido = true;
            Status = ECrmLeadStatus.Convertido;
            ClienteId = clienteId;
            ContatoId = contatoId;
            OportunidadeId = oportunidadeId;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>CRM-008: lead arquivado sai da visão operacional padrão mas permanece consultável.</summary>
        public void Arquivar(string alteradoPor)
        {
            EstadoArquivo = ECrmEstadoArquivo.Arquivado;
            Status = ECrmLeadStatus.Arquivado;
            Ativo = false;
            MarcarAlterado(alteradoPor);
        }

        public void Restaurar(string alteradoPor)
        {
            EstadoArquivo = ECrmEstadoArquivo.Ativo;
            Status = Convertido ? ECrmLeadStatus.Convertido : ECrmLeadStatus.EmQualificacao;
            Ativo = true;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Usuário atribuído ao lead (crm_lead_usuario). Fonte: EF §10.6.</summary>
    public class CrmLeadUsuario : EntidadeSaaSBase
    {
        public Guid LeadId { get; private set; }
        public Guid UsuarioId { get; private set; }
        public string? Papel { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected CrmLeadUsuario() { }

        public CrmLeadUsuario(Guid leadId, Guid usuarioId, string? papel, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            LeadId = leadId;
            UsuarioId = usuarioId;
            Papel = papel;
            Ativo = true;
            AddNotifications(new Contract<CrmLeadUsuario>()
                .Requires()
                .AreNotEquals(leadId, Guid.Empty, nameof(LeadId), "O lead é obrigatório. [Origem: CrmLeadUsuario]")
                .AreNotEquals(usuarioId, Guid.Empty, nameof(UsuarioId), "O usuário é obrigatório. [Origem: CrmLeadUsuario]"));
        }
    }
}
