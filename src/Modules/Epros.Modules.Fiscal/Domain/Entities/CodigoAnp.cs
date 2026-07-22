using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class CodigoAnp : EntidadeSaaSBase, IGlobalEntity
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public DateTime DataInicioVigencia { get; private set; }
        public DateTime? DataFinalVigencia { get; private set; }

        protected CodigoAnp() { } // EF Core

        public CodigoAnp(
            string codigo,
            string descricao,
            DateTime dataInicioVigencia,
            DateTime? dataFinalVigencia,
            string criadoPor) : base("system", criadoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            DataInicioVigencia = dataInicioVigencia;
            DataFinalVigencia = dataFinalVigencia;
            Validar();
        }

        public void Alterar(
            string codigo,
            string descricao,
            DateTime dataInicioVigencia,
            DateTime? dataFinalVigencia,
            string alteradoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            DataInicioVigencia = dataInicioVigencia;
            DataFinalVigencia = dataFinalVigencia;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<CodigoAnp>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O código é obrigatório [Origem: CodigoAnp]")
                .IsLowerOrEqualsThan((Codigo ?? "").Length, 20, nameof(Codigo), "O campo Codigo deve ter no máximo 20 caracteres [Origem: CodigoAnp]")
                .IsLowerOrEqualsThan((Descricao ?? "").Length, 1000, nameof(Descricao), "O campo Descricao deve ter no máximo 1000 caracteres [Origem: CodigoAnp]")
            );
        }
    }
}
