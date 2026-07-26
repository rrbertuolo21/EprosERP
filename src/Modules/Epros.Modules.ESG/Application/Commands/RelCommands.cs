using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.ESG.Application.Commands
{
    // ===== ESG-REL (Relatorios ESG) =====

    public record CriarFrameworkRelCommand(
        string Codigo,
        string Versao,
        string Descricao,
        DateTime? InicioVigencia,
        DateTime? FimVigencia
    ) : ICommand;

    public record AdicionarRequisitoRelCommand(
        Guid FrameworkId,
        string Codigo,
        string Titulo,
        string TipoResposta,
        bool Obrigatorio,
        int Ordem
    ) : ICommand;

    public record AdicionarItemRelatorioCommand(
        Guid RelatorioId,
        int Sequencia,
        decimal? Quantidade,
        string? Observacao,
        Guid? RequisitoId,
        string TipoConteudo,
        string? Justificativa
    ) : ICommand;

    public record VincularIndicadorReferenciaCommand(
        Guid ItemId,
        string OrigemDominio,
        string OrigemEntidade,
        string OrigemId,
        string CodigoIndicador,
        string? RegraComposicao
    ) : ICommand;

    public record CapturarSnapshotIndicadorCommand(
        Guid IndicadorReferenciaId,
        string OrigemVersao,
        DateTime DataCorte,
        decimal? ValorNumerico,
        string? ValorTexto,
        string? Unidade,
        string? Dimensoes,
        string StatusOrigem
    ) : ICommand;

    public record AbrirPendenciaRelatorioCommand(
        Guid RelatorioId,
        Guid? ItemId,
        string Criticidade,
        Guid ResponsavelId,
        DateTime? Prazo
    ) : ICommand;
}
