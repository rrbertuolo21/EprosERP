using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Queries
{
    /// <summary>
    /// Porte fiel de ComprasDadosController.GetServicos (GET api/v1/compras-dados/obter-servicos-por-ids).
    /// Retorna os serviços (por lista de Ids) usados na tela de compra. Servico é do módulo Fiscal;
    /// leitura via ServicoLookup (read-only cross-module).
    /// </summary>
    public record ObterDadosServicosPorIdsQuery(IReadOnlyCollection<Guid> IdsServicos) : IQuery<CommandResult>;

    public class ObterDadosServicosPorIdsQueryHandler : IRequestHandler<ObterDadosServicosPorIdsQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ObterDadosServicosPorIdsQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterDadosServicosPorIdsQuery request, CancellationToken cancellationToken)
        {
            if (request.IdsServicos == null || request.IdsServicos.Count == 0)
                return CommandResult.Falha("Deve haver no mínimo um Id de serviço!");

            var ids = request.IdsServicos.Distinct().ToList();

            var servicos = await _context.ServicosLookup
                .AsNoTracking()
                .Where(s => ids.Contains(s.Id))
                .Select(s => new
                {
                    s.Id,
                    s.UnidadeMedidaId,
                    s.CodigoServicoSefazId,
                    s.Codigo,
                    s.Descricao,
                    s.Valor,
                    s.InformacaoAdicional,
                    s.ServicoAtivo,
                    s.Cnae,
                    s.CodigoNbs,
                    s.IndicadorIss,
                    s.IndicadorIncentivo,
                    CstIbsCbs = s.CstIbsCbs != null && s.CstIbsCbs.Length >= 3 ? s.CstIbsCbs.Substring(0, 3) : s.CstIbsCbs,
                    CClassTrib = s.CClassTrib != null && s.CClassTrib.Length >= 6 ? s.CClassTrib.Substring(0, 6) : s.CClassTrib,
                    s.AliquotaIss,
                    s.AliquotaIssRetido,
                    s.AliquotaIrrfRetido,
                    s.AliquotaInss,
                    s.AliquotaPis,
                    s.AliquotaCofins,
                    s.CalcularRetencao
                })
                .ToListAsync(cancellationToken);

            var encontradosIds = servicos.Select(s => s.Id).ToHashSet();
            var naoEncontrados = ids.Where(id => !encontradosIds.Contains(id)).Select(id => $"Id {id} do serviço não encontrado!").ToList();

            if (servicos.Count == 0)
                return CommandResult.Falha(naoEncontrados.Count > 0 ? naoEncontrados : new[] { "Nenhum serviço encontrado." });

            return CommandResult.Ok("OK", new { Total = servicos.Count, Itens = servicos, NaoEncontrados = naoEncontrados });
        }
    }
}
