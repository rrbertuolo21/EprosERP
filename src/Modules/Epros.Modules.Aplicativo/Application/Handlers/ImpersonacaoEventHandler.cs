using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Domain.Events;
using MediatR;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    public class ImpersonacaoEventHandler : INotificationHandler<ImpersonacaoIniciadaEventNotification>
    {
        public Task Handle(ImpersonacaoIniciadaEventNotification notification, CancellationToken cancellationToken)
        {
            // Simula o registro em logs de auditoria de segurança de alto nível (REG-033)
            Console.WriteLine($"[SECURITY LOG] IMPERSONATION STARTED: Administrator '{notification.CriadoPor}' (Original ID: {notification.UsuarioOriginalId}) has started impersonating User target '{notification.UsuarioAlvoId}' for Company '{notification.EmpresaId}' on Tenant '{notification.TenantId}'. SessaoImpersonacaoId: {notification.SessaoImpersonacaoId}. Reason: '{notification.Motivo}'");

            // Simula envio de e-mail ao administrador do tenant alvo (REG-033)
            Console.WriteLine($"[SECURITY ALERT EMAIL SENT] to tenant administrators of Tenant '{notification.TenantId}': Attention, active impersonation session '{notification.SessaoImpersonacaoId}' has been initiated by system operator '{notification.CriadoPor}' with reason: '{notification.Motivo}'.");

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Auditoria de segurança do acesso de SUPORTE da Siser a um cliente (área Landlord). Mesma
    /// trilha/alerta da impersonação (REG-033), diferenciando o acesso por perfil de suporte.
    /// </summary>
    public class AcessoSuporteEventHandler : INotificationHandler<AcessoSuporteIniciadoEventNotification>
    {
        public Task Handle(AcessoSuporteIniciadoEventNotification notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"[SECURITY LOG] SUPPORT ACCESS STARTED: Siser operator '{notification.CriadoPor}' (Original ID: {notification.UsuarioOriginalId}) with support profile '{notification.PerfilSuporte}' has opened a support session on Client Tenant '{notification.TenantAlvo}', target user '{notification.UsuarioAlvoId}' for Company '{notification.EmpresaId}'. SessaoId: {notification.SessaoImpersonacaoId}. Reason: '{notification.Motivo}'");

            Console.WriteLine($"[SECURITY ALERT EMAIL SENT] to tenant administrators of Client '{notification.TenantAlvo}': a Siser support session '{notification.SessaoImpersonacaoId}' ('{notification.PerfilSuporte}') has been initiated by operator '{notification.CriadoPor}' with reason: '{notification.Motivo}'.");

            return Task.CompletedTask;
        }
    }
}
