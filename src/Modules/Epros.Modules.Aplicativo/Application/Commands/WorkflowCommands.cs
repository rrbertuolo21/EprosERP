using System;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    // ---------- Definição ----------

    public record CriarWfDefinicaoCommand(string Modulo, string Entidade, string Nome, int Versao) : ICommand;

    public class CriarWfDefinicaoCommandValidator : AbstractValidator<CriarWfDefinicaoCommand>
    {
        public CriarWfDefinicaoCommandValidator()
        {
            RuleFor(x => x.Modulo).NotEmpty().WithMessage("O módulo dono é obrigatório.");
            RuleFor(x => x.Entidade).NotEmpty().WithMessage("A entidade controlada é obrigatória.");
            RuleFor(x => x.Nome).NotEmpty().WithMessage("O nome da definição é obrigatório.");
        }
    }

    public record AdicionarWfEstadoCommand(Guid DefinicaoId, string Codigo, string Nome, bool Inicial, bool Final, bool Ativo) : ICommand;

    public class AdicionarWfEstadoCommandValidator : AbstractValidator<AdicionarWfEstadoCommand>
    {
        public AdicionarWfEstadoCommandValidator()
        {
            RuleFor(x => x.DefinicaoId).NotEmpty().WithMessage("A definição é obrigatória.");
            RuleFor(x => x.Codigo).NotEmpty().WithMessage("O código do estado é obrigatório.");
            RuleFor(x => x.Nome).NotEmpty().WithMessage("O nome do estado é obrigatório.");
        }
    }

    public record AdicionarWfTransicaoCommand(
        Guid DefinicaoId,
        Guid EstadoOrigemId,
        Guid EstadoDestinoId,
        EWfEvento Evento,
        EWfPermissao PermissaoRequerida,
        bool ExigeComentario,
        bool PublicaEvento) : ICommand;

    public class AdicionarWfTransicaoCommandValidator : AbstractValidator<AdicionarWfTransicaoCommand>
    {
        public AdicionarWfTransicaoCommandValidator()
        {
            RuleFor(x => x.DefinicaoId).NotEmpty().WithMessage("A definição é obrigatória.");
            RuleFor(x => x.EstadoOrigemId).NotEmpty().WithMessage("O estado de origem é obrigatório.");
            RuleFor(x => x.EstadoDestinoId).NotEmpty().WithMessage("O estado de destino é obrigatório.");
        }
    }

    public record AtivarWfDefinicaoCommand(Guid DefinicaoId) : ICommand;

    // ---------- Instância (contrato de aprovação por alçada) ----------

    /// <summary>
    /// Cria uma instância de workflow para um registro de negócio de qualquer módulo e publica
    /// AprovacaoSolicitada via Outbox. É o ponto de entrada declarativo que substitui a aprovação hardcoded.
    /// </summary>
    public record CriarWfInstanciaCommand(
        Guid DefinicaoId,
        string Modulo,
        string EntidadeTipo,
        string EntidadeId,
        string Descricao,
        decimal? ValorReferencia,
        Guid? ResponsavelUsuarioId,
        string? CommandType,
        string? Payload) : ICommand;

    public class CriarWfInstanciaCommandValidator : AbstractValidator<CriarWfInstanciaCommand>
    {
        public CriarWfInstanciaCommandValidator()
        {
            RuleFor(x => x.DefinicaoId).NotEmpty().WithMessage("A definição é obrigatória.");
            RuleFor(x => x.Modulo).NotEmpty().WithMessage("O módulo é obrigatório.");
            RuleFor(x => x.EntidadeTipo).NotEmpty().WithMessage("O tipo da entidade é obrigatório.");
            RuleFor(x => x.EntidadeId).NotEmpty().WithMessage("O identificador do registro é obrigatório.");
            RuleFor(x => x.Descricao).NotEmpty().WithMessage("A descrição é obrigatória.");
        }
    }

    /// <summary>
    /// Aplica um evento (Submeter/Aprovar/Rejeitar/Inativar/Encerrar/Reativar) à instância. Ao aprovar
    /// ou rejeitar, publica AprovacaoConcluida via Outbox e registra histórico imutável.
    /// </summary>
    public record TransicionarWfInstanciaCommand(
        Guid InstanciaId,
        EWfEvento Evento,
        Guid? EstadoDestinoId,
        string? Comentario) : ICommand;

    public class TransicionarWfInstanciaCommandValidator : AbstractValidator<TransicionarWfInstanciaCommand>
    {
        public TransicionarWfInstanciaCommandValidator()
        {
            RuleFor(x => x.InstanciaId).NotEmpty().WithMessage("A instância é obrigatória.");
        }
    }

    // ---------- Tarefa humana ----------

    public record CriarWfTarefaCommand(Guid InstanciaId, string Titulo, Guid? ResponsavelUsuarioId, EWfPermissao? ResponsavelPapel, DateTime? PrazoEm) : ICommand;

    public class CriarWfTarefaCommandValidator : AbstractValidator<CriarWfTarefaCommand>
    {
        public CriarWfTarefaCommandValidator()
        {
            RuleFor(x => x.InstanciaId).NotEmpty().WithMessage("A instância é obrigatória.");
            RuleFor(x => x.Titulo).NotEmpty().WithMessage("O título da tarefa é obrigatório.");
        }
    }

    public record ConcluirWfTarefaCommand(Guid TarefaId) : ICommand;

    // ---------- Solicitação com aprovação ----------

    public record CriarWfSolicitacaoCommand(
        DateTime? StartDate,
        DateTime? EndDate,
        decimal? TotalDays,
        string? Reason,
        string? Attachment,
        string? EmployeeId,
        string? LeaveTypeId,
        string? CreatorId) : ICommand;

    public record DecidirWfSolicitacaoCommand(Guid SolicitacaoId, bool Aprovar, string? ApproverComment) : ICommand;

    public class DecidirWfSolicitacaoCommandValidator : AbstractValidator<DecidirWfSolicitacaoCommand>
    {
        public DecidirWfSolicitacaoCommandValidator()
        {
            RuleFor(x => x.SolicitacaoId).NotEmpty().WithMessage("A solicitação é obrigatória.");
        }
    }
}
