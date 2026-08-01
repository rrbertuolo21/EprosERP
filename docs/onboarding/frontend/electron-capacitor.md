---
title: "Electron + Capacitor — mesmo repositório do front"
confluence_id: "194609153"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/194609153/Electron+Capacitor+mesmo+reposit+rio+do+front"
last_updated: "2026-07-06"
---

> [!NOTE]
> **Status:** FASE 2 (Bloco 9 e 10)
> **Repositório:** `epros-front`

O desktop (Electron) e o mobile (Capacitor) **não têm repositório separado**. Vivem dentro de `epros-front` como camadas sobre o mesmo código Nuxt 4.

### Estrutura no epros-front

```
epros-front/
├── app/                      ← código Nuxt 4 puro (compila para todos)
│   ├── pages/
│   ├── components/
│   ├── composables/
│   └── stores/
│
├── electron/                 ← shell do desktop (só o wrapper nativo)
│   ├── main.js               ← processo principal Electron
│   ├── preload.js            ← bridge segura Electron ↔ web
│   └── sqlite/               ← banco local + motor de sync
│
└── capacitor/                ← configuração mobile
    ├── capacitor.config.ts
    ├── ios/                  ← gerado pelo Capacitor (não editar manualmente)
    └── android/              ← gerado pelo Capacitor (não editar manualmente)
```

### Regra de ouro

```
Lógica de negócio ou interface?  → vai em app/ (Nuxt 4)
Toca hardware ou sistema operacional?
  → desktop: vai em electron/
  → mobile:  vai em capacitor/
```

### Pipeline de build — uma fonte, três targets

```
nuxt build → dist/
    ├── electron-builder --dir dist/  → Epros.App (.exe / .dmg / .deb)
    └── cap sync → cap build ios      → Epros iOS
                 → cap build android  → Epros Android
```

### O que o desktop faz que o web não faz

```
✅ Funciona sem internet (PDV offline, operação em loja)
✅ Motor fiscal local (NFC-e em contingência sem internet)
✅ Impressora térmica (via driver local)
✅ SQLite local com sincronização quando reconectar
✅ Instalável — não precisa de navegador
✅ Acesso ao sistema de arquivos (importação de XML)
```
