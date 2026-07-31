using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Aprova Assinatura Manual.</summary>
    public class AprovarAssinaturaManualCommandHandler : ICommandHandler<AprovarAssinaturaManualCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly ILogger<AprovarAssinaturaManualCommandHandler> _logger;

        public AprovarAssinaturaManualCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            ILogger<AprovarAssinaturaManualCommandHandler> logger)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<CommandResult> Handle(AprovarAssinaturaManualCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var alteradoPor = _currentUser.GetUserId() ?? "system";

            // Localiza o cliente
            var cliente = await _context.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == request.ClienteId && c.DeletadoEm == null, cancellationToken);

            if (cliente == null)
            {
                return CommandResult.Falha(new[] { "Cliente não encontrado ou excluído." });
            }

            // Ativa o cliente caso esteja suspenso
            if (!cliente.Ativo)
            {
                cliente.Ativar(alteradoPor);
            }

            // Validação de precedência de datas
            if (request.DataFim.HasValue && request.DataInicio > request.DataFim.Value)
            {
                return CommandResult.Falha(new[] { "A data de início não pode ser posterior à data fim." });
            }

            // Localiza ou cria o contrato de assinatura do cliente
            var contrato = await _context.Contratos
                .IgnoreQueryFilters()
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.ClienteId == request.ClienteId && c.DeletadoEm == null, cancellationToken);

            if (contrato == null)
            {
                // Criar novo contrato
                contrato = new Contrato(
                    request.ClienteId,
                    request.DiaVencimento,
                    request.DataInicio,
                    request.DataFim,
                    cliente.TenantId,
                    alteradoPor
                );
                
                contrato.DefinirValorRecorrenteManual(request.ValorRecorrente, alteradoPor);
                contrato.AprovarManualmente(request.Operador, request.Justificativa, alteradoPor);

                if (!contrato.IsValid)
                {
                    var erros = contrato.Notifications.Select(n => n.Message);
                    return CommandResult.Falha(erros, "Invariantes de domínio do novo contrato não foram atendidas.");
                }

                _context.Contratos.Add(contrato);
            }
            else
            {
                // Atualizar contrato existente
                contrato.AtualizarDatasEVencimento(request.DataInicio, request.DataFim, request.DiaVencimento, alteradoPor);
                contrato.DefinirValorRecorrenteManual(request.ValorRecorrente, alteradoPor);
                contrato.AprovarManualmente(request.Operador, request.Justificativa, alteradoPor);

                if (!contrato.IsValid)
                {
                    var erros = contrato.Notifications.Select(n => n.Message);
                    return CommandResult.Falha(erros, "Invariantes de domínio ao atualizar contrato não foram atendidas.");
                }
            }

            // REG-007 & REG-008: Atualiza qualquer assinatura em status Aguardando
            var assinatura = await _context.AssinaturasClientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.ClienteId == request.ClienteId && a.Status == AssinaturaStatus.AguardandoAprovacao && a.DeletadoEm == null, cancellationToken);

            if (assinatura != null)
            {
                assinatura.AprovarManualmente(request.Operador, request.Justificativa, alteradoPor);
                _context.AssinaturasClientes.Update(assinatura);
                
                // Associa o plano ativo ao cliente
                cliente.AlterarPlano(assinatura.PlanoId, alteradoPor);
            }

            // Baixar fatura pendente se informada
            if (request.FaturaPendenteIdParaBaixar.HasValue && request.FaturaPendenteIdParaBaixar != Guid.Empty)
            {
                var fatura = await _context.Faturas
                    .IgnoreQueryFilters() // Ignora filtro para acessar faturas de outros tenants, pois estamos no tenant "system" executando a ação
                    .FirstOrDefaultAsync(f => f.Id == request.FaturaPendenteIdParaBaixar.Value && f.ClienteId == request.ClienteId && f.DeletadoEm == null, cancellationToken);

                if (fatura != null)
                {
                    if (fatura.Status != FaturaStatus.Paga)
                    {
                        fatura.Baixar(alteradoPor);
                    }
                }
                else
                {
                    return CommandResult.Falha(new[] { "Fatura pendente especificada não encontrada para este cliente." });
                }
            }

            // Grava log estruturado AUDIT_TRAIL
            _logger.LogInformation(
                "AUDIT_TRAIL: Assinatura manual aprovada offline. Operador={Operador} | Justificativa={Justificativa} | ClienteId={ClienteId} | UsuarioExecutor={UserId}",
                request.Operador, request.Justificativa, request.ClienteId, alteradoPor);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Assinatura manual aprovada com sucesso!", new { ContratoId = contrato.Id });
        }
    }
}
