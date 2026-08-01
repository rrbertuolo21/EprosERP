---
title: "Caddy 2 — reverse proxy e TLS automático"
confluence_id: "194707457"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/194707457/Caddy+2+reverse+proxy+e+TLS+autom+tico"
last_updated: "2026-07-06"
---

> [!NOTE]
> **Versão fixada:** `2.x`

### Por que Caddy vs Nginx

| Critério | Caddy 2 | Nginx |
| --- | --- | --- |
| TLS automático | ✅ Let's Encrypt sem config | ❌ Manual / Certbot |
| HTTP/3 | ✅ Nativo | Plugin |
| Configuração | Declarativa, simples | Verbosa, errática |
| Renovação de cert | Automática | Precisa de cron |

```
# Caddyfile — configuração completa de produção
# (Nginx equivalente teria 100+ linhas)

api.epros.com.br {
    reverse_proxy epros-back:7000

    header {
        Strict-Transport-Security "max-age=31536000; includeSubDomains"
        X-Content-Type-Options "nosniff"
        X-Frame-Options "DENY"
    }
}

app.epros.com.br {
    reverse_proxy epros-front:3000
}

gateway.epros.com.br {
    reverse_proxy epros-api:8000

    rate_limit {
        zone api_zone {
            match path /v1/*
            key {remote_host}
            rate 100r/s
        }
    }
}
```
