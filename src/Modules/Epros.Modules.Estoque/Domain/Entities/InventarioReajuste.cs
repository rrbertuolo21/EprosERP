using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Cabeçalho de reajuste de estoque (EF Inventário §16.3). INV-009: exige colaborador responsável;
    /// INV-010: exige tipo de reajuste; INV-011: exige lista de detalhes.
    /// </summary>
    public class InventarioReajuste : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public DateTime? DataReajuste { get; private set; }
        public decimal? Porcentagem { get; private set; }
        public string TipoReajuste { get; private set; } = string.Empty;
        public EStatusRegistroEstoque Situacao { get; private set; } = EStatusRegistroEstoque.Rascunho;

        public ICollection<InventarioReajusteItem> Itens { get; private set; } = new List<InventarioReajusteItem>();

        protected InventarioReajuste() { } // EF Core

        public InventarioReajuste(Guid colaboradorId, DateTime? dataReajuste, decimal? porcentagem, string tipoReajuste, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            DataReajuste = dataReajuste;
            Porcentagem = porcentagem;
            TipoReajuste = tipoReajuste ?? string.Empty;
            Situacao = EStatusRegistroEstoque.Rascunho;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<InventarioReajuste>()
                .Requires()
                .IsNotEmpty(ColaboradorId, nameof(ColaboradorId), "O colaborador responsável é obrigatório [INV-009] [Origem: InventarioReajuste]")
                .IsNotNullOrEmpty(TipoReajuste, nameof(TipoReajuste), "O tipo de reajuste é obrigatório [INV-010] [Origem: InventarioReajuste]"));
        }

        public void Aplicar(string usuario)
        {
            Situacao = EStatusRegistroEstoque.Aplicado;
            MarcarAlterado(usuario);
        }
    }
}
