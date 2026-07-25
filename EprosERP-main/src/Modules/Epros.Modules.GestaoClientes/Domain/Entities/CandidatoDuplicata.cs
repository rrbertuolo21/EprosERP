using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Candidato a duplicidade entre duas pessoas (CAD-PEM 12.14).</summary>
    public class CandidatoDuplicata : EntidadeSaaSBase
    {
        public Guid PessoaAId { get; private set; }
        public Guid PessoaBId { get; private set; }
        public decimal Score { get; private set; }
        public EStatusDuplicata Status { get; private set; }

        protected CandidatoDuplicata() { } // EF Core

        public CandidatoDuplicata(
            Guid pessoaAId,
            Guid pessoaBId,
            decimal score,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PessoaAId = pessoaAId;
            PessoaBId = pessoaBId;
            Score = score;
            Status = EStatusDuplicata.Pendente;
            Validar();
        }

        public void Confirmar(string alteradoPor)
        {
            Status = EStatusDuplicata.Confirmada;
            MarcarAlterado(alteradoPor);
        }

        public void Descartar(string alteradoPor)
        {
            Status = EStatusDuplicata.Descartada;
            MarcarAlterado(alteradoPor);
        }

        public void MarcarMesclada(string alteradoPor)
        {
            Status = EStatusDuplicata.Mesclada;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<CandidatoDuplicata>()
                .Requires()
                .AreNotEquals(PessoaAId, Guid.Empty, nameof(PessoaAId), "PessoaAId é obrigatório [Origem: CandidatoDuplicata]")
                .AreNotEquals(PessoaBId, Guid.Empty, nameof(PessoaBId), "PessoaBId é obrigatório [Origem: CandidatoDuplicata]")
                .IsFalse(PessoaAId == PessoaBId, nameof(PessoaBId), "As pessoas do candidato a duplicata devem ser diferentes [Origem: CandidatoDuplicata]")
                .IsTrue(Enum.IsDefined(typeof(EStatusDuplicata), Status), nameof(Status), "Status não consta na lista [Origem: CandidatoDuplicata]")
            );
        }
    }
}
