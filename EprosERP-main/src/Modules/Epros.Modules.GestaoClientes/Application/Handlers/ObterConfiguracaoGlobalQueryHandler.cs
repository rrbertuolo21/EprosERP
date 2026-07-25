using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Contracts;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Obtém Configuracao Global.</summary>
    public class ObterConfiguracaoGlobalQueryHandler : IRequestHandler<ObterConfiguracaoGlobalQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IConfiguracaoGlobalCache _cache;
        private readonly ISegredoCofreService _cofreService;

        public ObterConfiguracaoGlobalQueryHandler(
            ContextGestaoClientes context, 
            ITenantProvider tenantProvider,
            IConfiguracaoGlobalCache cache,
            ISegredoCofreService cofreService)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _cache = cache;
            _cofreService = cofreService;
        }

        public async Task<CommandResult> Handle(ObterConfiguracaoGlobalQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            // Apenas o tenant "system" pode ler configurações globais diretamente
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Apenas o tenant do sistema pode ler configurações globais." });
            }

            var configDto = await _cache.ObterAsync(request.Chave, async () =>
            {
                return await _context.ConfiguracoesGlobais
                    .FirstOrDefaultAsync(c => c.Chave == request.Chave, cancellationToken);
            });

            if (configDto == null)
            {
                return CommandResult.Falha(new[] { "Configuração global não encontrada." });
            }

            var valorRetorno = configDto.Valor;
            if (configDto.EhSegredo)
            {
                valorRetorno = await _cofreService.DescriptografarAsync(configDto.Valor);
            }

            // Remonta a entidade ConfiguracaoGlobal a partir do DTO para garantir compatibilidade com consumidores existentes
            var config = new ConfiguracaoGlobal(
                configDto.Chave,
                valorRetorno,
                configDto.EhSegredo,
                configDto.Descricao,
                configDto.TenantId,
                "system"
            );

            // Define o ID correto da entidade original via reflexão na classe base EntidadeSaaSBase
            var backingFieldId = typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase)
                .GetField("<Id>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            backingFieldId?.SetValue(config, configDto.Id);

            return CommandResult.Ok("Configuração recuperada com sucesso.", config);
        }
    }
}
