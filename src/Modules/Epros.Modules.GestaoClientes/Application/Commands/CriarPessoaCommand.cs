using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using Epros.Modules.GestaoClientes.Domain.Entities;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record EnderecoDto(
        ETipoEndereco TipoEndereco,
        Guid PaisId,
        Guid MunicipioId,
        Guid? SubdivisaoId,
        string Uf,
        string? Cep,
        string Logradouro,
        string? Numero,
        string? Complemento,
        string Bairro,
        string? Referencia,
        string? CodigoPostalInternacional = null,
        string? LinhaEndereco1 = null,
        string? LinhaEndereco2 = null,
        decimal? Latitude = null,
        decimal? Longitude = null
    );

    public record ContatoDto(
        string? Nome,
        ETipoContatoTelefonico TipoContatoTelefonico,
        string? NumeroTelefone,
        ETipoContatoEmail TipoContatoEmail,
        string? Email,
        bool EhPrincipal
    );

    public record VeiculoDto(
        Guid PaisId,
        ETipoVeiculo TipoVeiculo,
        string Uf,
        string Placa,
        string? Rntrc
    );

    public record CriarPessoaCommand(
        ETipoPessoa TipoPessoa,
        ETipoIndicadorIe TipoIndicadorIe,
        Guid? PessoaGrupoId,
        long? InscricaoSuframa,
        string? TitularContaBancaria,
        string? AgenciaContaBancaria,
        string? NumeroContaBancaria,
        ETipoPix? TipoPix,
        string? ChavePix,
        string? Observacoes,

        // Fisica
        string? FisicaCpf,
        string? FisicaNome,
        string? FisicaSobrenome,
        string? RgNumero,
        string? RgOrgaoEmissor,
        ETipoGenero? TipoGenero,
        DateTime? DataNascimento,

        // Juridica
        string? JuridicaCnpj,
        string? RazaoSocial,
        string? NomeFantasia,
        string? InscricaoEstadual,
        string? InscricaoMunicipal,
        string? Cnae,

        // Estrangeiro
        string? EstrangeiroNome,
        string? IdentificacaoEstrangeiro,

        // Roles flags
        bool EhCliente,
        bool EhFornecedor,
        bool EhTransportadora,
        bool EhMotorista,
        bool EhPrestadorServico,
        bool EhFuncionario,
        bool EhProdutorRural,

        // Role data
        bool? ClienteEhConsumidorFinal,
        ETipoContribuinte? ClienteTipoContribuinte,
        ETipoCargo? FuncionarioTipoCargo,
        decimal? FuncionarioComissao,
        ETipoVinculoMotorista? MotoristaTipoVinculo,
        ETipoCategoriaCnh? MotoristaCategoriaCnh,
        DateTime? MotoristaDataEmissaoCnh,
        DateTime? MotoristaDataVencimentoCnh,
        string? MotoristaRntrc,
        string? TransportadoraCiot,
        string? TransportadoraRntrc,
        string? PrestadorCei,

        // Collections
        List<EnderecoDto>? Enderecos,
        List<ContatoDto>? Contatos,
        List<VeiculoDto>? Veiculos
    ) : ICommand;
}
