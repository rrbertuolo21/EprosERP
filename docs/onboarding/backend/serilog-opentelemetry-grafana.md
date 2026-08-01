---
title: "Serilog + OpenTelemetry + Grafana Stack — observabilidade"
confluence_id: "193986561"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/193986561/Serilog+OpenTelemetry+Grafana+Stack+observabilidade"
last_updated: "2026-07-06"
---

Pilha completa self-hosted

### Por que isso importa

No legado: logs em arquivo no `wwwroot`. Quando algo quebra em produção, a equipe descobre porque o cliente liga. MTTD (Mean Time to Detect) = tempo do cliente reclamar.

Com a nova stack: alerta automático em segundos, dashboard por tenant, trace de cada requisição.

### Serilog — logs estruturados

```csharp
// Program.cs — configuração do Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "epros-back")
    // Enricher de tenant — toda linha de log tem o tenantId
    .Enrich.With(new TenantEnricher(_tenantProvider))
    // DataMasking — CPF/CNPJ/PAN nunca aparecem em log
    .Enrich.With(new DataMaskingEnricher())
    // Sink para Loki (self-hosted) — logs pesquisáveis centralizados
    .WriteTo.GrafanaLoki("http://loki:3100")
    // Em dev: console legível
    .WriteTo.Console(new ExpressionTemplate(
        "[{@t:HH:mm:ss} {TenantId} {@l:u3}] {@m}\n{@x}"))
    .CreateLogger();

// Uso nos handlers — structured logging
public class LancarCompraHandler : IRequestHandler<LancarCompraCommand, CommandResult>
{
    private readonly ILogger<LancarCompraHandler> _logger;

    public async Task<CommandResult> Handle(LancarCompraCommand cmd, CancellationToken ct)
    {
        // Log estruturado: campos indexáveis no Loki, não string concatenada
        _logger.LogInformation(
            "Lançando compra para fornecedor {FornecedorId} com {QuantidadeItens} itens",
            cmd.FornecedorId, cmd.Itens.Count);

        // ... handler logic ...

        _logger.LogInformation(
            "Compra {CompraId} lançada com sucesso. Total: {ValorTotal}",
            compra.Id, compra.ValorTotal);

        return CommandResult.Ok(compra.Id);
    }
}
```

### OpenTelemetry — traces distribuídos

```
// Todo request HTTP gera um trace com spans encadeados
// Permite ver EXATAMENTE onde uma requisição passou e quanto tempo demorou

// GET /api/v1/contas-a-pagar?vencimento=2026-07-01
//   ├─ [2ms]  Authentication middleware
//   ├─ [1ms]  TenantSaaSMiddleware (cache hit)
//   ├─ [3ms]  ModuloTenantMiddleware
//   ├─ [45ms] ContasPagarQuery → Handler
//   │   ├─ [40ms] PostgreSQL query (← aqui está o gargalo)
//   │   └─ [2ms]  Serialize response
//   └─ [1ms]  AuditMiddleware

// Quando P95 > 200ms → alerta automático no Grafana → dev investiga o trace
```

### SLOs e alertas

```
# Alertas configurados no Grafana/Prometheus

# Leitura lenta (P95 > 200ms)
- alert: EprosReadSLOViolation
  expr: histogram_quantile(0.95, rate(http_request_duration_seconds_bucket{job="epros-back",method="GET"}[5m])) > 0.2
  for: 2m
  annotations:
    summary: "P95 de leitura acima de 200ms"

# Escrita lenta (P95 > 500ms)
- alert: EprosWriteSLOViolation
  expr: histogram_quantile(0.95, rate(http_request_duration_seconds_bucket{job="epros-back",method!="GET"}[5m])) > 0.5

# Taxa de erro alta (> 1% de 5xx)
- alert: EprosHighErrorRate
  expr: rate(http_requests_total{job="epros-back",status=~"5.."}[5m]) / rate(http_requests_total{job="epros-back"}[5m]) > 0.01
```
