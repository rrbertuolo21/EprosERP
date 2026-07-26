using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-LMS, tabela rh_lms_treinador). Fidelidade campo a campo.</summary>
    public partial class LmsTreinador : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string Contato { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Experiencia { get; private set; } = string.Empty;
        public Guid FilialId { get; private set; }
        public Guid DepartamentoId { get; private set; }
        public string? Especialidade { get; private set; }
        public string? Qualificacao { get; private set; }
        public Guid CriadoPorUsuarioId { get; private set; }
        public Guid DonoFuncionalId { get; private set; }

        protected LmsTreinador() { } // EF Core

        public LmsTreinador(
            string nome,
            string contato,
            string email,
            string experiencia,
            Guid filialId,
            Guid departamentoId,
            string? especialidade,
            string? qualificacao,
            Guid criadoPorUsuarioId,
            Guid donoFuncionalId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Contato = contato;
            Email = email;
            Experiencia = experiencia;
            FilialId = filialId;
            DepartamentoId = departamentoId;
            Especialidade = especialidade;
            Qualificacao = qualificacao;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            DonoFuncionalId = donoFuncionalId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<LmsTreinador>().Requires();
            contract.IsNotNullOrEmpty(Nome, nameof(Nome), "O campo Nome e obrigatorio.");
            contract.IsNotNullOrEmpty(Contato, nameof(Contato), "O campo Contato e obrigatorio.");
            contract.IsNotNullOrEmpty(Email, nameof(Email), "O campo Email e obrigatorio.");
            contract.IsNotNullOrEmpty(Experiencia, nameof(Experiencia), "O campo Experiencia e obrigatorio.");
            contract.AreNotEquals(FilialId, Guid.Empty, nameof(FilialId), "O campo FilialId e obrigatorio.");
            contract.AreNotEquals(DepartamentoId, Guid.Empty, nameof(DepartamentoId), "O campo DepartamentoId e obrigatorio.");
            contract.AreNotEquals(CriadoPorUsuarioId, Guid.Empty, nameof(CriadoPorUsuarioId), "O campo CriadoPorUsuarioId e obrigatorio.");
            contract.AreNotEquals(DonoFuncionalId, Guid.Empty, nameof(DonoFuncionalId), "O campo DonoFuncionalId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
