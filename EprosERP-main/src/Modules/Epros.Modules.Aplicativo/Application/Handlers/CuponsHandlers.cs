using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
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
                criadoPor: criadoPor
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
}
