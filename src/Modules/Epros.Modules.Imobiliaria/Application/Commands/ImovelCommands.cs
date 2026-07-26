using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Imobiliaria.Application.Commands
{
    public record ProprietarioInput(Guid PessoaId);
    public record VistoriaInput(string Local, string Descricao, DateTime? DataVistoria);
    public record CustoImovelInput(string Descricao, decimal Valor, DateTime? Competencia);

    /// <summary>
    /// Cadastra o imovel e seus agregados de forma atomica (EF 7.1, RN-001/RN-002/RN-019).
    /// </summary>
    public record CriarImovelCommand(
        string Descricao,
        Guid? MunicipioId,
        string? Cep,
        string? Logradouro,
        string? Numero,
        string? Complemento,
        string? Bairro,
        List<ProprietarioInput> Proprietarios,
        List<VistoriaInput>? Vistorias,
        List<CustoImovelInput>? Custos
    ) : ICommand;

    public class CriarImovelCommandValidator : AbstractValidator<CriarImovelCommand>
    {
        public CriarImovelCommandValidator()
        {
            RuleFor(c => c.Descricao).NotEmpty().WithMessage("A descricao do imovel e obrigatoria.");
            RuleFor(c => c.Proprietarios).NotEmpty().WithMessage("O imovel exige ao menos um proprietario.");
        }
    }

    /// <summary>
    /// Exclui (soft delete) o imovel e seus agregados de forma consistente (RN-005/RN-007).
    /// </summary>
    public record ExcluirImovelCommand(Guid ImovelId) : ICommand;

    public class ExcluirImovelCommandValidator : AbstractValidator<ExcluirImovelCommand>
    {
        public ExcluirImovelCommandValidator()
        {
            RuleFor(c => c.ImovelId).NotEmpty().WithMessage("Identificador do imovel invalido.");
        }
    }
}
