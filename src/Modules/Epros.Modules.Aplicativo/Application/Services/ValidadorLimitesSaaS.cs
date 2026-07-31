using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.Aplicativo.Application.Services
{
    public class ValidadorLimitesSaaS : IValidadorLimitesSaaS
    {
        private readonly ContextAplicativo _contextApp;
        private readonly ContextGestaoClientes _contextGestao;

        public ValidadorLimitesSaaS(ContextAplicativo contextApp, ContextGestaoClientes contextGestao)
        {
            _contextApp = contextApp;
            _contextGestao = contextGestao;
        }

        public async Task<bool> PossuiFolgaUsuariosAsync(string tenantId, CancellationToken cancellationToken = default)
        {
            var (excedido, _) = await ValidarLimiteUsuariosAsync(tenantId, cancellationToken);
            return !excedido;
        }

        public async Task<bool> PossuiFolgaEmpresasAsync(string tenantId, CancellationToken cancellationToken = default)
        {
            var (excedido, _) = await ValidarLimiteEmpresasAsync(tenantId, cancellationToken);
            return !excedido;
        }

        public async Task<(bool Excedido, string Mensagem)> ValidarLimiteUsuariosAsync(string tenantId, CancellationToken cancellationToken = default)
        {
            // Ignora se for o tenant do sistema
            if (tenantId == "system")
            {
                return (false, string.Empty);
            }

            var cliente = await _contextGestao.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Ativo && c.DeletadoEm == null, cancellationToken);

            if (cliente == null)
            {
                return (true, "Cliente assinante correspondente ao inquilino não foi encontrado.");
            }

            if (cliente.PlanoId == Guid.Empty)
            {
                return (true, "Cliente não possui plano associado.");
            }

            var plano = await _contextGestao.Planos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == cliente.PlanoId && p.Ativo && p.DeletadoEm == null, cancellationToken);

            if (plano == null)
            {
                return (true, "Plano contratado inativo ou não encontrado.");
            }

            // 1.01 — Override por cliente (snapshot): usa a cota contratada no Cliente quando preenchida,
            // senão cai para o limite do plano base.
            var limiteUsuarios = cliente.CotaUsuarios ?? plano.LimiteUsuarios;

            // Regra: Menor ou igual a zero representa Ilimitado
            if (limiteUsuarios <= 0)
            {
                return (false, string.Empty);
            }

            var totalUsuarios = await _contextApp.Usuarios
                .CountAsync(u => u.TenantId == tenantId && u.DeletadoEm == null && u.Status == UsuarioStatus.Active, cancellationToken);

            if (totalUsuarios >= limiteUsuarios)
            {
                return (true, $"O limite de usuários ativos contratados em seu plano foi atingido ({totalUsuarios}/{limiteUsuarios}). Por favor, faça um upgrade para adicionar novos usuários.");
            }

            return (false, string.Empty);
        }

        public async Task<(bool Excedido, string Mensagem)> ValidarLimiteEmpresasAsync(string tenantId, CancellationToken cancellationToken = default)
        {
            // Ignora se for o tenant do sistema
            if (tenantId == "system")
            {
                return (false, string.Empty);
            }

            var cliente = await _contextGestao.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Ativo && c.DeletadoEm == null, cancellationToken);

            if (cliente == null)
            {
                return (true, "Cliente assinante correspondente ao inquilino não foi encontrado.");
            }

            if (cliente.PlanoId == Guid.Empty)
            {
                return (true, "Cliente não possui plano associado.");
            }

            var plano = await _contextGestao.Planos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == cliente.PlanoId && p.Ativo && p.DeletadoEm == null, cancellationToken);

            if (plano == null)
            {
                return (true, "Plano contratado inativo ou não encontrado.");
            }

            // 1.01 — Override por cliente (snapshot): usa a cota contratada no Cliente quando preenchida,
            // senão cai para o limite do plano base.
            var limiteEmpresas = cliente.CotaEmpresas ?? plano.LimiteEmpresas;

            // Regra: Menor ou igual a zero representa Ilimitado
            if (limiteEmpresas <= 0)
            {
                return (false, string.Empty);
            }

            var totalEmpresas = await _contextGestao.Empresas
                .CountAsync(e => e.TenantId == tenantId && e.DeletadoEm == null, cancellationToken);

            if (totalEmpresas >= limiteEmpresas)
            {
                return (true, $"O limite de empresas cadastradas contratadas em seu plano foi atingido ({totalEmpresas}/{limiteEmpresas}). Por favor, faça um upgrade para adicionar novas empresas.");
            }

            return (false, string.Empty);
        }

        // 1.06 — Limite de CLIENTES (customers do tenant): conta Pessoas com papel Cliente ativas
        // do tenant e bloqueia antes de persistir. Override por cliente-assinante (CotaClientes)
        // sobrepõe o limite do plano; <= 0 (ou ausência de cota/plano) = ilimitado (REG-014).
        public async Task<(bool Excedido, string Mensagem)> ValidarLimiteClientesAsync(string tenantId, CancellationToken cancellationToken = default)
        {
            if (tenantId == "system")
            {
                return (false, string.Empty);
            }

            var cliente = await _contextGestao.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Ativo && c.DeletadoEm == null, cancellationToken);

            if (cliente == null)
            {
                return (true, "Cliente assinante correspondente ao inquilino não foi encontrado.");
            }

            if (cliente.PlanoId == Guid.Empty)
            {
                return (true, "Cliente não possui plano associado.");
            }

            var plano = await _contextGestao.Planos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == cliente.PlanoId && p.Ativo && p.DeletadoEm == null, cancellationToken);

            if (plano == null)
            {
                return (true, "Plano contratado inativo ou não encontrado.");
            }

            // Override por cliente (snapshot) sobrepõe o limite base do plano.
            var limiteClientes = cliente.CotaClientes ?? plano.LimiteClientes;

            // Regra REG-014: menor ou igual a zero = ilimitado.
            if (limiteClientes <= 0)
            {
                return (false, string.Empty);
            }

            var totalClientes = await _contextGestao.PessoasClientes
                .IgnoreQueryFilters()
                .CountAsync(pc => pc.TenantId == tenantId && pc.DeletadoEm == null, cancellationToken);

            if (totalClientes >= limiteClientes)
            {
                return (true, $"O limite de clientes cadastrados contratados em seu plano foi atingido ({totalClientes}/{limiteClientes}). Por favor, faça um upgrade para adicionar novos clientes.");
            }

            return (false, string.Empty);
        }

        // 1.06 — Limite de PERMISSÕES/PAPÉIS (RBAC) por tenant. Liga a cota órfã CotaPermissoes:
        // conta os papéis (roles) ativos do tenant. Aqui NÃO há teto no Plano — a cota vive apenas
        // no cliente-assinante (CotaPermissoes). Ausente/nula ou <= 0 = ilimitado (REG-014).
        public async Task<(bool Excedido, string Mensagem)> ValidarLimitePermissoesAsync(string tenantId, CancellationToken cancellationToken = default)
        {
            if (tenantId == "system")
            {
                return (false, string.Empty);
            }

            var cliente = await _contextGestao.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Ativo && c.DeletadoEm == null, cancellationToken);

            // Sem cliente-assinante correspondente não há cota a impor (mesma postura das demais camadas).
            if (cliente == null || cliente.CotaPermissoes == null || cliente.CotaPermissoes <= 0)
            {
                return (false, string.Empty);
            }

            var limitePermissoes = cliente.CotaPermissoes.Value;

            var totalPapeis = await _contextGestao.Papeis
                .IgnoreQueryFilters()
                .CountAsync(p => p.TenantId == tenantId && p.DeletadoEm == null, cancellationToken);

            if (totalPapeis >= limitePermissoes)
            {
                return (true, $"O limite de papéis/permissões contratados em seu plano foi atingido ({totalPapeis}/{limitePermissoes}). Por favor, faça um upgrade para criar novos papéis.");
            }

            return (false, string.Empty);
        }
    }
}
