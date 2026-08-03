using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Cabeçalho da resposta de cotação do fornecedor (EF Portal do Fornecedor §15.4 `pfo_resposta_cotacao`).
    /// Enviada ao módulo COMPRAS por evento (est.pfo.cotacao_respondida).
    /// </summary>
    public class PfoRespostaCotacao : EntidadeSaaSBase
    {
        public Guid CotacaoPublicadaId { get; private set; }
        public Guid FornecedorId { get; private set; }
        public EStatusRespostaCotacao Status { get; private set; } = EStatusRespostaCotacao.Rascunho;
        public decimal? ValorTotal { get; private set; }
        public string? Observacao { get; private set; }
        public DateTime? EnviadaEm { get; private set; }

        public ICollection<PfoRespostaCotacaoItem> Itens { get; private set; } = new List<PfoRespostaCotacaoItem>();

        protected PfoRespostaCotacao() { }

        public PfoRespostaCotacao(Guid cotacaoPublicadaId, Guid fornecedorId, decimal? valorTotal, string? observacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CotacaoPublicadaId = cotacaoPublicadaId;
            FornecedorId = fornecedorId;
            ValorTotal = valorTotal;
            Observacao = observacao;
            Status = EStatusRespostaCotacao.Rascunho;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PfoRespostaCotacao>()
                .Requires()
                .IsNotEmpty(CotacaoPublicadaId, nameof(CotacaoPublicadaId), "A cotação publicada é obrigatória [Origem: PfoRespostaCotacao]")
                .IsNotEmpty(FornecedorId, nameof(FornecedorId), "O fornecedor da resposta é obrigatório [PFO-002] [Origem: PfoRespostaCotacao]"));
        }

        public void Enviar(string usuario) { Status = EStatusRespostaCotacao.Enviada; EnviadaEm = DateTime.UtcNow; MarcarAlterado(usuario); }
    }
}
