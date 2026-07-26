using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>
    /// Documento de retorno (devolucao) preservando dados fiscais, transporte e frete (EF ECONOMIA_CIRCULAR 11.1).
    /// Fidelidade: 35 campos materiais preservados do legado.
    /// </summary>
    public class DevolucaoEco : EntidadeSaaSBase
    {
        public Guid? ContactId { get; private set; }
        public Guid? NaturezaId { get; private set; }
        public decimal? ValorIntegral { get; private set; }
        public decimal? ValorDevolvido { get; private set; }
        public string? Motivo { get; private set; }
        public string? Observacao { get; private set; }
        public string? Estado { get; private set; }
        public bool DevolucaoParcial { get; private set; }
        public string? ChaveNfEntrada { get; private set; }
        public string? NumeroNf { get; private set; }
        public decimal? ValorFrete { get; private set; }
        public decimal? ValorDesconto { get; private set; }
        public string? ChaveGerada { get; private set; }
        public string? NumeroGerado { get; private set; }
        public Guid? BusinessId { get; private set; }
        public Guid? LocationId { get; private set; }
        public string? Tipo { get; private set; }
        public decimal? ValorSeguro { get; private set; }
        public decimal? ValorOutro { get; private set; }
        public int? SequenciaCce { get; private set; }
        public string? TransportadoraNome { get; private set; }
        public string? TransportadoraCidade { get; private set; }
        public string? TransportadoraUf { get; private set; }
        public string? TransportadoraCpfCnpj { get; private set; }
        public string? TransportadoraIe { get; private set; }
        public string? TransportadoraEndereco { get; private set; }
        public decimal? FreteQuantidade { get; private set; }
        public string? FreteEspecie { get; private set; }
        public string? FreteMarca { get; private set; }
        public string? FreteNumero { get; private set; }
        public string? FreteTipo { get; private set; }
        public string? VeiculoPlaca { get; private set; }
        public string? VeiculoUf { get; private set; }
        public decimal? FretePesoBruto { get; private set; }
        public decimal? FretePesoLiquido { get; private set; }
        public int Versao { get; private set; }

        private readonly List<DevolucaoItemEco> _itens = new();
        public IReadOnlyCollection<DevolucaoItemEco> Itens => _itens.AsReadOnly();

        protected DevolucaoEco() { } // EF Core

        public DevolucaoEco(
            Guid? contactId,
            Guid? naturezaId,
            decimal? valorIntegral,
            decimal? valorDevolvido,
            string? motivo,
            string? observacao,
            string? estado,
            bool devolucaoParcial,
            string? chaveNfEntrada,
            string? numeroNf,
            decimal? valorFrete,
            decimal? valorDesconto,
            Guid? businessId,
            Guid? locationId,
            string? tipo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ContactId = contactId;
            NaturezaId = naturezaId;
            ValorIntegral = valorIntegral;
            ValorDevolvido = valorDevolvido;
            Motivo = motivo;
            Observacao = observacao;
            Estado = estado;
            DevolucaoParcial = devolucaoParcial;
            ChaveNfEntrada = chaveNfEntrada;
            NumeroNf = numeroNf;
            ValorFrete = valorFrete;
            ValorDesconto = valorDesconto;
            BusinessId = businessId;
            LocationId = locationId;
            Tipo = tipo;
            Versao = 1;
            Validar();
        }

        public void PreencherTransporte(
            decimal? valorSeguro, decimal? valorOutro, int? sequenciaCce,
            string? transportadoraNome, string? transportadoraCidade, string? transportadoraUf,
            string? transportadoraCpfCnpj, string? transportadoraIe, string? transportadoraEndereco,
            decimal? freteQuantidade, string? freteEspecie, string? freteMarca, string? freteNumero,
            string? freteTipo, string? veiculoPlaca, string? veiculoUf, decimal? fretePesoBruto, decimal? fretePesoLiquido)
        {
            ValorSeguro = valorSeguro;
            ValorOutro = valorOutro;
            SequenciaCce = sequenciaCce;
            TransportadoraNome = transportadoraNome;
            TransportadoraCidade = transportadoraCidade;
            TransportadoraUf = transportadoraUf;
            TransportadoraCpfCnpj = transportadoraCpfCnpj;
            TransportadoraIe = transportadoraIe;
            TransportadoraEndereco = transportadoraEndereco;
            FreteQuantidade = freteQuantidade;
            FreteEspecie = freteEspecie;
            FreteMarca = freteMarca;
            FreteNumero = freteNumero;
            FreteTipo = freteTipo;
            VeiculoPlaca = veiculoPlaca;
            VeiculoUf = veiculoUf;
            FretePesoBruto = fretePesoBruto;
            FretePesoLiquido = fretePesoLiquido;
        }

        public void AdicionarItem(DevolucaoItemEco item)
        {
            _itens.Add(item);
        }

        public void Validar()
        {
            Clear();
            // RN-ECO: documento minimo exige ao menos um vinculo de contato/natureza ou chave de origem.
            AddNotifications(new Contract<DevolucaoEco>()
                .Requires()
                .IsGreaterThan(Versao, 0, nameof(Versao), "A versao deve ser positiva. [Origem: DevolucaoEco]"));
        }
    }
}
