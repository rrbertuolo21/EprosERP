using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    /// <summary>
    /// Dados auxiliares de emitente/destinatário/fornecedor/transportadora para as telas de
    /// Venda (<c>venda-dados</c>) e Compra (<c>compras-dados</c>). Read-model do módulo dono
    /// dos cadastros (Empresa/Pessoa). Emitente = Empresa; destinatário/fornecedor/transportadora = Pessoa.
    /// </summary>

    // ----- EMITENTE (Empresa) -----
    public record ObterDadosEmitentePorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ObterDadosEmitentePorIdQueryHandler : IRequestHandler<ObterDadosEmitentePorIdQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;

        public ObterDadosEmitentePorIdQueryHandler(ContextGestaoClientes context) => _context = context;

        public async Task<CommandResult> Handle(ObterDadosEmitentePorIdQuery request, CancellationToken cancellationToken)
        {
            var emitente = await _context.Empresas
                .AsNoTracking()
                .Where(e => e.DeletadoEm == null && e.Id == request.Id)
                .Select(e => new
                {
                    e.Id,
                    Documento = string.IsNullOrEmpty(e.Cnpj) ? e.Cpf : e.Cnpj,
                    e.RazaoSocial,
                    e.NomeFantasia,
                    e.InscricaoEstadual,
                    e.EhIndustria,
                    e.EhMei,
                    e.RegimeTributario,
                    e.NcmTributacaoId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (emitente == null) return CommandResult.Falha("Emitente (empresa) não encontrado.");

            var contatos = await _context.EmpresasContatos
                .AsNoTracking()
                .Where(c => c.DeletadoEm == null && c.EmpresaId == request.Id)
                .Select(c => new { c.Id, c.Nome, c.Email, c.Telefone })
                .ToListAsync(cancellationToken);

            var endereco = await _context.EnderecosPessoas
                .AsNoTracking()
                .Where(en => en.DeletadoEm == null && en.EmpresaId == request.Id)
                .Select(en => new
                {
                    en.Id,
                    TipoEndereco = (int)en.TipoEndereco,
                    en.Cep,
                    en.Uf,
                    en.Logradouro,
                    en.Complemento,
                    en.Numero,
                    en.Bairro,
                    MunicipioNome = en.Municipio.Nome
                })
                .FirstOrDefaultAsync(cancellationToken);

            return CommandResult.Ok("OK", new
            {
                emitente.Id,
                emitente.Documento,
                emitente.RazaoSocial,
                emitente.NomeFantasia,
                emitente.InscricaoEstadual,
                emitente.EhIndustria,
                emitente.EhMei,
                RegimeTributario = (int)emitente.RegimeTributario,
                emitente.NcmTributacaoId,
                Contatos = contatos,
                Endereco = endereco
            });
        }
    }

    // ----- DESTINATÁRIO / FORNECEDOR / TRANSPORTADORA (Pessoa) -----
    public record ObterDadosPessoaPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ObterDadosPessoaPorIdQueryHandler : IRequestHandler<ObterDadosPessoaPorIdQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;

        public ObterDadosPessoaPorIdQueryHandler(ContextGestaoClientes context) => _context = context;

        public async Task<CommandResult> Handle(ObterDadosPessoaPorIdQuery request, CancellationToken cancellationToken)
        {
            var pessoa = await _context.Pessoas
                .AsNoTracking()
                .Include(p => p.PessoaFisica)
                .Include(p => p.PessoaJuridica)
                .Include(p => p.PessoaEstrangeiro)
                .Include(p => p.PessoaTransportadora)
                .Include(p => p.Contatos)
                .Include(p => p.Veiculos)
                .Include(p => p.Enderecos).ThenInclude(en => en.Municipio)
                .Where(p => p.DeletadoEm == null && p.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (pessoa == null) return CommandResult.Falha("Pessoa não encontrada.");

            string nome = pessoa.PessoaJuridica?.RazaoSocial
                          ?? pessoa.PessoaFisica?.Nome
                          ?? pessoa.PessoaEstrangeiro?.Nome
                          ?? string.Empty;

            string documento = pessoa.PessoaJuridica?.Cnpj?.Valor
                               ?? pessoa.PessoaFisica?.Cpf?.Valor
                               ?? string.Empty;

            var dados = new
            {
                pessoa.Id,
                Nome = nome,
                Documento = documento,
                IdentificacaoEstrangeiro = pessoa.PessoaEstrangeiro?.IdentificacaoEstrangeiro,
                TipoPessoa = (int)pessoa.TipoPessoa,
                IndicadorIE = (int)pessoa.TipoIndicadorIe,
                InscricaoEstadual = pessoa.PessoaJuridica?.InscricaoEstadual,
                pessoa.EhCliente,
                pessoa.EhFornecedor,
                pessoa.EhTransportadora,
                Rntrc = pessoa.PessoaTransportadora?.Rntrc,
                Contatos = pessoa.Contatos
                    .Where(c => c.DeletadoEm == null)
                    .Select(c => new { c.Id, c.Nome, c.Email, Telefone = c.NumeroTelefone })
                    .ToList(),
                Veiculos = pessoa.Veiculos
                    .Where(v => v.DeletadoEm == null)
                    .Select(v => new { v.Id, v.Placa, TipoVeiculo = (int)v.TipoVeiculo, v.Rntrc })
                    .ToList(),
                Enderecos = pessoa.Enderecos
                    .Where(en => en.DeletadoEm == null)
                    .Select(en => new
                    {
                        en.Id,
                        TipoEndereco = (int)en.TipoEndereco,
                        en.Cep,
                        en.Uf,
                        en.Logradouro,
                        en.Complemento,
                        en.Numero,
                        en.Bairro,
                        MunicipioNome = en.Municipio.Nome
                    })
                    .ToList()
            };

            return CommandResult.Ok("OK", dados);
        }
    }
}
