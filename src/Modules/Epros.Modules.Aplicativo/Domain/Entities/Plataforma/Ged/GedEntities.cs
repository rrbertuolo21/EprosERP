using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Ged
{
    /// <summary>
    /// PLT · GED (T10) — vínculo N:N de um <see cref="DocumentoGed"/> a uma entidade de negócio de
    /// qualquer módulo. Permite que um documento canônico único seja referenciado por vários
    /// registros (contrato ↔ locação, laudo ↔ ativo) sem duplicar o arquivo.
    /// </summary>
    public class VinculoDocumentoGed : EntidadeSaaSBase
    {
        public Guid DocumentoId { get; private set; }
        public string ModuloDestino { get; private set; } = string.Empty;
        public string EntidadeTipo { get; private set; } = string.Empty;
        public string EntidadeId { get; private set; } = string.Empty;

        protected VinculoDocumentoGed() { }

        public VinculoDocumentoGed(Guid documentoId, string moduloDestino, string entidadeTipo,
            string entidadeId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<VinculoDocumentoGed>()
                .Requires()
                .IsTrue(documentoId != Guid.Empty, nameof(DocumentoId), "O documento é obrigatório.")
                .IsNotNullOrEmpty(moduloDestino, nameof(ModuloDestino), "O módulo de destino é obrigatório.")
                .IsNotNullOrEmpty(entidadeTipo, nameof(EntidadeTipo), "O tipo da entidade é obrigatório.")
                .IsNotNullOrEmpty(entidadeId, nameof(EntidadeId), "O id da entidade é obrigatório."));

            DocumentoId = documentoId;
            ModuloDestino = moduloDestino;
            EntidadeTipo = entidadeTipo;
            EntidadeId = entidadeId;
        }
    }

    /// <summary>
    /// PLT · GED (T10) / NF-04 — política de retenção legal por tipo documental.
    /// ⛔ Regra #0: o PRAZO e a BASE LEGAL são regra de negócio/fiscal e vêm da skill
    /// <c>compliance/retencao-legal</c> (+ overlay do cliente) ou de validação de contador/jurídico —
    /// NUNCA são inventados aqui. Esta entidade apenas ARMAZENA e APLICA a política informada.
    /// </summary>
    public class PoliticaRetencaoGed : EntidadeSaaSBase
    {
        public string TipoDocumento { get; private set; } = string.Empty;
        public int PrazoRetencaoDias { get; private set; }
        public string BaseLegal { get; private set; } = string.Empty;
        /// <summary>Ação após o prazo: "Reter", "RevisarDescarte", "Descartar".</summary>
        public string AcaoAposPrazo { get; private set; } = "RevisarDescarte";
        /// <summary>Hold jurídico: quando true, o documento NÃO pode ser descartado mesmo vencido o prazo.</summary>
        public bool HoldJuridico { get; private set; }

        protected PoliticaRetencaoGed() { }

        public PoliticaRetencaoGed(string tipoDocumento, int prazoRetencaoDias, string baseLegal,
            string acaoAposPrazo, bool holdJuridico, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PoliticaRetencaoGed>()
                .Requires()
                .IsNotNullOrEmpty(tipoDocumento, nameof(TipoDocumento), "O tipo de documento é obrigatório.")
                .IsGreaterThan(prazoRetencaoDias, 0, nameof(PrazoRetencaoDias), "O prazo de retenção deve ser maior que zero.")
                .IsNotNullOrEmpty(baseLegal, nameof(BaseLegal), "A base legal da retenção é obrigatória (Regra #0 — não inventar).")
                .IsTrue(acaoAposPrazo is "Reter" or "RevisarDescarte" or "Descartar", nameof(AcaoAposPrazo),
                    "A ação após o prazo deve ser 'Reter', 'RevisarDescarte' ou 'Descartar'."));

            TipoDocumento = tipoDocumento;
            PrazoRetencaoDias = prazoRetencaoDias;
            BaseLegal = baseLegal;
            AcaoAposPrazo = acaoAposPrazo;
            HoldJuridico = holdJuridico;
        }

        public void Atualizar(int prazoRetencaoDias, string baseLegal, string acaoAposPrazo, bool holdJuridico, string usuario)
        {
            if (prazoRetencaoDias > 0) PrazoRetencaoDias = prazoRetencaoDias;
            if (!string.IsNullOrWhiteSpace(baseLegal)) BaseLegal = baseLegal;
            if (acaoAposPrazo is "Reter" or "RevisarDescarte" or "Descartar") AcaoAposPrazo = acaoAposPrazo;
            HoldJuridico = holdJuridico;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>
    /// PLT · GED — hash bloqueado (REG-013/REG-056): impede novos uploads de um conteúdo
    /// previamente marcado como proibido (moderação/segurança).
    /// </summary>
    public class HashBloqueadoGed : EntidadeSaaSBase
    {
        public string Hash { get; private set; } = string.Empty;
        public string Motivo { get; private set; } = string.Empty;

        protected HashBloqueadoGed() { }

        public HashBloqueadoGed(string hash, string motivo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<HashBloqueadoGed>()
                .Requires()
                .IsNotNullOrEmpty(hash, nameof(Hash), "O hash a bloquear é obrigatório.")
                .IsNotNullOrEmpty(motivo, nameof(Motivo), "O motivo do bloqueio é obrigatório."));

            Hash = hash;
            Motivo = motivo;
        }
    }
}
