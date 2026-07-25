using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Cabeçalho de requisição interna de itens de estoque (EF Movimentação Manual e Ajustes §15.13).
    /// MVM-027: exige colaborador, data, situação e itens. ColaboradorId referencia outro módulo por FK Guid.
    /// </summary>
    public class RequisicaoInterna : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public DateTime DataRequisicao { get; private set; }
        public EStatusRequisicaoInterna Situacao { get; private set; } = EStatusRequisicaoInterna.Rascunho;

        // Navegação intra-módulo
        public ICollection<RequisicaoInternaItem> Itens { get; private set; } = new List<RequisicaoInternaItem>();

        protected RequisicaoInterna() { } // EF Core

        public RequisicaoInterna(Guid colaboradorId, DateTime dataRequisicao, EStatusRequisicaoInterna situacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            DataRequisicao = dataRequisicao;
            Situacao = situacao;
        }

        public void Validar() { }

        public void AdicionarItem(RequisicaoInternaItem item) => Itens.Add(item);

        public void AlterarSituacao(EStatusRequisicaoInterna situacao, string usuario)
        {
            Situacao = situacao;
            MarcarAlterado(usuario);
        }
    }
}
