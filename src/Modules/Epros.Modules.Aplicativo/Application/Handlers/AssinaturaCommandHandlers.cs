using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    public class ContratarPlanoCommandHandler : ICommandHandler<ContratarPlanoCommand>
    {
        private readonly ContextGestaoClientes _contextGestao;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ContratarPlanoCommandHandler(
            ContextGestaoClientes contextGestao,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _contextGestao = contextGestao;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ContratarPlanoCommand request, CancellationToken cancellationToken)
        {
            var validator = new ContratarPlanoCommandValidator();
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

            var plano = await _contextGestao.Planos
                .IgnoreQueryFilters()
                .Include(p => p.Modulos)
                .FirstOrDefaultAsync(p => p.Id == request.PlanoId && p.Ativo && p.DeletadoEm == null, cancellationToken);

            if (plano == null)
            {
                return CommandResult.Falha(new[] { "Plano não encontrado ou inativo." });
            }

            // Trava de Trial Único / Plano Gratuito de Uso Único
            if (plano.Preco == 0)
            {
                var jaUsouPlano = await _contextGestao.AssinaturasClientes
                    .IgnoreQueryFilters()
                    .AnyAsync(a => a.ClienteId == cliente.Id && a.PlanoId == plano.Id && a.DeletadoEm == null, cancellationToken);

                if (jaUsouPlano)
                {
                    return CommandResult.Falha(new[] { "Este plano gratuito ou período de testes é limitado a uma utilização por cliente." });
                }
            }

            // Verifica se há alguma assinatura ativa do cliente
            var assinaturaAtiva = await _contextGestao.AssinaturasClientes
                .IgnoreQueryFilters()
                .Where(a => a.ClienteId == cliente.Id && a.Status == AssinaturaStatus.Ativa && a.DeletadoEm == null && !a.Arquivada)
                .OrderByDescending(a => a.DataFim)
                .FirstOrDefaultAsync(cancellationToken);

            DateTime dataInicio;
            DateTime? dataFim;
            AssinaturaStatus statusInicial;

            if (assinaturaAtiva != null && assinaturaAtiva.DataFim.HasValue)
            {
                // Encadeamento: começa no dia seguinte ao término da assinatura ativa
                dataInicio = assinaturaAtiva.DataFim.Value.AddDays(1);
                statusInicial = AssinaturaStatus.Futura;
                dataFim = dataInicio.AddDays(30); // Vigência padrão de 30 dias
            }
            else
            {
                dataInicio = DateTime.UtcNow;
                dataFim = plano.Preco == 0 ? (DateTime?)null : DateTime.UtcNow.AddDays(30);
                statusInicial = plano.Preco == 0 ? AssinaturaStatus.Ativa : AssinaturaStatus.AguardandoAprovacao;
            }

            // Snapshot do plano
            var detalhesSnapshot = new
            {
                NomePlano = plano.Nome,
                Preco = plano.Preco,
                LimiteUsuarios = plano.LimiteUsuarios,
                LimiteEmpresas = plano.LimiteEmpresas,
                Modulos = plano.Modulos.Select(m => m.NomeModulo).ToList()
            };
            var jsonSnapshot = JsonSerializer.Serialize(detalhesSnapshot);

            var novaAssinatura = new AssinaturaCliente(
                clienteId: cliente.Id,
                planoId: plano.Id,
                status: statusInicial,
                dataInicio: dataInicio,
                dataFim: dataFim,
                trialAte: plano.Preco == 0 ? DateTime.UtcNow.AddDays(15) : (DateTime?)null, // Simula período trial para gratuito
                metodoPagamento: request.MetodoPagamento,
                transacaoId: statusInicial == AssinaturaStatus.Ativa ? "zero-transaction-" + Guid.NewGuid() : null,
                detalhesPacoteJson: jsonSnapshot,
                tenantId: tenantId,
                criadoPor: criadoPor
            );

            _contextGestao.AssinaturasClientes.Add(novaAssinatura);

            // Atualiza o PlanoId no Cliente
            if (statusInicial == AssinaturaStatus.Ativa)
            {
                cliente.AlterarPlano(plano.Id, criadoPor);
            }

            // Se for assinatura paga pendente, gera uma fatura de cobrança
            if (plano.Preco > 0 && statusInicial == AssinaturaStatus.AguardandoAprovacao)
            {
                var fatura = new Fatura(
                    clienteId: cliente.Id,
                    valor: plano.Preco,
                    dataVencimento: DateTime.UtcNow.AddDays(5),
                    tenantId: tenantId,
                    criadoPor: criadoPor
                );
                _contextGestao.Faturas.Add(fatura);
            }

            await _contextGestao.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Plano contratado com sucesso!", new { AssinaturaId = novaAssinatura.Id, Status = novaAssinatura.Status.ToString() });
        }
    }

    public class GerarPixFaturaCommandHandler : ICommandHandler<GerarPixFaturaCommand>
    {
        private readonly ContextGestaoClientes _contextGestao;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public GerarPixFaturaCommandHandler(
            ContextGestaoClientes contextGestao,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _contextGestao = contextGestao;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(GerarPixFaturaCommand request, CancellationToken cancellationToken)
        {
            var validator = new GerarPixFaturaCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return CommandResult.Falha(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var criadoPor = _currentUser.GetUserId() ?? "system";
            var tenantId = _tenantProvider.GetTenantId();

            var fatura = await _contextGestao.Faturas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(f => f.Id == request.FaturaId && f.DeletadoEm == null, cancellationToken);

            if (fatura == null)
            {
                return CommandResult.Falha(new[] { "Fatura não encontrada." });
            }

            if (fatura.Status == FaturaStatus.Paga || fatura.Status == FaturaStatus.Cancelada)
            {
                return CommandResult.Falha(new[] { "A fatura já está quitada ou cancelada." });
            }

            var pagamentoExistente = await _contextGestao.PagamentosFaturas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.FaturaId == fatura.Id && p.Status == PagamentoFaturaStatus.Pending && p.DeletadoEm == null, cancellationToken);

            if (pagamentoExistente == null)
            {
                pagamentoExistente = new PagamentoFatura(
                    faturaId: fatura.Id,
                    tipoPagamento: "PIX",
                    status: PagamentoFaturaStatus.Pending,
                    valorPago: fatura.Valor,
                    valorTarifa: 0.75m,
                    identificadorPagamento: "mp-pix-" + Guid.NewGuid().ToString("N"),
                    pagoManualmente: false,
                    dataPagamento: null,
                    tenantId: tenantId,
                    criadoPor: criadoPor
                );
                _contextGestao.PagamentosFaturas.Add(pagamentoExistente);
                await _contextGestao.SaveChangesAsync(cancellationToken);
            }

            var pixDto = new PixResponseDto(
                FaturaId: fatura.Id,
                Valor: fatura.Valor,
                QrCodeBase64: "base64-string-simulated-qrcode-data-for-epros-billing",
                CheckoutUrl: "https://checkout.epros.com/pay/" + pagamentoExistente.IdentificadorPagamento,
                DataExpiracao: DateTime.UtcNow.AddMinutes(30)
            );

            return CommandResult.Ok("PIX gerado com sucesso!", pixDto);
        }
    }
}
