using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Vendas.Application.Commands
{
    public record AbrirCaixaCommand(string OperadorId, decimal SaldoAbertura) : ICommand;
    public record FecharCaixaCommand(Guid CaixaId, decimal SaldoFechamento) : ICommand;
    public record RegistrarCaixaMovimentoCommand(Guid CaixaId, string Tipo, decimal Valor, string? Observacao) : ICommand;
}
