using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    /// <summary>
    /// Comandos da alçada de aprovação de compras (CD3 / EF SOURCING §5.8, §6.2). Cobrem a manutenção
    /// das regras de alçada e o ciclo de aprovação multi-nível (solicitar → aprovar/reprovar por nível).
    /// Submódulo novo (não há código legado de alçada). ABAC nega por padrão.
    /// </summary>
    public record CriarComprasAlcadaRegraCommand(
        int Nivel,
        decimal ValorMinimo,
        decimal? ValorMaximo = null,
        Guid? CompradorId = null,
        string? CategoriaCompra = null,
        Guid? AprovadorId = null,
        string? PapelAprovador = null,
        bool Ativo = true
    ) : ICommand;

    public record AtualizarComprasAlcadaRegraCommand(
        Guid Id,
        int Nivel,
        decimal ValorMinimo,
        decimal? ValorMaximo,
        Guid? CompradorId,
        string? CategoriaCompra,
        Guid? AprovadorId,
        string? PapelAprovador,
        bool Ativo
    ) : ICommand;

    public record ExcluirComprasAlcadaRegraCommand(Guid Id) : ICommand;

    /// <summary>Abre uma instância de aprovação por alçada para uma origem (SRC-008 / §6.2).</summary>
    public record SolicitarAprovacaoCompraCommand(
        EOrigemAprovacaoCompra OrigemTipo,
        Guid OrigemId,
        decimal ValorTotal,
        Guid? CompradorId = null,
        string? CategoriaCompra = null
    ) : ICommand;

    public record AprovarNivelAprovacaoCompraCommand(Guid PedidoAprovacaoId, string? Justificativa = null) : ICommand;

    public record ReprovarNivelAprovacaoCompraCommand(Guid PedidoAprovacaoId, string? Justificativa = null) : ICommand;

    public record CancelarAprovacaoCompraCommand(Guid PedidoAprovacaoId) : ICommand;
}
