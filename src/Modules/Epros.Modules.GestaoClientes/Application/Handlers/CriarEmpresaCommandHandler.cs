using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
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
    /// <summary>Cria Empresa.</summary>
    public class CriarEmpresaCommandHandler : ICommandHandler<CriarEmpresaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IValidadorLimitesSaaS _validadorLimites;
        private readonly ISegredoCofreService _cofreService;

        public CriarEmpresaCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IValidadorLimitesSaaS validadorLimites,
            ISegredoCofreService cofreService)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _validadorLimites = validadorLimites;
            _cofreService = cofreService;
        }

        public async Task<CommandResult> Handle(CriarEmpresaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var (excedeuLimite, msgLimite) = await _validadorLimites.ValidarLimiteEmpresasAsync(tenantId, cancellationToken);
            if (excedeuLimite)
            {
                return CommandResult.Falha(new[] { msgLimite }, "Limite de empresas excedido");
            }

            // Uniqueness validation
            if (string.IsNullOrWhiteSpace(request.Cnpj))
            {
                return CommandResult.Falha(new[] { "CNPJ inválido" }, "Erro de validação");
            }
            var tempCnpj = new Cnpj(request.Cnpj);
            if (!tempCnpj.IsValid)
            {
                return CommandResult.Falha(new[] { "CNPJ inválido" }, "Erro de validação");
            }

            var existeCnpj = await _context.Empresas.AnyAsync(e => e.Cnpj == tempCnpj.Valor && e.TenantId == tenantId, cancellationToken);
            if (existeCnpj)
            {
                return CommandResult.Falha(new[] { "CNPJ já cadastrado!" }, "Erro de validação");
            }

            // Mapeia Endereco VO
            var endereco = new Epros.Modules.GestaoClientes.Domain.ValueObjects.Endereco(
                request.Endereco.Logradouro,
                request.Endereco.Numero,
                request.Endereco.Complemento,
                request.Endereco.Bairro,
                request.Endereco.Cep,
                request.Endereco.Cidade,
                request.Endereco.Estado
            );

            // T-01/P1-2: o TokenMercadoPagoPix nunca é persistido em claro. Se veio um valor (não vazio e
            // diferente da máscara), cifra via cofre; senão persiste nulo.
            var tokenInformado = !string.IsNullOrEmpty(request.TokenMercadoPagoPix) && request.TokenMercadoPagoPix != Empresa.MascaraTokenPix;
            var tokenParaPersistir = tokenInformado
                ? await _cofreService.CriptografarAsync(request.TokenMercadoPagoPix!)
                : null;

            // Instancia Empresa
            var empresa = new Empresa(
                request.RazaoSocial,
                request.NomeFantasia,
                request.Cnpj,
                request.InscricaoEstadual,
                request.InscricaoMunicipal,
                request.InscricaoSuframa,
                request.Cnae,
                request.RegimeTributario,
                request.RegimeApuracao,
                request.PessoaGrupoId,
                request.ProdutoGrupoId,
                request.PlanoContasFinanceiroId,
                request.TributarioGrupoId,
                request.NcmTributacaoId,
                request.CertificadoDigitalId,
                request.EmpresaParametrosDfeId,
                request.LinkWebApiAppVendas,
                tokenParaPersistir,
                request.Logo,
                endereco,
                tenantId,
                criadoPor,
                false,
                request.DateFormat ?? "MM-DD-YYYY",
                request.TimeZoneId,
                request.CurrencyId
            );

            // Valida invariantes de domínio
            if (!empresa.IsValid)
            {
                var erros = empresa.Notifications.Select(n => n.Message).Distinct();
                return CommandResult.Falha(erros, "Invariantes de domínio da Empresa não foram atendidas.");
            }

            // T-03: contrato de evento CAD-* empresa.criada, publicado via Outbox no mesmo commit.
            var eventoEmpresaCriada = new OutboxMessage(
                tenantId,
                "empresa.criada",
                JsonSerializer.Serialize(new EmpresaCriadaEventNotification(
                    empresa.Id, tenantId, tempCnpj.Valor, request.RazaoSocial ?? string.Empty, DateTime.UtcNow, criadoPor)));

            var existeEmpresas = await _context.Empresas.AnyAsync(e => e.TenantId == tenantId, cancellationToken);
            if (!existeEmpresas)
            {
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    _context.Empresas.Add(empresa);
                    _context.OutboxMessages.Add(eventoEmpresaCriada);
                    await _context.SaveChangesAsync(cancellationToken);

                    // Auditoria APP: removido o bloco "Criar Plano Inicial" que só gravava UpgradePlano (órfão —
                    // ninguém lia). O plano real do tenant é atribuído no fluxo de onboarding SaaS
                    // (OnboardingSaaSProvisioner / Cliente.PlanoId), não aqui.

                    // Criar Exercício Financeiro Inicial (365 dias)
                    var exercicio = new ExercicioFinanceiro(DateTime.UtcNow, DateTime.UtcNow.AddDays(365), null, "Aberto", tenantId, criadoPor);
                    if (!exercicio.IsValid)
                    {
                        var errosExercicio = exercicio.Notifications.Select(n => n.Message).Distinct();
                        await transaction.RollbackAsync(cancellationToken);
                        return CommandResult.Falha(errosExercicio, "Falha ao criar o exercício financeiro inicial.");
                    }
                    _context.ExerciciosFinanceiros.Add(exercicio);

                    // Cria as preferências iniciais do inquilino com caixa e estoque negativo true por padrão
                    var preferencias = new PreferenciaGeral(true, true, true, StockCalculationMode.CustoMedio, false, false, false, false, tenantId, criadoPor);
                    if (!preferencias.IsValid)
                    {
                        var errosPref = preferencias.Notifications.Select(n => n.Message).Distinct();
                        await transaction.RollbackAsync(cancellationToken);
                        return CommandResult.Falha(errosPref, "Falha ao criar as preferências operacionais.");
                    }
                    _context.PreferenciasGerais.Add(preferencias);

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return CommandResult.Falha(new[] { ex.Message }, "Erro ao inicializar empresa e parâmetros.");
                }
            }
            else
            {
                _context.Empresas.Add(empresa);
                _context.OutboxMessages.Add(eventoEmpresaCriada);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return CommandResult.Ok("Empresa cadastrada com sucesso!", new { EmpresaId = empresa.Id });
        }
    }
}
