using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Contracts;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Define Configuracao Global.</summary>
    public class DefinirConfiguracaoGlobalCommandHandler : IRequestHandler<DefinirConfiguracaoGlobalCommand, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IConfiguracaoGlobalCache _cache;
        private readonly ISegredoCofreService _cofreService;

        public DefinirConfiguracaoGlobalCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IConfiguracaoGlobalCache cache,
            ISegredoCofreService cofreService)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _cache = cache;
            _cofreService = cofreService;
        }

        public async Task<CommandResult> Handle(DefinirConfiguracaoGlobalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            // Apenas o tenant "system" pode cadastrar/alterar configurações globais
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Apenas o tenant do sistema pode gerenciar configurações globais." });
            }

            // Busca se já existe com a mesma chave
            var configExistente = await _context.ConfiguracoesGlobais
                .FirstOrDefaultAsync(c => c.Chave == request.Chave, cancellationToken);

            var valorASalvar = request.Valor;
            if (request.EhSegredo)
            {
                valorASalvar = await _cofreService.CriptografarAsync(request.Valor);
            }

            if (configExistente != null)
            {
                // Se a configuração existente ou a nova solicitação indicar segredo
                var ehSegredo = configExistente.EhSegredo || request.EhSegredo;
                if (ehSegredo)
                {
                    valorASalvar = await _cofreService.CriptografarAsync(request.Valor);
                }

                configExistente.Atualizar(valorASalvar, userId);

                // Se EhSegredo mudou de false para true, atualiza o campo via reflexão
                if (request.EhSegredo && !configExistente.EhSegredo)
                {
                    var propEhSegredo = typeof(ConfiguracaoGlobal).GetProperty(nameof(ConfiguracaoGlobal.EhSegredo));
                    propEhSegredo?.SetValue(configExistente, true);
                }

                if (!configExistente.IsValid)
                {
                    return CommandResult.Falha(configExistente.Notifications.Select(n => n.Message));
                }

                _context.ConfiguracoesGlobais.Update(configExistente);
            }
            else
            {
                var novaConfig = new ConfiguracaoGlobal(request.Chave, valorASalvar, request.EhSegredo, request.Descricao, tenantId, userId);
                if (!novaConfig.IsValid)
                {
                    return CommandResult.Falha(novaConfig.Notifications.Select(n => n.Message));
                }

                _context.ConfiguracoesGlobais.Add(novaConfig);
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Invalida o cache L1/L2 distribuído
            await _cache.InvalidarAsync(request.Chave);

            return CommandResult.Ok("Configuração global definida com sucesso.");
        }
    }
}
// 
