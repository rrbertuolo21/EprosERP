using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Producao.Application.Commands
{
    // ===================== PRD-MES — Execução de Manufatura MES =====================

    public record CriarMesOrdemItemInput(
        Guid ProdutoId,
        decimal QuantidadeProduzir,
        Guid? VariacaoId = null,
        decimal CustoPrevisto = 0m);

    public record CriarMesOrdemCommand(
        Guid EmpresaId,
        string? Referencia = null,
        DateTime? Inicio = null,
        DateTime? PrevisaoEntrega = null,
        Guid? EstruturaId = null,
        Guid? ProdutoAcabadoId = null,
        Guid? VariacaoProdutoAcabadoId = null,
        decimal CustoTotalPrevisto = 0m,
        decimal? PercentualVenda = null,
        decimal? PercentualEstoque = null,
        System.Collections.Generic.List<CriarMesOrdemItemInput>? Itens = null
    ) : ICommand;

    public class CriarMesOrdemCommandValidator : AbstractValidator<CriarMesOrdemCommand>
    {
        public CriarMesOrdemCommandValidator()
        {
            RuleFor(c => c.EmpresaId).NotEmpty().WithMessage("A empresa ou unidade operacional é obrigatória. (MES-REG-002)");
        }
    }

    public record AdicionarMesOrdemItemCommand(
        Guid OrdemId,
        Guid ProdutoId,
        decimal QuantidadeProduzir,
        Guid? VariacaoId = null,
        decimal CustoPrevisto = 0m
    ) : ICommand;

    public record RegistrarMesProducaoItemCommand(
        Guid ItemId,
        decimal QuantidadeProduzida,
        decimal QuantidadeEntregue,
        decimal CustoRealizado = 0m
    ) : ICommand;

    public record ApontarMesServicoCommand(
        Guid ItemOrdemId,
        DateTime? InicioPrevisto = null,
        DateTime? TerminoPrevisto = null,
        int HorasPrevisto = 0,
        int MinutosPrevisto = 0,
        int SegundosPrevisto = 0,
        decimal CustoPrevisto = 0m,
        DateTime? InicioRealizado = null,
        DateTime? TerminoRealizado = null,
        int HorasRealizado = 0,
        int MinutosRealizado = 0,
        int SegundosRealizado = 0,
        decimal CustoRealizado = 0m
    ) : ICommand;

    public record VincularMesEquipamentoCommand(Guid ServicoId, Guid EquipamentoId) : ICommand;

    public record FinalizarMesOrdemCommand(
        Guid OrdemId,
        DateTime DataTransacao,
        Guid LocalEstoqueId,
        decimal ValorTotalFinal,
        decimal DesperdicioUnidades = 0m,
        string? Lote = null,
        DateTime? Validade = null
    ) : ICommand;

    public class FinalizarMesOrdemCommandValidator : AbstractValidator<FinalizarMesOrdemCommand>
    {
        public FinalizarMesOrdemCommandValidator()
        {
            RuleFor(c => c.OrdemId).NotEmpty();
            RuleFor(c => c.DataTransacao).NotEmpty().WithMessage("A data de transação é obrigatória. (MES-REG-009)");
            RuleFor(c => c.LocalEstoqueId).NotEmpty().WithMessage("O local de estoque é obrigatório. (MES-REG-010)");
            RuleFor(c => c.ValorTotalFinal).GreaterThan(0).WithMessage("O valor total final é obrigatório. (MES-REG-011)");
        }
    }

    public record SubmeterMesOrdemCommand(Guid Id) : ICommand;
    public record AprovarMesOrdemCommand(Guid Id) : ICommand;
    public record RejeitarMesOrdemCommand(Guid Id, string Motivo) : ICommand;
    public record InativarMesOrdemCommand(Guid Id) : ICommand;
    public record ReativarMesOrdemCommand(Guid Id) : ICommand;
    public record EncerrarMesOrdemCommand(Guid Id) : ICommand;

    // Parâmetros por tenant (MES-REG-025/026/027)
    public record SalvarMesParametroCommand(
        string? PrefixoReferencia,
        bool BloquearEdicaoQuantidadeInsumo,
        bool AtualizarPrecoProdutoFinal,
        bool ExigirEstruturaAtiva,
        string? VersaoParametro = null
    ) : ICommand;
}
