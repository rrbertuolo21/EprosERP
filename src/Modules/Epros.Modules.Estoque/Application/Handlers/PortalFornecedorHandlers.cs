using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>
    /// EST-PFO — Portal do Fornecedor. Convite/acesso, publicação e resposta de cotação, pré-aviso (ASN) e
    /// upload de documentos. Isolamento por fornecedor (PFO-002) é obrigatório em toda operação — os handlers
    /// que agem "como fornecedor" exigem que o FornecedorId do comando bata com o dado referenciado.
    /// Integração com COMPRAS/LDE/GED por evento (est.pfo.*). Auth externa reusa Portal do Cliente (D6) —
    /// wiring de login externo fica no host (nota em DECISOES-PENDENTES-RAFAEL).
    /// </summary>
    public class ConvidarFornecedorCommandHandler : ICommandHandler<ConvidarFornecedorCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ConvidarFornecedorCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(ConvidarFornecedorCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var convite = new PfoConviteFornecedor(request.FornecedorId, request.EmailConvite, request.DataExpiracao, tenantId, usuario);
            if (!convite.IsValid)
                return CommandResult.Falha(convite.Notifications.Select(n => n.Message), "Dados do convite são inválidos.");

            convite.Enviar(usuario);
            _context.PfoConvitesFornecedor.Add(convite);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.PfoConviteEnviado,
                JsonSerializer.Serialize(new { convite.Id, convite.FornecedorId, convite.EmailConvite, tenantId, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Convite enviado ao fornecedor.", new { convite.Id });
        }
    }

    public class AtivarAcessoFornecedorCommandHandler : ICommandHandler<AtivarAcessoFornecedorCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtivarAcessoFornecedorCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AtivarAcessoFornecedorCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var convite = await _context.PfoConvitesFornecedor.FirstOrDefaultAsync(c => c.Id == request.ConviteId && c.DeletadoEm == null, cancellationToken);
            if (convite == null) return CommandResult.Falha("Convite não encontrado.");
            if (!convite.PodeAceitar()) return CommandResult.Falha("Convite não está em estado de aceite.");

            convite.Aceitar(usuario);
            var acesso = new PfoUsuarioFornecedor(convite.FornecedorId, request.UsuarioId, tenantId, usuario);
            if (!acesso.IsValid)
                return CommandResult.Falha(acesso.Notifications.Select(n => n.Message), "Dados do acesso são inválidos.");
            _context.PfoUsuariosFornecedor.Add(acesso);

            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.PfoAcessoAtivado,
                JsonSerializer.Serialize(new { convite.FornecedorId, request.UsuarioId, tenantId })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Acesso do fornecedor ativado.", new { acessoId = acesso.Id, convite.FornecedorId });
        }
    }

    public class PublicarCotacaoFornecedorCommandHandler : ICommandHandler<PublicarCotacaoFornecedorCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public PublicarCotacaoFornecedorCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(PublicarCotacaoFornecedorCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var cotacao = new PfoCotacaoPublicada(request.CotacaoOrigemId, request.FornecedorId, request.PrazoResposta, tenantId, usuario);
            if (!cotacao.IsValid)
                return CommandResult.Falha(cotacao.Notifications.Select(n => n.Message), "Dados da cotação são inválidos.");
            _context.PfoCotacoesPublicadas.Add(cotacao);

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Cotação publicada ao fornecedor.", new { cotacao.Id });
        }
    }

    public class ResponderCotacaoFornecedorCommandHandler : ICommandHandler<ResponderCotacaoFornecedorCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ResponderCotacaoFornecedorCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(ResponderCotacaoFornecedorCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var cotacao = await _context.PfoCotacoesPublicadas.FirstOrDefaultAsync(c => c.Id == request.CotacaoPublicadaId && c.DeletadoEm == null, cancellationToken);
            if (cotacao == null) return CommandResult.Falha("Cotação não encontrada.");

            // PFO-002 / VAL-PFO-001: isolamento — o fornecedor só responde a cotação vinculada a ele.
            if (cotacao.FornecedorId != request.FornecedorId)
                return CommandResult.Falha("Acesso negado: cotação de outro fornecedor [PFO-002/VAL-PFO-001].");

            // VAL-PFO-003: cotação encerrada/vencida não aceita resposta.
            if (!cotacao.AceitaResposta())
                return CommandResult.Falha("Cotação encerrada ou fora do prazo de resposta [VAL-PFO-003].");

            var resposta = new PfoRespostaCotacao(cotacao.Id, request.FornecedorId, request.ValorTotal, request.Observacao, tenantId, usuario);
            if (!resposta.IsValid)
                return CommandResult.Falha(resposta.Notifications.Select(n => n.Message), "Dados da resposta são inválidos.");
            _context.PfoRespostasCotacao.Add(resposta);

            if (request.Itens != null)
                foreach (var i in request.Itens)
                    _context.PfoRespostaCotacaoItens.Add(new PfoRespostaCotacaoItem(resposta.Id, i.ItemOrigemId, i.ProdutoId, i.Quantidade, i.ValorUnitario, i.PrazoEntregaDias, tenantId, usuario));

            resposta.Enviar(usuario);
            cotacao.MarcarRespondida(usuario);

            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.PfoCotacaoRespondida,
                JsonSerializer.Serialize(new { cotacaoOrigemId = cotacao.CotacaoOrigemId, cotacao.FornecedorId, respostaId = resposta.Id, resposta.ValorTotal })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Resposta de cotação enviada.", new { respostaId = resposta.Id });
        }
    }

    public class EnviarPreAvisoEmbarqueCommandHandler : ICommandHandler<EnviarPreAvisoEmbarqueCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public EnviarPreAvisoEmbarqueCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(EnviarPreAvisoEmbarqueCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (request.Itens == null || request.Itens.Count == 0)
                return CommandResult.Falha("O pré-aviso deve possuir ao menos um item.");

            var preAviso = new PfoPreAvisoEmbarque(request.PedidoCompraId, request.FornecedorId, request.DataPrevistaEntrega, request.Observacao, tenantId, usuario);
            if (!preAviso.IsValid)
                return CommandResult.Falha(preAviso.Notifications.Select(n => n.Message), "Dados do pré-aviso são inválidos.");
            _context.PfoPreAvisosEmbarque.Add(preAviso);

            foreach (var i in request.Itens)
            {
                var item = new PfoPreAvisoItem(preAviso.Id, i.PedidoCompraItemId, i.ProdutoId, i.QuantidadePrevista, i.Lote, tenantId, usuario);
                if (!item.IsValid)
                    return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Item de pré-aviso inválido.");
                _context.PfoPreAvisoItens.Add(item);
            }

            preAviso.Enviar(usuario);

            // PFO-007: alimenta a Logística de Entrada (recebimento → motor único, D1) via evento.
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.PfoPreAvisoEnviado,
                JsonSerializer.Serialize(new { preAviso.PedidoCompraId, preAviso.FornecedorId, preAvisoId = preAviso.Id, itens = request.Itens.Count, preAviso.DataPrevistaEntrega })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Pré-aviso de embarque enviado.", new { preAvisoId = preAviso.Id });
        }
    }

    public class EnviarDocumentoFornecedorCommandHandler : ICommandHandler<EnviarDocumentoFornecedorCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public EnviarDocumentoFornecedorCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(EnviarDocumentoFornecedorCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var doc = new PfoDocumentoFornecedor(request.FornecedorId, request.ReferenciaTipo, request.ReferenciaId, request.TipoDocumento, request.ArquivoId, tenantId, usuario);
            if (!doc.IsValid)
                return CommandResult.Falha(doc.Notifications.Select(n => n.Message), "Dados do documento são inválidos.");
            _context.PfoDocumentosFornecedor.Add(doc);

            // PFO-008: integra o repositório documental (GED) e notifica Compras/Fiscal.
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.PfoDocumentoEnviado,
                JsonSerializer.Serialize(new { doc.Id, doc.FornecedorId, doc.ReferenciaTipo, doc.ReferenciaId, doc.ArquivoId, doc.TipoDocumento })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Documento enviado.", new { doc.Id });
        }
    }
}
