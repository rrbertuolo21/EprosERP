using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Referência funcional do lançamento financeiro gerado pela fatura de serviço
    /// (ven_servico_lancamento_financeiro_ref). Fonte: EF §12 e §19.4.
    /// O detalhamento contábil/contas a receber é do Financeiro; aqui fica apenas a referência
    /// de integração (o lançamento efetivo é feito no Financeiro via evento/Outbox).
    /// </summary>
    public class ServicoLancamentoFinanceiroRef : EntidadeSaaSBase
    {
        public Guid FaturaId { get; private set; }
        public string NumeroFatura { get; private set; } = string.Empty;
        public ETipoLancamentoServicoFinanceiro TipoLancamento { get; private set; }
        public Guid? ContaId { get; private set; }
        public Guid? ClienteId { get; private set; }
        public decimal Valor { get; private set; }
        public EStatusIntegracaoServico StatusIntegracao { get; private set; } = EStatusIntegracaoServico.Pendente;
        public string? MensagemIntegracao { get; private set; }

        protected ServicoLancamentoFinanceiroRef() { } // EF Core

        public ServicoLancamentoFinanceiroRef(
            Guid faturaId,
            string numeroFatura,
            ETipoLancamentoServicoFinanceiro tipoLancamento,
            Guid? contaId,
            Guid? clienteId,
            decimal valor,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            FaturaId = faturaId;
            NumeroFatura = numeroFatura;
            TipoLancamento = tipoLancamento;
            ContaId = contaId;
            ClienteId = clienteId;
            Valor = valor;
            StatusIntegracao = EStatusIntegracaoServico.Pendente;

            AddNotifications(new Contract<ServicoLancamentoFinanceiroRef>()
                .Requires()
                .AreNotEquals(faturaId, Guid.Empty, nameof(FaturaId), "A fatura de origem é obrigatória. [Origem: ServicoLancamentoFinanceiroRef]")
                .IsNotNullOrEmpty(numeroFatura, nameof(NumeroFatura), "O número da fatura de referência é obrigatório. [Origem: ServicoLancamentoFinanceiroRef]"));
        }

        public void MarcarIntegrado(string alteradoPor)
        {
            StatusIntegracao = EStatusIntegracaoServico.Integrado;
            MarcarAlterado(alteradoPor);
        }

        public void MarcarEstornado(string alteradoPor)
        {
            StatusIntegracao = EStatusIntegracaoServico.Estornado;
            MarcarAlterado(alteradoPor);
        }

        public void MarcarErro(string mensagem, string alteradoPor)
        {
            StatusIntegracao = EStatusIntegracaoServico.Erro;
            MensagemIntegracao = mensagem;
            MarcarAlterado(alteradoPor);
        }
    }
}
