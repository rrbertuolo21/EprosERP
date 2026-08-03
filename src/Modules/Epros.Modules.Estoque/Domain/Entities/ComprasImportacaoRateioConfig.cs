using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Parâmetro de rateio do custo landed da importação (CD1 / EF COMERCIO_EXTERIOR §5.3, CEX-003/NF-02).
    /// Por tenant e (opcionalmente) por empresa. **Desligado por padrão** (<see cref="Habilitado"/> = false):
    /// enquanto o contador não definir a base, a nacionalização NÃO compõe landed (custo = valor do item).
    ///
    /// Quando habilitado, define QUAIS componentes já apurados entram no custo (tributos/frete/despesas) e
    /// a CHAVE de rateio (<see cref="Metodo"/>). ⚠️ Os VALORES dos tributos e a BASE de cálculo são
    /// factuais/valida-contador — esta config só liga/desliga e escolhe a chave de distribuição (estrutural).
    /// Frete de entrada compõe o custo por padrão (NF-04): <see cref="IncluirFrete"/> nasce true.
    /// </summary>
    public class ComprasImportacaoRateioConfig : EntidadeSaaSBase
    {
        public Guid? EmpresaId { get; private set; }
        public bool Habilitado { get; private set; }
        public bool IncluirTributos { get; private set; }
        public bool IncluirFrete { get; private set; }
        public bool IncluirDespesas { get; private set; }
        public ERateioLandedMetodo Metodo { get; private set; }

        protected ComprasImportacaoRateioConfig() { } // EF Core

        public ComprasImportacaoRateioConfig(
            Guid? empresaId,
            bool habilitado,
            bool incluirTributos,
            bool incluirFrete,
            bool incluirDespesas,
            ERateioLandedMetodo metodo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            Habilitado = habilitado;
            IncluirTributos = incluirTributos;
            IncluirFrete = incluirFrete;
            IncluirDespesas = incluirDespesas;
            Metodo = metodo;
        }

        /// <summary>Config-padrão SEGURA (NF-02): desligada; frete marcado para compor quando ligar (NF-04).</summary>
        public static ComprasImportacaoRateioConfig Padrao(Guid? empresaId, string tenantId, string criadoPor)
            => new(empresaId, habilitado: false, incluirTributos: false, incluirFrete: true,
                   incluirDespesas: false, metodo: ERateioLandedMetodo.PorValor, tenantId, criadoPor);

        public void Alterar(bool habilitado, bool incluirTributos, bool incluirFrete, bool incluirDespesas, ERateioLandedMetodo metodo, string usuario)
        {
            Habilitado = habilitado;
            IncluirTributos = incluirTributos;
            IncluirFrete = incluirFrete;
            IncluirDespesas = incluirDespesas;
            Metodo = metodo;
            MarcarAlterado(usuario);
        }
    }
}
