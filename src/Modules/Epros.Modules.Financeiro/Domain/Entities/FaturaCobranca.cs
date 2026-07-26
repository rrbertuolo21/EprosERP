using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Fatura de cobrança (EF FIN-SF §7.3 / §11 sf_fatura_cobranca).
    /// Regras-chave: RSF-004 (nasce Pendente), RSF-005 (marca Vencida), RSF-006 (Baixada não volta),
    /// RSF-013/014 (elegibilidade e marcação de remessa), RSF-023 (boleto só se não baixada).
    /// </summary>
    public class FaturaCobranca : EntidadeSaaSBase
    {
        public Guid SacadoId { get; private set; }
        public Guid? GrupoRecorrenciaId { get; private set; }
        public string? Referencia { get; private set; }
        public string? NumeroDocumento { get; private set; }
        public DateTime Data { get; private set; }
        public DateTime DataVencimento { get; private set; }
        public decimal Valor { get; private set; }
        public string? Email { get; private set; }
        public ETipoFaturaCobranca TipoFatura { get; private set; }
        public ESituacaoFaturaCobranca Situacao { get; private set; } = ESituacaoFaturaCobranca.Pendente;
        public bool Remetida { get; private set; }
        public long? NossoNumero { get; private set; }
        public Guid? BancoId { get; private set; }
        public DateTime? DataBaixa { get; private set; }
        public decimal? ValorRecebido { get; private set; }

        public Sacado Sacado { get; private set; } = null!;

        protected FaturaCobranca() { } // EF Core

        public FaturaCobranca(
            Guid sacadoId, Guid? grupoRecorrenciaId, string? referencia, string? numeroDocumento,
            DateTime data, DateTime dataVencimento, decimal valor, string? email,
            ETipoFaturaCobranca tipoFatura, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            SacadoId = sacadoId;
            GrupoRecorrenciaId = grupoRecorrenciaId;
            Referencia = referencia;
            NumeroDocumento = numeroDocumento;
            Data = data;
            DataVencimento = dataVencimento;
            Valor = valor;
            Email = email;
            TipoFatura = tipoFatura;
            Situacao = ESituacaoFaturaCobranca.Pendente; // RSF-004
            Validar();
        }

        public bool ElegivelRemessa => Situacao == ESituacaoFaturaCobranca.Pendente && !Remetida; // RSF-013
        public bool ElegivelBoleto => Situacao != ESituacaoFaturaCobranca.Baixada; // RSF-023

        /// <summary>RSF-005: fatura pendente vencida (vencimento anterior à data corrente) passa a Vencida.</summary>
        public void AtualizarVencimento(DateTime dataCorrente, string usuario)
        {
            if (Situacao == ESituacaoFaturaCobranca.Pendente && DataVencimento.Date < dataCorrente.Date)
            {
                Situacao = ESituacaoFaturaCobranca.Vencida;
                MarcarAlterado(usuario);
            }
        }

        /// <summary>RSF-014: remessa gerada marca a fatura como remetida.</summary>
        public void MarcarRemetida(string usuario)
        {
            Remetida = true;
            MarcarAlterado(usuario);
        }

        /// <summary>RSF-024: emissão de boleto persiste identificação bancária na fatura.</summary>
        public void AtribuirBoleto(long nossoNumero, Guid bancoId, string usuario)
        {
            NossoNumero = nossoNumero;
            BancoId = bancoId;
            MarcarAlterado(usuario);
        }

        /// <summary>RSF-007: baixa por retorno. RSF-006: Baixada não retorna a pendente por rotina automática.</summary>
        public void Baixar(DateTime dataBaixa, decimal valorRecebido, string usuario)
        {
            if (Situacao == ESituacaoFaturaCobranca.Baixada)
            {
                AddNotification(nameof(Situacao), "Fatura já baixada.");
                return;
            }
            Situacao = ESituacaoFaturaCobranca.Baixada;
            DataBaixa = dataBaixa;
            ValorRecebido = valorRecebido;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<FaturaCobranca>()
                .Requires()
                .AreNotEquals(SacadoId, Guid.Empty, nameof(SacadoId), "O sacado da fatura é obrigatório.") // RSF-020
                .IsGreaterThan(Valor, 0m, nameof(Valor), "O valor da fatura deve ser maior que zero.")
            );
        }
    }
}
