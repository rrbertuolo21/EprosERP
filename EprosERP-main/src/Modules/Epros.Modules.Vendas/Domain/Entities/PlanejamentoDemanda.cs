using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Previsão de demanda (ven_demanda_previsao). Fonte: EF §14.1. Regras §9/§16.
    /// Cabeçalho por período/cenário/versão. Publicação de demanda só quando aprovada (§16).
    /// </summary>
    public class DemandaPrevisao : EntidadeSaaSBase
    {
        public Guid? EmpresaId { get; private set; }
        public string? Codigo { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public DateTime PeriodoInicio { get; private set; }
        public DateTime PeriodoFim { get; private set; }
        public EDemandaStatus Status { get; private set; } = EDemandaStatus.Rascunho;
        public Guid? CenarioVigenteId { get; private set; }
        public Guid? VersaoVigenteId { get; private set; }
        public string? Observacoes { get; private set; }
        public long? SequenciaExibicao { get; private set; }

        protected DemandaPrevisao() { }

        public DemandaPrevisao(
            Guid? empresaId,
            string? codigo,
            string nome,
            DateTime periodoInicio,
            DateTime periodoFim,
            string? observacoes,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            Codigo = codigo;
            Nome = nome;
            PeriodoInicio = periodoInicio;
            PeriodoFim = periodoFim;
            Observacoes = observacoes;
            Status = EDemandaStatus.Rascunho;
            Validar();
        }

        public void Alterar(string nome, DateTime periodoInicio, DateTime periodoFim, string? observacoes, string alteradoPor)
        {
            Nome = nome;
            PeriodoInicio = periodoInicio;
            PeriodoFim = periodoFim;
            Observacoes = observacoes;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void EnviarParaRevisao(string alteradoPor)
        {
            Status = EDemandaStatus.EmRevisao;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>§16: aprovação habilita publicação para estoque/produção. Exige versão ou cenário quando o processo exigir.</summary>
        public void Aprovar(Guid? cenarioVigenteId, Guid? versaoVigenteId, string alteradoPor)
        {
            Status = EDemandaStatus.Aprovado;
            CenarioVigenteId = cenarioVigenteId;
            VersaoVigenteId = versaoVigenteId;
            MarcarAlterado(alteradoPor);
        }

        public void Cancelar(string alteradoPor)
        {
            Status = EDemandaStatus.Cancelado;
            MarcarAlterado(alteradoPor);
        }

        public void Substituir(string alteradoPor)
        {
            Status = EDemandaStatus.Substituido;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<DemandaPrevisao>()
                .Requires()
                .IsNotNullOrEmpty(Nome, nameof(Nome), "O nome da previsão é obrigatório. [Origem: DemandaPrevisao]")
                // §16: período final deve ser posterior ao inicial.
                .IsGreaterThan(PeriodoFim, PeriodoInicio, nameof(PeriodoFim), "O período final deve ser posterior ao período inicial. [Origem: DemandaPrevisao]"));
        }
    }

    /// <summary>Cenário de simulação da previsão (ven_demanda_cenario). Fonte: EF §14.2.</summary>
    public class DemandaCenario : EntidadeSaaSBase
    {
        public Guid PrevisaoId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string? Tipo { get; private set; }
        public string? Descricao { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected DemandaCenario() { }

        public DemandaCenario(Guid previsaoId, string nome, string? tipo, string? descricao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PrevisaoId = previsaoId;
            Nome = nome;
            Tipo = tipo;
            Descricao = descricao;
            Ativo = true;
            AddNotifications(new Contract<DemandaCenario>()
                .Requires()
                .AreNotEquals(previsaoId, Guid.Empty, nameof(PrevisaoId), "A previsão é obrigatória. [Origem: DemandaCenario]")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do cenário é obrigatório. [Origem: DemandaCenario]"));
        }
    }

    /// <summary>Versão (corte histórico) da previsão (ven_demanda_versao). Fonte: EF §14.3.</summary>
    public class DemandaVersao : EntidadeSaaSBase
    {
        public Guid PrevisaoId { get; private set; }
        public string NumeroVersao { get; private set; } = string.Empty;
        public EDemandaVersaoStatus Status { get; private set; } = EDemandaVersaoStatus.Vigente;
        public DateTime DataGeracao { get; private set; }
        public Guid GeradoPor { get; private set; }
        public string? Observacoes { get; private set; }

        protected DemandaVersao() { }

        public DemandaVersao(Guid previsaoId, string numeroVersao, Guid geradoPor, string? observacoes, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PrevisaoId = previsaoId;
            NumeroVersao = numeroVersao;
            Status = EDemandaVersaoStatus.Vigente;
            DataGeracao = DateTime.UtcNow;
            GeradoPor = geradoPor;
            Observacoes = observacoes;
            AddNotifications(new Contract<DemandaVersao>()
                .Requires()
                .AreNotEquals(previsaoId, Guid.Empty, nameof(PrevisaoId), "A previsão é obrigatória. [Origem: DemandaVersao]")
                .IsNotNullOrEmpty(numeroVersao, nameof(NumeroVersao), "O número da versão é obrigatório. [Origem: DemandaVersao]"));
        }

        public void Substituir(string alteradoPor)
        {
            Status = EDemandaVersaoStatus.Substituida;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Item de previsão por produto/SKU (ven_demanda_item). Fonte: EF §14.4. §16.</summary>
    public class DemandaItem : EntidadeSaaSBase
    {
        public Guid PrevisaoId { get; private set; }
        public Guid? CenarioId { get; private set; }
        public Guid? VersaoId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public string Periodo { get; private set; } = string.Empty;
        public decimal? QuantidadeHistorica { get; private set; }
        public decimal QuantidadePrevista { get; private set; }
        public decimal? QuantidadeAjustada { get; private set; }
        public decimal? QuantidadeAprovada { get; private set; }
        public Guid? UnidadeMedidaId { get; private set; }
        public string? Observacoes { get; private set; }

        protected DemandaItem() { }

        public DemandaItem(
            Guid previsaoId,
            Guid? cenarioId,
            Guid? versaoId,
            Guid produtoId,
            string periodo,
            decimal? quantidadeHistorica,
            decimal quantidadePrevista,
            Guid? unidadeMedidaId,
            string? observacoes,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PrevisaoId = previsaoId;
            CenarioId = cenarioId;
            VersaoId = versaoId;
            ProdutoId = produtoId;
            Periodo = periodo;
            QuantidadeHistorica = quantidadeHistorica;
            QuantidadePrevista = quantidadePrevista;
            UnidadeMedidaId = unidadeMedidaId;
            Observacoes = observacoes;
            Validar();
        }

        /// <summary>§16 / critério 6: ajuste humano da quantidade.</summary>
        public void Ajustar(decimal quantidadeAjustada, string alteradoPor)
        {
            QuantidadeAjustada = quantidadeAjustada;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>§16 / critério 7: quantidade final aprovada.</summary>
        public void Aprovar(decimal quantidadeAprovada, string alteradoPor)
        {
            QuantidadeAprovada = quantidadeAprovada;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<DemandaItem>()
                .Requires()
                .AreNotEquals(PrevisaoId, Guid.Empty, nameof(PrevisaoId), "A previsão é obrigatória. [Origem: DemandaItem]")
                .AreNotEquals(ProdutoId, Guid.Empty, nameof(ProdutoId), "O produto/SKU é obrigatório. [Origem: DemandaItem]")
                .IsNotNullOrEmpty(Periodo, nameof(Periodo), "O período do item é obrigatório. [Origem: DemandaItem]")
                .IsGreaterOrEqualsThan(QuantidadePrevista, 0m, nameof(QuantidadePrevista), "A quantidade prevista é obrigatória. [Origem: DemandaItem]"));
        }
    }

    /// <summary>Consenso/aprovação da previsão (ven_demanda_consenso). Fonte: EF §14.5.</summary>
    public class DemandaConsenso : EntidadeSaaSBase
    {
        public Guid PrevisaoId { get; private set; }
        public Guid? CenarioId { get; private set; }
        public Guid? VersaoId { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public Guid? AprovadoPor { get; private set; }
        public DateTime? AprovadoEm { get; private set; }
        public string? Observacoes { get; private set; }

        protected DemandaConsenso() { }

        public DemandaConsenso(Guid previsaoId, Guid? cenarioId, Guid? versaoId, string status, Guid? aprovadoPor, DateTime? aprovadoEm, string? observacoes, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PrevisaoId = previsaoId;
            CenarioId = cenarioId;
            VersaoId = versaoId;
            Status = status;
            AprovadoPor = aprovadoPor;
            AprovadoEm = aprovadoEm;
            Observacoes = observacoes;
            AddNotifications(new Contract<DemandaConsenso>()
                .Requires()
                .AreNotEquals(previsaoId, Guid.Empty, nameof(PrevisaoId), "A previsão é obrigatória. [Origem: DemandaConsenso]")
                .IsNotNullOrEmpty(status, nameof(Status), "O status do consenso é obrigatório. [Origem: DemandaConsenso]"));
        }
    }

    /// <summary>Integração da demanda com processos posteriores (ven_demanda_integracao). Fonte: EF §14.6.</summary>
    public class DemandaIntegracao : EntidadeSaaSBase
    {
        public Guid PrevisaoId { get; private set; }
        public EDemandaIntegracaoDestino Destino { get; private set; }
        public EDemandaIntegracaoDirecao Direcao { get; private set; }
        public EDemandaIntegracaoStatus Status { get; private set; } = EDemandaIntegracaoStatus.Pendente;
        public DateTime? DataProcessamento { get; private set; }
        public string? Mensagem { get; private set; }

        protected DemandaIntegracao() { }

        public DemandaIntegracao(Guid previsaoId, EDemandaIntegracaoDestino destino, EDemandaIntegracaoDirecao direcao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PrevisaoId = previsaoId;
            Destino = destino;
            Direcao = direcao;
            Status = EDemandaIntegracaoStatus.Pendente;
            AddNotifications(new Contract<DemandaIntegracao>()
                .Requires()
                .AreNotEquals(previsaoId, Guid.Empty, nameof(PrevisaoId), "A previsão é obrigatória. [Origem: DemandaIntegracao]"));
        }

        public void MarcarProcessado(string? mensagem, string alteradoPor)
        {
            Status = EDemandaIntegracaoStatus.Processado;
            DataProcessamento = DateTime.UtcNow;
            Mensagem = mensagem;
            MarcarAlterado(alteradoPor);
        }

        public void MarcarErro(string? mensagem, string alteradoPor)
        {
            Status = EDemandaIntegracaoStatus.Erro;
            DataProcessamento = DateTime.UtcNow;
            Mensagem = mensagem;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Histórico/auditoria da previsão (ven_demanda_historico). Fonte: EF §14.7 / §17.</summary>
    public class DemandaHistorico : EntidadeSaaSBase
    {
        public Guid PrevisaoId { get; private set; }
        public DateTime DataHora { get; private set; }
        public Guid UsuarioId { get; private set; }
        public string Evento { get; private set; } = string.Empty;
        public string? Detalhe { get; private set; }
        public string? ValoresAnteriores { get; private set; }
        public string? ValoresNovos { get; private set; }

        protected DemandaHistorico() { }

        public DemandaHistorico(Guid previsaoId, Guid usuarioId, string evento, string? detalhe, string? valoresAnteriores, string? valoresNovos, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PrevisaoId = previsaoId;
            DataHora = DateTime.UtcNow;
            UsuarioId = usuarioId;
            Evento = evento;
            Detalhe = detalhe;
            ValoresAnteriores = valoresAnteriores;
            ValoresNovos = valoresNovos;
            AddNotifications(new Contract<DemandaHistorico>()
                .Requires()
                .AreNotEquals(previsaoId, Guid.Empty, nameof(PrevisaoId), "A previsão é obrigatória. [Origem: DemandaHistorico]"));
        }
    }
}
