using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolCompetencia : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public string Competencia { get; private set; } = string.Empty;
        public string Tipo { get; private set; } = string.Empty;
        public DateTime? PeriodoInicio { get; private set; }
        public DateTime? PeriodoFim { get; private set; }
        public DateTime? DataPagamento { get; private set; }
        public Guid? BancoPagamentoId { get; private set; }
        public string? Numero { get; private set; }
        public string? Descricao { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public string? StatusPagamento { get; private set; }
        public decimal? ValorTotalPagamento { get; private set; }

        protected FolCompetencia() { } // EF Core

        public FolCompetencia(
            Guid empresaId,
            string competencia,
            string tipo,
            DateTime? periodoInicio,
            DateTime? periodoFim,
            DateTime? dataPagamento,
            Guid? bancoPagamentoId,
            string? numero,
            string? descricao,
            string status,
            string? statusPagamento,
            decimal? valorTotalPagamento,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            Competencia = competencia;
            Tipo = tipo;
            PeriodoInicio = periodoInicio;
            PeriodoFim = periodoFim;
            DataPagamento = dataPagamento;
            BancoPagamentoId = bancoPagamentoId;
            Numero = numero;
            Descricao = descricao;
            Status = status;
            StatusPagamento = statusPagamento;
            ValorTotalPagamento = valorTotalPagamento;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolCompetencia>().Requires();
            contract.AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "O campo EmpresaId e obrigatorio.");
            contract.IsNotNullOrEmpty(Competencia, nameof(Competencia), "O campo Competencia e obrigatorio.");
            contract.IsNotNullOrEmpty(Tipo, nameof(Tipo), "O campo Tipo e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
