using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Plataforma.Analytics
{
    // ===================== Commands =====================

    /// <summary>Define/versiona um KPI. Se o código já existir ativo, cria nova versão e desativa a anterior.</summary>
    public record DefinirKpiCommand(
        string Codigo, string Nome, string? Descricao, string Unidade, string? Categoria,
        string? FonteModulo, string? ChaveConsulta) : ICommand;

    public class DefinirKpiCommandValidator : AbstractValidator<DefinirKpiCommand>
    {
        public DefinirKpiCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty();
            RuleFor(c => c.Nome).NotEmpty();
            RuleFor(c => c.Unidade).NotEmpty();
        }
    }

    /// <summary>Ingere/atualiza o valor de um KPI numa competência/dimensão (valor vem da fonte/dono).</summary>
    public record RegistrarSnapshotMetricaCommand(
        string KpiCodigo, string Competencia, decimal Valor, string? Dimensao, string? Origem) : ICommand;

    public record CriarDashboardCommand(
        string Codigo, string Nome, string? Descricao, string? LayoutJson, bool Publico) : ICommand;

    public record AdicionarWidgetDashboardCommand(
        Guid DashboardId, string KpiCodigo, string Titulo, string TipoVisualizacao, int Ordem, string? ConfigJson) : ICommand;

    public record SolicitarExportacaoAnalyticsCommand(string Recurso, string Formato, string? FiltroJson) : ICommand;

    // ===================== Queries =====================

    public record ObterKpisQuery(string? Categoria = null, bool ApenasAtivos = true) : IQuery<IReadOnlyList<KpiDto>>;

    public record ObterDashboardsQuery() : IQuery<IReadOnlyList<DashboardDto>>;

    public record ObterDashboardPorIdQuery(Guid Id) : IQuery<DashboardDetalheDto?>;

    /// <summary>Série temporal (read-side) de um KPI: snapshots ordenados por competência.</summary>
    public record ObterSerieMetricaQuery(string KpiCodigo, string? Dimensao = null) : IQuery<IReadOnlyList<PontoSerieDto>>;

    public record ObterExportacoesAnalyticsQuery() : IQuery<IReadOnlyList<ExportacaoAnalyticsDto>>;

    // ===================== DTOs =====================

    public record KpiDto(Guid Id, string Codigo, string Nome, string? Descricao, string Unidade,
        string? Categoria, string? FonteModulo, int Versao, bool Ativo);

    public record DashboardDto(Guid Id, string Codigo, string Nome, string? Descricao, bool Publico, DateTime CriadoEm);

    public record WidgetDto(Guid Id, string KpiCodigo, string Titulo, string TipoVisualizacao, int Ordem, string? ConfigJson);

    public record DashboardDetalheDto(DashboardDto Dashboard, IReadOnlyList<WidgetDto> Widgets);

    public record PontoSerieDto(string Competencia, decimal Valor, string? Dimensao, DateTime CapturadoEm);

    public record ExportacaoAnalyticsDto(Guid Id, string Recurso, string Formato, string Status, string? UrlRef, DateTime CriadoEm);
}
