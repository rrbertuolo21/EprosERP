using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class CstIbsCbs : EntidadeSaaSBase, IGlobalEntity
    {
        public string Cst { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public DateTime DataInicioVigencia { get; private set; }
        public DateTime? DataFimVigencia { get; private set; }

        public ICollection<ClassificacaoTributaria> ClassesTributarias { get; private set; } = new List<ClassificacaoTributaria>();

        protected CstIbsCbs() { } // EF Core

        public CstIbsCbs(
            string cst,
            string descricao,
            DateTime dataInicioVigencia,
            DateTime? dataFimVigencia,
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            Cst = cst;
            Descricao = descricao;
            DataInicioVigencia = dataInicioVigencia;
            DataFimVigencia = dataFimVigencia;
            Validar();
        }

        public void Alterar(
            string cst,
            string descricao,
            DateTime dataInicioVigencia,
            DateTime? dataFimVigencia,
            string alteradoPor)
        {
            Cst = cst;
            Descricao = descricao;
            DataInicioVigencia = dataInicioVigencia;
            DataFimVigencia = dataFimVigencia;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<CstIbsCbs>()
                .Requires()
                .IsNotNullOrEmpty(Cst, nameof(Cst), "O CST é obrigatório [Origem: CstIbsCbs]")
                .IsLowerOrEqualsThan((Cst ?? "").Length, 5, nameof(Cst), "O campo Cst deve ter no máximo 5 caracteres [Origem: CstIbsCbs]")
                .IsLowerOrEqualsThan((Descricao ?? "").Length, 1000, nameof(Descricao), "O campo Descricao deve ter no máximo 1000 caracteres [Origem: CstIbsCbs]")
            );
        }

        public ClassificacaoTributaria AdicionarClasseTributaria(
            string codigo,
            string descricao,
            DateTime dataInicioVigencia,
            DateTime? dataFimVigencia,
            bool indNfe,
            bool indNfce,
            bool indCte,
            bool indCteos,
            bool indNfse,
            bool indTribRegular,
            string criadoPor)
        {
            var cctrib = new ClassificacaoTributaria(Id, codigo, descricao, dataInicioVigencia, dataFimVigencia, indNfe, indNfce, indCte, indCteos, indNfse, indTribRegular, TenantId, criadoPor);
            ClassesTributarias.Add(cctrib);
            return cctrib;
        }

        public void RemoverClasseTributaria(Guid classificacaoTributariaId, string alteradoPor)
        {
            var item = ClassesTributarias.FirstOrDefault(c => c.Id == classificacaoTributariaId);
            item?.Deletar(alteradoPor);
        }
    }
}
