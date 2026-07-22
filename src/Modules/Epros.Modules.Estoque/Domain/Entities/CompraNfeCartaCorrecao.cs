using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Carta de correção de uma CompraNfe. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraNfeCartaCorrecao.
    /// </summary>
    public class CompraNfeCartaCorrecao : EntidadeSaaSBase
    {
        public Guid CompraNfeId { get; private set; }
        public string TextoCorrecao { get; private set; } = string.Empty;
        public int SequenciaEvento { get; private set; }
        public int StatusSefaz { get; private set; } = 100;
        public string? MotivoRejeicaoSefaz { get; private set; }

        // Navegação intra-módulo
        public CompraNfe? CompraNfe { get; private set; }

        protected CompraNfeCartaCorrecao() { } // EF Core

        public CompraNfeCartaCorrecao(Guid compraNfeId, string textoCorrecao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraNfeId = compraNfeId;
            TextoCorrecao = textoCorrecao ?? string.Empty;
        }

        public void Corrigir(int sequenciaEvento)
        {
            SequenciaEvento = sequenciaEvento;
            StatusSefaz = 128;
        }

        public void Rejeitar(int sequenciaEvento, int statusSefaz, string? motivoRejeicaoSefaz)
        {
            SequenciaEvento = sequenciaEvento;
            StatusSefaz = statusSefaz;
            MotivoRejeicaoSefaz = motivoRejeicaoSefaz;
        }
    }
}
