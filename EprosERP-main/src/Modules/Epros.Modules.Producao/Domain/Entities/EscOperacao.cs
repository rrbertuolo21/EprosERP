using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-ESC — Operação sequenciada na programação (prd_esc_operacao). Fiel ao EF §17.
    /// ESC-REG-016/017/018: janelas prevista/realizada e durações; equipamento/colaborador como referência externa.
    /// </summary>
    public class EscOperacao : EntidadeSaaSBase
    {
        public Guid ProgramacaoId { get; private set; }
        public Guid? ServicoId { get; private set; }
        public Guid? EquipamentoId { get; private set; }
        public Guid? ColaboradorId { get; private set; }
        public int Sequencia { get; private set; }
        public DateTime? InicioPrevisto { get; private set; }
        public DateTime? TerminoPrevisto { get; private set; }
        public DateTime? InicioRealizado { get; private set; }
        public DateTime? TerminoRealizado { get; private set; }
        public int HorasPrevistas { get; private set; }
        public int MinutosPrevistos { get; private set; }
        public int SegundosPrevistos { get; private set; }
        public int HorasRealizadas { get; private set; }
        public int MinutosRealizados { get; private set; }
        public int SegundosRealizados { get; private set; }
        public decimal CustoPrevisto { get; private set; }
        public decimal CustoRealizado { get; private set; }

        protected EscOperacao() { } // EF Core

        public EscOperacao(
            Guid programacaoId,
            string tenantId,
            string criadoPor,
            int sequencia = 0,
            Guid? servicoId = null,
            Guid? equipamentoId = null,
            Guid? colaboradorId = null,
            DateTime? inicioPrevisto = null,
            DateTime? terminoPrevisto = null,
            int horasPrevistas = 0,
            int minutosPrevistos = 0,
            int segundosPrevistos = 0,
            decimal custoPrevisto = 0m)
            : base(tenantId, criadoPor)
        {
            ProgramacaoId = programacaoId;
            Sequencia = sequencia;
            ServicoId = servicoId;
            EquipamentoId = equipamentoId;
            ColaboradorId = colaboradorId;
            InicioPrevisto = inicioPrevisto;
            TerminoPrevisto = terminoPrevisto;
            HorasPrevistas = horasPrevistas;
            MinutosPrevistos = minutosPrevistos;
            SegundosPrevistos = segundosPrevistos;
            CustoPrevisto = custoPrevisto;

            AddNotifications(new Contract<EscOperacao>()
                .Requires()
                .AreNotEquals(programacaoId, Guid.Empty, nameof(ProgramacaoId), "A operação deve referenciar a programação [Origem: EscOperacao].")
                .IsGreaterOrEqualsThan(sequencia, 0, nameof(Sequencia), "A sequência deve ser maior ou igual a zero [Origem: EscOperacao].")
                .IsGreaterOrEqualsThan(minutosPrevistos, 0, nameof(MinutosPrevistos), "Minutos previstos inválidos [Origem: EscOperacao].")
                .IsLowerOrEqualsThan(minutosPrevistos, 59, nameof(MinutosPrevistos), "Minutos previstos devem estar entre 0 e 59 [Origem: EscOperacao].")
                .IsGreaterOrEqualsThan(segundosPrevistos, 0, nameof(SegundosPrevistos), "Segundos previstos inválidos [Origem: EscOperacao].")
                .IsLowerOrEqualsThan(segundosPrevistos, 59, nameof(SegundosPrevistos), "Segundos previstos devem estar entre 0 e 59 [Origem: EscOperacao].")
            );
        }

        /// <summary>ESC-REG-017/018: registra janela e duração realizadas da operação.</summary>
        public void RegistrarRealizado(
            DateTime? inicioRealizado,
            DateTime? terminoRealizado,
            int horas,
            int minutos,
            int segundos,
            string alteradoPor,
            decimal custoRealizado = 0m)
        {
            InicioRealizado = inicioRealizado;
            TerminoRealizado = terminoRealizado;
            HorasRealizadas = horas;
            MinutosRealizados = minutos;
            SegundosRealizados = segundos;
            CustoRealizado = custoRealizado;
            MarcarAlterado(alteradoPor);
        }
    }
}
