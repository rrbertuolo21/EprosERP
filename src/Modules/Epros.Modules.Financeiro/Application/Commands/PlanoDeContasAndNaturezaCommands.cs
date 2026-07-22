using System;
using System.Collections.Generic;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Enums;
using MediatR;

namespace Epros.Modules.Financeiro.Application.Commands
{
    // ----- Plano de Contas Financeiro -----
    public record CriarPlanoDeContasFinanceiroCommand(
        Guid? ConfiguracaoCodigoNaturezaFinanceiraRecebimentoId,
        Guid? ConfiguracaoCodigoNaturezaFinanceiraPagamentoId,
        string Descricao,
        string Mascara,
        IReadOnlyList<Guid>? EmpresaIds
    ) : IRequest<CommandResult>;

    public record AtualizarPlanoDeContasFinanceiroCommand(
        Guid Id,
        Guid? ConfiguracaoCodigoNaturezaFinanceiraRecebimentoId,
        Guid? ConfiguracaoCodigoNaturezaFinanceiraPagamentoId,
        string Descricao,
        string Mascara,
        IReadOnlyList<Guid>? EmpresaIds
    ) : IRequest<CommandResult>;

    public record DeletarPlanoDeContasFinanceiroCommand(Guid Id) : IRequest<CommandResult>;

    // ----- Plano de Contas Financeiro Item -----
    public record CriarPlanoDeContasFinanceiroItemCommand(
        Guid PlanoDeContasFinanceiroId,
        string Codigo,
        string Descricao,
        ETipoDetalhamento TipoDetalhamento,
        bool MovimentaCaixa
    ) : IRequest<CommandResult>;

    public record AtualizarPlanoDeContasFinanceiroItemCommand(
        Guid Id,
        string Codigo,
        string Descricao,
        ETipoDetalhamento TipoDetalhamento,
        bool MovimentaCaixa
    ) : IRequest<CommandResult>;

    public record DeletarPlanoDeContasFinanceiroItemCommand(Guid Id) : IRequest<CommandResult>;

    // ----- Configuracao Codigo Natureza Financeira -----
    public record CriarConfiguracaoCodigoNaturezaFinanceiraCommand(
        Guid EmpresaId,
        string Descricao,
        ETipoConfiguracaoNatureza TipoConfiguracaoNatureza,
        Guid? ItemPlanoDeContasFinanceiroDinheiroId,
        Guid? ItemPlanoDeContasFinanceiroCartaoChequeId,
        Guid? ItemPlanoDeContasFinanceiroCartaoCreditoId,
        Guid? ItemPlanoDeContasFinanceiroCartaoDebitoId,
        Guid? ItemPlanoDeContasFinanceiroCartaoDaLojaId,
        Guid? ItemPlanoDeContasFinanceiroValeAlimentacaoId,
        Guid? ItemPlanoDeContasFinanceiroValeRefeicaoId,
        Guid? ItemPlanoDeContasFinanceiroValePresenteId,
        Guid? ItemPlanoDeContasFinanceiroValeCombustivelId,
        Guid? ItemPlanoDeContasFinanceiroDuplicataMercantilId,
        Guid? ItemPlanoDeContasFinanceiroBoletoBancarioId,
        Guid? ItemPlanoDeContasFinanceiroDepositoBancarioId,
        Guid? ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamicoId,
        Guid? ItemPlanoDeContasFinanceiroTransferenciaBancariaId,
        Guid? ItemPlanoDeContasFinanceiroProgramaDeFidelidadeId,
        Guid? ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstaticoId,
        Guid? ItemPlanoDeContasFinanceiroCreditoEmLojaId,
        Guid? ItemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformadoId,
        Guid? ItemPlanoDeContasFinanceiroOutrosId,
        Guid? ItemPlanoDeContasFinanceiroDescontoId,
        Guid? ItemPlanoDeContasFinanceiroAcrescimoId,
        Guid? ItemPlanoDeContasFinanceiroJurosId,
        Guid? ItemPlanoDeContasFinanceiroMultaId,
        Guid? ItemPlanoDeContasFinanceiroTrocoId
    ) : IRequest<CommandResult>;

    public record AtualizarConfiguracaoCodigoNaturezaFinanceiraCommand(
        Guid Id,
        Guid EmpresaId,
        string Descricao,
        ETipoConfiguracaoNatureza TipoConfiguracaoNatureza,
        Guid? ItemPlanoDeContasFinanceiroDinheiroId,
        Guid? ItemPlanoDeContasFinanceiroCartaoChequeId,
        Guid? ItemPlanoDeContasFinanceiroCartaoCreditoId,
        Guid? ItemPlanoDeContasFinanceiroCartaoDebitoId,
        Guid? ItemPlanoDeContasFinanceiroCartaoDaLojaId,
        Guid? ItemPlanoDeContasFinanceiroValeAlimentacaoId,
        Guid? ItemPlanoDeContasFinanceiroValeRefeicaoId,
        Guid? ItemPlanoDeContasFinanceiroValePresenteId,
        Guid? ItemPlanoDeContasFinanceiroValeCombustivelId,
        Guid? ItemPlanoDeContasFinanceiroDuplicataMercantilId,
        Guid? ItemPlanoDeContasFinanceiroBoletoBancarioId,
        Guid? ItemPlanoDeContasFinanceiroDepositoBancarioId,
        Guid? ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamicoId,
        Guid? ItemPlanoDeContasFinanceiroTransferenciaBancariaId,
        Guid? ItemPlanoDeContasFinanceiroProgramaDeFidelidadeId,
        Guid? ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstaticoId,
        Guid? ItemPlanoDeContasFinanceiroCreditoEmLojaId,
        Guid? ItemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformadoId,
        Guid? ItemPlanoDeContasFinanceiroOutrosId,
        Guid? ItemPlanoDeContasFinanceiroDescontoId,
        Guid? ItemPlanoDeContasFinanceiroAcrescimoId,
        Guid? ItemPlanoDeContasFinanceiroJurosId,
        Guid? ItemPlanoDeContasFinanceiroMultaId,
        Guid? ItemPlanoDeContasFinanceiroTrocoId
    ) : IRequest<CommandResult>;

    public record DeletarConfiguracaoCodigoNaturezaFinanceiraCommand(Guid Id) : IRequest<CommandResult>;
}
