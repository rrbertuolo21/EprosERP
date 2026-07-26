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
    public class CriarListaMateriaisCommandHandler : ICommandHandler<CriarListaMateriaisCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarListaMateriaisCommandHandler(
            ContextProducao context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarListaMateriaisCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // Inativar qualquer lista ativa anterior para o mesmo produto acabado
            var listasAtivasAnteriores = await _context.ListasMateriais
                .Where(l => l.ProdutoAcabadoSku == request.ProdutoAcabadoSku && l.Ativa)
                .ToListAsync(cancellationToken);

            foreach (var listaAnterior in listasAtivasAnteriores)
            {
                listaAnterior.Inativar(usuario);
                _context.ListasMateriais.Update(listaAnterior);
            }

            // Criar a nova lista de materiais (BOM)
            var novaLista = new ListaMateriais(
                request.ProdutoAcabadoSku,
                request.Descricao,
                request.Versao,
                tenantId,
                usuario
            );

            foreach (var item in request.Itens)
            {
                novaLista.AdicionarItem(item.InsumoSku, item.QuantidadeNecessaria, item.UnidadeMedida, usuario);
            }

            if (!novaLista.IsValid)
            {
                return CommandResult.Falha(novaLista.Notifications.Select(n => n.Message));
            }

            _context.ListasMateriais.Add(novaLista);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Lista de materiais (BOM) criada com sucesso!", new { ListaMateriaisId = novaLista.Id, ProdutoAcabadoSku = novaLista.ProdutoAcabadoSku });
        }
    }
}
