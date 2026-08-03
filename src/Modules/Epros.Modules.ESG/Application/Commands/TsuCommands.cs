using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.ESG.Application.Commands
{
    // ===== ESG-TSU (Transporte Sustentavel) =====

    public record CriarRegistroTsuCommand(string Codigo, string Descricao, Guid ResponsavelId) : ICommand;
    public record SubmeterRegistroTsuCommand(Guid RegistroId) : ICommand;
    public record AprovarRegistroTsuCommand(Guid RegistroId) : ICommand;

    public record AdicionarOperacaoTsuCommand(Guid RegistroTsuId, string Referencia, DateTime DataOperacao, string Modal) : ICommand;

    public record AdicionarTrechoTsuCommand(
        Guid OperacaoId, string Modal, decimal Distancia, string UnidadeDistancia,
        decimal Massa, string UnidadeMassa, decimal? EnergiaCombustivel, string? UnidadeEnergia) : ICommand;

    /// <summary>
    /// Calcula CO2e/tkm do trecho. tkm = massa × distancia; CO2e = tkm × fator (catalogo compartilhado
    /// com GHG). Regra #0/NF-07: sem fator vigente o calculo fica pendente (sem numero inventado).
    /// </summary>
    public record CalcularEmissaoTsuCommand(Guid TrechoId, string FormulaVersao) : ICommand;

    public record DefinirMetaModalTsuCommand(
        Guid RegistroTsuId, string Modal, decimal ParticipacaoAlvo, DateTime PeriodoInicio, DateTime PeriodoFim) : ICommand;

    public record RegistrarMedicaoModalTsuCommand(Guid MetaModalId, string Periodo, decimal ParticipacaoRealizada) : ICommand;
}
