using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.ESG.Application.Commands
{
    // ===== ESG-DIV (Diversidade e Responsabilidade Social) =====

    public record CriarProgramaDivCommand(
        string Codigo, string Descricao, Guid ResponsavelId, string? Observacao) : ICommand;

    public record SubmeterProgramaDivCommand(Guid ProgramaId) : ICommand;
    public record AprovarProgramaDivCommand(Guid ProgramaId) : ICommand;

    public record CriarIndicadorDivCommand(
        Guid? ProgramaId, string Codigo, int Versao, string Descricao, string Unidade, string? Fonte, string? Formula) : ICommand;

    public record DefinirParametroDivCommand(string Chave, string ValorJson) : ICommand;

    /// <summary>
    /// Registra medicao SEMPRE agregada. O limiar de supressao (NF-09/T1) e lido do parametro
    /// <see cref="Epros.Modules.ESG.Domain.Entities.DivParametro.ChaveLimiteSupressaoGrupo"/>;
    /// grupos menores que o limiar sao suprimidos (valor nao exposto).
    /// </summary>
    public record RegistrarMedicaoDivCommand(
        Guid IndicadorId, DateTime PeriodoInicio, DateTime PeriodoFim, string Dimensao,
        int QtdIndividuos, decimal ValorAgregado, string Origem) : ICommand;

    public record DefinirMetaDivCommand(
        Guid IndicadorId, DateTime PeriodoInicio, DateTime PeriodoFim, decimal ValorEsperado, string Unidade) : ICommand;

    public record CriarAcaoDivCommand(
        Guid ProgramaId, string Descricao, Guid ResponsavelId, DateTime? Prazo) : ICommand;
}
