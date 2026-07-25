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
    public class CriarBomEstruturaCommandHandler : ICommandHandler<CriarBomEstruturaCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarBomEstruturaCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarBomEstruturaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (!string.IsNullOrWhiteSpace(request.Codigo))
            {
                var duplicado = await _context.BomEstruturas.AnyAsync(e => e.Codigo == request.Codigo, cancellationToken);
                if (duplicado)
                    return CommandResult.Falha($"Código de estrutura '{request.Codigo}' já está em uso.");
            }

            var estrutura = new BomEstrutura(
                request.ProdutoId, request.VariacaoId, request.Codigo,
                request.PercentualDesperdicio, request.CustoIngredientes, request.CustoExtra,
                request.QuantidadeTotal, tenantId, usuario,
                request.IngredientesJson, request.Instrucoes, request.TipoCustoProducao,
                request.PrecoFinal, request.SubUnidadeId, request.Versao,
                request.InicioVigencia, request.FimVigencia);

            if (request.Componentes != null)
            {
                foreach (var c in request.Componentes)
                {
                    estrutura.AdicionarComponente(
                        c.VariacaoComponenteId, c.Quantidade, usuario, c.SubUnidadeId,
                        c.MultiplicadorUnidade, c.PercentualDesperdicio, c.GrupoComponenteId,
                        c.OrdemMontagem, c.CustoUnitarioComImpostos);
                }
            }

            if (!estrutura.IsValid)
                return CommandResult.Falha(estrutura.Notifications.Select(n => n.Message));

            _context.BomEstruturas.Add(estrutura);
            RegistrarHistorico(estrutura, "Criacao", usuario, null, EStatusWorkflowProducao.Rascunho, tenantId);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Estrutura de produto (BOM) criada em Rascunho.", new { estrutura.Id, estrutura.Codigo });
        }

        private void RegistrarHistorico(BomEstrutura e, string acao, string usuario, EStatusWorkflowProducao? de, EStatusWorkflowProducao? para, string tenantId)
        {
            _context.BomHistoricos.Add(new BomHistorico(e.Id, acao, usuario, "{}", tenantId, usuario, de, para));
        }
    }

    public abstract class BomEstruturaTransicaoHandlerBase
    {
        protected readonly ContextProducao _context;
        protected readonly ITenantProvider _tenantProvider;
        protected readonly ICurrentUser _currentUser;

        protected BomEstruturaTransicaoHandlerBase(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        protected async Task<(BomEstrutura? estrutura, string usuario, string tenantId)> CarregarAsync(System.Guid id, CancellationToken ct)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var tenantId = _tenantProvider.GetTenantId();
            var estrutura = await _context.BomEstruturas.Include(e => e.Componentes).FirstOrDefaultAsync(e => e.Id == id, ct);
            return (estrutura, usuario, tenantId);
        }

        protected async Task<CommandResult> FinalizarAsync(BomEstrutura estrutura, string acao, string usuario, string tenantId, CancellationToken ct)
        {
            if (!estrutura.IsValid)
                return CommandResult.Falha(estrutura.Notifications.Select(n => n.Message));

            _context.BomHistoricos.Add(new BomHistorico(estrutura.Id, acao, usuario, "{}", tenantId, usuario, null, estrutura.Status));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok($"Estrutura {acao} com sucesso.", new { estrutura.Id, Status = estrutura.Status.ToString() });
        }
    }

    public class SubmeterBomEstruturaCommandHandler : BomEstruturaTransicaoHandlerBase, ICommandHandler<SubmeterBomEstruturaCommand>
    {
        public SubmeterBomEstruturaCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(SubmeterBomEstruturaCommand request, CancellationToken ct)
        {
            var (estrutura, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (estrutura == null) return CommandResult.Falha("Estrutura não encontrada.");
            estrutura.SubmeterParaAnalise(usuario);
            return await FinalizarAsync(estrutura, "Submissao", usuario, tenantId, ct);
        }
    }

    public class AprovarBomEstruturaCommandHandler : BomEstruturaTransicaoHandlerBase, ICommandHandler<AprovarBomEstruturaCommand>
    {
        public AprovarBomEstruturaCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(AprovarBomEstruturaCommand request, CancellationToken ct)
        {
            var (estrutura, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (estrutura == null) return CommandResult.Falha("Estrutura não encontrada.");
            estrutura.Aprovar(usuario);
            return await FinalizarAsync(estrutura, "Aprovacao", usuario, tenantId, ct);
        }
    }

    public class RejeitarBomEstruturaCommandHandler : BomEstruturaTransicaoHandlerBase, ICommandHandler<RejeitarBomEstruturaCommand>
    {
        public RejeitarBomEstruturaCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(RejeitarBomEstruturaCommand request, CancellationToken ct)
        {
            var (estrutura, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (estrutura == null) return CommandResult.Falha("Estrutura não encontrada.");
            estrutura.Rejeitar(request.Motivo, usuario);
            return await FinalizarAsync(estrutura, "Rejeicao", usuario, tenantId, ct);
        }
    }

    public class InativarBomEstruturaCommandHandler : BomEstruturaTransicaoHandlerBase, ICommandHandler<InativarBomEstruturaCommand>
    {
        public InativarBomEstruturaCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(InativarBomEstruturaCommand request, CancellationToken ct)
        {
            var (estrutura, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (estrutura == null) return CommandResult.Falha("Estrutura não encontrada.");
            estrutura.Inativar(usuario);
            return await FinalizarAsync(estrutura, "Inativacao", usuario, tenantId, ct);
        }
    }

    public class ReativarBomEstruturaCommandHandler : BomEstruturaTransicaoHandlerBase, ICommandHandler<ReativarBomEstruturaCommand>
    {
        public ReativarBomEstruturaCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(ReativarBomEstruturaCommand request, CancellationToken ct)
        {
            var (estrutura, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (estrutura == null) return CommandResult.Falha("Estrutura não encontrada.");
            estrutura.Reativar(usuario);
            return await FinalizarAsync(estrutura, "Reativacao", usuario, tenantId, ct);
        }
    }

    public class EncerrarBomEstruturaCommandHandler : BomEstruturaTransicaoHandlerBase, ICommandHandler<EncerrarBomEstruturaCommand>
    {
        public EncerrarBomEstruturaCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(EncerrarBomEstruturaCommand request, CancellationToken ct)
        {
            var (estrutura, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (estrutura == null) return CommandResult.Falha("Estrutura não encontrada.");
            estrutura.Encerrar(usuario);
            return await FinalizarAsync(estrutura, "Encerramento", usuario, tenantId, ct);
        }
    }

    public class CriarBomInstrucaoCommandHandler : ICommandHandler<CriarBomInstrucaoCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarBomInstrucaoCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarBomInstrucaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var duplicado = await _context.BomInstrucoes.AnyAsync(i => i.Codigo == request.Codigo, cancellationToken);
            if (duplicado)
                return CommandResult.Falha($"Código de instrução '{request.Codigo}' já está em uso. (BOM-REG-020)");

            var instrucao = new BomInstrucao(request.Codigo, request.Descricao, tenantId, usuario);
            if (!instrucao.IsValid)
                return CommandResult.Falha(instrucao.Notifications.Select(n => n.Message));

            _context.BomInstrucoes.Add(instrucao);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Instrução criada com sucesso.", new { instrucao.Id, instrucao.Codigo });
        }
    }

    public class VincularBomInstrucaoOrdemCommandHandler : ICommandHandler<VincularBomInstrucaoOrdemCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public VincularBomInstrucaoOrdemCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(VincularBomInstrucaoOrdemCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var instrucaoExiste = await _context.BomInstrucoes.AnyAsync(i => i.Id == request.InstrucaoId, cancellationToken);
            if (!instrucaoExiste)
                return CommandResult.Falha("Instrução não encontrada.");

            var vinculo = new BomInstrucaoOrdem(request.InstrucaoId, request.OrdemProducaoId, tenantId, usuario);
            if (!vinculo.IsValid)
                return CommandResult.Falha(vinculo.Notifications.Select(n => n.Message));

            _context.BomInstrucoesOrdem.Add(vinculo);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Instrução vinculada à ordem de produção.", new { vinculo.Id });
        }
    }
}
