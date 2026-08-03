using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>
    /// Bloco de parâmetros DF-e (NF-e) portado fielmente de EmpresaParametrosDfeNfe (legado).
    /// Owned Type embutido em EmpresaParametrosDfe. Série/próximo número de produção e homologação,
    /// alíquota de crédito de ICMS, contingência, indicador ST e emissão de NF-e conjugada.
    /// </summary>
    public class ParametrosDfeNfe
    {
        public int NfeSerieProducao { get; private set; }
        public long NfeProximoNrProducao { get; private set; }
        public int NfeSerieHomologacao { get; private set; }
        public long NfeProximoNrHomologacao { get; private set; }
        public decimal ValorAliquotaCreditoIcms { get; private set; }
        public bool NfeGerarContingenciaEmHomologacao { get; private set; }
        public bool IndicadorSt { get; private set; }
        public bool EmitirNfeConjugada { get; private set; }

        protected ParametrosDfeNfe() { } // EF Core

        public ParametrosDfeNfe(int nfeSerieProducao, long nfeProximoNrProducao, int nfeSerieHomologacao, long nfeProximoNrHomologacao, decimal valorAliquotaCreditoIcms, bool nfeGerarContingenciaEmHomologacao, bool indicadorSt, bool emitirNfeConjugada)
        {
            NfeSerieProducao = nfeSerieProducao;
            NfeProximoNrProducao = nfeProximoNrProducao;
            NfeSerieHomologacao = nfeSerieHomologacao;
            NfeProximoNrHomologacao = nfeProximoNrHomologacao;
            ValorAliquotaCreditoIcms = valorAliquotaCreditoIcms;
            NfeGerarContingenciaEmHomologacao = nfeGerarContingenciaEmHomologacao;
            IndicadorSt = indicadorSt;
            EmitirNfeConjugada = emitirNfeConjugada;
        }

        public void Alterar(int nfeSerieProducao, long nfeProximoNrProducao, int nfeSerieHomologacao, long nfeProximoNrHomologacao, decimal valorAliquotaCreditoIcms, bool nfeGerarContingenciaEmHomologacao, bool indicadorSt, bool emitirNfeConjugada)
        {
            NfeSerieProducao = nfeSerieProducao;
            NfeProximoNrProducao = nfeProximoNrProducao;
            NfeSerieHomologacao = nfeSerieHomologacao;
            NfeProximoNrHomologacao = nfeProximoNrHomologacao;
            ValorAliquotaCreditoIcms = valorAliquotaCreditoIcms;
            NfeGerarContingenciaEmHomologacao = nfeGerarContingenciaEmHomologacao;
            IndicadorSt = indicadorSt;
            EmitirNfeConjugada = emitirNfeConjugada;
        }

        public void AtualizarNrNfeHomologacao() => NfeProximoNrHomologacao += 1;
        public void AtualizarNrNfeProducao() => NfeProximoNrProducao += 1;
    }

    /// <summary>
    /// Parâmetros DF-e (NFC-e) do ambiente de homologação. Porte fiel de EmpresaParametrosDfeNfceHomologacao.
    /// CSC/ID CSC, série e próximo número em homologação.
    /// </summary>
    public class ParametrosDfeNfceHomologacao
    {
        public string? NfceCscHomologacao { get; private set; }
        public string? NfceIdCscHomologacao { get; private set; }
        public int NfceSerieHomologacao { get; private set; }
        public long NfceProximoNrHomologacao { get; private set; }
        public bool NfceGerarContingenciaEmHomologacao { get; private set; }

        protected ParametrosDfeNfceHomologacao() { } // EF Core

        public ParametrosDfeNfceHomologacao(string? nfceCscHomologacao, string? nfceIdCscHomologacao, int nfceSerieHomologacao, long nfceProximoNrHomologacao, bool nfceGerarContingenciaEmHomologacao)
        {
            NfceCscHomologacao = nfceCscHomologacao;
            NfceIdCscHomologacao = nfceIdCscHomologacao;
            NfceSerieHomologacao = nfceSerieHomologacao;
            NfceProximoNrHomologacao = nfceProximoNrHomologacao;
            NfceGerarContingenciaEmHomologacao = nfceGerarContingenciaEmHomologacao;
        }

        public void AtualizarNrNfce() => NfceProximoNrHomologacao += 1;
    }

    /// <summary>
    /// Parâmetros DF-e (NFC-e) do ambiente de produção. Porte fiel de EmpresaParametrosDfeNfceProducao.
    /// CSC/ID CSC, série e próximo número em produção.
    /// </summary>
    public class ParametrosDfeNfceProducao
    {
        public string? NfceCscProducao { get; private set; }
        public string? NfceIdCscProducao { get; private set; }
        public int NfceSerieProducao { get; private set; }
        public long NfceProximoNrProducao { get; private set; }

        protected ParametrosDfeNfceProducao() { } // EF Core

        public ParametrosDfeNfceProducao(string? nfceCscProducao, string? nfceIdCscProducao, int nfceSerieProducao, long nfceProximoNrProducao)
        {
            NfceCscProducao = nfceCscProducao;
            NfceIdCscProducao = nfceIdCscProducao;
            NfceSerieProducao = nfceSerieProducao;
            NfceProximoNrProducao = nfceProximoNrProducao;
        }

        public void AtualizarNrNfce() => NfceProximoNrProducao += 1;
    }

    /// <summary>
    /// Parâmetros DF-e (NF-e/NFC-e) de uma Empresa. Porte fiel de
    /// Epros.ERP.Domain.Entities.Cadastros.Empresas.EmpresaParametrosDfe.
    /// Agrega os blocos NF-e, NFC-e homologação e NFC-e produção (owned types), o tipo de ambiente
    /// de cada modelo e o flag de destaque de ICMS ST. Preserva toda a validação de negócio do legado.
    /// </summary>
    public class EmpresaParametrosDfe : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public bool DestacarIcmsSt { get; private set; }

        public ParametrosDfeNfe? Nfe { get; private set; }
        public ParametrosDfeNfceHomologacao? NfceHomologacao { get; private set; }
        public ParametrosDfeNfceProducao? NfceProducao { get; private set; }

        public ETipoAmbiente TipoAmbienteNfce { get; private set; }
        public ETipoAmbiente TipoAmbienteNfe { get; private set; }

        protected EmpresaParametrosDfe() { } // EF Core

        public EmpresaParametrosDfe(
            Guid empresaId,
            bool destacarIcmsSt,
            ParametrosDfeNfe? nfe,
            ParametrosDfeNfceHomologacao? nfceHomologacao,
            ParametrosDfeNfceProducao? nfceProducao,
            ETipoAmbiente tipoAmbienteNfce,
            ETipoAmbiente tipoAmbienteNfe,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            DestacarIcmsSt = destacarIcmsSt;
            Nfe = nfe;
            NfceHomologacao = nfceHomologacao;
            NfceProducao = nfceProducao;
            TipoAmbienteNfce = tipoAmbienteNfce;
            TipoAmbienteNfe = tipoAmbienteNfe;
            Validar();
        }

        /// <summary>
        /// 1.07 — Default fiscal seguro: cria os parâmetros DF-e em ambiente de HOMOLOGAÇÃO para NF-e e
        /// NFC-e (TipoAmbiente = 2 = <see cref="ETipoAmbiente.Homologacao"/>), NUNCA produção. Regra de
        /// negócio (Especialista Fiscal): um emitente recém-criado jamais transmite documento em produção
        /// sem configuração deliberada (certificado, CSC, séries). Ponto único de default caso os
        /// parâmetros DF-e passem a ser semeados no onboarding ou em qualquer primeiro uso.
        /// </summary>
        public static EmpresaParametrosDfe CriarPadraoHomologacao(Guid empresaId, string tenantId, string criadoPor)
            => new EmpresaParametrosDfe(
                empresaId: empresaId,
                destacarIcmsSt: false,
                nfe: null,
                nfceHomologacao: null,
                nfceProducao: null,
                tipoAmbienteNfce: ETipoAmbiente.Homologacao,
                tipoAmbienteNfe: ETipoAmbiente.Homologacao,
                tenantId: tenantId,
                criadoPor: criadoPor);

        public void Alterar(bool destacarIcmsSt, ParametrosDfeNfe? nfe, ParametrosDfeNfceHomologacao? nfceHomologacao, ParametrosDfeNfceProducao? nfceProducao, ETipoAmbiente tipoAmbienteNfce, ETipoAmbiente tipoAmbienteNfe, string alteradoPor)
        {
            DestacarIcmsSt = destacarIcmsSt;
            Nfe = nfe;
            NfceHomologacao = nfceHomologacao;
            NfceProducao = nfceProducao;
            TipoAmbienteNfce = tipoAmbienteNfce;
            TipoAmbienteNfe = tipoAmbienteNfe;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<EmpresaParametrosDfe>()
                .Requires()
                .AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "EmpresaId campo obrigatório [Origem: EmpresaParametrosDfe]")
                .IsTrue(Enum.IsDefined(typeof(ETipoAmbiente), TipoAmbienteNfce), nameof(TipoAmbienteNfce), "Tipo Ambiente Nfc-e obrigatório [Origem: EmpresaParametrosDfe]")
                .IsTrue(Enum.IsDefined(typeof(ETipoAmbiente), TipoAmbienteNfe), nameof(TipoAmbienteNfe), "Tipo Ambiente Nf-e obrigatório [Origem: EmpresaParametrosDfe]")
            );

            if (TipoAmbienteNfce == ETipoAmbiente.Producao && NfceProducao == null)
                AddNotifications(new Contract<EmpresaParametrosDfe>().Requires()
                    .IsTrue(false, "NfceProducao", "Como foi selecionado tipo ambiente nfc-e como produção os campos são obrigatórios [Origem: EmpresaParametrosDfe]"));

            if (TipoAmbienteNfe == ETipoAmbiente.Producao && Nfe == null)
                AddNotifications(new Contract<EmpresaParametrosDfe>().Requires()
                    .IsTrue(false, "Nfe", "Como foi selecionado tipo ambiente nf-e como produção os campos de produção são obrigatórios [Origem: EmpresaParametrosDfe]"));

            if (TipoAmbienteNfce == ETipoAmbiente.Producao && NfceProducao != null)
            {
                if (string.IsNullOrEmpty(NfceProducao.NfceCscProducao))
                    AddNotifications(new Contract<EmpresaParametrosDfe>().Requires().IsTrue(false, "NfceCscProducao", "Nfc-e csc produção obrigatório [Origem: EmpresaParametrosDfe]"));

                if (string.IsNullOrEmpty(NfceProducao.NfceIdCscProducao))
                    AddNotifications(new Contract<EmpresaParametrosDfe>().Requires().IsTrue(false, "NfceIdCscProducao", "Nfc-e id csc produção obrigatório [Origem: EmpresaParametrosDfe]"));

                if (NfceProducao.NfceSerieProducao == 0)
                    AddNotifications(new Contract<EmpresaParametrosDfe>().Requires().IsTrue(false, "NfceSerieProducao", "Nfc-e série produção não pode iniciar como zero, obrigatório [Origem: EmpresaParametrosDfe]"));

                if (NfceProducao.NfceProximoNrProducao == 0)
                    AddNotifications(new Contract<EmpresaParametrosDfe>().Requires().IsTrue(false, "NfceProximoNrProducao", "Nfc-e próximo número produção não pode iniciar como zero, obrigatório [Origem: EmpresaParametrosDfe]"));
            }

            if (TipoAmbienteNfe == ETipoAmbiente.Producao && Nfe != null)
            {
                if (Nfe.NfeSerieProducao == 0)
                    AddNotifications(new Contract<EmpresaParametrosDfe>().Requires().IsTrue(false, "NfeSerieProducao", "Nf-e série produção não pode iniciar como zero, obrigatório [Origem: EmpresaParametrosDfe]"));

                if (Nfe.NfeProximoNrProducao == 0)
                    AddNotifications(new Contract<EmpresaParametrosDfe>().Requires().IsTrue(false, "NfeProximoNrProducao", "Nf-e próximo número produção não pode iniciar como zero, obrigatório [Origem: EmpresaParametrosDfe]"));
            }
        }

        public void AtualizarNrNfce()
        {
            if (TipoAmbienteNfce == ETipoAmbiente.Producao)
                NfceProducao?.AtualizarNrNfce();
            else
                NfceHomologacao?.AtualizarNrNfce();
        }

        public void AtualizarNrNfe()
        {
            if (TipoAmbienteNfe == ETipoAmbiente.Producao)
                Nfe?.AtualizarNrNfeProducao();
            else
                Nfe?.AtualizarNrNfeHomologacao();
        }

        public int ObterSerieNfce() => TipoAmbienteNfce == ETipoAmbiente.Producao ? (NfceProducao?.NfceSerieProducao ?? 0) : (NfceHomologacao?.NfceSerieHomologacao ?? 0);
        public long ObterNumeroNfce() => TipoAmbienteNfce == ETipoAmbiente.Producao ? (NfceProducao?.NfceProximoNrProducao ?? 0) : (NfceHomologacao?.NfceProximoNrHomologacao ?? 0);
        public string ObterCscNfce() => (TipoAmbienteNfce == ETipoAmbiente.Producao ? NfceProducao?.NfceCscProducao : NfceHomologacao?.NfceCscHomologacao) ?? string.Empty;
        public string ObterIdCscNfce() => (TipoAmbienteNfce == ETipoAmbiente.Producao ? NfceProducao?.NfceIdCscProducao : NfceHomologacao?.NfceIdCscHomologacao) ?? string.Empty;
        public int ObterSerieNfe() => TipoAmbienteNfe == ETipoAmbiente.Producao ? (Nfe?.NfeSerieProducao ?? 0) : (Nfe?.NfeSerieHomologacao ?? 0);
        public long ObterNumeroNfe() => TipoAmbienteNfe == ETipoAmbiente.Producao ? (Nfe?.NfeProximoNrProducao ?? 0) : (Nfe?.NfeProximoNrHomologacao ?? 0);
    }
}
