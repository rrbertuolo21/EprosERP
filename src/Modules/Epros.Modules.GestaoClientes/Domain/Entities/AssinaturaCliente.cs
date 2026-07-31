using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    // 1.01 — Status do ciclo da assinatura do cliente (AssinaturaCliente.Status).
    // Nomes alinhados ao vocabulário da fatia 1.01: Ativa/AguardandoAprovacao/Recusada/Futura/Expirada.
    // Persistido como string via HasConversion<string>; valores legados migrados na
    // migration Fecha_1_01_StatusSaaS_Enums (Aprovada->Ativa, Aguardando->AguardandoAprovacao).
    public enum AssinaturaStatus
    {
        Ativa = 1,
        AguardandoAprovacao = 2,
        Recusada = 3,
        Futura = 4,
        Expirada = 5
    }

    public class AssinaturaCliente : EntidadeSaaSBase
    {
        public Guid ClienteId { get; private set; }
        public Guid PlanoId { get; private set; }
        public AssinaturaStatus Status { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public DateTime? TrialAte { get; private set; }
        public string MetodoPagamento { get; private set; } = string.Empty;
        public string? TransacaoId { get; private set; }
        public string? DetalhesPacoteJson { get; private set; }
        public bool Arquivada { get; private set; }
        public string? OperadorAprovacao { get; private set; }
        public string? JustificativaAprovacao { get; private set; }

        // 1.08A — Instante em que o fim do trial foi processado (fatura da 1ª mensalidade gerada + cobrança
        // disparada). Fonte de idempotência do job de fim-de-trial: preenchido → o trial não é reconvertido.
        public DateTime? TrialConvertidoEm { get; private set; }

        // 1.08B — Vínculo da assinatura ao CICLO de cobrança (fecha o desacoplamento assinatura↔recorrência).
        // ProximaCobrancaEm = vencimento do ciclo em que a próxima fatura deve ser gerada, conforme
        // PlanoDuration (Mensal → +1 mês; Anual → +1 ano; Vitalicia → null, cobrança única, sem renovação).
        // O job de renovação usa esta data como âncora idempotente: ao gerar a fatura do ciclo, avança a data.
        public DateTime? ProximaCobrancaEm { get; private set; }
        public DateTime? UltimaRenovacaoEm { get; private set; }

        protected AssinaturaCliente() { } // EF Core

        public AssinaturaCliente(
            Guid clienteId,
            Guid planoId,
            AssinaturaStatus status,
            DateTime? dataInicio,
            DateTime? dataFim,
            DateTime? trialAte,
            string metodoPagamento,
            string? transacaoId,
            string? detalhesPacoteJson,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AssinaturaCliente>()
                .Requires()
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "Cliente é obrigatório")
                .AreNotEquals(planoId, Guid.Empty, nameof(PlanoId), "Plano é obrigatório")
                .IsNotNullOrEmpty(metodoPagamento, nameof(MetodoPagamento), "Método de pagamento é obrigatório")
            );

            ClienteId = clienteId;
            PlanoId = planoId;
            Status = status;
            DataInicio = dataInicio;
            DataFim = dataFim;
            TrialAte = trialAte;
            MetodoPagamento = metodoPagamento;
            TransacaoId = transacaoId;
            DetalhesPacoteJson = detalhesPacoteJson;
            Arquivada = false;
        }

        public void Ativar(string alteradoPor)
        {
            Status = AssinaturaStatus.Ativa;
            MarcarAlterado(alteradoPor);
        }

        public void AprovarManualmente(string operador, string justificativa, string alteradoPor)
        {
            Status = AssinaturaStatus.Ativa;
            OperadorAprovacao = operador;
            JustificativaAprovacao = justificativa;
            
            // REG-008: Alinha as datas de início e fim da vigência com base na data de aprovação
            DataInicio = DateTime.UtcNow;
            if (DataFim.HasValue)
            {
                DataFim = DateTime.UtcNow.AddDays(30);
            }
            
            MarcarAlterado(alteradoPor);
        }

        /// <summary>
        /// 1.08A — Marca que o fim do trial já foi processado (fatura gerada + cobrança disparada),
        /// tornando o job de fim-de-trial idempotente. Não altera o Status da assinatura.
        /// </summary>
        public void MarcarTrialConvertido(string alteradoPor)
        {
            TrialConvertidoEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>
        /// 1.08B — Define o próximo vencimento do ciclo de cobrança conforme a duração do plano.
        /// <paramref name="proximaCobranca"/> null (plano Vitalícia) desliga a renovação (cobrança única).
        /// </summary>
        public void DefinirProximaCobranca(DateTime? proximaCobranca, string alteradoPor)
        {
            ProximaCobrancaEm = proximaCobranca;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>
        /// 1.08B — Registra que a fatura do ciclo foi gerada e AVANÇA o vínculo para o próximo vencimento
        /// (idempotência da renovação: o mesmo ciclo não gera fatura duas vezes).
        /// </summary>
        public void RegistrarRenovacao(DateTime? proximaCobranca, string alteradoPor)
        {
            UltimaRenovacaoEm = DateTime.UtcNow;
            ProximaCobrancaEm = proximaCobranca;
            MarcarAlterado(alteradoPor);
        }

        public void Expirar(string alteradoPor)
        {
            Status = AssinaturaStatus.Expirada;
            MarcarAlterado(alteradoPor);
        }

        public void Rejeitar(string alteradoPor)
        {
            Status = AssinaturaStatus.Recusada;
            MarcarAlterado(alteradoPor);
        }

        public void Arquivar(string alteradoPor)
        {
            Arquivada = true;
            MarcarAlterado(alteradoPor);
        }

        public void AtualizarDatas(DateTime? inicio, DateTime? fim, string alteradoPor)
        {
            DataInicio = inicio;
            DataFim = fim;
            MarcarAlterado(alteradoPor);
        }
    }
}
