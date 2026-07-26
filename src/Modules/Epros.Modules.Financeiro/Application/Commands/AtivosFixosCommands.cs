using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Application.Models;
using MediatR;

namespace Epros.Modules.Financeiro.Application.Commands
{
    // ----- Grupo de Bem -----
    public record CriarGrupoBemCommand(string? Codigo, string Nome, Guid? ContaAtivoId, Guid? ContaDepreciacaoId, Guid? ContaBaixaId) : IRequest<CommandResult>;
    public record AtualizarGrupoBemCommand(Guid Id, string? Codigo, string Nome, Guid? ContaAtivoId, Guid? ContaDepreciacaoId, Guid? ContaBaixaId) : IRequest<CommandResult>;

    // ----- Ativo Fixo -----
    public record CriarAtivoFixoCommand(
        string Descricao,
        decimal ValorCompra,
        DateTime DataAquisicao,
        Guid? GrupoBemId,
        Guid? TipoAquisicaoId,
        Guid? EstadoConservacaoId,
        Guid? SetorId,
        Guid? FornecedorId,
        Guid? ColaboradorId,
        string? NumeroNb,
        string? Nome,
        string? NumeroSerie,
        string? Funcao,
        string? NumeroNotaFiscal,
        string? ChaveNfe,
        decimal? ValorOriginal,
        DateTime? VencimentoGarantia,
        bool Deprecia,
        ETipoDepreciacaoAtivo? TipoDepreciacao,
        decimal? TaxaAnual,
        decimal? TaxaMensal
    ) : IRequest<CommandResult>;

    public record AtualizarAtivoFixoCommand(
        Guid Id,
        string Descricao,
        decimal ValorCompra,
        DateTime DataAquisicao,
        Guid? GrupoBemId,
        Guid? SetorId,
        Guid? FornecedorId,
        Guid? ColaboradorId,
        string? NumeroNb,
        string? Nome,
        string? NumeroSerie,
        string? Funcao,
        bool Deprecia,
        ETipoDepreciacaoAtivo? TipoDepreciacao,
        decimal? TaxaAnual,
        decimal? TaxaMensal
    ) : IRequest<CommandResult>;

    public record BaixarAtivoFixoCommand(Guid Id, DateTime DataBaixa, decimal? ValorBaixa, string? Observacao) : IRequest<CommandResult>;

    // ----- Depreciação -----
    public record RegistrarDepreciacaoMensalCommand(Guid AtivoId, string Competencia, decimal Valor, string? MetodoDepreciacao, decimal? TaxaAplicada) : IRequest<CommandResult>;

    // ----- Movimentação (vistoria/inventário) -----
    public record RegistrarMovimentacaoAtivoCommand(Guid AtivoId, ETipoMovimentacaoAtivo TipoMovimentacao, DateTime DataMovimentacao, decimal? Valor, string? Observacao, Guid? UsuarioId) : IRequest<CommandResult>;
}
