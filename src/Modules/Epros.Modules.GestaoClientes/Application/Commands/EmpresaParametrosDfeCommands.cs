using System;
using Epros.Shared.Application.Contracts;
using Epros.Modules.GestaoClientes.Domain.Entities;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    // DTOs de entrada dos blocos DF-e, espelhando os owned types portados do legado.
    public record ParametrosDfeNfeDto(
        int NfeSerieProducao,
        long NfeProximoNrProducao,
        int NfeSerieHomologacao,
        long NfeProximoNrHomologacao,
        decimal ValorAliquotaCreditoIcms,
        bool NfeGerarContingenciaEmHomologacao,
        bool IndicadorSt,
        bool EmitirNfeConjugada
    );

    public record ParametrosDfeNfceHomologacaoDto(
        string? NfceCscHomologacao,
        string? NfceIdCscHomologacao,
        int NfceSerieHomologacao,
        long NfceProximoNrHomologacao,
        bool NfceGerarContingenciaEmHomologacao
    );

    public record ParametrosDfeNfceProducaoDto(
        string? NfceCscProducao,
        string? NfceIdCscProducao,
        int NfceSerieProducao,
        long NfceProximoNrProducao
    );

    public record CriarEmpresaParametrosDfeCommand(
        Guid EmpresaId,
        bool DestacarIcmsSt,
        ParametrosDfeNfeDto? Nfe,
        ParametrosDfeNfceHomologacaoDto? NfceHomologacao,
        ParametrosDfeNfceProducaoDto? NfceProducao,
        ETipoAmbiente TipoAmbienteNfce,
        ETipoAmbiente TipoAmbienteNfe
    ) : ICommand;

    public record AtualizarEmpresaParametrosDfeCommand(
        Guid EmpresaId,
        bool DestacarIcmsSt,
        ParametrosDfeNfeDto? Nfe,
        ParametrosDfeNfceHomologacaoDto? NfceHomologacao,
        ParametrosDfeNfceProducaoDto? NfceProducao,
        ETipoAmbiente TipoAmbienteNfce,
        ETipoAmbiente TipoAmbienteNfe
    ) : ICommand;

    public record DeletarEmpresaParametrosDfeCommand(Guid EmpresaId) : ICommand;
}
