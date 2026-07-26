using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Queries
{
    public record ListarSacadosQuery(int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;
    public record ListarFaturasCobrancaQuery(Guid? SacadoId, ESituacaoFaturaCobranca? Situacao, int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;
    public record ListarContasEmissorasQuery() : IRequest<CommandResult>;
    public record ListarGruposRecorrenciaQuery() : IRequest<CommandResult>;
    public record ListarCobrancasEmailQuery(int Ultimos = 10) : IRequest<CommandResult>;

    public class ServicosFinanceirosQueryHandlers :
        IRequestHandler<ListarSacadosQuery, CommandResult>,
        IRequestHandler<ListarFaturasCobrancaQuery, CommandResult>,
        IRequestHandler<ListarContasEmissorasQuery, CommandResult>,
        IRequestHandler<ListarGruposRecorrenciaQuery, CommandResult>,
        IRequestHandler<ListarCobrancasEmailQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public ServicosFinanceirosQueryHandlers(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarSacadosQuery request, CancellationToken ct)
        {
            var tamanho = request.TamanhoPagina is <= 0 or > 100 ? 20 : request.TamanhoPagina;
            var pagina = request.Pagina <= 0 ? 1 : request.Pagina;
            var query = _context.Sacados.AsNoTracking().OrderBy(s => s.Nome);
            var total = await query.CountAsync(ct);
            var itens = await query.Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(s => new { s.Id, s.Nome, s.Documento, s.Email, s.GrupoRecorrenciaId, s.Bloqueado }).ToListAsync(ct);
            return CommandResult.Ok("Sacados listados.", new { total, pagina, tamanho, itens });
        }

        public async Task<CommandResult> Handle(ListarFaturasCobrancaQuery request, CancellationToken ct)
        {
            var tamanho = request.TamanhoPagina is <= 0 or > 100 ? 20 : request.TamanhoPagina;
            var pagina = request.Pagina <= 0 ? 1 : request.Pagina;
            var query = _context.FaturasCobranca.AsNoTracking().AsQueryable();
            if (request.SacadoId.HasValue) query = query.Where(f => f.SacadoId == request.SacadoId.Value);
            if (request.Situacao.HasValue) query = query.Where(f => f.Situacao == request.Situacao.Value);
            var total = await query.CountAsync(ct);
            var itens = await query.OrderBy(f => f.DataVencimento).Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(f => new { f.Id, f.SacadoId, f.Referencia, f.DataVencimento, f.Valor, f.Situacao, f.Remetida, f.NossoNumero }).ToListAsync(ct);
            return CommandResult.Ok("Faturas de cobrança listadas.", new { total, pagina, tamanho, itens });
        }

        public async Task<CommandResult> Handle(ListarContasEmissorasQuery request, CancellationToken ct)
        {
            var itens = await _context.ContasEmissoras.AsNoTracking().OrderBy(c => c.NomeBanco)
                .Select(c => new { c.Id, c.BancoId, c.NomeBanco, c.Agencia, c.Conta, c.Convenio, c.Ativa }).ToListAsync(ct);
            return CommandResult.Ok("Contas emissoras listadas.", itens);
        }

        public async Task<CommandResult> Handle(ListarGruposRecorrenciaQuery request, CancellationToken ct)
        {
            var itens = await _context.GruposRecorrencia.AsNoTracking().OrderBy(g => g.Descricao)
                .Select(g => new { g.Id, g.Descricao, g.Meses, g.DiaVencimento, g.Valor }).ToListAsync(ct);
            return CommandResult.Ok("Grupos de recorrência listados.", itens);
        }

        public async Task<CommandResult> Handle(ListarCobrancasEmailQuery request, CancellationToken ct)
        {
            var limite = request.Ultimos is <= 0 or > 100 ? 10 : request.Ultimos; // RSF-038
            var itens = await _context.CobrancasEmail.AsNoTracking().OrderByDescending(c => c.CriadoEm).Take(limite)
                .Select(c => new { c.Id, c.Nome, c.Valor, c.Status, c.Area }).ToListAsync(ct);
            return CommandResult.Ok("Cobranças por e-mail listadas.", itens);
        }
    }
}
