using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Processo de cotação multi-fornecedor (EF Sourcing e Compras §5.9 / §9.2 `sc_cotacao`).
    /// SC-059: estrutura hierárquica por fornecedor e detalhes.
    /// Situação preservada como texto conforme material (domínio não padronizado).
    /// </summary>
    public class ScCotacao : EntidadeSaaSBase
    {
        public DateTime? DataCotacao { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public string Situacao { get; private set; } = string.Empty;

        // CD2 — escolha do vencedor após o mapa comparativo. Fornecedor vencedor (cabeçalho) e data da
        // decisão. Quando decidida, a cotação está apta a originar o pedido de compra (SOURCING → COMPRAS).
        public Guid? FornecedorVencedorId { get; private set; }
        public DateTime? DecididaEm { get; private set; }

        /// <summary>Situação canônica de decisão (CD2): cotação com vencedor escolhido.</summary>
        public const string SituacaoDecidida = "DECIDIDA";

        // Navegação intra-módulo
        public ICollection<ScCotacaoFornecedor> Fornecedores { get; private set; } = new List<ScCotacaoFornecedor>();
        public ICollection<ScCotacaoItem> Itens { get; private set; } = new List<ScCotacaoItem>();

        protected ScCotacao() { } // EF Core

        public ScCotacao(DateTime? dataCotacao, string descricao, string situacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            DataCotacao = dataCotacao;
            Descricao = descricao ?? string.Empty;
            Situacao = situacao ?? string.Empty;
            Validar();
        }

        public void Alterar(DateTime? dataCotacao, string descricao, string situacao, string alteradoPor)
        {
            DataCotacao = dataCotacao;
            Descricao = descricao ?? string.Empty;
            Situacao = situacao ?? string.Empty;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void AdicionarFornecedor(ScCotacaoFornecedor fornecedor) => Fornecedores.Add(fornecedor);
        public void AdicionarItem(ScCotacaoItem item) => Itens.Add(item);

        /// <summary>
        /// CD2 — seleciona o fornecedor vencedor após o mapa comparativo. Exige que o fornecedor pertença à
        /// cotação. Idempotente-seguro: não decide duas vezes. Marca a situação como DECIDIDA.
        /// </summary>
        public bool SelecionarVencedor(Guid fornecedorId, string usuario)
        {
            if (FornecedorVencedorId.HasValue)
            {
                AddNotification("FornecedorVencedorId", "Cotação já teve o vencedor escolhido [SRC-020] [Origem: ScCotacao]");
                return false;
            }
            var pertence = false;
            foreach (var f in Fornecedores)
                if (f.FornecedorId == fornecedorId) { pertence = true; break; }
            if (!pertence)
            {
                AddNotification("FornecedorVencedorId", "O fornecedor vencedor deve ser um dos participantes da cotação [SRC-021] [Origem: ScCotacao]");
                return false;
            }
            FornecedorVencedorId = fornecedorId;
            DecididaEm = DateTime.UtcNow;
            Situacao = SituacaoDecidida;
            MarcarAlterado(usuario);
            return true;
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ScCotacao>()
                .Requires()
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descrição da cotação é obrigatória [Origem: ScCotacao]")
                .IsNotNullOrEmpty(Situacao, nameof(Situacao), "A situação da cotação é obrigatória [Origem: ScCotacao]"));
        }
    }
}
