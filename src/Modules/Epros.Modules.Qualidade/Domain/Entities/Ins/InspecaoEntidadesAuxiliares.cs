using System;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Qualidade.Domain.Entities
{
    /// <summary>qld_ins_caracteristica — Caracteristica medida/avaliada do plano (secao 11.2).</summary>
    public class CaracteristicaPlano : EntidadeSaaSBase
    {
        public Guid PlanoId { get; private set; }
        public int Sequencia { get; private set; }
        public Guid? AtributoId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public ETipoCaracteristica TipoCaracteristica { get; private set; }
        public ETipoDadoCaracteristica TipoDado { get; private set; }
        public Guid? UnidadeMedidaId { get; private set; }
        public string? ValorNominal { get; private set; }
        public decimal? LimiteInferior { get; private set; }
        public decimal? LimiteSuperior { get; private set; }
        public string? CriterioQualitativo { get; private set; }
        public bool Obrigatoria { get; private set; }
        public string? MetodoMedicao { get; private set; }
        public Guid? InstrumentoId { get; private set; }

        protected CaracteristicaPlano() { }

        public CaracteristicaPlano(Guid planoId, int sequencia, string nome, ETipoCaracteristica tipoCaracteristica,
            ETipoDadoCaracteristica tipoDado, bool obrigatoria, Guid? atributoId, Guid? unidadeMedidaId,
            string? valorNominal, decimal? limiteInferior, decimal? limiteSuperior, string? criterioQualitativo,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<CaracteristicaPlano>()
                .Requires()
                .AreNotEquals(planoId, Guid.Empty, nameof(PlanoId), "O plano e obrigatorio [Origem: CaracteristicaPlano]")
                .IsGreaterOrEqualsThan(sequencia, 1, nameof(Sequencia), "A sequencia deve ser maior ou igual a 1 [Origem: CaracteristicaPlano]")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome da caracteristica e obrigatorio [Origem: CaracteristicaPlano]"));

            if (limiteInferior.HasValue && limiteSuperior.HasValue && limiteInferior > limiteSuperior)
                AddNotification(nameof(LimiteInferior), "O limite inferior nao pode ser maior que o superior [Origem: CaracteristicaPlano]");

            PlanoId = planoId;
            Sequencia = sequencia;
            Nome = nome;
            TipoCaracteristica = tipoCaracteristica;
            TipoDado = tipoDado;
            Obrigatoria = obrigatoria;
            AtributoId = atributoId;
            UnidadeMedidaId = unidadeMedidaId;
            ValorNominal = valorNominal;
            LimiteInferior = limiteInferior;
            LimiteSuperior = limiteSuperior;
            CriterioQualitativo = criterioQualitativo;
        }
    }

    /// <summary>qld_ins_regra_amostragem — Regra de amostra/AQL e criterio de aceite (secao 11.3).</summary>
    public class RegraAmostragem : EntidadeSaaSBase
    {
        public Guid PlanoId { get; private set; }
        public Guid? CaracteristicaId { get; private set; }
        public ETipoAmostragem TipoAmostragem { get; private set; }
        public string? NivelInspecao { get; private set; }
        public string? Aql { get; private set; }
        public decimal? FaixaLoteMin { get; private set; }
        public decimal? FaixaLoteMax { get; private set; }
        public int? TamanhoAmostra { get; private set; }
        public int? CriterioAceite { get; private set; }
        public int? CriterioRejeicao { get; private set; }
        public string? Severidade { get; private set; }

        protected RegraAmostragem() { }

        public RegraAmostragem(Guid planoId, ETipoAmostragem tipoAmostragem, Guid? caracteristicaId, string? nivelInspecao,
            string? aql, decimal? faixaLoteMin, decimal? faixaLoteMax, int? tamanhoAmostra, int? criterioAceite,
            int? criterioRejeicao, string? severidade, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<RegraAmostragem>()
                .Requires()
                .AreNotEquals(planoId, Guid.Empty, nameof(PlanoId), "O plano e obrigatorio [Origem: RegraAmostragem]"));

            if (tipoAmostragem == ETipoAmostragem.AQL && string.IsNullOrWhiteSpace(aql))
                AddNotification(nameof(Aql), "O AQL e obrigatorio para amostragem do tipo AQL [Origem: RegraAmostragem]");

            PlanoId = planoId;
            TipoAmostragem = tipoAmostragem;
            CaracteristicaId = caracteristicaId;
            NivelInspecao = nivelInspecao;
            Aql = aql;
            FaixaLoteMin = faixaLoteMin;
            FaixaLoteMax = faixaLoteMax;
            TamanhoAmostra = tamanhoAmostra;
            CriterioAceite = criterioAceite;
            CriterioRejeicao = criterioRejeicao;
            Severidade = severidade;
        }
    }

    /// <summary>qld_ins_execucao — Aplicacao do plano a uma referencia operacional (secao 11.4).</summary>
    public class ExecucaoInspecao : EntidadeSaaSBase
    {
        public Guid PlanoId { get; private set; }
        public EReferenciaExecucao ReferenciaTipo { get; private set; }
        public string? ReferenciaId { get; private set; }
        public decimal? QuantidadeLote { get; private set; }
        public int? TamanhoAmostraCalculado { get; private set; }
        public EStatusExecucaoInspecao Status { get; private set; }
        public Guid? InspetorId { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime? DataConclusao { get; private set; }
        public EResultadoPreliminar? ResultadoPreliminar { get; private set; }
        public string? Observacao { get; private set; }

        protected ExecucaoInspecao() { }

        public ExecucaoInspecao(Guid planoId, EReferenciaExecucao referenciaTipo, string? referenciaId,
            decimal? quantidadeLote, Guid? inspetorId, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ExecucaoInspecao>()
                .Requires()
                .AreNotEquals(planoId, Guid.Empty, nameof(PlanoId), "O plano e obrigatorio [Origem: ExecucaoInspecao]"));
            PlanoId = planoId;
            ReferenciaTipo = referenciaTipo;
            ReferenciaId = referenciaId;
            QuantidadeLote = quantidadeLote;
            InspetorId = inspetorId;
            Status = EStatusExecucaoInspecao.Aberta;
            DataInicio = DateTime.UtcNow;
        }

        /// <summary>Define o tamanho da amostra calculado pelo motor AQL (ou outro tipo de amostragem).</summary>
        public void DefinirAmostraCalculada(int tamanhoAmostra, string usuario)
        {
            if (tamanhoAmostra < 0)
            {
                AddNotification(nameof(TamanhoAmostraCalculado), "O tamanho da amostra nao pode ser negativo [Origem: ExecucaoInspecao]");
                return;
            }
            TamanhoAmostraCalculado = tamanhoAmostra;
            if (Status == EStatusExecucaoInspecao.Aberta)
                Status = EStatusExecucaoInspecao.EmColeta;
            MarcarAlterado(usuario);
        }

        public void Concluir(EResultadoPreliminar resultadoPreliminar, string? observacao, string usuario)
        {
            if (Status == EStatusExecucaoInspecao.Concluida || Status == EStatusExecucaoInspecao.Cancelada)
            {
                AddNotification(nameof(Status), "Execucao ja finalizada [Origem: ExecucaoInspecao]");
                return;
            }
            Status = EStatusExecucaoInspecao.Concluida;
            ResultadoPreliminar = resultadoPreliminar;
            Observacao = observacao;
            DataConclusao = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }

        public void Cancelar(string usuario)
        {
            Status = EStatusExecucaoInspecao.Cancelada;
            DataConclusao = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>qld_ins_amostra — Amostra selecionada na execucao (secao 11.5).</summary>
    public class AmostraInspecionada : EntidadeSaaSBase
    {
        public Guid ExecucaoId { get; private set; }
        public int Sequencia { get; private set; }
        public string? IdentificadorAmostra { get; private set; }
        public decimal? Quantidade { get; private set; }
        public EStatusAmostra Status { get; private set; }
        public string? Observacao { get; private set; }

        protected AmostraInspecionada() { }

        public AmostraInspecionada(Guid execucaoId, int sequencia, string? identificadorAmostra, decimal? quantidade,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AmostraInspecionada>()
                .Requires()
                .AreNotEquals(execucaoId, Guid.Empty, nameof(ExecucaoId), "A execucao e obrigatoria [Origem: AmostraInspecionada]")
                .IsGreaterOrEqualsThan(sequencia, 1, nameof(Sequencia), "A sequencia deve ser maior ou igual a 1 [Origem: AmostraInspecionada]"));
            ExecucaoId = execucaoId;
            Sequencia = sequencia;
            IdentificadorAmostra = identificadorAmostra;
            Quantidade = quantidade;
            Status = EStatusAmostra.Pendente;
        }

        public void RegistrarStatus(EStatusAmostra status, string usuario)
        {
            Status = status;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>qld_ins_medicao — Valor observado por caracteristica/amostra (secao 11.6).</summary>
    public class Medicao : EntidadeSaaSBase
    {
        public Guid ExecucaoId { get; private set; }
        public Guid? AmostraId { get; private set; }
        public Guid CaracteristicaId { get; private set; }
        public decimal? ValorDecimal { get; private set; }
        public string? ValorTexto { get; private set; }
        public bool? ValorBooleano { get; private set; }
        public EResultadoMedicao Resultado { get; private set; }
        public string? Desvio { get; private set; }
        public string? Observacao { get; private set; }
        public Guid MedidoPor { get; private set; }
        public DateTime MedidoEm { get; private set; }

        protected Medicao() { }

        public Medicao(Guid execucaoId, Guid caracteristicaId, EResultadoMedicao resultado, Guid medidoPor,
            Guid? amostraId, decimal? valorDecimal, string? valorTexto, bool? valorBooleano, string? desvio,
            string? observacao, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Medicao>()
                .Requires()
                .AreNotEquals(execucaoId, Guid.Empty, nameof(ExecucaoId), "A execucao e obrigatoria [Origem: Medicao]")
                .AreNotEquals(caracteristicaId, Guid.Empty, nameof(CaracteristicaId), "A caracteristica e obrigatoria [Origem: Medicao]")
                .AreNotEquals(medidoPor, Guid.Empty, nameof(MedidoPor), "O responsavel pela medicao e obrigatorio [Origem: Medicao]"));
            ExecucaoId = execucaoId;
            CaracteristicaId = caracteristicaId;
            Resultado = resultado;
            MedidoPor = medidoPor;
            AmostraId = amostraId;
            ValorDecimal = valorDecimal;
            ValorTexto = valorTexto;
            ValorBooleano = valorBooleano;
            Desvio = desvio;
            Observacao = observacao;
            MedidoEm = DateTime.UtcNow;
        }
    }

    /// <summary>qld_ins_resultado — Consolidacao tecnica da execucao (secao 11.7).</summary>
    public class ResultadoInspecao : EntidadeSaaSBase
    {
        public Guid ExecucaoId { get; private set; }
        public EResultadoInspecaoConsolidado Resultado { get; private set; }
        public int TotalAmostras { get; private set; }
        public int TotalDesvios { get; private set; }
        public string? CriterioAceiteAplicado { get; private set; }
        public bool GerarAcr { get; private set; }
        public bool GerarNcr { get; private set; }
        public string? Conclusao { get; private set; }
        public Guid ConcluidoPor { get; private set; }

        protected ResultadoInspecao() { }

        public ResultadoInspecao(Guid execucaoId, EResultadoInspecaoConsolidado resultado, int totalAmostras,
            int totalDesvios, bool gerarAcr, bool gerarNcr, Guid concluidoPor, string? criterioAceiteAplicado,
            string? conclusao, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ResultadoInspecao>()
                .Requires()
                .AreNotEquals(execucaoId, Guid.Empty, nameof(ExecucaoId), "A execucao e obrigatoria [Origem: ResultadoInspecao]")
                .IsGreaterOrEqualsThan(totalAmostras, 0, nameof(TotalAmostras), "O total de amostras nao pode ser negativo [Origem: ResultadoInspecao]")
                .IsGreaterOrEqualsThan(totalDesvios, 0, nameof(TotalDesvios), "O total de desvios nao pode ser negativo [Origem: ResultadoInspecao]")
                .AreNotEquals(concluidoPor, Guid.Empty, nameof(ConcluidoPor), "O responsavel pela conclusao e obrigatorio [Origem: ResultadoInspecao]"));

            if ((resultado == EResultadoInspecaoConsolidado.Reprovado || resultado == EResultadoInspecaoConsolidado.Inconclusivo)
                && string.IsNullOrWhiteSpace(conclusao))
                AddNotification(nameof(Conclusao), "A conclusao e obrigatoria quando reprovado/inconclusivo [Origem: ResultadoInspecao]");

            ExecucaoId = execucaoId;
            Resultado = resultado;
            TotalAmostras = totalAmostras;
            TotalDesvios = totalDesvios;
            GerarAcr = gerarAcr;
            GerarNcr = gerarNcr;
            ConcluidoPor = concluidoPor;
            CriterioAceiteAplicado = criterioAceiteAplicado;
            Conclusao = conclusao;
        }
    }
}
