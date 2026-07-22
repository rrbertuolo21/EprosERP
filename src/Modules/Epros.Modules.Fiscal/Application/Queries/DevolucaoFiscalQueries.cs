using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Domain.Enums;
using Epros.Modules.Fiscal.Infrastructure.Data;

namespace Epros.Modules.Fiscal.Application.Queries
{
    /// <summary>Listagem paginada de devoluções fiscais (EF_DEVOLUCAO_FISCAL 8.7). Filtro por estado.</summary>
    public record ListarDevolucoesFiscaisQuery(
        int? Estado = null,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IQuery<CommandResult>;

    public record ObterDevolucaoFiscalPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarDevolucoesFiscaisQueryHandler : IRequestHandler<ListarDevolucoesFiscaisQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ListarDevolucoesFiscaisQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ListarDevolucoesFiscaisQuery request, CancellationToken cancellationToken)
        {
            var query = _context.DevolucoesFiscais
                .AsNoTracking()
                .Where(d => d.DeletadoEm == null);

            if (request.Estado.HasValue)
            {
                var estado = (EEstadoDevolucaoFiscal)request.Estado.Value;
                query = query.Where(d => d.Estado == estado);
            }

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(d => d.DataCriacao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(d => new
                {
                    d.Id,
                    Estado = d.Estado.ToString(),
                    EstadoCodigo = (int)d.Estado,
                    d.Modelo,
                    d.Serie,
                    d.ChaveNfEntrada,
                    d.ChaveGerada,
                    d.NumeroGerado,
                    d.DestinatarioNome,
                    d.Total,
                    d.DataCriacao,
                    d.DataTransmissao
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterDevolucaoFiscalPorIdQueryHandler : IRequestHandler<ObterDevolucaoFiscalPorIdQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ObterDevolucaoFiscalPorIdQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ObterDevolucaoFiscalPorIdQuery request, CancellationToken cancellationToken)
        {
            var devolucao = await _context.DevolucoesFiscais
                .AsNoTracking()
                .Include(d => d.Itens)
                .FirstOrDefaultAsync(d => d.Id == request.Id && d.DeletadoEm == null, cancellationToken);

            if (devolucao is null)
                return CommandResult.Falha("Devolução fiscal não localizada.");

            return CommandResult.Ok("OK", new
            {
                devolucao.Id,
                Estado = devolucao.Estado.ToString(),
                EstadoCodigo = (int)devolucao.Estado,
                devolucao.Modelo,
                devolucao.Ambiente,
                devolucao.Serie,
                devolucao.ChaveNfEntrada,
                devolucao.ChaveGerada,
                devolucao.NumeroGerado,
                devolucao.Protocolo,
                devolucao.Motivo,
                devolucao.MensagemRetorno,
                devolucao.DestinatarioCnpjCpf,
                devolucao.DestinatarioNome,
                devolucao.Total,
                devolucao.EmpresaId,
                devolucao.DocumentoOrigemId,
                devolucao.DataCriacao,
                devolucao.DataTransmissao,
                Itens = devolucao.Itens.Select(i => new
                {
                    i.Id,
                    i.ProdutoId,
                    i.Sku,
                    i.NomeProduto,
                    i.Ncm,
                    i.Cfop,
                    i.Cst,
                    i.Quantidade,
                    i.ValorUnitario,
                    i.ValorTotal,
                    i.AliquotaIcms
                })
            });
        }
    }
}
