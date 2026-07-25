using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Application.EventHandlers;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Epros.Modules.Financeiro.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class OutboxProcessorJob : IJob
    {
        private readonly ContextFinanceiro _context;
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OutboxProcessorJob(
            ContextFinanceiro context,
            IMediator mediator,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("[Quartz] Iniciando OutboxProcessorJob para o módulo Financeiro...");

            // Busca mensagens do outbox do Estoque (estoque.outbox_messages) não processadas do tipo CompraLancada ou CompraCancelada
            var messages = await _context.OutboxMessages
                .IgnoreQueryFilters()
                .Where(m => (m.EventType == "CompraLancada" || m.EventType == "CompraCancelada") && m.ProcessadoEm == null && m.Tentativas < 5)
                .OrderBy(m => m.CriadoEm)
                .ToListAsync();

            if (!messages.Any())
            {
                Console.WriteLine("[Quartz] Nenhuma mensagem do outbox para processar.");
                return;
            }

            foreach (var message in messages)
            {
                // Configura o HttpContext com o tenant da mensagem para garantir isolamento
                var httpContext = new DefaultHttpContext();
                httpContext.Items["TenantId"] = message.TenantId;
                _httpContextAccessor.HttpContext = httpContext;

                try
                {
                    if (message.EventType == "CompraLancada")
                    {
                        var payload = JsonSerializer.Deserialize<CompraLancadaPayload>(message.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload != null)
                        {
                            var cnpjLimpo = CleanCnpj(payload.FornecedorCnpj);
                            var fornecedor = await _context.PessoasJuridicasLookup
                                .IgnoreQueryFilters()
                                .FirstOrDefaultAsync(pj => pj.Cnpj == cnpjLimpo && pj.TenantId == message.TenantId);

                            Guid fornecedorId;

                            if (fornecedor != null)
                            {
                                fornecedorId = fornecedor.PessoaId;
                            }
                            else
                            {
                                // Criar novo fornecedor na tabela compartilhada
                                fornecedorId = Guid.NewGuid();
                                
                                var novaPessoa = new PessoaLookup
                                {
                                    Id = fornecedorId,
                                    EhFornecedor = true,
                                    Status = 3, // Ativo
                                    TipoPessoa = 2, // Juridica
                                    TenantId = message.TenantId,
                                    CriadoPor = "system_outbox",
                                    CriadoEm = DateTime.UtcNow
                                };

                                var novaPessoaJuridica = new PessoaJuridicaLookup
                                {
                                    PessoaId = fornecedorId,
                                    Cnpj = cnpjLimpo,
                                    RazaoSocial = payload.FornecedorNome,
                                    TenantId = message.TenantId,
                                    CriadoPor = "system_outbox",
                                    CriadoEm = DateTime.UtcNow
                                };

                                _context.PessoasLookup.Add(novaPessoa);
                                _context.PessoasJuridicasLookup.Add(novaPessoaJuridica);
                                await _context.SaveChangesAsync();
                            }

                            var notification = new CompraLancadaEventNotification(
                                CompraId: payload.CompraId,
                                FornecedorId: fornecedorId,
                                ValorTotal: payload.ValorTotal,
                                DataVencimento: DateTime.UtcNow.AddDays(30).Date, // Vencimento padrão de 30 dias
                                NumeroNota: payload.NumeroNota,
                                TenantId: message.TenantId,
                                UserId: "system_outbox",
                                Itens: payload.Itens.Select(i => new CompraLancadaItemNotification(i.Sku, i.NomeProduto, i.Quantidade, i.PrecoUnitario)).ToList()
                            );

                            await _mediator.Publish(notification);
                        }
                    }
                    else if (message.EventType == "CompraCancelada")
                    {
                        var payload = JsonSerializer.Deserialize<CompraCanceladaPayload>(message.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload != null)
                        {
                            var cnpjLimpo = CleanCnpj(payload.FornecedorCnpj);
                            var fornecedor = await _context.PessoasJuridicasLookup
                                .IgnoreQueryFilters()
                                .FirstOrDefaultAsync(pj => pj.Cnpj == cnpjLimpo && pj.TenantId == message.TenantId);

                            Guid fornecedorId = fornecedor != null ? fornecedor.PessoaId : Guid.Empty;

                            var notification = new CompraCanceladaEventNotification(
                                CompraId: payload.CompraId,
                                FornecedorId: fornecedorId,
                                ValorTotal: payload.ValorTotal,
                                NumeroNota: payload.NumeroNota,
                                TenantId: message.TenantId,
                                UserId: "system_outbox"
                            );

                            await _mediator.Publish(notification);
                        }
                    }

                    message.MarcarProcessado();
                }
                catch (Exception ex)
                {
                    message.RegistrarFalha(ex.Message);
                    Console.WriteLine($"[Quartz] Erro ao processar mensagem {message.Id}: {ex.Message}");
                }

                _context.OutboxMessages.Update(message);
                await _context.SaveChangesAsync();
            }

            Console.WriteLine("[Quartz] OutboxProcessorJob concluído.");
        }

        private static string CleanCnpj(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj)) return string.Empty;
            return new string(cnpj.Where(char.IsDigit).ToArray());
        }

        private class CompraLancadaPayload
        {
            public Guid CompraId { get; set; }
            public string TenantId { get; set; } = string.Empty;
            public string FornecedorCnpj { get; set; } = string.Empty;
            public string FornecedorNome { get; set; } = string.Empty;
            public string NumeroNota { get; set; } = string.Empty;
            public decimal ValorTotal { get; set; }
            public System.Collections.Generic.List<CompraLancadaItemPayload> Itens { get; set; } = new();
        }

        private class CompraLancadaItemPayload
        {
            public string Sku { get; set; } = string.Empty;
            public string NomeProduto { get; set; } = string.Empty;
            public decimal Quantidade { get; set; }
            public decimal PrecoUnitario { get; set; }
        }

        private class CompraCanceladaPayload
        {
            public Guid CompraId { get; set; }
            public string TenantId { get; set; } = string.Empty;
            public string FornecedorCnpj { get; set; } = string.Empty;
            public string NumeroNota { get; set; } = string.Empty;
            public decimal ValorTotal { get; set; }
        }
    }
}
