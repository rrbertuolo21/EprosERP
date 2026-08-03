using System;
using System.Collections.Generic;

namespace Epros.Modules.Aplicativo.Application.Dtos
{
    public record AssinaturaClienteDto(
        Guid Id,
        Guid ClienteId,
        Guid PlanoId,
        string Status,
        DateTime? DataInicio,
        DateTime? DataFim,
        DateTime? TrialAte,
        string MetodoPagamento,
        string? TransacaoId,
        string? DetalhesPacoteJson,
        bool Arquivada
    );

    public record PlanoPublicoDto(
        Guid Id,
        string Nome,
        decimal Preco,
        int LimiteUsuarios,
        int LimiteEmpresas,
        string? RecursosInclusos,
        List<string> Modulos
    );

    public record FaturaTenantDto(
        Guid Id,
        Guid ClienteId,
        decimal Valor,
        DateTime DataVencimento,
        DateTime? DataPagamento,
        string Status
    );

    public record PixResponseDto(
        Guid FaturaId,
        decimal Valor,
        string QrCodeBase64,
        string CheckoutUrl,
        DateTime DataExpiracao
    );

    // 1.08D — Resultado da mudança de plano com o ajuste de proração calculado (mecanismo pro-rata por dias).
    // ⚠️ ValidaContador=true: política de arredondamento/índice/contas contábeis é parâmetro do contador.
    public record MudancaPlanoResultadoDto(
        Guid AssinaturaId,
        Guid PlanoAnteriorId,
        Guid PlanoNovoId,
        string TipoMudanca,          // "Upgrade" | "Downgrade" | "Lateral"
        decimal PrecoAnterior,
        decimal PrecoNovo,
        int DiasCiclo,
        int DiasRestantes,
        decimal ValorProracao,       // positivo = débito (fatura de diferença) | negativo = crédito
        string TipoProracao,         // "Debito" | "Credito" | "Neutro"
        Guid? FaturaDiferencaId,
        bool ValidaContador
    );
}
