using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Moeda para câmbio e risco de mercado (EF FIN-CAM §10.3 cam_moeda).
    /// Submódulo de evolução — sobe desabilitado (ABAC nega por padrão). Isolamento por tenant via ContextBase.
    /// </summary>
    public class Moeda : EntidadeSaaSBase
    {
        public string CodigoIso { get; private set; } = string.Empty;
        public string Simbolo { get; private set; } = string.Empty;
        public string? Nome { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected Moeda() { } // EF Core

        public Moeda(string codigoIso, string simbolo, string? nome, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CodigoIso = codigoIso;
            Simbolo = simbolo;
            Nome = nome;
            Ativo = true;
            Validar();
        }

        public void Alterar(string codigoIso, string simbolo, string? nome, string usuario)
        {
            CodigoIso = codigoIso;
            Simbolo = simbolo;
            Nome = nome;
            MarcarAlterado(usuario);
            Validar();
        }

        public void DefinirAtivo(bool ativo, string usuario) { Ativo = ativo; MarcarAlterado(usuario); }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Moeda>()
                .Requires()
                .IsNotNullOrEmpty(CodigoIso, nameof(CodigoIso), "O código ISO da moeda é obrigatório [Origem: Moeda]")
                .IsLowerOrEqualsThan(CodigoIso?.Length ?? 0, 10, nameof(CodigoIso), "O código ISO deve ter no máximo 10 caracteres [Origem: Moeda]")
                .IsNotNullOrEmpty(Simbolo, nameof(Simbolo), "O símbolo da moeda é obrigatório [Origem: Moeda]")
            );
        }
    }
}
