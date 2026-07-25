using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Fatura recorrente gerada por competência de um contrato (EF FIN-GCF §13.3 gcf_fatura_recorrente).
    /// Não deve duplicar competência para o mesmo contrato. Título de CR referenciado por Guid FK.
    /// </summary>
    public class FaturaRecorrente : EntidadeSaaSBase
    {
        public Guid ContratoId { get; private set; }
        public string Competencia { get; private set; } = string.Empty;
        public Guid? TituloContasReceberId { get; private set; }
        public decimal? Valor { get; private set; }
        public EStatusFaturaRecorrente Status { get; private set; } = EStatusFaturaRecorrente.Pendente;

        protected FaturaRecorrente() { } // EF Core

        public FaturaRecorrente(Guid contratoId, string competencia, decimal? valor, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ContratoId = contratoId;
            Competencia = competencia;
            Valor = valor;
            Status = EStatusFaturaRecorrente.Pendente;
            Validar();
        }

        public void MarcarGerada(Guid? tituloContasReceberId, string usuario)
        {
            Status = EStatusFaturaRecorrente.Gerada;
            TituloContasReceberId = tituloContasReceberId;
            MarcarAlterado(usuario);
        }

        public void Cancelar(string usuario)
        {
            Status = EStatusFaturaRecorrente.Cancelada;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<FaturaRecorrente>()
                .Requires()
                .IsNotEmpty(ContratoId, nameof(ContratoId), "O contrato é obrigatório [Origem: FaturaRecorrente]")
                .IsNotNullOrEmpty(Competencia, nameof(Competencia), "A competência é obrigatória [Origem: FaturaRecorrente]")
            );
        }
    }
}
