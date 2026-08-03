using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Projetos.Application.Commands;
using Epros.Modules.Projetos.Domain.Entities;
using Epros.Modules.Projetos.Domain.Entities.Definicao;
using Epros.Modules.Projetos.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Projetos.Application.Handlers
{
    public class VincularClienteProjetoCommandHandler : ICommandHandler<VincularClienteProjetoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public VincularClienteProjetoCommandHandler(ContextProjetos context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(VincularClienteProjetoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var projetoExiste = await _context.Projetos.AnyAsync(p => p.Id == request.ProjetoId, cancellationToken);
            if (!projetoExiste)
                return CommandResult.Falha("Projeto nao encontrado.");

            var vinculo = new ProjetoCliente(request.ProjetoId, request.ClienteId, tenantId, usuario);
            if (!vinculo.IsValid)
                return CommandResult.Falha(vinculo.Notifications.Select(n => n.Message));

            _context.ProjetoClientes.Add(vinculo);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Cliente vinculado ao projeto com sucesso!", new { vinculo.Id });
        }
    }

    public class VincularMembroProjetoCommandHandler : ICommandHandler<VincularMembroProjetoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public VincularMembroProjetoCommandHandler(ContextProjetos context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(VincularMembroProjetoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var projetoExiste = await _context.Projetos.AnyAsync(p => p.Id == request.ProjetoId, cancellationToken);
            if (!projetoExiste)
                return CommandResult.Falha("Projeto nao encontrado.");

            var membro = new ProjetoMembro(request.ProjetoId, request.UsuarioId, request.Papel, tenantId, usuario);
            if (!membro.IsValid)
                return CommandResult.Falha(membro.Notifications.Select(n => n.Message));

            _context.ProjetoMembros.Add(membro);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Membro vinculado ao projeto com sucesso!", new { membro.Id });
        }
    }

    public class RegistrarAtividadeProjetoCommandHandler : ICommandHandler<RegistrarAtividadeProjetoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarAtividadeProjetoCommandHandler(ContextProjetos context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarAtividadeProjetoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var atividade = new ProjetoAtividade(request.ProjetoId, request.UsuarioId, request.TipoUsuario, request.TipoAtividade, request.Observacao, tenantId, usuario);
            if (!atividade.IsValid)
                return CommandResult.Falha(atividade.Notifications.Select(n => n.Message));

            _context.ProjetoAtividades.Add(atividade);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Atividade registrada com sucesso!", new { atividade.Id });
        }
    }

    public class AnexarArquivoProjetoCommandHandler : ICommandHandler<AnexarArquivoProjetoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AnexarArquivoProjetoCommandHandler(ContextProjetos context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AnexarArquivoProjetoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var projetoExiste = await _context.Projetos.AnyAsync(p => p.Id == request.ProjetoId, cancellationToken);
            if (!projetoExiste)
                return CommandResult.Falha("Projeto nao encontrado.");

            var arquivo = new ProjetoArquivo(request.ProjetoId, request.NomeArquivo, request.CaminhoArquivo, request.ArquivoId, tenantId, usuario);
            if (!arquivo.IsValid)
                return CommandResult.Falha(arquivo.Notifications.Select(n => n.Message));

            _context.ProjetoArquivos.Add(arquivo);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Arquivo anexado ao projeto com sucesso!", new { arquivo.Id });
        }
    }

    public class DuplicarProjetoCommandHandler : ICommandHandler<DuplicarProjetoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DuplicarProjetoCommandHandler(ContextProjetos context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DuplicarProjetoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var origem = await _context.Projetos.FirstOrDefaultAsync(p => p.Id == request.ProjetoOrigemId, cancellationToken);
            if (origem == null)
                return CommandResult.Falha("Projeto de origem nao encontrado.");

            // RN-DEF-016: nome derivado do original.
            var copia = new Projeto(
                $"{origem.Nome} (Copia)",
                origem.Descricao,
                origem.ClienteId,
                origem.DataInicio,
                origem.DataTermino,
                origem.OrcamentoTotal,
                tenantId,
                usuario);

            if (!copia.IsValid)
                return CommandResult.Falha(copia.Notifications.Select(n => n.Message));

            _context.Projetos.Add(copia);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Projeto duplicado com sucesso!", new { ProjetoOrigemId = origem.Id, ProjetoDestinoId = copia.Id });
        }
    }

    // ===== MM-d: workflow do mestre (Projeto) exposto =====

    /// <summary>Base para transições de workflow do mestre. Carrega o projeto e o usuário corrente.</summary>
    public abstract class ProjetoWorkflowHandlerBase
    {
        protected readonly ContextProjetos Context;
        private readonly ICurrentUser _currentUser;
        protected ProjetoWorkflowHandlerBase(ContextProjetos context, ICurrentUser currentUser) { Context = context; _currentUser = currentUser; }

        protected async Task<(Projeto? projeto, string usuario)> CarregarAsync(Guid id, CancellationToken ct)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var projeto = await Context.Projetos.FirstOrDefaultAsync(p => p.Id == id, ct);
            return (projeto, usuario);
        }

        protected async Task<CommandResult> ExecutarAsync(Guid id, Action<Projeto, string> transicao, string ok, CancellationToken ct)
        {
            var (projeto, usuario) = await CarregarAsync(id, ct);
            if (projeto == null) return CommandResult.Falha("Projeto nao encontrado.");
            transicao(projeto, usuario);
            if (!projeto.IsValid) return CommandResult.Falha(projeto.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(ct);
            return CommandResult.Ok(ok, new { projeto.Id, Status = projeto.Status.ToString(), Tipo = projeto.Tipo.ToString() });
        }
    }

    public class SubmeterProjetoCommandHandler : ProjetoWorkflowHandlerBase, ICommandHandler<SubmeterProjetoCommand>
    {
        public SubmeterProjetoCommandHandler(ContextProjetos c, ICurrentUser u) : base(c, u) { }
        public Task<CommandResult> Handle(SubmeterProjetoCommand r, CancellationToken ct) => ExecutarAsync(r.ProjetoId, (p, u) => p.Submeter(u), "Projeto submetido para analise.", ct);
    }

    public class AprovarProjetoCommandHandler : ProjetoWorkflowHandlerBase, ICommandHandler<AprovarProjetoCommand>
    {
        public AprovarProjetoCommandHandler(ContextProjetos c, ICurrentUser u) : base(c, u) { }
        public Task<CommandResult> Handle(AprovarProjetoCommand r, CancellationToken ct) => ExecutarAsync(r.ProjetoId, (p, u) => p.Aprovar(u), "Projeto aprovado e ativado.", ct);
    }

    public class SuspenderProjetoCommandHandler : ProjetoWorkflowHandlerBase, ICommandHandler<SuspenderProjetoCommand>
    {
        public SuspenderProjetoCommandHandler(ContextProjetos c, ICurrentUser u) : base(c, u) { }
        public Task<CommandResult> Handle(SuspenderProjetoCommand r, CancellationToken ct) => ExecutarAsync(r.ProjetoId, (p, u) => p.Suspender(u), "Projeto suspenso.", ct);
    }

    public class RetomarProjetoCommandHandler : ProjetoWorkflowHandlerBase, ICommandHandler<RetomarProjetoCommand>
    {
        public RetomarProjetoCommandHandler(ContextProjetos c, ICurrentUser u) : base(c, u) { }
        public Task<CommandResult> Handle(RetomarProjetoCommand r, CancellationToken ct) => ExecutarAsync(r.ProjetoId, (p, u) => p.Retomar(u), "Projeto retomado.", ct);
    }

    public class EncerrarProjetoCommandHandler : ProjetoWorkflowHandlerBase, ICommandHandler<EncerrarProjetoCommand>
    {
        public EncerrarProjetoCommandHandler(ContextProjetos c, ICurrentUser u) : base(c, u) { }
        public Task<CommandResult> Handle(EncerrarProjetoCommand r, CancellationToken ct) => ExecutarAsync(r.ProjetoId, (p, u) => p.Encerrar(r.Motivo, u), "Projeto encerrado.", ct);
    }

    public class InativarProjetoCommandHandler : ProjetoWorkflowHandlerBase, ICommandHandler<InativarProjetoCommand>
    {
        public InativarProjetoCommandHandler(ContextProjetos c, ICurrentUser u) : base(c, u) { }
        public Task<CommandResult> Handle(InativarProjetoCommand r, CancellationToken ct) => ExecutarAsync(r.ProjetoId, (p, u) => p.Inativar(u), "Projeto inativado.", ct);
    }

    public class ConverterProjetoTemplateCommandHandler : ProjetoWorkflowHandlerBase, ICommandHandler<ConverterProjetoTemplateCommand>
    {
        public ConverterProjetoTemplateCommandHandler(ContextProjetos c, ICurrentUser u) : base(c, u) { }
        public Task<CommandResult> Handle(ConverterProjetoTemplateCommand r, CancellationToken ct) =>
            ExecutarAsync(r.ProjetoId, (p, u) => { if (r.ParaTemplate) p.ConverterParaTemplate(u); else p.DefinirComoNormal(u); },
                r.ParaTemplate ? "Projeto convertido em template." : "Projeto definido como normal.", ct);
    }
}
