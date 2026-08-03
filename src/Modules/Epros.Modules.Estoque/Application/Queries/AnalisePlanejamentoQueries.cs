using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Queries
{
    /// <summary>APE-009: consulta posições com disponível = saldo − reservado e status de planejamento.</summary>
    public record ConsultarPosicaoEstoqueQuery(Guid? ProdutoId = null, bool ApenasEmAlerta = false, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public record ListarAlertasEstoqueQuery(EStatusAlertaEstoque? Status = null, ETipoAlertaEstoque? Tipo = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public class ConsultarPosicaoEstoqueQueryHandler : IRequestHandler<ConsultarPosicaoEstoqueQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ConsultarPosicaoEstoqueQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ConsultarPosicaoEstoqueQuery request, CancellationToken cancellationToken)
        {
            var query = _context.EstoqueProdutos.AsNoTracking().AsQueryable();
            if (request.ProdutoId.HasValue) query = query.Where(p => p.ProdutoId == request.ProdutoId.Value);

            var registros = await query
                .Select(p => new
                {
                    p.Id,
                    p.EmpresaId,
                    p.ProdutoId,
                    p.QuantidadeSaldoEstoque,
                    p.QuantidadeEstoqueMinimo,
                    p.QuantidadeEstoqueMaximo,
                    p.QuantidadeEstoqueReservado,
                    p.ValorSaldo,
                    p.ValorCustoMedio,
                    p.TipoCusteioEstoque
                })
                .ToListAsync(cancellationToken);

            // APE-009: disponível = saldo − reservado. Status de planejamento derivado (§11).
            var projetadas = registros.Select(p =>
            {
                var disponivel = p.QuantidadeSaldoEstoque - p.QuantidadeEstoqueReservado;
                EStatusPlanejamentoEstoque status;
                if (p.QuantidadeEstoqueMinimo <= 0m && p.QuantidadeEstoqueMaximo <= 0m) status = EStatusPlanejamentoEstoque.SemPoliticaCompleta;
                else if (p.QuantidadeEstoqueMinimo > 0m && p.QuantidadeSaldoEstoque <= p.QuantidadeEstoqueMinimo) status = EStatusPlanejamentoEstoque.EmAlertaReposicao;
                else if (p.QuantidadeEstoqueMaximo > 0m && p.QuantidadeSaldoEstoque > p.QuantidadeEstoqueMaximo) status = EStatusPlanejamentoEstoque.AcimaMaximo;
                else status = EStatusPlanejamentoEstoque.Normal;
                return new
                {
                    p.Id,
                    p.EmpresaId,
                    p.ProdutoId,
                    p.QuantidadeSaldoEstoque,
                    p.QuantidadeEstoqueMinimo,
                    p.QuantidadeEstoqueMaximo,
                    p.QuantidadeEstoqueReservado,
                    QuantidadeDisponivel = disponivel,
                    p.ValorSaldo,
                    p.ValorCustoMedio,
                    p.TipoCusteioEstoque,
                    StatusPlanejamento = status
                };
            });

            if (request.ApenasEmAlerta)
                projetadas = projetadas.Where(p => p.StatusPlanejamento == EStatusPlanejamentoEstoque.EmAlertaReposicao || p.StatusPlanejamento == EStatusPlanejamentoEstoque.AcimaMaximo);

            var lista = projetadas.ToList();
            var total = lista.Count;
            var pagina = lista.Skip((request.Pagina - 1) * request.TamanhoPagina).Take(request.TamanhoPagina).ToList();

            return CommandResult.Ok("OK", new { Total = total, request.Pagina, Itens = pagina });
        }
    }

    public class ListarAlertasEstoqueQueryHandler : IRequestHandler<ListarAlertasEstoqueQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ListarAlertasEstoqueQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarAlertasEstoqueQuery request, CancellationToken cancellationToken)
        {
            var query = _context.AlertasEstoque.AsNoTracking().Where(a => a.DeletadoEm == null);
            if (request.Status.HasValue) query = query.Where(a => a.StatusAlerta == request.Status.Value);
            if (request.Tipo.HasValue) query = query.Where(a => a.TipoAlerta == request.Tipo.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(a => a.DataAlerta)
                .Skip((request.Pagina - 1) * request.TamanhoPagina).Take(request.TamanhoPagina)
                .Select(a => new { a.Id, a.EmpresaId, a.ProdutoId, a.TipoAlerta, a.QuantidadeReferencia, a.QuantidadeAtual, a.StatusAlerta, a.DataAlerta })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, request.Pagina, Itens = itens });
        }
    }
}
