using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Workflow
{
    /// <summary>
    /// wf_anexo — anexos relacionados a instância, tarefa ou solicitação (referência ao GED). [Origem: EF WORKFLOW 10.13]
    /// </summary>
    public class WfAnexo : EntidadeSaaSBase
    {
        public string EntidadeTipo { get; private set; } = string.Empty; // instancia, tarefa, solicitacao
        public string EntidadeIdReferencia { get; private set; } = string.Empty;
        public string ArquivoId { get; private set; } = string.Empty;

        protected WfAnexo() { }

        public WfAnexo(string entidadeTipo, string entidadeIdReferencia, string arquivoId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            EntidadeTipo = entidadeTipo;
            EntidadeIdReferencia = entidadeIdReferencia;
            ArquivoId = arquivoId;

            AddNotifications(new Contract<WfAnexo>()
                .Requires()
                .IsNotNullOrEmpty(entidadeTipo, nameof(EntidadeTipo), "O tipo da entidade vinculada é obrigatório [Origem: WfAnexo]")
                .IsNotNullOrEmpty(entidadeIdReferencia, nameof(EntidadeIdReferencia), "O registro vinculado é obrigatório [Origem: WfAnexo]")
                .IsNotNullOrEmpty(arquivoId, nameof(ArquivoId), "A referência do arquivo é obrigatória [Origem: WfAnexo]"));
        }
    }

    /// <summary>
    /// wf_evento_dominio — eventos de domínio publicados após commit transacional. [Origem: EF WORKFLOW 10.13]
    /// </summary>
    public class WfEventoDominio : EntidadeSaaSBase
    {
        public string EntidadeTipo { get; private set; } = string.Empty;
        public string EntidadeIdReferencia { get; private set; } = string.Empty;
        public string Chave { get; private set; } = string.Empty;
        public string? Valor { get; private set; }
        public bool Publicado { get; private set; }
        public DateTime? PublicadoEm { get; private set; }

        protected WfEventoDominio() { }

        public WfEventoDominio(string entidadeTipo, string entidadeIdReferencia, string chave, string? valor, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            EntidadeTipo = entidadeTipo;
            EntidadeIdReferencia = entidadeIdReferencia;
            Chave = chave;
            Valor = valor;
            Publicado = false;

            AddNotifications(new Contract<WfEventoDominio>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do evento é obrigatória [Origem: WfEventoDominio]"));
        }

        public void MarcarPublicado(string alteradoPor)
        {
            Publicado = true;
            PublicadoEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>
    /// wf_parametro — parâmetros de workflow por tenant, sem deploy de código. [Origem: EF WORKFLOW 10.13]
    /// </summary>
    public class WfParametro : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string? Valor { get; private set; }

        protected WfParametro() { }

        public WfParametro(string chave, string? valor, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Chave = chave;
            Valor = valor;

            AddNotifications(new Contract<WfParametro>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do parâmetro é obrigatória [Origem: WfParametro]"));
        }

        public void Alterar(string? valor, string alteradoPor)
        {
            Valor = valor;
            MarcarAlterado(alteradoPor);
        }
    }
}
