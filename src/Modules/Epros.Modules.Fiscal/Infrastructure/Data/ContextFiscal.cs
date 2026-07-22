using Epros.Infrastructure.Data;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Infrastructure.Data
{
    public class ContextFiscal : ContextBase
    {
        public DbSet<DocumentoFiscal> DocumentosFiscais => Set<DocumentoFiscal>();
        public DbSet<DocumentoFiscalItem> DocumentoFiscalItens => Set<DocumentoFiscalItem>();
        public DbSet<EventoDocumentoFiscal> EventosDocumentosFiscais => Set<EventoDocumentoFiscal>();
        public DbSet<DevolucaoFiscal> DevolucoesFiscais => Set<DevolucaoFiscal>();
        public DbSet<DevolucaoFiscalItem> DevolucaoFiscalItens => Set<DevolucaoFiscalItem>();
        public DbSet<InutilizacaoFiscal> InutilizacoesFiscais => Set<InutilizacaoFiscal>();
        public DbSet<NotaServicoEletronica> NotasServicoEletronicas => Set<NotaServicoEletronica>();
        public DbSet<ConhecimentoTransporteEletronico> ConhecimentosTransporteEletronicos => Set<ConhecimentoTransporteEletronico>();
        public DbSet<ManifestoEletronicoDocumentosFiscais> ManifestosEletronicosDocumentosFiscais => Set<ManifestoEletronicoDocumentosFiscais>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
        public DbSet<ProdutoLookup> ProdutosLookup => Set<ProdutoLookup>();
        public DbSet<EmpresaLookup> EmpresasLookup => Set<EmpresaLookup>();
        public DbSet<EmpresaCertificadoLookup> EmpresasCertificadosLookup => Set<EmpresaCertificadoLookup>();
        public DbSet<EmpresaParametrosDfeLookup> EmpresasParametrosDfeLookup => Set<EmpresaParametrosDfeLookup>();
        public DbSet<ConfiguracaoGlobalLookup> ConfiguracoesGlobaisLookup => Set<ConfiguracaoGlobalLookup>();
        public DbSet<Cfop> Cfops => Set<Cfop>();
        public DbSet<CfopPadrao> CfopPadroes => Set<CfopPadrao>();
        public DbSet<TipoOperacaoFiscal> TiposOperacoesFiscais => Set<TipoOperacaoFiscal>();
        public DbSet<CodigoBeneficioFiscal> CodigosBeneficiosFiscais => Set<CodigoBeneficioFiscal>();
        public DbSet<CodigoBeneficioFiscalCst> CodigosBeneficiosFiscaisCst => Set<CodigoBeneficioFiscalCst>();
        public DbSet<CodigoBeneficioFiscalCsosn> CodigosBeneficiosFiscaisCsosn => Set<CodigoBeneficioFiscalCsosn>();
        public DbSet<Contador> Contadores => Set<Contador>();
        public DbSet<Servico> Servicos => Set<Servico>();
        public DbSet<CodigoServicoSefaz> CodigosServicosSefaz => Set<CodigoServicoSefaz>();
        public DbSet<Ncm> Ncms => Set<Ncm>();
        public DbSet<NcmTributacao> NcmTributacoes => Set<NcmTributacao>();
        public DbSet<NcmTributacaoEmpresa> NcmTributacaoEmpresas => Set<NcmTributacaoEmpresa>();
        public DbSet<NcmTributacaoSt> NcmTributacaoSts => Set<NcmTributacaoSt>();
        public DbSet<NcmTributacaoFundoCombatePobreza> NcmTributacaoFundoCombatePobrezas => Set<NcmTributacaoFundoCombatePobreza>();
        public DbSet<NcmConfiguracao> NcmConfiguracoes => Set<NcmConfiguracao>();
        public DbSet<Cest> Cests => Set<Cest>();
        public DbSet<CstIbsCbs> CstsIbsCbs => Set<CstIbsCbs>();
        public DbSet<ClassificacaoTributaria> ClassificacoesTributarias => Set<ClassificacaoTributaria>();
        public DbSet<ClassificacaoTributariaAnexo> ClassificacoesTributariasAnexos => Set<ClassificacaoTributariaAnexo>();
        public DbSet<CodigoAnp> CodigosAnp => Set<CodigoAnp>();
        public DbSet<EnquadramentoIpi> EnquadramentosIpi => Set<EnquadramentoIpi>();
        public DbSet<FcpAliquotaUf> FcpAliquotasUf => Set<FcpAliquotaUf>();
        public DbSet<IcmsAliquotaInterestadual> IcmsAliquotasInterestaduais => Set<IcmsAliquotaInterestadual>();
        public DbSet<TributarioGrupo> TributarioGrupos => Set<TributarioGrupo>();
        public DbSet<TributarioGrupoEmpresa> TributarioGrupoEmpresas => Set<TributarioGrupoEmpresa>();
        public DbSet<ObservacaoNfe> ObservacoesNfe => Set<ObservacaoNfe>();
        public DbSet<ConfiguracaoImpressaoNfce> ConfiguracoesImpressaoNfce => Set<ConfiguracaoImpressaoNfce>();
        public DbSet<ConfiguracaoDFe> ConfiguracoesDFe => Set<ConfiguracaoDFe>();
        public DbSet<Ibpt> Ibpts => Set<Ibpt>();

        public ContextFiscal(
            DbContextOptions<ContextFiscal> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define o schema do banco do PostgreSQL para o macrodomínio fiscal/plataforma
            modelBuilder.HasDefaultSchema("plataforma");

            modelBuilder.Entity<Cfop>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Descricao).HasMaxLength(1000);
                entity.Property(c => c.NaturezaOperacao).HasMaxLength(1000);
                entity.Property(c => c.CfopCorrelacao).HasMaxLength(4);
                entity.Property(c => c.CfopDevolucao).HasMaxLength(4);
            });

            modelBuilder.Entity<CfopPadrao>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Descricao).HasMaxLength(1000);
                entity.Property(c => c.NaturezaOperacao).HasMaxLength(1000);
                entity.Property(c => c.CfopCorrelacao).HasMaxLength(4);
                entity.Property(c => c.CfopDevolucao).HasMaxLength(4);
            });

            modelBuilder.Entity<TipoOperacaoFiscal>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Descricao).HasMaxLength(150);
                entity.HasOne(t => t.CfopNfe)
                      .WithMany()
                      .HasForeignKey(t => t.CfopNfeId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.CfopNfce)
                      .WithMany()
                      .HasForeignKey(t => t.CfopNfceId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CodigoBeneficioFiscal>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Codigo).HasMaxLength(10).IsRequired();
                entity.Property(c => c.Descricao).HasMaxLength(1000);
                entity.HasMany(c => c.Csts)
                      .WithOne()
                      .HasForeignKey(x => x.CodigoBeneficioFiscalId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(c => c.Csosns)
                      .WithOne()
                      .HasForeignKey(x => x.CodigoBeneficioFiscalId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CodigoBeneficioFiscalCst>(entity =>
            {
                entity.HasKey(c => c.Id);
            });

            modelBuilder.Entity<CodigoBeneficioFiscalCsosn>(entity =>
            {
                entity.HasKey(c => c.Id);
            });

            modelBuilder.Entity<DocumentoFiscal>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Total).HasPrecision(18, 2);
                entity.Property(d => d.ChaveReferenciada).HasMaxLength(44);
                entity.Property(d => d.JustificativaContingencia).HasMaxLength(256);
                entity.HasMany(d => d.Itens)
                      .WithOne()
                      .HasForeignKey(i => i.DocumentoFiscalId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DocumentoFiscalItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Quantidade).HasPrecision(18, 2);
                entity.Property(i => i.ValorUnitario).HasPrecision(18, 2);
                entity.Property(i => i.ValorTotal).HasPrecision(18, 2);
                entity.Property(i => i.AliquotaIcms).HasPrecision(18, 2);
                entity.Property(i => i.ValorIcms).HasPrecision(18, 2);
                entity.Property(i => i.Csosn).HasMaxLength(4);
                entity.Property(i => i.Origem).HasMaxLength(1);
                entity.Property(i => i.CstIpi).HasMaxLength(2);
                entity.Property(i => i.AliquotaIpi).HasPrecision(18, 2);
                entity.Property(i => i.CstPisCofins).HasMaxLength(2);
                entity.Property(i => i.AliquotaPis).HasPrecision(18, 2);
                entity.Property(i => i.AliquotaCofins).HasPrecision(18, 2);
                entity.Property(i => i.CstIbsCbs).HasMaxLength(5);
                entity.Property(i => i.CClassTrib).HasMaxLength(20);
                entity.Property(i => i.BaseCalculoIcms).HasPrecision(18, 2);
                entity.Property(i => i.ValorIpi).HasPrecision(18, 2);
                entity.Property(i => i.ValorPis).HasPrecision(18, 2);
                entity.Property(i => i.ValorCofins).HasPrecision(18, 2);
                entity.Property(i => i.ValorIcmsSt).HasPrecision(18, 2);
                entity.Property(i => i.ValorFcp).HasPrecision(18, 2);
            });

            modelBuilder.Entity<EventoDocumentoFiscal>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            // ----- DevolucaoFiscal (documento de devolução, tabela devolucaos) -----
            modelBuilder.Entity<DevolucaoFiscal>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Modelo).HasMaxLength(2);
                entity.Property(d => d.ChaveNfEntrada).HasMaxLength(44);
                entity.Property(d => d.ChaveGerada).HasMaxLength(44);
                entity.Property(d => d.Protocolo).HasMaxLength(50);
                entity.Property(d => d.Motivo).HasMaxLength(1000);
                entity.Property(d => d.MensagemRetorno).HasMaxLength(2000);
                entity.Property(d => d.DestinatarioCnpjCpf).HasMaxLength(14);
                entity.Property(d => d.DestinatarioNome).HasMaxLength(150);
                entity.Property(d => d.Total).HasPrecision(18, 2);
                entity.HasIndex(d => new { d.TenantId, d.Estado });
                entity.HasIndex(d => new { d.TenantId, d.ChaveNfEntrada });
                entity.HasMany(d => d.Itens)
                      .WithOne()
                      .HasForeignKey(i => i.DevolucaoFiscalId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ----- DevolucaoFiscalItem (itens da devolução, tabela item_devolucaos) -----
            modelBuilder.Entity<DevolucaoFiscalItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Sku).HasMaxLength(60);
                entity.Property(i => i.NomeProduto).HasMaxLength(150);
                entity.Property(i => i.Ncm).HasMaxLength(8);
                entity.Property(i => i.Cst).HasMaxLength(4);
                entity.Property(i => i.Quantidade).HasPrecision(18, 2);
                entity.Property(i => i.ValorUnitario).HasPrecision(18, 2);
                entity.Property(i => i.ValorTotal).HasPrecision(18, 2);
                entity.Property(i => i.AliquotaIcms).HasPrecision(18, 2);
                entity.HasIndex(i => new { i.TenantId, i.DevolucaoFiscalId });
            });

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("outbox_messages");
            });

            // ----- ProdutoLookup (Estoque) -----
            // Lookup de leitura cross-module: tabela estoque.produtos pertence ao módulo Estoque.
            // ExcludeFromMigrations para o Fiscal não tentar criar/alterar a tabela de outro módulo.
            modelBuilder.Entity<ProdutoLookup>(entity =>
            {
                entity.ToTable("produtos", "estoque", t => t.ExcludeFromMigrations());
                entity.HasKey(p => p.Id);
            });

            // ----- Lookups de emitente (GestaoClientes, schema plataforma) -----
            // Leitura cross-module (§6.4): o Fiscal NÃO referencia o projeto GestaoClientes; projeta as
            // tabelas do emitente/certificado/parâmetros DF-e como entidades planas, ExcludeFromMigrations
            // (as tabelas são criadas/mantidas pelo ContextGestaoClientes). Colunas seguem o snake_case do ContextBase.
            modelBuilder.Entity<EmpresaLookup>(entity =>
            {
                entity.ToTable("empresas", "plataforma", t => t.ExcludeFromMigrations());
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => e.DeletadoEm == null);
            });

            modelBuilder.Entity<EmpresaCertificadoLookup>(entity =>
            {
                entity.ToTable("empresa_certificado", "plataforma", t => t.ExcludeFromMigrations());
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => e.DeletadoEm == null);
            });

            modelBuilder.Entity<EmpresaParametrosDfeLookup>(entity =>
            {
                entity.ToTable("empresas_parametros_dfe", "plataforma", t => t.ExcludeFromMigrations());
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => e.DeletadoEm == null);
            });

            modelBuilder.Entity<ConfiguracaoGlobalLookup>(entity =>
            {
                entity.ToTable("configuracoes_globais", "plataforma", t => t.ExcludeFromMigrations());
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => e.DeletadoEm == null);
            });

            // ----- Contador -----
            modelBuilder.Entity<Contador>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.RazaoSocial).HasMaxLength(100);
                entity.Property(c => c.NomeContador).HasMaxLength(100);
                entity.Property(c => c.Cpf).HasMaxLength(11);
                entity.Property(c => c.Cnpj).HasMaxLength(14);
                entity.Property(c => c.NumeroCrc).HasMaxLength(15).IsRequired();
                entity.Property(c => c.Qualificacao).HasMaxLength(60);
                entity.Property(c => c.Funcao).HasMaxLength(60);
                entity.Property(c => c.Telefone).HasMaxLength(11);
                entity.Property(c => c.Email).HasMaxLength(150);
                entity.Property(c => c.Logradouro).HasMaxLength(100).IsRequired();
                entity.Property(c => c.Numero).HasMaxLength(10).IsRequired();
                entity.Property(c => c.Complemento).HasMaxLength(60);
                entity.Property(c => c.Bairro).HasMaxLength(60).IsRequired();
                entity.Property(c => c.Cep).HasMaxLength(8).IsRequired();
            });

            // ----- CodigoServicoSefaz -----
            modelBuilder.Entity<CodigoServicoSefaz>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Codigo).HasMaxLength(5).IsRequired();
                entity.Property(c => c.Descricao).HasMaxLength(1000).IsRequired();
            });

            // ----- Servico -----
            modelBuilder.Entity<Servico>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Codigo).HasMaxLength(10).IsRequired();
                entity.Property(s => s.Descricao).HasMaxLength(120).IsRequired();
                entity.Property(s => s.Valor).HasPrecision(18, 2);
                entity.Property(s => s.InformacaoAdicional).HasMaxLength(120);
                entity.Property(s => s.CodigoNbs).HasMaxLength(9);
                entity.Property(s => s.CstIbsCbs).HasMaxLength(5000);
                entity.Property(s => s.CClassTrib).HasMaxLength(5000);
                entity.Property(s => s.AliquotaIss).HasPrecision(18, 4);
                entity.Property(s => s.AliquotaIssRetido).HasPrecision(18, 4);
                entity.Property(s => s.AliquotaIrrfRetido).HasPrecision(18, 4);
                entity.Property(s => s.AliquotaInss).HasPrecision(18, 4);
                entity.Property(s => s.AliquotaPis).HasPrecision(18, 4);
                entity.Property(s => s.AliquotaCofins).HasPrecision(18, 4);
                entity.HasIndex(s => new { s.TenantId, s.EmpresaId });

                entity.HasOne(s => s.CodigoServicoSefaz)
                      .WithMany()
                      .HasForeignKey(s => s.CodigoServicoSefazId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ----- Ncm -----
            modelBuilder.Entity<Ncm>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.CodigoNcm).HasMaxLength(8).IsRequired();
                entity.Property(n => n.Descricao).HasMaxLength(1500);
                entity.Property(n => n.TipoAtoIni).HasMaxLength(20);
                entity.Property(n => n.NumeroAtoIni).HasMaxLength(20);
                entity.Property(n => n.AnoAtoIni).HasMaxLength(4);
                entity.HasIndex(n => new { n.TenantId, n.CodigoNcm });
            });

            // ----- NcmTributacao -----
            modelBuilder.Entity<NcmTributacao>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Descricao).HasMaxLength(200);
                entity.Property(n => n.EnquadramentoIpi).HasMaxLength(3);
                entity.Property(n => n.CodigoBeneficioFiscalIcms).HasMaxLength(10);
                entity.Property(n => n.InformacoesComplementares).HasMaxLength(5000);
                entity.Property(n => n.InformacoesAdicionaisAoFisco).HasMaxLength(2000);
                entity.Property(n => n.CstIbsCbsNfe).HasMaxLength(5000);
                entity.Property(n => n.CClassTribNfe).HasMaxLength(5000);
                entity.Property(n => n.CstIbsCbsNfce).HasMaxLength(5000);
                entity.Property(n => n.CClassTribNfce).HasMaxLength(5000);
                entity.Property(n => n.ValorUnitFixoPis).HasPrecision(18, 2);
                entity.Property(n => n.ValorUnitFixoCofins).HasPrecision(18, 2);
                entity.Property(n => n.ValorAliquotaPis).HasPrecision(18, 2);
                entity.Property(n => n.ValorAliquotaCofins).HasPrecision(18, 2);
                entity.Property(n => n.ValorAliquotaIpi).HasPrecision(18, 2);
                entity.Property(n => n.ValorPercentualReducacaoBcIpi).HasPrecision(18, 2);
                entity.Property(n => n.ValorAliquotaIcmsInterna).HasPrecision(18, 2);
                entity.Property(n => n.ValorPercentualReducacaoBcIcmsInterna).HasPrecision(18, 2);
                entity.Property(n => n.ValorAliquotaIcmsInterstadual).HasPrecision(18, 2);
                entity.Property(n => n.ValorPercentualReducacaoBcIcmsInterstadual).HasPrecision(18, 2);
                entity.HasIndex(n => new { n.TenantId, n.TributarioGrupoId });
                entity.HasIndex(n => n.SyncId).IsUnique();

                entity.HasOne(n => n.TributarioGrupo)
                      .WithMany()
                      .HasForeignKey(n => n.TributarioGrupoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(n => n.CodigoBeneficioFiscal)
                      .WithMany()
                      .HasForeignKey(n => n.CodigoBeneficioFiscalId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(n => n.NcmTributacaoSts)
                      .WithOne()
                      .HasForeignKey(x => x.NcmTributacaoId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(n => n.NcmTributacaoFundoCombatePobrezas)
                      .WithOne()
                      .HasForeignKey(x => x.NcmTributacaoId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(n => n.NcmConfiguracoes)
                      .WithOne()
                      .HasForeignKey(x => x.NcmTributacaoId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(n => n.Empresas)
                      .WithOne()
                      .HasForeignKey(x => x.NcmTributacaoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ----- NcmTributacaoEmpresa (join N:N legado NcmTributacao <-> Empresa) -----
            modelBuilder.Entity<NcmTributacaoEmpresa>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.HasIndex(n => new { n.TenantId, n.NcmTributacaoId });
                entity.HasIndex(n => new { n.TenantId, n.EmpresaId });
                entity.HasIndex(n => n.SyncId).IsUnique();
            });

            // ----- NcmTributacaoSt -----
            modelBuilder.Entity<NcmTributacaoSt>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Uf).HasMaxLength(2);
                entity.Property(n => n.ValorAliquotaIcmsSt).HasPrecision(18, 2);
                entity.Property(n => n.ValorMva).HasPrecision(18, 2);
                entity.Property(n => n.ValorPercentualReducaoBcIcmsSt).HasPrecision(18, 2);
                entity.Property(n => n.ValorUnitarioSt).HasPrecision(18, 2);
                entity.Property(n => n.ValorPercentualFcpSt).HasPrecision(18, 2);
                entity.HasIndex(n => new { n.TenantId, n.NcmTributacaoId });
                entity.HasIndex(n => n.SyncId).IsUnique();
            });

            // ----- NcmTributacaoFundoCombatePobreza -----
            modelBuilder.Entity<NcmTributacaoFundoCombatePobreza>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Uf).HasMaxLength(2);
                entity.Property(n => n.ValorPercentual).HasPrecision(18, 2);
                entity.HasIndex(n => new { n.TenantId, n.NcmTributacaoId });
                entity.HasIndex(n => n.SyncId).IsUnique();
            });

            // ----- NcmConfiguracao -----
            modelBuilder.Entity<NcmConfiguracao>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.HasIndex(n => new { n.TenantId, n.NcmTributacaoId });
                entity.HasIndex(n => n.SyncId).IsUnique();
            });

            // ----- Cest -----
            modelBuilder.Entity<Cest>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Codigo).HasMaxLength(7).IsRequired();
                entity.Property(c => c.Descricao).HasMaxLength(1000);
                entity.HasIndex(c => new { c.TenantId, c.Codigo });
            });

            // ----- CstIbsCbs -----
            modelBuilder.Entity<CstIbsCbs>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Cst).HasMaxLength(5).IsRequired();
                entity.Property(c => c.Descricao).HasMaxLength(1000);
                entity.HasIndex(c => new { c.TenantId, c.Cst });
                entity.HasMany(c => c.ClassesTributarias)
                      .WithOne()
                      .HasForeignKey(x => x.CstIbsCbsId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ----- ClassificacaoTributaria -----
            modelBuilder.Entity<ClassificacaoTributaria>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Codigo).HasMaxLength(20).IsRequired();
                entity.Property(c => c.Descricao).HasMaxLength(1000);
                entity.HasIndex(c => new { c.TenantId, c.CstIbsCbsId });
                entity.HasMany(c => c.Anexos)
                      .WithOne()
                      .HasForeignKey(x => x.ClassificacaoTributariaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ----- ClassificacaoTributariaAnexo -----
            modelBuilder.Entity<ClassificacaoTributariaAnexo>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Codigo).HasMaxLength(20).IsRequired();
                entity.HasIndex(c => new { c.TenantId, c.ClassificacaoTributariaId });
            });

            // ----- CodigoAnp -----
            modelBuilder.Entity<CodigoAnp>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Codigo).HasMaxLength(20).IsRequired();
                entity.Property(c => c.Descricao).HasMaxLength(1000);
                entity.HasIndex(c => new { c.TenantId, c.Codigo });
            });

            // ----- EnquadramentoIpi -----
            modelBuilder.Entity<EnquadramentoIpi>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Codigo).HasMaxLength(3).IsRequired();
                entity.Property(e => e.Descricao).HasMaxLength(1000);
                entity.HasIndex(e => new { e.TenantId, e.Codigo });
            });

            // ----- FcpAliquotaUf -----
            modelBuilder.Entity<FcpAliquotaUf>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.ValorAliquota).HasPrecision(18, 2);
                entity.Property(f => f.Observacao).HasMaxLength(1000);
                entity.HasIndex(f => new { f.TenantId, f.Uf });
            });

            // ----- IcmsAliquotaInterestadual -----
            modelBuilder.Entity<IcmsAliquotaInterestadual>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.ValorAliquota).HasPrecision(18, 2);
                entity.HasIndex(i => new { i.TenantId, i.UfOrigem, i.UfDestino });
            });

            // ----- TributarioGrupo -----
            modelBuilder.Entity<TributarioGrupo>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Descricao).HasMaxLength(100);
                entity.HasIndex(t => new { t.TenantId, t.Descricao });
                entity.HasMany(t => t.Empresas)
                      .WithOne()
                      .HasForeignKey(x => x.TributarioGrupoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ----- TributarioGrupoEmpresa (join N:N legado TributarioGrupo <-> Empresa) -----
            modelBuilder.Entity<TributarioGrupoEmpresa>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => new { t.TenantId, t.TributarioGrupoId });
                entity.HasIndex(t => new { t.TenantId, t.EmpresaId });
                entity.HasIndex(t => t.SyncId).IsUnique();
            });

            // ----- ObservacaoNfe -----
            modelBuilder.Entity<ObservacaoNfe>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.Descricao).HasMaxLength(5000).IsRequired();
                entity.HasIndex(o => o.TenantId);
            });

            // ----- ConfiguracaoImpressaoNfce -----
            modelBuilder.Entity<ConfiguracaoImpressaoNfce>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => new { c.TenantId, c.EmpresaId });
            });

            // ----- ConfiguracaoDFe -----
            modelBuilder.Entity<ConfiguracaoDFe>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.NFeSerieProducao).HasMaxLength(3);
                entity.Property(c => c.NFeUltimoNrProducao).HasMaxLength(20);
                entity.Property(c => c.NFeSerieHomologacao).HasMaxLength(3);
                entity.Property(c => c.NFeUltimoNrHomologacao).HasMaxLength(20);
                entity.Property(c => c.NfceCscProducao).HasMaxLength(100);
                entity.Property(c => c.NfceIdCscProducao).HasMaxLength(10);
                entity.Property(c => c.NfceSerieProducao).HasMaxLength(3);
                entity.Property(c => c.NfceUltimoNrProducao).HasMaxLength(20);
                entity.Property(c => c.NfceCscHomologacao).HasMaxLength(100);
                entity.Property(c => c.NfceIdCscHomologacao).HasMaxLength(10);
                entity.Property(c => c.NfceSerieHomologacao).HasMaxLength(3);
                entity.Property(c => c.NfceUltimoNrHomologacao).HasMaxLength(20);
                entity.HasIndex(c => new { c.TenantId, c.EmpresaId });
            });

            // ----- Ibpt (tabela nacional de alíquotas IBPT por NCM/UF) -----
            modelBuilder.Entity<Ibpt>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Codigo).HasMaxLength(8).IsRequired();
                entity.Property(i => i.Uf).HasMaxLength(2);
                entity.Property(i => i.Descricao).HasMaxLength(1000);
                entity.Property(i => i.Versao).HasMaxLength(20);
                entity.Property(i => i.Chave).HasMaxLength(50);
                entity.Property(i => i.AliquotaNacionalFederal).HasPrecision(18, 4);
                entity.Property(i => i.AliquotaImportadosFederal).HasPrecision(18, 4);
                entity.Property(i => i.AliquotaEstadual).HasPrecision(18, 4);
                entity.Property(i => i.AliquotaMunicipal).HasPrecision(18, 4);
                // Lookup principal: por NCM + UF (+ EX) — usado pelo obter-aliquotas-por-ncm-uf.
                entity.HasIndex(i => new { i.Codigo, i.Uf, i.Ex });
            });

            // ----- InutilizacaoFiscal -----
            modelBuilder.Entity<InutilizacaoFiscal>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Justificativa).HasMaxLength(255);
                entity.Property(i => i.Motivo).HasMaxLength(500);
                entity.Property(i => i.Protocolo).HasMaxLength(50);
                entity.HasIndex(i => new { i.TenantId, i.ModeloDocumento, i.Serie });
            });

            // ----- NotaServicoEletronica (NFS-e) -----
            modelBuilder.Entity<NotaServicoEletronica>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.RpsNumero).HasMaxLength(15);
                entity.Property(n => n.RpsSerie).HasMaxLength(5);
                entity.Property(n => n.PrestadorDocumento).HasMaxLength(14);
                entity.Property(n => n.PrestadorInscricaoMunicipal).HasMaxLength(15);
                entity.Property(n => n.TomadorDocumento).HasMaxLength(14);
                entity.Property(n => n.TomadorRazaoSocial).HasMaxLength(150);
                entity.Property(n => n.ItemListaServico).HasMaxLength(10);
                entity.Property(n => n.CodigoTributacaoMunicipio).HasMaxLength(20);
                entity.Property(n => n.CodigoCnae).HasMaxLength(10);
                entity.Property(n => n.CodigoNbs).HasMaxLength(9);
                entity.Property(n => n.Discriminacao).HasMaxLength(2000);
                entity.Property(n => n.Status).HasMaxLength(20);
                entity.Property(n => n.NumeroNfse).HasMaxLength(30);
                entity.Property(n => n.CodigoVerificacao).HasMaxLength(50);
                entity.Property(n => n.Protocolo).HasMaxLength(50);
                entity.Property(n => n.MotivoRejeicao).HasMaxLength(1000);
                entity.Property(n => n.ValorServicos).HasPrecision(18, 2);
                entity.Property(n => n.ValorDeducoes).HasPrecision(18, 2);
                entity.Property(n => n.ValorIss).HasPrecision(18, 2);
                entity.Property(n => n.ValorIssRetido).HasPrecision(18, 2);
                entity.Property(n => n.AliquotaIss).HasPrecision(18, 4);
                entity.Property(n => n.DescontoIncondicionado).HasPrecision(18, 2);
                entity.Property(n => n.DescontoCondicionado).HasPrecision(18, 2);
                entity.HasIndex(n => new { n.TenantId, n.Status });
                entity.HasIndex(n => new { n.TenantId, n.RpsNumero, n.RpsSerie });
            });

            // ----- ConhecimentoTransporteEletronico (CT-e) -----
            modelBuilder.Entity<ConhecimentoTransporteEletronico>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.ChaveAcesso).HasMaxLength(44);
                entity.Property(c => c.Protocolo).HasMaxLength(50);
                entity.Property(c => c.Status).HasMaxLength(20);
                entity.Property(c => c.MotivoRejeicao).HasMaxLength(1000);
                entity.Property(c => c.RemetenteDocumento).HasMaxLength(14);
                entity.Property(c => c.DestinatarioDocumento).HasMaxLength(14);
                entity.Property(c => c.JustificativaCancelamento).HasMaxLength(255);
                entity.Property(c => c.ValorTotal).HasPrecision(18, 2);
                entity.Property(c => c.ValorReceber).HasPrecision(18, 2);
                entity.HasIndex(c => new { c.TenantId, c.Status });
                entity.HasIndex(c => new { c.TenantId, c.ChaveAcesso });
            });

            // ----- ManifestoEletronicoDocumentosFiscais (MDF-e) -----
            modelBuilder.Entity<ManifestoEletronicoDocumentosFiscais>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.ChaveAcesso).HasMaxLength(44);
                entity.Property(m => m.Protocolo).HasMaxLength(50);
                entity.Property(m => m.Status).HasMaxLength(20);
                entity.Property(m => m.MotivoRejeicao).HasMaxLength(1000);
                entity.Property(m => m.UfInicio).HasMaxLength(2);
                entity.Property(m => m.UfFim).HasMaxLength(2);
                entity.Property(m => m.MunicipioEncerramentoIbge).HasMaxLength(7);
                entity.Property(m => m.ProtocoloEncerramento).HasMaxLength(50);
                entity.Property(m => m.ValorCarga).HasPrecision(18, 2);
                entity.HasIndex(m => new { m.TenantId, m.Status });
                entity.HasIndex(m => new { m.TenantId, m.ChaveAcesso });
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
