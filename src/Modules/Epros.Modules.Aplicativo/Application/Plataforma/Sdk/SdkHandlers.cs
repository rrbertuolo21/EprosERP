using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Sdk;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Plataforma.Sdk
{
    /// <summary>
    /// PLT · SDK/EXTENSÕES (recorte) — chaves de API públicas com escopos. Segredo devolvido uma vez;
    /// só o hash é persistido. Validação por hash + expiração + revogação + escopo.
    /// </summary>
    public class GerarChaveApiCommandHandler : ICommandHandler<GerarChaveApiCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public GerarChaveApiCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(GerarChaveApiCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var invalidos = request.Escopos.Where(e => !EscoposApi.EhValido(e)).ToList();
            if (invalidos.Any())
                return CommandResult.Falha($"Escopos inválidos: {string.Join(", ", invalidos)}. Válidos: {string.Join(", ", EscoposApi.Disponiveis)}");

            // Gera segredo aleatório. Formato: epk_<base64url(24 bytes)>. Nunca persistido em claro.
            var bytes = RandomNumberGenerator.GetBytes(24);
            var corpo = Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
            var segredo = $"epk_{corpo}";
            var prefixo = segredo.Substring(0, Math.Min(12, segredo.Length));
            var hash = HashChave(segredo);

            var chave = new ChaveApiPublica(request.Nome, prefixo, hash,
                JsonSerializer.Serialize(request.Escopos), request.ExpiraEm, tenantId, usuario);
            if (!chave.IsValid) return CommandResult.Falha(chave.Notifications.Select(n => n.Message));

            _context.ChavesApi.Add(chave);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Plataforma.SdkChaveApiGerada,
                JsonSerializer.Serialize(new { chave.Id, chave.Prefixo })));

            await _context.SaveChangesAsync(ct);
            // Segredo devolvido UMA ÚNICA VEZ — o chamador deve guardá-lo; não é recuperável depois.
            return CommandResult.Ok("Chave gerada. Guarde o segredo — ele não será exibido novamente.",
                new { ChaveId = chave.Id, chave.Prefixo, Segredo = segredo });
        }

        internal static string HashChave(string segredo)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(segredo))).ToLowerInvariant();
    }

    public class RevogarChaveApiCommandHandler : ICommandHandler<RevogarChaveApiCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public RevogarChaveApiCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(RevogarChaveApiCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var chave = await _context.ChavesApi.FirstOrDefaultAsync(c => c.Id == request.ChaveId, ct);
            if (chave == null) return CommandResult.Falha("Chave não encontrada.");
            if (chave.RevogadaEm != null) return CommandResult.Falha("Chave já revogada.");

            chave.Revogar(usuario);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Plataforma.SdkChaveApiRevogada,
                JsonSerializer.Serialize(new { chave.Id, chave.Prefixo })));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Chave revogada.");
        }
    }

    public class RegistrarUsoChaveApiCommandHandler : ICommandHandler<RegistrarUsoChaveApiCommand>
    {
        private readonly ContextAplicativo _context;

        public RegistrarUsoChaveApiCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(RegistrarUsoChaveApiCommand request, CancellationToken ct)
        {
            var chave = await _context.ChavesApi.FirstOrDefaultAsync(c => c.Id == request.ChaveId, ct);
            if (chave == null) return CommandResult.Falha("Chave não encontrada.");
            chave.RegistrarUso();
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Uso registrado.");
        }
    }

    // ===================== Queries =====================

    public class ObterChavesApiQueryHandler : IQueryHandler<ObterChavesApiQuery, IReadOnlyList<ChaveApiDto>>
    {
        private readonly ContextAplicativo _context;
        public ObterChavesApiQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IReadOnlyList<ChaveApiDto>> Handle(ObterChavesApiQuery request, CancellationToken ct)
        {
            var q = _context.ChavesApi.AsNoTracking().AsQueryable();
            if (request.ApenasAtivas) q = q.Where(c => c.Ativa);
            var lista = await q.OrderByDescending(c => c.CriadoEm).ToListAsync(ct);
            // Nunca expõe hash nem segredo.
            return lista.Select(c => new ChaveApiDto(c.Id, c.Nome, c.Prefixo,
                JsonSerializer.Deserialize<List<string>>(c.EscoposJson) ?? new(),
                c.Ativa, c.ExpiraEm, c.UltimoUsoEm, c.RevogadaEm, c.CriadoEm)).ToList();
        }
    }

    public class ObterEscoposDisponiveisQueryHandler : IQueryHandler<ObterEscoposDisponiveisQuery, IReadOnlyList<string>>
    {
        public Task<IReadOnlyList<string>> Handle(ObterEscoposDisponiveisQuery request, CancellationToken ct)
            => Task.FromResult(EscoposApi.Disponiveis);
    }

    public class ValidarChaveApiQueryHandler : IQueryHandler<ValidarChaveApiQuery, ResultadoValidacaoChaveDto>
    {
        private readonly ContextAplicativo _context;
        public ValidarChaveApiQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<ResultadoValidacaoChaveDto> Handle(ValidarChaveApiQuery request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Chave))
                return new ResultadoValidacaoChaveDto(false, null, "Chave não informada.");

            var hash = GerarChaveApiCommandHandler.HashChave(request.Chave);
            var chave = await _context.ChavesApi.AsNoTracking().FirstOrDefaultAsync(c => c.HashChave == hash, ct);
            if (chave == null) return new ResultadoValidacaoChaveDto(false, null, "Chave inválida.");
            if (!chave.Valida) return new ResultadoValidacaoChaveDto(false, chave.Id, "Chave revogada ou expirada.");

            var escopos = JsonSerializer.Deserialize<List<string>>(chave.EscoposJson) ?? new();
            if (!string.IsNullOrWhiteSpace(request.EscopoRequerido) && !escopos.Contains(request.EscopoRequerido))
                return new ResultadoValidacaoChaveDto(false, chave.Id, $"Escopo '{request.EscopoRequerido}' não concedido.");

            return new ResultadoValidacaoChaveDto(true, chave.Id, null);
        }
    }
}
