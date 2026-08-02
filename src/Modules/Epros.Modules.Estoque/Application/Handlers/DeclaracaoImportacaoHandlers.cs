using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>
    /// Handlers da Declaração de Importação (DI) por item de compra e suas adições (CD1 / EF COMERCIO_EXTERIOR,
    /// CEX-001..023). CRUD sobre CompraItemImportacao/CompraItemImportacaoAdicao, que existiam sem CQRS.
    /// valida-contador: montantes (AFRMM, desconto) são factuais — sem cálculo de tributo.
    /// </summary>
    public class RegistrarDeclaracaoImportacaoCommandHandler : ICommandHandler<RegistrarDeclaracaoImportacaoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarDeclaracaoImportacaoCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarDeclaracaoImportacaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (request.CompraItemId == Guid.Empty)
                return CommandResult.Falha("O item de compra da DI é obrigatório [CEX].");
            if (string.IsNullOrWhiteSpace(request.NumeroDeclaracaoImportacao))
                return CommandResult.Falha("O número da declaração de importação é obrigatório [CEX].");

            // CEX-001: a DI pertence a um item de compra existente.
            var itemExiste = await _context.CompraItens.AnyAsync(i => i.Id == request.CompraItemId, cancellationToken);
            if (!itemExiste)
                return CommandResult.Falha("Item de compra não encontrado para a DI.");

            var di = new CompraItemImportacao(
                request.CompraItemId, request.NumeroDeclaracaoImportacao, request.DataDeclaracaoImportacao,
                request.LocalDesembaraco, request.UfDesembaraco, request.DataDesembaraco, request.TipoViaTransporte,
                request.ValorAFRMM /* valida-contador */, request.TipoIntermedio, request.Cnpj, request.Cpf,
                request.UfTerceiro, request.CodigoExportador, tenantId, usuario);
            _context.CompraItemImportacoes.Add(di);

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Declaração de importação registrada.", new { di.Id, di.CompraItemId });
        }
    }

    public class AlterarDeclaracaoImportacaoCommandHandler : ICommandHandler<AlterarDeclaracaoImportacaoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public AlterarDeclaracaoImportacaoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AlterarDeclaracaoImportacaoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var di = await _context.CompraItemImportacoes.FirstOrDefaultAsync(d => d.Id == request.Id && d.DeletadoEm == null, cancellationToken);
            if (di == null)
                return CommandResult.Falha("Declaração de importação não encontrada.");

            di.Alterar(request.NumeroDeclaracaoImportacao, request.DataDeclaracaoImportacao, request.LocalDesembaraco,
                request.UfDesembaraco, request.DataDesembaraco, request.TipoViaTransporte, request.ValorAFRMM /* valida-contador */,
                request.TipoIntermedio, request.Cnpj, request.Cpf, request.UfTerceiro, request.CodigoExportador, usuario);

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Declaração de importação atualizada.", new { di.Id });
        }
    }

    public class ExcluirDeclaracaoImportacaoCommandHandler : ICommandHandler<ExcluirDeclaracaoImportacaoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public ExcluirDeclaracaoImportacaoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ExcluirDeclaracaoImportacaoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var di = await _context.CompraItemImportacoes.Include(d => d.Adicoes)
                .FirstOrDefaultAsync(d => d.Id == request.Id && d.DeletadoEm == null, cancellationToken);
            if (di == null)
                return CommandResult.Falha("Declaração de importação não encontrada.");

            di.DeletarComAdicoes(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Declaração de importação excluída.", new { di.Id });
        }
    }

    public class AdicionarAdicaoImportacaoCommandHandler : ICommandHandler<AdicionarAdicaoImportacaoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public AdicionarAdicaoImportacaoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarAdicaoImportacaoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            // A adição é um filho do agregado DI; a operamos diretamente no DbSet do filho (evita mutar a
            // coleção rastreada do pai). A DI precisa existir e não estar excluída.
            var di = await _context.CompraItemImportacoes.AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == request.DeclaracaoImportacaoId && d.DeletadoEm == null, cancellationToken);
            if (di == null)
                return CommandResult.Falha("Declaração de importação não encontrada.");

            var adicao = new CompraItemImportacaoAdicao(di.Id, request.NumeroAdicao, request.NumeroSequencialAdicao,
                request.CodigoFabricante, request.ValorDesconto /* valida-contador */, request.NumeroAtoConcessorio, di.TenantId, usuario);
            _context.CompraItemImportacaoAdicoes.Add(adicao);

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Adição registrada.", new { di.Id, adicaoId = adicao.Id });
        }
    }

    public class AlterarAdicaoImportacaoCommandHandler : ICommandHandler<AlterarAdicaoImportacaoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public AlterarAdicaoImportacaoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AlterarAdicaoImportacaoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var adicao = await _context.CompraItemImportacaoAdicoes
                .FirstOrDefaultAsync(a => a.Id == request.AdicaoId && a.CompraItemImportacaoId == request.DeclaracaoImportacaoId && a.DeletadoEm == null, cancellationToken);
            if (adicao == null)
                return CommandResult.Falha("Adição não encontrada na declaração de importação.");

            adicao.Alterar(request.NumeroAdicao, request.NumeroSequencialAdicao, request.CodigoFabricante,
                request.ValorDesconto /* valida-contador */, request.NumeroAtoConcessorio, usuario);

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Adição atualizada.", new { DeclaracaoImportacaoId = request.DeclaracaoImportacaoId, request.AdicaoId });
        }
    }

    public class ExcluirAdicaoImportacaoCommandHandler : ICommandHandler<ExcluirAdicaoImportacaoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public ExcluirAdicaoImportacaoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ExcluirAdicaoImportacaoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var adicao = await _context.CompraItemImportacaoAdicoes
                .FirstOrDefaultAsync(a => a.Id == request.AdicaoId && a.CompraItemImportacaoId == request.DeclaracaoImportacaoId && a.DeletadoEm == null, cancellationToken);
            if (adicao == null)
                return CommandResult.Falha("Adição não encontrada na declaração de importação.");

            adicao.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Adição excluída.", new { DeclaracaoImportacaoId = request.DeclaracaoImportacaoId, request.AdicaoId });
        }
    }
}
