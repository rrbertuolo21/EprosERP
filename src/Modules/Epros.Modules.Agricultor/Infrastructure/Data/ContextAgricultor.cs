using Epros.Infrastructure.Data;
using Epros.Modules.Agricultor.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Agricultor.Infrastructure.Data
{
    /// <summary>
    /// DbContext do módulo AGRICULTOR (schema "agricultor"). Herda ContextBase: RLS/query filter por
    /// tenant, soft delete e convenções snake_case. Dois submódulos: PAINEL (prefixo agr_) e
    /// LIVRO CAIXA DIGITAL LCDPR (prefixo lcdpr_). Módulo novo (greenfield, AGR-D07/D19).
    /// </summary>
    public class ContextAgricultor : ContextBase
    {
        // ---- Painel do produtor ----
        public DbSet<PropriedadeRural> Propriedades => Set<PropriedadeRural>();
        public DbSet<Talhao> Talhoes => Set<Talhao>();
        public DbSet<Cultura> Culturas => Set<Cultura>();
        public DbSet<Safra> Safras => Set<Safra>();
        public DbSet<CategoriaDespesa> CategoriasDespesa => Set<CategoriaDespesa>();
        public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
        public DbSet<Despesa> Despesas => Set<Despesa>();
        public DbSet<ReceitaProducao> Receitas => Set<ReceitaProducao>();
        public DbSet<Colaborador> Colaboradores => Set<Colaborador>();
        public DbSet<AnotacaoCampo> Anotacoes => Set<AnotacaoCampo>();

        // ---- Livro Caixa Digital (LCDPR) ----
        public DbSet<LcdprEscrituracao> Escrituracoes => Set<LcdprEscrituracao>();
        public DbSet<LcdprDadosCadastrais> LcdprDadosCadastrais => Set<LcdprDadosCadastrais>();
        public DbSet<LcdprImovel> LcdprImoveis => Set<LcdprImovel>();
        public DbSet<LcdprTerceiro> LcdprTerceiros => Set<LcdprTerceiro>();
        public DbSet<LcdprConta> LcdprContas => Set<LcdprConta>();
        public DbSet<LcdprLancamento> LcdprLancamentos => Set<LcdprLancamento>();
        public DbSet<LcdprParamObrigatoriedade> LcdprParamsObrigatoriedade => Set<LcdprParamObrigatoriedade>();

        // TRANSVERSAL T2 — Outbox pós-commit (eventos agr.*).
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public ContextAgricultor(
            DbContextOptions<ContextAgricultor> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("agricultor");

            // ================= PAINEL: Propriedade rural (agregado) =================
            modelBuilder.Entity<PropriedadeRural>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("agr_propriedade_rural");
                e.Property(x => x.NomeImovel).HasMaxLength(200).IsRequired();
                e.Property(x => x.Matricula).HasMaxLength(60);
                e.Property(x => x.Car).HasMaxLength(60);
                e.Property(x => x.CadItrCafir).HasMaxLength(8);
                e.Property(x => x.Caepf).HasMaxLength(14);
                e.Property(x => x.TipoExploracao).HasConversion<string>().HasMaxLength(40);
                e.Property(x => x.Participacao).HasPrecision(5, 2);
                e.Property(x => x.AreaTotalM2).HasPrecision(18, 2);
                e.Property(x => x.Uf).HasMaxLength(2);
                e.Property(x => x.CodigoMunicipioSped).HasMaxLength(7);
                e.Property(x => x.Cep).HasMaxLength(9);
                e.Property(x => x.Endereco).HasMaxLength(300);
                e.HasIndex(x => new { x.TenantId, x.NomeImovel });
                e.HasMany(x => x.Talhoes).WithOne().HasForeignKey(t => t.PropriedadeId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Talhao>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("agr_talhao");
                e.Property(x => x.Nome).HasMaxLength(150).IsRequired();
                e.Property(x => x.AreaM2).HasPrecision(18, 2);
                e.Property(x => x.PoligonoGeoJson).HasColumnType("text");
                e.Ignore(x => x.AreaHectares);
                e.HasIndex(x => new { x.TenantId, x.PropriedadeId });
            });

            modelBuilder.Entity<Cultura>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("agr_cultura");
                e.Property(x => x.Nome).HasMaxLength(150).IsRequired();
                e.Property(x => x.Codigo).HasMaxLength(30);
                e.HasIndex(x => new { x.TenantId, x.Nome });
            });

            modelBuilder.Entity<Safra>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("agr_safra");
                e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
                e.HasIndex(x => new { x.TenantId, x.TalhaoId });
                e.HasIndex(x => new { x.TenantId, x.CulturaId });
            });

            modelBuilder.Entity<CategoriaDespesa>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("agr_categoria_despesa");
                e.Property(x => x.Nome).HasMaxLength(150).IsRequired();
                e.Property(x => x.Codigo).HasMaxLength(30);
                e.HasIndex(x => new { x.TenantId, x.Nome });
            });

            modelBuilder.Entity<Fornecedor>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("agr_fornecedor");
                e.Property(x => x.Nome).HasMaxLength(200).IsRequired();
                e.Property(x => x.CpfCnpj).HasMaxLength(14);
                e.Property(x => x.Codigo).HasMaxLength(30);
                e.HasIndex(x => new { x.TenantId, x.Nome });
            });

            modelBuilder.Entity<Despesa>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("agr_despesa");
                e.Property(x => x.Valor).HasPrecision(18, 2);
                e.Property(x => x.Referencia).HasMaxLength(100);
                e.Property(x => x.TipoDocLcdpr).HasConversion<string>().HasMaxLength(20);
                e.Property(x => x.TipoLancLcdpr).HasConversion<string>().HasMaxLength(30);
                e.Property(x => x.IdParticLcdpr).HasMaxLength(14);
                e.HasIndex(x => new { x.TenantId, x.PropriedadeId });
                e.HasIndex(x => new { x.TenantId, x.SafraId });
            });

            modelBuilder.Entity<ReceitaProducao>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("agr_receita_producao");
                e.Property(x => x.Quantidade).HasPrecision(18, 3);
                e.Property(x => x.Preco).HasPrecision(18, 4);
                e.Property(x => x.Comprador).HasMaxLength(200);
                e.Property(x => x.NumNf).HasMaxLength(60);
                e.Property(x => x.IdParticLcdpr).HasMaxLength(14);
                e.Ignore(x => x.Valor);
                e.HasIndex(x => new { x.TenantId, x.PropriedadeId });
                e.HasIndex(x => new { x.TenantId, x.SafraId });
            });

            modelBuilder.Entity<Colaborador>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("agr_colaborador");
                e.Property(x => x.Nome).HasMaxLength(200).IsRequired();
                e.Property(x => x.Email).HasMaxLength(200);
                e.Property(x => x.Papel).HasConversion<string>().HasMaxLength(20);
                e.HasIndex(x => new { x.TenantId, x.PropriedadeId });
            });

            modelBuilder.Entity<AnotacaoCampo>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("agr_anotacao_campo");
                e.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
                e.Property(x => x.Descricao).HasColumnType("text");
                e.Property(x => x.Tipo).HasMaxLength(50);
                e.Property(x => x.LatLng).HasMaxLength(60);
                e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
                e.HasIndex(x => new { x.TenantId, x.TalhaoId });
            });

            // ================= LCDPR: Escrituração (agregado) =================
            modelBuilder.Entity<LcdprEscrituracao>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("lcdpr_escrituracao");
                e.Property(x => x.Cpf).HasMaxLength(11).IsRequired();
                e.Property(x => x.Nome).HasMaxLength(200).IsRequired();
                e.Property(x => x.FormaApuracao).HasConversion<string>().HasMaxLength(20);
                e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
                e.Property(x => x.IdentificacaoNome).HasMaxLength(200);
                e.Property(x => x.IdentificacaoCpfCnpj).HasMaxLength(14);
                // Chave da escrituração: CPF + DT_FIN (RN31).
                e.HasIndex(x => new { x.TenantId, x.Cpf, x.DtFin }).IsUnique();

                e.HasOne(x => x.DadosCadastrais).WithOne()
                    .HasForeignKey<LcdprDadosCadastrais>(d => d.EscrituracaoId).OnDelete(DeleteBehavior.Cascade);
                e.HasMany(x => x.Imoveis).WithOne()
                    .HasForeignKey(i => i.EscrituracaoId).OnDelete(DeleteBehavior.Cascade);
                e.HasMany(x => x.Contas).WithOne()
                    .HasForeignKey(c => c.EscrituracaoId).OnDelete(DeleteBehavior.Cascade);
                e.HasMany(x => x.Lancamentos).WithOne()
                    .HasForeignKey(l => l.EscrituracaoId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LcdprDadosCadastrais>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("lcdpr_dados_cadastrais");
                e.Property(x => x.Endereco).HasMaxLength(300);
                e.Property(x => x.Uf).HasMaxLength(2);
                e.Property(x => x.CodMunicipio).HasMaxLength(7);
                e.Property(x => x.Cep).HasMaxLength(9);
                e.Property(x => x.Email).HasMaxLength(200);
                e.HasIndex(x => new { x.TenantId, x.EscrituracaoId }).IsUnique();
            });

            modelBuilder.Entity<LcdprImovel>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("lcdpr_imovel");
                e.Property(x => x.NomeImovel).HasMaxLength(200).IsRequired();
                e.Property(x => x.CadItrCafir).HasMaxLength(8);
                e.Property(x => x.Caepf).HasMaxLength(14);
                e.Property(x => x.Uf).HasMaxLength(2);
                e.Property(x => x.CodMunicipio).HasMaxLength(7);
                e.Property(x => x.TipoExploracao).HasConversion<string>().HasMaxLength(40);
                e.Property(x => x.Participacao).HasPrecision(5, 2);
                e.HasIndex(x => new { x.TenantId, x.EscrituracaoId, x.CodImovel }).IsUnique();
                e.HasMany(x => x.Terceiros).WithOne()
                    .HasForeignKey(t => t.ImovelId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LcdprTerceiro>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("lcdpr_terceiro");
                e.Property(x => x.IdContraparte).HasMaxLength(14).IsRequired();
                e.Property(x => x.NomeContraparte).HasMaxLength(200);
                e.Property(x => x.PercContraparte).HasPrecision(5, 2);
                e.HasIndex(x => new { x.TenantId, x.ImovelId });
            });

            modelBuilder.Entity<LcdprConta>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("lcdpr_conta");
                e.Property(x => x.NumConta).HasMaxLength(20);
                e.HasIndex(x => new { x.TenantId, x.EscrituracaoId, x.CodConta }).IsUnique();
            });

            modelBuilder.Entity<LcdprLancamento>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("lcdpr_lancamento");
                e.Property(x => x.NumDoc).HasMaxLength(60);
                e.Property(x => x.Historico).HasColumnType("text");
                e.Property(x => x.IdPartic).HasMaxLength(14);
                e.Property(x => x.TipoDoc).HasConversion<string>().HasMaxLength(20);
                e.Property(x => x.TipoLanc).HasConversion<string>().HasMaxLength(30);
                e.Property(x => x.VlEntrada).HasPrecision(19, 2);
                e.Property(x => x.VlSaida).HasPrecision(19, 2);
                e.HasIndex(x => new { x.TenantId, x.EscrituracaoId, x.Data });
            });

            modelBuilder.Entity<LcdprParamObrigatoriedade>(e =>
            {
                e.HasKey(x => x.Id);
                e.ToTable("lcdpr_param_obrigatoriedade");
                e.Property(x => x.LimiteValor).HasPrecision(19, 2);
                e.Property(x => x.Origem).HasMaxLength(200);
                e.HasIndex(x => new { x.TenantId, x.Ano }).IsUnique();
            });

            // ================= Outbox (T2) =================
            modelBuilder.Entity<OutboxMessage>(e =>
            {
                e.HasKey(o => o.Id);
                e.ToTable("outbox_messages", "agricultor");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
