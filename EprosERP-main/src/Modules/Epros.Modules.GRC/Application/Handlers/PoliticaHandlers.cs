using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GRC.Application.Commands;
using Epros.Modules.GRC.Domain.Entities;
using Epros.Modules.GRC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GRC.Application.Handlers
{
    public class CriarPoliticaCommandHandler : ICommandHandler<CriarPoliticaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPoliticaCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPoliticaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // RN-POL-003: codigo unico por tenant.
            var duplicado = await _context.Politicas
                .AnyAsync(p => p.Codigo == request.Codigo, cancellationToken);
            if (duplicado)
            {
                return CommandResult.Falha("Ja existe politica com este codigo para a empresa/tenant.");
            }

            var politica = new Politica(
                request.Codigo, request.Titulo, request.Descricao, request.Categoria,
                request.OwnerId, request.ModuloAplicavel, tenantId, usuario);

            if (!politica.IsValid)
            {
                return CommandResult.Falha(politica.Notifications.Select(n => n.Message));
            }

            _context.Politicas.Add(politica);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Politica criada em rascunho com sucesso!", new { PoliticaId = politica.Id, politica.Status });
        }
    }

    public class CriarVersaoPoliticaCommandHandler : ICommandHandler<CriarVersaoPoliticaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarVersaoPoliticaCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarVersaoPoliticaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var politicaExiste = await _context.Politicas.AnyAsync(p => p.Id == request.PoliticaId, cancellationToken);
            if (!politicaExiste)
            {
                return CommandResult.Falha("Politica nao encontrada.");
            }

            // Versao unica por politica.
            var versaoDuplicada = await _context.PoliticaVersoes
                .AnyAsync(v => v.PoliticaId == request.PoliticaId && v.NumeroVersao == request.NumeroVersao, cancellationToken);
            if (versaoDuplicada)
            {
                return CommandResult.Falha("Ja existe esta versao para a politica.");
            }

            var versao = new PoliticaVersao(
                request.PoliticaId, request.NumeroVersao, request.DataInicioVigencia, request.DataFimVigencia,
                request.ResumoAlteracoes, request.MotivoVersao, tenantId, usuario);

            if (!versao.IsValid)
            {
                return CommandResult.Falha(versao.Notifications.Select(n => n.Message));
            }

            _context.PoliticaVersoes.Add(versao);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Versao da politica criada com sucesso!", new { VersaoId = versao.Id, versao.Status });
        }
    }

    public class PublicarPoliticaCommandHandler : ICommandHandler<PublicarPoliticaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ICurrentUser _currentUser;

        public PublicarPoliticaCommandHandler(ContextGRC context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(PublicarPoliticaCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var politica = await _context.Politicas.FirstOrDefaultAsync(p => p.Id == request.PoliticaId, cancellationToken);
            if (politica == null)
            {
                return CommandResult.Falha("Politica nao encontrada.");
            }

            var versao = await _context.PoliticaVersoes
                .FirstOrDefaultAsync(v => v.Id == request.VersaoId && v.PoliticaId == request.PoliticaId, cancellationToken);
            if (versao == null)
            {
                // RN-POL-004: politica publicada deve possuir versao vigente.
                return CommandResult.Falha("A politica nao possui a versao informada para publicacao.");
            }

            versao.Aprovar(usuario);
            versao.Publicar(usuario);
            if (!versao.IsValid)
            {
                return CommandResult.Falha(versao.Notifications.Select(n => n.Message));
            }

            politica.Publicar(usuario);
            if (!politica.IsValid)
            {
                return CommandResult.Falha(politica.Notifications.Select(n => n.Message));
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Politica publicada com sucesso!", new { PoliticaId = politica.Id, VersaoId = versao.Id });
        }
    }

    public class RegistrarAceitePoliticaCommandHandler : ICommandHandler<RegistrarAceitePoliticaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarAceitePoliticaCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarAceitePoliticaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // RN-POL-007: usuario nao pode aceitar versao arquivada ou nao publicada.
            var versao = await _context.PoliticaVersoes
                .FirstOrDefaultAsync(v => v.Id == request.VersaoId && v.PoliticaId == request.PoliticaId, cancellationToken);
            if (versao == null)
            {
                return CommandResult.Falha("Versao da politica nao encontrada.");
            }
            if (versao.Status != "Publicada")
            {
                return CommandResult.Falha("A politica nao esta publicada para este usuario ou versao.");
            }

            // RN-POL-005: um aceite por usuario/versao/tenant.
            var jaAceitou = await _context.PoliticaAceites
                .AnyAsync(a => a.VersaoId == request.VersaoId && a.UsuarioId == request.UsuarioId, cancellationToken);
            if (jaAceitou)
            {
                return CommandResult.Falha("O aceite desta versao ja foi registrado para este usuario.");
            }

            var aceite = new PoliticaAceite(
                request.PoliticaId, request.VersaoId, request.UsuarioId, request.Ip,
                request.Origem, request.DeclaracaoAceite, tenantId, usuario);

            if (!aceite.IsValid)
            {
                return CommandResult.Falha(aceite.Notifications.Select(n => n.Message));
            }

            _context.PoliticaAceites.Add(aceite);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Aceite registrado com sucesso!", new { AceiteId = aceite.Id });
        }
    }
}
