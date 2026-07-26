using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Handlers
{
    public class CriarContratoTipoCommandHandler : ICommandHandler<CriarContratoTipoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarContratoTipoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarContratoTipoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var tipo = new ContratoTipo(request.Nome, request.Ativo, null, tenantId, usuario);
            if (!tipo.IsValid) return CommandResult.Falha(tipo.Notifications.Select(n => n.Message), "Dados do tipo de contrato inválidos.");
            _context.ContratoTipos.Add(tipo);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Tipo de contrato criado.", new { tipo.Id });
        }
    }

    public class CriarContratoCommandHandler : ICommandHandler<CriarContratoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarContratoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarContratoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var contrato = new Contrato(
                request.Assunto, request.NumeroContrato, request.TipoOrigem, request.NumeroModelo, request.ClienteId,
                request.UsuarioResponsavelId, request.TipoId, request.Valor, request.DataInicio, request.DataFim,
                request.Descricao, request.CorpoDocumento, request.ProjetoId, request.LeadId, request.PropostaId,
                request.PedidoId, request.CategoriaId, request.AutomacaoHabilitada, request.AutomacaoConfigJson,
                request.CriadoPorUsuarioId, request.OwnerUsuarioId, tenantId, usuario);
            if (!contrato.IsValid) return CommandResult.Falha(contrato.Notifications.Select(n => n.Message), "Dados do contrato inválidos.");

            // GCV-009/GCV-011: número gerado por tenant/prefixo/origem quando vazio.
            if (string.IsNullOrWhiteSpace(request.NumeroContrato))
            {
                var config = await _context.ContratoConfiguracoes.FirstOrDefaultAsync(c => c.TenantId == tenantId, cancellationToken);
                var prefixo = config?.PrefixoContrato ?? "CT";
                var sequencia = await _context.Contratos.CountAsync(c => c.TenantId == tenantId && c.TipoOrigem == contrato.TipoOrigem, cancellationToken) + 1;
                contrato.DefinirNumero($"{prefixo}{sequencia:D4}", usuario);
            }

            _context.Contratos.Add(contrato);
            _context.ContratoHistoricos.Add(new ContratoHistorico(contrato.Id, EContratoEvento.Criacao, request.CriadoPorUsuarioId, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Contrato criado com sucesso!", new { contrato.Id, contrato.NumeroContrato, Status = contrato.Status.ToString() });
        }
    }

    public class PublicarContratoCommandHandler : ICommandHandler<PublicarContratoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public PublicarContratoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(PublicarContratoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var contrato = await _context.Contratos.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == request.ContratoId, cancellationToken);
            if (contrato == null) return CommandResult.Falha("Contrato não encontrado.");
            contrato.Publicar(request.PublicacaoAgendadaEm, request.Enviar, usuario);
            if (!contrato.IsValid) return CommandResult.Falha(contrato.Notifications.Select(n => n.Message), "Não foi possível publicar o contrato.");
            _context.ContratoHistoricos.Add(new ContratoHistorico(contrato.Id, EContratoEvento.Publicacao, null, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Contrato publicado.", new { contrato.Id, Status = contrato.Status.ToString() });
        }
    }

    public class AssinarContratoCommandHandler : ICommandHandler<AssinarContratoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AssinarContratoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AssinarContratoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var contrato = await _context.Contratos.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == request.ContratoId, cancellationToken);
            if (contrato == null) return CommandResult.Falha("Contrato não encontrado.");

            // GCV-030: uma assinatura por usuário e contrato.
            if (request.UsuarioId.HasValue)
            {
                var jaAssinou = await _context.ContratoAssinaturas.AnyAsync(a => a.TenantId == tenantId && a.ContratoId == request.ContratoId && a.UsuarioId == request.UsuarioId, cancellationToken);
                if (jaAssinou) return CommandResult.Falha("Usuário já assinou este contrato.");
            }

            var assinatura = new ContratoAssinatura(request.ContratoId, request.UsuarioId, request.Parte, request.TipoAssinatura, request.DadosAssinatura, request.UsuarioId, tenantId, usuario);
            if (!assinatura.IsValid) return CommandResult.Falha(assinatura.Notifications.Select(n => n.Message), "Dados da assinatura inválidos.");
            _context.ContratoAssinaturas.Add(assinatura);

            // GCV-034: assinaturas completas evoluem o contrato para Ativo.
            contrato.RegistrarAssinatura(request.Parte, usuario);
            _context.ContratoHistoricos.Add(new ContratoHistorico(contrato.Id, EContratoEvento.Assinatura, request.UsuarioId, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Contrato assinado.", new { contrato.Id, Status = contrato.Status.ToString() });
        }
    }

    public class CriarContratoRenovacaoCommandHandler : ICommandHandler<CriarContratoRenovacaoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarContratoRenovacaoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarContratoRenovacaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var contrato = await _context.Contratos.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == request.ContratoId, cancellationToken);
            if (contrato == null) return CommandResult.Falha("Contrato não encontrado.");
            var renovacao = new ContratoRenovacao(request.ContratoId, request.DataInicio, request.DataFim, request.Valor, request.Notas, request.Status, request.CriadoPorUsuarioId, tenantId, usuario);
            if (!renovacao.IsValid) return CommandResult.Falha(renovacao.Notifications.Select(n => n.Message), "Dados da renovação inválidos.");
            _context.ContratoRenovacoes.Add(renovacao);
            _context.ContratoHistoricos.Add(new ContratoHistorico(contrato.Id, EContratoEvento.Renovacao, request.CriadoPorUsuarioId, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Renovação criada.", new { renovacao.Id });
        }
    }

    public class AdicionarContratoComentarioCommandHandler : ICommandHandler<AdicionarContratoComentarioCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarContratoComentarioCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarContratoComentarioCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var comentario = new ContratoComentario(request.ContratoId, request.Comentario, request.UsuarioId, tenantId, usuario);
            if (!comentario.IsValid) return CommandResult.Falha(comentario.Notifications.Select(n => n.Message), "Dados do comentário inválidos.");
            _context.ContratoComentarios.Add(comentario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Comentário adicionado.", new { comentario.Id });
        }
    }

    public class CriarContratoModeloCommandHandler : ICommandHandler<CriarContratoModeloCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarContratoModeloCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarContratoModeloCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var modelo = new ContratoModelo(request.Titulo, request.Corpo, request.CorCabecalho, request.Sistema, tenantId, usuario);
            if (!modelo.IsValid) return CommandResult.Falha(modelo.Notifications.Select(n => n.Message), "Dados do modelo inválidos.");
            _context.ContratoModelos.Add(modelo);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Modelo de contrato criado.", new { modelo.Id });
        }
    }
}
