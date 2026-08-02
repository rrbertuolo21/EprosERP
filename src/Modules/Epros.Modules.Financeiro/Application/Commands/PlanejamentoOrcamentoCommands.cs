using System;
using System.Collections.Generic;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Application.Models;
using MediatR;

namespace Epros.Modules.Financeiro.Application.Commands
{
    // ----- Versão Orçamentária -----
    public record LinhaOrcamentoInput(Guid ContaId, Guid? CentroCustoId, string Periodo, decimal ValorOrcado);
    public record CriarVersaoOrcamentariaCommand(string? Nome, DateTime? PeriodoInicio, DateTime? PeriodoFim, IReadOnlyList<LinhaOrcamentoInput> Linhas) : IRequest<CommandResult>;
    public record AprovarVersaoOrcamentariaCommand(Guid Id) : IRequest<CommandResult>;
    public record AtivarVersaoOrcamentariaCommand(Guid Id) : IRequest<CommandResult>;
    public record EncerrarVersaoOrcamentariaCommand(Guid Id) : IRequest<CommandResult>;
    public record RegistrarRealizadoLinhaCommand(Guid LinhaId, decimal ValorRealizado) : IRequest<CommandResult>;

    // ----- Período Orçamentário e Budget -----
    public record CriarPeriodoOrcamentarioCommand(DateTime DataInicio, DateTime DataFim) : IRequest<CommandResult>;
    public record AprovarPeriodoOrcamentarioCommand(Guid Id) : IRequest<CommandResult>;
    public record AtivarPeriodoOrcamentarioCommand(Guid Id) : IRequest<CommandResult>;
    public record EncerrarPeriodoOrcamentarioCommand(Guid Id) : IRequest<CommandResult>;

    public record CriarBudgetCommand(Guid PeriodoId, string Tipo, decimal? Valor) : IRequest<CommandResult>;
    public record AprovarBudgetCommand(Guid Id) : IRequest<CommandResult>;
    public record AtivarBudgetCommand(Guid Id) : IRequest<CommandResult>;
    public record EncerrarBudgetCommand(Guid Id) : IRequest<CommandResult>;
    public record ExcluirBudgetCommand(Guid Id) : IRequest<CommandResult>;
    public record AlocarBudgetCommand(Guid BudgetId, Guid ContaId, decimal? ValorAlocado, bool AutoAprovar) : IRequest<CommandResult>;

    // ----- Orçamento comercial -----
    public record OrcamentoComercialItemInput(Guid ProdutoId, decimal? Quantidade, decimal? ValorUnitario, decimal? TaxaDesconto);
    public record CriarOrcamentoComercialCommand(
        Guid VendedorId, Guid TransportadoraId, Guid ClienteId, Guid CondicaoPagamentoId,
        string Tipo, string Codigo, string TipoFrete, string Observacao, string StatusPedido,
        DateTime? DataCadastro, DateTime? DataEntrega, DateTime? Validade,
        decimal? ValorFrete, decimal? TaxaComissao, decimal? TaxaDesconto,
        IReadOnlyList<OrcamentoComercialItemInput> Itens) : IRequest<CommandResult>;
    public record AlterarOrcamentoComercialCommand(
        Guid Id, Guid VendedorId, Guid TransportadoraId, Guid ClienteId, Guid CondicaoPagamentoId,
        string Tipo, string Codigo, string TipoFrete, string Observacao, string StatusPedido,
        DateTime? DataEntrega, DateTime? Validade,
        decimal? ValorFrete, decimal? TaxaComissao, decimal? TaxaDesconto,
        IReadOnlyList<OrcamentoComercialItemInput> Itens) : IRequest<CommandResult>;
    public record ExcluirOrcamentoComercialCommand(Guid Id) : IRequest<CommandResult>;

    // ----- Metas -----
    public record CriarMetaCategoriaCommand(string Nome, string Codigo, EEscopoMeta Escopo) : IRequest<CommandResult>;
    public record CriarMetaCommand(Guid CategoriaId, string Tipo, string Prioridade, DateTime DataInicio, DateTime DataAlvo) : IRequest<CommandResult>;
    public record AtivarMetaCommand(Guid Id) : IRequest<CommandResult>;
    public record RegistrarContribuicaoMetaCommand(Guid MetaId, decimal Valor, string Tipo, DateTime? Data) : IRequest<CommandResult>;
    public record RegistrarTrackingMetaCommand(Guid MetaId, decimal Percentual, string StatusProgresso, DateTime? Data) : IRequest<CommandResult>;
    public record CriarMilestoneMetaCommand(Guid MetaId, string? Descricao) : IRequest<CommandResult>;
    public record ConcluirMilestoneMetaCommand(Guid MilestoneId) : IRequest<CommandResult>;
}
