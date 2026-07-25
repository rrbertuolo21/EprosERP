using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Imobiliaria.Application.Commands;
using Epros.Modules.Imobiliaria.Application.Queries;
using Epros.Modules.Imobiliaria.Domain.Entities;
using Epros.Modules.Imobiliaria.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
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
