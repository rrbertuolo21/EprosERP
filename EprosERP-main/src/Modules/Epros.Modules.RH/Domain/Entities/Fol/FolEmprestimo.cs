using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolEmprestimo : EntidadeSaaSBase
    {
        public string? Titulo { get; private set; }
        public Guid? ColaboradorId { get; private set; }
        public Guid? TipoEmprestimoId { get; private set; }
        public string? TipoCalculo { get; private set; }
        public decimal? Valor { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public string? Motivo { get; private set; }
        public Guid? CriadoPorId { get; private set; }
        public Guid? OwnerId { get; private set; }

        protected FolEmprestimo() { } // EF Core

        public FolEmprestimo(
            string? titulo,
            Guid? colaboradorId,
            Guid? tipoEmprestimoId,
            string? tipoCalculo,
            decimal? valor,
            DateTime? dataInicio,
            DateTime? dataFim,
            string? motivo,
            Guid? criadoPorId,
            Guid? ownerId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Titulo = titulo;
            ColaboradorId = colaboradorId;
            TipoEmprestimoId = tipoEmprestimoId;
            TipoCalculo = tipoCalculo;
            Valor = valor;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Motivo = motivo;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolEmprestimo>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
