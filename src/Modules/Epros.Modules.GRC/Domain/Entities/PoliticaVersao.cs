using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-POL — Versao da politica (grc_pol_versao). Controla conteudo, vigencia e
    /// substituicao de versoes. Aceite sempre aponta para a versao.
    /// Fiel a EF_13_GRC_GESTAO_DE_POLITICAS_V1 (secao 11.2).
    /// </summary>
    public class PoliticaVersao : EntidadeSaaSBase
    {
        public Guid PoliticaId { get; private set; }
        public string NumeroVersao { get; private set; } = string.Empty; // ex.: 1.0, 1.1
        public DateTime DataInicioVigencia { get; private set; }
        public DateTime? DataFimVigencia { get; private set; }
        public string? ResumoAlteracoes { get; private set; }
        public string? MotivoVersao { get; private set; }
        public Guid? DocumentoOficialId { get; private set; }
        // Rascunho, EmRevisao, Aprovada, Publicada, Arquivada
        public string Status { get; private set; } = "Rascunho";

        protected PoliticaVersao() { } // EF Core

        public PoliticaVersao(
            Guid politicaId,
            string numeroVersao,
            DateTime dataInicioVigencia,
            DateTime? dataFimVigencia,
            string? resumoAlteracoes,
            string? motivoVersao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PoliticaVersao>()
                .Requires()
                .IsTrue(politicaId != Guid.Empty, nameof(PoliticaId), "A politica da versao e obrigatoria.")
                .IsNotNullOrEmpty(numeroVersao, nameof(NumeroVersao), "O numero da versao e obrigatorio.")
                // RN check 10.4: fim nao pode ser anterior ao inicio.
                .IsTrue(dataFimVigencia == null || dataFimVigencia >= dataInicioVigencia, nameof(DataFimVigencia),
                    "A data de fim de vigencia nao pode ser anterior ao inicio.")
            );

            PoliticaId = politicaId;
            NumeroVersao = numeroVersao;
            DataInicioVigencia = dataInicioVigencia;
            DataFimVigencia = dataFimVigencia;
            ResumoAlteracoes = resumoAlteracoes;
            MotivoVersao = motivoVersao;
            Status = "Rascunho";
        }

        public void VincularDocumento(Guid documentoOficialId, string usuario)
        {
            DocumentoOficialId = documentoOficialId;
            MarcarAlterado(usuario);
        }

        public void Aprovar(string usuario)
        {
            if (Status != "Rascunho" && Status != "EmRevisao")
            {
                AddNotification(nameof(Status), "Somente versoes em rascunho ou revisao podem ser aprovadas.");
                return;
            }
            Status = "Aprovada";
            MarcarAlterado(usuario);
        }

        public void Publicar(string usuario)
        {
            if (Status != "Aprovada")
            {
                AddNotification(nameof(Status), "Somente versoes aprovadas podem ser publicadas.");
                return;
            }
            Status = "Publicada";
            MarcarAlterado(usuario);
        }

        public void Arquivar(string usuario)
        {
            Status = "Arquivada";
            MarcarAlterado(usuario);
        }
    }
}
