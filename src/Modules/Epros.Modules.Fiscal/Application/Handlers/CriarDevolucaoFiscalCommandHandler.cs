using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    /// <summary>Cria a devolução fiscal em estado NOVO com seus itens (EF_DEVOLUCAO_FISCAL 8.1/8.2/8.3).</summary>
    public class CriarDevolucaoFiscalCommandHandler : ICommandHandler<CriarDevolucaoFiscalCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarDevolucaoFiscalCommandHandler(
            ContextFiscal context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarDevolucaoFiscalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var devolucao = new DevolucaoFiscal(
                request.Modelo,
                request.Ambiente,
                request.Serie,
                request.ChaveNfEntrada,
                request.Motivo,
                request.DestinatarioCnpjCpf,
                request.DestinatarioNome,
                request.Total,
                request.EmpresaId,
                request.DocumentoOrigemId,
                request.XmlEntrada,
                tenantId,
                usuario);

            if (!devolucao.IsValid)
                return CommandResult.Falha(devolucao.Notifications.Select(n => n.Message), "Dados da devolução fiscal são inválidos.");

            foreach (var item in request.Itens)
            {
                devolucao.AdicionarItem(
                    item.ProdutoId, item.Sku, item.NomeProduto, item.Ncm, item.Cfop, item.Cst,
                    item.Quantidade, item.ValorUnitario, item.AliquotaIcms, usuario);
            }

            if (!devolucao.IsValid)
                return CommandResult.Falha(devolucao.Notifications.Select(n => n.Message), "Erro ao validar itens da devolução.");

            _context.DevolucoesFiscais.Add(devolucao);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Devolução fiscal criada em estado NOVO.", new { devolucao.Id, Estado = devolucao.Estado.ToString() });
        }
    }
}
