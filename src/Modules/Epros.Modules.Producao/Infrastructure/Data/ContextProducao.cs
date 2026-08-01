using Epros.Infrastructure.Data;
using Epros.Modules.Producao.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Infrastructure.Data
{
    public class ContextProducao : ContextBase
    {
        public DbSet<ListaMateriais> ListasMateriais => Set<ListaMateriais>();
        public DbSet<BomItem> BomItens => Set<BomItem>();
        public DbSet<OrdemProducao> OrdensProducao => Set<OrdemProducao>();
        public DbSet<ApontamentoProducao> Apontamentos => Set<ApontamentoProducao>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        // PRD-BOM — Estrutura de Produto (BOM)
        public DbSet<BomEstrutura> BomEstruturas => Set<BomEstrutura>();
        public DbSet<BomComponente> BomComponentes => Set<BomComponente>();
        public DbSet<BomGrupoComponente> BomGruposComponente => Set<BomGrupoComponente>();
        public DbSet<BomInstrucao> BomInstrucoes => Set<BomInstrucao>();
        public DbSet<BomInstrucaoOrdem> BomInstrucoesOrdem => Set<BomInstrucaoOrdem>();
        public DbSet<BomHistorico> BomHistoricos => Set<BomHistorico>();
        public DbSet<BomAnexo> BomAnexos => Set<BomAnexo>();

        // PRD-CST — Custos de Produção
        public DbSet<CustoProducao> CustosProducao => Set<CustoProducao>();
        public DbSet<CustoReferencia> CustosReferencia => Set<CustoReferencia>();
        public DbSet<CustoParametro> CustosParametro => Set<CustoParametro>();
        public DbSet<CustoHistorico> CustosHistorico => Set<CustoHistorico>();
        public DbSet<CustoAnexo> CustosAnexo => Set<CustoAnexo>();

        // PRD-PLN — Planejamento de Produção
        public DbSet<PlanejamentoProducao> Planejamentos => Set<PlanejamentoProducao>();
        public DbSet<PlanejamentoSnapshotOp> PlanejamentoSnapshots => Set<PlanejamentoSnapshotOp>();
        public DbSet<PlanejamentoHistorico> PlanejamentoHistoricos => Set<PlanejamentoHistorico>();
        public DbSet<PlanejamentoAnexo> PlanejamentoAnexos => Set<PlanejamentoAnexo>();

        // PRD-GOS — Ordens de Serviço (Ficha de Produção)
        public DbSet<FichaProducao> FichasProducao => Set<FichaProducao>();
        public DbSet<FichaProducaoHistorico> FichasProducaoHistorico => Set<FichaProducaoHistorico>();
        public DbSet<FichaProducaoAnexo> FichasProducaoAnexo => Set<FichaProducaoAnexo>();

        // PRD-EST — Estimativa
        public DbSet<Estimativa> Estimativas => Set<Estimativa>();
        public DbSet<EstimativaComponente> EstimativaComponentes => Set<EstimativaComponente>();
        public DbSet<EstimativaParametro> EstimativaParametros => Set<EstimativaParametro>();
        public DbSet<EstimativaHistorico> EstimativaHistoricos => Set<EstimativaHistorico>();
        public DbSet<EstimativaAnexo> EstimativaAnexos => Set<EstimativaAnexo>();

        // PRD-MES — Execução de Manufatura MES
        public DbSet<MesOrdem> MesOrdens => Set<MesOrdem>();
        public DbSet<MesOrdemItem> MesOrdemItens => Set<MesOrdemItem>();
        public DbSet<MesServico> MesServicos => Set<MesServico>();
        public DbSet<MesServicoEquipamento> MesServicoEquipamentos => Set<MesServicoEquipamento>();
        public DbSet<MesConsumoMaterial> MesConsumos => Set<MesConsumoMaterial>();
        public DbSet<MesMovimentoProducao> MesMovimentos => Set<MesMovimentoProducao>();
        public DbSet<MesParametro> MesParametros => Set<MesParametro>();
        public DbSet<MesHistorico> MesHistoricos => Set<MesHistorico>();
        public DbSet<MesAnexo> MesAnexos => Set<MesAnexo>();

        // PRD-MRP — MRP / Planejamento Integrado IBP
        public DbSet<MrpPlanejamento> MrpPlanejamentos => Set<MrpPlanejamento>();
        public DbSet<MrpPlanejamentoHistorico> MrpHistoricos => Set<MrpPlanejamentoHistorico>();
        public DbSet<MrpPlanejamentoAnexo> MrpAnexos => Set<MrpPlanejamentoAnexo>();

        // PRD-ESC — Escalonamento e Programação
        public DbSet<EscProgramacao> EscProgramacoes => Set<EscProgramacao>();
        public DbSet<EscOperacao> EscOperacoes => Set<EscOperacao>();
        public DbSet<EscParametro> EscParametros => Set<EscParametro>();
        public DbSet<EscHistorico> EscHistoricos => Set<EscHistorico>();
        public DbSet<EscAnexo> EscAnexos => Set<EscAnexo>();

        public ContextProducao(
            DbContextOptions<ContextProducao> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("producao");

            modelBuilder.Entity<ListaMateriais>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.ToTable("listas_materiais");
                entity.HasIndex(l => new { l.TenantId, l.ProdutoAcabadoSku });

                entity.HasMany(l => l.Itens)
                    .WithOne()
                    .HasForeignKey(i => i.ListaMateriaisId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BomItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.ToTable("bom_itens");
                entity.HasIndex(i => new { i.TenantId, i.InsumoSku });
            });

            modelBuilder.Entity<OrdemProducao>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("ordens_producao");
                entity.HasIndex(o => new { o.TenantId, o.Codigo }).IsUnique();
                entity.HasIndex(o => new { o.TenantId, o.ProdutoAcabadoSku });

                entity.HasMany(o => o.Apontamentos)
                    .WithOne()
                    .HasForeignKey(a => a.OrdemProducaoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ApontamentoProducao>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.ToTable("apontamentos");
                entity.HasIndex(a => new { a.TenantId, a.OrdemProducaoId });
            });

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.ToTable("outbox_messages", "producao");
            });

            // ===================== PRD-BOM =====================
            modelBuilder.Entity<BomEstrutura>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_bom_estrutura");
                entity.HasIndex(e => new { e.TenantId, e.ProdutoId, e.VariacaoId });
                entity.HasIndex(e => new { e.TenantId, e.Codigo });
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Codigo).HasMaxLength(50);
                entity.Property(e => e.Instrucoes).HasMaxLength(4000);
                entity.Property(e => e.TipoCustoProducao).HasMaxLength(50);
                entity.Property(e => e.Versao).HasMaxLength(50);
                entity.Property(e => e.MotivoRejeicao).HasMaxLength(1000);
                entity.HasMany(e => e.Componentes)
                    .WithOne()
                    .HasForeignKey(c => c.EstruturaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BomComponente>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_bom_componente");
                entity.HasIndex(e => new { e.TenantId, e.EstruturaId });
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            });

            modelBuilder.Entity<BomGrupoComponente>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_bom_grupo_componente");
                entity.HasIndex(e => new { e.TenantId });
                entity.Property(e => e.Nome).HasMaxLength(200);
                entity.Property(e => e.Descricao).HasMaxLength(1000);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            });

            modelBuilder.Entity<BomInstrucao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_bom_instrucao");
                entity.HasIndex(e => new { e.TenantId, e.Codigo }).IsUnique();
                entity.Property(e => e.Codigo).HasMaxLength(50);
                entity.Property(e => e.Descricao).HasMaxLength(1000);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            });

            modelBuilder.Entity<BomInstrucaoOrdem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_bom_instrucao_ordem");
                entity.HasIndex(e => new { e.TenantId, e.OrdemProducaoId });
                entity.HasIndex(e => new { e.TenantId, e.InstrucaoId });
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            });

            modelBuilder.Entity<BomHistorico>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_bom_historico");
                entity.HasIndex(e => new { e.TenantId, e.EstruturaId });
                entity.Property(e => e.Acao).HasMaxLength(100);
                entity.Property(e => e.UsuarioId).HasMaxLength(100);
                entity.Property(e => e.OrigemIp).HasMaxLength(60);
                entity.Property(e => e.StatusAnterior).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.StatusNovo).HasConversion<string>().HasMaxLength(20);
            });

            modelBuilder.Entity<BomAnexo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_bom_anexo");
                entity.HasIndex(e => new { e.TenantId, e.EstruturaId });
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.UsuarioId).HasMaxLength(100);
            });

            // ===================== PRD-CST =====================
            modelBuilder.Entity<CustoProducao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_cst_custo_producao");
                entity.HasIndex(e => new { e.TenantId, e.Codigo }).IsUnique();
                entity.Property(e => e.Codigo).HasMaxLength(50);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.ReferenciaOrigem).HasMaxLength(50);
                entity.Property(e => e.MotivoRejeicao).HasMaxLength(1000);
                entity.Property(e => e.CustoTotalPrevisto).HasPrecision(18, 6);
                entity.Property(e => e.CustoTotalRealizado).HasPrecision(18, 6);
                entity.Property(e => e.DesvioTotal).HasPrecision(18, 6);
                entity.HasMany(e => e.Referencias)
                    .WithOne()
                    .HasForeignKey(r => r.CustoProducaoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CustoReferencia>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_cst_custo_referencia");
                entity.HasIndex(e => new { e.TenantId, e.CustoProducaoId });
                entity.Property(e => e.TipoReferencia).HasMaxLength(50);
                entity.Property(e => e.TipoCustoProducao).HasMaxLength(50);
                entity.Property(e => e.CustoPrevisto).HasPrecision(18, 6);
                entity.Property(e => e.CustoRealizado).HasPrecision(18, 6);
                entity.Property(e => e.CustoExtra).HasPrecision(18, 6);
                entity.Property(e => e.PercentualCustoProducao).HasPrecision(9, 4);
                entity.Property(e => e.Desvio).HasPrecision(18, 6);
            });

            modelBuilder.Entity<CustoParametro>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_cst_parametro");
                entity.HasIndex(e => new { e.TenantId, e.Chave }).IsUnique();
                entity.Property(e => e.Chave).HasMaxLength(150);
                entity.Property(e => e.Valor).HasMaxLength(4000);
            });

            modelBuilder.Entity<CustoHistorico>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_cst_historico");
                entity.HasIndex(e => new { e.TenantId, e.CustoProducaoId });
                entity.Property(e => e.Acao).HasMaxLength(100);
                entity.Property(e => e.UsuarioId).HasMaxLength(100);
                entity.Property(e => e.IpOrigem).HasMaxLength(60);
            });

            modelBuilder.Entity<CustoAnexo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_cst_anexo");
                entity.HasIndex(e => new { e.TenantId, e.CustoProducaoId });
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.UsuarioId).HasMaxLength(100);
            });

            // ===================== PRD-PLN =====================
            modelBuilder.Entity<PlanejamentoProducao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_pln_planejamento");
                entity.HasIndex(e => new { e.TenantId, e.Codigo }).IsUnique();
                entity.Property(e => e.Codigo).HasMaxLength(50);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.MotivoRejeicao).HasMaxLength(1000);
                entity.HasMany(e => e.Snapshots)
                    .WithOne()
                    .HasForeignKey(s => s.PlanejamentoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PlanejamentoSnapshotOp>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_pln_snapshot_op");
                entity.HasIndex(e => new { e.TenantId, e.PlanejamentoId });
                entity.Property(e => e.PorcentoVenda).HasPrecision(9, 4);
                entity.Property(e => e.PorcentoEstoque).HasPrecision(9, 4);
                entity.Property(e => e.CustoTotalPrevisto).HasPrecision(18, 6);
            });

            modelBuilder.Entity<PlanejamentoHistorico>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_pln_planejamento_historico");
                entity.HasIndex(e => new { e.TenantId, e.PlanejamentoId });
                entity.Property(e => e.Acao).HasMaxLength(100);
                entity.Property(e => e.UsuarioId).HasMaxLength(100);
                entity.Property(e => e.Ip).HasMaxLength(60);
            });

            modelBuilder.Entity<PlanejamentoAnexo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_pln_planejamento_anexo");
                entity.HasIndex(e => new { e.TenantId, e.PlanejamentoId });
                entity.Property(e => e.UsuarioId).HasMaxLength(100);
            });

            // ===================== PRD-GOS =====================
            modelBuilder.Entity<FichaProducao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_gos_ficha_producao");
                entity.HasIndex(e => new { e.TenantId, e.VendaId });
                entity.HasIndex(e => new { e.TenantId, e.PessoaId });
                entity.HasIndex(e => new { e.TenantId, e.Situacao });
                entity.Property(e => e.Situacao).HasConversion<int>();
                entity.Property(e => e.Logomarca).HasConversion<int>();
                entity.Property(e => e.Transportadora).HasMaxLength(200);
                entity.Property(e => e.AnoModelo).HasMaxLength(50);
                entity.Property(e => e.CorCouro).HasMaxLength(100);
                entity.Property(e => e.Costura).HasMaxLength(100);
                entity.Property(e => e.TipoAcento).HasMaxLength(100);
                entity.Property(e => e.TipoEncosto).HasMaxLength(100);
                entity.Property(e => e.Abd).HasMaxLength(100);
                entity.Property(e => e.Abt).HasMaxLength(100);
                entity.Property(e => e.Observacao).HasMaxLength(2000);
            });

            modelBuilder.Entity<FichaProducaoHistorico>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_gos_ficha_historico");
                entity.HasIndex(e => new { e.TenantId, e.FichaProducaoId });
                entity.Property(e => e.Acao).HasMaxLength(100);
                entity.Property(e => e.UsuarioId).HasMaxLength(100);
                entity.Property(e => e.Ip).HasMaxLength(60);
                entity.Property(e => e.Conteudo).HasMaxLength(4000);
                entity.Property(e => e.Motivo).HasMaxLength(1000);
            });

            modelBuilder.Entity<FichaProducaoAnexo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_gos_ficha_anexo");
                entity.HasIndex(e => new { e.TenantId, e.FichaProducaoId });
                entity.Property(e => e.Tipo).HasMaxLength(100);
                entity.Property(e => e.UsuarioId).HasMaxLength(100);
            });

            // ===================== PRD-EST =====================
            modelBuilder.Entity<Estimativa>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_est_estimativa");
                entity.HasIndex(e => new { e.TenantId, e.Codigo }).IsUnique();
                entity.Property(e => e.Codigo).HasMaxLength(50);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.MotivoRejeicao).HasMaxLength(1000);
                entity.Property(e => e.CustoPrevistoTotal).HasPrecision(18, 6);
                entity.HasMany(e => e.Componentes)
                    .WithOne()
                    .HasForeignKey(c => c.EstimativaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<EstimativaComponente>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_est_componente");
                entity.HasIndex(e => new { e.TenantId, e.EstimativaId });
                entity.Property(e => e.TipoComponente).HasMaxLength(50);
                entity.Property(e => e.Quantidade).HasPrecision(18, 6);
                entity.Property(e => e.TempoEstimado).HasPrecision(18, 6);
                entity.Property(e => e.TaxaEstimada).HasPrecision(18, 6);
                entity.Property(e => e.CustoPrevisto).HasPrecision(18, 6);
                entity.Property(e => e.Observacao).HasMaxLength(1000);
            });

            modelBuilder.Entity<EstimativaParametro>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_est_parametro");
                entity.HasIndex(e => new { e.TenantId, e.Chave }).IsUnique();
                entity.Property(e => e.Chave).HasMaxLength(150);
                entity.Property(e => e.Valor).HasMaxLength(4000);
            });

            modelBuilder.Entity<EstimativaHistorico>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_est_historico");
                entity.HasIndex(e => new { e.TenantId, e.EstimativaId });
                entity.Property(e => e.Acao).HasMaxLength(100);
                entity.Property(e => e.UsuarioId).HasMaxLength(100);
                entity.Property(e => e.IpOrigem).HasMaxLength(60);
            });

            modelBuilder.Entity<EstimativaAnexo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_est_anexo");
                entity.HasIndex(e => new { e.TenantId, e.EstimativaId });
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.UsuarioId).HasMaxLength(100);
            });

            // ===================== PRD-MES =====================
            modelBuilder.Entity<MesOrdem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_mes_ordem");
                entity.HasIndex(e => new { e.TenantId, e.Referencia });
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasIndex(e => new { e.TenantId, e.EmpresaId });
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Referencia).HasMaxLength(80);
                entity.Property(e => e.TipoCustoProducao).HasMaxLength(50);
                entity.Property(e => e.Lote).HasMaxLength(100);
                entity.Property(e => e.MotivoRejeicao).HasMaxLength(1000);
                entity.Property(e => e.CustoTotalPrevisto).HasPrecision(18, 6);
                entity.Property(e => e.CustoTotalRealizado).HasPrecision(18, 6);
                entity.Property(e => e.ValorTotalFinal).HasPrecision(18, 6);
                entity.Property(e => e.CustoProducao).HasPrecision(18, 6);
                entity.Property(e => e.DesperdicioUnidades).HasPrecision(18, 6);
                entity.Property(e => e.PercentualVenda).HasPrecision(9, 4);
                entity.Property(e => e.PercentualEstoque).HasPrecision(9, 4);
                entity.HasMany(e => e.Itens)
                    .WithOne()
                    .HasForeignKey(i => i.OrdemId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MesOrdemItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_mes_ordem_item");
                entity.HasIndex(e => new { e.TenantId, e.OrdemId });
                entity.HasIndex(e => new { e.TenantId, e.ProdutoId });
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.QuantidadeProduzir).HasPrecision(18, 6);
                entity.Property(e => e.QuantidadeProduzida).HasPrecision(18, 6);
                entity.Property(e => e.QuantidadeEntregue).HasPrecision(18, 6);
                entity.Property(e => e.CustoPrevisto).HasPrecision(18, 6);
                entity.Property(e => e.CustoRealizado).HasPrecision(18, 6);
            });

            modelBuilder.Entity<MesServico>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_mes_servico");
                entity.HasIndex(e => new { e.TenantId, e.ItemOrdemId });
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.CustoPrevisto).HasPrecision(18, 6);
                entity.Property(e => e.CustoRealizado).HasPrecision(18, 6);
            });

            modelBuilder.Entity<MesServicoEquipamento>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_mes_servico_equipamento");
                entity.HasIndex(e => new { e.TenantId, e.ServicoId });
                entity.HasIndex(e => new { e.TenantId, e.EquipamentoId });
            });

            modelBuilder.Entity<MesConsumoMaterial>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_mes_consumo_material");
                entity.HasIndex(e => new { e.TenantId, e.OrdemId });
                entity.HasIndex(e => new { e.TenantId, e.EstruturaId });
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.QuantidadePrevista).HasPrecision(18, 6);
                entity.Property(e => e.QuantidadeConsumida).HasPrecision(18, 6);
                entity.Property(e => e.PercentualDesperdicio).HasPrecision(9, 4);
                entity.Property(e => e.CustoConsumo).HasPrecision(18, 6);
            });

            modelBuilder.Entity<MesMovimentoProducao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_mes_movimento_producao");
                entity.HasIndex(e => new { e.TenantId, e.OrdemId });
                entity.HasIndex(e => new { e.TenantId, e.MovimentoPaiId });
                entity.Property(e => e.TipoMovimento).HasConversion<string>().HasMaxLength(30);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Quantidade).HasPrecision(18, 6);
                entity.Property(e => e.ValorUnitario).HasPrecision(18, 6);
                entity.Property(e => e.ValorTotal).HasPrecision(18, 6);
            });

            modelBuilder.Entity<MesParametro>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_mes_parametro");
                entity.HasIndex(e => new { e.TenantId });
                entity.Property(e => e.PrefixoReferencia).HasMaxLength(50);
                entity.Property(e => e.VersaoParametro).HasMaxLength(50);
            });

            modelBuilder.Entity<MesHistorico>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_mes_historico");
                entity.HasIndex(e => new { e.TenantId, e.OrdemId });
                entity.Property(e => e.Acao).HasMaxLength(100);
                entity.Property(e => e.UsuarioId).HasMaxLength(100);
                entity.Property(e => e.OrigemIp).HasMaxLength(60);
                entity.Property(e => e.StatusAnterior).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.StatusNovo).HasConversion<string>().HasMaxLength(20);
            });

            modelBuilder.Entity<MesAnexo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_mes_anexo");
                entity.HasIndex(e => new { e.TenantId, e.OrdemId });
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.UsuarioId).HasMaxLength(100);
            });

            // ===================== PRD-MRP =====================
            modelBuilder.Entity<MrpPlanejamento>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_mrp_planejamento");
                entity.HasIndex(e => new { e.TenantId, e.Codigo }).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.Property(e => e.Codigo).HasMaxLength(50);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.MotivoRejeicao).HasMaxLength(1000);
            });

            modelBuilder.Entity<MrpPlanejamentoHistorico>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_mrp_planejamento_historico");
                entity.HasIndex(e => new { e.TenantId, e.PlanejamentoId });
                entity.Property(e => e.Acao).HasMaxLength(100);
                entity.Property(e => e.UsuarioId).HasMaxLength(100);
                entity.Property(e => e.Ip).HasMaxLength(60);
                entity.Property(e => e.StatusAnterior).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.StatusNovo).HasConversion<string>().HasMaxLength(20);
            });

            modelBuilder.Entity<MrpPlanejamentoAnexo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_mrp_planejamento_anexo");
                entity.HasIndex(e => new { e.TenantId, e.PlanejamentoId });
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.UsuarioId).HasMaxLength(100);
            });

            // ===================== PRD-ESC =====================
            modelBuilder.Entity<EscProgramacao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_esc_programacao");
                entity.HasIndex(e => new { e.TenantId, e.Codigo }).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasIndex(e => new { e.TenantId, e.CentroTrabalhoId });
                entity.Property(e => e.Codigo).HasMaxLength(50);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.MotivoRejeicao).HasMaxLength(1000);
                entity.HasMany(e => e.Operacoes)
                    .WithOne()
                    .HasForeignKey(o => o.ProgramacaoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<EscOperacao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_esc_operacao");
                entity.HasIndex(e => new { e.TenantId, e.ProgramacaoId });
                entity.Property(e => e.CustoPrevisto).HasPrecision(18, 6);
                entity.Property(e => e.CustoRealizado).HasPrecision(18, 6);
            });

            modelBuilder.Entity<EscParametro>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_esc_parametro");
                entity.HasIndex(e => new { e.TenantId, e.Chave }).IsUnique();
                entity.Property(e => e.Chave).HasMaxLength(150);
                entity.Property(e => e.Valor).HasMaxLength(4000);
            });

            modelBuilder.Entity<EscHistorico>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_esc_historico");
                entity.HasIndex(e => new { e.TenantId, e.ProgramacaoId });
                entity.Property(e => e.Acao).HasMaxLength(100);
                entity.Property(e => e.UsuarioId).HasMaxLength(100);
                entity.Property(e => e.IpOrigem).HasMaxLength(60);
                entity.Property(e => e.StatusAnterior).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.StatusNovo).HasConversion<string>().HasMaxLength(20);
            });

            modelBuilder.Entity<EscAnexo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("prd_esc_anexo");
                entity.HasIndex(e => new { e.TenantId, e.ProgramacaoId });
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.UsuarioId).HasMaxLength(100);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
