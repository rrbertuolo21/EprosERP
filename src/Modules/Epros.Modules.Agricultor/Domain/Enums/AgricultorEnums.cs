namespace Epros.Modules.Agricultor.Domain.Enums
{
    // ===================== PAINEL DO PRODUTOR =====================

    /// <summary>
    /// Tipo de exploração do imóvel rural — alimenta o campo TIPO_EXPLORACAO do registro 0040 do LCDPR.
    /// Domínio 1..6 congelado pelo Manual LCDPR 1.3 (AGR-D01/AGR-D17). // valida-contador (rótulos exatos).
    /// </summary>
    public enum ETipoExploracao
    {
        Individual = 1,
        CondominioAdministracaoConjunta = 2,
        Imovel_Arrendado = 3,
        Parceria = 4,
        Comodato = 5,
        Outros = 6
    }

    /// <summary>Ciclo de vida da safra (talhão × cultura × período).</summary>
    public enum EStatusSafra
    {
        Planejada = 0,
        EmAndamento = 1,
        Colhida = 2,
        Encerrada = 3
    }

    /// <summary>Papel do colaborador na propriedade (Agris: User + role staff).</summary>
    public enum EPapelColaborador
    {
        Owner = 0,
        Staff = 1
    }

    /// <summary>Status de uma anotação de campo.</summary>
    public enum EStatusAnotacao
    {
        Aberta = 0,
        EmAndamento = 1,
        Concluida = 2,
        Cancelada = 3
    }

    // ===================== LIVRO CAIXA DIGITAL (LCDPR) =====================

    /// <summary>
    /// Forma de apuração do resultado da atividade rural — registro 0010 campo FORMA_APUR.
    /// 1 = Livro Caixa (resultado real); 2 = presunção 20% da receita bruta. // valida-contador.
    /// </summary>
    public enum EFormaApuracao
    {
        LivroCaixa = 1,
        Presumido20 = 2
    }

    /// <summary>
    /// Situação da escrituração LCDPR ao longo do seu ciclo (produto).
    /// Exportada = arquivo .txt gerado e retido (hash + versão).
    /// </summary>
    public enum EStatusEscrituracaoLcdpr
    {
        Aberta = 0,
        Fechada = 1,
        Exportada = 2,
        Retificadora = 3
    }

    /// <summary>
    /// TIPO_LANC do registro Q100 (campo 9). Domínio {1,2,3} pela norma vigente (Manual 1.3).
    /// AGR-D08: o "código 6" da FAQ Q12 fica BLOQUEADO até validação da RFB — não modelado. // valida-contador.
    /// </summary>
    public enum ETipoLancamentoLcdpr
    {
        Receita = 1,        // 1 = Receita da atividade rural
        Despesa_Custeio = 2, // 2 = Despesa de custeio / investimento
        ReceitaDespesaNaoDedutivel = 3 // 3 = demais valores (uso conforme norma) // valida-contador
    }

    /// <summary>
    /// TIPO_DOC do registro Q100 (campo 6). Domínio 1..6 do Manual 1.3. // valida-contador (rótulos exatos).
    /// </summary>
    public enum ETipoDocumentoLcdpr
    {
        NotaFiscal = 1,
        Fatura = 2,
        Recibo = 3,
        Contrato = 4,
        Folha = 5,     // Folha de pagamento / mão de obra (AGR-D02)
        Outros = 6
    }

    /// <summary>Natureza do saldo financeiro (NAT_SLD_FIN) — Positivo / Negativo.</summary>
    public enum ENaturezaSaldo
    {
        Positivo = 0, // 'P'
        Negativo = 1  // 'N'
    }
}
