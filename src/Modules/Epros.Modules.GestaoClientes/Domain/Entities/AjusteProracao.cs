using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Natureza do ajuste de proração gerado numa mudança de plano.</summary>
    public enum TipoAjusteProracao
    {
        /// <summary>Novo plano mais caro no restante do ciclo → diferença a COBRAR (fatura de diferença).</summary>
        Debito,
        /// <summary>Novo plano mais barato → CRÉDITO a compensar na próxima fatura.</summary>
        Credito,
        /// <summary>Sem diferença proporcional (mesmo preço no restante do ciclo).</summary>
        Neutro
    }

    /// <summary>
    /// 1.08D — Registro de PRORAÇÃO por mudança de plano (upgrade/downgrade self-service).
    ///
    /// ⚠️ VALIDA-CONTADOR: este registro guarda o MECANISMO (pro-rata por dias corridos, como default).
    /// A POLÍTICA fiscal/contábil exata — critério de arredondamento, índice de reajuste, contas
    /// contábeis de crédito/débito e reconhecimento de receita — é PARÂMETRO do cliente/contador
    /// (skill financeiro/contábil). O sistema NÃO inventa alíquota nem índice: apenas registra o fato
    /// proporcional aos dias restantes do ciclo. Ver Regra #0 (Negócio vem da skill).
    /// </summary>
    public class AjusteProracao : EntidadeSaaSBase
    {
        public Guid AssinaturaClienteId { get; private set; }
        public Guid ClienteId { get; private set; }
        public Guid PlanoAnteriorId { get; private set; }
        public Guid PlanoNovoId { get; private set; }
        public decimal PrecoAnterior { get; private set; }
        public decimal PrecoNovo { get; private set; }
        public int DiasCiclo { get; private set; }
        public int DiasRestantes { get; private set; }

        /// <summary>Valor do ajuste (positivo = débito a cobrar; negativo = crédito a compensar).</summary>
        public decimal ValorAjuste { get; private set; }
        public TipoAjusteProracao Tipo { get; private set; }

        /// <summary>Fatura de diferença gerada (quando débito). Null para crédito/neutro.</summary>
        public Guid? FaturaId { get; private set; }

        public string? Observacao { get; private set; }

        protected AjusteProracao() { } // EF Core

        public AjusteProracao(
            Guid assinaturaClienteId,
            Guid clienteId,
            Guid planoAnteriorId,
            Guid planoNovoId,
            decimal precoAnterior,
            decimal precoNovo,
            int diasCiclo,
            int diasRestantes,
            decimal valorAjuste,
            TipoAjusteProracao tipo,
            Guid? faturaId,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AjusteProracao>()
                .Requires()
                .AreNotEquals(assinaturaClienteId, Guid.Empty, nameof(AssinaturaClienteId), "Assinatura é obrigatória")
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "Cliente é obrigatório")
                .AreNotEquals(planoNovoId, Guid.Empty, nameof(PlanoNovoId), "Novo plano é obrigatório")
            );

            AssinaturaClienteId = assinaturaClienteId;
            ClienteId = clienteId;
            PlanoAnteriorId = planoAnteriorId;
            PlanoNovoId = planoNovoId;
            PrecoAnterior = precoAnterior;
            PrecoNovo = precoNovo;
            DiasCiclo = diasCiclo;
            DiasRestantes = diasRestantes;
            ValorAjuste = valorAjuste;
            Tipo = tipo;
            FaturaId = faturaId;
            Observacao = observacao;
        }

        public void VincularFatura(Guid faturaId, string alteradoPor)
        {
            FaturaId = faturaId;
            MarcarAlterado(alteradoPor);
        }
    }
}
