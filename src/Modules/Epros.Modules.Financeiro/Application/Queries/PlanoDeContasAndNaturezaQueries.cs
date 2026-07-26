using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Queries
{
    // ----- PLANO DE CONTAS FINANCEIRO QUERIES -----
    public record ListarPlanosDeContasFinanceiroQuery(
        string? Localizar = null,
        int Pagina = 1,
        int TamanhoPagina = 25
    ) : IRequest<CommandResult>;

    public class ListarPlanosDeContasFinanceiroQueryHandler : IRequestHandler<ListarPlanosDeContasFinanceiroQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public ListarPlanosDeContasFinanceiroQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarPlanosDeContasFinanceiroQuery request, CancellationToken cancellationToken)
        {
            var query = _context.PlanosDeContas.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(p => p.Descricao.Contains(request.Localizar) || p.Mascara.Contains(request.Localizar));

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(p => p.Descricao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(p => new
                {
                    p.Id,
                    p.SequenciaExibicao,
                    p.Descricao,
                    p.Mascara,
                    p.EhPadrao,
                    p.ConfiguracaoCodigoNaturezaFinanceiraRecebimentoId,
                    p.ConfiguracaoCodigoNaturezaFinanceiraPagamentoId
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public record ObterPlanoDeContasFinanceiroPorIdQuery(Guid Id) : IRequest<CommandResult>;

    public class ObterPlanoDeContasFinanceiroPorIdQueryHandler : IRequestHandler<ObterPlanoDeContasFinanceiroPorIdQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public ObterPlanoDeContasFinanceiroPorIdQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ObterPlanoDeContasFinanceiroPorIdQuery request, CancellationToken cancellationToken)
        {
            var plano = await _context.PlanosDeContas
                .AsNoTracking()
                .Where(p => p.Id == request.Id)
                .Select(p => new
                {
                    p.Id,
                    p.SequenciaExibicao,
                    p.Descricao,
                    p.Mascara,
                    p.EhPadrao,
                    p.ConfiguracaoCodigoNaturezaFinanceiraRecebimentoId,
                    p.ConfiguracaoCodigoNaturezaFinanceiraPagamentoId,
                    Empresas = p.Empresas.Where(e => e.DeletadoEm == null).Select(e => e.EmpresaId).ToList(),
                    Itens = _context.PlanoDeContasItens
                        .Where(i => i.PlanoDeContasFinanceiroId == p.Id)
                        .OrderBy(i => i.Codigo)
                        .Select(i => new
                        {
                            i.Id,
                            i.Codigo,
                            i.Descricao,
                            i.TipoDetalhamento,
                            i.MovimentaCaixa
                        }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (plano == null) return CommandResult.Falha("Plano de contas financeiro não encontrado.");
            return CommandResult.Ok("OK", plano);
        }
    }


    // ----- PLANO DE CONTAS FINANCEIRO ITEM QUERIES -----
    public record ListarPlanoDeContasFinanceiroItensQuery(
        Guid? PlanoDeContasFinanceiroId = null,
        ETipoDetalhamento? TipoDetalhamento = null,
        int Pagina = 1,
        int TamanhoPagina = 25
    ) : IRequest<CommandResult>;

    public class ListarPlanoDeContasFinanceiroItensQueryHandler : IRequestHandler<ListarPlanoDeContasFinanceiroItensQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public ListarPlanoDeContasFinanceiroItensQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarPlanoDeContasFinanceiroItensQuery request, CancellationToken cancellationToken)
        {
            var query = _context.PlanoDeContasItens.AsNoTracking().AsQueryable();

            if (request.PlanoDeContasFinanceiroId.HasValue)
                query = query.Where(i => i.PlanoDeContasFinanceiroId == request.PlanoDeContasFinanceiroId.Value);

            if (request.TipoDetalhamento.HasValue)
                query = query.Where(i => i.TipoDetalhamento == request.TipoDetalhamento.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(i => i.Codigo)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(i => new
                {
                    i.Id,
                    i.PlanoDeContasFinanceiroId,
                    i.Codigo,
                    i.Descricao,
                    i.TipoDetalhamento,
                    i.MovimentaCaixa
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public record ObterPlanoDeContasFinanceiroItemPorIdQuery(Guid Id) : IRequest<CommandResult>;

    public class ObterPlanoDeContasFinanceiroItemPorIdQueryHandler : IRequestHandler<ObterPlanoDeContasFinanceiroItemPorIdQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public ObterPlanoDeContasFinanceiroItemPorIdQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ObterPlanoDeContasFinanceiroItemPorIdQuery request, CancellationToken cancellationToken)
        {
            var item = await _context.PlanoDeContasItens
                .AsNoTracking()
                .Where(i => i.Id == request.Id)
                .Select(i => new
                {
                    i.Id,
                    i.PlanoDeContasFinanceiroId,
                    i.Codigo,
                    i.Descricao,
                    i.TipoDetalhamento,
                    i.MovimentaCaixa
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (item == null) return CommandResult.Falha("Item do plano de contas não encontrado.");
            return CommandResult.Ok("OK", item);
        }
    }


    // ----- CONFIGURACAO CODIGO NATUREZA FINANCEIRA QUERIES -----
    public record ListarConfiguracoesCodigoNaturezaFinanceiraQuery(
        string? Localizar = null,
        int Pagina = 1,
        int TamanhoPagina = 25
    ) : IRequest<CommandResult>;

    public class ListarConfiguracoesCodigoNaturezaFinanceiraQueryHandler : IRequestHandler<ListarConfiguracoesCodigoNaturezaFinanceiraQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public ListarConfiguracoesCodigoNaturezaFinanceiraQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarConfiguracoesCodigoNaturezaFinanceiraQuery request, CancellationToken cancellationToken)
        {
            var query = _context.NaturezasFinanceiras.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(c => c.Descricao.Contains(request.Localizar));

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(c => c.Descricao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(c => new
                {
                    c.Id,
                    c.SequenciaExibicao,
                    c.EmpresaId,
                    c.Descricao,
                    c.TipoConfiguracaoNatureza
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public record ObterConfiguracaoCodigoNaturezaFinanceiraPorIdQuery(Guid Id) : IRequest<CommandResult>;

    public class ObterConfiguracaoCodigoNaturezaFinanceiraPorIdQueryHandler : IRequestHandler<ObterConfiguracaoCodigoNaturezaFinanceiraPorIdQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public ObterConfiguracaoCodigoNaturezaFinanceiraPorIdQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ObterConfiguracaoCodigoNaturezaFinanceiraPorIdQuery request, CancellationToken cancellationToken)
        {
            var natureza = await _context.NaturezasFinanceiras
                .AsNoTracking()
                .Where(c => c.Id == request.Id)
                .Select(c => new
                {
                    c.Id,
                    c.SequenciaExibicao,
                    c.EmpresaId,
                    c.Descricao,
                    c.TipoConfiguracaoNatureza,
                    c.ItemPlanoDeContasFinanceiroDinheiroId,
                    c.ItemPlanoDeContasFinanceiroCartaoChequeId,
                    c.ItemPlanoDeContasFinanceiroCartaoCreditoId,
                    c.ItemPlanoDeContasFinanceiroCartaoDebitoId,
                    c.ItemPlanoDeContasFinanceiroCartaoDaLojaId,
                    c.ItemPlanoDeContasFinanceiroValeAlimentacaoId,
                    c.ItemPlanoDeContasFinanceiroValeRefeicaoId,
                    c.ItemPlanoDeContasFinanceiroValePresenteId,
                    c.ItemPlanoDeContasFinanceiroValeCombustivelId,
                    c.ItemPlanoDeContasFinanceiroDuplicataMercantilId,
                    c.ItemPlanoDeContasFinanceiroBoletoBancarioId,
                    c.ItemPlanoDeContasFinanceiroDepositoBancarioId,
                    c.ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamicoId,
                    c.ItemPlanoDeContasFinanceiroTransferenciaBancariaId,
                    c.ItemPlanoDeContasFinanceiroProgramaDeFidelidadeId,
                    c.ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstaticoId,
                    c.ItemPlanoDeContasFinanceiroCreditoEmLojaId,
                    c.ItemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformadoId,
                    c.ItemPlanoDeContasFinanceiroOutrosId,
                    c.ItemPlanoDeContasFinanceiroDescontoId,
                    c.ItemPlanoDeContasFinanceiroAcrescimoId,
                    c.ItemPlanoDeContasFinanceiroJurosId,
                    c.ItemPlanoDeContasFinanceiroMultaId,
                    c.ItemPlanoDeContasFinanceiroTrocoId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (natureza == null) return CommandResult.Falha("Configuração de natureza financeira não encontrada.");
            return CommandResult.Ok("OK", natureza);
        }
    }
}
