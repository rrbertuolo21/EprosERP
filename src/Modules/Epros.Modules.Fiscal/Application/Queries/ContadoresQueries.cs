using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Infrastructure.Data;

namespace Epros.Modules.Fiscal.Application.Queries
{
    public record ListarContadoresQuery(
        string? Localizar = null,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IQuery<CommandResult>;

    public record ObterContadorPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarContadoresQueryHandler : IRequestHandler<ListarContadoresQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ListarContadoresQueryHandler(ContextFiscal context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ListarContadoresQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Contadores
                .AsNoTracking()
                .Where(c => c.DeletadoEm == null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
            {
                query = query.Where(c => 
                    (c.RazaoSocial != null && c.RazaoSocial.Contains(request.Localizar)) || 
                    (c.NomeContador != null && c.NomeContador.Contains(request.Localizar)) || 
                    (c.NumeroCrc != null && c.NumeroCrc.Contains(request.Localizar)));
            }

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(c => c.NomeContador ?? c.RazaoSocial)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(c => new
                {
                    c.Id,
                    c.RazaoSocial,
                    c.NomeContador,
                    c.Cpf,
                    c.Cnpj,
                    c.NumeroCrc,
                    c.UfCrc,
                    c.DataVencimentoCrc,
                    c.Qualificacao,
                    c.Funcao,
                    c.Telefone,
                    c.Email,
                    c.PermissaoTransmissao,
                    c.Ativo,
                    c.Logradouro,
                    c.Numero,
                    c.Complemento,
                    c.Bairro,
                    c.Cep,
                    c.MunicipioId,
                    c.Uf
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterContadorPorIdQueryHandler : IRequestHandler<ObterContadorPorIdQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ObterContadorPorIdQueryHandler(ContextFiscal context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterContadorPorIdQuery request, CancellationToken cancellationToken)
        {
            var c = await _context.Contadores
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);

            if (c == null)
            {
                return CommandResult.Falha("Contador não encontrado.");
            }

            var dto = new
            {
                c.Id,
                c.RazaoSocial,
                c.NomeContador,
                c.Cpf,
                c.Cnpj,
                c.NumeroCrc,
                c.UfCrc,
                c.DataVencimentoCrc,
                c.Qualificacao,
                c.Funcao,
                c.Telefone,
                c.Email,
                c.PermissaoTransmissao,
                c.Ativo,
                c.Logradouro,
                c.Numero,
                c.Complemento,
                c.Bairro,
                c.Cep,
                c.MunicipioId,
                c.Uf
            };

            return CommandResult.Ok("OK", dto);
        }
    }
}
