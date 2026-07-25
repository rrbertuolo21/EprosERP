using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>Cheque emitido ou recebido (EF FIN-TS §11.7 cheque). Pessoa/conta/caixa cross-module por Guid FK.</summary>
    public class Cheque : EntidadeSaaSBase
    {
        public Guid? EmpresaId { get; private set; }
        public int Tipo { get; private set; } // emitido/recebido
        public int TipoPessoa { get; private set; }
        public Guid? PessoaId { get; private set; }
        public DateTime Emissao { get; private set; }
        public DateTime Vencimento { get; private set; }
        public ESituacaoCheque Situacao { get; private set; } = ESituacaoCheque.Emitido;
        public decimal Valor { get; private set; }
        public Guid? ContaId { get; private set; }
        public Guid? CaixaId { get; private set; }

        protected Cheque() { } // EF Core

        public Cheque(Guid? empresaId, int tipo, int tipoPessoa, Guid? pessoaId, DateTime emissao, DateTime vencimento,
            decimal valor, Guid? contaId, Guid? caixaId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            Tipo = tipo;
            TipoPessoa = tipoPessoa;
            PessoaId = pessoaId;
            Emissao = emissao;
            Vencimento = vencimento;
            Valor = valor;
            ContaId = contaId;
            CaixaId = caixaId;
            Situacao = ESituacaoCheque.Emitido;
            Validar();
        }

        public void DefinirSituacao(ESituacaoCheque situacao, string usuario) { Situacao = situacao; MarcarAlterado(usuario); }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Cheque>()
                .Requires()
                .IsGreaterThan(Valor, 0, nameof(Valor), "O valor do cheque deve ser maior que zero [Origem: Cheque]")
                .IsGreaterThan(Vencimento, DateTime.MinValue, nameof(Vencimento), "O vencimento do cheque é obrigatório [Origem: Cheque]"));
        }
    }

    /// <summary>Caixa operacional com abertura/fechamento (EF FIN-TS §11.10 caixa_operacional).</summary>
    public class CaixaOperacional : EntidadeSaaSBase
    {
        public EStatusCaixaOperacional Status { get; private set; } = EStatusCaixaOperacional.Aberto;
        public Guid? LocalId { get; private set; }
        public Guid? UsuarioId { get; private set; }
        public decimal ValorInicial { get; private set; }
        public decimal? ValorFechamento { get; private set; }
        public decimal? TotalComprovantesCartao { get; private set; }
        public decimal? TotalCheques { get; private set; }
        public string? ObservacaoFechamento { get; private set; }
        public DateTime? DataFechamento { get; private set; }

        protected CaixaOperacional() { } // EF Core

        public CaixaOperacional(Guid? localId, Guid? usuarioId, decimal valorInicial, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            LocalId = localId;
            UsuarioId = usuarioId;
            ValorInicial = valorInicial;
            Status = EStatusCaixaOperacional.Aberto;
            Validar();
        }

        public void Fechar(decimal? valorFechamento, decimal? totalComprovantesCartao, decimal? totalCheques, string? observacao, string usuario)
        {
            if (Status == EStatusCaixaOperacional.Fechado)
            {
                AddNotification(nameof(Status), "Caixa já está fechado.");
                return;
            }
            Status = EStatusCaixaOperacional.Fechado;
            ValorFechamento = valorFechamento;
            TotalComprovantesCartao = totalComprovantesCartao;
            TotalCheques = totalCheques;
            ObservacaoFechamento = observacao;
            DataFechamento = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<CaixaOperacional>()
                .Requires()
                .IsGreaterOrEqualsThan(ValorInicial, 0, nameof(ValorInicial), "O valor inicial não pode ser negativo [Origem: CaixaOperacional]"));
        }
    }
}
