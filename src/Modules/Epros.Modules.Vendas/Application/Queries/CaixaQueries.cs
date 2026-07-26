using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Infrastructure.Data;

namespace Epros.Modules.Vendas.Application.Queries
{
    public record ObterCaixaStatusQuery(string OperadorId) : IQuery<CommandResult>;

    public record ObterCaixaDetalhadoQuery(Guid CaixaId) : IQuery<CommandResult>;

    public class ObterCaixaStatusQueryHandler : IRequestHandler<ObterCaixaStatusQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterCaixaStatusQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterCaixaStatusQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            var caixa = await _context.Caixas
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.TenantId == tenantId &&
                                          c.OperadorId == request.OperadorId &&
                                          c.Status == ECaixaStatus.Aberto, cancellationToken);

            if (caixa == null)
            {
                return CommandResult.Ok("Fechado", new { Status = "Fechado" });
            }

            return CommandResult.Ok("Aberto", new
            {
                Status = "Aberto",
                caixa.Id,
                caixa.OperadorId,
                caixa.SaldoAbertura,
                caixa.CriadoEm
            });
        }
    }

    public class ObterCaixaDetalhadoQueryHandler : IRequestHandler<ObterCaixaDetalhadoQuery, CommandResult>
    {
        private readonly ContextVendas _context;

        public ObterCaixaDetalhadoQueryHandler(ContextVendas context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterCaixaDetalhadoQuery request, CancellationToken cancellationToken)
        {
            var caixa = await _context.Caixas
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CaixaId, cancellationToken);

            if (caixa == null)
            {
                return CommandResult.Falha("Caixa não encontrado.");
            }

            // Somar vendas no dinheiro (formaPagamento == "Dinheiro" ou similar)
            var vendasTotal = await _context.Vendas
                .AsNoTracking()
                .Where(v => v.CaixaId == request.CaixaId.ToString() && !v.Cancelada)
                .SumAsync(v => v.Total, cancellationToken);

            // Somar vendas especificamente em Dinheiro para o saldo de dinheiro
            var vendasDinheiro = await _context.Vendas
                .AsNoTracking()
                .Where(v => v.CaixaId == request.CaixaId.ToString() && !v.Cancelada && v.FormaPagamento == "Dinheiro")
                .SumAsync(v => v.Total, cancellationToken);

            // Somar suprimentos e sangrias
            var movimentos = await _context.CaixaMovimentos
                .AsNoTracking()
                .Where(m => m.CaixaId == request.CaixaId)
                .ToListAsync(cancellationToken);

            var suprimentos = movimentos.Where(m => m.Tipo == "Suprimento").Sum(m => m.Valor);
            var sangrias = movimentos.Where(m => m.Tipo == "Sangria").Sum(m => m.Valor);

            var saldoCalculadoDinheiro = caixa.SaldoAbertura + vendasDinheiro + suprimentos - sangrias;

            return CommandResult.Ok("OK", new
            {
                caixa.Id,
                caixa.OperadorId,
                caixa.Status,
                caixa.SaldoAbertura,
                caixa.SaldoFechamento,
                caixa.DiferencaFechamento,
                caixa.FechadoEm,
                caixa.CriadoEm,
                TotalVendas = vendasTotal,
                TotalVendasDinheiro = vendasDinheiro,
                TotalSuprimentos = suprimentos,
                TotalSangrias = sangrias,
                SaldoCalculadoDinheiro = saldoCalculadoDinheiro,
                Movimentos = movimentos.Select(m => new
                {
                    m.Id,
                    m.Tipo,
                    m.Valor,
                    m.Observacao,
                    m.CriadoEm
                })
            });
        }
    }
}
