using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class ClassificacaoTributaria : EntidadeSaaSBase
    {
        public Guid CstIbsCbsId { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public DateTime DataInicioVigencia { get; private set; }
        public DateTime? DataFimVigencia { get; private set; }
        public bool IndNfe { get; private set; }
        public bool IndNfce { get; private set; }
        public bool IndCte { get; private set; }
        public bool IndCteos { get; private set; }
        public bool IndNfse { get; private set; }
        public bool IndTribRegular { get; private set; }

        public ICollection<ClassificacaoTributariaAnexo> Anexos { get; private set; } = new List<ClassificacaoTributariaAnexo>();

        protected ClassificacaoTributaria() { } // EF Core

        public ClassificacaoTributaria(
            Guid cstIbsCbsId,
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
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            CstIbsCbsId = cstIbsCbsId;
            Codigo = codigo;
            Descricao = descricao;
            DataInicioVigencia = dataInicioVigencia;
            DataFimVigencia = dataFimVigencia;
            IndNfe = indNfe;
            IndNfce = indNfce;
            IndCte = indCte;
            IndCteos = indCteos;
            IndNfse = indNfse;
            IndTribRegular = indTribRegular;
            Validar();
        }

        public void Alterar(
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
            string alteradoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            DataInicioVigencia = dataInicioVigencia;
            DataFimVigencia = dataFimVigencia;
            IndNfe = indNfe;
            IndNfce = indNfce;
            IndCte = indCte;
            IndCteos = indCteos;
            IndNfse = indNfse;
            IndTribRegular = indTribRegular;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ClassificacaoTributaria>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O código é obrigatório [Origem: ClassificacaoTributaria]")
                .IsLowerOrEqualsThan((Codigo ?? "").Length, 20, nameof(Codigo), "O campo Codigo deve ter no máximo 20 caracteres [Origem: ClassificacaoTributaria]")
                .IsLowerOrEqualsThan((Descricao ?? "").Length, 1000, nameof(Descricao), "O campo Descricao deve ter no máximo 1000 caracteres [Origem: ClassificacaoTributaria]")
            );
        }

        public void AdicionarAnexo(int nroAnexo, string codigo, DateTime dataInicioVigencia, DateTime? dataFimVigencia, string criadoPor)
        {
            Anexos.Add(new ClassificacaoTributariaAnexo(Id, nroAnexo, codigo, dataInicioVigencia, dataFimVigencia, TenantId, criadoPor));
        }

        public void RemoverAnexo(Guid anexoId, string alteradoPor)
        {
            var item = Anexos.FirstOrDefault(a => a.Id == anexoId);
            item?.Deletar(alteradoPor);
        }
    }
}
