using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Events
{
    public class UsuarioSyncEventHandler :
        INotificationHandler<UsuarioCriadoEventNotification>,
        INotificationHandler<UsuarioAtualizadoEventNotification>,
        INotificationHandler<UsuarioDeletadoEventNotification>
    {
        private readonly ContextGestaoClientes _context;

        public UsuarioSyncEventHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task Handle(UsuarioCriadoEventNotification notification, CancellationToken cancellationToken)
        {
            if (!notification.PerfilUsuarioId.HasValue)
                return;

            var perfilId = notification.PerfilUsuarioId.Value;
            var userIdStr = notification.UsuarioId.ToString();

            var existe = await _context.PerfisColaboradores
                .IgnoreQueryFilters()
                .AnyAsync(p => p.Id == perfilId, cancellationToken);

            if (existe)
                return;

            var perfil = new PerfilColaborador(
                id: perfilId,
                userId: userIdStr,
                nome: notification.Nome,
                email: notification.Email,
                cargo: notification.Cargo,
                departamento: notification.Departamento,
                limiteDesconto: notification.LimiteDesconto,
                tenantId: notification.TenantId,
                criadoPor: notification.CriadoPor
            );

            _context.PerfisColaboradores.Add(perfil);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task Handle(UsuarioAtualizadoEventNotification notification, CancellationToken cancellationToken)
        {
            var userIdStr = notification.UsuarioId.ToString();

            if (notification.PerfilUsuarioId.HasValue)
            {
                var perfilId = notification.PerfilUsuarioId.Value;
                var perfil = await _context.PerfisColaboradores
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.Id == perfilId, cancellationToken);

                if (perfil == null)
                {
                    // Se não existia o perfil com esse ID, cria
                    perfil = new PerfilColaborador(
                        id: perfilId,
                        userId: userIdStr,
                        nome: notification.Nome,
                        email: notification.Email,
                        cargo: notification.Cargo,
                        departamento: notification.Departamento,
                        limiteDesconto: notification.LimiteDesconto,
                        tenantId: notification.TenantId,
                        criadoPor: notification.AlteradoPor
                    );
                    _context.PerfisColaboradores.Add(perfil);
                }
                else
                {
                    // Se existia, atualiza
                    perfil.MarcarAlterado(notification.AlteradoPor);
                    perfil.Atualizar(
                        nome: notification.Nome,
                        cargo: notification.Cargo,
                        departamento: notification.Departamento,
                        limiteDesconto: notification.LimiteDesconto,
                        alteradoPor: notification.AlteradoPor
                    );
                    
                    if (!perfil.Ativo)
                    {
                        perfil.Ativar(notification.AlteradoPor);
                    }
                }
            }
            else
            {
                // Se PerfilUsuarioId for nulo, inativa os perfis ativos existentes para este usuário
                var perfis = await _context.PerfisColaboradores
                    .Where(p => p.UserId == userIdStr && p.DeletadoEm == null)
                    .ToListAsync(cancellationToken);

                foreach (var p in perfis)
                {
                    p.Inativar(notification.AlteradoPor);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task Handle(UsuarioDeletadoEventNotification notification, CancellationToken cancellationToken)
        {
            var userIdStr = notification.UsuarioId.ToString();

            var perfis = await _context.PerfisColaboradores
                .Where(p => p.UserId == userIdStr && p.DeletadoEm == null)
                .ToListAsync(cancellationToken);

            foreach (var p in perfis)
            {
                p.Inativar(notification.AlteradoPor);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
