using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    // ===== Commands =====

    /// <summary>Altera vencimento/valor de fatura em aberto.</summary>
    public class AlterarFaturaCommandHandler : ICommandHandler<AlterarFaturaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;

        public AlterarFaturaCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AlterarFaturaCommand request, CancellationToken cancellationToken)
        {
            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var fatura = await _context.Faturas.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
            if (fatura == null)
            {
                return CommandResult.Falha(new[] { "Fatura não encontrada." }, "Erro");
            }

            if (fatura.Status == FaturaStatus.Paga)
            {
                return CommandResult.Falha(new[] { "Fatura já paga não pode ser alterada." }, "Erro");
            }

            fatura.Alterar(request.Valor, request.DataVencimento, alteradoPor);

            if (!fatura.IsValid)
            {
                return CommandResult.Falha(fatura.Notifications.Select(n => n.Message), "Falha na validação da fatura");
            }

            _context.Faturas.Update(fatura);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Fatura alterada com sucesso!", new { FaturaId = fatura.Id });
        }
    }

    /// <summary>Baixa manual de fatura com valor/forma/data e registro de PagamentoFatura.</summary>
    public class BaixarFaturaManualCommandHandler : ICommandHandler<BaixarFaturaManualCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;

        public BaixarFaturaManualCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(BaixarFaturaManualCommand request, CancellationToken cancellationToken)
        {
            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var fatura = await _context.Faturas.FirstOrDefaultAsync(f => f.Id == request.FaturaId, cancellationToken);
            if (fatura == null)
            {
                return CommandResult.Falha(new[] { "Fatura não encontrada." }, "Erro");
            }

            if (fatura.Status == FaturaStatus.Paga)
            {
                return CommandResult.Falha(new[] { "Fatura já está paga." }, "Erro");
            }

            var dataPagamento = request.DataPagamento ?? DateTime.UtcNow;

            // Cálculo do split de comissão (mesmo critério do BaixarFaturaCommandHandler).
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == fatura.ClienteId, cancellationToken);
            decimal percentualRevenda = 0;
            decimal percentualVendedor = 0;

            if (cliente != null)
            {
                if (cliente.RevendaId.HasValue)
                {
                    var revenda = await _context.Revendas.FirstOrDefaultAsync(r => r.Id == cliente.RevendaId.Value && r.Ativo, cancellationToken);
                    if (revenda != null) percentualRevenda = revenda.PercentualComissao;
                }

                if (cliente.VendedorId.HasValue)
                {
                    var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.Id == cliente.VendedorId.Value && v.Ativo, cancellationToken);
                    if (vendedor != null) percentualVendedor = vendedor.PercentualComissao;
                }
            }

            fatura.CalcularSplitComissao(percentualRevenda, percentualVendedor);
            fatura.BaixarManual(dataPagamento, alteradoPor);

            if (!fatura.IsValid)
            {
                return CommandResult.Falha(fatura.Notifications.Select(n => n.Message), "Falha na validação da fatura");
            }

            // Registra o pagamento manual.
            var pagamento = new PagamentoFatura(
                fatura.Id,
                request.FormaPagamento,
                PagamentoFaturaStatus.Paid,
                request.ValorPago,
                null,
                null,
                pagoManualmente: true,
                dataPagamento: dataPagamento,
                tenantId: fatura.TenantId,
                criadoPor: alteradoPor
            );

            if (!pagamento.IsValid)
            {
                return CommandResult.Falha(pagamento.Notifications.Select(n => n.Message), "Falha na validação do pagamento");
            }

            _context.PagamentosFaturas.Add(pagamento);
            _context.Faturas.Update(fatura);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Fatura baixada manualmente com sucesso!", new { FaturaId = fatura.Id, PagamentoId = pagamento.Id });
        }
    }

    /// <summary>Exclui (soft-delete) fatura.</summary>
    public class ExcluirFaturaCommandHandler : ICommandHandler<ExcluirFaturaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;

        public ExcluirFaturaCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ExcluirFaturaCommand request, CancellationToken cancellationToken)
        {
            var deletadoPor = _currentUser.GetUserId() ?? "system";

            var fatura = await _context.Faturas.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
            if (fatura == null)
            {
                return CommandResult.Falha(new[] { "Fatura não encontrada." }, "Erro");
            }

            fatura.Deletar(deletadoPor);
            _context.Faturas.Update(fatura);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Fatura excluída com sucesso!");
        }
    }

    // ===== Queries =====

    /// <summary>Lista faturas (landlord) com filtro por cliente/status e paginação.</summary>
    public class ListarFaturasQueryHandler : IQueryHandler<ListarFaturasQuery, PagedQueryResult<FaturaListaDto>>
    {
        private readonly ContextGestaoClientes _context;

        public ListarFaturasQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<PagedQueryResult<FaturaListaDto>> Handle(ListarFaturasQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Faturas.AsQueryable();

            if (request.ClienteId.HasValue && request.ClienteId.Value != Guid.Empty)
            {
                query = query.Where(f => f.ClienteId == request.ClienteId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Status)
                && Enum.TryParse<FaturaStatus>(request.Status, ignoreCase: true, out var status))
            {
                query = query.Where(f => f.Status == status);
            }

            var totalRegistros = await query.CountAsync(cancellationToken);
            var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)request.TamanhoPagina);

            var items = await query
                .OrderByDescending(f => f.DataVencimento)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(f => new FaturaListaDto
                {
                    Id = f.Id,
                    ClienteId = f.ClienteId,
                    ClienteRazaoSocial = _context.Clientes
                        .Where(c => c.Id == f.ClienteId)
                        .Select(c => c.RazaoSocial)
                        .FirstOrDefault() ?? string.Empty,
                    Valor = f.Valor,
                    DataVencimento = f.DataVencimento,
                    DataPagamento = f.DataPagamento,
                    Status = f.Status.ToString(),
                    CriadoEm = f.CriadoEm
                })
                .ToListAsync(cancellationToken);

            return new PagedQueryResult<FaturaListaDto>(items, totalRegistros, totalPaginas);
        }
    }

    /// <summary>Obtém fatura por Id (com pagamentos).</summary>
    public class ObterFaturaPorIdQueryHandler : IQueryHandler<ObterFaturaPorIdQuery, FaturaDetalhadaDto>
    {
        private readonly ContextGestaoClientes _context;

        public ObterFaturaPorIdQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<FaturaDetalhadaDto> Handle(ObterFaturaPorIdQuery request, CancellationToken cancellationToken)
        {
            var f = await _context.Faturas.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (f == null) return null!;

            var razaoSocial = await _context.Clientes
                .Where(c => c.Id == f.ClienteId)
                .Select(c => c.RazaoSocial)
                .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

            var pagamentos = await _context.PagamentosFaturas
                .Where(p => p.FaturaId == f.Id)
                .OrderByDescending(p => p.CriadoEm)
                .Select(p => new PagamentoFaturaDto
                {
                    Id = p.Id,
                    TipoPagamento = p.TipoPagamento,
                    Status = p.Status.ToString(),
                    ValorPago = p.ValorPago,
                    PagoManualmente = p.PagoManualmente,
                    DataPagamento = p.DataPagamento
                })
                .ToListAsync(cancellationToken);

            var itens = await _context.Set<FaturaItem>()
                .Where(i => i.FaturaId == f.Id)
                .Select(i => new FaturaItemDto { Id = i.Id, Descricao = i.Descricao, Valor = i.Valor })
                .ToListAsync(cancellationToken);

            return new FaturaDetalhadaDto
            {
                Id = f.Id,
                ClienteId = f.ClienteId,
                ClienteRazaoSocial = razaoSocial,
                Valor = f.Valor,
                DataVencimento = f.DataVencimento,
                DataPagamento = f.DataPagamento,
                Status = f.Status.ToString(),
                PercentualComissaoRevenda = f.PercentualComissaoRevenda,
                PercentualComissaoVendedor = f.PercentualComissaoVendedor,
                ValorComissaoRevenda = f.ValorComissaoRevenda,
                ValorComissaoVendedor = f.ValorComissaoVendedor,
                Quitada = f.Quitada,
                ValorPago = f.ValorPago ?? 0m,
                Numero = f.Numero,
                Observacoes = f.Observacoes,
                CriadoEm = f.CriadoEm,
                Itens = itens,
                Pagamentos = pagamentos
            };
        }
    }
}
