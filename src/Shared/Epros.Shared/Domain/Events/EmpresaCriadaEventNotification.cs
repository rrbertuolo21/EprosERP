using System;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    /// <summary>
    /// Evento publicado após commit da criação de uma Empresa/organização (CAD · contrato CAD-* T-03: empresa.criada).
    /// Consumido por Fiscal (emissão DFe), Financeiro e Plataforma. Schema congelado v1.
    /// </summary>
    public record EmpresaCriadaEventNotification(
        Guid EmpresaId,
        string TenantId,
        string Cnpj,
        string RazaoSocial,
        DateTime CriadoEm,
        string UserId
    ) : INotification;
}
