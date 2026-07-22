using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class ConfiguracaoGlobal : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string Valor { get; private set; } = string.Empty;
        public bool EhSegredo { get; private set; }
        public string Descricao { get; private set; } = string.Empty;

        protected ConfiguracaoGlobal() { } // EF Core

        public ConfiguracaoGlobal(string chave, string valor, bool ehSegredo, string descricao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ConfiguracaoGlobal>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "Chave da configuração é obrigatória")
                .IsNotNullOrEmpty(valor, nameof(Valor), "Valor da configuração é obrigatório")
            );

            Chave = chave;
            Valor = valor;
            EhSegredo = ehSegredo;
            Descricao = descricao;
        }

        public void Atualizar(string novoValor, string alteradoPor)
        {
            AddNotifications(new Contract<ConfiguracaoGlobal>()
                .Requires()
                .IsNotNullOrEmpty(novoValor, nameof(Valor), "Valor da configuração não pode ser vazio")
            );

            if (IsValid)
            {
                Valor = novoValor;
                MarcarAlterado(alteradoPor);
            }
        }
    }
}
