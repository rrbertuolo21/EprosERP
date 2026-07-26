using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Producao.Application.Commands;
using Epros.Modules.Producao.Domain.Entities;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Modules.Producao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Application.Handlers
{
    /// <summary>PRD-ESC — Criação da programação (ESC-EF §7.1).</summary>
    public class CriarEscProgramacaoCommandHandler : ICommandHandler<CriarEscProgramacaoCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarEscProgramacaoCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarEscProgramacaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var duplicado = await _context.EscProgramacoes.AnyAsync(p => p.Codigo == request.Codigo, cancellationToken);
            if (duplicado)
                return CommandResult.Falha($"Código de programação '{request.Codigo}' já está em uso.");

            var programacao = new EscProgramacao(
                request.Codigo, request.ResponsavelId, tenantId, usuario,
                request.PlanoProducaoId, request.OrdemProducaoId, request.CentroTrabalhoId, request.Prioridade);

            if (request.Operacoes != null)
            {
                foreach (var o in request.Operacoes)
                {
                    var operacao = new EscOperacao(
                        programacao.Id, tenantId, usuario, o.Sequencia, o.ServicoId, o.EquipamentoId, o.ColaboradorId,
                        o.InicioPrevisto, o.TerminoPrevisto, o.HorasPrevistas, o.MinutosPrevistos, o.SegundosPrevistos, o.CustoPrevisto);
                    programacao.AdicionarOperacao(operacao, usuario);
                }
            }

            if (!programacao.IsValid)
                return CommandResult.Falha(programacao.Notifications.Select(n => n.Message));

            _context.EscProgramacoes.Add(programacao);
            _context.EscHistoricos.Add(new EscHistorico(programacao.Id, "Criacao", usuario, "{}", tenantId, usuario, null, EStatusWorkflowProducao.Rascunho));
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Programação criada em Rascunho.", new { programacao.Id, programacao.Codigo });
        }
    }

    /// <summary>PRD-ESC — Inclusão de operação sequenciada.</summary>
    public class AdicionarEscOperacaoCommandHandler : ICommandHandler<AdicionarEscOperacaoCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarEscOperacaoCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarEscOperacaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var programacaoExiste = await _context.EscProgramacoes.AnyAsync(p => p.Id == request.ProgramacaoId, cancellationToken);
            if (!programacaoExiste) return CommandResult.Falha("Programação não encontrada.");

            var operacao = new EscOperacao(
                request.ProgramacaoId, tenantId, usuario, request.Sequencia, request.ServicoId, request.EquipamentoId, request.ColaboradorId,
                request.InicioPrevisto, request.TerminoPrevisto, request.HorasPrevistas, request.MinutosPrevistos, request.SegundosPrevistos, request.CustoPrevisto);

            if (!operacao.IsValid) return CommandResult.Falha(operacao.Notifications.Select(n => n.Message));

            _context.EscOperacoes.Add(operacao);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Operação incluída na programação.", new { operacao.Id });
        }
    }

    /// <summary>PRD-ESC — Registro de janela/duração realizada da operação (ESC-EF §7.4).</summary>
    public class RegistrarEscOperacaoRealizadoCommandHandler : ICommandHandler<RegistrarEscOperacaoRealizadoCommand>
    {
        private readonly ContextProducao _context;
        private readonly ICurrentUser _currentUser;

        public RegistrarEscOperacaoRealizadoCommandHandler(ContextProducao context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarEscOperacaoRealizadoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var operacao = await _context.EscOperacoes.FirstOrDefaultAsync(o => o.Id == request.OperacaoId, cancellationToken);
            if (operacao == null) return CommandResult.Falha("Operação não encontrada.");

            operacao.RegistrarRealizado(request.InicioRealizado, request.TerminoRealizado,
                request.HorasRealizadas, request.MinutosRealizados, request.SegundosRealizados, usuario, request.CustoRealizado);

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Janela realizada registrada.", new { operacao.Id });
        }
    }

    /// <summary>PRD-ESC — Salva parâmetro de sequenciamento por tenant (ESC-REG-023).</summary>
    public class SalvarEscParametroCommandHandler : ICommandHandler<SalvarEscParametroCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public SalvarEscParametroCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(SalvarEscParametroCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var parametro = await _context.EscParametros.FirstOrDefaultAsync(p => p.Chave == request.Chave, cancellationToken);
            if (parametro == null)
            {
                parametro = new EscParametro(request.Chave, tenantId, usuario, request.Valor, request.Ativo);
                if (!parametro.IsValid) return CommandResult.Falha(parametro.Notifications.Select(n => n.Message));
                _context.EscParametros.Add(parametro);
            }
            else
            {
                parametro.Alterar(request.Valor, request.Ativo, usuario);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Parâmetro de sequenciamento salvo.", new { parametro.Id });
        }
    }

    // ===================== Transições de workflow da programação =====================

    public abstract class EscTransicaoHandlerBase
    {
        protected readonly ContextProducao _context;
        protected readonly ITenantProvider _tenantProvider;
        protected readonly ICurrentUser _currentUser;

        protected EscTransicaoHandlerBase(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        protected async Task<(EscProgramacao? programacao, string usuario, string tenantId)> CarregarAsync(Guid id, CancellationToken ct)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var tenantId = _tenantProvider.GetTenantId();
            var programacao = await _context.EscProgramacoes.Include(p => p.Operacoes).FirstOrDefaultAsync(p => p.Id == id, ct);
            return (programacao, usuario, tenantId);
        }

        protected async Task<CommandResult> FinalizarAsync(EscProgramacao p, string acao, string usuario, string tenantId, CancellationToken ct)
        {
            if (!p.IsValid)
                return CommandResult.Falha(p.Notifications.Select(n => n.Message));

            _context.EscHistoricos.Add(new EscHistorico(p.Id, acao, usuario, "{}", tenantId, usuario, null, p.Status));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok($"Programação {acao} com sucesso.", new { p.Id, Status = p.Status.ToString() });
        }
    }

    public class SubmeterEscProgramacaoCommandHandler : EscTransicaoHandlerBase, ICommandHandler<SubmeterEscProgramacaoCommand>
    {
        public SubmeterEscProgramacaoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(SubmeterEscProgramacaoCommand request, CancellationToken ct)
        {
            var (p, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (p == null) return CommandResult.Falha("Programação não encontrada.");
            p.SubmeterParaAnalise(usuario);
            return await FinalizarAsync(p, "Submissao", usuario, tenantId, ct);
        }
    }

    public class AprovarEscProgramacaoCommandHandler : EscTransicaoHandlerBase, ICommandHandler<AprovarEscProgramacaoCommand>
    {
        public AprovarEscProgramacaoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(AprovarEscProgramacaoCommand request, CancellationToken ct)
        {
            var (p, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (p == null) return CommandResult.Falha("Programação não encontrada.");
            p.Aprovar(usuario);
            return await FinalizarAsync(p, "Aprovacao", usuario, tenantId, ct);
        }
    }

    public class RejeitarEscProgramacaoCommandHandler : EscTransicaoHandlerBase, ICommandHandler<RejeitarEscProgramacaoCommand>
    {
        public RejeitarEscProgramacaoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(RejeitarEscProgramacaoCommand request, CancellationToken ct)
        {
            var (p, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (p == null) return CommandResult.Falha("Programação não encontrada.");
            p.Rejeitar(request.Motivo, usuario);
            return await FinalizarAsync(p, "Rejeicao", usuario, tenantId, ct);
        }
    }

    public class InativarEscProgramacaoCommandHandler : EscTransicaoHandlerBase, ICommandHandler<InativarEscProgramacaoCommand>
    {
        public InativarEscProgramacaoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(InativarEscProgramacaoCommand request, CancellationToken ct)
        {
            var (p, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (p == null) return CommandResult.Falha("Programação não encontrada.");
            p.Inativar(usuario);
            return await FinalizarAsync(p, "Inativacao", usuario, tenantId, ct);
        }
    }

    public class ReativarEscProgramacaoCommandHandler : EscTransicaoHandlerBase, ICommandHandler<ReativarEscProgramacaoCommand>
    {
        public ReativarEscProgramacaoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(ReativarEscProgramacaoCommand request, CancellationToken ct)
        {
            var (p, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (p == null) return CommandResult.Falha("Programação não encontrada.");
            p.Reativar(usuario);
            return await FinalizarAsync(p, "Reativacao", usuario, tenantId, ct);
        }
    }

    public class EncerrarEscProgramacaoCommandHandler : EscTransicaoHandlerBase, ICommandHandler<EncerrarEscProgramacaoCommand>
    {
        public EncerrarEscProgramacaoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(EncerrarEscProgramacaoCommand request, CancellationToken ct)
        {
            var (p, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (p == null) return CommandResult.Falha("Programação não encontrada.");
            p.Encerrar(usuario);
            return await FinalizarAsync(p, "Encerramento", usuario, tenantId, ct);
        }
    }
}
