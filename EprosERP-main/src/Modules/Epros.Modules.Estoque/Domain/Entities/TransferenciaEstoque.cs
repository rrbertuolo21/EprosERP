using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Cabeçalho de transferência de estoque entre locais (EF Movimentação Manual e Ajustes §15.6).
    /// LocalOrigemId/LocalDestinoId referenciam o módulo WMS por FK Guid (sem navegação cruzada).
    /// </summary>
    public class TransferenciaEstoque : EntidadeSaaSBase
    {
        public Guid LocalOrigemId { get; private set; }
        public Guid LocalDestinoId { get; private set; }
        public DateTime DataTransferencia { get; private set; }
        public EStatusTransferenciaEstoque Situacao { get; private set; } = EStatusTransferenciaEstoque.Rascunho;
        public decimal? ValorFrete { get; private set; }
        public string? Observacao { get; private set; }

        // Navegação intra-módulo
        public ICollection<TransferenciaEstoqueItem> Itens { get; private set; } = new List<TransferenciaEstoqueItem>();

        protected TransferenciaEstoque() { } // EF Core

        public TransferenciaEstoque(Guid localOrigemId, Guid localDestinoId, DateTime dataTransferencia, decimal? valorFrete, string? observacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            LocalOrigemId = localOrigemId;
            LocalDestinoId = localDestinoId;
            DataTransferencia = dataTransferencia;
            ValorFrete = valorFrete;
            Observacao = observacao;
            Situacao = EStatusTransferenciaEstoque.Rascunho;
            Validar();
        }

        /// <summary>MVM-020: origem deve ser diferente do destino.</summary>
        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<TransferenciaEstoque>()
                .Requires()
                .IsFalse(LocalOrigemId == LocalDestinoId, nameof(LocalDestinoId), "O local de destino deve ser diferente do local de origem [MVM-020] [Origem: TransferenciaEstoque]"));

            if (LocalOrigemId == Guid.Empty)
                AddNotification("LocalOrigemId", "O local de origem é obrigatório [Origem: TransferenciaEstoque]");
            if (LocalDestinoId == Guid.Empty)
                AddNotification("LocalDestinoId", "O local de destino é obrigatório [Origem: TransferenciaEstoque]");
        }

        public void AdicionarItem(TransferenciaEstoqueItem item)
        {
            Itens.Add(item);
        }

        public void Confirmar(string usuario)
        {
            Situacao = EStatusTransferenciaEstoque.Confirmada;
            MarcarAlterado(usuario);
        }

        public void Receber(string usuario)
        {
            Situacao = EStatusTransferenciaEstoque.Recebida;
            MarcarAlterado(usuario);
        }

        public void Cancelar(string usuario)
        {
            Situacao = EStatusTransferenciaEstoque.Cancelada;
            MarcarAlterado(usuario);
        }

        public void Estornar(string usuario)
        {
            Situacao = EStatusTransferenciaEstoque.Estornada;
            MarcarAlterado(usuario);
        }
    }
}
