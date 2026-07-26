using System;
using System.Collections.Generic;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Demonstrativo consolidado de um grupo por período (EF FIN-CON §10.3 con_demonstrativo).
    /// Provisório → Publicado. Agrega linhas (con_demonstrativo_linha).
    /// </summary>
    public class Demonstrativo : EntidadeSaaSBase
    {
        public Guid GrupoConsolidacaoId { get; private set; }
        public string Periodo { get; private set; } = string.Empty;
        public EEstadoConsolidacao Estado { get; private set; } = EEstadoConsolidacao.Provisorio;
        public DateTime? DataPublicacao { get; private set; }
        public Guid? UsuarioPublicacaoId { get; private set; }
        public decimal? TotalAgregado { get; private set; }

        private readonly List<DemonstrativoLinha> _linhas = new();
        public IReadOnlyCollection<DemonstrativoLinha> Linhas => _linhas.AsReadOnly();

        protected Demonstrativo() { } // EF Core

        public Demonstrativo(Guid grupoConsolidacaoId, string periodo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            GrupoConsolidacaoId = grupoConsolidacaoId;
            Periodo = periodo;
            Estado = EEstadoConsolidacao.Provisorio;
            Validar();
        }

        public void AdicionarLinha(int? ordem, string? codigoLinha, string? descricaoLinha, decimal? valor, string? tipoLinha, string tenantId, string criadoPor)
        {
            _linhas.Add(new DemonstrativoLinha(Id, ordem, codigoLinha, descricaoLinha, valor, tipoLinha, tenantId, criadoPor));
        }

        public void Publicar(Guid? usuarioPublicacaoId, string usuario)
        {
            if (Estado == EEstadoConsolidacao.Publicado)
            {
                AddNotification(nameof(Estado), "Demonstrativo já publicado.");
                return;
            }
            Estado = EEstadoConsolidacao.Publicado;
            DataPublicacao = DateTime.UtcNow;
            UsuarioPublicacaoId = usuarioPublicacaoId;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Demonstrativo>()
                .Requires()
                .IsNotEmpty(GrupoConsolidacaoId, nameof(GrupoConsolidacaoId), "O grupo de consolidação é obrigatório [Origem: Demonstrativo]")
                .IsNotNullOrEmpty(Periodo, nameof(Periodo), "O período é obrigatório [Origem: Demonstrativo]"));
        }
    }

    /// <summary>Linha de demonstrativo consolidado (EF FIN-CON §10.4 con_demonstrativo_linha).</summary>
    public class DemonstrativoLinha : EntidadeSaaSBase
    {
        public Guid DemonstrativoId { get; private set; }
        public int? Ordem { get; private set; }
        public string? CodigoLinha { get; private set; }
        public string? DescricaoLinha { get; private set; }
        public decimal? Valor { get; private set; }
        public string? TipoLinha { get; private set; }

        public Demonstrativo Demonstrativo { get; private set; } = null!;

        protected DemonstrativoLinha() { } // EF Core

        public DemonstrativoLinha(Guid demonstrativoId, int? ordem, string? codigoLinha, string? descricaoLinha, decimal? valor, string? tipoLinha, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            DemonstrativoId = demonstrativoId;
            Ordem = ordem;
            CodigoLinha = codigoLinha;
            DescricaoLinha = descricaoLinha;
            Valor = valor;
            TipoLinha = tipoLinha;
        }
    }
}
