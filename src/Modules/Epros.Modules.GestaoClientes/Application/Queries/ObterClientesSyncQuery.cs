using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ObterClientesSyncQuery(DateTime Since) : IQuery<IEnumerable<ClienteSyncDto>>;

    public class ClienteSyncDto
    {
        public Guid Id { get; set; }
        public Guid SyncId { get; set; }
        public string RazaoSocial { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid PlanoId { get; set; }
        public int SyncVersion { get; set; }
        public bool Ativo { get; set; }
        public bool Deletado { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime? AlteradoEm { get; set; }
        public DateTime? DeletadoEm { get; set; }
    }
}
