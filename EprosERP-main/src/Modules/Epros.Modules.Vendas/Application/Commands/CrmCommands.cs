using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Vendas.Application.Commands
{
    // ===================== Setup CRM =====================

    public record CriarCrmPipelineCommand(string Nome, Guid CriadorUsuarioId) : ICommand;
    public record CriarCrmEtapaCommand(Guid PipelineId, ECrmTipoEtapa TipoEtapa, string Nome, int Ordem) : ICommand;
    public record CriarCrmEtiquetaCommand(Guid? PipelineId, string Nome, string? Cor) : ICommand;
    public record CriarCrmOrigemCommand(string Nome) : ICommand;

    // ===================== Lead =====================

    public record CriarCrmLeadCommand(
        Guid? PipelineId,
        Guid? EtapaId,
        string Nome,
        string? PrimeiroNome,
        string? Sobrenome,
        string? Email,
        string? Telefone,
        string? Assunto,
        decimal? ValorEstimado,
        ECrmLeadStatus? Status,
        Guid? OrigemId,
        string? Notas,
        Guid CriadoPorUsuarioId,
        Guid? ResponsavelUsuarioId,
        Guid? CampanhaId,
        string? EnderecoIpCaptacao) : ICommand;

    public record MoverCrmLeadEtapaCommand(Guid LeadId, Guid EtapaId, int? PosicaoKanban) : ICommand;

    public record ConverterCrmLeadCommand(
        Guid LeadId,
        Guid? ClienteId,
        Guid? ContatoId,
        bool CriarOportunidade,
        string? NomeOportunidade,
        decimal? ValorOportunidade) : ICommand;

    public record ArquivarCrmLeadCommand(Guid LeadId) : ICommand;

    // ===================== Oportunidade =====================

    public record CriarCrmOportunidadeCommand(
        Guid PipelineId,
        Guid EtapaId,
        string Nome,
        decimal? Valor,
        Guid? OrigemId,
        Guid? ClientePrincipalId,
        Guid? LeadOrigemId,
        string? Notas,
        DateTime? DataFechamentoPrevista,
        Guid CriadoPorUsuarioId) : ICommand;

    public record MoverCrmOportunidadeEtapaCommand(Guid OportunidadeId, Guid EtapaId, int? PosicaoKanban) : ICommand;
    public record GanharCrmOportunidadeCommand(Guid OportunidadeId) : ICommand;
    public record PerderCrmOportunidadeCommand(Guid OportunidadeId, string? MotivoPerda) : ICommand;

    // ===================== Campanha =====================

    public record CriarCrmCampanhaCommand(
        string Nome,
        string Tipo,
        string Status,
        DateTime? DataInicio,
        DateTime DataFim,
        string? Frequencia,
        Guid? MoedaId,
        decimal? Orcamento,
        decimal? CustoEsperado,
        decimal? CustoReal,
        decimal? ReceitaEsperada,
        string? Objetivo,
        string? Conteudo,
        Guid? ResponsavelUsuarioId) : ICommand;

    // ===================== Ticket =====================

    public record CriarCrmTicketStatusCommand(string Titulo, string? Cor, int Ordem, bool? UsoClienteRespondeu, bool? UsoEquipeRespondeu, bool PadraoSistema) : ICommand;

    public record CriarCrmTicketCommand(
        string Titulo,
        string? Descricao,
        Guid StatusId,
        ECrmPrioridade? Prioridade,
        Guid? CategoriaId,
        Guid? ClienteId,
        Guid? ProjetoId,
        ECrmOrigemAtendimento? Origem,
        string? TipoUsuarioAbertura,
        Guid? UsuarioAberturaId,
        Guid? ContatoAberturaId) : ICommand;

    public record ResponderCrmTicketCommand(
        Guid TicketId,
        string Texto,
        ECrmTipoRespostaTicket Tipo,
        ECrmOrigemAtendimento? Origem,
        string? RemetenteTipo,
        Guid? RemetenteId,
        string? AnexosJson) : ICommand;
}
