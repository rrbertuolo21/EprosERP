using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Cotação publicada a um fornecedor no Portal (EF Portal do Fornecedor §15.3 `pfo_cotacao_publicada`).
    /// PFO-006: a cotação está vinculada a um processo de compra. A cotação de origem é OWNED pelo módulo
    /// COMPRAS (D7) — aqui é apenas uma referência externa (`CotacaoOrigemId`), integrada por evento.
    /// </summary>
    public class PfoCotacaoPublicada : EntidadeSaaSBase
    {
        public Guid CotacaoOrigemId { get; private set; }
        public Guid FornecedorId { get; private set; }
        public EStatusCotacaoPublicada Status { get; private set; } = EStatusCotacaoPublicada.Aberta;
        public DateTime? PrazoResposta { get; private set; }

        protected PfoCotacaoPublicada() { }

        public PfoCotacaoPublicada(Guid cotacaoOrigemId, Guid fornecedorId, DateTime? prazoResposta, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CotacaoOrigemId = cotacaoOrigemId;
            FornecedorId = fornecedorId;
            PrazoResposta = prazoResposta;
            Status = EStatusCotacaoPublicada.Aberta;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PfoCotacaoPublicada>()
                .Requires()
                .IsNotEmpty(CotacaoOrigemId, nameof(CotacaoOrigemId), "A cotação de origem é obrigatória [PFO-006] [Origem: PfoCotacaoPublicada]")
                .IsNotEmpty(FornecedorId, nameof(FornecedorId), "O fornecedor da cotação é obrigatório [PFO-002] [Origem: PfoCotacaoPublicada]"));
        }

        /// <summary>VAL-PFO-003: cotação encerrada/cancelada não aceita resposta.</summary>
        public bool AceitaResposta() => Status == EStatusCotacaoPublicada.Aberta && (PrazoResposta == null || PrazoResposta >= DateTime.UtcNow);

        public void MarcarRespondida(string usuario) { Status = EStatusCotacaoPublicada.Respondida; MarcarAlterado(usuario); }
        public void Encerrar(string usuario) { Status = EStatusCotacaoPublicada.Encerrada; MarcarAlterado(usuario); }
    }
}
