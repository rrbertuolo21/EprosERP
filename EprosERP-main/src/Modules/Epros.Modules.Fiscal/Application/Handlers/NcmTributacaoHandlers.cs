using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    public class CriarNcmTributacaoCommandHandler : ICommandHandler<CriarNcmTributacaoCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarNcmTributacaoCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(CriarNcmTributacaoCommand r, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var ncmTrib = new NcmTributacao(
                r.TributarioGrupoId, r.CodigoBeneficioFiscalId, r.CodRegra, r.Descricao, r.CfopNotaConsumidor,
                r.CfopNotaFiscal, r.CfopNotaFiscalInterestadual, r.Origem, r.CsosnNotaConsumidor, r.CstIcmsNotaConsumidor,
                r.CsosnNotaFiscal, r.CstIcmsNotaFiscalInterna, r.CstIcmsNotaFiscalInterstadual, r.CstPis, r.CstCofins,
                r.ValorUnitFixoPis, r.ValorUnitFixoCofins, r.ValorAliquotaPis, r.ValorAliquotaCofins, r.CstPisCofinsEntrada,
                r.CstIpiSaida, r.CstIpiEntrada, r.ValorAliquotaIpi, r.ValorPercentualReducacaoBcIpi, r.TipoReducaoIpi,
                r.DestinoReducaoIpi, r.IpiEmbutido, r.EnquadramentoIpi, r.CodigoValorFiscalIcmsInterna, r.CodigoValorFiscalcmsInterstadual,
                r.ValorAliquotaIcmsInterna, r.ValorPercentualReducacaoBcIcmsInterna, r.TipoReducaoIcmsInterna, r.DestinoReducaoIcmsInterna,
                r.ValorAliquotaIcmsInterstadual, r.ValorPercentualReducacaoBcIcmsInterstadual, r.TipoReducaoIcmsInterstadual, r.DestinoReducaoIcmsInterstadual,
                r.CodigoBeneficioFiscalIcms, r.MotivoDesoneracaoIcms, r.InformacoesComplementares, r.InformacoesAdicionaisAoFisco,
                tenantId, usuario);

            if (r.CstIbsCbsNfe != null || r.CstIbsCbsNfce != null || r.CClassTribNfe != null || r.CClassTribNfce != null)
                ncmTrib.AdicionarIbsCbs(r.CstIbsCbsNfe, r.CClassTribNfe, r.CstIbsCbsNfce, r.CClassTribNfce);

            if (!ncmTrib.IsValid)
                return CommandResult.Falha(ncmTrib.Notifications.Select(n => n.Message), "Erro de validação da NCM Tributação.");

            _context.NcmTributacoes.Add(ncmTrib);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("NCM Tributação criada com sucesso!", new { ncmTrib.Id });
        }
    }

    public class AtualizarNcmTributacaoCommandHandler : ICommandHandler<AtualizarNcmTributacaoCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarNcmTributacaoCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AtualizarNcmTributacaoCommand r, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var ncmTrib = await _context.NcmTributacoes
                .Include(n => n.NcmTributacaoSts)
                .Include(n => n.NcmTributacaoFundoCombatePobrezas)
                .Include(n => n.NcmConfiguracoes)
                .FirstOrDefaultAsync(n => n.Id == r.Id && n.DeletadoEm == null, cancellationToken);
            if (ncmTrib == null)
                return CommandResult.Falha("NCM Tributação não localizada.");

            ncmTrib.Alterar(
                r.TributarioGrupoId, r.CodigoBeneficioFiscalId, r.CodRegra, r.Descricao, r.CfopNotaConsumidor,
                r.CfopNotaFiscal, r.CfopNotaFiscalInterestadual, r.Origem, r.CsosnNotaConsumidor, r.CstIcmsNotaConsumidor,
                r.CsosnNotaFiscal, r.CstIcmsNotaFiscalInterna, r.CstIcmsNotaFiscalInterstadual, r.CstPis, r.CstCofins,
                r.ValorUnitFixoPis, r.ValorUnitFixoCofins, r.ValorAliquotaPis, r.ValorAliquotaCofins, r.CstPisCofinsEntrada,
                r.CstIpiSaida, r.CstIpiEntrada, r.ValorAliquotaIpi, r.ValorPercentualReducacaoBcIpi, r.TipoReducaoIpi,
                r.DestinoReducaoIpi, r.IpiEmbutido, r.EnquadramentoIpi, r.CodigoValorFiscalIcmsInterna, r.CodigoValorFiscalcmsInterstadual,
                r.ValorAliquotaIcmsInterna, r.ValorPercentualReducacaoBcIcmsInterna, r.TipoReducaoIcmsInterna, r.DestinoReducaoIcmsInterna,
                r.ValorAliquotaIcmsInterstadual, r.ValorPercentualReducacaoBcIcmsInterstadual, r.TipoReducaoIcmsInterstadual, r.DestinoReducaoIcmsInterstadual,
                r.CodigoBeneficioFiscalIcms, r.MotivoDesoneracaoIcms, r.InformacoesComplementares, r.InformacoesAdicionaisAoFisco,
                usuario);

            if (!ncmTrib.IsValid)
                return CommandResult.Falha(ncmTrib.Notifications.Select(n => n.Message), "Erro de validação da NCM Tributação.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("NCM Tributação atualizada com sucesso!", new { ncmTrib.Id });
        }
    }

    public class DeletarNcmTributacaoCommandHandler : ICommandHandler<DeletarNcmTributacaoCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DeletarNcmTributacaoCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(DeletarNcmTributacaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var ncmTrib = await _context.NcmTributacoes
                .Include(n => n.NcmTributacaoSts)
                .Include(n => n.NcmTributacaoFundoCombatePobrezas)
                .Include(n => n.NcmConfiguracoes)
                .FirstOrDefaultAsync(n => n.Id == request.Id && n.TenantId == tenantId && n.DeletadoEm == null, cancellationToken);
            if (ncmTrib == null)
                return CommandResult.Falha("NCM Tributação não encontrada.");

            ncmTrib.DeletarCascata(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("NCM Tributação deletada com sucesso!");
        }
    }

    // ----- IBS/CBS -----
    public class DefinirIbsCbsNcmTributacaoCommandHandler : ICommandHandler<DefinirIbsCbsNcmTributacaoCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public DefinirIbsCbsNcmTributacaoCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(DefinirIbsCbsNcmTributacaoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var ncmTrib = await _context.NcmTributacoes.FirstOrDefaultAsync(n => n.Id == request.NcmTributacaoId && n.DeletadoEm == null, cancellationToken);
            if (ncmTrib == null)
                return CommandResult.Falha("NCM Tributação não localizada.");

            ncmTrib.AlterarIbsCbs(request.CstIbsCbsNfe, request.CClassTribNfe, request.CstIbsCbsNfce, request.CClassTribNfce, usuario);
            if (!ncmTrib.IsValid)
                return CommandResult.Falha(ncmTrib.Notifications.Select(n => n.Message), "Erro de validação do IBS/CBS.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("IBS/CBS definido com sucesso!", new { ncmTrib.Id });
        }
    }

    // ----- NcmTributacaoSt -----
    public class AdicionarNcmTributacaoStCommandHandler : ICommandHandler<AdicionarNcmTributacaoStCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AdicionarNcmTributacaoStCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AdicionarNcmTributacaoStCommand r, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var ncmTrib = await _context.NcmTributacoes.Include(n => n.NcmTributacaoSts)
                .FirstOrDefaultAsync(n => n.Id == r.NcmTributacaoId && n.DeletadoEm == null, cancellationToken);
            if (ncmTrib == null)
                return CommandResult.Falha("NCM Tributação não localizada.");

            ncmTrib.Clear();
            ncmTrib.AdicionarNcmTributacaoSt(r.Uf, r.TipoCalculo, r.ValorAliquotaIcmsSt, r.ValorMva, r.ValorReducaoBcIcmsSt, r.TipoReducaoIcmsSt, r.ValorUnitarioSt, r.ValorPercentualFcpSt, usuario);
            if (!ncmTrib.IsValid)
                return CommandResult.Falha(ncmTrib.Notifications.Select(n => n.Message), "Erro de validação do ICMS ST.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("ICMS ST adicionado com sucesso!");
        }
    }

    public class AlterarNcmTributacaoStCommandHandler : ICommandHandler<AlterarNcmTributacaoStCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AlterarNcmTributacaoStCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AlterarNcmTributacaoStCommand r, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var ncmTrib = await _context.NcmTributacoes.Include(n => n.NcmTributacaoSts)
                .FirstOrDefaultAsync(n => n.Id == r.NcmTributacaoId && n.DeletadoEm == null, cancellationToken);
            if (ncmTrib == null)
                return CommandResult.Falha("NCM Tributação não localizada.");

            ncmTrib.Clear();
            ncmTrib.AlterarNcmTributacaoSt(r.NcmTributacaoStId, r.Uf, r.TipoCalculo, r.ValorAliquotaIcmsSt, r.ValorMva, r.ValorReducaoBcIcmsSt, r.TipoReducaoIcmsSt, r.ValorUnitarioSt, r.ValorPercentualFcpSt, usuario);
            if (!ncmTrib.IsValid)
                return CommandResult.Falha(ncmTrib.Notifications.Select(n => n.Message), "Erro de validação do ICMS ST.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("ICMS ST alterado com sucesso!");
        }
    }

    public class DeletarNcmTributacaoStCommandHandler : ICommandHandler<DeletarNcmTributacaoStCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public DeletarNcmTributacaoStCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(DeletarNcmTributacaoStCommand r, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var ncmTrib = await _context.NcmTributacoes.Include(n => n.NcmTributacaoSts)
                .FirstOrDefaultAsync(n => n.Id == r.NcmTributacaoId && n.DeletadoEm == null, cancellationToken);
            if (ncmTrib == null)
                return CommandResult.Falha("NCM Tributação não localizada.");

            ncmTrib.DeletarNcmTributacaoSt(r.NcmTributacaoStId, usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("ICMS ST deletado com sucesso!");
        }
    }

    // ----- NcmTributacaoFundoCombatePobreza -----
    public class AdicionarNcmTributacaoFundoCombatePobrezaCommandHandler : ICommandHandler<AdicionarNcmTributacaoFundoCombatePobrezaCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AdicionarNcmTributacaoFundoCombatePobrezaCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AdicionarNcmTributacaoFundoCombatePobrezaCommand r, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var ncmTrib = await _context.NcmTributacoes.Include(n => n.NcmTributacaoFundoCombatePobrezas)
                .FirstOrDefaultAsync(n => n.Id == r.NcmTributacaoId && n.DeletadoEm == null, cancellationToken);
            if (ncmTrib == null)
                return CommandResult.Falha("NCM Tributação não localizada.");

            ncmTrib.Clear();
            ncmTrib.AdicionarNcmTributacaoFundoCombatePobreza(r.Uf, r.ValorPercentual, usuario);
            if (!ncmTrib.IsValid)
                return CommandResult.Falha(ncmTrib.Notifications.Select(n => n.Message), "Erro de validação do Fundo de Combate à Pobreza.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Fundo de Combate à Pobreza adicionado com sucesso!");
        }
    }

    public class AlterarNcmTributacaoFundoCombatePobrezaCommandHandler : ICommandHandler<AlterarNcmTributacaoFundoCombatePobrezaCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AlterarNcmTributacaoFundoCombatePobrezaCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AlterarNcmTributacaoFundoCombatePobrezaCommand r, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var ncmTrib = await _context.NcmTributacoes.Include(n => n.NcmTributacaoFundoCombatePobrezas)
                .FirstOrDefaultAsync(n => n.Id == r.NcmTributacaoId && n.DeletadoEm == null, cancellationToken);
            if (ncmTrib == null)
                return CommandResult.Falha("NCM Tributação não localizada.");

            ncmTrib.Clear();
            ncmTrib.AlterarNcmTributacaoFundoCombatePobreza(r.FundoCombatePobrezaId, r.Uf, r.ValorPercentual, usuario);
            if (!ncmTrib.IsValid)
                return CommandResult.Falha(ncmTrib.Notifications.Select(n => n.Message), "Erro de validação do Fundo de Combate à Pobreza.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Fundo de Combate à Pobreza alterado com sucesso!");
        }
    }

    public class DeletarNcmTributacaoFundoCombatePobrezaCommandHandler : ICommandHandler<DeletarNcmTributacaoFundoCombatePobrezaCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public DeletarNcmTributacaoFundoCombatePobrezaCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(DeletarNcmTributacaoFundoCombatePobrezaCommand r, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var ncmTrib = await _context.NcmTributacoes.Include(n => n.NcmTributacaoFundoCombatePobrezas)
                .FirstOrDefaultAsync(n => n.Id == r.NcmTributacaoId && n.DeletadoEm == null, cancellationToken);
            if (ncmTrib == null)
                return CommandResult.Falha("NCM Tributação não localizada.");

            ncmTrib.DeletarNcmTributacaoFundoCombatePobreza(r.FundoCombatePobrezaId, usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Fundo de Combate à Pobreza deletado com sucesso!");
        }
    }

    // ----- NcmConfiguracao -----
    public class AdicionarNcmConfiguracaoCommandHandler : ICommandHandler<AdicionarNcmConfiguracaoCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AdicionarNcmConfiguracaoCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AdicionarNcmConfiguracaoCommand r, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var ncmTrib = await _context.NcmTributacoes.Include(n => n.NcmConfiguracoes)
                .FirstOrDefaultAsync(n => n.Id == r.NcmTributacaoId && n.DeletadoEm == null, cancellationToken);
            if (ncmTrib == null)
                return CommandResult.Falha("NCM Tributação não localizada.");

            ncmTrib.Clear();
            ncmTrib.AdicionarNcmConfiguracao(r.NcmId, usuario);
            if (!ncmTrib.IsValid)
                return CommandResult.Falha(ncmTrib.Notifications.Select(n => n.Message), "Erro de validação da Configuração NCM.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Configuração NCM adicionada com sucesso!");
        }
    }

    public class AlterarNcmConfiguracaoCommandHandler : ICommandHandler<AlterarNcmConfiguracaoCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AlterarNcmConfiguracaoCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AlterarNcmConfiguracaoCommand r, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var ncmTrib = await _context.NcmTributacoes.Include(n => n.NcmConfiguracoes)
                .FirstOrDefaultAsync(n => n.Id == r.NcmTributacaoId && n.DeletadoEm == null, cancellationToken);
            if (ncmTrib == null)
                return CommandResult.Falha("NCM Tributação não localizada.");

            ncmTrib.Clear();
            ncmTrib.AlterarNcmConfiguracao(r.NcmConfiguracaoId, r.NcmId, usuario);
            if (!ncmTrib.IsValid)
                return CommandResult.Falha(ncmTrib.Notifications.Select(n => n.Message), "Erro de validação da Configuração NCM.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Configuração NCM alterada com sucesso!");
        }
    }

    public class DeletarNcmConfiguracaoCommandHandler : ICommandHandler<DeletarNcmConfiguracaoCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public DeletarNcmConfiguracaoCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(DeletarNcmConfiguracaoCommand r, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var ncmTrib = await _context.NcmTributacoes.Include(n => n.NcmConfiguracoes)
                .FirstOrDefaultAsync(n => n.Id == r.NcmTributacaoId && n.DeletadoEm == null, cancellationToken);
            if (ncmTrib == null)
                return CommandResult.Falha("NCM Tributação não localizada.");

            ncmTrib.DeletarNcmConfiguracao(r.NcmConfiguracaoId, usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Configuração NCM deletada com sucesso!");
        }
    }
}
