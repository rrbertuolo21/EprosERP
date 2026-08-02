using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    /// <summary>Linha de contagem informada na criação do inventário. Quando QuantidadeSistema é nula, o saldo é fotografado do kardex.</summary>
    public record InventarioItemInput(
        Guid ProdutoId,
        Guid? LocalId = null,
        string? Lote = null,
        decimal? QuantidadeSistema = null
    );

    /// <summary>INV-001/002: cria o documento de inventário (cabeçalho + N itens).</summary>
    public record CriarInventarioCommand(
        Guid EmpresaId,
        DateTime? DataContagem,
        ETipoInventario TipoInventario,
        List<InventarioItemInput> Itens,
        string? Observacao = null
    ) : ICommand;

    /// <summary>Coloca o inventário em contagem (Rascunho → EmContagem).</summary>
    public record IniciarContagemInventarioCommand(Guid Id) : ICommand;

    /// <summary>INV-006/007: registra até três contagens e a divergência de um item.</summary>
    public record RegistrarContagemInventarioItemCommand(
        Guid InventarioId,
        Guid ItemId,
        decimal? Contagem01 = null,
        decimal? Contagem02 = null,
        decimal? Contagem03 = null,
        decimal? QuantidadeContada = null,
        bool Fechar = false
    ) : ICommand;

    /// <summary>EmContagem → EmConferencia.</summary>
    public record EnviarConferenciaInventarioCommand(Guid Id) : ICommand;

    /// <summary>INV-008/D14: aprova e calcula a acurácia agregada (itens sem divergência / total contados).</summary>
    public record AprovarInventarioCommand(Guid Id) : ICommand;

    /// <summary>INV-014/D3: gera o ajuste por diferença via motor único para cada item divergente.</summary>
    public record AplicarAjusteInventarioCommand(Guid Id) : ICommand;

    /// <summary>Cancela o inventário (apenas nos estados iniciais).</summary>
    public record CancelarInventarioCommand(Guid Id, string Motivo) : ICommand;

    // ============ REAJUSTE (INV-009..013) ============

    public record InventarioReajusteItemInput(Guid ProdutoId, decimal? ValorOriginal = null, decimal? ValorReajuste = null);

    public record CriarReajusteEstoqueCommand(
        Guid ColaboradorId,
        DateTime? DataReajuste,
        decimal? Porcentagem,
        string TipoReajuste,
        List<InventarioReajusteItemInput> Itens
    ) : ICommand;
}
