using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Application.Services;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    public class SalvarConfiguracaoEmpresaCommandHandler : ICommandHandler<SalvarConfiguracaoEmpresaCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ICurrentUser _currentUser;
        private readonly ITenantProvider _tenantProvider;

        public SalvarConfiguracaoEmpresaCommandHandler(
            ContextAplicativo context, 
            ICurrentUser currentUser,
            ITenantProvider tenantProvider)
        {
            _context = context;
            _currentUser = currentUser;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(SalvarConfiguracaoEmpresaCommand request, CancellationToken cancellationToken)
        {
            var alteradoPor = _currentUser.GetUserId() ?? "system";
            var config = await _context.ConfiguracoesEmpresas
                .FirstOrDefaultAsync(c => c.EmpresaId == request.EmpresaId && c.DeletadoEm == null, cancellationToken);

            if (config == null)
            {
                var tenantId = _tenantProvider.GetTenantId();
                config = new ConfiguracaoEmpresa(
                    tenantId: tenantId,
                    empresaId: request.EmpresaId,
                    nome: request.Nome,
                    email: request.Email,
                    telefone: request.Telefone,
                    endereco: request.Endereco,
                    timeZoneId: request.TimeZoneId,
                    dateFormat: request.DateFormat,
                    currencyId: request.CurrencyId,
                    vatPercentage: request.VatPercentage,
                    vatType: request.VatType,
                    currencyPosition: request.CurrencyPosition,
                    footerText: request.FooterText,
                    logo: request.Logo,
                    favicon: request.Favicon,
                    criadoPor: alteradoPor
                );
                _context.ConfiguracoesEmpresas.Add(config);
            }
            else
            {
                config.Atualizar(
                    nome: request.Nome,
                    email: request.Email,
                    telefone: request.Telefone,
                    endereco: request.Endereco,
                    timeZoneId: request.TimeZoneId,
                    dateFormat: request.DateFormat,
                    currencyId: request.CurrencyId,
                    vatPercentage: request.VatPercentage,
                    vatType: request.VatType,
                    currencyPosition: request.CurrencyPosition,
                    footerText: request.FooterText,
                    logo: request.Logo,
                    favicon: request.Favicon,
                    alteradoPor: alteradoPor
                );
            }

            if (!config.IsValid)
            {
                return CommandResult.Falha(config.Notifications.Select(n => n.Message));
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Configurações da empresa salvas com sucesso!");
        }
    }

    public class HabilitarIdiomaCommandHandler : ICommandHandler<HabilitarIdiomaCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ICurrentUser _currentUser;
        private readonly ITenantProvider _tenantProvider;

        public HabilitarIdiomaCommandHandler(
            ContextAplicativo context, 
            ICurrentUser currentUser,
            ITenantProvider tenantProvider)
        {
            _context = context;
            _currentUser = currentUser;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(HabilitarIdiomaCommand request, CancellationToken cancellationToken)
        {
            var codeLower = request.Code.ToLowerInvariant().Trim();
            var alteradoPor = _currentUser.GetUserId() ?? "system";
            var tenantId = _tenantProvider.GetTenantId();

            var idioma = await _context.Idiomas
                .FirstOrDefaultAsync(i => i.Code == codeLower && i.DeletadoEm == null, cancellationToken);

            if (idioma == null)
            {
                idioma = new Idioma(
                    tenantId: tenantId,
                    code: codeLower,
                    name: codeLower == "es" ? "Español" : codeLower.ToUpperInvariant(),
                    countryCode: codeLower == "es" ? "ES" : "XX",
                    enabled: true,
                    criadoPor: alteradoPor
                );
                _context.Idiomas.Add(idioma);
            }
            else
            {
                idioma.Ativar(alteradoPor);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok($"Idioma {request.Code} habilitado com sucesso!");
        }
    }

    public class DesabilitarIdiomaCommandHandler : ICommandHandler<DesabilitarIdiomaCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ICurrentUser _currentUser;

        public DesabilitarIdiomaCommandHandler(ContextAplicativo context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DesabilitarIdiomaCommand request, CancellationToken cancellationToken)
        {
            var codeLower = request.Code.ToLowerInvariant().Trim();
            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var idioma = await _context.Idiomas
                .FirstOrDefaultAsync(i => i.Code == codeLower && i.DeletadoEm == null, cancellationToken);

            if (idioma == null)
            {
                return CommandResult.Falha(new[] { "Idioma não encontrado." });
            }

            idioma.Desativar(alteradoPor);

            if (!idioma.IsValid)
            {
                return CommandResult.Falha(idioma.Notifications.Select(n => n.Message));
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok($"Idioma {request.Code} desabilitado com sucesso!");
        }
    }

    /// <summary>
    /// APP-TEN-002 (RF-6.6): encadeia a criação do Cliente SaaS (dono = GestaoClientes) durante o
    /// onboarding, garantindo atomicidade com a semente de configuração da empresa no Aplicativo.
    /// </summary>
    public class CriarClienteSaaSOnboardingCommandHandler : ICommandHandler<CriarClienteSaaSOnboardingCommand>
    {
        private readonly ContextGestaoClientes _contextGestao;
        private readonly ICurrentUser _currentUser;

        public CriarClienteSaaSOnboardingCommandHandler(ContextGestaoClientes contextGestao, ICurrentUser currentUser)
        {
            _contextGestao = contextGestao;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarClienteSaaSOnboardingCommand request, CancellationToken cancellationToken)
        {
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var jaExiste = await _contextGestao.Clientes
                .IgnoreQueryFilters()
                .AnyAsync(c => c.TenantId == request.TenantId && c.DeletadoEm == null, cancellationToken);
            if (jaExiste)
            {
                return CommandResult.Falha("Já existe um Cliente SaaS para o tenant informado.");
            }

            var plano = await _contextGestao.Planos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == request.PlanoId && p.Ativo && p.DeletadoEm == null, cancellationToken);
            if (plano == null)
            {
                return CommandResult.Falha("Plano SaaS informado é inválido ou está inativo.");
            }

            // Reusa o ponto único de criação de Cliente SaaS (1.07) — mesma lógica do self-register.
            var cliente = OnboardingSaaSProvisioner.CriarCliente(
                tenantId: request.TenantId,
                razaoSocial: request.RazaoSocial,
                documento: request.Cnpj,
                email: request.Email,
                planoId: request.PlanoId,
                statusSaaS: Epros.Modules.GestaoClientes.Domain.Entities.StatusSaaS.Ativo,
                diaVencimento: request.DiaVencimento,
                criadoPor: criadoPor,
                telefone: request.Telefone,
                nomeContato: request.NomeContato,
                isDemo: request.IsDemo);

            if (!cliente.IsValid)
            {
                return CommandResult.Falha(cliente.Notifications.Select(n => n.Message));
            }

            _contextGestao.Clientes.Add(cliente);
            await _contextGestao.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Cliente SaaS criado com sucesso no onboarding.", new { cliente.Id, cliente.TenantId });
        }
    }

    public class ObterConfiguracaoEmpresaQueryHandler : IQueryHandler<ObterConfiguracaoEmpresaQuery, ConfiguracaoEmpresaDto>
    {
        private readonly ContextAplicativo _context;

        public ObterConfiguracaoEmpresaQueryHandler(ContextAplicativo context)
        {
            _context = context;
        }

        public async Task<ConfiguracaoEmpresaDto> Handle(ObterConfiguracaoEmpresaQuery request, CancellationToken cancellationToken)
        {
            var config = await _context.ConfiguracoesEmpresas
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.EmpresaId == request.EmpresaId && c.DeletadoEm == null, cancellationToken);

            if (config == null)
            {
                return null!;
            }

            return new ConfiguracaoEmpresaDto(
                EmpresaId: config.EmpresaId,
                Nome: config.Nome,
                Email: config.Email,
                Telefone: config.Telefone,
                Endereco: config.Endereco,
                TimeZoneId: config.TimeZoneId,
                DateFormat: config.DateFormat,
                CurrencyId: config.CurrencyId,
                VatPercentage: config.VatPercentage,
                VatType: config.VatType,
                CurrencyPosition: config.CurrencyPosition,
                FooterText: config.FooterText,
                Logo: config.Logo,
                Favicon: config.Favicon
            );
        }
    }

    public class ListarIdiomasHabilitadosQueryHandler : IQueryHandler<ListarIdiomasHabilitadosQuery, IEnumerable<IdiomaDto>>
    {
        private readonly ContextAplicativo _context;

        public ListarIdiomasHabilitadosQueryHandler(ContextAplicativo context)
        {
            _context = context;
        }

        public async Task<IEnumerable<IdiomaDto>> Handle(ListarIdiomasHabilitadosQuery request, CancellationToken cancellationToken)
        {
            var idiomas = await _context.Idiomas
                .AsNoTracking()
                .Where(i => i.Enabled && i.DeletadoEm == null)
                .Select(i => new IdiomaDto(i.Code, i.Name, i.CountryCode, i.Enabled))
                .ToListAsync(cancellationToken);

            return idiomas;
        }
    }

    public class ObterContextoSessaoQueryHandler : IQueryHandler<ObterContextoSessaoQuery, ContextoSessaoDto>
    {
        private readonly ContextAplicativo _contextAplicativo;
        private readonly ContextGestaoClientes _contextGestaoClientes;

        public ObterContextoSessaoQueryHandler(
            ContextAplicativo contextAplicativo,
            ContextGestaoClientes contextGestaoClientes)
        {
            _contextAplicativo = contextAplicativo;
            _contextGestaoClientes = contextGestaoClientes;
        }

        public async Task<ContextoSessaoDto> Handle(ObterContextoSessaoQuery request, CancellationToken cancellationToken)
        {
            var vinculos = await _contextAplicativo.UsuariosEmpresas
                .AsNoTracking()
                .Where(ue => ue.UsuarioId == request.UsuarioId && ue.DeletadoEm == null)
                .ToListAsync(cancellationToken);

            var empresaIds = vinculos.Select(v => v.EmpresaId).ToList();

            var empresas = await _contextGestaoClientes.Empresas
                .AsNoTracking()
                .Where(e => empresaIds.Contains(e.Id) && e.DeletadoEm == null)
                .ToDictionaryAsync(e => e.Id, e => e.RazaoSocial, cancellationToken);

            var empresaDtos = vinculos.Select(v => new UsuarioEmpresaDto(
                EmpresaId: v.EmpresaId,
                RazaoSocial: empresas.TryGetValue(v.EmpresaId, out var nome) ? nome : "Empresa Desconhecida",
                EhAdmin: v.EhAdmin,
                PerfilUsuarioId: v.PerfilAcessoId
            )).ToList();

            var cliente = await _contextGestaoClientes.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.TenantId == request.TenantId && c.Ativo && c.DeletadoEm == null, cancellationToken);

            int limiteEmpresas = 0;
            int limiteUsuarios = 0;
            bool block = false;

            if (cliente == null)
            {
                block = true;
            }
            else
            {
                var plano = await _contextGestaoClientes.Planos
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.Id == cliente.PlanoId && p.Ativo && p.DeletadoEm == null, cancellationToken);

                if (plano == null)
                {
                    block = true;
                }
                else
                {
                    limiteEmpresas = plano.LimiteEmpresas;
                    limiteUsuarios = plano.LimiteUsuarios;
                }

                // 1.07 (destrava §3.2) — TrialGratuito opera normalmente (alinhado ao InquilinoSaaSMiddleware,
                // que trata Ativo/TrialGratuito como operacionais). Só bloqueia fora desses dois status ou
                // se o cliente estiver inativo. O fim do trial recai no gating de StatusSaaS (1.01/REG-021).
                if ((cliente.StatusSaaS != Epros.Modules.GestaoClientes.Domain.Entities.StatusSaaS.Ativo
                     && cliente.StatusSaaS != Epros.Modules.GestaoClientes.Domain.Entities.StatusSaaS.TrialGratuito)
                    || !cliente.Ativo)
                {
                    block = true;
                }

                var hasOverdueInvoice = await _contextGestaoClientes.Faturas
                    .IgnoreQueryFilters()
                    .AnyAsync(f => f.ClienteId == cliente.Id && 
                                   (f.Status == Epros.Modules.GestaoClientes.Domain.Entities.FaturaStatus.Pendente || f.Status == Epros.Modules.GestaoClientes.Domain.Entities.FaturaStatus.Atrasada) &&
                                   f.DataVencimento.AddDays(15) < DateTime.UtcNow, cancellationToken);

                if (hasOverdueInvoice)
                {
                    block = true;
                }
            }

            return new ContextoSessaoDto(
                TenantId: request.TenantId,
                UsuarioId: request.UsuarioId,
                QtdeCadastroEmpresa: limiteEmpresas,
                QtdeCadastroUsuario: limiteUsuarios,
                Block: block,
                Empresas: empresaDtos
            );
        }
    }
}
