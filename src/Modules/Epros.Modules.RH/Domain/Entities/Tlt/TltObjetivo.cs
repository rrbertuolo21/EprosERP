using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-TLT, tabela rh_tlt_objetivo). Fidelidade campo a campo.</summary>
    public partial class TltObjetivo : EntidadeSaaSBase
    {
        public string? Nome { get; private set; }
        public string? Descricao { get; private set; }
        public Guid? CategoriaId { get; private set; }
        public string? TipoObjetivo { get; private set; }
        public decimal? ValorAlvo { get; private set; }
        public decimal? ValorAtual { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataAlvo { get; private set; }
        public string? Prioridade { get; private set; }
        public string? Status { get; private set; }
        public Guid? ContaId { get; private set; }
        public Guid? CriadoPorId { get; private set; }
        public Guid? OwnerId { get; private set; }

        protected TltObjetivo() { } // EF Core

        public TltObjetivo(
            string? nome,
            string? descricao,
            Guid? categoriaId,
            string? tipoObjetivo,
            decimal? valorAlvo,
            decimal? valorAtual,
            DateTime? dataInicio,
            DateTime? dataAlvo,
            string? prioridade,
            string? status,
            Guid? contaId,
            Guid? criadoPorId,
            Guid? ownerId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Descricao = descricao;
            CategoriaId = categoriaId;
            TipoObjetivo = tipoObjetivo;
            ValorAlvo = valorAlvo;
            ValorAtual = valorAtual;
            DataInicio = dataInicio;
            DataAlvo = dataAlvo;
            Prioridade = prioridade;
            Status = status;
            ContaId = contaId;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<TltObjetivo>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
