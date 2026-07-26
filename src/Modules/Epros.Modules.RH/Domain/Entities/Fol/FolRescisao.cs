using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolRescisao : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public DateTime? DataDemissao { get; private set; }
        public DateTime? DataPagamento { get; private set; }
        public string Motivo { get; private set; } = string.Empty;
        public DateTime? DataAvisoPrevio { get; private set; }
        public int? DiasAvisoPrevio { get; private set; }
        public string ComprovouNovoEmprego { get; private set; } = string.Empty;
        public string DispensouEmpregado { get; private set; } = string.Empty;
        public decimal? PensaoAlimenticia { get; private set; }
        public decimal? PensaoAlimenticiaFgts { get; private set; }
        public decimal? FgtsValorRescisao { get; private set; }
        public decimal? FgtsSaldoBanco { get; private set; }
        public decimal? FgtsComplementoSaldo { get; private set; }
        public string FgtsCodigoAfastamento { get; private set; } = string.Empty;
        public string FgtsCodigoSaque { get; private set; } = string.Empty;

        protected FolRescisao() { } // EF Core

        public FolRescisao(
            Guid colaboradorId,
            DateTime? dataDemissao,
            DateTime? dataPagamento,
            string motivo,
            DateTime? dataAvisoPrevio,
            int? diasAvisoPrevio,
            string comprovouNovoEmprego,
            string dispensouEmpregado,
            decimal? pensaoAlimenticia,
            decimal? pensaoAlimenticiaFgts,
            decimal? fgtsValorRescisao,
            decimal? fgtsSaldoBanco,
            decimal? fgtsComplementoSaldo,
            string fgtsCodigoAfastamento,
            string fgtsCodigoSaque,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            DataDemissao = dataDemissao;
            DataPagamento = dataPagamento;
            Motivo = motivo;
            DataAvisoPrevio = dataAvisoPrevio;
            DiasAvisoPrevio = diasAvisoPrevio;
            ComprovouNovoEmprego = comprovouNovoEmprego;
            DispensouEmpregado = dispensouEmpregado;
            PensaoAlimenticia = pensaoAlimenticia;
            PensaoAlimenticiaFgts = pensaoAlimenticiaFgts;
            FgtsValorRescisao = fgtsValorRescisao;
            FgtsSaldoBanco = fgtsSaldoBanco;
            FgtsComplementoSaldo = fgtsComplementoSaldo;
            FgtsCodigoAfastamento = fgtsCodigoAfastamento;
            FgtsCodigoSaque = fgtsCodigoSaque;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolRescisao>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(Motivo, nameof(Motivo), "O campo Motivo e obrigatorio.");
            contract.IsNotNullOrEmpty(ComprovouNovoEmprego, nameof(ComprovouNovoEmprego), "O campo ComprovouNovoEmprego e obrigatorio.");
            contract.IsNotNullOrEmpty(DispensouEmpregado, nameof(DispensouEmpregado), "O campo DispensouEmpregado e obrigatorio.");
            contract.IsNotNullOrEmpty(FgtsCodigoAfastamento, nameof(FgtsCodigoAfastamento), "O campo FgtsCodigoAfastamento e obrigatorio.");
            contract.IsNotNullOrEmpty(FgtsCodigoSaque, nameof(FgtsCodigoSaque), "O campo FgtsCodigoSaque e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
