using System;

namespace Epros.Modules.GestaoClientes.Infrastructure.Data
{
    // Leitura cross-module (schema financas) — apenas o mínimo necessário para
    // validar a exclusão de Pessoa (REG-PEM-126/129). Não é o agregado dono.
    public class ContaAPagarLookup
    {
        public Guid Id { get; set; }
        public Guid PessoaId { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public DateTime? DeletadoEm { get; set; }
    }

    public class ContaAReceberLookup
    {
        public Guid Id { get; set; }
        public Guid PessoaId { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public DateTime? DeletadoEm { get; set; }
    }
}
