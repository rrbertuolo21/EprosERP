namespace Epros.Modules.Manutencao.Domain.Enums
{
    // Ciclo de vida padrao dos agregados de manutencao (PRV, PEC, PAR, IND).
    public enum EStatusRegistroManutencao
    {
        Rascunho = 0,
        EmAnalise = 1,
        Ativo = 2,
        Suspenso = 3,
        Encerrado = 4,
        Inativo = 5
    }

    // ===== MAN-PRV (Manutencao Preventiva) =====
    public enum ETipoPeriodicidade
    {
        Calendario = 0,
        Contador = 1,
        Combinado = 2
    }

    public enum ESituacaoPeriodicidade
    {
        Ativo = 0,
        Suspenso = 1,
        Inativo = 2
    }

    public enum EStatusExecucaoPreventiva
    {
        Prevista = 0,
        Elegivel = 1,
        OrdemGerada = 2,
        EmExecucao = 3,
        Concluida = 4,
        Cancelada = 5,
        Atrasada = 6
    }

    // ===== MAN-TRB (Gestao de Trabalho / Ordem de Servico) =====
    public enum EPerfilOrdem
    {
        Oficina = 0,
        Campo = 1
    }

    // T5 — Origem da OS canonica. Manual = ordem externa (cliente); as demais sao
    // ordens INTERNAS geradas por outro submodulo (sem pessoa/cliente obrigatorio).
    public enum EOrigemOrdemServico
    {
        Manual = 0,        // OS externa aberta manualmente (cliente obrigatorio)
        Preventiva = 1,    // gerada por vencimento de plano preventivo (MAN-PRV)
        Preditiva = 2,     // gerada por alarme preditivo (MAN-PDT)
        Corretiva = 3,     // gerada por parada nao planejada (MAN-PAR)
        Confiabilidade = 4 // gerada por recomendacao de revisao (MAN-CRV)
    }

    public enum EStatusOrdemServico
    {
        Aberta = 1,
        EmOrcamento = 2,
        Aprovada = 3,
        Montagem = 4,
        Pronta = 5,
        Entregue = 6,
        Cancelada = 7
    }

    public enum ETipoItemOrdemServico
    {
        Produto = 0,
        Servico = 1
    }

    public enum ETipoSaidaItem
    {
        Venda = 0,
        Troca = 1,
        Bonificacao = 2,
        Comodato = 3
    }

    public enum ETipoVinculoFinanceiroOs
    {
        ContasReceber = 0,
        Recebimento = 1,
        NFe = 2,
        CFe = 3,
        SAT = 4,
        Pagamento = 5
    }

    public enum EStatusVinculoFinanceiroOs
    {
        Pendente = 0,
        Confirmado = 1,
        Falhou = 2,
        Cancelado = 3
    }

    // ===== MAN-PEC (Pecas de Reposicao) =====
    public enum EStatusItemPeca
    {
        Rascunho = 0,
        Reservado = 1,
        EntregueParcial = 2,
        EntregueTotal = 3,
        Devolvido = 4,
        Cancelado = 5,
        Encerrado = 6
    }

    public enum EStatusReservaPeca
    {
        Pendente = 0,
        Reservada = 1,
        Parcial = 2,
        Liberada = 3,
        Baixada = 4,
        Cancelada = 5
    }

    public enum ETipoMovimentoPeca
    {
        Baixa = 0,
        Devolucao = 1,
        Reserva = 2,
        LiberacaoReserva = 3,
        Ajuste = 4
    }

    public enum EStatusMovimentoPeca
    {
        Pendente = 0,
        Confirmado = 1,
        Falhou = 2,
        Cancelado = 3,
        Reprocessado = 4
    }

    // ===== MAN-PAR (Gestao de Paradas) =====
    public enum ETipoParada
    {
        Planejada = 0,
        NaoPlanejada = 1,
        Setup = 2
    }

    public enum ETipoVinculoOsParada
    {
        GeradaAutomaticamente = 0,
        VinculadaManual = 1,
        SolicitacaoPendente = 2
    }

    public enum EStatusVinculoOsParada
    {
        Pendente = 0,
        Gerada = 1,
        Vinculada = 2,
        Falhou = 3,
        Cancelada = 4
    }

    public enum ETipoIndicadorParada
    {
        Duracao = 0,
        Disponibilidade = 1,
        MTTR = 2,
        MTBF = 3,
        OEE = 4,
        Outro = 5
    }

    // ===== MAN-IND (Inducao / Config Equipamentos) =====
    public enum EStatusInducao
    {
        Rascunho = 0,
        EmAnalise = 1,
        Aprovada = 2,
        Rejeitada = 3,
        Ativa = 4,
        Cancelada = 5
    }

    // ===== Compartilhado: envio de evento de integracao =====
    public enum EStatusEnvioEvento
    {
        Pendente = 0,
        Enviado = 1,
        Falhou = 2,
        Reprocessado = 3,
        Cancelado = 4
    }

    // ===== MAN-CRV (Confiabilidade e Revisao) =====
    // Ciclo de vida da revisao reutiliza EStatusRegistroManutencao (Rascunho..Inativo).
    public enum ETipoIndicadorConfiabilidade
    {
        Mttr = 0,
        Mtbf = 1,
        Disponibilidade = 2,
        Rpn = 3,
        Outro = 4
    }

    public enum ECalculadoPorConfiabilidade
    {
        Sistema = 0,
        Usuario = 1
    }

    public enum EEstrategiaManutencao
    {
        Preventiva = 0,
        Preditiva = 1,
        CorretivaControlada = 2,
        OperacaoAteFalha = 3,
        RevisarPlano = 4,
        ManterPlano = 5
    }

    public enum EStatusRecomendacaoEstrategia
    {
        Proposta = 0,
        Aprovada = 1,
        Rejeitada = 2,
        Substituida = 3
    }

    public enum EAcaoHistoricoConfiabilidade
    {
        Criado = 0,
        Alterado = 1,
        Aprovado = 2,
        Rejeitado = 3,
        Suspenso = 4,
        Retomado = 5,
        Encerrado = 6,
        Inativado = 7,
        Parametrizado = 8
    }

    // ===== MAN-PDT (Manutencao Preditiva) =====
    // Ciclo de vida do monitoramento reutiliza EStatusRegistroManutencao (Rascunho..Inativo).
    public enum ESituacaoPontoMedicao
    {
        Ativo = 0,
        Suspenso = 1,
        Inativo = 2
    }

    public enum ETipoRegraMonitoramento
    {
        Limite = 0,
        Tendencia = 1,
        Desvio = 2,
        Sla = 3,
        Outro = 4
    }

    public enum ESituacaoRegraMonitoramento
    {
        Rascunho = 0,
        EmAnalise = 1,
        Ativo = 2,
        Suspenso = 3,
        Inativo = 4
    }

    public enum EStatusAlarmePreditivo
    {
        Aberto = 0,
        EmAnalise = 1,
        ConvertidoEmOrdem = 2,
        Descartado = 3,
        Encerrado = 4
    }

    public enum EAcaoHistoricoPreditivo
    {
        Criado = 0,
        Alterado = 1,
        Aprovado = 2,
        Rejeitado = 3,
        Disparado = 4,
        Encerrado = 5
    }

    public enum EDirecaoEventoPreditivo
    {
        Recebido = 0,
        Publicado = 1
    }

    public enum EStatusEventoPreditivo
    {
        Pendente = 0,
        Processado = 1,
        Erro = 2,
        Reprocessado = 3
    }

    public enum ESituacaoParametroManutencao
    {
        Ativo = 0,
        Inativo = 1
    }
}
