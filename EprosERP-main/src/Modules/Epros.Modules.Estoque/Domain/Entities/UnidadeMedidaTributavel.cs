using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Unidade de medida tributável por NCM (tabela de vigência). Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Cadastros.Produtos.UnidadeMedidaTributavel (EntityNoTenat no legado;
    /// aqui herda EntidadeSaaSBase mantendo o mesmo conjunto de campos de negócio).
    /// </summary>
    public class UnidadeMedidaTributavel : EntidadeSaaSBase
    {
        public string CodigoNcm { get; private set; } = string.Empty;
        public DateTime DataInicioVigencia { get; private set; }
        public DateTime? DataFimVigencia { get; private set; }
        public string UnidadeMedida { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;

        protected UnidadeMedidaTributavel() { } // EF Core

        public UnidadeMedidaTributavel(string codigoNcm, DateTime dataInicioVigencia, DateTime? dataFimVigencia, string unidadeMedida, string descricao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CodigoNcm = codigoNcm;
            DataInicioVigencia = dataInicioVigencia;
            DataFimVigencia = dataFimVigencia;
            UnidadeMedida = unidadeMedida;
            Descricao = descricao;
        }

        public void Alterar(string codigoNcm, DateTime dataInicioVigencia, DateTime? dataFimVigencia, string unidadeMedida, string descricao, string usuario)
        {
            CodigoNcm = codigoNcm;
            DataInicioVigencia = dataInicioVigencia;
            DataFimVigencia = dataFimVigencia;
            UnidadeMedida = unidadeMedida;
            Descricao = descricao;
            MarcarAlterado(usuario);
        }
    }
}
