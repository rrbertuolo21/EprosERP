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
    /// <summary>Lista Empresas.</summary>
    public class ListarEmpresasQueryHandler : IQueryHandler<ListarEmpresasQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarEmpresasQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarEmpresasQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var empresas = await _context.Empresas
                .AsNoTracking()
                .Where(e => e.TenantId == tenantId)
                .ToListAsync(cancellationToken);

            // T-01/P1-2: nunca devolver o segredo (ciphertext) na resposta — apenas a máscara.
            foreach (var e in empresas) e.MascararTokenPixParaLeitura();

            return CommandResult.Ok("Empresas listadas com sucesso.", empresas);
        }
    }

    /// <summary>Obtém Empresa Por Id.</summary>
    public class ObterEmpresaPorIdQueryHandler : IQueryHandler<ObterEmpresaPorIdQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterEmpresaPorIdQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterEmpresaPorIdQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var empresa = await _context.Empresas
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == request.Id && e.TenantId == tenantId, cancellationToken);

            if (empresa == null)
                return CommandResult.Falha(new[] { "Empresa não encontrada" });

            empresa.MascararTokenPixParaLeitura(); // T-01/P1-2: nunca devolver o segredo cifrado.
            return CommandResult.Ok("Empresa obtida com sucesso.", empresa);
        }
    }

    /// <summary>Obtém Empresa Por Cnpj.</summary>
    public class ObterEmpresaPorCnpjQueryHandler : IQueryHandler<ObterEmpresaPorCnpjQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterEmpresaPorCnpjQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterEmpresaPorCnpjQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var empresa = await _context.Empresas
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Cnpj == request.Cnpj && e.TenantId == tenantId, cancellationToken);

            if (empresa == null)
                return CommandResult.Falha(new[] { "Empresa não encontrada" });

            empresa.MascararTokenPixParaLeitura(); // T-01/P1-2: nunca devolver o segredo cifrado.
            return CommandResult.Ok("Empresa obtida com sucesso.", empresa);
        }
    }

    /// <summary>Obtém Empresa Por Cpf.</summary>
    public class ObterEmpresaPorCpfQueryHandler : IQueryHandler<ObterEmpresaPorCpfQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterEmpresaPorCpfQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterEmpresaPorCpfQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            // Mantém o comportamento legado (busca por Cnpj com o valor informado).
            var empresa = await _context.Empresas
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Cnpj == request.Cpf && e.TenantId == tenantId, cancellationToken);

            if (empresa == null)
                return CommandResult.Falha(new[] { "Empresa não encontrada" });

            empresa.MascararTokenPixParaLeitura(); // T-01/P1-2: nunca devolver o segredo cifrado.
            return CommandResult.Ok("Empresa obtida com sucesso.", empresa);
        }
    }

    /// <summary>Altera Logo Empresa.</summary>
    public class AlterarLogoEmpresaCommandHandler : ICommandHandler<AlterarLogoEmpresaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AlterarLogoEmpresaCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AlterarLogoEmpresaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var empresa = await _context.Empresas
                .FirstOrDefaultAsync(e => e.Id == request.EmpresaId && e.TenantId == tenantId, cancellationToken);

            if (empresa == null)
                return CommandResult.Falha(new[] { "Empresa não encontrada" });

            if (!string.IsNullOrEmpty(request.Logo))
            {
                if (request.Logo.Contains("data:") && !request.Logo.Contains("base64"))
                    return CommandResult.Falha(new[] { "Os dados da imagem não estão no formato esperado." });
            }

            empresa.AlterarLogo(request.Logo, userId);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Logo alterada com sucesso!", new { EmpresaId = empresa.Id });
        }
    }

    /// <summary>Exclui Empresa.</summary>
    public class ExcluirEmpresaCommandHandler : ICommandHandler<ExcluirEmpresaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ExcluirEmpresaCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ExcluirEmpresaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.Id == request.Id && e.TenantId == tenantId, cancellationToken);
            if (empresa == null)
                return CommandResult.Falha(new[] { "Empresa não encontrada" });

            empresa.Deletar(userId);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Empresa removida com sucesso.");
        }
    }

    /// <summary>Lista Certificados Empresa.</summary>
    public class ListarCertificadosEmpresaQueryHandler : IQueryHandler<ListarCertificadosEmpresaQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarCertificadosEmpresaQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarCertificadosEmpresaQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var certificados = await _context.EmpresasCertificados
                .Where(c => c.EmpresaId == request.EmpresaId && c.TenantId == tenantId)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Certificados listados com sucesso.", certificados);
        }
    }

    /// <summary>Exclui Certificado Digital.</summary>
    public class ExcluirCertificadoDigitalCommandHandler : ICommandHandler<ExcluirCertificadoDigitalCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ExcluirCertificadoDigitalCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ExcluirCertificadoDigitalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var certificado = await _context.EmpresasCertificados
                .FirstOrDefaultAsync(c => c.Id == request.CertificadoId && c.EmpresaId == request.EmpresaId && c.TenantId == tenantId, cancellationToken);

            if (certificado == null)
                return CommandResult.Falha(new[] { "Certificado digital não encontrado para esta empresa." });

            // REG-PEM-108 / REG-PEM-132: certificado ativo/em uso não pode ser excluído
            var empresaVinculada = await _context.Empresas
                .AnyAsync(e => e.CertificadoDigitalId == request.CertificadoId && e.TenantId == tenantId, cancellationToken);
            if (empresaVinculada)
                return CommandResult.Falha(new[] { "Não é possível excluir o certificado digital porque ele está atualmente ativo e em uso por uma empresa." });

            certificado.Deletar(userId);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Certificado digital removido com sucesso.");
        }
    }
}
