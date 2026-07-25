using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.ESG.Application.Commands
{
    // ===== ESG-EHS (Gestao Ambiental EHS) =====

    public record CriarRegistroEhsCommand(
        string Codigo,
        string Descricao,
        string Tipo,
        Guid ResponsavelId,
        Guid? LocalId
    ) : ICommand;

    public record RegistrarAtividadeOcupacionalCommand(
        Guid? RegistroEhsId,
        Guid? IdFolhaPpp,
        DateTime? DataInicio,
        DateTime? DataFim,
        string Descricao
    ) : ICommand;

    public record RegistrarFatorRiscoCommand(
        Guid? AtividadeId,
        Guid? IdFolhaPpp,
        DateTime? DataInicio,
        DateTime? DataFim,
        string Tipo,
        string FatorRiscoDescricao,
        string Intensidade,
        string TecnicaUtilizada,
        string EpcEficaz,
        string EpiEficaz,
        string? CaEpi,
        string AtendimentoNr061,
        string AtendimentoNr062,
        string AtendimentoNr063,
        string AtendimentoNr064,
        string AtendimentoNr065
    ) : ICommand;

    public record RegistrarLicencaAmbientalCommand(
        Guid RegistroEhsId,
        string Tipo,
        string Numero,
        string Autoridade,
        DateTime DataEmissao,
        DateTime DataValidade,
        Guid ResponsavelId,
        Guid? ArquivoId
    ) : ICommand;

    public record RegistrarResiduoMovimentoCommand(
        Guid RegistroEhsId,
        string TipoResiduo,
        string Classificacao,
        string Origem,
        decimal Quantidade,
        string Unidade,
        DateTime Data,
        Guid? LocalId,
        string TipoMovimento,
        Guid? DestinoId,
        Guid? EvidenciaArquivoId
    ) : ICommand;

    public record RegistrarIncidenteCommand(
        Guid RegistroEhsId,
        string Tipo,
        DateTime DataHora,
        Guid? LocalId,
        string Descricao,
        string Gravidade,
        string? Impacto,
        Guid? PessoaId,
        string? NumeroCat
    ) : ICommand;

    public record RegistrarAcaoCorretivaCommand(
        Guid? IncidenteId,
        Guid? FatorRiscoId,
        string Descricao,
        string? Causa,
        Guid ResponsavelId,
        DateTime? Prazo,
        Guid? EvidenciaArquivoId
    ) : ICommand;
}
