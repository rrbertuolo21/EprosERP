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
    /// Handlers do submódulo Subcontratação (EST-SUB). Gravam em transação única e respeitam tenant (filtro
    /// global do ContextBase). SUB-005: envio/retorno recalculam saldo em poder de terceiros. CA-004: impede
    /// retorno maior que o saldo enviado. Documento fiscal/CFOP (SUB-006/007/008) e integração com estoque/
    /// contas a pagar (SUB-009/010) permanecem como referências externas/pendências — nada de fiscal de memória.
    /// </summary>
    public class CriarSubOrdemCommandHandler : ICommandHandler<CriarSubOrdemCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarSubOrdemCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarSubOrdemCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var ordem = new SubOrdem(request.EmpresaId, request.NumeroOrdem, request.OrdemProducaoId, request.FornecedorId, request.DataEmissao, request.DataPrevistaRetorno, request.Observacao, tenantId, usuario);
            if (!ordem.IsValid)
                return CommandResult.Falha(ordem.Notifications.Select(n => n.Message), "Dados da ordem de subcontratação são inválidos.");

            _context.SubOrdens.Add(ordem);

            if (request.Itens != null)
            {
                foreach (var input in request.Itens)
                {
                    var item = new SubOrdemItem(ordem.Id, input.ProdutoId, input.QuantidadePlanejada, input.Unidade, input.OperacaoTerceirizada, tenantId, usuario);
                    if (!item.IsValid)
                        return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Item da ordem de subcontratação inválido.");
                    _context.SubOrdemItens.Add(item);
                }
            }

            _context.SubHistoricos.Add(new SubHistorico(ordem.Id, "ordem_criada", null, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Ordem de subcontratação criada com sucesso!", new { ordem.Id });
        }
    }

    public class RegistrarSubEnvioCommandHandler : ICommandHandler<RegistrarSubEnvioCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarSubEnvioCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarSubEnvioCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var ordem = await _context.SubOrdens.FirstOrDefaultAsync(o => o.Id == request.OrdemId && o.DeletadoEm == null, cancellationToken);
            if (ordem == null)
                return CommandResult.Falha("Ordem de subcontratação não encontrada.");

            var envio = new SubEnvio(request.OrdemId, request.DataEnvio, request.DocumentoFiscalId, tenantId, usuario);
            if (!envio.IsValid)
                return CommandResult.Falha(envio.Notifications.Select(n => n.Message), "Dados da remessa são inválidos.");

            _context.SubEnvios.Add(envio);

            if (request.Itens != null)
            {
                foreach (var input in request.Itens)
                {
                    var item = new SubEnvioItem(envio.Id, input.ProdutoId, input.QuantidadeEnviada, input.LoteId, input.LocalOrigemId, tenantId, usuario);
                    if (!item.IsValid)
                        return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Item de remessa inválido.");
                    _context.SubEnvioItens.Add(item);

                    // SUB-005/010: atualiza saldo em poder de terceiros (movimento físico de estoque é pendência).
                    await AtualizarSaldoEnvioAsync(ordem.FornecedorId, input.ProdutoId, ordem.Id, input.QuantidadeEnviada, tenantId, usuario, cancellationToken);
                }
            }

            envio.Confirmar(usuario);
            ordem.MarcarEmProcesso(usuario);

            _context.SubHistoricos.Add(new SubHistorico(ordem.Id, "envio_registrado", null, null, null, tenantId, usuario));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "estoque.sub.envio_registrado",
                JsonSerializer.Serialize(new { ordemId = ordem.Id, envioId = envio.Id, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Remessa de subcontratação registrada com sucesso!", new { envio.Id });
        }

        private async Task AtualizarSaldoEnvioAsync(Guid fornecedorId, Guid produtoId, Guid ordemId, decimal quantidade, string tenantId, string usuario, CancellationToken ct)
        {
            var saldo = await _context.SubSaldosTerceiro.FirstOrDefaultAsync(s => s.FornecedorId == fornecedorId && s.ProdutoId == produtoId && s.OrdemId == ordemId && s.DeletadoEm == null, ct);
            if (saldo == null)
            {
                saldo = new SubSaldoTerceiro(fornecedorId, produtoId, ordemId, tenantId, usuario);
                _context.SubSaldosTerceiro.Add(saldo);
            }
            saldo.RegistrarEnvio(quantidade, usuario);
        }
    }

    public class RegistrarSubRetornoCommandHandler : ICommandHandler<RegistrarSubRetornoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarSubRetornoCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarSubRetornoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var ordem = await _context.SubOrdens.FirstOrDefaultAsync(o => o.Id == request.OrdemId && o.DeletadoEm == null, cancellationToken);
            if (ordem == null)
                return CommandResult.Falha("Ordem de subcontratação não encontrada.");

            var retorno = new SubRetorno(request.OrdemId, request.DataRetorno, request.DocumentoFiscalId, tenantId, usuario);
            if (!retorno.IsValid)
                return CommandResult.Falha(retorno.Notifications.Select(n => n.Message), "Dados do retorno são inválidos.");

            _context.SubRetornos.Add(retorno);

            if (request.Itens != null)
            {
                foreach (var input in request.Itens)
                {
                    var item = new SubRetornoItem(retorno.Id, input.ProdutoId, input.QuantidadeRetorno, input.QuantidadeAprovada, input.QuantidadePerda, input.QuantidadeSucata, input.Rendimento, tenantId, usuario);
                    if (!item.IsValid)
                        return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Item de retorno inválido.");

                    // CA-004: impedir retorno maior que saldo em poder do terceiro.
                    var saldo = await _context.SubSaldosTerceiro.FirstOrDefaultAsync(s => s.FornecedorId == ordem.FornecedorId && s.ProdutoId == input.ProdutoId && s.OrdemId == ordem.Id && s.DeletadoEm == null, cancellationToken);
                    var perda = input.QuantidadePerda ?? 0m;
                    if (saldo == null || (input.QuantidadeRetorno + perda) > saldo.QuantidadeEmPoderTerceiro)
                        return CommandResult.Falha("Retorno maior que o saldo em poder do terceiro [CA-004].");

                    _context.SubRetornoItens.Add(item);
                    saldo.RegistrarRetorno(input.QuantidadeRetorno, perda, usuario);
                }
            }

            retorno.Confirmar(usuario);
            ordem.MarcarRetornada(usuario);

            _context.SubHistoricos.Add(new SubHistorico(ordem.Id, "retorno_registrado", null, null, null, tenantId, usuario));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "estoque.sub.retorno_registrado",
                JsonSerializer.Serialize(new { ordemId = ordem.Id, retornoId = retorno.Id, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Retorno de subcontratação registrado com sucesso!", new { retorno.Id });
        }
    }
}
