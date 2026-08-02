using System;
using Epros.Modules.Imobiliaria.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Imobiliaria.Domain.Entities
{
    /// <summary>
    /// Garantia da locacao (ID6/NF-04): fiador, caucao, seguro-fianca ou garantia bancaria, com
    /// valor/limite, vigencia, substituicao e liberacao. Registro FACTUAL. O tratamento financeiro
    /// da CAUCAO (retencao/correcao/devolucao) fica DESLIGADO ate o contador definir (NF-04) —
    /// nao presumir correcao nem gerar lancamento financeiro aqui.
    /// </summary>
    public class LocacaoGarantia : EntidadeSaaSBase
    {
        public Guid LocacaoId { get; private set; }
        public ETipoGarantia Tipo { get; private set; }
        public decimal ValorLimite { get; private set; }
        public DateTime? VigenciaInicio { get; private set; }
        public DateTime? VigenciaFim { get; private set; }
        public EStatusGarantia Status { get; private set; }
        public string? Descricao { get; private set; }
        /// <summary>Garantia que esta foi substituida (auto-referencia).</summary>
        public Guid? SubstituiId { get; private set; }
        /// <summary>Pessoa fiadora quando Tipo = Fiador (FK logica -> PESSOAS).</summary>
        public Guid? FiadorPessoaId { get; private set; }

        protected LocacaoGarantia() { } // EF Core

        public LocacaoGarantia(
            Guid locacaoId,
            ETipoGarantia tipo,
            decimal valorLimite,
            DateTime? vigenciaInicio,
            DateTime? vigenciaFim,
            string? descricao,
            Guid? fiadorPessoaId,
            Guid? substituiId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            LocacaoId = locacaoId;
            Tipo = tipo;
            ValorLimite = valorLimite;
            VigenciaInicio = vigenciaInicio?.Date;
            VigenciaFim = vigenciaFim?.Date;
            Descricao = descricao;
            FiadorPessoaId = fiadorPessoaId;
            SubstituiId = substituiId;
            Status = EStatusGarantia.Ativa;
            Validar();
        }

        /// <summary>Marca esta garantia como substituida por outra (ID6).</summary>
        public void MarcarSubstituida(string usuario)
        {
            if (Status != EStatusGarantia.Ativa)
            {
                AddNotification(nameof(Status), "Apenas garantia ativa pode ser substituida.");
                return;
            }
            Status = EStatusGarantia.Substituida;
            MarcarAlterado(usuario);
        }

        /// <summary>Libera a garantia (ID6). Tratamento financeiro da caucao permanece off (NF-04).</summary>
        public void Liberar(string usuario)
        {
            if (Status == EStatusGarantia.Liberada) return; // idempotente
            Status = EStatusGarantia.Liberada;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<LocacaoGarantia>()
                .Requires()
                .AreNotEquals(LocacaoId, Guid.Empty, nameof(LocacaoId),
                    "A garantia exige locacao. [Origem: LocacaoGarantia] (ID6)")
                .IsGreaterOrEqualsThan(ValorLimite, 0, nameof(ValorLimite),
                    "O valor/limite da garantia nao pode ser negativo. [Origem: LocacaoGarantia]"));

            if (Tipo == ETipoGarantia.Fiador && (!FiadorPessoaId.HasValue || FiadorPessoaId == Guid.Empty))
            {
                AddNotification(nameof(FiadorPessoaId),
                    "Garantia do tipo Fiador exige a pessoa fiadora. [Origem: LocacaoGarantia] (ID6)");
            }

            if (VigenciaInicio.HasValue && VigenciaFim.HasValue && VigenciaFim < VigenciaInicio)
            {
                AddNotification(nameof(VigenciaFim),
                    "O fim da vigencia da garantia deve ser posterior ao inicio. [Origem: LocacaoGarantia]");
            }
        }
    }
}
