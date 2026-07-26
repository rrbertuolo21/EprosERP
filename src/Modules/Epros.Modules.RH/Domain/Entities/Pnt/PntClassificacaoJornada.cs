using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PNT). Fidelidade campo a campo.</summary>
    public partial class PntClassificacaoJornada : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public string Padrao { get; private set; } = string.Empty;
        public string DescontarHoras { get; private set; } = string.Empty;

        protected PntClassificacaoJornada() { } // EF Core

        public PntClassificacaoJornada(
            Guid empresaId,
            string codigo,
            string nome,
            string descricao,
            string padrao,
            string descontarHoras,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            Codigo = codigo;
            Nome = nome;
            Descricao = descricao;
            Padrao = padrao;
            DescontarHoras = descontarHoras;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PntClassificacaoJornada>().Requires();
            contract.AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "O campo EmpresaId e obrigatorio.");
            contract.IsNotNullOrEmpty(Codigo, nameof(Codigo), "O campo Codigo e obrigatorio.");
            contract.IsNotNullOrEmpty(Nome, nameof(Nome), "O campo Nome e obrigatorio.");
            contract.IsNotNullOrEmpty(Descricao, nameof(Descricao), "O campo Descricao e obrigatorio.");
            contract.IsNotNullOrEmpty(Padrao, nameof(Padrao), "O campo Padrao e obrigatorio.");
            contract.IsNotNullOrEmpty(DescontarHoras, nameof(DescontarHoras), "O campo DescontarHoras e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
