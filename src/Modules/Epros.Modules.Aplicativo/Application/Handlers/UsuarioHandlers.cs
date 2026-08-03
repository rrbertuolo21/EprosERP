using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
// Desambiguação: SessaoImpersonacao existe em Aplicativo e GestaoClientes; aqui persiste em ContextAplicativo.SessoesImpersonacao.
using SessaoImpersonacao = Epros.Modules.Aplicativo.Domain.Entities.SessaoImpersonacao;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    // CRUD: Criar
    // CRUD: Criar
    public class CriarUsuarioCommandHandler : ICommandHandler<CriarUsuarioCommand>
    {
        private readonly ContextAplicativo _contextApp;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IValidadorLimitesSaaS _validadorLimites;
        private readonly IPasswordHasher _passwordHasher;

        public CriarUsuarioCommandHandler(
            ContextAplicativo contextApp,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IValidadorLimitesSaaS validadorLimites,
            IPasswordHasher passwordHasher)
        {
            _contextApp = contextApp;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _validadorLimites = validadorLimites;
            _passwordHasher = passwordHasher;
        }

        public async Task<CommandResult> Handle(CriarUsuarioCommand request, CancellationToken cancellationToken)
        {
            var validator = new CriarUsuarioCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return CommandResult.Falha(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var emailLower = request.Email.ToLowerInvariant().Trim();
            var criadoPor = _currentUser.GetUserId() ?? "system";
            var tenantId = _tenantProvider.GetTenantId();

            var (excedeuLimite, msgLimite) = await _validadorLimites.ValidarLimiteUsuariosAsync(tenantId, cancellationToken);
            if (excedeuLimite)
            {
                return CommandResult.Falha(new[] { msgLimite });
            }

            // 1. Validar duplicidade global de email
            var emailExiste = await _contextApp.Usuarios
                .AnyAsync(u => u.Email == emailLower && u.DeletadoEm == null, cancellationToken);

            if (emailExiste)
            {
                return CommandResult.Falha(new[] { "Já existe um usuário cadastrado com este e-mail." });
            }

            // 2. Criar Usuario Credencial
            var usuario = new Usuario(
                tenantId: tenantId,
                nome: request.Nome,
                email: emailLower,
                passwordHash: _passwordHasher.Hash(request.Senha), // Senha derivada via PBKDF2 — nunca em texto puro
                tipo: request.Tipo,
                criadoPor: criadoPor
            );

            if (request.Status != UsuarioStatus.Active)
            {
                if (request.Status == UsuarioStatus.Disabled) usuario.Bloquear(criadoPor);
            }

            if (!usuario.IsValid)
            {
                return CommandResult.Falha(usuario.Notifications.Select(n => n.Message));
            }

            _contextApp.Usuarios.Add(usuario);
            await _contextApp.SaveChangesAsync(cancellationToken);

            // 3. Criar Vínculos e Enfileirar Evento de Outbox
            var perfilUsuarioId = Guid.NewGuid();
            var primeiroVinculoComum = request.Empresas.FirstOrDefault(e => !e.EhAdmin);

            foreach (var empInput in request.Empresas)
            {
                // Cria o vínculo UsuarioEmpresa
                var vinculo = new UsuarioEmpresa(
                    tenantId: tenantId,
                    usuarioId: usuario.Id,
                    empresaId: empInput.EmpresaId,
                    perfilUsuarioId: empInput.EhAdmin ? (Guid?)null : perfilUsuarioId,
                    ehAdmin: empInput.EhAdmin,
                    criadoPor: criadoPor
                );

                _contextApp.UsuariosEmpresas.Add(vinculo);
            }

            await _contextApp.SaveChangesAsync(cancellationToken);

            // Grava a mensagem na fila de Outbox do monólito para consistência eventual
            var payload = new
            {
                UsuarioId = usuario.Id,
                Nome = request.Nome,
                Email = emailLower,
                TenantId = tenantId,
                PerfilUsuarioId = primeiroVinculoComum != null ? (Guid?)perfilUsuarioId : null,
                Cargo = primeiroVinculoComum?.Cargo ?? "",
                Departamento = primeiroVinculoComum?.Departamento ?? "",
                LimiteDesconto = primeiroVinculoComum?.LimiteDesconto ?? 0m,
                CriadoPor = criadoPor
            };

            var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload, new System.Text.Json.JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            var outboxMessage = new Epros.Shared.Domain.Events.OutboxMessage(tenantId, "UsuarioCriado", payloadJson);
            _contextApp.OutboxMessages.Add(outboxMessage);

            await _contextApp.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Usuário criado com sucesso!", new { UsuarioId = usuario.Id });
        }
    }

    // CRUD: Atualizar
    public class AtualizarUsuarioCommandHandler : ICommandHandler<AtualizarUsuarioCommand>
    {
        private readonly ContextAplicativo _contextApp;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IValidadorLimitesSaaS _validadorLimites;

        public AtualizarUsuarioCommandHandler(
            ContextAplicativo contextApp,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IValidadorLimitesSaaS validadorLimites)
        {
            _contextApp = contextApp;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _validadorLimites = validadorLimites;
        }

        public async Task<CommandResult> Handle(AtualizarUsuarioCommand request, CancellationToken cancellationToken)
        {
            var validator = new AtualizarUsuarioCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return CommandResult.Falha(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var alteradoPor = _currentUser.GetUserId() ?? "system";
            var tenantId = _tenantProvider.GetTenantId();

            var usuario = await _contextApp.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioId && u.DeletadoEm == null, cancellationToken);

            if (usuario == null)
            {
                return CommandResult.Falha(new[] { "Usuário não encontrado." });
            }

            // Atualiza dados básicos
            usuario.AtualizarTelefone(request.Telefone, alteradoPor);
            if (request.Status == UsuarioStatus.Active && usuario.Status != UsuarioStatus.Active)
            {
                var (excedeuLimite, msgLimite) = await _validadorLimites.ValidarLimiteUsuariosAsync(tenantId, cancellationToken);
                if (excedeuLimite)
                {
                    return CommandResult.Falha(new[] { msgLimite });
                }
                usuario.Ativar(alteradoPor);
            }
            else if (request.Status == UsuarioStatus.Disabled && usuario.Status != UsuarioStatus.Disabled)
            {
                usuario.Bloquear(alteradoPor);
            }

            // Sincronizar vínculos de empresa
            var vinculosAtuais = await _contextApp.UsuariosEmpresas
                .Where(ue => ue.UsuarioId == request.UsuarioId && ue.DeletadoEm == null)
                .ToListAsync(cancellationToken);

            var inputsEmpresaIds = request.Empresas.Select(e => e.EmpresaId).ToList();

            // 1. Identificar vínculos removidos e aplicar Trava do Último Admin
            var vinculosParaRemover = vinculosAtuais.Where(v => !inputsEmpresaIds.Contains(v.EmpresaId)).ToList();
            foreach (var link in vinculosParaRemover)
            {
                if (link.EhAdmin)
                {
                    var outroAdminExiste = await _contextApp.UsuariosEmpresas
                        .AnyAsync(ue => ue.EmpresaId == link.EmpresaId && ue.EhAdmin && ue.UsuarioId != request.UsuarioId && ue.DeletadoEm == null, cancellationToken);

                    if (!outroAdminExiste)
                    {
                        return CommandResult.Falha(new[] { $"Não é possível remover o vínculo da empresa (ID: {link.EmpresaId}) pois este usuário é o único administrador ativo." });
                    }
                }
                link.Deletar(alteradoPor);
            }

            // 2. Processar inserções e edições
            var vinculoComPerfilExistente = vinculosAtuais.FirstOrDefault(v => v.PerfilAcessoId.HasValue);
            var perfilUsuarioId = vinculoComPerfilExistente?.PerfilAcessoId;

            var primeiroInputComum = request.Empresas.FirstOrDefault(e => !e.EhAdmin);
            if (perfilUsuarioId == null && primeiroInputComum != null)
            {
                perfilUsuarioId = Guid.NewGuid();
            }

            foreach (var input in request.Empresas)
            {
                var vinculoExistente = vinculosAtuais.FirstOrDefault(v => v.EmpresaId == input.EmpresaId);
                var perfilIdParaVinculo = input.EhAdmin ? (Guid?)null : perfilUsuarioId;

                if (vinculoExistente == null)
                {
                    // Adicionar novo vínculo
                    var novoVinculo = new UsuarioEmpresa(
                        tenantId: tenantId,
                        usuarioId: usuario.Id,
                        empresaId: input.EmpresaId,
                        perfilUsuarioId: perfilIdParaVinculo,
                        ehAdmin: input.EhAdmin,
                        criadoPor: alteradoPor
                    );
                    _contextApp.UsuariosEmpresas.Add(novoVinculo);
                }
                else
                {
                    // Se estiver rebaixando admin para comum, verifica se há outro admin ativo na empresa
                    if (vinculoExistente.EhAdmin && !input.EhAdmin)
                    {
                        var outroAdminExiste = await _contextApp.UsuariosEmpresas
                            .AnyAsync(ue => ue.EmpresaId == input.EmpresaId && ue.EhAdmin && ue.UsuarioId != request.UsuarioId && ue.DeletadoEm == null, cancellationToken);

                        if (!outroAdminExiste)
                        {
                            return CommandResult.Falha(new[] { $"Não é possível remover privilégios de administrador da empresa (ID: {input.EmpresaId}) pois este usuário é o único administrador ativo." });
                        }
                    }

                    vinculoExistente.AtualizarPerfil(perfilIdParaVinculo, alteradoPor);
                    vinculoExistente.DefinirAdmin(input.EhAdmin, alteradoPor);
                }
            }

            await _contextApp.SaveChangesAsync(cancellationToken);

            // Grava o evento UsuarioAtualizado no Outbox
            var payload = new
            {
                UsuarioId = usuario.Id,
                Nome = request.Nome,
                Email = usuario.Email,
                TenantId = tenantId,
                PerfilUsuarioId = perfilUsuarioId,
                Cargo = primeiroInputComum?.Cargo ?? "",
                Departamento = primeiroInputComum?.Departamento ?? "",
                LimiteDesconto = primeiroInputComum?.LimiteDesconto ?? 0m,
                AlteradoPor = alteradoPor
            };

            var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload, new System.Text.Json.JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            var outboxMessage = new Epros.Shared.Domain.Events.OutboxMessage(tenantId, "UsuarioAtualizado", payloadJson);
            _contextApp.OutboxMessages.Add(outboxMessage);
            await _contextApp.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Usuário atualizado com sucesso!");
        }
    }

    // CRUD: Deletar
    public class DeletarUsuarioCommandHandler : ICommandHandler<DeletarUsuarioCommand>
    {
        private readonly ContextAplicativo _contextApp;
        private readonly ICurrentUser _currentUser;

        public DeletarUsuarioCommandHandler(
            ContextAplicativo contextApp,
            ICurrentUser currentUser)
        {
            _contextApp = contextApp;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarUsuarioCommand request, CancellationToken cancellationToken)
        {
            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var usuario = await _contextApp.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioId && u.DeletadoEm == null, cancellationToken);

            if (usuario == null)
            {
                return CommandResult.Falha(new[] { "Usuário não encontrado." });
            }

            var vinculos = await _contextApp.UsuariosEmpresas
                .Where(ue => ue.UsuarioId == request.UsuarioId && ue.DeletadoEm == null)
                .ToListAsync(cancellationToken);

            // Verifica Trava do Último Admin para todas as empresas em que o usuário atua como admin
            foreach (var link in vinculos)
            {
                if (link.EhAdmin)
                {
                    var outroAdminExiste = await _contextApp.UsuariosEmpresas
                        .AnyAsync(ue => ue.EmpresaId == link.EmpresaId && ue.EhAdmin && ue.UsuarioId != request.UsuarioId && ue.DeletadoEm == null, cancellationToken);

                    if (!outroAdminExiste)
                    {
                        return CommandResult.Falha(new[] { $"Não é possível excluir o usuário pois ele é o único administrador ativo da empresa (ID: {link.EmpresaId})." });
                    }
                }
            }

            // Soft delete no usuário
            usuario.Deletar(alteradoPor);

            // Soft delete nos vínculos
            foreach (var link in vinculos)
            {
                link.Deletar(alteradoPor);
            }

            await _contextApp.SaveChangesAsync(cancellationToken);

            // Grava o evento UsuarioDeletado no Outbox
            var payload = new
            {
                UsuarioId = usuario.Id,
                AlteradoPor = alteradoPor
            };

            var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload, new System.Text.Json.JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            var outboxMessage = new Epros.Shared.Domain.Events.OutboxMessage(usuario.TenantId, "UsuarioDeletado", payloadJson);
            _contextApp.OutboxMessages.Add(outboxMessage);
            await _contextApp.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Usuário excluído com sucesso!");
        }
    }

    // Troca de Senha Administrativa
    public class AlterarSenhaAdministrativaCommandHandler : ICommandHandler<AlterarSenhaAdministrativaCommand>
    {
        private readonly ContextAplicativo _contextApp;
        private readonly ICurrentUser _currentUser;
        private readonly IPasswordHasher _passwordHasher;

        public AlterarSenhaAdministrativaCommandHandler(ContextAplicativo contextApp, ICurrentUser currentUser, IPasswordHasher passwordHasher)
        {
            _contextApp = contextApp;
            _currentUser = currentUser;
            _passwordHasher = passwordHasher;
        }

        public async Task<CommandResult> Handle(AlterarSenhaAdministrativaCommand request, CancellationToken cancellationToken)
        {
            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var usuario = await _contextApp.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioId && u.DeletadoEm == null, cancellationToken);

            if (usuario == null)
            {
                return CommandResult.Falha(new[] { "Usuário não encontrado." });
            }

            // REG-046 / CT-008 — a nova senha não pode ser igual à senha atual. Comparação via hasher
            // (hashes são salgados; a igualdade só é detectável verificando a senha crua contra o hash).
            if (_passwordHasher.Verify(request.NovaSenha, usuario.PasswordHash))
            {
                return CommandResult.Falha(new[] { "A nova senha deve ser diferente da senha atual." });
            }

            usuario.AlterarSenha(_passwordHasher.Hash(request.NovaSenha), alteradoPor);

            if (!usuario.IsValid)
            {
                return CommandResult.Falha(usuario.Notifications.Select(n => n.Message));
            }

            await _contextApp.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Senha administrativa alterada com sucesso!");
        }
    }

    // API Key: Gerar
    public class GerarApiKeyCommandHandler : ICommandHandler<GerarApiKeyCommand>
    {
        private readonly ContextAplicativo _contextApp;
        private readonly ICurrentUser _currentUser;

        public GerarApiKeyCommandHandler(ContextAplicativo contextApp, ICurrentUser currentUser)
        {
            _contextApp = contextApp;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(GerarApiKeyCommand request, CancellationToken cancellationToken)
        {
            var validator = new GerarApiKeyCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return CommandResult.Falha(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var usuario = await _contextApp.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioId && u.DeletadoEm == null, cancellationToken);

            if (usuario == null)
            {
                return CommandResult.Falha(new[] { "Usuário não encontrado." });
            }

            var rateLimit = request.RateLimit ?? 60;
            var validadeDias = request.ValidadeDias ?? 90; // Default: rotação obrigatória de 90 dias

            usuario.GerarApiKey(rateLimit, validadeDias, alteradoPor);
            await _contextApp.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("API Key gerada com sucesso!", new { ApiKey = usuario.ApiKey });
        }
    }

    // API Key: Revogar
    public class RevogarApiKeyCommandHandler : ICommandHandler<RevogarApiKeyCommand>
    {
        private readonly ContextAplicativo _contextApp;
        private readonly ICurrentUser _currentUser;

        public RevogarApiKeyCommandHandler(ContextAplicativo contextApp, ICurrentUser currentUser)
        {
            _contextApp = contextApp;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RevogarApiKeyCommand request, CancellationToken cancellationToken)
        {
            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var usuario = await _contextApp.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioId && u.DeletadoEm == null, cancellationToken);

            if (usuario == null)
            {
                return CommandResult.Falha(new[] { "Usuário não encontrado." });
            }

            usuario.RevogarApiKey(alteradoPor);
            await _contextApp.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("API Key revogada com sucesso!");
        }
    }

    // Impersonação: Iniciar
    public class IniciarImpersonacaoCommandHandler : ICommandHandler<IniciarImpersonacaoCommand>
    {
        private readonly ContextAplicativo _contextApp;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IEprosTokenService _tokenService;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public IniciarImpersonacaoCommandHandler(
            ContextAplicativo contextApp,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IEprosTokenService tokenService,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            _contextApp = contextApp;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _tokenService = tokenService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CommandResult> Handle(IniciarImpersonacaoCommand request, CancellationToken cancellationToken)
        {
            var validator = new IniciarImpersonacaoCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return CommandResult.Falha(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var adminTenantId = _tenantProvider.GetTenantId();
            if (adminTenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Impersonação é restrita a administradores do sistema (Siser)." });
            }

            var adminIdStr = _currentUser.GetUserId() ?? "system";
            var adminId = Guid.TryParse(adminIdStr, out var aid) ? aid : Guid.Empty;

            if (adminId == request.UsuarioAlvoId)
            {
                return CommandResult.Falha(new[] { "Não é permitido impersonar a si mesmo." });
            }

            var usuarioAlvo = await _contextApp.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioAlvoId && u.DeletadoEm == null, cancellationToken);

            if (usuarioAlvo == null)
            {
                return CommandResult.Falha(new[] { "Usuário alvo não encontrado ou inativo." });
            }

            // SALVAGUARDA: proibido impersonar/dar suporte a usuário ADMIN do tenant.
            if (await AcessoSuportePolicy.AlvoEhAdminAsync(_contextApp, request.UsuarioAlvoId, usuarioAlvo.TenantId, cancellationToken))
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: não é permitido impersonar um usuário ADMIN do tenant." });
            }

            // Encontra um vínculo válido para gerar o token
            var vinculo = await _contextApp.UsuariosEmpresas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(ue => ue.UsuarioId == request.UsuarioAlvoId && ue.DeletadoEm == null, cancellationToken);

            if (vinculo == null && request.EmpresaId.HasValue)
            {
                return CommandResult.Falha(new[] { "Usuário alvo não possui vínculo com nenhuma empresa." });
            }

            var empresaId = request.EmpresaId ?? vinculo?.EmpresaId;
            var perfilId = vinculo?.PerfilAcessoId;

            // Registra auditoria
            var sessao = new SessaoImpersonacao(
                tenantId: adminTenantId,
                usuarioOriginalId: adminId,
                usuarioAlvoId: request.UsuarioAlvoId,
                empresaId: empresaId,
                motivo: request.Motivo,
                ipOrigem: AcessoSuportePolicy.ObterIpOrigem(_httpContextAccessor),
                criadoPor: adminIdStr
            );

            _contextApp.SessoesImpersonacao.Add(sessao);
            await _contextApp.SaveChangesAsync(cancellationToken);

            // Grava evento no outbox para logs/alerta de segurança (REG-033)
            var payloadImpersonacao = new
            {
                SessaoImpersonacaoId = sessao.Id,
                UsuarioOriginalId = sessao.UsuarioOriginalId,
                UsuarioAlvoId = sessao.UsuarioAlvoId,
                EmpresaId = sessao.EmpresaId,
                Motivo = sessao.Motivo,
                CriadoPor = adminIdStr,
                TenantId = usuarioAlvo.TenantId
            };

            var payloadImpersonacaoJson = System.Text.Json.JsonSerializer.Serialize(payloadImpersonacao, new System.Text.Json.JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            var outboxImpersonacao = new Epros.Shared.Domain.Events.OutboxMessage(adminTenantId, "ImpersonacaoIniciada", payloadImpersonacaoJson);
            _contextApp.OutboxMessages.Add(outboxImpersonacao);
            await _contextApp.SaveChangesAsync(cancellationToken);

            // Gera o token de segurança para a conta alvo
            var expiracao = DateTime.UtcNow.AddHours(2);
            var token = _tokenService.GerarCompleto(usuarioAlvo.TenantId, usuarioAlvo.Id.ToString(), empresaId?.ToString() ?? "null", perfilId?.ToString() ?? "null");

            var dto = new ImpersonacaoResultDto(
                Token: token,
                Expiracao: expiracao,
                UsuarioAlvoId: usuarioAlvo.Id,
                NomeAlvo: usuarioAlvo.Nome,
                EmpresaId: empresaId,
                TenantId: usuarioAlvo.TenantId,
                SessaoImpersonacaoId: sessao.Id
            );

            return CommandResult.Ok("Impersonação iniciada com sucesso!", dto);
        }
    }

    // Impersonação: Encerrar
    public class EncerrarImpersonacaoCommandHandler : ICommandHandler<EncerrarImpersonacaoCommand>
    {
        private readonly ContextAplicativo _contextApp;
        private readonly ICurrentUser _currentUser;

        public EncerrarImpersonacaoCommandHandler(ContextAplicativo contextApp, ICurrentUser currentUser)
        {
            _contextApp = contextApp;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(EncerrarImpersonacaoCommand request, CancellationToken cancellationToken)
        {
            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var sessao = await _contextApp.SessoesImpersonacao
                .FirstOrDefaultAsync(s => s.Id == request.SessaoImpersonacaoId && s.DeletadoEm == null, cancellationToken);

            if (sessao == null)
            {
                return CommandResult.Falha(new[] { "Sessão de impersonação não encontrada." });
            }

            sessao.Encerrar(alteradoPor);
            await _contextApp.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Impersonação encerrada com sucesso!");
        }
    }

    /// <summary>
    /// Política compartilhada de acesso de suporte/impersonação. Centraliza a detecção de "alvo é
    /// ADMIN do tenant" (salvaguarda) para não duplicar a regra entre impersonação e suporte.
    /// </summary>
    internal static class AcessoSuportePolicy
    {
        /// <summary>
        /// IP de origem REAL da requisição (antes ficava hardcoded "127.0.0.1"). Usa o RemoteIpAddress
        /// do HttpContext; cai para "127.0.0.1" apenas fora de um contexto HTTP (ex.: jobs/testes).
        /// </summary>
        public static string ObterIpOrigem(IHttpContextAccessor? httpContextAccessor)
        {
            var ip = httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            return string.IsNullOrWhiteSpace(ip) ? "127.0.0.1" : ip;
        }

        /// <summary>
        /// True se o usuário alvo é ADMIN do tenant: possui algum vínculo de empresa com
        /// <c>EhAdmin</c> ou é <c>Proprietario</c> (dono/admin primário) do tenant. Nesses casos o
        /// acesso de suporte/impersonação é proibido.
        /// </summary>
        public static async Task<bool> AlvoEhAdminAsync(
            ContextAplicativo contextApp,
            Guid usuarioAlvoId,
            string tenantAlvo,
            CancellationToken cancellationToken)
        {
            var ehAdminEmpresa = await contextApp.UsuariosEmpresas
                .IgnoreQueryFilters()
                .AnyAsync(ue => ue.UsuarioId == usuarioAlvoId
                                && ue.EhAdmin
                                && ue.DeletadoEm == null, cancellationToken);
            if (ehAdminEmpresa) return true;

            var ehProprietario = await contextApp.AcessosUsuarioTenant
                .IgnoreQueryFilters()
                .AnyAsync(a => a.UsuarioId == usuarioAlvoId
                               && a.TenantId == tenantAlvo
                               && a.Papel == PapelAcessoTenant.Proprietario
                               && a.DeletadoEm == null, cancellationToken);

            return ehProprietario;
        }
    }

    // Landlord — Iniciar acesso de suporte da Siser a um tenant cliente (1.04 Pass 4).
    // Reusa a trilha/entidade de auditoria da impersonação (SessaoImpersonacao) + outbox; a diferença
    // é que é um acesso Siser concedido por PERFIL DE SUPORTE, registrado com TipoAcesso de suporte.
    public class IniciarAcessoSuporteCommandHandler : ICommandHandler<IniciarAcessoSuporteCommand>
    {
        private readonly ContextAplicativo _contextApp;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IEprosTokenService _tokenService;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public IniciarAcessoSuporteCommandHandler(
            ContextAplicativo contextApp,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IEprosTokenService tokenService,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            _contextApp = contextApp;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _tokenService = tokenService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CommandResult> Handle(IniciarAcessoSuporteCommand request, CancellationToken cancellationToken)
        {
            var validator = new IniciarAcessoSuporteCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return CommandResult.Falha(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            // FRONTEIRA LANDLORD × CLIENTE: só o landlord/Siser (tenant "system") pode dar suporte.
            // Mesma checagem da impersonação (UsuarioHandlers: IniciarImpersonacaoCommandHandler).
            var landlordTenantId = _tenantProvider.GetTenantId();
            if (!string.Equals(landlordTenantId, "system", StringComparison.OrdinalIgnoreCase))
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: o acesso de suporte é restrito à área Landlord (Siser)." });
            }

            if (string.Equals(request.TenantAlvo, "system", StringComparison.OrdinalIgnoreCase))
            {
                return CommandResult.Falha(new[] { "O tenant alvo do suporte deve ser um cliente, não o landlord." });
            }

            var operadorIdStr = _currentUser.GetUserId() ?? "system";
            var operadorId = Guid.TryParse(operadorIdStr, out var oid) ? oid : Guid.Empty;

            // O portador precisa ser um usuário interno da Siser COM perfil de suporte (papel de sistema).
            var operador = await _contextApp.UsuariosInternos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == operadorId && u.DeletadoEm == null, cancellationToken);

            if (operador == null)
            {
                return CommandResult.Falha(new[] { "Operador Siser não encontrado na área Landlord." });
            }

            if (!operador.PodeDarSuporte)
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: o operador não possui perfil de suporte (Suporte Técnico ou Suporte Negócio)." });
            }

            if (operadorId == request.UsuarioAlvoId)
            {
                return CommandResult.Falha(new[] { "Não é permitido abrir suporte para si mesmo." });
            }

            // Usuário alvo precisa existir e pertencer ao tenant cliente informado.
            var usuarioAlvo = await _contextApp.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioAlvoId && u.DeletadoEm == null, cancellationToken);

            if (usuarioAlvo == null)
            {
                return CommandResult.Falha(new[] { "Usuário alvo não encontrado ou inativo." });
            }

            if (!string.Equals(usuarioAlvo.TenantId, request.TenantAlvo, StringComparison.OrdinalIgnoreCase))
            {
                return CommandResult.Falha(new[] { "O usuário alvo não pertence ao tenant cliente informado." });
            }

            // SALVAGUARDA: proibido abrir suporte para usuário ADMIN do tenant.
            if (await AcessoSuportePolicy.AlvoEhAdminAsync(_contextApp, request.UsuarioAlvoId, request.TenantAlvo, cancellationToken))
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: não é permitido abrir suporte para um usuário ADMIN do tenant." });
            }

            // Vínculo para escopar o token à empresa/perfil (mesmo padrão da impersonação).
            var vinculo = await _contextApp.UsuariosEmpresas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(ue => ue.UsuarioId == request.UsuarioAlvoId && ue.DeletadoEm == null, cancellationToken);

            if (vinculo == null && request.EmpresaId.HasValue)
            {
                return CommandResult.Falha(new[] { "Usuário alvo não possui vínculo com nenhuma empresa." });
            }

            var empresaId = request.EmpresaId ?? vinculo?.EmpresaId;
            var perfilId = vinculo?.PerfilAcessoId;

            var tipoAcesso = operador.PerfilSuporte == PerfilSuporteSiser.SuporteNegocio
                ? TipoSessaoAcesso.SuporteNegocio
                : TipoSessaoAcesso.SuporteTecnico;

            // Auditoria: MESMA entidade/trilha da impersonação, marcada como suporte (TipoAcesso + TenantAlvo).
            var sessao = new SessaoImpersonacao(
                tenantId: landlordTenantId,
                usuarioOriginalId: operadorId,
                usuarioAlvoId: request.UsuarioAlvoId,
                empresaId: empresaId,
                motivo: request.Motivo,
                ipOrigem: AcessoSuportePolicy.ObterIpOrigem(_httpContextAccessor),
                criadoPor: operadorIdStr,
                tipoAcesso: tipoAcesso,
                tenantAlvo: request.TenantAlvo);

            _contextApp.SessoesImpersonacao.Add(sessao);
            await _contextApp.SaveChangesAsync(cancellationToken);

            // Evento no outbox (mesma trilha/alerta de segurança da impersonação — REG-033).
            var payloadSuporte = new
            {
                SessaoImpersonacaoId = sessao.Id,
                UsuarioOriginalId = sessao.UsuarioOriginalId,
                UsuarioAlvoId = sessao.UsuarioAlvoId,
                EmpresaId = sessao.EmpresaId,
                Motivo = sessao.Motivo,
                CriadoPor = operadorIdStr,
                TenantAlvo = request.TenantAlvo,
                PerfilSuporte = tipoAcesso.ToString()
            };

            var payloadSuporteJson = System.Text.Json.JsonSerializer.Serialize(payloadSuporte, new System.Text.Json.JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            var outboxSuporte = new Epros.Shared.Domain.Events.OutboxMessage(landlordTenantId, "AcessoSuporteIniciado", payloadSuporteJson);
            _contextApp.OutboxMessages.Add(outboxSuporte);
            await _contextApp.SaveChangesAsync(cancellationToken);

            // Token escopado ao TENANT-ALVO (cliente), não ao landlord.
            var expiracao = DateTime.UtcNow.Add(_tokenService.Validade);
            var token = _tokenService.GerarCompleto(request.TenantAlvo, usuarioAlvo.Id.ToString(), empresaId?.ToString() ?? "null", perfilId?.ToString() ?? "null");

            var dto = new AcessoSuporteResultDto(
                Token: token,
                Expiracao: expiracao,
                UsuarioAlvoId: usuarioAlvo.Id,
                NomeAlvo: usuarioAlvo.Nome,
                EmpresaId: empresaId,
                TenantAlvo: request.TenantAlvo,
                PerfilSuporte: tipoAcesso.ToString(),
                SessaoSuporteId: sessao.Id);

            return CommandResult.Ok("Sessão de suporte iniciada com sucesso!", dto);
        }
    }

    // Landlord — Definir o perfil de suporte (papel de sistema da Siser) de um usuário interno.
    public class DefinirPerfilSuporteInternoCommandHandler : ICommandHandler<DefinirPerfilSuporteInternoCommand>
    {
        private readonly ContextAplicativo _contextApp;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DefinirPerfilSuporteInternoCommandHandler(
            ContextAplicativo contextApp,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _contextApp = contextApp;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DefinirPerfilSuporteInternoCommand request, CancellationToken cancellationToken)
        {
            var validator = new DefinirPerfilSuporteInternoCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return CommandResult.Falha(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            if (!string.Equals(_tenantProvider.GetTenantId(), "system", StringComparison.OrdinalIgnoreCase))
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: gerir perfis de suporte é restrito à área Landlord (Siser)." });
            }

            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var interno = await _contextApp.UsuariosInternos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioInternoId && u.DeletadoEm == null, cancellationToken);

            if (interno == null)
            {
                return CommandResult.Falha(new[] { "Usuário interno não encontrado." });
            }

            interno.DefinirPerfilSuporte(request.PerfilSuporte, alteradoPor);
            await _contextApp.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Perfil de suporte atualizado com sucesso!");
        }
    }

    // Preferências: Atualizar
    public class AtualizarPreferenciasUsuarioCommandHandler : ICommandHandler<AtualizarPreferenciasUsuarioCommand>
    {
        private readonly ContextAplicativo _contextApp;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtualizarPreferenciasUsuarioCommandHandler(
            ContextAplicativo contextApp,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _contextApp = contextApp;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarPreferenciasUsuarioCommand request, CancellationToken cancellationToken)
        {
            var alteradoPor = _currentUser.GetUserId() ?? "system";
            var tenantId = _tenantProvider.GetTenantId();

            var prefs = await _contextApp.PreferenciasUsuarios
                .FirstOrDefaultAsync(p => p.UsuarioId == request.UsuarioId && p.DeletadoEm == null, cancellationToken);

            if (prefs == null)
            {
                prefs = new PreferenciaUsuario(
                    tenantId: tenantId,
                    usuarioId: request.UsuarioId,
                    idioma: request.Idioma,
                    tema: request.Tema,
                    avatar: request.Avatar,
                    recebeNotificacoes: request.RecebeNotificacoes,
                    timezone: request.Timezone,
                    formatoData: request.FormatoData,
                    formatoHora: request.FormatoHora,
                    formatoNumero: request.FormatoNumero,
                    preferenciasJson: request.PreferenciasJson,
                    criadoPor: alteradoPor
                );
                _contextApp.PreferenciasUsuarios.Add(prefs);
            }
            else
            {
                prefs.Atualizar(
                    idioma: request.Idioma,
                    tema: request.Tema,
                    avatar: request.Avatar,
                    recebeNotificacoes: request.RecebeNotificacoes,
                    timezone: request.Timezone,
                    formatoData: request.FormatoData,
                    formatoHora: request.FormatoHora,
                    formatoNumero: request.FormatoNumero,
                    preferenciasJson: request.PreferenciasJson,
                    alteradoPor: alteradoPor
                );
            }

            await _contextApp.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Preferências salvas com sucesso!");
        }
    }

    // Query: Obter Usuário Detalhado
    public class ObterUsuarioQueryHandler : IQueryHandler<ObterUsuarioQuery, UsuarioDetalhadoDto>
    {
        private readonly ContextAplicativo _contextApp;
        private readonly ContextGestaoClientes _contextGestao;

        public ObterUsuarioQueryHandler(ContextAplicativo contextApp, ContextGestaoClientes contextGestao)
        {
            _contextApp = contextApp;
            _contextGestao = contextGestao;
        }

        public async Task<UsuarioDetalhadoDto> Handle(ObterUsuarioQuery request, CancellationToken cancellationToken)
        {
            var usuario = await _contextApp.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioId && u.DeletadoEm == null, cancellationToken);

            if (usuario == null) return null!;

            var prefs = await _contextApp.PreferenciasUsuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UsuarioId == request.UsuarioId && p.DeletadoEm == null, cancellationToken);

            var vinculos = await _contextApp.UsuariosEmpresas
                .AsNoTracking()
                .Where(ue => ue.UsuarioId == request.UsuarioId && ue.DeletadoEm == null)
                .ToListAsync(cancellationToken);

            var empresaIds = vinculos.Select(v => v.EmpresaId).ToList();

            var empresasMap = await _contextGestao.Empresas
                .AsNoTracking()
                .Where(e => empresaIds.Contains(e.Id) && e.DeletadoEm == null)
                .ToDictionaryAsync(e => e.Id, e => e.RazaoSocial, cancellationToken);

            var usuarioIdStr = usuario.Id.ToString();
            var perfisMap = await _contextGestao.PerfisUsuarios
                .AsNoTracking()
                .Where(p => p.UserId == usuarioIdStr && p.DeletadoEm == null)
                .ToDictionaryAsync(p => p.Id, p => p, cancellationToken);

            var vinculosDtos = vinculos.Select(v =>
            {
                var razaoSocial = empresasMap.TryGetValue(v.EmpresaId, out var r) ? r : "Empresa Desconhecida";
                var cargo = "";
                var depto = "";
                var limite = 0m;

                if (v.PerfilAcessoId.HasValue && perfisMap.TryGetValue(v.PerfilAcessoId.Value, out var p))
                {
                    cargo = p.Cargo;
                    depto = p.Departamento;
                    limite = p.LimiteDesconto;
                }

                return new UsuarioEmpresaVinculoDto(
                    EmpresaId: v.EmpresaId,
                    RazaoSocial: razaoSocial,
                    EhAdmin: v.EhAdmin,
                    PerfilUsuarioId: v.PerfilAcessoId,
                    Cargo: cargo,
                    Departamento: depto,
                    LimiteDesconto: limite
                );
            }).ToList();

            var prefsDto = prefs == null ? null : new PreferenciaUsuarioDto(
                Idioma: prefs.Idioma,
                Tema: prefs.Tema,
                Avatar: prefs.Avatar,
                RecebeNotificacoes: prefs.RecebeNotificacoes,
                Timezone: prefs.Timezone,
                FormatoData: prefs.FormatoData,
                FormatoHora: prefs.FormatoHora,
                FormatoNumero: prefs.FormatoNumero,
                PreferenciasJson: prefs.PreferenciasJson
            );

            return new UsuarioDetalhadoDto(
                Id: usuario.Id,
                Nome: usuario.Nome,
                Email: usuario.Email,
                Telefone: usuario.Telefone,
                Status: usuario.Status.ToString(),
                Tipo: usuario.Tipo.ToString(),
                MfaHabilitado: usuario.MfaHabilitado,
                ApiKey: usuario.ApiKey,
                ApiKeyCreated: usuario.ApiKeyCreated,
                ApiKeyExpiration: usuario.ApiKeyExpiration,
                ApiKeyLastUsed: usuario.ApiKeyLastUsed,
                ApiKeyRateLimit: usuario.ApiKeyRateLimit,
                Preferencias: prefsDto,
                Empresas: vinculosDtos
            );
        }
    }

    // Query: Listar Usuários do Tenant
    public class ListarUsuariosQueryHandler : IQueryHandler<ListarUsuariosQuery, IEnumerable<UsuarioDto>>
    {
        private readonly ContextAplicativo _contextApp;
        private readonly ITenantProvider _tenantProvider;

        public ListarUsuariosQueryHandler(ContextAplicativo contextApp, ITenantProvider tenantProvider)
        {
            _contextApp = contextApp;
            _tenantProvider = tenantProvider;
        }

        public async Task<IEnumerable<UsuarioDto>> Handle(ListarUsuariosQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var search = request.Search?.ToLowerInvariant().Trim();

            var query = _contextApp.Usuarios
                .AsNoTracking()
                .Where(u => u.TenantId == tenantId && u.DeletadoEm == null);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.Email.Contains(search) || u.Nome.ToLower().Contains(search));
            }

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => new UsuarioDto(
                    u.Id,
                    u.Nome,
                    u.Email,
                    u.Telefone,
                    u.Status.ToString(),
                    u.Tipo.ToString(),
                    u.MfaHabilitado,
                    u.ApiKey
                ))
                .ToListAsync(cancellationToken);

            return list;
        }
    }

    // Query: Listar Histórico de Login
    public class ListarHistoricoLoginQueryHandler : IQueryHandler<ListarHistoricoLoginQuery, IEnumerable<HistoricoLoginDto>>
    {
        private readonly ContextAplicativo _contextApp;
        private readonly ITenantProvider _tenantProvider;

        public ListarHistoricoLoginQueryHandler(ContextAplicativo contextApp, ITenantProvider tenantProvider)
        {
            _contextApp = contextApp;
            _tenantProvider = tenantProvider;
        }

        public async Task<IEnumerable<HistoricoLoginDto>> Handle(ListarHistoricoLoginQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var search = request.Search?.ToLowerInvariant().Trim();

            var query = _contextApp.HistoricosLogin
                .AsNoTracking()
                .Where(h => h.TenantId == tenantId && h.DeletadoEm == null);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(h => h.EmailTentado.Contains(search));
            }

            var list = await query
                .OrderByDescending(h => h.CriadoEm)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(h => new HistoricoLoginDto(
                    h.Id,
                    h.EmailTentado,
                    h.IpAddress,
                    h.UserAgent,
                    h.Sucesso,
                    h.Detalhes,
                    h.CriadoEm
                ))
                .ToListAsync(cancellationToken);

            return list;
        }
    }
}
