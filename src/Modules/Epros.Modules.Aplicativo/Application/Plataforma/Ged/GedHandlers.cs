using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Contracts;
using Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Ged;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Plataforma.Ged
{
    /// <summary>
    /// PLT · GED (T10) — handlers do repositório documental canônico único: registro com hash/dedupe,
    /// versionamento, vínculo a entidades de negócio, retenção legal (NF-04) e bloqueio de hash.
    /// Eventos pós-commit via Outbox (plt.ged.*). Storage atrás de <see cref="IArmazenamentoDocumentoService"/>.
    /// </summary>
    public class RegistrarDocumentoGedCommandHandler : ICommandHandler<RegistrarDocumentoGedCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        private readonly IArmazenamentoDocumentoService _storage;

        public RegistrarDocumentoGedCommandHandler(ContextAplicativo context, ITenantProvider tenant,
            ICurrentUser user, IArmazenamentoDocumentoService storage)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
            _storage = storage;
        }

        public async Task<CommandResult> Handle(RegistrarDocumentoGedCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            string hash;
            long tamanho;
            byte[]? conteudo = null;
            if (!string.IsNullOrWhiteSpace(request.ConteudoBase64))
            {
                try { conteudo = Convert.FromBase64String(request.ConteudoBase64); }
                catch (FormatException) { return CommandResult.Falha("Conteúdo base64 inválido."); }
                using var sha = SHA256.Create();
                hash = Convert.ToHexString(sha.ComputeHash(conteudo)).ToLowerInvariant();
                tamanho = conteudo.LongLength;
            }
            else
            {
                hash = request.HashInformado!.Trim().ToLowerInvariant();
                tamanho = request.TamanhoBytes;
            }

            // REG-013/REG-056: hash bloqueado impede o upload.
            var bloqueado = await _context.HashesBloqueadosGed.AnyAsync(h => h.Hash == hash, ct);
            if (bloqueado)
                return CommandResult.Falha("Conteúdo bloqueado por moderação/segurança (hash bloqueado).", block: true);

            // REG-020: dedupe físico — reaproveita o StorageRef de um registro ativo com mesmo hash+tamanho.
            var existente = await _context.DocumentosGed
                .Where(d => d.Hash == hash && d.TamanhoBytes == tamanho)
                .OrderBy(d => d.CriadoEm)
                .FirstOrDefaultAsync(ct);

            string? storageRef = existente?.StorageRef;
            if (storageRef == null && conteudo != null)
                storageRef = await _storage.ArmazenarAsync(conteudo, $"{tenantId}/{hash}", ct);

            var doc = new DocumentoGed(
                request.Nome, request.TipoDocumento, hash, tamanho, tenantId, usuario,
                request.MimeType, storageRef, request.RequerAssinatura,
                request.ModuloOrigem, request.EntidadeOrigemTipo, request.EntidadeOrigemId);

            _context.DocumentosGed.Add(doc);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Plataforma.GedDocumentoRegistrado,
                JsonSerializer.Serialize(new { doc.Id, doc.TipoDocumento, doc.Hash, deduplicado = existente != null })));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Documento registrado no GED canônico.",
                new { DocumentoId = doc.Id, doc.Hash, Deduplicado = existente != null, ProviderStorage = _storage.ProviderConfigurado });
        }
    }

    public class RegistrarNovaVersaoDocumentoGedCommandHandler : ICommandHandler<RegistrarNovaVersaoDocumentoGedCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        private readonly IArmazenamentoDocumentoService _storage;

        public RegistrarNovaVersaoDocumentoGedCommandHandler(ContextAplicativo context, ITenantProvider tenant,
            ICurrentUser user, IArmazenamentoDocumentoService storage)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
            _storage = storage;
        }

        public async Task<CommandResult> Handle(RegistrarNovaVersaoDocumentoGedCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var doc = await _context.DocumentosGed.FirstOrDefaultAsync(d => d.Id == request.DocumentoId, ct);
            if (doc == null) return CommandResult.Falha("Documento não encontrado.");

            string hash;
            long tamanho;
            string? storageRef = doc.StorageRef;
            if (!string.IsNullOrWhiteSpace(request.ConteudoBase64))
            {
                byte[] conteudo;
                try { conteudo = Convert.FromBase64String(request.ConteudoBase64); }
                catch (FormatException) { return CommandResult.Falha("Conteúdo base64 inválido."); }
                using var sha = SHA256.Create();
                hash = Convert.ToHexString(sha.ComputeHash(conteudo)).ToLowerInvariant();
                tamanho = conteudo.LongLength;
                storageRef = await _storage.ArmazenarAsync(conteudo, $"{tenantId}/{hash}", ct);
            }
            else if (!string.IsNullOrWhiteSpace(request.HashInformado))
            {
                hash = request.HashInformado.Trim().ToLowerInvariant();
                tamanho = request.TamanhoBytes;
            }
            else
            {
                return CommandResult.Falha("Informe o conteúdo (base64) ou o hash + tamanho da nova versão.");
            }

            doc.RegistrarNovaVersao(hash, tamanho, storageRef, usuario);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Plataforma.GedNovaVersaoRegistrada,
                JsonSerializer.Serialize(new { doc.Id, doc.Versao, doc.Hash })));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Nova versão registrada.", new { doc.Id, doc.Versao });
        }
    }

    public class VincularDocumentoGedCommandHandler : ICommandHandler<VincularDocumentoGedCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public VincularDocumentoGedCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(VincularDocumentoGedCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var doc = await _context.DocumentosGed.AnyAsync(d => d.Id == request.DocumentoId, ct);
            if (!doc) return CommandResult.Falha("Documento não encontrado.");

            var jaVinculado = await _context.VinculosDocumentoGed.AnyAsync(v =>
                v.DocumentoId == request.DocumentoId && v.EntidadeTipo == request.EntidadeTipo &&
                v.EntidadeId == request.EntidadeId, ct);
            if (jaVinculado) return CommandResult.Falha("Documento já vinculado a esta entidade.");

            var vinculo = new VinculoDocumentoGed(request.DocumentoId, request.ModuloDestino,
                request.EntidadeTipo, request.EntidadeId, tenantId, usuario);
            if (!vinculo.IsValid) return CommandResult.Falha(vinculo.Notifications.Select(n => n.Message));

            _context.VinculosDocumentoGed.Add(vinculo);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Plataforma.GedDocumentoVinculado,
                JsonSerializer.Serialize(new { request.DocumentoId, request.ModuloDestino, request.EntidadeTipo, request.EntidadeId })));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Documento vinculado.", new { VinculoId = vinculo.Id });
        }
    }

    public class DefinirPoliticaRetencaoGedCommandHandler : ICommandHandler<DefinirPoliticaRetencaoGedCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public DefinirPoliticaRetencaoGedCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(DefinirPoliticaRetencaoGedCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var existente = await _context.PoliticasRetencaoGed
                .FirstOrDefaultAsync(p => p.TipoDocumento == request.TipoDocumento, ct);

            if (existente != null)
            {
                existente.Atualizar(request.PrazoRetencaoDias, request.BaseLegal, request.AcaoAposPrazo, request.HoldJuridico, usuario);
                if (!existente.IsValid) return CommandResult.Falha(existente.Notifications.Select(n => n.Message));
                await _context.SaveChangesAsync(ct);
                return CommandResult.Ok("Política de retenção atualizada.", new { existente.Id });
            }

            var politica = new PoliticaRetencaoGed(request.TipoDocumento, request.PrazoRetencaoDias,
                request.BaseLegal, request.AcaoAposPrazo, request.HoldJuridico, tenantId, usuario);
            if (!politica.IsValid) return CommandResult.Falha(politica.Notifications.Select(n => n.Message));

            _context.PoliticasRetencaoGed.Add(politica);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Política de retenção definida.", new { politica.Id });
        }
    }

    public class BloquearHashGedCommandHandler : ICommandHandler<BloquearHashGedCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public BloquearHashGedCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(BloquearHashGedCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";
            var hash = request.Hash.Trim().ToLowerInvariant();

            if (await _context.HashesBloqueadosGed.AnyAsync(h => h.Hash == hash, ct))
                return CommandResult.Falha("Hash já está bloqueado.");

            var bloqueio = new HashBloqueadoGed(hash, request.Motivo, tenantId, usuario);
            if (!bloqueio.IsValid) return CommandResult.Falha(bloqueio.Notifications.Select(n => n.Message));

            _context.HashesBloqueadosGed.Add(bloqueio);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Hash bloqueado.", new { bloqueio.Id });
        }
    }

    // ===================== Queries =====================

    public class ObterDocumentosGedQueryHandler : IQueryHandler<ObterDocumentosGedQuery, IReadOnlyList<DocumentoGedDto>>
    {
        private readonly ContextAplicativo _context;
        public ObterDocumentosGedQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IReadOnlyList<DocumentoGedDto>> Handle(ObterDocumentosGedQuery request, CancellationToken ct)
        {
            var q = _context.DocumentosGed.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.TipoDocumento)) q = q.Where(d => d.TipoDocumento == request.TipoDocumento);
            if (!string.IsNullOrWhiteSpace(request.EntidadeOrigemTipo)) q = q.Where(d => d.EntidadeOrigemTipo == request.EntidadeOrigemTipo);
            if (!string.IsNullOrWhiteSpace(request.EntidadeOrigemId)) q = q.Where(d => d.EntidadeOrigemId == request.EntidadeOrigemId);

            return await q.OrderByDescending(d => d.CriadoEm).Select(Projecao()).ToListAsync(ct);
        }

        internal static System.Linq.Expressions.Expression<Func<DocumentoGed, DocumentoGedDto>> Projecao() =>
            d => new DocumentoGedDto(d.Id, d.Nome, d.TipoDocumento, d.Hash, d.TamanhoBytes, d.MimeType, d.Versao,
                d.StorageRef, d.StatusAssinatura.ToString(), d.ModuloOrigem, d.EntidadeOrigemTipo, d.EntidadeOrigemId, d.CriadoEm);
    }

    public class ObterDocumentoGedPorIdQueryHandler : IQueryHandler<ObterDocumentoGedPorIdQuery, DocumentoGedDto?>
    {
        private readonly ContextAplicativo _context;
        public ObterDocumentoGedPorIdQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<DocumentoGedDto?> Handle(ObterDocumentoGedPorIdQuery request, CancellationToken ct)
            => await _context.DocumentosGed.AsNoTracking().Where(d => d.Id == request.Id)
                .Select(ObterDocumentosGedQueryHandler.Projecao()).FirstOrDefaultAsync(ct);
    }

    public class BuscarDocumentosGedQueryHandler : IQueryHandler<BuscarDocumentosGedQuery, IReadOnlyList<DocumentoGedDto>>
    {
        private readonly ContextAplicativo _context;
        public BuscarDocumentosGedQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IReadOnlyList<DocumentoGedDto>> Handle(BuscarDocumentosGedQuery request, CancellationToken ct)
        {
            var termo = (request.Termo ?? string.Empty).Trim().ToLower();
            return await _context.DocumentosGed.AsNoTracking()
                .Where(d => d.Nome.ToLower().Contains(termo) || d.TipoDocumento.ToLower().Contains(termo))
                .OrderByDescending(d => d.CriadoEm)
                .Select(ObterDocumentosGedQueryHandler.Projecao()).ToListAsync(ct);
        }
    }

    public class ObterDocumentosRetencaoVencidaQueryHandler
        : IQueryHandler<ObterDocumentosRetencaoVencidaQuery, IReadOnlyList<DocumentoRetencaoVencidaDto>>
    {
        private readonly ContextAplicativo _context;
        public ObterDocumentosRetencaoVencidaQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IReadOnlyList<DocumentoRetencaoVencidaDto>> Handle(ObterDocumentosRetencaoVencidaQuery request, CancellationToken ct)
        {
            var agora = DateTime.UtcNow;
            // Junta documentos com a política do seu tipo; vencido = CriadoEm + prazo < agora e sem hold jurídico.
            var query =
                from d in _context.DocumentosGed.AsNoTracking()
                join p in _context.PoliticasRetencaoGed.AsNoTracking() on d.TipoDocumento equals p.TipoDocumento
                where !p.HoldJuridico && p.AcaoAposPrazo != "Reter"
                select new { d, p };

            var candidatos = await query.ToListAsync(ct);
            return candidatos
                .Select(x => new { x.d, x.p, VenceEm = x.d.CriadoEm.AddDays(x.p.PrazoRetencaoDias) })
                .Where(x => x.VenceEm < agora)
                .Select(x => new DocumentoRetencaoVencidaDto(x.d.Id, x.d.Nome, x.d.TipoDocumento, x.d.CriadoEm,
                    x.p.PrazoRetencaoDias, x.VenceEm, x.p.AcaoAposPrazo, x.p.BaseLegal))
                .ToList();
        }
    }
}
