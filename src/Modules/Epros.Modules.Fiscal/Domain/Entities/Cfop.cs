using System;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class Cfop : EntidadeSaaSBase, IGlobalEntity
    {
        public int CfopCodigo { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public string NaturezaOperacao { get; private set; } = string.Empty;
        public string CfopCorrelacao { get; private set; } = string.Empty;
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

        protected Cfop() { } // EF Core

        public Cfop(
            int cfopCodigo,
            string descricao,
            string naturezaOperacao,
            string cfopCorrelacao,
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
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            CfopCodigo = cfopCodigo;
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
            string cfopCorrelacao,
            string descricao,
            string naturezaOperacao,
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
            CfopCorrelacao = cfopCorrelacao;
            Descricao = descricao;
            NaturezaOperacao = naturezaOperacao;
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

            AddNotifications(new Contract<Cfop>()
                .Requires()
                .IsLowerOrEqualsThan(Descricao ?? string.Empty, 1000, nameof(Descricao), "O campo Descricao deve ter no máximo 1000 caracteres [Origem: Cfop]")
                .IsLowerOrEqualsThan(NaturezaOperacao ?? string.Empty, 1000, nameof(NaturezaOperacao), "O campo NaturezaOperacao deve ter no máximo 1000 caracteres [Origem: Cfop]")
                .IsLowerOrEqualsThan(CfopCorrelacao ?? string.Empty, 4, nameof(CfopCorrelacao), "O campo CfopCorrelacao deve ter no máximo 4 caracteres [Origem: Cfop]")
                .IsTrue(Enum.IsDefined(typeof(EIncidenciaSimples), IncidenciaSimples), nameof(IncidenciaSimples), "IncidenciaSimples não consta na lista [Origem: Cfop]")
            );

            ValidarCfopDevolucao();
        }

        private void ValidarCfopDevolucao()
        {
            if (string.IsNullOrWhiteSpace(CfopDevolucao))
                return;

            if (CfopDevolucao.Length > 4)
            {
                AddNotification(nameof(CfopDevolucao), "O campo CfopDevolucao deve ter no máximo 4 caracteres [Origem: Cfop]");
                return;
            }

            var cfopCodigoStr = CfopCodigo.ToString();
            if (cfopCodigoStr.Length == 0)
                return;

            char cfopCodigoInicial = cfopCodigoStr[0];

            /*
              cfop que inicia com 5 só posso deixar digitar cfop de devolução que inicia com 1 exemplo 5102 1202
              cfop que inicia com 6 só posso deixar digitar cfop de devolução que inicia com 2 exemplo 6102 2202
              cfop que inicia com 7 só posso deixar digitar cfop de devolução que inicia com 3 exemplo 7102 3202
              cfop que inicia com 1 só posso deixar digitar cfop de devolução que inicia com 5 exemplo 1102 5202
              cfop que inicia com 2 só posso deixar digitar cfop de devolução que inicia com 6 exemplo 2102 6202
              cfop que inicia com 3 só posso deixar digitar cfop de devolução que inicia com 7 exemplo 3102 7202
            */
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
