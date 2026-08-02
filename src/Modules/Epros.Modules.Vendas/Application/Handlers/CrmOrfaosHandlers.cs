using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Vendas.Application.Handlers
{
    // Write-path das entidades CRM antes órfãs (auditoria VENDAS, CRM P1).

    // ---------- Atividades ----------

    public class CriarCrmAtividadeCommandHandler : ICommandHandler<CriarCrmAtividadeCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCrmAtividadeCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCrmAtividadeCommand r, CancellationToken ct)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var atividade = new CrmAtividade(
                r.EntidadeTipo, r.TipoAtividade, r.LeadId, r.OportunidadeId, r.TicketId, r.CampanhaId,
                r.Nome, r.Assunto, r.Data, r.Hora, r.Prioridade, r.Status, r.TipoChamada, r.Duracao,
                r.Destinatario, r.Descricao, null /*resultado*/, r.Comentario, r.ArquivoNome, r.ArquivoReferencia,
                r.RecorrenciaJson, r.UsuarioId, tenantId, usuario);
            if (!atividade.IsValid) return CommandResult.Falha(atividade.Notifications.Select(n => n.Message), "Dados da atividade inválidos.");
            _context.CrmAtividades.Add(atividade);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Atividade registrada com sucesso!", new { atividade.Id });
        }
    }

    public class ConcluirCrmAtividadeCommandHandler : ICommandHandler<ConcluirCrmAtividadeCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ConcluirCrmAtividadeCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ConcluirCrmAtividadeCommand r, CancellationToken ct)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var atividade = await _context.CrmAtividades.FirstOrDefaultAsync(a => a.Id == r.Id && a.TenantId == tenantId, ct);
            if (atividade == null) return CommandResult.Falha(new[] { "Atividade não encontrada." }, "Erro de validação");
            atividade.Concluir(r.Resultado, usuario);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Atividade concluída!", new { atividade.Id });
        }
    }

    // ---------- Webforms ----------

    public class CriarCrmWebformCommandHandler : ICommandHandler<CriarCrmWebformCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCrmWebformCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCrmWebformCommand r, CancellationToken ct)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // CRM-018: identificador único por tenant.
            var jaExiste = await _context.CrmWebforms.AnyAsync(w => w.TenantId == tenantId && w.IdentificadorUnico == r.IdentificadorUnico, ct);
            if (jaExiste) return CommandResult.Falha(new[] { "Já existe um webform com este identificador." }, "Erro de validação");

            var webform = new CrmWebform(
                r.IdentificadorUnico, r.Titulo, r.EstruturaJson, r.CssCustomizado, r.MensagemAgradecimento, r.TextoBotao,
                r.CampanhaId, r.LeadTituloPadrao, r.LeadStatusPadrao, r.LeadOrigemId, r.UsarCaptcha, r.NotificarAdministrador,
                tenantId, usuario);
            if (!webform.IsValid) return CommandResult.Falha(webform.Notifications.Select(n => n.Message), "Dados do webform inválidos.");
            _context.CrmWebforms.Add(webform);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Webform criado com sucesso!", new { webform.Id });
        }
    }

    public class RegistrarSubmissaoCrmWebformCommandHandler : ICommandHandler<RegistrarSubmissaoCrmWebformCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarSubmissaoCrmWebformCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarSubmissaoCrmWebformCommand r, CancellationToken ct)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var webform = await _context.CrmWebforms.FirstOrDefaultAsync(
                w => w.TenantId == tenantId && w.IdentificadorUnico == r.IdentificadorUnico, ct);
            if (webform == null) return CommandResult.Falha(new[] { "Webform não encontrado." }, "Erro de validação");
            if (!webform.Ativo) return CommandResult.Falha(new[] { "Webform inativo não aceita submissões." }, "Erro de validação");

            // CRM-021: contabiliza a submissão aceita. A verificação de captcha (CRM-019/anti-spam) e a
            // geração automática do Lead a partir da estrutura são wiring de fluxo público. // valida-ambiente
            webform.RegistrarSubmissao(usuario);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Submissão registrada!", new { webform.Id, webform.ContadorSubmissoes });
        }
    }

    // ---------- Fidelização ----------

    public class CriarCrmClienteFidelizadoCommandHandler : ICommandHandler<CriarCrmClienteFidelizadoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCrmClienteFidelizadoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCrmClienteFidelizadoCommand r, CancellationToken ct)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var cliente = new CrmClienteFidelizado(r.ClienteId, r.Nome, r.Email, r.Celular, r.DataAniversario, tenantId, usuario);
            if (!cliente.IsValid) return CommandResult.Falha(cliente.Notifications.Select(n => n.Message), "Dados do cliente fidelizado inválidos.");
            _context.CrmClientesFidelizados.Add(cliente);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Cliente fidelizado cadastrado!", new { cliente.Id });
        }
    }

    public class RegistrarPontuacaoFidelizacaoCommandHandler : ICommandHandler<RegistrarPontuacaoFidelizacaoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarPontuacaoFidelizacaoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarPontuacaoFidelizacaoCommand r, CancellationToken ct)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var cliente = await _context.CrmClientesFidelizados.FirstOrDefaultAsync(
                c => c.Id == r.ClienteFidelizadoId && c.TenantId == tenantId, ct);
            if (cliente == null) return CommandResult.Falha(new[] { "Cliente fidelizado não encontrado." }, "Erro de validação");

            var data = r.Data ?? DateTime.UtcNow;
            var pontuacao = new CrmPontuacaoFidelizacao(cliente.Id, data, r.Pontos, r.Observacao, tenantId, usuario);
            if (!pontuacao.IsValid) return CommandResult.Falha(pontuacao.Notifications.Select(n => n.Message), "Dados da pontuação inválidos.");

            // CRM-067: acumula no saldo do cliente. A REGRA de quantos pontos gerar por real gasto é do
            // cliente (não temos skill de negócio para derivá-la): os pontos chegam prontos no comando. // valida-Siser
            _context.CrmPontuacoesFidelizacao.Add(pontuacao);
            cliente.AcumularPontos(r.Pontos, data, usuario);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Pontuação registrada!", new { pontuacao.Id, cliente.TotalPontos });
        }
    }

    // ---------- Pix relacional ----------

    public class SalvarCrmConfiguracaoPixRelacionalCommandHandler : ICommandHandler<SalvarCrmConfiguracaoPixRelacionalCommand>
    {
        // Máscara devolvida à UI; ao recebê-la de volta, o token NÃO mudou (CRM-071).
        public const string MascaraToken = "••••••••";

        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly ISegredoCofreService _cofreService;

        public SalvarCrmConfiguracaoPixRelacionalCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser, ISegredoCofreService cofreService)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; _cofreService = cofreService;
        }

        public async Task<CommandResult> Handle(SalvarCrmConfiguracaoPixRelacionalCommand r, CancellationToken ct)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // CRM-071: o token é segredo — cifra via cofre quando um novo valor é informado (≠ máscara/vazio);
            // caso contrário preserva o token cifrado atual.
            var tokenInformado = !string.IsNullOrWhiteSpace(r.TokenPix) && r.TokenPix != MascaraToken;

            var config = await _context.CrmConfiguracoesPixRelacional.FirstOrDefaultAsync(c => c.TenantId == tenantId, ct);
            if (config == null)
            {
                var tokenCifrado = tokenInformado ? await _cofreService.CriptografarAsync(r.TokenPix!) : null;
                config = new CrmConfiguracaoPixRelacional(r.LinkApiAppVendas, tokenCifrado, r.CnpjEmpresa, tenantId, usuario);
                _context.CrmConfiguracoesPixRelacional.Add(config);
            }
            else
            {
                var tokenCifrado = tokenInformado ? await _cofreService.CriptografarAsync(r.TokenPix!) : null; // null = preserva (entidade ignora vazio)
                config.Alterar(r.LinkApiAppVendas, tokenCifrado, r.CnpjEmpresa, r.Ativo, usuario);
            }
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Configuração de Pix relacional salva!", new { config.Id });
        }
    }

    public class CriarCrmPagamentoPixRelacionalCommandHandler : ICommandHandler<CriarCrmPagamentoPixRelacionalCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCrmPagamentoPixRelacionalCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCrmPagamentoPixRelacionalCommand r, CancellationToken ct)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var pagamento = new CrmPagamentoPixRelacional(
                r.EntidadeOrigemTipo, r.EntidadeOrigemId, r.Valor, r.QrCode, r.QrCodeImagem, r.ProvedorPagamentoId, tenantId, usuario);
            if (!pagamento.IsValid) return CommandResult.Falha(pagamento.Notifications.Select(n => n.Message), "Dados do pagamento Pix inválidos.");
            _context.CrmPagamentosPixRelacional.Add(pagamento);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Cobrança Pix relacional criada!", new { pagamento.Id, Status = pagamento.Status.ToString() });
        }
    }

    public class AtualizarStatusCrmPagamentoPixCommandHandler : ICommandHandler<AtualizarStatusCrmPagamentoPixCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtualizarStatusCrmPagamentoPixCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarStatusCrmPagamentoPixCommand r, CancellationToken ct)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var pagamento = await _context.CrmPagamentosPixRelacional.FirstOrDefaultAsync(p => p.Id == r.Id && p.TenantId == tenantId, ct);
            if (pagamento == null) return CommandResult.Falha(new[] { "Pagamento Pix não encontrado." }, "Erro de validação");
            pagamento.AtualizarStatus(r.Status, r.ProvedorPagamentoId, usuario);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Status do pagamento Pix atualizado!", new { pagamento.Id, Status = pagamento.Status.ToString() });
        }
    }
}
