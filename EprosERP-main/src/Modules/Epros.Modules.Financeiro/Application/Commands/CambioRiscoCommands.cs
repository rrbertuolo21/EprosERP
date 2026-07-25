using System;
using System.Collections.Generic;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Application.Models;
using MediatR;

namespace Epros.Modules.Financeiro.Application.Commands
{
    // ----- Moeda -----
    public record CriarMoedaCommand(string CodigoIso, string Simbolo, string? Nome) : IRequest<CommandResult>;
    public record AtualizarMoedaCommand(Guid Id, string CodigoIso, string Simbolo, string? Nome) : IRequest<CommandResult>;
    public record DeletarMoedaCommand(Guid Id) : IRequest<CommandResult>;

    // ----- Taxa de Câmbio -----
    public record RegistrarTaxaCambioCommand(
        Guid MoedaId,
        DateTime DataTaxa,
        decimal TaxaCompra,
        decimal TaxaVenda,
        EOrigemTaxaCambio? OrigemTaxa,
        string? Observacao
    ) : IRequest<CommandResult>;

    // ----- Exposição Cambial -----
    public record CriarExposicaoCambialCommand(
        Guid MoedaId,
        decimal ValorExposto,
        DateTime? DataReferencia,
        string? OrigemExposicao,
        string? EntidadeOrigemTipo,
        Guid? EntidadeOrigemId,
        Guid? TaxaReferenciaId,
        decimal? ValorMoedaBase
    ) : IRequest<CommandResult>;

    public record MarcarExposicaoHedgeadaCommand(Guid Id) : IRequest<CommandResult>;
    public record EncerrarExposicaoCambialCommand(Guid Id) : IRequest<CommandResult>;

    // ----- Reavaliação de Títulos -----
    public record ReavaliacaoItemInput(Guid MoedaId, string? TituloTipo, Guid? TituloId, Guid TaxaCambioId, decimal ValorOriginalMoeda, decimal ValorReavaliadoBase);

    public record CriarReavaliacaoTituloCommand(
        DateTime DataReavaliacao,
        string? Observacao,
        IReadOnlyList<ReavaliacaoItemInput> Itens
    ) : IRequest<CommandResult>;

    public record AprovarReavaliacaoTituloCommand(Guid Id) : IRequest<CommandResult>;
    public record ContabilizarReavaliacaoTituloCommand(Guid Id) : IRequest<CommandResult>;
    public record CancelarReavaliacaoTituloCommand(Guid Id) : IRequest<CommandResult>;
}
