using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using Epros.Modules.GestaoClientes.Domain.Entities;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarEmpresaContatosQuery(Guid EmpresaId) : IQuery<List<EmpresaContatoDto>>;

    public record ListarIeStsQuery(Guid EmpresaId) : IQuery<List<IeStDto>>;

    public class EmpresaContatoDto
    {
        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Email { get; set; }
        public ETipoContatoTelefonico? TipoTelefone { get; set; }
        public string? Telefone { get; set; }
    }

    public class IeStDto
    {
        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public EEstado Uf { get; set; }
        public string Ie { get; set; } = string.Empty;
    }
}
