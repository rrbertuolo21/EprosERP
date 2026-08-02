using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GRC.Application.Commands;
using Epros.Modules.GRC.Domain.Entities;
using Epros.Modules.GRC.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GRC.Application.Handlers
{
    /// <summary>D-REG-01 — cadastra certificado completo com referências de cofre (segredo nunca em claro).</summary>
    public class CadastrarCertificadoCompletoCommandHandler : ICommandHandler<CadastrarCertificadoCompletoCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CadastrarCertificadoCompletoCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CadastrarCertificadoCompletoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var cert = new CertificadoDigital(
                request.EmpresaId, request.Cnpj, request.Serial, request.Tipo, request.Origem, request.ValidadeFim, tenantId, usuario);
            if (!cert.IsValid)
                return CommandResult.Falha(cert.Notifications.Select(n => n.Message));

            cert.DefinirDetalhes(request.Nome, request.ResumoCertificado, request.ValidadeInicio, request.ValidadeFim, usuario);
            cert.VincularCofre(request.SenhaReferenciaSegura, request.CaminhoReferenciaSegura, usuario);
            if (!cert.IsValid)
                return CommandResult.Falha(cert.Notifications.Select(n => n.Message));

            _context.CertificadosDigitais.Add(cert);
            await _context.SaveChangesAsync(cancellationToken);

            // Nunca retornar o segredo: só metadados.
            return CommandResult.Ok("Certificado cadastrado com referencias de cofre.",
                new { CertificadoId = cert.Id, cert.Status, cert.ValidadeInicio, cert.ValidadeFim });
        }
    }

    /// <summary>D-REG-04 — validação local determinística; persiste a validação e ativa se válido.</summary>
    public class ValidarCertificadoLocalCommandHandler : ICommandHandler<ValidarCertificadoLocalCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ValidarCertificadoLocalCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ValidarCertificadoLocalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var cert = await _context.CertificadosDigitais.FirstOrDefaultAsync(c => c.Id == request.CertificadoId, cancellationToken);
            if (cert == null)
                return CommandResult.Falha("Certificado digital nao encontrado.");

            var falhas = cert.ValidarLocal(request.EmpresaExiste, request.CnpjEmpresa, DateTime.UtcNow);
            var resultado = falhas.Count == 0 ? "Sucesso" : "Falha";
            var mensagem = falhas.Count == 0 ? "Validacao local aprovada." : string.Join(" ", falhas);

            var validacao = new ValidacaoCertificado(cert.Id, resultado, mensagem, tenantId, usuario);
            _context.ValidacoesCertificado.Add(validacao);

            if (resultado == "Sucesso" && cert.Status == "Rascunho")
                cert.Validar(usuario);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok($"Validacao local: {resultado}.",
                new { cert.Id, Resultado = resultado, Falhas = falhas, cert.Status });
        }
    }

    /// <summary>D-REG-05 — define nome/framework/prazo da obrigação.</summary>
    public class DefinirObrigacaoRegistroCommandHandler : ICommandHandler<DefinirObrigacaoRegistroCommand>
    {
        private readonly ContextGRC _context;
        private readonly ICurrentUser _currentUser;

        public DefinirObrigacaoRegistroCommandHandler(ContextGRC context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DefinirObrigacaoRegistroCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var registro = await _context.RegistrosRegulatorios.FirstOrDefaultAsync(r => r.Id == request.RegistroId, cancellationToken);
            if (registro == null)
                return CommandResult.Falha("Registro regulatorio nao encontrado.");

            registro.DefinirObrigacao(request.Nome, request.Framework, request.DataInicio, request.DataVencimento, usuario);
            if (!registro.IsValid)
                return CommandResult.Falha(registro.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Obrigacao regulatoria definida.",
                new { registro.Id, registro.Framework, registro.DataVencimento });
        }
    }

    /// <summary>D-REG-02 — cria vencimento com janelas de alerta.</summary>
    public class CriarVencimentoRegulatorioCommandHandler : ICommandHandler<CriarVencimentoRegulatorioCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarVencimentoRegulatorioCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarVencimentoRegulatorioCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var calendario = new CalendarioRegulatorio(
                request.CertificadoId, request.RegistroId, request.Descricao, request.DataVencimento, tenantId, usuario);
            if (!calendario.IsValid)
                return CommandResult.Falha(calendario.Notifications.Select(n => n.Message));

            calendario.ConfigurarAlertas(request.DiasAlerta, request.ResponsavelId, request.Tratamento, usuario);

            _context.CalendariosRegulatorios.Add(calendario);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Vencimento regulatorio criado.",
                new { CalendarioId = calendario.Id, calendario.DiasAlerta, calendario.DataVencimento });
        }
    }

    /// <summary>D-REG-02 — rotina de alerta: marca 'Alertado' e emite evento para os que caem na janela.</summary>
    public class AvaliarVencimentosRegulatoriosCommandHandler : ICommandHandler<AvaliarVencimentosRegulatoriosCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AvaliarVencimentosRegulatoriosCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AvaliarVencimentosRegulatoriosCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var agora = DateTime.UtcNow;

            var pendentes = await _context.CalendariosRegulatorios
                .Where(c => c.Status == "Pendente")
                .ToListAsync(cancellationToken);

            var alertados = 0;
            foreach (var calendario in pendentes.Where(c => c.DeveAlertar(agora)))
            {
                calendario.Alertar(usuario);
                _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                    CatalogoEventosIntegracao.Grc.RegCertificadoAlertaVencimento,
                    JsonSerializer.Serialize(new { calendario.Id, calendario.CertificadoId, calendario.RegistroId, calendario.DataVencimento })));
                alertados++;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok($"{alertados} vencimento(s) alertado(s).", new { Alertados = alertados });
        }
    }
}
