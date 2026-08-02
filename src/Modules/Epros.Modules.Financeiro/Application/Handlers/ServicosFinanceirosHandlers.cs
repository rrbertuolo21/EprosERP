using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Modules.Financeiro.Domain.Services;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Handlers
{
    // ===== Configuração Cedente =====
    public class CriarConfiguracaoCedenteCommandHandler : IRequestHandler<CriarConfiguracaoCedenteCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ITenantProvider _tenant; private readonly ICurrentUser _user;
        public CriarConfiguracaoCedenteCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user) { _context = context; _tenant = tenant; _user = user; }
        public async Task<CommandResult> Handle(CriarConfiguracaoCedenteCommand r, CancellationToken ct)
        {
            var e = new ConfiguracaoCedente(r.EmpresaId, r.Nome, r.Email, r.Documento, r.Endereco, r.Numero, r.Bairro, r.Cidade, r.Cep, r.UF, r.Logo,
                r.ReceberAteDias, r.DiasAntecedencia, r.MultaAtraso, r.Juro, r.Instrucao1, r.Instrucao2, r.Instrucao3, r.Instrucao4, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!e.IsValid) return CommandResult.Falha(e.Notifications.Select(n => n.Message));
            _context.ConfiguracoesCedente.Add(e);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Configuração do cedente cadastrada.", new { e.Id });
        }
    }

    public class AtualizarConfiguracaoCedenteCommandHandler : IRequestHandler<AtualizarConfiguracaoCedenteCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ICurrentUser _user;
        public AtualizarConfiguracaoCedenteCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(AtualizarConfiguracaoCedenteCommand r, CancellationToken ct)
        {
            var e = await _context.ConfiguracoesCedente.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
            if (e == null) return CommandResult.Falha("Configuração do cedente não encontrada.");
            e.Alterar(r.Nome, r.Email, r.Documento, r.Endereco, r.Numero, r.Bairro, r.Cidade, r.Cep, r.UF, r.Logo,
                r.ReceberAteDias, r.DiasAntecedencia, r.MultaAtraso, r.Juro, r.Instrucao1, r.Instrucao2, r.Instrucao3, r.Instrucao4, _user.GetUserId() ?? "system");
            if (!e.IsValid) return CommandResult.Falha(e.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Configuração do cedente atualizada.", new { e.Id });
        }
    }

    // ===== Conta Emissora =====
    public class CriarContaEmissoraCommandHandler : IRequestHandler<CriarContaEmissoraCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ITenantProvider _tenant; private readonly ICurrentUser _user;
        public CriarContaEmissoraCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user) { _context = context; _tenant = tenant; _user = user; }
        public async Task<CommandResult> Handle(CriarContaEmissoraCommand r, CancellationToken ct)
        {
            var e = new ContaEmissora(r.BancoId, r.ConfiguracaoCedenteId, r.NomeBanco, r.Carteira, r.Agencia, r.DigitoAgencia, r.Conta, r.DigitoConta, r.Especie,
                r.NossoNumeroAtual, r.TipoCobranca, r.Convenio, r.Contrato, r.TipoCarteira, r.IncrementoNossoNumero, r.TipoRemessa, r.CodigoCliente, r.Posto, r.Ativa, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!e.IsValid) return CommandResult.Falha(e.Notifications.Select(n => n.Message));
            _context.ContasEmissoras.Add(e);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Conta emissora cadastrada.", new { e.Id });
        }
    }

    public class AtualizarContaEmissoraCommandHandler : IRequestHandler<AtualizarContaEmissoraCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ICurrentUser _user;
        public AtualizarContaEmissoraCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(AtualizarContaEmissoraCommand r, CancellationToken ct)
        {
            var e = await _context.ContasEmissoras.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
            if (e == null) return CommandResult.Falha("Conta emissora não encontrada.");
            e.Alterar(r.BancoId, r.ConfiguracaoCedenteId, r.NomeBanco, r.Carteira, r.Agencia, r.DigitoAgencia, r.Conta, r.DigitoConta, r.Especie,
                r.TipoCobranca, r.Convenio, r.Contrato, r.TipoCarteira, r.IncrementoNossoNumero, r.TipoRemessa, r.CodigoCliente, r.Posto, r.Ativa, _user.GetUserId() ?? "system");
            if (!e.IsValid) return CommandResult.Falha(e.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Conta emissora atualizada.", new { e.Id });
        }
    }

    public class AtivarContaEmissoraCommandHandler : IRequestHandler<AtivarContaEmissoraCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ICurrentUser _user;
        public AtivarContaEmissoraCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(AtivarContaEmissoraCommand r, CancellationToken ct)
        {
            var alvo = await _context.ContasEmissoras.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
            if (alvo == null) return CommandResult.Falha("Conta emissora não encontrada.");
            // RSF-003: apenas uma conta emissora ativa por contexto — desativa as demais.
            var outras = await _context.ContasEmissoras.Where(x => x.Ativa && x.Id != r.Id).ToListAsync(ct);
            foreach (var o in outras) o.Desativar(_user.GetUserId() ?? "system");
            alvo.Ativar(_user.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Conta emissora ativada.", new { alvo.Id });
        }
    }

    // ===== Grupo de Recorrência =====
    public class CriarGrupoRecorrenciaCommandHandler : IRequestHandler<CriarGrupoRecorrenciaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ITenantProvider _tenant; private readonly ICurrentUser _user;
        public CriarGrupoRecorrenciaCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user) { _context = context; _tenant = tenant; _user = user; }
        public async Task<CommandResult> Handle(CriarGrupoRecorrenciaCommand r, CancellationToken ct)
        {
            var e = new GrupoRecorrencia(r.Descricao, r.Meses, r.DiaVencimento, r.Valor, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!e.IsValid) return CommandResult.Falha(e.Notifications.Select(n => n.Message));
            _context.GruposRecorrencia.Add(e);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Grupo de recorrência cadastrado.", new { e.Id });
        }
    }

    public class AtualizarGrupoRecorrenciaCommandHandler : IRequestHandler<AtualizarGrupoRecorrenciaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ICurrentUser _user;
        public AtualizarGrupoRecorrenciaCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(AtualizarGrupoRecorrenciaCommand r, CancellationToken ct)
        {
            var e = await _context.GruposRecorrencia.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
            if (e == null) return CommandResult.Falha("Grupo de recorrência não encontrado.");
            e.Alterar(r.Descricao, r.Meses, r.DiaVencimento, r.Valor, _user.GetUserId() ?? "system");
            if (!e.IsValid) return CommandResult.Falha(e.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Grupo de recorrência atualizado.", new { e.Id });
        }
    }

    // ===== Sacado =====
    public class CriarSacadoCommandHandler : IRequestHandler<CriarSacadoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ITenantProvider _tenant; private readonly ICurrentUser _user;
        public CriarSacadoCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user) { _context = context; _tenant = tenant; _user = user; }
        public async Task<CommandResult> Handle(CriarSacadoCommand r, CancellationToken ct)
        {
            var e = new Sacado(r.PessoaId, r.GrupoRecorrenciaId, r.Nome, r.Documento, r.RG, r.Inscricao, r.Endereco, r.Numero, r.Complemento,
                r.Bairro, r.Cidade, r.UF, r.CEP, r.Telefone, r.Email, r.Observacao, r.Valor, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!e.IsValid) return CommandResult.Falha(e.Notifications.Select(n => n.Message));
            _context.Sacados.Add(e);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Sacado cadastrado.", new { e.Id });
        }
    }

    public class AtualizarSacadoCommandHandler : IRequestHandler<AtualizarSacadoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ICurrentUser _user;
        public AtualizarSacadoCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(AtualizarSacadoCommand r, CancellationToken ct)
        {
            var e = await _context.Sacados.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
            if (e == null) return CommandResult.Falha("Sacado não encontrado.");
            e.Alterar(r.PessoaId, r.GrupoRecorrenciaId, r.Nome, r.Documento, r.RG, r.Inscricao, r.Endereco, r.Numero, r.Complemento,
                r.Bairro, r.Cidade, r.UF, r.CEP, r.Telefone, r.Email, r.Observacao, r.Valor, _user.GetUserId() ?? "system");
            if (!e.IsValid) return CommandResult.Falha(e.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Sacado atualizado.", new { e.Id });
        }
    }

    public class BloquearSacadoCommandHandler : IRequestHandler<BloquearSacadoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ICurrentUser _user;
        public BloquearSacadoCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(BloquearSacadoCommand r, CancellationToken ct)
        {
            var e = await _context.Sacados.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
            if (e == null) return CommandResult.Falha("Sacado não encontrado.");
            if (r.Bloquear) e.Bloquear(_user.GetUserId() ?? "system"); else e.Desbloquear(_user.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok(r.Bloquear ? "Sacado bloqueado." : "Sacado desbloqueado.", new { e.Id });
        }
    }

    // ===== Fatura de Cobrança =====
    public class CriarFaturaCobrancaCommandHandler : IRequestHandler<CriarFaturaCobrancaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ITenantProvider _tenant; private readonly ICurrentUser _user;
        public CriarFaturaCobrancaCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user) { _context = context; _tenant = tenant; _user = user; }
        public async Task<CommandResult> Handle(CriarFaturaCobrancaCommand r, CancellationToken ct)
        {
            // RSF-019: sacado bloqueado não deve receber nova fatura.
            var sacado = await _context.Sacados.FirstOrDefaultAsync(s => s.Id == r.SacadoId, ct);
            if (sacado == null) return CommandResult.Falha("Sacado não encontrado.");
            if (sacado.Bloqueado) return CommandResult.Falha("Sacado bloqueado não pode receber nova fatura.");
            var e = new FaturaCobranca(r.SacadoId, r.GrupoRecorrenciaId, r.Referencia, r.NumeroDocumento, r.Data, r.DataVencimento, r.Valor, r.Email, r.TipoFatura, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!e.IsValid) return CommandResult.Falha(e.Notifications.Select(n => n.Message));
            _context.FaturasCobranca.Add(e);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Fatura de cobrança criada (Pendente).", new { e.Id });
        }
    }

    public class BaixarFaturaCobrancaCommandHandler : IRequestHandler<BaixarFaturaCobrancaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ICurrentUser _user;
        public BaixarFaturaCobrancaCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(BaixarFaturaCobrancaCommand r, CancellationToken ct)
        {
            var e = await _context.FaturasCobranca.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
            if (e == null) return CommandResult.Falha("Fatura de cobrança não encontrada.");
            e.Baixar(r.DataBaixa, r.ValorRecebido, _user.GetUserId() ?? "system");
            if (!e.IsValid) return CommandResult.Falha(e.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Fatura baixada.", new { e.Id });
        }
    }

    // ===== Boleto =====
    public class EmitirBoletoCommandHandler : IRequestHandler<EmitirBoletoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ITenantProvider _tenant; private readonly ICurrentUser _user;
        public EmitirBoletoCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user) { _context = context; _tenant = tenant; _user = user; }
        public async Task<CommandResult> Handle(EmitirBoletoCommand r, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            var fatura = await _context.FaturasCobranca.FirstOrDefaultAsync(f => f.Id == r.FaturaCobrancaId, ct);
            if (fatura == null) return CommandResult.Falha("Fatura de cobrança não encontrada.");
            if (!fatura.ElegivelBoleto) return CommandResult.Falha("Fatura baixada não é elegível para boleto."); // RSF-023
            var conta = await _context.ContasEmissoras.FirstOrDefaultAsync(c => c.Id == r.ContaEmissoraId, ct);
            if (conta == null) return CommandResult.Falha("Conta emissora não encontrada.");
            var nossoNumero = conta.GerarProximoNossoNumero(userId); // sequencial da conta emissora
            var boleto = new Boleto(r.FaturaCobrancaId, r.ContaEmissoraId, nossoNumero, r.NumeroDocumento, r.Valor, r.DataVencimento,
                r.LinhaDigitavel, r.Arquivo, r.Multa, r.Juros, r.Instrucao1, r.Instrucao2, r.Instrucao3, r.Instrucao4, _tenant.GetTenantId(), userId);
            if (!boleto.IsValid) return CommandResult.Falha(boleto.Notifications.Select(n => n.Message));
            fatura.AtribuirBoleto(nossoNumero, conta.BancoId, userId); // RSF-024
            _context.Boletos.Add(boleto);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Boleto emitido.", new { boleto.Id, nossoNumero });
        }
    }

    // ===== Remessa =====
    public class GerarRemessaCommandHandler : IRequestHandler<GerarRemessaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ITenantProvider _tenant; private readonly ICurrentUser _user;
        public GerarRemessaCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user) { _context = context; _tenant = tenant; _user = user; }
        public async Task<CommandResult> Handle(GerarRemessaCommand r, CancellationToken ct)
        {
            var e = new Remessa(r.NomeArquivo, r.DataGeracao, r.Grupo, r.Layout, r.ContaEmissoraId, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!e.IsValid) return CommandResult.Falha(e.Notifications.Select(n => n.Message));
            _context.Remessas.Add(e);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Remessa gerada.", new { e.Id });
        }
    }

    public class AdicionarBoletoRemessaCommandHandler : IRequestHandler<AdicionarBoletoRemessaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ICurrentUser _user;
        public AdicionarBoletoRemessaCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(AdicionarBoletoRemessaCommand r, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            var remessa = await _context.Remessas.Include(x => x.Boletos).FirstOrDefaultAsync(x => x.Id == r.RemessaId, ct);
            if (remessa == null) return CommandResult.Falha("Remessa não encontrada.");
            var boleto = await _context.Boletos.FirstOrDefaultAsync(b => b.Id == r.BoletoId, ct);
            if (boleto == null) return CommandResult.Falha("Boleto não encontrado.");
            var fatura = await _context.FaturasCobranca.FirstOrDefaultAsync(f => f.Id == boleto.FaturaCobrancaId, ct);
            if (fatura == null) return CommandResult.Falha("Fatura do boleto não encontrada.");
            if (!fatura.ElegivelRemessa) return CommandResult.Falha("Fatura não elegível para remessa (baixada ou já remetida)."); // RSF-013
            remessa.AdicionarBoleto(boleto.Id, fatura.Id, boleto.Valor, userId);
            fatura.MarcarRemetida(userId); // RSF-014
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Boleto adicionado à remessa.", new { RemessaId = remessa.Id, BoletoId = boleto.Id });
        }
    }

    // ===== Cobrança por E-mail =====
    public class CriarCobrancaEmailCommandHandler : IRequestHandler<CriarCobrancaEmailCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ITenantProvider _tenant; private readonly ICurrentUser _user;
        public CriarCobrancaEmailCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user) { _context = context; _tenant = tenant; _user = user; }
        public async Task<CommandResult> Handle(CriarCobrancaEmailCommand r, CancellationToken ct)
        {
            var e = new CobrancaEmail(r.SacadoId, r.Nome, r.Valor, r.Periodo, r.Servicos, r.Conta, r.LinkExterno, r.Observacao, r.Emails, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!e.IsValid) return CommandResult.Falha(e.Notifications.Select(n => n.Message));
            _context.CobrancasEmail.Add(e);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Cobrança por e-mail criada (Encubada).", new { e.Id });
        }
    }

    public class TransicionarCobrancaEmailCommandHandler : IRequestHandler<TransicionarCobrancaEmailCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ICurrentUser _user;
        public TransicionarCobrancaEmailCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(TransicionarCobrancaEmailCommand r, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            var e = await _context.CobrancasEmail.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
            if (e == null) return CommandResult.Falha("Cobrança por e-mail não encontrada.");
            switch ((r.Acao ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "enviar": e.EnviarPrimeiraCobranca(userId); break;
                case "recobrar": e.Recobrar(userId); break;
                case "inadimplente": e.MarcarInadimplente(userId); break;
                case "confirmar": e.ConfirmarPagamento(r.Comprovante ?? string.Empty, userId); break;
                case "validar": e.ValidarPagamento(userId); break;
                case "auto": e.DefinirArea(EAreaCobrancaEmail.Auto, userId); break;
                case "noauto": e.DefinirArea(EAreaCobrancaEmail.NoAuto, userId); break;
                default: return CommandResult.Falha("Ação de cobrança por e-mail inválida.");
            }
            if (!e.IsValid) return CommandResult.Falha(e.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Cobrança por e-mail atualizada.", new { e.Id, e.Status });
        }
    }

    // ===== Gateway de Pagamento =====
    public class RegistrarGatewayPagamentoCommandHandler : IRequestHandler<RegistrarGatewayPagamentoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ITenantProvider _tenant; private readonly ICurrentUser _user;
        public RegistrarGatewayPagamentoCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user) { _context = context; _tenant = tenant; _user = user; }
        public async Task<CommandResult> Handle(RegistrarGatewayPagamentoCommand r, CancellationToken ct)
        {
            var e = new GatewayPagamento(r.Provedor, r.Nome, r.ChaveAssinatura, r.IdentificadorExterno, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!e.IsValid) return CommandResult.Falha(e.Notifications.Select(n => n.Message));
            _context.GatewaysPagamento.Add(e);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Gateway de pagamento registrado.", new { e.Id, Provedor = e.Provedor.ToString() });
        }
    }

    public class AlterarGatewayPagamentoCommandHandler : IRequestHandler<AlterarGatewayPagamentoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ICurrentUser _user;
        public AlterarGatewayPagamentoCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(AlterarGatewayPagamentoCommand r, CancellationToken ct)
        {
            var e = await _context.GatewaysPagamento.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
            if (e == null) return CommandResult.Falha("Gateway de pagamento não encontrado.");
            e.Alterar(r.Nome, r.ChaveAssinatura, r.IdentificadorExterno, _user.GetUserId() ?? "system");
            if (!e.IsValid) return CommandResult.Falha(e.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Gateway de pagamento atualizado.", new { e.Id });
        }
    }

    public class AtivarGatewayPagamentoCommandHandler : IRequestHandler<AtivarGatewayPagamentoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ICurrentUser _user;
        public AtivarGatewayPagamentoCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(AtivarGatewayPagamentoCommand r, CancellationToken ct)
        {
            var e = await _context.GatewaysPagamento.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
            if (e == null) return CommandResult.Falha("Gateway de pagamento não encontrado.");
            if (r.Ativar) e.Ativar(_user.GetUserId() ?? "system"); else e.Desativar(_user.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok(r.Ativar ? "Gateway ativado." : "Gateway desativado.", new { e.Id, e.Ativo });
        }
    }

    // ===== Webhook de Pagamento (validação de assinatura + idempotência/dedup + baixa por nosso número) =====
    public class ProcessarWebhookPagamentoCommandHandler : IRequestHandler<ProcessarWebhookPagamentoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ITenantProvider _tenant; private readonly ICurrentUser _user;
        public ProcessarWebhookPagamentoCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user) { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(ProcessarWebhookPagamentoCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";

            var gateway = await _context.GatewaysPagamento.FirstOrDefaultAsync(g => g.Id == r.GatewayPagamentoId, ct);
            if (gateway == null) return CommandResult.Falha("Gateway de pagamento não encontrado.");
            if (!gateway.Ativo) return CommandResult.Falha("Gateway de pagamento inativo.");

            // Idempotência/dedup: mesmo (gateway x id de evento) não processa duas vezes (retorno idempotente).
            var jaRecebido = await _context.WebhooksPagamento
                .FirstOrDefaultAsync(w => w.GatewayPagamentoId == r.GatewayPagamentoId && w.EventoExternoId == r.EventoExternoId, ct);
            if (jaRecebido != null)
                return CommandResult.Ok("Webhook já processado (idempotente).", new
                {
                    jaRecebido.Id, Status = jaRecebido.Status.ToString(), Duplicado = true, jaRecebido.FaturaCobrancaId
                });

            var registro = new WebhookPagamentoRecebido(r.GatewayPagamentoId, r.EventoExternoId, r.TipoEvento,
                r.NossoNumero, r.Valor, r.DataPagamento, tenantId, userId);
            if (!registro.IsValid) return CommandResult.Falha(registro.Notifications.Select(n => n.Message));
            _context.WebhooksPagamento.Add(registro);

            // Validação de assinatura (HMAC-SHA256). Esquema exato do provedor real = // valida-ambiente.
            if (!ValidadorAssinaturaWebhook.Conferir(r.PayloadRaw ?? string.Empty, gateway.ChaveAssinatura, r.Assinatura))
            {
                registro.MarcarFalha(EStatusWebhookPagamento.AssinaturaInvalida, "Assinatura HMAC não confere.", userId);
                await _context.SaveChangesAsync(ct);
                return CommandResult.Falha("Assinatura do webhook inválida.");
            }

            // Efeito: baixa da fatura pelo nosso número (RSF-007), mesmo caminho do retorno CNAB.
            if (!r.NossoNumero.HasValue)
            {
                registro.MarcarFalha(EStatusWebhookPagamento.Ignorado, "Webhook sem nosso número — nada a baixar.", userId);
                await _context.SaveChangesAsync(ct);
                return CommandResult.Ok("Webhook recebido, sem nosso número para baixa.", new { registro.Id, Status = registro.Status.ToString() });
            }

            var fatura = await _context.FaturasCobranca.FirstOrDefaultAsync(f => f.NossoNumero == r.NossoNumero.Value, ct);
            if (fatura == null)
            {
                registro.MarcarFalha(EStatusWebhookPagamento.FaturaNaoLocalizada, $"Fatura com nosso número {r.NossoNumero} não localizada.", userId);
                await _context.SaveChangesAsync(ct);
                return CommandResult.Falha($"Fatura com nosso número {r.NossoNumero} não localizada.");
            }
            if (fatura.Situacao == ESituacaoFaturaCobranca.Baixada)
            {
                // Já baixada (por CNAB ou webhook anterior): idempotente, não reprocessa a baixa.
                registro.MarcarProcessado(fatura.Id, userId);
                await _context.SaveChangesAsync(ct);
                return CommandResult.Ok("Fatura já estava baixada (idempotente).", new { registro.Id, FaturaId = fatura.Id, Status = registro.Status.ToString() });
            }

            var valorRecebido = r.Valor ?? fatura.Valor;
            fatura.Baixar(r.DataPagamento ?? DateTime.UtcNow, valorRecebido, userId);
            if (!fatura.IsValid)
            {
                registro.MarcarFalha(EStatusWebhookPagamento.Ignorado, string.Join("; ", fatura.Notifications.Select(n => n.Message)), userId);
                await _context.SaveChangesAsync(ct);
                return CommandResult.Falha(fatura.Notifications.Select(n => n.Message));
            }

            registro.MarcarProcessado(fatura.Id, userId);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Financeiro.WebhookPagamentoProcessado,
                JsonSerializer.Serialize(new
                {
                    webhookId = registro.Id, gatewayId = gateway.Id, provedor = gateway.Provedor.ToString(),
                    faturaId = fatura.Id, nossoNumero = r.NossoNumero, valorRecebido, tenantId
                })));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Webhook processado; fatura baixada.", new
            {
                registro.Id, FaturaId = fatura.Id, NossoNumero = r.NossoNumero, valorRecebido, Status = registro.Status.ToString()
            });
        }
    }
}
