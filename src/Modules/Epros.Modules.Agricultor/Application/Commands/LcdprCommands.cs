using System;
using System.Collections.Generic;
using Epros.Modules.Agricultor.Domain.Enums;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Agricultor.Application.Commands
{
    // ===================== Escrituração (0000/0010) =====================

    public record AbrirEscrituracaoCommand(
        string Cpf, string Nome, DateTime DtIni, DateTime DtFin,
        int IndSituacaoInicioPeriodo, int SituacaoEspecial, EFormaApuracao FormaApuracao) : ICommand;

    public class AbrirEscrituracaoCommandValidator : AbstractValidator<AbrirEscrituracaoCommand>
    {
        public AbrirEscrituracaoCommandValidator()
        {
            RuleFor(c => c.Cpf).NotEmpty().Length(11).WithMessage("CPF deve ter 11 dígitos.");
            RuleFor(c => c.Nome).NotEmpty();
            RuleFor(c => c.DtFin).GreaterThanOrEqualTo(c => c.DtIni);
        }
    }

    // ===================== 0030 =====================

    public record DefinirDadosCadastraisCommand(
        Guid EscrituracaoId, string? Endereco, string? Uf, string? CodMunicipio, string? Cep, string? Email) : ICommand;

    // ===================== 0040 (+0045) =====================

    public record TerceiroInput(int TipoContraparte, string IdContraparte, string NomeContraparte, decimal PercContraparte);

    public record AdicionarImovelLcdprCommand(
        Guid EscrituracaoId, int CodImovel, string NomeImovel, string? CadItrCafir, string? Caepf,
        string? Uf, string? CodMunicipio, ETipoExploracao TipoExploracao, decimal Participacao,
        List<TerceiroInput>? Terceiros,
        // Bloco de endereço do 0040 (leiaute 1.3): ENDERECO/NUM/COMPL/BAIRRO/CEP.
        string? Endereco = null, string? Num = null, string? Compl = null, string? Bairro = null, string? Cep = null) : ICommand;

    // ===================== 0050 =====================

    public record AdicionarContaLcdprCommand(
        Guid EscrituracaoId, int CodConta, int? Banco, int? Agencia, string? NumConta) : ICommand;

    // ===================== Q100 =====================

    public record RegistrarLancamentoCommand(
        Guid EscrituracaoId, int CodImovel, int CodConta, DateTime Data, ETipoDocumentoLcdpr TipoDoc,
        string? NumDoc, string? Historico, string? IdPartic, ETipoLancamentoLcdpr TipoLanc,
        decimal VlEntrada, decimal VlSaida) : ICommand;

    public class RegistrarLancamentoCommandValidator : AbstractValidator<RegistrarLancamentoCommand>
    {
        public RegistrarLancamentoCommandValidator()
        {
            RuleFor(c => c.EscrituracaoId).NotEmpty();
            RuleFor(c => c.VlEntrada).GreaterThanOrEqualTo(0);
            RuleFor(c => c.VlSaida).GreaterThanOrEqualTo(0);
        }
    }

    // ===================== Ciclo / geração =====================

    public record FecharEscrituracaoCommand(Guid EscrituracaoId, string IdentificacaoNome, string IdentificacaoCpfCnpj) : ICommand;

    /// <summary>Gera o arquivo .txt LCDPR (após validar). Retorna conteúdo, hash e nome (AGR-D14/D20).</summary>
    public record GerarArquivoLcdprCommand(Guid EscrituracaoId) : ICommand;

    public record ReabrirRetificadoraCommand(Guid EscrituracaoId) : ICommand;

    /// <summary>AGR-D10: define o limite de obrigatoriedade por ano-calendário (nunca hardcoded). // valida-contador.</summary>
    public record DefinirParamObrigatoriedadeCommand(int Ano, decimal LimiteValor, string? Origem) : ICommand;
}
