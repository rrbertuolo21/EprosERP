using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>Cria Estoque Produto.</summary>
    public class CriarEstoqueProdutoCommandHandler : ICommandHandler<CriarEstoqueProdutoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarEstoqueProdutoCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarEstoqueProdutoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var jaExiste = await _context.EstoqueProdutos
                .AnyAsync(e => e.EmpresaId == request.EmpresaId && e.ProdutoId == request.ProdutoId, cancellationToken);
            if (jaExiste)
                return CommandResult.Falha("Já existe controle de estoque para este produto nesta empresa.");

            var estoque = new EstoqueProduto(request.EmpresaId, request.ProdutoId, request.QuantidadeSaldoEstoque, request.QuantidadeEstoqueMinimo, request.QuantidadeEstoqueMaximo, request.QuantidadeEstoqueReservado, request.ValorSaldo, request.TipoCusteioEstoque, tenantId, criadoPor);

            _context.EstoqueProdutos.Add(estoque);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Estoque do produto criado com sucesso!", new { estoque.Id });
        }
    }

    /// <summary>Atualiza Estoque Produto.</summary>
    public class AtualizarEstoqueProdutoCommandHandler : ICommandHandler<AtualizarEstoqueProdutoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarEstoqueProdutoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarEstoqueProdutoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var estoque = await _context.EstoqueProdutos.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
            if (estoque == null)
                return CommandResult.Falha("Estoque do produto não encontrado.");

            estoque.Alterar(request.EmpresaId, request.ProdutoId, request.QuantidadeSaldoEstoque, request.QuantidadeEstoqueMinimo, request.QuantidadeEstoqueMaximo, request.QuantidadeEstoqueReservado, request.ValorSaldo, request.TipoCusteioEstoque, usuario);

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Estoque do produto atualizado com sucesso!");
        }
    }

    /// <summary>Altera Dados Autorizados Estoque Produto.</summary>
    public class AlterarDadosAutorizadosEstoqueProdutoCommandHandler : ICommandHandler<AlterarDadosAutorizadosEstoqueProdutoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public AlterarDadosAutorizadosEstoqueProdutoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AlterarDadosAutorizadosEstoqueProdutoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var estoque = await _context.EstoqueProdutos.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
            if (estoque == null)
                return CommandResult.Falha("Estoque do produto não encontrado.");

            estoque.AlterarDadosAutorizados(request.EmpresaId, request.ProdutoId, request.QuantidadeEstoqueMinimo, request.QuantidadeEstoqueMaximo, request.TipoCusteioEstoque, usuario);

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Dados de estoque atualizados com sucesso!");
        }
    }

    /// <summary>Exclui Estoque Produto.</summary>
    public class DeletarEstoqueProdutoCommandHandler : ICommandHandler<DeletarEstoqueProdutoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public DeletarEstoqueProdutoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarEstoqueProdutoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var estoque = await _context.EstoqueProdutos.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
            if (estoque == null)
                return CommandResult.Falha("Estoque do produto não encontrado.");

            estoque.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Estoque do produto excluído com sucesso!");
        }
    }
}
