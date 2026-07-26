using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Domain.ValueObjects;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Cria Pessoa.</summary>
    public class CriarPessoaCommandHandler : ICommandHandler<CriarPessoaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPessoaCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPessoaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            // Obter documento do consumidor final padrão para permitir duplicidade (REG-PEM-006)
            string? documentoConsumidorPadrao = null;
            var configClientePadrao = await _context.ConfiguracoesGlobais
                .FirstOrDefaultAsync(c => c.Chave == "pdv.cliente_padrao_id" && c.TenantId == tenantId, cancellationToken);
            if (configClientePadrao != null && Guid.TryParse(configClientePadrao.Valor, out var clientePadraoId))
            {
                var pfPadrao = await _context.PessoasFisicas.FirstOrDefaultAsync(pf => pf.PessoaId == clientePadraoId && pf.TenantId == tenantId, cancellationToken);
                if (pfPadrao != null)
                {
                    documentoConsumidorPadrao = pfPadrao.Cpf.Valor;
                }
                else
                {
                    var pjPadrao = await _context.PessoasJuridicas.FirstOrDefaultAsync(pj => pj.PessoaId == clientePadraoId && pj.TenantId == tenantId, cancellationToken);
                    if (pjPadrao != null)
                    {
                        documentoConsumidorPadrao = pjPadrao.Cnpj.Valor;
                    }
                }
            }

            // Uniqueness validation
            if (request.TipoPessoa == ETipoPessoa.PessoaFisica)
            {
                if (string.IsNullOrWhiteSpace(request.FisicaCpf))
                {
                    return CommandResult.Falha(new[] { "CPF inválido" }, "Erro de validação");
                }
                var tempCpf = new Cpf(request.FisicaCpf);
                if (!tempCpf.IsValid)
                {
                    return CommandResult.Falha(new[] { "CPF inválido" }, "Erro de validação");
                }
                if (documentoConsumidorPadrao == null || tempCpf.Valor != documentoConsumidorPadrao)
                {
                    var existeCpf = await _context.PessoasFisicas.AnyAsync(pf => pf.Cpf.Valor == tempCpf.Valor && pf.TenantId == tenantId, cancellationToken);
                    if (existeCpf)
                    {
                        return CommandResult.Falha(new[] { "CPF já cadastrado" }, "Erro de validação");
                    }
                }
            }
            else if (request.TipoPessoa == ETipoPessoa.PessoaJuridica)
            {
                if (string.IsNullOrWhiteSpace(request.JuridicaCnpj))
                {
                    return CommandResult.Falha(new[] { "CNPJ inválido" }, "Erro de validação");
                }
                var tempCnpj = new Cnpj(request.JuridicaCnpj);
                if (!tempCnpj.IsValid)
                {
                    return CommandResult.Falha(new[] { "CNPJ inválido" }, "Erro de validação");
                }
                if (documentoConsumidorPadrao == null || tempCnpj.Valor != documentoConsumidorPadrao)
                {
                    var existeCnpj = await _context.PessoasJuridicas.AnyAsync(pj => pj.Cnpj.Valor == tempCnpj.Valor && pj.TenantId == tenantId, cancellationToken);
                    if (existeCnpj)
                    {
                        return CommandResult.Falha(new[] { "CNPJ já cadastrado" }, "Erro de validação");
                    }
                }
            }
            else if (request.TipoPessoa == ETipoPessoa.PessoaEstrangeira)
            {
                if (string.IsNullOrWhiteSpace(request.EstrangeiroNome))
                {
                    return CommandResult.Falha(new[] { "O Nome do Estrangeiro já cadastrado" }, "Erro de validação");
                }
                var existeNome = await _context.PessoasEstrangeiros.AnyAsync(pe => pe.Nome == request.EstrangeiroNome && pe.TenantId == tenantId, cancellationToken);
                if (existeNome)
                {
                    return CommandResult.Falha(new[] { "O Nome do Estrangeiro já cadastrado" }, "Erro de validação");
                }
            }

            // Validar Grupo se fornecido
            if (request.PessoaGrupoId.HasValue && request.PessoaGrupoId.Value != Guid.Empty)
            {
                var existeGrupo = await _context.PessoaGrupos.AnyAsync(g => g.Id == request.PessoaGrupoId.Value && g.TenantId == tenantId, cancellationToken);
                if (!existeGrupo)
                {
                    return CommandResult.Falha(new[] { "Grupo Pessoa informado não cadastrado" }, "Erro de validação");
                }
            }

            // Instancia a entidade raiz Pessoa
            var pessoa = new Pessoa(
                request.TipoPessoa,
                request.TipoIndicadorIe,
                request.PessoaGrupoId,
                request.InscricaoSuframa,
                request.TitularContaBancaria,
                request.AgenciaContaBancaria,
                request.NumeroContaBancaria,
                request.TipoPix,
                request.ChavePix,
                request.Observacoes,
                tenantId,
                criadoPor
            );

            // Vincular Subtipo
            if (request.TipoPessoa == ETipoPessoa.PessoaFisica)
            {
                var cpf = new Cpf(request.FisicaCpf ?? "");
                var fisica = new PessoaFisica(
                    pessoa.Id,
                    cpf,
                    request.FisicaNome ?? "",
                    request.FisicaSobrenome,
                    request.RgNumero,
                    request.RgOrgaoEmissor,
                    request.TipoGenero ?? ETipoGenero.NaoUtiliza,
                    request.DataNascimento,
                    tenantId,
                    criadoPor
                );
                pessoa.VincularFisica(fisica);
            }
            else if (request.TipoPessoa == ETipoPessoa.PessoaJuridica)
            {
                var cnpj = new Cnpj(request.JuridicaCnpj ?? "");
                var juridica = new PessoaJuridica(
                    pessoa.Id,
                    cnpj,
                    request.RazaoSocial ?? "",
                    request.NomeFantasia,
                    request.InscricaoEstadual,
                    request.InscricaoMunicipal,
                    request.Cnae,
                    tenantId,
                    criadoPor
                );
                pessoa.VincularJuridica(juridica);
            }
            else if (request.TipoPessoa == ETipoPessoa.PessoaEstrangeira)
            {
                var estrangeiro = new PessoaEstrangeiro(
                    pessoa.Id,
                    request.EstrangeiroNome ?? "",
                    request.IdentificacaoEstrangeiro ?? "",
                    tenantId,
                    criadoPor
                );
                pessoa.VincularEstrangeiro(estrangeiro);
            }

            // Vincular Papéis
            if (request.EhCliente)
            {
                var cliente = new PessoaCliente(
                    pessoa.Id,
                    request.ClienteEhConsumidorFinal ?? false,
                    request.ClienteTipoContribuinte ?? ETipoContribuinte.NaoInformado,
                    tenantId,
                    criadoPor
                );
                pessoa.VincularCliente(cliente);
            }
            if (request.EhFuncionario)
            {
                var funcionario = new PessoaFuncionario(
                    pessoa.Id,
                    request.FuncionarioTipoCargo ?? ETipoCargo.Operador,
                    request.FuncionarioComissao ?? 0,
                    tenantId,
                    criadoPor
                );
                pessoa.VincularFuncionario(funcionario);
            }
            if (request.EhMotorista)
            {
                var motorista = new PessoaMotorista(
                    pessoa.Id,
                    request.MotoristaTipoVinculo ?? ETipoVinculoMotorista.Proprio,
                    request.MotoristaCategoriaCnh ?? ETipoCategoriaCnh.NaoUtiliza,
                    request.MotoristaDataEmissaoCnh,
                    request.MotoristaDataVencimentoCnh,
                    request.MotoristaRntrc,
                    tenantId,
                    criadoPor
                );
                pessoa.VincularMotorista(motorista);
            }
            if (request.EhTransportadora)
            {
                var transportadora = new PessoaTransportadora(
                    pessoa.Id,
                    request.TransportadoraCiot,
                    request.TransportadoraRntrc,
                    tenantId,
                    criadoPor
                );
                pessoa.VincularTransportadora(transportadora);
            }
            if (request.EhPrestadorServico)
            {
                var prestador = new PessoaPrestadorServico(
                    pessoa.Id,
                    request.PrestadorCei,
                    tenantId,
                    criadoPor
                );
                pessoa.VincularPrestadorServico(prestador);
            }

            pessoa.DefinirFornecedor(request.EhFornecedor);
            pessoa.DefinirProdutorRural(request.EhProdutorRural);

            // Coleções: Endereços
            if (request.Enderecos != null)
            {
                foreach (var endDto in request.Enderecos)
                {
                    var endereco = new Epros.Modules.GestaoClientes.Domain.Entities.Endereco(
                        pessoa.Id,
                        endDto.TipoEndereco,
                        endDto.PaisId,
                        endDto.MunicipioId,
                        endDto.SubdivisaoId,
                        endDto.Uf,
                        endDto.Cep,
                        endDto.Logradouro,
                        endDto.Numero,
                        endDto.Complemento,
                        endDto.Bairro,
                        endDto.Referencia,
                        endDto.CodigoPostalInternacional,
                        endDto.LinhaEndereco1,
                        endDto.LinhaEndereco2,
                        endDto.Latitude,
                        endDto.Longitude,
                        tenantId,
                        criadoPor
                    );
                    pessoa.AdicionarEndereco(endereco);
                }
            }

            // Coleções: Contatos
            if (request.Contatos != null)
            {
                foreach (var contDto in request.Contatos)
                {
                    var contato = new PessoaContato(
                        pessoa.Id,
                        contDto.Nome,
                        contDto.TipoContatoTelefonico,
                        contDto.NumeroTelefone,
                        contDto.TipoContatoEmail,
                        contDto.Email,
                        contDto.EhPrincipal,
                        tenantId,
                        criadoPor
                    );
                    pessoa.AdicionarContato(contato);
                }
            }

            // Coleções: Veículos
            if (request.Veiculos != null)
            {
                foreach (var vDto in request.Veiculos)
                {
                    var veiculo = new PessoaVeiculo(
                        pessoa.Id,
                        vDto.PaisId,
                        vDto.TipoVeiculo,
                        vDto.Uf,
                        vDto.Placa,
                        vDto.Rntrc,
                        tenantId,
                        criadoPor
                    );
                    pessoa.AdicionarVeiculo(veiculo);
                }
            }

            // Executa as validações ricas de domínio agregadas
            pessoa.Validar();

            if (!pessoa.IsValid)
            {
                var erros = pessoa.Notifications.Select(n => n.Message).Distinct();
                return CommandResult.Falha(erros, "Invariantes de domínio da Pessoa não foram atendidas.");
            }

            // Persistência
            _context.Pessoas.Add(pessoa);

            // REG-PEM-160/161: publica evento pessoa.criada via Outbox (após commit transacional).
            var documento = request.TipoPessoa switch
            {
                ETipoPessoa.PessoaFisica => request.FisicaCpf,
                ETipoPessoa.PessoaJuridica => request.JuridicaCnpj,
                _ => request.IdentificacaoEstrangeiro
            };
            var payloadCriada = JsonSerializer.Serialize(new PessoaCriadaEventNotification(
                pessoa.Id, tenantId, (int)pessoa.TipoPessoa, documento, pessoa.EhCliente, pessoa.EhFornecedor, DateTime.UtcNow, criadoPor));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "pessoa.criada", payloadCriada));

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Pessoa cadastrada com sucesso!", new { PessoaId = pessoa.Id });
        }
    }
}
