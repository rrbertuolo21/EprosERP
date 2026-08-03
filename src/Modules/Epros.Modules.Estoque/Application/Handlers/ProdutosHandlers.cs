using System;
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
    /// <summary>Cria Produto.</summary>
    public class CriarProdutoCommandHandler : ICommandHandler<CriarProdutoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarProdutoCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarProdutoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            // Verificar SKU duplicado
            var skuExistente = await _context.Produtos.AnyAsync(p => p.Codigo == request.Codigo, cancellationToken);
            if (skuExistente)
            {
                return CommandResult.Falha("Já existe um produto cadastrado com este SKU/Código.");
            }

            var produto = new Produto(
                categoriaId: request.CategoriaId,
                marcaProdutoId: request.MarcaProdutoId,
                unidadeMedidaComercialId: request.UnidadeMedidaComercialId,
                codigo: request.Codigo,
                descricao: request.Descricao,
                ean: request.Ean,
                pesoLiquido: request.PesoLiquido,
                pesoBruto: request.PesoBruto,
                valorVenda: request.ValorVenda,
                valorVendaPrazo: request.ValorVendaPrazo,
                valorCompra: request.ValorCompra,
                tipoProduto: request.TipoProduto,
                ativo: request.Ativo,
                imagem: request.Imagem,
                cestId: request.CestId,
                codigoAnpId: request.CodigoAnpId,
                balancaId: request.BalancaId,
                utilizaBalanca: request.UtilizaBalanca,
                codigoProdutoBalanca: request.CodigoProdutoBalanca,
                ncmId: request.NcmId,
                tenantId: tenantId,
                criadoPor: criadoPor
            );

            if (!produto.IsValid)
            {
                return CommandResult.Falha(produto.Notifications.Select(n => n.Message), "Falha na validação do produto.");
            }

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Produto criado com sucesso!", new { ProdutoId = produto.Id });
        }
    }

    /// <summary>Atualiza Produto.</summary>
    public class AtualizarProdutoCommandHandler : ICommandHandler<AtualizarProdutoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarProdutoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarProdutoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            if (produto == null)
            {
                return CommandResult.Falha("Produto não encontrado.");
            }

            // Verificar se código mudou e colide
            if (produto.Codigo != request.Codigo)
            {
                var skuExistente = await _context.Produtos.AnyAsync(p => p.Codigo == request.Codigo && p.Id != request.Id, cancellationToken);
                if (skuExistente)
                {
                    return CommandResult.Falha("Já existe outro produto cadastrado com este SKU/Código.");
                }
            }

            produto.Alterar(
                categoriaId: request.CategoriaId,
                marcaProdutoId: request.MarcaProdutoId,
                unidadeMedidaComercialId: request.UnidadeMedidaComercialId,
                codigo: request.Codigo,
                descricao: request.Descricao,
                ean: request.Ean,
                pesoLiquido: request.PesoLiquido,
                pesoBruto: request.PesoBruto,
                valorVenda: request.ValorVenda,
                valorVendaPrazo: request.ValorVendaPrazo,
                valorCompra: request.ValorCompra,
                tipoProduto: request.TipoProduto,
                ativo: request.Ativo,
                cestId: request.CestId,
                codigoAnpId: request.CodigoAnpId,
                balancaId: request.BalancaId,
                utilizaBalanca: request.UtilizaBalanca,
                codigoProdutoBalanca: request.CodigoProdutoBalanca,
                ncmId: request.NcmId,
                usuario: usuario
            );

            if (!produto.IsValid)
            {
                return CommandResult.Falha(produto.Notifications.Select(n => n.Message), "Falha na validação do produto.");
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Produto atualizado com sucesso!");
        }
    }

    /// <summary>Exclui Produto.</summary>
    public class DeletarProdutoCommandHandler : ICommandHandler<DeletarProdutoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public DeletarProdutoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarProdutoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            if (produto == null)
            {
                return CommandResult.Falha("Produto não encontrado.");
            }

            // GUARDA DE USO (espelha a exclusão de Pessoa): não permite soft-delete de produto que já
            // participa de compra, tem saldo em estoque ou já gerou movimento/kardex. O produto deve ser
            // INATIVADO, nunca excluído, para preservar a integridade histórica (compras, custo, valorização).
            var emCompra = await _context.CompraItens.AnyAsync(ci => ci.ProdutoId == request.Id, cancellationToken);
            if (emCompra)
                return CommandResult.Falha("Não é possível excluir um produto com compras vinculadas. Ele deve ser inativado.");

            var temSaldo = await _context.EstoqueProdutos.AnyAsync(e => e.ProdutoId == request.Id && e.QuantidadeSaldoEstoque != 0m, cancellationToken);
            if (temSaldo)
                return CommandResult.Falha("Não é possível excluir um produto com saldo em estoque. Zere o saldo e inative o produto.");

            // Movimento/kardex: MovimentosEstoque registra as saídas/entradas físicas (inclui as de VENDA
            // faturada e de compra); as fichas são o kardex valorizado. Qualquer histórico bloqueia a exclusão.
            var temMovimento = await _context.MovimentosEstoque.AnyAsync(m => m.ProdutoId == request.Id, cancellationToken);
            var temFichaEntrada = await _context.ProdutoFichaEstoqueEntradas.AnyAsync(f => f.ProdutoId == request.Id, cancellationToken);
            var temFichaSaida = await _context.ProdutoFichaEstoqueSaidas.AnyAsync(f => f.ProdutoId == request.Id, cancellationToken);
            if (temMovimento || temFichaEntrada || temFichaSaida)
                return CommandResult.Falha("Não é possível excluir um produto com movimentações de estoque (histórico/kardex). Ele deve ser inativado.");

            produto.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Produto excluído com sucesso!");
        }
    }
}
