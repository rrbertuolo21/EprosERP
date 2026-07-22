using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Grupo de produtos. Porte fiel do legado Epros.ERP.Domain.Entities.Cadastros.Produtos.ProdutoGrupo.
    /// A relação N:N com Empresa do legado (ICollection&lt;Empresa&gt;) é restaurada por uma coleção de
    /// EmpresaId (Guid) via a entidade de junção ProdutoGrupoEmpresa, sem navegação cross-module.
    /// As coleções de Produtos/Marcas/Categorias do legado viram relacionamentos por FK Guid nas entidades filhas.
    /// </summary>
    public class ProdutoGrupo : EntidadeSaaSBase
    {
        // Sequência de exibição/UX (porte do legado SequenciaTenantId — exceção oficial, nome fixo).
        public long? SequenciaExibicao { get; private set; }

        public string Descricao { get; private set; } = string.Empty;

        // Escopo por Empresa (N:N legado) — coleção de EmpresaId via junção intra-módulo.
        public ICollection<ProdutoGrupoEmpresa> Empresas { get; private set; } = new List<ProdutoGrupoEmpresa>();

        protected ProdutoGrupo() { } // EF Core

        public ProdutoGrupo(string descricao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Descricao = descricao;
            Validar();
        }

        public void Validar()
        {
            AddNotifications(new Contract<ProdutoGrupo>()
                .Requires()
                .IsLowerOrEqualsThan((Descricao ?? "").Length, 100, nameof(Descricao), "O campo Descricao deve ter no máximo 100 caracteres [Origem: ProdutoGrupo]")
            );
        }

        public void Alterar(string descricao, string usuario)
        {
            Descricao = descricao;
            MarcarAlterado(usuario);
            Validar();
        }

        /// <summary>Define a sequência de exibição/UX (porte do legado SequenciaTenantId).</summary>
        public void AtualizarSequenciaExibicao(long sequenciaExibicao)
        {
            SequenciaExibicao = sequenciaExibicao;
        }

        /// <summary>Vincula uma empresa (EmpresaId) ao grupo (N:N do legado), sem duplicar.</summary>
        public void VincularEmpresa(Guid empresaId, string usuario)
        {
            if (Empresas.Any(e => e.EmpresaId == empresaId))
                return;

            Empresas.Add(new ProdutoGrupoEmpresa(Id, empresaId, TenantId, usuario));
        }

        /// <summary>Desvincula uma empresa do grupo (soft delete da junção).</summary>
        public void DesvincularEmpresa(Guid empresaId, string usuario)
        {
            var vinculo = Empresas.FirstOrDefault(e => e.EmpresaId == empresaId);
            vinculo?.Deletar(usuario);
        }
    }
}
