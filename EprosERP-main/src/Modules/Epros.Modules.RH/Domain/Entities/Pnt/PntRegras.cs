using System;

namespace Epros.Modules.RH.Domain.Entities
{
    // Regras de negocio do submodulo Ponto e Jornada (RH-PNT).

    public partial class PntPeriodoApuracao
    {
        public const string StAberto = "Aberto";
        public const string StEmConferencia = "Em conferencia";
        public const string StFechado = "Fechado";
        public const string StExportado = "Exportado";
        public const string StCancelado = "Cancelado";

        public bool EstaFechado => Status == StFechado || Status == StExportado;

        public void EnviarParaConferencia(string usuario)
        {
            if (Status != StAberto)
            {
                AddNotification(nameof(Status), "So periodo Aberto vai para conferencia.");
                return;
            }
            Status = StEmConferencia;
            MarcarAlterado(usuario);
        }

        // PNT-009: periodo fechado bloqueia alteracoes ordinarias.
        public void Fechar(string usuario)
        {
            if (Status != StEmConferencia && Status != StAberto)
            {
                AddNotification(nameof(Status), "Periodo nao pode ser fechado neste estado.");
                return;
            }
            Status = StFechado;
            FechadoEm = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }

        // PNT-012: exportacao para folha depende de periodo fechado.
        public void Exportar(string usuario)
        {
            if (Status != StFechado)
            {
                AddNotification(nameof(Status), "Exportacao para folha exige periodo Fechado.");
                return;
            }
            Status = StExportado;
            ExportadoEm = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }
    }
}
