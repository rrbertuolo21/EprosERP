using System;
using Epros.Modules.ESG.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.ESG.Application.Commands
{
    // ===== ESG-GHG (Pegada de Carbono) =====

    public record CriarInventarioGeeCommand(
        string Codigo,
        int Versao,
        DateTime PeriodoInicio,
        DateTime PeriodoFim,
        string CriterioFronteira,
        string Metodologia,
        Guid ResponsavelId,
        string? Observacao
    ) : ICommand;

    public record AdicionarFonteEmissaoGeeCommand(
        Guid InventarioId,
        string Codigo,
        string Descricao,
        int Escopo,
        string Categoria,
        Guid UnidadeOrganizacionalId
    ) : ICommand;

    public record RegistrarDadoAtividadeGeeCommand(
        Guid FonteEmissaoId,
        DateTime DataReferencia,
        decimal Quantidade,
        string Unidade,
        EOrigemDadoGhg OrigemDado,
        string? ReferenciaOperacional
    ) : ICommand;

    public record CriarFatorEmissaoGeeCommand(
        string Codigo,
        string Versao,
        string FonteReferencia,
        decimal Valor,
        string UnidadeEntrada,
        string UnidadeSaida,
        DateTime InicioVigencia,
        DateTime? FimVigencia
    ) : ICommand;

    public record CalcularEmissaoGeeCommand(
        Guid DadoAtividadeId,
        Guid FatorEmissaoId,
        string FormulaVersao
    ) : ICommand;

    public record SubmeterInventarioGeeCommand(Guid InventarioId) : ICommand;

    public record AprovarInventarioGeeCommand(Guid InventarioId) : ICommand;

    public record ConsolidarInventarioGeeCommand(Guid InventarioId) : ICommand;
}
