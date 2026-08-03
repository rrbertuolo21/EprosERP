using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    // ================= ESG-TSU (Transporte Sustentavel) =================
    // EF 14.06 §10.2. Construido do zero no V1 (CD1). O CALCULO ambiental (CO2e/tkm) usa fator
    // COMPARTILHADO com o GHG (mesmo catalogo esg.ghg_fator_emissao, NF-07/NF-04) e segue a Regra #0:
    // sem fator oficial vigente, o calculo fica "pendente de fator" — nunca inventa numero.

    /// <summary>Registro de transporte — agregado raiz (esg.tsu_registro). Codigo unico por tenant.</summary>
    public class TsuRegistro : AgregadoEsgComWorkflow
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public Guid ResponsavelId { get; private set; }

        protected TsuRegistro() { }

        public TsuRegistro(string codigo, string descricao, Guid responsavelId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            ResponsavelId = responsavelId;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<TsuRegistro>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo do registro e obrigatorio. [Origem: TsuRegistro]")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descricao e obrigatoria. [Origem: TsuRegistro]")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio. [Origem: TsuRegistro]"));
        }
    }

    /// <summary>Operacao/viagem de um registro (esg.tsu_operacao).</summary>
    public class TsuOperacao : EntidadeSaaSBase
    {
        public Guid RegistroTsuId { get; private set; }
        public string Referencia { get; private set; } = string.Empty; // NF/CT-e/OS
        public DateTime DataOperacao { get; private set; }
        public string Modal { get; private set; } = string.Empty; // Rodoviario, Ferroviario, Aquaviario, Aereo

        protected TsuOperacao() { }

        public TsuOperacao(Guid registroTsuId, string referencia, DateTime dataOperacao, string modal, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            RegistroTsuId = registroTsuId;
            Referencia = referencia;
            DataOperacao = dataOperacao.Date;
            Modal = modal;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<TsuOperacao>()
                .Requires()
                .AreNotEquals(RegistroTsuId, Guid.Empty, nameof(RegistroTsuId), "O registro e obrigatorio. [Origem: TsuOperacao]")
                .IsNotNullOrEmpty(Modal, nameof(Modal), "O modal e obrigatorio. [Origem: TsuOperacao]"));
        }
    }

    /// <summary>
    /// Trecho: segmento com modal, distancia e massa (esg.tsu_trecho). Carrega o dado de atividade
    /// (distancia, massa, energia/combustivel e unidades) — base do calculo tkm/CO2e (RN-TSU-013).
    /// </summary>
    public class TsuTrecho : EntidadeSaaSBase
    {
        public Guid OperacaoId { get; private set; }
        public string Modal { get; private set; } = string.Empty;
        public decimal Distancia { get; private set; }
        public string UnidadeDistancia { get; private set; } = string.Empty; // km
        public decimal Massa { get; private set; }
        public string UnidadeMassa { get; private set; } = string.Empty;     // t
        public decimal? EnergiaCombustivel { get; private set; }
        public string? UnidadeEnergia { get; private set; }

        protected TsuTrecho() { }

        public TsuTrecho(Guid operacaoId, string modal, decimal distancia, string unidadeDistancia, decimal massa, string unidadeMassa, decimal? energiaCombustivel, string? unidadeEnergia, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            OperacaoId = operacaoId;
            Modal = modal;
            Distancia = distancia;
            UnidadeDistancia = unidadeDistancia;
            Massa = massa;
            UnidadeMassa = unidadeMassa;
            EnergiaCombustivel = energiaCombustivel;
            UnidadeEnergia = unidadeEnergia;
            Validar();
        }

        /// <summary>Trabalho de transporte: tkm = massa × distancia (RN-TSU-013 / NF-07 default seguro).</summary>
        public decimal CalcularTkm() => Massa * Distancia;

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<TsuTrecho>()
                .Requires()
                .AreNotEquals(OperacaoId, Guid.Empty, nameof(OperacaoId), "A operacao e obrigatoria. [Origem: TsuTrecho]")
                .IsNotNullOrEmpty(Modal, nameof(Modal), "O modal do trecho e obrigatorio. [Origem: TsuTrecho]")
                .IsGreaterThan(Distancia, 0, nameof(Distancia), "A distancia deve ser maior que zero. [Origem: TsuTrecho]")
                .IsNotNullOrEmpty(UnidadeDistancia, nameof(UnidadeDistancia), "A unidade de distancia e obrigatoria. [Origem: TsuTrecho]")
                .IsGreaterThan(Massa, 0, nameof(Massa), "A massa transportada deve ser maior que zero. [Origem: TsuTrecho]")
                .IsNotNullOrEmpty(UnidadeMassa, nameof(UnidadeMassa), "A unidade de massa e obrigatoria. [Origem: TsuTrecho]"));
        }
    }

    /// <summary>
    /// Calculo ambiental do trecho (esg.tsu_calculo). tkm = massa × distancia;
    /// CO2e = tkm × fator.Valor (fator do catalogo COMPARTILHADO com GHG). Intensidade = CO2e/tkm.
    /// RN-TSU-014: grava fator+versao. Regra #0: sem fator vigente -> FatorPendente, sem numero.
    /// </summary>
    public class TsuCalculo : EntidadeSaaSBase
    {
        public Guid TrechoId { get; private set; }
        public decimal Tkm { get; private set; }
        public decimal? ResultadoCO2e { get; private set; }   // null quando pendente de fator
        public decimal? Intensidade { get; private set; }     // CO2e/tkm
        public string FormulaVersao { get; private set; } = string.Empty;
        public string? FatorCodigo { get; private set; }
        public string? FatorVersao { get; private set; }
        public string? FatorFonte { get; private set; }
        public bool FatorPendente { get; private set; }
        public string MemoriaCalculo { get; private set; } = string.Empty;

        protected TsuCalculo() { }

        private TsuCalculo(Guid trechoId, decimal tkm, string formulaVersao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            TrechoId = trechoId;
            Tkm = tkm;
            FormulaVersao = formulaVersao;
            Validar();
        }

        public static TsuCalculo CalculadoComFator(Guid trechoId, decimal tkm, string formulaVersao, FatorEmissaoGee fator, string tenantId, string criadoPor)
        {
            if (fator == null) throw new ArgumentNullException(nameof(fator));
            var c = new TsuCalculo(trechoId, tkm, formulaVersao, tenantId, criadoPor);
            c.ResultadoCO2e = tkm * fator.Valor;
            c.Intensidade = tkm != 0 ? c.ResultadoCO2e / tkm : 0m; // = fator.Valor (CO2e/tkm)
            c.FatorCodigo = fator.Codigo;
            c.FatorVersao = fator.Versao;
            c.FatorFonte = fator.FonteReferencia;
            c.FatorPendente = false;
            c.MemoriaCalculo = $"tkm={tkm};fator={fator.Codigo}@{fator.Versao}={fator.Valor};co2e={c.ResultadoCO2e};formula={formulaVersao}";
            return c;
        }

        public static TsuCalculo PendenteDeFator(Guid trechoId, decimal tkm, string formulaVersao, string fatorCodigoEsperado, string tenantId, string criadoPor)
        {
            var c = new TsuCalculo(trechoId, tkm, formulaVersao, tenantId, criadoPor);
            c.ResultadoCO2e = null; // Regra #0: nao inventa numero
            c.Intensidade = null;
            c.FatorCodigo = fatorCodigoEsperado;
            c.FatorPendente = true;
            c.MemoriaCalculo = $"tkm={tkm};fator={fatorCodigoEsperado}=PENDENTE;co2e=PENDENTE;formula={formulaVersao}";
            return c;
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<TsuCalculo>()
                .Requires()
                .AreNotEquals(TrechoId, Guid.Empty, nameof(TrechoId), "O trecho e obrigatorio. [Origem: TsuCalculo]")
                .IsNotNullOrEmpty(FormulaVersao, nameof(FormulaVersao), "A versao da formula e obrigatoria. [Origem: TsuCalculo]"));
        }
    }

    /// <summary>Meta de participacao modal (esg.tsu_meta_modal).</summary>
    public class TsuMetaModal : EntidadeSaaSBase
    {
        public Guid RegistroTsuId { get; private set; }
        public string Modal { get; private set; } = string.Empty;
        public decimal ParticipacaoAlvo { get; private set; } // %
        public DateTime PeriodoInicio { get; private set; }
        public DateTime PeriodoFim { get; private set; }

        protected TsuMetaModal() { }

        public TsuMetaModal(Guid registroTsuId, string modal, decimal participacaoAlvo, DateTime periodoInicio, DateTime periodoFim, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            RegistroTsuId = registroTsuId;
            Modal = modal;
            ParticipacaoAlvo = participacaoAlvo;
            PeriodoInicio = periodoInicio.Date;
            PeriodoFim = periodoFim.Date;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<TsuMetaModal>()
                .Requires()
                .AreNotEquals(RegistroTsuId, Guid.Empty, nameof(RegistroTsuId), "O registro e obrigatorio. [Origem: TsuMetaModal]")
                .IsNotNullOrEmpty(Modal, nameof(Modal), "O modal e obrigatorio. [Origem: TsuMetaModal]")
                .IsGreaterThan(ParticipacaoAlvo, -0.001m, nameof(ParticipacaoAlvo), "A participacao alvo nao pode ser negativa. [Origem: TsuMetaModal]")
                .IsFalse(PeriodoFim < PeriodoInicio, nameof(PeriodoFim), "O fim do periodo deve ser posterior ao inicio. [Origem: TsuMetaModal]"));
        }
    }

    /// <summary>Medicao de participacao modal realizada (esg.tsu_medicao).</summary>
    public class TsuMedicao : EntidadeSaaSBase
    {
        public Guid MetaModalId { get; private set; }
        public string Periodo { get; private set; } = string.Empty;
        public decimal ParticipacaoRealizada { get; private set; }

        protected TsuMedicao() { }

        public TsuMedicao(Guid metaModalId, string periodo, decimal participacaoRealizada, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            MetaModalId = metaModalId;
            Periodo = periodo;
            ParticipacaoRealizada = participacaoRealizada;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<TsuMedicao>()
                .Requires()
                .AreNotEquals(MetaModalId, Guid.Empty, nameof(MetaModalId), "A meta modal e obrigatoria. [Origem: TsuMedicao]")
                .IsNotNullOrEmpty(Periodo, nameof(Periodo), "O periodo e obrigatorio. [Origem: TsuMedicao]"));
        }
    }

    /// <summary>Parametro do tenant para TSU (esg.tsu_parametro). Chave/ValorJson (unidades, precedencia de fator).</summary>
    public class TsuParametro : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string ValorJson { get; private set; } = string.Empty;

        protected TsuParametro() { }

        public TsuParametro(string chave, string valorJson, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Chave = chave;
            ValorJson = valorJson;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<TsuParametro>()
                .Requires()
                .IsNotNullOrEmpty(Chave, nameof(Chave), "A chave do parametro e obrigatoria. [Origem: TsuParametro]")
                .IsNotNullOrEmpty(ValorJson, nameof(ValorJson), "O valor do parametro e obrigatorio. [Origem: TsuParametro]"));
        }
    }
}
