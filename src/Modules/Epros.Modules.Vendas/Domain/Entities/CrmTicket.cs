using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>Ticket de relacionamento (crm_ticket). Fonte: EF §10.18. Regras CRM-060..CRM-064.</summary>
    public class CrmTicket : EntidadeSaaSBase
    {
        public string? Numero { get; private set; }
        public string Titulo { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public Guid StatusId { get; private set; }
        public ECrmPrioridade? Prioridade { get; private set; }
        public Guid? CategoriaId { get; private set; }
        public Guid? ClienteId { get; private set; }
        public Guid? ProjetoId { get; private set; }
        public ECrmOrigemAtendimento? Origem { get; private set; }
        public ECrmEstadoArquivo EstadoArquivo { get; private set; } = ECrmEstadoArquivo.Ativo;
        public string? TipoUsuarioAbertura { get; private set; }
        public Guid? UsuarioAberturaId { get; private set; }
        public Guid? ContatoAberturaId { get; private set; }
        public string? CamposCustomizadosJson { get; private set; }
        public DateTime? UltimaAtualizacao { get; private set; }
        public DateTime? DataMudancaStatus { get; private set; }
        public DateTime? DataResolucao { get; private set; }

        protected CrmTicket() { }

        public CrmTicket(
            string? numero,
            string titulo,
            string? descricao,
            Guid statusId,
            ECrmPrioridade? prioridade,
            Guid? categoriaId,
            Guid? clienteId,
            Guid? projetoId,
            ECrmOrigemAtendimento? origem,
            string? tipoUsuarioAbertura,
            Guid? usuarioAberturaId,
            Guid? contatoAberturaId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Numero = numero;
            Titulo = titulo;
            Descricao = descricao;
            StatusId = statusId;
            Prioridade = prioridade;
            CategoriaId = categoriaId;
            ClienteId = clienteId;
            ProjetoId = projetoId;
            Origem = origem;
            TipoUsuarioAbertura = tipoUsuarioAbertura;
            UsuarioAberturaId = usuarioAberturaId;
            ContatoAberturaId = contatoAberturaId;
            EstadoArquivo = ECrmEstadoArquivo.Ativo;
            UltimaAtualizacao = DateTime.UtcNow;

            AddNotifications(new Contract<CrmTicket>()
                .Requires()
                .IsNotNullOrEmpty(titulo, nameof(Titulo), "O ticket deve possuir título. [Origem: CrmTicket]")
                .AreNotEquals(statusId, Guid.Empty, nameof(StatusId), "O ticket deve possuir status. [Origem: CrmTicket]"));
        }

        public void MudarStatus(Guid statusId, string alteradoPor)
        {
            StatusId = statusId;
            DataMudancaStatus = DateTime.UtcNow;
            UltimaAtualizacao = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Resolver(Guid statusResolvidoId, string alteradoPor)
        {
            StatusId = statusResolvidoId;
            DataResolucao = DateTime.UtcNow;
            DataMudancaStatus = DateTime.UtcNow;
            UltimaAtualizacao = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Arquivar(string alteradoPor)
        {
            EstadoArquivo = ECrmEstadoArquivo.Arquivado;
            UltimaAtualizacao = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Status de ticket configurável (crm_ticket_status). Fonte: EF §10.19. CRM-065.</summary>
    public class CrmTicketStatus : EntidadeSaaSBase
    {
        public string Titulo { get; private set; } = string.Empty;
        public string? Cor { get; private set; }
        public int Ordem { get; private set; }
        public bool? UsoClienteRespondeu { get; private set; }
        public bool? UsoEquipeRespondeu { get; private set; }
        public bool PadraoSistema { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected CrmTicketStatus() { }

        public CrmTicketStatus(string titulo, string? cor, int ordem, bool? usoClienteRespondeu, bool? usoEquipeRespondeu, bool padraoSistema, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Titulo = titulo;
            Cor = cor;
            Ordem = ordem;
            UsoClienteRespondeu = usoClienteRespondeu;
            UsoEquipeRespondeu = usoEquipeRespondeu;
            PadraoSistema = padraoSistema;
            Ativo = true;
            AddNotifications(new Contract<CrmTicketStatus>()
                .Requires()
                .IsNotNullOrEmpty(titulo, nameof(Titulo), "O status de ticket deve possuir título. [Origem: CrmTicketStatus]"));
        }
    }

    /// <summary>Resposta ou nota interna de ticket (crm_ticket_resposta). Fonte: EF §10.20. CRM-062/CRM-063.</summary>
    public class CrmTicketResposta : EntidadeSaaSBase
    {
        public Guid TicketId { get; private set; }
        public string Texto { get; private set; } = string.Empty;
        public ECrmTipoRespostaTicket Tipo { get; private set; }
        public ECrmOrigemAtendimento? Origem { get; private set; }
        public string? RemetenteTipo { get; private set; }
        public Guid? RemetenteId { get; private set; }
        public string? AnexosJson { get; private set; }

        protected CrmTicketResposta() { }

        public CrmTicketResposta(Guid ticketId, string texto, ECrmTipoRespostaTicket tipo, ECrmOrigemAtendimento? origem, string? remetenteTipo, Guid? remetenteId, string? anexosJson, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            TicketId = ticketId;
            Texto = texto;
            Tipo = tipo;
            Origem = origem;
            RemetenteTipo = remetenteTipo;
            RemetenteId = remetenteId;
            AnexosJson = anexosJson;
            AddNotifications(new Contract<CrmTicketResposta>()
                .Requires()
                .AreNotEquals(ticketId, Guid.Empty, nameof(TicketId), "O ticket é obrigatório. [Origem: CrmTicketResposta]")
                .IsNotNullOrEmpty(texto, nameof(Texto), "A resposta deve possuir texto. [Origem: CrmTicketResposta]"));
        }

        /// <summary>CRM-063: nota interna não dispara comunicação externa.</summary>
        public bool EhNotaInterna() => Tipo == ECrmTipoRespostaTicket.NotaInterna;
    }
}
