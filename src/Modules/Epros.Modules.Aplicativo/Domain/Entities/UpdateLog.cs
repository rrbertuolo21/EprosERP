using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class UpdateLog : EntidadeSaaSBase
    {
        public string VersaoAlvo { get; private set; } = string.Empty;
        public DateTime ExecutadoEm { get; private set; }
        public string ExecutadoPor { get; private set; } = string.Empty;
        public bool Sucesso { get; private set; }
        public string? Log { get; private set; }

        protected UpdateLog() { } // EF Core

        public UpdateLog(string versaoAlvo, string executadoPor, bool sucesso, string? log, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            VersaoAlvo = versaoAlvo;
            ExecutadoEm = DateTime.UtcNow;
            ExecutadoPor = executadoPor;
            Sucesso = sucesso;
            Log = log;
        }
    }
}
