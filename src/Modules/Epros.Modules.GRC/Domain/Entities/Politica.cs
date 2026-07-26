using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-POL — Politica corporativa (grc_pol_politica). Entidade raiz do ciclo de vida
    /// de politicas: cadastro, workflow de publicacao, versionamento e aceite.
    /// Fiel a EF_13_GRC_GESTAO_DE_POLITICAS_V1 (secoes 10.2 e 11.1).
    /// </summary>
    public class Politica : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Titulo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public string? Categoria { get; private set; } // Conduta, Preco, Aprovacao, Privacidade...
        public Guid OwnerId { get; private set; }
        public string? ModuloAplicavel { get; private set; } // RH, Vendas, Financeiro, GRC...
        // Rascunho, EmAnalise, RevisaoJuridica, Aprovado, Publicado, Suspenso, Arquivado, Inativo
        public string Status { get; private set; } = "Rascunho";
        public DateTime DataCriacao { get; private set; }
        public string? MotivoUltimaTransicao { get; private set; }

        protected Politica() { } // EF Core

        public Politica(
            string codigo,
            string titulo,
            string descricao,
            string? categoria,
            Guid ownerId,
            string? moduloAplicavel,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Politica>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O codigo da politica e obrigatorio.")
                .IsNotNullOrEmpty(titulo, nameof(Titulo), "O titulo da politica e obrigatorio.")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descricao da politica e obrigatoria.")
                .IsTrue(ownerId != Guid.Empty, nameof(OwnerId), "O owner (responsavel) da politica e obrigatorio.")
            );

            Codigo = codigo;
            Titulo = titulo;
            Descricao = descricao;
            Categoria = categoria;
            OwnerId = ownerId;
            ModuloAplicavel = moduloAplicavel;
            Status = "Rascunho";
            DataCriacao = DateTime.UtcNow;
        }

        public void Submeter(string usuario)
        {
            if (Status != "Rascunho")
            {
                AddNotification(nameof(Status), "Somente politicas em rascunho podem ser submetidas para analise.");
                return;
            }
            Status = "EmAnalise";
            MarcarAlterado(usuario);
        }

        public void Aprovar(string usuario)
        {
            if (Status != "EmAnalise" && Status != "RevisaoJuridica")
            {
                AddNotification(nameof(Status), "Somente politicas em analise ou revisao juridica podem ser aprovadas.");
                return;
            }
            Status = "Aprovado";
            MarcarAlterado(usuario);
        }

        /// <summary>RN-POL-004: politica publicada deve possuir versao vigente (validado no handler).</summary>
        public void Publicar(string usuario)
        {
            if (Status != "Aprovado")
            {
                AddNotification(nameof(Status), "Somente politicas aprovadas podem ser publicadas.");
                return;
            }
            Status = "Publicado";
            MarcarAlterado(usuario);
        }

        /// <summary>RN-POL-009: suspensao exige motivo.</summary>
        public void Suspender(string motivo, string usuario)
        {
            if (Status != "Publicado")
            {
                AddNotification(nameof(Status), "Somente politicas publicadas podem ser suspensas.");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoUltimaTransicao), "O motivo da suspensao e obrigatorio.");
                return;
            }
            Status = "Suspenso";
            MotivoUltimaTransicao = motivo;
            MarcarAlterado(usuario);
        }

        /// <summary>RN-POL-008/009: arquivamento mantem aceites e exige motivo.</summary>
        public void Arquivar(string motivo, string usuario)
        {
            if (Status == "Arquivado" || Status == "Inativo")
            {
                AddNotification(nameof(Status), "A politica ja esta arquivada ou inativa.");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoUltimaTransicao), "O motivo do arquivamento e obrigatorio.");
                return;
            }
            Status = "Arquivado";
            MotivoUltimaTransicao = motivo;
            MarcarAlterado(usuario);
        }
    }
}
