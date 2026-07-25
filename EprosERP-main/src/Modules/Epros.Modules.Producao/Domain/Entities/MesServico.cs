using System;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-MES — Serviço/operação/apontamento de tempo da ordem (prd_mes_servico). Fiel ao EF §17.
    /// MES-REG-006: deve referenciar o item da ordem. Diferencia tempo previsto e realizado (MES-REG-013).
    /// </summary>
    public class MesServico : EntidadeSaaSBase
    {
        public Guid ItemOrdemId { get; private set; }
        public DateTime? InicioPrevisto { get; private set; }
        public DateTime? TerminoPrevisto { get; private set; }
        public int HorasPrevisto { get; private set; }
        public int MinutosPrevisto { get; private set; }
        public int SegundosPrevisto { get; private set; }
        public decimal CustoPrevisto { get; private set; }
        public DateTime? InicioRealizado { get; private set; }
        public DateTime? TerminoRealizado { get; private set; }
        public int HorasRealizado { get; private set; }
        public int MinutosRealizado { get; private set; }
        public int SegundosRealizado { get; private set; }
        public decimal CustoRealizado { get; private set; }
        public EStatusServicoMes Status { get; private set; } = EStatusServicoMes.Previsto;

        protected MesServico() { } // EF Core

        public MesServico(
            Guid itemOrdemId,
            string tenantId,
            string criadoPor,
            DateTime? inicioPrevisto = null,
            DateTime? terminoPrevisto = null,
            int horasPrevisto = 0,
            int minutosPrevisto = 0,
            int segundosPrevisto = 0,
            decimal custoPrevisto = 0m)
            : base(tenantId, criadoPor)
        {
            ItemOrdemId = itemOrdemId;
            InicioPrevisto = inicioPrevisto;
            TerminoPrevisto = terminoPrevisto;
            HorasPrevisto = horasPrevisto;
            MinutosPrevisto = minutosPrevisto;
            SegundosPrevisto = segundosPrevisto;
            CustoPrevisto = custoPrevisto;
            Status = EStatusServicoMes.Previsto;

            AddNotifications(new Contract<MesServico>()
                .Requires()
                .AreNotEquals(itemOrdemId, Guid.Empty, nameof(ItemOrdemId), "O serviço deve referenciar o item da ordem [Origem: MesServico]. (MES-REG-006)")
                .IsGreaterOrEqualsThan(minutosPrevisto, 0, nameof(MinutosPrevisto), "Minutos previstos inválidos [Origem: MesServico].")
                .IsLowerOrEqualsThan(minutosPrevisto, 59, nameof(MinutosPrevisto), "Minutos previstos devem estar entre 0 e 59 [Origem: MesServico].")
                .IsGreaterOrEqualsThan(segundosPrevisto, 0, nameof(SegundosPrevisto), "Segundos previstos inválidos [Origem: MesServico].")
                .IsLowerOrEqualsThan(segundosPrevisto, 59, nameof(SegundosPrevisto), "Segundos previstos devem estar entre 0 e 59 [Origem: MesServico].")
            );
        }

        /// <summary>MES-REG-013: registra tempo e custo realizados do apontamento.</summary>
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
            HorasRealizado = horas;
            MinutosRealizado = minutos;
            SegundosRealizado = segundos;
            CustoRealizado = custoRealizado;
            Status = terminoRealizado.HasValue ? EStatusServicoMes.Realizado : EStatusServicoMes.EmExecucao;
            MarcarAlterado(alteradoPor);
        }
    }
}
