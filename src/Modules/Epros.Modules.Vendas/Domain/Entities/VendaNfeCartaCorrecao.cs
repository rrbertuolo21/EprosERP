using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaNfeCartaCorrecao (evento de carta de correção da NF-e).
    /// FK long -> Guid; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaNfeCartaCorrecao : EntidadeSaaSBase
    {
        public Guid VendaNfeId { get; private set; }
        public string TextoCorrecao { get; private set; } = string.Empty;
        public int SequenciaEvento { get; private set; }
        public int StatusSefaz { get; private set; } = 100;
        public string? MotivoRejeicaoSefaz { get; private set; }

        // Navegação intra-módulo
        public VendaNfe VendaNfe { get; private set; } = null!;

        protected VendaNfeCartaCorrecao() { } // EF Core

        public VendaNfeCartaCorrecao(Guid vendaNfeId, string textoCorrecao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            VendaNfeId = vendaNfeId;
            TextoCorrecao = textoCorrecao;
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
