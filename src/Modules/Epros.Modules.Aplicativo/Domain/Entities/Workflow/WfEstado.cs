using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Workflow
{
    /// <summary>
    /// wf_estado — estados permitidos em uma definição de workflow. [Origem: EF WORKFLOW 10.4]
    /// </summary>
    public class WfEstado : EntidadeSaaSBase
    {
        public Guid DefinicaoId { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public bool Inicial { get; private set; }
        public bool Final { get; private set; }
        public bool Ativo { get; private set; }

        protected WfEstado() { } // EF Core

        public WfEstado(
            Guid definicaoId,
            string codigo,
            string nome,
            bool inicial,
            bool final,
            bool ativo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            DefinicaoId = definicaoId;
            Codigo = codigo;
            Nome = nome;
            Inicial = inicial;
            Final = final;
            Ativo = ativo;
            Validar();
        }

        public void Alterar(string nome, bool inicial, bool final, bool ativo, string alteradoPor)
        {
            Nome = nome;
            Inicial = inicial;
            Final = final;
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<WfEstado>()
                .Requires()
                .AreNotEquals(DefinicaoId, Guid.Empty, nameof(DefinicaoId), "A definição do estado é obrigatória [Origem: WfEstado]")
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O código do estado é obrigatório [Origem: WfEstado]")
                .IsNotNullOrEmpty(Nome, nameof(Nome), "O nome do estado é obrigatório [Origem: WfEstado]"));
        }
    }
}
