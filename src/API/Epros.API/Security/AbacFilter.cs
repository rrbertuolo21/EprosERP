using System;
using System.Linq;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Epros.API.Security
{
    public class AbacFilter : IAsyncAuthorizationFilter
    {
        private readonly string _recurso;
        private readonly string _acao;
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;
        private readonly ITenantProvider _tenantProvider;

        public AbacFilter(string recurso, string acao, ContextGestaoClientes context, ICurrentUser currentUser, ITenantProvider tenantProvider)
        {
            _recurso = recurso;
            _acao = acao;
            _context = context;
            _currentUser = currentUser;
            _tenantProvider = tenantProvider;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Curto-circuito do operador interno da Siser (UsuarioInterno): seu token é emitido pelo
            // fluxo de autenticação interno com tenantId="system" E perfilId="interno" (ver
            // AutenticarUsuarioInternoCommandHandler). Esse operador não tem PerfilColaborador em
            // ContextGestaoClientes, então a verificação ABAC abaixo o barraria — por isso liberamos
            // antes de consultar PerfisUsuarios.
            //
            // SEGURANÇA (fechamento do "gato"): NÃO basta tenantId="system". Como o antigo atalho de
            // header foi removido do runtime, tenantId="system" só existe num token ASSINADO; ainda
            // assim exigimos o marcador de operador (claim perfilId="interno") emitido exclusivamente
            // pela autenticação de UsuarioInterno. Isso fecha o combo "forjo/injeto tenant=system ->
            // acesso total" e faz o AbacFilter cobrar ACL real de qualquer identidade "system" que não
            // seja comprovadamente um operador interno.
            var ehTenantSystem = string.Equals(
                _tenantProvider.GetTenantId(), "system", StringComparison.OrdinalIgnoreCase);
            var ehOperadorInterno = ehTenantSystem && string.Equals(
                context.HttpContext.User.FindFirst("perfilId")?.Value,
                "interno",
                StringComparison.OrdinalIgnoreCase);

            if (ehOperadorInterno)
            {
                return;
            }

            var userId = _currentUser.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                Log.Warning("Tentativa de acesso não autenticada no recurso {Recurso}:{Acao}", _recurso, _acao);
                context.Result = new UnauthorizedResult();
                return;
            }

            // Busca o perfil do usuário no tenant atual (o tenant filter é aplicado automaticamente pelo EF Core)
            var perfil = await _context.PerfisUsuarios
                .Include(p => p.Permissoes)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (perfil == null || !perfil.Ativo)
            {
                Log.Warning("Acesso negado: Perfil não encontrado ou inativo para o usuário {UserId} no recurso {Recurso}:{Acao}", userId, _recurso, _acao);
                context.Result = new ForbidResult();
                return;
            }

            // Regra ABAC 1: Administrador ignora validações de ACL básicas
            if (perfil.Cargo.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Regra ABAC 2: Validação dinâmica de limites de desconto
            if (_recurso.Equals("Desconto", StringComparison.OrdinalIgnoreCase) && _acao.Equals("Aplicar", StringComparison.OrdinalIgnoreCase))
            {
                if (context.HttpContext.Request.Query.TryGetValue("percentual", out var percentualStr) &&
                    decimal.TryParse(percentualStr, out var percentual))
                {
                    if (percentual > perfil.LimiteDesconto)
                    {
                        Log.Warning("Usuário {UserId} tentou aplicar desconto de {Percentual}%, mas seu limite é {Limite}%", userId, percentual, perfil.LimiteDesconto);
                        context.Result = new JsonResult(new { Sucesso = false, Mensagem = $"Acesso negado: O limite de desconto permitido para o seu perfil é {perfil.LimiteDesconto}%." })
                        {
                            StatusCode = 403
                        };
                        return;
                    }
                }
            }

            // Regra ABAC 3: Verificação de ACL fina na tabela de permissões
            var permissao = perfil.Permissoes
                .FirstOrDefault(p => p.Recurso.Equals(_recurso, StringComparison.OrdinalIgnoreCase) &&
                                     p.Acao.Equals(_acao, StringComparison.OrdinalIgnoreCase));

            if (permissao == null || !permissao.Permitido)
            {
                Log.Warning("Acesso negado: Usuário {UserId} não possui permissão explícita para {Recurso}:{Acao}", userId, _recurso, _acao);
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
