using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Memoria e resultado reproduzivel do calculo de emissao (EF PEGADA_DE_CARBONO 11.2 Calculo).</summary>
    public class CalculoGee : EntidadeSaaSBase
    {
        public Guid DadoAtividadeId { get; private set; }
        public Guid FatorEmissaoId { get; private set; }
        public string FormulaVersao { get; private set; } = string.Empty;
        public decimal ResultadoGas { get; private set; }
        public decimal ResultadoCO2e { get; private set; }
        public string MemoriaCalculo { get; private set; } = string.Empty;

        protected CalculoGee() { } // EF Core

        public CalculoGee(
            Guid dadoAtividadeId,
            Guid fatorEmissaoId,
            string formulaVersao,
            decimal quantidade,
            decimal fatorValor,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            DadoAtividadeId = dadoAtividadeId;
            FatorEmissaoId = fatorEmissaoId;
            FormulaVersao = formulaVersao;
            // Formula minima homologavel: resultado = quantidade * fator (RN-GHG calculo reproduzivel).
            ResultadoGas = quantidade * fatorValor;
            ResultadoCO2e = quantidade * fatorValor;
            MemoriaCalculo = $"quantidade={quantidade};fator={fatorValor};formula={formulaVersao};co2e={ResultadoCO2e}";
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<CalculoGee>()
                .Requires()
                .AreNotEquals(DadoAtividadeId, Guid.Empty, nameof(DadoAtividadeId), "O dado de atividade e obrigatorio. [Origem: CalculoGee]")
                .AreNotEquals(FatorEmissaoId, Guid.Empty, nameof(FatorEmissaoId), "O fator de emissao e obrigatorio. [Origem: CalculoGee]")
                .IsNotNullOrEmpty(FormulaVersao, nameof(FormulaVersao), "A versao da formula e obrigatoria. [Origem: CalculoGee]"));
        }
    }
}
