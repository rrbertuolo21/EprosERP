using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-WFM, tabela rh_wfm_dados_trabalhistas). Fidelidade campo a campo.</summary>
    public partial class WfmDadosTrabalhistas : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public string? FgtsOptante { get; private set; }
        public DateTime? FgtsDataOpcao { get; private set; }
        public int? FgtsConta { get; private set; }
        public string? PisNumero { get; private set; }
        public DateTime? PisDataCadastro { get; private set; }
        public string? PisBanco { get; private set; }
        public string? PisAgencia { get; private set; }
        public string? PisAgenciaDigito { get; private set; }
        public string? CtpsNumero { get; private set; }
        public string? CtpsSerie { get; private set; }
        public DateTime? CtpsDataExpedicao { get; private set; }
        public string? CtpsUf { get; private set; }
        public string? SaiNaRais { get; private set; }
        public string? CategoriaSefip { get; private set; }
        public int? OcorrenciaSefip { get; private set; }
        public int? CodigoAdmissaoCaged { get; private set; }
        public int? CodigoDemissaoCaged { get; private set; }
        public int? CodigoDemissaoSefip { get; private set; }
        public DateTime? DataDemissao { get; private set; }
        public string? CodigoTurmaPonto { get; private set; }

        protected WfmDadosTrabalhistas() { } // EF Core

        public WfmDadosTrabalhistas(
            Guid colaboradorId,
            string? fgtsOptante,
            DateTime? fgtsDataOpcao,
            int? fgtsConta,
            string? pisNumero,
            DateTime? pisDataCadastro,
            string? pisBanco,
            string? pisAgencia,
            string? pisAgenciaDigito,
            string? ctpsNumero,
            string? ctpsSerie,
            DateTime? ctpsDataExpedicao,
            string? ctpsUf,
            string? saiNaRais,
            string? categoriaSefip,
            int? ocorrenciaSefip,
            int? codigoAdmissaoCaged,
            int? codigoDemissaoCaged,
            int? codigoDemissaoSefip,
            DateTime? dataDemissao,
            string? codigoTurmaPonto,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            FgtsOptante = fgtsOptante;
            FgtsDataOpcao = fgtsDataOpcao;
            FgtsConta = fgtsConta;
            PisNumero = pisNumero;
            PisDataCadastro = pisDataCadastro;
            PisBanco = pisBanco;
            PisAgencia = pisAgencia;
            PisAgenciaDigito = pisAgenciaDigito;
            CtpsNumero = ctpsNumero;
            CtpsSerie = ctpsSerie;
            CtpsDataExpedicao = ctpsDataExpedicao;
            CtpsUf = ctpsUf;
            SaiNaRais = saiNaRais;
            CategoriaSefip = categoriaSefip;
            OcorrenciaSefip = ocorrenciaSefip;
            CodigoAdmissaoCaged = codigoAdmissaoCaged;
            CodigoDemissaoCaged = codigoDemissaoCaged;
            CodigoDemissaoSefip = codigoDemissaoSefip;
            DataDemissao = dataDemissao;
            CodigoTurmaPonto = codigoTurmaPonto;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<WfmDadosTrabalhistas>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
