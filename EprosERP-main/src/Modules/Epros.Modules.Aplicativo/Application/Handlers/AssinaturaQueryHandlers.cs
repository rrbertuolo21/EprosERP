using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    public class ObterAssinaturaVigenteQueryHandler : IQueryHandler<ObterAssinaturaVigenteQuery, AssinaturaClienteDto>
    {
        private readonly ContextGestaoClientes _contextGestao;
        private readonly ITenantProvider _tenantProvider;

        public ObterAssinaturaVigenteQueryHandler(ContextGestaoClientes contextGestao, ITenantProvider tenantProvider)
        {
            _contextGestao = contextGestao;
            _tenantProvider = tenantProvider;
        }

        public async Task<AssinaturaClienteDto> Handle(ObterAssinaturaVigenteQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            var cliente = await _contextGestao.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Ativo && c.DeletadoEm == null, cancellationToken);

            if (cliente == null) return null!;

            var assinatura = await _contextGestao.AssinaturasClientes
                .IgnoreQueryFilters()
                .Where(a => a.ClienteId == cliente.Id && a.DeletadoEm == null && !a.Arquivada)
                .OrderByDescending(a => a.CriadoEm)
                .FirstOrDefaultAsync(cancellationToken);

            if (assinatura == null) return null!;

            return new AssinaturaClienteDto(
                Id: assinatura.Id,
                ClienteId: assinatura.ClienteId,
                PlanoId: assinatura.PlanoId,
                Status: assinatura.Status.ToString(),
                DataInicio: assinatura.DataInicio,
                DataFim: assinatura.DataFim,
                TrialAte: assinatura.TrialAte,
                MetodoPagamento: assinatura.MetodoPagamento,
                TransacaoId: assinatura.TransacaoId,
                DetalhesPacoteJson: assinatura.DetalhesPacoteJson,
                Arquivada: assinatura.Arquivada
            );
        }
    }

    public class ListarPlanosPublicosQueryHandler : IQueryHandler<ListarPlanosPublicosQuery, IEnumerable<PlanoPublicoDto>>
    {
        private readonly ContextGestaoClientes _contextGestao;

        public ListarPlanosPublicosQueryHandler(ContextGestaoClientes contextGestao)
        {
            _contextGestao = contextGestao;
        }

        public async Task<IEnumerable<PlanoPublicoDto>> Handle(ListarPlanosPublicosQuery request, CancellationToken cancellationToken)
        {
            var planos = await _contextGestao.Planos
                .IgnoreQueryFilters()
                .Include(p => p.Modulos)
                .Where(p => p.Ativo && p.DeletadoEm == null)
                .OrderBy(p => p.Preco)
                .ToListAsync(cancellationToken);

            return planos.Select(p => new PlanoPublicoDto(
                Id: p.Id,
                Nome: p.Nome,
                Preco: p.Preco,
                LimiteUsuarios: p.LimiteUsuarios,
                LimiteEmpresas: p.LimiteEmpresas,
                RecursosInclusos: p.RecursosInclusos,
                Modulos: p.Modulos.Where(m => m.DeletadoEm == null).Select(m => m.NomeModulo).ToList()
            )).ToList();
        }
    }

    public class ListarFaturasTenantQueryHandler : IQueryHandler<ListarFaturasTenantQuery, IEnumerable<FaturaTenantDto>>
    {
        private readonly ContextGestaoClientes _contextGestao;
        private readonly ITenantProvider _tenantProvider;

        public ListarFaturasTenantQueryHandler(ContextGestaoClientes contextGestao, ITenantProvider tenantProvider)
        {
            _contextGestao = contextGestao;
            _tenantProvider = tenantProvider;
        }

        public async Task<IEnumerable<FaturaTenantDto>> Handle(ListarFaturasTenantQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            var cliente = await _contextGestao.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Ativo && c.DeletadoEm == null, cancellationToken);

            if (cliente == null) return Enumerable.Empty<FaturaTenantDto>();

            var faturas = await _contextGestao.Faturas
                .IgnoreQueryFilters()
                .Where(f => f.ClienteId == cliente.Id && f.DeletadoEm == null)
                .OrderByDescending(f => f.DataVencimento)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return faturas.Select(f => new FaturaTenantDto(
                Id: f.Id,
                ClienteId: f.ClienteId,
                Valor: f.Valor,
                DataVencimento: f.DataVencimento,
                DataPagamento: f.DataPagamento,
                Status: f.Status.ToString()
            )).ToList();
        }
    }
}
