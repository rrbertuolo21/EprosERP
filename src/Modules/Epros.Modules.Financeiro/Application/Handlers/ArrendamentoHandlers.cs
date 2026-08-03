using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Handlers
{
    // ===== Arrendamento mercantil (IFRS-16 / CPC 06 R2) =====
    public class CriarContratoArrendamentoCommandHandler : IRequestHandler<CriarContratoArrendamentoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarContratoArrendamentoCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarContratoArrendamentoCommand request, CancellationToken ct)
        {
            var contrato = new ContratoArrendamento(
                request.Descricao, request.PessoaId, request.DataInicio, request.ValorContraprestacao,
                request.QuantidadeParcelas, request.TaxaIncrementalPeriodo, request.PagamentoAntecipado,
                request.CustosDiretosIniciais, request.IncentivosRecebidos,
                _tenant.GetTenantId(), _user.GetUserId() ?? "system");

            if (!contrato.IsValid) return CommandResult.Falha(contrato.Notifications.Select(n => n.Message));

            _context.ContratosArrendamento.Add(contrato);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Arrendamento reconhecido (IFRS-16).", new
            {
                contrato.Id,
                contrato.PassivoArrendamentoInicial,
                contrato.DireitoDeUsoInicial
            });
        }
    }

    public class EncerrarContratoArrendamentoCommandHandler : IRequestHandler<EncerrarContratoArrendamentoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public EncerrarContratoArrendamentoCommandHandler(ContextFinanceiro context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(EncerrarContratoArrendamentoCommand request, CancellationToken ct)
        {
            var contrato = await _context.ContratosArrendamento.FirstOrDefaultAsync(c => c.Id == request.Id, ct);
            if (contrato == null) return CommandResult.Falha("Arrendamento não encontrado.");
            contrato.Encerrar(request.Motivo, request.DataEncerramento, _user.GetUserId() ?? "system");
            if (!contrato.IsValid) return CommandResult.Falha(contrato.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Arrendamento encerrado.", new { contrato.Id });
        }
    }
}
