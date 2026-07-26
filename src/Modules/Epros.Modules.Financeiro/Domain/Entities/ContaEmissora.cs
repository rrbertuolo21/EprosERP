using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Conta emissora de cobrança (EF FIN-SF §7.1 / §11.2 sf_conta_emissora).
    /// Dados bancários usados para emissão de boleto e remessa. BancoId referencia Banco (mesmo módulo).
    /// </summary>
    public class ContaEmissora : EntidadeSaaSBase
    {
        public Guid BancoId { get; private set; }
        public Guid? ConfiguracaoCedenteId { get; private set; }
        public string? NomeBanco { get; private set; }
        public string? Carteira { get; private set; }
        public string? Agencia { get; private set; }
        public string? DigitoAgencia { get; private set; }
        public string? Conta { get; private set; }
        public string? DigitoConta { get; private set; }
        public string? Especie { get; private set; }
        public long NossoNumeroAtual { get; private set; }
        public string? TipoCobranca { get; private set; }
        public string? Convenio { get; private set; }
        public string? Contrato { get; private set; }
        public string? TipoCarteira { get; private set; }
        public long IncrementoNossoNumero { get; private set; }
        public string? TipoRemessa { get; private set; }
        public string? CodigoCliente { get; private set; }
        public string? Posto { get; private set; }
        public bool Ativa { get; private set; }

        protected ContaEmissora() { } // EF Core

        public ContaEmissora(
            Guid bancoId, Guid? configuracaoCedenteId, string? nomeBanco, string? carteira, string? agencia,
            string? digitoAgencia, string? conta, string? digitoConta, string? especie, long nossoNumeroAtual,
            string? tipoCobranca, string? convenio, string? contrato, string? tipoCarteira, long incrementoNossoNumero,
            string? tipoRemessa, string? codigoCliente, string? posto, bool ativa, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            BancoId = bancoId; ConfiguracaoCedenteId = configuracaoCedenteId; NomeBanco = nomeBanco; Carteira = carteira;
            Agencia = agencia; DigitoAgencia = digitoAgencia; Conta = conta; DigitoConta = digitoConta; Especie = especie;
            NossoNumeroAtual = nossoNumeroAtual; TipoCobranca = tipoCobranca; Convenio = convenio; Contrato = contrato;
            TipoCarteira = tipoCarteira; IncrementoNossoNumero = incrementoNossoNumero; TipoRemessa = tipoRemessa;
            CodigoCliente = codigoCliente; Posto = posto; Ativa = ativa;
            Validar();
        }

        public void Alterar(
            Guid bancoId, Guid? configuracaoCedenteId, string? nomeBanco, string? carteira, string? agencia,
            string? digitoAgencia, string? conta, string? digitoConta, string? especie,
            string? tipoCobranca, string? convenio, string? contrato, string? tipoCarteira, long incrementoNossoNumero,
            string? tipoRemessa, string? codigoCliente, string? posto, bool ativa, string usuario)
        {
            BancoId = bancoId; ConfiguracaoCedenteId = configuracaoCedenteId; NomeBanco = nomeBanco; Carteira = carteira;
            Agencia = agencia; DigitoAgencia = digitoAgencia; Conta = conta; DigitoConta = digitoConta; Especie = especie;
            TipoCobranca = tipoCobranca; Convenio = convenio; Contrato = contrato; TipoCarteira = tipoCarteira;
            IncrementoNossoNumero = incrementoNossoNumero; TipoRemessa = tipoRemessa; CodigoCliente = codigoCliente;
            Posto = posto; Ativa = ativa;
            MarcarAlterado(usuario);
            Validar();
        }

        public void Ativar(string usuario) { Ativa = true; MarcarAlterado(usuario); }
        public void Desativar(string usuario) { Ativa = false; MarcarAlterado(usuario); }

        /// <summary>Consome o próximo nosso número (sequencial), retornando o valor gerado.</summary>
        public long GerarProximoNossoNumero(string usuario)
        {
            NossoNumeroAtual += 1;
            MarcarAlterado(usuario);
            return NossoNumeroAtual;
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ContaEmissora>()
                .Requires()
                .AreNotEquals(BancoId, Guid.Empty, nameof(BancoId), "O banco da conta emissora é obrigatório.")
            );
        }
    }
}
