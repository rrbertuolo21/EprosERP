using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Projetos.Application.Commands;
using Epros.Modules.Projetos.Domain.Entities.Encerramento;
using Epros.Modules.Projetos.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Projetos.Application.Handlers
{
    // ===== PRJ-ENC (Encerramento de Projeto) =====

    public class CriarEncerramentoProjetoCommandHandler : ICommandHandler<CriarEncerramentoProjetoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarEncerramentoProjetoCommandHandler(ContextProjetos context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarEncerramentoProjetoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // RN-ENC-002: codigo unico por tenant.
            var codigoExiste = await _context.Encerramentos
                .AnyAsync(e => e.Codigo == request.Codigo, cancellationToken);
            if (codigoExiste)
                return CommandResult.Falha("Ja existe encerramento com este codigo para o tenant.");

            var encerramento = new EncerramentoProjeto(request.ProjetoId, request.Codigo, request.Descricao, request.ResponsavelId, tenantId, usuario);
            if (!encerramento.IsValid)
                return CommandResult.Falha(encerramento.Notifications.Select(n => n.Message));

            _context.Encerramentos.Add(encerramento);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Encerramento criado com sucesso!", new { encerramento.Id });
        }
    }

    public class AdicionarItemEncerramentoCommandHandler : ICommandHandler<AdicionarItemEncerramentoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public AdicionarItemEncerramentoCommandHandler(ContextProjetos context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarItemEncerramentoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var encerramento = await _context.Encerramentos
                .Include(e => e.Itens)
                .FirstOrDefaultAsync(e => e.Id == request.EncerramentoId, cancellationToken);
            if (encerramento == null)
                return CommandResult.Falha("Encerramento nao encontrado.");

            encerramento.AdicionarItem(request.Sequencia, request.Quantidade, request.Observacao, usuario);
            if (!encerramento.IsValid)
                return CommandResult.Falha(encerramento.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Item de encerramento adicionado com sucesso!", new { encerramento.Id });
        }
    }

    public class AnexarDocumentoEncerramentoCommandHandler : ICommandHandler<AnexarDocumentoEncerramentoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public AnexarDocumentoEncerramentoCommandHandler(ContextProjetos context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AnexarDocumentoEncerramentoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var encerramento = await _context.Encerramentos
                .Include(e => e.Anexos)
                .FirstOrDefaultAsync(e => e.Id == request.EncerramentoId, cancellationToken);
            if (encerramento == null)
                return CommandResult.Falha("Encerramento nao encontrado.");

            encerramento.AnexarDocumento(request.ArquivoId, usuario);
            if (!encerramento.IsValid)
                return CommandResult.Falha(encerramento.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Documento anexado com sucesso!", new { encerramento.Id });
        }
    }

    public abstract class EncerramentoWorkflowHandlerBase
    {
        protected readonly ContextProjetos Context;
        protected readonly ICurrentUser CurrentUser;

        protected EncerramentoWorkflowHandlerBase(ContextProjetos context, ICurrentUser currentUser)
        {
            Context = context;
            CurrentUser = currentUser;
        }

        protected async Task<(EncerramentoProjeto? enc, Guid usuarioId, string usuario)> CarregarAsync(Guid id, CancellationToken ct)
        {
            var usuario = CurrentUser.GetUserId() ?? "system";
            Guid.TryParse(usuario, out var usuarioId);
            var enc = await Context.Encerramentos
                .Include(e => e.Historicos)
                .FirstOrDefaultAsync(e => e.Id == id, ct);
            return (enc, usuarioId, usuario);
        }
    }

    public class SubmeterEncerramentoCommandHandler : EncerramentoWorkflowHandlerBase, ICommandHandler<SubmeterEncerramentoCommand>
    {
        public SubmeterEncerramentoCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(SubmeterEncerramentoCommand request, CancellationToken cancellationToken)
        {
            var (enc, usuarioId, usuario) = await CarregarAsync(request.EncerramentoId, cancellationToken);
            if (enc == null) return CommandResult.Falha("Encerramento nao encontrado.");
            enc.Submeter(usuarioId, usuario);
            if (!enc.IsValid) return CommandResult.Falha(enc.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Encerramento submetido para analise.", new { enc.Id });
        }
    }

    public class AprovarEncerramentoCommandHandler : EncerramentoWorkflowHandlerBase, ICommandHandler<AprovarEncerramentoCommand>
    {
        public AprovarEncerramentoCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(AprovarEncerramentoCommand request, CancellationToken cancellationToken)
        {
            var (enc, usuarioId, usuario) = await CarregarAsync(request.EncerramentoId, cancellationToken);
            if (enc == null) return CommandResult.Falha("Encerramento nao encontrado.");
            enc.Aprovar(request.StatusFinalProjeto, usuarioId, usuario);
            if (!enc.IsValid) return CommandResult.Falha(enc.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Encerramento aprovado com sucesso!", new { enc.Id, enc.StatusFinalProjeto });
        }
    }

    public class RejeitarEncerramentoCommandHandler : EncerramentoWorkflowHandlerBase, ICommandHandler<RejeitarEncerramentoCommand>
    {
        public RejeitarEncerramentoCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(RejeitarEncerramentoCommand request, CancellationToken cancellationToken)
        {
            var (enc, usuarioId, usuario) = await CarregarAsync(request.EncerramentoId, cancellationToken);
            if (enc == null) return CommandResult.Falha("Encerramento nao encontrado.");
            enc.Rejeitar(request.Motivo, usuarioId, usuario);
            if (!enc.IsValid) return CommandResult.Falha(enc.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Encerramento rejeitado.", new { enc.Id });
        }
    }

    public class SuspenderEncerramentoCommandHandler : EncerramentoWorkflowHandlerBase, ICommandHandler<SuspenderEncerramentoCommand>
    {
        public SuspenderEncerramentoCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(SuspenderEncerramentoCommand request, CancellationToken cancellationToken)
        {
            var (enc, usuarioId, usuario) = await CarregarAsync(request.EncerramentoId, cancellationToken);
            if (enc == null) return CommandResult.Falha("Encerramento nao encontrado.");
            enc.Suspender(usuarioId, usuario);
            if (!enc.IsValid) return CommandResult.Falha(enc.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Encerramento suspenso.", new { enc.Id });
        }
    }

    public class RetomarEncerramentoCommandHandler : EncerramentoWorkflowHandlerBase, ICommandHandler<RetomarEncerramentoCommand>
    {
        public RetomarEncerramentoCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(RetomarEncerramentoCommand request, CancellationToken cancellationToken)
        {
            var (enc, usuarioId, usuario) = await CarregarAsync(request.EncerramentoId, cancellationToken);
            if (enc == null) return CommandResult.Falha("Encerramento nao encontrado.");
            enc.Retomar(usuarioId, usuario);
            if (!enc.IsValid) return CommandResult.Falha(enc.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Encerramento retomado.", new { enc.Id });
        }
    }

    public class EncerrarEncerramentoCommandHandler : EncerramentoWorkflowHandlerBase, ICommandHandler<EncerrarEncerramentoCommand>
    {
        public EncerrarEncerramentoCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(EncerrarEncerramentoCommand request, CancellationToken cancellationToken)
        {
            var (enc, usuarioId, usuario) = await CarregarAsync(request.EncerramentoId, cancellationToken);
            if (enc == null) return CommandResult.Falha("Encerramento nao encontrado.");
            enc.Encerrar(request.Motivo, usuarioId, usuario);
            if (!enc.IsValid) return CommandResult.Falha(enc.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Processo de encerramento concluido.", new { enc.Id });
        }
    }

    public class ArquivarEncerramentoCommandHandler : EncerramentoWorkflowHandlerBase, ICommandHandler<ArquivarEncerramentoCommand>
    {
        public ArquivarEncerramentoCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(ArquivarEncerramentoCommand request, CancellationToken cancellationToken)
        {
            var (enc, usuarioId, usuario) = await CarregarAsync(request.EncerramentoId, cancellationToken);
            if (enc == null) return CommandResult.Falha("Encerramento nao encontrado.");
            enc.Arquivar(usuarioId, usuario);
            if (!enc.IsValid) return CommandResult.Falha(enc.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Projeto arquivado.", new { enc.Id });
        }
    }
}
