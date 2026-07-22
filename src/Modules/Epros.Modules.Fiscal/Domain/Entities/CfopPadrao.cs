using System;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class CfopPadrao : EntidadeSaaSBase, IGlobalEntity
    {
        public int CfopCodigo { get; private set; }
        public DateTime DataInicioVigencia { get; private set; }
        public DateTime? DataFimVigencia { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public string NaturezaOperacao { get; private set; } = string.Empty;
        public string? CfopCorrelacao { get; private set; }
        public bool IntegraFaturamento { get; private set; }
        public bool IndicadorNfe { get; private set; }
        public bool IndicadorComunicacao { get; private set; }
        public bool IndicadorTransporte { get; private set; }
        public bool IndicadorDevolucao { get; private set; }
        public bool IndicadorRetorno { get; private set; }
        public bool IndicadorAnulacao { get; private set; }
        public bool IndicadorRemessa { get; private set; }
        public bool IndicadorCombustivel { get; private set; }
        public bool IndicadorTransferencia { get; private set; }
        public bool IndicadorNfce { get; private set; }
        public bool IndicadorCiap { get; private set; }
        public bool IndicadorUsoConsumo { get; private set; }
        public bool IndicadorUsoSemOperacao { get; private set; }
        public bool IndicadorSt { get; private set; }
        public bool IndicadorMei { get; private set; }
        public EIncidenciaSimples IncidenciaSimples { get; private set; }
        public string? CfopDevolucao { get; private set; }

        protected CfopPadrao() { } // EF Core

        public CfopPadrao(
            int cfopCodigo,
            DateTime dataInicioVigencia,
            DateTime? dataFimVigencia,
            string descricao,
            string naturezaOperacao,
            string? cfopCorrelacao,
            bool integraFaturamento,
            bool indicadorNfe,
            bool indicadorComunicacao,
            bool indicadorTransporte,
            bool indicadorDevolucao,
            bool indicadorRetorno,
            bool indicadorAnulacao,
            bool indicadorRemessa,
            bool indicadorCombustivel,
            bool indicadorTransferencia,
            bool indicadorNfce,
            bool indicadorCiap,
            bool indicadorUsoConsumo,
            bool indicadorUsoSemOperacao,
            bool indicadorSt,
            bool indicadorMei,
            EIncidenciaSimples incidenciaSimples,
            string? cfopDevolucao,
            string criadoPor) : base("system", criadoPor)
        {
            CfopCodigo = cfopCodigo;
            DataInicioVigencia = dataInicioVigencia;
            DataFimVigencia = dataFimVigencia;
            Descricao = descricao;
            NaturezaOperacao = naturezaOperacao;
            CfopCorrelacao = cfopCorrelacao;
            IntegraFaturamento = integraFaturamento;
            IndicadorNfe = indicadorNfe;
            IndicadorComunicacao = indicadorComunicacao;
            IndicadorTransporte = indicadorTransporte;
            IndicadorDevolucao = indicadorDevolucao;
            IndicadorRetorno = indicadorRetorno;
            IndicadorAnulacao = indicadorAnulacao;
            IndicadorRemessa = indicadorRemessa;
            IndicadorCombustivel = indicadorCombustivel;
            IndicadorTransferencia = indicadorTransferencia;
            IndicadorNfce = indicadorNfce;
            IndicadorCiap = indicadorCiap;
            IndicadorUsoConsumo = indicadorUsoConsumo;
            IndicadorUsoSemOperacao = indicadorUsoSemOperacao;
            IndicadorSt = indicadorSt;
            IndicadorMei = indicadorMei;
            IncidenciaSimples = incidenciaSimples;
            CfopDevolucao = cfopDevolucao;
            Validar();
        }

        public void Alterar(
            DateTime dataInicioVigencia,
            DateTime? dataFimVigencia,
            string descricao,
            string naturezaOperacao,
            string? cfopCorrelacao,
            bool integraFaturamento,
            bool indicadorNfe,
            bool indicadorComunicacao,
            bool indicadorTransporte,
            bool indicadorDevolucao,
            bool indicadorRetorno,
            bool indicadorAnulacao,
            bool indicadorRemessa,
            bool indicadorCombustivel,
            bool indicadorTransferencia,
            bool indicadorNfce,
            bool indicadorCiap,
            bool indicadorUsoConsumo,
            bool indicadorUsoSemOperacao,
            bool indicadorSt,
            bool indicadorMei,
            EIncidenciaSimples incidenciaSimples,
            string? cfopDevolucao,
            string alteradoPor)
        {
            DataInicioVigencia = dataInicioVigencia;
            DataFimVigencia = dataFimVigencia;
            Descricao = descricao;
            NaturezaOperacao = naturezaOperacao;
            CfopCorrelacao = cfopCorrelacao;
            IntegraFaturamento = integraFaturamento;
            IndicadorNfe = indicadorNfe;
            IndicadorComunicacao = indicadorComunicacao;
            IndicadorTransporte = indicadorTransporte;
            IndicadorDevolucao = indicadorDevolucao;
            IndicadorRetorno = indicadorRetorno;
            IndicadorAnulacao = indicadorAnulacao;
            IndicadorRemessa = indicadorRemessa;
            IndicadorCombustivel = indicadorCombustivel;
            IndicadorTransferencia = indicadorTransferencia;
            IndicadorNfce = indicadorNfce;
            IndicadorCiap = indicadorCiap;
            IndicadorUsoConsumo = indicadorUsoConsumo;
            IndicadorUsoSemOperacao = indicadorUsoSemOperacao;
            IndicadorSt = indicadorSt;
            IndicadorMei = indicadorMei;
            IncidenciaSimples = incidenciaSimples;
            CfopDevolucao = cfopDevolucao;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();

            AddNotifications(new Contract<CfopPadrao>()
                .Requires()
                .IsLowerOrEqualsThan(Descricao ?? string.Empty, 1000, nameof(Descricao), "O campo Descricao deve ter no máximo 1000 caracteres [Origem: CfopPadrao]")
                .IsLowerOrEqualsThan(NaturezaOperacao ?? string.Empty, 1000, nameof(NaturezaOperacao), "O campo NaturezaOperacao deve ter no máximo 1000 caracteres [Origem: CfopPadrao]")
                .IsLowerOrEqualsThan(CfopCorrelacao ?? string.Empty, 4, nameof(CfopCorrelacao), "O campo CfopCorrelacao deve ter no máximo 4 caracteres [Origem: CfopPadrao]")
                .IsTrue(Enum.IsDefined(typeof(EIncidenciaSimples), IncidenciaSimples), nameof(IncidenciaSimples), "IncidenciaSimples não consta na lista [Origem: CfopPadrao]")
            );

            ValidarCfopDevolucao();
        }

        private void ValidarCfopDevolucao()
        {
            if (string.IsNullOrWhiteSpace(CfopDevolucao))
                return;

            if (CfopDevolucao.Length > 4)
            {
                AddNotification(nameof(CfopDevolucao), "O campo CfopDevolucao deve ter no máximo 4 caracteres [Origem: CfopPadrao]");
                return;
            }

            var cfopCodigoStr = CfopCodigo.ToString();
            if (cfopCodigoStr.Length == 0)
                return;

            char cfopCodigoInicial = cfopCodigoStr[0];

            switch (cfopCodigoInicial)
            {
                case '1':
                    if (!CfopDevolucao.StartsWith("5"))
                        AddNotification(nameof(CfopDevolucao), "CFOP de devolução deve iniciar com 5");
                    break;
                case '2':
                    if (!CfopDevolucao.StartsWith("6"))
                        AddNotification(nameof(CfopDevolucao), "CFOP de devolução deve iniciar com 6");
                    break;
                case '3':
                    if (!CfopDevolucao.StartsWith("7"))
                        AddNotification(nameof(CfopDevolucao), "CFOP de devolução deve iniciar com 7");
                    break;
                case '5':
                    if (!CfopDevolucao.StartsWith("1"))
                        AddNotification(nameof(CfopDevolucao), "CFOP de devolução deve iniciar com 1");
                    break;
                case '6':
                    if (!CfopDevolucao.StartsWith("2"))
                        AddNotification(nameof(CfopDevolucao), "CFOP de devolução deve iniciar com 2");
                    break;
                case '7':
                    if (!CfopDevolucao.StartsWith("3"))
                        AddNotification(nameof(CfopDevolucao), "CFOP de devolução deve iniciar com 3");
                    break;
            }
        }
    }
}
