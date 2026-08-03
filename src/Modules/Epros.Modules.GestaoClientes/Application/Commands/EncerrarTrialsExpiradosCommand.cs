using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    /// <summary>
    /// 1.08A — Encerra os trials expirados do tenant corrente: gera a 1ª fatura do plano e dispara a
    /// cobrança (método-agnóstico). Idempotente por <see cref="Domain.Entities.AssinaturaCliente.TrialConvertidoEm"/>.
    /// </summary>
    public record EncerrarTrialsExpiradosCommand(DateTime? Referencia = null) : ICommand;
}
