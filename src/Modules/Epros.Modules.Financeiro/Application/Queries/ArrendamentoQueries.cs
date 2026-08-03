using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Domain.Services;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Queries
{
    public record ListarContratosArrendamentoQuery(int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;
    public record ObterContratoArrendamentoPorIdQuery(Guid Id) : IRequest<CommandResult>;
    /// <summary>Deriva o cronograma IFRS-16 (amortização do passivo + depreciação do direito de uso) sob demanda.</summary>
    public record ObterCronogramaArrendamentoQuery(Guid Id) : IRequest<CommandResult>;

    public class ArrendamentoQueryHandlers :
        IRequestHandler<ListarContratosArrendamentoQuery, CommandResult>,
        IRequestHandler<ObterContratoArrendamentoPorIdQuery, CommandResult>,
        IRequestHandler<ObterCronogramaArrendamentoQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public ArrendamentoQueryHandlers(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarContratosArrendamentoQuery request, CancellationToken ct)
        {
            var pagina = request.Pagina <= 0 ? 1 : request.Pagina;
            var tamanho = request.TamanhoPagina is <= 0 or > 100 ? 20 : request.TamanhoPagina;
            var query = _context.ContratosArrendamento.AsNoTracking();
            var total = await query.CountAsync(ct);
            var itens = await query.OrderByDescending(c => c.DataInicio)
                .Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(c => new
                {
                    c.Id,
                    c.Descricao,
                    c.PessoaId,
                    c.DataInicio,
                    c.ValorContraprestacao,
                    c.QuantidadeParcelas,
                    c.TaxaIncrementalPeriodo,
                    c.PassivoArrendamentoInicial,
                    c.DireitoDeUsoInicial,
                    c.Status
                }).ToListAsync(ct);
            return CommandResult.Ok("Arrendamentos listados.", new { total, pagina, tamanho, itens });
        }

        public async Task<CommandResult> Handle(ObterContratoArrendamentoPorIdQuery request, CancellationToken ct)
        {
            var c = await _context.ContratosArrendamento.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (c == null) return CommandResult.Falha("Arrendamento não encontrado.");
            return CommandResult.Ok("Arrendamento localizado.", new
            {
                c.Id,
                c.Descricao,
                c.PessoaId,
                c.DataInicio,
                c.ValorContraprestacao,
                c.QuantidadeParcelas,
                c.TaxaIncrementalPeriodo,
                c.PagamentoAntecipado,
                c.CustosDiretosIniciais,
                c.IncentivosRecebidos,
                c.PassivoArrendamentoInicial,
                c.DireitoDeUsoInicial,
                c.Status
            });
        }

        public async Task<CommandResult> Handle(ObterCronogramaArrendamentoQuery request, CancellationToken ct)
        {
            var c = await _context.ContratosArrendamento.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (c == null) return CommandResult.Falha("Arrendamento não encontrado.");

            var cronograma = CalculoLeasing.Cronograma(
                c.ValorContraprestacao, c.TaxaIncrementalPeriodo, c.QuantidadeParcelas, c.PagamentoAntecipado,
                c.CustosDiretosIniciais, 0m, c.IncentivosRecebidos);

            return CommandResult.Ok("Cronograma de arrendamento (IFRS-16) derivado.", new
            {
                c.Id,
                c.PassivoArrendamentoInicial,
                c.DireitoDeUsoInicial,
                TotalJuros = CalculoLeasing.TotalJuros(cronograma),
                Parcelas = cronograma.Select(l => new
                {
                    l.Numero,
                    l.Pagamento,
                    l.Juros,
                    l.AmortizacaoPrincipal,
                    l.SaldoPassivo,
                    l.DepreciacaoDireitoUso,
                    l.SaldoDireitoUso
                })
            });
        }
    }
}
