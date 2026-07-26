using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Retorno do terceiro (EF Subcontratação §7.4 `sub_retorno`). SUB-007: retorno gera/referencia documento
    /// fiscal de retorno quando exigido (via referência — motor fiscal externo). SUB-010: retorno ajusta
    /// estoque/saldo. Modelo proposto por autoria (§16).
    /// </summary>
    public class SubRetorno : EntidadeSaaSBase
    {
        public Guid OrdemId { get; private set; }
        public DateTime? DataRetorno { get; private set; }
        public Guid? DocumentoFiscalId { get; private set; }
        public EStatusSubRetorno Status { get; private set; } = EStatusSubRetorno.Rascunho;

        // Navegação intra-módulo
        public ICollection<SubRetornoItem> Itens { get; private set; } = new List<SubRetornoItem>();

        protected SubRetorno() { } // EF Core

        public SubRetorno(Guid ordemId, DateTime? dataRetorno, Guid? documentoFiscalId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            OrdemId = ordemId;
            DataRetorno = dataRetorno ?? DateTime.UtcNow;
            DocumentoFiscalId = documentoFiscalId;
            Status = EStatusSubRetorno.Rascunho;
            Validar();
        }

        public void AdicionarItem(SubRetornoItem item) => Itens.Add(item);

        public void Confirmar(string usuario)
        {
            Status = EStatusSubRetorno.Recebido;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            if (OrdemId == Guid.Empty)
                AddNotification("OrdemId", "A ordem de subcontratação é obrigatória [Origem: SubRetorno]");
        }
    }
}
