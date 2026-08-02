using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Linha de contagem do inventário (EF Inventário §16.2). INV-004: exige produto.
    /// Guarda o saldo do sistema (INV-005), até três contagens (INV-006) e a divergência
    /// [DECIDIDO D3] = quantidade_contada − quantidade_sistema. LocalId/Lote (D2/D10) qualificam a posição.
    /// </summary>
    public class InventarioItem : EntidadeSaaSBase
    {
        public Guid InventarioId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public Guid? LocalId { get; private set; }
        public string? Lote { get; private set; }
        public decimal QuantidadeSistema { get; private set; }
        public decimal? Contagem01 { get; private set; }
        public decimal? Contagem02 { get; private set; }
        public decimal? Contagem03 { get; private set; }
        public decimal? QuantidadeContada { get; private set; }
        public bool FechadoContagem { get; private set; }
        /// <summary>[DECIDIDO D3] Divergência = QuantidadeContada − QuantidadeSistema (persistida ao registrar a contagem).</summary>
        public decimal? Divergencia { get; private set; }

        public Inventario? Inventario { get; private set; }

        protected InventarioItem() { } // EF Core

        public InventarioItem(Guid inventarioId, Guid produtoId, decimal quantidadeSistema, Guid? localId, string? lote, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            InventarioId = inventarioId;
            ProdutoId = produtoId;
            QuantidadeSistema = quantidadeSistema;
            LocalId = localId;
            Lote = lote;
            FechadoContagem = false;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<InventarioItem>()
                .Requires()
                .IsNotEmpty(ProdutoId, nameof(ProdutoId), "O produto do item de inventário é obrigatório [INV-004] [Origem: InventarioItem]"));
        }

        /// <summary>
        /// INV-006/007: registra até três contagens e a contagem final, calculando a divergência.
        /// Se <paramref name="quantidadeContada"/> não vier, assume a última contagem preenchida.
        /// </summary>
        public void RegistrarContagem(decimal? contagem01, decimal? contagem02, decimal? contagem03, decimal? quantidadeContada, bool fechar, string usuario)
        {
            if (contagem01.HasValue) Contagem01 = contagem01;
            if (contagem02.HasValue) Contagem02 = contagem02;
            if (contagem03.HasValue) Contagem03 = contagem03;

            var final = quantidadeContada ?? Contagem03 ?? Contagem02 ?? Contagem01;
            if (final.HasValue)
            {
                QuantidadeContada = final;
                Divergencia = final.Value - QuantidadeSistema; // [D3]
            }
            FechadoContagem = fechar;
            MarcarAlterado(usuario);
        }

        /// <summary>True quando o item foi contado e não há divergência (base da acurácia agregada, D14).</summary>
        public bool SemDivergencia() => QuantidadeContada.HasValue && Divergencia.HasValue && Divergencia.Value == 0m;
    }
}
