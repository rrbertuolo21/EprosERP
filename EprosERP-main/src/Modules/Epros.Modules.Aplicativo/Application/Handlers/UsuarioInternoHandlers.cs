using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    public class CriarUsuarioInternoCommandHandler : ICommandHandler<CriarUsuarioInternoCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IPasswordHasher _passwordHasher;

        public CriarUsuarioInternoCommandHandler(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IPasswordHasher passwordHasher)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _passwordHasher = passwordHasher;
        }

        public async Task<CommandResult> Handle(CriarUsuarioInternoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var criadoPor = _currentUser.GetUserId() ?? "system";

            // Verifica e-mail único
            var emailExiste = await _context.UsuariosInternos
                .AnyAsync(u => u.Email == request.Email && u.DeletadoEm == null, cancellationToken);

            if (emailExiste)
            {
                return CommandResult.Falha(new[] { "Já existe um usuário cadastrado com este e-mail." });
            }

            if (string.IsNullOrWhiteSpace(request.Senha) || request.Senha.Length < 8)
            {
                return CommandResult.Falha(new[] { "A senha deve ter no mínimo 8 caracteres" });
            }

            var usuario = new UsuarioInterno(
                nome: request.Nome,
                email: request.Email,
                senha: _passwordHasher.Hash(request.Senha), // Senha derivada via PBKDF2 — nunca em texto puro
                creatorId: Guid.NewGuid(),
                uniqueId: Guid.NewGuid().ToString(),
                timezone: request.Timezone,
                primaryAdmin: request.PrimaryAdmin,
                tenantId: "system",
                criadoPor: criadoPor
            );

            if (!usuario.IsValid)
            {
                var erros = usuario.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Erro ao validar os dados do usuário interno.");
            }

            _context.UsuariosInternos.Add(usuario);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Usuário interno criado com sucesso!", new { UsuarioInternoId = usuario.Id });
        }
    }

    public class AlterarSenhaUsuarioInternoCommandHandler : ICommandHandler<AlterarSenhaUsuarioInternoCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IPasswordHasher _passwordHasher;

        public AlterarSenhaUsuarioInternoCommandHandler(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IPasswordHasher passwordHasher)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _passwordHasher = passwordHasher;
        }

        public async Task<CommandResult> Handle(AlterarSenhaUsuarioInternoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var usuario = await _context.UsuariosInternos
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioInternoId && u.DeletadoEm == null, cancellationToken);

            if (usuario == null)
            {
                return CommandResult.Falha(new[] { "Usuário interno não encontrado." });
            }

            if (string.IsNullOrWhiteSpace(request.NovaSenha) || request.NovaSenha.Length < 8)
            {
                return CommandResult.Falha(new[] { "A senha deve ter no mínimo 8 caracteres" });
            }

            usuario.AlterarSenha(_passwordHasher.Hash(request.NovaSenha), alteradoPor);

            if (!usuario.IsValid)
            {
                var erros = usuario.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Erro ao validar a alteração de senha.");
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Senha alterada com sucesso!");
        }
    }

    public class TornarAdminPrincipalCommandHandler : ICommandHandler<TornarAdminPrincipalCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public TornarAdminPrincipalCommandHandler(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(TornarAdminPrincipalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var usuario = await _context.UsuariosInternos
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioInternoId && u.DeletadoEm == null, cancellationToken);

            if (usuario == null)
            {
                return CommandResult.Falha(new[] { "Usuário interno não encontrado." });
            }

            usuario.TornarAdminPrincipal(alteradoPor);

            if (!usuario.IsValid)
            {
                var erros = usuario.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Erro ao validar a promoção para administrador principal.");
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Usuário promovido a administrador principal com sucesso!");
        }
    }
}
