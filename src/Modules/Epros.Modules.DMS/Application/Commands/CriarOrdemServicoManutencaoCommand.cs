using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.DMS.Application.Commands
{
    public record CriarOrdemServicoManutencaoCommand(
        Guid PessoaId,
        Guid? ProdutoId,
        Guid VeiculoId,
        string ChassiVin,
        string Placa,
        decimal QuilometragemEntrada,
        Guid ConsultorId,
        Guid UnidadeId,
        DateTime DataAbertura,
        DateTime? PrevisaoEntrega
    ) : ICommand;

    public class CriarOrdemServicoManutencaoCommandValidator : AbstractValidator<CriarOrdemServicoManutencaoCommand>
    {
        public CriarOrdemServicoManutencaoCommandValidator()
        {
            RuleFor(c => c.PessoaId).NotEmpty().WithMessage("A pessoa/cliente é obrigatória na ordem de serviço.");
            RuleFor(c => c.VeiculoId).NotEmpty().WithMessage("O veículo é obrigatório na ordem de serviço.");
            RuleFor(c => c.ChassiVin).NotEmpty().Length(17).WithMessage("O número de chassi (VIN) do veículo deve possuir exatamente 17 caracteres.");
            RuleFor(c => c.Placa).NotEmpty().WithMessage("A placa do veículo é obrigatória.");
            RuleFor(c => c.QuilometragemEntrada).GreaterThanOrEqualTo(0).WithMessage("A quilometragem de entrada não pode ser negativa.");
            RuleFor(c => c.ConsultorId).NotEmpty().WithMessage("O consultor é obrigatório na ordem de serviço.");
            RuleFor(c => c.UnidadeId).NotEmpty().WithMessage("A unidade é obrigatória na ordem de serviço.");
        }
    }
}
