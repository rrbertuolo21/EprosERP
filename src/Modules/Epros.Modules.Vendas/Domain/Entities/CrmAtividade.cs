using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Atividade unificada do CRM (crm_atividade): tarefa, chamada, reunião, email, nota,
    /// discussão, arquivo ou log. Fonte: EF §10.9 e nota de autoria §14.1. Regras CRM-029..CRM-037.
    /// </summary>
    public class CrmAtividade : EntidadeSaaSBase
    {
        public ECrmEntidadeContexto EntidadeTipo { get; private set; }
        public Guid? LeadId { get; private set; }
        public Guid? OportunidadeId { get; private set; }
        public Guid? TicketId { get; private set; }
        public Guid? CampanhaId { get; private set; }
        public ECrmTipoAtividade TipoAtividade { get; private set; }
        public string? Nome { get; private set; }
        public string? Assunto { get; private set; }
        public DateTime? Data { get; private set; }
        public string? Hora { get; private set; }
        public ECrmPrioridade? Prioridade { get; private set; }
        public ECrmStatusAtividade? Status { get; private set; }
        public ECrmTipoChamada? TipoChamada { get; private set; }
        public string? Duracao { get; private set; }
        public string? Destinatario { get; private set; }
        public string? Descricao { get; private set; }
        public string? Resultado { get; private set; }
        public string? Comentario { get; private set; }
        public string? ArquivoNome { get; private set; }
        public string? ArquivoReferencia { get; private set; }
        public string? RecorrenciaJson { get; private set; }
        public Guid? UsuarioId { get; private set; }

        protected CrmAtividade() { }

        public CrmAtividade(
            ECrmEntidadeContexto entidadeTipo,
            ECrmTipoAtividade tipoAtividade,
            Guid? leadId,
            Guid? oportunidadeId,
            Guid? ticketId,
            Guid? campanhaId,
            string? nome,
            string? assunto,
            DateTime? data,
            string? hora,
            ECrmPrioridade? prioridade,
            ECrmStatusAtividade? status,
            ECrmTipoChamada? tipoChamada,
            string? duracao,
            string? destinatario,
            string? descricao,
            string? resultado,
            string? comentario,
            string? arquivoNome,
            string? arquivoReferencia,
            string? recorrenciaJson,
            Guid? usuarioId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EntidadeTipo = entidadeTipo;
            TipoAtividade = tipoAtividade;
            LeadId = leadId;
            OportunidadeId = oportunidadeId;
            TicketId = ticketId;
            CampanhaId = campanhaId;
            Nome = nome;
            Assunto = assunto;
            Data = data;
            Hora = hora;
            Prioridade = prioridade;
            Status = status;
            TipoChamada = tipoChamada;
            Duracao = duracao;
            Destinatario = destinatario;
            Descricao = descricao;
            Resultado = resultado;
            Comentario = comentario;
            ArquivoNome = arquivoNome;
            ArquivoReferencia = arquivoReferencia;
            RecorrenciaJson = recorrenciaJson;
            UsuarioId = usuarioId;
        }

        public void Concluir(string? resultado, string alteradoPor)
        {
            Status = TipoAtividade == ECrmTipoAtividade.Tarefa ? ECrmStatusAtividade.Completa : ECrmStatusAtividade.Realizada;
            if (resultado != null) Resultado = resultado;
            MarcarAlterado(alteradoPor);
        }
    }
}
