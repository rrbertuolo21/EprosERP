using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Services;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;
using MediatR;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>
    /// Handler de importação de XML de NF-e de entrada. Faz o parse do XML em um
    /// <see cref="LancarCompraFiscalCommand"/> e re-despacha via MediatR para reutilizar todo o fluxo de
    /// persistência do agregado (itens/estoque/emitente/destinatário/total/transporte/pagamentos/Outbox).
    /// Grava também o histórico do XML importado em <see cref="ImportacaoXml"/>, com o status do
    /// processamento (Finalizado/Erro) e a mensagem de erro quando houver falha no parse ou no lançamento.
    /// </summary>
    public class ImportarCompraXmlCommandHandler : ICommandHandler<ImportarCompraXmlCommand>
    {
        private readonly ISender _sender;
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ImportarCompraXmlCommandHandler(
            ISender sender,
            ContextEstoque context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _sender = sender;
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ImportarCompraXmlCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            LancarCompraFiscalCommand comando;
            try
            {
                comando = NfeXmlParser.Parse(request.ConteudoXml);
            }
            catch (ArgumentException ex)
            {
                // Histórico do XML que falhou já no parse.
                var registroErro = new ImportacaoXml(
                    empresaId: null,
                    xml: request.ConteudoXml,
                    tipoDeXml: ETipoXml.NotaFiscalEntrada,
                    nfeId: string.Empty,
                    mensagemErro: ex.Message,
                    codigoSefaz: 0,
                    tipoEvento: string.Empty,
                    tenantId: tenantId,
                    criadoPor: usuario,
                    statusImportacaoXml: EStatusProcessamento.Erro);
                _context.ImportacoesXml.Add(registroErro);
                await _context.SaveChangesAsync(cancellationToken);

                return CommandResult.Falha(ex.Message);
            }

            var resultado = await _sender.Send(comando, cancellationToken);

            // Histórico do XML importado (chave de acesso da NF-e como NfeId).
            var registro = new ImportacaoXml(
                empresaId: null,
                xml: request.ConteudoXml,
                tipoDeXml: ETipoXml.NotaFiscalEntrada,
                nfeId: comando.ChaveAcesso,
                mensagemErro: resultado.Sucesso ? null : resultado.Mensagem,
                codigoSefaz: 0,
                tipoEvento: string.Empty,
                tenantId: tenantId,
                criadoPor: usuario,
                statusImportacaoXml: resultado.Sucesso ? EStatusProcessamento.Finalizado : EStatusProcessamento.Erro,
                statusCadastro: resultado.Sucesso ? EStatusProcessamento.Finalizado : EStatusProcessamento.NaoProcessado);
            _context.ImportacoesXml.Add(registro);
            await _context.SaveChangesAsync(cancellationToken);

            return resultado;
        }
    }
}
