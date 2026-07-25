using System;
using Epros.Shared.Application.Contracts;
using Epros.Modules.GestaoClientes.Domain.Entities;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record EmpresaEnderecoDto(
        string Logradouro,
        string Numero,
        string? Complemento,
        string Bairro,
        string Cep,
        string Cidade,
        string Estado
    );

    public record CriarEmpresaCommand(
        string RazaoSocial,
        string? NomeFantasia,
        string Cnpj,
        string? InscricaoEstadual,
        string? InscricaoMunicipal,
        string? InscricaoSuframa,
        string? Cnae,
        RegimeTributario RegimeTributario,
        RegimeApuracao RegimeApuracao,
        Guid? PessoaGrupoId,
        Guid? ProdutoGrupoId,
        Guid? PlanoContasFinanceiroId,
        Guid? TributarioGrupoId,
        Guid? NcmTributacaoId,
        Guid? CertificadoDigitalId,
        Guid? EmpresaParametrosDfeId,
        string? LinkWebApiAppVendas,
        string? TokenMercadoPagoPix,
        string? Logo,
        EmpresaEnderecoDto Endereco,
        string? DateFormat = "MM-DD-YYYY",
        Guid? TimeZoneId = null,
        Guid? CurrencyId = null
    ) : ICommand;
}
