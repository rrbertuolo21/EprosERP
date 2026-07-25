using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Services;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>
    /// Cria e aplica um ajuste de estoque com motivo e auditoria (MVM-025 / VAL-MVM-015).
    /// Item positivo é tratado como entrada; negativo como saída controlada (EF §7.8), aplicado
    /// pelo motor de estoque em transação única, gerando fato gerador de ajuste e histórico.
    /// </summary>
    public class CriarAjusteEstoqueCommandHandler : ICommandHandler<CriarAjusteEstoqueCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarAjusteEstoqueCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarAjusteEstoqueCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (request.Itens == null || request.Itens.Count == 0)
                return CommandResult.Falha("O ajuste deve possuir ao menos um item.");

            // MVM-025: motivo obrigatório (validado no domínio).
            var ajuste = new AjusteEstoque(request.LocalId, request.DataAjuste, request.TipoAjuste, null, request.ValorRecuperado, request.Observacao, tenantId, usuario);
            if (!ajuste.IsValid)
                return CommandResult.Falha(ajuste.Notifications.Select(n => n.Message), "Dados do ajuste são inválidos.");

            _context.AjustesEstoque.Add(ajuste);

            var fato = new FatoGeradorEstoque(null, null, null, EOrigemFatoGeradorEstoque.AjusteEstoque, tenantId, usuario, referenciaExterna: $"Ajuste {ajuste.Id}");
            _context.FatosGeradoresEstoque.Add(fato);

            var motor = new MotorMovimentacaoEstoque(_context, tenantId, usuario);
            decimal valorTotal = 0m;

            foreach (var input in request.Itens)
            {
                var item = new AjusteEstoqueItem(ajuste.Id, input.ProdutoId, input.Quantidade, input.ValorUnitario, input.Lote, null, tenantId, usuario);
                _context.AjustesEstoqueItens.Add(item);

                var custeioPadrao = await _context.EstoqueProdutos
                    .Where(e => e.EmpresaId == request.EmpresaId && e.ProdutoId == input.ProdutoId)
                    .Select(e => (ETipoCusteioEstoque?)e.TipoCusteioEstoque)
                    .FirstOrDefaultAsync(cancellationToken) ?? ETipoCusteioEstoque.CustoMedio;

                ResultadoMovimentacao resultado = input.Quantidade >= 0m
                    ? await motor.AplicarEntradaAsync(request.EmpresaId, input.ProdutoId, ETipoEstoque.Geral, Math.Abs(input.Quantidade), input.ValorUnitario, fato.Id, request.LocalId, input.Lote, null, custeioPadrao, cancellationToken)
                    : await motor.AplicarSaidaAsync(request.EmpresaId, input.ProdutoId, Math.Abs(input.Quantidade), fato.Id, request.LocalId, cancellationToken);

                if (!resultado.Sucesso)
                    return CommandResult.Falha(resultado.Erro ?? "Falha ao aplicar item de ajuste.");

                valorTotal += Math.Abs(input.Quantidade) * input.ValorUnitario;
            }

            ajuste.Aplicar(usuario);

            CriarEstoqueMovimentoManualCommandHandler.RegistrarHistorico(_context, "AjusteEstoque", ajuste.Id, "ajuste_aplicado", "Rascunho", "Aplicado", request.Observacao, usuario, tenantId);
            CriarEstoqueMovimentoManualCommandHandler.PublicarEvento(_context, tenantId, "est.mvm.ajuste_aplicado", new { ajuste.Id, request.Observacao, valorTotal, usuario });

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Ajuste de estoque aplicado com sucesso!", new { ajuste.Id, valorTotal });
        }
    }
}
