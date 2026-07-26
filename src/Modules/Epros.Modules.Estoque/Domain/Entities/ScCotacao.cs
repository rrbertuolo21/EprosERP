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
