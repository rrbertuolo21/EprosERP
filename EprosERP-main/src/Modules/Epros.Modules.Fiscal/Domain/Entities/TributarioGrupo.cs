using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class TributarioGrupo : EntidadeSaaSBase
    {
        // Legado: SequenciaTenantId (somente exibição/UX).
        public long? SequenciaExibicao { get; private set; }
        public string Descricao { get; private set; } = string.Empty;

        // N:N com Empresa (outro módulo) — legado tinha ICollection<Empresa> Empresas.
        // Restaurado por coleção de vínculos com Guid EmpresaId (sem navegação cross-module).
        public ICollection<TributarioGrupoEmpresa> Empresas { get; private set; } = new List<TributarioGrupoEmpresa>();

        protected TributarioGrupo() { } // EF Core

        public TributarioGrupo(string descricao, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            Descricao = descricao;
            Validar();
        }

        public void Alterar(string descricao, string alteradoPor)
        {
            Descricao = descricao;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void AdicionarEmpresa(Guid empresaId, string criadoPor)
        {
            if (Empresas.Any(e => e.EmpresaId == empresaId && e.DeletadoEm == null))
                return;

            var vinculo = new TributarioGrupoEmpresa(Id, empresaId, TenantId, criadoPor);
            Empresas.Add(vinculo);

            if (vinculo.Notifications.Any())
                AddNotifications(vinculo.Notifications);
        }

        public void RemoverEmpresa(Guid empresaId, string deletadoPor)
        {
            var vinculo = Empresas.FirstOrDefault(e => e.EmpresaId == empresaId && e.DeletadoEm == null);
            vinculo?.Deletar(deletadoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<TributarioGrupo>()
                .Requires()
                .IsLowerOrEqualsThan((Descricao ?? "").Length, 100, nameof(Descricao), "O campo Descricao deve ter no máximo 100 caracteres [Origem: TributarioGrupo]")
            );
        }
    }
}
