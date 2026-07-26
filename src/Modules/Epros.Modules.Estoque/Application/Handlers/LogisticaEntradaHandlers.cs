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
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>
    /// Handlers da Logística de Entrada (EST-LDE): criar entrada, local de entrega, vincular documento,
    /// confirmar, cancelar e estornar. Histórico imutável (LDE-021) e eventos via Outbox (EF §13).
    /// </summary>
    public class CriarLdeEntradaCommandHandler : ICommandHandler<CriarLdeEntradaCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarLdeEntradaCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarLdeEntradaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var entrada = new LdeEntrada(request.CompraId, request.FornecedorId, request.LocalEntregaId, request.DocumentoEntradaId, tenantId, usuario);
            if (!entrada.IsValid)
                return CommandResult.Falha(entrada.Notifications.Select(n => n.Message), "Dados da entrada são inválidos.");

            _context.LdeEntradas.Add(entrada);
            RegistrarHistorico(entrada.Id, "entrada_criada", null, ESituacaoEntradaLogistica.Rascunho, null, usuario, tenantId);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "est.lde.entrada_criada",
                JsonSerializer.Serialize(new { entrada.Id, entrada.CompraId, tenantId, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Entrada criada em rascunho.", new { entrada.Id });
        }

        internal void RegistrarHistorico(Guid entradaId, string evento, ESituacaoEntradaLogistica? anterior, ESituacaoEntradaLogistica nova, string? motivo, string usuario, string tenantId)
        {
            _context.LdeHistoricos.Add(new LdeHistorico(entradaId, evento, anterior, nova, motivo, usuario, tenantId, usuario));
        }
    }

    public class RegistrarLdeLocalEntregaCommandHandler : ICommandHandler<RegistrarLdeLocalEntregaCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarLdeLocalEntregaCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarLdeLocalEntregaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var entrada = await _context.LdeEntradas.FirstOrDefaultAsync(e => e.Id == request.EntradaId && e.DeletadoEm == null, cancellationToken);
            if (entrada == null)
                return CommandResult.Falha("Entrada não encontrada.");

            var local = new LdeLocalEntregaCompra(
                request.CompraId, request.Nome, request.Fone, request.Email, request.InscricaoEstadual, request.Documento,
                request.Uf, request.Logradouro, request.Numero, request.Complemento, request.Bairro,
                request.MunicipioId, request.MunicipioNome, request.Cep, request.PaisId, request.PaisNome,
                tenantId, usuario);
            if (!local.IsValid)
                return CommandResult.Falha(local.Notifications.Select(n => n.Message), "Dados do local de entrega são inválidos.");

            _context.LdeLocaisEntregaCompra.Add(local);
            entrada.VincularLocalEntrega(local.Id, usuario);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "est.lde.local_entrega_alterado",
                JsonSerializer.Serialize(new { entrada.Id, entrada.CompraId, enderecoId = local.Id, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Local de entrega registrado.", new { localId = local.Id, entradaId = entrada.Id });
        }
    }

    public class VincularLdeDocumentoCommandHandler : ICommandHandler<VincularLdeDocumentoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public VincularLdeDocumentoCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(VincularLdeDocumentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var entrada = await _context.LdeEntradas.FirstOrDefaultAsync(e => e.Id == request.EntradaId && e.DeletadoEm == null, cancellationToken);
            if (entrada == null)
                return CommandResult.Falha("Entrada não encontrada.");

            var documento = new LdeDocumentoEntrada(
                request.ChaveAcesso, request.Numero, request.Serie, request.DataEmissao, request.NaturezaOperacao,
                request.ValorTotal, request.FornecedorId, request.DestinatarioId, request.EmitenteId,
                null, null, "Vinculado", tenantId, usuario);
            _context.LdeDocumentosEntrada.Add(documento);

            if (request.Itens != null)
            {
                foreach (var i in request.Itens)
                {
                    var item = new LdeDocumentoEntradaItem(documento.Id, i.ProdutoId, i.QuantidadeDocumento, i.ValorItem, i.DadosTributariosItem, tenantId, usuario);
                    if (!item.IsValid)
                        return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Item de documento inválido.");
                    _context.LdeDocumentoEntradaItens.Add(item);
                }
            }

            if (request.Duplicatas != null)
            {
                foreach (var d in request.Duplicatas)
                {
                    var dup = new LdeDocumentoEntradaDuplicata(documento.Id, d.Numero, d.DataVencimento, d.Valor, null, tenantId, usuario);
                    _context.LdeDocumentoEntradaDuplicatas.Add(dup);
                }
            }

            entrada.VincularDocumento(documento.Id, usuario);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "est.lde.documento_vinculado",
                JsonSerializer.Serialize(new { entrada.Id, documentoId = documento.Id, documento.ChaveAcesso, documento.Numero, documento.Serie })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Documento de entrada vinculado.", new { documentoId = documento.Id, entradaId = entrada.Id });
        }
    }

    public class ConfirmarLdeEntradaCommandHandler : ICommandHandler<ConfirmarLdeEntradaCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ConfirmarLdeEntradaCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ConfirmarLdeEntradaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var entrada = await _context.LdeEntradas.FirstOrDefaultAsync(e => e.Id == request.Id && e.DeletadoEm == null, cancellationToken);
            if (entrada == null)
                return CommandResult.Falha("Entrada não encontrada.");

            if (!entrada.PodeConfirmar())
                return CommandResult.Falha("Somente entradas em rascunho podem ser confirmadas.");

            // LDE-012: fornecedor obrigatório (garantido no domínio).
            // LDE-015: documento fiscal vinculado obrigatório quando a operação exigir documento.
            if (entrada.DocumentoEntradaId == null)
                return CommandResult.Falha("A entrada deve possuir documento fiscal vinculado para confirmação [LDE-015].");

            var documento = await _context.LdeDocumentosEntrada.AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == entrada.DocumentoEntradaId && d.DeletadoEm == null, cancellationToken);
            if (documento == null)
                return CommandResult.Falha("Documento de entrada não encontrado.");

            // LDE-013: emitente obrigatório para confirmação.
            if (documento.EmitenteId == null || documento.EmitenteId == Guid.Empty)
                return CommandResult.Falha("A entrada exige emitente no documento para confirmação [LDE-013].");

            // LDE-014: itens obrigatórios para confirmação.
            var temItem = await _context.LdeDocumentoEntradaItens.AsNoTracking()
                .AnyAsync(i => i.DocumentoEntradaId == documento.Id && i.DeletadoEm == null, cancellationToken);
            if (!temItem)
                return CommandResult.Falha("A entrada exige ao menos um item no documento para confirmação [LDE-014].");

            var anterior = entrada.Situacao;
            entrada.Confirmar(usuario);
            _context.LdeHistoricos.Add(new LdeHistorico(entrada.Id, "entrada_confirmada", anterior, entrada.Situacao, null, usuario, tenantId, usuario));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "est.lde.entrada_confirmada",
                JsonSerializer.Serialize(new { entrada.Id, entrada.CompraId, documentoId = documento.Id, tenantId })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Entrada confirmada com sucesso!", new { entrada.Id });
        }
    }

    public class CancelarLdeEntradaCommandHandler : ICommandHandler<CancelarLdeEntradaCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CancelarLdeEntradaCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CancelarLdeEntradaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (string.IsNullOrWhiteSpace(request.Motivo))
                return CommandResult.Falha("O motivo do cancelamento é obrigatório [LDE-018].");

            var entrada = await _context.LdeEntradas.FirstOrDefaultAsync(e => e.Id == request.Id && e.DeletadoEm == null, cancellationToken);
            if (entrada == null)
                return CommandResult.Falha("Entrada não encontrada.");

            if (entrada.EstaCancelada())
                return CommandResult.Falha("Entrada já cancelada ou estornada não pode ser cancelada novamente.");

            var anterior = entrada.Situacao;
            entrada.Cancelar(request.Motivo, usuario);
            _context.LdeHistoricos.Add(new LdeHistorico(entrada.Id, "entrada_cancelada", anterior, entrada.Situacao, request.Motivo, usuario, tenantId, usuario));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "est.lde.entrada_cancelada",
                JsonSerializer.Serialize(new { entrada.Id, motivo = request.Motivo, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Entrada cancelada.", new { entrada.Id });
        }
    }

    public class EstornarLdeEntradaCommandHandler : ICommandHandler<EstornarLdeEntradaCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public EstornarLdeEntradaCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(EstornarLdeEntradaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (string.IsNullOrWhiteSpace(request.Motivo))
                return CommandResult.Falha("O motivo do estorno é obrigatório [LDE-019].");

            var entrada = await _context.LdeEntradas.FirstOrDefaultAsync(e => e.Id == request.Id && e.DeletadoEm == null, cancellationToken);
            if (entrada == null)
                return CommandResult.Falha("Entrada não encontrada.");

            if (entrada.EstaCancelada())
                return CommandResult.Falha("Entrada já cancelada ou estornada não pode ser estornada.");

            var anterior = entrada.Situacao;
            entrada.Estornar(request.Motivo, usuario);
            _context.LdeHistoricos.Add(new LdeHistorico(entrada.Id, "entrada_estornada", anterior, entrada.Situacao, request.Motivo, usuario, tenantId, usuario));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "est.lde.entrada_estornada",
                JsonSerializer.Serialize(new { entrada.Id, motivo = request.Motivo, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Entrada estornada.", new { entrada.Id });
        }
    }
}
