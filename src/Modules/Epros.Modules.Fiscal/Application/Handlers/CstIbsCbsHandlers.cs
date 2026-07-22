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
    public class CriarCstIbsCbsCommandHandler : ICommandHandler<CriarCstIbsCbsCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCstIbsCbsCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(CriarCstIbsCbsCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var cst = new CstIbsCbs(request.Cst, request.Descricao, request.DataInicioVigencia, request.DataFimVigencia, tenantId, usuario);
            if (!cst.IsValid)
                return CommandResult.Falha(cst.Notifications.Select(n => n.Message), "Erro de validação do CST IBS/CBS.");

            if (request.ClassesTributarias != null)
            {
                foreach (var classe in request.ClassesTributarias)
                {
                    var cctrib = cst.AdicionarClasseTributaria(
                        classe.Codigo, classe.Descricao, classe.DataInicioVigencia, classe.DataFimVigencia,
                        classe.IndNfe, classe.IndNfce, classe.IndCte, classe.IndCteos, classe.IndNfse, classe.IndTribRegular, usuario);

                    if (classe.Anexos != null)
                        foreach (var a in classe.Anexos)
                            cctrib.AdicionarAnexo(a.NroAnexo, a.Codigo, a.DataInicioVigencia, a.DataFimVigencia, usuario);

                    if (!cctrib.IsValid)
                        return CommandResult.Falha(cctrib.Notifications.Select(n => n.Message), "Erro de validação da Classificação Tributária.");
                }
            }

            _context.CstsIbsCbs.Add(cst);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("CST IBS/CBS criado com sucesso!", new { cst.Id });
        }
    }

    public class AtualizarCstIbsCbsCommandHandler : ICommandHandler<AtualizarCstIbsCbsCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarCstIbsCbsCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AtualizarCstIbsCbsCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var cst = await _context.CstsIbsCbs.FirstOrDefaultAsync(c => c.Id == request.Id && c.DeletadoEm == null, cancellationToken);
            if (cst == null)
                return CommandResult.Falha("CST IBS/CBS não localizado.");

            cst.Alterar(request.Cst, request.Descricao, request.DataInicioVigencia, request.DataFimVigencia, usuario);
            if (!cst.IsValid)
                return CommandResult.Falha(cst.Notifications.Select(n => n.Message), "Erro de validação do CST IBS/CBS.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("CST IBS/CBS atualizado com sucesso!", new { cst.Id });
        }
    }

    public class DeletarCstIbsCbsCommandHandler : ICommandHandler<DeletarCstIbsCbsCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DeletarCstIbsCbsCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(DeletarCstIbsCbsCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var cst = await _context.CstsIbsCbs.FirstOrDefaultAsync(c => c.Id == request.Id && c.TenantId == tenantId && c.DeletadoEm == null, cancellationToken);
            if (cst == null)
                return CommandResult.Falha("CST IBS/CBS não encontrado.");

            cst.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("CST IBS/CBS deletado com sucesso!");
        }
    }

    public class AdicionarClassificacaoTributariaCommandHandler : ICommandHandler<AdicionarClassificacaoTributariaCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AdicionarClassificacaoTributariaCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AdicionarClassificacaoTributariaCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var cst = await _context.CstsIbsCbs
                .Include(c => c.ClassesTributarias)
                .FirstOrDefaultAsync(c => c.Id == request.CstIbsCbsId && c.DeletadoEm == null, cancellationToken);
            if (cst == null)
                return CommandResult.Falha("CST IBS/CBS não localizado.");

            var classe = request.Classe;
            var cctrib = cst.AdicionarClasseTributaria(
                classe.Codigo, classe.Descricao, classe.DataInicioVigencia, classe.DataFimVigencia,
                classe.IndNfe, classe.IndNfce, classe.IndCte, classe.IndCteos, classe.IndNfse, classe.IndTribRegular, usuario);

            if (classe.Anexos != null)
                foreach (var a in classe.Anexos)
                    cctrib.AdicionarAnexo(a.NroAnexo, a.Codigo, a.DataInicioVigencia, a.DataFimVigencia, usuario);

            if (!cctrib.IsValid)
                return CommandResult.Falha(cctrib.Notifications.Select(n => n.Message), "Erro de validação da Classificação Tributária.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Classificação Tributária adicionada com sucesso!", new { cctrib.Id });
        }
    }

    public class RemoverClassificacaoTributariaCommandHandler : ICommandHandler<RemoverClassificacaoTributariaCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public RemoverClassificacaoTributariaCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(RemoverClassificacaoTributariaCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var cst = await _context.CstsIbsCbs
                .Include(c => c.ClassesTributarias)
                .FirstOrDefaultAsync(c => c.Id == request.CstIbsCbsId && c.DeletadoEm == null, cancellationToken);
            if (cst == null)
                return CommandResult.Falha("CST IBS/CBS não localizado.");

            cst.RemoverClasseTributaria(request.ClassificacaoTributariaId, usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Classificação Tributária removida com sucesso!");
        }
    }
}
