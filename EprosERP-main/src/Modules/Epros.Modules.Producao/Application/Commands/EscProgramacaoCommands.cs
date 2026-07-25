using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Producao.Application.Commands
{
    // ===================== PRD-ESC — Escalonamento e Programação =====================
    // Motor de sequenciamento/capacidade finita é lacuna controlada (ver EF §14).

    public record CriarEscOperacaoInput(
        int Sequencia = 0,
        Guid? ServicoId = null,
        Guid? EquipamentoId = null,
        Guid? ColaboradorId = null,
        DateTime? InicioPrevisto = null,
        DateTime? TerminoPrevisto = null,
        int HorasPrevistas = 0,
        int MinutosPrevistos = 0,
        int SegundosPrevistos = 0,
        decimal CustoPrevisto = 0m);

    public record CriarEscProgramacaoCommand(
        string Codigo,
        Guid ResponsavelId,
        Guid? PlanoProducaoId = null,
        Guid? OrdemProducaoId = null,
        Guid? CentroTrabalhoId = null,
        int? Prioridade = null,
        List<CriarEscOperacaoInput>? Operacoes = null
    ) : ICommand;

    public class CriarEscProgramacaoCommandValidator : AbstractValidator<CriarEscProgramacaoCommand>
    {
        public CriarEscProgramacaoCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O código da programação é obrigatório. (ESC-REG-002)");
            RuleFor(c => c.ResponsavelId).NotEmpty().WithMessage("O responsável é obrigatório. (ESC-REG-004)");
        }
    }

    public record AdicionarEscOperacaoCommand(
        Guid ProgramacaoId,
        int Sequencia = 0,
        Guid? ServicoId = null,
        Guid? EquipamentoId = null,
        Guid? ColaboradorId = null,
        DateTime? InicioPrevisto = null,
        DateTime? TerminoPrevisto = null,
        int HorasPrevistas = 0,
        int MinutosPrevistos = 0,
        int SegundosPrevistos = 0,
        decimal CustoPrevisto = 0m
    ) : ICommand;

    public record RegistrarEscOperacaoRealizadoCommand(
        Guid OperacaoId,
        DateTime? InicioRealizado,
        DateTime? TerminoRealizado,
        int HorasRealizadas = 0,
        int MinutosRealizados = 0,
        int SegundosRealizados = 0,
        decimal CustoRealizado = 0m
    ) : ICommand;

    public record SubmeterEscProgramacaoCommand(Guid Id) : ICommand;
    public record AprovarEscProgramacaoCommand(Guid Id) : ICommand;
    public record RejeitarEscProgramacaoCommand(Guid Id, string Motivo) : ICommand;
    public record InativarEscProgramacaoCommand(Guid Id) : ICommand;
    public record ReativarEscProgramacaoCommand(Guid Id) : ICommand;
    public record EncerrarEscProgramacaoCommand(Guid Id) : ICommand;

    public record SalvarEscParametroCommand(string Chave, string? Valor, bool Ativo = true) : ICommand;

    public class SalvarEscParametroCommandValidator : AbstractValidator<SalvarEscParametroCommand>
    {
        public SalvarEscParametroCommandValidator()
        {
            RuleFor(c => c.Chave).NotEmpty().WithMessage("A chave do parâmetro é obrigatória. (ESC-REG-023)");
        }
    }
}
