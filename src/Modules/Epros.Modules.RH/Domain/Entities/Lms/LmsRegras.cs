using System;

namespace Epros.Modules.RH.Domain.Entities
{
    // Regras de negocio do submodulo Treinamento e Certificacoes (RH-LMS).

    public partial class LmsTreinamento
    {
        // Secao 19: dominio de status.
        public const string StScheduled = "scheduled";
        public const string StOngoing = "ongoing";
        public const string StCompleted = "completed";
        public const string StCancelled = "cancelled";

        public static bool StatusValido(string status)
            => status == StScheduled || status == StOngoing || status == StCompleted || status == StCancelled;

        // Secao 16: data_fim >= data_inicio e hora_fim > hora_inicio.
        public bool PeriodoValido() => DataFim >= DataInicio;
        public bool HorarioValido() => HoraFim > HoraInicio;

        public void ValidarRegras()
        {
            if (!StatusValido(Status))
                AddNotification(nameof(Status), "Status de treinamento invalido (scheduled, ongoing, completed, cancelled).");
            if (!PeriodoValido())
                AddNotification(nameof(DataFim), "A data final deve ser maior ou igual a data inicial.");
            if (!HorarioValido())
                AddNotification(nameof(HoraFim), "A hora final deve ser maior que a hora inicial.");
        }

        public void AlterarStatus(string status, string usuario)
        {
            if (!StatusValido(status))
            {
                AddNotification(nameof(Status), "Status de treinamento invalido.");
                return;
            }
            Status = status;
            MarcarAlterado(usuario);
        }
    }

    public partial class LmsTarefa
    {
        public const string StPending = "pending";
        public const string StCompleted = "completed";

        public static bool StatusValido(string status)
            => status == StPending || status == StCompleted;

        public void Concluir(string usuario)
        {
            Status = StCompleted;
            MarcarAlterado(usuario);
        }
    }

    public partial class LmsFeedback
    {
        // Secao 19: nota de feedback entre 1 e 5.
        public static bool NotaValida(int nota) => nota >= 1 && nota <= 5;

        public void ValidarNota()
        {
            if (!NotaValida(Nota))
                AddNotification(nameof(Nota), "A nota do feedback deve estar entre 1 e 5.");
        }
    }

    public partial class LmsAlertaCertificacao
    {
        // Secao 19: alertas de certificacao 30/60/90 dias.
        public static bool DiasAntecedenciaValido(int dias) => dias == 30 || dias == 60 || dias == 90;

        public const string StPendente = "Pendente";
        public const string StEnviado = "Enviado";
        public const string StCancelado = "Cancelado";
    }
}
