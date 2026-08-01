using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Services;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Epros.API.Security
{
    public class PermissaoMenuFilter : IAsyncAuthorizationFilter
    {
        private readonly string _menuIdStr;
        private readonly string? _nivel1IdStr;
        private readonly string? _nivel2IdStr;
        private readonly string _acao;

        private readonly ContextAplicativo _contextApp;
        private readonly ContextGestaoClientes _contextGestao;
        private readonly IPermissaoCacheManager _cacheManager;
        private readonly IEprosTokenService _tokenService;

        public PermissaoMenuFilter(
            string menuId,
            string? nivel1Id,
            string? nivel2Id,
            string acao,
            ContextAplicativo contextApp,
            ContextGestaoClientes contextGestao,
            IPermissaoCacheManager cacheManager,
            IEprosTokenService tokenService)
        {
            _menuIdStr = menuId;
            _nivel1IdStr = nivel1Id;
            _nivel2IdStr = nivel2Id;
            _acao = acao;
            _contextApp = contextApp;
            _contextGestao = contextGestao;
            _cacheManager = cacheManager;
            _tokenService = tokenService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;

            // 1. Extrair credenciais (Token ou Headers)
            string? tenantId = null;
            Guid? usuarioId = null;
            Guid? empresaId = null;

            var authHeader = httpContext.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader))
            {
                var token = authHeader.Replace("Bearer ", "").Trim();
                // Valida o JWT assinado (assinatura + expiração + emissor/audiência) e lê os claims.
                var principal = _tokenService.Validar(token);
                if (principal != null)
                {
                    tenantId = principal.FindFirst("tenantId")?.Value;
                    var uidClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (Guid.TryParse(uidClaim, out var parsedUid)) usuarioId = parsedUid;
                    var eidClaim = principal.FindFirst("empresaId")?.Value;
                    if (Guid.TryParse(eidClaim, out var parsedEid)) empresaId = parsedEid;
                }
            }

            // Fallback para os Headers explícitos (facilidade de desenvolvimento e testes de integração)
            if (string.IsNullOrEmpty(tenantId) && httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var hTenant))
            {
                tenantId = hTenant.ToString();
            }
            if (usuarioId == null && httpContext.Request.Headers.TryGetValue("X-User-Id", out var hUser) && Guid.TryParse(hUser.ToString(), out var uid))
            {
                usuarioId = uid;
            }
            if (empresaId == null && httpContext.Request.Headers.TryGetValue("X-Empresa-Id", out var hEmpresa) && Guid.TryParse(hEmpresa.ToString(), out var eid))
            {
                empresaId = eid;
            }

            // Se ainda não temos os dados essenciais, barra a requisição como não autenticada
            if (string.IsNullOrEmpty(tenantId) || usuarioId == null || empresaId == null)
            {
                Log.Warning("Acesso não autenticado: dados insuficientes. Tenant: {Tenant}, User: {User}, Empresa: {Empresa}", tenantId, usuarioId, empresaId);
                context.Result = new UnauthorizedResult();
                return;
            }

            // 2. Verificar se o usuário é administrador na empresa (Bypass total)
            var vinculo = await _contextApp.UsuariosEmpresas
                .FirstOrDefaultAsync(ue => ue.UsuarioId == usuarioId.Value && ue.EmpresaId == empresaId.Value && ue.DeletadoEm == null);

            if (vinculo == null)
            {
                Log.Warning("Acesso negado: Usuário {UserId} não possui vínculo ativo com a empresa {EmpresaId}", usuarioId, empresaId);
                context.Result = new ForbidResult();
                return;
            }

            if (vinculo.EhAdmin)
            {
                // Administrador possui bypass total
                return;
            }

            // 3. Usuário comum: carregar as permissões efetivas do cache
            if (!Guid.TryParse(_menuIdStr, out var menuId))
            {
                Log.Error("MenuId inválido no filtro de permissão: {MenuId}", _menuIdStr);
                context.Result = new ForbidResult();
                return;
            }

            Guid? nivel1Id = Guid.TryParse(_nivel1IdStr, out var n1) ? n1 : null;
            Guid? nivel2Id = Guid.TryParse(_nivel2IdStr, out var n2) ? n2 : null;

            if (!vinculo.PerfilAcessoId.HasValue)
            {
                Log.Warning("Acesso negado: Usuário comum {UserId} sem perfil associado na empresa {EmpresaId}", usuarioId, empresaId);
                context.Result = new ForbidResult();
                return;
            }

            var permissoes = await _cacheManager.ObterPermissoesEfetivasAsync(
                tenantId,
                usuarioId.Value,
                empresaId.Value,
                async () =>
                {
                    var perfil = await _contextGestao.PerfisAcessos
                        .Include(p => p.Acessos)
                        .FirstOrDefaultAsync(p => p.Id == vinculo.PerfilAcessoId.Value && p.TenantId == tenantId && p.Ativo && p.DeletadoEm == null);

                    if (perfil == null)
                        return new List<PerfilAcessoMenu>();

                    return perfil.Acessos.Where(a => a.DeletadoEm == null).ToList();
                }
            );

            // Localiza a regra correspondente
            var regra = permissoes.FirstOrDefault(p => p.MenuId == menuId &&
                                                      p.MenuItemNivel1Id == nivel1Id &&
                                                      p.MenuItemNivel2Id == nivel2Id);

            if (regra == null)
            {
                Log.Warning("Acesso negado: Usuário {UserId} sem permissão configurada para Menu: {MenuId}, Nivel1: {Nivel1}, Nivel2: {Nivel2}", usuarioId, menuId, nivel1Id, nivel2Id);
                context.Result = CriarAcessoProibidoResult();
                return;
            }

            // Valida as permissões baseadas nas flags booleanas Ver, Editar, Excluir
            var permitido = false;
            if (_acao.Equals("Ver", StringComparison.OrdinalIgnoreCase) || _acao.Equals("Ler", StringComparison.OrdinalIgnoreCase))
            {
                permitido = regra.Ver;
            }
            else if (_acao.Equals("Editar", StringComparison.OrdinalIgnoreCase) || _acao.Equals("Criar", StringComparison.OrdinalIgnoreCase) || _acao.Equals("Atualizar", StringComparison.OrdinalIgnoreCase))
            {
                permitido = regra.Editar;
            }
            else if (_acao.Equals("Excluir", StringComparison.OrdinalIgnoreCase) || _acao.Equals("Deletar", StringComparison.OrdinalIgnoreCase))
            {
                permitido = regra.Excluir;
            }

            if (!permitido)
            {
                Log.Warning("Acesso negado: Usuário {UserId} não tem permissão para a ação '{Acao}' em Menu: {MenuId}", usuarioId, _acao, menuId);
                context.Result = CriarAcessoProibidoResult();
                return;
            }
        }

        private static IActionResult CriarAcessoProibidoResult()
        {
            return new JsonResult(new { error = "Acesso proibido." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
