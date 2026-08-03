using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Instância de aprovação por alçada de compras (CD3 / EF SOURCING §6.2 `com_pedido_aprovacao`).
    /// Representa o ciclo de aprovação multi-nível de uma origem (pedido de compra, compra ou contrato):
    /// os níveis aplicáveis são montados a partir das <see cref="ComprasAlcadaRegra"/> vigentes; cada
    /// nível aprova/reprova em ordem. Reprovação em QUALQUER nível bloqueia o pedido (SRC-008). Só
    /// quando todos os níveis aprovam a origem fica apta a prosseguir (originar/faturar a compra).
    ///
    /// Sem nível aplicável (valor abaixo de toda faixa) → não há alçada a exercer: nasce já Aprovado.
    /// </summary>
    public class ComprasPedidoAprovacao : EntidadeSaaSBase
    {
        public EOrigemAprovacaoCompra OrigemTipo { get; private set; }
        public Guid OrigemId { get; private set; }
        public decimal ValorTotal { get; private set; }
        public Guid? CompradorId { get; private set; }
        public string? CategoriaCompra { get; private set; }
        public EStatusPedidoAprovacaoCompra Status { get; private set; } = EStatusPedidoAprovacaoCompra.Pendente;
        public int NivelAtual { get; private set; }
        public int QuantidadeNiveis { get; private set; }
        public DateTime? DecididoEm { get; private set; }

        public ICollection<ComprasPedidoAprovacaoNivel> Niveis { get; private set; } = new List<ComprasPedidoAprovacaoNivel>();

        protected ComprasPedidoAprovacao() { } // EF Core

        public ComprasPedidoAprovacao(
            EOrigemAprovacaoCompra origemTipo,
            Guid origemId,
            decimal valorTotal,
            Guid? compradorId,
            string? categoriaCompra,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            OrigemTipo = origemTipo;
            OrigemId = origemId;
            ValorTotal = valorTotal;
            CompradorId = compradorId;
            CategoriaCompra = string.IsNullOrWhiteSpace(categoriaCompra) ? null : categoriaCompra.Trim();
            Status = EStatusPedidoAprovacaoCompra.Pendente;
            NivelAtual = 0;
            Validar();
        }

        public void AdicionarNivel(ComprasPedidoAprovacaoNivel nivel) => Niveis.Add(nivel);

        /// <summary>
        /// Conclui a montagem dos níveis: define a quantidade e o nível corrente. Sem níveis → nasce
        /// Aprovado (não há alçada a exercer). Deve ser chamado após adicionar todos os níveis.
        /// </summary>
        public void FinalizarMontagem(string usuario)
        {
            QuantidadeNiveis = Niveis.Count;
            if (QuantidadeNiveis == 0)
            {
                Status = EStatusPedidoAprovacaoCompra.Aprovado;
                DecididoEm = DateTime.UtcNow;
                NivelAtual = 0;
            }
            else
            {
                NivelAtual = Niveis.OrderBy(n => n.Nivel).First().Nivel;
            }
            MarcarAlterado(usuario);
        }

        private ComprasPedidoAprovacaoNivel? ProximoNivelPendente() =>
            Niveis.Where(n => n.EstaPendente()).OrderBy(n => n.Nivel).FirstOrDefault();

        /// <summary>Aprova o nível corrente. Se era o último pendente, o pedido fica Aprovado.</summary>
        public bool Aprovar(string usuario, string? justificativa)
        {
            if (Status != EStatusPedidoAprovacaoCompra.Pendente)
            {
                AddNotification("Status", "Apenas pedidos de aprovação pendentes podem ser decididos [ALC-010] [Origem: ComprasPedidoAprovacao]");
                return false;
            }

            var nivel = ProximoNivelPendente();
            if (nivel == null)
            {
                AddNotification("Niveis", "Não há nível pendente para aprovar [ALC-011] [Origem: ComprasPedidoAprovacao]");
                return false;
            }

            nivel.Aprovar(usuario, justificativa);

            var proximo = ProximoNivelPendente();
            if (proximo == null)
            {
                Status = EStatusPedidoAprovacaoCompra.Aprovado;
                DecididoEm = DateTime.UtcNow;
            }
            else
            {
                NivelAtual = proximo.Nivel;
            }
            MarcarAlterado(usuario);
            return true;
        }

        /// <summary>Reprova o nível corrente: bloqueia o pedido inteiro (SRC-008).</summary>
        public bool Reprovar(string usuario, string? justificativa)
        {
            if (Status != EStatusPedidoAprovacaoCompra.Pendente)
            {
                AddNotification("Status", "Apenas pedidos de aprovação pendentes podem ser decididos [ALC-010] [Origem: ComprasPedidoAprovacao]");
                return false;
            }

            var nivel = ProximoNivelPendente();
            if (nivel == null)
            {
                AddNotification("Niveis", "Não há nível pendente para reprovar [ALC-011] [Origem: ComprasPedidoAprovacao]");
                return false;
            }

            nivel.Reprovar(usuario, justificativa);
            Status = EStatusPedidoAprovacaoCompra.Reprovado;
            DecididoEm = DateTime.UtcNow;
            MarcarAlterado(usuario);
            return true;
        }

        public bool Cancelar(string usuario)
        {
            if (Status == EStatusPedidoAprovacaoCompra.Aprovado || Status == EStatusPedidoAprovacaoCompra.Reprovado)
            {
                AddNotification("Status", "Pedido de aprovação já decidido não pode ser cancelado [ALC-012] [Origem: ComprasPedidoAprovacao]");
                return false;
            }
            Status = EStatusPedidoAprovacaoCompra.Cancelado;
            DecididoEm = DateTime.UtcNow;
            MarcarAlterado(usuario);
            return true;
        }

        public bool EstaConcluido() =>
            Status == EStatusPedidoAprovacaoCompra.Aprovado ||
            Status == EStatusPedidoAprovacaoCompra.Reprovado ||
            Status == EStatusPedidoAprovacaoCompra.Cancelado;

        public bool FoiAprovado() => Status == EStatusPedidoAprovacaoCompra.Aprovado;

        public void Validar()
        {
            Clear();
            if (OrigemId == Guid.Empty)
                AddNotification("OrigemId", "A origem da aprovação é obrigatória [ALC-005] [Origem: ComprasPedidoAprovacao]");
            if (ValorTotal < 0)
                AddNotification("ValorTotal", "O valor total da aprovação não pode ser negativo [ALC-006] [Origem: ComprasPedidoAprovacao]");
        }
    }
}
