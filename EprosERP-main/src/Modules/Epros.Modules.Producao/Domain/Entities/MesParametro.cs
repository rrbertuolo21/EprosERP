using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-MES — Parâmetros de produção por tenant (prd_mes_parametro). Fiel ao EF §17.
    /// MES-REG-025/026/027: prefixo de referência, bloqueio de edição de insumo e atualização de preço.
    /// </summary>
    public class MesParametro : EntidadeSaaSBase
    {
        public string? PrefixoReferencia { get; private set; }
        public bool BloquearEdicaoQuantidadeInsumo { get; private set; }
        public bool AtualizarPrecoProdutoFinal { get; private set; }
        public bool ExigirEstruturaAtiva { get; private set; }
        public string? VersaoParametro { get; private set; }

        protected MesParametro() { } // EF Core

        public MesParametro(
            string tenantId,
            string criadoPor,
            string? prefixoReferencia = null,
            bool bloquearEdicaoQuantidadeInsumo = false,
            bool atualizarPrecoProdutoFinal = false,
            bool exigirEstruturaAtiva = false,
            string? versaoParametro = null)
            : base(tenantId, criadoPor)
        {
            PrefixoReferencia = prefixoReferencia;
            BloquearEdicaoQuantidadeInsumo = bloquearEdicaoQuantidadeInsumo;
            AtualizarPrecoProdutoFinal = atualizarPrecoProdutoFinal;
            ExigirEstruturaAtiva = exigirEstruturaAtiva;
            VersaoParametro = versaoParametro;
        }

        public void Alterar(
            string? prefixoReferencia,
            bool bloquearEdicaoQuantidadeInsumo,
            bool atualizarPrecoProdutoFinal,
            bool exigirEstruturaAtiva,
            string? versaoParametro,
            string alteradoPor)
        {
            PrefixoReferencia = prefixoReferencia;
            BloquearEdicaoQuantidadeInsumo = bloquearEdicaoQuantidadeInsumo;
            AtualizarPrecoProdutoFinal = atualizarPrecoProdutoFinal;
            ExigirEstruturaAtiva = exigirEstruturaAtiva;
            VersaoParametro = versaoParametro;
            MarcarAlterado(alteradoPor);
        }
    }
}
