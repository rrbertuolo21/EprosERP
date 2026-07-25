using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Application.Models;
using MediatR;

namespace Epros.Modules.Financeiro.Application.Commands
{
    // ----- Centro de Custo -----
    public record CriarCentroCustoCommand(string Codigo, string Descricao, Guid? PaiId) : IRequest<CommandResult>;
    public record AtualizarCentroCustoCommand(Guid Id, string Codigo, string Descricao, Guid? PaiId) : IRequest<CommandResult>;
    public record DefinirEstadoCentroCustoCommand(Guid Id, EEstadoCentroCusto Estado) : IRequest<CommandResult>;
    public record DeletarCentroCustoCommand(Guid Id) : IRequest<CommandResult>;

    // ----- Alocação a Centro de Custo -----
    public record AlocarTituloCentroCustoCommand(Guid TituloId, ETipoTituloAlocacao? TipoTitulo, Guid CentroCustoId, decimal Percentual, decimal? ValorRateado) : IRequest<CommandResult>;
    public record RemoverAlocacaoCentroCustoCommand(Guid Id) : IRequest<CommandResult>;

    // ----- Dimensão Analítica -----
    public record CriarDimensaoAnaliticaCommand(string Tipo, string Valor) : IRequest<CommandResult>;
    public record VincularAlocacaoDimensaoCommand(Guid AlocacaoCentroCustoId, Guid DimensaoAnaliticaId) : IRequest<CommandResult>;
}
