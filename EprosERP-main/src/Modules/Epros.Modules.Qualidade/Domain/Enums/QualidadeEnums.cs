namespace Epros.Modules.Qualidade.Domain.Enums
{
    // ============================================================
    // Ciclo de vida generico do registro (comum a NCR, INS, ACR, ADM, ATR).
    // Dominio informado nas EFs: Rascunho, EmAnalise, Ativo, Suspenso, Encerrado, Inativo.
    // ============================================================
    public enum EStatusRegistroQualidade
    {
        Rascunho = 0,
        EmAnalise = 1,
        Ativo = 2,
        Suspenso = 3,
        Encerrado = 4,
        Inativo = 5
    }

    public enum EAcaoHistoricoQualidade
    {
        Criado = 0,
        Alterado = 1,
        Submetido = 2,
        Aprovado = 3,
        Rejeitado = 4,
        Suspenso = 5,
        Encerrado = 6,
        Inativado = 7,
        Reativado = 8,
        Cancelado = 9,
        Reaberto = 10,
        EventoEnviado = 11
    }

    // ============================================================
    // QLD-NCR — Nao Conformidades
    // ============================================================
    public enum ENcrOrigem
    {
        RejeicaoLote = 0,
        ReclamacaoCliente = 1,
        Auditoria = 2,
        Garantia = 3,
        Inspecao = 4,
        Producao = 5,
        Estoque = 6,
        Manual = 7,
        Outro = 8
    }

    public enum ENcrPrioridade
    {
        Baixa = 0,
        Media = 1,
        Alta = 2,
        Urgente = 3
    }

    public enum ENcrEtapa
    {
        Rascunho = 0,
        Triagem = 1,
        Investigacao = 2,
        CAPA = 3,
        Verificacao = 4,
        Encerrada = 5,
        Cancelada = 6
    }

    public enum ENcrMetodoCausa
    {
        CincoPorques = 0,
        Ishikawa = 1,
        Outro = 2
    }

    public enum ENcrTipoAcao
    {
        Corretiva = 0,
        Preventiva = 1,
        Contencao = 2,
        Outro = 3
    }

    public enum ENcrStatusAcao
    {
        Pendente = 0,
        EmExecucao = 1,
        Concluida = 2,
        Cancelada = 3,
        Vencida = 4
    }

    public enum ENcrResultadoVerificacao
    {
        Aprovada = 0,
        Reprovada = 1,
        Inconclusiva = 2
    }

    public enum ENcrEntidadeAlvo
    {
        Ncr = 0,
        CausaRaiz = 1,
        AcaoCapa = 2,
        Verificacao = 3
    }

    public enum EDirecaoEvento
    {
        Entrada = 0,
        Saida = 1
    }

    // ============================================================
    // QLD-INS — Planos de Inspecao e Amostragem
    // ============================================================
    public enum EContextoPlano
    {
        Produto = 0,
        Processo = 1,
        Recebimento = 2,
        Lote = 3,
        Ordem = 4,
        Etapa = 5,
        Manual = 6,
        Outro = 7
    }

    public enum ETipoCaracteristica
    {
        Dimensional = 0,
        Visual = 1,
        Funcional = 2,
        Documental = 3,
        Regulatoria = 4,
        Outro = 5
    }

    public enum ETipoDadoCaracteristica
    {
        Decimal = 0,
        Inteiro = 1,
        Texto = 2,
        Booleano = 3,
        Enum = 4,
        Imagem = 5,
        Arquivo = 6
    }

    public enum ETipoAmostragem
    {
        AQL = 0,
        Percentual = 1,
        QuantidadeFixa = 2,
        CemPorCento = 3,
        Outro = 4
    }

    public enum EReferenciaExecucao
    {
        Recebimento = 0,
        Lote = 1,
        OrdemProducao = 2,
        EtapaProcesso = 3,
        Manual = 4,
        Outro = 5
    }

    public enum EStatusExecucaoInspecao
    {
        Aberta = 0,
        EmColeta = 1,
        Concluida = 2,
        Cancelada = 3,
        Inconclusiva = 4
    }

    public enum EResultadoPreliminar
    {
        Conforme = 0,
        NaoConforme = 1,
        Alerta = 2,
        Inconclusivo = 3
    }

    public enum EStatusAmostra
    {
        Pendente = 0,
        Medida = 1,
        Reprovada = 2,
        Aprovada = 3,
        NaoAplicavel = 4
    }

    public enum EResultadoMedicao
    {
        Conforme = 0,
        NaoConforme = 1,
        Alerta = 2,
        NaoAplicavel = 3
    }

    public enum EResultadoInspecaoConsolidado
    {
        Aprovado = 0,
        Reprovado = 1,
        AprovadoComRestricao = 2,
        Inconclusivo = 3
    }

    // ============================================================
    // QLD-ACR — Analise de Aceitacao e Rejeicao
    // ============================================================
    public enum ETipoAnaliseAcr
    {
        Recebimento = 0,
        Processo = 1,
        Devolucao = 2,
        Manual = 3
    }

    public enum EResultadoAcr
    {
        Aceito = 0,
        Rejeitado = 1,
        Quarentena = 2,
        AceitoComDesvio = 3,
        Reinspecao = 4
    }

    public enum ETipoEventoEstoqueAcr
    {
        Bloquear = 0,
        Liberar = 1,
        Quarentena = 2,
        Reverter = 3
    }

    public enum EStatusEventoAcr
    {
        Pendente = 0,
        Enviado = 1,
        Confirmado = 2,
        Erro = 3
    }

    public enum EStatusEventoNcrAcr
    {
        Pendente = 0,
        Criado = 1,
        Erro = 2,
        Ignorado = 3
    }

    public enum EGatilhoNcrAcr
    {
        Severidade = 0,
        Recorrencia = 1,
        Persistencia = 2,
        Manual = 3
    }

    // ============================================================
    // QLD-ADM — Administracao da Qualidade
    // ============================================================
    public enum ETipoAuditoria
    {
        Interna = 0,
        Externa = 1
    }

    // ============================================================
    // QLD-ATR — Gestao de Atributos
    // ============================================================
    public enum ETipoAtributo
    {
        Comercial = 0,
        Qualidade = 1,
        Regulatorio = 2
    }

    public enum ETipoDadoAtributo
    {
        Texto = 0,
        Numero = 1,
        Decimal = 2,
        Data = 3,
        Lista = 4,
        Booleano = 5
    }

    public enum EEscopoAtributo
    {
        Produto = 0,
        Item = 1,
        Familia = 2,
        Processo = 3,
        Plano = 4
    }

    public enum EContextoVinculoAtributo
    {
        Produto = 0,
        Item = 1,
        Familia = 2,
        Processo = 3,
        Plano = 4,
        BOM = 5
    }
}
