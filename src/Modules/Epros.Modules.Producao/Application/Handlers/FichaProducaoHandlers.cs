using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Producao.Application.Commands;
using Epros.Modules.Producao.Domain.Entities;
using Epros.Modules.Producao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Application.Handlers
{
    public class CriarFichaProducaoCommandHandler : ICommandHandler<CriarFichaProducaoCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarFichaProducaoCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarFichaProducaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var ficha = new FichaProducao(
                request.VendaId, request.ItemVendaId, request.PessoaId, request.Logomarca,
                request.LateraisPorta, request.ApoioCabeca, tenantId, usuario,
                request.Entrada, request.Saida, request.Transportadora, request.AnoModelo,
                request.CorCouro, request.Costura, request.TipoAcento, request.TipoEncosto,
                request.Abd, request.Abt, request.Observacao);

            if (!ficha.IsValid)
                return CommandResult.Falha(ficha.Notifications.Select(n => n.Message));

            _context.FichasProducao.Add(ficha);
            _context.FichasProducaoHistorico.Add(new FichaProducaoHistorico(ficha.Id, "Criacao", usuario, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Ficha de produção criada (Aguardando pagamento).", new { ficha.Id });
        }
    }

    public class AtualizarConfiguracaoFichaProducaoCommandHandler : ICommandHandler<AtualizarConfiguracaoFichaProducaoCommand>
    {
        private readonly ContextProducao _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarConfiguracaoFichaProducaoCommandHandler(ContextProducao context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarConfiguracaoFichaProducaoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var ficha = await _context.FichasProducao.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
            if (ficha == null) return CommandResult.Falha("Ficha de produção não encontrada.");

            ficha.AlterarConfiguracao(request.Logomarca, request.LateraisPorta, request.ApoioCabeca, usuario,
                request.Transportadora, request.AnoModelo, request.CorCouro, request.Costura,
                request.TipoAcento, request.TipoEncosto, request.Abd, request.Abt, request.Observacao);

            if (!ficha.IsValid)
                return CommandResult.Falha(ficha.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Configuração da ficha atualizada.", new { ficha.Id });
        }
    }

    public class IniciarProducaoFichaCommandHandler : ICommandHandler<IniciarProducaoFichaCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public IniciarProducaoFichaCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(IniciarProducaoFichaCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var tenantId = _tenantProvider.GetTenantId();
            var ficha = await _context.FichasProducao.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
            if (ficha == null) return CommandResult.Falha("Ficha de produção não encontrada.");

            ficha.IniciarProducao(usuario);
            if (!ficha.IsValid)
                return CommandResult.Falha(ficha.Notifications.Select(n => n.Message));

            _context.FichasProducaoHistorico.Add(new FichaProducaoHistorico(ficha.Id, "IniciarProducao", usuario, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Ficha em produção.", new { ficha.Id, Situacao = ficha.Situacao.ToString() });
        }
    }

    public class ConcluirFichaProducaoCommandHandler : ICommandHandler<ConcluirFichaProducaoCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ConcluirFichaProducaoCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ConcluirFichaProducaoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var tenantId = _tenantProvider.GetTenantId();
            var ficha = await _context.FichasProducao.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
            if (ficha == null) return CommandResult.Falha("Ficha de produção não encontrada.");

            ficha.Concluir(usuario);
            if (!ficha.IsValid)
                return CommandResult.Falha(ficha.Notifications.Select(n => n.Message));

            _context.FichasProducaoHistorico.Add(new FichaProducaoHistorico(ficha.Id, "Concluir", usuario, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Ficha concluída.", new { ficha.Id, Situacao = ficha.Situacao.ToString() });
        }
    }
}
