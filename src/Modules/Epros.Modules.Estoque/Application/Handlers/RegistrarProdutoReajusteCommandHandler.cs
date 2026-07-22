using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>
    /// Registra o reajuste de valor (venda/compra) de um produto e grava o histórico correspondente.
    /// A criação do <c>ProdutoHistoricoReajuste</c> é responsabilidade do domínio
    /// (<c>Produto.AlterarValorVenda/AlterarValorCompra</c>), mantendo o controller/handler finos.
    /// </summary>
    public class RegistrarProdutoReajusteCommandHandler : IRequestHandler<RegistrarProdutoReajusteCommand, CommandResult>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public RegistrarProdutoReajusteCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarProdutoReajusteCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == request.ProdutoId, cancellationToken);
            if (produto == null)
                return CommandResult.Falha("Produto não encontrado.");

            var tipo = (ETipoReajuste)request.Tipo;
            var valorAtual = tipo == ETipoReajuste.Valorcompra ? produto.ValorCompra : produto.ValorVenda;

            // ValorNovo explícito tem prioridade; senão calcula ValorAtual * Fator + ValorFixo (fluxo legado).
            var valorNovo = request.ValorNovo > 0
                ? request.ValorNovo
                : (valorAtual * (request.Fator == 0 ? 1 : request.Fator)) + request.ValorFixo;

            if (tipo == ETipoReajuste.Valorcompra)
                produto.AlterarValorCompra(valorAtual, request.Fator, request.ValorFixo, valorNovo, request.Motivo, usuario);
            else
                produto.AlterarValorVenda(valorAtual, request.Fator, request.ValorFixo, valorNovo, request.Motivo, usuario);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Reajuste registrado com sucesso!", new { produto.Id, ValorAntigo = valorAtual, ValorNovo = valorNovo });
        }
    }
}
