using System;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Security
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class AbacAuthorizeAttribute : TypeFilterAttribute
    {
        public string Recurso { get; }
        public string Acao { get; }

        public AbacAuthorizeAttribute(string recurso, string acao) : base(typeof(AbacFilter))
        {
            Recurso = recurso;
            Acao = acao;
            Arguments = new object[] { recurso, acao };
        }
    }
}
