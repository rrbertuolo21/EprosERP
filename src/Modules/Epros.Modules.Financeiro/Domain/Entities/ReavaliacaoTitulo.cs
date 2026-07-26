using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Reavaliação de títulos em moeda estrangeira (EF FIN-CAM §10.6 cam_reavaliacao_titulo).
    /// Agrega itens (cam_reavaliacao_item) e totaliza variação cambial.
    /// </summary>
    public class ReavaliacaoTitulo : EntidadeSaaSBase
    {
        public DateTime DataReavaliacao { get; private set; }
        public EStatusReavaliacaoTitulo Status { get; private set; } = EStatusReavaliacaoTitulo.Rascunho;
        public decimal TotalValorOriginal { get; private set; }
        public decimal TotalValorReavaliado { get; private set; }
        public decimal TotalVariacao { get; private set; }
        public string? Observacao { get; private set; }

        private readonly List<ReavaliacaoItem> _itens = new();
        public IReadOnlyCollection<ReavaliacaoItem> Itens => _itens.AsReadOnly();

        protected ReavaliacaoTitulo() { } // EF Core

        public ReavaliacaoTitulo(DateTime dataReavaliacao, string? observacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            DataReavaliacao = dataReavaliacao;
            Observacao = observacao;
            Status = EStatusReavaliacaoTitulo.Rascunho;
            Validar();
        }

        public void AdicionarItem(Guid moedaId, string? tituloTipo, Guid? tituloId, Guid taxaCambioId,
            decimal valorOriginalMoeda, decimal valorReavaliadoBase, string tenantId, string criadoPor)
        {
            var item = new ReavaliacaoItem(Id, moedaId, tituloTipo, tituloId, taxaCambioId,
                valorOriginalMoeda, valorReavaliadoBase, tenantId, criadoPor);
            _itens.Add(item);
            Recalcular();
        }

        private void Recalcular()
        {
            TotalValorOriginal = _itens.Sum(i => i.ValorOriginalMoeda);
            TotalValorReavaliado = _itens.Sum(i => i.ValorReavaliadoBase);
            TotalVariacao = _itens.Sum(i => i.ValorVariacao);
        }

        public void Aprovar(string usuario)
        {
            if (Status != EStatusReavaliacaoTitulo.Rascunho)
            {
                AddNotification(nameof(Status), "Somente reavaliação em rascunho pode ser aprovada.");
                return;
            }
            Status = EStatusReavaliacaoTitulo.Aprovada;
            MarcarAlterado(usuario);
        }

        public void Contabilizar(string usuario)
        {
            if (Status != EStatusReavaliacaoTitulo.Aprovada)
            {
                AddNotification(nameof(Status), "Somente reavaliação aprovada pode ser contabilizada.");
                return;
            }
            Status = EStatusReavaliacaoTitulo.Contabilizada;
            MarcarAlterado(usuario);
        }

        public void Cancelar(string usuario)
        {
            if (Status == EStatusReavaliacaoTitulo.Contabilizada)
            {
                AddNotification(nameof(Status), "Reavaliação contabilizada não pode ser cancelada.");
                return;
            }
            Status = EStatusReavaliacaoTitulo.Cancelada;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ReavaliacaoTitulo>()
                .Requires()
                .IsGreaterThan(DataReavaliacao, DateTime.MinValue, nameof(DataReavaliacao), "A data da reavaliação é obrigatória [Origem: ReavaliacaoTitulo]")
            );
        }
    }
}
