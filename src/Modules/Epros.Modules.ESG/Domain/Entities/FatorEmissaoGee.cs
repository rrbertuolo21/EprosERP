using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Coeficiente de emissao versionado (EF PEGADA_DE_CARBONO 11.2 Fator). Entidade mestre.</summary>
    public class FatorEmissaoGee : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Versao { get; private set; } = string.Empty;
        public string FonteReferencia { get; private set; } = string.Empty;
        public decimal Valor { get; private set; }
        public string UnidadeEntrada { get; private set; } = string.Empty;
        public string UnidadeSaida { get; private set; } = string.Empty;
        public DateTime InicioVigencia { get; private set; }
        public DateTime? FimVigencia { get; private set; }

        protected FatorEmissaoGee() { } // EF Core

        public FatorEmissaoGee(
            string codigo,
            string versao,
            string fonteReferencia,
            decimal valor,
            string unidadeEntrada,
            string unidadeSaida,
            DateTime inicioVigencia,
            DateTime? fimVigencia,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Versao = versao;
            FonteReferencia = fonteReferencia;
            Valor = valor;
            UnidadeEntrada = unidadeEntrada;
            UnidadeSaida = unidadeSaida;
            InicioVigencia = inicioVigencia.Date;
            FimVigencia = fimVigencia?.Date;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<FatorEmissaoGee>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo do fator e obrigatorio. [Origem: FatorEmissaoGee]")
                .IsNotNullOrEmpty(Versao, nameof(Versao), "A versao do fator e obrigatoria. [Origem: FatorEmissaoGee]")
                .IsNotNullOrEmpty(FonteReferencia, nameof(FonteReferencia), "A fonte de referencia e obrigatoria. [Origem: FatorEmissaoGee]")
                .IsNotNullOrEmpty(UnidadeEntrada, nameof(UnidadeEntrada), "A unidade de entrada e obrigatoria. [Origem: FatorEmissaoGee]")
                .IsNotNullOrEmpty(UnidadeSaida, nameof(UnidadeSaida), "A unidade de saida e obrigatoria. [Origem: FatorEmissaoGee]")
                .IsFalse(FimVigencia.HasValue && FimVigencia.Value < InicioVigencia, nameof(FimVigencia), "O fim de vigencia deve ser posterior ao inicio. [Origem: FatorEmissaoGee]"));
        }

        public bool VigenteEm(DateTime data)
        {
            var d = data.Date;
            return d >= InicioVigencia && (!FimVigencia.HasValue || d <= FimVigencia.Value);
        }
    }
}
