using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Plataforma.Iot
{
    // ===================== Commands =====================

    public record RegistrarDispositivoIotCommand(
        string Codigo, string Nome, string? Tipo, string? Protocolo,
        string? AtivoVinculadoTipo, string? AtivoVinculadoId) : ICommand;

    public class RegistrarDispositivoIotCommandValidator : AbstractValidator<RegistrarDispositivoIotCommand>
    {
        public RegistrarDispositivoIotCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty();
            RuleFor(c => c.Nome).NotEmpty();
        }
    }

    public record RegistrarSensorIotCommand(
        Guid DispositivoId, string Codigo, string Grandeza, string Unidade,
        decimal? LimiteMin, decimal? LimiteMax, int RetencaoDias) : ICommand;

    public record VincularDispositivoAtivoCommand(Guid DispositivoId, string AtivoTipo, string AtivoId) : ICommand;

    /// <summary>Ingesta uma leitura de sensor. Fora da faixa emite condição p/ Manutenção preditiva.</summary>
    public record IngestarLeituraCommand(Guid SensorId, decimal Valor, DateTime? MedidoEm) : ICommand;

    // ===================== Queries =====================

    public record ObterDispositivosIotQuery(bool ApenasAtivos = false) : IQuery<IReadOnlyList<DispositivoIotDto>>;

    public record ObterSensoresQuery(Guid DispositivoId) : IQuery<IReadOnlyList<SensorIotDto>>;

    public record ObterLeiturasQuery(Guid SensorId, DateTime? Desde = null, bool ApenasForaFaixa = false)
        : IQuery<IReadOnlyList<LeituraSensorDto>>;

    public record ObterLeiturasVencidasQuery() : IQuery<IReadOnlyList<LeituraVencidaDto>>;

    // ===================== DTOs =====================

    public record DispositivoIotDto(Guid Id, string Codigo, string Nome, string? Tipo, string? Protocolo,
        string? AtivoVinculadoTipo, string? AtivoVinculadoId, bool Ativo, DateTime? UltimaLeituraEm);

    public record SensorIotDto(Guid Id, Guid DispositivoId, string Codigo, string Grandeza, string Unidade,
        decimal? LimiteMin, decimal? LimiteMax, int RetencaoDias);

    public record LeituraSensorDto(Guid Id, Guid SensorId, decimal Valor, DateTime MedidoEm, bool ForaFaixa);

    public record LeituraVencidaDto(Guid Id, Guid SensorId, DateTime MedidoEm, int RetencaoDias, DateTime VenceEm);
}
