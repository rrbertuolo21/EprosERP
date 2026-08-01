---
title: "Capacitor 6 — mobile e PDV"
confluence_id: "194248706"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/194248706/Capacitor+6+mobile+e+PDV"
last_updated: "2026-07-06"
---

> [!NOTE]
> **Versão fixada:** `6.x`

### Por que Capacitor vs React Native (ADR-009)

| Critério | Capacitor | React Native |
| --- | --- | --- |
| Reutiliza código Nuxt 4 | ✅ 100% | ❌ Nova codebase |
| Mesmo time frontend | ✅ | ❌ Devs React separados |
| Performance nativa | Web View (boa o suficiente) | Nativa (melhor) |
| Plugins nativos | ✅ Impressora, câmera, biometria | ✅ |
| Curva de aprendizado | Mínima (já sabe Vue/Nuxt) | Alta (React + Mobile) |

### O que Capacitor faz

```
epros-front (Nuxt 4)
      ↓
  Capacitor 6
      ↓
┌─────────────────────────────────┐
│  iOS (Swift Wrapper)            │
│  Android (Kotlin Wrapper)       │
│                                 │
│  Web View carrega a app Nuxt 4  │
│  Plugins nativos disponíveis:   │
│  - Câmera                       │
│  - Impressora térmica           │
│  - Biometria (Face ID / Touch)  │
│  - NFC                          │
│  - SQLite (offline)             │
└─────────────────────────────────┘
```
