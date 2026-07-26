using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Queries
{
    // ===================== Gestão de Contratos de Venda (VEN-GCV) =====================

    public record ListarContratosQuery(
        EContratoStatus? Status = null,
        Guid? TipoId = null,
        Guid? UsuarioResponsavelId = null,
        string? Localizar = null,
        int Pagina = 1,
        int TamanhoPagina = 10) : IQuery<CommandResult>;

    public class ListarContratosQueryHandler : IRequestHandler<ListarContratosQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarContratosQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarContratosQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var query = _context.Contratos.AsNoTracking().Where(c => c.TenantId == tenantId);
            if (request.Status.HasValue) query = query.Where(c => c.Status == request.Status.Value);
            if (request.TipoId.HasValue) query = query.Where(c => c.TipoId == request.TipoId.Value);
            if (request.UsuarioResponsavelId.HasValue) query = query.Where(c => c.UsuarioResponsavelId == request.UsuarioResponsavelId.Value);
            // GCV-012: busca por assunto, descrição, número e valor.
            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(c => c.Assunto.Contains(request.Localizar) ||
                                         (c.NumeroContrato != null && c.NumeroContrato.Contains(request.Localizar)) ||
                                         (c.Descricao != null && c.Descricao.Contains(request.Localizar)));

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(c => c.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(c => new { c.Id, c.NumeroContrato, c.Assunto, c.ClienteId, c.Valor, c.DataInicio, c.DataFim, Status = c.Status.ToString() })
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Contratos listados.", new { total, request.Pagina, request.TamanhoPagina, itens });
        }
    }

    public record ObterContratoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ObterContratoPorIdQueryHandler : IRequestHandler<ObterContratoPorIdQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterContratoPorIdQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterContratoPorIdQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var contrato = await _context.Contratos.AsNoTracking().FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == request.Id, cancellationToken);
            if (contrato == null) return CommandResult.Falha("Contrato não encontrado.");
            // GCV-025: detalhe carrega anexos, comentários, notas e renovações.
            var anexos = await _context.ContratoAnexos.AsNoTracking().Where(a => a.TenantId == tenantId && a.ContratoId == request.Id).Select(a => new { a.Id, a.NomeArquivo }).ToListAsync(cancellationToken);
            var comentarios = await _context.ContratoComentarios.AsNoTracking().Where(a => a.TenantId == tenantId && a.ContratoId == request.Id).Select(a => new { a.Id, a.Comentario, a.Editado }).ToListAsync(cancellationToken);
            var renovacoes = await _context.ContratoRenovacoes.AsNoTracking().Where(a => a.TenantId == tenantId && a.ContratoId == request.Id).Select(a => new { a.Id, a.DataInicio, a.DataFim, a.Valor, Status = a.Status.ToString() }).ToListAsync(cancellationToken);
            return CommandResult.Ok("Contrato encontrado.", new
            {
                contrato.Id, contrato.NumeroContrato, contrato.Assunto, contrato.ClienteId, contrato.Valor,
                contrato.DataInicio, contrato.DataFim, Status = contrato.Status.ToString(),
                contrato.EmpresaAssinou, contrato.ClienteAssinou, anexos, comentarios, renovacoes
            });
        }
    }
}
