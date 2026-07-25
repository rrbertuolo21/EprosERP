using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-DEV, tabela rh_dev_comunicado). Fidelidade campo a campo.</summary>
    public partial class DevComunicado : EntidadeSaaSBase
    {
        public string? Titulo { get; private set; }
        public string? Descricao { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public string? Prioridade { get; private set; }
        public string? Status { get; private set; }
        public Guid? CategoriaId { get; private set; }
        public Guid? AprovadoPor { get; private set; }

        protected DevComunicado() { } // EF Core

        public DevComunicado(
            string? titulo,
            string? descricao,
            DateTime? dataInicio,
            DateTime? dataFim,
            string? prioridade,
            string? status,
            Guid? categoriaId,
            Guid? aprovadoPor,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Titulo = titulo;
            Descricao = descricao;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Prioridade = prioridade;
            Status = status;
            CategoriaId = categoriaId;
            AprovadoPor = aprovadoPor;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<DevComunicado>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
