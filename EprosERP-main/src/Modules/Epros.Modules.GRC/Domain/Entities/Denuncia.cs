using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-DEN — Denuncia/investigacao (grc_den_denuncia, tabela "denuncias").
    /// Agregado principal do canal de denuncias. Fiel a
    /// EF_13_GRC_INVESTIGACOES_E_DENUNCIAS_V1 (secoes 10.2, 11.2 e 12).
    /// Expandido de forma aditiva: os campos e metodos legados
    /// (CodigoAcompanhamento, Relato, Julgar) foram preservados.
    /// </summary>
    public class Denuncia : EntidadeSaaSBase
    {
        // Protocolo unico por tenant (origem material: ticket_id). Preservado como CodigoAcompanhamento.
        public string CodigoAcompanhamento { get; private set; } = string.Empty;
        public string Relato { get; private set; } = string.Empty;
        // Recebida, Rascunho, Triagem, EmAnalise, Ativo, Investigacao, Conclusao, Suspenso, Encerrado, Inativo,
        // Procedente, Improcedente (estados de julgamento legados)
        public string Status { get; private set; } = "Recebida";
        public DateTime DataRegistro { get; private set; }
        public string? ParecerFinal { get; private set; }

        // --- Campos EF (secao 11.2) ---
        public string? Titulo { get; private set; } // origem material: title
        public string? Prioridade { get; private set; } // origem material: priority
        public Guid? CategoriaId { get; private set; } // origem material: category_id
        public bool Anonima { get; private set; } // canal anonimo
        public string? TokenAcompanhamentoHash { get; private set; } // acompanhamento anonimo
        public DateTime? ResolvedAt { get; private set; } // origem material: resolved_at (RN-DEN-001)

        protected Denuncia() { } // EF Core

        // Construtor legado (canal anonimo simples) — preservado para nao quebrar o fluxo Frente 1.
        public Denuncia(
            string relato,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Denuncia>()
                .Requires()
                .IsNotNullOrEmpty(relato, nameof(Relato), "O relato detalhado da denúncia é obrigatório.")
            );

            Relato = relato;
            Status = "Recebida";
            DataRegistro = DateTime.UtcNow;
            Anonima = true;
            // Gera um código de acompanhamento aleatório e curto (protocolo)
            CodigoAcompanhamento = $"DEN-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }

        // Construtor detalhado (EF secao 7.1 / 11.2).
        public Denuncia(
            string? titulo,
            string relato,
            Guid? categoriaId,
            string? prioridade,
            bool anonima,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Denuncia>()
                .Requires()
                .IsNotNullOrEmpty(relato, nameof(Relato), "O relato detalhado da denúncia é obrigatório.")
            );

            Titulo = titulo;
            Relato = relato;
            CategoriaId = categoriaId;
            Prioridade = prioridade;
            Anonima = anonima;
            Status = "Recebida";
            DataRegistro = DateTime.UtcNow;
            CodigoAcompanhamento = $"DEN-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }

        /// <summary>Define o hash do token de acompanhamento (denuncia anonima).</summary>
        public void DefinirTokenAcompanhamento(string tokenHash)
        {
            TokenAcompanhamentoHash = tokenHash;
        }

        public void IniciarAnalise(string usuario)
        {
            if (Status != "Recebida")
            {
                AddNotification(nameof(Status), "A denúncia já está em análise ou já foi julgada.");
                return;
            }

            Status = "EmAnalise";
            MarcarAlterado(usuario);
        }

        /// <summary>7.3 Triagem: classifica categoria/prioridade e move para Triagem.</summary>
        public void Triar(Guid? categoriaId, string? prioridade, string usuario)
        {
            if (Status != "Recebida" && Status != "Rascunho" && Status != "Triagem")
            {
                AddNotification(nameof(Status), "Somente denúncias recebidas ou em triagem podem ser triadas.");
                return;
            }

            if (categoriaId.HasValue) CategoriaId = categoriaId;
            if (!string.IsNullOrWhiteSpace(prioridade)) Prioridade = prioridade;
            Status = "Triagem";
            MarcarAlterado(usuario);
        }

        /// <summary>Move a denuncia para investigacao ativa apos atribuicao de investigador.</summary>
        public void ColocarEmInvestigacao(string usuario)
        {
            if (Status != "Triagem" && Status != "EmAnalise" && Status != "Recebida" && Status != "Ativo")
            {
                AddNotification(nameof(Status), "A denúncia precisa estar triada para entrar em investigação.");
                return;
            }
            Status = "Investigacao";
            MarcarAlterado(usuario);
        }

        /// <summary>7.6 Conclusao e encerramento. RN-DEN-001: resolved_at deve ser data/hora valida.</summary>
        public void Concluir(DateTime resolvedAt, string? parecerFinal, string usuario)
        {
            if (Status == "Encerrado" || Status == "Inativo")
            {
                AddNotification(nameof(Status), "A denúncia já está encerrada ou inativa.");
                return;
            }
            // RN-DEN-001: bloqueia data/hora invalida (default/MinValue nao e aceitavel).
            if (resolvedAt == default || resolvedAt == DateTime.MinValue)
            {
                AddNotification(nameof(ResolvedAt), "A data/hora de conclusão deve ser válida.");
                return;
            }

            ResolvedAt = resolvedAt;
            if (!string.IsNullOrWhiteSpace(parecerFinal)) ParecerFinal = parecerFinal;
            Status = "Encerrado";
            MarcarAlterado(usuario);
        }

        public void Julgar(string statusFinal, string parecer, string usuario)
        {
            if (Status != "Recebida" && Status != "EmAnalise")
            {
                AddNotification(nameof(Status), "Apenas denúncias recebidas ou em análise podem ser julgadas.");
                return;
            }

            if (statusFinal != "Procedente" && statusFinal != "Improcedente")
            {
                AddNotification(nameof(Status), "Julgamento final deve ser 'Procedente' ou 'Improcedente'.");
                return;
            }

            if (string.IsNullOrWhiteSpace(parecer))
            {
                AddNotification(nameof(ParecerFinal), "O parecer final do julgamento é obrigatório.");
                return;
            }

            Status = statusFinal;
            ParecerFinal = parecer;
            MarcarAlterado(usuario);
        }
    }
}
