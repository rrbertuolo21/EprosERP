using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    public class CriarPedidoSaaSCommandHandler : ICommandHandler<CriarPedidoSaaSCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPedidoSaaSCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPedidoSaaSCommand request, CancellationToken cancellationToken)
        {
            var validator = new CriarPedidoSaaSCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return CommandResult.Falha(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var cliente = await _context.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Ativo && c.DeletadoEm == null, cancellationToken);

            if (cliente == null)
            {
                return CommandResult.Falha(new[] { "Cliente correspondente ao inquilino atual não encontrado." });
            }

            var plano = await _context.Planos
                .IgnoreQueryFilters()
                .Include(p => p.Modulos)
                .FirstOrDefaultAsync(p => p.Id == request.PlanoId && p.Ativo && p.DeletadoEm == null, cancellationToken);

            if (plano == null)
            {
                return CommandResult.Falha(new[] { "Plano não encontrado ou inativo." });
            }

            // Tratamento especial para Plano Gratuito (Preço 0)
            if (plano.Preco == 0)
            {
                var jaUsouPlano = await _context.AssinaturasClientes
                    .IgnoreQueryFilters()
                    .AnyAsync(a => a.ClienteId == cliente.Id && a.PlanoId == plano.Id && a.DeletadoEm == null, cancellationToken);

                if (jaUsouPlano)
                {
                    return CommandResult.Falha(new[] { "Este plano gratuito ou período de testes é limitado a uma utilização por cliente." });
                }
            }

            // Validar e calcular cupom de desconto se informado
            Cupom? cupom = null;
            decimal desconto = 0;

            if (!string.IsNullOrWhiteSpace(request.CodigoCupom))
            {
                var codigoUpper = request.CodigoCupom.Trim().ToUpperInvariant();
                cupom = await _context.Cupons
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Codigo == codigoUpper && c.DeletadoEm == null, cancellationToken);

                if (cupom == null)
                {
                    return CommandResult.Falha(new[] { "Cupom inválido ou inexistente." });
                }

                if (!cupom.Validar())
                {
                    return CommandResult.Falha(new[] { "Este cupom está expirado, inativo ou com limite de usos esgotado." });
                }

                desconto = cupom.CalcularDesconto(plano.Preco);
            }

            var valorTotal = Math.Max(0, plano.Preco - desconto);

            // Cria o registro do Pedido
            var pedido = new PedidoSaaS(
                clienteId: cliente.Id,
                planoId: plano.Id,
                cupomId: cupom?.Id,
                valorBase: plano.Preco,
                valorDesconto: desconto,
                moeda: "BRL",
                metodoPagamento: request.MetodoPagamento,
                tenantId: tenantId,
                criadoPor: criadoPor
            );

            _context.PedidosSaaS.Add(pedido);

            // Se aplicou cupom, registra o uso
            if (cupom != null)
            {
                var usoCupom = new UsoCupom(
                    clienteId: cliente.Id,
                    cupomId: cupom.Id,
                    pedidoId: pedido.Id,
                    tenantId: tenantId,
                    criadoPor: criadoPor
                );
                _context.UsosCupons.Add(usoCupom);
                cupom.IncrementarUso(criadoPor);
            }

            // Verifica se há alguma assinatura ativa do cliente para encadeamento
            var assinaturaAtiva = await _context.AssinaturasClientes
                .IgnoreQueryFilters()
                .Where(a => a.ClienteId == cliente.Id && a.Status == AssinaturaStatus.Ativa && a.DeletadoEm == null && !a.Arquivada)
                .OrderByDescending(a => a.DataFim)
                .FirstOrDefaultAsync(cancellationToken);

            DateTime dataInicio;
            DateTime? dataFim;
            AssinaturaStatus statusInicial;

            if (assinaturaAtiva != null && assinaturaAtiva.DataFim.HasValue)
            {
                dataInicio = assinaturaAtiva.DataFim.Value.AddDays(1);
                statusInicial = AssinaturaStatus.Futura;
                dataFim = dataInicio.AddDays(30);
            }
            else
            {
                dataInicio = DateTime.UtcNow;
                dataFim = valorTotal == 0 ? (DateTime?)null : DateTime.UtcNow.AddDays(30);
                statusInicial = valorTotal == 0 ? AssinaturaStatus.Ativa : AssinaturaStatus.AguardandoAprovacao;
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
                trialAte: valorTotal == 0 ? DateTime.UtcNow.AddDays(15) : (DateTime?)null,
                metodoPagamento: request.MetodoPagamento,
                transacaoId: statusInicial == AssinaturaStatus.Ativa ? "free-pedido-" + Guid.NewGuid() : null,
                detalhesPacoteJson: jsonSnapshot,
                tenantId: tenantId,
                criadoPor: criadoPor
            );

            _context.AssinaturasClientes.Add(novaAssinatura);

            // Se foi liquidado de imediato (plano gratuito ou 100% de desconto)
            if (statusInicial == AssinaturaStatus.Ativa)
            {
                pedido.Liquidar(novaAssinatura.Id, criadoPor);
                cliente.AlterarPlano(plano.Id, criadoPor);

                // Registra o pagamento global zerado
                var pagamentoGlobal = new PagamentoGlobal(
                    assinaturaId: novaAssinatura.Id,
                    pedidoId: pedido.Id,
                    faturaId: null,
                    dataPagamento: DateTime.UtcNow,
                    valor: 0,
                    gateway: request.MetodoPagamento,
                    transactionId: "free-tx-" + Guid.NewGuid().ToString("N"),
                    tenantId: tenantId,
                    criadoPor: criadoPor
                );
                _context.PagamentosGlobais.Add(pagamentoGlobal);
            }
            else if (statusInicial == AssinaturaStatus.AguardandoAprovacao)
            {
                // Se for pago e pendente, gera uma fatura mensal de cobrança
                var fatura = new Fatura(
                    clienteId: cliente.Id,
                    valor: valorTotal,
                    dataVencimento: DateTime.UtcNow.AddDays(5),
                    tenantId: tenantId,
                    criadoPor: criadoPor
                );
                _context.Faturas.Add(fatura);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Pedido criado com sucesso!", new
            {
                PedidoId = pedido.Id,
                ValorTotal = pedido.ValorTotal,
                Status = pedido.Status,
                AssinaturaId = novaAssinatura.Id,
                AssinaturaStatus = novaAssinatura.Status.ToString()
            });
        }
    }

    /// <summary>
    /// 1.08B — Checkout online REAL (substitui o stub). Cria uma preferência de Checkout Pro no gateway
    /// (Mercado Pago): o cliente escolhe o método (PIX/cartão/boleto) e paga na tela HOSPEDADA do gateway;
    /// o retorno concilia pelo webhook unificado (external_reference = Id da Fatura do pedido).
    /// ⛔ PCI: nenhum dado de cartão passa pelo backend — o pagamento acontece no ambiente do gateway.
    /// Fail-closed: sem gateway configurado, o checkout não é iniciado.
    /// </summary>
    public class IniciarCheckoutCommandHandler : ICommandHandler<IniciarCheckoutCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly Epros.Modules.GestaoClientes.Application.Interfaces.IPaymentGateway _paymentGateway;

        public IniciarCheckoutCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            Epros.Modules.GestaoClientes.Application.Interfaces.IPaymentGateway paymentGateway)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _paymentGateway = paymentGateway;
        }

        public async Task<CommandResult> Handle(IniciarCheckoutCommand request, CancellationToken cancellationToken)
        {
            var validator = new IniciarCheckoutCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return CommandResult.Falha(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var pedido = await _context.PedidosSaaS
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == request.PedidoId && p.TenantId == tenantId && p.DeletadoEm == null, cancellationToken);

            if (pedido == null)
            {
                return CommandResult.Falha(new[] { "Pedido não encontrado." });
            }

            if (pedido.Status == PedidoSaaSStatus.Succeeded || pedido.Status == PedidoSaaSStatus.Refunded)
            {
                return CommandResult.Falha(new[] { "Este pedido já foi liquidado ou estornado." });
            }

            var assinatura = await _context.AssinaturasClientes
                .IgnoreQueryFilters()
                .Where(a => a.ClienteId == pedido.ClienteId && a.PlanoId == pedido.PlanoId && a.DeletadoEm == null && !a.Arquivada)
                .OrderByDescending(a => a.CriadoEm)
                .FirstOrDefaultAsync(cancellationToken);

            if (assinatura == null)
            {
                return CommandResult.Falha(new[] { "Assinatura do pedido correspondente não foi encontrada." });
            }

            // Resolve a config do gateway (por tenant, senão global). Fail-closed.
            var config = await _context.ConfiguracoesGatewayPagamento
                .Where(c => c.Ativo && (c.TenantAlvo == tenantId || c.TenantAlvo == null))
                .OrderByDescending(c => c.TenantAlvo == tenantId)
                .ThenByDescending(c => c.CriadoEm)
                .FirstOrDefaultAsync(cancellationToken);
            if (config == null)
                return CommandResult.Falha(new[] { "Gateway de pagamento não configurado." }, "Gateway não configurado");

            // Fatura de referência do pedido (external_reference do checkout). Se não existir, gera uma.
            var fatura = await _context.Faturas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(f => f.ClienteId == pedido.ClienteId && f.Valor == pedido.ValorTotal
                                          && f.Status == FaturaStatus.Pendente && f.DeletadoEm == null, cancellationToken);
            if (fatura == null)
            {
                fatura = new Fatura(pedido.ClienteId, pedido.ValorTotal, DateTime.UtcNow.AddDays(5), tenantId, criadoPor);
                if (!fatura.IsValid)
                    return CommandResult.Falha(fatura.Notifications.Select(n => n.Message), "Falha ao gerar a fatura do checkout");
                _context.Faturas.Add(fatura);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var cliente = await _context.Clientes.IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == pedido.ClienteId && c.DeletadoEm == null, cancellationToken);
            var pagador = new Epros.Modules.GestaoClientes.Application.Interfaces.DadosPagador(
                cliente?.Email ?? "sem-email@epros.com", cliente?.RazaoSocial, cliente?.Cnpj);

            var pref = await _paymentGateway.CriarPreferenciaCheckoutAsync(
                fatura, config, $"Assinatura Epros (pedido {pedido.Id})", pagador, null, cancellationToken);
            if (!pref.Sucesso || pref.Dados is not Epros.Modules.GestaoClientes.Application.Interfaces.PreferenciaCheckoutResultado dto)
                return pref;

            var sessaoExistente = await _context.SessoesPagamentos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.PedidoId == pedido.Id && s.Status == SessaoPagamentoStatus.Pending && s.DeletadoEm == null, cancellationToken);

            if (sessaoExistente == null)
            {
                sessaoExistente = new SessaoPagamento(
                    gatewayRef: dto.PreferenceId,
                    assinaturaId: assinatura.Id,
                    pedidoId: pedido.Id,
                    tenantId: tenantId,
                    criadoPor: criadoPor
                );
                _context.SessoesPagamentos.Add(sessaoExistente);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return CommandResult.Ok("Sessão de checkout iniciada com sucesso!", new
            {
                PedidoId = pedido.Id,
                FaturaId = fatura.Id,
                SessaoId = sessaoExistente.Id,
                PreferenceId = dto.PreferenceId,
                CheckoutUrl = dto.InitPoint,
                Status = sessaoExistente.Status
            });
        }
    }

    public class RegistrarTransferenciaCommandHandler : ICommandHandler<RegistrarTransferenciaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarTransferenciaCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarTransferenciaCommand request, CancellationToken cancellationToken)
        {
            var validator = new RegistrarTransferenciaCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return CommandResult.Falha(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            if (request.PedidoId.HasValue)
            {
                var pedido = await _context.PedidosSaaS
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.Id == request.PedidoId.Value && p.TenantId == tenantId && p.DeletadoEm == null, cancellationToken);
                if (pedido == null)
                {
                    return CommandResult.Falha(new[] { "Pedido de referência não encontrado." });
                }
            }

            if (request.FaturaId.HasValue)
            {
                var fatura = await _context.Faturas
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(f => f.Id == request.FaturaId.Value && f.TenantId == tenantId && f.DeletadoEm == null, cancellationToken);
                if (fatura == null)
                {
                    return CommandResult.Falha(new[] { "Fatura de referência não encontrada." });
                }
            }

            var pagamento = new PagamentoTransferencia(
                faturaId: request.FaturaId,
                pedidoId: request.PedidoId,
                valor: request.Valor,
                moeda: "BRL",
                tenantId: tenantId,
                criadoPor: criadoPor
            );

            if (!pagamento.IsValid)
            {
                return CommandResult.Falha(pagamento.Notifications.Select(n => n.Message));
            }

            _context.PagamentosTransferencias.Add(pagamento);
            await _context.SaveChangesAsync(cancellationToken);

            var comprovante = new ComprovantePagamento(
                pagamentoTransferenciaId: pagamento.Id,
                nomeArquivo: request.NomeArquivo,
                caminhoArquivo: request.CaminhoArquivo,
                tamanhoBytes: request.TamanhoBytes,
                valor: request.Valor,
                dataComprovante: request.DataComprovante,
                tenantId: tenantId,
                criadoPor: criadoPor
            );

            if (!comprovante.IsValid)
            {
                return CommandResult.Falha(comprovante.Notifications.Select(n => n.Message));
            }

            _context.ComprovantesPagamentos.Add(comprovante);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Comprovante de transferência bancária enviado com sucesso!", new
            {
                PagamentoTransferenciaId = pagamento.Id,
                ComprovanteId = comprovante.Id,
                Status = pagamento.Status
            });
        }
    }

    public class AnalisarTransferenciaCommandHandler : ICommandHandler<AnalisarTransferenciaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;

        public AnalisarTransferenciaCommandHandler(
            ContextGestaoClientes context,
            ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AnalisarTransferenciaCommand request, CancellationToken cancellationToken)
        {
            var validator = new AnalisarTransferenciaCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return CommandResult.Falha(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var operador = _currentUser.GetUserName() ?? "operator";
            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var pagamento = await _context.PagamentosTransferencias
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == request.PagamentoTransferenciaId && p.DeletadoEm == null, cancellationToken);

            if (pagamento == null)
            {
                return CommandResult.Falha(new[] { "Registro de pagamento por transferência não encontrado." });
            }

            if (pagamento.Status != PagamentoTransferenciaStatus.Pending)
            {
                return CommandResult.Falha(new[] { $"Este comprovante já foi analisado. Status atual: {pagamento.Status}" });
            }

            var comprovante = await _context.ComprovantesPagamentos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.PagamentoTransferenciaId == pagamento.Id && c.DeletadoEm == null, cancellationToken);

            if (comprovante == null)
            {
                return CommandResult.Falha(new[] { "Comprovante físico não encontrado." });
            }

            if (request.Aprovado)
            {
                pagamento.Aprovar(operador, alteradoPor);
                comprovante.MarcarComoLido(alteradoPor);

                Guid? assinaturaId = null;

                // Se houver PedidoSaaS, liquida
                if (pagamento.PedidoId.HasValue)
                {
                    var pedido = await _context.PedidosSaaS
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(p => p.Id == pagamento.PedidoId.Value && p.DeletadoEm == null, cancellationToken);
                    if (pedido != null)
                    {
                        var cliente = await _context.Clientes.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == pedido.ClienteId, cancellationToken);
                        
                        var assinatura = await _context.AssinaturasClientes
                            .IgnoreQueryFilters()
                            .Where(a => a.ClienteId == pedido.ClienteId && a.PlanoId == pedido.PlanoId && a.DeletadoEm == null && !a.Arquivada)
                            .OrderByDescending(a => a.CriadoEm)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (assinatura != null)
                        {
                            assinatura.Ativar(alteradoPor);
                            assinaturaId = assinatura.Id;
                            pedido.Liquidar(assinatura.Id, alteradoPor);
                            
                            if (cliente != null)
                            {
                                cliente.AlterarPlano(pedido.PlanoId, alteradoPor);
                                cliente.AtualizarStatusSaaS(StatusSaaS.Ativo, alteradoPor);
                            }
                        }
                    }
                }

                // Se houver Fatura, liquida
                if (pagamento.FaturaId.HasValue)
                {
                    var fatura = await _context.Faturas
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(f => f.Id == pagamento.FaturaId.Value && f.DeletadoEm == null, cancellationToken);
                    if (fatura != null)
                    {
                        fatura.Baixar(alteradoPor);

                        // Registra o PagamentoFatura
                        var pgFatura = new PagamentoFatura(
                            faturaId: fatura.Id,
                            tipoPagamento: "Transferencia",
                            status: PagamentoFaturaStatus.Paid,
                            valorPago: pagamento.Valor,
                            valorTarifa: 0,
                            identificadorPagamento: "tx-offline-" + pagamento.Id.ToString("N"),
                            pagoManualmente: true,
                            dataPagamento: DateTime.UtcNow,
                            tenantId: pagamento.TenantId,
                            criadoPor: alteradoPor
                        );
                        _context.PagamentosFaturas.Add(pgFatura);

                        // 1.08A — recibo do pagamento offline (transferência/comprovante).
                        _context.RecibosPagamento.Add(ReciboPagamento.Emitir(
                            fatura, pgFatura.Id, pagamento.Valor, "Transferencia", null, null, alteradoPor));

                        // Se ainda não ativou a assinatura
                        if (assinaturaId == null)
                        {
                            var assinatura = await _context.AssinaturasClientes
                                .IgnoreQueryFilters()
                                .Where(a => a.ClienteId == fatura.ClienteId && a.DeletadoEm == null && !a.Arquivada)
                                .OrderByDescending(a => a.CriadoEm)
                                .FirstOrDefaultAsync(cancellationToken);

                            if (assinatura != null)
                            {
                                assinatura.Ativar(alteradoPor);
                                assinaturaId = assinatura.Id;

                                var cliente = await _context.Clientes.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == fatura.ClienteId, cancellationToken);
                                if (cliente != null)
                                {
                                    cliente.AlterarPlano(assinatura.PlanoId, alteradoPor);
                                    cliente.AtualizarStatusSaaS(StatusSaaS.Ativo, alteradoPor);
                                }
                            }
                        }
                    }
                }

                // Registra o PagamentoGlobal
                if (assinaturaId.HasValue)
                {
                    var pagamentoGlobal = new PagamentoGlobal(
                        assinaturaId: assinaturaId.Value,
                        pedidoId: pagamento.PedidoId,
                        faturaId: pagamento.FaturaId,
                        dataPagamento: DateTime.UtcNow,
                        valor: pagamento.Valor,
                        gateway: "Transferencia",
                        transactionId: "tx-manual-" + pagamento.Id.ToString("N"),
                        tenantId: pagamento.TenantId,
                        criadoPor: alteradoPor
                    );
                    _context.PagamentosGlobais.Add(pagamentoGlobal);
                }
            }
            else
            {
                pagamento.Rejeitar(operador, request.Justificativa!, alteradoPor);
                if (!pagamento.IsValid)
                {
                    return CommandResult.Falha(pagamento.Notifications.Select(n => n.Message));
                }
                comprovante.MarcarComoLido(alteradoPor);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok($"Comprovante de pagamento analisado com sucesso! Aprovado: {request.Aprovado}", new
            {
                PagamentoTransferenciaId = pagamento.Id,
                Status = pagamento.Status
            });
        }
    }

    public class ProcessarWebhookPagamentoCommandHandler : ICommandHandler<ProcessarWebhookPagamentoCommand>
    {
        private readonly ContextGestaoClientes _context;

        public ProcessarWebhookPagamentoCommandHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ProcessarWebhookPagamentoCommand request, CancellationToken cancellationToken)
        {
            var validator = new ProcessarWebhookPagamentoCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return CommandResult.Falha(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var alteradoPor = "system-webhook";

            // IDEMPOTÊNCIA: Verifica se o PaymentId (ou TransactionId) já existe no banco de dados e está liquidado
            var pagamentoGlobalExistente = await _context.PagamentosGlobais
                .IgnoreQueryFilters()
                .AnyAsync(p => p.TransactionId == request.TransactionId && p.DeletadoEm == null, cancellationToken);

            var pagamentoFaturaExistente = await _context.PagamentosFaturas
                .IgnoreQueryFilters()
                .AnyAsync(p => p.IdentificadorPagamento == request.TransactionId && p.Status == PagamentoFaturaStatus.Paid && p.DeletadoEm == null, cancellationToken);

            if (pagamentoGlobalExistente || pagamentoFaturaExistente)
            {
                // Retorna sucesso de forma idempotente, sem lançar erros
                return CommandResult.Ok("Pagamento já havia sido processado anteriormente (Idempotente).", new { TransactionId = request.TransactionId });
            }

            // Apenas prossegue se o status for aprovado/pago
            if (request.Status.ToLowerInvariant() != "approved" && request.Status.ToLowerInvariant() != "succeeded" && request.Status.ToLowerInvariant() != "paid")
            {
                return CommandResult.Ok("Webhook recebido com status não-liquidado. Nenhuma ação executada.", new { Status = request.Status });
            }

            Guid? assinaturaId = request.AssinaturaId;
            var tenantId = string.Empty;

            // Se veio por FaturaId
            if (request.FaturaId.HasValue)
            {
                var fatura = await _context.Faturas
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(f => f.Id == request.FaturaId.Value && f.DeletadoEm == null, cancellationToken);
                
                if (fatura != null)
                {
                    tenantId = fatura.TenantId;
                    fatura.Baixar(alteradoPor);

                    // Cria ou atualiza o PagamentoFatura
                    var pagFatura = await _context.PagamentosFaturas
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(p => p.FaturaId == fatura.Id && p.DeletadoEm == null, cancellationToken);

                    // 1.08A — caminho INTERNO/simulado (não é o webhook real do MP). A tarifa REAL só é
                    // conhecida consultando o gateway, o que acontece no motor unificado
                    // (GestaoClientes.ProcessarWebhookPagamentoCommandHandler / webhook mercadopago). Aqui NÃO
                    // fabricamos tarifa: registramos tarifa desconhecida (null) → líquido = bruto.
                    if (pagFatura != null)
                    {
                        pagFatura.Liquidar(request.Valor, null, alteradoPor);
                    }
                    else
                    {
                        pagFatura = new PagamentoFatura(
                            faturaId: fatura.Id,
                            tipoPagamento: request.Gateway,
                            status: PagamentoFaturaStatus.Paid,
                            valorPago: request.Valor,
                            valorTarifa: null,
                            identificadorPagamento: request.TransactionId,
                            pagoManualmente: false,
                            dataPagamento: DateTime.UtcNow,
                            tenantId: fatura.TenantId,
                            criadoPor: alteradoPor
                        );
                        _context.PagamentosFaturas.Add(pagFatura);
                    }

                    // 1.08A — recibo de pagamento (documento simples; NFS-e diferida).
                    _context.RecibosPagamento.Add(ReciboPagamento.Emitir(
                        fatura, pagFatura.Id, request.Valor, request.Gateway, null, null, alteradoPor));

                    // Ativa a assinatura
                    var assinatura = await _context.AssinaturasClientes
                        .IgnoreQueryFilters()
                        .Where(a => a.ClienteId == fatura.ClienteId && a.DeletadoEm == null && !a.Arquivada)
                        .OrderByDescending(a => a.CriadoEm)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (assinatura != null)
                    {
                        assinatura.Ativar(alteradoPor);
                        assinaturaId = assinatura.Id;

                        var cliente = await _context.Clientes.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == fatura.ClienteId, cancellationToken);
                        if (cliente != null)
                        {
                            cliente.AlterarPlano(assinatura.PlanoId, alteradoPor);
                            cliente.AtualizarStatusSaaS(StatusSaaS.Ativo, alteradoPor); // §12: awaiting-payment → active
                        }
                    }
                }
            }

            // Se veio por PedidoId
            if (request.PedidoId.HasValue)
            {
                var pedido = await _context.PedidosSaaS
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.Id == request.PedidoId.Value && p.DeletadoEm == null, cancellationToken);

                if (pedido != null)
                {
                    tenantId = pedido.TenantId;

                    var assinatura = await _context.AssinaturasClientes
                        .IgnoreQueryFilters()
                        .Where(a => a.ClienteId == pedido.ClienteId && a.PlanoId == pedido.PlanoId && a.DeletadoEm == null && !a.Arquivada)
                        .OrderByDescending(a => a.CriadoEm)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (assinatura != null)
                    {
                        assinatura.Ativar(alteradoPor);
                        assinaturaId = assinatura.Id;
                        pedido.Liquidar(assinatura.Id, alteradoPor);

                        var cliente = await _context.Clientes.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == pedido.ClienteId, cancellationToken);
                        if (cliente != null)
                        {
                            cliente.AlterarPlano(pedido.PlanoId, alteradoPor);
                            cliente.AtualizarStatusSaaS(StatusSaaS.Ativo, alteradoPor); // §12: awaiting-payment → active
                        }
                    }

                    // Se houver Fatura gerada para o pedido, dá baixa nela também
                    var faturaVinculada = await _context.Faturas
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(f => f.ClienteId == pedido.ClienteId && f.Valor == pedido.ValorTotal && f.Status == FaturaStatus.Pendente && f.DeletadoEm == null, cancellationToken);

                    if (faturaVinculada != null)
                    {
                        faturaVinculada.Baixar(alteradoPor);

                        // 1.08A — tarifa desconhecida no caminho interno (null), NÃO fabricada.
                        var pagFatura = new PagamentoFatura(
                            faturaId: faturaVinculada.Id,
                            tipoPagamento: request.Gateway,
                            status: PagamentoFaturaStatus.Paid,
                            valorPago: request.Valor,
                            valorTarifa: null,
                            identificadorPagamento: request.TransactionId,
                            pagoManualmente: false,
                            dataPagamento: DateTime.UtcNow,
                            tenantId: faturaVinculada.TenantId,
                            criadoPor: alteradoPor
                        );
                        _context.PagamentosFaturas.Add(pagFatura);

                        // 1.08A — recibo de pagamento (documento simples; NFS-e diferida).
                        _context.RecibosPagamento.Add(ReciboPagamento.Emitir(
                            faturaVinculada, pagFatura.Id, request.Valor, request.Gateway, null, null, alteradoPor));
                    }
                }
            }

            // Registra no PagamentoGlobal
            if (assinaturaId.HasValue)
            {
                var pgGlobal = new PagamentoGlobal(
                    assinaturaId: assinaturaId.Value,
                    pedidoId: request.PedidoId,
                    faturaId: request.FaturaId,
                    dataPagamento: DateTime.UtcNow,
                    valor: request.Valor,
                    gateway: request.Gateway,
                    transactionId: request.TransactionId,
                    tenantId: string.IsNullOrEmpty(tenantId) ? "system-tenant" : tenantId,
                    criadoPor: alteradoPor
                );
                _context.PagamentosGlobais.Add(pgGlobal);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Pagamento processado com sucesso via Webhook!", new { TransactionId = request.TransactionId });
        }
    }
}
