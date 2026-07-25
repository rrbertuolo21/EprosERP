using System;

namespace Epros.Modules.Vendas.Infrastructure.Data
{
    // ============================================================================
    // Lookups de LEITURA cross-module (Guid FK), conforme CONVENCAO_CODIGO seção 6.4.
    // O módulo Vendas NÃO referencia Estoque/Fiscal/GestaoClientes; para portar consultas
    // que atravessam módulos (ex.: obter-informacoes-complementares-por-produtos-ids) mapeamos
    // as tabelas de outros schemas como entidades read-only, excluídas das migrations do Vendas.
    // ============================================================================

    /// <summary>Leitura de produtos (schema estoque). Só o necessário p/ resolver o NCM do produto.</summary>
    public class ProdutoLookup
    {
        public Guid Id { get; private set; }
        public Guid? NcmId { get; private set; }
        public DateTime? DeletadoEm { get; private set; }
    }

    /// <summary>Leitura de ncm_configuracoes (schema plataforma). Liga NcmId -> NcmTributacaoId.</summary>
    public class NcmConfiguracaoLookup
    {
        public Guid Id { get; private set; }
        public Guid NcmId { get; private set; }
        public Guid NcmTributacaoId { get; private set; }
        public DateTime? DeletadoEm { get; private set; }
    }

    /// <summary>Leitura de ncm_tributacoes (schema plataforma). Fonte das informações complementares.</summary>
    public class NcmTributacaoLookup
    {
        public Guid Id { get; private set; }
        public string? InformacoesComplementares { get; private set; }
        public DateTime? DeletadoEm { get; private set; }
    }

    /// <summary>Leitura de empresas (schema plataforma). Fallback do NcmTributacaoId quando o NCM não tem config.</summary>
    public class EmpresaLookup
    {
        public Guid Id { get; private set; }
        public Guid? NcmTributacaoId { get; private set; }
        public DateTime? DeletadoEm { get; private set; }
    }
}
