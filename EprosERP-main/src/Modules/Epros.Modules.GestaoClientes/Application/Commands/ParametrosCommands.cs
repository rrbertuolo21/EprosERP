using System;
using Epros.Shared.Application.Contracts;
using Epros.Modules.GestaoClientes.Domain.Entities;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    // Preferências Gerais
    public record AtualizarPreferenciasCommand(
        bool ShowCurrency,
        bool NegativeCash,
        bool NegativeStock,
        StockCalculationMode StockCalculationMode,
        bool CreditLimit,
        bool Discount,
        bool VatOnPurchase,
        bool VatOnSales,
        string? Justificativa = null
    ) : ICommand;

    // Configuração de E-mail
    public record AtualizarConfiguracaoEmailCommand(
        string? Host,
        int? Port,
        string? Username,
        string? Password,
        string? FromEmail
    ) : ICommand;

    // Categorias
    public record CriarCategoriaCommand(string Nome, string? Image) : ICommand;
    public record AtualizarCategoriaCommand(Guid Id, string Nome, string? Image) : ICommand;
    public record ExcluirCategoriaCommand(Guid Id) : ICommand;

    // Unidades de Medida
    public record CriarUnidadeMedidaCommand(string Nome, string? CodigoUNECE) : ICommand;
    public record AtualizarUnidadeMedidaCommand(Guid Id, string Nome, string? CodigoUNECE) : ICommand;
    public record ExcluirUnidadeMedidaCommand(Guid Id) : ICommand;

    // Armazéns
    public record CriarArmazemCommand(string Nome, string? Pais, string? Cidade, string? Mobile, string? Email) : ICommand;
    public record AtualizarArmazemCommand(Guid Id, string Nome, string? Pais, string? Cidade, string? Mobile, string? Email) : ICommand;
    public record ExcluirArmazemCommand(Guid Id) : ICommand;

    // Projetos
    public record CriarProjetoCommand(string Nome) : ICommand;
    public record AtualizarProjetoCommand(Guid Id, string Nome) : ICommand;
    public record ExcluirProjetoCommand(Guid Id) : ICommand;

    // Impostos
    public record CriarImpostoCommand(string Nome, decimal Rate, bool IsActive, DateTime? VigenciaInicio, DateTime? VigenciaFim) : ICommand;
    public record AtualizarImpostoCommand(Guid Id, string Nome, decimal Rate, bool IsActive, DateTime? VigenciaInicio, DateTime? VigenciaFim) : ICommand;
    public record ExcluirImpostoCommand(Guid Id) : ICommand;

    // Conversão de Unidades
    public record AdicionarConversaoUnidadeCommand(Guid UnidadeOrigemId, Guid UnidadeDestinoId, decimal Fator) : ICommand;
    public record ExcluirConversaoUnidadeCommand(Guid Id) : ICommand;
}
