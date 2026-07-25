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
}
