using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Epros.Modules.GestaoClientes.Infrastructure.Jobs
{
    /// <summary>
    /// 1.08C — Processador de Outbox do módulo GestaoClientes. FECHA O DEFEITO da régua de cobrança:
    /// o <see cref="ReguaCobrancaJob"/> MONTAVA os alertas D-3/D-1/D+1/D+5/D+10/D+14 e enfileirava
    /// <c>FaturaAlertaCobrancaEvent</c> no Outbox, mas NENHUM consumidor existia — a régua nunca entregava.
    /// Este job consome as <c>OutboxMessages</c> do contexto GestaoClientes e ENTREGA a notificação via
    /// <see cref="INotificacaoService"/> (o mesmo do boas-vindas da 1.07), cobrindo três eventos:
    ///   • <c>FaturaAlertaCobrancaEvent</c> — dunning (régua de cobrança) — avisa ANTES de bloquear (REG-021);
    ///   • <c>TrialEncerradoEvent</c> — fim de trial + 1ª fatura disponível (1.08A);
    ///   • <c>ReciboEmitidoEvent</c> — recibo de pagamento entregue na quitação (FaturaLiquidacaoService).
    ///
    /// Idempotente: só processa mensagens com <c>ProcessadoEm == null</c> e <c>Tentativas &lt; 5</c>, e marca
    /// <see cref="Epros.Shared.Domain.Events.OutboxMessage.MarcarProcessado"/> ao concluir — processar 2x não
    /// reenvia. Resiliente: se NÃO houver <see cref="INotificacaoService"/> registrado (ambiente sem provedor
    /// SMTP), o envio vira no-op e a mensagem é consumida mesmo assim (nada a entregar). Falha de envio não
    /// trava o job (try/catch + <c>RegistrarFalha</c>): a mensagem fica para retry conforme o padrão do outbox.
    ///
    /// ⚠️ Dependência de AMBIENTE: o provedor SMTP real do <see cref="INotificacaoService"/> é externo. Sem ele
    /// registrado (ex.: testes/dev), o serviço resolve vazio → no-op/log. Registrado na MC da 1.08.
    /// </summary>
    [DisallowConcurrentExecution]
    public class GestaoClientesOutboxProcessorJob : IJob
    {
        private static readonly string[] EventosTratados =
        {
            "FaturaAlertaCobrancaEvent",
            "TrialEncerradoEvent",
            "ReciboEmitidoEvent",
            "PagamentoEstornadoEvent",
            // TRANSVERSAL T1 — eventos que eram PUBLICADOS mas MORRIAM na fila (nenhum consumidor os drenava,
            // apesar do comentário dos handlers dizerem "o GestaoClientesOutboxProcessorJob entrega"). Como este
            // job é o ÚNICO leitor de plataforma.outbox_messages (ContextGestaoClientes E ContextFiscal apontam
            // para a MESMA tabela física), NÃO se registra um segundo dispatcher (evita corrida no flag processado):
            // ele passa a ser o drenador único também destes eventos.
            //  • mudança/cancelamento/reativação de plano  -> NOTIFICAÇÃO REAL ao cliente (efeito preservado da 1.08D);
            //  • ComissaoApurada (factual) e DocumentoFiscal* -> FALLBACK (pendência de regra: efeito já é síncrono na
            //    apuração/emissão; NÃO se inventa regra fiscal/contábil aqui — valida-contador).
            CatalogoEventosIntegracao.Assinatura.PlanoAlterado,
            CatalogoEventosIntegracao.Assinatura.AssinaturaCancelada,
            CatalogoEventosIntegracao.Assinatura.AssinaturaReativada,
            CatalogoEventosIntegracao.Assinatura.ComissaoApurada,
            CatalogoEventosIntegracao.Fiscal.DocumentoFiscalAutorizado,
            CatalogoEventosIntegracao.Fiscal.DocumentoFiscalCancelado
        };

        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        private readonly ContextGestaoClientes _context;
        private readonly IEnumerable<INotificacaoService> _notificacaoServices;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Injetamos IEnumerable<INotificacaoService> (e não INotificacaoService direto) para manter o padrão
        // resiliente do boas-vindas (1.07): sem provedor registrado a coleção resolve vazia e o envio vira no-op,
        // sem quebrar quem não configura SMTP (testes/dev/providers alternativos).
        public GestaoClientesOutboxProcessorJob(
            ContextGestaoClientes context,
            IEnumerable<INotificacaoService> notificacaoServices,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _notificacaoServices = notificacaoServices;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("[Quartz] Iniciando GestaoClientesOutboxProcessorJob...");

            var messages = await _context.OutboxMessages
                .IgnoreQueryFilters()
                .Where(m => EventosTratados.Contains(m.EventType)
                            && m.ProcessadoEm == null
                            && m.Tentativas < 5)
                .OrderBy(m => m.CriadoEm)
                .ToListAsync();

            if (!messages.Any())
            {
                Console.WriteLine("[Quartz] Nenhuma mensagem do outbox de GestaoClientes para processar.");
                return;
            }

            var notificador = _notificacaoServices?.FirstOrDefault();

            foreach (var message in messages)
            {
                // Isola o tenant da mensagem no HttpContext (consistente com os demais processadores).
                var httpContext = new DefaultHttpContext();
                httpContext.Items["TenantId"] = message.TenantId;
                if (_httpContextAccessor != null)
                    _httpContextAccessor.HttpContext = httpContext;

                try
                {
                    switch (message.EventType)
                    {
                        case "FaturaAlertaCobrancaEvent":
                            await ProcessarAlertaCobrancaAsync(message.Payload, notificador);
                            break;
                        case "TrialEncerradoEvent":
                            await ProcessarTrialEncerradoAsync(message.Payload, notificador);
                            break;
                        case "ReciboEmitidoEvent":
                            await ProcessarReciboEmitidoAsync(message.Payload, notificador);
                            break;
                        case "PagamentoEstornadoEvent":
                            await ProcessarPagamentoEstornadoAsync(message.Payload, notificador);
                            break;
                        case "PlanoAlteradoEvent":
                            await ProcessarPlanoAlteradoAsync(message.Payload, notificador);
                            break;
                        case "AssinaturaCanceladaEvent":
                            await ProcessarAssinaturaCanceladaAsync(message.Payload, notificador);
                            break;
                        case "AssinaturaReativadaEvent":
                            await ProcessarAssinaturaReativadaAsync(message.Payload, notificador);
                            break;
                        case "ComissaoApuradaEvent":
                        case "DocumentoFiscalAutorizado":
                        case "DocumentoFiscalCancelado":
                            // FALLBACK (pendência de regra): evento CONHECIDO do catálogo, drenado por este leitor único,
                            // mas SEM efeito in-process próprio. ComissaoApurada é factual (a apuração já grava a
                            // ApuracaoComissao + campos da Fatura de forma síncrona); DocumentoFiscal* não tem consumidor
                            // de efeito e o efeito fiscal/financeiro é síncrono na emissão — ⚠️ valida-contador, NÃO se
                            // inventa regra aqui. Loga a pendência e MARCA como processado (não deixa acumular na fila).
                            Console.WriteLine($"[Quartz] GestaoClientesOutbox: evento '{message.EventType}' (tenant {message.TenantId}, msg {message.Id}) drenado SEM consumidor de efeito (PENDENTE DE REGRA — registrar em DECISOES-PENDENTES).");
                            break;
                    }

                    message.MarcarProcessado();
                }
                catch (Exception ex)
                {
                    // Falha de envio NÃO trava o job: registra a tentativa e deixa a mensagem para retry.
                    message.RegistrarFalha(ex.Message);
                    Console.WriteLine($"[Quartz] Erro ao processar mensagem de GestaoClientes {message.Id} ({message.EventType}): {ex.Message}");
                }

                _context.OutboxMessages.Update(message);
                await _context.SaveChangesAsync();
            }

            Console.WriteLine("[Quartz] GestaoClientesOutboxProcessorJob concluído.");
        }

        private async Task ProcessarAlertaCobrancaAsync(string payloadJson, INotificacaoService? notificador)
        {
            var payload = JsonSerializer.Deserialize<FaturaAlertaCobrancaPayload>(payloadJson, JsonOpts);
            if (payload == null) return;

            var cliente = await ResolverClienteAsync(payload.ClienteId);
            if (notificador == null || cliente == null || string.IsNullOrWhiteSpace(cliente.Email))
                return; // sem provedor ou sem destinatário → no-op (mensagem é consumida assim mesmo).

            var assunto = $"[EprosERP] Aviso de cobrança {payload.TipoAlerta} — fatura {payload.FaturaId}";
            var corpo =
                $"<p>Olá, {cliente.RazaoSocial}.</p>" +
                $"<p>{payload.Mensagem}</p>" +
                (string.IsNullOrWhiteSpace(payload.PixCopiaECola)
                    ? string.Empty
                    : $"<p><strong>Pix copia e cola:</strong><br/>{payload.PixCopiaECola}</p>") +
                "<p>Equipe EprosERP.</p>";

            await notificador.EnviarEmailAsync(cliente.Email, assunto, corpo);
        }

        private async Task ProcessarTrialEncerradoAsync(string payloadJson, INotificacaoService? notificador)
        {
            var payload = JsonSerializer.Deserialize<TrialEncerradoPayload>(payloadJson, JsonOpts);
            if (payload == null) return;

            var cliente = await ResolverClienteAsync(payload.ClienteId);
            if (notificador == null || cliente == null || string.IsNullOrWhiteSpace(cliente.Email))
                return;

            var assunto = "[EprosERP] Seu período de avaliação terminou — 1ª fatura disponível";
            var trechoFatura = payload.FaturaId != null && payload.Valor != null
                ? $"<p>Sua primeira fatura no valor de <strong>R$ {payload.Valor:N2}</strong>" +
                  (payload.DataVencimento != null ? $", com vencimento em {payload.DataVencimento:dd/MM/yyyy}," : ",") +
                  " já está disponível na área do cliente.</p>"
                : "<p>Contrate um plano para continuar utilizando o EprosERP.</p>";
            var corpo =
                $"<p>Olá, {cliente.RazaoSocial}.</p>" +
                "<p>Seu período de avaliação (trial) foi encerrado.</p>" +
                trechoFatura +
                "<p>Efetue o pagamento para manter seus serviços ativos.</p>" +
                "<p>Equipe EprosERP.</p>";

            await notificador.EnviarEmailAsync(cliente.Email, assunto, corpo);
        }

        private async Task ProcessarReciboEmitidoAsync(string payloadJson, INotificacaoService? notificador)
        {
            var payload = JsonSerializer.Deserialize<ReciboEmitidoPayload>(payloadJson, JsonOpts);
            if (payload == null) return;

            var cliente = await ResolverClienteAsync(payload.ClienteId);
            if (notificador == null || cliente == null || string.IsNullOrWhiteSpace(cliente.Email))
                return;

            // TODO (fatia PDF do recibo): anexar/linkar o PDF do recibo quando a fatia de geração de PDF entrar.
            var assunto = $"[EprosERP] Recibo de pagamento {payload.ReciboNumero}";
            var corpo =
                $"<p>Olá, {cliente.RazaoSocial}.</p>" +
                $"<p>Confirmamos o pagamento da sua fatura. Segue o resumo do recibo:</p>" +
                "<ul>" +
                $"<li><strong>Recibo:</strong> {payload.ReciboNumero}</li>" +
                $"<li><strong>Valor:</strong> R$ {payload.Valor:N2}</li>" +
                $"<li><strong>Meio de pagamento:</strong> {payload.MeioPagamento}</li>" +
                $"<li><strong>Data:</strong> {payload.DataPagamento:dd/MM/yyyy}</li>" +
                "</ul>" +
                "<p>Obrigado! Equipe EprosERP.</p>";

            await notificador.EnviarEmailAsync(cliente.Email, assunto, corpo);
        }

        private async Task ProcessarPagamentoEstornadoAsync(string payloadJson, INotificacaoService? notificador)
        {
            var payload = JsonSerializer.Deserialize<PagamentoEstornadoPayload>(payloadJson, JsonOpts);
            if (payload == null) return;

            var cliente = await ResolverClienteAsync(payload.ClienteId);
            if (notificador == null || cliente == null || string.IsNullOrWhiteSpace(cliente.Email))
                return; // sem provedor ou sem destinatário → no-op (mensagem é consumida assim mesmo).

            var assunto = $"[EprosERP] Estorno de pagamento — fatura {payload.FaturaId}";
            var corpo =
                $"<p>Olá, {cliente.RazaoSocial}.</p>" +
                $"<p>Registramos o estorno do pagamento no valor de <strong>R$ {payload.Valor:N2}</strong>" +
                (payload.DataEstorno != null ? $" em {payload.DataEstorno:dd/MM/yyyy}" : string.Empty) + ".</p>" +
                (string.IsNullOrWhiteSpace(payload.Motivo) ? string.Empty : $"<p>Motivo: {payload.Motivo}</p>") +
                "<p>Em caso de dúvidas, fale com nosso time. Equipe EprosERP.</p>";

            await notificador.EnviarEmailAsync(cliente.Email, assunto, corpo);
        }

        // ===== 1.08D — notificações de ciclo de assinatura (efeito REAL preservado; morriam na fila antes) =====

        private async Task ProcessarPlanoAlteradoAsync(string payloadJson, INotificacaoService? notificador)
        {
            var payload = JsonSerializer.Deserialize<PlanoAlteradoPayload>(payloadJson, JsonOpts);
            if (payload == null) return;

            var cliente = await ResolverClienteAsync(payload.ClienteId);
            if (notificador == null || cliente == null || string.IsNullOrWhiteSpace(cliente.Email))
                return; // sem provedor ou sem destinatário → no-op (mensagem é consumida assim mesmo).

            var titulo = payload.TipoMudanca switch
            {
                "Upgrade" => "Seu plano foi atualizado (upgrade)",
                "Downgrade" => "Seu plano foi alterado (downgrade)",
                _ => "Seu plano foi alterado"
            };
            var trechoProracao = payload.TipoProracao switch
            {
                "Debito" => $"<p>Foi gerada uma cobrança de ajuste proporcional no valor de <strong>R$ {payload.ValorProracao:N2}</strong>.</p>",
                "Credito" => $"<p>Um crédito proporcional de <strong>R$ {Math.Abs(payload.ValorProracao):N2}</strong> será compensado na sua próxima fatura.</p>",
                _ => string.Empty
            };
            var assunto = $"[EprosERP] {titulo}";
            var corpo =
                $"<p>Olá, {cliente.RazaoSocial}.</p>" +
                $"<p>{titulo} com sucesso.</p>" +
                trechoProracao +
                "<p>Equipe EprosERP.</p>";

            await notificador.EnviarEmailAsync(cliente.Email, assunto, corpo);
        }

        private async Task ProcessarAssinaturaCanceladaAsync(string payloadJson, INotificacaoService? notificador)
        {
            var payload = JsonSerializer.Deserialize<AssinaturaCanceladaPayload>(payloadJson, JsonOpts);
            if (payload == null) return;

            var cliente = await ResolverClienteAsync(payload.ClienteId);
            if (notificador == null || cliente == null || string.IsNullOrWhiteSpace(cliente.Email))
                return;

            var assunto = "[EprosERP] Assinatura cancelada — acesso somente-leitura por 30 dias";
            var corpo =
                $"<p>Olá, {cliente.RazaoSocial}.</p>" +
                "<p>Sua assinatura foi cancelada. Você mantém acesso somente-leitura por 30 dias para exportar seus dados; " +
                "reative quando quiser dentro desse período.</p>" +
                (string.IsNullOrWhiteSpace(payload.Motivo) ? string.Empty : $"<p>Motivo informado: {payload.Motivo}</p>") +
                "<p>Equipe EprosERP.</p>";

            await notificador.EnviarEmailAsync(cliente.Email, assunto, corpo);
        }

        private async Task ProcessarAssinaturaReativadaAsync(string payloadJson, INotificacaoService? notificador)
        {
            var payload = JsonSerializer.Deserialize<AssinaturaReativadaPayload>(payloadJson, JsonOpts);
            if (payload == null) return;

            var cliente = await ResolverClienteAsync(payload.ClienteId);
            if (notificador == null || cliente == null || string.IsNullOrWhiteSpace(cliente.Email))
                return;

            var assunto = "[EprosERP] Assinatura reativada";
            var corpo =
                $"<p>Olá, {cliente.RazaoSocial}.</p>" +
                "<p>Sua assinatura foi reativada com sucesso e seus serviços voltaram a ficar ativos.</p>" +
                "<p>Bem-vindo(a) de volta! Equipe EprosERP.</p>";

            await notificador.EnviarEmailAsync(cliente.Email, assunto, corpo);
        }

        private async Task<ClienteDestinatario?> ResolverClienteAsync(Guid clienteId)
        {
            if (clienteId == Guid.Empty) return null;

            return await _context.Clientes
                .IgnoreQueryFilters()
                .Where(c => c.Id == clienteId && c.DeletadoEm == null)
                .Select(c => new ClienteDestinatario { Email = c.Email, RazaoSocial = c.RazaoSocial })
                .FirstOrDefaultAsync();
        }

        private sealed class ClienteDestinatario
        {
            public string Email { get; set; } = string.Empty;
            public string RazaoSocial { get; set; } = string.Empty;
        }

        private sealed class FaturaAlertaCobrancaPayload
        {
            public Guid FaturaId { get; set; }
            public Guid ClienteId { get; set; }
            public decimal Valor { get; set; }
            public DateTime DataVencimento { get; set; }
            public int DiasDiferenca { get; set; }
            public string TipoAlerta { get; set; } = string.Empty;
            public string PixCopiaECola { get; set; } = string.Empty;
            public string Mensagem { get; set; } = string.Empty;
            public DateTime EnviadoEm { get; set; }
        }

        private sealed class TrialEncerradoPayload
        {
            public Guid ClienteId { get; set; }
            public string TenantId { get; set; } = string.Empty;
            public Guid? FaturaId { get; set; }
            public decimal? Valor { get; set; }
            public DateTime? DataVencimento { get; set; }
        }

        private sealed class ReciboEmitidoPayload
        {
            public Guid ClienteId { get; set; }
            public string TenantId { get; set; } = string.Empty;
            public Guid FaturaId { get; set; }
            public string ReciboNumero { get; set; } = string.Empty;
            public decimal Valor { get; set; }
            public string MeioPagamento { get; set; } = string.Empty;
            public DateTime DataPagamento { get; set; }
        }

        private sealed class PlanoAlteradoPayload
        {
            public Guid AssinaturaId { get; set; }
            public Guid ClienteId { get; set; }
            public string TenantId { get; set; } = string.Empty;
            public Guid PlanoAnteriorId { get; set; }
            public Guid PlanoNovoId { get; set; }
            public string TipoMudanca { get; set; } = string.Empty;
            public decimal ValorProracao { get; set; }
            public string TipoProracao { get; set; } = string.Empty;
            public Guid? FaturaDiferencaId { get; set; }
        }

        private sealed class AssinaturaCanceladaPayload
        {
            public Guid AssinaturaId { get; set; }
            public Guid ClienteId { get; set; }
            public string TenantId { get; set; } = string.Empty;
            public string Motivo { get; set; } = string.Empty;
            public DateTime? CanceladaEm { get; set; }
            public string CanceladaPor { get; set; } = string.Empty;
        }

        private sealed class AssinaturaReativadaPayload
        {
            public Guid AssinaturaId { get; set; }
            public Guid ClienteId { get; set; }
            public string TenantId { get; set; } = string.Empty;
            public string ReativadaPor { get; set; } = string.Empty;
            public DateTime? ReativadaEm { get; set; }
        }

        private sealed class PagamentoEstornadoPayload
        {
            public Guid ClienteId { get; set; }
            public string TenantId { get; set; } = string.Empty;
            public Guid FaturaId { get; set; }
            public Guid PagamentoFaturaId { get; set; }
            public Guid? PedidoId { get; set; }
            public decimal? Valor { get; set; }
            public string? RefundId { get; set; }
            public string? Motivo { get; set; }
            public DateTime? DataEstorno { get; set; }
        }
    }
}
