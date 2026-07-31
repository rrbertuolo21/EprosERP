using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    public class ObterDashboardGlobalQueryHandler : IQueryHandler<ObterDashboardGlobalQuery, DashboardGlobalDto>
    {
        private readonly ContextAplicativo _context;
        private readonly ContextGestaoClientes _contextGestao;
        private readonly ITenantProvider _tenantProvider;

        public ObterDashboardGlobalQueryHandler(
            ContextAplicativo context,
            ContextGestaoClientes contextGestao,
            ITenantProvider tenantProvider)
        {
            _context = context;
            _contextGestao = contextGestao;
            _tenantProvider = tenantProvider;
        }

        public async Task<DashboardGlobalDto> Handle(ObterDashboardGlobalQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                throw new UnauthorizedAccessException("Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser).");
            }

            // Realiza aggregations do módulo GestaoClientes (ignorando filtros RLS para visão consolidada global)
            var totalTenants = await _contextGestao.Clientes
                .IgnoreQueryFilters()
                .CountAsync(c => c.DeletadoEm == null, cancellationToken);

            var totalAssinaturasAtivas = await _contextGestao.AssinaturasClientes
                .IgnoreQueryFilters()
                .CountAsync(a => a.Status == AssinaturaStatus.Ativa && a.DeletadoEm == null, cancellationToken);

            var activeSubscribers = await _contextGestao.AssinaturasClientes
                .IgnoreQueryFilters()
                .Where(a => a.Status == AssinaturaStatus.Ativa && a.DeletadoEm == null)
                .Select(a => a.ClienteId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var mrr = await _contextGestao.Contratos
                .IgnoreQueryFilters()
                .Where(c => activeSubscribers.Contains(c.ClienteId) && c.Ativo && c.DeletadoEm == null)
                .SumAsync(c => c.ValorRecorrente, cancellationToken);

            var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var novasAssinaturasMes = await _contextGestao.AssinaturasClientes
                .IgnoreQueryFilters()
                .CountAsync(a => a.Status == AssinaturaStatus.Ativa && a.CriadoEm >= inicioMes && a.DeletadoEm == null, cancellationToken);

            var totalAtivas = await _contextGestao.AssinaturasClientes
                .IgnoreQueryFilters()
                .CountAsync(a => a.Status == AssinaturaStatus.Ativa && a.DeletadoEm == null, cancellationToken);

            var canceladas30Dias = await _contextGestao.AssinaturasClientes
                .IgnoreQueryFilters()
                .CountAsync(a => (a.Status == AssinaturaStatus.Expirada || a.Status == AssinaturaStatus.Recusada)
                            && a.AlteradoEm >= DateTime.UtcNow.AddDays(-30) && a.DeletadoEm == null, cancellationToken);

            decimal churnRate = 0.0m;
            if (totalAtivas + canceladas30Dias > 0)
            {
                churnRate = Math.Round(((decimal)canceladas30Dias * 100m) / (totalAtivas + canceladas30Dias), 2);
            }

            var receitaTotal = await _contextGestao.Faturas
                .IgnoreQueryFilters()
                .Where(f => f.Status == FaturaStatus.Paga && f.DeletadoEm == null)
                .SumAsync(f => f.Valor, cancellationToken);

            return new DashboardGlobalDto(
                TotalTenants: totalTenants,
                TotalAssinaturasAtivas: totalAssinaturasAtivas,
                ReceitaEstimadaMRR: mrr,
                NovasAssinaturasMes: novasAssinaturasMes,
                ChurnRate: churnRate,
                ReceitaTotal: receitaTotal
            );
        }
    }

    public class ListarUsuariosInternosQueryHandler : IQueryHandler<ListarUsuariosInternosQuery, IEnumerable<UsuarioInternoDto>>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarUsuariosInternosQueryHandler(ContextAplicativo context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<IEnumerable<UsuarioInternoDto>> Handle(ListarUsuariosInternosQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                throw new UnauthorizedAccessException("Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser).");
            }

            var usuarios = await _context.UsuariosInternos
                .Where(u => u.DeletadoEm == null)
                .Select(u => new UsuarioInternoDto(
                    u.Id,
                    u.Nome,
                    u.Email,
                    u.Timezone ?? string.Empty,
                    u.PrimaryAdmin
                ))
                .ToListAsync(cancellationToken);

            return usuarios;
        }
    }

    public class ListarSystemSettingsQueryHandler : IQueryHandler<ListarSystemSettingsQuery, IEnumerable<SystemSettingDto>>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarSystemSettingsQueryHandler(ContextAplicativo context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<IEnumerable<SystemSettingDto>> Handle(ListarSystemSettingsQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                throw new UnauthorizedAccessException("Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser).");
            }

            var settings = await _context.SystemSettings
                .Where(s => s.DeletadoEm == null)
                .Select(s => new SystemSettingDto(
                    s.Id,
                    s.Chave,
                    s.Valor,
                    s.Escopo,
                    s.EhSegredo
                ))
                .ToListAsync(cancellationToken);

            return settings;
        }
    }

    public class ListarExecucoesMassaGlobalQueryHandler : IQueryHandler<ListarExecucoesMassaGlobalQuery, IEnumerable<ExecucaoMassaGlobalDto>>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarExecucoesMassaGlobalQueryHandler(ContextAplicativo context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<IEnumerable<ExecucaoMassaGlobalDto>> Handle(ListarExecucoesMassaGlobalQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                throw new UnauthorizedAccessException("Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser).");
            }

            var execucoes = await _context.ExecucoesMassaGlobal
                .Where(e => e.DeletadoEm == null)
                .Select(e => new ExecucaoMassaGlobalDto(
                    e.Id,
                    e.Descricao,
                    e.ActionPayload,
                    e.Status,
                    e.AprovadoPor,
                    e.CriadoPor ?? string.Empty,
                    e.CriadoEm
                ))
                .ToListAsync(cancellationToken);

            return execucoes;
        }
    }

    public class ListarCustomPagesQueryHandler : IQueryHandler<ListarCustomPagesQuery, IEnumerable<CustomPageDto>>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarCustomPagesQueryHandler(ContextAplicativo context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<IEnumerable<CustomPageDto>> Handle(ListarCustomPagesQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                throw new UnauthorizedAccessException("Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser).");
            }

            var paginas = await _context.CustomPages
                .Where(p => p.DeletadoEm == null)
                .Select(p => new CustomPageDto(
                    p.Id,
                    p.Slug,
                    p.Status,
                    p.Conteudo
                ))
                .ToListAsync(cancellationToken);

            return paginas;
        }
    }

    public class ListarNewsletterSubscribersQueryHandler : IQueryHandler<ListarNewsletterSubscribersQuery, IEnumerable<NewsletterSubscriberDto>>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarNewsletterSubscribersQueryHandler(ContextAplicativo context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<IEnumerable<NewsletterSubscriberDto>> Handle(ListarNewsletterSubscribersQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                throw new UnauthorizedAccessException("Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser).");
            }

            var subscribers = await _context.NewsletterSubscribers
                .Where(n => n.DeletadoEm == null)
                .Select(n => new NewsletterSubscriberDto(
                    n.Id,
                    n.Email,
                    n.Ativo,
                    n.CriadoEm
                ))
                .ToListAsync(cancellationToken);

            return subscribers;
        }
    }

    public class ListarClientesQueryHandler : IQueryHandler<ListarClientesQuery, IEnumerable<SuperAdminClienteDto>>
    {
        private readonly ContextGestaoClientes _contextGestao;
        private readonly ITenantProvider _tenantProvider;

        public ListarClientesQueryHandler(ContextGestaoClientes contextGestao, ITenantProvider tenantProvider)
        {
            _contextGestao = contextGestao;
            _tenantProvider = tenantProvider;
        }

        public async Task<IEnumerable<SuperAdminClienteDto>> Handle(ListarClientesQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                throw new UnauthorizedAccessException("Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser).");
            }

            var planList = await _contextGestao.Planos.IgnoreQueryFilters().ToListAsync(cancellationToken);
            var clientes = await _contextGestao.Clientes
                .IgnoreQueryFilters()
                .Where(c => c.DeletadoEm == null)
                .ToListAsync(cancellationToken);

            var resultado = new List<SuperAdminClienteDto>();
            foreach (var c in clientes)
            {
                var plano = planList.FirstOrDefault(p => p.Id == c.PlanoId);
                resultado.Add(new SuperAdminClienteDto(
                    c.Id,
                    c.TenantId,
                    c.RazaoSocial,
                    c.Cnpj,
                    c.Email,
                    plano?.Nome ?? "Plano Desconhecido",
                    c.StatusSaaS.ToString(),
                    c.Ativo
                ));
            }

            return resultado;
        }
    }
}
