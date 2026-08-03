using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    /// <summary>
    /// 1.08B — Adiciona um cartão-on-file ao cliente do tenant corrente. ⛔ PCI: recebe apenas o TOKEN do
    /// cartão gerado pela lib do Mercado Pago no FRONT — PAN/CVV nunca chegam ao backend.
    /// </summary>
    public record AdicionarCartaoCommand(string CardToken, bool DefinirComoPadrao = true) : ICommand;

    /// <summary>1.08B — Remove (desativa) um meio de pagamento salvo do cliente do tenant corrente.</summary>
    public record RemoverMeioPagamentoCommand(Guid MeioPagamentoId) : ICommand;

    /// <summary>1.08B — Define um meio de pagamento salvo como padrão (débito automático) do cliente.</summary>
    public record DefinirMeioPagamentoPadraoCommand(Guid MeioPagamentoId) : ICommand;
}
