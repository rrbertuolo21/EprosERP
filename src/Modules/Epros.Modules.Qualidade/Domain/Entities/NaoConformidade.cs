using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Qualidade.Domain.Entities
{
    public class NaoConformidade : EntidadeSaaSBase
    {
        public Guid InspecaoLoteId { get; private set; }
        public string Sku { get; private set; } = string.Empty;
        public string Titulo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public string Status { get; private set; } = "Aberta";
        public string? CausaRaiz { get; private set; }
        public string? PlanoAcao { get; private set; }
        public DateTime? ResolvidoEm { get; private set; }
        public string? ResolvidoPor { get; private set; }

        protected NaoConformidade() { } // EF Core

        public NaoConformidade(Guid inspecaoLoteId, string sku, string titulo, string descricao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<NaoConformidade>()
                .Requires()
                .AreNotEquals(inspecaoLoteId, Guid.Empty, nameof(InspecaoLoteId), "O ID da inspeção do lote é obrigatório.")
                .IsNotNullOrEmpty(sku, nameof(Sku), "O SKU do produto é obrigatório.")
                .IsNotNullOrEmpty(titulo, nameof(Titulo), "O Título da não conformidade é obrigatório.")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A Descrição da não conformidade é obrigatória.")
            );

            InspecaoLoteId = inspecaoLoteId;
            Sku = sku;
            Titulo = titulo;
            Descricao = descricao;
            Status = "Aberta";
        }

        public void Tratar(string causaRaiz, string planoAcao, string resolvidoPor, string usuario)
        {
            if (Status != "Aberta")
            {
                AddNotification(nameof(Status), "Esta não conformidade já foi resolvida.");
                return;
            }

            AddNotifications(new Contract<NaoConformidade>()
                .Requires()
                .IsNotNullOrEmpty(causaRaiz, nameof(CausaRaiz), "A análise de Causa Raiz é obrigatória.")
                .IsNotNullOrEmpty(planoAcao, nameof(PlanoAcao), "O Plano de Ação é obrigatório.")
                .IsNotNullOrEmpty(resolvidoPor, nameof(ResolvidoPor), "O responsável pela resolução é obrigatório.")
            );

            if (!IsValid) return;

            CausaRaiz = causaRaiz;
            PlanoAcao = planoAcao;
            ResolvidoPor = resolvidoPor;
            ResolvidoEm = DateTime.UtcNow;
            Status = "Resolvida";

            MarcarAlterado(usuario);
        }
    }
}
