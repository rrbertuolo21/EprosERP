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

        // Navegação intra-módulo
        public WmsArmazem? Armazem { get; private set; }

        protected WmsEnderecoOperacional() { } // EF Core

        public WmsEnderecoOperacional(Guid armazemId, string? rua, string? estante, string? caixa, bool ativo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ArmazemId = armazemId;
            Rua = rua;
            Estante = estante;
            Caixa = caixa;
            Ativo = ativo;
        }

        public void Alterar(string? rua, string? estante, string? caixa, bool ativo, string alteradoPor)
        {
            Rua = rua;
            Estante = estante;
            Caixa = caixa;
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
        }
    }
}
