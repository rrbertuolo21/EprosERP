using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Modules.Financeiro.Domain.Services;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Contrato de arrendamento mercantil (leasing) — IFRS-16 / CPC 06 (R2) (EF FIN-GCF, extensão).
    /// Guarda os parâmetros do arrendamento e o RECONHECIMENTO INICIAL calculado pelo
    /// <see cref="CalculoLeasing"/> (passivo de arrendamento = valor presente das contraprestações à
    /// taxa incremental; ativo de direito de uso correspondente). O cronograma de amortização do passivo
    /// e a depreciação do direito de uso são derivados sob demanda a partir destes parâmetros.
    ///
    /// A TAXA incremental, o prazo e a classificação chegam INFORMADOS (convenção do contrato). A
    /// contabilização das contas envolvidas = // valida-contador. Submódulo de evolução — sobe
    /// desabilitado (ABAC nega por padrão). Isolamento por tenant via ContextBase.
    /// </summary>
    public class ContratoArrendamento : EntidadeSaaSBase
    {
        public string Descricao { get; private set; } = string.Empty;
        public Guid? PessoaId { get; private set; }
        public DateTime DataInicio { get; private set; }
        public decimal ValorContraprestacao { get; private set; }
        public int QuantidadeParcelas { get; private set; }
        /// <summary>Taxa incremental de captação por período (ex.: mensal). INFORMADA (// valida-contador).</summary>
        public decimal TaxaIncrementalPeriodo { get; private set; }
        public bool PagamentoAntecipado { get; private set; }
        public decimal CustosDiretosIniciais { get; private set; }
        public decimal IncentivosRecebidos { get; private set; }

        // Reconhecimento inicial (IFRS-16 §22–24) — calculado na criação/remensuração.
        public decimal PassivoArrendamentoInicial { get; private set; }
        public decimal DireitoDeUsoInicial { get; private set; }

        public EStatusContratoFinanceiro Status { get; private set; } = EStatusContratoFinanceiro.Ativo;
        public string? MotivoEncerramento { get; private set; }
        public DateTime? DataEncerramento { get; private set; }

        protected ContratoArrendamento() { } // EF Core

        public ContratoArrendamento(
            string descricao, Guid? pessoaId, DateTime dataInicio, decimal valorContraprestacao,
            int quantidadeParcelas, decimal taxaIncrementalPeriodo, bool pagamentoAntecipado,
            decimal custosDiretosIniciais, decimal incentivosRecebidos, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Descricao = descricao;
            PessoaId = pessoaId;
            DataInicio = dataInicio;
            ValorContraprestacao = valorContraprestacao;
            QuantidadeParcelas = quantidadeParcelas;
            TaxaIncrementalPeriodo = taxaIncrementalPeriodo;
            PagamentoAntecipado = pagamentoAntecipado;
            CustosDiretosIniciais = custosDiretosIniciais;
            IncentivosRecebidos = incentivosRecebidos;
            Status = EStatusContratoFinanceiro.Ativo;
            Validar();
            if (IsValid) CalcularReconhecimentoInicial();
        }

        /// <summary>Recalcula o reconhecimento inicial (passivo + direito de uso) a partir dos parâmetros.</summary>
        private void CalcularReconhecimentoInicial()
        {
            var rec = CalculoLeasing.ReconhecerInicial(
                ValorContraprestacao, TaxaIncrementalPeriodo, QuantidadeParcelas, PagamentoAntecipado,
                CustosDiretosIniciais, 0m, IncentivosRecebidos);
            PassivoArrendamentoInicial = rec.PassivoArrendamento;
            DireitoDeUsoInicial = rec.DireitoDeUso;
        }

        public void Encerrar(string motivo, DateTime? dataEncerramento, string usuario)
        {
            if (Status == EStatusContratoFinanceiro.Cancelado)
            {
                AddNotification(nameof(Status), "Arrendamento já encerrado.");
                return;
            }
            Status = EStatusContratoFinanceiro.Cancelado;
            MotivoEncerramento = motivo;
            DataEncerramento = dataEncerramento ?? DateTime.UtcNow;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ContratoArrendamento>()
                .Requires()
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descrição do arrendamento é obrigatória [Origem: ContratoArrendamento]")
                .IsGreaterThan(ValorContraprestacao, 0m, nameof(ValorContraprestacao), "A contraprestação deve ser maior que zero [Origem: ContratoArrendamento]")
                .IsGreaterThan(QuantidadeParcelas, 0, nameof(QuantidadeParcelas), "O número de contraprestações deve ser maior que zero [Origem: ContratoArrendamento]")
                .IsGreaterOrEqualsThan(TaxaIncrementalPeriodo, 0m, nameof(TaxaIncrementalPeriodo), "A taxa incremental não pode ser negativa [Origem: ContratoArrendamento]")
                .IsGreaterThan(DataInicio, DateTime.MinValue, nameof(DataInicio), "A data de início é obrigatória [Origem: ContratoArrendamento]")
            );
        }
    }
}
