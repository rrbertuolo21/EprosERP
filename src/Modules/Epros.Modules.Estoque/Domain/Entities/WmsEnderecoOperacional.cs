using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Endereço operacional interno do armazém — rua/estante/caixa (EF Gestão de Armazém WMS §16.4
    /// `wms_endereco_operacional`). Cobertura parcial conforme lacuna controlada §21 (slotting/FEFO/coletor
    /// permanecem fora do escopo comprovado).
    /// </summary>
    public class WmsEnderecoOperacional : EntidadeSaaSBase
    {
        public Guid ArmazemId { get; private set; }
        public string? Rua { get; private set; }
        public string? Estante { get; private set; }
        public string? Caixa { get; private set; }
        public bool Ativo { get; private set; } = true;

        // D5 (WMS operacional) — endereçamento rico: coordenada física da posição para roteiro de
        // separação/armazenagem. Aditivo/opcional; endereços legados permanecem válidos (tudo null).
        public string? Zona { get; private set; }
        public string? Nivel { get; private set; }
        public string? Posicao { get; private set; }
        /// <summary>Capacidade máxima da posição (na UM do produto). Null = sem restrição declarada.</summary>
        public decimal? CapacidadeMaxima { get; private set; }

        // Navegação intra-módulo
        public WmsArmazem? Armazem { get; private set; }

        protected WmsEnderecoOperacional() { } // EF Core

        public WmsEnderecoOperacional(Guid armazemId, string? rua, string? estante, string? caixa, bool ativo, string tenantId, string criadoPor,
            string? zona = null, string? nivel = null, string? posicao = null, decimal? capacidadeMaxima = null)
            : base(tenantId, criadoPor)
        {
            ArmazemId = armazemId;
            Rua = rua;
            Estante = estante;
            Caixa = caixa;
            Ativo = ativo;
            Zona = zona;
            Nivel = nivel;
            Posicao = posicao;
            CapacidadeMaxima = capacidadeMaxima;
        }

        public void Alterar(string? rua, string? estante, string? caixa, bool ativo, string alteradoPor,
            string? zona = null, string? nivel = null, string? posicao = null, decimal? capacidadeMaxima = null)
        {
            Rua = rua;
            Estante = estante;
            Caixa = caixa;
            Ativo = ativo;
            Zona = zona;
            Nivel = nivel;
            Posicao = posicao;
            CapacidadeMaxima = capacidadeMaxima;
            MarcarAlterado(alteradoPor);
        }
    }
}
