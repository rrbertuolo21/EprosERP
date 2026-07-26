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
    // ============================ CategoriaProduto ============================

    /// <summary>Cria Categoria Produto.</summary>
    public class CriarCategoriaProdutoCommandHandler : ICommandHandler<CriarCategoriaProdutoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCategoriaProdutoCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCategoriaProdutoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var categoria = new CategoriaProduto(request.Descricao, request.ProdutoGrupoId, tenantId, criadoPor);
            if (!categoria.IsValid)
                return CommandResult.Falha(categoria.Notifications.Select(n => n.Message), "Falha na validação da categoria.");

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Categoria criada com sucesso!", new { categoria.Id });
        }
    }

    /// <summary>Atualiza Categoria Produto.</summary>
    public class AtualizarCategoriaProdutoCommandHandler : ICommandHandler<AtualizarCategoriaProdutoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarCategoriaProdutoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarCategoriaProdutoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            if (categoria == null)
                return CommandResult.Falha("Categoria não encontrada.");

            categoria.Alterar(request.Descricao, request.ProdutoGrupoId, usuario);
            if (!categoria.IsValid)
                return CommandResult.Falha(categoria.Notifications.Select(n => n.Message), "Falha na validação da categoria.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Categoria atualizada com sucesso!");
        }
    }

    /// <summary>Exclui Categoria Produto.</summary>
    public class DeletarCategoriaProdutoCommandHandler : ICommandHandler<DeletarCategoriaProdutoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public DeletarCategoriaProdutoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarCategoriaProdutoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            if (categoria == null)
                return CommandResult.Falha("Categoria não encontrada.");

            categoria.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Categoria excluída com sucesso!");
        }
    }

    // ============================ MarcaProduto ============================

    /// <summary>Cria Marca Produto.</summary>
    public class CriarMarcaProdutoCommandHandler : ICommandHandler<CriarMarcaProdutoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarMarcaProdutoCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarMarcaProdutoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var marca = new MarcaProduto(request.Descricao, request.ProdutoGrupoId, tenantId, criadoPor);
            if (!marca.IsValid)
                return CommandResult.Falha(marca.Notifications.Select(n => n.Message), "Falha na validação da marca.");

            _context.Marcas.Add(marca);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Marca criada com sucesso!", new { marca.Id });
        }
    }

    /// <summary>Atualiza Marca Produto.</summary>
    public class AtualizarMarcaProdutoCommandHandler : ICommandHandler<AtualizarMarcaProdutoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarMarcaProdutoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarMarcaProdutoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var marca = await _context.Marcas.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            if (marca == null)
                return CommandResult.Falha("Marca não encontrada.");

            marca.Alterar(request.Descricao, request.ProdutoGrupoId, usuario);
            if (!marca.IsValid)
                return CommandResult.Falha(marca.Notifications.Select(n => n.Message), "Falha na validação da marca.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Marca atualizada com sucesso!");
        }
    }

    /// <summary>Exclui Marca Produto.</summary>
    public class DeletarMarcaProdutoCommandHandler : ICommandHandler<DeletarMarcaProdutoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public DeletarMarcaProdutoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarMarcaProdutoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var marca = await _context.Marcas.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            if (marca == null)
                return CommandResult.Falha("Marca não encontrada.");

            marca.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Marca excluída com sucesso!");
        }
    }

    // ============================ UnidadeMedidaComercial ============================

    /// <summary>Cria Unidade Medida Comercial.</summary>
    public class CriarUnidadeMedidaComercialCommandHandler : ICommandHandler<CriarUnidadeMedidaComercialCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarUnidadeMedidaComercialCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarUnidadeMedidaComercialCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var unidade = new UnidadeMedidaComercial(request.UnidadeMedida, request.Descricao, request.Fator, request.ProdutoGrupoId, tenantId, criadoPor);
            if (!unidade.IsValid)
                return CommandResult.Falha(unidade.Notifications.Select(n => n.Message), "Falha na validação da unidade de medida.");

            _context.UnidadesMedida.Add(unidade);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Unidade de medida criada com sucesso!", new { unidade.Id });
        }
    }

    /// <summary>Atualiza Unidade Medida Comercial.</summary>
    public class AtualizarUnidadeMedidaComercialCommandHandler : ICommandHandler<AtualizarUnidadeMedidaComercialCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarUnidadeMedidaComercialCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarUnidadeMedidaComercialCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var unidade = await _context.UnidadesMedida.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
            if (unidade == null)
                return CommandResult.Falha("Unidade de medida não encontrada.");

            unidade.Alterar(request.UnidadeMedida, request.Descricao, request.Fator, request.ProdutoGrupoId, usuario);
            if (!unidade.IsValid)
                return CommandResult.Falha(unidade.Notifications.Select(n => n.Message), "Falha na validação da unidade de medida.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Unidade de medida atualizada com sucesso!");
        }
    }

    /// <summary>Exclui Unidade Medida Comercial.</summary>
    public class DeletarUnidadeMedidaComercialCommandHandler : ICommandHandler<DeletarUnidadeMedidaComercialCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public DeletarUnidadeMedidaComercialCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarUnidadeMedidaComercialCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var unidade = await _context.UnidadesMedida.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
            if (unidade == null)
                return CommandResult.Falha("Unidade de medida não encontrada.");

            unidade.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Unidade de medida excluída com sucesso!");
        }
    }
}
