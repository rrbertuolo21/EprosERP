using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using Epros.Modules.GestaoClientes.Domain.Entities;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record AtualizarPessoaCommand(
        Guid Id,
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
