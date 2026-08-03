using System.ComponentModel;

namespace Epros.Modules.Financeiro.Domain.Enums
{
    /// <summary>Situação da fatura de cobrança (EF FIN-SF RSF-004/005/006). Códigos funcionais P/V/B.</summary>
    public enum ESituacaoFaturaCobranca
    {
        [Description("Pendente")] Pendente = 0,   // P
        [Description("Vencida")] Vencida = 1,     // V
        [Description("Baixada")] Baixada = 2      // B
    }

    /// <summary>Tipo de fatura de cobrança (EF FIN-SF §7.3 / RSF-020/021/022).</summary>
    public enum ETipoFaturaCobranca
    {
        [Description("Avulsa")] Avulsa = 0,
        [Description("Periódica")] Periodica = 1,
        [Description("Carnê")] Carne = 2
    }

    /// <summary>Status da cobrança por e-mail (EF FIN-SF RSF-030 a RSF-035). Códigos funcionais 1..6.</summary>
    public enum EStatusCobrancaEmail
    {
        [Description("Encubada")] Encubada = 5,
        [Description("Em andamento")] EmAndamento = 1,
        [Description("Inadimplente")] Inadimplente = 2,
        [Description("Recobrado")] Recobrado = 3,
        [Description("Aguardando validação")] AguardandoValidacao = 4,
        [Description("Finalizada")] Finalizada = 6
    }

    /// <summary>Área/fila da cobrança por e-mail (EF FIN-SF RSF-037).</summary>
    public enum EAreaCobrancaEmail
    {
        [Description("auto")] Auto = 0,
        [Description("noauto")] NoAuto = 1
    }

    /// <summary>Layout CNAB de remessa/retorno (EF FIN-SF RSF-009/010).</summary>
    public enum ELayoutCnab
    {
        [Description("CNAB 240")] Cnab240 = 240,
        [Description("CNAB 400")] Cnab400 = 400
    }

    /// <summary>Tipo do documento da categoria financeira (EF FIN-SF RSF-040). Domínio CP/CR.</summary>
    public enum ETipoDocumentoCategoria
    {
        [Description("Contas a Pagar")] CP = 0,
        [Description("Contas a Receber")] CR = 1
    }

    /// <summary>Provedor de pagamento/cobrança registrado como gateway (EF FIN-SF — webhook de baixa).</summary>
    public enum EProvedorPagamento
    {
        [Description("Mercado Pago")] MercadoPago = 0,
        [Description("Genérico")] Generico = 1,
        [Description("Outro")] Outro = 2
    }

    /// <summary>
    /// Situação do processamento de um webhook de pagamento recebido (idempotência/dedup + auditoria).
    /// </summary>
    public enum EStatusWebhookPagamento
    {
        [Description("Recebido")] Recebido = 0,
        [Description("Processado")] Processado = 1,
        [Description("Duplicado")] Duplicado = 2,
        [Description("Assinatura inválida")] AssinaturaInvalida = 3,
        [Description("Fatura não localizada")] FaturaNaoLocalizada = 4,
        [Description("Ignorado")] Ignorado = 5
    }
}
