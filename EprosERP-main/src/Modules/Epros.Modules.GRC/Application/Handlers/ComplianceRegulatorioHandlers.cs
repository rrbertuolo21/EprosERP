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
    public class RegistrarRegistroRegulatorioCommandHandler : ICommandHandler<RegistrarRegistroRegulatorioCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarRegistroRegulatorioCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarRegistroRegulatorioCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var duplicado = await _context.RegistrosRegulatorios.AnyAsync(r => r.Codigo == request.Codigo, cancellationToken);
            if (duplicado)
            {
                return CommandResult.Falha("Ja existe registro regulatorio com este codigo para a empresa/tenant.");
            }

            var registro = new RegistroRegulatorio(request.Codigo, request.Descricao, request.Norma, request.ResponsavelId, tenantId, usuario);
            if (!registro.IsValid)
            {
                return CommandResult.Falha(registro.Notifications.Select(n => n.Message));
            }

            _context.RegistrosRegulatorios.Add(registro);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Registro regulatorio criado com sucesso!", new { RegistroId = registro.Id, registro.Status });
        }
    }

    public class CadastrarCertificadoDigitalCommandHandler : ICommandHandler<CadastrarCertificadoDigitalCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CadastrarCertificadoDigitalCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CadastrarCertificadoDigitalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var certificado = new CertificadoDigital(
                request.EmpresaId, request.Cnpj, request.Serial, request.Tipo, request.Origem, request.DataValidade, tenantId, usuario);

            if (!certificado.IsValid)
            {
                return CommandResult.Falha(certificado.Notifications.Select(n => n.Message));
            }

            _context.CertificadosDigitais.Add(certificado);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Certificado digital cadastrado com sucesso!", new { CertificadoId = certificado.Id, certificado.Status });
        }
    }

    public class RegistrarValidacaoCertificadoCommandHandler : ICommandHandler<RegistrarValidacaoCertificadoCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarValidacaoCertificadoCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarValidacaoCertificadoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var certificadoExiste = await _context.CertificadosDigitais.AnyAsync(c => c.Id == request.CertificadoId, cancellationToken);
            if (!certificadoExiste)
            {
                return CommandResult.Falha("Certificado digital nao encontrado.");
            }

            var validacao = new ValidacaoCertificado(request.CertificadoId, request.Resultado, request.Mensagem, tenantId, usuario);
            if (!validacao.IsValid)
            {
                return CommandResult.Falha(validacao.Notifications.Select(n => n.Message));
            }

            _context.ValidacoesCertificado.Add(validacao);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Validacao de certificado registrada com sucesso!", new { ValidacaoId = validacao.Id, validacao.Resultado });
        }
    }
}
