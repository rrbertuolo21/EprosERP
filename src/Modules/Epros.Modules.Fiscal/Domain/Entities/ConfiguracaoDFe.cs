using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    /// <summary>
    /// Configuração de emissão de documentos fiscais eletrônicos (NF-e / NFC-e) por empresa:
    /// séries, últimos números e CSC para os ambientes de Produção e Homologação.
    /// Porte do legado (Epros.ERP.Domain/Entities/Configuracoes/ConfiguracaoDFe) — FK Empresa por Guid.
    /// </summary>
    public class ConfiguracaoDFe : EntidadeSaaSBase
    {
        // FK para Empresa (outro módulo) — referenciada por Guid, sem navegação cruzada.
        public Guid EmpresaId { get; private set; }

        // NF-e
        public string? NFeSerieProducao { get; private set; }
        public string? NFeUltimoNrProducao { get; private set; }
        public string? NFeSerieHomologacao { get; private set; }
        public string? NFeUltimoNrHomologacao { get; private set; }

        // NFC-e — Produção
        public string? NfceCscProducao { get; private set; }
        public string? NfceIdCscProducao { get; private set; }
        public string? NfceSerieProducao { get; private set; }
        public string? NfceUltimoNrProducao { get; private set; }

        // NFC-e — Homologação
        public string? NfceCscHomologacao { get; private set; }
        public string? NfceIdCscHomologacao { get; private set; }
        public string? NfceSerieHomologacao { get; private set; }
        public string? NfceUltimoNrHomologacao { get; private set; }

        protected ConfiguracaoDFe() { } // EF Core

        public ConfiguracaoDFe(
            Guid empresaId,
            string? nFeSerieProducao,
            string? nFeUltimoNrProducao,
            string? nFeSerieHomologacao,
            string? nFeUltimoNrHomologacao,
            string? nfceCscProducao,
            string? nfceIdCscProducao,
            string? nfceSerieProducao,
            string? nfceUltimoNrProducao,
            string? nfceCscHomologacao,
            string? nfceIdCscHomologacao,
            string? nfceSerieHomologacao,
            string? nfceUltimoNrHomologacao,
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            NFeSerieProducao = nFeSerieProducao;
            NFeUltimoNrProducao = nFeUltimoNrProducao;
            NFeSerieHomologacao = nFeSerieHomologacao;
            NFeUltimoNrHomologacao = nFeUltimoNrHomologacao;
            NfceCscProducao = nfceCscProducao;
            NfceIdCscProducao = nfceIdCscProducao;
            NfceSerieProducao = nfceSerieProducao;
            NfceUltimoNrProducao = nfceUltimoNrProducao;
            NfceCscHomologacao = nfceCscHomologacao;
            NfceIdCscHomologacao = nfceIdCscHomologacao;
            NfceSerieHomologacao = nfceSerieHomologacao;
            NfceUltimoNrHomologacao = nfceUltimoNrHomologacao;
            Validar();
        }

        public void Alterar(
            string? nFeSerieProducao,
            string? nFeUltimoNrProducao,
            string? nFeSerieHomologacao,
            string? nFeUltimoNrHomologacao,
            string? nfceCscProducao,
            string? nfceIdCscProducao,
            string? nfceSerieProducao,
            string? nfceUltimoNrProducao,
            string? nfceCscHomologacao,
            string? nfceIdCscHomologacao,
            string? nfceSerieHomologacao,
            string? nfceUltimoNrHomologacao,
            string alteradoPor)
        {
            NFeSerieProducao = nFeSerieProducao;
            NFeUltimoNrProducao = nFeUltimoNrProducao;
            NFeSerieHomologacao = nFeSerieHomologacao;
            NFeUltimoNrHomologacao = nFeUltimoNrHomologacao;
            NfceCscProducao = nfceCscProducao;
            NfceIdCscProducao = nfceIdCscProducao;
            NfceSerieProducao = nfceSerieProducao;
            NfceUltimoNrProducao = nfceUltimoNrProducao;
            NfceCscHomologacao = nfceCscHomologacao;
            NfceIdCscHomologacao = nfceIdCscHomologacao;
            NfceSerieHomologacao = nfceSerieHomologacao;
            NfceUltimoNrHomologacao = nfceUltimoNrHomologacao;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ConfiguracaoDFe>()
                .Requires()
                .IsNotNull(EmpresaId, nameof(EmpresaId), "A empresa é obrigatória [Origem: ConfiguracaoDFe]")
                .IsLowerOrEqualsThan((NFeSerieProducao ?? "").Length, 3, nameof(NFeSerieProducao), "O campo NFeSerieProducao deve ter no máximo 3 caracteres [Origem: ConfiguracaoDFe]")
                .IsLowerOrEqualsThan((NFeSerieHomologacao ?? "").Length, 3, nameof(NFeSerieHomologacao), "O campo NFeSerieHomologacao deve ter no máximo 3 caracteres [Origem: ConfiguracaoDFe]")
                .IsLowerOrEqualsThan((NfceSerieProducao ?? "").Length, 3, nameof(NfceSerieProducao), "O campo NfceSerieProducao deve ter no máximo 3 caracteres [Origem: ConfiguracaoDFe]")
                .IsLowerOrEqualsThan((NfceSerieHomologacao ?? "").Length, 3, nameof(NfceSerieHomologacao), "O campo NfceSerieHomologacao deve ter no máximo 3 caracteres [Origem: ConfiguracaoDFe]")
            );
        }
    }
}
