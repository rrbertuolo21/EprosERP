using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-SOD — Simulacao SoD (grc_sod_simulacao). Guarda o resultado de uma simulacao de
    /// conflitos para um perfil ou usuario (RF-GRC-SOD-002).
    /// Fiel a EF_13_GRC_SEGREGACAO_DE_FUNCOES_SOD_V1 (secao 10.2).
    /// </summary>
    public class SimulacaoSoD : EntidadeSaaSBase
    {
        // Perfil ou Usuario referenciados por Id (RBAC pertence a GestaoClientes)
        public Guid? PerfilId { get; private set; }
        public Guid? UsuarioId { get; private set; }
        public string Alvo { get; private set; } = "Perfil"; // Perfil, Usuario
        public DateTime DataSimulacao { get; private set; }
        public int QuantidadeViolacoes { get; private set; }
        public string? Resumo { get; private set; }

        protected SimulacaoSoD() { } // EF Core

        public SimulacaoSoD(
            Guid? perfilId,
            Guid? usuarioId,
            string alvo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<SimulacaoSoD>()
                .Requires()
                .IsTrue(alvo == "Perfil" || alvo == "Usuario", nameof(Alvo), "O alvo deve ser 'Perfil' ou 'Usuario'.")
                .IsTrue(perfilId != null || usuarioId != null, nameof(PerfilId),
                    "A simulacao deve informar um perfil ou um usuario.")
            );

            PerfilId = perfilId;
            UsuarioId = usuarioId;
            Alvo = alvo;
            DataSimulacao = DateTime.UtcNow;
            QuantidadeViolacoes = 0;
        }

        public void RegistrarResultado(int quantidadeViolacoes, string? resumo)
        {
            QuantidadeViolacoes = quantidadeViolacoes < 0 ? 0 : quantidadeViolacoes;
            Resumo = resumo;
        }
    }
}
