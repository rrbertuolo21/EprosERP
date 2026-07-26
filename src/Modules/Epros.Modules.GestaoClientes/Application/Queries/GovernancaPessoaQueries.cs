using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarCandidatosDuplicataQuery(int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public record ListarConsentimentosPorPessoaQuery(Guid PessoaId) : IQuery<CommandResult>;

    /// <summary>REG-PEM-153: exporta todos os dados pessoais agregados por PessoaId.</summary>
    public record ExportarDadosPessoaisQuery(Guid PessoaId) : IQuery<CommandResult>;

    public class ListarCandidatosDuplicataQueryHandler : IQueryHandler<ListarCandidatosDuplicataQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarCandidatosDuplicataQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarCandidatosDuplicataQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var pagina = request.Pagina < 1 ? 1 : request.Pagina;
            var tamanho = request.TamanhoPagina < 1 ? 20 : request.TamanhoPagina;

            var query = _context.CandidatosDuplicata.Where(c => c.TenantId == tenantId);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(c => c.Score)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .Select(c => new { c.Id, c.PessoaAId, c.PessoaBId, c.Score, Status = c.Status.ToString() })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Candidatos a duplicata listados com sucesso.", new { Total = total, Pagina = pagina, Itens = itens });
        }
    }

    public class ListarConsentimentosPorPessoaQueryHandler : IQueryHandler<ListarConsentimentosPorPessoaQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarConsentimentosPorPessoaQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarConsentimentosPorPessoaQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var itens = await _context.ConsentimentosTitular
                .Where(c => c.TenantId == tenantId && c.PessoaId == request.PessoaId)
                .OrderByDescending(c => c.DataConsentimento)
                .Select(c => new
                {
                    c.Id,
                    Finalidade = c.Finalidade.ToString(),
                    BaseLegal = c.BaseLegal.ToString(),
                    c.DataConsentimento,
                    c.DataRevogacao,
                    c.Canal
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Consentimentos listados com sucesso.", itens);
        }
    }

    public class ExportarDadosPessoaisQueryHandler : IQueryHandler<ExportarDadosPessoaisQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ExportarDadosPessoaisQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ExportarDadosPessoaisQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            var pessoa = await _context.Pessoas
                .Include(p => p.PessoaFisica)
                .Include(p => p.PessoaJuridica)
                .Include(p => p.PessoaEstrangeiro)
                .Include(p => p.Enderecos)
                .Include(p => p.Contatos)
                .FirstOrDefaultAsync(p => p.Id == request.PessoaId && p.TenantId == tenantId, cancellationToken);

            if (pessoa == null)
                return CommandResult.Falha(new[] { "Pessoa não encontrada para o inquilino atual." });

            var consentimentos = await _context.ConsentimentosTitular.Where(c => c.PessoaId == request.PessoaId && c.TenantId == tenantId).ToListAsync(cancellationToken);
            var solicitacoes = await _context.SolicitacoesTitular.Where(s => s.PessoaId == request.PessoaId && s.TenantId == tenantId).ToListAsync(cancellationToken);
            var identificadores = await _context.IdentificadoresFiscais.Where(i => i.PessoaId == request.PessoaId && i.TenantId == tenantId).ToListAsync(cancellationToken);

            var dados = new
            {
                Pessoa = new { pessoa.Id, TipoPessoa = pessoa.TipoPessoa.ToString(), Status = pessoa.Status.ToString() },
                Fisica = pessoa.PessoaFisica == null ? null : new { pessoa.PessoaFisica.Nome, pessoa.PessoaFisica.Sobrenome },
                Juridica = pessoa.PessoaJuridica == null ? null : new { pessoa.PessoaJuridica.RazaoSocial },
                Estrangeiro = pessoa.PessoaEstrangeiro == null ? null : new { pessoa.PessoaEstrangeiro.Nome },
                Enderecos = pessoa.Enderecos.Count,
                Contatos = pessoa.Contatos.Count,
                Consentimentos = consentimentos.Select(c => new { c.Id, Finalidade = c.Finalidade.ToString() }),
                Solicitacoes = solicitacoes.Select(s => new { s.Id, Tipo = s.Tipo.ToString(), Status = s.Status.ToString() }),
                IdentificadoresFiscais = identificadores.Select(i => new { i.Id, Tipo = i.Tipo.ToString(), i.Valor })
            };

            return CommandResult.Ok("Dados pessoais agregados com sucesso.", dados);
        }
    }
}
