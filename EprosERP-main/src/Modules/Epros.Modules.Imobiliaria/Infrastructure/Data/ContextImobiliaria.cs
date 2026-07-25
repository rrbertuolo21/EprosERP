using Epros.Infrastructure.Data;
using Epros.Modules.Imobiliaria.Domain.Entities;
using Epros.Shared.Application.Contracts;
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

            base.OnModelCreating(modelBuilder);
        }
    }
}
