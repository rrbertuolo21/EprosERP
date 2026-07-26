using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>Depreciação mensal por ativo e competência (EF FIN-AFX §10.4 afx_depreciacao_mensal).</summary>
    public class DepreciacaoMensal : EntidadeSaaSBase
    {
        public Guid AtivoId { get; private set; }
        public string Competencia { get; private set; } = string.Empty; // AAAA-MM
        public decimal Valor { get; private set; }
        public string? MetodoDepreciacao { get; private set; }
        public decimal? TaxaAplicada { get; private set; }
        public bool Contabilizado { get; private set; }
        public DateTime? DataContabilizacao { get; private set; }

        protected DepreciacaoMensal() { } // EF Core

        public DepreciacaoMensal(Guid ativoId, string competencia, decimal valor, string? metodoDepreciacao, decimal? taxaAplicada,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AtivoId = ativoId;
            Competencia = competencia;
            Valor = valor;
            MetodoDepreciacao = metodoDepreciacao;
            TaxaAplicada = taxaAplicada;
            Contabilizado = false;
            Validar();
        }

        public void Contabilizar(DateTime data, string usuario)
        {
            Contabilizado = true;
            DataContabilizacao = data;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<DepreciacaoMensal>()
                .Requires()
                .IsNotEmpty(AtivoId, nameof(AtivoId), "O ativo é obrigatório [Origem: DepreciacaoMensal]")
                .IsNotNullOrEmpty(Competencia, nameof(Competencia), "A competência é obrigatória [Origem: DepreciacaoMensal]")
                .IsGreaterThan(Valor, 0, nameof(Valor), "O valor da depreciação deve ser maior que zero [Origem: DepreciacaoMensal]")
            );
        }
    }
}
