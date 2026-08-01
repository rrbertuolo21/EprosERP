---
title: "Valkey 7 — cache e locks distribuídos"
confluence_id: "192086056"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/192086056/Valkey+7+cache+e+locks+distribu+dos"
last_updated: "2026-07-06"
---

**Versão fixada:** `7.x`

### Por que Valkey e não Redis

Redis mudou para BSL (Business Source License) em 2024 — não é mais open source. Valkey é o fork criado pela Linux Foundation com suporte de AWS, Google, Alibaba, Redis Labs ex-engenheiros. API 100% compatível — troca de uma linha no docker-compose.

### Usos no Epros

```csharp
// 1. Cache do Catalog DB — evita ir ao banco a cada request
// TenantSaaSMiddleware
public async Task<TenantConfig> GetTenantConfig(string tenantId)
{
    var cacheKey = $"tenant:{tenantId}:config";

    // Tenta o cache primeiro (TTL 5 minutos)
    var cached = await _valkey.GetStringAsync(cacheKey);
    if (cached is not null)
        return JsonSerializer.Deserialize<TenantConfig>(cached)!;

    // Cache miss — vai ao banco e armazena
    var config = await _catalogDb.GetTenantConfigAsync(tenantId);
    await _valkey.SetStringAsync(
        cacheKey,
        JsonSerializer.Serialize(config),
        TimeSpan.FromMinutes(5)); // TTL garante que config atualizada é refletida

    return config;
}

// 2. Lock distribuído — garante que o VencimentoWorker não rode em paralelo
public class VencimentoWorker : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var lockKey = "lock:vencimento-worker";
        var lockValue = Guid.NewGuid().ToString();

        // Tenta adquirir o lock (SET NX EX)
        var acquired = await _valkey.SetNXAsync(lockKey, lockValue, TimeSpan.FromMinutes(5));
        if (!acquired) return; // outra instância já está rodando

        try
        {
            await ProcessarVencimentos();
        }
        finally
        {
            // Libera o lock apenas se ainda é o nosso (evita liberar lock de outra instância)
            var script = @"
                if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('del', KEYS[1])
                else
                    return 0
                end";
            await _valkey.ScriptEvaluateAsync(script, new[] { lockKey }, new[] { lockValue });
        }
    }
}
```
