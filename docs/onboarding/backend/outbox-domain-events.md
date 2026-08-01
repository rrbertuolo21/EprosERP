---
title: "Outbox Pattern + Domain Events — comunicação entre módulos"
confluence_id: "192086045"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/192086045/Outbox+Pattern+Domain+Events+comunica+o+entre+m+dulos"
last_updated: "2026-07-06"
---

> [!NOTE]
> **Padrão central de integração**

### O problema que resolve

```
❌ SEM OUTBOX: se o sistema cair depois de salvar a venda mas antes de criar a ContaAReceber
   → Venda existe no banco
   → ContaAReceber não foi criada
   → Inconsistência de dados que ninguém detecta

✅ COM OUTBOX: o evento "VendaFaturada" é gravado na MESMA TRANSAÇÃO da venda
   → Sistema cai? O evento sobrevive no banco
   → Worker entrega o evento quando o sistema voltar
   → ContaAReceber é criada com certeza
```

### Implementação completa

```csharp
// Shared/DomainObjects/OutboxMessage.cs
public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;  // "VendaFaturada"
    public string Payload { get; set; } = string.Empty;    // JSON do evento
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessadoEm { get; set; }            // null = pendente
    public string? Erro { get; set; }
    public int Tentativas { get; set; } = 0;
}

// UnitOfWork — publica eventos para Outbox na mesma transação
public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;

    public async Task CommitAsync(CancellationToken ct = default)
    {
        // Coleta eventos de todas as entidades modificadas
        var entidades = _context.ChangeTracker
            .Entries<EntidadeSaaSBase>()
            .Where(e => e.Entity.Eventos.Any())
            .Select(e => e.Entity)
            .ToList();

        // Serializa cada evento como OutboxMessage
        foreach (var entidade in entidades)
        {
            foreach (var evento in entidade.Eventos)
            {
                _context.Set<OutboxMessage>().Add(new OutboxMessage
                {
                    TenantId = entidade.TenantId,
                    EventType = evento.GetType().Name,
                    Payload = JsonSerializer.Serialize(evento, evento.GetType())
                });
            }
            entidade.LimparEventos();
        }

        // Salva entidades + outbox em UMA única transação
        await _context.SaveChangesAsync(ct);
    }
}

// Outbox Worker — Quartz.NET roda a cada 5 segundos
public class OutboxWorker : IJob
{
    private readonly DbContext _context;
    private readonly IEventDispatcher _dispatcher;

    public async Task Execute(IJobExecutionContext context)
    {
        // Busca mensagens pendentes (sem ProcessadoEm)
        var mensagens = await _context.Set<OutboxMessage>()
            .Where(m => m.ProcessadoEm == null && m.Tentativas < 5)
            .OrderBy(m => m.CriadoEm)
            .Take(100)
            .ToListAsync();

        foreach (var msg in mensagens)
        {
            try
            {
                await _dispatcher.Dispatch(msg.EventType, msg.Payload);
                msg.ProcessadoEm = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                msg.Tentativas++;
                msg.Erro = ex.Message;
                // Tentou 5 vezes? Vai para dead letter — alerta no Grafana
            }
        }

        await _context.SaveChangesAsync();
    }
}
```

### Mapa de Domain Events do Epros

```
VendaFaturada        VENDAS    → FINANCEIRO (cria ContaReceber)
                               → FISCAL (autoriza NF-e / NFC-e)

VendaCancelada       VENDAS    → FINANCEIRO (estorna ContaReceber)
                               → FISCAL (cancela DFe)
                               → ESTOQUE (devolve estoque)

CompraLancada        ESTOQUE   → FINANCEIRO (cria ContaPagar)
                               → FISCAL (NF-e de entrada)

MercadoriaRecebida   ESTOQUE   → QUALIDADE (dispara inspeção)
                               → FINANCEIRO (confirma ContaPagar)

FolhaProcessada      RH        → FINANCEIRO (provisão salarial no GL)

ColaboradorDesligado HCM       → GRC (fecha perfil de acesso)
                               → KEYCLOAK (revoga todos os tokens)

CaixaFechado         PDV       → FINANCEIRO (lança resumo no fluxo de caixa)

ViolacaoSoDDetectada GRC       → GRC (abre incidente de auditoria)
                               → KEYCLOAK (bloqueia acesso temporário)
```
