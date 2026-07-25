using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Catálogo comercial de serviços vendáveis (ven_servico_catalogo).
    /// Fonte: EF_7_VENDAS_GESTAO_DE_SERVICOS_V1 §8 e §19.1.
    /// Serviço não movimenta estoque (EF §3.15) e a taxa do catálogo NÃO alimenta
    /// automaticamente o imposto da fatura (EF §8.9).
    /// </summary>
    public class ServicoCatalogo : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public decimal Valor { get; private set; }
        public decimal Taxa { get; private set; }
        public bool PermiteCamposCustomizados { get; private set; }
        public string? GrupoPrecoLocalidadeJson { get; private set; }
        public decimal? TaxaEmbalagemEntrega { get; private set; }
        public string? TipoTaxaEmbalagemEntrega { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected ServicoCatalogo() { } // EF Core

        public ServicoCatalogo(
            string nome,
            string? descricao,
            decimal valor,
            decimal taxa,
            bool permiteCamposCustomizados,
            string? grupoPrecoLocalidadeJson,
            decimal? taxaEmbalagemEntrega,
            string? tipoTaxaEmbalagemEntrega,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Descricao = descricao;
            Valor = valor < 0 ? 0 : valor;
            Taxa = taxa < 0 ? 0 : taxa;
            PermiteCamposCustomizados = permiteCamposCustomizados;
            GrupoPrecoLocalidadeJson = grupoPrecoLocalidadeJson;
            TaxaEmbalagemEntrega = taxaEmbalagemEntrega;
            TipoTaxaEmbalagemEntrega = tipoTaxaEmbalagemEntrega;
            Ativo = true;
            Validar();
        }

        public void Alterar(
            string nome,
            string? descricao,
            decimal valor,
            decimal taxa,
            bool permiteCamposCustomizados,
            string? grupoPrecoLocalidadeJson,
            decimal? taxaEmbalagemEntrega,
            string? tipoTaxaEmbalagemEntrega,
            string alteradoPor)
        {
            Nome = nome;
            Descricao = descricao;
            Valor = valor < 0 ? 0 : valor;
            Taxa = taxa < 0 ? 0 : taxa;
            PermiteCamposCustomizados = permiteCamposCustomizados;
            GrupoPrecoLocalidadeJson = grupoPrecoLocalidadeJson;
            TaxaEmbalagemEntrega = taxaEmbalagemEntrega;
            TipoTaxaEmbalagemEntrega = tipoTaxaEmbalagemEntrega;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        /// <summary>Reativa serviço excluído logicamente (EF §8.11 / §14).</summary>
        public void Reativar(string alteradoPor)
        {
            Ativo = true;
            MarcarAlterado(alteradoPor);
        }

        public void Desativar(string alteradoPor)
        {
            Ativo = false;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ServicoCatalogo>()
                .Requires()
                .IsNotNullOrEmpty(Nome, nameof(Nome), "O nome do serviço é obrigatório. [Origem: ServicoCatalogo]")
                .IsLowerOrEqualsThan(Nome?.Length ?? 0, 250, nameof(Nome), "O nome do serviço deve ter no máximo 250 caracteres. [Origem: ServicoCatalogo]")
                .IsGreaterThan(Valor, -0.01m, nameof(Valor), "O valor do serviço não pode ser negativo. [Origem: ServicoCatalogo]")
                .IsGreaterThan(Taxa, -0.01m, nameof(Taxa), "A taxa do serviço não pode ser negativa. [Origem: ServicoCatalogo]"));

            if (!string.IsNullOrEmpty(Descricao) && Descricao.Length > 4000)
                AddNotification(nameof(Descricao), "A descrição do serviço deve ter no máximo 4000 caracteres. [Origem: ServicoCatalogo]");
        }
    }
}
