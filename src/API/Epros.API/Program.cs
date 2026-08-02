using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Serilog;
using Quartz;
using MediatR;
using Epros.API.Middlewares;
using Epros.API.Providers;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Infrastructure.Services;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.Aplicativo.Application.Services;
using Epros.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Migrations;

using Epros.Modules.Financeiro.Infrastructure.Jobs;
using Epros.Modules.Vendas.Infrastructure.Jobs;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Modules.Qualidade.Infrastructure.Jobs;
using Epros.Modules.Aplicativo.Infrastructure.Jobs;
using Epros.Modules.Producao.Infrastructure.Data;
using Epros.Modules.Producao.Infrastructure.Jobs;
using Epros.Modules.RH.Infrastructure.Data;
using Epros.Modules.RH.Infrastructure.Jobs;
using Epros.Modules.Projetos.Infrastructure.Data;
using Epros.Modules.Projetos.Infrastructure.Jobs;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Epros.Modules.Manutencao.Infrastructure.Jobs;
using Epros.Modules.GRC.Infrastructure.Data;
using Epros.Modules.GRC.Infrastructure.Jobs;
using Epros.Modules.ESG.Infrastructure.Data;
using Epros.Modules.Imobiliaria.Infrastructure.Data;
using Epros.Modules.DMS.Infrastructure.Data;

