using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Entities;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Domain.ValueObjects;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using EnderecoEntity = Epros.Modules.GestaoClientes.Domain.Entities.Endereco;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Atualiza Cliente Detalhado.</summary>
    public class AtualizarClienteDetalhadoCommandHandler : ICommandHandler<AtualizarClienteDetalhadoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarClienteDetalhadoCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarClienteDetalhadoCommand request, CancellationToken cancellationToken)
        {
            var alteradoPor = _currentUser.GetUserId() ?? "system";

            // Busca o cliente ignorando filtros RLS (pois o admin opera sob o tenant system)
            var cliente = await _context.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (cliente == null)
            {
                return CommandResult.Falha(new[] { "Cliente não encontrado." }, "Erro");
            }

            // Atualiza os dados básicos do cliente
            cliente.Alterar(
                request.RazaoSocial,
                request.Cnpj,
                request.Email,
                request.PlanoId,
                request.RevendaId,
                request.VendedorId,
                request.DiaVencimento,
                request.Ativo,
                request.Telefone,
                request.NomeContato,
                request.IsDemo,
                request.TokenAcesso,
                alteradoPor
            );

            // 1.01 — cota (snapshot) que sobrepõe os limites do plano
            cliente.AtualizarCota(request.CotaUsuarios, request.CotaEmpresas, request.CotaPermissoes, alteradoPor);

            if (!cliente.IsValid)
            {
                return CommandResult.Falha(cliente.Notifications.Select(n => n.Message), "Erro de validação nos dados do cliente");
            }

            _context.Clientes.Update(cliente);

            // =========================================================================
            // 1. Processar Composições de Faturamento
            // =========================================================================
            var composicoesExistentes = await _context.ComposicoesFaturamento
                .IgnoreQueryFilters()
                .Where(c => c.ClienteId == cliente.Id && c.DeletadoEm == null)
                .ToListAsync(cancellationToken);

            // Identifica composições para remover
            var idsNoRequest = request.Composicoes.Select(c => c.Id).ToList();
            var composicoesRemover = composicoesExistentes.Where(c => !idsNoRequest.Contains(c.Id)).ToList();
            foreach (var compRem in composicoesRemover)
            {
                _context.ComposicoesFaturamento.Remove(compRem);
            }

            // Adiciona ou atualiza composições
            foreach (var compReq in request.Composicoes)
            {
                if (compReq.Id == Guid.Empty)
                {
                    var novaComp = new ComposicaoFaturamento(
                        cliente.Id,
                        compReq.Descricao,
                        compReq.Valor,
                        compReq.DataInicial,
                        compReq.DataFinal,
                        compReq.PodeReajustar,
                        cliente.TenantId,
                        alteradoPor
                    );

                    if (!novaComp.IsValid)
                    {
                        return CommandResult.Falha(novaComp.Notifications.Select(n => n.Message), "Erro ao criar nova composição de faturamento");
                    }
                    _context.ComposicoesFaturamento.Add(novaComp);
                }
                else
                {
                    var compExistente = composicoesExistentes.FirstOrDefault(c => c.Id == compReq.Id);
                    if (compExistente != null)
                    {
                        compExistente.Alterar(
                            compReq.Descricao,
                            compReq.Valor,
                            compReq.DataInicial,
                            compReq.DataFinal,
                            compReq.PodeReajustar,
                            alteradoPor
                        );

                        if (!compExistente.IsValid)
                        {
                            return CommandResult.Falha(compExistente.Notifications.Select(n => n.Message), "Erro ao atualizar composição de faturamento");
                        }
                        _context.ComposicoesFaturamento.Update(compExistente);
                    }
                }
            }

            // =========================================================================
            // 2. Processar Endereços
            // =========================================================================
            if (request.Enderecos != null && request.Enderecos.Any())
            {
                // Garante que uma Pessoa correspondente ao Cliente exista no banco (devido à FK relacional do Endereço)
                var pessoaExistente = await _context.Pessoas
                    .IgnoreQueryFilters()
                    .Include(p => p.PessoaJuridica)
                    .Include(p => p.PessoaCliente)
                    .FirstOrDefaultAsync(p => p.Id == cliente.Id, cancellationToken);

                if (pessoaExistente == null)
                {
                    // Cria entidade Pessoa rica para o Cliente
                    var novaPessoa = new Pessoa(
                        ETipoPessoa.PessoaJuridica,
                        ETipoIndicadorIe.NaoContribuinte,
                        null, null, null, null, null, null, null, "Pessoa criada para suporte de faturamento de inquilino",
                        cliente.TenantId,
                        alteradoPor
                    );

                    // Força o mesmo ID do cliente
                    typeof(EntidadeSaaSBase).GetProperty("Id")?.SetValue(novaPessoa, cliente.Id);

                    var pessoaJuridica = new PessoaJuridica(
                        cliente.Id,
                        new Cnpj(cliente.Cnpj),
                        cliente.RazaoSocial,
                        null, null, null, null,
                        cliente.TenantId,
                        alteradoPor
                    );
                    novaPessoa.VincularJuridica(pessoaJuridica);

                    var pessoaCliente = new PessoaCliente(
                        cliente.Id,
                        false,
                        ETipoContribuinte.NaoInformado,
                        cliente.TenantId,
                        alteradoPor
                    );
                    novaPessoa.VincularCliente(pessoaCliente);

                    _context.Pessoas.Add(novaPessoa);
                }

                // Processa endereços reais
                var enderecosExistentes = await _context.EnderecosPessoas
                    .IgnoreQueryFilters()
                    .Where(e => e.PessoaId == cliente.Id && e.DeletadoEm == null)
                    .ToListAsync(cancellationToken);

                // Identifica endereços para remover
                var idsEndNoRequest = request.Enderecos.Select(e => e.Id).ToList();
                var enderecosRemover = enderecosExistentes.Where(e => !idsEndNoRequest.Contains(e.Id)).ToList();
                foreach (var endRem in enderecosRemover)
                {
                    _context.EnderecosPessoas.Remove(endRem);
                }

                // Adiciona ou atualiza endereços
                foreach (var endReq in request.Enderecos)
                {
                    Enum.TryParse(endReq.TipoEndereco.ToString(), out ETipoEndereco tipoEndereco);

                    if (endReq.Id == Guid.Empty)
                    {
                        var novoEnd = new EnderecoEntity(
                            cliente.Id,
                            tipoEndereco,
                            endReq.PaisId,
                            endReq.MunicipioId,
                            endReq.SubdivisaoId,
                            endReq.Uf,
                            endReq.Cep,
                            endReq.Logradouro,
                            endReq.Numero,
                            endReq.Complemento,
                            endReq.Bairro,
                            endReq.Referencia,
                            null, null, null, null, null,
                            cliente.TenantId,
                            alteradoPor
                        );

                        if (!novoEnd.IsValid)
                        {
                            return CommandResult.Falha(novoEnd.Notifications.Select(n => n.Message), "Erro ao criar novo endereço do cliente");
                        }
                        novoEnd.DefinirPrincipal(endReq.Principal, alteradoPor);
                        _context.EnderecosPessoas.Add(novoEnd);
                    }
                    else
                    {
                        var endExistente = enderecosExistentes.FirstOrDefault(e => e.Id == endReq.Id);
                        if (endExistente != null)
                        {
                            // Cria uma nova instância temporária para validação das invariantes ricas do domínio
                            var endValidador = new EnderecoEntity(
                                cliente.Id,
                                tipoEndereco,
                                endReq.PaisId,
                                endReq.MunicipioId,
                                endReq.SubdivisaoId,
                                endReq.Uf,
                                endReq.Cep,
                                endReq.Logradouro,
                                endReq.Numero,
                                endReq.Complemento,
                                endReq.Bairro,
                                endReq.Referencia,
                                null, null, null, null, null,
                                cliente.TenantId,
                                alteradoPor
                            );

                            if (!endValidador.IsValid)
                            {
                                return CommandResult.Falha(endValidador.Notifications.Select(n => n.Message), "Erro na validação do endereço atualizado");
                            }

                            // Utiliza reflection/propriedades privadas do domínio para atualizar a entidade rastreada
                            typeof(EnderecoEntity).GetProperty("TipoEndereco")?.SetValue(endExistente, tipoEndereco);
                            typeof(EnderecoEntity).GetProperty("PaisId")?.SetValue(endExistente, endReq.PaisId);
                            typeof(EnderecoEntity).GetProperty("MunicipioId")?.SetValue(endExistente, endReq.MunicipioId);
                            typeof(EnderecoEntity).GetProperty("SubdivisaoId")?.SetValue(endExistente, endReq.SubdivisaoId);
                            typeof(EnderecoEntity).GetProperty("Uf")?.SetValue(endExistente, endReq.Uf);
                            typeof(EnderecoEntity).GetProperty("Cep")?.SetValue(endExistente, endReq.Cep);
                            typeof(EnderecoEntity).GetProperty("Logradouro")?.SetValue(endExistente, endReq.Logradouro);
                            typeof(EnderecoEntity).GetProperty("Numero")?.SetValue(endExistente, endReq.Numero);
                            typeof(EnderecoEntity).GetProperty("Complemento")?.SetValue(endExistente, endReq.Complemento);
                            typeof(EnderecoEntity).GetProperty("Bairro")?.SetValue(endExistente, endReq.Bairro);
                            typeof(EnderecoEntity).GetProperty("Referencia")?.SetValue(endExistente, endReq.Referencia);
                            endExistente.DefinirPrincipal(endReq.Principal, alteradoPor);
                            endExistente.MarcarAlterado(alteradoPor);

                            _context.EnderecosPessoas.Update(endExistente);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Cadastro de cliente atualizado com sucesso!", new { ClienteId = cliente.Id });
        }
    }
}
