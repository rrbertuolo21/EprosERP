using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Analytics
{
    /// <summary>
    /// PLT · ANALYTICS (PD-06) — definição VERSIONADA de KPI no catálogo cross-módulo (read-side).
    /// ⛔ A plataforma ORQUESTRA consulta/exportação; a FÓRMULA de negócio (ex.: "resultado mensal") é
    /// do módulo DONO (Financeiro) e vem validada — não é calculada aqui. Esta definição guarda a
    /// fonte e a chave de consulta; o valor efetivo é ingerido como snapshot pelo dono/fonte.
    /// </summary>
    public class DefinicaoKpi : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public string Unidade { get; private set; } = string.Empty; // R$, %, h, un
        public string? Categoria { get; private set; }
        public string? FonteModulo { get; private set; }
        public string? ChaveConsulta { get; private set; }
        public int Versao { get; private set; }
        public bool Ativo { get; private set; }

        protected DefinicaoKpi() { }

        public DefinicaoKpi(string codigo, string nome, string? descricao, string unidade, string? categoria,
            string? fonteModulo, string? chaveConsulta, int versao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<DefinicaoKpi>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O código do KPI é obrigatório.")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do KPI é obrigatório.")
                .IsNotNullOrEmpty(unidade, nameof(Unidade), "A unidade do KPI é obrigatória.")
                .IsGreaterOrEqualsThan(versao, 1, nameof(Versao), "A versão deve ser >= 1."));

            Codigo = codigo;
            Nome = nome;
            Descricao = descricao;
            Unidade = unidade;
            Categoria = categoria;
            FonteModulo = fonteModulo;
            ChaveConsulta = chaveConsulta;
            Versao = versao;
            Ativo = true;
        }

        public void Desativar(string usuario)
        {
            Ativo = false;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>Dashboard configurável (layout como JSON) — hub visual do Analytics.</summary>
    public class Dashboard : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public string? LayoutJson { get; private set; }
        public bool Publico { get; private set; }

        protected Dashboard() { }

        public Dashboard(string codigo, string nome, string? descricao, string? layoutJson, bool publico,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Dashboard>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O código do dashboard é obrigatório.")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do dashboard é obrigatório."));

            Codigo = codigo;
            Nome = nome;
            Descricao = descricao;
            LayoutJson = layoutJson;
            Publico = publico;
        }
    }

    /// <summary>Widget de um dashboard referenciando um KPI do catálogo.</summary>
    public class DashboardWidget : EntidadeSaaSBase
    {
        public Guid DashboardId { get; private set; }
        public string KpiCodigo { get; private set; } = string.Empty;
        public string Titulo { get; private set; } = string.Empty;
        public string TipoVisualizacao { get; private set; } = "numero"; // numero, linha, barra, pizza, tabela
        public int Ordem { get; private set; }
        public string? ConfigJson { get; private set; }

        protected DashboardWidget() { }

        public DashboardWidget(Guid dashboardId, string kpiCodigo, string titulo, string tipoVisualizacao,
            int ordem, string? configJson, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<DashboardWidget>()
                .Requires()
                .IsTrue(dashboardId != Guid.Empty, nameof(DashboardId), "O dashboard é obrigatório.")
                .IsNotNullOrEmpty(kpiCodigo, nameof(KpiCodigo), "O KPI do widget é obrigatório.")
                .IsNotNullOrEmpty(titulo, nameof(Titulo), "O título do widget é obrigatório."));

            DashboardId = dashboardId;
            KpiCodigo = kpiCodigo;
            Titulo = titulo;
            TipoVisualizacao = string.IsNullOrWhiteSpace(tipoVisualizacao) ? "numero" : tipoVisualizacao;
            Ordem = ordem;
            ConfigJson = configJson;
        }
    }

    /// <summary>
    /// Snapshot de métrica (read-side): valor de um KPI numa competência/dimensão, capturado num
    /// instante. O VALOR é fornecido pela fonte/dono (a plataforma não calcula fórmula contábil).
    /// </summary>
    public class SnapshotMetrica : EntidadeSaaSBase
    {
        public string KpiCodigo { get; private set; } = string.Empty;
        public string Competencia { get; private set; } = string.Empty; // "2026-07" ou "2026-07-31"
        public decimal Valor { get; private set; }
        public string? Dimensao { get; private set; } // filial, produto, centro de custo...
        public DateTime CapturadoEm { get; private set; }
        public string? Origem { get; private set; }

        protected SnapshotMetrica() { }

        public SnapshotMetrica(string kpiCodigo, string competencia, decimal valor, string? dimensao,
            string? origem, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<SnapshotMetrica>()
                .Requires()
                .IsNotNullOrEmpty(kpiCodigo, nameof(KpiCodigo), "O KPI do snapshot é obrigatório.")
                .IsNotNullOrEmpty(competencia, nameof(Competencia), "A competência do snapshot é obrigatória."));

            KpiCodigo = kpiCodigo;
            Competencia = competencia;
            Valor = valor;
            Dimensao = dimensao;
            Origem = origem;
            CapturadoEm = DateTime.UtcNow;
        }

        public void AtualizarValor(decimal valor, string? origem, string usuario)
        {
            Valor = valor;
            if (!string.IsNullOrWhiteSpace(origem)) Origem = origem;
            CapturadoEm = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>Solicitação de exportação auditada (o arquivo real é gerado no ambiente).</summary>
    public class ExportacaoAnalytics : EntidadeSaaSBase
    {
        public string Recurso { get; private set; } = string.Empty;
        public string Formato { get; private set; } = "CSV"; // CSV, XLSX, JSON
        public string? FiltroJson { get; private set; }
        public string Status { get; private set; } = "Solicitada"; // Solicitada, Processando, Concluida, Falha
        public string? UrlRef { get; private set; }

        protected ExportacaoAnalytics() { }

        public ExportacaoAnalytics(string recurso, string formato, string? filtroJson, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ExportacaoAnalytics>()
                .Requires()
                .IsNotNullOrEmpty(recurso, nameof(Recurso), "O recurso a exportar é obrigatório.")
                .IsTrue(formato is "CSV" or "XLSX" or "JSON", nameof(Formato), "Formato deve ser CSV, XLSX ou JSON."));

            Recurso = recurso;
            Formato = formato;
            FiltroJson = filtroJson;
            Status = "Solicitada";
        }

        public void Concluir(string urlRef, string usuario)
        {
            Status = "Concluida";
            UrlRef = urlRef;
            MarcarAlterado(usuario);
        }

        public void Falhar(string usuario)
        {
            Status = "Falha";
            MarcarAlterado(usuario);
        }
    }
}
