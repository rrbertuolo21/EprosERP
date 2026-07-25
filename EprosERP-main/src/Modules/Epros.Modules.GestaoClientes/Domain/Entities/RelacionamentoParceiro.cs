using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Relacionamento/hierarquia entre pessoas parceiras (CAD-PEM 12.14).</summary>
    public class RelacionamentoParceiro : EntidadeSaaSBase
    {
        public Guid PessoaOrigemId { get; private set; }
        public Guid PessoaDestinoId { get; private set; }
        public ETipoRelacaoParceiro TipoRelacao { get; private set; }
        public DateTime? VigenciaInicio { get; private set; }
        public DateTime? VigenciaFim { get; private set; }

        protected RelacionamentoParceiro() { } // EF Core

        public RelacionamentoParceiro(
            Guid pessoaOrigemId,
            Guid pessoaDestinoId,
            ETipoRelacaoParceiro tipoRelacao,
            DateTime? vigenciaInicio,
            DateTime? vigenciaFim,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PessoaOrigemId = pessoaOrigemId;
            PessoaDestinoId = pessoaDestinoId;
            TipoRelacao = tipoRelacao;
            VigenciaInicio = vigenciaInicio;
            VigenciaFim = vigenciaFim;
            Validar();
        }

        public void Alterar(ETipoRelacaoParceiro tipoRelacao, DateTime? vigenciaInicio, DateTime? vigenciaFim, string alteradoPor)
        {
            TipoRelacao = tipoRelacao;
            VigenciaInicio = vigenciaInicio;
            VigenciaFim = vigenciaFim;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<RelacionamentoParceiro>()
                .Requires()
                .AreNotEquals(PessoaOrigemId, Guid.Empty, nameof(PessoaOrigemId), "PessoaOrigemId é obrigatório [Origem: RelacionamentoParceiro]")
                .AreNotEquals(PessoaDestinoId, Guid.Empty, nameof(PessoaDestinoId), "PessoaDestinoId é obrigatório [Origem: RelacionamentoParceiro]")
                .IsFalse(PessoaOrigemId == PessoaDestinoId, nameof(PessoaDestinoId), "Uma pessoa não pode se relacionar com ela mesma [Origem: RelacionamentoParceiro]")
                .IsTrue(Enum.IsDefined(typeof(ETipoRelacaoParceiro), TipoRelacao), nameof(TipoRelacao), "TipoRelacao não consta na lista [Origem: RelacionamentoParceiro]")
            );

            if (VigenciaInicio.HasValue && VigenciaFim.HasValue && VigenciaFim.Value < VigenciaInicio.Value)
                AddNotification(nameof(VigenciaFim), "VigenciaFim não pode ser anterior à VigenciaInicio [Origem: RelacionamentoParceiro]");
        }
    }
}
