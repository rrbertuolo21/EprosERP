using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PNT). Fidelidade campo a campo.</summary>
    public partial class PntImportacaoAfd : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public Guid? RelogioId { get; private set; }
        public string? ArquivoReferencia { get; private set; }
        public DateTime DataImportacao { get; private set; }
        public int? QuantidadeRegistros { get; private set; }
        public int? QuantidadePareados { get; private set; }
        public int? QuantidadePendentes { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public string? Observacao { get; private set; }

        protected PntImportacaoAfd() { } // EF Core

        public PntImportacaoAfd(
            Guid empresaId,
            Guid? relogioId,
            string? arquivoReferencia,
            DateTime dataImportacao,
            int? quantidadeRegistros,
            int? quantidadePareados,
            int? quantidadePendentes,
            string status,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            RelogioId = relogioId;
            ArquivoReferencia = arquivoReferencia;
            DataImportacao = dataImportacao;
            QuantidadeRegistros = quantidadeRegistros;
            QuantidadePareados = quantidadePareados;
            QuantidadePendentes = quantidadePendentes;
            Status = status;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PntImportacaoAfd>().Requires();
            contract.AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "O campo EmpresaId e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
