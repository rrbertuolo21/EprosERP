using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.ESG.Application.Commands;
using Epros.Modules.ESG.Domain.Entities;
using Epros.Modules.ESG.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.ESG.Application.Handlers
{
    public class CriarInventarioGeeCommandHandler : ICommandHandler<CriarInventarioGeeCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarInventarioGeeCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarInventarioGeeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var jaExiste = await _context.InventariosGee
                .AnyAsync(i => i.Codigo == request.Codigo && i.Versao == request.Versao, cancellationToken);
            if (jaExiste)
                return CommandResult.Falha($"Ja existe inventario GEE com codigo {request.Codigo} versao {request.Versao}.");

            var inventario = new InventarioGee(
                request.Codigo, request.Versao, request.PeriodoInicio, request.PeriodoFim,
                request.CriterioFronteira, request.Metodologia, request.ResponsavelId, request.Observacao,
                tenantId, usuario);

            if (!inventario.IsValid)
                return CommandResult.Falha(inventario.Notifications.Select(n => n.Message));

            _context.InventariosGee.Add(inventario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Inventario GEE criado com sucesso!", new { inventario.Id });
        }
    }

    public class AdicionarFonteEmissaoGeeCommandHandler : ICommandHandler<AdicionarFonteEmissaoGeeCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarFonteEmissaoGeeCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarFonteEmissaoGeeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var inventario = await _context.InventariosGee.FirstOrDefaultAsync(i => i.Id == request.InventarioId, cancellationToken);
            if (inventario == null)
                return CommandResult.Falha("Inventario GEE nao encontrado.");

            var fonte = new FonteEmissaoGee(
                request.InventarioId, request.Codigo, request.Descricao, request.Escopo,
                request.Categoria, request.UnidadeOrganizacionalId, tenantId, usuario);

            if (!fonte.IsValid)
                return CommandResult.Falha(fonte.Notifications.Select(n => n.Message));

            _context.FontesEmissaoGee.Add(fonte);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Fonte de emissao adicionada!", new { fonte.Id });
        }
    }

    public class RegistrarDadoAtividadeGeeCommandHandler : ICommandHandler<RegistrarDadoAtividadeGeeCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarDadoAtividadeGeeCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarDadoAtividadeGeeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var fonte = await _context.FontesEmissaoGee.AnyAsync(f => f.Id == request.FonteEmissaoId, cancellationToken);
            if (!fonte)
                return CommandResult.Falha("Fonte de emissao nao encontrada.");

            var dado = new DadoAtividadeGee(
                request.FonteEmissaoId, request.DataReferencia, request.Quantidade, request.Unidade,
                request.OrigemDado, request.ReferenciaOperacional, tenantId, usuario);

            if (!dado.IsValid)
                return CommandResult.Falha(dado.Notifications.Select(n => n.Message));

            _context.DadosAtividadeGee.Add(dado);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Dado de atividade registrado!", new { dado.Id });
        }
    }

    public class CriarFatorEmissaoGeeCommandHandler : ICommandHandler<CriarFatorEmissaoGeeCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarFatorEmissaoGeeCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarFatorEmissaoGeeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var jaExiste = await _context.FatoresEmissaoGee
                .AnyAsync(f => f.Codigo == request.Codigo && f.Versao == request.Versao, cancellationToken);
            if (jaExiste)
                return CommandResult.Falha($"Ja existe fator {request.Codigo} versao {request.Versao}.");

            var fator = new FatorEmissaoGee(
                request.Codigo, request.Versao, request.FonteReferencia, request.Valor,
                request.UnidadeEntrada, request.UnidadeSaida, request.InicioVigencia, request.FimVigencia,
                tenantId, usuario);

            if (!fator.IsValid)
                return CommandResult.Falha(fator.Notifications.Select(n => n.Message));

            _context.FatoresEmissaoGee.Add(fator);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Fator de emissao criado!", new { fator.Id });
        }
    }

    public class CalcularEmissaoGeeCommandHandler : ICommandHandler<CalcularEmissaoGeeCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CalcularEmissaoGeeCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CalcularEmissaoGeeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var dado = await _context.DadosAtividadeGee.FirstOrDefaultAsync(d => d.Id == request.DadoAtividadeId, cancellationToken);
            if (dado == null)
                return CommandResult.Falha("Dado de atividade nao encontrado.");

            var fator = await _context.FatoresEmissaoGee.FirstOrDefaultAsync(f => f.Id == request.FatorEmissaoId, cancellationToken);
            if (fator == null)
                return CommandResult.Falha("Fator de emissao nao encontrado.");

            // RN-GHG: fator deve estar vigente na data de referencia do dado.
            if (!fator.VigenteEm(dado.DataReferencia))
                return CommandResult.Falha("O fator de emissao nao esta vigente na data de referencia do dado de atividade.");

            var calculo = new CalculoGee(dado.Id, fator.Id, request.FormulaVersao, dado.Quantidade, fator.Valor, tenantId, usuario);
            if (!calculo.IsValid)
                return CommandResult.Falha(calculo.Notifications.Select(n => n.Message));

            _context.CalculosGee.Add(calculo);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Emissao calculada!", new { calculo.Id, calculo.ResultadoCO2e });
        }
    }

    public class SubmeterInventarioGeeCommandHandler : ICommandHandler<SubmeterInventarioGeeCommand>
    {
        private readonly ContextESG _context;
        private readonly ICurrentUser _currentUser;

        public SubmeterInventarioGeeCommandHandler(ContextESG context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(SubmeterInventarioGeeCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var inventario = await _context.InventariosGee.FirstOrDefaultAsync(i => i.Id == request.InventarioId, cancellationToken);
            if (inventario == null)
                return CommandResult.Falha("Inventario GEE nao encontrado.");

            inventario.Submeter(usuario);
            if (!inventario.IsValid)
                return CommandResult.Falha(inventario.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Inventario submetido para analise.", new { inventario.Id, Status = inventario.Status.ToString() });
        }
    }

    public class AprovarInventarioGeeCommandHandler : ICommandHandler<AprovarInventarioGeeCommand>
    {
        private readonly ContextESG _context;
        private readonly ICurrentUser _currentUser;

        public AprovarInventarioGeeCommandHandler(ContextESG context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AprovarInventarioGeeCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var inventario = await _context.InventariosGee.FirstOrDefaultAsync(i => i.Id == request.InventarioId, cancellationToken);
            if (inventario == null)
                return CommandResult.Falha("Inventario GEE nao encontrado.");

            inventario.Aprovar(usuario);
            if (!inventario.IsValid)
                return CommandResult.Falha(inventario.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Inventario aprovado.", new { inventario.Id, Status = inventario.Status.ToString() });
        }
    }

    public class ConsolidarInventarioGeeCommandHandler : ICommandHandler<ConsolidarInventarioGeeCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ConsolidarInventarioGeeCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ConsolidarInventarioGeeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var inventario = await _context.InventariosGee.FirstOrDefaultAsync(i => i.Id == request.InventarioId, cancellationToken);
            if (inventario == null)
                return CommandResult.Falha("Inventario GEE nao encontrado.");

            // Total por escopo: fontes -> dados -> calculos.
            var fontes = await _context.FontesEmissaoGee
                .Where(f => f.InventarioId == inventario.Id)
                .Select(f => new { f.Id, f.Escopo })
                .ToListAsync(cancellationToken);
            var fonteIds = fontes.Select(f => f.Id).ToList();

            var dados = await _context.DadosAtividadeGee
                .Where(d => fonteIds.Contains(d.FonteEmissaoId))
                .Select(d => new { d.Id, d.FonteEmissaoId })
                .ToListAsync(cancellationToken);
            var dadoIds = dados.Select(d => d.Id).ToList();

            var calculos = await _context.CalculosGee
                .Where(c => dadoIds.Contains(c.DadoAtividadeId))
                .Select(c => new { c.DadoAtividadeId, c.ResultadoCO2e })
                .ToListAsync(cancellationToken);

            decimal TotalPorEscopo(int escopo)
            {
                var fontesEscopo = fontes.Where(f => f.Escopo == escopo).Select(f => f.Id).ToHashSet();
                var dadosEscopo = dados.Where(d => fontesEscopo.Contains(d.FonteEmissaoId)).Select(d => d.Id).ToHashSet();
                return calculos.Where(c => dadosEscopo.Contains(c.DadoAtividadeId)).Sum(c => c.ResultadoCO2e);
            }

            var e1 = TotalPorEscopo(1);
            var e2 = TotalPorEscopo(2);
            var e3 = TotalPorEscopo(3);

            _context.ConsolidacoesGee.Add(new ConsolidacaoGee(inventario.Id, "Escopo1", e1, tenantId, usuario));
            _context.ConsolidacoesGee.Add(new ConsolidacaoGee(inventario.Id, "Escopo2", e2, tenantId, usuario));
            _context.ConsolidacoesGee.Add(new ConsolidacaoGee(inventario.Id, "Escopo3", e3, tenantId, usuario));
            _context.ConsolidacoesGee.Add(new ConsolidacaoGee(inventario.Id, "TotalGeral", e1 + e2 + e3, tenantId, usuario));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Inventario consolidado!", new { Escopo1 = e1, Escopo2 = e2, Escopo3 = e3, TotalGeral = e1 + e2 + e3 });
        }
    }
}
