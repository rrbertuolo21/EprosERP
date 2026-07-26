using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolRubrica : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public string Tipo { get; private set; } = string.Empty;
        public string Unidade { get; private set; } = string.Empty;
        public string BaseCalculo { get; private set; } = string.Empty;
        public decimal? Taxa { get; private set; }
        public bool Ativo { get; private set; }

        protected FolRubrica() { } // EF Core

        public FolRubrica(
            Guid empresaId,
            string codigo,
            string nome,
            string descricao,
            string tipo,
            string unidade,
            string baseCalculo,
            decimal? taxa,
            bool ativo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            Codigo = codigo;
            Nome = nome;
            Descricao = descricao;
            Tipo = tipo;
            Unidade = unidade;
            BaseCalculo = baseCalculo;
            Taxa = taxa;
            Ativo = ativo;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolRubrica>().Requires();
            contract.AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "O campo EmpresaId e obrigatorio.");
            contract.IsNotNullOrEmpty(Codigo, nameof(Codigo), "O campo Codigo e obrigatorio.");
            contract.IsNotNullOrEmpty(Nome, nameof(Nome), "O campo Nome e obrigatorio.");
            contract.IsNotNullOrEmpty(Descricao, nameof(Descricao), "O campo Descricao e obrigatorio.");
            contract.IsNotNullOrEmpty(Tipo, nameof(Tipo), "O campo Tipo e obrigatorio.");
            contract.IsNotNullOrEmpty(Unidade, nameof(Unidade), "O campo Unidade e obrigatorio.");
            contract.IsNotNullOrEmpty(BaseCalculo, nameof(BaseCalculo), "O campo BaseCalculo e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
