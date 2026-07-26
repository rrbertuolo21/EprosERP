using System;
using System.Collections.Generic;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Grupo de consolidação (holding lógica) que agrega empresas do tenant (EF FIN-CON §10.1 con_grupo_consolidacao).
    /// Submódulo de evolução — sobe desabilitado (ABAC nega por padrão). Isolamento por tenant via ContextBase.
    /// </summary>
    public class GrupoConsolidacao : EntidadeSaaSBase
    {
        public string? Codigo { get; private set; }
        public string? Nome { get; private set; }
        public string? Descricao { get; private set; }
        public EEstadoConsolidacao Situacao { get; private set; } = EEstadoConsolidacao.Provisorio;

        private readonly List<GrupoEmpresa> _empresas = new();
        public IReadOnlyCollection<GrupoEmpresa> Empresas => _empresas.AsReadOnly();

        protected GrupoConsolidacao() { } // EF Core

        public GrupoConsolidacao(string? codigo, string? nome, string? descricao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Nome = nome;
            Descricao = descricao;
            Situacao = EEstadoConsolidacao.Provisorio;
            Validar();
        }

        public void Alterar(string? codigo, string? nome, string? descricao, string usuario)
        {
            Codigo = codigo;
            Nome = nome;
            Descricao = descricao;
            MarcarAlterado(usuario);
            Validar();
        }

        public void AdicionarEmpresa(Guid empresaId, DateTime? dataInicio, DateTime? dataFim, string tenantId, string criadoPor)
        {
            _empresas.Add(new GrupoEmpresa(Id, empresaId, dataInicio, dataFim, tenantId, criadoPor));
        }

        public void Publicar(string usuario)
        {
            Situacao = EEstadoConsolidacao.Publicado;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            // Nome/Codigo não confirmados como obrigatórios no material — validação mínima do agregado.
        }
    }

    /// <summary>Vínculo empresa ↔ grupo de consolidação (EF FIN-CON §10.2 con_grupo_empresa). Empresa cross-module por Guid FK.</summary>
    public class GrupoEmpresa : EntidadeSaaSBase
    {
        public Guid GrupoConsolidacaoId { get; private set; }
        public Guid EmpresaId { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }

        public GrupoConsolidacao GrupoConsolidacao { get; private set; } = null!;

        protected GrupoEmpresa() { } // EF Core

        public GrupoEmpresa(Guid grupoConsolidacaoId, Guid empresaId, DateTime? dataInicio, DateTime? dataFim, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            GrupoConsolidacaoId = grupoConsolidacaoId;
            EmpresaId = empresaId;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<GrupoEmpresa>()
                .Requires()
                .IsNotEmpty(EmpresaId, nameof(EmpresaId), "A empresa é obrigatória [Origem: GrupoEmpresa]"));
        }
    }
}
