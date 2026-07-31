using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Application.Queries;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    public class CriarCupomCommandHandler : ICommandHandler<CriarCupomCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCupomCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCupomCommand request, CancellationToken cancellationToken)
        {
            var validator = new CriarCupomCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return CommandResult.Falha(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var codigoUpper = request.Codigo.ToUpperInvariant();
            var cupomExistente = await _context.Cupons
                .IgnoreQueryFilters()
                .AnyAsync(c => c.Codigo == codigoUpper && c.DeletadoEm == null, cancellationToken);

            if (cupomExistente)
            {
                return CommandResult.Falha(new[] { "Já existe um cupom cadastrado com este código." });
            }

            var cupom = new Cupom(
                codigo: request.Codigo,
                tipo: request.Tipo,
                valorDesconto: request.ValorDesconto,
                limiteUso: request.LimiteUso,
                validoAte: request.ValidoAte,
                tenantId: tenantId,
                criadoPor: criadoPor,
                nome: request.Nome
            );

            if (!cupom.IsValid)
            {
                return CommandResult.Falha(cupom.Notifications.Select(n => n.Message));
            }

            _context.Cupons.Add(cupom);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Cupom criado com sucesso!", new { CupomId = cupom.Id, Codigo = cupom.Codigo });
        }
    }

    public class ValidarCupomCommandHandler : ICommandHandler<ValidarCupomCommand>
    {
        private readonly ContextGestaoClientes _context;

        public ValidarCupomCommandHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ValidarCupomCommand request, CancellationToken cancellationToken)
        {
            var validator = new ValidarCupomCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return CommandResult.Falha(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var plano = await _context.Planos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == request.PlanoId && p.Ativo && p.DeletadoEm == null, cancellationToken);

            if (plano == null)
            {
                return CommandResult.Falha(new[] { "Plano não encontrado ou inativo." });
            }

            var codigoUpper = request.Codigo.ToUpperInvariant();
            var cupom = await _context.Cupons
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Codigo == codigoUpper && c.DeletadoEm == null, cancellationToken);

            if (cupom == null)
            {
                return CommandResult.Falha(new[] { "Cupom inválido ou inexistente." });
            }

            if (!cupom.Validar())
            {
                return CommandResult.Falha(new[] { "Este cupom está expirado, inativo ou com limite de usos esgotado." });
            }

            var desconto = cupom.CalcularDesconto(plano.Preco);
            var valorFinal = Math.Max(0, plano.Preco - desconto);

            return CommandResult.Ok("Cupom válido!", new
            {
                CupomId = cupom.Id,
                Codigo = cupom.Codigo,
                Tipo = cupom.Tipo,
                ValorDesconto = cupom.ValorDesconto,
                DescontoCalculado = desconto,
                ValorBase = plano.Preco,
                ValorFinal = valorFinal
            });
        }
    }

    public class AtualizarCupomCommandHandler : ICommandHandler<AtualizarCupomCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;
        public AtualizarCupomCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AtualizarCupomCommand request, CancellationToken cancellationToken)
        {
            var cupom = await _context.Cupons.FirstOrDefaultAsync(c => c.Id == request.Id && c.DeletadoEm == null, cancellationToken);
            if (cupom == null) return CommandResult.Falha(new[] { "Cupom não encontrado." });
            cupom.Atualizar(request.Nome, request.Tipo, request.ValorDesconto, request.LimiteUso, request.ValidoAte, _currentUser.GetUserId() ?? "system");
            if (!cupom.IsValid) return CommandResult.Falha(cupom.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Cupom atualizado com sucesso!");
        }
    }

    public class ExcluirCupomCommandHandler : ICommandHandler<ExcluirCupomCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;
        public ExcluirCupomCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(ExcluirCupomCommand request, CancellationToken cancellationToken)
        {
            var cupom = await _context.Cupons.FirstOrDefaultAsync(c => c.Id == request.Id && c.DeletadoEm == null, cancellationToken);
            if (cupom == null) return CommandResult.Falha(new[] { "Cupom não encontrado." });
            cupom.Deletar(_currentUser.GetUserId() ?? "system");
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Cupom excluído com sucesso!");
        }
    }

    public class ListarCuponsQueryHandler : IQueryHandler<ListarCuponsQuery, PagedQueryResult<CupomDto>>
    {
        private readonly ContextGestaoClientes _context;
        public ListarCuponsQueryHandler(ContextGestaoClientes context) { _context = context; }

        public async Task<PagedQueryResult<CupomDto>> Handle(ListarCuponsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Cupons.Where(c => c.DeletadoEm == null);
            var total = await query.CountAsync(cancellationToken);
            var totalPaginas = (int)Math.Ceiling(total / (double)request.TamanhoPagina);
            var items = await query
                .OrderByDescending(c => c.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(c => new CupomDto
                {
                    Id = c.Id, Nome = c.Nome, Codigo = c.Codigo, Tipo = c.Tipo,
                    ValorDesconto = c.ValorDesconto, LimiteUso = c.LimiteUso, QuantidadeUsos = c.QuantidadeUsos,
                    Ativo = c.Ativo, ValidoAte = c.ValidoAte, Global = c.TenantId == "system"
                })
                .ToListAsync(cancellationToken);
            return new PagedQueryResult<CupomDto>(items, total, totalPaginas);
        }
    }

    public class ObterCupomQueryHandler : IQueryHandler<ObterCupomQuery, CupomDto>
    {
        private readonly ContextGestaoClientes _context;
        public ObterCupomQueryHandler(ContextGestaoClientes context) { _context = context; }

        public async Task<CupomDto> Handle(ObterCupomQuery request, CancellationToken cancellationToken)
        {
            var c = await _context.Cupons.FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (c == null) return null!;
            return new CupomDto
            {
                Id = c.Id, Nome = c.Nome, Codigo = c.Codigo, Tipo = c.Tipo,
                ValorDesconto = c.ValorDesconto, LimiteUso = c.LimiteUso, QuantidadeUsos = c.QuantidadeUsos,
                Ativo = c.Ativo, ValidoAte = c.ValidoAte, Global = c.TenantId == "system"
            };
        }
    }
}
