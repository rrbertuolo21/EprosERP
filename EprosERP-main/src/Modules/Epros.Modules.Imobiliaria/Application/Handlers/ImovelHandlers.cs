using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Imobiliaria.Application.Commands;
using Epros.Modules.Imobiliaria.Application.Queries;
using Epros.Modules.Imobiliaria.Domain.Entities;
using Epros.Modules.Imobiliaria.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Imobiliaria.Application.Handlers
{
    public class CriarImovelCommandHandler : ICommandHandler<CriarImovelCommand>
    {
        private readonly ContextImobiliaria _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarImovelCommandHandler(ContextImobiliaria context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarImovelCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var imovel = new Imovel(
                request.Descricao,
                request.MunicipioId,
                request.Cep,
                request.Logradouro,
                request.Numero,
                request.Complemento,
                request.Bairro,
                tenantId,
                usuario);

            foreach (var p in request.Proprietarios ?? Enumerable.Empty<ProprietarioInput>())
                imovel.AdicionarProprietario(new ImovelProprietario(p.PessoaId, tenantId, usuario));

            foreach (var v in request.Vistorias ?? Enumerable.Empty<VistoriaInput>())
                imovel.AdicionarVistoria(new ImovelVistoria(v.Local, v.Descricao, v.DataVistoria, tenantId, usuario));

            foreach (var c in request.Custos ?? Enumerable.Empty<CustoImovelInput>())
                imovel.AdicionarCusto(new ImovelCusto(c.Descricao, c.Valor, c.Competencia, tenantId, usuario));

            // RN-002: validar o conjunto antes de persistir; agregacao atomica via um unico SaveChanges.
            imovel.Validar();
            if (!imovel.IsValid)
                return CommandResult.Falha(imovel.Notifications.Select(n => n.Message));

            _context.Imoveis.Add(imovel);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Imovel cadastrado com sucesso!", new { ImovelId = imovel.Id });
        }
    }

    public class ExcluirImovelCommandHandler : ICommandHandler<ExcluirImovelCommand>
    {
        private readonly ContextImobiliaria _context;

        public ExcluirImovelCommandHandler(ContextImobiliaria context) => _context = context;

        public async Task<CommandResult> Handle(ExcluirImovelCommand request, CancellationToken cancellationToken)
        {
            var imovel = await _context.Imoveis.FirstOrDefaultAsync(i => i.Id == request.ImovelId, cancellationToken);
            if (imovel is null)
                return CommandResult.Falha("Imovel nao encontrado."); // RN-005

            // RN-016/RN-013: nao excluir imovel com locacao vigente.
            var possuiLocacaoVigente = await _context.Locacoes
                .AnyAsync(l => l.ImovelId == request.ImovelId
                    && l.Status == Domain.Enums.EStatusLocacao.Vigente, cancellationToken);
            if (possuiLocacaoVigente)
                return CommandResult.Falha("Imovel possui locacao vigente e nao pode ser excluido."); // RN-007

            // Soft delete cascateia pelos agregados carregados.
            _context.Imoveis.Remove(imovel);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Imovel excluido com sucesso!");
        }
    }

    public class ListarImoveisQueryHandler : IQueryHandler<ListarImoveisQuery, CommandResult>
    {
        private readonly ContextImobiliaria _context;

        public ListarImoveisQueryHandler(ContextImobiliaria context) => _context = context;

        public async Task<CommandResult> Handle(ListarImoveisQuery request, CancellationToken cancellationToken)
        {
            var imoveis = await _context.Imoveis
                .AsNoTracking()
                .OrderByDescending(i => i.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Imoveis listados com sucesso!", imoveis);
        }
    }

    public class ObterImovelQueryHandler : IQueryHandler<ObterImovelQuery, CommandResult>
    {
        private readonly ContextImobiliaria _context;

        public ObterImovelQueryHandler(ContextImobiliaria context) => _context = context;

        public async Task<CommandResult> Handle(ObterImovelQuery request, CancellationToken cancellationToken)
        {
            // RN-004: recupera o imovel e seus agregados.
            var imovel = await _context.Imoveis
                .AsNoTracking()
                .Include(i => i.Proprietarios)
                .Include(i => i.Imagens)
                .Include(i => i.Custos)
                .Include(i => i.Vistorias)
                .FirstOrDefaultAsync(i => i.Id == request.ImovelId, cancellationToken);

            if (imovel is null)
                return CommandResult.Falha("Imovel nao encontrado.");

            return CommandResult.Ok("Imovel recuperado com sucesso!", imovel);
        }
    }
}
