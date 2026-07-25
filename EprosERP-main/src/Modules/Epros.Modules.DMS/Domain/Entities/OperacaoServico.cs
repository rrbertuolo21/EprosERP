using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class OperacaoServico : EntidadeSaaSBase
    {
        public Guid TipoServicoId { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public int Versao { get; private set; } = 1;
        public decimal TmoQuantidade { get; private set; }
        public string TmoUnidade { get; private set; } = string.Empty;
        public string? NaturezaPadrao { get; private set; }

        protected OperacaoServico() { } // EF Core

        public OperacaoServico(
            Guid tipoServicoId,
            string codigo,
            string descricao,
            decimal tmoQuantidade,
            string tmoUnidade,
            string? naturezaPadrao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<OperacaoServico>()
                .Requires()
                .AreNotEquals(tipoServicoId, Guid.Empty, nameof(TipoServicoId), "O tipo de serviço é obrigatório na operação de serviço.")
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O código da operação de serviço é obrigatório.")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descrição da operação de serviço é obrigatória.")
                .IsGreaterThan(tmoQuantidade, 0, nameof(TmoQuantidade), "A quantidade de TMO deve ser maior que zero.")
                .IsNotNullOrEmpty(tmoUnidade, nameof(TmoUnidade), "A unidade de TMO é obrigatória.")
            );

            TipoServicoId = tipoServicoId;
            Codigo = codigo;
            Descricao = descricao;
            Versao = 1;
            TmoQuantidade = tmoQuantidade;
            TmoUnidade = tmoUnidade;
            NaturezaPadrao = naturezaPadrao;
        }

        public void NovaVersao(string usuario)
        {
            Versao++;
            MarcarAlterado(usuario);
        }
    }
}
