using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    /// <summary>
    /// Comandos do submódulo Subcontratação (EST-SUB). Modelo proposto por autoria (EF §16), sobe desabilitado
    /// (ABAC nega por padrão). CFOP/documento fiscal de remessa/retorno vêm do MOTOR FISCAL (SUB-008) — não são
    /// calculados aqui. Integração com movimento de estoque, fiscal e contas a pagar fica em pendências.
    /// </summary>
    public record SubOrdemItemInput(Guid ProdutoId, decimal QuantidadePlanejada, string? Unidade, string? OperacaoTerceirizada);

    public record CriarSubOrdemCommand(
        Guid FornecedorId,
        Guid? EmpresaId = null,
        string? NumeroOrdem = null,
        Guid? OrdemProducaoId = null,
        DateTime? DataEmissao = null,
        DateTime? DataPrevistaRetorno = null,
        string? Observacao = null,
        List<SubOrdemItemInput>? Itens = null
    ) : ICommand;

    public record SubEnvioItemInput(Guid ProdutoId, decimal QuantidadeEnviada, Guid? LoteId, Guid? LocalOrigemId);

    public record RegistrarSubEnvioCommand(
        Guid OrdemId,
        DateTime? DataEnvio = null,
        Guid? DocumentoFiscalId = null,
        List<SubEnvioItemInput>? Itens = null
    ) : ICommand;

    public record SubRetornoItemInput(Guid ProdutoId, decimal QuantidadeRetorno, decimal? QuantidadeAprovada, decimal? QuantidadePerda, decimal? QuantidadeSucata, decimal? Rendimento);

    public record RegistrarSubRetornoCommand(
        Guid OrdemId,
        DateTime? DataRetorno = null,
        Guid? DocumentoFiscalId = null,
        List<SubRetornoItemInput>? Itens = null
    ) : ICommand;

    /// <summary>
    /// SUB-009 — registra a cobrança do serviço de beneficiamento da ordem; publica evento para gerar a
    /// COMPRA do serviço + contas a pagar (fato gerador único). Idempotente por cobrança.
    /// </summary>
    public record RegistrarSubServicoCobrancaCommand(
        Guid OrdemId,
        decimal ValorServico,
        Guid? CompraId = null
    ) : ICommand;

    /// <summary>
    /// SUB-008 — registra o documento fiscal de remessa/retorno da subcontratação com CFOP PARAMETRIZADO
    /// (valida-contador — não é calculado no código). Vincula ao envio/retorno quando aplicável.
    /// </summary>
    public record RegistrarSubDocumentoFiscalCommand(
        Guid OrdemId,
        Guid? EnvioId = null,
        Guid? RetornoId = null,
        Guid? DocumentoFiscalId = null,
        string? CfopRemessa = null,
        string? CfopRetorno = null
    ) : ICommand;
}
