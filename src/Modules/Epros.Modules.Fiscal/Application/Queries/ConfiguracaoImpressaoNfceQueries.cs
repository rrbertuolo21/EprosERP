using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Queries
{
    public record ObterConfiguracaoImpressaoNfcePorEmpresaQuery(Guid EmpresaId) : IQuery<CommandResult>;
    public record ObterConfiguracaoImpressaoNfcePorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ObterConfiguracaoImpressaoNfcePorEmpresaQueryHandler : IRequestHandler<ObterConfiguracaoImpressaoNfcePorEmpresaQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ObterConfiguracaoImpressaoNfcePorEmpresaQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ObterConfiguracaoImpressaoNfcePorEmpresaQuery request, CancellationToken cancellationToken)
        {
            var c = await _context.ConfiguracoesImpressaoNfce.AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmpresaId == request.EmpresaId && x.DeletadoEm == null, cancellationToken);
            if (c == null) return CommandResult.Falha("Configuração de Impressão NFCe não encontrada.");
            return CommandResult.Ok("OK", ProjetarDto(c));
        }

        internal static object ProjetarDto(Domain.Entities.ConfiguracaoImpressaoNfce c) => new
        {
            c.Id,
            c.EmpresaId,
            c.DetalheVendaNormal,
            c.DetalheVendaContingencia,
            c.ImprimeDescontoItem,
            c.ImprimeFoneEmitente,
            c.MargemEsquerda,
            c.MargemDireita,
            c.ModoImpressao,
            c.NfceLayoutQrCode,
            c.VersaoQrCode,
            c.SegundaViaContingencia
        };
    }

    public class ObterConfiguracaoImpressaoNfcePorIdQueryHandler : IRequestHandler<ObterConfiguracaoImpressaoNfcePorIdQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ObterConfiguracaoImpressaoNfcePorIdQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ObterConfiguracaoImpressaoNfcePorIdQuery request, CancellationToken cancellationToken)
        {
            var c = await _context.ConfiguracoesImpressaoNfce.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (c == null) return CommandResult.Falha("Configuração de Impressão NFCe não encontrada.");
            return CommandResult.Ok("OK", ObterConfiguracaoImpressaoNfcePorEmpresaQueryHandler.ProjetarDto(c));
        }
    }
}
