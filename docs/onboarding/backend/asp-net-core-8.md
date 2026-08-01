---
title: "ASP.NET Core 8 — o runtime"
confluence_id: "192315417"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/192315417/ASP.NET+Core+8+o+runtime"
last_updated: "2026-07-06"
---

> [!NOTE]
> **Versão fixada:** `8.0 LTS` (suporte até Novembro 2026)
> **Licença:** MIT

### O que é

Framework web da Microsoft para APIs e aplicações server-side em C#. É o runtime de toda a camada de backend do Epros.

### Por que foi escolhido

* Performance #1 no benchmark TechEmpower (acima de Go, Node, Spring em muitos cenários)
* O time já conhece C# — zero curva de aprendizado de linguagem
* LTS com suporte garantido — não há surpresa de mudança de licença
* Roda em Alpine Linux em container de 80MB

### O que resolve do legado

O legado já usa .NET 8 — a mudança não é de runtime, é de **como o código é organizado dentro dele**. Controllers de 4.800 linhas continuam sendo ASP.NET Core; handlers de 80 linhas também. A diferença é arquitetura.

### Exemplo no Epros

```csharp
// Program.cs — configuração da aplicação
var builder = WebApplication.CreateBuilder(args);

// Pipeline de middlewares (ordem obrigatória)
var app = builder.Build();

app.UseAuthentication();          // 1. valida JWT
app.UseMiddleware<ExcecaoGlobalMiddleware>(); // 2. captura erros → ProblemDetails
app.UseMiddleware<TenantSaaSMiddleware>(); // 3. resolve tenant do claim
app.UseMiddleware<ModuloTenantMiddleware>();  // 4. verifica entitlement
app.UseMiddleware<DataMaskingMiddleware>();   // 5. mascara PII nos logs
app.UseMiddleware<AuditMiddleware>();         // 6. registra ações sensíveis
app.UseAuthorization();           // 7. verifica roles/políticas
app.MapControllers();             // 8. roteamento para controllers

app.Run();
```

### Onde aprender

* Documentação oficial: [https://learn.microsoft.com/aspnet/core](https://learn.microsoft.com/aspnet/core)
* Tutorial "Tour of ASP.NET Core" na doc oficial (início obrigatório)
