using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-LMS, tabela rh_lms_tipo_treinamento). Fidelidade campo a campo.</summary>
    public partial class LmsTipoTreinamento : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public Guid FilialId { get; private set; }
        public Guid DepartamentoId { get; private set; }
        public Guid CriadoPorUsuarioId { get; private set; }
        public Guid DonoFuncionalId { get; private set; }

        protected LmsTipoTreinamento() { } // EF Core

        public LmsTipoTreinamento(
            string nome,
            string? descricao,
            Guid filialId,
            Guid departamentoId,
            Guid criadoPorUsuarioId,
            Guid donoFuncionalId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Descricao = descricao;
            FilialId = filialId;
            DepartamentoId = departamentoId;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            DonoFuncionalId = donoFuncionalId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<LmsTipoTreinamento>().Requires();
            contract.IsNotNullOrEmpty(Nome, nameof(Nome), "O campo Nome e obrigatorio.");
            contract.AreNotEquals(FilialId, Guid.Empty, nameof(FilialId), "O campo FilialId e obrigatorio.");
            contract.AreNotEquals(DepartamentoId, Guid.Empty, nameof(DepartamentoId), "O campo DepartamentoId e obrigatorio.");
            contract.AreNotEquals(CriadoPorUsuarioId, Guid.Empty, nameof(CriadoPorUsuarioId), "O campo CriadoPorUsuarioId e obrigatorio.");
            contract.AreNotEquals(DonoFuncionalId, Guid.Empty, nameof(DonoFuncionalId), "O campo DonoFuncionalId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
