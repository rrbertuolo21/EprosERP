using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GRC.Application.Commands;
using Epros.Modules.GRC.Domain.Entities;
using Epros.Modules.GRC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GRC.Application.Handlers
{
    public class CriarFuncaoSoDCommandHandler : ICommandHandler<CriarFuncaoSoDCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarFuncaoSoDCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarFuncaoSoDCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var duplicado = await _context.FuncoesSoD.AnyAsync(f => f.Codigo == request.Codigo, cancellationToken);
            if (duplicado)
            {
                return CommandResult.Falha("Ja existe funcao SoD com este codigo para a empresa/tenant.");
            }

            var funcao = new FuncaoSoD(request.Codigo, request.Nome, request.Descricao, tenantId, usuario);
            if (!funcao.IsValid)
            {
                return CommandResult.Falha(funcao.Notifications.Select(n => n.Message));
            }

            _context.FuncoesSoD.Add(funcao);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Funcao SoD criada com sucesso!", new { FuncaoId = funcao.Id });
        }
    }

    public class CriarRegraSoDCommandHandler : ICommandHandler<CriarRegraSoDCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarRegraSoDCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarRegraSoDCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var funcoesValidas = await _context.FuncoesSoD
                .CountAsync(f => f.Id == request.FuncaoAId || f.Id == request.FuncaoBId, cancellationToken);
            if (funcoesValidas < 2)
            {
                return CommandResult.Falha("As funcoes A e B da regra devem existir e ser distintas.");
            }

            var regra = new RegraSoD(request.Codigo, request.FuncaoAId, request.FuncaoBId, request.Criticidade,
                request.VigenciaInicio, request.VigenciaFim, tenantId, usuario);
            if (!regra.IsValid)
            {
                return CommandResult.Falha(regra.Notifications.Select(n => n.Message));
            }

            _context.RegrasSoD.Add(regra);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Regra SoD criada com sucesso!", new { RegraId = regra.Id, regra.Status });
        }
    }

    public class SimularSoDCommandHandler : ICommandHandler<SimularSoDCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public SimularSoDCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(SimularSoDCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var simulacao = new SimulacaoSoD(request.PerfilId, request.UsuarioId, request.Alvo, tenantId, usuario);
            if (!simulacao.IsValid)
            {
                return CommandResult.Falha(simulacao.Notifications.Select(n => n.Message));
            }

            var funcoes = request.FuncoesAtribuidas.Distinct().ToHashSet();

            // Regras SoD ativas cujas duas funcoes estao no conjunto atribuido => conflito.
            var regrasAtivas = await _context.RegrasSoD
                .Where(r => r.Status == "Ativo")
                .ToListAsync(cancellationToken);

            var conflitos = regrasAtivas
                .Where(r => funcoes.Contains(r.FuncaoAId) && funcoes.Contains(r.FuncaoBId))
                .ToList();

            simulacao.RegistrarResultado(conflitos.Count, conflitos.Count == 0 ? "Sem conflitos" : $"{conflitos.Count} conflito(s) detectado(s)");
            _context.SimulacoesSoD.Add(simulacao);

            foreach (var regra in conflitos)
            {
                var violacao = new ViolacaoSoD(simulacao.Id, regra.Id, request.PerfilId, request.UsuarioId, tenantId, usuario);
                if (violacao.IsValid)
                {
                    _context.ViolacoesSoD.Add(violacao);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok(
                conflitos.Count == 0 ? "Simulacao concluida: nenhum conflito SoD." : $"Simulacao concluida: {conflitos.Count} conflito(s) SoD.",
                new { SimulacaoId = simulacao.Id, QuantidadeViolacoes = conflitos.Count });
        }
    }
}
