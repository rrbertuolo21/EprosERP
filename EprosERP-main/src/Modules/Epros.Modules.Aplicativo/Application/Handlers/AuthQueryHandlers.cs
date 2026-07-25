using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    public class ListarEmpresasDisponiveisQueryHandler : IQueryHandler<ListarEmpresasDisponiveisQuery, IEnumerable<UsuarioEmpresaDto>>
    {
        private readonly ContextAplicativo _contextAplicativo;
        private readonly ContextGestaoClientes _contextGestaoClientes;

        public ListarEmpresasDisponiveisQueryHandler(
            ContextAplicativo contextAplicativo,
            ContextGestaoClientes contextGestaoClientes)
        {
            _contextAplicativo = contextAplicativo;
            _contextGestaoClientes = contextGestaoClientes;
        }

        public async Task<IEnumerable<UsuarioEmpresaDto>> Handle(ListarEmpresasDisponiveisQuery request, CancellationToken cancellationToken)
        {
            // Busca os vínculos de empresa do usuário no schema aplicativo
            var vinculos = await _contextAplicativo.UsuariosEmpresas
                .Where(ue => ue.UsuarioId == request.UsuarioId && ue.DeletadoEm == null)
                .ToListAsync(cancellationToken);

            if (!vinculos.Any())
            {
                return Enumerable.Empty<UsuarioEmpresaDto>();
            }

            var empresaIds = vinculos.Select(v => v.EmpresaId).ToList();

            // Busca os nomes das empresas correspondentes no schema plataforma (legado de clientes)
            var empresas = await _contextGestaoClientes.Empresas
                .Where(e => empresaIds.Contains(e.Id) && e.DeletadoEm == null)
                .ToDictionaryAsync(e => e.Id, e => e.RazaoSocial, cancellationToken);

            var resultado = vinculos.Select(v => new UsuarioEmpresaDto(
                EmpresaId: v.EmpresaId,
                RazaoSocial: empresas.TryGetValue(v.EmpresaId, out var nome) ? nome : "Empresa Desconhecida",
                EhAdmin: v.EhAdmin,
                PerfilUsuarioId: v.PerfilAcessoId
            )).ToList();

            return resultado;
        }
    }
}
