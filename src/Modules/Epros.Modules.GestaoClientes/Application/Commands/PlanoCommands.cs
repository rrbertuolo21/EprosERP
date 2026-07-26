using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    /// <summary>
    /// Item de módulo vinculado a um plano.
    /// OBS: a entidade <c>ModuloPlano</c> persiste apenas <c>NomeModulo</c>.
    /// ModuloGeralId/Descricao/Valor/Ativo são aceitos para compatibilidade com o form,
    /// mas hoje só <c>Nome</c> (ou o ModuloGeralId como texto) é persistido.
    /// </summary>
    public record ModuloPlanoInput(
        Guid? ModuloGeralId = null,
        string? Nome = null,
        string? Descricao = null,
        decimal Valor = 0m,
        bool Ativo = true
    );

    /// <summary>Criação rica de Plano (landlord) — casa com a entidade Plano + módulos.</summary>
    public record CriarPlanoRicoCommand(
        string Nome,
        decimal Valor,
        Guid? GrupoPlanoId = null,
        string? DescricaoCurta = null,
        string? DescricaoCompleta = null,
        int LimiteUsuarios = 999,
        int LimiteEmpresas = 99,
        DateTime? DataInicio = null,
        DateTime? DataFim = null,
        bool Ativo = true,
        string? RecursosInclusos = null,
        List<ModuloPlanoInput>? Modulos = null
    ) : ICommand;

    public class CriarPlanoRicoCommandValidator : AbstractValidator<CriarPlanoRicoCommand>
    {
        public CriarPlanoRicoCommandValidator()
        {
            RuleFor(p => p.Nome)
                .NotEmpty().WithMessage("O Nome do plano é obrigatório.");

            RuleFor(p => p.Valor)
                .GreaterThanOrEqualTo(0).WithMessage("O Valor do plano deve ser maior ou igual a zero.");

            RuleFor(p => p.LimiteUsuarios)
                .GreaterThan(0).WithMessage("O Limite de usuários deve ser maior que zero.");

            RuleFor(p => p.LimiteEmpresas)
                .GreaterThan(0).WithMessage("O Limite de empresas deve ser maior que zero.");
        }
    }

    /// <summary>Atualização rica de Plano (landlord) + reconciliação de módulos.</summary>
    public record AtualizarPlanoCommand(
        Guid Id,
        string Nome,
        decimal Valor,
        Guid? GrupoPlanoId = null,
        string? DescricaoCurta = null,
        string? DescricaoCompleta = null,
        int LimiteUsuarios = 999,
        int LimiteEmpresas = 99,
        DateTime? DataInicio = null,
        DateTime? DataFim = null,
        bool Ativo = true,
        string? RecursosInclusos = null,
        List<ModuloPlanoInput>? Modulos = null
    ) : ICommand;

    public class AtualizarPlanoCommandValidator : AbstractValidator<AtualizarPlanoCommand>
    {
        public AtualizarPlanoCommandValidator()
        {
            RuleFor(p => p.Id)
                .NotEmpty().WithMessage("O ID do plano é obrigatório.");

            RuleFor(p => p.Nome)
                .NotEmpty().WithMessage("O Nome do plano é obrigatório.");

            RuleFor(p => p.Valor)
                .GreaterThanOrEqualTo(0).WithMessage("O Valor do plano deve ser maior ou igual a zero.");

            RuleFor(p => p.LimiteUsuarios)
                .GreaterThan(0).WithMessage("O Limite de usuários deve ser maior que zero.");

            RuleFor(p => p.LimiteEmpresas)
                .GreaterThan(0).WithMessage("O Limite de empresas deve ser maior que zero.");
        }
    }

    public record ExcluirPlanoCommand(Guid Id) : ICommand;
}
