using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Application.Models;
using MediatR;

namespace Epros.Modules.Financeiro.Application.Commands
{
    // ----- Configuração Cedente -----
    public record CriarConfiguracaoCedenteCommand(
        Guid EmpresaId, string Nome, string? Email, string? Documento, string? Endereco, string? Numero,
        string? Bairro, string? Cidade, string? Cep, string? UF, string? Logo,
        int ReceberAteDias, int DiasAntecedencia, decimal MultaAtraso, decimal Juro,
        string? Instrucao1, string? Instrucao2, string? Instrucao3, string? Instrucao4) : IRequest<CommandResult>;

    public record AtualizarConfiguracaoCedenteCommand(
        Guid Id, string Nome, string? Email, string? Documento, string? Endereco, string? Numero,
        string? Bairro, string? Cidade, string? Cep, string? UF, string? Logo,
        int ReceberAteDias, int DiasAntecedencia, decimal MultaAtraso, decimal Juro,
        string? Instrucao1, string? Instrucao2, string? Instrucao3, string? Instrucao4) : IRequest<CommandResult>;

    // ----- Conta Emissora -----
    public record CriarContaEmissoraCommand(
        Guid BancoId, Guid? ConfiguracaoCedenteId, string? NomeBanco, string? Carteira, string? Agencia,
        string? DigitoAgencia, string? Conta, string? DigitoConta, string? Especie, long NossoNumeroAtual,
        string? TipoCobranca, string? Convenio, string? Contrato, string? TipoCarteira, long IncrementoNossoNumero,
        string? TipoRemessa, string? CodigoCliente, string? Posto, bool Ativa) : IRequest<CommandResult>;

    public record AtualizarContaEmissoraCommand(
        Guid Id, Guid BancoId, Guid? ConfiguracaoCedenteId, string? NomeBanco, string? Carteira, string? Agencia,
        string? DigitoAgencia, string? Conta, string? DigitoConta, string? Especie,
        string? TipoCobranca, string? Convenio, string? Contrato, string? TipoCarteira, long IncrementoNossoNumero,
        string? TipoRemessa, string? CodigoCliente, string? Posto, bool Ativa) : IRequest<CommandResult>;

    public record AtivarContaEmissoraCommand(Guid Id) : IRequest<CommandResult>;

    // ----- Grupo de Recorrência -----
    public record CriarGrupoRecorrenciaCommand(string Descricao, int Meses, int DiaVencimento, decimal Valor) : IRequest<CommandResult>;
    public record AtualizarGrupoRecorrenciaCommand(Guid Id, string Descricao, int Meses, int DiaVencimento, decimal Valor) : IRequest<CommandResult>;

    // ----- Sacado -----
    public record CriarSacadoCommand(
        Guid? PessoaId, Guid? GrupoRecorrenciaId, string Nome, string? Documento, string? RG, string? Inscricao,
        string? Endereco, string? Numero, string? Complemento, string? Bairro, string? Cidade, string? UF,
        string? CEP, string? Telefone, string? Email, string? Observacao, decimal Valor) : IRequest<CommandResult>;

    public record AtualizarSacadoCommand(
        Guid Id, Guid? PessoaId, Guid? GrupoRecorrenciaId, string Nome, string? Documento, string? RG, string? Inscricao,
        string? Endereco, string? Numero, string? Complemento, string? Bairro, string? Cidade, string? UF,
        string? CEP, string? Telefone, string? Email, string? Observacao, decimal Valor) : IRequest<CommandResult>;

    public record BloquearSacadoCommand(Guid Id, bool Bloquear) : IRequest<CommandResult>;

    // ----- Fatura de Cobrança -----
    public record CriarFaturaCobrancaCommand(
        Guid SacadoId, Guid? GrupoRecorrenciaId, string? Referencia, string? NumeroDocumento,
        DateTime Data, DateTime DataVencimento, decimal Valor, string? Email, ETipoFaturaCobranca TipoFatura) : IRequest<CommandResult>;

    public record BaixarFaturaCobrancaCommand(Guid Id, DateTime DataBaixa, decimal ValorRecebido) : IRequest<CommandResult>;

    // ----- Boleto -----
    public record EmitirBoletoCommand(
        Guid FaturaCobrancaId, Guid ContaEmissoraId, string? NumeroDocumento, decimal Valor, DateTime DataVencimento,
        string? LinhaDigitavel, string? Arquivo, decimal Multa, decimal Juros,
        string? Instrucao1, string? Instrucao2, string? Instrucao3, string? Instrucao4) : IRequest<CommandResult>;

    // ----- Remessa -----
    public record GerarRemessaCommand(string NomeArquivo, DateTime DataGeracao, int Grupo, ELayoutCnab Layout, Guid ContaEmissoraId) : IRequest<CommandResult>;
    public record AdicionarBoletoRemessaCommand(Guid RemessaId, Guid BoletoId) : IRequest<CommandResult>;

    // ----- Cobrança por E-mail -----
    public record CriarCobrancaEmailCommand(
        Guid? SacadoId, string Nome, decimal Valor, string? Periodo, string? Servicos, string? Conta,
        string? LinkExterno, string? Observacao, string? Emails) : IRequest<CommandResult>;

    public record TransicionarCobrancaEmailCommand(Guid Id, string Acao, string? Comprovante) : IRequest<CommandResult>;
}
