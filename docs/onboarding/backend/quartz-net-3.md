---
title: "Quartz.NET 3 — jobs agendados"
confluence_id: "192512017"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/192512017/Quartz.NET+3+jobs+agendados"
last_updated: "2026-07-06"
---

> [!NOTE]
> **Versão fixada:** `3.x`

### Por que Quartz.NET vs Hangfire (ADR-015)

| Critério | Quartz.NET | Hangfire |
| --- | --- | --- |
| Banco de persistência | PostgreSQL nativo | Requer SQL Server ou banco separado |
| Licença | Apache 2.0 | Hangfire Core livre; Dashboard = pago |
| Clustering | Nativo com PG | Pago (Hangfire Pro) |
| Dashboard público | Não tem (Grafana) | Tinha — era o bug do legado |

### Jobs que o Epros vai ter

```csharp
// 1. OutboxWorker — entrega de Domain Events (a cada 5s)
[DisallowConcurrentExecution] // nunca duas instâncias ao mesmo tempo
public class OutboxWorker : IJob { ... }

// 2. VencimentoWorker — marca títulos vencidos (diário, 00:05)
public class VencimentoWorker : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        // Busca todos os tenants ativos
        var tenants = await _catalogDb.GetTenantIds();

        foreach (var tenantId in tenants)
        {
            // Para cada tenant, processa separadamente
            // Isolamento garantido — um tenant não vaza para outro
            var vencidas = await _repo.BuscarVencidas(tenantId, DateTime.UtcNow);
            foreach (var cp in vencidas)
            {
                cp.MarcarVencida();
                cp.AdicionarEvento(new ContaPagarVencida(cp.Id, tenantId));
            }
        }

        await _uow.CommitAsync();
    }
}

// 3. AnonimizacaoLGPDWorker — apaga dados de ex-clientes (mensal)
// 4. FaturasSaaSWorker — gera cobranças recorrentes dos planos (mensal)
// 5. RelatorioFiscalWorker — consolida SPED (mensal)

// Registro no Startup
builder.Services.AddQuartz(q =>
{
    q.UsePersistentStore(s =>
    {
        s.UsePostgres(connectionString);
        s.UseJsonSerializer();
    });

    // Outbox: a cada 5 segundos
    q.ScheduleJob<OutboxWorker>(trigger => trigger
        .WithIdentity("outbox-trigger")
        .WithSimpleSchedule(x => x.WithIntervalInSeconds(5).RepeatForever()));

    // Vencimento: todo dia às 00:05
    q.ScheduleJob<VencimentoWorker>(trigger => trigger
        .WithIdentity("vencimento-trigger")
        .WithCronSchedule("0 5 0 * * ?"));
});
```
