using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Registro de um gateway/endpoint de pagamento (EF FIN-SF — estrutura de webhook de baixa).
    /// Guarda a identificação do provedor e a CHAVE de assinatura usada para validar o HMAC dos
    /// webhooks recebidos. A integração real com o provedor (ex.: consultar o pagamento no Mercado
    /// Pago, ler cabeçalho x-signature) é // valida-ambiente — este cadastro é a estrutura, não o cliente HTTP.
    /// </summary>
    public class GatewayPagamento : EntidadeSaaSBase
    {
        public EProvedorPagamento Provedor { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        /// <summary>Segredo compartilhado usado para validar a assinatura (HMAC) dos webhooks. Nunca exposto em API.</summary>
        public string ChaveAssinatura { get; private set; } = string.Empty;
        /// <summary>Identificador externo da conta/aplicação no provedor (opcional; // valida-ambiente).</summary>
        public string? IdentificadorExterno { get; private set; }
        public bool Ativo { get; private set; }

        protected GatewayPagamento() { } // EF Core

        public GatewayPagamento(EProvedorPagamento provedor, string nome, string chaveAssinatura,
            string? identificadorExterno, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<GatewayPagamento>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do gateway é obrigatório.")
                .IsNotNullOrEmpty(chaveAssinatura, nameof(ChaveAssinatura), "A chave de assinatura do gateway é obrigatória."));

            Provedor = provedor;
            Nome = nome;
            ChaveAssinatura = chaveAssinatura;
            IdentificadorExterno = identificadorExterno;
            Ativo = true;
        }

        public void Alterar(string nome, string chaveAssinatura, string? identificadorExterno, string usuario)
        {
            Clear();
            AddNotifications(new Contract<GatewayPagamento>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do gateway é obrigatório.")
                .IsNotNullOrEmpty(chaveAssinatura, nameof(ChaveAssinatura), "A chave de assinatura do gateway é obrigatória."));
            if (!IsValid) return;
            Nome = nome;
            ChaveAssinatura = chaveAssinatura;
            IdentificadorExterno = identificadorExterno;
            MarcarAlterado(usuario);
        }

        public void Ativar(string usuario) { Ativo = true; MarcarAlterado(usuario); }
        public void Desativar(string usuario) { Ativo = false; MarcarAlterado(usuario); }
    }
}
