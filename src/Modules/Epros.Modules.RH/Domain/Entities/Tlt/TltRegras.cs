using System;

namespace Epros.Modules.RH.Domain.Entities
{
    // Regras de negocio do submodulo Gestao de Talentos (RH-TLT).

    public partial class TltMetaColaborador
    {
        // Secao 18/20: status da meta.
        public const string StNaoIniciada = "Nao iniciada";
        public const string StEmAndamento = "Em andamento";
        public const string StConcluida = "Concluida";
        public const string StAtrasada = "Atrasada";

        // Data final posterior a data inicial; progresso entre 0 e 100.
        public bool PeriodoValido() => DataFim > DataInicio;
        public bool ProgressoValido() => Progresso >= 0m && Progresso <= 100m;

        public void ValidarRegras()
        {
            if (!PeriodoValido())
                AddNotification(nameof(DataFim), "A data final da meta deve ser posterior a data inicial.");
            if (!ProgressoValido())
                AddNotification(nameof(Progresso), "O progresso deve estar entre 0 e 100.");
        }

        public void AtualizarProgresso(decimal progresso, string usuario)
        {
            Progresso = progresso;
            if (!ProgressoValido())
            {
                AddNotification(nameof(Progresso), "O progresso deve estar entre 0 e 100.");
                return;
            }
            MarcarAlterado(usuario);
        }
    }

    public partial class TltAvaliacaoColaborador
    {
        public const string StPendente = "Pendente";
        public const string StEmAndamento = "Em andamento";
        public const string StConcluida = "Concluida";
        public const string StCancelada = "Cancelada";

        // Secao 18: media = media das notas maiores que zero, arredondada (2 casas).
        public static decimal CalcularMedia(params decimal[] notas)
        {
            var validas = new System.Collections.Generic.List<decimal>();
            foreach (var n in notas) if (n > 0m) validas.Add(n);
            if (validas.Count == 0) return 0m;
            decimal soma = 0m;
            foreach (var n in validas) soma += n;
            return Math.Round(soma / validas.Count, 2);
        }

        public void Concluir(decimal media, DateTime dataConclusao, string usuario)
        {
            Status = StConcluida;
            MediaNota = media;
            DataConclusao = dataConclusao;
            MarcarAlterado(usuario);
        }
    }

    public partial class TltNotaIndicador
    {
        // Secao 18/20: nota inteira entre 1 e 5.
        public static bool NotaValida(int nota) => nota >= 1 && nota <= 5;

        public void ValidarNota()
        {
            if (!NotaValida(Nota))
                AddNotification(nameof(Nota), "A nota do indicador deve estar entre 1 e 5.");
        }
    }

    public partial class TltSolicitacaoLicenca
    {
        // Secao 18/20: status Pendente, Aprovada ou Rejeitada.
        public const string StPendente = "Pendente";
        public const string StAprovada = "Aprovada";
        public const string StRejeitada = "Rejeitada";

        public static bool StatusValido(string status)
            => status == StPendente || status == StAprovada || status == StRejeitada;

        public bool PeriodoValido() => !DataInicio.HasValue || !DataFim.HasValue || DataFim.Value >= DataInicio.Value;

        public void Aprovar(Guid aprovadoPorId, string? comentario, string usuario)
        {
            if (Status != StPendente)
            {
                AddNotification(nameof(Status), "So e possivel aprovar solicitacao pendente.");
                return;
            }
            Status = StAprovada;
            AprovadoPorId = aprovadoPorId;
            ComentarioAprovador = comentario;
            AprovadoEm = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }

        public void Rejeitar(Guid aprovadoPorId, string? comentario, string usuario)
        {
            if (Status != StPendente)
            {
                AddNotification(nameof(Status), "So e possivel rejeitar solicitacao pendente.");
                return;
            }
            Status = StRejeitada;
            AprovadoPorId = aprovadoPorId;
            ComentarioAprovador = comentario;
            AprovadoEm = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }
    }
}
