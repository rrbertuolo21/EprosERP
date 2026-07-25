using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolParametro : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public string Competencia { get; private set; } = string.Empty;
        public string ContribuiPis { get; private set; } = string.Empty;
        public decimal? AliquotaPis { get; private set; }
        public string DiscriminarDsr { get; private set; } = string.Empty;
        public string DiaPagamento { get; private set; } = string.Empty;
        public string CalculoProporcionalidade { get; private set; } = string.Empty;
        public string DescontarFaltas13 { get; private set; } = string.Empty;
        public string PagarAdicionais13 { get; private set; } = string.Empty;
        public string MesAdiantamento13 { get; private set; } = string.Empty;
        public decimal? PercentualAdiantamento13 { get; private set; }
        public string FeriasDescontarFaltas { get; private set; } = string.Empty;
        public string FeriasPagarAdicionais { get; private set; } = string.Empty;
        public string FeriasAdiantar13 { get; private set; } = string.Empty;
        public string FeriasPagarEstagiarios { get; private set; } = string.Empty;
        public string FeriasCalcJustaCausa { get; private set; } = string.Empty;
        public string FeriasMovimentoMensal { get; private set; } = string.Empty;

        protected FolParametro() { } // EF Core

        public FolParametro(
            Guid empresaId,
            string competencia,
            string contribuiPis,
            decimal? aliquotaPis,
            string discriminarDsr,
            string diaPagamento,
            string calculoProporcionalidade,
            string descontarFaltas13,
            string pagarAdicionais13,
            string mesAdiantamento13,
            decimal? percentualAdiantamento13,
            string feriasDescontarFaltas,
            string feriasPagarAdicionais,
            string feriasAdiantar13,
            string feriasPagarEstagiarios,
            string feriasCalcJustaCausa,
            string feriasMovimentoMensal,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            Competencia = competencia;
            ContribuiPis = contribuiPis;
            AliquotaPis = aliquotaPis;
            DiscriminarDsr = discriminarDsr;
            DiaPagamento = diaPagamento;
            CalculoProporcionalidade = calculoProporcionalidade;
            DescontarFaltas13 = descontarFaltas13;
            PagarAdicionais13 = pagarAdicionais13;
            MesAdiantamento13 = mesAdiantamento13;
            PercentualAdiantamento13 = percentualAdiantamento13;
            FeriasDescontarFaltas = feriasDescontarFaltas;
            FeriasPagarAdicionais = feriasPagarAdicionais;
            FeriasAdiantar13 = feriasAdiantar13;
            FeriasPagarEstagiarios = feriasPagarEstagiarios;
            FeriasCalcJustaCausa = feriasCalcJustaCausa;
            FeriasMovimentoMensal = feriasMovimentoMensal;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolParametro>().Requires();
            contract.AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "O campo EmpresaId e obrigatorio.");
            contract.IsNotNullOrEmpty(Competencia, nameof(Competencia), "O campo Competencia e obrigatorio.");
            contract.IsNotNullOrEmpty(ContribuiPis, nameof(ContribuiPis), "O campo ContribuiPis e obrigatorio.");
            contract.IsNotNullOrEmpty(DiscriminarDsr, nameof(DiscriminarDsr), "O campo DiscriminarDsr e obrigatorio.");
            contract.IsNotNullOrEmpty(DiaPagamento, nameof(DiaPagamento), "O campo DiaPagamento e obrigatorio.");
            contract.IsNotNullOrEmpty(CalculoProporcionalidade, nameof(CalculoProporcionalidade), "O campo CalculoProporcionalidade e obrigatorio.");
            contract.IsNotNullOrEmpty(DescontarFaltas13, nameof(DescontarFaltas13), "O campo DescontarFaltas13 e obrigatorio.");
            contract.IsNotNullOrEmpty(PagarAdicionais13, nameof(PagarAdicionais13), "O campo PagarAdicionais13 e obrigatorio.");
            contract.IsNotNullOrEmpty(MesAdiantamento13, nameof(MesAdiantamento13), "O campo MesAdiantamento13 e obrigatorio.");
            contract.IsNotNullOrEmpty(FeriasDescontarFaltas, nameof(FeriasDescontarFaltas), "O campo FeriasDescontarFaltas e obrigatorio.");
            contract.IsNotNullOrEmpty(FeriasPagarAdicionais, nameof(FeriasPagarAdicionais), "O campo FeriasPagarAdicionais e obrigatorio.");
            contract.IsNotNullOrEmpty(FeriasAdiantar13, nameof(FeriasAdiantar13), "O campo FeriasAdiantar13 e obrigatorio.");
            contract.IsNotNullOrEmpty(FeriasPagarEstagiarios, nameof(FeriasPagarEstagiarios), "O campo FeriasPagarEstagiarios e obrigatorio.");
            contract.IsNotNullOrEmpty(FeriasCalcJustaCausa, nameof(FeriasCalcJustaCausa), "O campo FeriasCalcJustaCausa e obrigatorio.");
            contract.IsNotNullOrEmpty(FeriasMovimentoMensal, nameof(FeriasMovimentoMensal), "O campo FeriasMovimentoMensal e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
