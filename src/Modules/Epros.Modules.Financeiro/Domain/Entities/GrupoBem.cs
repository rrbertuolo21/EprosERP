using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>Grupo/hierarquia de bens patrimoniais e contas contábeis associadas (EF FIN-AFX §10.6 afx_grupo_bem).</summary>
    public class GrupoBem : EntidadeSaaSBase
    {
        public string? Codigo { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public Guid? ContaAtivoId { get; private set; }
        public Guid? ContaDepreciacaoId { get; private set; }
        public Guid? ContaBaixaId { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected GrupoBem() { } // EF Core

        public GrupoBem(string? codigo, string nome, Guid? contaAtivoId, Guid? contaDepreciacaoId, Guid? contaBaixaId,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Nome = nome;
            ContaAtivoId = contaAtivoId;
            ContaDepreciacaoId = contaDepreciacaoId;
            ContaBaixaId = contaBaixaId;
            Ativo = true;
            Validar();
        }

        public void Alterar(string? codigo, string nome, Guid? contaAtivoId, Guid? contaDepreciacaoId, Guid? contaBaixaId, string usuario)
        {
            Codigo = codigo;
            Nome = nome;
            ContaAtivoId = contaAtivoId;
            ContaDepreciacaoId = contaDepreciacaoId;
            ContaBaixaId = contaBaixaId;
            MarcarAlterado(usuario);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<GrupoBem>()
                .Requires()
                .IsNotNullOrEmpty(Nome, nameof(Nome), "O nome do grupo de bem é obrigatório [Origem: GrupoBem]")
            );
        }
    }
}
