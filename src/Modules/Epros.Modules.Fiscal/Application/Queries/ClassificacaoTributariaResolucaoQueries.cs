using System;
using System.Collections.Generic;
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
    /// <summary>Resolve o CST IBS/CBS por código, filtrando pelo modelo (55 = NF-e, 65 = NFC-e).</summary>
    /// <param name="Cst">Código do CST (ex.: "000").</param>
    /// <param name="Modelo">Modelo do documento: 55 (NF-e) ou 65 (NFC-e).</param>
    public record ObterCstIbsCbsPorCstQuery(string Cst, int Modelo) : IQuery<CommandResult>;

    /// <summary>
    /// Resolve as classificações tributárias (cClassTrib) aplicáveis a um NCM, filtrando pelo modelo.
    /// Quando não há classificação específica para o NCM, retorna o CST genérico "000" (fallback do legado).
    /// </summary>
    /// <param name="Ncm">Código NCM (8 dígitos).</param>
    /// <param name="Modelo">Modelo do documento: 55 (NF-e) ou 65 (NFC-e).</param>
    public record ObterClassificacoesPorNcmQuery(string Ncm, int Modelo) : IQuery<CommandResult>;

    /// <summary>Resolve CST + cClassTrib para uma lista de NCMs (batch), usado na emissão IBS/CBS.</summary>
    /// <param name="Ncms">Lista de códigos NCM a classificar.</param>
    /// <param name="Modelo">Modelo do documento: 55 (NF-e) ou 65 (NFC-e).</param>
    public record ObterNcmsClassificadosQuery(string[] Ncms, int Modelo) : IQuery<CommandResult>;

    /// <summary>Item de retorno da classificação de um NCM: CST + cClassTrib.</summary>
    public record NcmClassificadoDto(string Ncm, string Cst, string CClassTrib);

    /// <summary>Montagem compartilhada do DTO de retorno de um CST IBS/CBS com suas classes tributárias.</summary>
    internal static class ClassificacaoTributariaResolucaoDto
    {
        public static object MontarCst(Domain.Entities.CstIbsCbs cst) => new
        {
            cst.Id,
            cst.Cst,
            cst.Descricao,
            cst.DataInicioVigencia,
            cst.DataFimVigencia,
            ClassesTributarias = cst.ClassesTributarias.Select(ct => new
            {
                ct.Id,
                ct.Codigo,
                ct.Descricao,
                ct.IndNfe,
                ct.IndNfce,
                Anexos = ct.Anexos.Select(a => new { a.Id, a.NroAnexo, a.Codigo }).ToList()
            }).ToList()
        };
    }

    public class ObterCstIbsCbsPorCstQueryHandler : IRequestHandler<ObterCstIbsCbsPorCstQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ObterCstIbsCbsPorCstQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ObterCstIbsCbsPorCstQuery request, CancellationToken cancellationToken)
        {
            if (request.Modelo != 55 && request.Modelo != 65)
                return CommandResult.Falha("Modelo inválido. Use 55 para NF-e ou 65 para NFC-e.");

            var cst = await _context.CstsIbsCbs.AsNoTracking()
                .Include(c => c.ClassesTributarias.Where(ct => ct.DeletadoEm == null && (request.Modelo == 55 ? ct.IndNfe : ct.IndNfce)))
                    .ThenInclude(ct => ct.Anexos.Where(a => a.DeletadoEm == null))
                .FirstOrDefaultAsync(c => c.Cst == request.Cst && c.DeletadoEm == null, cancellationToken);

            if (cst == null)
                return CommandResult.Falha("CST não encontrado.");

            return CommandResult.Ok("OK", ClassificacaoTributariaResolucaoDto.MontarCst(cst));
        }
    }

    public class ObterClassificacoesPorNcmQueryHandler : IRequestHandler<ObterClassificacoesPorNcmQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ObterClassificacoesPorNcmQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ObterClassificacoesPorNcmQuery request, CancellationToken cancellationToken)
        {
            if (request.Modelo != 55 && request.Modelo != 65)
                return CommandResult.Falha("Modelo inválido. Use 55 para NF-e ou 65 para NFC-e.");

            var ncm = (request.Ncm ?? string.Empty).Replace(".", string.Empty).Trim();

            var classes = await _context.ClassificacoesTributarias.AsNoTracking()
                .Include(c => c.Anexos.Where(a => a.DeletadoEm == null))
                .Where(c => c.DeletadoEm == null
                            && (request.Modelo == 55 ? c.IndNfe : c.IndNfce)
                            && c.Anexos.Any(a => a.DeletadoEm == null && a.Codigo == ncm))
                .ToListAsync(cancellationToken);

            if (classes.Count == 0)
            {
                // Fallback: CST genérico "000" (não tributado / sem classificação específica).
                var generico = await _context.CstsIbsCbs.AsNoTracking()
                    .Include(c => c.ClassesTributarias.Where(ct => ct.DeletadoEm == null))
                        .ThenInclude(ct => ct.Anexos.Where(a => a.DeletadoEm == null))
                    .FirstOrDefaultAsync(c => c.Cst == "000" && c.DeletadoEm == null, cancellationToken);

                if (generico == null)
                    return CommandResult.Ok("OK", new List<object>());

                return CommandResult.Ok("OK", new List<object> { ClassificacaoTributariaResolucaoDto.MontarCst(generico) });
            }

            var retorno = classes.Select(c => new
            {
                c.Id,
                c.CstIbsCbsId,
                c.Codigo,
                c.Descricao,
                Cst = c.Codigo.Length >= 3 ? c.Codigo[..3] : c.Codigo,
                CClassTrib = c.Codigo,
                c.IndNfe,
                c.IndNfce,
                Anexos = c.Anexos.Select(a => new { a.Id, a.NroAnexo, a.Codigo }).ToList()
            }).ToList();

            return CommandResult.Ok("OK", retorno);
        }
    }

    public class ObterNcmsClassificadosQueryHandler : IRequestHandler<ObterNcmsClassificadosQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ObterNcmsClassificadosQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ObterNcmsClassificadosQuery request, CancellationToken cancellationToken)
        {
            if (request.Modelo != 55 && request.Modelo != 65)
                return CommandResult.Falha("Modelo inválido. Use 55 para NF-e ou 65 para NFC-e.");

            var ncms = (request.Ncms ?? Array.Empty<string>())
                .Select(n => (n ?? string.Empty).Replace(".", string.Empty).Trim())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .ToArray();

            if (ncms.Length == 0)
                return CommandResult.Falha("Informe ao menos um NCM.");

            var classes = await _context.ClassificacoesTributarias.AsNoTracking()
                .Include(ct => ct.Anexos.Where(a => a.DeletadoEm == null))
                .Where(ct => ct.DeletadoEm == null
                             && (request.Modelo == 55 ? ct.IndNfe : ct.IndNfce)
                             && ct.Anexos.Any(a => a.DeletadoEm == null && ncms.Contains(a.Codigo)))
                .ToListAsync(cancellationToken);

            var retorno = new List<NcmClassificadoDto>();
            foreach (var ncm in ncms)
            {
                var localizados = classes
                    .Where(c => c.Anexos.Any(a => a.DeletadoEm == null && a.Codigo == ncm))
                    .ToList();

                if (localizados.Count == 0)
                {
                    // Fallback do legado: CST "000" e cClassTrib "000001".
                    retorno.Add(new NcmClassificadoDto(ncm, "000", "000001"));
                }
                else
                {
                    foreach (var item in localizados)
                    {
                        var cst = item.Codigo.Length >= 3 ? item.Codigo[..3] : item.Codigo;
                        retorno.Add(new NcmClassificadoDto(ncm, cst, item.Codigo));
                    }
                }
            }

            return CommandResult.Ok("OK", retorno);
        }
    }

}
