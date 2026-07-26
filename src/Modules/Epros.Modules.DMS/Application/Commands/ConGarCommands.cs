using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.DMS.Application.Commands
{
    public record CriarPlanoGarantiaCommand(
        string Codigo,
        string? Nome,
        string? Descricao,
        int Duracao,
        string DuracaoTipo
    ) : ICommand;

    public class CriarPlanoGarantiaCommandValidator : AbstractValidator<CriarPlanoGarantiaCommand>
    {
        public CriarPlanoGarantiaCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O código do plano é obrigatório.");
            RuleFor(c => c.Duracao).GreaterThan(0).WithMessage("A duração do plano deve ser maior que zero.");
            RuleFor(c => c.DuracaoTipo).NotEmpty().Must(t => t == "Dias" || t == "Meses" || t == "Anos")
                .WithMessage("O tipo de duração deve ser 'Dias', 'Meses' ou 'Anos'.");
        }
    }

    public record CriarVeiculoGarantiaCommand(
        Guid VeiculoId,
        Guid VendaId,
        string ChassiVin,
        Guid PlanoVersaoId,
        DateTime DataEntrega,
        DateTime InicioVigencia,
        DateTime FimVigencia,
        decimal? QuilometragemInicio,
        decimal? QuilometragemLimite
    ) : ICommand;

    public class CriarVeiculoGarantiaCommandValidator : AbstractValidator<CriarVeiculoGarantiaCommand>
    {
        public CriarVeiculoGarantiaCommandValidator()
        {
            RuleFor(c => c.VeiculoId).NotEmpty().WithMessage("O veículo é obrigatório.");
            RuleFor(c => c.VendaId).NotEmpty().WithMessage("A venda é obrigatória.");
            RuleFor(c => c.ChassiVin).NotEmpty().Length(17).WithMessage("O chassi/VIN deve possuir exatamente 17 caracteres.");
            RuleFor(c => c.PlanoVersaoId).NotEmpty().WithMessage("A versão do plano é obrigatória.");
            RuleFor(c => c.FimVigencia).GreaterThan(c => c.InicioVigencia).WithMessage("A data de fim da vigência deve ser posterior ao início.");
        }
    }

    public record CriarSolicitacaoGarantiaCommand(
        Guid VeiculoGarantiaId,
        string Protocolo,
        DateTime DataOcorrencia,
        decimal Quilometragem,
        string Sintoma,
        string RelatoCliente,
        Guid? OrdemServicoId
    ) : ICommand;

    public class CriarSolicitacaoGarantiaCommandValidator : AbstractValidator<CriarSolicitacaoGarantiaCommand>
    {
        public CriarSolicitacaoGarantiaCommandValidator()
        {
            RuleFor(c => c.VeiculoGarantiaId).NotEmpty().WithMessage("A garantia do veículo é obrigatória.");
            RuleFor(c => c.Protocolo).NotEmpty().WithMessage("O protocolo é obrigatório.");
            RuleFor(c => c.Quilometragem).GreaterThanOrEqualTo(0).WithMessage("A quilometragem não pode ser negativa.");
            RuleFor(c => c.Sintoma).NotEmpty().WithMessage("O sintoma é obrigatório.");
            RuleFor(c => c.RelatoCliente).NotEmpty().WithMessage("O relato do cliente é obrigatório.");
        }
    }
}