// Inicializa o logger Serilog antes do bootstrap da aplicação
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Iniciando o Epros API Gateway...");

    var builder = WebApplication.CreateBuilder(args);

    // Configura Serilog como o logger padrão
    builder.Host.UseSerilog();

    // Serviços básicos de controle
    builder.Services.AddControllers();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevCorsPolicy", policy =>
        {
            policy.WithOrigins(
                      "http://localhost:3000",
                      "http://localhost:3003",
                      "http://127.0.0.1:3000",
                      "http://127.0.0.1:3003")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });

        var corsOrigins = builder.Configuration["CORS_ORIGINS"]?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? Array.Empty<string>();

        options.AddPolicy("ProdCorsPolicy", policy =>
        {
            if (corsOrigins.Length > 0)
            {
                policy.WithOrigins(corsOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            }
        });
    });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    });
    builder.Services.AddHttpContextAccessor();

    // Host-guard do Landlord (1.04): hostnames do painel Siser vs cliente. Seção "Hosts".
    // Vazio => guard inativo (fail-safe dev/test). Em produção Hosts:Landlord DEVE ser configurado.
    builder.Services.Configure<Epros.API.Middlewares.HostGuardOptions>(
        builder.Configuration.GetSection(Epros.API.Middlewares.HostGuardOptions.SecaoConfig));

    // Registra os provedores de contexto de Tenant e Usuário
    builder.Services.AddScoped<ITenantProvider, TenantProvider>();
    builder.Services.AddScoped<ICurrentUser, CurrentUser>();

    // Registra o serviço de hashing de senhas (PBKDF2 / HMAC-SHA256). Sem estado -> Singleton.
    builder.Services.AddSingleton<IPasswordHasher, Epros.Infrastructure.Services.Pbkdf2PasswordHasher>();
    builder.Services.AddScoped<IValidadorLimitesSaaS, Epros.Modules.Aplicativo.Application.Services.ValidadorLimitesSaaS>();
    // 1.10 (PERMISSOES_DE_MENU) — fonte única das capacidades efetivas do RBAC. Scoped: vive por request
    // (memoiza por usuário/empresa — REG-070/item 6), compartilhada entre o AbacFilter (gate) e a projeção
    // de menu (GET /menu), garantindo "item visível ⇔ endpoint autoriza" (LC-1/LC-2).
    builder.Services.AddScoped<Epros.Modules.GestaoClientes.Application.Services.ICapacidadesEfetivasService, Epros.Modules.GestaoClientes.Application.Services.CapacidadesEfetivasService>();
    // Serviço de cálculo de próximas execuções de agendamentos de workflow (PLT-WF §7.4.3).
    builder.Services.AddScoped<Epros.Modules.Aplicativo.Application.Services.IAgendaIntervalarService, Epros.Modules.Aplicativo.Application.Services.AgendaIntervalarService>();

    // Registra o Cofre de Segredos (Vault)
    builder.Services.AddHttpClient<ISegredoCofreService, Epros.Infrastructure.Services.VaultEncryptionService>();
    // T5 — rotação de segredos: mesma instância concreta do cofre (VaultEncryptionService) exposta
    // também como ISegredoRotacaoService (resolve o registro tipado acima; não recria HttpClient).
    builder.Services.AddScoped<Epros.Shared.Application.Contracts.ISegredoRotacaoService>(sp =>
        (Epros.Infrastructure.Services.VaultEncryptionService)sp.GetRequiredService<ISegredoCofreService>());

    // ===== TRANSVERSAIS COMPARTILHADAS (kernel) — serviços centrais =====
    // T9 numeração central · T8 auditoria imutável · T10 assinatura ICP (default fail-safe).
    builder.Services.AddScoped<Epros.Shared.Application.Contracts.INumeracaoService, Epros.Modules.Aplicativo.Infrastructure.Services.NumeracaoService>();
    builder.Services.AddScoped<Epros.Shared.Application.Contracts.IRegistroAuditoriaService, Epros.Modules.Aplicativo.Infrastructure.Services.RegistroAuditoriaService>();
    builder.Services.AddScoped<Epros.Shared.Application.Contracts.IAssinaturaDigitalService, Epros.Modules.Aplicativo.Infrastructure.Services.AssinaturaDigitalPendenteService>();

    // Gateway de pagamento (outbound) — Mercado Pago. HttpClient nomeado + implementação.
    builder.Services.AddHttpClient(Epros.Modules.GestaoClientes.Infrastructure.Gateways.MercadoPagoGateway.HttpClientName, client =>
    {
        client.BaseAddress = new Uri("https://api.mercadopago.com/");
        client.Timeout = TimeSpan.FromSeconds(30);
    });
    builder.Services.AddScoped<Epros.Modules.GestaoClientes.Application.Interfaces.IPaymentGateway, Epros.Modules.GestaoClientes.Infrastructure.Gateways.MercadoPagoGateway>();

    // 1.08B — Serviço de liquidação de fatura compartilhado (webhook PIX/boleto/checkout + cartão recorrente).
    builder.Services.AddScoped<Epros.Modules.GestaoClientes.Application.Services.FaturaLiquidacaoService>();

    // 1.08F — Renderizador de documentos financeiros (PDF real via QuestPDF, licença Community).
    builder.Services.AddScoped<Epros.Modules.GestaoClientes.Application.Documentos.IDocumentoFinanceiroRenderer,
        Epros.Modules.GestaoClientes.Infrastructure.Documentos.QuestPdfDocumentoFinanceiroRenderer>();

    // 1.08B — Cobrança recorrente por cartão-on-file: implementação CONCRETA (Mercado Pago Customers/Cards).
    // Substitui o no-op da passada A. ⛔ PCI: só o token do MP toca o backend, nunca PAN/CVV.
    builder.Services.AddScoped<Epros.Modules.GestaoClientes.Application.Interfaces.ICobrancaRecorrenteGateway, Epros.Modules.GestaoClientes.Infrastructure.Gateways.CobrancaRecorrenteGatewayMercadoPago>();

    // Registra o serviço de notificações (Mock para homologação local) (REG-020)
    builder.Services.AddScoped<INotificacaoService, Epros.Infrastructure.Services.MockNotificacaoService>();

    // Registra o cache de permissões do menu e gerenciador
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<IPermissaoCacheManager, Epros.Modules.Aplicativo.Application.Services.PermissaoCacheManager>();
    builder.Services.AddSingleton<Epros.Modules.GestaoClientes.Application.Contracts.IConfiguracaoGlobalCache, Epros.Modules.GestaoClientes.Infrastructure.Services.ConfiguracaoGlobalCache>();

    // Login social (1.04 PASS 3): OAuth 2.0 / OIDC (Google + Microsoft). Config por IConfiguration
    // (Autenticacao:Social:{Google|Microsoft}) — segredos via env/secret; sem hardcode. HttpClient
    // nomeado para discovery/token/JWKS + cliente OIDC (valida id_token contra o JWKS do provedor).
    builder.Services.Configure<Epros.Modules.Aplicativo.Application.Services.AutenticacaoSocialOptions>(
        builder.Configuration.GetSection(Epros.Modules.Aplicativo.Application.Services.AutenticacaoSocialOptions.SecaoConfig));
    builder.Services.AddHttpClient(Epros.Modules.Aplicativo.Infrastructure.Services.OidcSocialClient.HttpClientName, client =>
    {
        client.Timeout = TimeSpan.FromSeconds(15);
    });
    builder.Services.AddScoped<Epros.Modules.Aplicativo.Infrastructure.Services.IOidcSocialClient, Epros.Modules.Aplicativo.Infrastructure.Services.OidcSocialClient>();

    // Registra o interceptor de RLS
    builder.Services.AddScoped<TenantRlsInterceptor>();

    // Registra o DbContext do módulo GestaoClientes (com PostgreSQL e RLS)
    builder.Services.AddDbContext<ContextGestaoClientes>((serviceProvider, options) =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
               .AddInterceptors(serviceProvider.GetRequiredService<TenantRlsInterceptor>())
               .ReplaceService<IMigrationsSqlGenerator, EprosMigrationsSqlGenerator>());

    // Registra o DbContext do módulo Aplicativo (com PostgreSQL e RLS)
    builder.Services.AddDbContext<ContextAplicativo>((serviceProvider, options) =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
               .AddInterceptors(serviceProvider.GetRequiredService<TenantRlsInterceptor>())
               .ReplaceService<IMigrationsSqlGenerator, EprosMigrationsSqlGenerator>());

    // Registra o DbContext do módulo Estoque (com PostgreSQL e RLS)
    builder.Services.AddDbContext<ContextEstoque>((serviceProvider, options) =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
               .AddInterceptors(serviceProvider.GetRequiredService<TenantRlsInterceptor>())
               .ReplaceService<IMigrationsSqlGenerator, EprosMigrationsSqlGenerator>());

    // Registra o DbContext do módulo Fiscal (com PostgreSQL e RLS)
    builder.Services.AddDbContext<ContextFiscal>((serviceProvider, options) =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
               .AddInterceptors(serviceProvider.GetRequiredService<TenantRlsInterceptor>())
               .ReplaceService<IMigrationsSqlGenerator, EprosMigrationsSqlGenerator>());

    // Registra o DbContext do módulo Financeiro (com PostgreSQL e RLS)
    builder.Services.AddDbContext<ContextFinanceiro>((serviceProvider, options) =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
               .AddInterceptors(serviceProvider.GetRequiredService<TenantRlsInterceptor>())
               .ReplaceService<IMigrationsSqlGenerator, EprosMigrationsSqlGenerator>());

    // Registra o DbContext do módulo Vendas (com PostgreSQL e RLS)
    builder.Services.AddDbContext<ContextVendas>((serviceProvider, options) =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
               .AddInterceptors(serviceProvider.GetRequiredService<TenantRlsInterceptor>())
               .ReplaceService<IMigrationsSqlGenerator, EprosMigrationsSqlGenerator>());

    // Registra o DbContext do módulo Qualidade (com PostgreSQL e RLS)
    builder.Services.AddDbContext<ContextQualidade>((serviceProvider, options) =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
               .AddInterceptors(serviceProvider.GetRequiredService<TenantRlsInterceptor>())
               .ReplaceService<IMigrationsSqlGenerator, EprosMigrationsSqlGenerator>());

    // Registra o DbContext do módulo Producao (com PostgreSQL e RLS)
    builder.Services.AddDbContext<ContextProducao>((serviceProvider, options) =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
               .AddInterceptors(serviceProvider.GetRequiredService<TenantRlsInterceptor>())
               .ReplaceService<IMigrationsSqlGenerator, EprosMigrationsSqlGenerator>());

    // Registra o DbContext do módulo RH (com PostgreSQL e RLS)
    builder.Services.AddDbContext<ContextRH>((serviceProvider, options) =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
               .AddInterceptors(serviceProvider.GetRequiredService<TenantRlsInterceptor>())
               .ReplaceService<IMigrationsSqlGenerator, EprosMigrationsSqlGenerator>());

    // Registra o DbContext do módulo Projetos (com PostgreSQL e RLS)
    builder.Services.AddDbContext<ContextProjetos>((serviceProvider, options) =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
               .AddInterceptors(serviceProvider.GetRequiredService<TenantRlsInterceptor>())
               .ReplaceService<IMigrationsSqlGenerator, EprosMigrationsSqlGenerator>());

    // Registra o DbContext do módulo Manutencao (com PostgreSQL e RLS)
    builder.Services.AddDbContext<ContextManutencao>((serviceProvider, options) =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
               .AddInterceptors(serviceProvider.GetRequiredService<TenantRlsInterceptor>())
               .ReplaceService<IMigrationsSqlGenerator, EprosMigrationsSqlGenerator>());

    // Registra o DbContext do módulo GRC (com PostgreSQL e RLS)
    builder.Services.AddDbContext<ContextGRC>((serviceProvider, options) =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
               .AddInterceptors(serviceProvider.GetRequiredService<TenantRlsInterceptor>())
               .ReplaceService<IMigrationsSqlGenerator, EprosMigrationsSqlGenerator>());

    // Registra o DbContext do módulo ESG (com PostgreSQL e RLS)
    builder.Services.AddDbContext<ContextESG>((serviceProvider, options) =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
               .AddInterceptors(serviceProvider.GetRequiredService<TenantRlsInterceptor>())
               .ReplaceService<IMigrationsSqlGenerator, EprosMigrationsSqlGenerator>());

    // Registra o DbContext do módulo DMS (com PostgreSQL e RLS)
    builder.Services.AddDbContext<ContextDMS>((serviceProvider, options) =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
               .AddInterceptors(serviceProvider.GetRequiredService<TenantRlsInterceptor>())
               .ReplaceService<IMigrationsSqlGenerator, EprosMigrationsSqlGenerator>());

    // Registra o DbContext do módulo Imobiliária (com PostgreSQL e RLS)
    builder.Services.AddDbContext<ContextImobiliaria>((serviceProvider, options) =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
               .AddInterceptors(serviceProvider.GetRequiredService<TenantRlsInterceptor>())
               .ReplaceService<IMigrationsSqlGenerator, EprosMigrationsSqlGenerator>());

    // Registra serviços do módulo Fiscal
    // Motor de CÁLCULO fiscal (envelopa Epros.ERP.DfeCalculos)
    builder.Services.AddScoped<ICalculoFiscalService, MotorLegadoCalculoFiscalService>();
    // Calculadora de impostos por item de DocumentoFiscal (ponto único reutilizado na emissão manual
    // e na geração automática a partir da venda faturada).
    builder.Services.AddScoped<CalculadoraImpostosDocumentoFiscal>();

    // Adapter de TRANSMISSÃO SEFAZ real (Hercules ServicosNFe): autorização, cancelamento, CC-e e inutilização.
    // Fábrica de configuração/certificado (certificado A1 -> ConfiguracaoServico do Hercules).
    builder.Services.AddScoped<HerculesConfiguracaoFactory>();
    // Provider REAL do emitente/certificado: resolve Empresa + EmpresaCertificado (cofre) + parâmetros DF-e
    // pela DocumentoFiscal.EmpresaId, via Lookups no ContextFiscal. Fallback honesto (null) quando faltar
    // empresa/certificado/parâmetros — a transmissão degrada de forma controlada, sem chave/protocolo simulado.
    builder.Services.AddScoped<IEmitenteFiscalProvider, EmpresaEmitenteFiscalProvider>();
    builder.Services.AddScoped<IHerculesFiscalService, MotorLegadoFiscalService>();

    // Geração de PDF do DANFE/cupom (QuestPDF) e armazenamento de XML/PDF.
    // Storage local hoje (sem MinIO no repositório); trocar a implementação aqui quando houver MinIO/S3.
    builder.Services.AddSingleton<IDanfeService, DanfeQuestPdfService>();
    builder.Services.AddSingleton<IArmazenamentoArquivoFiscal>(_ => new ArmazenamentoArquivoFiscalLocal());

    // DANFE de PRÉ-VISUALIZAÇÃO (sem autorização) para o módulo Vendas, por contrato Shared (Guid FK).
    // Fallback HONESTO: retorna indisponível até existir o adaptador Vendas->Fiscal (DocumentoFiscal + IDanfeService).
    builder.Services.AddScoped<Epros.Shared.Application.Contracts.IDanfeVendaService,
        Epros.Modules.Vendas.Infrastructure.Services.DanfeVendaServiceIndisponivel>();

    // Adapters de transmissão de NFS-e / CT-e / MDF-e. A integração real (OpenAC.Net.NFSe por município;
    // Hercules/Zeus.Net.CTe/MDFe) é específica do ambiente e entra na homologação. Por ora usam fallbacks
    // HONESTOS: degradam de forma controlada (motivo claro), NUNCA fabricando número/chave/protocolo.
    // Trocar a implementação aqui quando a integração municipal/transporte for configurada.
    builder.Services.AddScoped<INfseFiscalService, NfseFiscalServiceNaoConfigurado>();
    builder.Services.AddScoped<ICteFiscalService, CteFiscalServiceNaoConfigurado>();
    builder.Services.AddScoped<IMdfeFiscalService, MdfeFiscalServiceNaoConfigurado>();

    // Registra o MediatR para o processamento de Commands/Queries nos módulos
    // Motores de dominio do modulo Qualidade (sem estado; QLD-INS amostragem AQL / comutacao).
    builder.Services.AddSingleton<Epros.Modules.Qualidade.Domain.Services.Aql.MotorAql>();
    builder.Services.AddSingleton<Epros.Modules.Qualidade.Domain.Services.Aql.MotorComutacao>();
    builder.Services.AddSingleton<Epros.Modules.Qualidade.Domain.Services.Qps.MotorScoreFornecedor>();
    builder.Services.AddSingleton<Epros.Modules.Qualidade.Domain.Services.Rst.MotorGenealogia>();

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        // Modulo RELATORIOS & BI (RPT): read-side puro, sem DbContext proprio, portanto seu
        // assembly nao e carregado por AddDbContext. Registro explicito garante a descoberta dos
        // query handlers (RPT-OPB/RPT-ONM) independentemente da ordem de carga de assemblies.
        cfg.RegisterServicesFromAssembly(typeof(Epros.Modules.Relatorios.Application.Queries.KpiFaturamentoQuery).Assembly);
        // Também registrará os Handlers dos módulos adicionados como referências de projeto
        cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(Epros.Modules.Aplicativo.Infrastructure.Behaviors.MakerCheckerPipelineBehavior<,>));
    });

    // Registra o Quartz.NET para Jobs em segundo plano
    builder.Services.AddQuartz(q =>
    {
        var jobKey = new JobKey("VerificarFaturasVencidasJob");
        q.AddJob<Epros.Modules.GestaoClientes.Infrastructure.Jobs.VerificarFaturasVencidasJob>(opts => opts.WithIdentity(jobKey));

        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity("VerificarFaturasVencidasJob-trigger")
            // Cron expression para rodar diariamente à meia-noite
            .WithCronSchedule("0 0 0 * * ?"));

        var recJobKey = new JobKey("ProcessarFaturamentoRecorrenteJob");
        q.AddJob<Epros.Modules.GestaoClientes.Infrastructure.Jobs.ProcessarFaturamentoRecorrenteJob>(opts => opts.WithIdentity(recJobKey));

        q.AddTrigger(opts => opts
            .ForJob(recJobKey)
            .WithIdentity("ProcessarFaturamentoRecorrenteJob-trigger")
            // Cron expression para rodar diariamente às 00:05
            .WithCronSchedule("0 5 0 * * ?"));

        var reguaCobrancaJobKey = new JobKey("ReguaCobrancaJob");
        q.AddJob<Epros.Modules.GestaoClientes.Infrastructure.Jobs.ReguaCobrancaJob>(opts => opts.WithIdentity(reguaCobrancaJobKey));

        q.AddTrigger(opts => opts
            .ForJob(reguaCobrancaJobKey)
            .WithIdentity("ReguaCobrancaJob-trigger")
            // Cron expression para rodar diariamente à meia-noite e 10 minutos
            .WithCronSchedule("0 10 0 * * ?"));

        var reajusteContratoJobKey = new JobKey("ReajusteContratoJob");
        q.AddJob<Epros.Modules.GestaoClientes.Infrastructure.Jobs.ReajusteContratoJob>(opts => opts.WithIdentity(reajusteContratoJobKey));

        q.AddTrigger(opts => opts
            .ForJob(reajusteContratoJobKey)
            .WithIdentity("ReajusteContratoJob-trigger")
            // Cron expression para rodar mensalmente no dia 1 às 00:30
            .WithCronSchedule("0 30 0 1 * ?"));

        // 1.08A — Encerra trials expirados (gera 1ª fatura + dispara cobrança). Diário às 00:20.
        var encerrarTrialsJobKey = new JobKey("EncerrarTrialsExpiradosJob");
        q.AddJob<Epros.Modules.GestaoClientes.Infrastructure.Jobs.EncerrarTrialsExpiradosJob>(opts => opts.WithIdentity(encerrarTrialsJobKey));

        q.AddTrigger(opts => opts
            .ForJob(encerrarTrialsJobKey)
            .WithIdentity("EncerrarTrialsExpiradosJob-trigger")
            .WithCronSchedule("0 20 0 * * ?"));

        // 1.08B — Renova assinaturas por ciclo (mensal/anual; vitalícia não renova). Diário às 00:25.
        var renovacaoJobKey = new JobKey("ProcessarRenovacaoAssinaturasJob");
        q.AddJob<Epros.Modules.GestaoClientes.Infrastructure.Jobs.ProcessarRenovacaoAssinaturasJob>(opts => opts.WithIdentity(renovacaoJobKey));

        q.AddTrigger(opts => opts
            .ForJob(renovacaoJobKey)
            .WithIdentity("ProcessarRenovacaoAssinaturasJob-trigger")
            .WithCronSchedule("0 25 0 * * ?"));

        var outboxJobKey = new JobKey("OutboxProcessorJob");
        q.AddJob<OutboxProcessorJob>(opts => opts.WithIdentity(outboxJobKey));

        q.AddTrigger(opts => opts
            .ForJob(outboxJobKey)
            .WithIdentity("OutboxProcessorJob-trigger")
            .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()));

        var vendasOutboxJobKey = new JobKey("VendasOutboxProcessorJob");
        q.AddJob<VendasOutboxProcessorJob>(opts => opts.WithIdentity(vendasOutboxJobKey));

        q.AddTrigger(opts => opts
            .ForJob(vendasOutboxJobKey)
            .WithIdentity("VendasOutboxProcessorJob-trigger")
            .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()));

        var aplicativoOutboxJobKey = new JobKey("AplicativoOutboxProcessorJob");
        q.AddJob<AplicativoOutboxProcessorJob>(opts => opts.WithIdentity(aplicativoOutboxJobKey));

        q.AddTrigger(opts => opts
            .ForJob(aplicativoOutboxJobKey)
            .WithIdentity("AplicativoOutboxProcessorJob-trigger")
            .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()));

        // 1.08C — Processador de Outbox do GestaoClientes: ENTREGA os alertas da régua de cobrança
        // (FaturaAlertaCobrancaEvent), fim de trial (TrialEncerradoEvent) e recibo (ReciboEmitidoEvent),
        // que antes eram enfileirados e nunca consumidos.
        var gestaoClientesOutboxJobKey = new JobKey("GestaoClientesOutboxProcessorJob");
        q.AddJob<Epros.Modules.GestaoClientes.Infrastructure.Jobs.GestaoClientesOutboxProcessorJob>(opts => opts.WithIdentity(gestaoClientesOutboxJobKey));

        q.AddTrigger(opts => opts
            .ForJob(gestaoClientesOutboxJobKey)
            .WithIdentity("GestaoClientesOutboxProcessorJob-trigger")
            .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()));

        var qualidadeOutboxJobKey = new JobKey("QualidadeOutboxProcessorJob");
        q.AddJob<QualidadeOutboxProcessorJob>(opts => opts.WithIdentity(qualidadeOutboxJobKey));

        q.AddTrigger(opts => opts
            .ForJob(qualidadeOutboxJobKey)
            .WithIdentity("QualidadeOutboxProcessorJob-trigger")
            .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()));

        var producaoOutboxJobKey = new JobKey("ProducaoOutboxProcessorJob");
        q.AddJob<ProducaoOutboxProcessorJob>(opts => opts.WithIdentity(producaoOutboxJobKey));

        q.AddTrigger(opts => opts
            .ForJob(producaoOutboxJobKey)
            .WithIdentity("ProducaoOutboxProcessorJob-trigger")
            .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()));

        var rhOutboxJobKey = new JobKey("RHOutboxProcessorJob");
        q.AddJob<RHOutboxProcessorJob>(opts => opts.WithIdentity(rhOutboxJobKey));

        q.AddTrigger(opts => opts
            .ForJob(rhOutboxJobKey)
            .WithIdentity("RHOutboxProcessorJob-trigger")
            .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()));

        var projetosOutboxJobKey = new JobKey("ProjetosOutboxProcessorJob");
        q.AddJob<ProjetosOutboxProcessorJob>(opts => opts.WithIdentity(projetosOutboxJobKey));

        q.AddTrigger(opts => opts
            .ForJob(projetosOutboxJobKey)
            .WithIdentity("ProjetosOutboxProcessorJob-trigger")
            .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()));

        var manutencaoOutboxJobKey = new JobKey("ManutencaoOutboxProcessorJob");
        q.AddJob<ManutencaoOutboxProcessorJob>(opts => opts.WithIdentity(manutencaoOutboxJobKey));

        q.AddTrigger(opts => opts
            .ForJob(manutencaoOutboxJobKey)
            .WithIdentity("ManutencaoOutboxProcessorJob-trigger")
            .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()));

        // MAN-PRV D7 — scheduler de vencimento da preventiva (calendario/contador).
        var preventivaSchedulerJobKey = new JobKey("PreventivaSchedulerJob");
        q.AddJob<PreventivaSchedulerJob>(opts => opts.WithIdentity(preventivaSchedulerJobKey));

        q.AddTrigger(opts => opts
            .ForJob(preventivaSchedulerJobKey)
            .WithIdentity("PreventivaSchedulerJob-trigger")
            .WithSimpleSchedule(x => x.WithIntervalInMinutes(30).RepeatForever()));

        var grcOutboxJobKey = new JobKey("GRCOutboxProcessorJob");
        q.AddJob<GRCOutboxProcessorJob>(opts => opts.WithIdentity(grcOutboxJobKey));

        q.AddTrigger(opts => opts
            .ForJob(grcOutboxJobKey)
            .WithIdentity("GRCOutboxProcessorJob-trigger")
            .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()));

        var expiracaoSessoesJobKey = new JobKey("ExpiracaoSessoesJob");
        q.AddJob<ExpiracaoSessoesJob>(opts => opts.WithIdentity(expiracaoSessoesJobKey));

        q.AddTrigger(opts => opts
            .ForJob(expiracaoSessoesJobKey)
            .WithIdentity("ExpiracaoSessoesJob-trigger")
            .WithSimpleSchedule(x => x.WithIntervalInMinutes(10).RepeatForever()));

        var workflowAgendamentoJobKey = new JobKey("WorkflowAgendamentoJob");
        q.AddJob<Epros.Modules.Aplicativo.Infrastructure.Jobs.WorkflowAgendamentoJob>(opts => opts.WithIdentity(workflowAgendamentoJobKey));

        q.AddTrigger(opts => opts
            .ForJob(workflowAgendamentoJobKey)
            .WithIdentity("WorkflowAgendamentoJob-trigger")
            // Granularidade da expressão intervalar de workflow é o minuto (PLT-WF §7.4.3).
            .WithSimpleSchedule(x => x.WithIntervalInMinutes(1).RepeatForever()));

        var expurgarNewsletterJobKey = new JobKey("ExpurgarNewsletterInativaJob");
        q.AddJob<ExpurgarNewsletterInativaJob>(opts => opts.WithIdentity(expurgarNewsletterJobKey));

        q.AddTrigger(opts => opts
            .ForJob(expurgarNewsletterJobKey)
            .WithIdentity("ExpurgarNewsletterInativaJob-trigger")
            .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0)));

        var sincronizarGeografiaJobKey = new JobKey("SincronizarGeografiaJob");
        q.AddJob<Epros.Modules.GestaoClientes.Infrastructure.Jobs.SincronizarGeografiaJob>(opts => opts.WithIdentity(sincronizarGeografiaJobKey));

        q.AddTrigger(opts => opts
            .ForJob(sincronizarGeografiaJobKey)
            .WithIdentity("SincronizarGeografiaJob-trigger")
            .WithCronSchedule("0 0 1 * * ?"));
    });
    builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

    // AUTENTICAÇÃO
    // Esquema nativo do EprosERP (EprosToken): valida os JWTs assinados (HS256, com expiração)
    // emitidos pelo login via IEprosTokenService e materializa o ClaimsPrincipal autenticado.
    // SEGURANÇA (fechamento do "gato"): NÃO existe mais autenticação por header no runtime — o
    // handler aceita apenas o token assinado. Testes injetam identidade pelo host de teste.
    // Mantém o JWT Bearer do Keycloak registrado (como esquema adicional) para migração futura,
    // mas o esquema PADRÃO é o EprosToken — é ele quem impõe a autenticação hoje.

    // Serviço central do token nativo (JWT HS256 assinado). Substitui o antigo token em texto
    // plano forjável. A chave vem de configuração (env/secret). Fail-closed: em QUALQUER ambiente
    // deployado (Production/Staging/Testing, ou Development em contêiner/CI) a chave é obrigatória
    // e sua ausência aborta o startup. Só desenvolvimento local puro cai na chave fixa de dev.
    var permiteFallbackDevLocal = Epros.Shared.Security.AmbienteImplantacao
        .EhDesenvolvimentoLocal(builder.Environment.EnvironmentName);
    var jwtSigningKey = builder.Configuration["Seguranca:JwtSigningKey"];
    if (string.IsNullOrWhiteSpace(jwtSigningKey))
    {
        if (!permiteFallbackDevLocal)
        {
            throw new InvalidOperationException(
                "Seguranca:JwtSigningKey não configurada. Defina a chave de assinatura do token (env/secret) antes de iniciar fora de desenvolvimento local.");
        }

        // Chave fixa de desenvolvimento (>= 32 chars) — apenas para dev local puro.
        jwtSigningKey = "epros-dev-signing-key-please-change-0123456789";
    }
    builder.Services.AddSingleton<Epros.Shared.Security.IEprosTokenService>(
        new Epros.Shared.Security.EprosTokenService(jwtSigningKey));

    builder.Services.AddAuthentication(Epros.API.Security.EprosTokenAuthenticationHandler.SchemeName)
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, Epros.API.Security.EprosTokenAuthenticationHandler>(
            Epros.API.Security.EprosTokenAuthenticationHandler.SchemeName, _ => { })
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = "http://localhost:8080/realms/epros-tenant";
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false
            };
        });

    // AUTORIZAÇÃO
    // FallbackPolicy fecha a API por padrão: todo endpoint sem política explícita exige usuário
    // autenticado. Rotas realmente públicas (login/auth, health, swagger, webhooks) usam
    // [AllowAnonymous]. Isso elimina o gap S1 (API aberta) sem depender do Keycloak.
    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder(
                Epros.API.Security.EprosTokenAuthenticationHandler.SchemeName)
            .RequireAuthenticatedUser()
            .Build();
    });

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        builder.Services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgresql");
    }

    var app = builder.Build();

    // Barreira de boot (REG-001): falha o startup se alguma entidade mapeada não estiver
    // classificada quanto ao tenant (não herda EntidadeSaaSBase nem é IGlobalEntity). Fecha o
    // vazamento silencioso de uma entidade nova criada fora do padrão de isolamento.
    using (var guardScope = app.Services.CreateScope())
    {
        var sp = guardScope.ServiceProvider;
        var contextosParaValidar = new Microsoft.EntityFrameworkCore.DbContext[]
        {
            sp.GetRequiredService<ContextGestaoClientes>(),
            sp.GetRequiredService<Epros.Modules.Aplicativo.Infrastructure.Data.ContextAplicativo>(),
            sp.GetRequiredService<ContextEstoque>(),
            sp.GetRequiredService<ContextFiscal>(),
            sp.GetRequiredService<ContextFinanceiro>(),
            sp.GetRequiredService<ContextVendas>(),
            sp.GetRequiredService<ContextQualidade>(),
            sp.GetRequiredService<ContextProducao>(),
            sp.GetRequiredService<ContextRH>(),
            sp.GetRequiredService<ContextProjetos>(),
            sp.GetRequiredService<ContextManutencao>(),
            sp.GetRequiredService<ContextGRC>(),
            sp.GetRequiredService<ContextESG>(),
            sp.GetRequiredService<ContextDMS>(),
            sp.GetRequiredService<ContextImobiliaria>(),
        };
        Epros.Infrastructure.Data.GuardaEntidadeOrfa.ValidarModelos(contextosParaValidar);
    }

    if (args.Contains("--seed-fiscal"))
    {
        using var seedScope = app.Services.CreateScope();
        var dbFiscalSeed = seedScope.ServiceProvider.GetRequiredService<ContextFiscal>();
        Log.Information("Semeando catálogos fiscais (CFOP/CST IBS-CBS)...");
        await Epros.Modules.Fiscal.Infrastructure.Data.CatalogoFiscalSeeder.SeedAsync(dbFiscalSeed);
        Log.Information("Catálogos fiscais semeados com sucesso.");
        return;
    }

    // Executa as migrations automáticas para todos os DbContexts se estiver em Desenvolvimento
    if (app.Environment.IsDevelopment())
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                Log.Information("Aplicando migrations pendentes para ContextGestaoClientes...");
                var dbClientes = services.GetRequiredService<ContextGestaoClientes>();
                dbClientes.Database.Migrate();

                Log.Information("Aplicando migrations pendentes para ContextAplicativo...");
                var dbAplicativo = services.GetRequiredService<ContextAplicativo>();
                dbAplicativo.Database.Migrate();

                Log.Information("Aplicando migrations pendentes para ContextEstoque...");
                var dbEstoque = services.GetRequiredService<ContextEstoque>();
                dbEstoque.Database.Migrate();

                Log.Information("Aplicando migrations pendentes para ContextFiscal...");
                var dbFiscal = services.GetRequiredService<ContextFiscal>();
                dbFiscal.Database.Migrate();

                Log.Information("Aplicando migrations pendentes para ContextFinanceiro...");
                var dbFinanceiro = services.GetRequiredService<ContextFinanceiro>();
                dbFinanceiro.Database.Migrate();

                Log.Information("Aplicando migrations pendentes para ContextVendas...");
                var dbVendas = services.GetRequiredService<ContextVendas>();
                dbVendas.Database.Migrate();

                Log.Information("Aplicando migrations pendentes para ContextQualidade...");
                var dbQualidade = services.GetRequiredService<ContextQualidade>();
                dbQualidade.Database.Migrate();

                Log.Information("Aplicando migrations pendentes para ContextProducao...");
                var dbProducao = services.GetRequiredService<ContextProducao>();
                dbProducao.Database.Migrate();

                Log.Information("Aplicando migrations pendentes para ContextRH...");
                var dbRH = services.GetRequiredService<ContextRH>();
                dbRH.Database.Migrate();

                Log.Information("Aplicando migrations pendentes para ContextProjetos...");
                var dbProjetos = services.GetRequiredService<ContextProjetos>();
                dbProjetos.Database.Migrate();

                Log.Information("Aplicando migrations pendentes para ContextManutencao...");
                var dbManutencao = services.GetRequiredService<ContextManutencao>();
                dbManutencao.Database.Migrate();

                Log.Information("Aplicando migrations pendentes para ContextGRC...");
                var dbGRC = services.GetRequiredService<ContextGRC>();
                dbGRC.Database.Migrate();

                Log.Information("Aplicando migrations pendentes para ContextESG...");
                var dbESG = services.GetRequiredService<ContextESG>();
                dbESG.Database.Migrate();

                Log.Information("Aplicando migrations pendentes para ContextDMS...");
                var dbDMS = services.GetRequiredService<ContextDMS>();
                dbDMS.Database.Migrate();

                Log.Information("Aplicando migrations pendentes para ContextImobiliaria...");
                var dbImobiliaria = services.GetRequiredService<ContextImobiliaria>();
                dbImobiliaria.Database.Migrate();

                Log.Information("Todas as migrations de módulos foram aplicadas com sucesso no PostgreSQL!");

                // Semeia os catálogos fiscais nacionais (CFOP + CST IBS/CBS) — pré-requisito para emitir nota.
                // Idempotente: só insere o que falta. NCM/CEST dependem de fonte externa (migração de dados).
                Log.Information("Semeando catálogos fiscais (CFOP/CST IBS-CBS)...");
                await Epros.Modules.Fiscal.Infrastructure.Data.CatalogoFiscalSeeder.SeedAsync(dbFiscal);
                Log.Information("Catálogos fiscais semeados.");

                // 1.09 — catálogo AUTORITATIVO de permissões RBAC: descobre os [AbacAuthorize] dos controllers
                // e materializa as Capacidades (system) + papel de sistema Administrador (todas as caps).
                // Idempotente. É a fonte que o AbacFilter cobra (LC-2) e conserta o admin travado (LC-1).
                Log.Information("Semeando catálogo de permissões RBAC (Capacidade/Papel Administrador)...");
                await Epros.API.Seed.CapacidadeCatalogoSeeder.SeedAsync(dbClientes);
                Log.Information("Catálogo de permissões RBAC semeado.");

                // 1.10 — catálogo de MENU real com CapacidadeRequerida amarrada às capacidades descobertas,
                // para que GET /api/v1/menu projete o menu dinâmico (em vez do fallback estático). Idempotente.
                Log.Information("Semeando catálogo de MENU (capacidades por item)...");
                await Epros.API.Seed.MenuCatalogoSeeder.SeedAsync(dbClientes);
                Log.Information("Catálogo de MENU semeado.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Falha crítica ao aplicar migrations na inicialização.");
            }
        }
    }

    // Seed de VALIDAÇÃO ponta a ponta (multi-tenant / contador parceiro + isolamento).
    // Gating DURO:
    //  - Roda SÓ em Development/Staging; FAIL-CLOSED em Production (e em qualquer outro ambiente,
    //    inclusive "Testing" do host de testes) — a condição exige explicitamente Dev OU Staging.
    //  - Guardado atrás da config "Seed:Validacao": default LIGADO em Development, DESLIGADO caso
    //    contrário (em Staging é preciso setar Seed:Validacao=true para ligar). Setar =false desliga.
    //  - Executa DEPOIS das migrations e do CatalogoFiscalSeeder, no mesmo ponto de bootstrap.
    //  - Idempotente e envolto em try/catch: nunca derruba o boot.
    {
        var ambienteElegivel = app.Environment.IsDevelopment() || app.Environment.IsStaging();
        var seedValidacaoLigado = app.Configuration.GetValue<bool?>("Seed:Validacao") ?? app.Environment.IsDevelopment();

        if (ambienteElegivel && !app.Environment.IsProduction() && seedValidacaoLigado)
        {
            using var scope = app.Services.CreateScope();
            try
            {
                await Epros.API.Seed.SeedValidacaoSeeder.SeedAsync(scope.ServiceProvider, app.Environment.EnvironmentName);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Falha ao executar o seed de validação no bootstrap (boot continua).");
            }
        }
    }

    // Inicializa o Cofre de Segredos (Vault/Local)
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var cofreService = scope.ServiceProvider.GetRequiredService<ISegredoCofreService>();
            if (cofreService is Epros.Infrastructure.Services.VaultEncryptionService vaultService)
            {
                Log.Information("Inicializando o Cofre de Segredos...");
                await vaultService.InicializarCofreAsync();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Falha ao inicializar o cofre de segredos no bootstrap.");
        }
    }

    // Rota padrão do Swagger em desenvolvimento
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // ORDEM OBRIGATÓRIA DO PIPELINE HTTP:
    // UseAuthentication -> ExcecaoGlobalMiddleware -> HostGuardMiddleware -> InquilinoSaaSMiddleware -> ModuloTenantMiddleware -> DataMaskingMiddleware -> AuditMiddleware -> Controllers

    app.UseCors(app.Environment.IsDevelopment() ? "DevCorsPolicy" : "ProdCorsPolicy");
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseMiddleware<ExcecaoGlobalMiddleware>();
    // Host-guard do Landlord (defesa em profundidade sobre o gate 1.11): rotas do painel Siser só
    // respondem no host do Landlord; num host de cliente devolvem 404. Fail-safe se Hosts:Landlord
    // não estiver configurado (dev/test). DEPOIS do roteamento, ANTES dos controllers.
    app.UseMiddleware<HostGuardMiddleware>();
    app.UseMiddleware<ApiKeyMiddleware>();
    app.UseMiddleware<InquilinoSaaSMiddleware>();
    app.UseMiddleware<BloqueioInadimplenciaMiddleware>();
    app.UseMiddleware<ModuloTenantMiddleware>();
    app.UseMiddleware<DataMaskingMiddleware>();
    app.UseMiddleware<AuditMiddleware>();

    app.MapHealthChecks("/health").AllowAnonymous();
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not Microsoft.Extensions.Hosting.HostAbortedException)
{
    Log.Fatal(ex, "A API falhou na inicialização.");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }

