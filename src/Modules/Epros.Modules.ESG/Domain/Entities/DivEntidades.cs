using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    // ================= ESG-DIV (Diversidade e Responsabilidade Social) =================
    // EF 14.01 §10.2. Construido do zero no V1 (CD1). A regra quantitativa de PRIVACIDADE
    // (limiar de supressao de grupos pequenos) e PARAMETRO (DivParametro.LimiteSupressaoGrupo),
    // nunca constante inventada — NF-09/T1 (LGPD). // valida-humano (privacidade) antes do go-live.

    /// <summary>Programa/iniciativa social — agregado raiz (esg.div_programa). Codigo unico por tenant.</summary>
    public class DivPrograma : AgregadoEsgComWorkflow
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public Guid ResponsavelId { get; private set; }
        public string? Observacao { get; private set; }

        protected DivPrograma() { }

        public DivPrograma(string codigo, string descricao, Guid responsavelId, string? observacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            ResponsavelId = responsavelId;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<DivPrograma>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo do programa e obrigatorio. [Origem: DivPrograma]")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descricao do programa e obrigatoria. [Origem: DivPrograma]")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio. [Origem: DivPrograma]"));
        }
    }

    /// <summary>Indicador social versionado (esg.div_indicador). Codigo+Versao unicos por tenant.</summary>
    public class DivIndicador : EntidadeSaaSBase
    {
        public Guid? ProgramaId { get; private set; } // corporativo (null) ou vinculado a programa
        public string Codigo { get; private set; } = string.Empty;
        public int Versao { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public string Unidade { get; private set; } = string.Empty;
        public string? Fonte { get; private set; }
        public string? Formula { get; private set; }

        protected DivIndicador() { }

        public DivIndicador(Guid? programaId, string codigo, int versao, string descricao, string unidade, string? fonte, string? formula, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ProgramaId = programaId;
            Codigo = codigo;
            Versao = versao <= 0 ? 1 : versao;
            Descricao = descricao;
            Unidade = unidade;
            Fonte = fonte;
            Formula = formula;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<DivIndicador>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo do indicador e obrigatorio. [Origem: DivIndicador]")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descricao do indicador e obrigatoria. [Origem: DivIndicador]")
                .IsNotNullOrEmpty(Unidade, nameof(Unidade), "A unidade do indicador e obrigatoria. [Origem: DivIndicador]")
                .IsGreaterThan(Versao, 0, nameof(Versao), "A versao deve ser positiva. [Origem: DivIndicador]"));
        }
    }

    /// <summary>
    /// Medicao SEMPRE agregada (esg.div_medicao). NF-09/T1: nunca dado individual. Se o tamanho do
    /// grupo (QtdIndividuos) for menor que o limiar de supressao PARAMETRIZADO, a medicao e SUPRIMIDA
    /// (ValorAgregado nao exposto) — o limiar nunca e inventado em codigo.
    /// </summary>
    public class DivMedicao : EntidadeSaaSBase
    {
        public Guid IndicadorId { get; private set; }
        public DateTime PeriodoInicio { get; private set; }
        public DateTime PeriodoFim { get; private set; }
        public string Dimensao { get; private set; } = string.Empty;       // ex.: genero, raca-cor, faixa-etaria (agregada)
        public string DimensoesHash { get; private set; } = string.Empty;  // unicidade funcional
        public int QtdIndividuos { get; private set; }                     // tamanho do grupo agregado
        public decimal? ValorAgregado { get; private set; }                // null quando suprimido
        public bool Suprimido { get; private set; }
        public string Origem { get; private set; } = string.Empty;

        protected DivMedicao() { }

        public DivMedicao(
            Guid indicadorId, DateTime periodoInicio, DateTime periodoFim, string dimensao,
            int qtdIndividuos, decimal valorAgregado, int limiteSupressaoGrupo, string origem,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            IndicadorId = indicadorId;
            PeriodoInicio = periodoInicio.Date;
            PeriodoFim = periodoFim.Date;
            Dimensao = dimensao;
            DimensoesHash = GerarDimensoesHash(dimensao, periodoInicio, periodoFim);
            QtdIndividuos = qtdIndividuos;
            Origem = origem;

            // NF-09/T1: supressao estatistica de grupos pequenos. Limiar vem de parametro (nao inventado).
            if (limiteSupressaoGrupo > 0 && qtdIndividuos < limiteSupressaoGrupo)
            {
                Suprimido = true;
                ValorAgregado = null; // nao expoe grupo pequeno
            }
            else
            {
                Suprimido = false;
                ValorAgregado = valorAgregado;
            }

            Validar();
        }

        private static string GerarDimensoesHash(string dimensao, DateTime ini, DateTime fim)
        {
            var payload = string.Join("|", dimensao ?? string.Empty,
                ini.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                fim.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<DivMedicao>()
                .Requires()
                .AreNotEquals(IndicadorId, Guid.Empty, nameof(IndicadorId), "O indicador e obrigatorio. [Origem: DivMedicao]")
                .IsNotNullOrEmpty(Dimensao, nameof(Dimensao), "A dimensao agregada e obrigatoria. [Origem: DivMedicao]")
                .IsGreaterThan(QtdIndividuos, 0, nameof(QtdIndividuos), "A quantidade de individuos do grupo deve ser positiva. [Origem: DivMedicao]")
                .IsFalse(PeriodoFim < PeriodoInicio, nameof(PeriodoFim), "O fim do periodo deve ser posterior ao inicio. [Origem: DivMedicao]"));
        }
    }

    /// <summary>Meta de indicador social por periodo (esg.div_meta).</summary>
    public class DivMeta : EntidadeSaaSBase
    {
        public Guid IndicadorId { get; private set; }
        public DateTime PeriodoInicio { get; private set; }
        public DateTime PeriodoFim { get; private set; }
        public decimal ValorEsperado { get; private set; }
        public string Unidade { get; private set; } = string.Empty;

        protected DivMeta() { }

        public DivMeta(Guid indicadorId, DateTime periodoInicio, DateTime periodoFim, decimal valorEsperado, string unidade, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            IndicadorId = indicadorId;
            PeriodoInicio = periodoInicio.Date;
            PeriodoFim = periodoFim.Date;
            ValorEsperado = valorEsperado;
            Unidade = unidade;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<DivMeta>()
                .Requires()
                .AreNotEquals(IndicadorId, Guid.Empty, nameof(IndicadorId), "O indicador e obrigatorio. [Origem: DivMeta]")
                .IsNotNullOrEmpty(Unidade, nameof(Unidade), "A unidade da meta e obrigatoria. [Origem: DivMeta]")
                .IsFalse(PeriodoFim < PeriodoInicio, nameof(PeriodoFim), "O fim do periodo deve ser posterior ao inicio. [Origem: DivMeta]"));
        }
    }

    /// <summary>Acao social de um programa (esg.div_acao).</summary>
    public class DivAcao : EntidadeSaaSBase
    {
        public Guid ProgramaId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public Guid ResponsavelId { get; private set; }
        public DateTime? Prazo { get; private set; }
        public string Status { get; private set; } = "Aberta";
        public string? Resultado { get; private set; }

        protected DivAcao() { }

        public DivAcao(Guid programaId, string descricao, Guid responsavelId, DateTime? prazo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ProgramaId = programaId;
            Descricao = descricao;
            ResponsavelId = responsavelId;
            Prazo = prazo?.Date;
            Status = "Aberta";
            Validar();
        }

        public void Concluir(string resultado, string usuario)
        {
            Status = "Concluida";
            Resultado = resultado;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<DivAcao>()
                .Requires()
                .AreNotEquals(ProgramaId, Guid.Empty, nameof(ProgramaId), "O programa e obrigatorio. [Origem: DivAcao]")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descricao da acao e obrigatoria. [Origem: DivAcao]")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel da acao e obrigatorio. [Origem: DivAcao]"));
        }
    }

    /// <summary>Parametro do tenant para DIV (esg.div_parametro). Chave/ValorJson. Guarda o limiar de supressao.</summary>
    public class DivParametro : EntidadeSaaSBase
    {
        public const string ChaveLimiteSupressaoGrupo = "LimiteSupressaoGrupo";

        public string Chave { get; private set; } = string.Empty;
        public string ValorJson { get; private set; } = string.Empty;

        protected DivParametro() { }

        public DivParametro(string chave, string valorJson, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Chave = chave;
            ValorJson = valorJson;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<DivParametro>()
                .Requires()
                .IsNotNullOrEmpty(Chave, nameof(Chave), "A chave do parametro e obrigatoria. [Origem: DivParametro]")
                .IsNotNullOrEmpty(ValorJson, nameof(ValorJson), "O valor do parametro e obrigatorio. [Origem: DivParametro]"));
        }
    }
}
