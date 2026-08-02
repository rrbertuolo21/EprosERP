using Epros.Infrastructure.Data;
using Epros.Modules.GRC.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GRC.Infrastructure.Data
{
    public class ContextGRC : ContextBase
    {
        // Frente 1 (ja existentes)
        public DbSet<RiscoCorporativo> RiscosCorporativos => Set<RiscoCorporativo>();
        public DbSet<ControleInterno> ControlesInternos => Set<ControleInterno>();
        public DbSet<IncidenteCompliance> IncidentesCompliance => Set<IncidenteCompliance>();
        public DbSet<Denuncia> Denuncias => Set<Denuncia>();

        // GRC-DEN (Investigacoes e Denuncias)
        public DbSet<DenunciaCategoria> DenunciaCategorias => Set<DenunciaCategoria>();
        public DbSet<DenunciaResposta> DenunciaRespostas => Set<DenunciaResposta>();
        public DbSet<DenunciaParticipante> DenunciaParticipantes => Set<DenunciaParticipante>();
        public DbSet<DenunciaInvestigacao> DenunciaInvestigacoes => Set<DenunciaInvestigacao>();
        public DbSet<DenunciaAnexo> DenunciaAnexos => Set<DenunciaAnexo>();
        public DbSet<DenunciaHistorico> DenunciaHistoricos => Set<DenunciaHistorico>();
        public DbSet<DenunciaParametro> DenunciaParametros => Set<DenunciaParametro>();

        // GRC-POL (Gestao de Politicas)
        public DbSet<Politica> Politicas => Set<Politica>();
        public DbSet<PoliticaVersao> PoliticaVersoes => Set<PoliticaVersao>();
        public DbSet<PoliticaPublicoAlvo> PoliticaPublicosAlvo => Set<PoliticaPublicoAlvo>();
        public DbSet<PoliticaAceite> PoliticaAceites => Set<PoliticaAceite>();

        // GRC-REG (Compliance Regulatorio)
        public DbSet<RegistroRegulatorio> RegistrosRegulatorios => Set<RegistroRegulatorio>();
        public DbSet<CertificadoDigital> CertificadosDigitais => Set<CertificadoDigital>();
        public DbSet<CalendarioRegulatorio> CalendariosRegulatorios => Set<CalendarioRegulatorio>();
        public DbSet<ValidacaoCertificado> ValidacoesCertificado => Set<ValidacaoCertificado>();

        // GRC-RIS (Riscos Corporativos — aditivas ao RiscoCorporativo)
        public DbSet<AvaliacaoRisco> AvaliacoesRisco => Set<AvaliacaoRisco>();
        public DbSet<PlanoAcaoRisco> PlanosAcaoRisco => Set<PlanoAcaoRisco>();
        public DbSet<ControleMitigador> ControlesMitigadores => Set<ControleMitigador>();
        public DbSet<Kri> Kris => Set<Kri>();
        public DbSet<KriLeitura> KriLeituras => Set<KriLeitura>();

        // GRC-CIA (Controles Internos / Auditoria — aditivas ao ControleInterno)
        public DbSet<PlanoAuditoria> PlanosAuditoria => Set<PlanoAuditoria>();
        public DbSet<TesteControle> TestesControle => Set<TesteControle>();
        public DbSet<Achado> Achados => Set<Achado>();
        public DbSet<PlanoAcaoAuditoria> PlanosAcaoAuditoria => Set<PlanoAcaoAuditoria>();
        public DbSet<AmostraAuditoria> AmostrasAuditoria => Set<AmostraAuditoria>();
        public DbSet<TokenAcessoAuditoria> TokensAcessoAuditoria => Set<TokenAcessoAuditoria>();
        public DbSet<EvidenciaAuditoria> EvidenciasAuditoria => Set<EvidenciaAuditoria>();

        // GRC-SOD (Segregacao de Funcoes)
        public DbSet<FuncaoSoD> FuncoesSoD => Set<FuncaoSoD>();
        public DbSet<RegraSoD> RegrasSoD => Set<RegraSoD>();
        public DbSet<SimulacaoSoD> SimulacoesSoD => Set<SimulacaoSoD>();
        public DbSet<ViolacaoSoD> ViolacoesSoD => Set<ViolacaoSoD>();
        public DbSet<ExcecaoSoD> ExcecoesSoD => Set<ExcecaoSoD>();
        public DbSet<BypassSoD> BypassesSoD => Set<BypassSoD>();

        // GRC — Taxonomia normativa unica (D-TEC-05)
        public DbSet<TaxonomiaNormativa> TaxonomiasNormativas => Set<TaxonomiaNormativa>();
        public DbSet<TaxonomiaVinculo> TaxonomiaVinculos => Set<TaxonomiaVinculo>();

        // GRC — Parametrizacao por tenant (D-TEC-04): os 5 grc_*_parametro que faltavam
        public DbSet<PoliticaParametro> PoliticaParametros => Set<PoliticaParametro>();
        public DbSet<ComplianceParametro> ComplianceParametros => Set<ComplianceParametro>();
        public DbSet<RiscoParametro> RiscoParametros => Set<RiscoParametro>();
        public DbSet<ControleParametro> ControleParametros => Set<ControleParametro>();
        public DbSet<SegregacaoParametro> SegregacaoParametros => Set<SegregacaoParametro>();

        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public ContextGRC(
            DbContextOptions<ContextGRC> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("grc");

            modelBuilder.Entity<RiscoCorporativo>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.ToTable("riscos_corporativos");
            });

            modelBuilder.Entity<ControleInterno>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.ToTable("controles_internos");
                entity.HasIndex(c => new { c.TenantId, c.Codigo }).IsUnique();
            });

            modelBuilder.Entity<IncidenteCompliance>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.ToTable("incidentes_compliance");
            });

            modelBuilder.Entity<Denuncia>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.ToTable("denuncias");
                entity.HasIndex(d => new { d.TenantId, d.CodigoAcompanhamento }).IsUnique();
                entity.HasIndex(d => new { d.TenantId, d.Status });
                entity.HasIndex(d => d.CategoriaId);
            });

            // ===================== GRC-DEN (Investigacoes e Denuncias) =====================
            modelBuilder.Entity<DenunciaCategoria>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.ToTable("grc_den_categoria");
                entity.HasIndex(c => new { c.TenantId, c.Nome }).IsUnique();
            });

            modelBuilder.Entity<DenunciaResposta>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.ToTable("grc_den_resposta");
                entity.HasIndex(r => new { r.DenunciaId, r.Interna });
            });

            modelBuilder.Entity<DenunciaParticipante>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.ToTable("grc_den_participante");
                entity.HasIndex(p => new { p.DenunciaId, p.Papel });
            });

            modelBuilder.Entity<DenunciaInvestigacao>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.ToTable("grc_den_investigacao");
                entity.HasIndex(i => new { i.DenunciaId, i.Status });
                entity.HasIndex(i => i.InvestigadorId);
            });

            modelBuilder.Entity<DenunciaAnexo>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.ToTable("grc_den_anexo");
                entity.HasIndex(a => a.DenunciaId);
            });

            modelBuilder.Entity<DenunciaHistorico>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.ToTable("grc_den_historico");
                entity.HasIndex(h => new { h.DenunciaId, h.DataHora });
            });

            modelBuilder.Entity<DenunciaParametro>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.ToTable("grc_den_parametro");
                entity.HasIndex(p => new { p.TenantId, p.Chave }).IsUnique();
            });

            // ===================== GRC-POL =====================
            modelBuilder.Entity<Politica>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.ToTable("grc_pol_politica");
                entity.HasIndex(p => new { p.TenantId, p.Codigo }).IsUnique();
                entity.HasIndex(p => new { p.TenantId, p.Status });
            });

            modelBuilder.Entity<PoliticaVersao>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.ToTable("grc_pol_versao");
                entity.HasIndex(v => new { v.PoliticaId, v.NumeroVersao }).IsUnique();
            });

            modelBuilder.Entity<PoliticaPublicoAlvo>(entity =>
            {
                entity.HasKey(pa => pa.Id);
                entity.ToTable("grc_pol_publico_alvo");
                entity.HasIndex(pa => pa.PoliticaId);
            });

            modelBuilder.Entity<PoliticaAceite>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.ToTable("grc_pol_aceite");
                entity.HasIndex(a => new { a.TenantId, a.VersaoId, a.UsuarioId }).IsUnique();
            });

            // ===================== GRC-REG =====================
            modelBuilder.Entity<RegistroRegulatorio>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.ToTable("grc_reg_registro");
                entity.HasIndex(r => new { r.TenantId, r.Codigo }).IsUnique();
            });

            modelBuilder.Entity<CertificadoDigital>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.ToTable("grc_reg_certificado_digital");
                entity.HasIndex(c => new { c.Cnpj, c.Serial, c.EmpresaId });
            });

            modelBuilder.Entity<CalendarioRegulatorio>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.ToTable("grc_reg_calendario");
                entity.HasIndex(c => new { c.DataVencimento, c.Status });
            });

            modelBuilder.Entity<ValidacaoCertificado>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.ToTable("grc_reg_validacao_certificado");
                entity.HasIndex(v => new { v.CertificadoId, v.DataHora });
            });

            // ===================== GRC-RIS =====================
            modelBuilder.Entity<AvaliacaoRisco>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.ToTable("grc_ris_avaliacao");
                entity.HasIndex(a => a.RiscoId);
            });

            modelBuilder.Entity<PlanoAcaoRisco>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.ToTable("grc_ris_plano_acao");
                entity.HasIndex(p => p.RiscoId);
            });

            modelBuilder.Entity<ControleMitigador>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.ToTable("grc_ris_controle_mitigador");
                entity.HasIndex(c => new { c.RiscoId, c.ControleId }).IsUnique();
            });

            modelBuilder.Entity<Kri>(entity =>
            {
                entity.HasKey(k => k.Id);
                entity.ToTable("grc_ris_kri");
                entity.HasIndex(k => k.RiscoId);
            });

            modelBuilder.Entity<KriLeitura>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.ToTable("grc_ris_kri_leitura");
                entity.HasIndex(l => l.KriId);
            });

            // ===================== GRC-CIA =====================
            modelBuilder.Entity<PlanoAuditoria>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.ToTable("grc_cia_plano_auditoria");
                entity.HasIndex(p => new { p.TenantId, p.Codigo }).IsUnique();
            });

            modelBuilder.Entity<TesteControle>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.ToTable("grc_cia_teste_controle");
                entity.HasIndex(t => t.ControleId);
            });

            modelBuilder.Entity<Achado>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.ToTable("grc_cia_achado");
                entity.HasIndex(a => new { a.Severidade, a.Status });
            });

            modelBuilder.Entity<PlanoAcaoAuditoria>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.ToTable("grc_cia_plano_acao");
                entity.HasIndex(p => new { p.ResponsavelId, p.Status });
            });

            modelBuilder.Entity<AmostraAuditoria>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.ToTable("grc_cia_amostra");
                entity.HasIndex(a => a.TesteControleId);
                entity.HasIndex(a => a.PlanoAuditoriaId);
            });

            modelBuilder.Entity<TokenAcessoAuditoria>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.ToTable("grc_cia_token_acesso");
                entity.HasIndex(t => t.PlanoAuditoriaId);
                entity.HasIndex(t => new { t.TenantId, t.TokenHash }).IsUnique();
            });

            modelBuilder.Entity<EvidenciaAuditoria>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("grc_cia_evidencia");
                entity.HasIndex(e => e.AchadoId);
                entity.HasIndex(e => e.TesteControleId);
            });

            // ===================== GRC-SOD =====================
            modelBuilder.Entity<FuncaoSoD>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.ToTable("grc_sod_funcao");
                entity.HasIndex(f => new { f.TenantId, f.Codigo }).IsUnique();
            });

            modelBuilder.Entity<RegraSoD>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.ToTable("grc_sod_regra");
                entity.HasIndex(r => new { r.TenantId, r.FuncaoAId, r.FuncaoBId, r.VigenciaInicio }).IsUnique();
            });

            modelBuilder.Entity<SimulacaoSoD>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.ToTable("grc_sod_simulacao");
            });

            modelBuilder.Entity<ViolacaoSoD>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.ToTable("grc_sod_violacao");
                entity.HasIndex(v => new { v.RegraId, v.Status });
            });

            modelBuilder.Entity<ExcecaoSoD>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("grc_sod_excecao");
                entity.HasIndex(e => e.ViolacaoId);
            });

            modelBuilder.Entity<BypassSoD>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.ToTable("grc_sod_bypass_admin");
                entity.HasIndex(b => new { b.TenantId, b.RegraId });
                entity.HasIndex(b => b.AtorId);
            });

            // ===================== GRC — Taxonomia normativa unica (D-TEC-05) =====================
            modelBuilder.Entity<TaxonomiaNormativa>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.ToTable("grc_taxonomia_normativa");
                entity.HasIndex(t => new { t.TenantId, t.Codigo }).IsUnique();
                entity.HasIndex(t => new { t.TenantId, t.Tipo });
                entity.HasIndex(t => t.CatalogoPaiId);
            });

            modelBuilder.Entity<TaxonomiaVinculo>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.ToTable("grc_taxonomia_vinculo");
                entity.HasIndex(v => new { v.OrigemTipo, v.OrigemId });
                entity.HasIndex(v => new { v.DestinoTipo, v.DestinoId });
            });

            // ===================== GRC — Parametros por tenant (D-TEC-04) =====================
            modelBuilder.Entity<PoliticaParametro>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.ToTable("grc_pol_parametro");
                entity.HasIndex(p => new { p.TenantId, p.Chave }).IsUnique();
            });

            modelBuilder.Entity<ComplianceParametro>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.ToTable("grc_reg_parametro");
                entity.HasIndex(p => new { p.TenantId, p.Chave }).IsUnique();
            });

            modelBuilder.Entity<RiscoParametro>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.ToTable("grc_ris_parametro");
                entity.HasIndex(p => new { p.TenantId, p.Chave }).IsUnique();
            });

            modelBuilder.Entity<ControleParametro>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.ToTable("grc_cia_parametro");
                entity.HasIndex(p => new { p.TenantId, p.Chave }).IsUnique();
            });

            modelBuilder.Entity<SegregacaoParametro>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.ToTable("grc_sod_parametro");
                entity.HasIndex(p => new { p.TenantId, p.Chave }).IsUnique();
            });

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("outbox_messages", "grc");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
