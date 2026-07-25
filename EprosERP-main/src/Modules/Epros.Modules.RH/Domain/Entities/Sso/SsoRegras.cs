using System;

namespace Epros.Modules.RH.Domain.Entities
{
    // Regras de negocio do submodulo Saude e Seguranca Ocupacional (RH-SSO).

    public partial class SsoPpp
    {
        public const string StElaboracao = "Em elaboracao";
        public const string StAtivo = "Ativo";
        public const string StEncerrado = "Encerrado";

        public void Ativar(string usuario)
        {
            Status = StAtivo;
            MarcarAlterado(usuario);
        }

        public void Encerrar(string usuario)
        {
            Status = StEncerrado;
            MarcarAlterado(usuario);
        }
    }

    public partial class SsoExameMedico
    {
        public const string StRegistrado = "Registrado";
        public const string StVencido = "Vencido";
        public const string StSubstituido = "Substituido";

        // SSO-REG-006: exame vencido gera alerta/bloqueio quando politica habilitada.
        public void MarcarVencido(string usuario)
        {
            Status = StVencido;
            MarcarAlterado(usuario);
        }

        public void Substituir(string usuario)
        {
            Status = StSubstituido;
            MarcarAlterado(usuario);
        }
    }

    public partial class SsoBloqueioAlocacao
    {
        public const string StAtivo = "Ativo";
        public const string StResolvido = "Resolvido";

        public void Resolver(string usuario, string observacao)
        {
            Status = StResolvido;
            DataResolucao = DateTime.UtcNow;
            Observacao = observacao;
            MarcarAlterado(usuario);
        }
    }
}
