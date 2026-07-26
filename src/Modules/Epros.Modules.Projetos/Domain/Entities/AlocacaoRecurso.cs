using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities
{
    public class AlocacaoRecurso : EntidadeSaaSBase
    {
        public Guid ProjetoId { get; private set; }
        public Guid ColaboradorId { get; private set; }
        public string Funcao { get; private set; } = string.Empty;
        public decimal CustoHora { get; private set; }
        public decimal HorasPlanejadas { get; private set; }

        protected AlocacaoRecurso() { } // EF Core

        public AlocacaoRecurso(
            Guid projetoId,
            Guid colaboradorId,
            string funcao,
            decimal custoHora,
            decimal horasPlanejadas,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AlocacaoRecurso>()
                .Requires()
                .AreNotEquals(projetoId, Guid.Empty, nameof(ProjetoId), "O ID do projeto é obrigatório.")
                .AreNotEquals(colaboradorId, Guid.Empty, nameof(ColaboradorId), "O ID do colaborador é obrigatório.")
                .IsNotNullOrEmpty(funcao, nameof(Funcao), "A função de alocação é obrigatória.")
                .IsGreaterThan(custoHora, 0, nameof(CustoHora), "O custo por hora deve ser maior que zero.")
                .IsGreaterThan(horasPlanejadas, 0, nameof(HorasPlanejadas), "As horas planejadas devem ser maiores que zero.")
            );

            ProjetoId = projetoId;
            ColaboradorId = colaboradorId;
            Funcao = funcao;
            CustoHora = custoHora;
            HorasPlanejadas = horasPlanejadas;
        }
    }
}
