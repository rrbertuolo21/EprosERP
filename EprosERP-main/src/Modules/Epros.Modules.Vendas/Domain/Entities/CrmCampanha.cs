using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>Campanha de marketing (crm_campanha). Fonte: EF §10.10. Regras CRM-038..CRM-050.</summary>
    public class CrmCampanha : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string Tipo { get; private set; } = string.Empty;
        public string Status { get; private set; } = string.Empty;
        public DateTime? DataInicio { get; private set; }
        public DateTime DataFim { get; private set; }
        public string? Frequencia { get; private set; }
        public Guid? MoedaId { get; private set; }
        public decimal? Orcamento { get; private set; }
        public decimal? CustoEsperado { get; private set; }
        public decimal? CustoReal { get; private set; }
        public decimal? ReceitaEsperada { get; private set; }
        public int? Impressoes { get; private set; }
        public string? Objetivo { get; private set; }
        public string? Conteudo { get; private set; }
        public Guid? ResponsavelUsuarioId { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected CrmCampanha() { }

        public CrmCampanha(
            string nome,
            string tipo,
            string status,
            DateTime? dataInicio,
            DateTime dataFim,
            string? frequencia,
            Guid? moedaId,
            decimal? orcamento,
            decimal? custoEsperado,
            decimal? custoReal,
            decimal? receitaEsperada,
            string? objetivo,
            string? conteudo,
            Guid? responsavelUsuarioId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Tipo = tipo;
            Status = status;
            DataInicio = dataInicio;
            DataFim = dataFim;
            // CRM-042: newsletter mantém frequência; outros tipos limpam.
            Frequencia = string.Equals(tipo, "Newsletter", StringComparison.OrdinalIgnoreCase) ? frequencia : null;
            MoedaId = moedaId;
            Orcamento = orcamento;
            CustoEsperado = custoEsperado;
            CustoReal = custoReal;
            ReceitaEsperada = receitaEsperada;
            Objetivo = objetivo;
            Conteudo = conteudo;
            ResponsavelUsuarioId = responsavelUsuarioId;
            Ativo = true;

            AddNotifications(new Contract<CrmCampanha>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "A campanha deve possuir nome. [Origem: CrmCampanha]")  // CRM-038
                .IsNotNullOrEmpty(tipo, nameof(Tipo), "A campanha deve possuir tipo. [Origem: CrmCampanha]")
                .IsNotNullOrEmpty(status, nameof(Status), "A campanha deve possuir status. [Origem: CrmCampanha]"));

            // CRM-039: data inicial deve ser anterior à data final quando ambas existirem.
            if (dataInicio.HasValue && dataInicio.Value.Date > dataFim.Date)
                AddNotification(nameof(DataInicio), "A data inicial da campanha deve ser anterior à data final. [Origem: CrmCampanha]");
        }

        public void Alterar(string nome, string status, DateTime? dataInicio, DateTime dataFim, decimal? orcamento, decimal? custoReal, string? conteudo, string alteradoPor)
        {
            Nome = nome;
            Status = status;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Orcamento = orcamento;
            CustoReal = custoReal;
            Conteudo = conteudo;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Lista de público (crm_lista_publico). Fonte: EF §10.11. CRM-051..CRM-054.</summary>
    public class CrmListaPublico : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string Tipo { get; private set; } = string.Empty;
        public string? DominioExclusao { get; private set; }
        public int ContagemMembros { get; private set; }
        public Guid? ResponsavelUsuarioId { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected CrmListaPublico() { }

        public CrmListaPublico(string nome, string tipo, string? dominioExclusao, Guid? responsavelUsuarioId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Tipo = tipo;
            DominioExclusao = dominioExclusao;
            ResponsavelUsuarioId = responsavelUsuarioId;
            ContagemMembros = 0;
            Ativo = true;
            AddNotifications(new Contract<CrmListaPublico>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "A lista de público deve possuir nome. [Origem: CrmListaPublico]")
                .IsNotNullOrEmpty(tipo, nameof(Tipo), "A lista de público deve possuir tipo. [Origem: CrmListaPublico]"));
        }

        /// <summary>CRM-053: contagem considera apenas membros ativos.</summary>
        public void AtualizarContagem(int contagemAtivos, string alteradoPor)
        {
            ContagemMembros = contagemAtivos < 0 ? 0 : contagemAtivos;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Membro de lista de público (crm_lista_publico_membro). Fonte: EF §10.12. CRM-052.</summary>
    public class CrmListaPublicoMembro : EntidadeSaaSBase
    {
        public Guid ListaPublicoId { get; private set; }
        public ECrmTipoMembro TipoMembro { get; private set; }
        public Guid? LeadId { get; private set; }
        public Guid? ContatoId { get; private set; }
        public Guid? ClienteId { get; private set; }
        public Guid? UsuarioId { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected CrmListaPublicoMembro() { }

        public CrmListaPublicoMembro(Guid listaPublicoId, ECrmTipoMembro tipoMembro, Guid? leadId, Guid? contatoId, Guid? clienteId, Guid? usuarioId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ListaPublicoId = listaPublicoId;
            TipoMembro = tipoMembro;
            LeadId = leadId;
            ContatoId = contatoId;
            ClienteId = clienteId;
            UsuarioId = usuarioId;
            Ativo = true;
            AddNotifications(new Contract<CrmListaPublicoMembro>()
                .Requires()
                .AreNotEquals(listaPublicoId, Guid.Empty, nameof(ListaPublicoId), "A lista de público é obrigatória. [Origem: CrmListaPublicoMembro]"));
        }
    }

    /// <summary>Vínculo campanha x lista de público (crm_campanha_lista). Fonte: EF §10.13.</summary>
    public class CrmCampanhaLista : EntidadeSaaSBase
    {
        public Guid CampanhaId { get; private set; }
        public Guid ListaPublicoId { get; private set; }
        public string? TipoUso { get; private set; }

        protected CrmCampanhaLista() { }

        public CrmCampanhaLista(Guid campanhaId, Guid listaPublicoId, string? tipoUso, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CampanhaId = campanhaId;
            ListaPublicoId = listaPublicoId;
            TipoUso = tipoUso;
            AddNotifications(new Contract<CrmCampanhaLista>()
                .Requires()
                .AreNotEquals(campanhaId, Guid.Empty, nameof(CampanhaId), "A campanha é obrigatória. [Origem: CrmCampanhaLista]")
                .AreNotEquals(listaPublicoId, Guid.Empty, nameof(ListaPublicoId), "A lista de público é obrigatória. [Origem: CrmCampanhaLista]"));
        }
    }

    /// <summary>Rastreador de campanha (crm_campanha_rastreador). Fonte: EF §10.14. CRM-045/CRM-046.</summary>
    public class CrmCampanhaRastreador : EntidadeSaaSBase
    {
        public Guid CampanhaId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Url { get; private set; } = string.Empty;
        public string? ChaveRastreamento { get; private set; }
        public bool OptOut { get; private set; }
        public string? UrlMensagem { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected CrmCampanhaRastreador() { }

        public CrmCampanhaRastreador(Guid campanhaId, string nome, string url, string? chaveRastreamento, bool optOut, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CampanhaId = campanhaId;
            Nome = nome;
            Url = url;
            ChaveRastreamento = chaveRastreamento;
            OptOut = optOut;
            Ativo = true;
            AddNotifications(new Contract<CrmCampanhaRastreador>()
                .Requires()
                .AreNotEquals(campanhaId, Guid.Empty, nameof(CampanhaId), "A campanha é obrigatória. [Origem: CrmCampanhaRastreador]")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O rastreador deve possuir nome. [Origem: CrmCampanhaRastreador]")
                .IsNotNullOrEmpty(url, nameof(Url), "O rastreador deve possuir URL. [Origem: CrmCampanhaRastreador]"));
        }
    }

    /// <summary>Log de campanha (crm_campanha_log). Fonte: EF §10.15. CRM-047/CRM-048.</summary>
    public class CrmCampanhaLog : EntidadeSaaSBase
    {
        public Guid CampanhaId { get; private set; }
        public Guid? RastreadorId { get; private set; }
        public string? AlvoTipo { get; private set; }
        public Guid? AlvoId { get; private set; }
        public string AtividadeTipo { get; private set; } = string.Empty;
        public Guid? MarketingId { get; private set; }
        public string? Email { get; private set; }
        public int Ocorrencias { get; private set; }
        public string? EnderecoIp { get; private set; }
        public DateTime DataEvento { get; private set; }

        protected CrmCampanhaLog() { }

        public CrmCampanhaLog(Guid campanhaId, Guid? rastreadorId, string? alvoTipo, Guid? alvoId, string atividadeTipo, Guid? marketingId, string? email, int ocorrencias, string? enderecoIp, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CampanhaId = campanhaId;
            RastreadorId = rastreadorId;
            AlvoTipo = alvoTipo;
            AlvoId = alvoId;
            AtividadeTipo = atividadeTipo;
            MarketingId = marketingId;
            Email = email;
            Ocorrencias = ocorrencias <= 0 ? 1 : ocorrencias;
            EnderecoIp = enderecoIp;
            DataEvento = DateTime.UtcNow;
            AddNotifications(new Contract<CrmCampanhaLog>()
                .Requires()
                .AreNotEquals(campanhaId, Guid.Empty, nameof(CampanhaId), "A campanha é obrigatória. [Origem: CrmCampanhaLog]")
                .IsNotNullOrEmpty(atividadeTipo, nameof(AtividadeTipo), "O tipo de atividade do log é obrigatório. [Origem: CrmCampanhaLog]"));
        }

        /// <summary>CRM-048: incrementa ocorrências quando log duplicado por campanha/alvo/rastreador.</summary>
        public void IncrementarOcorrencia(string alteradoPor)
        {
            Ocorrencias++;
            MarcarAlterado(alteradoPor);
        }
    }
}
