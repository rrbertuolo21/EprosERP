using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-REC). Fidelidade campo a campo.</summary>
    public partial class RecLocalVaga : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public bool TrabalhoRemoto { get; private set; }
        public string? Endereco { get; private set; }
        public string? Cidade { get; private set; }
        public string? Estado { get; private set; }
        public string? Pais { get; private set; }
        public string? Cep { get; private set; }
        public bool Status { get; private set; }
        public Guid CriadoPorUsuarioId { get; private set; }
        public Guid DonoFuncionalId { get; private set; }

        protected RecLocalVaga() { } // EF Core

        public RecLocalVaga(
            string nome,
            bool trabalhoRemoto,
            string? endereco,
            string? cidade,
            string? estado,
            string? pais,
            string? cep,
            bool status,
            Guid criadoPorUsuarioId,
            Guid donoFuncionalId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            TrabalhoRemoto = trabalhoRemoto;
            Endereco = endereco;
            Cidade = cidade;
            Estado = estado;
            Pais = pais;
            Cep = cep;
            Status = status;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            DonoFuncionalId = donoFuncionalId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<RecLocalVaga>().Requires();
            contract.IsNotNullOrEmpty(Nome, nameof(Nome), "O campo Nome e obrigatorio.");
            contract.AreNotEquals(CriadoPorUsuarioId, Guid.Empty, nameof(CriadoPorUsuarioId), "O campo CriadoPorUsuarioId e obrigatorio.");
            contract.AreNotEquals(DonoFuncionalId, Guid.Empty, nameof(DonoFuncionalId), "O campo DonoFuncionalId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
