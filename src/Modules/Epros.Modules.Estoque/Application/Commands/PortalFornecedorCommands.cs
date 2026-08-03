using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    /// <summary>PFO-001/002: convida um fornecedor (interno). Publica est.pfo.convite_enviado.</summary>
    public record ConvidarFornecedorCommand(Guid FornecedorId, string EmailConvite, DateTime? DataExpiracao = null) : ICommand;

    /// <summary>Ativa o acesso do fornecedor (aceite do convite). Publica est.pfo.acesso_ativado.</summary>
    public record AtivarAcessoFornecedorCommand(Guid ConviteId, Guid UsuarioId) : ICommand;

    /// <summary>Publica uma cotação (de COMPRAS) a um fornecedor no Portal.</summary>
    public record PublicarCotacaoFornecedorCommand(Guid CotacaoOrigemId, Guid FornecedorId, DateTime? PrazoResposta = null) : ICommand;

    public record RespostaCotacaoItemInput(Guid? ItemOrigemId, Guid? ProdutoId, decimal? Quantidade, decimal? ValorUnitario, int? PrazoEntregaDias = null);

    /// <summary>PFO-002/VAL-PFO-003: fornecedor responde a cotação (isolado; cotação precisa estar aberta).</summary>
    public record ResponderCotacaoFornecedorCommand(
        Guid CotacaoPublicadaId,
        Guid FornecedorId,
        decimal? ValorTotal,
        List<RespostaCotacaoItemInput> Itens,
        string? Observacao = null
    ) : ICommand;

    public record PreAvisoItemInput(Guid PedidoCompraItemId, Guid? ProdutoId, decimal? QuantidadePrevista, string? Lote = null);

    /// <summary>PFO-007/VAL-PFO-004: fornecedor envia pré-aviso (ASN) de um pedido a ele vinculado.</summary>
    public record EnviarPreAvisoEmbarqueCommand(
        Guid PedidoCompraId,
        Guid FornecedorId,
        DateTime? DataPrevistaEntrega,
        List<PreAvisoItemInput> Itens,
        string? Observacao = null
    ) : ICommand;

    /// <summary>PFO-008/VAL-PFO-005: fornecedor envia documento (metadado + FK GED) referenciando cotação/pedido/pré-aviso.</summary>
    public record EnviarDocumentoFornecedorCommand(
        Guid FornecedorId,
        EReferenciaDocumentoFornecedor ReferenciaTipo,
        Guid ReferenciaId,
        Guid ArquivoId,
        string? TipoDocumento = null
    ) : ICommand;
}
