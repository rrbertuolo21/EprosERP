using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Events;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Services;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>Utilitário compartilhado de montagem do agregado Compra a partir dos inputs fiscais.</summary>
    internal static class CompraAgregadoBuilder
    {
        public static async Task<(Compra? compra, CommandResult? erro)> MontarAgregadoAsync(
            ContextEstoque context,
            Compra compra,
            IReadOnlyList<ItemCompraFiscalInput> itens,
            EmitenteInput? emit,
            DestinatarioInput? dest,
            ImpostoCompraInput? imp,
            TotalCompraInput? tot,
            TransporteCompraInput? tr,
            IReadOnlyList<PagamentoCompraInput>? pagamentos,
            string tenantId,
            string usuario,
            string numeroNota,
            string descricaoMovimento,
            CancellationToken cancellationToken)
        {
            if (!compra.IsValid)
                return (null, CommandResult.Falha(compra.Notifications.Select(n => n.Message), "Dados de cabeçalho da nota fiscal são inválidos."));

            // Entrada de estoque via MOTOR ÚNICO (kardex, D1).
            var motor = new MotorMovimentacaoEstoque(context, tenantId, usuario);
            var fato = new FatoGeradorEstoque(null, compra.Id, null, EOrigemFatoGeradorEstoque.EntradaFiscal, tenantId, usuario, referenciaExterna: $"{descricaoMovimento} NF {numeroNota}");
            context.FatosGeradoresEstoque.Add(fato);

            foreach (var itemInput in itens)
            {
                var produto = await context.Produtos.FirstOrDefaultAsync(p => p.Sku == itemInput.Sku, cancellationToken);
                if (produto == null)
                {
                    produto = new Produto(itemInput.Sku, itemInput.NomeProduto, 0m, tenantId, usuario);
                    if (!produto.IsValid)
                        return (null, CommandResult.Falha(produto.Notifications.Select(n => n.Message), $"Erro ao criar produto com SKU {itemInput.Sku}."));
                    context.Produtos.Add(produto);
                }

                var custeioPadrao = await context.EstoqueProdutos
                    .Where(e => e.EmpresaId == MotorMovimentacaoEstoque.EmpresaPadrao && e.ProdutoId == produto.Id)
                    .Select(e => (ETipoCusteioEstoque?)e.TipoCusteioEstoque)
                    .FirstOrDefaultAsync(cancellationToken) ?? ETipoCusteioEstoque.CustoMedio;

                var resEntrada = await motor.AplicarEntradaAsync(
                    MotorMovimentacaoEstoque.EmpresaPadrao, produto.Id, ETipoEstoque.Geral, itemInput.Quantidade, itemInput.PrecoUnitario,
                    fato.Id, null, null, null, custeioPadrao, cancellationToken);
                if (!resEntrada.Sucesso)
                    return (null, CommandResult.Falha(resEntrada.Erro ?? $"Erro ao lançar entrada de estoque para o SKU {itemInput.Sku}."));

                var movimento = new MovimentoEstoque(produto.Id, itemInput.Quantidade, "Entrada", $"{descricaoMovimento} - NF-e nº {numeroNota}", tenantId, usuario);
                if (!movimento.IsValid)
                    return (null, CommandResult.Falha(movimento.Notifications.Select(n => n.Message), $"Erro ao criar movimentação de estoque para o SKU {itemInput.Sku}."));
                context.MovimentosEstoque.Add(movimento);

                compra.AdicionarItem(
                    produto.Id, itemInput.Quantidade, itemInput.PrecoUnitario, itemInput.ValorIms, itemInput.ValorIpi, usuario,
                    codigoProduto: itemInput.Sku, descricaoProduto: itemInput.NomeProduto, ncm: itemInput.Ncm, cfop: itemInput.Cfop, unidadeComercial: itemInput.UnidadeComercial);
            }

            if (emit is not null)
            {
                var emitente = new CompraEmitente(
                    compra.Id, null, null, emit.Cnpj, emit.Cpf, emit.RazaoSocial, emit.NomeFantasia,
                    emit.Telefone, emit.InscricaoEstadual, null, null, emit.Cnae, emit.RegimeTributario, null, tenantId, usuario);
                compra.DefinirEmitente(emitente);
            }

            if (dest is not null)
            {
                var destinatario = new CompraDestinatario(
                    compra.Id, dest.PessoaId, dest.Cnpj, dest.Cpf, dest.RazaoSocial, dest.Telefone,
                    dest.InscricaoEstadual, null, dest.IndicadorIE, dest.Email, dest.EhConsumidorFinal, tenantId, usuario);
                compra.DefinirDestinatario(destinatario);
            }

            if (imp is not null)
            {
                var imposto = new CompraImposto(compra.Id, imp.ValorAliquotaCreditoIcms, tenantId, usuario);
                compra.DefinirImposto(imposto);
            }

            if (tot is not null)
            {
                var total = new CompraTotal(
                    compra.Id, 0m, tot.ValorIcms, 0m, 0m, 0m, 0m, 0m, 0m, tot.ValorProduto, tot.ValorFrete,
                    tot.ValorSeguro, tot.ValorDesconto, 0m, tot.ValorIpi, 0m, 0m, 0m, 0m, tot.ValorNotaFiscal, tenantId, usuario);
                compra.DefinirTotal(total);
            }

            if (tr is not null)
            {
                var transporte = new CompraTransporte(compra.Id, tenantId, usuario);
                if (!string.IsNullOrWhiteSpace(tr.TransportadoraRazaoSocialOuNome))
                {
                    transporte.IncluirTransportadora(
                        tr.TransportadoraPessoaId, tr.TransportadoraCnpj, tr.TransportadoraCpf,
                        tr.TransportadoraRazaoSocialOuNome, tr.TransportadoraInscricaoEstadual, null, null, tr.TransportadoraUf, usuario);
                }
                compra.DefinirTransporte(transporte);
            }

            if (pagamentos is { Count: > 0 })
            {
                foreach (var pag in pagamentos)
                {
                    var pagamento = new CompraPagamento(
                        compra.Id, pag.ValorTroco, pag.TipoPagamento, pag.ValorPagamento,
                        ETipoIntegracaoPagamentoCartao.NaoUtiliza, null, EBandeiraCartao.NaoUtiliza, null, tenantId, usuario);
                    compra.AdicionarPagamento(pagamento);
                }
            }

            if (!compra.AgregadoValido())
                return (null, CommandResult.Falha(compra.Notifications.Select(n => n.Message), "Invariantes de domínio do agregado de compra não foram atendidas."));

            return (compra, null);
        }
    }

    // ============================ Entrada própria ============================

    /// <summary>Handler de aplicação (EntradaPropriaCompraCommandHandler).</summary>
    public class EntradaPropriaCompraCommandHandler : ICommandHandler<EntradaPropriaCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public EntradaPropriaCompraCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(EntradaPropriaCompraCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var compra = new Compra(
                request.Emitente.Cnpj ?? request.Emitente.Cpf ?? string.Empty,
                request.Emitente.RazaoSocial,
                request.NumeroNota,
                // Entrada própria ainda não tem chave (será gerada na emissão); usa placeholder de 44 chars.
                new string('0', 44),
                request.ValorTotal,
                request.DataEmissao,
                tenantId,
                usuario,
                modeloFiscal: EModeloDocumento.NFe,
                naturezaOperacao: request.NaturezaOperacao,
                informacoesComplementares: request.InformacoesComplementares,
                informacoesAdicionaisFisco: request.InformacoesAdicionaisFisco,
                compraOrigem: EVendaOrigem.NfeCompleta,
                tipoNota: ETipoNFeCompra.NotaFiscalEntradaPropria);

            var (agregado, erro) = await CompraAgregadoBuilder.MontarAgregadoAsync(
                _context, compra, request.Itens, request.Emitente, request.Destinatario, request.Imposto, request.Total,
                request.Transporte, request.Pagamentos, tenantId, usuario, request.NumeroNota, "Entrada própria de compra", cancellationToken);

            if (erro != null) return erro;

            // NF-e própria (número/série do emitente) marcada para emissão.
            var nfe = new CompraNfe(compra.Id, request.NumeroNfe, request.SerieNfe, request.DataHoraSaida, tenantId, usuario);
            compra.DefinirNfe(nfe);

            if (!compra.AgregadoValido())
                return CommandResult.Falha(compra.Notifications.Select(n => n.Message), "Invariantes de domínio do agregado de compra não foram atendidas.");

            _context.Compras.Add(compra);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "CompraEntradaPropriaLancada",
                JsonSerializer.Serialize(new { CompraId = compra.Id, TenantId = tenantId, compra.NumeroNota, compra.ValorTotal, QtdItens = compra.Itens.Count })));

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Entrada própria lançada com sucesso! A transmissão da NF-e é feita pela emissão fiscal.", new { CompraId = compra.Id });
        }
    }

    // ============================ Entrada fornecedor ============================

    /// <summary>Handler de aplicação (EntradaFornecedorCompraCommandHandler).</summary>
    public class EntradaFornecedorCompraCommandHandler : ICommandHandler<EntradaFornecedorCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public EntradaFornecedorCompraCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(EntradaFornecedorCompraCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var notaExistente = await _context.Compras.AnyAsync(c => c.ChaveAcesso == request.ChaveAcesso, cancellationToken);
            if (notaExistente)
                return CommandResult.Falha("Esta nota fiscal já foi lançada para este inquilino.");

            var compra = new Compra(
                request.Emitente.Cnpj ?? request.Emitente.Cpf ?? string.Empty,
                request.Emitente.RazaoSocial,
                request.NumeroNota,
                request.ChaveAcesso,
                request.ValorTotal,
                request.DataEmissao,
                tenantId,
                usuario,
                modeloFiscal: EModeloDocumento.NFe,
                naturezaOperacao: request.NaturezaOperacao,
                informacoesComplementares: request.InformacoesComplementares,
                informacoesAdicionaisFisco: request.InformacoesAdicionaisFisco,
                compraOrigem: EVendaOrigem.NfeCompleta,
                tipoNota: ETipoNFeCompra.NotaFiscalEntrada);

            var (agregado, erro) = await CompraAgregadoBuilder.MontarAgregadoAsync(
                _context, compra, request.Itens, request.Emitente, request.Destinatario, request.Imposto, request.Total,
                request.Transporte, request.Pagamentos, tenantId, usuario, request.NumeroNota, "Entrada de fornecedor", cancellationToken);

            if (erro != null) return erro;

            // NF-e já emitida pelo fornecedor (chave/série/número informados).
            var nfe = new CompraNfe(compra.Id, request.ChaveAcesso, request.NumeroNfe, request.SerieNfe, request.DataEmissao, request.DataHoraSaida, tenantId, usuario);
            compra.DefinirNfe(nfe);

            if (!compra.AgregadoValido())
                return CommandResult.Falha(compra.Notifications.Select(n => n.Message), "Invariantes de domínio do agregado de compra não foram atendidas.");

            _context.Compras.Add(compra);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "CompraEntradaFornecedorLancada",
                JsonSerializer.Serialize(new { CompraId = compra.Id, TenantId = tenantId, compra.ChaveAcesso, compra.NumeroNota, compra.ValorTotal, QtdItens = compra.Itens.Count })));

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Entrada de fornecedor lançada com sucesso!", new { CompraId = compra.Id });
        }
    }

    // ============================ Carta de correção ============================

    /// <summary>Handler de aplicação (CartaCorrecaoCompraCommandHandler).</summary>
    public class CartaCorrecaoCompraCommandHandler : ICommandHandler<CartaCorrecaoCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public CartaCorrecaoCompraCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CartaCorrecaoCompraCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var compra = await _context.Compras
                .Include(c => c.Nfe)
                    .ThenInclude(n => n!.CartasCorrecoes)
                .FirstOrDefaultAsync(c => c.Id == request.CompraId && c.DeletadoEm == null, cancellationToken);

            if (compra == null)
                return CommandResult.Falha("Compra não localizada.");
            if (compra.Nfe == null)
                return CommandResult.Falha("A compra não possui NF-e vinculada para carta de correção.");
            if (compra.Nfe.StatusInterno != EDocumentoFiscalStatus.Autorizado)
                return CommandResult.Falha("Só é possível emitir carta de correção para NF-e autorizada.");

            var sequencia = compra.Nfe.CartasCorrecoes.Count + 1;
            compra.Nfe.AdicionarCartaCorrecao(request.Ambiente, request.TextoCorrecao, usuario);
            // Marca a sequência do evento (a transmissão à SEFAZ da CC-e é feita pela camada de emissão fiscal).
            compra.Nfe.CorrigirCartaCorrecao(sequencia);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Carta de correção registrada com sucesso!", new { compra.Id, SequenciaEvento = sequencia });
        }
    }

    // ============================ Duplicar compra ============================

    /// <summary>Handler de aplicação (DuplicarCompraCommandHandler).</summary>
    public class DuplicarCompraCommandHandler : ICommandHandler<DuplicarCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DuplicarCompraCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DuplicarCompraCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var origem = await _context.Compras
                .AsNoTracking()
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.Id == request.CompraId && c.DeletadoEm == null, cancellationToken);

            if (origem == null)
                return CommandResult.Falha("Id da compra não localizado.");

            var chaveExistente = await _context.Compras.AnyAsync(c => c.ChaveAcesso == request.NovaChaveAcesso, cancellationToken);
            if (chaveExistente)
                return CommandResult.Falha("Já existe uma compra com a nova Chave de Acesso informada.");

            var nova = new Compra(
                origem.FornecedorCnpj,
                origem.FornecedorNome,
                request.NovoNumeroNota,
                request.NovaChaveAcesso,
                origem.ValorTotal,
                origem.DataEmissao,
                tenantId,
                usuario,
                modeloFiscal: origem.ModeloFiscal,
                naturezaOperacao: origem.NaturezaOperacao,
                informacoesComplementares: origem.InformacoesComplementares,
                informacoesAdicionaisFisco: origem.InformacoesAdicionaisFisco,
                modalidadeFrete: origem.ModalidadeFrete,
                compraOrigem: origem.CompraOrigem,
                tipoNota: origem.TipoNota,
                formaPagamento: origem.FormaPagamento);

            foreach (var item in origem.Itens)
            {
                nova.AdicionarItem(
                    item.ProdutoId, item.Quantidade, item.PrecoUnitario, item.ValorIms, item.ValorIpi, usuario,
                    codigoProduto: item.CodigoProduto, codigoEan: item.CodigoEan, descricaoProduto: item.DescricaoProduto,
                    ncm: item.Ncm, cestId: item.CestId, cest: item.Cest, codigoAnpId: item.CodigoAnpId, codigoAnp: item.CodigoAnp,
                    cfop: item.Cfop, unidadeComercial: item.UnidadeComercial, valorDesconto: item.ValorDesconto,
                    valorFreteRateado: item.ValorFreteRateado, valorCusto: item.ValorCusto);
            }

            if (!nova.AgregadoValido())
                return CommandResult.Falha(nova.Notifications.Select(n => n.Message), "Invariantes de domínio da compra duplicada não foram atendidas.");

            _context.Compras.Add(nova);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Compra duplicada com sucesso!", new { CompraId = nova.Id });
        }
    }

    // ============================ Gerar DANFE sem autorização (pré-visualização) ============================

    /// <summary>Gera Danfe Sem Autorizacao Compra.</summary>
    public class GerarDanfeSemAutorizacaoCompraCommandHandler : ICommandHandler<GerarDanfeSemAutorizacaoCompraCommand>
    {
        private readonly ContextEstoque _context;

        public GerarDanfeSemAutorizacaoCompraCommandHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(GerarDanfeSemAutorizacaoCompraCommand request, CancellationToken cancellationToken)
        {
            var compra = await _context.Compras
                .AsNoTracking()
                .Include(c => c.Itens)
                .Include(c => c.Emitente)
                .Include(c => c.Destinatario)
                .FirstOrDefaultAsync(c => c.Id == request.CompraId && c.DeletadoEm == null, cancellationToken);

            if (compra == null)
                return CommandResult.Falha("Compra não localizada.");

            // Honestidade fiscal: pré-visualização SEM VALOR FISCAL (compra não autorizada). Gera um PDF
            // mínimo (cabeçalho + itens) marcado como rascunho. A geração do DANFE oficial autorizado é
            // feita pela camada de emissão fiscal (IDanfeService) a partir do DocumentoFiscal.
            var pdfBytes = GerarPdfPreview(compra);

            return CommandResult.Ok("Pré-visualização (SEM VALOR FISCAL) gerada.",
                new DanfePreviewResult(compra.Id, $"danfe-preview-{compra.NumeroNota}.pdf", pdfBytes));
        }

        /// <summary>
        /// Gera um PDF mínimo (PDF 1.4 monolítico) de pré-visualização, sem dependências externas, com a
        /// marca "SEM VALOR FISCAL". Não pretende ser o DANFE oficial — é um rascunho para conferência.
        /// </summary>
        private static byte[] GerarPdfPreview(Compra compra)
        {
            var linhas = new List<string>
            {
                "DANFE - PRE-VISUALIZACAO (SEM VALOR FISCAL)",
                $"Numero da Nota: {compra.NumeroNota}",
                $"Fornecedor: {compra.FornecedorNome}  CNPJ: {compra.FornecedorCnpj}",
                $"Emissao: {compra.DataEmissao:dd/MM/yyyy}   Valor Total: {compra.ValorTotal:0.00}",
                $"Natureza: {compra.NaturezaOperacao}   Modelo: {compra.ModeloFiscal}",
                "Itens:"
            };
            foreach (var item in compra.Itens)
                linhas.Add($"  {item.CodigoProduto} - {item.DescricaoProduto}  Qtd {item.Quantidade:0.###} x {item.PrecoUnitario:0.00} = {item.ValorTotal:0.00}");

            return PdfSimples.Gerar(linhas);
        }
    }

    /// <summary>Gerador de PDF mínimo (texto) sem bibliotecas externas — apenas pré-visualizações internas.</summary>
    internal static class PdfSimples
    {
        public static byte[] Gerar(IReadOnlyList<string> linhas)
        {
            var conteudo = new StringBuilder();
            conteudo.Append("BT\n/F1 10 Tf\n50 780 Td\n12 TL\n");
            foreach (var linha in linhas)
            {
                conteudo.Append('(').Append(Escapar(linha)).Append(") Tj\nT*\n");
            }
            conteudo.Append("ET");
            var stream = conteudo.ToString();
            var streamBytes = Encoding.ASCII.GetBytes(stream);

            var sb = new StringBuilder();
            var offsets = new List<int>();
            void Obj(string s) { offsets.Add(sb.Length); sb.Append(s); }

            sb.Append("%PDF-1.4\n");
            Obj("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
            Obj("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");
            Obj("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\nendobj\n");
            Obj("4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");
            Obj($"5 0 obj\n<< /Length {streamBytes.Length} >>\nstream\n{stream}\nendstream\nendobj\n");

            var xrefPos = sb.Length;
            sb.Append("xref\n0 6\n0000000000 65535 f \n");
            foreach (var off in offsets)
                sb.Append(off.ToString("D10")).Append(" 00000 n \n");
            sb.Append("trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n").Append(xrefPos).Append("\n%%EOF");

            return Encoding.ASCII.GetBytes(sb.ToString());
        }

        private static string Escapar(string s) =>
            (s ?? string.Empty).Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
    }
}
