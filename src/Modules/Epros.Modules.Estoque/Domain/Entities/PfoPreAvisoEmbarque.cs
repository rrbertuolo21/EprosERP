using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Pré-aviso de embarque / ASN do fornecedor (EF Portal do Fornecedor §15.6 `pfo_pre_aviso_embarque`).
    /// PFO-007: o pré-aviso integra a Logística de Entrada (recebimento → motor único, D1). O pedido de
    /// compra é OWNED por COMPRAS — aqui é referência externa (`PedidoCompraId`).
    /// </summary>
    public class PfoPreAvisoEmbarque : EntidadeSaaSBase
    {
        public Guid PedidoCompraId { get; private set; }
        public Guid FornecedorId { get; private set; }
        public EStatusPreAvisoEmbarque Status { get; private set; } = EStatusPreAvisoEmbarque.Rascunho;
        public DateTime? DataPrevistaEntrega { get; private set; }
        public string? Observacao { get; private set; }

        public ICollection<PfoPreAvisoItem> Itens { get; private set; } = new List<PfoPreAvisoItem>();

        protected PfoPreAvisoEmbarque() { }

        public PfoPreAvisoEmbarque(Guid pedidoCompraId, Guid fornecedorId, DateTime? dataPrevistaEntrega, string? observacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PedidoCompraId = pedidoCompraId;
            FornecedorId = fornecedorId;
            DataPrevistaEntrega = dataPrevistaEntrega;
            Observacao = observacao;
            Status = EStatusPreAvisoEmbarque.Rascunho;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PfoPreAvisoEmbarque>()
                .Requires()
                .IsNotEmpty(PedidoCompraId, nameof(PedidoCompraId), "O pedido de compra do pré-aviso é obrigatório [PFO-007] [Origem: PfoPreAvisoEmbarque]")
                .IsNotEmpty(FornecedorId, nameof(FornecedorId), "O fornecedor do pré-aviso é obrigatório [PFO-002] [Origem: PfoPreAvisoEmbarque]"));
        }

        public void Enviar(string usuario) { Status = EStatusPreAvisoEmbarque.Enviado; MarcarAlterado(usuario); }
    }
}
