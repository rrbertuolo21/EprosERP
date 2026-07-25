using Epros.Infrastructure.Data;
using Epros.Modules.Qualidade.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Qualidade.Infrastructure.Data
{
    public class ContextQualidade : ContextBase
    {
        // --- Legado / fatia de referencia (mantido para compatibilidade) ---
        public DbSet<InspecaoLote> InspecoesLote => Set<InspecaoLote>();
        public DbSet<NaoConformidade> NaoConformidades => Set<NaoConformidade>();

        // --- QLD-NCR (Nao Conformidades) ---
        public DbSet<NcrRegistro> NcrRegistros => Set<NcrRegistro>();
        public DbSet<NcrOrigemRef> NcrOrigensRef => Set<NcrOrigemRef>();
        public DbSet<NcrCausaRaiz> NcrCausasRaiz => Set<NcrCausaRaiz>();
        public DbSet<NcrAcaoCapa> NcrAcoesCapa => Set<NcrAcaoCapa>();
        public DbSet<NcrVerificacaoEficacia> NcrVerificacoesEficacia => Set<NcrVerificacaoEficacia>();
        public DbSet<NcrAnexo> NcrAnexos => Set<NcrAnexo>();
        public DbSet<NcrHistorico> NcrHistoricos => Set<NcrHistorico>();
        public DbSet<NcrParametro> NcrParametros => Set<NcrParametro>();
        public DbSet<NcrEventoIntegracao> NcrEventosIntegracao => Set<NcrEventoIntegracao>();

        // --- QLD-INS (Planos de Inspecao e Amostragem) ---
        public DbSet<PlanoInspecao> PlanosInspecao => Set<PlanoInspecao>();
        public DbSet<CaracteristicaPlano> CaracteristicasPlano => Set<CaracteristicaPlano>();
        public DbSet<RegraAmostragem> RegrasAmostragem => Set<RegraAmostragem>();
        public DbSet<ExecucaoInspecao> ExecucoesInspecao => Set<ExecucaoInspecao>();
        public DbSet<AmostraInspecionada> AmostrasInspecionadas => Set<AmostraInspecionada>();
        public DbSet<Medicao> Medicoes => Set<Medicao>();
        public DbSet<ResultadoInspecao> ResultadosInspecao => Set<ResultadoInspecao>();

        // --- QLD-ACR (Analise de Aceitacao e Rejeicao) ---
        public DbSet<AcrAnalise> AcrAnalises => Set<AcrAnalise>();
        public DbSet<AcrItem> AcrItens => Set<AcrItem>();
        public DbSet<AcrResultado> AcrResultados => Set<AcrResultado>();
        public DbSet<AcrMotivo> AcrMotivos => Set<AcrMotivo>();
        public DbSet<AcrDocumentoFiscal> AcrDocumentosFiscais => Set<AcrDocumentoFiscal>();
        public DbSet<AcrEventoEstoque> AcrEventosEstoque => Set<AcrEventoEstoque>();
        public DbSet<AcrEventoNcr> AcrEventosNcr => Set<AcrEventoNcr>();
        public DbSet<AcrHistorico> AcrHistoricos => Set<AcrHistorico>();
        public DbSet<AcrAnexo> AcrAnexos => Set<AcrAnexo>();
        public DbSet<AcrParametro> AcrParametros => Set<AcrParametro>();

        // --- QLD-ADM (Administracao da Qualidade) ---
        public DbSet<AdmQualidade> AdmQualidades => Set<AdmQualidade>();
        public DbSet<AdmItem> AdmItens => Set<AdmItem>();
        public DbSet<AdmDocumentoQms> AdmDocumentosQms => Set<AdmDocumentoQms>();
        public DbSet<AdmObjetivo> AdmObjetivos => Set<AdmObjetivo>();
        public DbSet<AdmKpi> AdmKpis => Set<AdmKpi>();
        public DbSet<AdmProgramaAuditoria> AdmProgramasAuditoria => Set<AdmProgramaAuditoria>();
        public DbSet<AdmHistorico> AdmHistoricos => Set<AdmHistorico>();
        public DbSet<AdmAnexo> AdmAnexos => Set<AdmAnexo>();
        public DbSet<AdmParametro> AdmParametros => Set<AdmParametro>();

        // --- QLD-ATR (Gestao de Atributos) ---
        public DbSet<AtrAtributo> AtrAtributos => Set<AtrAtributo>();
        public DbSet<AtrVinculoContexto> AtrVinculosContexto => Set<AtrVinculoContexto>();
        public DbSet<AtrEspecificacao> AtrEspecificacoes => Set<AtrEspecificacao>();
        public DbSet<AtrValor> AtrValores => Set<AtrValor>();
        public DbSet<AtrOpcao> AtrOpcoes => Set<AtrOpcao>();
        public DbSet<AtrHistorico> AtrHistoricos => Set<AtrHistorico>();
        public DbSet<AtrAnexo> AtrAnexos => Set<AtrAnexo>();
        public DbSet<AtrParametro> AtrParametros => Set<AtrParametro>();

        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public ContextQualidade(
            DbContextOptions<ContextQualidade> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("qualidade");

            // ===== Legado / fatia de referencia =====
            modelBuilder.Entity<InspecaoLote>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.ToTable("inspecoes_lote");
                entity.HasIndex(i => new { i.TenantId, i.CompraId });
            });

            modelBuilder.Entity<NaoConformidade>(entity =>
            {
                entity.HasKey(nc => nc.Id);
                entity.ToTable("nao_conformidades");
                entity.HasIndex(nc => new { nc.TenantId, nc.InspecaoLoteId });
            });

            // ===== QLD-NCR =====
            modelBuilder.Entity<NcrRegistro>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_ncr_registro");
                e.Property(x => x.Codigo).HasMaxLength(30);
                e.Property(x => x.Titulo).HasMaxLength(255);
                e.Property(x => x.Descricao).HasMaxLength(4000);
                e.Property(x => x.Serial).HasMaxLength(100);
                e.Property(x => x.Conclusao).HasMaxLength(4000);
                e.Property(x => x.MotivoCancelamento).HasMaxLength(1000);
                e.HasIndex(x => new { x.TenantId, x.Codigo }).IsUnique();
                e.HasIndex(x => new { x.TenantId, x.StatusRegistro, x.EtapaNcr });
                e.HasIndex(x => new { x.TenantId, x.OrigemPrincipal, x.Prioridade });
                e.Ignore(x => x.Origens);
                e.Ignore(x => x.CausasRaiz);
                e.Ignore(x => x.AcoesCapa);
            });
            modelBuilder.Entity<NcrOrigemRef>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_ncr_origem_ref");
                e.Property(x => x.ReferenciaId).HasMaxLength(100);
                e.Property(x => x.ReferenciaCodigo).HasMaxLength(100);
                e.Property(x => x.Observacao).HasMaxLength(1000);
                e.HasIndex(x => new { x.TenantId, x.NcrId });
            });
            modelBuilder.Entity<NcrCausaRaiz>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_ncr_causa_raiz");
                e.Property(x => x.DescricaoAnalise).HasMaxLength(4000);
                e.Property(x => x.CausaIdentificada).HasMaxLength(2000);
                e.Property(x => x.Conclusao).HasMaxLength(4000);
                e.HasIndex(x => new { x.TenantId, x.NcrId });
            });
            modelBuilder.Entity<NcrAcaoCapa>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_ncr_acao_capa");
                e.Property(x => x.Descricao).HasMaxLength(4000);
                e.Property(x => x.Resultado).HasMaxLength(2000);
                e.Property(x => x.MotivoCancelamento).HasMaxLength(1000);
                e.HasIndex(x => new { x.TenantId, x.NcrId });
                e.HasIndex(x => new { x.TenantId, x.Status });
            });
            modelBuilder.Entity<NcrVerificacaoEficacia>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_ncr_verificacao_eficacia");
                e.Property(x => x.Criterio).HasMaxLength(2000);
                e.Property(x => x.DescricaoResultado).HasMaxLength(4000);
                e.Property(x => x.ProximaAcao).HasMaxLength(2000);
                e.HasIndex(x => new { x.TenantId, x.NcrId });
            });
            modelBuilder.Entity<NcrAnexo>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_ncr_anexo");
                e.Property(x => x.TipoEvidencia).HasMaxLength(100);
                e.Property(x => x.Observacao).HasMaxLength(1000);
                e.HasIndex(x => new { x.TenantId, x.NcrId });
            });
            modelBuilder.Entity<NcrHistorico>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_ncr_historico");
                e.Property(x => x.EstadoAnterior).HasMaxLength(100);
                e.Property(x => x.EstadoNovo).HasMaxLength(100);
                e.Property(x => x.IpOrigem).HasMaxLength(64);
                e.Property(x => x.Justificativa).HasMaxLength(2000);
                e.HasIndex(x => new { x.TenantId, x.NcrId });
            });
            modelBuilder.Entity<NcrParametro>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_ncr_parametro");
                e.Property(x => x.Chave).HasMaxLength(150);
                e.HasIndex(x => new { x.TenantId, x.Chave }).IsUnique();
            });
            modelBuilder.Entity<NcrEventoIntegracao>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_ncr_evento_integracao");
                e.Property(x => x.TipoEvento).HasMaxLength(150);
                e.HasIndex(x => new { x.TenantId, x.NcrId });
            });

            // ===== QLD-INS =====
            modelBuilder.Entity<PlanoInspecao>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_ins_plano");
                e.Property(x => x.Codigo).HasMaxLength(30);
                e.Property(x => x.Descricao).HasMaxLength(500);
                e.Property(x => x.MotivoStatus).HasMaxLength(1000);
                e.HasIndex(x => new { x.TenantId, x.Codigo }).IsUnique();
                e.HasIndex(x => new { x.TenantId, x.Status });
                e.Ignore(x => x.Caracteristicas);
                e.Ignore(x => x.RegrasAmostragem);
            });
            modelBuilder.Entity<CaracteristicaPlano>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_ins_caracteristica");
                e.Property(x => x.Nome).HasMaxLength(255);
                e.Property(x => x.ValorNominal).HasMaxLength(255);
                e.Property(x => x.CriterioQualitativo).HasMaxLength(2000);
                e.Property(x => x.MetodoMedicao).HasMaxLength(255);
                e.HasIndex(x => new { x.TenantId, x.PlanoId, x.Sequencia }).IsUnique();
            });
            modelBuilder.Entity<RegraAmostragem>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_ins_regra_amostragem");
                e.Property(x => x.NivelInspecao).HasMaxLength(50);
                e.Property(x => x.Aql).HasMaxLength(50);
                e.Property(x => x.Severidade).HasMaxLength(50);
                e.HasIndex(x => new { x.TenantId, x.PlanoId });
            });
            modelBuilder.Entity<ExecucaoInspecao>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_ins_execucao");
                e.Property(x => x.ReferenciaId).HasMaxLength(100);
                e.Property(x => x.Observacao).HasMaxLength(2000);
                e.HasIndex(x => new { x.TenantId, x.PlanoId });
                e.HasIndex(x => new { x.TenantId, x.Status });
            });
            modelBuilder.Entity<AmostraInspecionada>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_ins_amostra");
                e.Property(x => x.IdentificadorAmostra).HasMaxLength(150);
                e.Property(x => x.Observacao).HasMaxLength(1000);
                e.HasIndex(x => new { x.TenantId, x.ExecucaoId, x.Sequencia }).IsUnique();
            });
            modelBuilder.Entity<Medicao>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_ins_medicao");
                e.Property(x => x.ValorTexto).HasMaxLength(2000);
                e.Property(x => x.Desvio).HasMaxLength(100);
                e.Property(x => x.Observacao).HasMaxLength(1000);
                e.HasIndex(x => new { x.TenantId, x.ExecucaoId });
            });
            modelBuilder.Entity<ResultadoInspecao>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_ins_resultado");
                e.Property(x => x.CriterioAceiteAplicado).HasMaxLength(2000);
                e.Property(x => x.Conclusao).HasMaxLength(4000);
                e.HasIndex(x => new { x.TenantId, x.ExecucaoId }).IsUnique();
            });

            // ===== QLD-ACR =====
            modelBuilder.Entity<AcrAnalise>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_acr_analise");
                e.Property(x => x.Codigo).HasMaxLength(30);
                e.Property(x => x.Descricao).HasMaxLength(500);
                e.HasIndex(x => new { x.TenantId, x.Codigo }).IsUnique();
                e.HasIndex(x => new { x.TenantId, x.Status });
                e.Ignore(x => x.Itens);
                e.Ignore(x => x.Resultados);
            });
            modelBuilder.Entity<AcrItem>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_acr_item");
                e.Property(x => x.CodigoItem).HasMaxLength(100);
                e.Property(x => x.NomeItem).HasMaxLength(255);
                e.Property(x => x.Lote).HasMaxLength(100);
                e.Property(x => x.UnidadeMedida).HasMaxLength(20);
                e.Property(x => x.Ncm).HasMaxLength(20);
                e.Property(x => x.Cfop).HasMaxLength(10);
                e.Property(x => x.Observacao).HasMaxLength(2000);
                e.HasIndex(x => new { x.TenantId, x.AnaliseId });
            });
            modelBuilder.Entity<AcrResultado>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_acr_resultado");
                e.Property(x => x.Severidade).HasMaxLength(50);
                e.Property(x => x.Justificativa).HasMaxLength(2000);
                e.HasIndex(x => new { x.TenantId, x.AnaliseId });
            });
            modelBuilder.Entity<AcrMotivo>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_acr_motivo");
                e.Property(x => x.Codigo).HasMaxLength(30);
                e.Property(x => x.Descricao).HasMaxLength(255);
                e.Property(x => x.Categoria).HasMaxLength(50);
                e.Property(x => x.Severidade).HasMaxLength(50);
                e.HasIndex(x => new { x.TenantId, x.Codigo }).IsUnique();
            });
            modelBuilder.Entity<AcrDocumentoFiscal>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_acr_documento_fiscal");
                e.Property(x => x.ChaveFiscalReferencia).HasMaxLength(44);
                e.Property(x => x.ChaveFiscalGerada).HasMaxLength(44);
                e.Property(x => x.NumeroFiscalReferencia).HasMaxLength(50);
                e.Property(x => x.NumeroFiscalGerado).HasMaxLength(50);
                e.Property(x => x.StatusFiscal).HasMaxLength(50);
                e.Property(x => x.TipoDocumento).HasMaxLength(50);
                e.Property(x => x.MotivoFiscal).HasMaxLength(500);
                e.Property(x => x.ObservacaoFiscal).HasMaxLength(2000);
                e.HasIndex(x => new { x.TenantId, x.ChaveFiscalReferencia });
            });
            modelBuilder.Entity<AcrEventoEstoque>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_acr_evento_estoque");
                e.Property(x => x.Lote).HasMaxLength(100);
                e.HasIndex(x => new { x.TenantId, x.ResultadoId });
            });
            modelBuilder.Entity<AcrEventoNcr>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_acr_evento_ncr");
                e.HasIndex(x => new { x.TenantId, x.ResultadoId });
            });
            modelBuilder.Entity<AcrHistorico>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_acr_historico");
                e.Property(x => x.Entidade).HasMaxLength(100);
                e.Property(x => x.Ip).HasMaxLength(64);
                e.Property(x => x.Motivo).HasMaxLength(1000);
                e.HasIndex(x => new { x.TenantId, x.AnaliseId });
            });
            modelBuilder.Entity<AcrAnexo>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_acr_anexo");
                e.Property(x => x.Entidade).HasMaxLength(100);
                e.Property(x => x.TipoAnexo).HasMaxLength(50);
                e.HasIndex(x => new { x.TenantId, x.AnaliseId });
            });
            modelBuilder.Entity<AcrParametro>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_acr_parametro");
                e.Property(x => x.Chave).HasMaxLength(150);
                e.HasIndex(x => new { x.TenantId, x.Chave }).IsUnique();
            });

            // ===== QLD-ADM =====
            modelBuilder.Entity<AdmQualidade>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_adm_qualidade");
                e.Property(x => x.Codigo).HasMaxLength(30);
                e.Property(x => x.Descricao).HasMaxLength(500);
                e.HasIndex(x => new { x.TenantId, x.Codigo }).IsUnique();
                e.HasIndex(x => new { x.TenantId, x.Status });
                e.Ignore(x => x.Documentos);
                e.Ignore(x => x.Objetivos);
                e.Ignore(x => x.ProgramasAuditoria);
            });
            modelBuilder.Entity<AdmItem>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_adm_item");
                e.Property(x => x.TipoItem).HasMaxLength(50);
                e.Property(x => x.Observacao).HasMaxLength(2000);
                e.HasIndex(x => new { x.TenantId, x.QualidadeId });
            });
            modelBuilder.Entity<AdmDocumentoQms>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_adm_documento_qms");
                e.Property(x => x.Codigo).HasMaxLength(50);
                e.Property(x => x.Titulo).HasMaxLength(255);
                e.Property(x => x.TipoDocumento).HasMaxLength(50);
                e.Property(x => x.VersaoDocumento).HasMaxLength(20);
                e.Property(x => x.MotivoRevisao).HasMaxLength(1000);
                e.HasIndex(x => new { x.TenantId, x.QualidadeId });
                e.HasIndex(x => new { x.TenantId, x.Codigo, x.VersaoDocumento }).IsUnique();
            });
            modelBuilder.Entity<AdmObjetivo>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_adm_objetivo");
                e.Property(x => x.Objetivo).HasMaxLength(255);
                e.Property(x => x.Descricao).HasMaxLength(2000);
                e.Property(x => x.Meta).HasMaxLength(100);
                e.Property(x => x.Unidade).HasMaxLength(50);
                e.HasIndex(x => new { x.TenantId, x.QualidadeId });
            });
            modelBuilder.Entity<AdmKpi>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_adm_kpi");
                e.Property(x => x.Codigo).HasMaxLength(50);
                e.Property(x => x.Nome).HasMaxLength(255);
                e.Property(x => x.Formula).HasMaxLength(1000);
                e.Property(x => x.FonteDados).HasMaxLength(255);
                e.Property(x => x.Periodicidade).HasMaxLength(50);
                e.Property(x => x.Unidade).HasMaxLength(50);
                e.Property(x => x.Periodo).HasMaxLength(50);
                e.Property(x => x.StatusResultado).HasMaxLength(50);
                e.HasIndex(x => new { x.TenantId, x.ObjetivoId });
            });
            modelBuilder.Entity<AdmProgramaAuditoria>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_adm_programa_auditoria");
                e.Property(x => x.Nome).HasMaxLength(255);
                e.Property(x => x.Escopo).HasMaxLength(2000);
                e.Property(x => x.Observacao).HasMaxLength(2000);
                e.HasIndex(x => new { x.TenantId, x.QualidadeId });
            });
            modelBuilder.Entity<AdmHistorico>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_adm_historico");
                e.Property(x => x.Entidade).HasMaxLength(100);
                e.Property(x => x.Ip).HasMaxLength(64);
                e.Property(x => x.Motivo).HasMaxLength(1000);
                e.HasIndex(x => new { x.TenantId, x.QualidadeId });
            });
            modelBuilder.Entity<AdmAnexo>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_adm_anexo");
                e.Property(x => x.Entidade).HasMaxLength(100);
                e.Property(x => x.TipoAnexo).HasMaxLength(50);
                e.HasIndex(x => new { x.TenantId, x.QualidadeId });
            });
            modelBuilder.Entity<AdmParametro>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_adm_parametro");
                e.Property(x => x.Chave).HasMaxLength(150);
                e.HasIndex(x => new { x.TenantId, x.Chave }).IsUnique();
            });

            // ===== QLD-ATR =====
            modelBuilder.Entity<AtrAtributo>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_atr_atributo");
                e.Property(x => x.Codigo).HasMaxLength(30);
                e.Property(x => x.NomeInterno).HasMaxLength(100);
                e.Property(x => x.Rotulo).HasMaxLength(255);
                e.HasIndex(x => new { x.TenantId, x.Codigo }).IsUnique();
                e.HasIndex(x => new { x.TenantId, x.NomeInterno, x.Escopo }).IsUnique();
                e.HasIndex(x => new { x.TenantId, x.Status });
                e.Ignore(x => x.Opcoes);
            });
            modelBuilder.Entity<AtrVinculoContexto>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_atr_vinculo_contexto");
                e.HasIndex(x => new { x.TenantId, x.AtributoId });
                e.HasIndex(x => new { x.TenantId, x.ContextoTipo, x.ContextoId });
            });
            modelBuilder.Entity<AtrEspecificacao>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_atr_especificacao");
                e.Property(x => x.VersaoEspecificacao).HasMaxLength(20);
                e.Property(x => x.ValorNominal).HasMaxLength(255);
                e.Property(x => x.Unidade).HasMaxLength(50);
                e.Property(x => x.MetodoMedicao).HasMaxLength(255);
                e.Property(x => x.Criticidade).HasMaxLength(50);
                e.HasIndex(x => new { x.TenantId, x.AtributoId });
            });
            modelBuilder.Entity<AtrValor>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_atr_valor");
                e.Property(x => x.ContextoTipo).HasMaxLength(50);
                e.Property(x => x.ValorTexto).HasMaxLength(255);
                e.HasIndex(x => new { x.TenantId, x.AtributoId });
                e.HasIndex(x => new { x.TenantId, x.ContextoTipo, x.ContextoId });
            });
            modelBuilder.Entity<AtrOpcao>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_atr_opcao");
                e.Property(x => x.Codigo).HasMaxLength(50);
                e.Property(x => x.Rotulo).HasMaxLength(255);
                e.HasIndex(x => new { x.TenantId, x.AtributoId, x.Codigo }).IsUnique();
            });
            modelBuilder.Entity<AtrHistorico>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_atr_historico");
                e.Property(x => x.Entidade).HasMaxLength(100);
                e.Property(x => x.Motivo).HasMaxLength(1000);
                e.HasIndex(x => new { x.TenantId, x.AtributoId });
            });
            modelBuilder.Entity<AtrAnexo>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_atr_anexo");
                e.Property(x => x.Entidade).HasMaxLength(100);
                e.Property(x => x.TipoAnexo).HasMaxLength(50);
                e.HasIndex(x => new { x.TenantId, x.AtributoId });
            });
            modelBuilder.Entity<AtrParametro>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("qld_atr_parametro");
                e.Property(x => x.Chave).HasMaxLength(150);
                e.HasIndex(x => new { x.TenantId, x.Chave }).IsUnique();
            });

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("outbox_messages", "qualidade");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
