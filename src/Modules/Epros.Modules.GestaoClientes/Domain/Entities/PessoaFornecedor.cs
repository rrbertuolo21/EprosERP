using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Extensão de papel Fornecedor (CAD-PEM 12.6). 1:1 com Pessoa via PessoaId.</summary>
    public class PessoaFornecedor : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }
        public Guid? CompradorId { get; private set; }
        public Guid? GrupoFornecedorId { get; private set; }
        public bool? OptanteSimplesNacional { get; private set; }
        public string? Localizacao { get; private set; }
        public bool? SofreRetencao { get; private set; }
        public string? ChequeNominalA { get; private set; }
        public string? Observacao { get; private set; }
        public string? ContaRemetente { get; private set; }
        public int? PrazoMedioEntrega { get; private set; }
        public bool? GeraFaturamento { get; private set; }
        public int? NumDiasPrimeiroVencimento { get; private set; }
        public int? NumDiasIntervalo { get; private set; }
        public int? QuantidadeParcelas { get; private set; }
        public int? PayTermNumber { get; private set; }
        public ETipoPeriodoPagamento? PayTermType { get; private set; }
        public DateTime? UltimaCompra { get; private set; }

        protected PessoaFornecedor() { } // EF Core

        public PessoaFornecedor(
            Guid pessoaId,
            Guid? compradorId,
            Guid? grupoFornecedorId,
            bool? optanteSimplesNacional,
            string? localizacao,
            bool? sofreRetencao,
            string? chequeNominalA,
            string? observacao,
            string? contaRemetente,
            int? prazoMedioEntrega,
            bool? geraFaturamento,
            int? numDiasPrimeiroVencimento,
            int? numDiasIntervalo,
            int? quantidadeParcelas,
            int? payTermNumber,
            ETipoPeriodoPagamento? payTermType,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PessoaId = pessoaId;
            CompradorId = compradorId;
            GrupoFornecedorId = grupoFornecedorId;
            OptanteSimplesNacional = optanteSimplesNacional;
            Localizacao = localizacao;
            SofreRetencao = sofreRetencao;
            ChequeNominalA = chequeNominalA;
            Observacao = observacao;
            ContaRemetente = contaRemetente;
            PrazoMedioEntrega = prazoMedioEntrega;
            GeraFaturamento = geraFaturamento;
            NumDiasPrimeiroVencimento = numDiasPrimeiroVencimento;
            NumDiasIntervalo = numDiasIntervalo;
            QuantidadeParcelas = quantidadeParcelas;
            PayTermNumber = payTermNumber;
            PayTermType = payTermType;
            Validar();
        }

        public void Alterar(
            Guid? compradorId,
            Guid? grupoFornecedorId,
            bool? optanteSimplesNacional,
            string? localizacao,
            bool? sofreRetencao,
            string? chequeNominalA,
            string? observacao,
            string? contaRemetente,
            int? prazoMedioEntrega,
            bool? geraFaturamento,
            int? numDiasPrimeiroVencimento,
            int? numDiasIntervalo,
            int? quantidadeParcelas,
            int? payTermNumber,
            ETipoPeriodoPagamento? payTermType,
            string alteradoPor)
        {
            CompradorId = compradorId;
            GrupoFornecedorId = grupoFornecedorId;
            OptanteSimplesNacional = optanteSimplesNacional;
            Localizacao = localizacao;
            SofreRetencao = sofreRetencao;
            ChequeNominalA = chequeNominalA;
            Observacao = observacao;
            ContaRemetente = contaRemetente;
            PrazoMedioEntrega = prazoMedioEntrega;
            GeraFaturamento = geraFaturamento;
            NumDiasPrimeiroVencimento = numDiasPrimeiroVencimento;
            NumDiasIntervalo = numDiasIntervalo;
            QuantidadeParcelas = quantidadeParcelas;
            PayTermNumber = payTermNumber;
            PayTermType = payTermType;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void RegistrarUltimaCompra(DateTime dataCompra, string alteradoPor)
        {
            UltimaCompra = dataCompra;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PessoaFornecedor>()
                .Requires()
                .AreNotEquals(PessoaId, Guid.Empty, nameof(PessoaId), "PessoaId é obrigatório [Origem: PessoaFornecedor]")
                .HasMaxLen(Localizacao ?? string.Empty, 250, nameof(Localizacao), "Localizacao deve ter no máximo 250 caracteres [Origem: PessoaFornecedor]")
                .HasMaxLen(ChequeNominalA ?? string.Empty, 150, nameof(ChequeNominalA), "ChequeNominalA deve ter no máximo 150 caracteres [Origem: PessoaFornecedor]")
                .HasMaxLen(Observacao ?? string.Empty, 300, nameof(Observacao), "Observacao deve ter no máximo 300 caracteres [Origem: PessoaFornecedor]")
                .HasMaxLen(ContaRemetente ?? string.Empty, 50, nameof(ContaRemetente), "ContaRemetente deve ter no máximo 50 caracteres [Origem: PessoaFornecedor]")
            );

            if (PayTermType.HasValue && !Enum.IsDefined(typeof(ETipoPeriodoPagamento), PayTermType.Value))
                AddNotification(nameof(PayTermType), "PayTermType não consta na lista [Origem: PessoaFornecedor]");
        }
    }
}
