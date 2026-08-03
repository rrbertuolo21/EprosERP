using System;
using Epros.Modules.GestaoClientes.Domain.Entities;

namespace Epros.Modules.GestaoClientes.Application.Services
{
    /// <summary>
    /// 1.08E — Ponto ÚNICO de aplicação de um <see cref="Cupom"/> sobre um valor base (não duplica a
    /// matemática de desconto: apenas orquestra <see cref="Cupom.Validar"/> + <see cref="Cupom.CalcularDesconto"/>).
    /// Reutilizado tanto pelo pedido inicial (Aplicativo) quanto pela geração da fatura do ciclo (renovação).
    /// </summary>
    public static class AplicacaoCupom
    {
        /// <summary>Resultado do cálculo do desconto de um cupom sobre um valor base.</summary>
        public readonly record struct Resultado(bool Aplicou, decimal Desconto, decimal ValorFinal);

        /// <summary>
        /// Valida o cupom (validade + limite de uso + ativo) e calcula o desconto sobre <paramref name="valorBase"/>.
        /// NÃO registra uso nem incrementa o cupom — o chamador decide o efeito colateral.
        /// Cupom nulo ou inválido → <c>Aplicou = false</c>, desconto 0 e valor final = base.
        /// </summary>
        public static Resultado Calcular(Cupom? cupom, decimal valorBase)
        {
            var piso = Math.Max(0m, valorBase);
            if (cupom == null || !cupom.Validar())
                return new Resultado(false, 0m, piso);

            var desconto = cupom.CalcularDesconto(valorBase);
            return new Resultado(true, desconto, Math.Max(0m, valorBase - desconto));
        }
    }
}
