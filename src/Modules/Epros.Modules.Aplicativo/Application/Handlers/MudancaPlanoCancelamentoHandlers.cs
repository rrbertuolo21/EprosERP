using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    /// <summary>
    /// 1.08D — Mudança de plano SELF-SERVICE (upgrade/downgrade) na área do cliente.
    ///
    /// • Troca o <c>Plano</c> da <see cref="AssinaturaCliente"/> vigente e do <see cref="Cliente"/>.
    /// • Reavaliação de limites/entitlement: NÃO apaga excedente. O <c>ValidadorLimitesSaaS</c> lê o
    ///   plano vigente do Cliente e passa a BLOQUEAR novas criações se o novo plano tiver limite menor
    ///   que o uso atual (decisão registrada na 1.06). O uso existente é preservado.
    /// • PRORAÇÃO: mecanismo pro-rata por DIAS corridos do ciclo (default). Débito → fatura de diferença;
    ///   crédito → registrado para compensar na próxima fatura.
    ///   ⚠️ VALIDA-CONTADOR: a POLÍTICA exata de proração (arredondamento, índice de reajuste, contas
    ///   contábeis de crédito/débito, reconhecimento de receita) é PARÂMETRO do cliente/contador
    ///   (skill financeiro/contábil). Aqui só há o mecanismo — NÃO se inventa alíquota nem índice.
    /// • Enfileira evento no Outbox (o GestaoClientesOutboxProcessorJob / 1.08C entrega a notificação).
    /// </summary>
    public class MudarPlanoCommandHandler : ICommandHandler<MudarPlanoCommand>
    {
        private readonly ContextGestaoClientes _contextGestao;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public MudarPlanoCommandHandler(
            ContextGestaoClientes contextGestao,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _contextGestao = contextGestao;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(MudarPlanoCommand request, CancellationToken cancellationToken)
        {
            var validator = new MudarPlanoCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return CommandResult.Falha(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var cliente = await _contextGestao.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Ativo && c.DeletadoEm == null, cancellationToken);

            if (cliente == null)
            {
                return CommandResult.Falha(new[] { "Cliente assinante correspondente ao inquilino atual não foi encontrado." });
            }

            var novoPlano = await _contextGestao.Planos
                .IgnoreQueryFilters()
                .Include(p => p.Modulos)
                .FirstOrDefaultAsync(p => p.Id == request.NovoPlanoId && p.Ativo && p.DeletadoEm == null, cancellationToken);

            if (novoPlano == null)
            {
                return CommandResult.Falha(new[] { "Plano de destino não encontrado ou inativo." });
            }

            var assinatura = await _contextGestao.AssinaturasClientes
                .IgnoreQueryFilters()
                .Where(a => a.ClienteId == cliente.Id && a.Status == AssinaturaStatus.Ativa && a.DeletadoEm == null && !a.Arquivada)
                .OrderByDescending(a => a.DataFim)
                .FirstOrDefaultAsync(cancellationToken);

            if (assinatura == null)
            {
                return CommandResult.Falha(new[] { "Nenhuma assinatura ativa para trocar de plano. Contrate um plano primeiro." });
            }

            var planoAnteriorId = assinatura.PlanoId;
            if (planoAnteriorId == novoPlano.Id)
            {
                return CommandResult.Falha(new[] { "A assinatura já está neste plano." });
            }

            var planoAnterior = await _contextGestao.Planos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == planoAnteriorId, cancellationToken);

            var precoAnterior = planoAnterior?.Preco ?? 0m;
            var precoNovo = novoPlano.Preco;

            // ===== Proração: mecanismo pro-rata por DIAS corridos do ciclo (default) =====
            var hoje = DateTime.UtcNow;
            var inicio = assinatura.DataInicio ?? assinatura.CriadoEm;
            var fim = assinatura.DataFim ?? inicio.AddDays(30); // ciclo mensal default quando sem DataFim
            var diasCiclo = Math.Max(1, (int)Math.Round((fim - inicio).TotalDays));
            var diasRestantes = Math.Max(0, (int)Math.Round((fim - hoje).TotalDays));
            if (diasRestantes > diasCiclo) diasRestantes = diasCiclo;

            var valorDiaAnterior = precoAnterior / diasCiclo;
            var valorDiaNovo = precoNovo / diasCiclo;
            // ⚠️ Arredondamento a 2 casas é o DEFAULT do mecanismo — critério fiscal/contábil = valida contador.
            var valorAjuste = Math.Round((valorDiaNovo - valorDiaAnterior) * diasRestantes, 2, MidpointRounding.AwayFromZero);

            var tipoProracao = valorAjuste > 0 ? TipoAjusteProracao.Debito
                             : valorAjuste < 0 ? TipoAjusteProracao.Credito
                             : TipoAjusteProracao.Neutro;

            var tipoMudanca = precoNovo > precoAnterior ? "Upgrade"
                            : precoNovo < precoAnterior ? "Downgrade"
                            : "Lateral";

            // Snapshot do novo plano (mesmo formato do ContratarPlano).
            var snapshot = JsonSerializer.Serialize(new
            {
                NomePlano = novoPlano.Nome,
                Preco = novoPlano.Preco,
                LimiteUsuarios = novoPlano.LimiteUsuarios,
                LimiteEmpresas = novoPlano.LimiteEmpresas,
                Modulos = novoPlano.Modulos.Select(m => m.NomeModulo).ToList()
            });

            // Troca de plano: assinatura + cliente (entitlement do ValidadorLimitesSaaS segue cliente.PlanoId).
            assinatura.TrocarPlano(novoPlano.Id, snapshot, criadoPor);
            cliente.AlterarPlano(novoPlano.Id, criadoPor);

            // Débito → gera FATURA DE DIFERENÇA (reusa o padrão Fatura). Fatura exige valor > 0.
            Guid? faturaDiferencaId = null;
            if (tipoProracao == TipoAjusteProracao.Debito)
            {
                var fatura = new Fatura(cliente.Id, valorAjuste, hoje.AddDays(5), tenantId, criadoPor);
                if (fatura.IsValid)
                {
                    fatura.DefinirDadosEmissao(
                        null,
                        "Ajuste de proração por mudança de plano (upgrade). ⚠️ Política de proração = valida contador.",
                        criadoPor);
                    _contextGestao.Faturas.Add(fatura);
                    faturaDiferencaId = fatura.Id;
                }
            }

            var observacao = tipoProracao switch
            {
                TipoAjusteProracao.Debito => "Débito de proração cobrado em fatura de diferença. ⚠️ Política de proração e contas contábeis = valida contador.",
                TipoAjusteProracao.Credito => "Crédito de proração a compensar na próxima fatura. ⚠️ Política de proração e contas contábeis = valida contador.",
                _ => "Sem diferença proporcional no restante do ciclo."
            };

            var registro = new AjusteProracao(
                assinaturaClienteId: assinatura.Id,
                clienteId: cliente.Id,
                planoAnteriorId: planoAnteriorId,
                planoNovoId: novoPlano.Id,
                precoAnterior: precoAnterior,
                precoNovo: precoNovo,
                diasCiclo: diasCiclo,
                diasRestantes: diasRestantes,
                valorAjuste: valorAjuste,
                tipo: tipoProracao,
                faturaId: faturaDiferencaId,
                observacao: observacao,
                tenantId: tenantId,
                criadoPor: criadoPor);
            _contextGestao.AjustesProracao.Add(registro);

            // 1.08C — evento de notificação (o GestaoClientesOutboxProcessorJob entrega).
            var payload = JsonSerializer.Serialize(new
            {
                AssinaturaId = assinatura.Id,
                ClienteId = cliente.Id,
                TenantId = tenantId,
                PlanoAnteriorId = planoAnteriorId,
                PlanoNovoId = novoPlano.Id,
                TipoMudanca = tipoMudanca,
                ValorProracao = valorAjuste,
                TipoProracao = tipoProracao.ToString(),
                FaturaDiferencaId = faturaDiferencaId
            });
            _contextGestao.OutboxMessages.Add(new OutboxMessage(tenantId, "PlanoAlteradoEvent", payload));

            await _contextGestao.SaveChangesAsync(cancellationToken);

            var dto = new MudancaPlanoResultadoDto(
                AssinaturaId: assinatura.Id,
                PlanoAnteriorId: planoAnteriorId,
                PlanoNovoId: novoPlano.Id,
                TipoMudanca: tipoMudanca,
                PrecoAnterior: precoAnterior,
                PrecoNovo: precoNovo,
                DiasCiclo: diasCiclo,
                DiasRestantes: diasRestantes,
                ValorProracao: valorAjuste,
                TipoProracao: tipoProracao.ToString(),
                FaturaDiferencaId: faturaDiferencaId,
                ValidaContador: true);

            return CommandResult.Ok("Plano alterado com sucesso. Ajuste de proração registrado.", dto);
        }
    }

    /// <summary>
    /// 1.08D — Cancelamento self-service GOVERNADO. Move o tenant para <c>StatusSaaS.Cancelado</c>
    /// (o middleware ancora a janela somente-leitura/export de 30 dias — REG-021 — em
    /// Cliente.StatusSaaSAtualizadoEm) e registra quem/quando/por quê na assinatura. Não bloqueia de
    /// imediato: a janela de export já cobre. Reversível via <see cref="ReativarAssinaturaCommandHandler"/>.
    /// </summary>
    public class CancelarAssinaturaCommandHandler : ICommandHandler<CancelarAssinaturaCommand>
    {
        private readonly ContextGestaoClientes _contextGestao;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CancelarAssinaturaCommandHandler(
            ContextGestaoClientes contextGestao,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _contextGestao = contextGestao;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CancelarAssinaturaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var cliente = await _contextGestao.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Ativo && c.DeletadoEm == null, cancellationToken);

            if (cliente == null)
            {
                return CommandResult.Falha(new[] { "Cliente assinante correspondente ao inquilino atual não foi encontrado." });
            }

            var assinatura = await _contextGestao.AssinaturasClientes
                .IgnoreQueryFilters()
                .Where(a => a.ClienteId == cliente.Id
                            && (a.Status == AssinaturaStatus.Ativa || a.Status == AssinaturaStatus.Futura)
                            && a.DeletadoEm == null && !a.Arquivada)
                .OrderByDescending(a => a.DataFim)
                .FirstOrDefaultAsync(cancellationToken);

            if (assinatura == null)
            {
                return CommandResult.Falha(new[] { "Nenhuma assinatura ativa para cancelar." });
            }

            assinatura.RegistrarCancelamento(request.Motivo, criadoPor);
            // Janela de 30 dias somente-leitura/export ancorada aqui (StatusSaaSAtualizadoEm = agora).
            cliente.AtualizarStatusSaaS(StatusSaaS.Cancelado, criadoPor);

            var payload = JsonSerializer.Serialize(new
            {
                AssinaturaId = assinatura.Id,
                ClienteId = cliente.Id,
                TenantId = tenantId,
                Motivo = request.Motivo,
                CanceladaEm = assinatura.CanceladaEm,
                CanceladaPor = criadoPor
            });
            _contextGestao.OutboxMessages.Add(new OutboxMessage(tenantId, "AssinaturaCanceladaEvent", payload));

            await _contextGestao.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok(
                "Assinatura cancelada. Você mantém acesso somente-leitura por 30 dias para exportar seus dados; reative quando quiser dentro desse período.",
                new { AssinaturaId = assinatura.Id, Status = assinatura.Status.ToString(), CanceladaEm = assinatura.CanceladaEm });
        }
    }

    /// <summary>
    /// 1.08D — Reativação self-service dentro da janela de 30 dias: volta o tenant a <c>Ativo</c> e a
    /// assinatura a <c>Ativa</c> (honra a skill jurídica: reativar ao voltar). Exige plano ainda disponível.
    /// </summary>
    public class ReativarAssinaturaCommandHandler : ICommandHandler<ReativarAssinaturaCommand>
    {
        private readonly ContextGestaoClientes _contextGestao;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ReativarAssinaturaCommandHandler(
            ContextGestaoClientes contextGestao,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _contextGestao = contextGestao;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ReativarAssinaturaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var cliente = await _contextGestao.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Ativo && c.DeletadoEm == null, cancellationToken);

            if (cliente == null)
            {
                return CommandResult.Falha(new[] { "Cliente assinante correspondente ao inquilino atual não foi encontrado." });
            }

            if (cliente.StatusSaaS != StatusSaaS.Cancelado)
            {
                return CommandResult.Falha(new[] { "A assinatura não está cancelada; não há o que reativar." });
            }

            // Janela de 30 dias (mesma âncora do middleware: StatusSaaSAtualizadoEm).
            var desde = cliente.StatusSaaSAtualizadoEm ?? cliente.AlteradoEm ?? cliente.CriadoEm;
            if (DateTime.UtcNow - desde > TimeSpan.FromDays(30))
            {
                return CommandResult.Falha(new[] { "A janela de 30 dias para reativação expirou. Contrate um novo plano para voltar a operar." });
            }

            var assinatura = await _contextGestao.AssinaturasClientes
                .IgnoreQueryFilters()
                .Where(a => a.ClienteId == cliente.Id && a.Status == AssinaturaStatus.Cancelada && a.DeletadoEm == null && !a.Arquivada)
                .OrderByDescending(a => a.CanceladaEm)
                .FirstOrDefaultAsync(cancellationToken);

            if (assinatura == null)
            {
                return CommandResult.Falha(new[] { "Nenhuma assinatura cancelada encontrada para reativar." });
            }

            var plano = await _contextGestao.Planos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == assinatura.PlanoId && p.Ativo && p.DeletadoEm == null, cancellationToken);

            if (plano == null)
            {
                return CommandResult.Falha(new[] { "O plano da assinatura não está mais disponível. Contrate um novo plano." });
            }

            assinatura.Reativar(criadoPor);
            cliente.AtualizarStatusSaaS(StatusSaaS.Ativo, criadoPor);
            cliente.AlterarPlano(assinatura.PlanoId, criadoPor);

            var payload = JsonSerializer.Serialize(new
            {
                AssinaturaId = assinatura.Id,
                ClienteId = cliente.Id,
                TenantId = tenantId,
                ReativadaPor = criadoPor,
                ReativadaEm = DateTime.UtcNow
            });
            _contextGestao.OutboxMessages.Add(new OutboxMessage(tenantId, "AssinaturaReativadaEvent", payload));

            await _contextGestao.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Assinatura reativada com sucesso.",
                new { AssinaturaId = assinatura.Id, Status = assinatura.Status.ToString() });
        }
    }
}
