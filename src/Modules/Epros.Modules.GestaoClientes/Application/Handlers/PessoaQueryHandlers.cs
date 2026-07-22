using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Lista Pessoas.</summary>
    public class ListarPessoasQueryHandler : IQueryHandler<ListarPessoasQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarPessoasQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarPessoasQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var query = _context.Pessoas
                .Include(p => p.PessoaFisica)
                .Include(p => p.PessoaJuridica)
                .Include(p => p.PessoaEstrangeiro)
                .Where(p => p.TenantId == tenantId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
            {
                var localizar = request.Localizar;
                query = query.Where(p =>
                    (p.PessoaFisica != null && p.PessoaFisica.Nome.Contains(localizar)) ||
                    (p.PessoaJuridica != null && p.PessoaJuridica.RazaoSocial.Contains(localizar)) ||
                    (p.PessoaEstrangeiro != null && p.PessoaEstrangeiro.Nome.Contains(localizar))
                );
            }

            if (request.EhCliente.HasValue) query = query.Where(p => p.EhCliente == request.EhCliente.Value);
            if (request.EhFornecedor.HasValue) query = query.Where(p => p.EhFornecedor == request.EhFornecedor.Value);
            if (request.EhTransportadora.HasValue) query = query.Where(p => p.EhTransportadora == request.EhTransportadora.Value);
            if (request.EhMotorista.HasValue) query = query.Where(p => p.EhMotorista == request.EhMotorista.Value);
            if (request.EhPrestadorServico.HasValue) query = query.Where(p => p.EhPrestadorServico == request.EhPrestadorServico.Value);
            if (request.EhFuncionario.HasValue) query = query.Where(p => p.EhFuncionario == request.EhFuncionario.Value);
            if (request.EhProdutorRural.HasValue) query = query.Where(p => p.EhProdutorRural == request.EhProdutorRural.Value);

            if (request.Status.HasValue)
            {
                if (request.Status.Value == EStatusPessoa.Ativo)
                    query = query.Where(p => p.Status == EEstadoPessoa.Ativo);
                else if (request.Status.Value == EStatusPessoa.Inativo)
                    query = query.Where(p => p.Status == EEstadoPessoa.Inativo);
            }

            var pagina = request.Pagina < 1 ? 1 : request.Pagina;
            var tamanho = request.TamanhoPagina < 1 ? 10 : request.TamanhoPagina;

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Pessoas listadas com sucesso.", new { Total = total, Pagina = pagina, Itens = itens });
        }
    }

    /// <summary>Obtém Pessoa Por Id.</summary>
    public class ObterPessoaPorIdQueryHandler : IQueryHandler<ObterPessoaPorIdQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterPessoaPorIdQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterPessoaPorIdQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var pessoa = await _context.Pessoas
                .Include(p => p.PessoaFisica)
                .Include(p => p.PessoaJuridica)
                .Include(p => p.PessoaEstrangeiro)
                .Include(p => p.PessoaCliente)
                .Include(p => p.PessoaFuncionario)
                .Include(p => p.PessoaMotorista)
                .Include(p => p.PessoaTransportadora)
                .Include(p => p.PessoaPrestadorServico)
                .Include(p => p.Enderecos)
                .Include(p => p.Contatos)
                .Include(p => p.Veiculos)
                .FirstOrDefaultAsync(p => p.Id == request.Id && p.TenantId == tenantId, cancellationToken);

            if (pessoa == null)
                return CommandResult.Falha(new[] { "Pessoa não encontrada" });

            return CommandResult.Ok("Pessoa obtida com sucesso.", pessoa);
        }
    }

    /// <summary>Handler de aplicação (LocalizarClienteQueryHandler).</summary>
    public class LocalizarClienteQueryHandler : IQueryHandler<LocalizarClienteQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public LocalizarClienteQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(LocalizarClienteQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var termo = request.Termo ?? string.Empty;

            var clientes = await _context.Pessoas
                .Include(p => p.PessoaFisica)
                .Include(p => p.PessoaJuridica)
                .Include(p => p.PessoaEstrangeiro)
                .Where(p => p.EhCliente && p.TenantId == tenantId &&
                    ((p.PessoaFisica != null && p.PessoaFisica.Nome.Contains(termo)) ||
                     (p.PessoaJuridica != null && p.PessoaJuridica.RazaoSocial.Contains(termo)) ||
                     (p.PessoaEstrangeiro != null && p.PessoaEstrangeiro.Nome.Contains(termo))))
                .Take(10)
                .Select(p => new
                {
                    p.Id,
                    Nome = p.TipoPessoa == ETipoPessoa.PessoaFisica ? p.PessoaFisica!.Nome :
                           p.TipoPessoa == ETipoPessoa.PessoaJuridica ? p.PessoaJuridica!.RazaoSocial :
                           p.PessoaEstrangeiro!.Nome,
                    Documento = p.TipoPessoa == ETipoPessoa.PessoaFisica ? p.PessoaFisica!.Cpf.Valor :
                                p.TipoPessoa == ETipoPessoa.PessoaJuridica ? p.PessoaJuridica!.Cnpj.Valor :
                                p.PessoaEstrangeiro!.IdentificacaoEstrangeiro
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Clientes localizados com sucesso.", clientes);
        }
    }

    /// <summary>Exclui Pessoa.</summary>
    public class ExcluirPessoaCommandHandler : ICommandHandler<Commands.ExcluirPessoaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ExcluirPessoaCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(Commands.ExcluirPessoaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var pessoa = await _context.Pessoas.FirstOrDefaultAsync(p => p.Id == request.Id && p.TenantId == tenantId, cancellationToken);
            if (pessoa == null)
                return CommandResult.Falha(new[] { "Pessoa não encontrada" });

            // REG-PEM-128: Cliente padrão de PDV não pode ser excluído
            var configClientePadrao = await _context.ConfiguracoesGlobais
                .FirstOrDefaultAsync(c => c.Chave == "pdv.cliente_padrao_id" && c.TenantId == tenantId, cancellationToken);
            if (configClientePadrao != null && Guid.TryParse(configClientePadrao.Valor, out var clientePadraoId) && clientePadraoId == request.Id)
                return CommandResult.Falha(new[] { "O Cliente padrão de PDV não pode ser excluído." });

            // REG-PEM-126/129: Exclusão de pessoa deve verificar contratos e financeiro
            var temContrato = await _context.Contratos.AnyAsync(c => c.ClienteId == request.Id && c.TenantId == tenantId, cancellationToken);
            if (temContrato)
                return CommandResult.Falha(new[] { "Não é possível excluir uma pessoa com contratos vinculados. Ela deve ser inativada." });

            var temContasReceber = await _context.ContasAReceberLookup.AnyAsync(cr => cr.PessoaId == request.Id && cr.TenantId == tenantId, cancellationToken);
            var temContasPagar = await _context.ContasAPagarLookup.AnyAsync(cp => cp.PessoaId == request.Id && cp.TenantId == tenantId, cancellationToken);
            if (temContasReceber || temContasPagar)
                return CommandResult.Falha(new[] { "Não é possível excluir uma pessoa com títulos financeiros (contas a pagar/receber) vinculados. Ela deve ser inativada." });

            pessoa.Deletar(userId);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Pessoa removida com sucesso.");
        }
    }
}
