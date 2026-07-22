using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Dados específicos de combustível de um produto (GLP/gás natural). Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Cadastros.Produtos.ProdutoEspecifico.
    /// </summary>
    public class ProdutoEspecifico : EntidadeSaaSBase
    {
        public Guid ProdutoId { get; private set; }
        public decimal ValorPercentualGlpDerivadoPetroleo { get; private set; }
        public decimal ValorPercentualGasNaturalNacional { get; private set; }
        public decimal ValorPercentualGasNaturalImportado { get; private set; }
        public decimal ValorPartida { get; private set; }
        public EEstado UfConsumo { get; private set; }

        // Navegação intra-módulo
        public ICollection<ProdutoEspecificoCombustivelOrigem> Origens { get; private set; } = new List<ProdutoEspecificoCombustivelOrigem>();
        public Produto? Produto { get; private set; }

        protected ProdutoEspecifico() { } // EF Core

        public ProdutoEspecifico(Guid produtoId, decimal valorPercentualGlpDerivadoPetroleo, decimal valorPercentualGasNaturalNacional, decimal valorPercentualGasNaturalImportado, decimal valorPartida, EEstado ufConsumo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ProdutoId = produtoId;
            ValorPercentualGlpDerivadoPetroleo = valorPercentualGlpDerivadoPetroleo;
            ValorPercentualGasNaturalNacional = valorPercentualGasNaturalNacional;
            ValorPercentualGasNaturalImportado = valorPercentualGasNaturalImportado;
            ValorPartida = valorPartida;
            UfConsumo = ufConsumo;
            Validar();
        }

        public void Validar()
        {
            AddNotifications(new Contract<ProdutoEspecifico>()
                .Requires()
                .IsTrue(Enum.IsDefined(typeof(EEstado), UfConsumo), nameof(UfConsumo), "UF do Consumo não consta na lista [Origem: ProdutoEspecificoUfCombustivel]")
            );
        }

        public void Alterar(decimal valorPercentualGlpDerivadoPetroleo, decimal valorPercentualGasNaturalNacional, decimal valorPercentualGasNaturalImportado, decimal valorPartida, EEstado ufConsumo, string usuario)
        {
            ValorPercentualGlpDerivadoPetroleo = valorPercentualGlpDerivadoPetroleo;
            ValorPercentualGasNaturalNacional = valorPercentualGasNaturalNacional;
            ValorPercentualGasNaturalImportado = valorPercentualGasNaturalImportado;
            ValorPartida = valorPartida;
            UfConsumo = ufConsumo;
            MarcarAlterado(usuario);
            Validar();
        }

        public void AdicionarOrigem(EOrigemTributacaoCombustivel indicadorImportacao, EEstado ufOrigem, decimal valorPercentualUf, string criadoPor)
        {
            Origens.Add(new ProdutoEspecificoCombustivelOrigem(Id, indicadorImportacao, ufOrigem, valorPercentualUf, TenantId, criadoPor));
        }

        public void AlterarOrigem(Guid produtoEspecificoCombustivelOrigemId, EOrigemTributacaoCombustivel indicadorImportacao, EEstado ufOrigem, decimal valorPercentualUf, string usuario)
        {
            var origem = Origens.FirstOrDefault(o => o.Id == produtoEspecificoCombustivelOrigemId);
            origem?.Alterar(indicadorImportacao, ufOrigem, valorPercentualUf, usuario);
        }

        public void DeletarOrigem(Guid produtoEspecificoCombustivelOrigemId, string usuario)
        {
            var origem = Origens.FirstOrDefault(o => o.Id == produtoEspecificoCombustivelOrigemId);
            origem?.Deletar(usuario);
        }

        public void DeletarComOrigens(string usuario)
        {
            Deletar(usuario);
            Origens.ToList().ForEach(o => o.Deletar(usuario));
        }
    }
}
