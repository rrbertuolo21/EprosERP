using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Queries
{
    public record ListarNcmTributacoesQuery(Guid? TributarioGrupoId = null, string? Localizar = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterNcmTributacaoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarNcmTributacoesQueryHandler : IRequestHandler<ListarNcmTributacoesQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ListarNcmTributacoesQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ListarNcmTributacoesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.NcmTributacoes.AsNoTracking().Where(n => n.DeletadoEm == null).AsQueryable();

            if (request.TributarioGrupoId.HasValue)
                query = query.Where(n => n.TributarioGrupoId == request.TributarioGrupoId.Value);

            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(n => n.Descricao.Contains(request.Localizar));

            var total = await query.CountAsync(cancellationToken);
            var itens = await query.OrderBy(n => n.CodRegra)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(n => new { n.Id, n.TributarioGrupoId, n.CodRegra, n.Descricao, n.Origem, n.CfopNotaFiscal, n.CfopNotaConsumidor })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterNcmTributacaoPorIdQueryHandler : IRequestHandler<ObterNcmTributacaoPorIdQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ObterNcmTributacaoPorIdQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ObterNcmTributacaoPorIdQuery request, CancellationToken cancellationToken)
        {
            var n = await _context.NcmTributacoes.AsNoTracking()
                .Include(x => x.NcmTributacaoSts.Where(s => s.DeletadoEm == null))
                .Include(x => x.NcmTributacaoFundoCombatePobrezas.Where(f => f.DeletadoEm == null))
                .Include(x => x.NcmConfiguracoes.Where(c => c.DeletadoEm == null))
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (n == null)
                return CommandResult.Falha("NCM Tributação não encontrada.");

            var dto = new
            {
                n.Id,
                n.TributarioGrupoId,
                n.CodigoBeneficioFiscalId,
                n.CodRegra,
                n.Descricao,
                n.CfopNotaConsumidor,
                n.CfopNotaFiscal,
                n.CfopNotaFiscalInterestadual,
                n.Origem,
                n.CsosnNotaConsumidor,
                n.CstIcmsNotaConsumidor,
                n.CsosnNotaFiscal,
                n.CstIcmsNotaFiscalInterna,
                n.CstIcmsNotaFiscalInterstadual,
                n.CstPis,
                n.CstCofins,
                n.ValorUnitFixoPis,
                n.ValorUnitFixoCofins,
                n.ValorAliquotaPis,
                n.ValorAliquotaCofins,
                n.CstPisCofinsEntrada,
                n.CstIpiSaida,
                n.CstIpiEntrada,
                n.ValorAliquotaIpi,
                n.ValorPercentualReducacaoBcIpi,
                n.TipoReducaoIpi,
                n.DestinoReducaoIpi,
                n.IpiEmbutido,
                n.EnquadramentoIpi,
                n.CodigoValorFiscalIcmsInterna,
                n.CodigoValorFiscalcmsInterstadual,
                n.ValorAliquotaIcmsInterna,
                n.ValorPercentualReducacaoBcIcmsInterna,
                n.TipoReducaoIcmsInterna,
                n.DestinoReducaoIcmsInterna,
                n.ValorAliquotaIcmsInterstadual,
                n.ValorPercentualReducacaoBcIcmsInterstadual,
                n.TipoReducaoIcmsInterstadual,
                n.DestinoReducaoIcmsInterstadual,
                n.CodigoBeneficioFiscalIcms,
                n.MotivoDesoneracaoIcms,
                n.InformacoesComplementares,
                n.InformacoesAdicionaisAoFisco,
                n.CstIbsCbsNfe,
                n.CClassTribNfe,
                n.CstIbsCbsNfce,
                n.CClassTribNfce,
                Sts = n.NcmTributacaoSts.Select(s => new { s.Id, s.Uf, s.TipoCalculo, s.ValorAliquotaIcmsSt, s.ValorMva, s.ValorPercentualReducaoBcIcmsSt, s.TipoReducaoIcmsSt, s.ValorUnitarioSt, s.ValorPercentualFcpSt }).ToList(),
                FundosCombatePobreza = n.NcmTributacaoFundoCombatePobrezas.Select(f => new { f.Id, f.Uf, f.ValorPercentual }).ToList(),
                Configuracoes = n.NcmConfiguracoes.Select(c => new { c.Id, c.NcmId }).ToList()
            };

            return CommandResult.Ok("OK", dto);
        }
    }
}
