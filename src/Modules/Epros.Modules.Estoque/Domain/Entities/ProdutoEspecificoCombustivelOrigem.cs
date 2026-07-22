using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Origem (UF) da tributação de combustível de um ProdutoEspecifico. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Cadastros.Produtos.ProdutoEspecificoCombustivelOrigem.
    /// </summary>
    public class ProdutoEspecificoCombustivelOrigem : EntidadeSaaSBase
    {
        public Guid ProdutoEspecificoId { get; private set; }
        public EOrigemTributacaoCombustivel IndicadorImportacao { get; private set; }
        public EEstado UfOrigem { get; private set; }
        public decimal ValorPercentualUf { get; private set; }

        // Navegação intra-módulo
        public ProdutoEspecifico? ProdutoEspecifico { get; private set; }

        protected ProdutoEspecificoCombustivelOrigem() { } // EF Core

        public ProdutoEspecificoCombustivelOrigem(Guid produtoEspecificoId, EOrigemTributacaoCombustivel indicadorImportacao, EEstado ufOrigem, decimal valorPercentualUf, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ProdutoEspecificoId = produtoEspecificoId;
            UfOrigem = ufOrigem;
            ValorPercentualUf = valorPercentualUf;
            IndicadorImportacao = indicadorImportacao;
            Validar();
        }

        public void Validar()
        {
            AddNotifications(new Contract<ProdutoEspecificoCombustivelOrigem>()
                .Requires()
                .IsTrue(Enum.IsDefined(typeof(EEstado), UfOrigem), nameof(UfOrigem), "UF não consta na lista [Origem: ProdutoEspecificoUfCombustivel]")
            );
        }

        public void Alterar(EOrigemTributacaoCombustivel indicadorImportacao, EEstado ufOrigem, decimal valorPercentualUf, string usuario)
        {
            IndicadorImportacao = indicadorImportacao;
            UfOrigem = ufOrigem;
            ValorPercentualUf = valorPercentualUf;
            MarcarAlterado(usuario);
            Validar();
        }
    }
}
