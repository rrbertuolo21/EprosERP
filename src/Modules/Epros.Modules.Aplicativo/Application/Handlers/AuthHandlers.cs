using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Modules.Aplicativo.Application.Services;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Domain.ValueObjects;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
// Desambiguação: HistoricoLogin existe em Aplicativo e GestaoClientes; aqui persiste em ContextAplicativo.HistoricosLogin.
using HistoricoLogin = Epros.Modules.Aplicativo.Domain.Entities.HistoricoLogin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    public class AutenticarUsuarioCommandHandler : ICommandHandler<AutenticarUsuarioCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ContextGestaoClientes _contextGestao;
        private readonly ICurrentUser _currentUser;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEprosTokenService _tokenService;

        public AutenticarUsuarioCommandHandler(
            ContextAplicativo context,
            ContextGestaoClientes contextGestao,
            ICurrentUser currentUser,
            IPasswordHasher passwordHasher,
            IHttpContextAccessor httpContextAccessor,
            IEprosTokenService tokenService)
        {
            _context = context;
            _contextGestao = contextGestao;
            _currentUser = currentUser;
            _passwordHasher = passwordHasher;
            _httpContextAccessor = httpContextAccessor;
            _tokenService = tokenService;
        }

        public async Task<CommandResult> Handle(AutenticarUsuarioCommand request, CancellationToken cancellationToken)
        {
            var emailLower = request.Email.ToLowerInvariant().Trim();

            // 1. Verificar se o IP está banido
            var ipBanido = await _context.BannedIps
                .FirstOrDefaultAsync(b => b.IpAddress == request.IpAddress && b.DeletadoEm == null, cancellationToken);

            if (ipBanido != null && !ipBanido.EstaExpirado())
            {
                return CommandResult.Falha(new[] { "Acesso temporariamente bloqueado." });
            }

            // 2. Tentar localizar o usuário (cross-tenant: login anônimo não conhece o inquilino)
            await AuthRlsBypass.EnableAsync(_context, cancellationToken);
            var usuario = await _context.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == emailLower && u.DeletadoEm == null, cancellationToken);
            await AuthRlsBypass.DisableAsync(_context, cancellationToken);

            if (usuario != null)
            {
                DefinirTenantDaRequisicao(usuario.TenantId);
            }

            if (usuario == null)
            {
                // Grava falha anonimizada na auditoria
                var historicoFalha = new HistoricoLogin("system", null, emailLower, request.IpAddress, request.UserAgent, false, "Usuário não encontrado.", "system");
                _context.HistoricosLogin.Add(historicoFalha);
                await _context.SaveChangesAsync(cancellationToken);

                return CommandResult.Falha(new[] { "E-mail ou senha incorretos." });
            }

            // 3. Verificar bloqueio temporário (Lockout)
            if (usuario.LockoutEnd.HasValue && usuario.LockoutEnd.Value > DateTime.UtcNow)
            {
                return CommandResult.Falha(new[] { "Conta temporariamente bloqueada." });
            }

            // 4. Validar senha (hash PBKDF2 — comparação em tempo constante)
            if (!_passwordHasher.Verify(request.Senha, usuario.PasswordHash))
            {
                // Registra tentativa falha e lockout se atingir limite
                usuario.RegistrarFalhaLogin(5, TimeSpan.FromMinutes(15));

                var historicoFalha = new HistoricoLogin(usuario.TenantId, usuario.Id, emailLower, request.IpAddress, request.UserAgent, false, "Senha incorreta.", "system");
                _context.HistoricosLogin.Add(historicoFalha);

                await _context.SaveChangesAsync(cancellationToken);

                return CommandResult.Falha(new[] { "E-mail ou senha incorretos." });
            }

            // 4.1. REG-006 (P0): barrar contas não-ativas JÁ NO LOGIN, antes de emitir qualquer token.
            // A senha foi validada primeiro (custo constante), mas uma conta Pending/Disabled/Suspended
            // não pode receber token básico — antes ela era barrada só depois, em obter-acessos/session.
            // Mensagem GENÉRICA (anti-enumeração): não revela o estado da conta a quem acertou a senha.
            if (usuario.Status != UsuarioStatus.Active)
            {
                var historicoBloqueio = new HistoricoLogin(usuario.TenantId, usuario.Id, emailLower, request.IpAddress, request.UserAgent, false, $"Login negado: status da conta = {usuario.Status}.", "system");
                _context.HistoricosLogin.Add(historicoBloqueio);
                await _context.SaveChangesAsync(cancellationToken);

                return CommandResult.Falha(new[] { "E-mail ou senha incorretos." });
            }

            // 5. Sucesso. A partir daqui, a emissão de sessão + resolução multi-tenant é compartilhada
            //    com o login social (1.04 PASS 3) — ver AcessoMultiTenant.EmitirResultadoLoginAsync.
            usuario.ResetarFalhasLogin();

            return await AcessoMultiTenant.EmitirResultadoLoginAsync(
                _context, _contextGestao, _tokenService, _httpContextAccessor,
                usuario, request.IpAddress, request.UserAgent, emailLower,
                "Login realizado com sucesso.", cancellationToken);
        }

        private void DefinirTenantDaRequisicao(string tenantId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Items["TenantId"] = tenantId;
            }
        }
    }

    public class AutenticarUsuarioInternoCommandHandler : ICommandHandler<AutenticarUsuarioInternoCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEprosTokenService _tokenService;

        public AutenticarUsuarioInternoCommandHandler(
            ContextAplicativo context,
            IPasswordHasher passwordHasher,
            IEprosTokenService tokenService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<CommandResult> Handle(AutenticarUsuarioInternoCommand request, CancellationToken cancellationToken)
        {
            var emailLower = request.Email.ToLowerInvariant().Trim();

            // Login de operador interno é anônimo: não conhece o inquilino. O UsuarioInterno vive no
            // tenant fixo "system"; buscamos cross-tenant driblando o filtro de tenant do EF Core
            // (IgnoreQueryFilters) e a RLS do Postgres (AuthRlsBypass), como no login de tenant.
            // A comparação de e-mail é case-insensitive porque o seed não normaliza o e-mail.
            await AuthRlsBypass.EnableAsync(_context, cancellationToken);
            var usuario = await _context.UsuariosInternos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower && u.DeletadoEm == null, cancellationToken);
            await AuthRlsBypass.DisableAsync(_context, cancellationToken);

            // Mesma mensagem para usuário inexistente ou senha incorreta (evita enumeração de contas).
            if (usuario == null || !_passwordHasher.Verify(request.Senha, usuario.Senha))
            {
                return CommandResult.Falha(new[] { "E-mail ou senha incorretos." });
            }

            // Token marca o operador como interno: tenantId="system" (curto-circuito do AbacFilter),
            // empresaId="system" e perfilId="interno".
            var token = _tokenService.GerarCompleto("system", usuario.Id.ToString(), "system", "interno");

            return CommandResult.Ok("Autenticação de operador interno realizada com sucesso!", new
            {
                token,
                usuarioId = usuario.Id,
                nome = usuario.Nome,
                email = usuario.Email
            });
        }
    }

    public class SelecionarEmpresaCommandHandler : ICommandHandler<SelecionarEmpresaCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ContextGestaoClientes _contextGestao;
        private readonly IEprosTokenService _tokenService;

        public SelecionarEmpresaCommandHandler(ContextAplicativo context, ContextGestaoClientes contextGestao, IEprosTokenService tokenService)
        {
            _context = context;
            _contextGestao = contextGestao;
            _tokenService = tokenService;
        }

        public async Task<CommandResult> Handle(SelecionarEmpresaCommand request, CancellationToken cancellationToken)
        {
            var vinculo = await _context.UsuariosEmpresas
                .FirstOrDefaultAsync(ue => ue.UsuarioId == request.UsuarioId && ue.EmpresaId == request.EmpresaId && ue.DeletadoEm == null, cancellationToken);

            if (vinculo == null)
            {
                return CommandResult.Falha(new[] { "O usuário não possui permissão de acesso à empresa informada." });
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioId && u.DeletadoEm == null, cancellationToken);

            if (usuario == null)
            {
                return CommandResult.Falha(new[] { "Usuário inválido ou inativo." });
            }

            // Verificar se o tenant está inadimplente (Bloqueio SaaS)
            var cliente = await _contextGestao.Clientes
                .FirstOrDefaultAsync(c => c.TenantId == usuario.TenantId && c.DeletadoEm == null, cancellationToken);

            var block = false;
            if (cliente != null && cliente.Ativo)
            {
                var dataLimite = DateTime.UtcNow.AddDays(-15);
                block = await _contextGestao.Faturas
                    .AnyAsync(f => f.ClienteId == cliente.Id &&
                                   f.Status != FaturaStatus.Paga &&
                                   f.Status != FaturaStatus.Cancelada &&
                                   f.DataVencimento < dataLimite &&
                                   f.DeletadoEm == null, cancellationToken);
            }

            // Emite token completo com empresa e perfil
            var tokenCompleto = _tokenService.GerarCompleto(usuario.TenantId, usuario.Id.ToString(), vinculo.EmpresaId.ToString(), vinculo.PerfilAcessoId?.ToString() ?? "null");
            var expiracao = DateTime.UtcNow.Add(_tokenService.Validade); // Fonte única de expiração (8h — REG-024)

            var dto = new AuthResponseDto(
                Token: tokenCompleto,
                Expiracao: expiracao,
                UsuarioId: usuario.Id,
                Nome: usuario.Nome,
                Email: usuario.Email,
                ExigeSelecaoEmpresa: false,
                TenantId: usuario.TenantId,
                Block: block
            );

            return CommandResult.Ok("Empresa selecionada e contexto completo emitido com sucesso!", dto);
        }
    }

    /// <summary>
    /// Acesso multi-tenant (1.04 PASS 2): a identidade global escolhe UM tenant a que tem acesso
    /// (membership ativa) e recebe um token escopado a ele. Espelha selecionar-empresa, mas um nível acima:
    /// tenant → (depois) empresa. Se o tenant tiver uma única empresa vinculada, já emite o token completo.
    /// A validação de membership e as leituras de empresa/cliente são cross-tenant (bypass de RLS), pois o
    /// token corrente ainda está escopado à identidade/tenant de origem, não ao tenant-alvo.
    /// </summary>
    public class SelecionarTenantCommandHandler : ICommandHandler<SelecionarTenantCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ContextGestaoClientes _contextGestao;
        private readonly IEprosTokenService _tokenService;

        public SelecionarTenantCommandHandler(ContextAplicativo context, ContextGestaoClientes contextGestao, IEprosTokenService tokenService)
        {
            _context = context;
            _contextGestao = contextGestao;
            _tokenService = tokenService;
        }

        public async Task<CommandResult> Handle(SelecionarTenantCommand request, CancellationToken cancellationToken)
        {
            // 1. Valida a membership ativa (identidade global × tenant-alvo), cross-tenant.
            await AuthRlsBypass.EnableAsync(_context, cancellationToken);
            var acesso = await _context.AcessosUsuarioTenant
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.UsuarioId == request.UsuarioId
                                          && a.TenantId == request.TenantId
                                          && a.Ativo
                                          && a.DeletadoEm == null, cancellationToken);

            var usuario = await _context.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioId && u.DeletadoEm == null, cancellationToken);
            await AuthRlsBypass.DisableAsync(_context, cancellationToken);

            if (acesso == null)
            {
                return CommandResult.Falha(new[] { "O usuário não possui acesso ao tenant informado." });
            }

            if (usuario == null || usuario.Status != UsuarioStatus.Active)
            {
                return CommandResult.Falha(new[] { "Usuário inválido ou inativo." });
            }

            // 2. Empresas do usuário DENTRO do tenant-alvo e bloqueio SaaS (por-tenant).
            var (empresasDto, exigeSelecao, empresaUnica) =
                await AcessoMultiTenant.LerEmpresasAsync(_context, _contextGestao, usuario.Id, request.TenantId, cancellationToken);

            var block = await AcessoMultiTenant.TenantInadimplenteAsync(_contextGestao, request.TenantId, cancellationToken);

            // 3. Token escopado ao tenant. Empresa única → token completo direto; senão, básico + seleção de empresa.
            string token;
            if (empresaUnica != null)
            {
                token = _tokenService.GerarCompleto(request.TenantId, usuario.Id.ToString(), empresaUnica.EmpresaId.ToString(), empresaUnica.PerfilUsuarioId?.ToString() ?? "null");
            }
            else
            {
                token = _tokenService.GerarBasico(request.TenantId, usuario.Id.ToString());
            }

            var expiracao = DateTime.UtcNow.Add(_tokenService.Validade); // Fonte única de expiração (8h — REG-024)

            var dto = new AuthResponseDto(
                Token: token,
                Expiracao: expiracao,
                UsuarioId: usuario.Id,
                Nome: usuario.Nome,
                Email: usuario.Email,
                ExigeSelecaoEmpresa: exigeSelecao,
                TenantId: request.TenantId,
                Block: block,
                Empresas: empresasDto
            );

            return CommandResult.Ok("Tenant selecionado com sucesso!", dto);
        }
    }

    /// <summary>
    /// Rotinas compartilhadas do acesso multi-tenant (1.04 PASS 2) entre o login e a seleção de tenant:
    /// leitura de memberships e de empresas por tenant e checagem de inadimplência. Todas as leituras
    /// cross-tenant usam <see cref="AuthRlsBypass"/> + IgnoreQueryFilters, pois ocorrem antes do token
    /// estar escopado ao tenant-alvo.
    /// </summary>
    internal static class AcessoMultiTenant
    {
        /// <summary>
        /// Emite o resultado final de um login já autenticado (por senha OU social — 1.04 PASS 3):
        /// cria a <see cref="SessaoUsuario"/> (jti liga o JWT à sessão — logout/revogação REG-013),
        /// grava o <see cref="HistoricoLogin"/> e resolve o acesso multi-tenant (0/1/N tenants →
        /// entra direto ou exige seleção de tenant). Centraliza o que antes vivia só no login por senha,
        /// para o login social cair EXATAMENTE no mesmo fluxo (Pass 2). O <paramref name="usuario"/> deve
        /// estar sendo rastreado por <paramref name="context"/> (alterações pendentes são persistidas aqui).
        /// </summary>
        public static async Task<CommandResult> EmitirResultadoLoginAsync(
            ContextAplicativo context,
            ContextGestaoClientes contextGestao,
            IEprosTokenService tokenService,
            IHttpContextAccessor httpContextAccessor,
            Usuario usuario,
            string ipAddress,
            string userAgent,
            string emailLog,
            string mensagemHistorico,
            CancellationToken cancellationToken)
        {
            // Escopa a requisição ao tenant de origem para que a escrita de sessão/histórico satisfaça a RLS.
            DefinirTenantDaRequisicao(httpContextAccessor, usuario.TenantId);

            var jti = Guid.NewGuid();
            var tokenSessao = tokenService.GerarBasico(usuario.TenantId, usuario.Id.ToString(), jti.ToString());
            var expiracaoSessao = DateTime.UtcNow.Add(tokenService.Validade); // Fonte única de expiração (8h — REG-024)

            var sessao = new SessaoUsuario(
                usuario.TenantId, usuario.Id, tokenSessao, ipAddress, userAgent, expiracaoSessao, usuario.Id.ToString(), jti);
            var historicoSucesso = new HistoricoLogin(
                usuario.TenantId, usuario.Id, emailLog, ipAddress, userAgent, true, mensagemHistorico, usuario.Id.ToString());

            context.SessoesUsuarios.Add(sessao);
            context.HistoricosLogin.Add(historicoSucesso);
            await context.SaveChangesAsync(cancellationToken);

            // Acesso multi-tenant (1.04 PASS 2): a identidade global pode ter acesso a vários tenants
            // (ex.: contador parceiro). Lê as memberships ativas cross-tenant (login anônimo quanto ao
            // inquilino). Fallback: sem membership (base pré-1.04) → trata o tenant de origem como único.
            var acessos = await LerAcessosAtivosAsync(context, usuario.Id, cancellationToken);
            var tenantsAcessiveis = acessos.Select(a => a.TenantId).Distinct().ToList();
            if (tenantsAcessiveis.Count == 0)
            {
                tenantsAcessiveis.Add(usuario.TenantId);
            }

            // Mais de um tenant: não escopa direto — devolve a lista e exige a seleção de tenant.
            if (tenantsAcessiveis.Count > 1)
            {
                var tenantsDto = await MontarTenantsDtoAsync(contextGestao, acessos, cancellationToken);

                var dtoMulti = new AuthResponseDto(
                    Token: tokenSessao,
                    Expiracao: expiracaoSessao,
                    UsuarioId: usuario.Id,
                    Nome: usuario.Nome,
                    Email: usuario.Email,
                    ExigeSelecaoEmpresa: false,
                    TenantId: usuario.TenantId,
                    Block: false,
                    Empresas: null,
                    ExigeSelecaoTenant: true,
                    Tenants: tenantsDto);

                return CommandResult.Ok("Autenticação realizada. Selecione o tenant para continuar.", dtoMulti);
            }

            // Exatamente um tenant acessível: escopa direto (fluxo single-tenant preservado).
            var tenantEfetivo = tenantsAcessiveis[0];
            DefinirTenantDaRequisicao(httpContextAccessor, tenantEfetivo);

            var (empresasDto, exigeSelecao, empresaUnica) =
                await LerEmpresasAsync(context, contextGestao, usuario.Id, tenantEfetivo, cancellationToken);

            // Sem acesso a NENHUMA empresa (decisão Rafael, item 2): a identidade autenticou mas não tem
            // membership ativa NEM vínculo de empresa. Não é erro cru e não entra num tenant vazio: devolve
            // o estado SemAcesso para o front oferecer onboarding/contato. O token de sessão já foi emitido
            // (identidade autenticada), então "trocar de empresa depois" e o onboarding continuam possíveis.
            // Nota: usuário convidado tem vínculo de empresa (empresasDto.Count > 0), logo NÃO cai aqui —
            // preserva o fallback legado do tenant de origem para quem só tem UsuarioEmpresa, sem membership.
            if (acessos.Count == 0 && empresasDto.Count == 0)
            {
                var dtoSemAcesso = new AuthResponseDto(
                    Token: tokenSessao,
                    Expiracao: expiracaoSessao,
                    UsuarioId: usuario.Id,
                    Nome: usuario.Nome,
                    Email: usuario.Email,
                    ExigeSelecaoEmpresa: false,
                    TenantId: usuario.TenantId,
                    Block: false,
                    Empresas: null,
                    SemAcesso: true);

                return CommandResult.Ok("Autenticação realizada, mas você ainda não tem acesso a nenhuma empresa.", dtoSemAcesso);
            }

            var block = await TenantInadimplenteAsync(contextGestao, tenantEfetivo, cancellationToken);

            var tokenRetorno = tokenSessao;
            if (empresaUnica != null)
            {
                // Reusa o MESMO jti da sessão para que o token completo permaneça vinculado à SessaoUsuario.
                tokenRetorno = tokenService.GerarCompleto(tenantEfetivo, usuario.Id.ToString(), empresaUnica.EmpresaId.ToString(), empresaUnica.PerfilUsuarioId?.ToString() ?? "null", jti.ToString());
            }

            var dto = new AuthResponseDto(
                Token: tokenRetorno,
                Expiracao: expiracaoSessao,
                UsuarioId: usuario.Id,
                Nome: usuario.Nome,
                Email: usuario.Email,
                ExigeSelecaoEmpresa: exigeSelecao,
                TenantId: tenantEfetivo,
                Block: block,
                Empresas: empresasDto);

            return CommandResult.Ok("Autenticação realizada com sucesso!", dto);
        }

        private static void DefinirTenantDaRequisicao(IHttpContextAccessor httpContextAccessor, string tenantId)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Items["TenantId"] = tenantId;
            }
        }

        public static async Task<List<AcessoUsuarioTenant>> LerAcessosAtivosAsync(ContextAplicativo context, Guid usuarioId, CancellationToken cancellationToken)
        {
            await AuthRlsBypass.EnableAsync(context, cancellationToken);
            var acessos = await context.AcessosUsuarioTenant
                .IgnoreQueryFilters()
                .Where(a => a.UsuarioId == usuarioId && a.Ativo && a.DeletadoEm == null)
                .ToListAsync(cancellationToken);
            await AuthRlsBypass.DisableAsync(context, cancellationToken);
            return acessos;
        }

        /// <summary>Monta os DTOs de tenant (com nome do cliente) para a lista de seleção.</summary>
        public static async Task<List<TenantDisponivelDto>> MontarTenantsDtoAsync(ContextGestaoClientes contextGestao, List<AcessoUsuarioTenant> acessos, CancellationToken cancellationToken)
        {
            var tenantIds = acessos.Select(a => a.TenantId).Distinct().ToList();

            await AuthRlsBypass.EnableAsync(contextGestao, cancellationToken);
            var nomes = await contextGestao.Clientes
                .IgnoreQueryFilters()
                .Where(c => tenantIds.Contains(c.TenantId) && c.DeletadoEm == null)
                .ToDictionaryAsync(c => c.TenantId, c => c.RazaoSocial, cancellationToken);
            await AuthRlsBypass.DisableAsync(contextGestao, cancellationToken);

            return acessos
                .GroupBy(a => a.TenantId)
                .Select(g => new TenantDisponivelDto(
                    TenantId: g.Key,
                    Nome: nomes.TryGetValue(g.Key, out var nome) ? nome : g.Key,
                    Papel: g.First().Papel.ToString()
                ))
                .ToList();
        }

        /// <summary>
        /// Empresas vinculadas ao usuário dentro de um tenant. Retorna os DTOs, se exige seleção (mais de uma)
        /// e o vínculo único (quando há exatamente uma), para emissão direta de token completo.
        /// </summary>
        public static async Task<(List<UsuarioEmpresaDto> Empresas, bool ExigeSelecao, UsuarioEmpresaDto? EmpresaUnica)> LerEmpresasAsync(
            ContextAplicativo context, ContextGestaoClientes contextGestao, Guid usuarioId, string tenantId, CancellationToken cancellationToken)
        {
            await AuthRlsBypass.EnableAsync(context, cancellationToken);
            var vinculos = await context.UsuariosEmpresas
                .IgnoreQueryFilters()
                .Where(ue => ue.UsuarioId == usuarioId && ue.TenantId == tenantId && ue.DeletadoEm == null)
                .ToListAsync(cancellationToken);
            await AuthRlsBypass.DisableAsync(context, cancellationToken);

            var empresaIds = vinculos.Select(v => v.EmpresaId).ToList();

            await AuthRlsBypass.EnableAsync(contextGestao, cancellationToken);
            var empresas = await contextGestao.Empresas
                .IgnoreQueryFilters()
                .Where(e => e.TenantId == tenantId && empresaIds.Contains(e.Id) && e.DeletadoEm == null)
                .ToDictionaryAsync(e => e.Id, e => e.RazaoSocial, cancellationToken);
            await AuthRlsBypass.DisableAsync(contextGestao, cancellationToken);

            var dtos = vinculos.Select(v => new UsuarioEmpresaDto(
                EmpresaId: v.EmpresaId,
                RazaoSocial: empresas.TryGetValue(v.EmpresaId, out var nome) ? nome : "Empresa Desconhecida",
                EhAdmin: v.EhAdmin,
                PerfilUsuarioId: v.PerfilAcessoId
            )).ToList();

            var exigeSelecao = dtos.Count > 1;
            var unica = dtos.Count == 1 ? dtos[0] : null;
            return (dtos, exigeSelecao, unica);
        }

        /// <summary>Inadimplência SaaS do tenant (15 dias — decisão 1.01). Leitura cross-tenant.</summary>
        public static async Task<bool> TenantInadimplenteAsync(ContextGestaoClientes contextGestao, string tenantId, CancellationToken cancellationToken)
        {
            await AuthRlsBypass.EnableAsync(contextGestao, cancellationToken);
            var cliente = await contextGestao.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.DeletadoEm == null, cancellationToken);

            var block = false;
            if (cliente != null && cliente.Ativo)
            {
                var dataLimite = DateTime.UtcNow.AddDays(-15);
                block = await contextGestao.Faturas
                    .IgnoreQueryFilters()
                    .AnyAsync(f => f.ClienteId == cliente.Id &&
                                   f.Status != FaturaStatus.Paga &&
                                   f.Status != FaturaStatus.Cancelada &&
                                   f.DataVencimento < dataLimite &&
                                   f.DeletadoEm == null, cancellationToken);
            }
            await AuthRlsBypass.DisableAsync(contextGestao, cancellationToken);
            return block;
        }
    }

    /// <summary>
    /// Logout / revogação (REG-013). Revoga as <c>SessaoUsuario</c> ativas do usuário (registro
    /// persistente/auditoria) e grava um marco de logout em cache. A partir daí o handler de
    /// autenticação rejeita, na borda, qualquer token do usuário emitido antes desse instante —
    /// tornando o logout efetivo para toda a API (não só para os endpoints de sessão).
    /// </summary>
    public class EncerrarSessaoCommandHandler : ICommandHandler<EncerrarSessaoCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly IMemoryCache _cache;
        private readonly IEprosTokenService _tokenService;

        public EncerrarSessaoCommandHandler(ContextAplicativo context, IMemoryCache cache, IEprosTokenService tokenService)
        {
            _context = context;
            _cache = cache;
            _tokenService = tokenService;
        }

        public async Task<CommandResult> Handle(EncerrarSessaoCommand request, CancellationToken cancellationToken)
        {
            var sessoes = await _context.SessoesUsuarios
                .Where(s => s.UsuarioId == request.UsuarioId && !s.Revogado)
                .ToListAsync(cancellationToken);

            foreach (var sessao in sessoes)
            {
                sessao.Revogar(request.UsuarioId.ToString());
            }

            if (sessoes.Count > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Marco de logout: rejeita na borda qualquer token do usuário emitido antes de agora.
            // TTL = validade do token, para o marco cobrir a vida de qualquer token ainda válido.
            _cache.Set(RevogacaoSessao.ChaveCache(request.UsuarioId.ToString()), DateTime.UtcNow, _tokenService.Validade);

            return CommandResult.Ok("Sessão encerrada com sucesso.");
        }
    }

    public class SolicitarRecuperacaoSenhaCommandHandler : ICommandHandler<SolicitarRecuperacaoSenhaCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SolicitarRecuperacaoSenhaCommandHandler(
            ContextAplicativo context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CommandResult> Handle(SolicitarRecuperacaoSenhaCommand request, CancellationToken cancellationToken)
        {
            var emailLower = request.Email.ToLowerInvariant().Trim();

            await AuthRlsBypass.EnableAsync(_context, cancellationToken);
            var usuario = await _context.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == emailLower && u.DeletadoEm == null, cancellationToken);
            await AuthRlsBypass.DisableAsync(_context, cancellationToken);

            if (usuario != null && _httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Items["TenantId"] = usuario.TenantId;
            }

            if (usuario != null)
            {
                // Gera token temporário de redefinição com validade de 3 horas
                var token = Guid.NewGuid().ToString("N");
                usuario.GerarTokenRecuperacaoSenha(token, TimeSpan.FromHours(3));
                await _context.SaveChangesAsync(cancellationToken);

                // Em ambiente real, aqui despacharíamos o e-mail de redefinição.
                // Registramos o sucesso da solicitação.
            }

            // Para evitar enumeração de contas, retornamos Ok independente de encontrar o usuário
            return CommandResult.Ok("Se o e-mail informado existir na plataforma, você receberá as instruções de redefinição.");
        }
    }

    public class ResetarSenhaCommandHandler : ICommandHandler<ResetarSenhaCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResetarSenhaCommandHandler(
            ContextAplicativo context,
            IPasswordHasher passwordHasher,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CommandResult> Handle(ResetarSenhaCommand request, CancellationToken cancellationToken)
        {
            var emailLower = request.Email.ToLowerInvariant().Trim();

            await AuthRlsBypass.EnableAsync(_context, cancellationToken);
            var usuario = await _context.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == emailLower && u.DeletadoEm == null, cancellationToken);
            await AuthRlsBypass.DisableAsync(_context, cancellationToken);

            if (usuario != null && _httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Items["TenantId"] = usuario.TenantId;
            }

            if (usuario == null || usuario.ForgotPasswordToken != request.Token)
            {
                return CommandResult.Falha(new[] { "O e-mail informado ou o token de recuperação é inválido." });
            }

            if (usuario.ForgotPasswordTokenExpiry.HasValue && usuario.ForgotPasswordTokenExpiry.Value < DateTime.UtcNow)
            {
                return CommandResult.Falha(new[] { "O token de recuperação de senha expirou." });
            }

            // Altera a senha (hash PBKDF2) e limpa o token
            usuario.AlterarSenha(_passwordHasher.Hash(request.NovaSenha), "system-reset");
            
            if (!usuario.IsValid)
            {
                var erros = usuario.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Erro ao validar a nova senha.");
            }

            usuario.LimparTokenRecuperacaoSenha();
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Senha redefinida com sucesso!");
        }
    }

    public class AlterarSenhaUsuarioCommandHandler : ICommandHandler<AlterarSenhaUsuarioCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly IPasswordHasher _passwordHasher;

        public AlterarSenhaUsuarioCommandHandler(ContextAplicativo context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<CommandResult> Handle(AlterarSenhaUsuarioCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioId && u.DeletadoEm == null, cancellationToken);

            if (usuario == null)
            {
                return CommandResult.Falha(new[] { "Usuário não encontrado." });
            }

            if (!_passwordHasher.Verify(request.SenhaAtual, usuario.PasswordHash))
            {
                return CommandResult.Falha(new[] { "A senha atual informada é inválida." });
            }

            usuario.AlterarSenha(_passwordHasher.Hash(request.NovaSenha), request.UsuarioId.ToString());

            if (!usuario.IsValid)
            {
                var erros = usuario.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Erro ao validar a nova senha.");
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Senha alterada com sucesso!");
        }
    }

    public class RegistrarNovoTenantCommandHandler : ICommandHandler<RegistrarNovoTenantCommand>
    {
        private readonly ContextAplicativo _contextAplicativo;
        private readonly ContextGestaoClientes _contextGestaoClientes;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        // 1.07 (REG-045) — Serviço de notificação para o e-mail de boas-vindas/1º acesso. Injetado como
        // IEnumerable para não quebrar quem não o registra (ex.: providers de teste): resolve vazio e o
        // envio vira no-op. Em produção há um INotificacaoService registrado (Program.cs) → o hook liga.
        private readonly IEnumerable<INotificacaoService> _notificacaoServices;

        public RegistrarNovoTenantCommandHandler(
            ContextAplicativo contextAplicativo,
            ContextGestaoClientes contextGestaoClientes,
            IPasswordHasher passwordHasher,
            IHttpContextAccessor httpContextAccessor,
            IEnumerable<INotificacaoService> notificacaoServices)
        {
            _contextAplicativo = contextAplicativo;
            _contextGestaoClientes = contextGestaoClientes;
            _passwordHasher = passwordHasher;
            _httpContextAccessor = httpContextAccessor;
            _notificacaoServices = notificacaoServices;
        }

        public async Task<CommandResult> Handle(RegistrarNovoTenantCommand request, CancellationToken cancellationToken)
        {
            var emailLower = request.EmailAdmin.ToLowerInvariant().Trim();

            // 0. REG-036 (decisão de negócio — fonte: overlay fiscal): documento fiscal ÍNTEGRO.
            //    Quem emite documento fiscal precisa de cadastro válido. Validamos o documento por
            //    dígito verificador (CNPJ ou CPF) — nada de placeholder — ANTES de qualquer escrita.
            var cnpjInformado = request.Cnpj?.Trim() ?? string.Empty;
            var cpfInformado = request.Cpf?.Trim() ?? string.Empty;
            string? cpfLimpo = null;

            // 1.07 (REG-004 / EF §7.1,11.1) — Emitente PJ (CNPJ) OU PF (somente CPF: autônomo/produtor
            // rural/MEI). Ao menos um documento fiscal é obrigatório. Nenhum dos dois → rejeita.
            var possuiCnpj = !string.IsNullOrWhiteSpace(cnpjInformado);
            var possuiCpf = !string.IsNullOrWhiteSpace(cpfInformado);

            if (!possuiCnpj && !possuiCpf)
            {
                return CommandResult.Falha(new[] { "Informe um documento fiscal: CNPJ (pessoa jurídica) ou CPF (pessoa física)." });
            }

            if (possuiCpf)
            {
                var cpfDigitos = SomenteDigitos(cpfInformado);
                if (!ValidarCpf(cpfDigitos))
                {
                    return CommandResult.Falha(new[] { "CPF inválido." });
                }
                cpfLimpo = cpfDigitos;
            }

            // CNPJ (quando informado) validado por dígito verificador; PF mantém o CNPJ vazio (nulo
            // deliberado — nunca placeholder). REG-036: integridade fiscal preservada.
            var cnpjFormatado = string.Empty;
            if (possuiCnpj)
            {
                var cnpjVo = new Cnpj(cnpjInformado);
                if (!cnpjVo.IsValid)
                {
                    return CommandResult.Falha(new[] { "CNPJ inválido." });
                }
                cnpjFormatado = cnpjVo.Valor;
            }

            // 1. Validar duplicidade de e-mail na base de usuários (cross-tenant)
            await AuthRlsBypass.EnableAsync(_contextAplicativo, cancellationToken);
            await AuthRlsBypass.EnableAsync(_contextGestaoClientes, cancellationToken);

            var emailExiste = await _contextAplicativo.Usuarios
                .IgnoreQueryFilters()
                .AnyAsync(u => u.Email == emailLower && u.DeletadoEm == null, cancellationToken);

            if (emailExiste)
            {
                await AuthRlsBypass.DisableAsync(_contextGestaoClientes, cancellationToken);
                await AuthRlsBypass.DisableAsync(_contextAplicativo, cancellationToken);
                return CommandResult.Falha(new[] { "Já existe um usuário cadastrado com este e-mail." });
            }

            // 2. Validar duplicidade do documento fiscal (dígitos limpos — a Empresa armazena limpo).
            //    PJ → CNPJ único (REG-003); PF → CPF único (REG-004).
            var cnpjExiste = false;
            var cpfExiste = false;
            if (possuiCnpj)
            {
                cnpjExiste = await _contextGestaoClientes.Empresas
                    .IgnoreQueryFilters()
                    .AnyAsync(e => e.Cnpj == cnpjFormatado && e.DeletadoEm == null, cancellationToken);
            }
            else if (possuiCpf)
            {
                cpfExiste = await _contextGestaoClientes.Empresas
                    .IgnoreQueryFilters()
                    .AnyAsync(e => e.Cpf == cpfLimpo && e.DeletadoEm == null, cancellationToken);
            }

            await AuthRlsBypass.DisableAsync(_contextGestaoClientes, cancellationToken);
            await AuthRlsBypass.DisableAsync(_contextAplicativo, cancellationToken);

            if (cnpjExiste)
            {
                return CommandResult.Falha(new[] { "Já existe uma empresa cadastrada com este CNPJ." });
            }
            if (cpfExiste)
            {
                return CommandResult.Falha(new[] { "Já existe um cadastro com este CPF." });
            }

            // 2.5. REG-036: município IBGE precisa existir no cadastro de geografia (catálogo global).
            //      Sem município válido não há como compor cUF/cMun de documento fiscal — rejeita.
            var municipio = await _contextGestaoClientes.Municipios
                .IgnoreQueryFilters()
                .Include(m => m.Subdivisao)
                .FirstOrDefaultAsync(m => m.CodigoIbge == request.CodigoIbgeMunicipio && m.DeletadoEm == null, cancellationToken);

            if (municipio == null)
            {
                return CommandResult.Falha(new[] { "Município (código IBGE) inválido ou não cadastrado." });
            }

            var cidadeMunicipio = municipio.Nome;
            var ufMunicipio = municipio.Uf ?? municipio.Subdivisao?.CodigoISO31662?.Replace("BR-", "") ?? "";

            // 3. Transação compartilhada — fechar conexões abertas nas validações acima
            var isRelational = _contextAplicativo.Database.IsRelational() && _contextGestaoClientes.Database.IsRelational();
            IDbContextTransaction? transaction = null;
            if (isRelational)
            {
                await _contextAplicativo.Database.CloseConnectionAsync();
                await _contextGestaoClientes.Database.CloseConnectionAsync();

                // Uma transação do PostgreSQL/Npgsql pertence à conexão que a abriu; ela só pode ser
                // reaproveitada por outro DbContext se ambos compartilharem a MESMA conexão física.
                // Como os dois contextos apontam para o mesmo banco (DefaultConnection), passamos a
                // conexão do ContextAplicativo para o ContextGestaoClientes antes de iniciar a
                // transação compartilhada. Sem isto, UseTransactionAsync lança
                // "The specified transaction is not associated with the current connection".
                var conexaoCompartilhada = _contextAplicativo.Database.GetDbConnection();
                _contextGestaoClientes.Database.SetDbConnection(conexaoCompartilhada, contextOwnsConnection: false);

                transaction = await _contextAplicativo.Database.BeginTransactionAsync(cancellationToken);
                await _contextGestaoClientes.Database.UseTransactionAsync(transaction.GetDbTransaction(), cancellationToken);
            }

            try
            {
                var tenantId = $"tenant-{Guid.NewGuid().ToString("N").Substring(0, 12)}";
                var criadoPor = "self-register";

                if (_httpContextAccessor.HttpContext != null)
                {
                    _httpContextAccessor.HttpContext.Items["TenantId"] = tenantId;
                }

                // 1.07 — Endereço: quando o cadastro informa o logradouro, persiste o endereço REAL; caso
                // contrário mantém um mínimo VÁLIDO com cidade/UF reais do IBGE. Obs.: "nulos honestos"
                // para logradouro/CEP não são viáveis sem relaxar o VO Endereco (invariante compartilhada
                // exige campos não-vazios) — registrado na MC como ponto adiado. Cidade/UF são íntegros.
                var temEnderecoReal = !string.IsNullOrWhiteSpace(request.Logradouro);
                var numeroEndereco = string.IsNullOrWhiteSpace(request.Numero) ? "S/N" : request.Numero!.Trim();
                var endereco = new Epros.Modules.GestaoClientes.Domain.ValueObjects.Endereco(
                    logradouro: temEnderecoReal ? request.Logradouro!.Trim() : "Não informado",
                    numero: numeroEndereco,
                    complemento: string.IsNullOrWhiteSpace(request.Complemento) ? null : request.Complemento!.Trim(),
                    bairro: string.IsNullOrWhiteSpace(request.Bairro) ? "Não informado" : request.Bairro!.Trim(),
                    cep: string.IsNullOrWhiteSpace(request.Cep) ? "00000000" : SomenteDigitos(request.Cep!),
                    cidade: cidadeMunicipio,
                    estado: ufMunicipio);

                var enderecoTexto = temEnderecoReal
                    ? $"{request.Logradouro!.Trim()}, {numeroEndereco} - {cidadeMunicipio}/{ufMunicipio}"
                    : $"{cidadeMunicipio}/{ufMunicipio}";

                // 3.5. Criar PessoaGrupo para o tenant
                var pessoaGrupo = new PessoaGrupo(
                    descricao: $"Grupo Geral - {request.NomeEmpresa}",
                    tenantId: tenantId,
                    criadoPor: criadoPor
                );
                _contextGestaoClientes.PessoaGrupos.Add(pessoaGrupo);
                await _contextGestaoClientes.SaveChangesAsync(cancellationToken);

                // 4. Criar Empresa no schema plataforma (ContextGestaoClientes)
                var empresa = new Empresa(
                    razaoSocial: request.NomeEmpresa,
                    nomeFantasia: request.NomeEmpresa,
                    cnpj: cnpjFormatado,
                    inscricaoEstadual: null,
                    inscricaoMunicipal: null,
                    inscricaoSuframa: null,
                    cnae: null,
                    regimeTributario: RegimeTributario.SimplesNacional,
                    regimeApuracao: RegimeApuracao.Cumulativo,
                    pessoaGrupoId: pessoaGrupo.Id,
                    // REG-036: NÃO fabricar IDs fiscais/agrupadores com Guid.NewGuid() (apontavam para
                    // entidades inexistentes). Ficam nulos; os grupos reais são semeados no onboarding.
                    produtoGrupoId: null,
                    planoContasFinanceiroId: null,
                    tributarioGrupoId: null,
                    ncmTributacaoId: null,
                    certificadoDigitalId: null,
                    empresaParametrosDfeId: null,
                    linkWebApiAppVendas: null,
                    tokenMercadoPagoPix: null,
                    logo: null,
                    endereco: endereco,
                    tenantId: tenantId,
                    criadoPor: criadoPor,
                    cpf: cpfLimpo
                );

                if (!empresa.IsValid)
                {
                    var erros = empresa.Notifications.Select(n => n.Message);
                    return CommandResult.Falha(erros, "Erro ao validar os dados cadastrais da empresa.");
                }

                _contextGestaoClientes.Empresas.Add(empresa);
                await _contextGestaoClientes.SaveChangesAsync(cancellationToken);

                // 4.5. TRIAL AUTOMÁTICO (1.07 — destrava §3.2): na MESMA transação atômica, cria o Cliente
                //      SaaS em StatusSaaS.TrialGratuito + assinatura (data-fim = agora + trial_days) num
                //      plano trial. Sem isto o tenant nasceria bloqueado (ObterContextoSessao: cliente ==
                //      null → block). Reusa OnboardingSaaSProvisioner (mesma lógica do handler cliente-saas).
                var documentoFiscal = possuiCnpj ? cnpjFormatado : (cpfLimpo ?? string.Empty);

                var planoTrial = await OnboardingSaaSProvisioner.ObterOuCriarPlanoTrialAsync(
                    _contextGestaoClientes, tenantId, criadoPor, cancellationToken);

                var trialDays = await OnboardingSaaSProvisioner.ObterTrialDaysAsync(_contextGestaoClientes, cancellationToken);
                var trialAte = DateTime.UtcNow.AddDays(trialDays);

                var clienteSaaS = OnboardingSaaSProvisioner.CriarCliente(
                    tenantId: tenantId,
                    razaoSocial: request.NomeEmpresa,
                    documento: documentoFiscal,
                    email: emailLower,
                    planoId: planoTrial.Id,
                    statusSaaS: StatusSaaS.TrialGratuito,
                    diaVencimento: 10,
                    criadoPor: criadoPor,
                    telefone: request.Telefone,
                    nomeContato: request.NomeAdmin,
                    isDemo: false);

                if (!clienteSaaS.IsValid)
                {
                    var erros = clienteSaaS.Notifications.Select(n => n.Message);
                    return CommandResult.Falha(erros, "Erro ao criar a assinatura trial do tenant.");
                }
                _contextGestaoClientes.Clientes.Add(clienteSaaS);
                await _contextGestaoClientes.SaveChangesAsync(cancellationToken);

                var assinaturaTrial = OnboardingSaaSProvisioner.CriarAssinaturaTrial(
                    tenantId, clienteSaaS.Id, planoTrial.Id, trialAte, criadoPor);
                if (!assinaturaTrial.IsValid)
                {
                    var erros = assinaturaTrial.Notifications.Select(n => n.Message);
                    return CommandResult.Falha(erros, "Erro ao criar a assinatura trial do tenant.");
                }
                _contextGestaoClientes.AssinaturasClientes.Add(assinaturaTrial);
                await _contextGestaoClientes.SaveChangesAsync(cancellationToken);

                // 5. Criar Usuário Admin no schema aplicativo (ContextAplicativo)
                var usuarioAdmin = new Usuario(
                    tenantId: tenantId,
                    nome: request.NomeAdmin,
                    email: emailLower,
                    passwordHash: _passwordHasher.Hash(request.SenhaAdmin), // Senha derivada via PBKDF2 — nunca em texto puro
                    tipo: UsuarioTipo.Company,
                    criadoPor: criadoPor
                );

                if (!usuarioAdmin.IsValid)
                {
                    var erros = usuarioAdmin.Notifications.Select(n => n.Message);
                    return CommandResult.Falha(erros, "Erro ao validar os dados cadastrais do usuário administrador.");
                }

                _contextAplicativo.Usuarios.Add(usuarioAdmin);
                await _contextAplicativo.SaveChangesAsync(cancellationToken);

                // 6. Criar vínculo UsuarioEmpresa
                var vinculo = new UsuarioEmpresa(
                    tenantId: tenantId,
                    usuarioId: usuarioAdmin.Id,
                    empresaId: empresa.Id,
                    perfilUsuarioId: null, // Admin primário dispensa perfil inicial
                    ehAdmin: true,
                    criadoPor: criadoPor
                );

                _contextAplicativo.UsuariosEmpresas.Add(vinculo);

                // 6.1. Membership N:N (1.04 PASS 2): vincula a identidade global ao tenant recém-criado como
                //      Proprietário. É a base do acesso multi-tenant — o login lista tenants a partir daqui.
                var acessoTenant = new AcessoUsuarioTenant(
                    tenantId: tenantId,
                    usuarioId: usuarioAdmin.Id,
                    papel: PapelAcessoTenant.Proprietario,
                    criadoPor: criadoPor
                );
                _contextAplicativo.AcessosUsuarioTenant.Add(acessoTenant);

                await _contextAplicativo.SaveChangesAsync(cancellationToken);

                // 6.5. Criar ConfiguracaoEmpresa padrão
                var configEmpresa = new ConfiguracaoEmpresa(
                    tenantId: tenantId,
                    empresaId: empresa.Id,
                    nome: request.NomeEmpresa,
                    email: emailLower,
                    telefone: request.Telefone,
                    endereco: enderecoTexto,
                    timeZoneId: 1,
                    dateFormat: "DD-MM-YYYY",
                    currencyId: 1,
                    vatPercentage: 0,
                    vatType: 1,
                    currencyPosition: 1,
                    footerText: null,
                    logo: null,
                    favicon: null,
                    criadoPor: criadoPor,
                    // 1.07 — tipo de telefone agora persistido (antes validado e descartado).
                    telefoneTipo: request.TipoTelefone
                );
                _contextAplicativo.ConfiguracoesEmpresas.Add(configEmpresa);

                // 6.6. Criar primeiro AnoFinanceiro ativo
                var anoFinanceiro = new AnoFinanceiro(
                    tenantId: tenantId,
                    fromDate: DateTime.UtcNow,
                    toDate: DateTime.UtcNow.AddDays(365),
                    isActive: true,
                    criadoPor: criadoPor
                );
                _contextAplicativo.AnosFinanceiros.Add(anoFinanceiro);

                // 6.7. Criar Idiomas padrão (en, pt-BR)
                var langEn = new Idioma(tenantId, "en", "English", "US", true, criadoPor);
                var langPt = new Idioma(tenantId, "pt-BR", "Português", "BR", true, criadoPor);
                _contextAplicativo.Idiomas.Add(langEn);
                _contextAplicativo.Idiomas.Add(langPt);

                await _contextAplicativo.SaveChangesAsync(cancellationToken);

                // Commit transação
                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                // 7. REG-045 — E-mail de boas-vindas / 1º acesso. DEPOIS do commit e SEM bloquear o
                //    cadastro: falha de e-mail nunca desfaz o tenant já criado (try/catch + no-op se não
                //    houver provedor configurado). Ver EnviarEmailBoasVindasAsync.
                await EnviarEmailBoasVindasAsync(emailLower, request.NomeAdmin, request.NomeEmpresa);

                return CommandResult.Ok("Tenant e administrador registrados com sucesso!", new Dictionary<string, object>
                {
                    { "TenantId", tenantId },
                    { "UsuarioAdminId", usuarioAdmin.Id },
                    { "EmpresaId", empresa.Id }
                });
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                return CommandResult.Falha(new[] { $"Erro interno ao realizar registro de tenant: {ex.Message}" });
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        private static string SomenteDigitos(string valor)
            => string.IsNullOrEmpty(valor) ? string.Empty : new string(valor.Where(char.IsDigit).ToArray());

        /// <summary>
        /// REG-045 — Dispara o e-mail de boas-vindas / primeiro acesso ao fim do onboarding. Resiliente:
        /// se nenhum <see cref="INotificacaoService"/> estiver registrado (ex.: ambiente sem provedor),
        /// vira no-op; qualquer exceção do provedor é engolida (log) para NÃO bloquear o cadastro — o
        /// tenant já foi criado e comitado. A pendência de um provedor SMTP real é registrada na MC.
        /// </summary>
        private async Task EnviarEmailBoasVindasAsync(string emailDestino, string nomeAdmin, string nomeEmpresa)
        {
            var notificador = _notificacaoServices?.FirstOrDefault();
            if (notificador == null)
            {
                return; // hook pronto, sem provedor configurado → no-op
            }

            try
            {
                var assunto = "Bem-vindo(a) ao EprosERP — seu ambiente está pronto";
                var corpo =
                    $"<p>Olá, {nomeAdmin}!</p>" +
                    $"<p>O ambiente da empresa <strong>{nomeEmpresa}</strong> foi criado com sucesso e já está " +
                    $"disponível em período de avaliação (trial).</p>" +
                    $"<p>Acesse com o e-mail <strong>{emailDestino}</strong> e a senha que você definiu no cadastro " +
                    $"para começar a configurar sua operação.</p>" +
                    $"<p>Equipe EprosERP.</p>";
                await notificador.EnviarEmailAsync(emailDestino, assunto, corpo);
            }
            catch
            {
                // Envio de e-mail é best-effort: falha aqui não desfaz o cadastro já concluído.
            }
        }

        /// <summary>Valida CPF pelos dois dígitos verificadores (rejeita sequências repetidas).</summary>
        private static bool ValidarCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf) || cpf.Length != 11) return false;
            if (cpf.Distinct().Count() == 1) return false;

            var numeros = cpf.Select(c => c - '0').ToArray();

            int soma = 0;
            for (int i = 0; i < 9; i++) soma += numeros[i] * (10 - i);
            int resto = soma % 11;
            int dig1 = resto < 2 ? 0 : 11 - resto;
            if (numeros[9] != dig1) return false;

            soma = 0;
            for (int i = 0; i < 10; i++) soma += numeros[i] * (11 - i);
            resto = soma % 11;
            int dig2 = resto < 2 ? 0 : 11 - resto;
            return numeros[10] == dig2;
        }
    }
}
