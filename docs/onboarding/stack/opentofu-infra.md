---
title: "OpenTofu 1.7 — infraestrutura como código"
confluence_id: "194412548"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/194412548/OpenTofu+1.7+infraestrutura+como+c+digo"
last_updated: "2026-07-06"
---

> [!NOTE]
> **Versão fixada:** `1.7+`

### O que o OpenTofu provisiona para o Epros

```
# Exemplo: provisionar ambiente completo em qualquer cloud ou VPS

module "epros_stack" {
  source = "./modules/epros"

  # Muda só isso para mudar o destino (VPS, AWS, GCP, on-premise do cliente)
  environment = "production"
  provider    = "hetzner"  # ou "aws", "gcp", "on-premise"

  # O código de infra é o mesmo — só o provider muda
  postgresql = {
    version = "16"
    storage = "100Gi"
  }

  keycloak = {
    version = "24.0"
    realm   = "epros"
  }
}
```
