using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using Epros.Modules.GestaoClientes.Domain.Entities;
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
        PlanoDuration Duration = PlanoDuration.Mensal,
        bool ModuloCrm = false,
        bool ModuloProjetos = false,
        bool ModuloRh = false,
        bool ModuloFinanceiro = false,
        bool ModuloPdv = false,
        List<ModuloPlanoInput>? Modulos = null,
        // 1.06 — 0 = ilimitado; DiasToleranciaInadimplencia null → fallback 15 no middleware.
        int LimiteClientes = 0,
        int? DiasToleranciaInadimplencia = 15
    ) : ICommand;

    public class CriarPlanoRicoCommandValidator : AbstractValidator<CriarPlanoRicoCommand>
    {
        public CriarPlanoRicoCommandValidator()
        {
            RuleFor(p => p.Nome)
                .NotEmpty().WithMessage("O Nome do plano é obrigatório.");

            RuleFor(p => p.Valor)
                .GreaterThanOrEqualTo(0).WithMessage("O Valor do plano deve ser maior ou igual a zero.");

            // 1.06 (REG-014): 0 = ilimitado.
            RuleFor(p => p.LimiteUsuarios)
                .GreaterThanOrEqualTo(0).WithMessage("O Limite de usuários deve ser maior ou igual a zero (0 = ilimitado).");

            RuleFor(p => p.LimiteEmpresas)
                .GreaterThanOrEqualTo(0).WithMessage("O Limite de empresas deve ser maior ou igual a zero (0 = ilimitado).");

            RuleFor(p => p.LimiteClientes)
                .GreaterThanOrEqualTo(0).WithMessage("O Limite de clientes deve ser maior ou igual a zero (0 = ilimitado).");
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
        PlanoDuration Duration = PlanoDuration.Mensal,
        bool ModuloCrm = false,
        bool ModuloProjetos = false,
        bool ModuloRh = false,
        bool ModuloFinanceiro = false,
        bool ModuloPdv = false,
        List<ModuloPlanoInput>? Modulos = null,
        // 1.06 — 0 = ilimitado; DiasToleranciaInadimplencia null → fallback 15 no middleware.
        int LimiteClientes = 0,
        int? DiasToleranciaInadimplencia = 15
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

            // 1.06 (REG-014): 0 = ilimitado.
            RuleFor(p => p.LimiteUsuarios)
                .GreaterThanOrEqualTo(0).WithMessage("O Limite de usuários deve ser maior ou igual a zero (0 = ilimitado).");

            RuleFor(p => p.LimiteEmpresas)
                .GreaterThanOrEqualTo(0).WithMessage("O Limite de empresas deve ser maior ou igual a zero (0 = ilimitado).");

            RuleFor(p => p.LimiteClientes)
                .GreaterThanOrEqualTo(0).WithMessage("O Limite de clientes deve ser maior ou igual a zero (0 = ilimitado).");
        }
    }

    public record ExcluirPlanoCommand(Guid Id) : ICommand;
}
