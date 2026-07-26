using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;

namespace Epros.Modules.Vendas.Application.Commands
{
    // ============================================================================
    // Commands das AÇÕES de venda portadas na Onda 7 (frente Vendas-Acoes):
    // baixar-cupom-nao-fiscal (MEI), nfe-simplificado (criar/atualizar/transmitir),
    // duplicar-venda e gerar-danfe-sem-autorizacao.
    // Cada Command tem um Handler em VendaAcoesHandlers.cs (CQRS/MediatR).
    // ============================================================================

    // ---------- Cupom não fiscal (MEI) ----------

    /// <summary>
    /// Porte fiel de VendaController.baixar-cupom-nao-fiscal (MEI): retorna os bytes do PDF do
    /// cupom não fiscal previamente gerado para a venda (Status = SalvarImprimirMei e emitente MEI).
    /// Os bytes voltam em CommandResult.Dados como <see cref="Models.ArquivoPdfResult"/>.
    /// </summary>
    public record BaixarCupomNaoFiscalCommand(Guid VendaId) : ICommand;

    // ---------- Duplicar venda ----------

    /// <summary>
    /// Porte fiel de VendaNfeController.duplicar-venda (PUT): duplica a venda informada como novo
    /// rascunho (Status = Salvar), sem NFC-e/NF-e. Retorna o Id da nova venda em CommandResult.Dados.
    /// </summary>
    public record DuplicarVendaCommand(Guid VendaId) : ICommand;

    // ---------- Gerar DANFE sem autorização (pré-visualização) ----------

    /// <summary>
    /// Porte fiel de VendaNfeController.gerar-danfe-sem-autorizacao/{vendaId} (POST): gera o PDF de
    /// PRÉ-VISUALIZAÇÃO do DANFE (SEM VALOR FISCAL) a partir do documento fiscal vinculado à venda.
    /// A geração é delegada ao módulo Fiscal via contrato Shared IDanfeVendaService (por Guid FK).
    /// </summary>
    public record GerarDanfeSemAutorizacaoCommand(Guid VendaId) : ICommand;

    // ---------- NF-e simplificado ----------

    /// <summary>
    /// Porte de VendaController.nfe-simplificado (POST): cria o cabeçalho de uma venda de NF-e
    /// (modelo 55) simplificada. O cálculo fiscal pesado (impostos por item via motor DFe) é
    /// responsabilidade do módulo Fiscal (IHerculesFiscalService/ICalculoFiscalService) — aqui
    /// portamos a criação fiel do agregado de venda. Retorna o Id da venda criada.
    /// </summary>
    public record CriarVendaSimplificadaNfeCommand(
        string CaixaId,
        string NaturezaOperacao,
        DateTime DataVenda,
        string? InformacoesComplementares,
        string? InformacoesAdicionaisFisco,
        string Status,
        EModalidadeFrete ModalidadeFrete,
        bool IncluirFreteNoTotal,
        Guid? ClienteId,
        decimal Total,
        decimal ValorDesconto,
        decimal ValorFrete,
        string? FormaPagamento
    ) : ICommand;

    /// <summary>
    /// Porte de VendaController.nfe-simplificado (PUT): atualiza o cabeçalho de uma venda de NF-e
    /// simplificada existente (mesma semântica de Venda.Alterar).
    /// </summary>
    public record AtualizarVendaSimplificadaNfeCommand(
        Guid Id,
        string NaturezaOperacao,
        DateTime DataVenda,
        string? InformacoesComplementares,
        string? InformacoesAdicionaisFisco,
        EModalidadeFrete ModalidadeFrete,
        decimal ValorDesconto,
        decimal ValorFrete
    ) : ICommand;

    /// <summary>
    /// Porte de VendaController.nfe-simplificado-transmitir (POST): inclui a NF-e (série/número) na
    /// venda e marca a situação como Transmitido (porte fiel do marcador de transmissão; a
    /// comunicação com a SEFAZ é do motor fiscal). Série/número são atribuídos pelo chamador/fiscal.
    /// </summary>
    public record TransmitirVendaSimplificadaNfeCommand(
        Guid VendaId,
        int Serie,
        long Numero,
        DateTime? DataHoraSaida
    ) : ICommand;
}
