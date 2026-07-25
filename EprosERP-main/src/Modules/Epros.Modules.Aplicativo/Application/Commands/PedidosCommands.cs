using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    public record CriarPedidoSaaSCommand(
        Guid PlanoId,
        string? CodigoCupom,
        string MetodoPagamento
    ) : ICommand;

    public class CriarPedidoSaaSCommandValidator : AbstractValidator<CriarPedidoSaaSCommand>
    {
        public CriarPedidoSaaSCommandValidator()
        {
            RuleFor(c => c.PlanoId)
                .NotEmpty().WithMessage("O ID do plano é obrigatório.");

            RuleFor(c => c.MetodoPagamento)
                .NotEmpty().WithMessage("O método de pagamento é obrigatório.")
                .Must(m => m == "PIX" || m == "Manual" || m == "Gateway" || m == "Transferencia")
                .WithMessage("Métodos de pagamento aceitos: 'PIX', 'Manual', 'Gateway' ou 'Transferencia'.");
        }
    }

    public record IniciarCheckoutCommand(
        Guid PedidoId
    ) : ICommand;

    public class IniciarCheckoutCommandValidator : AbstractValidator<IniciarCheckoutCommand>
    {
        public IniciarCheckoutCommandValidator()
        {
            RuleFor(c => c.PedidoId)
                .NotEmpty().WithMessage("O ID do pedido é obrigatório.");
        }
    }

    public record RegistrarTransferenciaCommand(
        Guid? PedidoId,
        Guid? FaturaId,
        decimal Valor,
        string NomeArquivo,
        string CaminhoArquivo,
        long TamanhoBytes,
        DateTime DataComprovante
    ) : ICommand;

    public class RegistrarTransferenciaCommandValidator : AbstractValidator<RegistrarTransferenciaCommand>
    {
        public RegistrarTransferenciaCommandValidator()
        {
            RuleFor(c => c.Valor)
                .GreaterThan(0).WithMessage("O valor do comprovante deve ser maior que zero.");

            RuleFor(c => c.NomeArquivo)
                .NotEmpty().WithMessage("O nome do arquivo é obrigatório.");

            RuleFor(c => c.CaminhoArquivo)
                .NotEmpty().WithMessage("O caminho do arquivo é obrigatório.");

            RuleFor(c => c.TamanhoBytes)
                .GreaterThan(0).WithMessage("O tamanho do arquivo deve ser maior que zero.");

            RuleFor(c => c.DataComprovante)
                .NotEmpty().WithMessage("A data do comprovante é obrigatória.");

            RuleFor(c => c)
                .Must(c => c.PedidoId.HasValue || c.FaturaId.HasValue)
                .WithMessage("FaturaId ou PedidoId deve ser informado.");
        }
    }

    public record AnalisarTransferenciaCommand(
        Guid PagamentoTransferenciaId,
        bool Aprovado,
        string? Justificativa
    ) : ICommand;

    public class AnalisarTransferenciaCommandValidator : AbstractValidator<AnalisarTransferenciaCommand>
    {
        public AnalisarTransferenciaCommandValidator()
        {
            RuleFor(c => c.PagamentoTransferenciaId)
                .NotEmpty().WithMessage("O ID do pagamento da transferência é obrigatório.");

            RuleFor(c => c.Justificativa)
                .NotEmpty().When(c => !c.Aprovado)
                .WithMessage("A justificativa é obrigatória na rejeição do comprovante.");
        }
    }

    public record ProcessarWebhookPagamentoCommand(
        string TransactionId,
        string Status,
        string Gateway,
        decimal Valor,
        Guid? AssinaturaId,
        Guid? PedidoId,
        Guid? FaturaId
    ) : ICommand;

    public class ProcessarWebhookPagamentoCommandValidator : AbstractValidator<ProcessarWebhookPagamentoCommand>
    {
        public ProcessarWebhookPagamentoCommandValidator()
        {
            RuleFor(c => c.TransactionId)
                .NotEmpty().WithMessage("O ID da transação é obrigatório.");

            RuleFor(c => c.Status)
                .NotEmpty().WithMessage("O status do pagamento é obrigatório.");

            RuleFor(c => c.Gateway)
                .NotEmpty().WithMessage("O gateway de pagamento é obrigatório.");

            RuleFor(c => c.Valor)
                .GreaterThan(0).WithMessage("O valor do pagamento deve ser maior que zero.");
        }
    }
}
