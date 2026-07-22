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

    public record WfDefinicaoDto(Guid Id, string Modulo, string Entidade, string Nome, int Versao, EWfDefinicaoStatus Status);

    public record WfInstanciaDto(
        Guid Id,
        Guid DefinicaoId,
        string Modulo,
        string EntidadeTipo,
        string EntidadeId,
        string Descricao,
        EWfInstanciaStatus Status,
        decimal? ValorReferencia,
        Guid? ResponsavelUsuarioId,
        string? AprovadoPor,
        string? Comentario,
        DateTime? DecididoEm,
        DateTime CriadoEm);

    public record WfTarefaDto(Guid Id, Guid InstanciaId, string Titulo, Guid? ResponsavelUsuarioId, EWfPermissao? ResponsavelPapel, EWfTarefaStatus Status, DateTime? PrazoEm, DateTime? ConcluidaEm);

    public record WfHistoricoDto(Guid Id, string Acao, string? EstadoAnterior, string? EstadoNovo, Guid? UsuarioId, string? IpOrigem, DateTime CriadoEm);

    // ---------- Queries ----------

    public record ListarWfDefinicoesQuery(string? Modulo) : IQuery<IEnumerable<WfDefinicaoDto>>;

    public record ListarWfInstanciasQuery(EWfInstanciaStatus? Status, string? EntidadeTipo, Guid? ResponsavelUsuarioId, int Pagina = 1, int TamanhoPagina = 20) : IQuery<IEnumerable<WfInstanciaDto>>;

    public record ObterWfInstanciaPorIdQuery(Guid InstanciaId) : IQuery<WfInstanciaDto?>;

    public record ListarWfHistoricoQuery(Guid InstanciaId) : IQuery<IEnumerable<WfHistoricoDto>>;

    public record ListarWfTarefasQuery(Guid? InstanciaId, Guid? ResponsavelUsuarioId) : IQuery<IEnumerable<WfTarefaDto>>;

    // ---------- Handlers ----------

    public class ListarWfDefinicoesQueryHandler : IQueryHandler<ListarWfDefinicoesQuery, IEnumerable<WfDefinicaoDto>>
    {
        private readonly ContextAplicativo _context;
        public ListarWfDefinicoesQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IEnumerable<WfDefinicaoDto>> Handle(ListarWfDefinicoesQuery request, CancellationToken cancellationToken)
        {
            var q = _context.WfDefinicoes.AsNoTracking().Where(d => d.DeletadoEm == null);
            if (!string.IsNullOrWhiteSpace(request.Modulo))
                q = q.Where(d => d.Modulo == request.Modulo);

            return await q.OrderBy(d => d.Modulo).ThenBy(d => d.Nome)
                .Select(d => new WfDefinicaoDto(d.Id, d.Modulo, d.Entidade, d.Nome, d.Versao, d.Status))
                .ToListAsync(cancellationToken);
        }
    }

    public class ListarWfInstanciasQueryHandler : IQueryHandler<ListarWfInstanciasQuery, IEnumerable<WfInstanciaDto>>
    {
        private readonly ContextAplicativo _context;
        public ListarWfInstanciasQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IEnumerable<WfInstanciaDto>> Handle(ListarWfInstanciasQuery request, CancellationToken cancellationToken)
        {
            var q = _context.WfInstancias.AsNoTracking().Where(i => i.DeletadoEm == null);
            if (request.Status.HasValue) q = q.Where(i => i.Status == request.Status.Value);
            if (!string.IsNullOrWhiteSpace(request.EntidadeTipo)) q = q.Where(i => i.EntidadeTipo == request.EntidadeTipo);
            if (request.ResponsavelUsuarioId.HasValue) q = q.Where(i => i.ResponsavelUsuarioId == request.ResponsavelUsuarioId);

            var pagina = request.Pagina < 1 ? 1 : request.Pagina;
            var tamanho = request.TamanhoPagina < 1 ? 20 : request.TamanhoPagina;

            return await q.OrderByDescending(i => i.CriadoEm)
                .Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(i => new WfInstanciaDto(i.Id, i.DefinicaoId, i.Modulo, i.EntidadeTipo, i.EntidadeIdReferencia, i.Descricao, i.Status, i.ValorReferencia, i.ResponsavelUsuarioId, i.AprovadoPor, i.Comentario, i.DecididoEm, i.CriadoEm))
                .ToListAsync(cancellationToken);
        }
    }

    public class ObterWfInstanciaPorIdQueryHandler : IQueryHandler<ObterWfInstanciaPorIdQuery, WfInstanciaDto?>
    {
        private readonly ContextAplicativo _context;
        public ObterWfInstanciaPorIdQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<WfInstanciaDto?> Handle(ObterWfInstanciaPorIdQuery request, CancellationToken cancellationToken)
        {
            var i = await _context.WfInstancias.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.InstanciaId && x.DeletadoEm == null, cancellationToken);
            if (i == null) return null;
            return new WfInstanciaDto(i.Id, i.DefinicaoId, i.Modulo, i.EntidadeTipo, i.EntidadeIdReferencia, i.Descricao, i.Status, i.ValorReferencia, i.ResponsavelUsuarioId, i.AprovadoPor, i.Comentario, i.DecididoEm, i.CriadoEm);
        }
    }

    public class ListarWfHistoricoQueryHandler : IQueryHandler<ListarWfHistoricoQuery, IEnumerable<WfHistoricoDto>>
    {
        private readonly ContextAplicativo _context;
        public ListarWfHistoricoQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IEnumerable<WfHistoricoDto>> Handle(ListarWfHistoricoQuery request, CancellationToken cancellationToken)
        {
            return await _context.WfHistoricos.AsNoTracking()
                .Where(h => h.InstanciaId == request.InstanciaId && h.DeletadoEm == null)
                .OrderBy(h => h.CriadoEm)
                .Select(h => new WfHistoricoDto(h.Id, h.Acao, h.EstadoAnterior, h.EstadoNovo, h.UsuarioId, h.IpOrigem, h.CriadoEm))
                .ToListAsync(cancellationToken);
        }
    }

    public class ListarWfTarefasQueryHandler : IQueryHandler<ListarWfTarefasQuery, IEnumerable<WfTarefaDto>>
    {
        private readonly ContextAplicativo _context;
        public ListarWfTarefasQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IEnumerable<WfTarefaDto>> Handle(ListarWfTarefasQuery request, CancellationToken cancellationToken)
        {
            var q = _context.WfTarefas.AsNoTracking().Where(t => t.DeletadoEm == null);
            if (request.InstanciaId.HasValue) q = q.Where(t => t.InstanciaId == request.InstanciaId);
            if (request.ResponsavelUsuarioId.HasValue) q = q.Where(t => t.ResponsavelUsuarioId == request.ResponsavelUsuarioId);

            return await q.OrderByDescending(t => t.CriadoEm)
                .Select(t => new WfTarefaDto(t.Id, t.InstanciaId, t.Titulo, t.ResponsavelUsuarioId, t.ResponsavelPapel, t.Status, t.PrazoEm, t.ConcluidaEm))
                .ToListAsync(cancellationToken);
        }
    }
}
