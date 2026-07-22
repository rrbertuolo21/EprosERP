using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class ClassificacaoTributariaAnexo : EntidadeSaaSBase
    {
        public Guid ClassificacaoTributariaId { get; private set; }
        public int NroAnexo { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public DateTime DataInicioVigencia { get; private set; }
        public DateTime? DataFimVigencia { get; private set; }

        protected ClassificacaoTributariaAnexo() { } // EF Core

        public ClassificacaoTributariaAnexo(
            Guid classificacaoTributariaId,
            int nroAnexo,
            string codigo,
            DateTime dataInicioVigencia,
            DateTime? dataFimVigencia,
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            ClassificacaoTributariaId = classificacaoTributariaId;
            NroAnexo = nroAnexo;
            Codigo = codigo;
            DataInicioVigencia = dataInicioVigencia;
            DataFimVigencia = dataFimVigencia;
            Validar();
        }

        public void Alterar(int nroAnexo, string codigo, DateTime dataInicioVigencia, DateTime? dataFimVigencia, string alteradoPor)
        {
            NroAnexo = nroAnexo;
            Codigo = codigo;
            DataInicioVigencia = dataInicioVigencia;
            DataFimVigencia = dataFimVigencia;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ClassificacaoTributariaAnexo>()
                .Requires()
                .IsLowerOrEqualsThan((Codigo ?? "").Length, 20, nameof(Codigo), "O campo Codigo deve ter no máximo 20 caracteres [Origem: ClassificacaoTributariaAnexo]")
            );
        }
    }
}
