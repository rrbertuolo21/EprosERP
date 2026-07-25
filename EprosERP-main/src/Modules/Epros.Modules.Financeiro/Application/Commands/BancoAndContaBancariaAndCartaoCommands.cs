using System;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Enums;
using MediatR;

namespace Epros.Modules.Financeiro.Application.Commands
{
    // Banco
    public record CriarBancoCommand(
        string Codigo,
        string Descricao
    ) : IRequest<CommandResult>;

    public record AtualizarBancoCommand(
        Guid Id,
        string Codigo,
        string Descricao
    ) : IRequest<CommandResult>;

    public record DeletarBancoCommand(Guid Id) : IRequest<CommandResult>;

    // Conta Bancaria
    public record CriarContaBancariaCommand(
        Guid EmpresaId,
        Guid BancoId,
        ETipoContaBancaria TipoContaBancaria,
        string Apelido,
        string Titular,
        string Agencia,
        string Conta,
        string? Gerente,
        string? FoneGerente,
        string? Detalhe,
        string? DigitoAgencia,
        DateTime? DataEncerramento
    ) : IRequest<CommandResult>;

    public record AtualizarContaBancariaCommand(
        Guid Id,
        Guid EmpresaId,
        Guid BancoId,
        ETipoContaBancaria TipoContaBancaria,
        string Apelido,
        string Titular,
        string Agencia,
        string Conta,
        string? Gerente,
        string? FoneGerente,
        string? Detalhe,
        string? DigitoAgencia,
        DateTime? DataEncerramento
    ) : IRequest<CommandResult>;

    public record DeletarContaBancariaCommand(Guid Id) : IRequest<CommandResult>;

    // Cartao de Credito
    public record CriarCartaoDeCreditoCommand(
        Guid ContaBancariaId,
        string Apelido,
        string Titular,
        EBandeiraCartao BandeiraCartao,
        string? Observacao
    ) : IRequest<CommandResult>;

    public record AtualizarCartaoDeCreditoCommand(
        Guid Id,
        Guid ContaBancariaId,
        string Apelido,
        string Titular,
        EBandeiraCartao BandeiraCartao,
        string? Observacao
    ) : IRequest<CommandResult>;

    public record DeletarCartaoDeCreditoCommand(Guid Id) : IRequest<CommandResult>;

    // Cartao de Credito Fatura
    public record LancarFaturaCartaoCommand(
        Guid CartaoDeCreditoId,
        DateTime DataLancamento,
        DateTime DataVencimento,
        decimal Valor,
        bool Pago
    ) : IRequest<CommandResult>;

    public record BaixarFaturaCartaoCommand(
        Guid FaturaId
    ) : IRequest<CommandResult>;

    // Reconciliação OFX
    public record ProcessarExtratoOfxCommand(
        Guid ContaBancariaId,
        string OfxConteudo
    ) : IRequest<CommandResult>;
}
