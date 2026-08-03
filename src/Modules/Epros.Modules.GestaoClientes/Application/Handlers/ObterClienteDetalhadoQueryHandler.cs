using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Obtém Cliente Detalhado.</summary>
    public class ObterClienteDetalhadoQueryHandler : IQueryHandler<ObterClienteDetalhadoQuery, ClienteDetalhadoDto>
    {
        private readonly ContextGestaoClientes _context;

        public ObterClienteDetalhadoQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<ClienteDetalhadoDto> Handle(ObterClienteDetalhadoQuery request, CancellationToken cancellationToken)
        {
            // Busca o cliente ignorando RLS porque esta consulta roda sob o contexto do landlord
            var cliente = await _context.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (cliente == null) return null!;

            // Busca os nomes associados
            var plano = await _context.Planos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == cliente.PlanoId, cancellationToken);

            var revenda = cliente.RevendaId.HasValue
                ? await _context.Revendas.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == cliente.RevendaId.Value, cancellationToken)
                : null;

            var vendedor = cliente.VendedorId.HasValue
                ? await _context.Vendedores.IgnoreQueryFilters().FirstOrDefaultAsync(v => v.Id == cliente.VendedorId.Value, cancellationToken)
                : null;

            // Busca composições de faturamento
            var composicoes = await _context.ComposicoesFaturamento
                .IgnoreQueryFilters()
                .Where(c => c.ClienteId == cliente.Id && c.DeletadoEm == null)
                .Select(c => new ClienteComposicaoDto
                {
                    Id = c.Id,
                    Descricao = c.Descricao,
                    Valor = c.Valor,
                    DataInicial = c.DataInicial,
                    DataFinal = c.DataFinal,
                    PodeReajustar = c.PodeReajustar
                })
                .ToListAsync(cancellationToken);

            // Busca endereços vinculados
            var enderecos = await _context.EnderecosPessoas
                .IgnoreQueryFilters()
                .Where(e => e.PessoaId == cliente.Id && e.DeletadoEm == null)
                .Select(e => new ClienteEnderecoDto
                {
                    Id = e.Id,
                    TipoEndereco = (int)e.TipoEndereco,
                    PaisId = e.PaisId,
                    MunicipioId = e.MunicipioId,
                    MunicipioNome = e.Municipio.Nome,
                    SubdivisaoId = e.SubdivisaoId,
                    Uf = e.Uf,
                    Cep = e.Cep,
                    Logradouro = e.Logradouro,
                    Numero = e.Numero,
                    Complemento = e.Complemento,
                    Bairro = e.Bairro,
                    Referencia = e.Referencia,
                    Principal = e.Principal
                })
                .ToListAsync(cancellationToken);

            return new ClienteDetalhadoDto
            {
                Id = cliente.Id,
                RazaoSocial = cliente.RazaoSocial,
                Cnpj = cliente.Cnpj,
                Email = cliente.Email,
                PlanoId = cliente.PlanoId,
                PlanoNome = plano?.Nome ?? "Plano Desconhecido",
                RevendaId = cliente.RevendaId,
                RevendaNome = revenda?.Nome,
                VendedorId = cliente.VendedorId,
                VendedorNome = vendedor?.Nome,
                DiaVencimento = cliente.DiaVencimento,
                StatusSaaS = cliente.StatusSaaS.ToString(),
                CotaUsuarios = cliente.CotaUsuarios,
                CotaEmpresas = cliente.CotaEmpresas,
                CotaPermissoes = cliente.CotaPermissoes,
                Ativo = cliente.Ativo,
                Telefone = cliente.Telefone,
                NomeContato = cliente.NomeContato,
                IsDemo = cliente.IsDemo,
                TokenAcesso = cliente.TokenAcesso,
                Enderecos = enderecos,
                Composicoes = composicoes
            };
        }
    }
}
