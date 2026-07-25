using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Política de garantia (ven_garantia_politica). Fonte: EF §10.1. Regras GAR-001..GAR-011.
    /// GAR-002/GAR-003: duração e tipo de duração são obrigatórios. Nome/descrição existem no
    /// modelo mas não são comprovados obrigatórios (GAR-004/GAR-005) → opcionais.
    /// </summary>
    public class GarantiaPolitica : EntidadeSaaSBase
    {
        public string? Nome { get; private set; }
        public string? Descricao { get; private set; }
        public int Duracao { get; private set; }
        public EGarantiaTipoDuracao TipoDuracao { get; private set; }
        public bool Ativo { get; private set; } = true;
        public long? SequenciaExibicao { get; private set; }

        protected GarantiaPolitica() { }

        public GarantiaPolitica(
            string? nome,
            string? descricao,
            int duracao,
            EGarantiaTipoDuracao tipoDuracao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Descricao = descricao;
            Duracao = duracao;
            TipoDuracao = tipoDuracao;
            Ativo = true;
            Validar();
        }

        public void Alterar(string? nome, string? descricao, int duracao, EGarantiaTipoDuracao tipoDuracao, string alteradoPor)
        {
            Nome = nome;
            Descricao = descricao;
            Duracao = duracao;
            TipoDuracao = tipoDuracao;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        /// <summary>GAR: inativação operacional (mantém histórico, não disponível para novas aplicações).</summary>
        public void Inativar(string alteradoPor)
        {
            Ativo = false;
            MarcarAlterado(alteradoPor);
        }

        public void Reativar(string alteradoPor)
        {
            Ativo = true;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            // GAR-002 / GAR-006: duração obrigatória e inteiro positivo.
            AddNotifications(new Contract<GarantiaPolitica>()
                .Requires()
                .IsGreaterThan(Duracao, 0, nameof(Duracao), "A duração da garantia deve ser maior que zero. [Origem: GarantiaPolitica]"));
        }
    }

    /// <summary>
    /// Cobertura de garantia aplicada a venda/produto (ven_garantia_cobertura). Fonte: EF §10.2.
    /// GAR-014/GAR-015: guarda referência da venda/linha e/ou do produto. GAR-016/GAR-017: calcula
    /// vencimento quando há data de origem + duração; senão marca situação Indeterminada.
    /// </summary>
    public class GarantiaCobertura : EntidadeSaaSBase
    {
        public Guid GarantiaPoliticaId { get; private set; }
        public Guid? VendaId { get; private set; }
        public Guid? VendaItemId { get; private set; }
        public Guid? ProdutoId { get; private set; }
        public Guid? ClienteId { get; private set; }
        public string? NumeroSerieLote { get; private set; }
        public DateTime? DataOrigem { get; private set; }
        public DateTime? DataVencimento { get; private set; }
        public EGarantiaSituacaoCobertura Situacao { get; private set; } = EGarantiaSituacaoCobertura.Indeterminada;
        public string? Observacao { get; private set; }

        protected GarantiaCobertura() { }

        public GarantiaCobertura(
            Guid garantiaPoliticaId,
            Guid? vendaId,
            Guid? vendaItemId,
            Guid? produtoId,
            Guid? clienteId,
            string? numeroSerieLote,
            DateTime? dataOrigem,
            string? observacao,
            int duracaoPolitica,
            EGarantiaTipoDuracao tipoDuracaoPolitica,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            GarantiaPoliticaId = garantiaPoliticaId;
            VendaId = vendaId;
            VendaItemId = vendaItemId;
            ProdutoId = produtoId;
            ClienteId = clienteId;
            NumeroSerieLote = numeroSerieLote;
            DataOrigem = dataOrigem;
            Observacao = observacao;
            CalcularVencimento(duracaoPolitica, tipoDuracaoPolitica);

            AddNotifications(new Contract<GarantiaCobertura>()
                .Requires()
                .AreNotEquals(garantiaPoliticaId, Guid.Empty, nameof(GarantiaPoliticaId), "A política de garantia é obrigatória. [Origem: GarantiaCobertura]"));
        }

        /// <summary>GAR-016/GAR-017: calcula vencimento; sem dados suficientes → Indeterminada.</summary>
        public void CalcularVencimento(int duracao, EGarantiaTipoDuracao tipoDuracao)
        {
            if (DataOrigem == null || duracao <= 0)
            {
                DataVencimento = null;
                Situacao = EGarantiaSituacaoCobertura.Indeterminada;
                return;
            }

            var origem = DataOrigem.Value;
            DataVencimento = tipoDuracao switch
            {
                EGarantiaTipoDuracao.Dias => origem.AddDays(duracao),
                EGarantiaTipoDuracao.Meses => origem.AddMonths(duracao),
                EGarantiaTipoDuracao.Anos => origem.AddYears(duracao),
                _ => origem.AddDays(duracao)
            };
            Situacao = DateTime.UtcNow.Date <= DataVencimento.Value.Date
                ? EGarantiaSituacaoCobertura.Vigente
                : EGarantiaSituacaoCobertura.Vencida;
        }
    }

    /// <summary>Histórico/auditoria de garantia (ven_garantia_historico). Fonte: EF §10.3. GAR-010/GAR-011.</summary>
    public class GarantiaHistorico : EntidadeSaaSBase
    {
        public EGarantiaEntidadeTipo EntidadeTipo { get; private set; }
        public Guid EntidadeId { get; private set; }
        public EGarantiaEvento Evento { get; private set; }
        public Guid? UsuarioId { get; private set; }
        public string? DadosAnterioresJson { get; private set; }
        public string? DadosNovosJson { get; private set; }
        public DateTime DataEvento { get; private set; }

        protected GarantiaHistorico() { }

        public GarantiaHistorico(
            EGarantiaEntidadeTipo entidadeTipo,
            Guid entidadeId,
            EGarantiaEvento evento,
            Guid? usuarioId,
            string? dadosAnterioresJson,
            string? dadosNovosJson,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EntidadeTipo = entidadeTipo;
            EntidadeId = entidadeId;
            Evento = evento;
            UsuarioId = usuarioId;
            DadosAnterioresJson = dadosAnterioresJson;
            DadosNovosJson = dadosNovosJson;
            DataEvento = DateTime.UtcNow;

            AddNotifications(new Contract<GarantiaHistorico>()
                .Requires()
                .AreNotEquals(entidadeId, Guid.Empty, nameof(EntidadeId), "A entidade do histórico é obrigatória. [Origem: GarantiaHistorico]"));
        }
    }
}
