---
title: "YARP + ASP.NET Core — API Gateway público (epros-api)"
confluence_id: "194576387"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/194576387/YARP+ASP.NET+Core+API+Gateway+p+blico+epros-api"
last_updated: "2026-07-06"
---

> [!NOTE]
> **Tecnologia:** YARP (Yet Another Reverse Proxy)

### Por que YARP e não Kong, Nginx, ou AWS API Gateway

| Critério | YARP | Kong | AWS API Gateway |
| --- | --- | --- | --- |
| Linguagem | C# (time já sabe) | Lua/Go | JSON/YAML gerenciado |
| Cloud-agnostic | ✅ | ✅ | ❌ Lock-in AWS |
| Open source | ✅ MIT | Parcialmente | ❌ |
| Customização | Máxima (é código) | Plugins | Limitada |
| Observabilidade | OpenTelemetry nativo | Separado | CloudWatch |
| Custo | Zero | Free tier limitado | Por chamada |

### Arquitetura do epros-api

```mermaid
flowchart TB
    Clientes["🌐 Clientes Externos<br/>(parceiros, integradores, outros ERPs)"]

    subgraph gateway["🖥️ epros-api"]
        direction TB

        Auth["🔐 1. Autenticação Client Credentials"]
        Version["🔀 2. Versionamento /v1 /v2<br/>⚠️ ordem assumida"]
        DefRate{{"❓ Rate Limit por plano vs Throttling por tenant<br/>ordem ainda não definida"}}

        subgraph lim[" "]
            direction LR
            Rate["⏱️ 3a. Rate Limiting por plano API"]
            Throttle["🛑 3b. Throttling por tenant"]
        end

        Transform["🔃 4. Transformação de payload"]
        Audit["📋 5. Audit log de chamadas"]
        DefSuccess{{"❓ 'Sucesso' = passou no gateway ou = epros-back respondeu 2xx?<br/>gatilho ainda não definido"}}
        Billing["💰 6. Billing hooks (contador) — só em sucesso"]

        Auth --> Version
        Version --> DefRate
        DefRate -.-> Rate
        DefRate -.-> Throttle
        Rate --> Transform
        Throttle --> Transform
        Transform --> Audit
        Audit --> DefSuccess
        DefSuccess -.-> Billing
    end

    Back["🔐 epros-back<br/>não exposto ao mundo externo"]

    Clientes -->|requisição HTTP| Auth
    Billing -->|chamada interna validada| Back

    classDef externo fill:#f5c6a0,stroke:#c47a3a,color:#333
    classDef passo fill:#c5dff8,stroke:#6a9fd4,color:#333
    classDef assumido fill:#c5dff8,stroke:#6a9fd4,stroke-dasharray:5 5,color:#333
    classDef duvida fill:#ffffff,stroke:#c0392b,stroke-dasharray:5 5,color:#c0392b
    classDef interno fill:#a8d5a2,stroke:#5a9a52,color:#333
    classDef caixa fill:#faf6e8,stroke:#d4c48a,color:#333
    classDef invisivel fill:transparent,stroke:transparent,color:transparent

    class Clientes externo
    class Auth,Rate,Throttle,Transform,Audit,Billing passo
    class Version assumido
    class DefRate,DefSuccess duvida
    class Back interno
    class gateway caixa
    class lim invisivel
```

### Configuração básica do YARP

```csharp
// Program.cs do epros-api
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
```

```json
// appsettings.json
{
  "ReverseProxy": {
    "Routes": {
      "epros-back-v1": {
        "ClusterId": "epros-back",
        "Match": {
          "Path": "/v1/{**catch-all}"
        },
        "Transforms": [
          { "PathPattern": "/api/v1/{**catch-all}" },
          { "RequestHeader": "X-API-Client", "Set": "{client_id}" }
        ]
      }
    },
    "Clusters": {
      "epros-back": {
        "Destinations": {
          "primary": {
            "Address": "http://epros-back:7000/"
          }
        }
      }
    }
  }
}
```

### Rate limiting por plano

```csharp
// Middleware customizado de rate limiting
public class ApiRateLimitMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var clientId = context.User.FindFirstValue("client_id");
        var plano = await _planService.GetApiPlan(clientId);

        var limite = plano switch
        {
            "starter" => 1_000,      // 1k chamadas/dia
            "business" => 10_000,    // 10k chamadas/dia
            "enterprise" => 100_000, // 100k chamadas/dia
            _ => 100                 // sem plano = 100/dia
        };

        var contadorKey = $"api:calls:{clientId}:{DateTime.UtcNow:yyyy-MM-dd}";
        var atual = await _valkey.IncrementAsync(contadorKey);

        if (atual == 1)
            await _valkey.ExpireAsync(contadorKey, TimeSpan.FromDays(1));

        if (atual > limite)
        {
            context.Response.StatusCode = 429;
            context.Response.Headers["X-RateLimit-Limit"] = limite.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = "0";
            context.Response.Headers["Retry-After"] = "86400";
            return;
        }

        context.Response.Headers["X-RateLimit-Limit"] = limite.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = (limite - atual).ToString();

        await next(context);
    }
}
```
