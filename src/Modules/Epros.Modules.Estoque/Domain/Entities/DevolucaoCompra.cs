using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Devolução de compra (CD4 / EF DEVOLUCAO_DE_COMPRA §6.1 `com_devolucao_compra`). Submódulo PRÓPRIO
    /// de COMPRAS: registra a devolução de mercadoria ao fornecedor (total ou parcial) referenciando a
    /// compra de origem (`com_compra`). Reaproveitado por compra nacional e por importação.
    ///
    /// Ciclo de vida (T3): nasce Rascunho (SEM efeito, DEV-003) → Confirmada (gera SAÍDA de estoque por
    /// evento — motor único D1, DEV-004 — + documento fiscal com CFOP parametrizado + estorno financeiro
    /// idempotente por compra, DEV-006) → Cancelada (compensa os efeitos, sem apagar o original, TEC-C6).
    ///
    /// ⚠️ Sentido do movimento (saída) e CFOP = valida-contador (NF-06). O CFOP é PARAMETRIZADO (campo
    /// <see cref="Cfop"/>), nunca fixo em código: o contador informa antes do go-live (DEV-005).
    /// </summary>
    public class DevolucaoCompra : EntidadeSaaSBase
    {
        public Guid CompraOrigemId { get; private set; }
        public Guid FornecedorId { get; private set; }
        public string Numero { get; private set; } = string.Empty;
        public DateTime DataDevolucao { get; private set; }
        public ETipoDevolucaoCompra Tipo { get; private set; }
        public string? Motivo { get; private set; }
        public EStatusDevolucaoCompra Status { get; private set; } = EStatusDevolucaoCompra.Rascunho;
        public Guid? DocumentoFiscalId { get; private set; }
        /// <summary>CFOP da devolução — PARAMETRIZADO (NF-06, valida-contador). Nunca fixo em código.</summary>
        public string? Cfop { get; private set; }
        public decimal Total { get; private set; }
        public DateTime? ConfirmadaEm { get; private set; }
        public DateTime? CanceladaEm { get; private set; }

        public ICollection<DevolucaoCompraItem> Itens { get; private set; } = new List<DevolucaoCompraItem>();

        protected DevolucaoCompra() { } // EF Core

        public DevolucaoCompra(
            Guid compraOrigemId,
            Guid fornecedorId,
            string numero,
            DateTime dataDevolucao,
            ETipoDevolucaoCompra tipo,
            string? motivo,
            string? cfop,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraOrigemId = compraOrigemId;
            FornecedorId = fornecedorId;
            Numero = numero?.Trim() ?? string.Empty;
            DataDevolucao = dataDevolucao;
            Tipo = tipo;
            Motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();
            Cfop = string.IsNullOrWhiteSpace(cfop) ? null : cfop.Trim();
            Status = EStatusDevolucaoCompra.Rascunho;
            Validar();
        }

        public void AdicionarItem(DevolucaoCompraItem item)
        {
            Itens.Add(item);
            RecalcularTotal();
        }

        public void RecalcularTotal()
        {
            Total = Math.Round(Itens.Sum(i => i.ValorTotal), 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>Altera dados de cabeçalho. Permitido apenas em Rascunho (DEV-003).</summary>
        public bool Alterar(DateTime dataDevolucao, ETipoDevolucaoCompra tipo, string? motivo, string? cfop, string usuario)
        {
            if (Status != EStatusDevolucaoCompra.Rascunho)
            {
                AddNotification("Status", "Apenas devoluções em rascunho podem ser alteradas [DEV-010] [Origem: DevolucaoCompra]");
                return false;
            }
            DataDevolucao = dataDevolucao;
            Tipo = tipo;
            Motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();
            Cfop = string.IsNullOrWhiteSpace(cfop) ? null : cfop.Trim();
            Validar();
            if (!IsValid) return false;
            MarcarAlterado(usuario);
            return true;
        }

        public void DefinirDocumentoFiscal(Guid documentoFiscalId, string usuario)
        {
            DocumentoFiscalId = documentoFiscalId;
            MarcarAlterado(usuario);
        }

        /// <summary>
        /// Confirma a devolução (DEV-004): habilita a publicação dos eventos de saída de estoque e estorno
        /// financeiro. Só a partir de Rascunho e com ao menos um item. O efeito em si é materializado por
        /// evento/Outbox no handler (EC-05) — a entidade só transiciona o estado.
        /// </summary>
        public bool Confirmar(string usuario)
        {
            if (Status != EStatusDevolucaoCompra.Rascunho)
            {
                AddNotification("Status", "Apenas devoluções em rascunho podem ser confirmadas [DEV-011] [Origem: DevolucaoCompra]");
                return false;
            }
            if (!Itens.Any())
            {
                AddNotification("Itens", "A devolução deve conter ao menos um item para ser confirmada [DEV-012] [Origem: DevolucaoCompra]");
                return false;
            }
            RecalcularTotal();
            Status = EStatusDevolucaoCompra.Confirmada;
            ConfirmadaEm = DateTime.UtcNow;
            MarcarAlterado(usuario);
            return true;
        }

        /// <summary>
        /// Cancela a devolução. Se já Confirmada, o handler publica os eventos de COMPENSAÇÃO (reverte a
        /// saída e o estorno, sem apagar o original — TEC-C6). Não é possível cancelar duas vezes.
        /// </summary>
        public bool Cancelar(string usuario)
        {
            if (Status == EStatusDevolucaoCompra.Cancelada)
            {
                AddNotification("Status", "Devolução já cancelada [DEV-013] [Origem: DevolucaoCompra]");
                return false;
            }
            Status = EStatusDevolucaoCompra.Cancelada;
            CanceladaEm = DateTime.UtcNow;
            MarcarAlterado(usuario);
            return true;
        }

        public bool EstaConfirmada() => Status == EStatusDevolucaoCompra.Confirmada;

        public void Validar()
        {
            Clear();
            if (CompraOrigemId == Guid.Empty)
                AddNotification("CompraOrigemId", "A devolução referencia obrigatoriamente uma compra de origem [DEV-001] [Origem: DevolucaoCompra]");
            if (FornecedorId == Guid.Empty)
                AddNotification("FornecedorId", "O fornecedor é obrigatório [DEV-002] [Origem: DevolucaoCompra]");
            if (string.IsNullOrWhiteSpace(Numero))
                AddNotification("Numero", "O número da devolução é obrigatório [DEV-007] [Origem: DevolucaoCompra]");
        }
    }
}
