using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Obtém Menu Dinamico.</summary>
    public class ObterMenuDinamicoQueryHandler : IQueryHandler<ObterMenuDinamicoQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterMenuDinamicoQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterMenuDinamicoQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            // 1. Obtém o perfil do usuário
            var perfil = await _context.PerfisColaboradores
                .Include(p => p.Permissoes)
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

            if (perfil == null || !perfil.Ativo)
            {
                return CommandResult.Falha(new[] { "Perfil de usuário não encontrado ou inativo." });
            }

            // 2. Obtém o cliente (inquilino) e seu plano
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.DeletadoEm == null, cancellationToken);

            if (cliente == null || !cliente.Ativo)
            {
                return CommandResult.Falha(new[] { "Tenant inativo ou não cadastrado." });
            }

            var plano = await _context.Planos
                .Include(p => p.Modulos)
                .FirstOrDefaultAsync(p => p.Id == cliente.PlanoId, cancellationToken);

            if (plano == null || !plano.Ativo)
            {
                return CommandResult.Falha(new[] { "Plano do tenant inativo ou inválido." });
            }

            var modulosAtivosNoPlano = plano.Modulos.Select(m => m.NomeModulo.ToLower()).ToList();

            // 3. Monta o menu dinâmico baseado no Plano + ACL do usuário
            var menu = new List<MenuDinamicoDto>();
            var isAdmin = perfil.Cargo.Equals("Administrador", StringComparison.OrdinalIgnoreCase);

            // Módulo Financeiro
            if (modulosAtivosNoPlano.Contains("financeiro") && (isAdmin || TemAcessoRecurso(perfil, "Financeiro", "Ler")))
            {
                var submenus = new List<string>();
                if (isAdmin || TemAcessoRecurso(perfil, "ContasPagar", "Ler")) submenus.Add("Contas a Pagar");
                if (isAdmin || TemAcessoRecurso(perfil, "ContasReceber", "Ler")) submenus.Add("Contas a Receber");

                menu.Add(new MenuDinamicoDto
                {
                    Modulo = "Financeiro",
                    Nome = "Financeiro",
                    Icone = "credit-card",
                    Rota = "/financeiro",
                    SubMenus = submenus
                });
            }

            // Módulo Estoque
            if (modulosAtivosNoPlano.Contains("estoque") && (isAdmin || TemAcessoRecurso(perfil, "Estoque", "Ler")))
            {
                var submenus = new List<string>();
                if (isAdmin || TemAcessoRecurso(perfil, "Produtos", "Ler")) submenus.Add("Produtos");
                if (isAdmin || TemAcessoRecurso(perfil, "Compras", "Ler")) submenus.Add("Compras");

                menu.Add(new MenuDinamicoDto
                {
                    Modulo = "Estoque",
                    Nome = "Estoque & Compras",
                    Icone = "package",
                    Rota = "/estoque",
                    SubMenus = submenus
                });
            }

            // Módulo Vendas
            if (modulosAtivosNoPlano.Contains("vendas") && (isAdmin || TemAcessoRecurso(perfil, "Vendas", "Ler")))
            {
                var submenus = new List<string>();
                if (isAdmin || TemAcessoRecurso(perfil, "Pedidos", "Ler")) submenus.Add("Pedidos");
                if (isAdmin || TemAcessoRecurso(perfil, "PDV", "Ler")) submenus.Add("PDV");

                menu.Add(new MenuDinamicoDto
                {
                    Modulo = "Vendas",
                    Nome = "Vendas & PDV",
                    Icone = "shopping-cart",
                    Rota = "/vendas",
                    SubMenus = submenus
                });
            }

            return CommandResult.Ok("Menu obtido com sucesso.", menu);
        }

        private bool TemAcessoRecurso(PerfilColaborador perfil, string recurso, string acao)
        {
            return perfil.Permissoes.Any(p => 
                p.Recurso.Equals(recurso, StringComparison.OrdinalIgnoreCase) && 
                p.Acao.Equals(acao, StringComparison.OrdinalIgnoreCase) && 
                p.Permitido);
        }
    }
}
