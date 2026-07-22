using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Fiscal.Application.Commands
{
    /// <summary>Exclui (soft-delete) um CFOP pelo seu identificador.</summary>
    /// <param name="Id">Identificador do CFOP.</param>
    public record DeletarCfopCommand(Guid Id) : ICommand;

    /// <summary>Exclui (soft-delete) um Tipo de Operação Fiscal pelo seu identificador.</summary>
    /// <param name="Id">Identificador do tipo de operação fiscal.</param>
    public record DeletarTipoOperacaoFiscalCommand(Guid Id) : ICommand;

    /// <summary>Exclui (soft-delete) um Código de Benefício Fiscal pelo seu identificador.</summary>
    /// <param name="Id">Identificador do código de benefício fiscal.</param>
    public record DeletarCodigoBeneficioFiscalCommand(Guid Id) : ICommand;
}
