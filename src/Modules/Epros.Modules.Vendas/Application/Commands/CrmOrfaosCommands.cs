using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Vendas.Application.Commands
{
    // Write-path das entidades CRM antes órfãs (auditoria VENDAS, submódulo 3 CRM, P1):
    // Atividades/Agenda, Webforms, Fidelização e Pix relacional tinham entidade+DbSet mas nenhum
    // command/handler/endpoint. Estes contratos fecham o gap ligando o CRUD/ações reais.

    // ===================== Atividades / Agenda (CRM-029..037) =====================

    public record CriarCrmAtividadeCommand(
        ECrmEntidadeContexto EntidadeTipo,
        ECrmTipoAtividade TipoAtividade,
        Guid? LeadId,
        Guid? OportunidadeId,
        Guid? TicketId,
        Guid? CampanhaId,
        string? Nome,
        string? Assunto,
        DateTime? Data,
        string? Hora,
        ECrmPrioridade? Prioridade,
        ECrmStatusAtividade? Status,
        ECrmTipoChamada? TipoChamada,
        string? Duracao,
        string? Destinatario,
        string? Descricao,
        string? Comentario,
        string? ArquivoNome,
        string? ArquivoReferencia,
        string? RecorrenciaJson,
        Guid? UsuarioId) : ICommand;

    public record ConcluirCrmAtividadeCommand(Guid Id, string? Resultado) : ICommand;

    // ===================== Webforms (CRM-018..024) =====================

    public record CriarCrmWebformCommand(
        string IdentificadorUnico,
        string Titulo,
        string EstruturaJson,
        string? CssCustomizado,
        string? MensagemAgradecimento,
        string? TextoBotao,
        Guid? CampanhaId,
        string? LeadTituloPadrao,
        string? LeadStatusPadrao,
        Guid? LeadOrigemId,
        bool UsarCaptcha,
        bool NotificarAdministrador) : ICommand;

    /// <summary>CRM-021: registra uma submissão aceita do webform (incrementa o contador anti-spam).</summary>
    public record RegistrarSubmissaoCrmWebformCommand(string IdentificadorUnico) : ICommand;

    // ===================== Fidelização (CRM-066/067) =====================

    public record CriarCrmClienteFidelizadoCommand(
        Guid? ClienteId,
        string Nome,
        string? Email,
        string? Celular,
        DateTime? DataAniversario) : ICommand;

    /// <summary>
    /// CRM-067: lança pontuação para um cliente fidelizado e acumula no total.
    /// A REGRA de conversão (valor da compra → pontos) é decisão de negócio do cliente e NÃO é derivada
    /// aqui: o comando recebe os pontos já calculados. // valida-Siser
    /// </summary>
    public record RegistrarPontuacaoFidelizacaoCommand(
        Guid ClienteFidelizadoId,
        decimal Pontos,
        DateTime? Data,
        string? Observacao) : ICommand;

    // ===================== Pix relacional (CRM-069..071) =====================

    /// <summary>Salva (upsert por tenant) a configuração de Pix relacional. O TokenPix é segredo (cofre).</summary>
    public record SalvarCrmConfiguracaoPixRelacionalCommand(
        string? LinkApiAppVendas,
        string? TokenPix,
        string? CnpjEmpresa,
        bool Ativo) : ICommand;

    public record CriarCrmPagamentoPixRelacionalCommand(
        string? EntidadeOrigemTipo,
        Guid? EntidadeOrigemId,
        decimal Valor,
        string? QrCode,
        string? QrCodeImagem,
        string? ProvedorPagamentoId) : ICommand;

    /// <summary>CRM-070: atualiza o status do pagamento Pix (consulta/webhook do provedor).</summary>
    public record AtualizarStatusCrmPagamentoPixCommand(
        Guid Id,
        ECrmStatusPagamentoPix Status,
        string? ProvedorPagamentoId) : ICommand;
}
