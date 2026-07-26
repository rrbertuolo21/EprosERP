using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;
using FluentValidation;

namespace Epros.Modules.Estoque.Application.Commands
{
    /// <summary>
    /// Lançamento fiscal completo de uma compra: grava o agregado raiz Compra junto com emitente,
    /// destinatário, itens, impostos (nível compra), transporte, total, fatura e pagamentos.
    /// Complementa <see cref="LancarCompraCommand"/> (que só grava cabeçalho + itens/estoque).
    /// </summary>
    public record LancarCompraFiscalCommand(
        string FornecedorCnpj,
        string FornecedorNome,
        string NumeroNota,
        string ChaveAcesso,
        decimal ValorTotal,
        DateTime DataEmissao,
        List<ItemCompraFiscalInput> Itens,
        EmitenteInput? Emitente = null,
        DestinatarioInput? Destinatario = null,
        ImpostoCompraInput? Imposto = null,
        TotalCompraInput? Total = null,
        TransporteCompraInput? Transporte = null,
        List<PagamentoCompraInput>? Pagamentos = null
    ) : ICommand;

    public record ItemCompraFiscalInput(
        string Sku,
        string NomeProduto,
        decimal Quantidade,
        decimal PrecoUnitario,
        decimal ValorIms,
        decimal ValorIpi,
        string? Ncm = null,
        int Cfop = 0,
        string? UnidadeComercial = null
    );

    public record EmitenteInput(
        string? Cnpj,
        string? Cpf,
        string RazaoSocial,
        string? NomeFantasia,
        string? Telefone,
        string InscricaoEstadual,
        int Cnae = 0,
        ERegimeTributario RegimeTributario = ERegimeTributario.SimplesNacional
    );

    public record DestinatarioInput(
        Guid? PessoaId,
        string? Cnpj,
        string? Cpf,
        string RazaoSocial,
        string? Telefone,
        string? InscricaoEstadual,
        string? Email,
        ETipoIndicadorIe IndicadorIE = ETipoIndicadorIe.ContribuinteICMS,
        bool EhConsumidorFinal = false
    );

    public record ImpostoCompraInput(decimal ValorAliquotaCreditoIcms);

    public record TotalCompraInput(
        decimal ValorProduto,
        decimal ValorFrete,
        decimal ValorSeguro,
        decimal ValorDesconto,
        decimal ValorIpi,
        decimal ValorIcms,
        decimal ValorNotaFiscal
    );

    public record TransporteCompraInput(
        Guid? TransportadoraPessoaId,
        string? TransportadoraCnpj,
        string? TransportadoraCpf,
        string? TransportadoraRazaoSocialOuNome,
        string? TransportadoraInscricaoEstadual,
        EEstado TransportadoraUf = EEstado.SP
    );

    public record PagamentoCompraInput(
        ETipoPagamento TipoPagamento,
        decimal ValorPagamento,
        decimal ValorTroco = 0m
    );

    public class LancarCompraFiscalCommandValidator : AbstractValidator<LancarCompraFiscalCommand>
    {
        public LancarCompraFiscalCommandValidator()
        {
            RuleFor(c => c.FornecedorCnpj).NotEmpty().WithMessage("O CNPJ do fornecedor é obrigatório.");
            RuleFor(c => c.FornecedorNome).NotEmpty().WithMessage("O Nome do fornecedor é obrigatório.");
            RuleFor(c => c.NumeroNota).NotEmpty().WithMessage("O Número da nota é obrigatório.");
            RuleFor(c => c.ChaveAcesso)
                .NotEmpty().WithMessage("A Chave de Acesso é obrigatória.")
                .Length(44).WithMessage("A Chave de Acesso da NF-e deve possuir exatamente 44 dígitos.");
            RuleFor(c => c.ValorTotal).GreaterThanOrEqualTo(0).WithMessage("O Valor Total da nota não pode ser negativo.");
            RuleFor(c => c.Itens).NotEmpty().WithMessage("A compra deve possuir pelo menos um item.");

            RuleForEach(c => c.Itens).ChildRules(item =>
            {
                item.RuleFor(i => i.Sku).NotEmpty().WithMessage("O SKU do produto é obrigatório.");
                item.RuleFor(i => i.NomeProduto).NotEmpty().WithMessage("O Nome do produto é obrigatório.");
                item.RuleFor(i => i.Quantidade).GreaterThan(0).WithMessage("A quantidade do item deve ser maior que zero.");
                item.RuleFor(i => i.PrecoUnitario).GreaterThan(0).WithMessage("O preço unitário do item deve ser maior que zero.");
            });
        }
    }
}
