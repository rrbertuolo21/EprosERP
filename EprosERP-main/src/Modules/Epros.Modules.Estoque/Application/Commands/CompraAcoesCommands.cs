using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Estoque.Application.Commands
{
    /// <summary>
    /// Porte de CompraController.PostEntradaPropria (POST api/v1/compras/entrada-propria).
    /// Lança uma NF-e de ENTRADA PRÓPRIA (a empresa é a emitente; modelo 55). Persiste o agregado Compra
    /// completo (emitente próprio, destinatário, itens, total, transporte, pagamentos) e cria a CompraNfe
    /// (número/série próprios) marcada para emissão. A transmissão à SEFAZ é feita pela camada de emissão
    /// fiscal (Hercules/DFe) — não reimplementada aqui.
    /// </summary>
    public record EntradaPropriaCompraCommand(
        string NumeroNota,
        DateTime DataEmissao,
        decimal ValorTotal,
        int SerieNfe,
        long NumeroNfe,
        string NaturezaOperacao,
        List<ItemCompraFiscalInput> Itens,
        EmitenteInput Emitente,
        DestinatarioInput Destinatario,
        ImpostoCompraInput? Imposto = null,
        TotalCompraInput? Total = null,
        TransporteCompraInput? Transporte = null,
        List<PagamentoCompraInput>? Pagamentos = null,
        string? InformacoesComplementares = null,
        string? InformacoesAdicionaisFisco = null,
        DateTime? DataHoraSaida = null
    ) : ICommand;

    /// <summary>
    /// Porte de CompraController.PostEntradaFornecedor (POST api/v1/compras/entrada-fornecedor).
    /// Lança uma compra a partir de uma NF-e já emitida pelo FORNECEDOR (o fornecedor é o emitente;
    /// modelo 55). Persiste o agregado Compra completo e a CompraNfe (com a chave/série/número da nota
    /// do fornecedor). Equivalente ao fluxo fiscal completo (mesmo agregado do LancarCompraFiscalCommand),
    /// com TipoNota = NotaFiscalEntrada e origem NfeCompleta.
    /// </summary>
    public record EntradaFornecedorCompraCommand(
        string ChaveAcesso,
        string NumeroNota,
        int SerieNfe,
        long NumeroNfe,
        DateTime DataEmissao,
        decimal ValorTotal,
        string NaturezaOperacao,
        List<ItemCompraFiscalInput> Itens,
        EmitenteInput Emitente,
        DestinatarioInput? Destinatario = null,
        ImpostoCompraInput? Imposto = null,
        TotalCompraInput? Total = null,
        TransporteCompraInput? Transporte = null,
        List<PagamentoCompraInput>? Pagamentos = null,
        string? InformacoesComplementares = null,
        string? InformacoesAdicionaisFisco = null,
        DateTime? DataHoraSaida = null
    ) : ICommand;

    /// <summary>
    /// Porte de CompraController.PostCartaCorrecao (POST api/v1/compras/carta-correcao).
    /// Registra uma Carta de Correção Eletrônica (CC-e) na NF-e da compra (CompraNfe). A transmissão do
    /// evento à SEFAZ é responsabilidade da camada de emissão fiscal; aqui persistimos a CC-e fielmente
    /// à entidade (porte de CompraNfe.AdicionarCartaCorrecao).
    /// </summary>
    public record CartaCorrecaoCompraCommand(
        Guid CompraId,
        string TextoCorrecao,
        ETipoAmbiente Ambiente = ETipoAmbiente.Producao
    ) : ICommand;

    /// <summary>
    /// Porte de CompraController.PutDuplicarCompras (PUT api/v1/compras/duplicar-compras).
    /// Duplica uma compra existente (cabeçalho + itens) em uma nova compra rascunho, com nova chave/número
    /// informados. Não copia a NF-e autorizada nem os dados de transmissão.
    /// </summary>
    public record DuplicarCompraCommand(
        Guid CompraId,
        string NovoNumeroNota,
        string NovaChaveAcesso
    ) : ICommand;

    public class EntradaPropriaCompraCommandValidator : AbstractValidator<EntradaPropriaCompraCommand>
    {
        public EntradaPropriaCompraCommandValidator()
        {
            RuleFor(c => c.NumeroNota).NotEmpty().WithMessage("O Número da nota é obrigatório.");
            RuleFor(c => c.Emitente).NotNull().WithMessage("O Emitente (empresa) é obrigatório para entrada própria.");
            RuleFor(c => c.Destinatario).NotNull().WithMessage("O Destinatário é obrigatório para NF-e de entrada própria.");
            RuleFor(c => c.Itens).NotEmpty().WithMessage("A compra deve possuir pelo menos um item.");
            RuleFor(c => c.ValorTotal).GreaterThanOrEqualTo(0).WithMessage("O Valor Total da nota não pode ser negativo.");
        }
    }

    public class EntradaFornecedorCompraCommandValidator : AbstractValidator<EntradaFornecedorCompraCommand>
    {
        public EntradaFornecedorCompraCommandValidator()
        {
            RuleFor(c => c.ChaveAcesso)
                .NotEmpty().WithMessage("A Chave de Acesso é obrigatória.")
                .Length(44).WithMessage("A Chave de Acesso da NF-e deve possuir exatamente 44 dígitos.");
            RuleFor(c => c.NumeroNota).NotEmpty().WithMessage("O Número da nota é obrigatório.");
            RuleFor(c => c.Emitente).NotNull().WithMessage("O Emitente (fornecedor) é obrigatório.");
            RuleFor(c => c.Itens).NotEmpty().WithMessage("A compra deve possuir pelo menos um item.");
            RuleFor(c => c.ValorTotal).GreaterThanOrEqualTo(0).WithMessage("O Valor Total da nota não pode ser negativo.");
        }
    }

    public class CartaCorrecaoCompraCommandValidator : AbstractValidator<CartaCorrecaoCompraCommand>
    {
        public CartaCorrecaoCompraCommandValidator()
        {
            RuleFor(c => c.CompraId).NotEmpty().WithMessage("O Id da compra é obrigatório.");
            RuleFor(c => c.TextoCorrecao)
                .NotEmpty().WithMessage("O texto da correção é obrigatório.")
                .MinimumLength(15).WithMessage("O texto da correção deve ter no mínimo 15 caracteres.")
                .MaximumLength(1000).WithMessage("O texto da correção deve ter no máximo 1000 caracteres.");
        }
    }

    public class DuplicarCompraCommandValidator : AbstractValidator<DuplicarCompraCommand>
    {
        public DuplicarCompraCommandValidator()
        {
            RuleFor(c => c.CompraId).NotEmpty().WithMessage("O Id da compra é obrigatório.");
            RuleFor(c => c.NovoNumeroNota).NotEmpty().WithMessage("O novo número da nota é obrigatório.");
            RuleFor(c => c.NovaChaveAcesso)
                .NotEmpty().WithMessage("A nova Chave de Acesso é obrigatória.")
                .Length(44).WithMessage("A Chave de Acesso da NF-e deve possuir exatamente 44 dígitos.");
        }
    }

    /// <summary>
    /// Porte de CompraController.GerarDanfeSemAutorizacao (POST api/v1/compras/gerar-danfe-sem-autorizacao/{compraId}).
    /// Gera uma pré-visualização (DANFE SEM VALOR FISCAL) da compra, em PDF, a partir dos dados persistidos.
    /// Honestidade fiscal: como a nota não está autorizada, o PDF é marcado como SEM VALOR FISCAL.
    /// </summary>
    public record GerarDanfeSemAutorizacaoCompraCommand(Guid CompraId) : ICommand;

    /// <summary>Resultado da geração da pré-visualização do DANFE (PDF em bytes), retornado em CommandResult.Dados.</summary>
    public record DanfePreviewResult(Guid CompraId, string NomeArquivo, byte[] Conteudo, bool SemValorFiscal = true);
}
