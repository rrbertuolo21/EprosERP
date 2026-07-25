using System;

namespace Epros.Modules.RH.Domain.Entities
{
    // Regras de negocio do submodulo Desenvolvimento de Funcionarios (RH-DEV).

    public partial class DevPromocao
    {
        // Secao 14/21.10: status de promocao aceita apenas Pendente, Aprovado ou Rejeitado.
        public const string StPendente = "Pendente";
        public const string StAprovado = "Aprovado";
        public const string StRejeitado = "Rejeitado";

        public static bool StatusValido(string status)
            => status == StPendente || status == StAprovado || status == StRejeitado;

        public void Aprovar(string usuario)
        {
            if (Status != StPendente)
            {
                AddNotification(nameof(Status), "So e possivel aprovar promocao pendente.");
                return;
            }
            Status = StAprovado;
            MarcarAlterado(usuario);
        }

        public void Rejeitar(string usuario)
        {
            if (Status != StPendente)
            {
                AddNotification(nameof(Status), "So e possivel rejeitar promocao pendente.");
                return;
            }
            Status = StRejeitado;
            MarcarAlterado(usuario);
        }
    }

    public partial class DevAdvertencia
    {
        // Secao 17: colaborador pode registrar resposta (texto ou nula).
        public void RegistrarResposta(string? resposta, string usuario)
        {
            RespostaColaborador = resposta;
            MarcarAlterado(usuario);
        }
    }

    public partial class DevReclamacao
    {
        public const string StAberta = "Aberta";
        public const string StResolvida = "Resolvida";

        // Secao 12: resolucao registra responsavel e data.
        public void Resolver(Guid resolvidoPor, DateTime dataResolucao, string usuario)
        {
            ResolvidoPor = resolvidoPor;
            DataResolucao = dataResolucao;
            Status = StResolvida;
            MarcarAlterado(usuario);
        }
    }
}
