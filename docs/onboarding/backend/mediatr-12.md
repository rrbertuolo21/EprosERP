---
title: "MediatR 12 — CQRS"
confluence_id: "192774159"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/192774159/MediatR+12+CQRS"
last_updated: "2026-07-06"
---

> [!NOTE]
> **Versão fixada:** `12.x`
> **Licença:** Apache 2.0

### O que é

Biblioteca que implementa o padrão Mediator — despacha mensagens (Commands e Queries) para handlers. É a engrenagem principal do CQRS no Epros.

### Por que foi escolhido

* Acaba com os controllers de 3.000 linhas: cada operação vira um handler isolado de dezenas de linhas
* Testável: handler pode ser testado sem HTTP, sem controller, sem DI completo
* Pipeline de Behaviors: permite adicionar logging, validação e auditoria de forma transversal

### O que resolve do legado

O legado mistura HTTP handler com regra de negócio, acesso a banco e chamadas externas no mesmo lugar. MediatR força a separação: controller recebe e despacha; handler executa.

### Anatomia completa de um CQRS handler no Epros

```csharp
// 1. COMMAND — o que o sistema vai fazer (imutável, record)
public record LancarCompraCommand(
    Guid FornecedorId,
    List<ItemCompraDto> Itens,
    string? Observacao
) : IRequest<CommandResult>;

// 2. VALIDATOR — valida o input antes do handler executar
public class LancarCompraValidator : AbstractValidator<LancarCompraCommand>
{
    public LancarCompraValidator()
    {
        RuleFor(x => x.FornecedorId)
            .NotEmpty().WithMessage("Fornecedor é obrigatório");

        RuleFor(x => x.Itens)
            .NotEmpty().WithMessage("Compra deve ter pelo menos 1 item")
            .ForEach(item =>
            {
                item.ChildRules(i =>
                {
                    i.RuleFor(x => x.ProdutoId).NotEmpty();
                    i.RuleFor(x => x.Quantidade).GreaterThan(0);
                    i.RuleFor(x => x.PrecoUnitario).GreaterThan(0);
                });
            });
    }
}

// 3. HANDLER — executa a operação
public class LancarCompraHandler : IRequestHandler<LancarCompraCommand, CommandResult>
{
    private readonly ICompraRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ITenantProvider _tenant;

    public LancarCompraHandler(
        ICompraRepository repo,
        IUnitOfWork uow,
        ITenantProvider tenant)
    {
        _repo = repo;
        _uow = uow;
        _tenant = tenant;
    }

    public async Task<CommandResult> Handle(
        LancarCompraCommand cmd,
        CancellationToken ct)
    {
        // Cria o agregado via factory method (domínio decide as regras)
        var compra = Compra.Criar(
            _tenant.GetTenantId(),
            cmd.FornecedorId,
            cmd.Itens.Select(i => new ItemCompra(i.ProdutoId, i.Quantidade, i.PrecoUnitario))
        );

        // Valida as regras de domínio (Flunt)
        if (!compra.IsValid)
            return CommandResult.Falha(compra.Notifications);

        // Persiste
        await _repo.AdicionarAsync(compra, ct);

        // Domain Event registrado na mesma transação
        // Outbox garante entrega mesmo em falha
        compra.AdicionarEvento(new CompraLancada(
            compra.Id,
            compra.TenantId,
            compra.ValorTotal
        ));

        await _uow.CommitAsync(ct);

        return CommandResult.Ok(compra.Id);
    }
}

// 4. CONTROLLER — fino, sem lógica
[ApiController]
[Route("api/v1/compras")]
[Authorize]
public class ComprasController : ControllerBase
{
    private readonly IMediator _mediator;

    public ComprasController(IMediator mediator)
        => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> LancarCompra(
        [FromBody] LancarCompraCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.Sucesso
            ? CreatedAtAction(nameof(ObterCompra), new { id = result.Dados }, result.Dados)
            : BadRequest(result.Erros);
    }
}
```

### Behaviors — pipeline transversal

```csharp
// ValidationBehavior — roda ANTES de qualquer handler automaticamente
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // Valida todos os validators registrados para este command
        var failures = _validators
            .SelectMany(v => v.Validate(request).Errors)
            .Where(e => e != null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        // Se passou na validação, chama o próximo (o handler)
        return await next();
    }
}

// LoggingBehavior — loga entrada/saída de todo command automaticamente
public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ITenantProvider _tenant;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var commandName = typeof(TRequest).Name;
        var tenantId = _tenant.GetTenantId();

        _logger.LogInformation(
            "Executando {Command} para tenant {TenantId}",
            commandName, tenantId);

        var response = await next();

        _logger.LogInformation(
            "{Command} concluído para tenant {TenantId}",
            commandName, tenantId);

        return response;
    }
}
```

> [!WARNING]
> **Armadilhas:**
> * **Command vs Query:** command muda estado (POST/PUT/DELETE); query só lê (GET). Nunca misturar — uma Query que modifica dados viola CQRS e dificulta cache/replica
> * **Handler não chama handler:** se precisar, repensar a fronteira do módulo ou usar Domain Event
> * **Regra de negócio no handler não no controller:** o controller nunca tem `if` de negócio

### Onde aprender

* Repositório oficial: [https://github.com/jbogard/MediatR](https://github.com/jbogard/MediatR)
* Padrão CQRS: [https://martinfowler.com/bliki/CQRS.html](https://martinfowler.com/bliki/CQRS.html)
