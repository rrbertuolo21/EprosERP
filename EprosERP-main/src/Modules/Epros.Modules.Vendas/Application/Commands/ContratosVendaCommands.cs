using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Vendas.Application.Commands
{
    // ===================== Gestão de Contratos de Venda (VEN-GCV) =====================
    // Fonte: EF_7_VENDAS_GESTAO_DE_CONTRATOS_DE_VENDA_V1. Estilo B (ICommand).

    public record CriarContratoTipoCommand(string Nome, bool Ativo) : ICommand;

    public record CriarContratoCommand(
        string Assunto,
        string? NumeroContrato,
        EContratoTipoOrigem? TipoOrigem,
        string? NumeroModelo,
        Guid ClienteId,
        Guid UsuarioResponsavelId,
        Guid TipoId,
        decimal Valor,
        DateTime DataInicio,
        DateTime DataFim,
        string? Descricao,
        string? CorpoDocumento,
        Guid? ProjetoId,
        Guid? LeadId,
        Guid? PropostaId,
        Guid? PedidoId,
        Guid? CategoriaId,
        bool AutomacaoHabilitada,
        string? AutomacaoConfigJson,
        Guid CriadoPorUsuarioId,
        Guid OwnerUsuarioId) : ICommand;

    public record PublicarContratoCommand(Guid ContratoId, DateTime? PublicacaoAgendadaEm, bool Enviar) : ICommand;

    public record AssinarContratoCommand(
        Guid ContratoId,
        Guid? UsuarioId,
        EContratoParteAssinatura Parte,
        EContratoTipoAssinatura TipoAssinatura,
        string DadosAssinatura) : ICommand;

    public record CriarContratoRenovacaoCommand(
        Guid ContratoId,
        DateTime DataInicio,
        DateTime DataFim,
        decimal? Valor,
        string? Notas,
        EContratoRenovacaoStatus Status,
        Guid CriadoPorUsuarioId) : ICommand;

    public record AdicionarContratoComentarioCommand(Guid ContratoId, string Comentario, Guid UsuarioId) : ICommand;

    public record CriarContratoModeloCommand(string Titulo, string Corpo, string? CorCabecalho, bool Sistema) : ICommand;
}
