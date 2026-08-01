---
title: "Flunt + FluentValidation — validação em duas camadas"
confluence_id: "192774169"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/192774169/Flunt+FluentValidation+valida+o+em+duas+camadas"
last_updated: "2026-07-06"
---

> [!NOTE]
> **Versão fixada:** Flunt `2.x` · FluentValidation `11.x`

### Por que duas bibliotecas?

```
FluentValidation → valida o INPUT (o Command que chegou via HTTP)
                   "o campo está preenchido? o formato é válido?"
                   Retorna 422 Unprocessable Entity antes do handler executar

Flunt           → valida a REGRA DE NEGÓCIO (dentro da entidade de domínio)
                   "este valor faz sentido para este negócio?"
                   A entidade acumula notificações e o handler decide o que fazer
```

### Exemplo completo

```csharp
// FluentValidation: valida o command (input HTTP)
public class CriarContaPagarValidator : AbstractValidator<CriarContaPagarCommand>
{
    public CriarContaPagarValidator()
    {
        RuleFor(x => x.Valor)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero")
            .LessThanOrEqualTo(9_999_999.99m).WithMessage("Valor excede limite");

        RuleFor(x => x.DataVencimento)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Vencimento não pode ser no passado");

        RuleFor(x => x.FornecedorId)
            .NotEmpty().WithMessage("Fornecedor é obrigatório");

        RuleFor(x => x.Descricao)
            .MaximumLength(500).WithMessage("Descrição máximo 500 caracteres");
    }
}

// Flunt: valida regra de negócio dentro da entidade
public class ContaPagar : EntidadeSaaSBase
{
    public decimal Valor { get; private set; }
    public DateOnly DataVencimento { get; private set; }
    public ContaPagarStatus Status { get; private set; }
    public DateTime? DataBaixa { get; private set; }

    public static ContaPagar Criar(
        string tenantId,
        string criadoPor,
        Guid fornecedorId,
        decimal valor,
        DateOnly vencimento)
    {
        var cp = new ContaPagar(tenantId, criadoPor)
        {
            FornecedorId = fornecedorId,
            Valor = valor,
            DataVencimento = vencimento,
            Status = ContaPagarStatus.Aberta
        };

        // Regras de negócio com Flunt
        cp.AddNotifications(
            new Flunt.Validations.Contract()
                .Requires()
                .IsGreaterThan(valor, 0, "ContaPagar.Valor", "Valor deve ser positivo")
                .IsNotEmpty(fornecedorId.ToString(), "ContaPagar.FornecedorId", "Fornecedor inválido")
        );

        return cp; // IsValid = !Notifications.Any()
    }

    public void Baixar(decimal valorPago, string usuarioId)
    {
        // Regra: só pode baixar se estiver aberta
        AddNotification("ContaPagar.Status",
            Status != ContaPagarStatus.Aberta
                ? "Conta já foi baixada ou cancelada"
                : string.Empty);

        // Regra: valor pago deve ser maior que zero
        AddNotification("ContaPagar.ValorPago",
            valorPago <= 0 ? "Valor pago deve ser positivo" : string.Empty);

        if (!IsValid) return; // sai sem modificar estado se há erros

        Status = ContaPagarStatus.Baixada;
        DataBaixa = DateTime.UtcNow;
        MarcarAlterado(usuarioId);
        AdicionarEvento(new ContaPagarBaixada(Id, TenantId, valorPago));
    }
}
```
