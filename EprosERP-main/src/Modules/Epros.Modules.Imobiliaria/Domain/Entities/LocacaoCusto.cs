using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Imobiliaria.Domain.Entities
{
    /// <summary>
    /// Custo da locacao, vinculado ao custo do imovel de origem
    /// (EF GESTAO_IMOBILIARIA 11.10, tabela imo_locacao_custo, RN-018).
    /// </summary>
    public class LocacaoCusto : EntidadeSaaSBase
    {
        public Guid LocacaoId { get; private set; }
        public Guid? CustoImovelId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public decimal Valor { get; private set; }

        protected LocacaoCusto() { } // EF Core

        public LocacaoCusto(Guid? custoImovelId, string descricao, decimal valor, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CustoImovelId = custoImovelId;
            Descricao = descricao;
            Valor = valor;
            Validar();
        }

        internal void VincularALocacao(Guid locacaoId) => LocacaoId = locacaoId;

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<LocacaoCusto>()
                .Requires()
                .IsNotNullOrEmpty(Descricao, nameof(Descricao),
                    "A descricao do custo da locacao e obrigatoria. [Origem: LocacaoCusto]")
                .IsGreaterOrEqualsThan(Valor, 0, nameof(Valor),
                    "O valor do custo nao pode ser negativo. [Origem: LocacaoCusto]"));
        }
    }
}
