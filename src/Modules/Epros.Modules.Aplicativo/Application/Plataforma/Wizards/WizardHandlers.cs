using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Wizards;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Plataforma.Wizards
{
    /// <summary>
    /// PLT · WIZARDS (PD-04) — motor de assistente: definição (etapas/campos), execução por tenant,
    /// canal público sanitizado, conclusão idempotente que emite evento para conversão a N destinos.
    /// </summary>
    internal static class Sanitizador
    {
        private static readonly Regex Tags = new("<[^>]*>", RegexOptions.Compiled);
        /// <summary>Remove tags HTML/embed (script/iframe) — canal público (PD-04).</summary>
        public static string Limpar(string? valor)
            => valor == null ? string.Empty : Tags.Replace(valor, string.Empty).Trim();
    }

    public class CriarDefinicaoWizardCommandHandler : ICommandHandler<CriarDefinicaoWizardCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public CriarDefinicaoWizardCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(CriarDefinicaoWizardCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            if (await _context.DefinicoesWizard.AnyAsync(d => d.Codigo == request.Codigo, ct))
                return CommandResult.Falha("Já existe wizard com este código.");

            var def = new DefinicaoWizard(request.Codigo, request.Nome, request.Descricao, request.Publico, tenantId, usuario);
            if (!def.IsValid) return CommandResult.Falha(def.Notifications.Select(n => n.Message));

            _context.DefinicoesWizard.Add(def);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Wizard criado (inativo — publique após montar as etapas).", new { def.Id });
        }
    }

    public class AdicionarEtapaWizardCommandHandler : ICommandHandler<AdicionarEtapaWizardCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public AdicionarEtapaWizardCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(AdicionarEtapaWizardCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            if (!await _context.DefinicoesWizard.AnyAsync(d => d.Id == request.DefinicaoId, ct))
                return CommandResult.Falha("Wizard não encontrado.");
            if (await _context.EtapasWizard.AnyAsync(e => e.DefinicaoId == request.DefinicaoId && e.Ordem == request.Ordem, ct))
                return CommandResult.Falha("Já existe etapa com esta ordem no wizard.");

            var etapa = new EtapaWizard(request.DefinicaoId, request.Ordem, request.Titulo, request.Descricao, tenantId, usuario);
            if (!etapa.IsValid) return CommandResult.Falha(etapa.Notifications.Select(n => n.Message));

            _context.EtapasWizard.Add(etapa);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Etapa adicionada.", new { etapa.Id });
        }
    }

    public class AdicionarCampoWizardCommandHandler : ICommandHandler<AdicionarCampoWizardCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public AdicionarCampoWizardCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(AdicionarCampoWizardCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            if (!await _context.EtapasWizard.AnyAsync(e => e.Id == request.EtapaId, ct))
                return CommandResult.Falha("Etapa não encontrada.");
            if (await _context.CamposWizard.AnyAsync(c => c.EtapaId == request.EtapaId && c.Chave == request.Chave, ct))
                return CommandResult.Falha("Já existe campo com esta chave na etapa.");

            var campo = new CampoWizard(request.EtapaId, request.Chave, request.Rotulo, request.Tipo,
                request.Obrigatorio, request.OpcoesJson, request.Ordem, tenantId, usuario);
            if (!campo.IsValid) return CommandResult.Falha(campo.Notifications.Select(n => n.Message));

            _context.CamposWizard.Add(campo);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Campo adicionado.", new { campo.Id });
        }
    }

    public class PublicarDefinicaoWizardCommandHandler : ICommandHandler<PublicarDefinicaoWizardCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public PublicarDefinicaoWizardCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(PublicarDefinicaoWizardCommand request, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            var def = await _context.DefinicoesWizard.FirstOrDefaultAsync(d => d.Id == request.DefinicaoId, ct);
            if (def == null) return CommandResult.Falha("Wizard não encontrado.");

            var temEtapa = await _context.EtapasWizard.AnyAsync(e => e.DefinicaoId == def.Id, ct);
            if (!temEtapa) return CommandResult.Falha("Wizard sem etapas não pode ser publicado.");

            def.Publicar(usuario);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Wizard publicado.");
        }
    }

    public class IniciarExecucaoWizardCommandHandler : ICommandHandler<IniciarExecucaoWizardCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public IniciarExecucaoWizardCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(IniciarExecucaoWizardCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var def = await _context.DefinicoesWizard.FirstOrDefaultAsync(d => d.Id == request.DefinicaoId, ct);
            if (def == null) return CommandResult.Falha("Wizard não encontrado.");
            if (!def.Ativo) return CommandResult.Falha("Wizard não está publicado.");
            if (request.CanalPublico && !def.Publico)
                return CommandResult.Falha("Este wizard não permite canal público.");

            var token = request.CanalPublico ? Guid.NewGuid().ToString("N") : null;
            var exec = new ExecucaoWizard(def.Id, request.CanalPublico, token, tenantId, usuario);
            if (!exec.IsValid) return CommandResult.Falha(exec.Notifications.Select(n => n.Message));

            _context.ExecucoesWizard.Add(exec);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Execução iniciada.", new { ExecucaoId = exec.Id, TokenPublico = token });
        }
    }

    public class ResponderEtapaWizardCommandHandler : ICommandHandler<ResponderEtapaWizardCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public ResponderEtapaWizardCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(ResponderEtapaWizardCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var exec = await _context.ExecucoesWizard.FirstOrDefaultAsync(e => e.Id == request.ExecucaoId, ct);
            if (exec == null) return CommandResult.Falha("Execução não encontrada.");
            if (!exec.EmAndamento) return CommandResult.Falha($"Execução não está em andamento (status {exec.Status}).");

            var etapa = await _context.EtapasWizard
                .FirstOrDefaultAsync(e => e.DefinicaoId == exec.DefinicaoId && e.Ordem == exec.EtapaAtualOrdem, ct);
            if (etapa == null) return CommandResult.Falha("Etapa atual não encontrada.");

            var campos = await _context.CamposWizard.Where(c => c.EtapaId == etapa.Id).ToListAsync(ct);

            // Sanitiza respostas (canal público) e valida obrigatórios da etapa atual.
            var respostasSanitizadas = new Dictionary<string, string>();
            foreach (var kv in request.Respostas ?? new())
                respostasSanitizadas[kv.Key] = Sanitizador.Limpar(kv.Value);

            var faltando = campos.Where(c => c.Obrigatorio &&
                (!respostasSanitizadas.TryGetValue(c.Chave, out var v) || string.IsNullOrWhiteSpace(v)))
                .Select(c => c.Rotulo).ToList();
            if (faltando.Any())
                return CommandResult.Falha($"Campos obrigatórios não preenchidos: {string.Join(", ", faltando)}");

            // Acumula sobre as respostas já existentes.
            var acumulado = string.IsNullOrWhiteSpace(exec.RespostasJson)
                ? new Dictionary<string, string>()
                : JsonSerializer.Deserialize<Dictionary<string, string>>(exec.RespostasJson) ?? new();
            foreach (var kv in respostasSanitizadas) acumulado[kv.Key] = kv.Value;
            var acumuladoJson = JsonSerializer.Serialize(acumulado);

            var ultimaOrdem = await _context.EtapasWizard.Where(e => e.DefinicaoId == exec.DefinicaoId).MaxAsync(e => e.Ordem, ct);

            if (exec.EtapaAtualOrdem >= ultimaOrdem)
            {
                exec.Concluir(acumuladoJson, usuario);
                // Conversão idempotente para N destinos: o evento carrega a chave da execução; os
                // módulos donos consomem e aplicam o efeito (a plataforma não grava no domínio deles).
                _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                    CatalogoEventosIntegracao.Plataforma.WizardExecucaoConcluida,
                    JsonSerializer.Serialize(new { ExecucaoId = exec.Id, exec.DefinicaoId, Respostas = acumulado })));
                await _context.SaveChangesAsync(ct);
                return CommandResult.Ok("Wizard concluído.", new { Concluida = true });
            }

            // Próxima etapa existente (pode haver saltos de ordem).
            var proxima = await _context.EtapasWizard
                .Where(e => e.DefinicaoId == exec.DefinicaoId && e.Ordem > exec.EtapaAtualOrdem)
                .OrderBy(e => e.Ordem).Select(e => e.Ordem).FirstAsync(ct);
            exec.RegistrarRespostas(acumuladoJson, proxima, usuario);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Etapa registrada.", new { ProximaEtapa = proxima });
        }
    }

    // ===================== Queries =====================

    public class ObterDefinicoesWizardQueryHandler : IQueryHandler<ObterDefinicoesWizardQuery, IReadOnlyList<DefinicaoWizardDto>>
    {
        private readonly ContextAplicativo _context;
        public ObterDefinicoesWizardQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IReadOnlyList<DefinicaoWizardDto>> Handle(ObterDefinicoesWizardQuery request, CancellationToken ct)
        {
            var q = _context.DefinicoesWizard.AsNoTracking().AsQueryable();
            if (request.ApenasAtivos) q = q.Where(d => d.Ativo);
            return await q.OrderBy(d => d.Nome)
                .Select(d => new DefinicaoWizardDto(d.Id, d.Codigo, d.Nome, d.Descricao, d.Publico, d.Ativo)).ToListAsync(ct);
        }
    }

    public class ObterDefinicaoWizardPorIdQueryHandler : IQueryHandler<ObterDefinicaoWizardPorIdQuery, DefinicaoWizardDetalheDto?>
    {
        private readonly ContextAplicativo _context;
        public ObterDefinicaoWizardPorIdQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<DefinicaoWizardDetalheDto?> Handle(ObterDefinicaoWizardPorIdQuery request, CancellationToken ct)
        {
            var d = await _context.DefinicoesWizard.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (d == null) return null;
            var etapas = await _context.EtapasWizard.AsNoTracking()
                .Where(e => e.DefinicaoId == d.Id).OrderBy(e => e.Ordem).ToListAsync(ct);
            var etapaIds = etapas.Select(e => e.Id).ToList();
            var campos = await _context.CamposWizard.AsNoTracking()
                .Where(c => etapaIds.Contains(c.EtapaId)).OrderBy(c => c.Ordem).ToListAsync(ct);

            var etapasDto = etapas.Select(e => new EtapaWizardDto(e.Id, e.Ordem, e.Titulo, e.Descricao,
                campos.Where(c => c.EtapaId == e.Id)
                    .Select(c => new CampoWizardDto(c.Id, c.Chave, c.Rotulo, c.Tipo, c.Obrigatorio, c.OpcoesJson, c.Ordem)).ToList()))
                .ToList();

            return new DefinicaoWizardDetalheDto(
                new DefinicaoWizardDto(d.Id, d.Codigo, d.Nome, d.Descricao, d.Publico, d.Ativo), etapasDto);
        }
    }

    public class ObterExecucoesWizardQueryHandler : IQueryHandler<ObterExecucoesWizardQuery, IReadOnlyList<ExecucaoWizardDto>>
    {
        private readonly ContextAplicativo _context;
        public ObterExecucoesWizardQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IReadOnlyList<ExecucaoWizardDto>> Handle(ObterExecucoesWizardQuery request, CancellationToken ct)
        {
            var q = _context.ExecucoesWizard.AsNoTracking().AsQueryable();
            if (request.DefinicaoId.HasValue) q = q.Where(e => e.DefinicaoId == request.DefinicaoId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status)) q = q.Where(e => e.Status == request.Status);
            return await q.OrderByDescending(e => e.CriadoEm)
                .Select(e => new ExecucaoWizardDto(e.Id, e.DefinicaoId, e.Status, e.EtapaAtualOrdem, e.CanalPublico, e.CriadoEm))
                .ToListAsync(ct);
        }
    }
}
