using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Estoque.Application.Services;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Tests
{
    /// <summary>
    /// Helper de testes: semeia saldo inicial de estoque SEMPRE pelo motor único (kardex), refletindo o
    /// novo caminho de produção (D1). Substitui o antigo Produto.LancarEntradaEstoque nos setups de teste.
    /// </summary>
    internal static class EstoqueTestSeed
    {
        public static async Task SemearSaldoAsync(
            ContextEstoque context,
            string tenantId,
            string usuario,
            Guid produtoId,
            decimal quantidade,
            decimal custoUnitario,
            Guid? empresaId = null,
            ETipoCusteioEstoque custeio = ETipoCusteioEstoque.CustoMedio,
            CancellationToken ct = default)
        {
            var empresa = empresaId ?? MotorMovimentacaoEstoque.EmpresaPadrao;
            var motor = new MotorMovimentacaoEstoque(context, tenantId, usuario);

            var fato = new FatoGeradorEstoque(null, null, null, EOrigemFatoGeradorEstoque.SaldoInicial, tenantId, usuario, referenciaExterna: "seed-teste");
            context.FatosGeradoresEstoque.Add(fato);

            var resultado = await motor.AplicarEntradaAsync(
                empresa, produtoId, ETipoEstoque.Geral, quantidade, custoUnitario, fato.Id, null, null, null, custeio, ct);
            if (!resultado.Sucesso)
                throw new InvalidOperationException($"Falha ao semear saldo de teste: {resultado.Erro}");

            await context.SaveChangesAsync(ct);
        }
    }
}
