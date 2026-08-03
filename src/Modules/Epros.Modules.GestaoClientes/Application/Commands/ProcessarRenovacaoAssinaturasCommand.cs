using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    /// <summary>
    /// 1.08B — Renova as assinaturas cujo ciclo de cobrança venceu (<c>ProximaCobrancaEm &lt;= referência</c>),
    /// gerando a próxima fatura conforme a duração do plano (Mensal → mensal; Anual → anual; Vitalícia → nunca,
    /// cobrança única) e disparando a cobrança método-agnóstica (cartão-on-file → débito; senão PIX). Idempotente.
    /// </summary>
    public record ProcessarRenovacaoAssinaturasCommand(DateTime? Referencia = null) : ICommand;
}
