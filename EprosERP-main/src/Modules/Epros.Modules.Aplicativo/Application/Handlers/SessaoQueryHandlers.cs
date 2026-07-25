using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    /// <summary>
    /// Handler de recuperação de sessão. Concentra toda a leitura de dados (usuário, empresas
    /// vinculadas e verificação de bloqueio por inadimplência SaaS) que antes estava inline no
    /// <c>AccountController.Session</c> — resolvendo o gap Q5 (controller com DbContext + regra
    /// de negócio). O controller agora apenas valida o token e delega.
    /// </summary>
    public class RecuperarSessaoQueryHandler : IQueryHandler<RecuperarSessaoQuery, CommandResult>
    {
        private readonly ContextAplicativo _contextApp;
        private readonly ContextGestaoClientes _contextGestao;

        public RecuperarSessaoQueryHandler(ContextAplicativo contextApp, ContextGestaoClientes contextGestao)
        {
            _contextApp = contextApp;
            _contextGestao = contextGestao;
        }

        public async Task<CommandResult> Handle(RecuperarSessaoQuery request, CancellationToken cancellationToken)
        {
            var usuario = await _contextApp.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioId
                                          && u.TenantId == request.TenantId
                                          && u.DeletadoEm == null, cancellationToken);

            if (usuario == null || usuario.Status != UsuarioStatus.Active)
            {
                return CommandResult.Falha(new[] { "Usuário inativo ou inexistente." });
            }

            // Empresas vinculadas ao usuário (schema aplicativo)
            var empresasVinculadas = await _contextApp.UsuariosEmpresas
                .Where(ue => ue.UsuarioId == usuario.Id && ue.DeletadoEm == null)
                .ToListAsync(cancellationToken);

            var empresaIds = empresasVinculadas.Select(ue => ue.EmpresaId).ToList();
            var empresasDetalhadas = await _contextGestao.Empresas
                .Where(e => e.TenantId == request.TenantId && empresaIds.Contains(e.Id) && e.DeletadoEm == null)
                .ToListAsync(cancellationToken);

            var empresasDto = empresasVinculadas.Select(ue =>
            {
                var emp = empresasDetalhadas.FirstOrDefault(e => e.Id == ue.EmpresaId);
                return new UsuarioEmpresaDto(
                    ue.EmpresaId,
                    emp?.RazaoSocial ?? "Empresa Desconhecida",
                    ue.EhAdmin,
                    ue.PerfilAcessoId
                );
            }).ToList();

            // Regra de bloqueio SaaS por inadimplência (mantida idêntica ao comportamento anterior)
            var cliente = await _contextGestao.Clientes
                .FirstOrDefaultAsync(c => c.TenantId == request.TenantId && c.DeletadoEm == null, cancellationToken);

            var block = false;
            if (cliente != null && cliente.Ativo)
            {
                var dataLimite = DateTime.UtcNow.AddDays(-15);
                block = await _contextGestao.Faturas
                    .AnyAsync(f => f.ClienteId == cliente.Id
                                   && f.Status != FaturaStatus.Paga
                                   && f.Status != FaturaStatus.Cancelada
                                   && f.DataVencimento < dataLimite
                                   && f.DeletadoEm == null, cancellationToken);
            }

            var response = new
            {
                token = request.Token,
                empresas = empresasDto,
                tenantId = request.TenantId,
                login = usuario.Email,
                block = block
            };

            return CommandResult.Ok("Sessão recuperada com sucesso.", response);
        }
    }
}
