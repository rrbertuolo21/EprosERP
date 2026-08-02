using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-SOD — Regra SoD (grc_sod_regra). Define a incompatibilidade entre duas funcoes
    /// (matriz de segregacao). Fiel a EF_13_GRC_SEGREGACAO_DE_FUNCOES_SOD_V1 (secoes 10.2, 10.4 e 12).
    /// Unicidade: (TenantId, FuncaoAId, FuncaoBId, VigenciaInicio).
    /// </summary>
    public class RegraSoD : EntidadeSaaSBase
    {
        public string? Codigo { get; private set; }
        public Guid FuncaoAId { get; private set; }
        public Guid FuncaoBId { get; private set; }
        // Baixa, Media, Alta, Critica
        public string Criticidade { get; private set; } = "Alta";
        // D-SOD-02 — modo de tratamento do conflito por regra: Bloqueia (default) x PermiteExcecao.
        public string ModoTratamento { get; private set; } = "Bloqueia";
        public DateTime VigenciaInicio { get; private set; }
        public DateTime? VigenciaFim { get; private set; }
        // Rascunho, EmAnalise, Ativo, Suspenso, Encerrado, Inativo
        public string Status { get; private set; } = "Rascunho";
        public string? MotivoUltimaTransicao { get; private set; }

        protected RegraSoD() { } // EF Core

        public RegraSoD(
            string? codigo,
            Guid funcaoAId,
            Guid funcaoBId,
            string criticidade,
            DateTime vigenciaInicio,
            DateTime? vigenciaFim,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<RegraSoD>()
                .Requires()
                .IsTrue(funcaoAId != Guid.Empty, nameof(FuncaoAId), "A funcao A da regra e obrigatoria.")
                .IsTrue(funcaoBId != Guid.Empty, nameof(FuncaoBId), "A funcao B da regra e obrigatoria.")
                .IsTrue(funcaoAId != funcaoBId, nameof(FuncaoBId), "A regra SoD deve relacionar duas funcoes distintas.")
                .IsTrue(criticidade == "Baixa" || criticidade == "Media" || criticidade == "Alta" || criticidade == "Critica",
                    nameof(Criticidade), "A criticidade deve ser 'Baixa', 'Media', 'Alta' ou 'Critica'.")
                .IsTrue(vigenciaFim == null || vigenciaFim >= vigenciaInicio, nameof(VigenciaFim),
                    "A vigencia final nao pode ser anterior ao inicio.")
            );

            Codigo = codigo;
            FuncaoAId = funcaoAId;
            FuncaoBId = funcaoBId;
            Criticidade = criticidade;
            ModoTratamento = "Bloqueia";
            VigenciaInicio = vigenciaInicio;
            VigenciaFim = vigenciaFim;
            Status = "Rascunho";
        }

        /// <summary>
        /// D-SOD-02 — define o modo de tratamento do conflito: "Bloqueia" (impede a concessão) ou
        /// "PermiteExcecao" (concede mediante exceção aprovada com controle compensatório).
        /// </summary>
        public void DefinirModoTratamento(string modo, string usuario)
        {
            if (modo != "Bloqueia" && modo != "PermiteExcecao")
            {
                AddNotification(nameof(ModoTratamento), "O modo de tratamento deve ser 'Bloqueia' ou 'PermiteExcecao'.");
                return;
            }
            ModoTratamento = modo;
            MarcarAlterado(usuario);
        }

        public bool BloqueiaConcessao() => ModoTratamento == "Bloqueia";

        public void Submeter(string usuario)
        {
            if (Status != "Rascunho")
            {
                AddNotification(nameof(Status), "Somente regras em rascunho podem ser submetidas.");
                return;
            }
            Status = "EmAnalise";
            MarcarAlterado(usuario);
        }

        public void Ativar(string usuario)
        {
            if (Status != "EmAnalise")
            {
                AddNotification(nameof(Status), "Somente regras em analise podem ser ativadas.");
                return;
            }
            Status = "Ativo";
            MarcarAlterado(usuario);
        }

        public void Suspender(string motivo, string usuario)
        {
            if (Status != "Ativo")
            {
                AddNotification(nameof(Status), "Somente regras ativas podem ser suspensas.");
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

        public void Encerrar(string motivo, string usuario)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoUltimaTransicao), "O motivo do encerramento e obrigatorio.");
                return;
            }
            Status = "Encerrado";
            MotivoUltimaTransicao = motivo;
            MarcarAlterado(usuario);
        }

        public bool EstaVigente(DateTime referencia) =>
            Status == "Ativo" && VigenciaInicio <= referencia && (VigenciaFim == null || VigenciaFim >= referencia);
    }
}
