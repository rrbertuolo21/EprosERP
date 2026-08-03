using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Imobiliaria.Application.Commands;
using Epros.Modules.Imobiliaria.Application.Queries;
using Epros.Modules.Imobiliaria.Domain.Entities;
using Epros.Modules.Imobiliaria.Domain.Enums;
using Epros.Modules.Imobiliaria.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Imobiliaria.Application.Handlers
{
    public class CriarLocacaoCommandHandler : ICommandHandler<CriarLocacaoCommand>
    {
        private readonly ContextImobiliaria _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarLocacaoCommandHandler(ContextImobiliaria context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarLocacaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var locacao = new Locacao(
                request.ImovelId,
                request.PeriodoInicial,
                request.PeriodoFinal,
                request.Valor,
                request.Vencimento,
                tenantId,
                usuario);

            // RN-013: vinculos N:N de locatarios e fiadores.
            foreach (var pessoaId in request.LocatarioIds ?? Enumerable.Empty<System.Guid>())
                locacao.AdicionarLocatario(pessoaId, usuario);

            foreach (var pessoaId in request.FiadorIds ?? Enumerable.Empty<System.Guid>())
                locacao.AdicionarFiador(pessoaId, usuario);

            // RN-011: validacao funcional obrigatoria antes da gravacao.
            locacao.Validar();
            if (!locacao.IsValid)
                return CommandResult.Falha(locacao.Notifications.Select(n => n.Message));

            _context.Locacoes.Add(locacao);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Locacao formalizada com sucesso!", new { LocacaoId = locacao.Id });
        }
    }

    public class ExcluirLocacaoCommandHandler : ICommandHandler<ExcluirLocacaoCommand>
    {
        private readonly ContextImobiliaria _context;

        public ExcluirLocacaoCommandHandler(ContextImobiliaria context) => _context = context;

        public async Task<CommandResult> Handle(ExcluirLocacaoCommand request, CancellationToken cancellationToken)
        {
            var locacao = await _context.Locacoes.FirstOrDefaultAsync(l => l.Id == request.LocacaoId, cancellationToken);
            if (locacao is null)
                return CommandResult.Falha("Locacao nao encontrada."); // RN-005/RN-016

            _context.Locacoes.Remove(locacao);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Locacao excluida com sucesso!");
        }
    }

    public class ListarLocacoesQueryHandler : IQueryHandler<ListarLocacoesQuery, CommandResult>
    {
        private readonly ContextImobiliaria _context;

        public ListarLocacoesQueryHandler(ContextImobiliaria context) => _context = context;

        public async Task<CommandResult> Handle(ListarLocacoesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Locacoes.AsNoTracking().AsQueryable();

            // RN-017: consulta por periodo (sobreposicao de intervalos).
            if (request.PeriodoDe.HasValue)
                query = query.Where(l => l.PeriodoFinal >= request.PeriodoDe.Value.Date);
            if (request.PeriodoAte.HasValue)
                query = query.Where(l => l.PeriodoInicial <= request.PeriodoAte.Value.Date);

            var locacoes = await query
                .OrderByDescending(l => l.PeriodoInicial)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Locacoes listadas com sucesso!", locacoes);
        }
    }

    public class AlterarLocacaoCommandHandler : ICommandHandler<AlterarLocacaoCommand>
    {
        private readonly ContextImobiliaria _context;
        private readonly ICurrentUser _currentUser;

        public AlterarLocacaoCommandHandler(ContextImobiliaria context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AlterarLocacaoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var locacao = await _context.Locacoes.FirstOrDefaultAsync(l => l.Id == request.LocacaoId, cancellationToken);
            if (locacao is null)
                return CommandResult.Falha("Locacao nao encontrada.");

            locacao.AtualizarDados(request.ImovelId, request.PeriodoInicial, request.PeriodoFinal,
                request.Valor, request.Vencimento, usuario);
            if (!locacao.IsValid)
                return CommandResult.Falha(locacao.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Locacao alterada com sucesso!", new { LocacaoId = locacao.Id });
        }
    }

    /// <summary>
    /// Formaliza a locacao e aplica o efeito colateral no imovel (Disponivel → Locado), publicando
    /// o evento imo.locacao.formalizada no Outbox (T2). Tudo em uma unica transacao (TEC-05).
    /// </summary>
    public class FormalizarLocacaoCommandHandler : ICommandHandler<FormalizarLocacaoCommand>
    {
        private readonly ContextImobiliaria _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public FormalizarLocacaoCommandHandler(ContextImobiliaria context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(FormalizarLocacaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var locacao = await _context.Locacoes.Include(l => l.Partes)
                .FirstOrDefaultAsync(l => l.Id == request.LocacaoId, cancellationToken);
            if (locacao is null)
                return CommandResult.Falha("Locacao nao encontrada.");

            locacao.Formalizar(usuario);
            if (!locacao.IsValid)
                return CommandResult.Falha(locacao.Notifications.Select(n => n.Message));

            // Efeito colateral (ID1): imovel vinculado passa a Locado.
            if (locacao.ImovelId.HasValue)
            {
                var imovel = await _context.Imoveis.FirstOrDefaultAsync(i => i.Id == locacao.ImovelId.Value, cancellationToken);
                if (imovel is not null)
                {
                    imovel.MarcarLocado(usuario);
                    if (!imovel.IsValid)
                        return CommandResult.Falha(imovel.Notifications.Select(n => n.Message));
                }
            }

            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Imobiliaria.LocacaoFormalizada,
                JsonSerializer.Serialize(new { locacaoId = locacao.Id, imovelId = locacao.ImovelId, locacao.Valor, locacao.Vencimento, tenantId })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Locacao formalizada com sucesso!", new { LocacaoId = locacao.Id, Status = locacao.Status.ToString() });
        }
    }

    public class EncerrarLocacaoCommandHandler : ICommandHandler<EncerrarLocacaoCommand>
    {
        private readonly ContextImobiliaria _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public EncerrarLocacaoCommandHandler(ContextImobiliaria context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(EncerrarLocacaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var locacao = await _context.Locacoes.FirstOrDefaultAsync(l => l.Id == request.LocacaoId, cancellationToken);
            if (locacao is null)
                return CommandResult.Falha("Locacao nao encontrada.");

            locacao.Encerrar(usuario);
            if (!locacao.IsValid)
                return CommandResult.Falha(locacao.Notifications.Select(n => n.Message));

            await LiberarImovelSeVinculado(locacao, usuario, cancellationToken);

            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Imobiliaria.LocacaoEncerrada,
                JsonSerializer.Serialize(new { locacaoId = locacao.Id, imovelId = locacao.ImovelId, tenantId })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Locacao encerrada com sucesso!", new { LocacaoId = locacao.Id, Status = locacao.Status.ToString() });
        }

        private async Task LiberarImovelSeVinculado(Locacao locacao, string usuario, CancellationToken ct)
        {
            if (!locacao.ImovelId.HasValue) return;
            var imovel = await _context.Imoveis.FirstOrDefaultAsync(i => i.Id == locacao.ImovelId.Value, ct);
            imovel?.LiberarLocacao(usuario);
        }
    }

    public class CancelarLocacaoCommandHandler : ICommandHandler<CancelarLocacaoCommand>
    {
        private readonly ContextImobiliaria _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CancelarLocacaoCommandHandler(ContextImobiliaria context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(CancelarLocacaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var locacao = await _context.Locacoes.FirstOrDefaultAsync(l => l.Id == request.LocacaoId, cancellationToken);
            if (locacao is null)
                return CommandResult.Falha("Locacao nao encontrada.");

            locacao.Cancelar(usuario);
            if (!locacao.IsValid)
                return CommandResult.Falha(locacao.Notifications.Select(n => n.Message));

            if (locacao.ImovelId.HasValue)
            {
                var imovel = await _context.Imoveis.FirstOrDefaultAsync(i => i.Id == locacao.ImovelId.Value, cancellationToken);
                imovel?.LiberarLocacao(usuario);
            }

            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Imobiliaria.LocacaoCancelada,
                JsonSerializer.Serialize(new { locacaoId = locacao.Id, imovelId = locacao.ImovelId, tenantId })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Locacao cancelada com sucesso!", new { LocacaoId = locacao.Id, Status = locacao.Status.ToString() });
        }
    }

    public class RenovarLocacaoCommandHandler : ICommandHandler<RenovarLocacaoCommand>
    {
        private readonly ContextImobiliaria _context;
        private readonly ICurrentUser _currentUser;

        public RenovarLocacaoCommandHandler(ContextImobiliaria context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(RenovarLocacaoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var locacao = await _context.Locacoes.FirstOrDefaultAsync(l => l.Id == request.LocacaoId, cancellationToken);
            if (locacao is null)
                return CommandResult.Falha("Locacao nao encontrada.");

            locacao.Renovar(request.NovoPeriodoFinal, usuario);
            if (!locacao.IsValid)
                return CommandResult.Falha(locacao.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Locacao renovada com sucesso!", new { LocacaoId = locacao.Id, locacao.PeriodoFinal });
        }
    }

    public class ObterResumoAluguelQueryHandler : IQueryHandler<ObterResumoAluguelQuery, CommandResult>
    {
        private readonly ContextImobiliaria _context;

        public ObterResumoAluguelQueryHandler(ContextImobiliaria context) => _context = context;

        public async Task<CommandResult> Handle(ObterResumoAluguelQuery request, CancellationToken cancellationToken)
        {
            // RN-015/RN-022: resumo do aluguel usado para localizar o recebivel em contas a receber.
            var locacao = await _context.Locacoes
                .AsNoTracking()
                .Include(l => l.Partes)
                .FirstOrDefaultAsync(l => l.Id == request.LocacaoId, cancellationToken);

            if (locacao is null)
                return CommandResult.Falha("Locacao nao encontrada.");

            var resumo = new
            {
                LocacaoId = locacao.Id,
                locacao.ImovelId,
                locacao.Valor,
                locacao.Vencimento,
                locacao.PeriodoInicial,
                locacao.PeriodoFinal,
                Status = locacao.Status.ToString(),
                Locatarios = locacao.Locatarios.Select(p => p.PessoaId).ToList(),
                Fiadores = locacao.Fiadores.Select(p => p.PessoaId).ToList()
            };

            return CommandResult.Ok("Resumo do aluguel gerado com sucesso!", resumo);
        }
    }
}
