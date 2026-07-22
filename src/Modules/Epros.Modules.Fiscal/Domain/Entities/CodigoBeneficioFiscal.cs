using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class CodigoBeneficioFiscal : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public EEstado Uf { get; private set; }

        public ICollection<CodigoBeneficioFiscalCsosn> Csosns { get; private set; } = new List<CodigoBeneficioFiscalCsosn>();
        public ICollection<CodigoBeneficioFiscalCst> Csts { get; private set; } = new List<CodigoBeneficioFiscalCst>();

        protected CodigoBeneficioFiscal() { } // EF Core

        public CodigoBeneficioFiscal(
            string codigo,
            EEstado uf,
            string? descricao,
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            Codigo = string.IsNullOrWhiteSpace(codigo) ? "SEM CBENEF" : codigo;
            Uf = uf;
            Descricao = descricao;
            Validar();
        }

        public void Alterar(string codigo, EEstado uf, string? descricao, string alteradoPor)
        {
            Codigo = string.IsNullOrWhiteSpace(codigo) ? "SEM CBENEF" : codigo;
            Uf = uf;
            Descricao = descricao;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<CodigoBeneficioFiscal>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O código é obrigatório.")
                .IsLowerOrEqualsThan(Codigo, 10, nameof(Codigo), "O código deve ter no máximo 10 caracteres.")
                .IsLowerOrEqualsThan(Descricao ?? string.Empty, 1000, nameof(Descricao), "A descrição deve ter no máximo 1000 caracteres.")
                .IsTrue(Enum.IsDefined(typeof(EEstado), Uf), nameof(Uf), "UF inválida.")
            );
        }

        public void AdicionarCst(ECodigoSituacaoTributariaIcms cst, string criadoPor)
        {
            if (Csts.Any(c => c.Cst == cst && c.DeletadoEm == null))
            {
                AddNotification("Csts", $"O CST {cst} já está vinculado a este código de benefício.");
                return;
            }

            var item = new CodigoBeneficioFiscalCst(Id, cst, TenantId, criadoPor);
            Csts.Add(item);
        }

        public void RemoverCst(Guid cstId, string alteradoPor)
        {
            var item = Csts.FirstOrDefault(c => c.Id == cstId);
            if (item != null)
            {
                item.Deletar(alteradoPor);
            }
        }

        public void AdicionarCsosn(ECodigoSituacaoOperacaoSimplesNacional csosn, string criadoPor)
        {
            if (Csosns.Any(c => c.Csosn == csosn && c.DeletadoEm == null))
            {
                AddNotification("Csosns", $"O CSOSN {csosn} já está vinculado a este código de benefício.");
                return;
            }

            var item = new CodigoBeneficioFiscalCsosn(Id, csosn, TenantId, criadoPor);
            Csosns.Add(item);
        }

        public void RemoverCsosn(Guid csosnId, string alteradoPor)
        {
            var item = Csosns.FirstOrDefault(c => c.Id == csosnId);
            if (item != null)
            {
                item.Deletar(alteradoPor);
            }
        }
    }
}
