using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Agregado de título a pagar (porte fiel do legado Financeiros/ContasAPagar).
    /// Mantém decomposição de valores (inicial x total), parcelas (itens) e situação.
    /// PessoaId referencia Pessoa do módulo Plataforma por Guid FK (sem navegação cruzada).
    /// </summary>
    public class ContasAPagar : EntidadeSaaSBase
    {
        /// <summary>Sequência legada (SequenciaTenantId) — somente exibição/UX. Não substitui o Id (Guid).</summary>
        public long? SequenciaExibicao { get; private set; }

        public Guid PessoaId { get; private set; }
        public Guid PlanoDeContasFinanceiroItemId { get; private set; }
        public Guid FatoGeradorFinanceiroId { get; private set; }
        public string? NomePessoa { get; private set; }
        public ESituacao Situacao { get; private set; } = ESituacao.Aberto;
        public DateTime DataVencimento { get; private set; }
        public DateTime DataEmissao { get; set; }
        public DateTime? DataBaixa { get; private set; }
        public string Documento { get; private set; } = string.Empty;
        public decimal ValorTitulo { get; private set; }
        public decimal ValorTotalDesconto { get; private set; }
        public decimal ValorTotalMulta { get; private set; }
        public decimal ValorTotalJuros { get; private set; }
        public decimal ValorTotalTroco { get; private set; }
        public decimal ValorTotalAcrescimo { get; private set; }
        public decimal ValorTotalPago { get; private set; }
        public decimal ValorTotalAPagarTitulo { get; private set; }

        public decimal ValorInicialDesconto { get; private set; }
        public decimal ValorInicialMulta { get; private set; }
        public decimal ValorInicialJuros { get; private set; }
        public decimal ValorInicialAcrescimo { get; private set; }
        public decimal ValorInicialAPagarTitulo { get; private set; }

        public int NumeroParcela { get; private set; }
        public string? Detalhamento { get; private set; }
        public string? JustificativaCancelamento { get; private set; }

        // EF (navegação intra-módulo)
        public PlanoDeContasFinanceiroItem PlanoDeContasFinanceiroItem { get; private set; } = null!;
        public FatoGeradorFinanceiro FatoGeradorFinanceiro { get; private set; } = null!;

        private readonly List<ContasAPagarItem> _contasAPagarItens = new();
        public ICollection<ContasAPagarItem> ContasAPagarItens => _contasAPagarItens;

        protected ContasAPagar() { } // EF Core

        public ContasAPagar(Guid pessoaId, Guid planoDeContasFinanceiroItemId, Guid fatoGeradorFinanceiroId,
                            string? nomePessoa, ESituacao situacao, DateTime dataVencimento, DateTime dataEmissao,
                            DateTime? dataBaixa, string documento, decimal valorTitulo, decimal valorInicialDesconto,
                            decimal valorInicialMulta, decimal valorInicialJuros, decimal valorInicialAcrescimo,
                            int numeroParcela, string? detalhamento, string? justificativaCancelamento,
                            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            PessoaId = pessoaId;
            PlanoDeContasFinanceiroItemId = planoDeContasFinanceiroItemId;
            FatoGeradorFinanceiroId = fatoGeradorFinanceiroId;
            NomePessoa = nomePessoa;
            Situacao = situacao;
            DataVencimento = dataVencimento;
            DataEmissao = dataEmissao;
            DataBaixa = dataBaixa;
            Documento = documento;
            ValorTitulo = valorTitulo;
            ValorInicialDesconto = valorInicialDesconto;
            ValorInicialMulta = valorInicialMulta;
            ValorInicialJuros = valorInicialJuros;
            ValorInicialAcrescimo = valorInicialAcrescimo;
            NumeroParcela = numeroParcela;
            Detalhamento = detalhamento;
            JustificativaCancelamento = justificativaCancelamento;
            CalcularTotais();
            Validar();
        }

        public void Validar()
        {
            AddNotifications(new Contract<ContasAPagar>()
                .Requires()
                .AreNotEquals(PlanoDeContasFinanceiroItemId, Guid.Empty, nameof(PlanoDeContasFinanceiroItemId), "O campo PlanoDeContasFinanceiroItemId é obrigatório [Origem: ContasAPagar]")
                .IsTrue(Enum.IsDefined(typeof(ESituacao), Situacao), nameof(Situacao), "Situacao não consta na lista [Origem: ContasAPagar]")
                .IsLowerOrEqualsThan((Documento ?? "").Length, 30, nameof(Documento), "O campo Documento deve ter no máximo 30 caracteres [Origem: ContasAPagar]")
                .IsLowerOrEqualsThan((Detalhamento ?? "").Length, 255, nameof(Detalhamento), "O campo Detalhamento deve ter no máximo 255 caracteres [Origem: ContasAPagar]")
            );

            if (Situacao == ESituacao.Aberto) DataBaixa = null;

            if (Situacao == ESituacao.Pago || Situacao == ESituacao.PagoParcial)
            {
                if (DataBaixa < DataEmissao) AddNotification("DataBaixa", "A Data da Baixa não pode ser menor que a Data da Emissão [Origem: ContasAPagar]");
            }

            if (ValorTotalAPagarTitulo > 0)
            {
                if (Situacao == ESituacao.Pago) AddNotification("Situacao", "Não pode ser selecionado como Pago, ainda consta valor a pagar [Origem: ContasAPagar]");
            }

            foreach (var item in _contasAPagarItens)
            {
                AddNotifications(item.Notifications);
            }

            if (Situacao == ESituacao.Cancelado)
            {
                AddNotifications(new Contract<ContasAPagar>()
                    .Requires()
                    .IsNotNull(JustificativaCancelamento, nameof(JustificativaCancelamento), "É obrigatório o preenchimento da justificativa de cancelamento [Origem: ContasAPagar]")
                    .IsLowerOrEqualsThan((JustificativaCancelamento ?? "").Length, 255, nameof(JustificativaCancelamento), "O campo Justificativa de Cancelamento deve ter no máximo 255 caracteres [Origem: ContasAPagar]")
                );
            }
        }

        public void Alterar(Guid pessoaId, Guid planoDeContasFinanceiroItemId, string? nomePessoa, ESituacao situacao,
                            DateTime dataVencimento, DateTime dataEmissao, DateTime? dataBaixa, string documento,
                            decimal valorTitulo, decimal valorInicialDesconto, decimal valorInicialMulta,
                            decimal valorInicialJuros, decimal valorInicialAcrescimo, int numeroParcela,
                            string? detalhamento, string? justificativaCancelamento, string alteradoPor)
        {
            PessoaId = pessoaId;
            PlanoDeContasFinanceiroItemId = planoDeContasFinanceiroItemId;
            NomePessoa = nomePessoa;
            Situacao = situacao;
            DataVencimento = dataVencimento;
            DataEmissao = dataEmissao;
            DataBaixa = dataBaixa;
            Documento = documento;
            ValorTitulo = valorTitulo;
            ValorInicialDesconto = valorInicialDesconto;
            ValorInicialMulta = valorInicialMulta;
            ValorInicialJuros = valorInicialJuros;
            ValorInicialAcrescimo = valorInicialAcrescimo;
            NumeroParcela = numeroParcela;
            Detalhamento = detalhamento;
            JustificativaCancelamento = justificativaCancelamento;
            MarcarAlterado(alteradoPor);
            CalcularTotais();
            Validar();
        }

        public void BaixarTituloConciliado(decimal valorPago)
        {
            if (Situacao == ESituacao.Aberto)
            {
                Situacao = ESituacao.Pago;
                ValorTotalPago = valorPago;
                DataBaixa = DateTime.UtcNow;
                ValorTotalAPagarTitulo = 0m;
            }
        }

        public void EstornarTituloConciliado()
        {
            if (Situacao == ESituacao.Pago)
            {
                Situacao = ESituacao.Aberto;
                ValorTotalPago = 0;
                DataBaixa = null;
            }
        }

        public void Cancelar(string? justificativaCancelamento, string alteradoPor)
        {
            Situacao = ESituacao.Cancelado;
            JustificativaCancelamento = justificativaCancelamento;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void DeletarComItens(string deletadoPor)
        {
            Deletar(deletadoPor);
            foreach (var item in _contasAPagarItens)
            {
                item.Deletar(deletadoPor);
            }
        }

        public void IncluirContasAPagarItem(Guid planoDeContasFinanceiroItemId, Guid? contaBancariaId,
                                            ETipoPagamento tipoPagamento, decimal valorParcela, decimal valorPago,
                                            decimal valorDesconto, decimal valorMulta, decimal valorJuros,
                                            decimal valorAcrescimo, DateTime dataPagamento, string criadoPor)
        {
            var item = new ContasAPagarItem(Id, planoDeContasFinanceiroItemId, contaBancariaId, tipoPagamento,
                valorParcela, valorPago, valorDesconto, valorMulta, valorJuros, valorAcrescimo, dataPagamento,
                TenantId, criadoPor);

            _contasAPagarItens.Add(item);

            if (item.Notifications.Any())
            {
                AddNotifications(item.Notifications);
            }

            CalcularTotais();
        }

        public void AlterarContasAPagarItem(Guid contasAPagarItemId, Guid planoDeContasFinanceiroItemId,
                                            Guid? contaBancariaId, ETipoPagamento tipoPagamento, decimal valorParcela,
                                            decimal valorPago, decimal valorDesconto, decimal valorMulta,
                                            decimal valorJuros, decimal valorAcrescimo, DateTime dataPagamento,
                                            string alteradoPor)
        {
            var localizar = _contasAPagarItens.FirstOrDefault(x => x.Id == contasAPagarItemId);

            if (localizar != null)
            {
                localizar.Alterar(planoDeContasFinanceiroItemId, contaBancariaId, tipoPagamento, valorParcela,
                    valorPago, valorDesconto, valorMulta, valorJuros, valorAcrescimo, dataPagamento, alteradoPor);

                if (localizar.Notifications.Any())
                {
                    AddNotifications(localizar.Notifications);
                }

                CalcularTotais();
            }
        }

        public void DeletarContasAPagarItem(Guid contasAPagarItemId, string deletadoPor)
        {
            var localizar = _contasAPagarItens.FirstOrDefault(x => x.Id == contasAPagarItemId);

            if (localizar != null)
            {
                localizar.Deletar(deletadoPor);
                CalcularTotais();
            }
        }

        public void CalcularTotais()
        {
            CalcularValorTotalMulta();
            CalcularValorTotalJuros();
            CalcularValorTotalAcrescimo();
            CalcularValorTotalDesconto();
            CalcularValorTotalPago();
            CalcularValorAPagarTitulo();
            CalcularValorTotalTroco();
        }

        public List<ContasAPagarItem> LocalizarContasAPagarItensNaoDeletados()
            => _contasAPagarItens.Where(x => x.DeletadoEm == null).ToList();

        public void CalcularValorTotalMulta()
        {
            decimal total = 0;
            foreach (var item in LocalizarContasAPagarItensNaoDeletados()) total += item.ValorMulta;
            ValorTotalMulta = ValorInicialMulta + total;
        }

        public void CalcularValorTotalJuros()
        {
            decimal total = 0;
            foreach (var item in LocalizarContasAPagarItensNaoDeletados()) total += item.ValorJuros;
            ValorTotalJuros = ValorInicialJuros + total;
        }

        public void CalcularValorTotalAcrescimo()
        {
            decimal total = 0;
            foreach (var item in LocalizarContasAPagarItensNaoDeletados()) total += item.ValorAcrescimo;
            ValorTotalAcrescimo = ValorInicialAcrescimo + total;
        }

        public void CalcularValorTotalDesconto()
        {
            decimal total = 0;
            foreach (var item in LocalizarContasAPagarItensNaoDeletados()) total += item.ValorDesconto;
            ValorTotalDesconto = ValorInicialDesconto + total;
        }

        public void CalcularValorTotalPago()
        {
            decimal total = 0;
            foreach (var item in LocalizarContasAPagarItensNaoDeletados()) total += item.ValorPago;
            ValorTotalPago = total;
        }

        public void CalcularValorAPagarTitulo()
        {
            ValorInicialAPagarTitulo = (ValorTitulo + ValorInicialMulta + ValorInicialJuros + ValorInicialAcrescimo) - ValorInicialDesconto;

            if (_contasAPagarItens.Any())
            {
                ValorTotalAPagarTitulo = ((ValorTitulo + ValorTotalMulta + ValorTotalJuros + ValorTotalAcrescimo) - ValorTotalDesconto) - (ValorTotalPago - ValorTotalTroco);
            }
            else
            {
                ValorTotalAPagarTitulo = ValorInicialAPagarTitulo;
            }
        }

        public void CalcularValorTotalTroco()
        {
            decimal total = 0;
            foreach (var item in LocalizarContasAPagarItensNaoDeletados()) total += item.ValorTroco;
            ValorTotalTroco = total;
        }
    }
}
