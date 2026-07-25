using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Imobiliaria.Domain.Entities
{
    /// <summary>
    /// Custo vinculado ao imovel (EF GESTAO_IMOBILIARIA 11.4, tabela imo_imovel_custo).
    /// Campos financeiros detalhados nao informados no material; modelo minimo implantavel.
    /// </summary>
    public class ImovelCusto : EntidadeSaaSBase
    {
        public Guid ImovelId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public decimal Valor { get; private set; }
        public DateTime? Competencia { get; private set; }

        protected ImovelCusto() { } // EF Core

        public ImovelCusto(string descricao, decimal valor, DateTime? competencia, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Descricao = descricao;
            Valor = valor;
            Competencia = competencia?.Date;
            Validar();
        }

        internal void VincularAoImovel(Guid imovelId) => ImovelId = imovelId;

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ImovelCusto>()
                .Requires()
                .IsNotNullOrEmpty(Descricao, nameof(Descricao),
                    "A descricao do custo e obrigatoria. [Origem: ImovelCusto]")
                .IsGreaterOrEqualsThan(Valor, 0, nameof(Valor),
                    "O valor do custo nao pode ser negativo. [Origem: ImovelCusto]"));
        }
    }
}
