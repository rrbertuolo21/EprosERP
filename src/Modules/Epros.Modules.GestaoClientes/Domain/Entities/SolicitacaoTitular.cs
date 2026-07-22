using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Solicitação do titular de dados - DSAR (CAD-PEM 12.15, LGPD).</summary>
    public class SolicitacaoTitular : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }
        public ETipoSolicitacaoTitular Tipo { get; private set; }
        public EStatusSolicitacaoTitular Status { get; private set; }
        public DateTime Prazo { get; private set; }
        public DateTime? DataConclusao { get; private set; }

        protected SolicitacaoTitular() { } // EF Core

        public SolicitacaoTitular(
            Guid pessoaId,
            ETipoSolicitacaoTitular tipo,
            DateTime prazo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PessoaId = pessoaId;
            Tipo = tipo;
            Status = EStatusSolicitacaoTitular.Aberta;
            Prazo = prazo;
            Validar();
        }

        public void IniciarAtendimento(string alteradoPor)
        {
            Status = EStatusSolicitacaoTitular.EmAndamento;
            MarcarAlterado(alteradoPor);
        }

        public void Concluir(string alteradoPor)
        {
            Status = EStatusSolicitacaoTitular.Concluida;
            DataConclusao = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Negar(string alteradoPor)
        {
            Status = EStatusSolicitacaoTitular.Negada;
            DataConclusao = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<SolicitacaoTitular>()
                .Requires()
                .AreNotEquals(PessoaId, Guid.Empty, nameof(PessoaId), "PessoaId é obrigatório [Origem: SolicitacaoTitular]")
                .IsTrue(Enum.IsDefined(typeof(ETipoSolicitacaoTitular), Tipo), nameof(Tipo), "Tipo não consta na lista [Origem: SolicitacaoTitular]")
                .IsTrue(Enum.IsDefined(typeof(EStatusSolicitacaoTitular), Status), nameof(Status), "Status não consta na lista [Origem: SolicitacaoTitular]")
            );
        }
    }
}
