using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.RH.Application.Commands;
using Epros.Modules.RH.Application.Queries;
using Epros.Modules.RH.Domain.Entities;
using Epros.Modules.RH.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.RH.Application.Handlers
{
    public class CriarVagaCommandHandler : ICommandHandler<CriarVagaCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public CriarVagaCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context; _tenant = tenant; _user = user;
        }

        public async Task<CommandResult> Handle(CriarVagaCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            // REC-REG-017/018: codigos sequenciais por tenant (e por ano no publico).
            var total = await _context.RecVagas.CountAsync(ct);
            var codigoInterno = $"JOB{(total + 1):D4}";
            var codigoPublico = $"JP{DateTime.UtcNow:yyyy}{(total + 1):D4}";

            var vaga = new RecVaga(
                codigoInterno, codigoPublico, request.Titulo, request.Posicoes, request.Prioridade,
                null, null, null, null, request.Descricao, null, request.Habilidades, null, null, false,
                null, false, null, false, RecVaga.StRascunho, request.TipoCandidatura, request.UrlCandidatura,
                null, null, request.FilialId, request.TipoVagaId, request.LocalVagaId, null,
                request.CriadoPorUsuarioId, request.DonoFuncionalId, tenantId, usuario);

            if (!vaga.IsValid)
                return CommandResult.Falha(vaga.Notifications.Select(n => n.Message));

            _context.RecVagas.Add(vaga);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Vaga criada em rascunho.", new { VagaId = vaga.Id, vaga.CodigoInterno, vaga.CodigoPublico });
        }
    }

    public class PublicarVagaCommandHandler : ICommandHandler<PublicarVagaCommand>
    {
        private readonly ContextRH _context;
        private readonly ICurrentUser _user;

        public PublicarVagaCommandHandler(ContextRH context, ICurrentUser user)
        {
            _context = context; _user = user;
        }

        public async Task<CommandResult> Handle(PublicarVagaCommand request, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            var vaga = await _context.RecVagas.FirstOrDefaultAsync(v => v.Id == request.VagaId, ct);
            if (vaga == null) return CommandResult.Falha("Vaga nao encontrada.");

            vaga.Publicar(usuario);
            if (!vaga.IsValid)
                return CommandResult.Falha(vaga.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Vaga publicada.", new { vaga.Id, vaga.Status, vaga.Publicada });
        }
    }

    public class RegistrarCandidaturaCommandHandler : ICommandHandler<RegistrarCandidaturaCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public RegistrarCandidaturaCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context; _tenant = tenant; _user = user;
        }

        public async Task<CommandResult> Handle(RegistrarCandidaturaCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            // REC-REG-020: vaga precisa estar publicada e ativa.
            var vaga = await _context.RecVagas.FirstOrDefaultAsync(v => v.Id == request.VagaId, ct);
            if (vaga == null) return CommandResult.Falha("Vaga nao encontrada.");
            if (!vaga.Publicada || vaga.Status != RecVaga.StAtiva)
                return CommandResult.Falha("A vaga precisa estar publicada e ativa para receber candidaturas (REC-REG-020).");

            // REC-REG-022: e-mail unico entre candidatos.
            var emailExiste = await _context.RecCandidatos.AnyAsync(c => c.Email == request.Email, ct);
            if (emailExiste)
                return CommandResult.Falha("Ja existe candidato com este e-mail.");

            // REC-REG-025: protocolo por tenant e ano com sequencial.
            var total = await _context.RecCandidatos.CountAsync(ct);
            var protocolo = $"{DateTime.UtcNow:yyyy}{(total + 1):D6}";

            var candidato = new RecCandidato(
                request.PrimeiroNome, request.Sobrenome, request.Email, null, null, null, null, null, null,
                null, null, request.AnosExperiencia, null, null, null, null, null, null, null, null, null, null,
                "0", DateTime.UtcNow, null, protocolo, request.VagaId, null, request.FonteCandidatoId,
                request.CriadoPorUsuarioId, request.DonoFuncionalId, tenantId, usuario);

            if (!candidato.IsValid)
                return CommandResult.Falha(candidato.Notifications.Select(n => n.Message));

            _context.RecCandidatos.Add(candidato);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Candidatura registrada.", new { CandidatoId = candidato.Id, ProtocoloRastreio = protocolo });
        }
    }

    public class RegistrarFeedbackEntrevistaCommandHandler : ICommandHandler<RegistrarFeedbackEntrevistaCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public RegistrarFeedbackEntrevistaCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context; _tenant = tenant; _user = user;
        }

        public async Task<CommandResult> Handle(RegistrarFeedbackEntrevistaCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var entrevista = await _context.RecEntrevistas.FirstOrDefaultAsync(e => e.Id == request.EntrevistaId, ct);
            if (entrevista == null) return CommandResult.Falha("Entrevista nao encontrada.");

            // REC-REG-036: feedback so para entrevista concluida.
            if (entrevista.Status != RecEntrevista.StConcluida)
                return CommandResult.Falha("Feedback so pode ser registrado para entrevista concluida (REC-REG-036).");

            // REC-REG-037: nota geral = media das tres notas.
            var notaGeral = RecFeedbackEntrevista.CalcularNotaGeral(
                request.NotaTecnica, request.NotaComunicacao, request.NotaAderenciaCultural);

            var feedback = new RecFeedbackEntrevista(
                request.NotaTecnica, request.NotaComunicacao, request.NotaAderenciaCultural, notaGeral,
                request.PontosFortes, request.PontosFracos, request.Comentarios, request.Recomendacao,
                request.EntrevistaId, request.EntrevistadoresJson, request.CriadoPorUsuarioId,
                request.DonoFuncionalId, tenantId, usuario);

            if (!feedback.IsValid)
                return CommandResult.Falha(feedback.Notifications.Select(n => n.Message));

            // REC-REG-038: marca entrevista como feedback enviado.
            entrevista.MarcarFeedbackEnviado(usuario);

            _context.RecFeedbackEntrevistas.Add(feedback);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Feedback registrado.", new { FeedbackId = feedback.Id, NotaGeral = notaGeral });
        }
    }

    public class ListarVagasQueryHandler : IQueryHandler<ListarVagasQuery, CommandResult>
    {
        private readonly ContextRH _context;
        public ListarVagasQueryHandler(ContextRH context) => _context = context;

        public async Task<CommandResult> Handle(ListarVagasQuery request, CancellationToken ct)
        {
            var itens = await _context.RecVagas.OrderByDescending(v => v.CriadoEm).ToListAsync(ct);
            return CommandResult.Ok("Vagas listadas.", itens);
        }
    }

    public class ListarCandidatosQueryHandler : IQueryHandler<ListarCandidatosQuery, CommandResult>
    {
        private readonly ContextRH _context;
        public ListarCandidatosQueryHandler(ContextRH context) => _context = context;

        public async Task<CommandResult> Handle(ListarCandidatosQuery request, CancellationToken ct)
        {
            var itens = await _context.RecCandidatos.OrderByDescending(c => c.DataCandidatura).ToListAsync(ct);
            return CommandResult.Ok("Candidatos listados.", itens);
        }
    }
}
