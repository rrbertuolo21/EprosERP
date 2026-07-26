using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    internal static class ModuloPlanoInputExtensions
    {
        /// <summary>Nome efetivamente persistido em ModuloPlano.NomeModulo.</summary>
        public static string NomePersistido(this ModuloPlanoInput input)
        {
            if (!string.IsNullOrWhiteSpace(input.Nome)) return input.Nome!.Trim();
            if (input.ModuloGeralId.HasValue && input.ModuloGeralId.Value != Guid.Empty) return input.ModuloGeralId.Value.ToString();
            return string.Empty;
        }
    }

    // ===== Commands =====

    /// <summary>Cria Plano (rico) com módulos.</summary>
    public class CriarPlanoRicoCommandHandler : ICommandHandler<CriarPlanoRicoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPlanoRicoCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPlanoRicoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var plano = new Plano(
                request.Nome,
                request.Valor,
                request.GrupoPlanoId,
                request.LimiteUsuarios,
                request.LimiteEmpresas,
                request.RecursosInclusos,
                tenantId,
                criadoPor
            );

            if (request.Modulos != null)
            {
                foreach (var m in request.Modulos)
                {
                    var nome = m.NomePersistido();
                    if (!string.IsNullOrWhiteSpace(nome))
                    {
                        plano.AdicionarModulo(nome, criadoPor);
                    }
                }
            }

            foreach (var modulo in plano.Modulos.Where(mo => !mo.IsValid))
            {
                plano.AddNotifications(modulo.Notifications);
            }

            if (!plano.IsValid)
            {
                return CommandResult.Falha(plano.Notifications.Select(n => n.Message), "Falha na validação do plano");
            }

            if (!request.Ativo)
            {
                plano.DefinirAtivo(false, criadoPor);
            }

            _context.Planos.Add(plano);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Plano cadastrado com sucesso!", new { PlanoId = plano.Id });
        }
    }

    /// <summary>Atualiza Plano (rico) e reconcilia módulos (add/update/remove).</summary>
    public class AtualizarPlanoCommandHandler : ICommandHandler<AtualizarPlanoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarPlanoCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarPlanoCommand request, CancellationToken cancellationToken)
        {
            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var plano = await _context.Planos
                .Include(p => p.Modulos)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (plano == null)
            {
                return CommandResult.Falha(new[] { "Plano não encontrado." }, "Erro");
            }

            plano.Atualizar(
                request.Nome,
                request.Valor,
                request.GrupoPlanoId,
                request.LimiteUsuarios,
                request.LimiteEmpresas,
                request.RecursosInclusos,
                alteradoPor
            );

            if (!plano.IsValid)
            {
                return CommandResult.Falha(plano.Notifications.Select(n => n.Message), "Falha na validação do plano");
            }

            plano.DefinirAtivo(request.Ativo, alteradoPor);

            // Reconciliação de módulos por NomeModulo (add/update/remove).
            var desejados = (request.Modulos ?? new List<ModuloPlanoInput>())
                .Select(m => m.NomePersistido())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Remove os que não estão mais na lista desejada.
            var aRemover = plano.Modulos
                .Where(mo => !desejados.Contains(mo.NomeModulo, StringComparer.OrdinalIgnoreCase))
                .ToList();
            foreach (var mo in aRemover)
            {
                plano.RemoverModulo(mo);
                _context.ModulosPlano.Remove(mo);
            }

            // Adiciona os novos.
            var existentes = plano.Modulos
                .Select(mo => mo.NomeModulo)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var nome in desejados.Where(n => !existentes.Contains(n)))
            {
                plano.AdicionarModulo(nome, alteradoPor);
            }

            foreach (var modulo in plano.Modulos.Where(mo => !mo.IsValid))
            {
                plano.AddNotifications(modulo.Notifications);
            }

            if (!plano.IsValid)
            {
                return CommandResult.Falha(plano.Notifications.Select(n => n.Message), "Falha na validação do plano");
            }

            _context.Planos.Update(plano);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Plano atualizado com sucesso!", new { PlanoId = plano.Id });
        }
    }

    /// <summary>Exclui (soft-delete) Plano.</summary>
    public class ExcluirPlanoCommandHandler : ICommandHandler<ExcluirPlanoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;

        public ExcluirPlanoCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ExcluirPlanoCommand request, CancellationToken cancellationToken)
        {
            var deletadoPor = _currentUser.GetUserId() ?? "system";

            var plano = await _context.Planos.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            if (plano == null)
            {
                return CommandResult.Falha(new[] { "Plano não encontrado." }, "Erro");
            }

            plano.Deletar(deletadoPor);
            _context.Planos.Update(plano);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Plano excluído com sucesso!");
        }
    }

    // ===== Queries =====

    /// <summary>Lista Planos (paginado).</summary>
    public class ListarPlanosQueryHandler : IQueryHandler<ListarPlanosQuery, PagedQueryResult<PlanoListaDto>>
    {
        private readonly ContextGestaoClientes _context;

        public ListarPlanosQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<PagedQueryResult<PlanoListaDto>> Handle(ListarPlanosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Planos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(p => p.Nome.Contains(request.Search));
            }

            var totalRegistros = await query.CountAsync(cancellationToken);
            var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)request.TamanhoPagina);

            var items = await query
                .OrderBy(p => p.Nome)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(p => new PlanoListaDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Valor = p.Preco,
                    GrupoPlanoId = p.GrupoPlanoId,
                    LimiteUsuarios = p.LimiteUsuarios,
                    LimiteEmpresas = p.LimiteEmpresas,
                    Ativo = p.Ativo,
                    QtdeModulos = p.Modulos.Count,
                    CriadoEm = p.CriadoEm
                })
                .ToListAsync(cancellationToken);

            return new PagedQueryResult<PlanoListaDto>(items, totalRegistros, totalPaginas);
        }
    }

    /// <summary>Obtém Plano por Id (com módulos).</summary>
    public class ObterPlanoPorIdQueryHandler : IQueryHandler<ObterPlanoPorIdQuery, PlanoDetalhadoDto>
    {
        private readonly ContextGestaoClientes _context;

        public ObterPlanoPorIdQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<PlanoDetalhadoDto> Handle(ObterPlanoPorIdQuery request, CancellationToken cancellationToken)
        {
            var p = await _context.Planos
                .Include(x => x.Modulos)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (p == null) return null!;

            return new PlanoDetalhadoDto
            {
                Id = p.Id,
                Nome = p.Nome,
                Valor = p.Preco,
                GrupoPlanoId = p.GrupoPlanoId,
                LimiteUsuarios = p.LimiteUsuarios,
                LimiteEmpresas = p.LimiteEmpresas,
                RecursosInclusos = p.RecursosInclusos,
                Ativo = p.Ativo,
                CriadoEm = p.CriadoEm,
                Modulos = p.Modulos.Select(m => new ModuloPlanoDto
                {
                    Id = m.Id,
                    NomeModulo = m.NomeModulo
                }).ToList()
            };
        }
    }
}
