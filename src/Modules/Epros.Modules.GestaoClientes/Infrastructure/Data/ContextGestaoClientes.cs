using Epros.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Infrastructure.Data
{
    public class ContextGestaoClientes : ContextBase
    {
        public DbSet<Plano> Planos => Set<Plano>();
        public DbSet<ModuloPlano> ModulosPlano => Set<ModuloPlano>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Fatura> Faturas => Set<Fatura>();
        public DbSet<FaturaItem> FaturaItens => Set<FaturaItem>();
        public DbSet<GrupoPlano> GrupoPlanos => Set<GrupoPlano>();
        public DbSet<AssinaturaCliente> AssinaturasClientes => Set<AssinaturaCliente>();
        public DbSet<PagamentoFatura> PagamentosFaturas => Set<PagamentoFatura>();
        public DbSet<ReciboPagamento> RecibosPagamento => Set<ReciboPagamento>();
        public DbSet<MeioPagamentoCliente> MeiosPagamentoClientes => Set<MeioPagamentoCliente>();
        public DbSet<ConfiguracaoGatewayPagamento> ConfiguracoesGatewayPagamento => Set<ConfiguracaoGatewayPagamento>();
        public DbSet<ComposicaoFaturamento> ComposicoesFaturamento => Set<ComposicaoFaturamento>();
        public DbSet<HistoricoReajuste> HistoricosReajustes => Set<HistoricoReajuste>();

        // Novas tabelas do submódulo Pedidos e Cobrança SaaS (APP-TEN-006)
        public DbSet<Cupom> Cupons => Set<Cupom>();
        public DbSet<PedidoSaaS> PedidosSaaS => Set<PedidoSaaS>();
        public DbSet<UsoCupom> UsosCupons => Set<UsoCupom>();
        public DbSet<PagamentoTransferencia> PagamentosTransferencias => Set<PagamentoTransferencia>();
        public DbSet<ComprovantePagamento> ComprovantesPagamentos => Set<ComprovantePagamento>();
        public DbSet<SessaoPagamento> SessoesPagamentos => Set<SessaoPagamento>();
        public DbSet<PagamentoGlobal> PagamentosGlobais => Set<PagamentoGlobal>();
        public DbSet<Revenda> Revendas => Set<Revenda>();
        public DbSet<Vendedor> Vendedores => Set<Vendedor>();
        public DbSet<Pessoa> Pessoas => Set<Pessoa>();
        public DbSet<PessoaFisica> PessoasFisicas => Set<PessoaFisica>();
        public DbSet<PessoaJuridica> PessoasJuridicas => Set<PessoaJuridica>();
        public DbSet<PessoaEstrangeiro> PessoasEstrangeiros => Set<PessoaEstrangeiro>();
        public DbSet<PessoaCliente> PessoasClientes => Set<PessoaCliente>();
        public DbSet<PessoaFuncionario> PessoasFuncionarios => Set<PessoaFuncionario>();
        public DbSet<PessoaMotorista> PessoasMotoristas => Set<PessoaMotorista>();
        public DbSet<PessoaTransportadora> PessoasTransportadoras => Set<PessoaTransportadora>();
        public DbSet<PessoaPrestadorServico> PessoasPrestadoresServico => Set<PessoaPrestadorServico>();
        public DbSet<Endereco> EnderecosPessoas => Set<Endereco>();
        public DbSet<PessoaContato> PessoasContatos => Set<PessoaContato>();
        public DbSet<PessoaVeiculo> PessoasVeiculos => Set<PessoaVeiculo>();
        public DbSet<PessoaGrupo> PessoaGrupos => Set<PessoaGrupo>();
        public DbSet<Empresa> Empresas => Set<Empresa>();
        public DbSet<EmpresaCertificado> EmpresasCertificados => Set<EmpresaCertificado>();

        // Lookups cross-module (schema financas) — somente leitura, para validação de exclusão de Pessoa.
        public DbSet<ContaAPagarLookup> ContasAPagarLookup => Set<ContaAPagarLookup>();
        public DbSet<ContaAReceberLookup> ContasAReceberLookup => Set<ContaAReceberLookup>();
        public DbSet<EmpresaParametrosDfe> EmpresasParametrosDfe => Set<EmpresaParametrosDfe>();
        public DbSet<EmpresaContato> EmpresasContatos => Set<EmpresaContato>();
        public DbSet<IeSt> IeSts => Set<IeSt>();
        public DbSet<PerfilColaborador> PerfisColaboradores => Set<PerfilColaborador>();
        // Alias de compatibilidade: PerfilUsuario foi renomeado para PerfilColaborador (RBAC canônico = PerfilAcesso).
        // Mantido para não quebrar consumidores externos até a reconciliação do lead.
        public DbSet<PerfilColaborador> PerfisUsuarios => Set<PerfilColaborador>();
        public DbSet<UsuarioPermissao> UsuariosPermissoes => Set<UsuarioPermissao>();
        public DbSet<Menu> Menus => Set<Menu>();
        public DbSet<MenuItemNivel1> MenusItensNivel1 => Set<MenuItemNivel1>();
        public DbSet<MenuItemNivel2> MenusItensNivel2 => Set<MenuItemNivel2>();
        public DbSet<PerfilAcesso> PerfisAcessos => Set<PerfilAcesso>();
        public DbSet<PerfilAcessoMenu> PerfisAcessosMenus => Set<PerfilAcessoMenu>();
        public DbSet<Contrato> Contratos => Set<Contrato>();
        public DbSet<ContratoItem> ContratoItens => Set<ContratoItem>();
        public DbSet<ConfiguracaoGlobal> ConfiguracoesGlobais => Set<ConfiguracaoGlobal>();
        public DbSet<ExecucaoMassa> ExecucoesMassa => Set<ExecucaoMassa>();
        public DbSet<Epros.Shared.Domain.Events.OutboxMessage> OutboxMessages => Set<Epros.Shared.Domain.Events.OutboxMessage>();

        // CAD-PEM: extensões de papel adicionais (Fornecedor/Comprador/Contador/Vendedor)
        public DbSet<PessoaFornecedor> PessoasFornecedores => Set<PessoaFornecedor>();
        public DbSet<PessoaComprador> PessoasCompradores => Set<PessoaComprador>();
        public DbSet<PessoaContador> PessoasContadores => Set<PessoaContador>();
        public DbSet<PessoaVendedor> PessoasVendedores => Set<PessoaVendedor>();

        // CAD-PEM: governança (identidade fiscal, relacionamento, deduplicação, privacidade, auditoria, importação)
        public DbSet<IdentificadorFiscal> IdentificadoresFiscais => Set<IdentificadorFiscal>();
        public DbSet<RelacionamentoParceiro> RelacionamentosParceiro => Set<RelacionamentoParceiro>();
        public DbSet<RegraDeduplicacao> RegrasDeduplicacao => Set<RegraDeduplicacao>();
        public DbSet<CandidatoDuplicata> CandidatosDuplicata => Set<CandidatoDuplicata>();
        public DbSet<ConsentimentoTitular> ConsentimentosTitular => Set<ConsentimentoTitular>();
        public DbSet<SolicitacaoTitular> SolicitacoesTitular => Set<SolicitacaoTitular>();
        public DbSet<PessoaHistoricoEstado> PessoasHistoricoEstado => Set<PessoaHistoricoEstado>();
        public DbSet<PessoaLogAuditoria> PessoasLogAuditoria => Set<PessoaLogAuditoria>();
        public DbSet<PessoaImportacaoLote> PessoasImportacaoLote => Set<PessoaImportacaoLote>();
        public DbSet<PessoaImportacaoLinha> PessoasImportacaoLinha => Set<PessoaImportacaoLinha>();
        public DbSet<EmpresaGrupo> EmpresaGrupos => Set<EmpresaGrupo>();

        // APP-TEN-003: RBAC estendido (papel/capacidade/usuario_papel/nivel_usuario/preco_nivel_usuario)
        public DbSet<Papel> Papeis => Set<Papel>();
        public DbSet<Capacidade> Capacidades => Set<Capacidade>();
        public DbSet<PapelCapacidade> PapeisCapacidades => Set<PapelCapacidade>();
        public DbSet<UsuarioPapel> UsuariosPapeis => Set<UsuarioPapel>();
        public DbSet<NivelUsuario> NiveisUsuario => Set<NivelUsuario>();
        public DbSet<PrecoNivelUsuario> PrecosNivelUsuario => Set<PrecoNivelUsuario>();

        // APP-TEN-003: auditoria e segurança de usuário (grant/deny direto, histórico de login, impersonação)
        public DbSet<UsuarioCapacidade> UsuariosCapacidades => Set<UsuarioCapacidade>();

        // APP-CAT: catálogos globais SaaS (funcionalidade/add-on) + módulos ativos por usuário
        public DbSet<Funcionalidade> Funcionalidades => Set<Funcionalidade>();
        public DbSet<AddOn> AddOns => Set<AddOn>();
        public DbSet<ModuloAtivoUsuario> ModulosAtivosUsuario => Set<ModuloAtivoUsuario>();

        // Geografia e Localização
        public DbSet<Pais> Paises => Set<Pais>();
        public DbSet<Subdivisao> Subdivisoes => Set<Subdivisao>();
        public DbSet<Municipio> Municipios => Set<Municipio>();
        public DbSet<FormatoCodigoPostal> FormatosCodigoPostal => Set<FormatoCodigoPostal>();
        public DbSet<CodigoPostalCache> CodigosPostaisCache => Set<CodigoPostalCache>();
        public DbSet<ZonaEntrega> ZonasEntrega => Set<ZonaEntrega>();
        public DbSet<SincronizacaoGeografica> SincronizacoesGeograficas => Set<SincronizacaoGeografica>();

        // Parâmetros Operacionais
        public DbSet<FusoHorario> FusosHorarios => Set<FusoHorario>();
        public DbSet<Moeda> Moedas => Set<Moeda>();
        public DbSet<UpgradePlano> UpgradesPlanos => Set<UpgradePlano>();

        // 1.06 — Idempotência do webhook de pagamento (dedupe por id de evento/pagamento).
        public DbSet<WebhookEventoProcessado> WebhookEventosProcessados => Set<WebhookEventoProcessado>();
        public DbSet<ExercicioFinanceiro> ExerciciosFinanceiros => Set<ExercicioFinanceiro>();
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<UnidadeMedida> UnidadesMedida => Set<UnidadeMedida>();
        public DbSet<Armazem> Armazens => Set<Armazem>();
        public DbSet<Projeto> Projetos => Set<Projeto>();
        public DbSet<PreferenciaGeral> PreferenciasGerais => Set<PreferenciaGeral>();
        public DbSet<ConfiguracaoEmail> ConfiguracoesEmail => Set<ConfiguracaoEmail>();
        public DbSet<Imposto> Impostos => Set<Imposto>();
        public DbSet<ConversaoUnidade> ConversoesUnidades => Set<ConversaoUnidade>();
        public DbSet<LogAuditoriaConfiguracao> LogsAuditoriaConfiguracao => Set<LogAuditoriaConfiguracao>();

        public ContextGestaoClientes(
            DbContextOptions<ContextGestaoClientes> options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options, tenantProvider, currentUser)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define o schema do banco do PostgreSQL para o macrodomínio da plataforma
            modelBuilder.HasDefaultSchema("plataforma");

            modelBuilder.Entity<Epros.Shared.Domain.Events.OutboxMessage>(entity =>
            {
                entity.ToTable("outbox_messages", "plataforma");
                entity.HasKey(o => o.Id);
            });

            // Configurações específicas das entidades do módulo
            modelBuilder.Entity<Plano>(entity =>
            {
                entity.HasKey(p => p.Id);
                // 1.01 — Duration persistida como texto (vitalicia/mensal/anual).
                entity.Property(p => p.Duration).HasConversion<string>().HasMaxLength(20);
                entity.HasMany(p => p.Modulos)
                      .WithOne()
                      .HasForeignKey(m => m.PlanoId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<GrupoPlano>()
                      .WithMany()
                      .HasForeignKey(p => p.GrupoPlanoId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ModuloPlano>(entity =>
            {
                entity.HasKey(m => m.Id);
            });

            // 1.06 — Idempotência de webhook: índice único por (provedor, evento_id) garante que o
            // mesmo pagamento não seja processado duas vezes mesmo sob concorrência/reentrega.
            modelBuilder.Entity<WebhookEventoProcessado>(entity =>
            {
                entity.HasKey(w => w.Id);
                entity.Property(w => w.Provedor).HasMaxLength(50);
                entity.Property(w => w.EventoId).HasMaxLength(200);
                entity.HasIndex(w => new { w.Provedor, w.EventoId })
                      .IsUnique()
                      .HasDatabaseName("ux__webhook_evento_provedor_evento_id");
            });

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(c => c.Id);
                // 1.01 — StatusSaaS como enum tipado, persistido como texto (coluna "status_saa_s" inalterada).
                entity.Property(c => c.StatusSaaS).HasConversion<string>();
                entity.HasOne<Revenda>()
                      .WithMany()
                      .HasForeignKey(c => c.RevendaId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne<Vendedor>()
                      .WithMany()
                      .HasForeignKey(c => c.VendedorId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Fatura>(entity =>
            {
                entity.HasKey(f => f.Id);
                // Status como enum tipado, persistido como texto (coluna varchar inalterada).
                entity.Property(f => f.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(f => f.Numero).HasMaxLength(50);
                // Itens/composição da fatura emitida (1.01 / EF 11.8).
                entity.HasMany(f => f.Itens)
                      .WithOne()
                      .HasForeignKey(i => i.FaturaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FaturaItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Descricao).HasMaxLength(200);
            });

            modelBuilder.Entity<GrupoPlano>(entity =>
            {
                entity.HasKey(g => g.Id);
                // Coluna real de ativação (snake_case "ativo" aplicado pela convenção global).
                entity.Property(g => g.Ativo).HasDefaultValue(true);
            });

            modelBuilder.Entity<AssinaturaCliente>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Status).HasConversion<string>();
                entity.HasOne<Cliente>()
                      .WithMany()
                      .HasForeignKey(a => a.ClienteId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<Plano>()
                      .WithMany()
                      .HasForeignKey(a => a.PlanoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PagamentoFatura>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
                // Obs.: a precisão 18,3 de ValorTarifa é aplicada APÓS base.OnModelCreating (a convenção
                // global reescreveria 18,2 aqui). Ver bloco no fim deste método.
                entity.HasOne<Fatura>()
                      .WithMany()
                      .HasForeignKey(p => p.FaturaId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(p => p.IdentificadorPagamento)
                      .IsUnique()
                      .HasDatabaseName("ix_pagamentos_fatura_payment_id")
                      .HasFilter("identificador_pagamento IS NOT NULL");
                // Dados da cobrança PIX (payloads podem ser longos → text).
                entity.Property(p => p.QrCode).HasColumnType("text");
                entity.Property(p => p.QrCodeBase64).HasColumnType("text");
                // 1.08B — dados de boleto (linha digitável / código de barras / URL do PDF).
                entity.Property(p => p.LinhaDigitavel).HasMaxLength(100);
                entity.Property(p => p.CodigoBarras).HasMaxLength(100);
                entity.Property(p => p.UrlBoleto).HasColumnType("text");
            });

            modelBuilder.Entity<ConfiguracaoGatewayPagamento>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Provedor).HasConversion<string>().HasMaxLength(30);
                entity.Property(g => g.Ambiente).HasConversion<string>().HasMaxLength(20);
                entity.Property(g => g.AccessToken).HasColumnType("text");
                entity.Property(g => g.PublicKey).HasColumnType("text");
                entity.Property(g => g.WebhookSecret).HasColumnType("text");
                entity.Property(g => g.NotificationUrl).HasMaxLength(500);
                entity.Property(g => g.Moeda).HasMaxLength(3);
                entity.Property(g => g.TenantAlvo).HasMaxLength(100);
                entity.HasIndex(g => new { g.TenantAlvo, g.Provedor, g.Ativo })
                      .HasDatabaseName("ix_config_gateway_pagamento_tenant_provedor_ativo");
            });

            // 1.08A — Recibo de pagamento (documento simples; NFS-e é diferida). Número único por recibo.
            modelBuilder.Entity<ReciboPagamento>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Numero).HasMaxLength(40);
                entity.Property(r => r.MeioPagamento).HasMaxLength(30);
                entity.Property(r => r.PagadorNome).HasMaxLength(200);
                entity.Property(r => r.PagadorDocumento).HasMaxLength(30);
                entity.HasIndex(r => r.Numero).IsUnique().HasDatabaseName("ux_recibos_pagamento_numero");
                entity.HasIndex(r => r.FaturaId).HasDatabaseName("ix_recibos_pagamento_fatura");
                entity.HasOne<Fatura>()
                      .WithMany()
                      .HasForeignKey(r => r.FaturaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 1.08B — Meio de pagamento salvo do cliente (cartão-on-file tokenizado). Só metadados +
            // identificadores opacos do gateway (PCI: nunca PAN/CVV).
            modelBuilder.Entity<MeioPagamentoCliente>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Tipo).HasMaxLength(20);
                entity.Property(m => m.Bandeira).HasMaxLength(30);
                entity.Property(m => m.UltimosQuatro).HasMaxLength(4);
                entity.Property(m => m.CustomerIdGateway).HasMaxLength(100);
                entity.Property(m => m.CardIdGateway).HasMaxLength(100);
                entity.HasIndex(m => new { m.ClienteId, m.Ativo }).HasDatabaseName("ix_meios_pagamento_cliente_cliente_ativo");
                entity.HasOne<Cliente>()
                      .WithMany()
                      .HasForeignKey(m => m.ClienteId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Cupom>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.Codigo).IsUnique().HasDatabaseName("ix_cupons_codigo");
            });

            modelBuilder.Entity<PedidoSaaS>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(p => p.ValorBase).HasPrecision(18, 2);
                entity.Property(p => p.ValorDesconto).HasPrecision(18, 2);
                entity.Property(p => p.ValorTotal).HasPrecision(18, 2);
                entity.HasOne<Cliente>()
                      .WithMany()
                      .HasForeignKey(p => p.ClienteId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<Plano>()
                      .WithMany()
                      .HasForeignKey(p => p.PlanoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<Cupom>()
                      .WithMany()
                      .HasForeignKey(p => p.CupomId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<UsoCupom>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => new { u.ClienteId, u.CupomId, u.PedidoId })
                      .IsUnique()
                      .HasDatabaseName("ix_usos_cupons_usuario_cupom_pedido");
                entity.HasOne<Cliente>()
                      .WithMany()
                      .HasForeignKey(u => u.ClienteId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<Cupom>()
                      .WithMany()
                      .HasForeignKey(u => u.CupomId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<PedidoSaaS>()
                      .WithMany()
                      .HasForeignKey(u => u.PedidoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PagamentoTransferencia>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(p => p.Valor).HasPrecision(18, 2);
                entity.HasOne<Fatura>()
                      .WithMany()
                      .HasForeignKey(p => p.FaturaId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne<PedidoSaaS>()
                      .WithMany()
                      .HasForeignKey(p => p.PedidoId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ComprovantePagamento>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Valor).HasPrecision(18, 2);
                entity.HasOne<PagamentoTransferencia>()
                      .WithMany()
                      .HasForeignKey(c => c.PagamentoTransferenciaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SessaoPagamento>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
                entity.HasOne<AssinaturaCliente>()
                      .WithMany()
                      .HasForeignKey(s => s.AssinaturaId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<PedidoSaaS>()
                      .WithMany()
                      .HasForeignKey(s => s.PedidoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PagamentoGlobal>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Valor).HasPrecision(18, 2);
                entity.HasOne<AssinaturaCliente>()
                      .WithMany()
                      .HasForeignKey(p => p.AssinaturaId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<PedidoSaaS>()
                      .WithMany()
                      .HasForeignKey(p => p.PedidoId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne<Fatura>()
                      .WithMany()
                      .HasForeignKey(p => p.FaturaId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ComposicaoFaturamento>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasOne<Cliente>()
                      .WithMany()
                      .HasForeignKey(c => c.ClienteId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<HistoricoReajuste>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.HasOne<ComposicaoFaturamento>()
                      .WithMany()
                      .HasForeignKey(h => h.ComposicaoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Revenda>(entity =>
            {
                entity.HasKey(r => r.Id);
            });

            modelBuilder.Entity<Vendedor>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.HasOne<Revenda>()
                      .WithMany()
                      .HasForeignKey(v => v.RevendaId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Pessoa>(entity =>
            {
                entity.HasKey(p => p.Id);

                // 1:1 relations
                entity.HasOne(p => p.PessoaFisica)
                      .WithOne()
                      .HasForeignKey<PessoaFisica>(pf => pf.PessoaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.PessoaJuridica)
                      .WithOne()
                      .HasForeignKey<PessoaJuridica>(pj => pj.PessoaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.PessoaEstrangeiro)
                      .WithOne()
                      .HasForeignKey<PessoaEstrangeiro>(pe => pe.PessoaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.PessoaCliente)
                      .WithOne()
                      .HasForeignKey<PessoaCliente>(pc => pc.PessoaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.PessoaFuncionario)
                      .WithOne()
                      .HasForeignKey<PessoaFuncionario>(pf => pf.PessoaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.PessoaMotorista)
                      .WithOne()
                      .HasForeignKey<PessoaMotorista>(pm => pm.PessoaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.PessoaTransportadora)
                      .WithOne()
                      .HasForeignKey<PessoaTransportadora>(pt => pt.PessoaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.PessoaPrestadorServico)
                      .WithOne()
                      .HasForeignKey<PessoaPrestadorServico>(pp => pp.PessoaId)
                      .OnDelete(DeleteBehavior.Cascade);

                // 1:N relations
                entity.HasMany(p => p.Enderecos)
                      .WithOne()
                      .HasForeignKey(e => e.PessoaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Contatos)
                      .WithOne()
                      .HasForeignKey(c => c.PessoaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Veiculos)
                      .WithOne()
                      .HasForeignKey(v => v.PessoaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PessoaFisica>(entity =>
            {
                entity.HasKey(pf => pf.PessoaId);
                entity.OwnsOne(pf => pf.Cpf, cpf =>
                {
                    cpf.Property(c => c.Valor).HasColumnName("cpf");
                });
            });

            modelBuilder.Entity<PessoaJuridica>(entity =>
            {
                entity.HasKey(pj => pj.PessoaId);
                entity.OwnsOne(pj => pj.Cnpj, cnpj =>
                {
                    cnpj.Property(c => c.Valor).HasColumnName("cnpj");
                });
            });

            modelBuilder.Entity<PessoaEstrangeiro>(entity =>
            {
                entity.HasKey(pe => pe.PessoaId);
            });

            modelBuilder.Entity<PessoaCliente>(entity =>
            {
                entity.HasKey(pc => pc.PessoaId);
            });

            modelBuilder.Entity<PessoaFuncionario>(entity =>
            {
                entity.HasKey(pf => pf.PessoaId);
            });

            modelBuilder.Entity<PessoaMotorista>(entity =>
            {
                entity.HasKey(pm => pm.PessoaId);
            });

            modelBuilder.Entity<PessoaTransportadora>(entity =>
            {
                entity.HasKey(pt => pt.PessoaId);
            });

            modelBuilder.Entity<PessoaPrestadorServico>(entity =>
            {
                entity.HasKey(pp => pp.PessoaId);
            });

            modelBuilder.Entity<Endereco>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Pais)
                      .WithMany()
                      .HasForeignKey(e => e.PaisId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Municipio)
                      .WithMany()
                      .HasForeignKey(e => e.MunicipioId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Subdivisao)
                      .WithMany()
                      .HasForeignKey(e => e.SubdivisaoId)
                      .OnDelete(DeleteBehavior.Restrict);
                // Dimensão Empresa/Contador do vínculo de endereço (cross-module: FK por Guid, sem navegação).
                entity.HasIndex(e => new { e.TenantId, e.EmpresaId }).HasDatabaseName("ix_enderecos_tenant_empresa");
            });

            modelBuilder.Entity<PessoaContato>(entity =>
            {
                entity.HasKey(c => c.Id);
            });

            modelBuilder.Entity<PessoaVeiculo>(entity =>
            {
                entity.HasKey(v => v.Id);
                // D4: PaisId agora é Guid (era long) — FK consistente ao catálogo global Pais.
                entity.HasOne<Pais>()
                      .WithMany()
                      .HasForeignKey(v => v.PaisId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PessoaGrupo>(entity =>
            {
                entity.HasKey(g => g.Id);
            });

            modelBuilder.Entity<Empresa>(entity =>
            {
                entity.HasKey(e => e.Id);
                // Configura o endereço como Owned Type embutido na tabela empresas
                entity.OwnsOne(e => e.Endereco, end =>
                {
                    end.Property(p => p.Logradouro).HasColumnName("logradouro");
                    end.Property(p => p.Numero).HasColumnName("numero");
                    end.Property(p => p.Complemento).HasColumnName("complemento");
                    end.Property(p => p.Bairro).HasColumnName("bairro");
                    end.Property(p => p.Cep).HasColumnName("cep");
                    end.Property(p => p.Cidade).HasColumnName("cidade");
                    end.Property(p => p.Estado).HasColumnName("estado");
                });
                entity.HasOne(e => e.TimeZone)
                      .WithMany()
                      .HasForeignKey(e => e.TimeZoneId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Currency)
                      .WithMany()
                      .HasForeignKey(e => e.CurrencyId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Campos portados fielmente do legado
                entity.Property(e => e.Cpf).HasMaxLength(14);
                entity.Property(e => e.TipoConfiguracaoEstoque).HasConversion<string>();

                // Coleções (1:N) portadas do legado
                entity.HasMany(e => e.Contatos)
                      .WithOne()
                      .HasForeignKey(c => c.EmpresaId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.IeSts)
                      .WithOne()
                      .HasForeignKey(i => i.EmpresaId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Agregado 1:1 de parâmetros DF-e
                entity.HasOne(e => e.EmpresaParametrosDfe)
                      .WithOne()
                      .HasForeignKey<EmpresaParametrosDfe>(p => p.EmpresaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<EmpresaContato>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Nome).HasMaxLength(150);
                entity.Property(c => c.Email).HasMaxLength(150);
                entity.Property(c => c.Telefone).HasMaxLength(14);
                entity.Property(c => c.TipoTelefone).HasConversion<string>();
                entity.HasIndex(c => new { c.TenantId, c.EmpresaId }).HasDatabaseName("ix_empresas_contatos_tenant_empresa");
            });

            modelBuilder.Entity<IeSt>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Ie).HasMaxLength(20);
                entity.Property(i => i.Uf).HasConversion<string>();
                entity.HasIndex(i => new { i.TenantId, i.EmpresaId }).HasDatabaseName("ix_iests_tenant_empresa");
            });

            modelBuilder.Entity<EmpresaParametrosDfe>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.TipoAmbienteNfce).HasConversion<string>();
                entity.Property(p => p.TipoAmbienteNfe).HasConversion<string>();
                entity.HasIndex(p => new { p.TenantId, p.EmpresaId }).HasDatabaseName("ix_empresas_parametros_dfe_tenant_empresa");

                // Owned types (blocos NF-e / NFC-e) embutidos na tabela de parâmetros DF-e
                entity.OwnsOne(p => p.Nfe, nfe =>
                {
                    nfe.Property(n => n.ValorAliquotaCreditoIcms).HasColumnName("nfe_valor_aliquota_credito_icms").HasPrecision(18, 2);
                    nfe.Property(n => n.NfeSerieProducao).HasColumnName("nfe_serie_producao");
                    nfe.Property(n => n.NfeProximoNrProducao).HasColumnName("nfe_proximo_nr_producao");
                    nfe.Property(n => n.NfeSerieHomologacao).HasColumnName("nfe_serie_homologacao");
                    nfe.Property(n => n.NfeProximoNrHomologacao).HasColumnName("nfe_proximo_nr_homologacao");
                    nfe.Property(n => n.NfeGerarContingenciaEmHomologacao).HasColumnName("nfe_gerar_contingencia_em_homologacao");
                    nfe.Property(n => n.IndicadorSt).HasColumnName("nfe_indicador_st");
                    nfe.Property(n => n.EmitirNfeConjugada).HasColumnName("nfe_emitir_conjugada");
                });
                entity.OwnsOne(p => p.NfceHomologacao, h =>
                {
                    h.Property(n => n.NfceCscHomologacao).HasColumnName("nfce_csc_homologacao").HasMaxLength(36);
                    h.Property(n => n.NfceIdCscHomologacao).HasColumnName("nfce_id_csc_homologacao").HasMaxLength(6);
                    h.Property(n => n.NfceSerieHomologacao).HasColumnName("nfce_serie_homologacao");
                    h.Property(n => n.NfceProximoNrHomologacao).HasColumnName("nfce_proximo_nr_homologacao");
                    h.Property(n => n.NfceGerarContingenciaEmHomologacao).HasColumnName("nfce_gerar_contingencia_em_homologacao");
                });
                entity.OwnsOne(p => p.NfceProducao, pr =>
                {
                    pr.Property(n => n.NfceCscProducao).HasColumnName("nfce_csc_producao").HasMaxLength(36);
                    pr.Property(n => n.NfceIdCscProducao).HasColumnName("nfce_id_csc_producao").HasMaxLength(6);
                    pr.Property(n => n.NfceSerieProducao).HasColumnName("nfce_serie_producao");
                    pr.Property(n => n.NfceProximoNrProducao).HasColumnName("nfce_proximo_nr_producao");
                });
            });

            modelBuilder.Entity<EmpresaCertificado>(entity =>
            {
                entity.ToTable("empresa_certificado", "plataforma");
                entity.HasKey(c => c.Id);
                entity.HasOne<Empresa>()
                      .WithMany()
                      .HasForeignKey(c => c.EmpresaId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(c => new { c.EmpresaId, c.Serial, c.ValidadeFinal })
                      .HasDatabaseName("ix_empresa_certificado_empresa_serial_validade");
            });

            modelBuilder.Entity<PerfilColaborador>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.LimiteDesconto).HasPrecision(18, 2);
                entity.HasMany(p => p.Permissoes)
                      .WithOne()
                      .HasForeignKey(up => up.PerfilColaboradorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UsuarioPermissao>(entity =>
            {
                entity.HasKey(up => up.Id);
            });

            modelBuilder.Entity<Menu>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasMany(m => m.ItensNivel1)
                      .WithOne()
                      .HasForeignKey(m1 => m1.MenuId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MenuItemNivel1>(entity =>
            {
                entity.HasKey(m1 => m1.Id);
                entity.HasMany(m1 => m1.ItensNivel2)
                      .WithOne()
                      .HasForeignKey(m2 => m2.MenuItemNivel1Id)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MenuItemNivel2>(entity =>
            {
                entity.HasKey(m2 => m2.Id);
            });

            modelBuilder.Entity<PerfilAcesso>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasMany(p => p.Acessos)
                      .WithOne()
                      .HasForeignKey(a => a.PerfilAcessoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(p => new { p.TenantId, p.Descricao })
                      .IsUnique()
                      .HasDatabaseName("ix_perfis_acesso_tenant_descricao");
            });

            modelBuilder.Entity<PerfilAcessoMenu>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasIndex(a => new { a.PerfilAcessoId, a.MenuId, a.MenuItemNivel1Id, a.MenuItemNivel2Id })
                      .IsUnique()
                      .HasDatabaseName("ix_perfis_acessos_menus_combinacao_unica");
            });

            modelBuilder.Entity<Contrato>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.ValorRecorrente).HasPrecision(18, 2);
                entity.HasMany(c => c.Itens)
                      .WithOne()
                      .HasForeignKey(ci => ci.ContratoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ContratoItem>(entity =>
            {
                entity.HasKey(ci => ci.Id);
                entity.Property(ci => ci.ValorUnitario).HasPrecision(18, 2);
            });

            modelBuilder.Entity<ConfiguracaoGlobal>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.Chave).IsUnique().HasDatabaseName("ix_configuracoes_globais_chave");
            });

            modelBuilder.Entity<ExecucaoMassa>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            // Configurações de Geografia e Localização
            modelBuilder.Entity<Pais>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.CodigoIsoAlpha2).IsUnique().HasDatabaseName("ix_paises_codigo_iso_alpha_2");
                entity.HasIndex(p => p.CodigoIsoAlpha3).IsUnique().HasDatabaseName("ix_paises_codigo_iso_alpha_3");
            });

            modelBuilder.Entity<Subdivisao>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasOne(s => s.Pais)
                      .WithMany()
                      .HasForeignKey(s => s.PaisId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(s => s.TerritorioPai)
                      .WithMany()
                      .HasForeignKey(s => s.TerritorioPaiId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Municipio>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Uf).HasMaxLength(2);
                entity.HasIndex(m => m.CodigoIbge).IsUnique().HasDatabaseName("ix_municipios_codigo_ibge");
                entity.HasOne(m => m.Pais)
                      .WithMany()
                      .HasForeignKey(m => m.PaisId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(m => m.Subdivisao)
                      .WithMany()
                      .HasForeignKey(m => m.SubdivisaoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FormatoCodigoPostal>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.HasOne(f => f.Pais)
                      .WithMany()
                      .HasForeignKey(f => f.PaisId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CodigoPostalCache>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => new { c.PaisId, c.CodigoPostal }).IsUnique().HasDatabaseName("ix_codigos_postais_cache_pais_cep");
                entity.HasOne(c => c.Pais)
                      .WithMany()
                      .HasForeignKey(c => c.PaisId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(c => c.Municipio)
                      .WithMany()
                      .HasForeignKey(c => c.MunicipioId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ZonaEntrega>(entity =>
            {
                entity.HasKey(z => z.Id);
            });

            modelBuilder.Entity<SincronizacaoGeografica>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Status).HasConversion<string>();
            });

            modelBuilder.Entity<FusoHorario>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.HasIndex(f => f.FusoHorarioId).IsUnique().HasDatabaseName("ix_fusos_horarios_fuso_id");
            });

            modelBuilder.Entity<Moeda>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasIndex(m => m.CodigoISO).IsUnique().HasDatabaseName("ix_moedas_codigo_iso");
            });

            modelBuilder.Entity<UpgradePlano>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.OrderNo).IsUnique().HasDatabaseName("ix_upgrades_planos_order_no");
                entity.HasOne<Plano>()
                      .WithMany()
                      .HasForeignKey(u => u.PlanoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ExercicioFinanceiro>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => new { c.TenantId, c.Nome }).IsUnique().HasDatabaseName("ix_categorias_tenant_nome");
            });

            modelBuilder.Entity<UnidadeMedida>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => new { u.TenantId, u.Nome }).IsUnique().HasDatabaseName("ix_unidades_medida_tenant_nome");
            });

            modelBuilder.Entity<Armazem>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasIndex(a => new { a.TenantId, a.Nome }).IsUnique().HasDatabaseName("ix_armazens_tenant_nome");
            });

            modelBuilder.Entity<Projeto>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => new { p.TenantId, p.Nome }).IsUnique().HasDatabaseName("ix_projetos_tenant_nome");
            });

            modelBuilder.Entity<PreferenciaGeral>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.TenantId).IsUnique().HasDatabaseName("ix_preferencias_gerais_tenant_id");
                entity.Property(p => p.StockCalculationMode).HasConversion<string>();
            });

            modelBuilder.Entity<ConfiguracaoEmail>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.TenantId).IsUnique().HasDatabaseName("ix_configuracoes_email_tenant_id");
            });

            modelBuilder.Entity<Imposto>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.HasIndex(i => new { i.TenantId, i.Nome }).IsUnique().HasDatabaseName("ix_impostos_tenant_nome");
                entity.Property(i => i.Rate).HasPrecision(18, 2);
            });

            modelBuilder.Entity<ConversaoUnidade>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasOne<UnidadeMedida>()
                      .WithMany()
                      .HasForeignKey(c => c.UnidadeOrigemId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<UnidadeMedida>()
                      .WithMany()
                      .HasForeignKey(c => c.UnidadeDestinoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.Property(c => c.Fator).HasPrecision(18, 6);
                entity.HasIndex(c => new { c.TenantId, c.UnidadeOrigemId, c.UnidadeDestinoId }).IsUnique().HasDatabaseName("ix_conversoes_unidades_tenant_origem_destino");
            });

            modelBuilder.Entity<LogAuditoriaConfiguracao>(entity =>
            {
                entity.HasKey(l => l.Id);
            });

            // Lookups cross-module (schema financas) — leitura para REG-PEM-126/129.
            // Mapeados a tabelas de outro módulo; NÃO geram migration aqui.
            modelBuilder.Entity<ContaAPagarLookup>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.ToTable("contas_a_pagar", "financas", t => t.ExcludeFromMigrations());
                entity.Property(c => c.Id).HasColumnName("id");
                entity.Property(c => c.PessoaId).HasColumnName("pessoa_id");
                entity.Property(c => c.TenantId).HasColumnName("tenant_id");
                entity.Property(c => c.DeletadoEm).HasColumnName("deletado_em");
                entity.HasQueryFilter(c => c.DeletadoEm == null);
            });

            modelBuilder.Entity<ContaAReceberLookup>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.ToTable("contas_a_receber", "financas", t => t.ExcludeFromMigrations());
                entity.Property(c => c.Id).HasColumnName("id");
                entity.Property(c => c.PessoaId).HasColumnName("pessoa_id");
                entity.Property(c => c.TenantId).HasColumnName("tenant_id");
                entity.Property(c => c.DeletadoEm).HasColumnName("deletado_em");
                entity.HasQueryFilter(c => c.DeletadoEm == null);
            });

            // ===================== CAD-PEM: extensões de papel (1:1 via PessoaId) =====================
            modelBuilder.Entity<PessoaFornecedor>(entity =>
            {
                entity.HasKey(pf => pf.PessoaId);
                entity.Property(pf => pf.Localizacao).HasMaxLength(250);
                entity.Property(pf => pf.ChequeNominalA).HasMaxLength(150);
                entity.Property(pf => pf.Observacao).HasMaxLength(300);
                entity.Property(pf => pf.ContaRemetente).HasMaxLength(50);
                entity.HasOne<Pessoa>().WithOne().HasForeignKey<PessoaFornecedor>(pf => pf.PessoaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(pf => pf.GrupoFornecedorId).HasDatabaseName("ix_pessoas_fornecedores_grupo");
                entity.HasIndex(pf => pf.CompradorId).HasDatabaseName("ix_pessoas_fornecedores_comprador");
            });

            modelBuilder.Entity<PessoaComprador>(entity =>
            {
                entity.HasKey(pc => pc.PessoaId);
                entity.HasOne<Pessoa>().WithOne().HasForeignKey<PessoaComprador>(pc => pc.PessoaId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PessoaContador>(entity =>
            {
                entity.HasKey(pc => pc.PessoaId);
                entity.Property(pc => pc.Crc).HasMaxLength(15);
                entity.HasOne<Pessoa>().WithOne().HasForeignKey<PessoaContador>(pc => pc.PessoaId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PessoaVendedor>(entity =>
            {
                entity.HasKey(pv => pv.PessoaId);
                entity.Property(pv => pv.SenhaAPP).HasMaxLength(6);
                entity.Property(pv => pv.Email).HasMaxLength(150);
                entity.Property(pv => pv.FormaDesconto).HasMaxLength(50);
                entity.Property(pv => pv.TipoDesconto).HasMaxLength(50);
                entity.Property(pv => pv.Meta).HasPrecision(18, 2);
                entity.HasOne<Pessoa>().WithOne().HasForeignKey<PessoaVendedor>(pv => pv.PessoaId).OnDelete(DeleteBehavior.Cascade);
            });

            // ===================== CAD-PEM: governança =====================
            modelBuilder.Entity<IdentificadorFiscal>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Valor).HasMaxLength(50);
                entity.HasOne<Pessoa>().WithMany().HasForeignKey(i => i.PessoaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(i => new { i.TenantId, i.PaisId, i.Tipo, i.Valor })
                      .IsUnique()
                      .HasDatabaseName("ix_identificadores_fiscais_tenant_pais_tipo_valor");
            });

            modelBuilder.Entity<RelacionamentoParceiro>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasOne<Pessoa>().WithMany().HasForeignKey(r => r.PessoaOrigemId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<Pessoa>().WithMany().HasForeignKey(r => r.PessoaDestinoId).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(r => new { r.TenantId, r.PessoaOrigemId, r.PessoaDestinoId, r.TipoRelacao })
                      .HasDatabaseName("ix_relacionamentos_parceiro_origem_destino_tipo");
            });

            modelBuilder.Entity<RegraDeduplicacao>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Campo).HasMaxLength(100);
                entity.Property(r => r.Peso).HasPrecision(18, 4);
                entity.Property(r => r.LimiarBloqueio).HasPrecision(18, 4);
                entity.Property(r => r.LimiarAlerta).HasPrecision(18, 4);
                entity.HasIndex(r => r.TenantId).HasDatabaseName("ix_regras_deduplicacao_tenant");
            });

            modelBuilder.Entity<CandidatoDuplicata>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Score).HasPrecision(18, 4);
                entity.HasOne<Pessoa>().WithMany().HasForeignKey(c => c.PessoaAId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<Pessoa>().WithMany().HasForeignKey(c => c.PessoaBId).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(c => new { c.TenantId, c.Status, c.PessoaAId, c.PessoaBId, c.Score })
                      .HasDatabaseName("ix_candidatos_duplicata_tenant_status_pessoas_score");
            });

            modelBuilder.Entity<ConsentimentoTitular>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Canal).HasMaxLength(100);
                entity.HasOne<Pessoa>().WithMany().HasForeignKey(c => c.PessoaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(c => new { c.PessoaId, c.Finalidade, c.BaseLegal, c.DataRevogacao })
                      .HasDatabaseName("ix_consentimentos_titular_pessoa_finalidade");
            });

            modelBuilder.Entity<SolicitacaoTitular>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasOne<Pessoa>().WithMany().HasForeignKey(s => s.PessoaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(s => new { s.TenantId, s.PessoaId, s.Status })
                      .HasDatabaseName("ix_solicitacoes_titular_tenant_pessoa_status");
            });

            modelBuilder.Entity<PessoaHistoricoEstado>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.Motivo).HasMaxLength(300);
                entity.Property(h => h.UsuarioId).HasMaxLength(200);
                entity.Property(h => h.Ip).HasMaxLength(45);
                entity.HasOne<Pessoa>().WithMany().HasForeignKey(h => h.PessoaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(h => new { h.PessoaId, h.DataEvento })
                      .HasDatabaseName("ix_pessoas_historico_estado_pessoa_data");
            });

            modelBuilder.Entity<PessoaLogAuditoria>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.Entidade).HasMaxLength(100);
                entity.Property(l => l.Campo).HasMaxLength(100);
                entity.Property(l => l.UsuarioId).HasMaxLength(200);
                entity.Property(l => l.TipoEvento).HasMaxLength(50);
                entity.HasIndex(l => new { l.TenantId, l.Entidade, l.EntidadeId, l.DataEvento })
                      .HasDatabaseName("ix_pessoas_log_auditoria_tenant_entidade_data");
            });

            modelBuilder.Entity<PessoaImportacaoLote>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.NomeArquivo).HasMaxLength(260);
                entity.Property(l => l.LayoutVersao).HasMaxLength(50);
                entity.HasMany(l => l.Linhas)
                      .WithOne()
                      .HasForeignKey(li => li.LoteId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(l => l.TenantId).HasDatabaseName("ix_pessoas_importacao_lote_tenant");
            });

            modelBuilder.Entity<PessoaImportacaoLinha>(entity =>
            {
                entity.HasKey(li => li.Id);
                entity.Property(li => li.MensagemErro).HasMaxLength(1000);
                entity.HasIndex(li => li.LoteId).HasDatabaseName("ix_pessoas_importacao_linha_lote");
            });

            modelBuilder.Entity<EmpresaGrupo>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Nome).HasMaxLength(250);
                entity.HasIndex(g => new { g.TenantId, g.Nome })
                      .IsUnique()
                      .HasDatabaseName("ix_empresa_grupos_tenant_nome");
            });

            // ===================== APP-TEN-003: RBAC estendido =====================
            modelBuilder.Entity<Papel>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).HasMaxLength(100);
                entity.Property(p => p.Label).HasMaxLength(100);
                entity.Property(p => p.GuardName).HasMaxLength(100);
                entity.Property(p => p.RoleHomepage).HasMaxLength(250);
                entity.HasMany(p => p.Capacidades)
                      .WithOne()
                      .HasForeignKey(pc => pc.PapelId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(p => new { p.TenantId, p.Name })
                      .IsUnique()
                      .HasDatabaseName("ix_papeis_tenant_name");
            });

            modelBuilder.Entity<Capacidade>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).HasMaxLength(100);
                entity.Property(c => c.Label).HasMaxLength(100);
                entity.Property(c => c.Module).HasMaxLength(100);
                entity.Property(c => c.AddOn).HasMaxLength(100);
                entity.Property(c => c.PermissionKey).HasMaxLength(100);
                entity.HasIndex(c => new { c.TenantId, c.Name })
                      .IsUnique()
                      .HasDatabaseName("ix_capacidades_tenant_name");
            });

            modelBuilder.Entity<PapelCapacidade>(entity =>
            {
                entity.HasKey(pc => pc.Id);
                entity.HasIndex(pc => new { pc.PapelId, pc.CapacidadeId })
                      .IsUnique()
                      .HasDatabaseName("ix_papeis_capacidades_papel_capacidade");
            });

            modelBuilder.Entity<UsuarioPapel>(entity =>
            {
                entity.HasKey(up => up.Id);
                entity.Property(up => up.ModelType).HasMaxLength(100);
                // 1.09 — papel por empresa: a chave natural passa a incluir a empresa (nulo = todas as
                // empresas do tenant). Permite o mesmo (usuário, papel) em empresas distintas.
                entity.HasIndex(up => new { up.UsuarioId, up.PapelId, up.EmpresaId })
                      .IsUnique()
                      .HasDatabaseName("ix_usuarios_papeis_usuario_papel_empresa");
            });

            // ===================== APP-TEN-003: auditoria e segurança de usuário =====================
            modelBuilder.Entity<UsuarioCapacidade>(entity =>
            {
                entity.HasKey(uc => uc.Id);
                entity.HasOne<Capacidade>().WithMany().HasForeignKey(uc => uc.CapacidadeId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(uc => new { uc.TenantId, uc.UsuarioId, uc.CapacidadeId })
                      .IsUnique()
                      .HasDatabaseName("ix_usuarios_capacidades_tenant_usuario_capacidade");
            });

            modelBuilder.Entity<NivelUsuario>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Label).HasMaxLength(20);
                entity.HasMany(n => n.Precos)
                      .WithOne()
                      .HasForeignKey(p => p.NivelUsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(n => new { n.TenantId, n.LevelId })
                      .IsUnique()
                      .HasDatabaseName("ix_niveis_usuario_tenant_level");
            });

            modelBuilder.Entity<PrecoNivelUsuario>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.PricingLabel).HasMaxLength(50);
                entity.Property(p => p.PackagePricingType).HasMaxLength(10);
                entity.Property(p => p.Period).HasMaxLength(10);
                entity.Property(p => p.Price).HasPrecision(18, 2);
            });

            // ===================== APP-CAT: catálogos globais SaaS =====================
            modelBuilder.Entity<Funcionalidade>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Title).HasMaxLength(150);
                entity.Property(f => f.Description).HasMaxLength(1000);
                entity.HasIndex(f => f.Title)
                      .IsUnique()
                      .HasDatabaseName("ix_funcionalidades_title");
            });

            modelBuilder.Entity<AddOn>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.NomeModulo).HasMaxLength(100);
                entity.Property(a => a.Alias).HasMaxLength(100);
                entity.Property(a => a.Midia).HasMaxLength(500);
                entity.Property(a => a.PrecoMensal).HasPrecision(18, 2);
                entity.Property(a => a.PrecoAnual).HasPrecision(18, 2);
                entity.HasOne<AddOn>().WithMany().HasForeignKey(a => a.ParentAddOnId).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(a => a.NomeModulo)
                      .IsUnique()
                      .HasDatabaseName("ix_addons_nome_modulo");
            });

            modelBuilder.Entity<ModuloAtivoUsuario>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Modulo).HasMaxLength(100);
                entity.HasIndex(m => new { m.TenantId, m.UsuarioId, m.Modulo })
                      .IsUnique()
                      .HasDatabaseName("ix_modulos_ativos_usuario_tenant_usuario_modulo");
            });

            // Aplica as convenções globais de ContextBase (snake_case, RLS, Precision(18,2), unique sync_id index, etc.)
            base.OnModelCreating(modelBuilder);

            // 1.01 — Plano HÍBRIDO: sobrepõe o filtro de tenant padrão (aplicado por ContextBase) para que o
            // catálogo GLOBAL do Siser (TenantId == "system") seja visível a todos os tenants, além do plano
            // custom do próprio tenant. A criação sob contexto landlord ("system") gera catálogo; sob contexto
            // de tenant gera custom (ProcessarEntidadesSaaS). Não quebra dados existentes: planos com TenantId
            // real continuam visíveis apenas ao seu tenant.
            modelBuilder.Entity<Plano>().HasQueryFilter(p =>
                (p.TenantId == _tenantProvider.GetTenantId() || p.TenantId == "system") && p.DeletadoEm == null);

            // 1.01 — tarifa do PagamentoFatura com precisão 18,3 (EF 11.9). Definido após base.OnModelCreating
            // porque a convenção global de decimais (Precision 18,2) sobrescreveria qualquer valor definido antes.
            modelBuilder.Entity<PagamentoFatura>().Property(p => p.ValorTarifa).HasPrecision(18, 3);

            // 1.02 — Cupom HÍBRIDO (mesmo padrão do Plano): cupom global do Siser ("system") visível a todos
            // os tenants + cupom custom do próprio tenant. Não quebra dados existentes.
            modelBuilder.Entity<Cupom>().HasQueryFilter(c =>
                (c.TenantId == _tenantProvider.GetTenantId() || c.TenantId == "system") && c.DeletadoEm == null);

            // 1.02 — unicidade de catálogos globais: Moeda por CodigoISO, Pais por Nome (REG-006).
            modelBuilder.Entity<Moeda>().HasIndex(m => m.CodigoISO).IsUnique();
            modelBuilder.Entity<Pais>().HasIndex(p => p.Nome).IsUnique();
        }
    }
}
