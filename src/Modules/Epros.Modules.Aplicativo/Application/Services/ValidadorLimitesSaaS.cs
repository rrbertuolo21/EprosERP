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

            // Regra: Menor ou igual a zero representa Ilimitado
            if (plano.LimiteUsuarios <= 0)
            {
                return (false, string.Empty);
            }

            var totalUsuarios = await _contextApp.Usuarios
                .CountAsync(u => u.TenantId == tenantId && u.DeletadoEm == null && u.Status == UsuarioStatus.Active, cancellationToken);

            if (totalUsuarios >= plano.LimiteUsuarios)
            {
                return (true, $"O limite de usuários ativos contratados em seu plano foi atingido ({totalUsuarios}/{plano.LimiteUsuarios}). Por favor, faça um upgrade para adicionar novos usuários.");
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

            // Regra: Menor ou igual a zero representa Ilimitado
            if (plano.LimiteEmpresas <= 0)
            {
                return (false, string.Empty);
            }

            var totalEmpresas = await _contextGestao.Empresas
                .CountAsync(e => e.TenantId == tenantId && e.DeletadoEm == null, cancellationToken);

            if (totalEmpresas >= plano.LimiteEmpresas)
            {
                return (true, $"O limite de empresas cadastradas contratadas em seu plano foi atingido ({totalEmpresas}/{plano.LimiteEmpresas}). Por favor, faça um upgrade para adicionar novas empresas.");
            }

            return (false, string.Empty);
        }
    }
}
