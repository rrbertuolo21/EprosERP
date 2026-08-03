using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Epros.API.Seed;
using Epros.Modules.GestaoClientes.Application.Services;
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
                // 1.11 decisão #5 (menor privilégio): o operador interno NÃO é mais "bypass total".
                // Ele é autorizado pela FAIXA de capacidades do seu perfil de suporte (PrimaryAdmin =
                // todas). A faixa exigida pelo recurso e o perfil do operador vêm do token (claims
                // perfilSuporte/primaryAdmin, emitidos por AutenticarUsuarioInternoCommandHandler).
                var faixaExigida = SuperAdminSeguranca.FaixaDe(_recurso);
                var primaryAdmin = string.Equals(
                    context.HttpContext.User.FindFirst(SuperAdminSeguranca.ClaimPrimaryAdmin)?.Value,
                    "true", StringComparison.OrdinalIgnoreCase);
                var perfilSuporte = context.HttpContext.User.FindFirst(SuperAdminSeguranca.ClaimPerfilSuporte)?.Value;

                if (SuperAdminSeguranca.OperadorInternoAutorizado(primaryAdmin, perfilSuporte, faixaExigida))
                {
                    return;
                }

                Log.Warning(
                    "Acesso negado (faixa de suporte insuficiente — 1.11) do operador interno em {Recurso}:{Acao} (perfilSuporte={Perfil}, primaryAdmin={Primary}, faixaExigida={Faixa})",
                    _recurso, _acao, perfilSuporte ?? "(nenhum)", primaryAdmin, faixaExigida);
                context.Result = new ForbidResult();
                return;
            }

            // 1.11 fix #1 (fecha o escalonamento): recursos SuperAdmin/Landlord (e sub-recursos de
            // suporte) só podem ser autorizados por um operador interno REAL. Qualquer outra
            // identidade — inclusive um "Administrador" de tenant comum via fallback legado abaixo —
            // é barrada aqui, fail-closed, ANTES de qualquer atalho de cargo.
            if (SuperAdminSeguranca.ExigeOperadorInterno(_recurso))
            {
                Log.Warning(
                    "Acesso negado (recurso super-admin exige operador interno — 1.11) em {Recurso}:{Acao} para identidade não-interna (tenant={Tenant})",
                    _recurso, _acao, _tenantProvider.GetTenantId());
                context.Result = new ForbidResult();
                return;
            }

            var userId = _currentUser.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                Log.Warning("Tentativa de acesso não autenticada no recurso {Recurso}:{Acao}", _recurso, _acao);
                context.Result = new UnauthorizedResult();
                return;
            }

            // ================= 1.09/1.10: AUTORIZAÇÃO UNIFICADA POR RBAC (fonte autoritativa) =================
            // Capacidades efetivas do usuário = união das capacidades dos PAPÉIS atribuídos a ele NA
            // EMPRESA CORRENTE (papel com EmpresaId nulo vale p/ todas as empresas) + GRANT direto, com o
            // DENY direto SOBREPONDO (REG-040/041). O cálculo agora vive em ICapacidadesEfetivasService —
            // MESMA fonte que a projeção de menu (GET /menu), garantindo o invariante "item visível ⇔ o gate
            // autoriza" (LC-1/LC-2). Só vale para identidade real (userId é Guid de Usuario); operadores
            // internos e o legado com id textual seguem pelo caminho legado abaixo (intacto).
            if (Guid.TryParse(userId, out var usuarioGuid))
            {
                var requerida = CapacidadeCatalogoSeeder.NomeCapacidade(_recurso, _acao);

                // Empresa corrente da sessão (claim do token completo); nulo = sem empresa selecionada.
                Guid? empresaCorrente =
                    Guid.TryParse(context.HttpContext.User.FindFirst("empresaId")?.Value, out var eid)
                        ? eid
                        : (Guid?)null;

                // Resolve o serviço Scoped do request (compartilha o cache por request com o menu); em testes
                // unitários que constroem o filtro sem DI, cai para uma instância direta sobre o mesmo contexto.
                var servico =
                    context.HttpContext.RequestServices?.GetService(typeof(ICapacidadesEfetivasService)) as ICapacidadesEfetivasService
                    ?? new CapacidadesEfetivasService(_context);

                var efetivas = await servico.ObterAsync(usuarioGuid, empresaCorrente);

                // DENY direto sobrepõe tudo (REG-041): capacidade exigida negada ao usuário → 403.
                if (efetivas.Negadas.Contains(requerida))
                {
                    Log.Warning("Acesso negado (deny direto — REG-041) do usuário {UserId} em {Recurso}:{Acao}", userId, _recurso, _acao);
                    context.Result = new ForbidResult();
                    return;
                }

                if (efetivas.Concedidas.Contains(requerida))
                {
                    return; // Autorizado pelo RBAC unificado.
                }
            }
            // =============== fim RBAC 1.09/1.10; abaixo o caminho LEGADO (transitório, preservado) ===============

            // origin/main: ABAC no tenant system quando houver perfil; sem perfil, libera (usado abaixo).
            var isSystemTenant = string.Equals(_tenantProvider.GetTenantId(), "system", StringComparison.OrdinalIgnoreCase);

            // Busca o perfil do usuário no tenant atual (o tenant filter é aplicado automaticamente pelo EF Core)
            var perfil = await _context.PerfisUsuarios
                .Include(p => p.Permissoes)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (perfil == null || !perfil.Ativo)
            {
                // UsuarioInterno (tenant "system") sem PerfilUsuario em GestaoClientes: libera.
                // Se há perfil no tenant system (ex.: Operador/Administrador Siser), aplica ABAC abaixo.
                if (isSystemTenant)
                {
                    return;
                }

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
