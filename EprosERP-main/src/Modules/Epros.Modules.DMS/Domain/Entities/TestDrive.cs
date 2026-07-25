using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class TestDrive : EntidadeSaaSBase
    {
        public Guid OportunidadeId { get; private set; }
        public Guid VeiculoDemonstracaoId { get; private set; }
        public DateTime Inicio { get; private set; }
        public DateTime Fim { get; private set; }
        public string Status { get; private set; } = "Agendado";
        public Guid? TermoDocumentoId { get; private set; }
        public string? Resultado { get; private set; }

        protected TestDrive() { } // EF Core

        public TestDrive(
            Guid oportunidadeId,
            Guid veiculoDemonstracaoId,
            DateTime inicio,
            DateTime fim,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<TestDrive>()
                .Requires()
                .AreNotEquals(oportunidadeId, Guid.Empty, nameof(OportunidadeId), "A oportunidade é obrigatória.")
                .AreNotEquals(veiculoDemonstracaoId, Guid.Empty, nameof(VeiculoDemonstracaoId), "O veículo de demonstração é obrigatório.")
            );

            if (fim <= inicio)
            {
                AddNotification(nameof(Fim), "A data/hora de término deve ser posterior ao início.");
            }

            OportunidadeId = oportunidadeId;
            VeiculoDemonstracaoId = veiculoDemonstracaoId;
            Inicio = inicio;
            Fim = fim;
            Status = "Agendado";
        }

        public void Realizar(string resultado, string usuario)
        {
            Status = "Realizado";
            Resultado = resultado;
            MarcarAlterado(usuario);
        }

        public void Cancelar(string usuario)
        {
            Status = "Cancelado";
            MarcarAlterado(usuario);
        }
    }
}
