using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Regra de alçada de aprovação de compras (CD3 / EF SOURCING §5.8 `com_alcada_regra`).
    /// Define, por NÍVEL, a faixa de valor (min/max), e os filtros opcionais de comprador e
    /// categoria de compra, além do aprovador (por pessoa ou por papel). Não há código de alçada
    /// legado — este é o modelo novo. Referências externas (comprador/aprovador) por FK Guid.
    ///
    /// Regras: nível ≥ 1; valor mínimo ≥ 0; valor máximo, quando informado, deve ser maior que o
    /// mínimo; é obrigatório indicar um aprovador — por pessoa (AprovadorId) ou por papel
    /// (PapelAprovador). Filtros nulos = "qualquer" (regra genérica).
    /// </summary>
    public class ComprasAlcadaRegra : EntidadeSaaSBase
    {
        public int Nivel { get; private set; }
        public decimal ValorMinimo { get; private set; }
        public decimal? ValorMaximo { get; private set; }
        public Guid? CompradorId { get; private set; }
        public string? CategoriaCompra { get; private set; }
        public Guid? AprovadorId { get; private set; }
        public string? PapelAprovador { get; private set; }
        public bool Ativo { get; private set; }

        protected ComprasAlcadaRegra() { } // EF Core

        public ComprasAlcadaRegra(
            int nivel,
            decimal valorMinimo,
            decimal? valorMaximo,
            Guid? compradorId,
            string? categoriaCompra,
            Guid? aprovadorId,
            string? papelAprovador,
            bool ativo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nivel = nivel;
            ValorMinimo = valorMinimo;
            ValorMaximo = valorMaximo;
            CompradorId = compradorId;
            CategoriaCompra = string.IsNullOrWhiteSpace(categoriaCompra) ? null : categoriaCompra.Trim();
            AprovadorId = aprovadorId;
            PapelAprovador = string.IsNullOrWhiteSpace(papelAprovador) ? null : papelAprovador.Trim();
            Ativo = ativo;
            Validar();
        }

        public void Alterar(
            int nivel,
            decimal valorMinimo,
            decimal? valorMaximo,
            Guid? compradorId,
            string? categoriaCompra,
            Guid? aprovadorId,
            string? papelAprovador,
            bool ativo,
            string alteradoPor)
        {
            Nivel = nivel;
            ValorMinimo = valorMinimo;
            ValorMaximo = valorMaximo;
            CompradorId = compradorId;
            CategoriaCompra = string.IsNullOrWhiteSpace(categoriaCompra) ? null : categoriaCompra.Trim();
            AprovadorId = aprovadorId;
            PapelAprovador = string.IsNullOrWhiteSpace(papelAprovador) ? null : papelAprovador.Trim();
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        /// <summary>True se esta regra se aplica a um pedido de valor/comprador/categoria informados (CD3),
        /// tratando a faixa como banda única [min, max]. Use <see cref="ExigidaEmEscala"/> para montar a
        /// cadeia de aprovação multi-nível.</summary>
        public bool Aplica(decimal valorTotal, Guid? compradorId, string? categoriaCompra)
        {
            if (!Ativo) return false;
            if (valorTotal < ValorMinimo) return false;
            if (ValorMaximo.HasValue && valorTotal > ValorMaximo.Value) return false;
            if (CompradorId.HasValue && CompradorId.Value != compradorId) return false;
            if (!string.IsNullOrWhiteSpace(CategoriaCompra) &&
                !string.Equals(CategoriaCompra, categoriaCompra?.Trim(), StringComparison.OrdinalIgnoreCase))
                return false;
            return true;
        }

        /// <summary>
        /// True se este nível é EXIGIDO na cadeia de escalonamento (CD3). A alçada multi-nível é
        /// CUMULATIVA: um valor que ultrapassa o piso (<see cref="ValorMinimo"/>) de um nível exige a
        /// aprovação daquele nível — de modo que R$ 8.000 percorre Gerente (piso 0) E Diretor (piso 5.000).
        /// O teto (<see cref="ValorMaximo"/>) é apenas descritivo da banda do nível, não limita a exigência.
        /// Filtros de comprador/categoria nulos = "qualquer".
        /// </summary>
        public bool ExigidaEmEscala(decimal valorTotal, Guid? compradorId, string? categoriaCompra)
        {
            if (!Ativo) return false;
            if (valorTotal <= ValorMinimo) return false;
            if (CompradorId.HasValue && CompradorId.Value != compradorId) return false;
            if (!string.IsNullOrWhiteSpace(CategoriaCompra) &&
                !string.Equals(CategoriaCompra, categoriaCompra?.Trim(), StringComparison.OrdinalIgnoreCase))
                return false;
            return true;
        }

        public void Validar()
        {
            Clear();
            if (Nivel < 1)
                AddNotification("Nivel", "O nível da alçada deve ser maior ou igual a 1 [ALC-001] [Origem: ComprasAlcadaRegra]");
            if (ValorMinimo < 0)
                AddNotification("ValorMinimo", "O valor mínimo da faixa não pode ser negativo [ALC-002] [Origem: ComprasAlcadaRegra]");
            if (ValorMaximo.HasValue && ValorMaximo.Value <= ValorMinimo)
                AddNotification("ValorMaximo", "O valor máximo da faixa deve ser maior que o mínimo [ALC-003] [Origem: ComprasAlcadaRegra]");
            if (AprovadorId == null && string.IsNullOrWhiteSpace(PapelAprovador))
                AddNotification("Aprovador", "É obrigatório informar o aprovador (pessoa ou papel) [ALC-004] [Origem: ComprasAlcadaRegra]");
        }
    }
}
