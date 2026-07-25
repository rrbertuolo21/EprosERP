using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-WFM, tabela rh_wfm_dados_pagamento). Fidelidade campo a campo.</summary>
    public partial class WfmDadosPagamento : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public string? PagamentoForma { get; private set; }
        public string? PagamentoBanco { get; private set; }
        public string? PagamentoAgencia { get; private set; }
        public string? PagamentoAgenciaDigito { get; private set; }
        public string? PagamentoConta { get; private set; }
        public string? PagamentoContaDigito { get; private set; }
        public string? TitularConta { get; private set; }
        public string? CodigoBancario { get; private set; }

        protected WfmDadosPagamento() { } // EF Core

        public WfmDadosPagamento(
            Guid colaboradorId,
            string? pagamentoForma,
            string? pagamentoBanco,
            string? pagamentoAgencia,
            string? pagamentoAgenciaDigito,
            string? pagamentoConta,
            string? pagamentoContaDigito,
            string? titularConta,
            string? codigoBancario,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            PagamentoForma = pagamentoForma;
            PagamentoBanco = pagamentoBanco;
            PagamentoAgencia = pagamentoAgencia;
            PagamentoAgenciaDigito = pagamentoAgenciaDigito;
            PagamentoConta = pagamentoConta;
            PagamentoContaDigito = pagamentoContaDigito;
            TitularConta = titularConta;
            CodigoBancario = codigoBancario;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<WfmDadosPagamento>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
