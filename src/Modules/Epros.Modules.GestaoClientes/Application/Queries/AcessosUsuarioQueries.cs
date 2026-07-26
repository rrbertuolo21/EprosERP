using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    /// <summary>
    /// APP-TEN-008 (account/obter-acessos): monta a árvore de menu permitida (AcessosResponse)
    /// para a empresa selecionada, aplicando a matriz do perfil ou bypass de administrador.
    /// </summary>
    public record ObterAcessosUsuarioQuery(
        Guid? PerfilAcessoId,
        bool IsAdmin,
        string Login,
        string? Token,
        int? PlanoContasFinanceiroId,
        int? RegimeTributario,
        int? TributarioGrupoId
    ) : IQuery<CommandResult>;

    // Contrato AcessoItem (EF 11.8)
    public class AcessoItemDto
    {
        public bool r { get; set; }
        public bool u { get; set; }
        public bool d { get; set; }
        public string? icon { get; set; }
        public string sub { get; set; } = string.Empty;
        public int ordem { get; set; }
        public string? to { get; set; }
        public List<AcessoItemDto> itens { get; set; } = new();
    }

    // Contrato Acesso (EF 11.9)
    public class AcessoDto
    {
        public string menu { get; set; } = string.Empty;
        public string? icon { get; set; }
        public int ordem { get; set; }
        public string? to { get; set; }
        public List<AcessoItemDto> itens { get; set; } = new();
    }

    // Contrato AcessosResponse (EF 11.10)
    public class AcessosResponseDto
    {
        public List<AcessoDto> acesso { get; set; } = new();
        public bool isAdmin { get; set; }
        public string login { get; set; } = string.Empty;
        public string tenantId { get; set; } = string.Empty;
        public string? token { get; set; }
        public int? planoContasFinanceiroId { get; set; }
        public int? regimeTributario { get; set; }
        public int? tributarioGrupoId { get; set; }
    }

    public class ObterAcessosUsuarioQueryHandler : IQueryHandler<ObterAcessosUsuarioQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterAcessosUsuarioQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterAcessosUsuarioQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            // REG-004: usuário não administrador deve possuir perfil válido.
            if (!request.IsAdmin && (request.PerfilAcessoId == null || request.PerfilAcessoId == Guid.Empty))
                return CommandResult.Falha(new[] { "Usuário sem perfil válido para a empresa selecionada." });

            var menus = await _context.Menus
                .Include(m => m.ItensNivel1)
                .ThenInclude(i1 => i1.ItensNivel2)
                .OrderBy(m => m.Ordem)
                .ToListAsync(cancellationToken);

            // Matriz de permissões do perfil (quando não admin).
            var matriz = new Dictionary<(Guid menu, Guid? n1, Guid? n2), (bool r, bool u, bool d)>();
            if (!request.IsAdmin)
            {
                var acessos = await _context.PerfisAcessosMenus
                    .Where(a => a.PerfilAcessoId == request.PerfilAcessoId && a.TenantId == tenantId)
                    .ToListAsync(cancellationToken);
                foreach (var a in acessos)
                    matriz[(a.MenuId, a.MenuItemNivel1Id, a.MenuItemNivel2Id)] = (a.Ver, a.Editar, a.Excluir);
            }

            (bool r, bool u, bool d) Flags(Guid menuId, Guid? n1, Guid? n2)
            {
                if (request.IsAdmin) return (true, true, true); // REG-003/072: bypass de administrador
                return matriz.TryGetValue((menuId, n1, n2), out var f) ? f : (false, false, false);
            }

            var arvore = new List<AcessoDto>();
            foreach (var menu in menus)
            {
                var menuFlags = Flags(menu.Id, null, null);
                var itensN1 = new List<AcessoItemDto>();

                foreach (var n1 in menu.ItensNivel1.OrderBy(i => i.Ordem))
                {
                    var n1Flags = Flags(menu.Id, n1.Id, null);
                    var itensN2 = new List<AcessoItemDto>();

                    foreach (var n2 in n1.ItensNivel2.OrderBy(i => i.Ordem))
                    {
                        var n2Flags = Flags(menu.Id, n1.Id, n2.Id);
                        if (!request.IsAdmin && !n2Flags.r) continue;
                        itensN2.Add(new AcessoItemDto
                        {
                            sub = n2.Descricao, icon = n2.Icon, to = n2.To, ordem = n2.Ordem,
                            r = n2Flags.r, u = n2Flags.u, d = n2Flags.d
                        });
                    }

                    if (!request.IsAdmin && !n1Flags.r && itensN2.Count == 0) continue;
                    itensN1.Add(new AcessoItemDto
                    {
                        sub = n1.Descricao, icon = n1.Icon, to = n1.To, ordem = n1.Ordem,
                        r = n1Flags.r, u = n1Flags.u, d = n1Flags.d, itens = itensN2
                    });
                }

                if (!request.IsAdmin && !menuFlags.r && itensN1.Count == 0) continue;
                arvore.Add(new AcessoDto
                {
                    menu = menu.Descricao, icon = menu.Icon, to = menu.To, ordem = menu.Ordem, itens = itensN1
                });
            }

            var response = new AcessosResponseDto
            {
                acesso = arvore,
                isAdmin = request.IsAdmin,
                login = request.Login,
                tenantId = tenantId,
                token = request.Token,
                planoContasFinanceiroId = request.PlanoContasFinanceiroId,
                regimeTributario = request.RegimeTributario,
                tributarioGrupoId = request.TributarioGrupoId
            };

            return CommandResult.Ok("Acessos obtidos com sucesso.", response);
        }
    }
}
