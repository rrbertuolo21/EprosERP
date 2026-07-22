using System;

namespace Epros.Modules.GestaoClientes.Application.Dtos
{
    public class ConfiguracaoGlobalCacheDto
    {
        public Guid Id { get; set; }
        public string Chave { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public bool EhSegredo { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;

        public ConfiguracaoGlobalCacheDto() { }

        public ConfiguracaoGlobalCacheDto(Guid id, string chave, string valor, bool ehSegredo, string descricao, string tenantId)
        {
            Id = id;
            Chave = chave;
            Valor = valor;
            EhSegredo = ehSegredo;
            Descricao = descricao;
            TenantId = tenantId;
        }
    }
}
