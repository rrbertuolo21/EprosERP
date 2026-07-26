using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    // ============ ENTRADA ============

    /// <summary>Cria uma entrada em rascunho vinculada à compra (EF Logística de Entrada §10.1 passo 1).</summary>
    public record CriarLdeEntradaCommand(
        Guid CompraId,
        Guid FornecedorId,
        Guid? LocalEntregaId = null,
        Guid? DocumentoEntradaId = null
    ) : ICommand;

    /// <summary>LDE-002 a LDE-010: registra local de entrega com obrigatórios.</summary>
    public record RegistrarLdeLocalEntregaCommand(
        Guid EntradaId,
        Guid CompraId,
        string Nome,
        string Fone,
        string Email,
        string InscricaoEstadual,
        string Documento,
        string Uf,
        Guid MunicipioId,
        int PaisId,
        string? Logradouro = null,
        string? Numero = null,
        string? Complemento = null,
        string? Bairro = null,
        string? MunicipioNome = null,
        string? Cep = null,
        string? PaisNome = null
    ) : ICommand;

    public record LdeDocumentoItemInput(
        Guid? ProdutoId,
        decimal? QuantidadeDocumento = null,
        decimal? ValorItem = null,
        string? DadosTributariosItem = null
    );

    public record LdeDocumentoDuplicataInput(
        string? Numero,
        DateTime? DataVencimento,
        decimal? Valor
    );

    /// <summary>LDE-013/014/017: vincula documento de entrada com emitente, itens, duplicatas.</summary>
    public record VincularLdeDocumentoCommand(
        Guid EntradaId,
        string? ChaveAcesso,
        string? Numero,
        string? Serie,
        DateTime? DataEmissao,
        string? NaturezaOperacao,
        decimal? ValorTotal,
        Guid? FornecedorId,
        Guid? DestinatarioId,
        Guid? EmitenteId,
        List<LdeDocumentoItemInput> Itens,
        List<LdeDocumentoDuplicataInput>? Duplicatas = null
    ) : ICommand;

    /// <summary>LDE-012/013/014/015: confirma entrada exigindo fornecedor, emitente, itens e documento.</summary>
    public record ConfirmarLdeEntradaCommand(Guid Id) : ICommand;

    /// <summary>LDE-018: cancelamento exige motivo e publica evento de reversão.</summary>
    public record CancelarLdeEntradaCommand(Guid Id, string Motivo) : ICommand;

    /// <summary>LDE-019: estorno exige permissão específica e motivo.</summary>
    public record EstornarLdeEntradaCommand(Guid Id, string Motivo) : ICommand;
}
