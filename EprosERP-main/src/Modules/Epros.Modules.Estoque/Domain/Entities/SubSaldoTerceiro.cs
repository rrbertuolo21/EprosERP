using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Saldo de material em poder de terceiros (EF Subcontratação §7.5 `sub_saldo_terceiro`).
    /// SUB-005: saldo = enviado − retornado − perdas/baixas reconhecidas. FornecedorId/ProdutoId/OrdemId são
    /// referências externas por FK Guid. Modelo proposto por autoria (§16).
    /// </summary>
    public class SubSaldoTerceiro : EntidadeSaaSBase
    {
        public Guid FornecedorId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public Guid? OrdemId { get; private set; }
        public decimal QuantidadeEnviada { get; private set; }
        public decimal QuantidadeRetornada { get; private set; }
        public decimal QuantidadeEmPoderTerceiro { get; private set; }
        public decimal? QuantidadePerda { get; private set; }
        public DateTime? UltimoMovimento { get; private set; }

        protected SubSaldoTerceiro() { } // EF Core

        public SubSaldoTerceiro(Guid fornecedorId, Guid produtoId, Guid? ordemId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            FornecedorId = fornecedorId;
            ProdutoId = produtoId;
            OrdemId = ordemId;
            QuantidadeEnviada = 0m;
            QuantidadeRetornada = 0m;
            QuantidadeEmPoderTerceiro = 0m;
        }

        /// <summary>Acumula envio e recalcula o saldo em poder do terceiro — SUB-005.</summary>
        public void RegistrarEnvio(decimal quantidade, string usuario)
        {
            QuantidadeEnviada += quantidade;
            Recalcular();
            UltimoMovimento = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }

        /// <summary>Acumula retorno (e perda opcional) e recalcula o saldo — SUB-005.</summary>
        public void RegistrarRetorno(decimal quantidade, decimal perda, string usuario)
        {
            QuantidadeRetornada += quantidade;
            QuantidadePerda = (QuantidadePerda ?? 0m) + perda;
            Recalcular();
            UltimoMovimento = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }

        private void Recalcular()
        {
            QuantidadeEmPoderTerceiro = QuantidadeEnviada - QuantidadeRetornada - (QuantidadePerda ?? 0m);
        }
    }
}
