using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GRC.Application.Queries;
using Epros.Modules.GRC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GRC.Application.Handlers
{
    public class ObterPoliticasQueryHandler : IQueryHandler<ObterPoliticasQuery, CommandResult>
    {
        private readonly ContextGRC _context;
        public ObterPoliticasQueryHandler(ContextGRC context) => _context = context;

        public async Task<CommandResult> Handle(ObterPoliticasQuery request, CancellationToken cancellationToken)
        {
            var politicas = await _context.Politicas.OrderBy(p => p.Codigo).ToListAsync(cancellationToken);
            return CommandResult.Ok("Politicas listadas com sucesso!", politicas);
        }
    }

    public class ObterAceitesPoliticaQueryHandler : IQueryHandler<ObterAceitesPoliticaQuery, CommandResult>
    {
        private readonly ContextGRC _context;
        public ObterAceitesPoliticaQueryHandler(ContextGRC context) => _context = context;

        public async Task<CommandResult> Handle(ObterAceitesPoliticaQuery request, CancellationToken cancellationToken)
        {
            var aceites = await _context.PoliticaAceites
                .Where(a => a.PoliticaId == request.PoliticaId)
                .OrderByDescending(a => a.DataHoraAceite)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Aceites listados com sucesso!", aceites);
        }
    }

    public class ObterRegistrosRegulatoriosQueryHandler : IQueryHandler<ObterRegistrosRegulatoriosQuery, CommandResult>
    {
        private readonly ContextGRC _context;
        public ObterRegistrosRegulatoriosQueryHandler(ContextGRC context) => _context = context;

        public async Task<CommandResult> Handle(ObterRegistrosRegulatoriosQuery request, CancellationToken cancellationToken)
        {
            var registros = await _context.RegistrosRegulatorios.OrderBy(r => r.Codigo).ToListAsync(cancellationToken);
            return CommandResult.Ok("Registros regulatorios listados com sucesso!", registros);
        }
    }

    public class ObterCertificadosDigitaisQueryHandler : IQueryHandler<ObterCertificadosDigitaisQuery, CommandResult>
    {
        private readonly ContextGRC _context;
        public ObterCertificadosDigitaisQueryHandler(ContextGRC context) => _context = context;

        public async Task<CommandResult> Handle(ObterCertificadosDigitaisQuery request, CancellationToken cancellationToken)
        {
            var certificados = await _context.CertificadosDigitais.OrderBy(c => c.DataValidade).ToListAsync(cancellationToken);
            return CommandResult.Ok("Certificados digitais listados com sucesso!", certificados);
        }
    }

    public class ObterAvaliacoesRiscoQueryHandler : IQueryHandler<ObterAvaliacoesRiscoQuery, CommandResult>
    {
        private readonly ContextGRC _context;
        public ObterAvaliacoesRiscoQueryHandler(ContextGRC context) => _context = context;

        public async Task<CommandResult> Handle(ObterAvaliacoesRiscoQuery request, CancellationToken cancellationToken)
        {
            var avaliacoes = await _context.AvaliacoesRisco
                .Where(a => a.RiscoId == request.RiscoId)
                .OrderByDescending(a => a.DataAvaliacao)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Avaliacoes de risco listadas com sucesso!", avaliacoes);
        }
    }

    public class ObterPlanosAuditoriaQueryHandler : IQueryHandler<ObterPlanosAuditoriaQuery, CommandResult>
    {
        private readonly ContextGRC _context;
        public ObterPlanosAuditoriaQueryHandler(ContextGRC context) => _context = context;

        public async Task<CommandResult> Handle(ObterPlanosAuditoriaQuery request, CancellationToken cancellationToken)
        {
            var planos = await _context.PlanosAuditoria.OrderBy(p => p.Codigo).ToListAsync(cancellationToken);
            return CommandResult.Ok("Planos de auditoria listados com sucesso!", planos);
        }
    }

    public class ObterAchadosQueryHandler : IQueryHandler<ObterAchadosQuery, CommandResult>
    {
        private readonly ContextGRC _context;
        public ObterAchadosQueryHandler(ContextGRC context) => _context = context;

        public async Task<CommandResult> Handle(ObterAchadosQuery request, CancellationToken cancellationToken)
        {
            var achados = await _context.Achados.OrderByDescending(a => a.CriadoEm).ToListAsync(cancellationToken);
            return CommandResult.Ok("Achados listados com sucesso!", achados);
        }
    }

    public class ObterRegrasSoDQueryHandler : IQueryHandler<ObterRegrasSoDQuery, CommandResult>
    {
        private readonly ContextGRC _context;
        public ObterRegrasSoDQueryHandler(ContextGRC context) => _context = context;

        public async Task<CommandResult> Handle(ObterRegrasSoDQuery request, CancellationToken cancellationToken)
        {
            var regras = await _context.RegrasSoD.OrderByDescending(r => r.Criticidade).ToListAsync(cancellationToken);
            return CommandResult.Ok("Regras SoD listadas com sucesso!", regras);
        }
    }

    public class ObterViolacoesSoDQueryHandler : IQueryHandler<ObterViolacoesSoDQuery, CommandResult>
    {
        private readonly ContextGRC _context;
        public ObterViolacoesSoDQueryHandler(ContextGRC context) => _context = context;

        public async Task<CommandResult> Handle(ObterViolacoesSoDQuery request, CancellationToken cancellationToken)
        {
            var violacoes = await _context.ViolacoesSoD
                .OrderByDescending(v => v.DataDeteccao)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Violacoes SoD listadas com sucesso!", violacoes);
        }
    }
}
