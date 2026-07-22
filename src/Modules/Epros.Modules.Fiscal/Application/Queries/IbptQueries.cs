using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Enums;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;

namespace Epros.Modules.Fiscal.Application.Queries
{
    /// <summary>
    /// Retorna as alíquotas IBPT (federal nacional/importado, estadual, municipal) de um NCM em uma UF,
    /// a partir da tabela nacional <see cref="Ibpt"/>. Fiel ao legado
    /// <c>IbptDfeController.obter-aliquotas-por-ncm-uf</c>. Pega a linha vigente mais recente
    /// (EX = 0 quando não informado), ou a de maior vigência quando há várias versões.
    /// </summary>
    public record ObterAliquotasIbptPorNcmUfQuery(string Ncm, string Uf, int Ex = 0) : IQuery<CommandResult>;

    public class ObterAliquotasIbptPorNcmUfQueryHandler : IRequestHandler<ObterAliquotasIbptPorNcmUfQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ObterAliquotasIbptPorNcmUfQueryHandler(ContextFiscal context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterAliquotasIbptPorNcmUfQuery request, CancellationToken cancellationToken)
        {
            var ibpt = await BuscarAliquotasAsync(_context, request.Ncm, request.Uf, request.Ex, cancellationToken);
            if (ibpt is null)
            {
                return CommandResult.Falha($"Não há alíquotas IBPT cadastradas para o NCM {request.Ncm} na UF {request.Uf}.");
            }

            return CommandResult.Ok("OK", new
            {
                ibpt.Codigo,
                Uf = ibpt.Uf.ToString(),
                ibpt.Ex,
                ibpt.Descricao,
                AliquotaFederalNacional = ibpt.AliquotaNacionalFederal,
                AliquotaFederalImportado = ibpt.AliquotaImportadosFederal,
                AliquotaEstadual = ibpt.AliquotaEstadual,
                AliquotaMunicipal = ibpt.AliquotaMunicipal,
                ibpt.Versao,
                ibpt.Chave,
                ibpt.VigenciaInicio,
                ibpt.VigenciaFim
            });
        }

        /// <summary>
        /// Lookup compartilhado: acha a linha IBPT do NCM/UF (EX opcional), preferindo a de maior
        /// vigência. Retorna null quando a tabela não tem o NCM/UF (a tabela pode não ter sido carregada
        /// ainda — os dados vêm do loader/migração).
        /// </summary>
        internal static async Task<Ibpt?> BuscarAliquotasAsync(ContextFiscal context, string ncm, string uf, int ex, CancellationToken ct)
        {
            var codigo = new string((ncm ?? string.Empty).Where(char.IsDigit).ToArray());
            if (string.IsNullOrWhiteSpace(codigo))
                return null;

            if (!Enum.TryParse<EEstado>(uf, true, out var estado))
                return null;

            var query = context.Ibpts.AsNoTracking().Where(i => i.Codigo == codigo && i.Uf == estado);
            if (ex > 0)
                query = query.Where(i => i.Ex == ex);

            return await query
                .OrderByDescending(i => i.VigenciaFim)
                .ThenByDescending(i => i.VigenciaInicio)
                .FirstOrDefaultAsync(ct);
        }
    }

    /// <summary>
    /// Calcula o valor aproximado dos tributos (IBPT / Lei da Transparência) de um item,
    /// aplicando as alíquotas federal/estadual/municipal sobre o valor base, via
    /// <see cref="ICalculoFiscalService"/> (motor legado).
    ///
    /// Diferente do legado (onde o chamador passava as alíquotas), aqui, quando o request NÃO informa
    /// alíquotas, resolvemos automaticamente a linha da tabela <see cref="Ibpt"/> por NCM/UF. Se o
    /// chamador informar alíquotas, elas prevalecem (compatibilidade). Se não houver tabela nem
    /// alíquotas informadas, o cálculo prossegue com zeros (fallback honesto — sem inventar percentual).
    /// </summary>
    public record CalcularIbptValorAproximadoQuery(
        string Ncm,
        string Uf,
        decimal ValorBase,
        string Origem,
        decimal AliquotaFederalNacional = 0,
        decimal AliquotaFederalImportado = 0,
        decimal AliquotaEstadual = 0,
        decimal AliquotaMunicipal = 0,
        string? VersaoIbpt = null
    ) : IQuery<CommandResult>;

    public class CalcularIbptValorAproximadoQueryHandler : IRequestHandler<CalcularIbptValorAproximadoQuery, CommandResult>
    {
        private readonly ICalculoFiscalService _calculoService;
        private readonly ContextFiscal _context;

        public CalcularIbptValorAproximadoQueryHandler(ICalculoFiscalService calculoService, ContextFiscal context)
        {
            _calculoService = calculoService;
            _context = context;
        }

        public async Task<CommandResult> Handle(CalcularIbptValorAproximadoQuery request, CancellationToken cancellationToken)
        {
            var aliqFedNac = request.AliquotaFederalNacional;
            var aliqFedImp = request.AliquotaFederalImportado;
            var aliqEst = request.AliquotaEstadual;
            var aliqMun = request.AliquotaMunicipal;
            var versao = request.VersaoIbpt;

            // Sem alíquotas informadas => tenta a tabela nacional (NCM/UF).
            var nenhumaInformada = aliqFedNac == 0 && aliqFedImp == 0 && aliqEst == 0 && aliqMun == 0;
            if (nenhumaInformada)
            {
                var ibpt = await ObterAliquotasIbptPorNcmUfQueryHandler.BuscarAliquotasAsync(_context, request.Ncm, request.Uf, 0, cancellationToken);
                if (ibpt is not null)
                {
                    aliqFedNac = ibpt.AliquotaNacionalFederal;
                    aliqFedImp = ibpt.AliquotaImportadosFederal;
                    aliqEst = ibpt.AliquotaEstadual;
                    aliqMun = ibpt.AliquotaMunicipal;
                    versao ??= ibpt.Versao;
                }
            }

            var req = new CalculoFiscalRequest
            {
                Ncm = request.Ncm,
                UfDestino = request.Uf,
                UfOrigem = request.Uf,
                Origem = string.IsNullOrEmpty(request.Origem) ? "0" : request.Origem,
                ValorUnitario = request.ValorBase,
                Quantidade = 1,
                AliquotaIbptFederalNacional = aliqFedNac,
                AliquotaIbptFederalImportado = aliqFedImp,
                AliquotaIbptEstadual = aliqEst,
                AliquotaIbptMunicipal = aliqMun,
                VersaoIbpt = versao
            };

            var resultado = _calculoService.Calcular(req);

            if (!resultado.Sucesso)
            {
                return CommandResult.Falha(resultado.Erros, "Falha ao calcular o valor aproximado de tributos (IBPT).");
            }

            var ibptResult = resultado.Ibpt;
            var aliquotaTotal = ibptResult.AliquotaFederal + ibptResult.AliquotaImportado + ibptResult.AliquotaEstadual + ibptResult.AliquotaMunicipal;

            return CommandResult.Ok("OK", new
            {
                Ncm = request.Ncm,
                Uf = request.Uf,
                ValorBase = request.ValorBase,
                Aliquota = aliquotaTotal,
                AliquotaFederal = ibptResult.AliquotaFederal,
                AliquotaImportado = ibptResult.AliquotaImportado,
                AliquotaEstadual = ibptResult.AliquotaEstadual,
                AliquotaMunicipal = ibptResult.AliquotaMunicipal,
                ValorImposto = ibptResult.ValorAproximado,
                FonteAliquota = nenhumaInformada ? "tabela-ibpt" : "informada-no-request"
            });
        }
    }
}
