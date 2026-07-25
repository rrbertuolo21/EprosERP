using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Infrastructure.Data
{
    /// <summary>
    /// Helper de infraestrutura para geração automática de títulos financeiros no modelo FIEL
    /// (ContasAPagar / ContasAReceber com FatoGeradorFinanceiro e PlanoDeContasFinanceiroItem).
    ///
    /// Usado pelos handlers de integração (eventos de Venda/Compra/Folha/Projeto), que precisam
    /// garantir a existência de um Plano de Contas item padrão (FK Restrict obrigatória no fiel)
    /// e de um Fato Gerador antes de criar o título.
    /// </summary>
    public static class FinanceiroFielFactory
    {
        /// <summary>
        /// Garante a existência de um item de plano de contas padrão para o tenant e devolve seu Id.
        /// O Id é determinístico por tenant, tornando a operação idempotente entre execuções.
        /// </summary>
        public static async Task<Guid> GarantirPlanoItemPadraoAsync(
            ContextFinanceiro context, string tenantId, string criadoPor, CancellationToken ct = default)
        {
            var planoItemId = GetDeterministicGuid($"DefaultPlanoItem_{tenantId}");

            var existeItem = await context.PlanoDeContasItens
                .IgnoreQueryFilters()
                .AnyAsync(i => i.Id == planoItemId, ct);

            if (existeItem)
                return planoItemId;

            var planoId = GetDeterministicGuid($"DefaultPlano_{tenantId}");
            var existePlano = await context.PlanosDeContas
                .IgnoreQueryFilters()
                .AnyAsync(p => p.Id == planoId, ct);

            if (!existePlano)
            {
                var plano = new PlanoDeContasFinanceiro(null, null, "Plano Padrão", "9.9.9", tenantId, criadoPor);
                ForcarId(plano, planoId);
                context.PlanosDeContas.Add(plano);
            }

            var item = new PlanoDeContasFinanceiroItem(planoId, "9.9.9", "Lançamento automático",
                ETipoDetalhamento.Credito, true, tenantId, criadoPor);
            ForcarId(item, planoItemId);
            context.PlanoDeContasItens.Add(item);

            await context.SaveChangesAsync(ct);
            return planoItemId;
        }

        /// <summary>
        /// Cria e persiste um FatoGeradorFinanceiro (não faz SaveChanges — o chamador salva junto do título).
        /// </summary>
        public static FatoGeradorFinanceiro CriarFatoGerador(
            ContextFinanceiro context, EOrigem origem, Guid? vendaId, Guid? compraId, string? descricao,
            string tenantId, string criadoPor)
        {
            var fato = new FatoGeradorFinanceiro(origem, vendaId, compraId, descricao, tenantId, criadoPor);
            context.FatosGeradoresFinanceiros.Add(fato);
            return fato;
        }

        public static Guid GetDeterministicGuid(string value)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value));
            return new Guid(hash);
        }

        private static void ForcarId(object entidade, Guid id)
        {
            var field = typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase)
                .GetField("<Id>k__BackingField",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(entidade, id);
        }
    }
}
