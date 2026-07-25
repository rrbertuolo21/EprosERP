using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Configuração de matching para deduplicação de pessoas (CAD-PEM 12.14).</summary>
    public class RegraDeduplicacao : EntidadeSaaSBase
    {
        public string Campo { get; private set; } = string.Empty;
        public EEstrategiaMatch Estrategia { get; private set; }
        public decimal Peso { get; private set; }
        public decimal LimiarBloqueio { get; private set; }
        public decimal LimiarAlerta { get; private set; }

        protected RegraDeduplicacao() { } // EF Core

        public RegraDeduplicacao(
            string campo,
            EEstrategiaMatch estrategia,
            decimal peso,
            decimal limiarBloqueio,
            decimal limiarAlerta,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Campo = campo;
            Estrategia = estrategia;
            Peso = peso;
            LimiarBloqueio = limiarBloqueio;
            LimiarAlerta = limiarAlerta;
            Validar();
        }

        public void Alterar(
            string campo,
            EEstrategiaMatch estrategia,
            decimal peso,
            decimal limiarBloqueio,
            decimal limiarAlerta,
            string alteradoPor)
        {
            Campo = campo;
            Estrategia = estrategia;
            Peso = peso;
            LimiarBloqueio = limiarBloqueio;
            LimiarAlerta = limiarAlerta;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<RegraDeduplicacao>()
                .Requires()
                .IsNotNullOrEmpty(Campo, nameof(Campo), "Campo é obrigatório [Origem: RegraDeduplicacao]")
                .HasMaxLen(Campo ?? string.Empty, 100, nameof(Campo), "Campo deve ter no máximo 100 caracteres [Origem: RegraDeduplicacao]")
                .IsTrue(Enum.IsDefined(typeof(EEstrategiaMatch), Estrategia), nameof(Estrategia), "Estrategia não consta na lista [Origem: RegraDeduplicacao]")
                .IsGreaterOrEqualsThan(Peso, 0, nameof(Peso), "Peso deve ser maior ou igual a zero [Origem: RegraDeduplicacao]")
            );
        }
    }
}
