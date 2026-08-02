using System;
using System.Collections.Generic;
using Epros.Modules.Financeiro.Domain.Enums;

namespace Epros.Modules.Financeiro.Domain.Services.Cnab
{
    /// <summary>Dados do cedente/beneficiário para o arquivo bancário (EF FIN-SF §7.2).</summary>
    public sealed record DadosCedenteCnab(
        string CodigoBanco, string NomeBanco, string NomeCedente, string DocumentoCedente,
        string Agencia, string DigitoAgencia, string Conta, string DigitoConta,
        string Carteira, string Convenio);

    /// <summary>Um título a incluir na remessa (boleto/fatura elegível).</summary>
    public sealed record TituloRemessaCnab(
        long NossoNumero, string NumeroDocumento, decimal Valor, DateTime DataVencimento, DateTime DataEmissao,
        string SacadoNome, string SacadoDocumento, string SacadoEndereco, string SacadoBairro,
        string SacadoCidade, string SacadoUF, string SacadoCep,
        // Ocorrência/comando de remessa. 01 = entrada de título (registro). // valida-contador por banco.
        string CodigoOcorrencia = "01");

    /// <summary>Resultado de leitura de um título recuperado de um arquivo (remessa própria — auditoria/round-trip).</summary>
    public sealed record TituloLidoCnab(long NossoNumero, string NumeroDocumento, decimal Valor, DateTime DataVencimento);

    /// <summary>Uma ocorrência de retorno bancário (liquidação, baixa, tarifa…).</summary>
    public sealed record OcorrenciaRetornoCnab(
        long NossoNumero, string NumeroDocumento, decimal ValorTitulo, decimal ValorPago, decimal ValorTarifa,
        DateTime? DataOcorrencia, string CodigoOcorrencia, bool Liquidado);

    /// <summary>Retorno bancário processado.</summary>
    public sealed record RetornoCnab(ELayoutCnab Layout, IReadOnlyList<OcorrenciaRetornoCnab> Ocorrencias);
}
