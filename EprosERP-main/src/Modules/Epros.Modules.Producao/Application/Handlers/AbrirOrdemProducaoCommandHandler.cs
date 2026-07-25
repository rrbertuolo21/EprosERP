using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Producao.Application.Commands;
using Epros.Modules.Producao.Domain.Entities;
using Epros.Modules.Producao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Application.Handlers
{
    public class AbrirOrdemProducaoCommandHandler : ICommandHandler<AbrirOrdemProducaoCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AbrirOrdemProducaoCommandHandler(
            ContextProducao context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AbrirOrdemProducaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // Validar se há uma BOM ativa para o produto
            var bomAtiva = await _context.ListasMateriais
                .AnyAsync(l => l.ProdutoAcabadoSku == request.ProdutoAcabadoSku && l.Ativa, cancellationToken);

            if (!bomAtiva)
            {
                return CommandResult.Falha($"Não é possível abrir uma Ordem de Produção para o SKU '{request.ProdutoAcabadoSku}' pois não há nenhuma ficha técnica (BOM) ativa cadastrada.");
            }

            var codigo = string.IsNullOrWhiteSpace(request.Codigo)
                ? "OP-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()
                : request.Codigo;

            // Verificar duplicidade de código para o mesmo inquilino
            var codigoDuplicado = await _context.OrdensProducao
                .AnyAsync(o => o.Codigo == codigo, cancellationToken);

            if (codigoDuplicado)
            {
                return CommandResult.Falha($"Código de Ordem de Produção '{codigo}' já está em uso.");
            }

            var ordem = new OrdemProducao(
                codigo,
                request.ProdutoAcabadoSku,
                request.QuantidadePlanejada,
                tenantId,
                usuario
            );

            if (!ordem.IsValid)
            {
                return CommandResult.Falha(ordem.Notifications.Select(n => n.Message));
            }

            _context.OrdensProducao.Add(ordem);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Ordem de Produção aberta com sucesso!", new { OrdemProducaoId = ordem.Id, Codigo = ordem.Codigo });
        }
    }
}
