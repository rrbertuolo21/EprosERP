using System;
using System.Collections.Generic;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>Categoria de meta (EF FIN-PO §12.9 po_meta_categoria).</summary>
    public class MetaCategoria : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string Codigo { get; private set; } = string.Empty;
        public EEscopoMeta Escopo { get; private set; } = EEscopoMeta.Own;

        protected MetaCategoria() { } // EF Core

        public MetaCategoria(string nome, string codigo, EEscopoMeta escopo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome; Codigo = codigo; Escopo = escopo; Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<MetaCategoria>()
                .Requires()
                .IsNotNullOrEmpty(Nome, nameof(Nome), "O nome da categoria é obrigatório [Origem: MetaCategoria]")
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O código da categoria é obrigatório [Origem: MetaCategoria]"));
        }
    }

    /// <summary>Meta financeira/comercial (EF FIN-PO §12.10 po_meta).</summary>
    public class Meta : EntidadeSaaSBase
    {
        public Guid CategoriaId { get; private set; }
        public string Tipo { get; private set; } = string.Empty;
        public string Prioridade { get; private set; } = string.Empty;
        public DateTime DataInicio { get; private set; }
        public DateTime DataAlvo { get; private set; }
        public EStatusMeta Status { get; private set; } = EStatusMeta.Rascunho;

        private readonly List<MetaMilestone> _milestones = new();
        private readonly List<MetaContribuicao> _contribuicoes = new();
        private readonly List<MetaTracking> _trackings = new();
        public IReadOnlyCollection<MetaMilestone> Milestones => _milestones.AsReadOnly();
        public IReadOnlyCollection<MetaContribuicao> Contribuicoes => _contribuicoes.AsReadOnly();
        public IReadOnlyCollection<MetaTracking> Trackings => _trackings.AsReadOnly();

        protected Meta() { } // EF Core

        public Meta(Guid categoriaId, string tipo, string prioridade, DateTime dataInicio, DateTime dataAlvo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CategoriaId = categoriaId; Tipo = tipo; Prioridade = prioridade;
            DataInicio = dataInicio; DataAlvo = dataAlvo;
            Status = EStatusMeta.Rascunho;
            Validar();
        }

        public void Ativar(string usuario)
        {
            if (Status != EStatusMeta.Rascunho) { AddNotification(nameof(Status), "Somente meta em rascunho pode ser ativada."); return; }
            Status = EStatusMeta.Ativa; MarcarAlterado(usuario);
        }

        public void AdicionarMilestone(string? descricao, string tenantId, string criadoPor)
            => _milestones.Add(new MetaMilestone(Id, descricao, tenantId, criadoPor));

        public void RegistrarContribuicao(decimal valor, string tipo, DateTime? data, string tenantId, string criadoPor)
            => _contribuicoes.Add(new MetaContribuicao(Id, valor, tipo, data, tenantId, criadoPor));

        public void RegistrarTracking(decimal percentual, string statusProgresso, DateTime? data, string tenantId, string criadoPor)
            => _trackings.Add(new MetaTracking(Id, percentual, statusProgresso, data, tenantId, criadoPor));

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Meta>()
                .Requires()
                .IsNotEmpty(CategoriaId, nameof(CategoriaId), "A categoria é obrigatória [Origem: Meta]"));
            if (DataAlvo <= DataInicio)
                AddNotification(nameof(DataAlvo), "A data alvo deve ser posterior à data início [Origem: Meta]");
        }
    }

    /// <summary>Milestone de meta (EF FIN-PO §12.11 po_meta_milestone).</summary>
    public class MetaMilestone : EntidadeSaaSBase
    {
        public Guid MetaId { get; private set; }
        public EStatusMilestone Status { get; private set; } = EStatusMilestone.Pendente;
        public string? Descricao { get; private set; }

        public Meta Meta { get; private set; } = null!;

        protected MetaMilestone() { } // EF Core

        public MetaMilestone(Guid metaId, string? descricao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            MetaId = metaId; Descricao = descricao; Status = EStatusMilestone.Pendente;
        }

        public void Concluir(string usuario) { Status = EStatusMilestone.Concluido; MarcarAlterado(usuario); }
    }

    /// <summary>Contribuição para uma meta (EF FIN-PO §12.12 po_meta_contribuicao).</summary>
    public class MetaContribuicao : EntidadeSaaSBase
    {
        public Guid MetaId { get; private set; }
        public decimal Valor { get; private set; }
        public string Tipo { get; private set; } = string.Empty;
        public DateTime? Data { get; private set; }

        public Meta Meta { get; private set; } = null!;

        protected MetaContribuicao() { } // EF Core

        public MetaContribuicao(Guid metaId, decimal valor, string tipo, DateTime? data, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            MetaId = metaId; Valor = valor; Tipo = tipo; Data = data;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<MetaContribuicao>()
                .Requires()
                .IsGreaterThan(Valor, 0, nameof(Valor), "A contribuição deve ser positiva [Origem: MetaContribuicao]"));
        }
    }

    /// <summary>Tracking de progresso de meta (EF FIN-PO §12.13 po_meta_tracking).</summary>
    public class MetaTracking : EntidadeSaaSBase
    {
        public Guid MetaId { get; private set; }
        public decimal Percentual { get; private set; }
        public string StatusProgresso { get; private set; } = string.Empty;
        public DateTime? Data { get; private set; }

        public Meta Meta { get; private set; } = null!;

        protected MetaTracking() { } // EF Core

        public MetaTracking(Guid metaId, decimal percentual, string statusProgresso, DateTime? data, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            MetaId = metaId; Percentual = percentual; StatusProgresso = statusProgresso; Data = data;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<MetaTracking>()
                .Requires()
                .IsGreaterOrEqualsThan(Percentual, 0, nameof(Percentual), "O percentual não pode ser negativo [Origem: MetaTracking]")
                .IsLowerOrEqualsThan(Percentual, 100, nameof(Percentual), "O percentual não pode exceder 100 [Origem: MetaTracking]"));
        }
    }
}
