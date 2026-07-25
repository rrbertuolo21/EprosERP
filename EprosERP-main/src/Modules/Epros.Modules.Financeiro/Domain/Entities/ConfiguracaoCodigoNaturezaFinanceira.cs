using System;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    public class ConfiguracaoCodigoNaturezaFinanceira : EntidadeSaaSBase
    {
        /// <summary>Sequência legada (SequenciaTenantId) — somente exibição/UX. Não substitui o Id (Guid).</summary>
        public long? SequenciaExibicao { get; private set; }

        /// <summary>Empresa dona da configuração (referência ao módulo Plataforma por Guid FK).</summary>
        public Guid EmpresaId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public ETipoConfiguracaoNatureza TipoConfiguracaoNatureza { get; private set; }

        // Mapeamentos de Plano de Contas para formas de pagamento
        public Guid? ItemPlanoDeContasFinanceiroDinheiroId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroCartaoChequeId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroCartaoCreditoId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroCartaoDebitoId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroCartaoDaLojaId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroValeAlimentacaoId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroValeRefeicaoId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroValePresenteId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroValeCombustivelId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroDuplicataMercantilId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroBoletoBancarioId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroDepositoBancarioId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamicoId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroTransferenciaBancariaId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroProgramaDeFidelidadeId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstaticoId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroCreditoEmLojaId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformadoId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroOutrosId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroDescontoId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroAcrescimoId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroJurosId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroMultaId { get; private set; }
        public Guid? ItemPlanoDeContasFinanceiroTrocoId { get; private set; }

        // Propriedades de navegação EF
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroDinheiro { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroCartaoCheque { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroCartaoCredito { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroCartaoDebito { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroCartaoDaLoja { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroValeAlimentacao { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroValeRefeicao { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroValePresente { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroValeCombustivel { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroDuplicataMercantil { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroBoletoBancario { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroDepositoBancario { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamico { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroTransferenciaBancaria { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroProgramaDeFidelidade { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstatico { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroCreditoEmLoja { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformado { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroOutros { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroDesconto { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroAcrescimo { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroJuros { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroMulta { get; private set; }
        public PlanoDeContasFinanceiroItem? ItemPlanoDeContasFinanceiroTroco { get; private set; }

        protected ConfiguracaoCodigoNaturezaFinanceira() { } // EF Core

        public ConfiguracaoCodigoNaturezaFinanceira(
            Guid empresaId,
            string descricao,
            ETipoConfiguracaoNatureza tipoConfiguracaoNatureza,
            Guid? itemPlanoDeContasFinanceiroDinheiroId,
            Guid? itemPlanoDeContasFinanceiroCartaoChequeId,
            Guid? itemPlanoDeContasFinanceiroCartaoCreditoId,
            Guid? itemPlanoDeContasFinanceiroCartaoDebitoId,
            Guid? itemPlanoDeContasFinanceiroCartaoDaLojaId,
            Guid? itemPlanoDeContasFinanceiroValeAlimentacaoId,
            Guid? itemPlanoDeContasFinanceiroValeRefeicaoId,
            Guid? itemPlanoDeContasFinanceiroValePresenteId,
            Guid? itemPlanoDeContasFinanceiroValeCombustivelId,
            Guid? itemPlanoDeContasFinanceiroDuplicataMercantilId,
            Guid? itemPlanoDeContasFinanceiroBoletoBancarioId,
            Guid? itemPlanoDeContasFinanceiroDepositoBancarioId,
            Guid? itemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamicoId,
            Guid? itemPlanoDeContasFinanceiroTransferenciaBancariaId,
            Guid? itemPlanoDeContasFinanceiroProgramaDeFidelidadeId,
            Guid? itemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstaticoId,
            Guid? itemPlanoDeContasFinanceiroCreditoEmLojaId,
            Guid? itemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformadoId,
            Guid? itemPlanoDeContasFinanceiroOutrosId,
            Guid? itemPlanoDeContasFinanceiroDescontoId,
            Guid? itemPlanoDeContasFinanceiroAcrescimoId,
            Guid? itemPlanoDeContasFinanceiroJurosId,
            Guid? itemPlanoDeContasFinanceiroMultaId,
            Guid? itemPlanoDeContasFinanceiroTrocoId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            Descricao = descricao;
            TipoConfiguracaoNatureza = tipoConfiguracaoNatureza;
            ItemPlanoDeContasFinanceiroDinheiroId = itemPlanoDeContasFinanceiroDinheiroId;
            ItemPlanoDeContasFinanceiroCartaoChequeId = itemPlanoDeContasFinanceiroCartaoChequeId;
            ItemPlanoDeContasFinanceiroCartaoCreditoId = itemPlanoDeContasFinanceiroCartaoCreditoId;
            ItemPlanoDeContasFinanceiroCartaoDebitoId = itemPlanoDeContasFinanceiroCartaoDebitoId;
            ItemPlanoDeContasFinanceiroCartaoDaLojaId = itemPlanoDeContasFinanceiroCartaoDaLojaId;
            ItemPlanoDeContasFinanceiroValeAlimentacaoId = itemPlanoDeContasFinanceiroValeAlimentacaoId;
            ItemPlanoDeContasFinanceiroValeRefeicaoId = itemPlanoDeContasFinanceiroValeRefeicaoId;
            ItemPlanoDeContasFinanceiroValePresenteId = itemPlanoDeContasFinanceiroValePresenteId;
            ItemPlanoDeContasFinanceiroValeCombustivelId = itemPlanoDeContasFinanceiroValeCombustivelId;
            ItemPlanoDeContasFinanceiroDuplicataMercantilId = itemPlanoDeContasFinanceiroDuplicataMercantilId;
            ItemPlanoDeContasFinanceiroBoletoBancarioId = itemPlanoDeContasFinanceiroBoletoBancarioId;
            ItemPlanoDeContasFinanceiroDepositoBancarioId = itemPlanoDeContasFinanceiroDepositoBancarioId;
            ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamicoId = itemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamicoId;
            ItemPlanoDeContasFinanceiroTransferenciaBancariaId = itemPlanoDeContasFinanceiroTransferenciaBancariaId;
            ItemPlanoDeContasFinanceiroProgramaDeFidelidadeId = itemPlanoDeContasFinanceiroProgramaDeFidelidadeId;
            ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstaticoId = itemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstaticoId;
            ItemPlanoDeContasFinanceiroCreditoEmLojaId = itemPlanoDeContasFinanceiroCreditoEmLojaId;
            ItemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformadoId = itemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformadoId;
            ItemPlanoDeContasFinanceiroOutrosId = itemPlanoDeContasFinanceiroOutrosId;
            ItemPlanoDeContasFinanceiroDescontoId = itemPlanoDeContasFinanceiroDescontoId;
            ItemPlanoDeContasFinanceiroAcrescimoId = itemPlanoDeContasFinanceiroAcrescimoId;
            ItemPlanoDeContasFinanceiroJurosId = itemPlanoDeContasFinanceiroJurosId;
            ItemPlanoDeContasFinanceiroMultaId = itemPlanoDeContasFinanceiroMultaId;
            ItemPlanoDeContasFinanceiroTrocoId = itemPlanoDeContasFinanceiroTrocoId;

            Validar();
        }

        public void Validar()
        {
            AddNotifications(new Contract<ConfiguracaoCodigoNaturezaFinanceira>()
                .Requires()
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descrição é obrigatória.")
                .IsLowerOrEqualsThan(Descricao?.Length ?? 0, 150, nameof(Descricao), "O campo Descricao deve ter no máximo 150 caracteres.")
            );
        }

        public void Alterar(
            Guid empresaId,
            string descricao,
            ETipoConfiguracaoNatureza tipoConfiguracaoNatureza,
            Guid? itemPlanoDeContasFinanceiroDinheiroId,
            Guid? itemPlanoDeContasFinanceiroCartaoChequeId,
            Guid? itemPlanoDeContasFinanceiroCartaoCreditoId,
            Guid? itemPlanoDeContasFinanceiroCartaoDebitoId,
            Guid? itemPlanoDeContasFinanceiroCartaoDaLojaId,
            Guid? itemPlanoDeContasFinanceiroValeAlimentacaoId,
            Guid? itemPlanoDeContasFinanceiroValeRefeicaoId,
            Guid? itemPlanoDeContasFinanceiroValePresenteId,
            Guid? itemPlanoDeContasFinanceiroValeCombustivelId,
            Guid? itemPlanoDeContasFinanceiroDuplicataMercantilId,
            Guid? itemPlanoDeContasFinanceiroBoletoBancarioId,
            Guid? itemPlanoDeContasFinanceiroDepositoBancarioId,
            Guid? itemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamicoId,
            Guid? itemPlanoDeContasFinanceiroTransferenciaBancariaId,
            Guid? itemPlanoDeContasFinanceiroProgramaDeFidelidadeId,
            Guid? itemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstaticoId,
            Guid? itemPlanoDeContasFinanceiroCreditoEmLojaId,
            Guid? itemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformadoId,
            Guid? itemPlanoDeContasFinanceiroOutrosId,
            Guid? itemPlanoDeContasFinanceiroDescontoId,
            Guid? itemPlanoDeContasFinanceiroAcrescimoId,
            Guid? itemPlanoDeContasFinanceiroJurosId,
            Guid? itemPlanoDeContasFinanceiroMultaId,
            Guid? itemPlanoDeContasFinanceiroTrocoId,
            string usuario)
        {
            EmpresaId = empresaId;
            Descricao = descricao;
            TipoConfiguracaoNatureza = tipoConfiguracaoNatureza;
            ItemPlanoDeContasFinanceiroDinheiroId = itemPlanoDeContasFinanceiroDinheiroId;
            ItemPlanoDeContasFinanceiroCartaoChequeId = itemPlanoDeContasFinanceiroCartaoChequeId;
            ItemPlanoDeContasFinanceiroCartaoCreditoId = itemPlanoDeContasFinanceiroCartaoCreditoId;
            ItemPlanoDeContasFinanceiroCartaoDebitoId = itemPlanoDeContasFinanceiroCartaoDebitoId;
            ItemPlanoDeContasFinanceiroCartaoDaLojaId = itemPlanoDeContasFinanceiroCartaoDaLojaId;
            ItemPlanoDeContasFinanceiroValeAlimentacaoId = itemPlanoDeContasFinanceiroValeAlimentacaoId;
            ItemPlanoDeContasFinanceiroValeRefeicaoId = itemPlanoDeContasFinanceiroValeRefeicaoId;
            ItemPlanoDeContasFinanceiroValePresenteId = itemPlanoDeContasFinanceiroValePresenteId;
            ItemPlanoDeContasFinanceiroValeCombustivelId = itemPlanoDeContasFinanceiroValeCombustivelId;
            ItemPlanoDeContasFinanceiroDuplicataMercantilId = itemPlanoDeContasFinanceiroDuplicataMercantilId;
            ItemPlanoDeContasFinanceiroBoletoBancarioId = itemPlanoDeContasFinanceiroBoletoBancarioId;
            ItemPlanoDeContasFinanceiroDepositoBancarioId = itemPlanoDeContasFinanceiroDepositoBancarioId;
            ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamicoId = itemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamicoId;
            ItemPlanoDeContasFinanceiroTransferenciaBancariaId = itemPlanoDeContasFinanceiroTransferenciaBancariaId;
            ItemPlanoDeContasFinanceiroProgramaDeFidelidadeId = itemPlanoDeContasFinanceiroProgramaDeFidelidadeId;
            ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstaticoId = itemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstaticoId;
            ItemPlanoDeContasFinanceiroCreditoEmLojaId = itemPlanoDeContasFinanceiroCreditoEmLojaId;
            ItemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformadoId = itemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformadoId;
            ItemPlanoDeContasFinanceiroOutrosId = itemPlanoDeContasFinanceiroOutrosId;
            ItemPlanoDeContasFinanceiroDescontoId = itemPlanoDeContasFinanceiroDescontoId;
            ItemPlanoDeContasFinanceiroAcrescimoId = itemPlanoDeContasFinanceiroAcrescimoId;
            ItemPlanoDeContasFinanceiroJurosId = itemPlanoDeContasFinanceiroJurosId;
            ItemPlanoDeContasFinanceiroMultaId = itemPlanoDeContasFinanceiroMultaId;
            ItemPlanoDeContasFinanceiroTrocoId = itemPlanoDeContasFinanceiroTrocoId;

            MarcarAlterado(usuario);
            Validar();
        }
    }
}
