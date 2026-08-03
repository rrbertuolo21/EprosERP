using Epros.Infrastructure.Data;
using Epros.Modules.Imobiliaria.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Imobiliaria.Infrastructure.Data
{
    /// <summary>
    /// DbContext do modulo Imobiliaria (schema "imobiliaria"). Herda ContextBase:
    /// aplica RLS/query filter por tenant, soft delete e convencoes snake_case.
    /// Submodulo IMO-001 (Gestao Imobiliaria) - EF_17_IMOBILIARIA_GESTAO_IMOBILIARIA_V1.
    /// </summary>
    public class ContextImobiliaria : ContextBase
    {
        // IMO-001: Gestao Imobiliaria
        public DbSet<Imovel> Imoveis => Set<Imovel>();
        public DbSet<ImovelProprietario> ImovelProprietarios => Set<ImovelProprietario>();
        public DbSet<ImovelImagem> ImovelImagens => Set<ImovelImagem>();
        public DbSet<ImovelCusto> ImovelCustos => Set<ImovelCusto>();
        public DbSet<ImovelVistoria> ImovelVistorias => Set<ImovelVistoria>();
        public DbSet<ContratoServico> ContratosServico => Set<ContratoServico>();
        public DbSet<Locacao> Locacoes => Set<Locacao>();
        public DbSet<LocacaoParte> LocacaoPartes => Set<LocacaoParte>();
        public DbSet<LocacaoCusto> LocacaoCustos => Set<LocacaoCusto>();
        public DbSet<LocacaoDocumento> LocacaoDocumentos => Set<LocacaoDocumento>();

        // Ciclo do aluguel + garantias/reajuste/rescisao/proposta (escopo maximo)
        public DbSet<LocacaoGarantia> LocacaoGarantias => Set<LocacaoGarantia>();
        public DbSet<LocacaoReajuste> LocacaoReajustes => Set<LocacaoReajuste>();
        public DbSet<LocacaoRescisao> LocacaoRescisoes => Set<LocacaoRescisao>();
        public DbSet<CobrancaAluguel> CobrancasAluguel => Set<CobrancaAluguel>();
        public DbSet<ReciboAluguel> RecibosAluguel => Set<ReciboAluguel>();
        public DbSet<Proposta> Propostas => Set<Proposta>();
        public DbSet<PropostaParte> PropostaPartes => Set<PropostaParte>();

        // TRANSVERSAL T2 — Outbox pos-commit (eventos imo.*).
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public ContextImobiliaria(
            DbContextOptions<ContextImobiliaria> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("imobiliaria");

            // ================= Imovel (agregado raiz) =================
            modelBuilder.Entity<Imovel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("imo_imovel");
                entity.Property(e => e.Descricao).HasMaxLength(300).IsRequired();
                entity.Property(e => e.Cep).HasMaxLength(9);
                entity.Property(e => e.Logradouro).HasMaxLength(200);
                entity.Property(e => e.Numero).HasMaxLength(20);
                entity.Property(e => e.Complemento).HasMaxLength(100);
                entity.Property(e => e.Bairro).HasMaxLength(100);
                entity.Property(e => e.Test1).HasMaxLength(100);
                entity.Property(e => e.Test2).HasMaxLength(100);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(e => new { e.TenantId, e.MunicipioId });

                entity.HasMany(e => e.Proprietarios)
                    .WithOne()
                    .HasForeignKey(p => p.ImovelId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Imagens)
                    .WithOne()
                    .HasForeignKey(i => i.ImovelId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Custos)
                    .WithOne()
                    .HasForeignKey(c => c.ImovelId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Vistorias)
                    .WithOne()
                    .HasForeignKey(v => v.ImovelId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ImovelProprietario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("imo_imovel_proprietario");
                entity.HasIndex(e => new { e.TenantId, e.ImovelId, e.PessoaId }).IsUnique();
            });

            modelBuilder.Entity<ImovelImagem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("imo_imovel_imagem");
                entity.Property(e => e.NomeArquivo).HasMaxLength(255);
                entity.Property(e => e.ContentType).HasMaxLength(100);
                entity.HasIndex(e => new { e.TenantId, e.ImovelId });
            });

            modelBuilder.Entity<ImovelCusto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("imo_imovel_custo");
                entity.Property(e => e.Descricao).HasMaxLength(300);
                entity.HasIndex(e => new { e.TenantId, e.ImovelId });
            });

            modelBuilder.Entity<ImovelVistoria>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("imo_imovel_vistoria");
                entity.Property(e => e.Local).HasMaxLength(200);
                entity.Property(e => e.Descricao).HasMaxLength(2000);
                entity.HasIndex(e => new { e.TenantId, e.ImovelId });
            });

            // ================= Contrato de servico =================
            modelBuilder.Entity<ContratoServico>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("imo_contrato_servico");
                entity.Property(e => e.Descricao).HasMaxLength(2000);
                entity.HasIndex(e => new { e.TenantId, e.ProprietarioId });
                entity.HasIndex(e => new { e.TenantId, e.ImovelId });
            });

            // ================= Locacao (agregado raiz) =================
            modelBuilder.Entity<Locacao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("imo_locacao");
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(e => new { e.TenantId, e.ImovelId });
                entity.HasIndex(e => new { e.TenantId, e.PeriodoInicial, e.PeriodoFinal });

                entity.HasMany(e => e.Partes)
                    .WithOne()
                    .HasForeignKey(p => p.LocacaoId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Custos)
                    .WithOne()
                    .HasForeignKey(c => c.LocacaoId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Documentos)
                    .WithOne()
                    .HasForeignKey(d => d.LocacaoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Ignore(e => e.Locatarios);
                entity.Ignore(e => e.Fiadores);
            });

            modelBuilder.Entity<LocacaoParte>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("imo_locacao_parte");
                entity.Property(e => e.Papel).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(e => new { e.TenantId, e.LocacaoId, e.PessoaId, e.Papel }).IsUnique();
            });

            modelBuilder.Entity<LocacaoCusto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("imo_locacao_custo");
                entity.Property(e => e.Descricao).HasMaxLength(300);
                entity.HasIndex(e => new { e.TenantId, e.LocacaoId });
                entity.HasIndex(e => new { e.TenantId, e.CustoImovelId });
            });

            modelBuilder.Entity<LocacaoDocumento>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("imo_locacao_documento");
                entity.Property(e => e.NomeArquivo).HasMaxLength(255);
                entity.Property(e => e.ContentType).HasMaxLength(100);
                entity.HasIndex(e => new { e.TenantId, e.LocacaoId });
            });

            // ================= Garantias da locacao (ID6/NF-04) =================
            modelBuilder.Entity<LocacaoGarantia>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("imo_locacao_garantia");
                entity.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(30);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.HasIndex(e => new { e.TenantId, e.LocacaoId });
                entity.HasIndex(e => new { e.TenantId, e.SubstituiId });
            });

            // ================= Reajuste (ID7/NF-02) =================
            modelBuilder.Entity<LocacaoReajuste>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("imo_locacao_reajuste");
                entity.Property(e => e.Indice).HasMaxLength(20);
                entity.HasIndex(e => new { e.TenantId, e.LocacaoId });
            });

            // ================= Rescisao (ID7) =================
            modelBuilder.Entity<LocacaoRescisao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("imo_locacao_rescisao");
                entity.Property(e => e.Motivo).HasMaxLength(1000);
                entity.HasIndex(e => new { e.TenantId, e.LocacaoId }).IsUnique();
            });

            // ================= Cobranca de aluguel (ID8/NF-01) =================
            modelBuilder.Entity<CobrancaAluguel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("imo_cobranca_aluguel");
                entity.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.ReceberRef).HasMaxLength(100);
                // Idempotencia por (tenant, locacao, competencia, tipo) — T2/NF-01.
                entity.HasIndex(e => new { e.TenantId, e.LocacaoId, e.Competencia, e.Tipo }).IsUnique();
            });

            // ================= Recibo (ID8/NF-05, numeracao T9) =================
            modelBuilder.Entity<ReciboAluguel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("imo_recibo_aluguel");
                entity.Property(e => e.DocumentoRef).HasMaxLength(200);
                entity.HasIndex(e => new { e.TenantId, e.CobrancaId }).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.Numero }).IsUnique();
            });

            // ================= Proposta (ID2) =================
            modelBuilder.Entity<Proposta>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("imo_proposta");
                entity.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Observacao).HasMaxLength(2000);
                entity.HasIndex(e => new { e.TenantId, e.ImovelId });
                entity.HasIndex(e => new { e.TenantId, e.ContrapropostaDeId });

                entity.HasMany(e => e.Partes)
                    .WithOne()
                    .HasForeignKey(p => p.PropostaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PropostaParte>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("imo_proposta_parte");
                entity.Property(e => e.Papel).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(e => new { e.TenantId, e.PropostaId, e.PessoaId, e.Papel }).IsUnique();
            });

            // ================= Outbox (T2) =================
            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("outbox_messages", "imobiliaria");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
