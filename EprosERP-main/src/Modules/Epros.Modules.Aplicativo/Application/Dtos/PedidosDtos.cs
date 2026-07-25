using System;

namespace Epros.Modules.Aplicativo.Application.Dtos
{
    public record PedidoSaaSDto(
        Guid Id,
        Guid ClienteId,
        Guid PlanoId,
        string PlanoNome,
        Guid? CupomId,
        decimal ValorBase,
        decimal ValorDesconto,
        decimal ValorTotal,
        string Moeda,
        string MetodoPagamento,
        string Status,
        DateTime CriadoEm
    );

    public record TransferenciaPendenteDto(
        Guid Id,
        Guid? FaturaId,
        Guid? PedidoId,
        decimal Valor,
        string Moeda,
        string Status,
        DateTime CriadoEm,
        string NomeArquivo,
        string CaminhoArquivo,
        long TamanhoBytes
    );
}
