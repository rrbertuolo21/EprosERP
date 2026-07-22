using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Handlers
{
    // ----- BANCO HANDLERS -----
    public class CriarBancoCommandHandler : IRequestHandler<CriarBancoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public CriarBancoCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarBancoCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var banco = new Banco(request.Codigo, request.Descricao, userId);

            if (!banco.IsValid)
                return CommandResult.Falha(banco.Notifications.Select(n => n.Message));

            _context.Bancos.Add(banco);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Banco cadastrado com sucesso.", new { banco.Id });
        }
    }

    public class AtualizarBancoCommandHandler : IRequestHandler<AtualizarBancoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarBancoCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarBancoCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var banco = await _context.Bancos.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (banco == null)
                return CommandResult.Falha("Banco não encontrado.");

            banco.MarcarAlterado(userId);
            banco.Alterar(request.Codigo, request.Descricao, userId);

            if (!banco.IsValid)
                return CommandResult.Falha(banco.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Banco atualizado com sucesso.");
        }
    }

    public class DeletarBancoCommandHandler : IRequestHandler<DeletarBancoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public DeletarBancoCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarBancoCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var banco = await _context.Bancos.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (banco == null)
                return CommandResult.Falha("Banco não encontrado.");

            banco.Deletar(userId);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Banco excluído logicamente com sucesso.");
        }
    }


    // ----- CONTA BANCARIA HANDLERS -----
    public class CriarContaBancariaCommandHandler : IRequestHandler<CriarContaBancariaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarContaBancariaCommandHandler(
            ContextFinanceiro context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarContaBancariaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var conta = new ContaBancaria(
                request.EmpresaId,
                request.BancoId,
                request.TipoContaBancaria,
                request.Apelido,
                request.Titular,
                request.Agencia,
                request.Conta,
                request.Gerente,
                request.FoneGerente,
                request.Detalhe,
                request.DigitoAgencia,
                request.DataEncerramento,
                tenantId,
                userId
            );

            if (!conta.IsValid)
                return CommandResult.Falha(conta.Notifications.Select(n => n.Message));

            _context.ContasBancarias.Add(conta);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Conta bancária cadastrada com sucesso.", new { conta.Id });
        }
    }

    public class AtualizarContaBancariaCommandHandler : IRequestHandler<AtualizarContaBancariaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarContaBancariaCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarContaBancariaCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var conta = await _context.ContasBancarias.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (conta == null)
                return CommandResult.Falha("Conta bancária não encontrada.");

            conta.Alterar(
                request.EmpresaId,
                request.BancoId,
                request.TipoContaBancaria,
                request.Apelido,
                request.Titular,
                request.Agencia,
                request.Conta,
                request.Gerente,
                request.FoneGerente,
                request.Detalhe,
                request.DigitoAgencia,
                request.DataEncerramento,
                userId
            );

            if (!conta.IsValid)
                return CommandResult.Falha(conta.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Conta bancária atualizada com sucesso.");
        }
    }

    public class DeletarContaBancariaCommandHandler : IRequestHandler<DeletarContaBancariaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public DeletarContaBancariaCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarContaBancariaCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var conta = await _context.ContasBancarias.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (conta == null)
                return CommandResult.Falha("Conta bancária não encontrada.");

            conta.Deletar(userId);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Conta bancária excluída logicamente.");
        }
    }


    // ----- CARTAO DE CREDITO HANDLERS -----
    public class CriarCartaoDeCreditoCommandHandler : IRequestHandler<CriarCartaoDeCreditoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCartaoDeCreditoCommandHandler(
            ContextFinanceiro context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCartaoDeCreditoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var cartao = new CartaoDeCredito(
                request.ContaBancariaId,
                request.Apelido,
                request.Titular,
                request.BandeiraCartao,
                request.Observacao,
                tenantId,
                userId
            );

            if (!cartao.IsValid)
                return CommandResult.Falha(cartao.Notifications.Select(n => n.Message));

            _context.CartoesDeCredito.Add(cartao);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Cartão de crédito cadastrado com sucesso.", new { cartao.Id });
        }
    }

    public class AtualizarCartaoDeCreditoCommandHandler : IRequestHandler<AtualizarCartaoDeCreditoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarCartaoDeCreditoCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarCartaoDeCreditoCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var cartao = await _context.CartoesDeCredito.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (cartao == null)
                return CommandResult.Falha("Cartão de crédito não encontrado.");

            cartao.Alterar(
                request.ContaBancariaId,
                request.Apelido,
                request.Titular,
                request.BandeiraCartao,
                request.Observacao,
                userId
            );

            if (!cartao.IsValid)
                return CommandResult.Falha(cartao.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Cartão de crédito atualizado com sucesso.");
        }
    }

    public class DeletarCartaoDeCreditoCommandHandler : IRequestHandler<DeletarCartaoDeCreditoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public DeletarCartaoDeCreditoCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarCartaoDeCreditoCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var cartao = await _context.CartoesDeCredito.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (cartao == null)
                return CommandResult.Falha("Cartão de crédito não encontrado.");

            cartao.Deletar(userId);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Cartão de crédito excluído logicamente.");
        }
    }


    // ----- FATURA CARTAO HANDLERS -----
    public class LancarFaturaCartaoCommandHandler : IRequestHandler<LancarFaturaCartaoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public LancarFaturaCartaoCommandHandler(
            ContextFinanceiro context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(LancarFaturaCartaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var fatura = new CartaoDeCreditoFatura(
                request.CartaoDeCreditoId,
                request.DataLancamento,
                request.DataVencimento,
                request.Valor,
                request.Pago,
                tenantId,
                userId
            );

            if (!fatura.IsValid)
                return CommandResult.Falha(fatura.Notifications.Select(n => n.Message));

            _context.CartoesDeCreditoFaturas.Add(fatura);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Fatura lançada com sucesso.", new { fatura.Id });
        }
    }

    public class BaixarFaturaCartaoCommandHandler : IRequestHandler<BaixarFaturaCartaoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public BaixarFaturaCartaoCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(BaixarFaturaCartaoCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var fatura = await _context.CartoesDeCreditoFaturas.FirstOrDefaultAsync(x => x.Id == request.FaturaId, cancellationToken);

            if (fatura == null)
                return CommandResult.Falha("Fatura não encontrada.");

            fatura.Alterar(
                fatura.CartaoDeCreditoId,
                fatura.DataLancamento,
                fatura.DataVencimento,
                fatura.Valor,
                true, // Pago
                userId
            );

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Fatura baixada com sucesso.");
        }
    }


    // ----- OFX PARSER & RECONCILIATION HANDLER -----
    public class ProcessarExtratoOfxCommandHandler : IRequestHandler<ProcessarExtratoOfxCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ProcessarExtratoOfxCommandHandler(
            ContextFinanceiro context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ProcessarExtratoOfxCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var contaBancaria = await _context.ContasBancarias
                .FirstOrDefaultAsync(x => x.Id == request.ContaBancariaId, cancellationToken);

            if (contaBancaria == null)
                return CommandResult.Falha("Conta bancária não encontrada.");

            // Parser de OFX via Regex
            var matches = Regex.Matches(request.OfxConteudo, @"<STMTTRN>([\s\S]*?)</STMTTRN>");
            int reconciledCount = 0;
            int totalCount = 0;

            var resultsList = new List<object>();

            foreach (Match m in matches)
            {
                var block = m.Groups[1].Value;
                var type = Regex.Match(block, @"<TRNTYPE>([^<\r\n]+)").Groups[1].Value.Trim();
                var dateStr = Regex.Match(block, @"<DTPOSTED>([^<\r\n]+)").Groups[1].Value.Trim();
                var amountStr = Regex.Match(block, @"<TRNAMT>([^<\r\n]+)").Groups[1].Value.Trim().Replace(",", ".");
                var fitid = Regex.Match(block, @"<FITID>([^<\r\n]+)").Groups[1].Value.Trim();
                var memo = Regex.Match(block, @"<MEMO>([^<\r\n]+)").Groups[1].Value.Trim();

                if (decimal.TryParse(amountStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var amount))
                {
                    totalCount++;
                    var date = DateTime.Today;
                    if (dateStr.Length >= 8)
                    {
                        if (int.TryParse(dateStr.Substring(0, 4), out var year) &&
                            int.TryParse(dateStr.Substring(4, 2), out var month) &&
                            int.TryParse(dateStr.Substring(6, 2), out var day))
                        {
                            date = new DateTime(year, month, day);
                        }
                    }

                    bool reconciled = false;
                    string targetName = "";

                    if (amount > 0)
                    {
                        // Crédito -> concilia um título a receber (fiel) em aberto com valor correspondente.
                        var matchingCR = await _context.ContasAReceberAgregado
                            .Where(c => c.Situacao == ESituacao.Aberto && c.ValorTitulo == amount)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (matchingCR != null)
                        {
                            matchingCR.BaixarTituloConciliado(amount);

                            reconciled = true;
                            reconciledCount++;
                            targetName = matchingCR.NomePessoa ?? matchingCR.Documento;
                        }
                    }
                    else
                    {
                        // Débito -> concilia um título a pagar (fiel) em aberto com o valor absoluto.
                        var absoluteAmount = Math.Abs(amount);
                        var matchingCP = await _context.ContasAPagarAgregado
                            .Where(c => c.Situacao == ESituacao.Aberto && c.ValorTitulo == absoluteAmount)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (matchingCP != null)
                        {
                            matchingCP.BaixarTituloConciliado(absoluteAmount);

                            reconciled = true;
                            reconciledCount++;
                            targetName = matchingCP.NomePessoa ?? matchingCP.Documento;
                        }
                    }

                    resultsList.Add(new
                    {
                        FitId = fitid,
                        Data = date,
                        Memo = memo,
                        Valor = amount,
                        Conciliado = reconciled,
                        Descricao = targetName
                    });
                }
            }

            if (reconciledCount > 0)
            {
                try
                {
                    await _context.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var debugInfo = string.Join("\n", _context.ChangeTracker.Entries()
                        .Select(e => $"Entity: {e.Entity.GetType().Name}, State: {e.State}, PK: {e.Property("Id").CurrentValue}"));
                    throw new Exception($"Concurrency error on save. Tracked entries state:\n{debugInfo}", ex);
                }
            }

            return CommandResult.Ok($"Processamento do OFX concluído. {reconciledCount} de {totalCount} transações foram conciliadas automaticamente.", new
            {
                Total = totalCount,
                Conciliados = reconciledCount,
                Itens = resultsList
            });
        }
    }
}
