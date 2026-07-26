using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Remessa de material ao terceiro (EF Subcontratação §7.3 `sub_envio`). SUB-006: remessa gera/referencia
    /// documento fiscal de remessa quando exigido (documento fiscal via referência — motor fiscal externo).
    /// SUB-010: envio ajusta estoque/saldo em terceiro. Modelo proposto por autoria (§16).
    /// </summary>
    public class SubEnvio : EntidadeSaaSBase
    {
        public Guid OrdemId { get; private set; }
        public DateTime? DataEnvio { get; private set; }
        public Guid? DocumentoFiscalId { get; private set; }
        public EStatusSubEnvio Status { get; private set; } = EStatusSubEnvio.Rascunho;

        // Navegação intra-módulo
        public ICollection<SubEnvioItem> Itens { get; private set; } = new List<SubEnvioItem>();

        protected SubEnvio() { } // EF Core

        public SubEnvio(Guid ordemId, DateTime? dataEnvio, Guid? documentoFiscalId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            OrdemId = ordemId;
            DataEnvio = dataEnvio ?? DateTime.UtcNow;
            DocumentoFiscalId = documentoFiscalId;
            Status = EStatusSubEnvio.Rascunho;
            Validar();
        }

        public void AdicionarItem(SubEnvioItem item) => Itens.Add(item);

        public void Confirmar(string usuario)
        {
            Status = EStatusSubEnvio.Enviado;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            if (OrdemId == Guid.Empty)
                AddNotification("OrdemId", "A ordem de subcontratação é obrigatória [Origem: SubEnvio]");
        }
    }
}
