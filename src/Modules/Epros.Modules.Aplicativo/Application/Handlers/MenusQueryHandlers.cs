using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Application.Services;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Application.Services;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Security;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    // DTOs Auxiliares para o Catálogo e Sessão
    public record PerfilAcessoDto(Guid Id, string Descricao, bool Ativo, int QuantidadeAcessos);
    
    public record PerfilAcessoDetalhadoDto(
        Guid Id, 
        string Descricao, 
        bool Ativo, 
        List<PerfilAcessoMenuDto> Acessos
    );

    public record PerfilAcessoMenuDto(
        Guid MenuId,
        Guid? MenuItemNivel1Id,
        Guid? MenuItemNivel2Id,
        bool Ver,
        bool Editar,
        bool Excluir
    );

    public record AcessoItemDto(
        string Sub,
        string? Icon,
        string? To,
        int Ordem,
        bool R, // Ver (Leitura)
        bool U, // Editar (Alteração)
        bool D, // Excluir
        List<AcessoItemDto> Itens
    );

    public record AcessoDto(
        string Menu,
        string? Icon,
        string? To,
        int Ordem,
        List<AcessoItemDto> Itens
    );

    public record MenuItemNivel2Dto(
        Guid Id,
        string Descricao,
        string? Icon,
        string? To,
        int Ordem
    );

    public record MenuItemNivel1Dto(
        Guid Id,
        string Descricao,
        string? Icon,
        string? To,
        int Ordem,
        List<MenuItemNivel2Dto> Itens
    );

    public record MenuDto(
        Guid Id,
        string Descricao,
        string? Icon,
        string? To,
        int Ordem,
        string? Modulo,
        List<MenuItemNivel1Dto> Itens
    );

    public record AcessosResponseDto(
        List<AcessoDto> Acesso,
        bool IsAdmin,
        string Login,
        string TenantId,
        Guid? PlanoContasFinanceiroId,
        int RegimeTributario,
        Guid? TributarioGrupoId,
        string Token
    );

    // ===== 1.10 (PERMISSOES_DE_MENU): menu VISÍVEL projetado das capacidades efetivas =====
    public record MenuVisivelItemDto(
        string Sub,
        string? Icon,
        string? To,
        int Ordem,
        string? CapacidadeRequerida,
        List<MenuVisivelItemDto> Itens
    );

    public record MenuVisivelDto(
        string Menu,
        string? Icon,
        string? To,
        int Ordem,
        string? CapacidadeRequerida,
        List<MenuVisivelItemDto> Itens
    );

    public record MenuDoUsuarioResponseDto(
        List<MenuVisivelDto> Menu,
        bool IsAdmin,
        string TenantId,
        Guid EmpresaId
    );

    public class ListarPerfisAcessoQueryHandler : IQueryHandler<ListarPerfisAcessoQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarPerfisAcessoQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarPerfisAcessoQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            var total = await _context.PerfisAcessos
                .CountAsync(p => p.TenantId == tenantId && p.DeletadoEm == null, cancellationToken);

            var perfis = await _context.PerfisAcessos
                .Include(p => p.Acessos)
                .Where(p => p.TenantId == tenantId && p.DeletadoEm == null)
                .OrderBy(p => p.Descricao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(p => new PerfilAcessoDto(
                    p.Id,
                    p.Descricao,
                    p.Ativo,
                    p.Acessos.Count(a => a.DeletadoEm == null)
                ))
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Perfis de acesso listados com sucesso.", new { Total = total, Pagina = request.Pagina, Itens = perfis });
        }
    }

    public class ObterPerfilAcessoPorIdQueryHandler : IQueryHandler<ObterPerfilAcessoPorIdQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterPerfilAcessoPorIdQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterPerfilAcessoPorIdQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            var perfil = await _context.PerfisAcessos
                .Include(p => p.Acessos)
                .FirstOrDefaultAsync(p => p.Id == request.Id && p.TenantId == tenantId && p.DeletadoEm == null, cancellationToken);

            if (perfil == null)
            {
                return CommandResult.Falha(new[] { "Perfil de acesso não encontrado." });
            }

            var acessosDto = perfil.Acessos
                .Where(a => a.DeletadoEm == null)
                .Select(a => new PerfilAcessoMenuDto(
                    a.MenuId,
                    a.MenuItemNivel1Id,
                    a.MenuItemNivel2Id,
                    a.Ver,
                    a.Editar,
                    a.Excluir
                ))
                .ToList();

            var dto = new PerfilAcessoDetalhadoDto(perfil.Id, perfil.Descricao, perfil.Ativo, acessosDto);
            return CommandResult.Ok("Perfil de acesso recuperado com sucesso.", dto);
        }
    }

    public class ObterArvoreCompletaMenusQueryHandler : IQueryHandler<ObterArvoreCompletaMenusQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;

        public ObterArvoreCompletaMenusQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterArvoreCompletaMenusQuery request, CancellationToken cancellationToken)
        {
            var menusCompleto = await _context.Menus
                .Include(m => m.ItensNivel1)
                    .ThenInclude(i1 => i1.ItensNivel2)
                .OrderBy(m => m.Ordem)
                .ToListAsync(cancellationToken);

            // Ordena manualmente na memória para garantir ordem correta em todos os níveis
            foreach (var menu in menusCompleto)
            {
                menu.ItensNivel1.Sort((a, b) => a.Ordem.CompareTo(b.Ordem));
                foreach (var item1 in menu.ItensNivel1)
                {
                    item1.ItensNivel2.Sort((a, b) => a.Ordem.CompareTo(b.Ordem));
                }
            }

            return CommandResult.Ok("Catálogo completo de menus recuperado.", menusCompleto);
        }
    }

    public class ListarMenusQueryHandler : IQueryHandler<ListarMenusQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;

        public ListarMenusQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ListarMenusQuery request, CancellationToken cancellationToken)
        {
            var total = await _context.Menus.CountAsync(cancellationToken);

            var menus = await _context.Menus
                .AsNoTracking()
                .Include(m => m.ItensNivel1)
                    .ThenInclude(i1 => i1.ItensNivel2)
                .OrderBy(m => m.Ordem)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);

            var itens = menus.Select(MapearMenuDto).ToList();

            return CommandResult.Ok("Menus listados com sucesso.", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }

        internal static MenuDto MapearMenuDto(Epros.Modules.GestaoClientes.Domain.Entities.Menu menu)
        {
            return new MenuDto(
                menu.Id,
                menu.Descricao,
                menu.Icon,
                menu.To,
                menu.Ordem,
                menu.Modulo,
                menu.ItensNivel1
                    .OrderBy(i1 => i1.Ordem)
                    .Select(i1 => new MenuItemNivel1Dto(
                        i1.Id,
                        i1.Descricao,
                        i1.Icon,
                        i1.To,
                        i1.Ordem,
                        i1.ItensNivel2
                            .OrderBy(i2 => i2.Ordem)
                            .Select(i2 => new MenuItemNivel2Dto(i2.Id, i2.Descricao, i2.Icon, i2.To, i2.Ordem))
                            .ToList()))
                    .ToList());
        }
    }

    public class ObterMenuPorIdQueryHandler : IQueryHandler<ObterMenuPorIdQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;

        public ObterMenuPorIdQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterMenuPorIdQuery request, CancellationToken cancellationToken)
        {
            var menu = await _context.Menus
                .AsNoTracking()
                .Include(m => m.ItensNivel1)
                    .ThenInclude(i1 => i1.ItensNivel2)
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (menu == null)
            {
                return CommandResult.Falha(new[] { "Menu não encontrado." });
            }

            return CommandResult.Ok("Menu recuperado com sucesso.", ListarMenusQueryHandler.MapearMenuDto(menu));
        }
    }

    public class ObterAcessosSessaoQueryHandler : IQueryHandler<ObterAcessosSessaoQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _contextGestao;
        private readonly ContextAplicativo _contextApp;
        private readonly ITenantProvider _tenantProvider;
        private readonly IPermissaoCacheManager _cacheManager;
        private readonly IEprosTokenService _tokenService;

        public ObterAcessosSessaoQueryHandler(
            ContextGestaoClientes contextGestao,
            ContextAplicativo contextApp,
            ITenantProvider tenantProvider,
            IPermissaoCacheManager cacheManager,
            IEprosTokenService tokenService)
        {
            _contextGestao = contextGestao;
            _contextApp = contextApp;
            _tenantProvider = tenantProvider;
            _cacheManager = cacheManager;
            _tokenService = tokenService;
        }

        public async Task<CommandResult> Handle(ObterAcessosSessaoQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            // 1. Obter usuário ativo
            var usuario = await _contextApp.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioId && u.DeletadoEm == null, cancellationToken);

            if (usuario == null || usuario.Status != UsuarioStatus.Active)
            {
                return CommandResult.Falha(new[] { "Usuário inexistente ou inativo." });
            }

            // 2. Verificar se o tenant está ativo e seu plano contratado
            var cliente = await _contextGestao.Clientes
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.DeletadoEm == null, cancellationToken);

            if (cliente == null || !cliente.Ativo)
            {
                return CommandResult.Falha(new[] { "Tenant inativo ou inexistente." });
            }

            // Trava SaaS de bloqueio por inadimplência
            var dataLimite = DateTime.UtcNow.AddDays(-15);
            var inadimplente = await _contextGestao.Faturas
                .AnyAsync(f => f.ClienteId == cliente.Id &&
                               f.Status != FaturaStatus.Paga &&
                               f.Status != FaturaStatus.Cancelada &&
                               f.DataVencimento < dataLimite &&
                               f.DeletadoEm == null, cancellationToken);

            if (inadimplente)
            {
                // Retorna indicando bloqueio SaaS
                return CommandResult.Falha(new[] { "Acesso operacional bloqueado por inadimplência. Regularize as suas faturas." }, block: true);
            }

            var plano = await _contextGestao.Planos
                .Include(p => p.Modulos)
                .FirstOrDefaultAsync(p => p.Id == cliente.PlanoId, cancellationToken);

            if (plano == null || !plano.Ativo)
            {
                return CommandResult.Falha(new[] { "Plano do inquilino inválido ou inativo." });
            }

            var modulosAtivos = plano.Modulos.Select(m => m.NomeModulo.ToLowerInvariant()).ToList();

            // 3. Obter a empresa ativa e seus dados
            var empresa = await _contextGestao.Empresas
                .FirstOrDefaultAsync(e => e.Id == request.EmpresaId && e.TenantId == tenantId && e.Ativo && e.DeletadoEm == null, cancellationToken);

            if (empresa == null)
            {
                return CommandResult.Falha(new[] { "Empresa selecionada inválida ou inativa." });
            }

            // 4. Obter vínculo usuário-empresa
            var vinculo = await _contextApp.UsuariosEmpresas
                .FirstOrDefaultAsync(ue => ue.UsuarioId == request.UsuarioId && ue.EmpresaId == request.EmpresaId && ue.DeletadoEm == null, cancellationToken);

            if (vinculo == null)
            {
                return CommandResult.Falha(new[] { "O usuário não possui vínculo de acesso a esta empresa." });
            }

            // 5. Carregar as permissões efetivas (com cache)
            List<PerfilAcessoMenu> permissoes = new();

            if (!vinculo.EhAdmin)
            {
                if (!vinculo.PerfilAcessoId.HasValue)
                {
                    return CommandResult.Falha(new[] { "Vínculo comum do usuário na empresa exige um perfil de acesso associado." });
                }

                // Carrega do Cache Manager
                permissoes = await _cacheManager.ObterPermissoesEfetivasAsync(
                    tenantId,
                    request.UsuarioId,
                    request.EmpresaId,
                    async () =>
                    {
                        var perfil = await _contextGestao.PerfisAcessos
                            .Include(p => p.Acessos)
                            .FirstOrDefaultAsync(p => p.Id == vinculo.PerfilAcessoId.Value && p.TenantId == tenantId && p.Ativo && p.DeletadoEm == null, cancellationToken);

                        if (perfil == null)
                            return new List<PerfilAcessoMenu>();

                        return perfil.Acessos.Where(a => a.DeletadoEm == null).ToList();
                    }
                );

                if (permissoes.Count == 0)
                {
                    return CommandResult.Falha(new[] { "Perfil de acesso do usuário está inválido, inativo ou sem permissões atribuídas." });
                }
            }

            // 6. Carregar catálogo global de menus
            var catalogoCompleto = await _contextGestao.Menus
                .Include(m => m.ItensNivel1)
                    .ThenInclude(i1 => i1.ItensNivel2)
                .OrderBy(m => m.Ordem)
                .ToListAsync(cancellationToken);

            // 7. Montar árvore filtrada
            var acessosPermitidos = new List<AcessoDto>();

            // Regra fiscal de MEI
            var ehSimplesMei = empresa.RegimeTributario == RegimeTributario.SimplesNacional && empresa.EhMei;

            foreach (var menu in catalogoCompleto)
            {
                // Filtra pelo módulo ativo no plano
                if (!string.IsNullOrEmpty(menu.Modulo) && !modulosAtivos.Contains(menu.Modulo.ToLowerInvariant()))
                    continue;

                // Ocultação específica do MEI para o menu principal
                var ehMenuMei = menu.Descricao.Contains("MEI", StringComparison.OrdinalIgnoreCase) || 
                                (menu.Modulo != null && menu.Modulo.Equals("MEI", StringComparison.OrdinalIgnoreCase));

                if (ehMenuMei && !ehSimplesMei)
                    continue;

                // Para usuários normais, verifica se há pelo menos um item permitido Ver no menu principal ou submenus
                var menuPermitido = vinculo.EhAdmin;
                var verMenu = false;
                var editarMenu = false;
                var excluirMenu = false;

                if (!vinculo.EhAdmin)
                {
                    var permMenu = permissoes.FirstOrDefault(p => p.MenuId == menu.Id && p.MenuItemNivel1Id == null && p.MenuItemNivel2Id == null);
                    if (permMenu != null && permMenu.Ver)
                    {
                        menuPermitido = true;
                        verMenu = permMenu.Ver;
                        editarMenu = permMenu.Editar;
                        excluirMenu = permMenu.Excluir;
                    }
                }

                var itensN1Permitidos = new List<AcessoItemDto>();

                foreach (var item1 in menu.ItensNivel1.OrderBy(i => i.Ordem))
                {
                    // Ocultação específica do MEI (ID funcional 16 ou rota com /mei)
                    // Vamos considerar a verificação se Descricao contém "MEI" ou se o Modulo é "MEI" ou ID funcional é mapeado
                    // O doc funcional diz: "identificado no material pelo id funcional 16"
                    // Vamos criar um Guid determinístico para o Menu/Item MEI de forma que possamos identificá-lo pelo ID ou pelo Modulo / Descricao
                    var ehItemMei = item1.Descricao.Contains("MEI", StringComparison.OrdinalIgnoreCase) || 
                                    (item1.Modulo != null && item1.Modulo.Equals("MEI", StringComparison.OrdinalIgnoreCase));

                    if (ehItemMei && !ehSimplesMei)
                        continue;

                    if (!string.IsNullOrEmpty(item1.Modulo) && !modulosAtivos.Contains(item1.Modulo.ToLowerInvariant()))
                        continue;

                    var item1Permitido = vinculo.EhAdmin;
                    var verI1 = false;
                    var editarI1 = false;
                    var excluirI1 = false;

                    if (!vinculo.EhAdmin)
                    {
                        var permI1 = permissoes.FirstOrDefault(p => p.MenuId == menu.Id && p.MenuItemNivel1Id == item1.Id && p.MenuItemNivel2Id == null);
                        if (permI1 != null && permI1.Ver)
                        {
                            item1Permitido = true;
                            verI1 = permI1.Ver;
                            editarI1 = permI1.Editar;
                            excluirI1 = permI1.Excluir;
                        }
                    }

                    var itensN2Permitidos = new List<AcessoItemDto>();

                    foreach (var item2 in item1.ItensNivel2.OrderBy(i => i.Ordem))
                    {
                        var ehItem2Mei = item2.Descricao.Contains("MEI", StringComparison.OrdinalIgnoreCase) ||
                                         (item2.Modulo != null && item2.Modulo.Equals("MEI", StringComparison.OrdinalIgnoreCase));

                        if (ehItem2Mei && !ehSimplesMei)
                            continue;

                        if (!string.IsNullOrEmpty(item2.Modulo) && !modulosAtivos.Contains(item2.Modulo.ToLowerInvariant()))
                            continue;

                        var item2Permitido = vinculo.EhAdmin;
                        var verI2 = false;
                        var editarI2 = false;
                        var excluirI2 = false;

                        if (!vinculo.EhAdmin)
                        {
                            var permI2 = permissoes.FirstOrDefault(p => p.MenuId == menu.Id && p.MenuItemNivel1Id == item1.Id && p.MenuItemNivel2Id == item2.Id);
                            if (permI2 != null && permI2.Ver)
                            {
                                item2Permitido = true;
                                verI2 = permI2.Ver;
                                editarI2 = permI2.Editar;
                                excluirI2 = permI2.Excluir;
                            }
                        }

                        if (item2Permitido)
                        {
                            itensN2Permitidos.Add(new AcessoItemDto(
                                Sub: item2.Descricao,
                                Icon: item2.Icon,
                                To: item2.To,
                                Ordem: item2.Ordem,
                                R: vinculo.EhAdmin || verI2,
                                U: vinculo.EhAdmin || editarI2,
                                D: vinculo.EhAdmin || excluirI2,
                                Itens: new List<AcessoItemDto>()
                            ));
                            
                            // Se algum item filho for permitido, o pai (nível 1) também se torna implicitamente visível
                            item1Permitido = true; 
                        }
                    }

                    if (item1Permitido)
                    {
                        itensN1Permitidos.Add(new AcessoItemDto(
                            Sub: item1.Descricao,
                            Icon: item1.Icon,
                            To: item1.To,
                            Ordem: item1.Ordem,
                            R: vinculo.EhAdmin || verI1 || itensN2Permitidos.Any(),
                            U: vinculo.EhAdmin || editarI1,
                            D: vinculo.EhAdmin || excluirI1,
                            Itens: itensN2Permitidos
                        ));

                        // Se algum submenu for permitido, o menu pai principal também se torna implicitamente visível
                        menuPermitido = true;
                    }
                }

                if (menuPermitido)
                {
                    acessosPermitidos.Add(new AcessoDto(
                        Menu: menu.Descricao,
                        Icon: menu.Icon,
                        To: menu.To,
                        Ordem: menu.Ordem,
                        Itens: itensN1Permitidos
                    ));
                }
            }

            // 8. Emitir Token Completo
            var tokenCompleto = _tokenService.GerarCompleto(tenantId, usuario.Id.ToString(), empresa.Id.ToString(), vinculo.PerfilAcessoId?.ToString() ?? "null");

            var response = new AcessosResponseDto(
                Acesso: acessosPermitidos,
                IsAdmin: vinculo.EhAdmin,
                // REG/MC: transporta o login REAL (Usuario.Login); cai para o e-mail só quando não há login definido.
                Login: usuario.Login ?? usuario.Email,
                TenantId: tenantId,
                PlanoContasFinanceiroId: empresa.PlanoContasFinanceiroId,
                RegimeTributario: (int)empresa.RegimeTributario,
                TributarioGrupoId: empresa.TributarioGrupoId,
                Token: tokenCompleto
            );

            return CommandResult.Ok("Sessão e acessos consolidados com sucesso!", response);
        }
    }

    /// <summary>
    /// 1.10 (PERMISSOES_DE_MENU) — PROJEÇÃO de menu a partir das CAPACIDADES efetivas do RBAC (decisão 1.09).
    /// Fonte única = <see cref="ICapacidadesEfetivasService"/> (a MESMA do AbacFilter): um item aparece sse a
    /// sua <c>CapacidadeRequerida</c> ("recurso:acao") está no conjunto efetivo do usuário NA EMPRESA CORRENTE
    /// (papel-por-empresa, 1.09) — fecha LC-1/LC-2/LC-4 e o invariante REG-002/REG-080/§8.2/CA-007
    /// ("visível ⇔ o gate autoriza"). Aplica também o entitlement de plano (1.06) e a ocultação MEI existente.
    ///
    /// NÃO substitui o gate: esconder no menu é defense-in-depth; o [AbacAuthorize] segue sendo a barreira real.
    /// Item ESTRUTURAL (sem CapacidadeRequerida) só aparece se tiver filho visível — "nega por padrão" (REG-002).
    /// </summary>
    public class ObterMenuDoUsuarioQueryHandler : IQueryHandler<ObterMenuDoUsuarioQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _contextGestao;
        private readonly ContextAplicativo _contextApp;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICapacidadesEfetivasService _capacidades;

        public ObterMenuDoUsuarioQueryHandler(
            ContextGestaoClientes contextGestao,
            ContextAplicativo contextApp,
            ITenantProvider tenantProvider,
            ICapacidadesEfetivasService capacidades)
        {
            _contextGestao = contextGestao;
            _contextApp = contextApp;
            _tenantProvider = tenantProvider;
            _capacidades = capacidades;
        }

        public async Task<CommandResult> Handle(ObterMenuDoUsuarioQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            // 1. Usuário ativo.
            var usuario = await _contextApp.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioId && u.DeletadoEm == null, cancellationToken);
            if (usuario == null || usuario.Status != UsuarioStatus.Active)
                return CommandResult.Falha(new[] { "Usuário inexistente ou inativo." });

            // 2. Empresa corrente válida no tenant.
            var empresa = await _contextGestao.Empresas
                .FirstOrDefaultAsync(e => e.Id == request.EmpresaId && e.TenantId == tenantId && e.Ativo && e.DeletadoEm == null, cancellationToken);
            if (empresa == null)
                return CommandResult.Falha(new[] { "Empresa selecionada inválida ou inativa." });

            // 3. Vínculo do usuário com a empresa (papel-por-empresa é do RBAC; aqui só confirmamos o acesso).
            var vinculo = await _contextApp.UsuariosEmpresas
                .FirstOrDefaultAsync(ue => ue.UsuarioId == request.UsuarioId && ue.EmpresaId == request.EmpresaId && ue.DeletadoEm == null, cancellationToken);
            if (vinculo == null)
                return CommandResult.Falha(new[] { "O usuário não possui vínculo de acesso a esta empresa." });

            // 4. Entitlement de plano (1.06): módulos contratados pelo tenant. Fail-safe: sem plano resolvível,
            //    não filtra por módulo (mesma postura do ModuloTenantMiddleware) — o gate ainda protege a API.
            List<string>? modulosAtivos = null;
            var cliente = await _contextGestao.Clientes
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.DeletadoEm == null, cancellationToken);
            if (cliente != null)
            {
                var plano = await _contextGestao.Planos
                    .Include(p => p.Modulos)
                    .FirstOrDefaultAsync(p => p.Id == cliente.PlanoId, cancellationToken);
                if (plano != null && plano.Ativo)
                    modulosAtivos = plano.Modulos.Select(m => m.NomeModulo.ToLowerInvariant()).ToList();
            }

            bool ModuloContratado(string? modulo)
                => string.IsNullOrEmpty(modulo) || modulosAtivos == null || modulosAtivos.Contains(modulo.ToLowerInvariant());

            // 5. Capacidades efetivas do usuário NA EMPRESA CORRENTE (fonte única, papel-por-empresa 1.09).
            var efetivas = await _capacidades.ObterAsync(request.UsuarioId, request.EmpresaId, cancellationToken);

            // isAdmin informativo: possui o papel de sistema "Administrador" (todas as capacidades). Não é bypass —
            // o admin vê tudo porque as capacidades estão no conjunto efetivo, mantendo menu ⇔ gate coerentes.
            var isAdmin = await (
                from up in _contextGestao.UsuariosPapeis
                join p in _contextGestao.Papeis.IgnoreQueryFilters() on up.PapelId equals p.Id
                where up.UsuarioId == request.UsuarioId
                      && (up.EmpresaId == null || up.EmpresaId == request.EmpresaId)
                      && p.Name == CapacidadeCatalogoNomePapelAdministrador
                      && p.DeletadoEm == null
                select up.Id
            ).AnyAsync(cancellationToken);

            // 6. Ocultação MEI (REG-030) — mantida como no fluxo existente (por texto). D-6 (id funcional 16 vs
            //    texto) é regra FISCAL: roteada/diferida, não reimplementada aqui.
            var ehSimplesMei = empresa.RegimeTributario == RegimeTributario.SimplesNacional && empresa.EhMei;
            bool OcultaPorMei(string descricao, string? modulo)
            {
                var ehMei = descricao.Contains("MEI", StringComparison.OrdinalIgnoreCase)
                            || (modulo != null && modulo.Equals("MEI", StringComparison.OrdinalIgnoreCase));
                return ehMei && !ehSimplesMei;
            }

            // 7. Catálogo global e projeção pela capacidade efetiva.
            var catalogo = await _contextGestao.Menus
                .Include(m => m.ItensNivel1)
                    .ThenInclude(i1 => i1.ItensNivel2)
                .OrderBy(m => m.Ordem)
                .ToListAsync(cancellationToken);

            var menusVisiveis = new List<MenuVisivelDto>();

            foreach (var menu in catalogo)
            {
                if (!ModuloContratado(menu.Modulo)) continue;
                if (OcultaPorMei(menu.Descricao, menu.Modulo)) continue;

                var itensN1 = new List<MenuVisivelItemDto>();
                foreach (var i1 in menu.ItensNivel1.OrderBy(i => i.Ordem))
                {
                    if (!ModuloContratado(i1.Modulo)) continue;
                    if (OcultaPorMei(i1.Descricao, i1.Modulo)) continue;

                    var itensN2 = new List<MenuVisivelItemDto>();
                    foreach (var i2 in i1.ItensNivel2.OrderBy(i => i.Ordem))
                    {
                        if (!ModuloContratado(i2.Modulo)) continue;
                        if (OcultaPorMei(i2.Descricao, i2.Modulo)) continue;

                        // Folha: só aparece se declarar capacidade E o usuário a tiver (nega por padrão).
                        if (i2.CapacidadeRequerida != null && efetivas.PodeUsar(i2.CapacidadeRequerida))
                        {
                            itensN2.Add(new MenuVisivelItemDto(
                                i2.Descricao, i2.Icon, i2.To, i2.Ordem, i2.CapacidadeRequerida,
                                new List<MenuVisivelItemDto>()));
                        }
                    }

                    var n1Proprio = i1.CapacidadeRequerida != null && efetivas.PodeUsar(i1.CapacidadeRequerida);
                    if (n1Proprio || itensN2.Count > 0)
                    {
                        itensN1.Add(new MenuVisivelItemDto(
                            i1.Descricao, i1.Icon, i1.To, i1.Ordem, i1.CapacidadeRequerida, itensN2));
                    }
                }

                var menuProprio = menu.CapacidadeRequerida != null && efetivas.PodeUsar(menu.CapacidadeRequerida);
                if (menuProprio || itensN1.Count > 0)
                {
                    menusVisiveis.Add(new MenuVisivelDto(
                        menu.Descricao, menu.Icon, menu.To, menu.Ordem, menu.CapacidadeRequerida, itensN1));
                }
            }

            var response = new MenuDoUsuarioResponseDto(menusVisiveis, isAdmin, tenantId, request.EmpresaId);
            return CommandResult.Ok("Menu do usuário projetado a partir das capacidades efetivas.", response);
        }

        // Nome do papel de sistema Administrador (espelha CapacidadeCatalogoSeeder.PapelAdministradorNome, que
        // vive na camada de API; duplicar a constante aqui evita referência inversa Aplicativo→API).
        private const string CapacidadeCatalogoNomePapelAdministrador = "Administrador";
    }
}
