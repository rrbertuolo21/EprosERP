using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Infrastructure.Data;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    public class ObterInstalacaoStateQueryHandler : IQueryHandler<ObterInstalacaoStateQuery, InstalacaoStateDto>
    {
        private readonly ContextAplicativo _context;

        public ObterInstalacaoStateQueryHandler(ContextAplicativo context)
        {
            _context = context;
        }

        public async Task<InstalacaoStateDto> Handle(ObterInstalacaoStateQuery request, CancellationToken cancellationToken)
        {
            var state = await _context.InstalacaoStates
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(cancellationToken);

            if (state == null)
            {
                return new InstalacaoStateDto(
                    IsCompleted: false,
                    CompletedAt: null,
                    CompletedBy: null,
                    DatabaseInitialized: false,
                    AdminCreated: false,
                    SystemSettingsSeeded: false
                );
            }

            return new InstalacaoStateDto(
                IsCompleted: state.IsCompleted,
                CompletedAt: state.CompletedAt,
                CompletedBy: state.CompletedBy,
                DatabaseInitialized: state.DatabaseInitialized,
                AdminCreated: state.AdminCreated,
                SystemSettingsSeeded: state.SystemSettingsSeeded
            );
        }
    }

    public class VerificarRequisitosQueryHandler : IQueryHandler<VerificarRequisitosQuery, RequisitosCheckResultDto>
    {
        private readonly ContextAplicativo _context;

        public VerificarRequisitosQueryHandler(ContextAplicativo context)
        {
            _context = context;
        }

        public async Task<RequisitosCheckResultDto> Handle(VerificarRequisitosQuery request, CancellationToken cancellationToken)
        {
            var itens = new List<RequisitoItemDto>();

            // 1. Verificação de Conexão com Banco de Dados
            bool dbOk = false;
            string dbDetail;
            try
            {
                dbOk = await _context.Database.CanConnectAsync(cancellationToken);
                dbDetail = dbOk ? "Conexão com PostgreSQL estabelecida com sucesso." : "Não foi possível conectar ao PostgreSQL.";
            }
            catch (Exception ex)
            {
                dbDetail = $"Erro de conexão com o banco de dados: {ex.Message}";
            }
            itens.Add(new RequisitoItemDto("Conexão com Banco de Dados", dbOk, dbDetail));

            // 2. Verificação de Permissão de Leitura/Escrita de Diretório Temporário
            bool dirOk = false;
            string dirDetail;
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "temp_install_test");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var testFile = Path.Combine(path, "test.tmp");
                await File.WriteAllTextAsync(testFile, "test", cancellationToken);
                File.Delete(testFile);
                Directory.Delete(path);
                dirOk = true;
                dirDetail = "Permissão de leitura e escrita ativa no diretório de trabalho.";
            }
            catch (Exception ex)
            {
                dirDetail = $"Falha de acesso ao diretório temporário: {ex.Message}";
            }
            itens.Add(new RequisitoItemDto("Diretório Temporário", dirOk, dirDetail));

            // 3. Verificação de Timezone do Sistema
            bool tzOk = true;
            string tzDetail = "Timezone configurado do sistema operacional é válido.";
            try
            {
                var tz = TimeZoneInfo.Local;
                tzDetail = $"Fuso horário local: {tz.Id} ({tz.DisplayName})";
            }
            catch (Exception ex)
            {
                tzOk = false;
                tzDetail = $"Erro ao obter fuso horário: {ex.Message}";
            }
            itens.Add(new RequisitoItemDto("Fuso Horário", tzOk, tzDetail));

            bool pass = dbOk && dirOk && tzOk;

            return new RequisitosCheckResultDto(pass, itens);
        }
    }

    public class ExecutarInstalacaoCommandHandler : ICommandHandler<ExecutarInstalacaoCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPasswordHasher _passwordHasher;

        public ExecutarInstalacaoCommandHandler(ContextAplicativo context, IServiceProvider serviceProvider, IPasswordHasher passwordHasher)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            _passwordHasher = passwordHasher;
        }

        public async Task<CommandResult> Handle(ExecutarInstalacaoCommand request, CancellationToken cancellationToken)
        {
            var state = await _context.InstalacaoStates
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(cancellationToken);

            if (state != null && state.IsCompleted)
            {
                return CommandResult.Falha("Instalação já concluída. Reexecução bloqueada.", "Instalação já concluída. Reexecução bloqueada.");
            }

            if (state == null)
            {
                state = new InstalacaoState("system", "system");
                _context.InstalacaoStates.Add(state);
                await _context.SaveChangesAsync(cancellationToken);
            }

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // 1. Executar migrations em todos os DbContexts do sistema que herdam de ContextBase
                var contextTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(ContextBase)))
                    .ToList();

                foreach (var type in contextTypes)
                {
                    var contextInstance = _serviceProvider.GetService(type) as ContextBase;
                    if (contextInstance != null)
                    {
                        if (contextInstance.Database.IsRelational())
                        {
                            await contextInstance.Database.MigrateAsync(cancellationToken);
                        }
                        else
                        {
                            await contextInstance.Database.EnsureCreatedAsync(cancellationToken);
                        }
                    }
                }

                state.MarcarPassoConcluido(dbInit: true, adminCreated: false, settingsSeeded: false, "system");
                await _context.SaveChangesAsync(cancellationToken);

                // 2. Semear configurações globais padrão
                var defaultSettings = new List<SystemSetting>
                {
                    new SystemSetting("empresa_siser", "Siser Holding S.A.", "global", false, "system", "system"),
                    new SystemSetting("dominio_base", "epros.local", "global", false, "system", "system"),
                    new SystemSetting("timezone_global", "America/Sao_Paulo", "global", false, "system", "system"),
                    new SystemSetting("periodo_tolerancia", "15", "global", false, "system", "system")
                };

                foreach (var setting in defaultSettings)
                {
                    var exists = await _context.SystemSettings
                        .IgnoreQueryFilters()
                        .AnyAsync(s => s.Chave == setting.Chave && s.Escopo == setting.Escopo, cancellationToken);
                    if (!exists)
                    {
                        _context.SystemSettings.Add(setting);
                    }
                }

                state.MarcarPassoConcluido(dbInit: true, adminCreated: false, settingsSeeded: true, "system");
                await _context.SaveChangesAsync(cancellationToken);

                // 3. Criar usuário administrador inicial
                var hasAdmin = await _context.UsuariosInternos
                    .IgnoreQueryFilters()
                    .AnyAsync(u => u.Email == request.AdminEmail, cancellationToken);

                if (!hasAdmin)
                {
                    if (string.IsNullOrWhiteSpace(request.AdminSenha) || request.AdminSenha.Length < 8)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return CommandResult.Falha("A senha do administrador deve ter no mínimo 8 caracteres.");
                    }

                    var admin = new UsuarioInterno(
                        nome: request.AdminNome,
                        email: request.AdminEmail,
                        senha: _passwordHasher.Hash(request.AdminSenha), // Senha derivada via PBKDF2 — nunca em texto puro
                        creatorId: null,
                        uniqueId: "admin-siser",
                        timezone: "America/Sao_Paulo",
                        primaryAdmin: true,
                        tenantId: "system",
                        criadoPor: "system"
                    );
                    _context.UsuariosInternos.Add(admin);
                }

                state.MarcarPassoConcluido(dbInit: true, adminCreated: true, settingsSeeded: true, "system");
                await _context.SaveChangesAsync(cancellationToken);

                // 4. Finalizar e concluir instalação
                state.Concluir("system");
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                return CommandResult.Ok("Instalação do EprosERP concluída com sucesso!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return CommandResult.Falha($"Falha na instalação: {ex.Message}");
            }
        }
    }

    public class ExecutarAtualizacaoCommandHandler : ICommandHandler<ExecutarAtualizacaoCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICurrentUser _currentUser;
        private readonly ITenantProvider _tenantProvider;

        public ExecutarAtualizacaoCommandHandler(
            ContextAplicativo context,
            IServiceProvider serviceProvider,
            ICurrentUser currentUser,
            ITenantProvider tenantProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            _currentUser = currentUser;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ExecutarAtualizacaoCommand request, CancellationToken cancellationToken)
        {
            // 1.11 fix #3 — atualização roda MigrateAsync em todos os contextos: exige operador interno.
            if (!string.Equals(_tenantProvider.GetTenantId(), "system", StringComparison.OrdinalIgnoreCase))
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: a atualização do sistema é restrita ao operador interno da Siser." });
            }

            var executadoPor = _currentUser.GetUserId() ?? "system";

            try
            {
                // Executar migrations em todos os DbContexts do sistema
                var contextTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(ContextBase)))
                    .ToList();

                foreach (var type in contextTypes)
                {
                    var contextInstance = _serviceProvider.GetService(type) as ContextBase;
                    if (contextInstance != null)
                    {
                        if (contextInstance.Database.IsRelational())
                        {
                            await contextInstance.Database.MigrateAsync(cancellationToken);
                        }
                        else
                        {
                            await contextInstance.Database.EnsureCreatedAsync(cancellationToken);
                        }
                    }
                }

                var updateLog = new UpdateLog(
                    versaoAlvo: request.VersaoAlvo,
                    executadoPor: executadoPor,
                    sucesso: true,
                    log: "Atualizações do banco de dados e migrações aplicadas com sucesso.",
                    tenantId: "system",
                    criadoPor: executadoPor
                );
                _context.UpdateLogs.Add(updateLog);
                await _context.SaveChangesAsync(cancellationToken);

                return CommandResult.Ok("Atualização aplicada com sucesso!");
            }
            catch (Exception ex)
            {
                var updateLog = new UpdateLog(
                    versaoAlvo: request.VersaoAlvo,
                    executadoPor: executadoPor,
                    sucesso: false,
                    log: $"Erro crítico durante aplicação do update: {ex.ToString()}",
                    tenantId: "system",
                    criadoPor: executadoPor
                );
                _context.UpdateLogs.Add(updateLog);
                await _context.SaveChangesAsync(cancellationToken);

                return CommandResult.Falha($"Erro ao aplicar atualização: {ex.Message}");
            }
        }
    }

    public class ListarUpdateLogsQueryHandler : IQueryHandler<ListarUpdateLogsQuery, IEnumerable<UpdateLogDto>>
    {
        private readonly ContextAplicativo _context;

        public ListarUpdateLogsQueryHandler(ContextAplicativo context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UpdateLogDto>> Handle(ListarUpdateLogsQuery request, CancellationToken cancellationToken)
        {
            var logs = await _context.UpdateLogs
                .AsNoTracking()
                .IgnoreQueryFilters()
                .OrderByDescending(l => l.ExecutadoEm)
                .Select(l => new UpdateLogDto(
                    l.Id,
                    l.VersaoAlvo,
                    l.ExecutadoEm,
                    l.ExecutadoPor,
                    l.Sucesso,
                    l.Log
                ))
                .ToListAsync(cancellationToken);

            return logs;
        }
    }
}
