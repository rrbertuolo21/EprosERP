using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    /// <summary>
    /// Comandos do submódulo Gestão de Contratos de Compra (EST-GCC). Modelo proposto por autoria (EF §22),
    /// sobe desabilitado (ABAC nega por padrão). Workflow de aprovação e integração fiscal/financeira ficam
    /// registrados em pendências (dependência de kernel/contrato compartilhado).
    /// </summary>
    public record GccContratoItemInput(Guid ProdutoId, decimal PrecoUnitario, decimal QuantidadeComprometida);

    public record CriarGccContratoCompraCommand(
        Guid FornecedorId,
        string? NumeroContrato = null,
        DateTime? VigenciaInicio = null,
        DateTime? VigenciaFim = null,
        decimal? ValorTotal = null,
        string? Observacao = null,
        List<GccContratoItemInput>? Itens = null
    ) : ICommand;

    public record AtualizarGccContratoCompraCommand(
        Guid Id,
        Guid FornecedorId,
        string? NumeroContrato,
        DateTime? VigenciaInicio,
        DateTime? VigenciaFim,
        decimal? ValorTotal,
        string? Observacao
    ) : ICommand;

    public record EnviarGccContratoParaAprovacaoCommand(Guid Id) : ICommand;

    public record AprovarGccContratoCompraCommand(Guid Id) : ICommand;

    /// <summary>Registra consumo de saldo contratual por uma compra (GCC-007/008).</summary>
    public record RegistrarGccConsumoContratoCommand(
        Guid ContratoCompraId,
        Guid ContratoCompraItemId,
        Guid? CompraId,
        decimal QuantidadeConsumida,
        decimal ValorConsumido
    ) : ICommand;

    /// <summary>
    /// CD5 — registra um aditivo contratual em contrato APROVADO. Conforme o tipo aplica: Vigencia
    /// (NovaVigenciaFim), Preco/Quantidade (por item: ContratoCompraItemId + NovoPreco/QuantidadeAdicional,
    /// e opcionalmente NovoValorTotal no cabeçalho), Condicoes (NovaObservacao).
    /// </summary>
    public record RegistrarGccAditivoCommand(
        Guid ContratoCompraId,
        ETipoAditivoContrato TipoAditivo,
        string? NumeroAditivo = null,
        string? Justificativa = null,
        DateTime? DataAditivo = null,
        DateTime? NovaVigenciaFim = null,
        decimal? NovoValorTotal = null,
        string? NovaObservacao = null,
        Guid? ContratoCompraItemId = null,
        decimal? NovoPreco = null,
        decimal? QuantidadeAdicional = null
    ) : ICommand;
}
