using System;

namespace Epros.Modules.RH.Domain.Entities
{
    // Regras de negocio do submodulo Recrutamento (RH-REC).

    public partial class RecVaga
    {
        public const string StRascunho = "draft";
        public const string StAtiva = "active";

        // REC-REG-019: publicar/despublicar sincroniza status, indicador e data.
        public void Publicar(string usuario)
        {
            Publicada = true;
            Status = StAtiva;
            DataPublicacao = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }

        public void Despublicar(string usuario)
        {
            Publicada = false;
            Status = StRascunho;
            MarcarAlterado(usuario);
        }
    }

    public partial class RecCandidato
    {
        // REC-REG-027: status aceita apenas 0,1,2,3,4,5.
        public static bool StatusValido(string status)
            => status == "0" || status == "1" || status == "2" || status == "3" || status == "4" || status == "5";

        public void AtualizarStatus(string status, string usuario)
        {
            if (!StatusValido(status))
            {
                AddNotification(nameof(Status), "Status de candidato deve ser 0,1,2,3,4 ou 5.");
                return;
            }
            Status = status;
            MarcarAlterado(usuario);
        }

        // REC-REG-046: conversao para colaborador atualiza candidato para contratado (4).
        public void MarcarContratado(string usuario)
        {
            Status = "4";
            MarcarAlterado(usuario);
        }
    }

    public partial class RecEntrevista
    {
        public const int StPendente = 0;
        public const int StConcluida = 1;

        // REC-REG-035: entrevista concluida.
        public void Concluir(string usuario)
        {
            Status = StConcluida;
            MarcarAlterado(usuario);
        }

        // REC-REG-038: salvar feedback marca entrevista como feedback enviado.
        public void MarcarFeedbackEnviado(string usuario)
        {
            FeedbackEnviado = true;
            MarcarAlterado(usuario);
        }
    }

    public partial class RecFeedbackEntrevista
    {
        // REC-REG-037: nota geral e a media entre tecnica, comunicacao e aderencia cultural.
        public static decimal CalcularNotaGeral(decimal tecnica, decimal comunicacao, decimal aderenciaCultural)
            => Math.Round((tecnica + comunicacao + aderenciaCultural) / 3m, 2);

        public void AplicarNotaGeral()
        {
            NotaGeral = CalcularNotaGeral(NotaTecnica, NotaComunicacao, NotaAderenciaCultural);
        }
    }

    public partial class RecOferta
    {
        // REC-REG-046: conversao cria colaborador e marca oferta como convertida.
        public void ConverterEmColaborador(Guid colaboradorId, string usuario)
        {
            if (ConvertidaColaborador)
            {
                AddNotification(nameof(ConvertidaColaborador), "Oferta ja convertida em colaborador.");
                return;
            }
            ColaboradorId = colaboradorId;
            ConvertidaColaborador = true;
            MarcarAlterado(usuario);
        }
    }
}
