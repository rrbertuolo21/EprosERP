using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Assinatura;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Plataforma.Assinatura
{
    /// <summary>
    /// PLT · ASSINATURA ELETRÔNICA ICP — handlers. O ato de assinar passa por
    /// <see cref="IAssinaturaDigitalService"/> (provedor ICP = dependência externa). Sem provedor,
    /// o resultado é "aguardando provedor": nada é marcado como assinado — NUNCA se forja validade.
    /// </summary>
    public class SolicitarAssinaturasCommandHandler : ICommandHandler<SolicitarAssinaturasCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public SolicitarAssinaturasCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(SolicitarAssinaturasCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var doc = await _context.DocumentosGed.FirstOrDefaultAsync(d => d.Id == request.DocumentoId, ct);
            if (doc == null) return CommandResult.Falha("Documento não encontrado no GED.");

            if (request.Signatarios == null || request.Signatarios.Count == 0)
                return CommandResult.Falha("Informe ao menos um signatário.");

            // Política do tipo documental define o mínimo; senão, o nº de obrigatórios.
            var politica = request.TipoDocumento == null ? null :
                await _context.PoliticasAssinatura.FirstOrDefaultAsync(p => p.TipoDocumento == request.TipoDocumento, ct);
            var obrigatorios = request.Signatarios.Count(s => s.Obrigatorio);
            var minAssinaturas = politica?.MinAssinaturas ?? Math.Max(1, obrigatorios);
            var exigeOrdem = politica?.ExigeOrdem ?? request.ExigeOrdem;
            var tipoAssinatura = request.TipoAssinatura ?? politica?.TipoAssinatura ?? "ICP-Brasil";
            var temExterno = request.Signatarios.Any(s => s.Externo);

            var solicitacao = new SolicitacaoAssinatura(doc.Id, doc.Versao, minAssinaturas, exigeOrdem,
                temExterno, tipoAssinatura, tenantId, usuario);
            if (!solicitacao.IsValid) return CommandResult.Falha(solicitacao.Notifications.Select(n => n.Message));
            _context.SolicitacoesAssinatura.Add(solicitacao);

            foreach (var s in request.Signatarios.OrderBy(s => s.Ordem))
            {
                var token = s.Externo ? Guid.NewGuid().ToString("N") : null;
                var sig = new SignatarioAssinatura(solicitacao.Id, s.Ordem, s.Obrigatorio, s.Externo,
                    s.Nome, s.Identificacao, token, tenantId, usuario);
                if (!sig.IsValid) return CommandResult.Falha(sig.Notifications.Select(n => n.Message));
                _context.SignatariosAssinatura.Add(sig);
            }

            doc.MarcarPendenteAssinatura(usuario);
            _context.HistoricosAssinatura.Add(new HistoricoAssinatura(solicitacao.Id, "Solicitada",
                $"{request.Signatarios.Count} signatário(s); min {minAssinaturas}", tenantId, usuario));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Plataforma.AssinaturaSolicitada,
                JsonSerializer.Serialize(new { solicitacao.Id, solicitacao.DocumentoId, minAssinaturas })));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Solicitação de assinatura criada.",
                new { SolicitacaoId = solicitacao.Id, LinkPublico = temExterno });
        }
    }

    /// <summary>Núcleo compartilhado do fluxo de assinar (interno ou por link público).</summary>
    internal static class AssinaturaFluxo
    {
        public static async Task<CommandResult> AssinarAsync(
            ContextAplicativo context, IAssinaturaDigitalService assinaturaService,
            string tenantId, string usuario, SolicitacaoAssinatura solicitacao,
            SignatarioAssinatura signatario, CancellationToken ct)
        {
            if (!solicitacao.EmAndamento)
                return CommandResult.Falha("A solicitação não está em assinatura.");
            if (signatario.Status != "Pendente")
                return CommandResult.Falha("Signatário já respondeu a esta solicitação.");

            // Ordem sequencial: nenhum obrigatório de ordem menor pode estar pendente.
            if (solicitacao.ExigeOrdem)
            {
                var pendenteAnterior = await context.SignatariosAssinatura
                    .AnyAsync(s => s.SolicitacaoId == solicitacao.Id && s.Obrigatorio &&
                                   s.Ordem < signatario.Ordem && s.Status == "Pendente", ct);
                if (pendenteAnterior)
                    return CommandResult.Falha("Assinatura sequencial: há signatário anterior pendente.");
            }

            // Passa pela abstração ICP. Sem provedor => aguardando (documento continua pendente).
            var resultado = await assinaturaService.SolicitarAssinaturaAsync(solicitacao.DocumentoId, ct);
            if (resultado.Pendente)
                return CommandResult.Ok("Aguardando provedor ICP (certificado/provedor = dependência externa). " +
                                        "Documento permanece pendente — validade não é forjada.",
                    new { Pendente = true, resultado.Detalhe });
            if (!resultado.Assinado)
                return CommandResult.Falha(resultado.Detalhe ?? "Falha na assinatura pelo provedor.");

            var registro = new RegistroAssinatura(solicitacao.Id, signatario.Id, carimboTempo: DateTime.UtcNow.ToString("O"),
                tenantId, usuario);
            context.Set<RegistroAssinatura>().Add(registro);
            context.Set<EvidenciaAssinatura>().Add(new EvidenciaAssinatura(registro.Id,
                resultado.EvidenciaHash ?? string.Empty, certificadoSerial: null, cadeiaIcp: null,
                valorEvidencia: resultado.Detalhe, tenantId, usuario));
            signatario.MarcarAssinado(usuario);
            context.Set<HistoricoAssinatura>().Add(new HistoricoAssinatura(solicitacao.Id, "Assinada",
                $"Signatário {signatario.Nome}", tenantId, usuario));
            context.Set<OutboxMessage>().Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Plataforma.AssinaturaRegistrada,
                JsonSerializer.Serialize(new { SolicitacaoId = solicitacao.Id, SignatarioId = signatario.Id })));

            // Conclusão: todos os obrigatórios assinaram E o mínimo foi atingido.
            var assinadas = await context.SignatariosAssinatura
                .CountAsync(s => s.SolicitacaoId == solicitacao.Id && s.Status == "Assinado", ct);
            assinadas += 1; // inclui o atual (ainda não salvo)
            var obrigatoriosPendentes = await context.SignatariosAssinatura
                .CountAsync(s => s.SolicitacaoId == solicitacao.Id && s.Obrigatorio &&
                                 s.Status == "Pendente" && s.Id != signatario.Id, ct);

            if (obrigatoriosPendentes == 0 && assinadas >= solicitacao.MinAssinaturas)
            {
                solicitacao.Concluir(usuario);
                var doc = await context.DocumentosGed.FirstAsync(d => d.Id == solicitacao.DocumentoId, ct);
                doc.ConfirmarAssinatura(usuario);
                context.Set<HistoricoAssinatura>().Add(new HistoricoAssinatura(solicitacao.Id, "Concluida", null, tenantId, usuario));
                context.Set<OutboxMessage>().Add(new OutboxMessage(tenantId,
                    CatalogoEventosIntegracao.Plataforma.AssinaturaConcluida,
                    JsonSerializer.Serialize(new { solicitacao.Id, solicitacao.DocumentoId })));
            }

            await context.SaveChangesAsync(ct);
            return CommandResult.Ok("Assinatura registrada.",
                new { RegistroId = registro.Id, Concluida = solicitacao.Estado == "Concluida" });
        }
    }

    public class RegistrarAssinaturaCommandHandler : ICommandHandler<RegistrarAssinaturaCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        private readonly IAssinaturaDigitalService _assinatura;

        public RegistrarAssinaturaCommandHandler(ContextAplicativo context, ITenantProvider tenant,
            ICurrentUser user, IAssinaturaDigitalService assinatura)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
            _assinatura = assinatura;
        }

        public async Task<CommandResult> Handle(RegistrarAssinaturaCommand request, CancellationToken ct)
        {
            var solicitacao = await _context.SolicitacoesAssinatura.FirstOrDefaultAsync(s => s.Id == request.SolicitacaoId, ct);
            if (solicitacao == null) return CommandResult.Falha("Solicitação não encontrada.");
            var signatario = await _context.SignatariosAssinatura
                .FirstOrDefaultAsync(s => s.Id == request.SignatarioId && s.SolicitacaoId == request.SolicitacaoId, ct);
            if (signatario == null) return CommandResult.Falha("Signatário não encontrado.");

            return await AssinaturaFluxo.AssinarAsync(_context, _assinatura, _tenant.GetTenantId(),
                _user.GetUserId() ?? "system", solicitacao, signatario, ct);
        }
    }

    public class AssinarPorLinkPublicoCommandHandler : ICommandHandler<AssinarPorLinkPublicoCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        private readonly IAssinaturaDigitalService _assinatura;

        public AssinarPorLinkPublicoCommandHandler(ContextAplicativo context, ITenantProvider tenant,
            ICurrentUser user, IAssinaturaDigitalService assinatura)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
            _assinatura = assinatura;
        }

        public async Task<CommandResult> Handle(AssinarPorLinkPublicoCommand request, CancellationToken ct)
        {
            var signatario = await _context.SignatariosAssinatura
                .FirstOrDefaultAsync(s => s.LinkToken == request.LinkToken && s.Externo, ct);
            if (signatario == null) return CommandResult.Falha("Link de assinatura inválido.");
            if (signatario.RevogadoEm != null) return CommandResult.Falha("Link de assinatura revogado.", block: true);

            var solicitacao = await _context.SolicitacoesAssinatura.FirstOrDefaultAsync(s => s.Id == signatario.SolicitacaoId, ct);
            if (solicitacao == null) return CommandResult.Falha("Solicitação não encontrada.");

            return await AssinaturaFluxo.AssinarAsync(_context, _assinatura, _tenant.GetTenantId(),
                signatario.Identificacao, solicitacao, signatario, ct);
        }
    }

    public class RecusarAssinaturaCommandHandler : ICommandHandler<RecusarAssinaturaCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public RecusarAssinaturaCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(RecusarAssinaturaCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var solicitacao = await _context.SolicitacoesAssinatura.FirstOrDefaultAsync(s => s.Id == request.SolicitacaoId, ct);
            if (solicitacao == null) return CommandResult.Falha("Solicitação não encontrada.");
            if (!solicitacao.EmAndamento) return CommandResult.Falha("A solicitação não está em assinatura.");
            var signatario = await _context.SignatariosAssinatura
                .FirstOrDefaultAsync(s => s.Id == request.SignatarioId && s.SolicitacaoId == request.SolicitacaoId, ct);
            if (signatario == null) return CommandResult.Falha("Signatário não encontrado.");
            if (signatario.Status != "Pendente") return CommandResult.Falha("Signatário já respondeu.");

            signatario.MarcarRecusado(usuario);
            solicitacao.Recusar(usuario);
            var doc = await _context.DocumentosGed.FirstAsync(d => d.Id == solicitacao.DocumentoId, ct);
            doc.RecusarAssinatura(usuario);
            _context.HistoricosAssinatura.Add(new HistoricoAssinatura(solicitacao.Id, "Recusada", request.Motivo, tenantId, usuario));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Plataforma.AssinaturaRecusada,
                JsonSerializer.Serialize(new { SolicitacaoId = solicitacao.Id, SignatarioId = signatario.Id, request.Motivo })));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Assinatura recusada.");
        }
    }

    public class RevogarLinkPublicoCommandHandler : ICommandHandler<RevogarLinkPublicoCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public RevogarLinkPublicoCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(RevogarLinkPublicoCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var signatario = await _context.SignatariosAssinatura.FirstOrDefaultAsync(s => s.Id == request.SignatarioId, ct);
            if (signatario == null) return CommandResult.Falha("Signatário não encontrado.");
            if (!signatario.Externo) return CommandResult.Falha("Apenas signatários externos possuem link público.");
            if (signatario.RevogadoEm != null) return CommandResult.Falha("Link já revogado.");

            signatario.RevogarLink(usuario);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Plataforma.AssinaturaLinkPublicoRevogado,
                JsonSerializer.Serialize(new { signatario.Id, signatario.SolicitacaoId })));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Link público revogado.");
        }
    }

    public class DefinirPoliticaAssinaturaCommandHandler : ICommandHandler<DefinirPoliticaAssinaturaCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public DefinirPoliticaAssinaturaCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(DefinirPoliticaAssinaturaCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var existente = await _context.PoliticasAssinatura.FirstOrDefaultAsync(p => p.TipoDocumento == request.TipoDocumento, ct);
            if (existente != null)
            {
                existente.Atualizar(request.MinAssinaturas, request.TipoAssinatura, request.ExigeOrdem, usuario);
                if (!existente.IsValid) return CommandResult.Falha(existente.Notifications.Select(n => n.Message));
                await _context.SaveChangesAsync(ct);
                return CommandResult.Ok("Política de assinatura atualizada.", new { existente.Id });
            }

            var pol = new PoliticaAssinatura(request.TipoDocumento, request.MinAssinaturas, request.TipoAssinatura, request.ExigeOrdem, tenantId, usuario);
            if (!pol.IsValid) return CommandResult.Falha(pol.Notifications.Select(n => n.Message));
            _context.PoliticasAssinatura.Add(pol);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Política de assinatura definida.", new { pol.Id });
        }
    }

    // ===================== Queries =====================

    public class ObterSolicitacoesAssinaturaQueryHandler
        : IQueryHandler<ObterSolicitacoesAssinaturaQuery, IReadOnlyList<SolicitacaoAssinaturaDto>>
    {
        private readonly ContextAplicativo _context;
        public ObterSolicitacoesAssinaturaQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IReadOnlyList<SolicitacaoAssinaturaDto>> Handle(ObterSolicitacoesAssinaturaQuery request, CancellationToken ct)
        {
            var q = _context.SolicitacoesAssinatura.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Estado)) q = q.Where(s => s.Estado == request.Estado);
            if (request.DocumentoId.HasValue) q = q.Where(s => s.DocumentoId == request.DocumentoId.Value);

            return await q.OrderByDescending(s => s.CriadoEm)
                .Select(s => new SolicitacaoAssinaturaDto(s.Id, s.DocumentoId, s.VersaoDocumento, s.Estado,
                    s.LinkPublico, s.MinAssinaturas, s.ExigeOrdem, s.TipoAssinatura, s.CriadoEm))
                .ToListAsync(ct);
        }
    }

    public class ObterSolicitacaoAssinaturaPorIdQueryHandler
        : IQueryHandler<ObterSolicitacaoAssinaturaPorIdQuery, SolicitacaoAssinaturaDetalheDto?>
    {
        private readonly ContextAplicativo _context;
        public ObterSolicitacaoAssinaturaPorIdQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<SolicitacaoAssinaturaDetalheDto?> Handle(ObterSolicitacaoAssinaturaPorIdQuery request, CancellationToken ct)
        {
            var s = await _context.SolicitacoesAssinatura.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (s == null) return null;
            var sigs = await _context.SignatariosAssinatura.AsNoTracking()
                .Where(x => x.SolicitacaoId == s.Id).OrderBy(x => x.Ordem)
                .Select(x => new SignatarioDto(x.Id, x.Ordem, x.Obrigatorio, x.Externo, x.Nome, x.Identificacao,
                    x.Status, x.Externo && x.LinkToken != null && x.RevogadoEm == null))
                .ToListAsync(ct);
            var dto = new SolicitacaoAssinaturaDto(s.Id, s.DocumentoId, s.VersaoDocumento, s.Estado, s.LinkPublico,
                s.MinAssinaturas, s.ExigeOrdem, s.TipoAssinatura, s.CriadoEm);
            return new SolicitacaoAssinaturaDetalheDto(dto, sigs, sigs.Count(x => x.Status == "Assinado"));
        }
    }
}
