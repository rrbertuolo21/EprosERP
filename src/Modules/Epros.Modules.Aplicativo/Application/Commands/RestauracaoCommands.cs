using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    /// <summary>
    /// REG-016 — entidades-chave do control-plane que admitem restauração GOVERNADA de soft-delete.
    /// Restringe o alvo a um conjunto fechado (nega por padrão): não é um "undelete" genérico de qualquer
    /// tabela, e sim uma operação de operador interno sobre cadastros centrais.
    /// </summary>
    public enum EntidadeRestauravel
    {
        Cliente,
        Plano,
        Cupom,
        Usuario
    }

    /// <summary>
    /// REG-016 — comando de restauração de uma entidade soft-deleted das entidades-chave. Idempotente:
    /// restaurar algo que já está ativo é no-op. Respeita o tenant corrente (só enxerga a linha do tenant,
    /// exceto entidades globais). Autorização é do endpoint (operador interno / SuperAdmin).
    /// </summary>
    public record RestaurarEntidadeCommand(EntidadeRestauravel Tipo, Guid Id) : ICommand;
}
