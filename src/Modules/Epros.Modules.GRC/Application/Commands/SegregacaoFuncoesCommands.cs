using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GRC.Application.Commands
{
    // GRC-SOD — Segregacao de Funcoes

    public record CriarFuncaoSoDCommand(
        string Codigo,
        string Nome,
        string? Descricao
    ) : ICommand;

    public class CriarFuncaoSoDCommandValidator : AbstractValidator<CriarFuncaoSoDCommand>
    {
        public CriarFuncaoSoDCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O codigo da funcao SoD e obrigatorio.");
            RuleFor(c => c.Nome).NotEmpty().WithMessage("O nome da funcao SoD e obrigatorio.");
        }
    }

    public record CriarRegraSoDCommand(
        string? Codigo,
        Guid FuncaoAId,
        Guid FuncaoBId,
        string Criticidade, // Baixa, Media, Alta, Critica
        DateTime VigenciaInicio,
        DateTime? VigenciaFim
    ) : ICommand;

    public class CriarRegraSoDCommandValidator : AbstractValidator<CriarRegraSoDCommand>
    {
        public CriarRegraSoDCommandValidator()
        {
            RuleFor(c => c.FuncaoAId).NotEmpty().WithMessage("A funcao A e obrigatoria.");
            RuleFor(c => c.FuncaoBId).NotEmpty().WithMessage("A funcao B e obrigatoria.");
            RuleFor(c => c).Must(c => c.FuncaoAId != c.FuncaoBId)
                .WithMessage("A regra SoD deve relacionar duas funcoes distintas.");
            RuleFor(c => c.Criticidade).Must(cr => cr == "Baixa" || cr == "Media" || cr == "Alta" || cr == "Critica")
                .WithMessage("A criticidade deve ser 'Baixa', 'Media', 'Alta' ou 'Critica'.");
        }
    }

    /// <summary>
    /// RF-GRC-SOD-002: simula os conflitos de um perfil/usuario contra as regras SoD ativas,
    /// dado o conjunto de funcoes atribuidas. Persiste a simulacao e as violacoes detectadas.
    /// </summary>
    public record SimularSoDCommand(
        Guid? PerfilId,
        Guid? UsuarioId,
        string Alvo, // Perfil, Usuario
        List<Guid> FuncoesAtribuidas
    ) : ICommand;

    public class SimularSoDCommandValidator : AbstractValidator<SimularSoDCommand>
    {
        public SimularSoDCommandValidator()
        {
            RuleFor(c => c.Alvo).Must(a => a == "Perfil" || a == "Usuario")
                .WithMessage("O alvo deve ser 'Perfil' ou 'Usuario'.");
            RuleFor(c => c).Must(c => c.PerfilId != null || c.UsuarioId != null)
                .WithMessage("A simulacao deve informar um perfil ou um usuario.");
            RuleFor(c => c.FuncoesAtribuidas).NotNull().WithMessage("As funcoes atribuidas sao obrigatorias.");
        }
    }
}
