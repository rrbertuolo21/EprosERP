using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    public class CancelarDocumentoFiscalCommandHandler : ICommandHandler<CancelarDocumentoFiscalCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IHerculesFiscalService _fiscalService;

        public CancelarDocumentoFiscalCommandHandler(
            ContextFiscal context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IHerculesFiscalService fiscalService)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _fiscalService = fiscalService;
        }

        public async Task<CommandResult> Handle(CancelarDocumentoFiscalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // 1. Localizar o documento fiscal (com base no tenant ativo)
            var documento = await _context.DocumentosFiscais
                .Include(d => d.Itens)
                .FirstOrDefaultAsync(d => d.Id == request.DocumentoFiscalId, cancellationToken);

            if (documento == null)
            {
                return CommandResult.Falha("Documento fiscal não localizado.");
            }

            // 2. Verificar restrições de estado
            if (documento.Status != "Autorizado")
            {
                return CommandResult.Falha($"Apenas documentos com status 'Autorizado' podem ser cancelados. Status atual: {documento.Status}");
            }

            // 3. Executar o cancelamento junto à SEFAZ via Hercules.NET
            var resultadoCancelamento = await _fiscalService.CancelarAsync(documento, request.Justificativa);

            // 3.1. cStat 573 = "Duplicidade de evento" — o cancelamento JÁ foi registrado na SEFAZ numa
            // tentativa anterior (ex.: timeout após o registro). Fiel ao legado (NfceNfeController), em vez
            // de falhar, re-consultamos a situação da nota pela chave; se estiver cancelada (cStat 101),
            // tratamos como cancelamento efetivo e sincronizamos o estado local. NÃO fabricamos protocolo.
            if (!resultadoCancelamento.Sucesso && resultadoCancelamento.StatusSefaz == 573)
            {
                var reconsulta = await _fiscalService.ConsultarProtocoloAsync(new ConsultaProtocoloRequest
                {
                    Chave = documento.ChaveAcesso,
                    Modelo = documento.Modelo,
                    Ambiente = documento.Ambiente
                });

                // 101 = Cancelamento de NF-e homologado (situação atual da chave já é "cancelada").
                if (reconsulta.StatusSefaz == 101)
                {
                    resultadoCancelamento = new RetornoCancelamentoDto
                    {
                        Sucesso = true,
                        StatusSefaz = 101,
                        Motivo = "Cancelamento já registrado na SEFAZ (confirmado por reconsulta após duplicidade cStat 573).",
                        Protocolo = documento.Protocolo ?? string.Empty,
                        XmlRetorno = reconsulta.XmlRetorno
                    };
                }
            }

            if (resultadoCancelamento.Sucesso && (resultadoCancelamento.StatusSefaz == 135 || resultadoCancelamento.StatusSefaz == 155 || resultadoCancelamento.StatusSefaz == 101))
            {
                // Registrar o evento fiscal de cancelamento. A justificativa (xJust) vai no campo
                // próprio Justificativa (não em XCorrecao, que é da Carta de Correção) — REG-CANC-014.
                var eventoFiscal = new EventoDocumentoFiscal(
                    documento.Id,
                    "Cancelamento",
                    resultadoCancelamento.StatusSefaz,
                    resultadoCancelamento.Motivo,
                    resultadoCancelamento.Protocolo,
                    1, // Sequência 1 para o cancelamento
                    null, // xCorrecao (exclusivo da CC-e)
                    resultadoCancelamento.XmlRetorno,
                    tenantId,
                    usuario,
                    justificativa: request.Justificativa
                );

                _context.EventosDocumentosFiscais.Add(eventoFiscal);

                // Atualizar o status do documento fiscal principal
                documento.Cancelar(request.Justificativa, resultadoCancelamento.XmlRetorno, usuario);

                // Comunicar os módulos donos (Vendas/Financeiro/Estoque) via Outbox transacional — EF §14.
                // Payload agnóstico; os consumidores reagem ao cancelamento (estorno/baixa/estoque).
                var eventoIntegracao = new
                {
                    DocumentoFiscalId = documento.Id,
                    TenantId = tenantId,
                    ChaveAcesso = documento.ChaveAcesso,
                    Modelo = documento.Modelo,
                    Numero = documento.Numero,
                    Protocolo = resultadoCancelamento.Protocolo,
                    StatusSefaz = resultadoCancelamento.StatusSefaz,
                    VendaOrigemId = documento.VendaOrigemId,
                    Justificativa = request.Justificativa
                };
                var payloadJson = JsonSerializer.Serialize(eventoIntegracao);
                _context.OutboxMessages.Add(new OutboxMessage(tenantId, "DocumentoFiscalCancelado", payloadJson));

                await _context.SaveChangesAsync(cancellationToken);

                return CommandResult.Ok("Cancelamento homologado e registrado com sucesso!");
            }
            else
            {
                return CommandResult.Falha(new[] { resultadoCancelamento.Motivo }, $"Falha ao cancelar o documento fiscal na SEFAZ: {resultadoCancelamento.Motivo} (Código: {resultadoCancelamento.StatusSefaz})");
            }
        }
    }
}
