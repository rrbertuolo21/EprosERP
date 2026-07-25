using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-BOM — Agrupamento visual/funcional de componentes (prd_bom_grupo_componente).</summary>
    public class BomGrupoComponente : EntidadeSaaSBase
    {
        public string? Nome { get; private set; }
        public string? Descricao { get; private set; }
        public EStatusAtivoInativo Status { get; private set; } = EStatusAtivoInativo.Ativo;

        protected BomGrupoComponente() { } // EF Core

        public BomGrupoComponente(string? nome, string? descricao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Descricao = descricao;
            Status = EStatusAtivoInativo.Ativo;
        }

        public void Alterar(string? nome, string? descricao, string alteradoPor)
        {
            Nome = nome;
            Descricao = descricao;
            MarcarAlterado(alteradoPor);
        }

        public void Inativar(string alteradoPor)
        {
            Status = EStatusAtivoInativo.Inativo;
            MarcarAlterado(alteradoPor);
        }
    }
}
