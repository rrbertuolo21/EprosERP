using System;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Enums;
using MediatR;

namespace Epros.Modules.Financeiro.Application.Commands
{
    // ─────────────────────────────────────────────────────
    // FatoGeradorFinanceiro
    // ─────────────────────────────────────────────────────
    public record CriarFatoGeradorFinanceiroCommand(
        EOrigem Origem,
        Guid? VendaId,
        Guid? CompraId,
        string? Descricao
    ) : IRequest<CommandResult>;

    public record AtualizarFatoGeradorFinanceiroCommand(
        Guid Id,
        EOrigem Origem,
        Guid? VendaId,
        Guid? CompraId,
        string? Descricao
    ) : IRequest<CommandResult>;

    public record DeletarFatoGeradorFinanceiroCommand(Guid Id) : IRequest<CommandResult>;

    // ─────────────────────────────────────────────────────
    // Importação de arquivo OFX
    // ─────────────────────────────────────────────────────
    public record CriarImportacaoArquivoOfxCommand(
        string CodigoBanco,
        string NumeroConta,
        string TipoConta,
        DateTime DataInicioExtrato,
        DateTime DataFimExtrato
    ) : IRequest<CommandResult>;

    public record AdicionarTransacaoOfxCommand(
        Guid ImportacacaoArquivoOfxId,
        string IdentificadorTransacao,
        DateTime Data,
        decimal Valor,
        string Tipo,
        string? Descricao
    ) : IRequest<CommandResult>;

    public record DeletarImportacaoArquivoOfxCommand(Guid Id) : IRequest<CommandResult>;

    public record ConciliarTransacaoOfxContasAReceberCommand(
        Guid ImportacacaoArquivoOfxId,
        Guid TransacaoId,
        Guid ContasAReceberId
    ) : IRequest<CommandResult>;

    public record ConciliarTransacaoOfxContasAPagarCommand(
        Guid ImportacacaoArquivoOfxId,
        Guid TransacaoId,
        Guid ContasAPagarId
    ) : IRequest<CommandResult>;

    public record EstornarConciliacaoTransacaoOfxCReceberCommand(Guid TransacaoId) : IRequest<CommandResult>;

    public record EstornarConciliacaoTransacaoOfxCPagarCommand(Guid TransacaoId) : IRequest<CommandResult>;
}
