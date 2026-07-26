using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Obtém Empresa Configuracoes.</summary>
    public class ObterEmpresaConfiguracoesQueryHandler : IQueryHandler<ObterEmpresaConfiguracoesQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterEmpresaConfiguracoesQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterEmpresaConfiguracoesQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var empresa = await _context.Empresas
                .Where(e => e.TenantId == tenantId)
                .OrderBy(e => e.CriadoEm)
                .FirstOrDefaultAsync(cancellationToken);

            if (empresa == null)
            {
                return CommandResult.Falha(new[] { "Nenhuma empresa cadastrada para este inquilino." }, "Erro");
            }

            return CommandResult.Ok("Configurações da empresa obtidas com sucesso.", new
            {
                empresa.Id,
                empresa.RazaoSocial,
                empresa.NomeFantasia,
                empresa.Cnpj,
                empresa.InscricaoEstadual,
                empresa.InscricaoMunicipal,
                empresa.InscricaoSuframa,
                empresa.Cnae,
                empresa.RegimeTributario,
                empresa.RegimeApuracao,
                empresa.Logo,
                empresa.DateFormat,
                empresa.TimeZoneId,
                empresa.CurrencyId,
                Endereco = new
                {
                    empresa.Endereco.Logradouro,
                    empresa.Endereco.Numero,
                    empresa.Endereco.Complemento,
                    empresa.Endereco.Bairro,
                    empresa.Endereco.Cep,
                    empresa.Endereco.Cidade,
                    empresa.Endereco.Estado
                }
            });
        }
    }

    /// <summary>Obtém Preferencias.</summary>
    public class ObterPreferenciasQueryHandler : IQueryHandler<ObterPreferenciasQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterPreferenciasQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterPreferenciasQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var pref = await _context.PreferenciasGerais
                .FirstOrDefaultAsync(p => p.TenantId == tenantId, cancellationToken);

            if (pref == null)
            {
                // Retorna objeto padrão
                return CommandResult.Ok("Preferências padrões.", new
                {
                    ShowCurrency = true,
                    NegativeCash = true,
                    NegativeStock = true,
                    StockCalculationMode = "CustoMedio",
                    CreditLimit = false,
                    Discount = false,
                    VatOnPurchase = false,
                    VatOnSales = false
                });
            }

            return CommandResult.Ok("Preferências obtidas com sucesso.", new
            {
                pref.Id,
                pref.ShowCurrency,
                pref.NegativeCash,
                pref.NegativeStock,
                StockCalculationMode = pref.StockCalculationMode.ToString(),
                pref.CreditLimit,
                pref.Discount,
                pref.VatOnPurchase,
                pref.VatOnSales
            });
        }
    }

    /// <summary>Obtém Configuracao Email.</summary>
    public class ObterConfiguracaoEmailQueryHandler : IQueryHandler<ObterConfiguracaoEmailQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterConfiguracaoEmailQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterConfiguracaoEmailQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var email = await _context.ConfiguracoesEmail
                .FirstOrDefaultAsync(e => e.TenantId == tenantId, cancellationToken);

            if (email == null)
            {
                return CommandResult.Ok("Nenhuma configuração registrada.", new
                {
                    Host = "",
                    Port = 587,
                    Username = "",
                    Password = "",
                    FromEmail = ""
                });
            }

            // REG-069: Senha de e-mail mascarada antes do retorno
            string maskedPassword = string.IsNullOrEmpty(email.Password) ? "" : "••••••••";

            return CommandResult.Ok("Configurações de e-mail obtidas.", new
            {
                email.Id,
                email.Host,
                email.Port,
                email.Username,
                Password = maskedPassword,
                email.FromEmail
            });
        }
    }

    /// <summary>Lista Categorias.</summary>
    public class ListarCategoriasQueryHandler : IQueryHandler<ListarCategoriasQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarCategoriasQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarCategoriasQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var query = _context.Categorias.Where(c => c.TenantId == tenantId);

            if (!string.IsNullOrEmpty(request.NomeFiltro))
            {
                query = query.Where(c => c.Nome.Contains(request.NomeFiltro));
            }

            var list = await query.OrderBy(c => c.Nome).ToListAsync(cancellationToken);
            return CommandResult.Ok("Categorias obtidas com sucesso.", list);
        }
    }

    /// <summary>Lista Unidades Medida.</summary>
    public class ListarUnidadesMedidaQueryHandler : IQueryHandler<ListarUnidadesMedidaQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarUnidadesMedidaQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarUnidadesMedidaQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var list = await _context.UnidadesMedida
                .Where(u => u.TenantId == tenantId)
                .OrderBy(u => u.Nome)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Unidades de medida obtidas com sucesso.", list);
        }
    }

    /// <summary>Lista Armazens.</summary>
    public class ListarArmazensQueryHandler : IQueryHandler<ListarArmazensQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarArmazensQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarArmazensQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var list = await _context.Armazens
                .Where(a => a.TenantId == tenantId)
                .OrderBy(a => a.Nome)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Armazéns obtidos com sucesso.", list);
        }
    }

    /// <summary>Lista Projetos.</summary>
    public class ListarProjetosQueryHandler : IQueryHandler<ListarProjetosQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarProjetosQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarProjetosQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var list = await _context.Projetos
                .Where(p => p.TenantId == tenantId)
                .OrderBy(p => p.Nome)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Projetos obtidos com sucesso.", list);
        }
    }

    /// <summary>Lista Impostos.</summary>
    public class ListarImpostosQueryHandler : IQueryHandler<ListarImpostosQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarImpostosQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarImpostosQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var list = await _context.Impostos
                .Where(i => i.TenantId == tenantId)
                .OrderBy(i => i.Nome)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Impostos obtidos com sucesso.", list);
        }
    }

    /// <summary>Lista Conversoes Unidades.</summary>
    public class ListarConversoesUnidadesQueryHandler : IQueryHandler<ListarConversoesUnidadesQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarConversoesUnidadesQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarConversoesUnidadesQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var list = await _context.ConversoesUnidades
                .Where(c => c.TenantId == tenantId)
                .Select(c => new
                {
                    c.Id,
                    c.UnidadeOrigemId,
                    UnidadeOrigemNome = _context.UnidadesMedida.Where(u => u.Id == c.UnidadeOrigemId).Select(u => u.Nome).FirstOrDefault(),
                    c.UnidadeDestinoId,
                    UnidadeDestinoNome = _context.UnidadesMedida.Where(u => u.Id == c.UnidadeDestinoId).Select(u => u.Nome).FirstOrDefault(),
                    c.Fator
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Regras de conversão obtidas com sucesso.", list);
        }
    }

    /// <summary>Lista Logs Auditoria.</summary>
    public class ListarLogsAuditoriaQueryHandler : IQueryHandler<ListarLogsAuditoriaQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarLogsAuditoriaQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarLogsAuditoriaQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var list = await _context.LogsAuditoriaConfiguracao
                .Where(l => l.TenantId == tenantId)
                .OrderByDescending(l => l.DataHora)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Logs de auditoria obtidos.", list);
        }
    }

    /// <summary>Lista Fusos Horarios.</summary>
    public class ListarFusosHorariosQueryHandler : IQueryHandler<ListarFusosHorariosQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;

        public ListarFusosHorariosQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ListarFusosHorariosQuery request, CancellationToken cancellationToken)
        {
            var list = await _context.FusosHorarios
                .OrderBy(f => f.FusoHorarioId)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Fusos horários globais obtidos.", list);
        }
    }

    /// <summary>Lista Moedas.</summary>
    public class ListarMoedasQueryHandler : IQueryHandler<ListarMoedasQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;

        public ListarMoedasQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ListarMoedasQuery request, CancellationToken cancellationToken)
        {
            var list = await _context.Moedas
                .OrderBy(m => m.CodigoISO)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Moedas globais obtidas.", list);
        }
    }
}
