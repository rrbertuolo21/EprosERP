using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Application.Security;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Handlers
{
    public class CriarPortalUsuarioClienteCommandHandler : ICommandHandler<CriarPortalUsuarioClienteCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPortalUsuarioClienteCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPortalUsuarioClienteCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // T-02: para principal externo, o cliente vem do vínculo autenticado (nunca do request).
            var clienteReq = request.ClienteId == System.Guid.Empty ? (System.Guid?)null : request.ClienteId;
            var (erroAcesso, clienteEfetivo) = await PortalClienteAcesso.ResolverAsync(_currentUser, _context, tenantId, clienteReq, cancellationToken);
            if (erroAcesso != null) return erroAcesso;
            var clienteId = clienteEfetivo ?? request.ClienteId;

            // §16.1 (UK email): e-mail único por tenant.
            var duplicado = await _context.PortalUsuariosCliente.AnyAsync(u => u.TenantId == tenantId && u.Email == request.Email, cancellationToken);
            if (duplicado) return CommandResult.Falha("Já existe um usuário do portal com este e-mail.");

            var novo = new PortalUsuarioCliente(clienteId, request.Nome, request.Email, request.Telefone, request.AdministradorCliente, null, tenantId, usuario);
            if (!novo.IsValid) return CommandResult.Falha(novo.Notifications.Select(n => n.Message), "Dados do usuário do portal inválidos.");
            _context.PortalUsuariosCliente.Add(novo);
            _context.PortalAuditorias.Add(new PortalAuditoria(null, null, clienteId, "Usuarios", "Criacao", novo.Id, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Usuário do portal criado.", new { novo.Id });
        }
    }

    public class DefinirPortalPermissaoCommandHandler : ICommandHandler<DefinirPortalPermissaoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DefinirPortalPermissaoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DefinirPortalPermissaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // T-02/§18: a permissão só pode ser definida sobre usuário do próprio cliente (para principal externo).
            var alvo = await _context.PortalUsuariosCliente.AsNoTracking()
                .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Id == request.UsuarioClienteId, cancellationToken);
            if (alvo == null) return CommandResult.Falha("Usuário do portal não encontrado.");
            var erroAcesso = await PortalClienteAcesso.GarantirClienteDoRecursoAsync(_currentUser, _context, tenantId, alvo.ClienteId, cancellationToken);
            if (erroAcesso != null) return erroAcesso;

            var existente = await _context.PortalPermissoes.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.UsuarioClienteId == request.UsuarioClienteId && p.Recurso == request.Recurso, cancellationToken);
            if (existente != null)
            {
                existente.Alterar(request.PodeVisualizar, request.PodeCriar, request.PodeBaixar, request.PodeAdministrar, usuario);
            }
            else
            {
                var permissao = new PortalPermissao(request.UsuarioClienteId, request.Recurso, request.PodeVisualizar, request.PodeCriar, request.PodeBaixar, request.PodeAdministrar, tenantId, usuario);
                if (!permissao.IsValid) return CommandResult.Falha(permissao.Notifications.Select(n => n.Message), "Dados da permissão inválidos.");
                _context.PortalPermissoes.Add(permissao);
            }
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Permissão do portal definida.", new { request.UsuarioClienteId, Recurso = request.Recurso.ToString() });
        }
    }

    public class CriarPortalFormularioCommandHandler : ICommandHandler<CriarPortalFormularioCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPortalFormularioCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPortalFormularioCommand request, CancellationToken cancellationToken)
        {
            // Catálogo de formulários é backoffice interno — nunca acessível por usuário externo do portal.
            var erroInterno = PortalClienteAcesso.SomenteInterno(_currentUser);
            if (erroInterno != null) return erroInterno;
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var form = new PortalFormulario(request.Codigo, request.Nome, request.Descricao, request.Publico, request.ConfiguracaoCampos, tenantId, usuario);
            if (!form.IsValid) return CommandResult.Falha(form.Notifications.Select(n => n.Message), "Dados do formulário inválidos.");
            _context.PortalFormularios.Add(form);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Formulário criado.", new { form.Id });
        }
    }

    public class PublicarPortalFormularioCommandHandler : ICommandHandler<PublicarPortalFormularioCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public PublicarPortalFormularioCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(PublicarPortalFormularioCommand request, CancellationToken cancellationToken)
        {
            var erroInterno = PortalClienteAcesso.SomenteInterno(_currentUser);
            if (erroInterno != null) return erroInterno;
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var form = await _context.PortalFormularios.FirstOrDefaultAsync(f => f.TenantId == tenantId && f.Id == request.FormularioId, cancellationToken);
            if (form == null) return CommandResult.Falha("Formulário não encontrado.");
            form.Publicar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Formulário publicado.", new { form.Id, Status = form.Status.ToString() });
        }
    }

    public class AtribuirPortalFormularioResponsavelCommandHandler : ICommandHandler<AtribuirPortalFormularioResponsavelCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtribuirPortalFormularioResponsavelCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtribuirPortalFormularioResponsavelCommand request, CancellationToken cancellationToken)
        {
            var erroInterno = PortalClienteAcesso.SomenteInterno(_currentUser);
            if (erroInterno != null) return erroInterno;
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var form = await _context.PortalFormularios.FirstOrDefaultAsync(f => f.TenantId == tenantId && f.Id == request.FormularioId, cancellationToken);
            if (form == null) return CommandResult.Falha("Formulário não encontrado.");
            var resp = new PortalFormularioResponsavel(request.FormularioId, request.UsuarioInternoId, request.Papel, tenantId, usuario);
            if (!resp.IsValid) return CommandResult.Falha(resp.Notifications.Select(n => n.Message), "Dados do responsável inválidos.");
            _context.PortalFormularioResponsaveis.Add(resp);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Responsável atribuído.", new { resp.Id });
        }
    }

    public class AbrirPortalSolicitacaoCommandHandler : ICommandHandler<AbrirPortalSolicitacaoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AbrirPortalSolicitacaoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AbrirPortalSolicitacaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // T-02: para principal externo, o cliente da solicitação é o do vínculo autenticado (nunca o request).
            var (erroAcesso, clienteEfetivo) = await PortalClienteAcesso.ResolverAsync(_currentUser, _context, tenantId, request.ClienteId, cancellationToken);
            if (erroAcesso != null) return erroAcesso;
            var clienteId = clienteEfetivo ?? request.ClienteId;

            var solicitacao = new PortalSolicitacao(clienteId, request.UsuarioClienteId, request.FormularioId, request.Assunto, request.Descricao, request.DadosFormulario, tenantId, usuario);
            if (!solicitacao.IsValid) return CommandResult.Falha(solicitacao.Notifications.Select(n => n.Message), "Dados da solicitação inválidos.");
            _context.PortalSolicitacoes.Add(solicitacao);
            _context.PortalAuditorias.Add(new PortalAuditoria(request.UsuarioClienteId, null, clienteId, "Solicitacoes", "Abertura", solicitacao.Id, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Solicitação aberta.", new { solicitacao.Id, Status = solicitacao.Status.ToString() });
        }
    }

    public class ResponderPortalSolicitacaoCommandHandler : ICommandHandler<ResponderPortalSolicitacaoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ResponderPortalSolicitacaoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ResponderPortalSolicitacaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var solicitacao = await _context.PortalSolicitacoes.FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == request.SolicitacaoId, cancellationToken);
            if (solicitacao == null) return CommandResult.Falha("Solicitação não encontrada.");
            // T-02: principal externo só atua sobre solicitação do próprio cliente.
            var erroAcesso = await PortalClienteAcesso.GarantirClienteDoRecursoAsync(_currentUser, _context, tenantId, solicitacao.ClienteId, cancellationToken);
            if (erroAcesso != null) return erroAcesso;
            solicitacao.Responder(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Solicitação respondida.", new { solicitacao.Id, Status = solicitacao.Status.ToString() });
        }
    }
}
