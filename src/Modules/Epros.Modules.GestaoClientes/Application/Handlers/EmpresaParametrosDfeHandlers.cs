using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    internal static class ParametrosDfeMapper
    {
        public static ParametrosDfeNfe? Map(ParametrosDfeNfeDto? d) =>
            d == null ? null : new ParametrosDfeNfe(d.NfeSerieProducao, d.NfeProximoNrProducao, d.NfeSerieHomologacao, d.NfeProximoNrHomologacao, d.ValorAliquotaCreditoIcms, d.NfeGerarContingenciaEmHomologacao, d.IndicadorSt, d.EmitirNfeConjugada);

        public static ParametrosDfeNfceHomologacao? Map(ParametrosDfeNfceHomologacaoDto? d) =>
            d == null ? null : new ParametrosDfeNfceHomologacao(d.NfceCscHomologacao, d.NfceIdCscHomologacao, d.NfceSerieHomologacao, d.NfceProximoNrHomologacao, d.NfceGerarContingenciaEmHomologacao);

        public static ParametrosDfeNfceProducao? Map(ParametrosDfeNfceProducaoDto? d) =>
            d == null ? null : new ParametrosDfeNfceProducao(d.NfceCscProducao, d.NfceIdCscProducao, d.NfceSerieProducao, d.NfceProximoNrProducao);
    }

    /// <summary>Cria Empresa Parametros Dfe.</summary>
    public class CriarEmpresaParametrosDfeCommandHandler : ICommandHandler<CriarEmpresaParametrosDfeCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarEmpresaParametrosDfeCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarEmpresaParametrosDfeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.Id == request.EmpresaId && e.TenantId == tenantId, cancellationToken);
            if (empresa == null)
                return CommandResult.Falha(new[] { "Empresa não encontrada." }, "Erro");

            var jaExiste = await _context.EmpresasParametrosDfe.AnyAsync(p => p.EmpresaId == request.EmpresaId && p.TenantId == tenantId, cancellationToken);
            if (jaExiste)
                return CommandResult.Falha(new[] { "Parâmetros DF-e já cadastrados para esta empresa." }, "Erro");

            var parametros = new EmpresaParametrosDfe(
                request.EmpresaId,
                request.DestacarIcmsSt,
                ParametrosDfeMapper.Map(request.Nfe),
                ParametrosDfeMapper.Map(request.NfceHomologacao),
                ParametrosDfeMapper.Map(request.NfceProducao),
                request.TipoAmbienteNfce,
                request.TipoAmbienteNfe,
                tenantId,
                criadoPor);

            if (!parametros.IsValid)
                return CommandResult.Falha(parametros.Notifications.Select(n => n.Message).Distinct(), "Falha na validação dos parâmetros DF-e");

            _context.EmpresasParametrosDfe.Add(parametros);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Parâmetros DF-e cadastrados com sucesso!", new { EmpresaParametrosDfeId = parametros.Id });
        }
    }

    /// <summary>Atualiza Empresa Parametros Dfe.</summary>
    public class AtualizarEmpresaParametrosDfeCommandHandler : ICommandHandler<AtualizarEmpresaParametrosDfeCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtualizarEmpresaParametrosDfeCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarEmpresaParametrosDfeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var parametros = await _context.EmpresasParametrosDfe.FirstOrDefaultAsync(p => p.EmpresaId == request.EmpresaId && p.TenantId == tenantId, cancellationToken);
            if (parametros == null)
                return CommandResult.Falha(new[] { "Parâmetros DF-e não encontrados." }, "Erro");

            parametros.Alterar(
                request.DestacarIcmsSt,
                ParametrosDfeMapper.Map(request.Nfe),
                ParametrosDfeMapper.Map(request.NfceHomologacao),
                ParametrosDfeMapper.Map(request.NfceProducao),
                request.TipoAmbienteNfce,
                request.TipoAmbienteNfe,
                alteradoPor);

            if (!parametros.IsValid)
                return CommandResult.Falha(parametros.Notifications.Select(n => n.Message).Distinct(), "Falha na validação dos parâmetros DF-e");

            _context.EmpresasParametrosDfe.Update(parametros);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Parâmetros DF-e atualizados com sucesso!", new { EmpresaParametrosDfeId = parametros.Id });
        }
    }

    /// <summary>Exclui Empresa Parametros Dfe.</summary>
    public class DeletarEmpresaParametrosDfeCommandHandler : ICommandHandler<DeletarEmpresaParametrosDfeCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DeletarEmpresaParametrosDfeCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarEmpresaParametrosDfeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var deletadoPor = _currentUser.GetUserId() ?? "system";

            var parametros = await _context.EmpresasParametrosDfe.FirstOrDefaultAsync(p => p.EmpresaId == request.EmpresaId && p.TenantId == tenantId, cancellationToken);
            if (parametros == null)
                return CommandResult.Falha(new[] { "Parâmetros DF-e não encontrados." }, "Erro");

            parametros.Deletar(deletadoPor);
            _context.EmpresasParametrosDfe.Update(parametros);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Parâmetros DF-e removidos com sucesso!");
        }
    }

    /// <summary>Obtém Empresa Parametros Dfe.</summary>
    public class ObterEmpresaParametrosDfeQueryHandler : IQueryHandler<ObterEmpresaParametrosDfeQuery, EmpresaParametrosDfeDto?>
    {
        private readonly ContextGestaoClientes _context;

        public ObterEmpresaParametrosDfeQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<EmpresaParametrosDfeDto?> Handle(ObterEmpresaParametrosDfeQuery request, CancellationToken cancellationToken)
        {
            var p = await _context.EmpresasParametrosDfe.AsNoTracking().FirstOrDefaultAsync(x => x.EmpresaId == request.EmpresaId, cancellationToken);
            if (p == null) return null;

            return new EmpresaParametrosDfeDto
            {
                Id = p.Id,
                EmpresaId = p.EmpresaId,
                DestacarIcmsSt = p.DestacarIcmsSt,
                TipoAmbienteNfce = p.TipoAmbienteNfce,
                TipoAmbienteNfe = p.TipoAmbienteNfe,
                SerieNfce = p.ObterSerieNfce(),
                NumeroNfce = p.ObterNumeroNfce(),
                SerieNfe = p.ObterSerieNfe(),
                NumeroNfe = p.ObterNumeroNfe()
            };
        }
    }
}
