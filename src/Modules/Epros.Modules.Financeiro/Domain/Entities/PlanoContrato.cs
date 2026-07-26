using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Plano de contrato recorrente (EF FIN-GCF §13.1 gcf_plano_contrato).
    /// Submódulo de evolução — sobe desabilitado (ABAC nega por padrão). Isolamento por tenant via ContextBase.
    /// </summary>
    public class PlanoContrato : EntidadeSaaSBase
    {
        public string Descricao { get; private set; } = string.Empty;
        public decimal Valor { get; private set; }
        public EPeriodicidadeContrato Periodicidade { get; private set; }
        public EStatusPlanoContrato Status { get; private set; } = EStatusPlanoContrato.Ativo;

        protected PlanoContrato() { } // EF Core

        public PlanoContrato(string descricao, decimal valor, EPeriodicidadeContrato periodicidade, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Descricao = descricao;
            Valor = valor;
            Periodicidade = periodicidade;
            Status = EStatusPlanoContrato.Ativo;
            Validar();
        }

        public void Alterar(string descricao, decimal valor, EPeriodicidadeContrato periodicidade, string usuario)
        {
            Descricao = descricao;
            Valor = valor;
            Periodicidade = periodicidade;
            MarcarAlterado(usuario);
            Validar();
        }

        public void DefinirStatus(EStatusPlanoContrato status, string usuario) { Status = status; MarcarAlterado(usuario); }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PlanoContrato>()
                .Requires()
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descrição do plano é obrigatória [Origem: PlanoContrato]")
                .IsGreaterThan(Valor, 0, nameof(Valor), "O valor do plano deve ser maior que zero [Origem: PlanoContrato]")
            );
        }
    }
}
