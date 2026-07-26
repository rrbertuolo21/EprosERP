using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>
    /// Inscrição Estadual de Substituto Tributário (IE ST) da Empresa por UF.
    /// Porte fiel de Epros.ERP.Domain.Entities.Tributarios.IeSt.
    /// FK EmpresaId (long -> Guid). Uf portado como enum EEstado.
    /// </summary>
    public class IeSt : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public EEstado Uf { get; private set; }
        public string Ie { get; private set; } = string.Empty;

        protected IeSt() { } // EF Core

        public IeSt(Guid empresaId, EEstado uf, string ie, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            Uf = uf;
            Ie = ie;
            Validar();
        }

        public void Alterar(EEstado uf, string ie, string alteradoPor)
        {
            Uf = uf;
            Ie = ie;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<IeSt>()
                .Requires()
                .AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "EmpresaId campo obrigatório [Origem: IeSt]")
                .HasMaxLen(Ie ?? string.Empty, 20, nameof(Ie), "Inscrição Estadual pode conter no max 20 caracteres [Origem: IeSt]")
                .IsTrue(Enum.IsDefined(typeof(EEstado), Uf), nameof(Uf), "UF do IE ST informado inválido [Origem: IeSt]")
            );
        }
    }
}
