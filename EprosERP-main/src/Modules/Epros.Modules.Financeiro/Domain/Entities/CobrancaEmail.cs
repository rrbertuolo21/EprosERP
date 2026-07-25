using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Cobrança por e-mail com confirmação manual de pagamento (EF FIN-SF §7.7 / §11 sf_cobranca_email).
    /// Entidade distinta da fatura de boleto. Ciclo de status RSF-030 a RSF-035.
    /// </summary>
    public class CobrancaEmail : EntidadeSaaSBase
    {
        public Guid? SacadoId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public decimal Valor { get; private set; }
        public string? Periodo { get; private set; }
        public string? Servicos { get; private set; }
        public string? Conta { get; private set; }
        public string? LinkExterno { get; private set; }
        public string? Observacao { get; private set; }
        public string? Emails { get; private set; }
        public EStatusCobrancaEmail Status { get; private set; } = EStatusCobrancaEmail.Encubada;
        public EAreaCobrancaEmail Area { get; private set; } = EAreaCobrancaEmail.NoAuto;
        public string? ComprovanteConfirmacao { get; private set; }

        protected CobrancaEmail() { } // EF Core

        public CobrancaEmail(Guid? sacadoId, string nome, decimal valor, string? periodo, string? servicos, string? conta, string? linkExterno, string? observacao, string? emails, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            SacadoId = sacadoId; Nome = nome; Valor = valor; Periodo = periodo; Servicos = servicos;
            Conta = conta; LinkExterno = linkExterno; Observacao = observacao; Emails = emails;
            Status = EStatusCobrancaEmail.Encubada; // RSF-030
            Area = EAreaCobrancaEmail.NoAuto;
            Validar();
        }

        /// <summary>RSF-031: primeiro envio → Em andamento.</summary>
        public void EnviarPrimeiraCobranca(string usuario) { Status = EStatusCobrancaEmail.EmAndamento; MarcarAlterado(usuario); }
        /// <summary>RSF-032: recobrança → Recobrado.</summary>
        public void Recobrar(string usuario) { Status = EStatusCobrancaEmail.Recobrado; MarcarAlterado(usuario); }
        /// <summary>RSF-033: marcar inadimplência → Inadimplente.</summary>
        public void MarcarInadimplente(string usuario) { Status = EStatusCobrancaEmail.Inadimplente; MarcarAlterado(usuario); }

        /// <summary>RSF-034/036: confirmação de pagamento pelo sacado exige texto/comprovante → Aguardando validação.</summary>
        public void ConfirmarPagamento(string comprovante, string usuario)
        {
            if (string.IsNullOrWhiteSpace(comprovante))
            {
                AddNotification(nameof(ComprovanteConfirmacao), "A confirmação de pagamento exige texto/comprovante.");
                return;
            }
            ComprovanteConfirmacao = comprovante;
            Status = EStatusCobrancaEmail.AguardandoValidacao;
            MarcarAlterado(usuario);
        }

        /// <summary>RSF-035: validação de pagamento → Finalizada.</summary>
        public void ValidarPagamento(string usuario) { Status = EStatusCobrancaEmail.Finalizada; MarcarAlterado(usuario); }

        /// <summary>RSF-037: colocar/retirar da fila diária (auto/noauto).</summary>
        public void DefinirArea(EAreaCobrancaEmail area, string usuario) { Area = area; MarcarAlterado(usuario); }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<CobrancaEmail>()
                .Requires()
                .IsNotNullOrEmpty(Nome, nameof(Nome), "O nome da cobrança é obrigatório.")
                .IsGreaterThan(Valor, 0m, nameof(Valor), "O valor da cobrança deve ser maior que zero.")
            );
        }
    }
}
