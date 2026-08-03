using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    /// <summary>
    /// Comandos da Declaração de Importação (DI) por item de compra (CD1 / EF COMERCIO_EXTERIOR, CEX-001..023).
    /// Cobrem o CRUD da DI (CompraItemImportacao) e de suas ADIÇÕES (CompraItemImportacaoAdicao), que existiam
    /// no domínio sem command/handler/endpoint.
    /// ⚠️ valida-contador: ValorAFRMM, ValorDesconto e demais montantes tributários entram FACTUALMENTE
    /// (informados pelo despacho aduaneiro/contador); o sistema apenas armazena e apropria — não calcula tributo.
    /// </summary>
    public record RegistrarDeclaracaoImportacaoCommand(
        Guid CompraItemId,
        string NumeroDeclaracaoImportacao,
        DateTime DataDeclaracaoImportacao,
        string LocalDesembaraco,
        string UfDesembaraco,
        DateTime DataDesembaraco,
        ETipoViaTransporte TipoViaTransporte,
        decimal ValorAFRMM,               // valida-contador (factual)
        ETipoIntermedioImportacao TipoIntermedio,
        string? Cnpj = null,
        string? Cpf = null,
        string? UfTerceiro = null,
        string CodigoExportador = ""
    ) : ICommand;

    /// <summary>Altera os dados de uma DI existente.</summary>
    public record AlterarDeclaracaoImportacaoCommand(
        Guid Id,
        string NumeroDeclaracaoImportacao,
        DateTime DataDeclaracaoImportacao,
        string LocalDesembaraco,
        string UfDesembaraco,
        DateTime DataDesembaraco,
        ETipoViaTransporte TipoViaTransporte,
        decimal ValorAFRMM,               // valida-contador (factual)
        ETipoIntermedioImportacao TipoIntermedio,
        string? Cnpj = null,
        string? Cpf = null,
        string? UfTerceiro = null,
        string CodigoExportador = ""
    ) : ICommand;

    /// <summary>Exclui (soft-delete) a DI e suas adições.</summary>
    public record ExcluirDeclaracaoImportacaoCommand(Guid Id) : ICommand;

    /// <summary>Adiciona uma ADIÇÃO à DI.</summary>
    public record AdicionarAdicaoImportacaoCommand(
        Guid DeclaracaoImportacaoId,
        int NumeroAdicao,
        int NumeroSequencialAdicao,
        string CodigoFabricante,
        decimal ValorDesconto,            // valida-contador (factual)
        string? NumeroAtoConcessorio = null
    ) : ICommand;

    /// <summary>Altera uma ADIÇÃO da DI.</summary>
    public record AlterarAdicaoImportacaoCommand(
        Guid DeclaracaoImportacaoId,
        Guid AdicaoId,
        int NumeroAdicao,
        int NumeroSequencialAdicao,
        string CodigoFabricante,
        decimal ValorDesconto,            // valida-contador (factual)
        string? NumeroAtoConcessorio = null
    ) : ICommand;

    /// <summary>Exclui (soft-delete) uma ADIÇÃO da DI.</summary>
    public record ExcluirAdicaoImportacaoCommand(Guid DeclaracaoImportacaoId, Guid AdicaoId) : ICommand;
}
