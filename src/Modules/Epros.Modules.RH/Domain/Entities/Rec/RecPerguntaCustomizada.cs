using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-REC). Fidelidade campo a campo.</summary>
    public partial class RecPerguntaCustomizada : EntidadeSaaSBase
    {
        public string Pergunta { get; private set; } = string.Empty;
        public string Tipo { get; private set; } = string.Empty;
        public string? OpcoesJson { get; private set; }
        public bool Obrigatoria { get; private set; }
        public bool Ativa { get; private set; }
        public int Ordem { get; private set; }
        public Guid CriadoPorUsuarioId { get; private set; }
        public Guid DonoFuncionalId { get; private set; }

        protected RecPerguntaCustomizada() { } // EF Core

        public RecPerguntaCustomizada(
            string pergunta,
            string tipo,
            string? opcoesJson,
            bool obrigatoria,
            bool ativa,
            int ordem,
            Guid criadoPorUsuarioId,
            Guid donoFuncionalId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Pergunta = pergunta;
            Tipo = tipo;
            OpcoesJson = opcoesJson;
            Obrigatoria = obrigatoria;
            Ativa = ativa;
            Ordem = ordem;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            DonoFuncionalId = donoFuncionalId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<RecPerguntaCustomizada>().Requires();
            contract.IsNotNullOrEmpty(Pergunta, nameof(Pergunta), "O campo Pergunta e obrigatorio.");
            contract.IsNotNullOrEmpty(Tipo, nameof(Tipo), "O campo Tipo e obrigatorio.");
            contract.AreNotEquals(CriadoPorUsuarioId, Guid.Empty, nameof(CriadoPorUsuarioId), "O campo CriadoPorUsuarioId e obrigatorio.");
            contract.AreNotEquals(DonoFuncionalId, Guid.Empty, nameof(DonoFuncionalId), "O campo DonoFuncionalId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
