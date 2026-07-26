using System;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Security
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class PermissaoMenuAttribute : TypeFilterAttribute
    {
        public string MenuId { get; }
        public string? Nivel1Id { get; }
        public string? Nivel2Id { get; }
        public string Acao { get; }

        public PermissaoMenuAttribute(string menuId, string acao) : base(typeof(PermissaoMenuFilter))
        {
            MenuId = menuId;
            Acao = acao;
            Arguments = new object[] { menuId, string.Empty, string.Empty, acao };
        }

        public PermissaoMenuAttribute(string menuId, string nivel1Id, string acao) : base(typeof(PermissaoMenuFilter))
        {
            MenuId = menuId;
            Nivel1Id = nivel1Id;
            Acao = acao;
            Arguments = new object[] { menuId, nivel1Id, string.Empty, acao };
        }

        public PermissaoMenuAttribute(string menuId, string nivel1Id, string nivel2Id, string acao) : base(typeof(PermissaoMenuFilter))
        {
            MenuId = menuId;
            Nivel1Id = nivel1Id;
            Nivel2Id = nivel2Id;
            Acao = acao;
            Arguments = new object[] { menuId, nivel1Id, nivel2Id, acao };
        }
    }
}
