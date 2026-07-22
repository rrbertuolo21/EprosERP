using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Queries
{
    /// <summary>
    /// Porte fiel de VendaController.obter-informacoes-complementares-por-produtos-ids.
    /// Percorre Produto -> NcmConfiguracao (por NcmId) -> NcmTributacao.InformacoesComplementares,
    /// com fallback para a NcmTributacao da empresa quando o NCM do produto não tem configuração.
    /// Cross-module por Lookup (schemas estoque/plataforma) + Guid FK.
    ///
    /// <paramref name="EmpresaId"/> é a empresa ativa (o legado usava a empresa logada). Deve ser
    /// informada pelo chamador — o módulo Vendas não resolve a empresa por conta própria.
    /// </summary>
    public record ObterInformacoesComplementaresPorProdutosQuery(Guid EmpresaId, IReadOnlyList<Guid> ProdutosIds) : IQuery<CommandResult>;

    public class ObterInformacoesComplementaresPorProdutosQueryHandler
        : IRequestHandler<ObterInformacoesComplementaresPorProdutosQuery, CommandResult>
    {
        private readonly ContextVendas _context;

        public ObterInformacoesComplementaresPorProdutosQueryHandler(ContextVendas context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterInformacoesComplementaresPorProdutosQuery request, CancellationToken cancellationToken)
        {
            var ids = (request.ProdutosIds ?? Array.Empty<Guid>()).Distinct().ToList();
            if (ids.Count == 0)
                return CommandResult.Falha("Informe pelo menos um produto.");

            var produtos = await _context.ProdutosLookup
                .AsNoTracking()
                .Where(p => ids.Contains(p.Id))
                .Select(p => new { p.Id, p.NcmId })
                .ToListAsync(cancellationToken);

            var idsNaoEncontrados = ids.Except(produtos.Select(p => p.Id)).ToList();
            if (idsNaoEncontrados.Count > 0)
                return CommandResult.Falha($"Id(s) não localizado(s): {string.Join(", ", idsNaoEncontrados)}");

            var empresa = await _context.EmpresasLookup
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == request.EmpresaId, cancellationToken);
            if (empresa == null)
                return CommandResult.Falha("Empresa não localizada.");

            // NcmConfiguracao por NcmId dos produtos -> resolve NcmTributacaoId por produto.
            var ncmIds = produtos.Where(p => p.NcmId.HasValue).Select(p => p.NcmId!.Value).Distinct().ToList();
            var configs = ncmIds.Count == 0
                ? new List<(Guid NcmId, Guid NcmTributacaoId)>()
                : (await _context.NcmConfiguracoesLookup.AsNoTracking()
                        .Where(c => ncmIds.Contains(c.NcmId))
                        .Select(c => new { c.NcmId, c.NcmTributacaoId })
                        .ToListAsync(cancellationToken))
                    .Select(c => (NcmId: c.NcmId, NcmTributacaoId: c.NcmTributacaoId))
                    .ToList();

            var configPorNcm = configs
                .GroupBy(c => c.NcmId)
                .ToDictionary(g => g.Key, g => g.First().NcmTributacaoId);

            // NcmTributacaoIds necessários = das configs + fallback da empresa.
            var tributacaoIds = configPorNcm.Values.Distinct().ToList();
            if (empresa.NcmTributacaoId.HasValue && !tributacaoIds.Contains(empresa.NcmTributacaoId.Value))
                tributacaoIds.Add(empresa.NcmTributacaoId.Value);

            var tributacoes = tributacaoIds.Count == 0
                ? new List<(Guid Id, string? Inf)>()
                : (await _context.NcmTributacoesLookup.AsNoTracking()
                        .Where(t => tributacaoIds.Contains(t.Id))
                        .Select(t => new { t.Id, t.InformacoesComplementares })
                        .ToListAsync(cancellationToken))
                    .Select(t => (Id: t.Id, Inf: t.InformacoesComplementares))
                    .ToList();

            var infPorTributacao = tributacoes.ToDictionary(t => t.Id, t => t.Inf);

            var partes = new List<string>();

            // Produtos com NcmConfiguracao: informação complementar da respectiva NcmTributacao.
            var tributacoesDeProdutos = produtos
                .Where(p => p.NcmId.HasValue && configPorNcm.ContainsKey(p.NcmId.Value))
                .Select(p => configPorNcm[p.NcmId!.Value])
                .Distinct();
            foreach (var tribId in tributacoesDeProdutos)
                if (infPorTributacao.TryGetValue(tribId, out var inf) && !string.IsNullOrEmpty(inf))
                    partes.Add(inf!);

            // Produtos sem NcmConfiguracao: fallback para a NcmTributacao da empresa.
            var algumSemConfig = produtos.Any(p => !p.NcmId.HasValue || !configPorNcm.ContainsKey(p.NcmId!.Value));
            if (algumSemConfig && empresa.NcmTributacaoId.HasValue
                && infPorTributacao.TryGetValue(empresa.NcmTributacaoId.Value, out var infEmpresa)
                && !string.IsNullOrEmpty(infEmpresa))
                partes.Add(infEmpresa!);

            var resultado = new
            {
                informacoesComplementares = string.Join("; ", partes.Where(s => !string.IsNullOrEmpty(s)))
            };

            return CommandResult.Ok("OK", resultado);
        }
    }
}
