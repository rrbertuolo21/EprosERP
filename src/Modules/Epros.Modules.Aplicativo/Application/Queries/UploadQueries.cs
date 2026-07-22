using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Queries
{
    // ---------- DTOs ----------

    public record UplExecucaoImportacaoDto(
        Guid Id,
        string ImportRef,
        string TipoImportacao,
        EUplStatusImportacao Status,
        int? TotalLinhas,
        int? LinhasSucesso,
        int? LinhasIgnoradas,
        int? QuantidadeErros,
        EUplResultadoImportacao? Resultado,
        DateTime CriadoEm,
        DateTime? FinalizadoEm);

    public record UplImportacaoErroDto(int? NumeroLinha, string? Atributo, string Mensagem, DateTime CriadoEm);

    public record UplArquivoDto(Guid Id, string NomeOriginal, string NomeArmazenado, string? Extensao, long TamanhoBytes, string? HashArquivo, EUplStatusArquivo Status, DateTime CriadoEm);

    // ---------- Queries ----------

    public record ListarExecucoesImportacaoQuery(string? TipoImportacao, EUplStatusImportacao? Status, int Pagina = 1, int TamanhoPagina = 20) : IQuery<IEnumerable<UplExecucaoImportacaoDto>>;

    public record ObterExecucaoImportacaoQuery(Guid Id) : IQuery<UplExecucaoImportacaoDto?>;

    public record ListarErrosImportacaoQuery(string ReferenciaErro) : IQuery<IEnumerable<UplImportacaoErroDto>>;

    public record ObterArquivoPorIdQuery(Guid Id) : IQuery<UplArquivoDto?>;

    // ---------- Handlers ----------

    public class ListarExecucoesImportacaoQueryHandler : IQueryHandler<ListarExecucoesImportacaoQuery, IEnumerable<UplExecucaoImportacaoDto>>
    {
        private readonly ContextAplicativo _context;
        public ListarExecucoesImportacaoQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IEnumerable<UplExecucaoImportacaoDto>> Handle(ListarExecucoesImportacaoQuery request, CancellationToken ct)
        {
            var q = _context.UplExecucoesImportacao.AsNoTracking().Where(e => e.DeletadoEm == null);
            if (!string.IsNullOrWhiteSpace(request.TipoImportacao)) q = q.Where(e => e.TipoImportacao == request.TipoImportacao);
            if (request.Status.HasValue) q = q.Where(e => e.Status == request.Status.Value);

            var pagina = request.Pagina < 1 ? 1 : request.Pagina;
            var tamanho = request.TamanhoPagina < 1 ? 20 : request.TamanhoPagina;

            return await q.OrderByDescending(e => e.CriadoEm)
                .Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(e => new UplExecucaoImportacaoDto(e.Id, e.ImportRef, e.TipoImportacao, e.Status, e.TotalLinhas, e.LinhasSucesso, e.LinhasIgnoradas, e.QuantidadeErros, e.Resultado, e.CriadoEm, e.FinalizadoEm))
                .ToListAsync(ct);
        }
    }

    public class ObterExecucaoImportacaoQueryHandler : IQueryHandler<ObterExecucaoImportacaoQuery, UplExecucaoImportacaoDto?>
    {
        private readonly ContextAplicativo _context;
        public ObterExecucaoImportacaoQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<UplExecucaoImportacaoDto?> Handle(ObterExecucaoImportacaoQuery request, CancellationToken ct)
        {
            var e = await _context.UplExecucoesImportacao.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, ct);
            return e == null ? null : new UplExecucaoImportacaoDto(e.Id, e.ImportRef, e.TipoImportacao, e.Status, e.TotalLinhas, e.LinhasSucesso, e.LinhasIgnoradas, e.QuantidadeErros, e.Resultado, e.CriadoEm, e.FinalizadoEm);
        }
    }

    public class ListarErrosImportacaoQueryHandler : IQueryHandler<ListarErrosImportacaoQuery, IEnumerable<UplImportacaoErroDto>>
    {
        private readonly ContextAplicativo _context;
        public ListarErrosImportacaoQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IEnumerable<UplImportacaoErroDto>> Handle(ListarErrosImportacaoQuery request, CancellationToken ct)
        {
            return await _context.UplImportacaoErros.AsNoTracking()
                .Where(e => e.ReferenciaErro == request.ReferenciaErro && e.DeletadoEm == null)
                .OrderBy(e => e.NumeroLinha)
                .Select(e => new UplImportacaoErroDto(e.NumeroLinha, e.Atributo, e.Mensagem, e.CriadoEm))
                .ToListAsync(ct);
        }
    }

    public class ObterArquivoPorIdQueryHandler : IQueryHandler<ObterArquivoPorIdQuery, UplArquivoDto?>
    {
        private readonly ContextAplicativo _context;
        public ObterArquivoPorIdQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<UplArquivoDto?> Handle(ObterArquivoPorIdQuery request, CancellationToken ct)
        {
            var a = await _context.UplArquivos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, ct);
            return a == null ? null : new UplArquivoDto(a.Id, a.NomeOriginal, a.NomeArmazenado, a.Extensao, a.TamanhoBytes, a.HashArquivo, a.Status, a.CriadoEm);
        }
    }
}
