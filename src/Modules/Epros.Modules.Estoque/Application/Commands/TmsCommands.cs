using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;

namespace Epros.Modules.Estoque.Application.Commands
{
    /// <summary>
    /// Comandos do submódulo Transporte e Frete TMS (EST-TMS). Reutiliza o agregado CompraTransporte
    /// (transportadora/veículo/reboque/volume). TMS-001: ao menos transportadora, veículo ou volume.
    /// Sobe desabilitado (ABAC nega por padrão).
    /// </summary>
    public record TmsTransportadoraInput(
        Guid? PessoaId,
        string? Cnpj,
        string? Cpf,
        string? RazaoSocial,
        string? InscricaoEstadual,
        string? EnderecoCompleto,
        string? NomeMunicipio,
        EEstado Uf
    );

    public record TmsVeiculoInput(Guid? VeiculoId, string Placa, EEstado Uf, string? Rntrc);

    public record TmsReboqueInput(Guid? VeiculoId, string Placa, EEstado Uf, string? Rntrc);

    public record TmsVolumeInput(int QuantidadeVolumes, string? Especie, string? NumeroVolumes, decimal PesoLiquido, decimal PesoBruto, string? Marca);

    public record CriarTmsTransporteCompraCommand(
        Guid CompraId,
        TmsTransportadoraInput? Transportadora = null,
        TmsVeiculoInput? Veiculo = null,
        List<TmsReboqueInput>? Reboques = null,
        List<TmsVolumeInput>? Volumes = null
    ) : ICommand;

    public record ConfirmarTmsTransporteCompraCommand(Guid Id) : ICommand;

    public record CancelarTmsTransporteCompraCommand(Guid Id) : ICommand;
}
