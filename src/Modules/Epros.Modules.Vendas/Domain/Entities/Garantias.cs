using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Política de garantia (ven_garantia_politica). Fonte: EF §0.1/§10.1. Regras GAR-001..GAR-011.
    ///
    /// CORREÇÃO INV-01 (EF §0.1, GAR-003/GAR-016): duas dimensões de vigência —
    /// TEMPO (<see cref="Duracao"/> + <see cref="TipoDuracao"/> ∈ {Dias,Meses,Anos}) e
    /// USO (<see cref="LimiteUso"/> + <see cref="UnidadeUso"/> ∈ {km,horas}). Ao menos uma
    /// dimensão é obrigatória; quando ambas existem, o vencimento é o que ocorrer primeiro.
    /// Nome/descrição opcionais (GAR-004/GAR-005 não comprovados obrigatórios).
    /// </summary>
    public class GarantiaPolitica : EntidadeSaaSBase
    {
        /// <summary>Piso legal CDC art. 26 — MÍNIMO em dias. 30 (não durável) / 90 (durável). A política amplia, nunca reduz (NF-07, valida-jurídico).</summary>
        public const int PisoCdcDiasNaoDuravel = 30;
        public const int PisoCdcDiasDuravel = 90;

        public string? Nome { get; private set; }
        public string? Descricao { get; private set; }
        /// <summary>Prazo da dimensão de TEMPO (0 = sem dimensão de tempo). GAR-003.</summary>
        public int Duracao { get; private set; }
        /// <summary>Unidade da dimensão de tempo (unidade_tempo). GAR-007.</summary>
        public EGarantiaTipoDuracao TipoDuracao { get; private set; }
        /// <summary>Limite da dimensão de USO (null/0 = sem dimensão de uso). Ex.: 100000 km, 500 horas. GAR-003/GAR-007.</summary>
        public decimal? LimiteUso { get; private set; }
        /// <summary>Unidade da dimensão de uso (unidade_uso). GAR-007.</summary>
        public EGarantiaUnidadeUso UnidadeUso { get; private set; } = EGarantiaUnidadeUso.Nenhuma;
        public bool Ativo { get; private set; } = true;
        public long? SequenciaExibicao { get; private set; }

        /// <summary>Verdadeiro se a política define prazo de tempo. GAR-003.</summary>
        public bool TemDimensaoTempo => Duracao > 0;
        /// <summary>Verdadeiro se a política define limite de uso. GAR-003.</summary>
        public bool TemDimensaoUso => LimiteUso.HasValue && LimiteUso.Value > 0 && UnidadeUso != EGarantiaUnidadeUso.Nenhuma;

        protected GarantiaPolitica() { }

        public GarantiaPolitica(
            string? nome,
            string? descricao,
            int duracao,
            EGarantiaTipoDuracao tipoDuracao,
            string tenantId,
            string criadoPor,
            decimal? limiteUso = null,
            EGarantiaUnidadeUso unidadeUso = EGarantiaUnidadeUso.Nenhuma)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Descricao = descricao;
            Duracao = duracao;
            TipoDuracao = tipoDuracao;
            LimiteUso = limiteUso;
            UnidadeUso = unidadeUso;
            Ativo = true;
            Validar();
        }

        public void Alterar(string? nome, string? descricao, int duracao, EGarantiaTipoDuracao tipoDuracao, string alteradoPor,
            decimal? limiteUso = null, EGarantiaUnidadeUso unidadeUso = EGarantiaUnidadeUso.Nenhuma)
        {
            Nome = nome;
            Descricao = descricao;
            Duracao = duracao;
            TipoDuracao = tipoDuracao;
            LimiteUso = limiteUso;
            UnidadeUso = unidadeUso;
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
            // GAR-003 (INV-01 corrigido): ao menos UMA dimensão de vigência (tempo e/ou uso).
            AddNotifications(new Contract<GarantiaPolitica>()
                .Requires()
                .IsTrue(TemDimensaoTempo || TemDimensaoUso, nameof(Duracao),
                    "A garantia deve ter ao menos uma dimensão de vigência: prazo de tempo (duração) e/ou limite de uso (km/horas). [Origem: GarantiaPolitica GAR-003]")
                // GAR-006: se há dimensão de tempo, a duração é inteiro positivo.
                .IsFalse(Duracao < 0, nameof(Duracao), "A duração da garantia não pode ser negativa. [Origem: GarantiaPolitica]")
                // Se há dimensão de uso, o limite é positivo.
                .IsFalse(LimiteUso.HasValue && LimiteUso.Value < 0, nameof(LimiteUso), "O limite de uso não pode ser negativo. [Origem: GarantiaPolitica]"));
        }
    }

    /// <summary>
    /// Cobertura de garantia aplicada a venda/produto (ven_garantia_cobertura). Fonte: EF §0.1/§10.2.
    /// GAR-014/GAR-015: guarda referência da venda/linha e/ou do produto.
    ///
    /// CORREÇÃO INV-01/GAR-016 (duas dimensões): o vencimento é o que ocorrer PRIMEIRO entre
    /// (a) vencimento por TEMPO = <see cref="DataOrigem"/> + duração/unidade_tempo e
    /// (b) vencimento por USO = <see cref="UsoOrigem"/> + limite_uso/unidade_uso.
    /// <see cref="DataOrigem"/> = data de ENTREGA (NF-07, evento VEN-EVT-003). Sem nenhuma
    /// das origens → situação Indeterminada.
    /// </summary>
    public class GarantiaCobertura : EntidadeSaaSBase
    {
        public Guid GarantiaPoliticaId { get; private set; }
        public Guid? VendaId { get; private set; }
        public Guid? VendaItemId { get; private set; }
        public Guid? ProdutoId { get; private set; }
        public Guid? ClienteId { get; private set; }
        public string? NumeroSerieLote { get; private set; }
        /// <summary>Data de entrega (NF-07); base do vencimento por tempo.</summary>
        public DateTime? DataOrigem { get; private set; }
        public DateTime? DataVencimento { get; private set; }
        /// <summary>Leitura de uso (km/horas) na entrega; base do vencimento por uso. GAR-016.</summary>
        public decimal? UsoOrigem { get; private set; }
        /// <summary>Limite de uso final (uso_origem + limite_uso). GAR-016.</summary>
        public decimal? UsoVencimento { get; private set; }
        /// <summary>Unidade da dimensão de uso herdada da política (para exibição/apuração).</summary>
        public EGarantiaUnidadeUso UnidadeUso { get; private set; } = EGarantiaUnidadeUso.Nenhuma;
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
            string criadoPor,
            decimal? usoOrigem = null,
            decimal? limiteUsoPolitica = null,
            EGarantiaUnidadeUso unidadeUsoPolitica = EGarantiaUnidadeUso.Nenhuma)
            : base(tenantId, criadoPor)
        {
            GarantiaPoliticaId = garantiaPoliticaId;
            VendaId = vendaId;
            VendaItemId = vendaItemId;
            ProdutoId = produtoId;
            ClienteId = clienteId;
            NumeroSerieLote = numeroSerieLote;
            DataOrigem = dataOrigem;
            UsoOrigem = usoOrigem;
            UnidadeUso = unidadeUsoPolitica;
            Observacao = observacao;
            CalcularVencimento(duracaoPolitica, tipoDuracaoPolitica, limiteUsoPolitica, unidadeUsoPolitica);

            AddNotifications(new Contract<GarantiaCobertura>()
                .Requires()
                .AreNotEquals(garantiaPoliticaId, Guid.Empty, nameof(GarantiaPoliticaId), "A política de garantia é obrigatória. [Origem: GarantiaCobertura]"));
        }

        /// <summary>
        /// GAR-016/GAR-017: calcula o vencimento pelas duas dimensões e a situação corrente.
        /// Vencimento por tempo → <see cref="DataVencimento"/>; por uso → <see cref="UsoVencimento"/>.
        /// Sem nenhuma origem → Indeterminada. Vencida quando QUALQUER dimensão for atingida (o que vier primeiro).
        /// </summary>
        public void CalcularVencimento(int duracao, EGarantiaTipoDuracao tipoDuracao, decimal? limiteUso, EGarantiaUnidadeUso unidadeUso)
        {
            UnidadeUso = unidadeUso;

            // Dimensão TEMPO.
            if (DataOrigem != null && duracao > 0)
            {
                var origem = DataOrigem.Value;
                DataVencimento = tipoDuracao switch
                {
                    EGarantiaTipoDuracao.Dias => origem.AddDays(duracao),
                    EGarantiaTipoDuracao.Meses => origem.AddMonths(duracao),
                    EGarantiaTipoDuracao.Anos => origem.AddYears(duracao),
                    _ => origem.AddDays(duracao)
                };
            }
            else
            {
                DataVencimento = null;
            }

            // Dimensão USO.
            UsoVencimento = (UsoOrigem.HasValue && limiteUso.HasValue && limiteUso.Value > 0 && unidadeUso != EGarantiaUnidadeUso.Nenhuma)
                ? UsoOrigem.Value + limiteUso.Value
                : (decimal?)null;

            RecalcularSituacao(UsoOrigem);
        }

        /// <summary>
        /// GAR-016: apura a situação corrente. Se informada a leitura de uso atual, considera a dimensão de uso.
        /// Vencida = o tempo expirou OU o uso atingiu o limite (o que vier primeiro).
        /// </summary>
        public void RecalcularSituacao(decimal? usoAtual)
        {
            var temTempo = DataVencimento.HasValue;
            var temUso = UsoVencimento.HasValue;

            if (!temTempo && !temUso)
            {
                Situacao = EGarantiaSituacaoCobertura.Indeterminada;
                return;
            }

            var vencidaPorTempo = temTempo && DateTime.UtcNow.Date > DataVencimento!.Value.Date;
            var vencidaPorUso = temUso && usoAtual.HasValue && usoAtual.Value >= UsoVencimento!.Value;

            Situacao = (vencidaPorTempo || vencidaPorUso)
                ? EGarantiaSituacaoCobertura.Vencida
                : EGarantiaSituacaoCobertura.Vigente;
        }

        /// <summary>GAR-016: registra nova leitura de uso (km/horas) e reapura a situação (aciona vencimento por uso).</summary>
        public void RegistrarUso(decimal usoAtual, string alteradoPor)
        {
            RecalcularSituacao(usoAtual);
            MarcarAlterado(alteradoPor);
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
