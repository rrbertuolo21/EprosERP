using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-CIA — Achado de auditoria (grc_cia_achado). Registra falha, severidade e prazo de
    /// remediacao. Fiel a EF_13_GRC_CONTROLES_INTERNOS_E_AUDITORIA_V1 (secoes 10.2, 10.4 e 12).
    /// </summary>
    public class Achado : EntidadeSaaSBase
    {
        public Guid? TesteControleId { get; private set; }
        public string Titulo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        // Baixa, Media, Alta, Critica
        public string Severidade { get; private set; } = "Media";
        public DateTime? PrazoRemediacao { get; private set; }
        // Aberto, EmAnalise, EmRemediacao, Encerrado, Atrasado, Cancelado
        public string Status { get; private set; } = "Aberto";
        public string? MotivoEncerramento { get; private set; }

        protected Achado() { } // EF Core

        public Achado(
            Guid? testeControleId,
            string titulo,
            string descricao,
            string severidade,
            DateTime? prazoRemediacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Achado>()
                .Requires()
                .IsNotNullOrEmpty(titulo, nameof(Titulo), "O titulo do achado e obrigatorio.")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descricao do achado e obrigatoria.")
                .IsTrue(severidade == "Baixa" || severidade == "Media" || severidade == "Alta" || severidade == "Critica",
                    nameof(Severidade), "A severidade deve ser 'Baixa', 'Media', 'Alta' ou 'Critica'.")
            );

            TesteControleId = testeControleId;
            Titulo = titulo;
            Descricao = descricao;
            Severidade = severidade;
            PrazoRemediacao = prazoRemediacao;
            Status = "Aberto";
        }

        public void ColocarEmAnalise(string usuario)
        {
            if (Status != "Aberto")
            {
                AddNotification(nameof(Status), "Somente achados abertos podem ir para analise.");
                return;
            }
            Status = "EmAnalise";
            MarcarAlterado(usuario);
        }

        public void IniciarRemediacao(string usuario)
        {
            if (Status != "Aberto" && Status != "EmAnalise")
            {
                AddNotification(nameof(Status), "Somente achados abertos ou em analise podem entrar em remediacao.");
                return;
            }
            Status = "EmRemediacao";
            MarcarAlterado(usuario);
        }

        public void Encerrar(string motivo, string usuario)
        {
            if (Status != "EmRemediacao" && Status != "Atrasado")
            {
                AddNotification(nameof(Status), "Somente achados em remediacao ou atrasados podem ser encerrados.");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoEncerramento), "O motivo do encerramento do achado e obrigatorio.");
                return;
            }
            Status = "Encerrado";
            MotivoEncerramento = motivo;
            MarcarAlterado(usuario);
        }

        public void Cancelar(string usuario)
        {
            if (Status == "Encerrado")
            {
                AddNotification(nameof(Status), "Nao e possivel cancelar um achado ja encerrado.");
                return;
            }
            Status = "Cancelado";
            MarcarAlterado(usuario);
        }
    }
}
