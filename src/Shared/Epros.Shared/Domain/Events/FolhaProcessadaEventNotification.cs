using System;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    /// <summary>
    /// Evento publicado quando a folha de pagamento de um colaborador é processada.
    /// Consumido pelo módulo Financeiro para gerar a correspondente Conta a Pagar de salários.
    /// </summary>
    public record FolhaProcessadaEventNotification(
        Guid FolhaPagamentoId,
        Guid ColaboradorId,
        string NomeColaborador,
        string CpfColaborador,
        int MesCompetencia,
        int AnoCompetencia,
        decimal SalarioLiquido,
        string TenantId
    ) : INotification;
}
