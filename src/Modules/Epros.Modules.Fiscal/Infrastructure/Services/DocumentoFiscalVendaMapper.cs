using System;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Domain.Entities;

// ISOLAMENTO DO NAMESPACE LEGADO por alias (evita colisão de enums Epros.ERP.Shared vs Epros.Shared).
using MotorVenda = Epros.ERP.DfeCalculos.Models.Vendas.Venda;
using MotorEmitente = Epros.ERP.DfeCalculos.Models.Vendas.VendaEmitente;
using MotorEmitenteEndereco = Epros.ERP.DfeCalculos.Models.Vendas.VendaEmitenteEndereco;
using MotorDestinatario = Epros.ERP.DfeCalculos.Models.Vendas.VendaDestinatario;
using MotorDestinatarioEndereco = Epros.ERP.DfeCalculos.Models.Vendas.VendaDestinatarioEndereco;
using MotorDocumentoDfe = Epros.ERP.DfeCalculos.Models.Vendas.VendaDocumentoDfe;
using MotorTributacaoCfop = Epros.ERP.DfeCalculos.Models.Vendas.VendaItemTributacaoCfop;
using MotorContingencia = Epros.ERP.DfeCalculos.Models.Vendas.VendaDocumentoContigencia;
using MotorReferenciadaNFe = Epros.ERP.DfeCalculos.Models.Vendas.VendaDocumentoDfeNumeroReferenciadaNFe;
using MotorTipoEmissao = NFe.Classes.Informacoes.Identificacao.Tipos.TipoEmissao;

using MotorCrt = NFe.Classes.Informacoes.Emitente.CRT;
using MotorEstado = DFe.Classes.Entidades.Estado;
using MotorModeloDocumento = DFe.Classes.Flags.ModeloDocumento;
using MotorTipoAmbiente = DFe.Classes.Flags.TipoAmbiente;
using MotorTipoNFe = NFe.Classes.Informacoes.Identificacao.Tipos.TipoNFe;
using MotorFinalidadeNFe = NFe.Classes.Informacoes.Identificacao.Tipos.FinalidadeNFe;
using MotorIndIeDest = NFe.Classes.Informacoes.Destinatario.indIEDest;
using MotorConsumidorFinal = NFe.Classes.Informacoes.Identificacao.Tipos.ConsumidorFinal;
using MotorFormaPagamento = NFe.Classes.Informacoes.Pagamento.FormaPagamento;
using MotorIndPagamento = NFe.Classes.Informacoes.Identificacao.Tipos.IndicadorPagamentoDetalhePagamento;
using MotorIndicadorTotal = NFe.Classes.Informacoes.Detalhe.IndicadorTotal;
using MotorDeterminacaoBaseIcmsSt = NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.Tipos.DeterminacaoBaseIcmsSt;
using MotorTipoReducao = Epros.ERP.Shared.Enums.ETipoReducaoBaseDeCalculo;

namespace Epros.Modules.Fiscal.Infrastructure.Services
{
    /// <summary>
    /// Mapeia um <see cref="DocumentoFiscal"/> do módulo Fiscal + <see cref="ContextoTransmissaoFiscal"/>
    /// (emitente/destinatário neutros) para o modelo <c>Venda</c> do motor legado, que é a entrada de
    /// <c>GeraXmlDfe.ObterNf(venda, versao)</c>.
    /// Reaproveita a mesma estratégia do adapter de cálculo (MotorLegadoCalculoFiscalService).
    /// </summary>
    public static class DocumentoFiscalVendaMapper
    {
        public static MotorVenda Mapear(DocumentoFiscal documento, ContextoTransmissaoFiscal contexto)
        {
            var emitente = MapearEmitente(contexto.Emitente);
            var destinatario = MapearDestinatario(documento, contexto.Destinatario);
            var vendaDoc = MapearDocumento(documento, contexto.Emitente);

            // Construtor completo do Venda legado (mesma assinatura usada pelo ERP legado ao montar a NF).
            var venda = new MotorVenda(
                naturezaOperacao: "Venda de mercadorias",
                valorDesconto: decimal.Zero,
                valorAcrescimo: decimal.Zero,
                valorSeguro: decimal.Zero,
                valorOutro: decimal.Zero,
                valorTotal: documento.Total,
                documento: vendaDoc,
                emitente: emitente,
                destinatario: destinatario!,
                transporte: null!,
                cobranca: null!,
                modalidadeFrete: NFe.Classes.Informacoes.Transporte.ModalidadeFrete.mfSemFrete,
                valorFrete: decimal.Zero,
                informacoesComplementares: null,
                informacoesAdicionaisFisco: null,
                localizadorExternoId: documento.Id.ToString(),
                documentoAutorizacaoXml: null,
                indicadorPresenca: NFe.Classes.Informacoes.Identificacao.Tipos.PresencaComprador.pcPresencial,
                indicadorIntermediador: NFe.Classes.Informacoes.Identificacao.Tipos.IndicadorIntermediador.iiSemIntermediador,
                entrega: null!,
                intermediador: null,
                imposto: new Epros.ERP.DfeCalculos.Models.Vendas.VendaImposto(decimal.Zero));

            foreach (var item in documento.Itens)
                MapearItem(venda, contexto.Emitente, documento, item);

            // Pagamento à vista (dinheiro) totalizando a nota — necessário para NFC-e/NF-e válida.
            venda.AdicionarPagamento(
                valorPago: documento.Total,
                valorTroco: decimal.Zero,
                formaPagamento: MotorFormaPagamento.fpDinheiro,
                indicacaoPagamento: MotorIndPagamento.ipDetPgVista,
                descricao: string.Empty,
                cartao: null);

            return venda;
        }

        private static MotorEmitente MapearEmitente(EmitenteFiscalDto e)
        {
            Enum.TryParse<MotorEstado>(e.Uf, ignoreCase: true, out var uf);

            var endereco = new MotorEmitenteEndereco(
                uf: uf,
                logradouro: e.Logradouro,
                numero: e.Numero,
                complemento: e.Complemento ?? string.Empty,
                bairro: e.Bairro,
                codMunicipioIbge: e.CodMunicipioIbge,
                nomeMunicipio: e.NomeMunicipio,
                cep: e.Cep);

            return new MotorEmitente(
                documento: e.Documento,
                regimeTributario: (MotorCrt)e.RegimeTributario,
                razaoSocial: e.RazaoSocial,
                nomeFantasia: e.NomeFantasia ?? string.Empty,
                ie: e.InscricaoEstadual ?? string.Empty,
                im: e.InscricaoMunicipal ?? string.Empty,
                cnae: e.Cnae ?? string.Empty,
                telefone: e.Telefone ?? string.Empty,
                linkLogoMarca: e.LinkLogoMarca,
                endereco: endereco);
        }

        private static MotorDestinatario? MapearDestinatario(DocumentoFiscal documento, DestinatarioFiscalDto? d)
        {
            // Prioriza o DTO de contexto; se ausente, usa os dados mínimos do próprio DocumentoFiscal.
            var doc = d?.Documento ?? documento.DestinatarioCnpjCpf;
            var nome = d?.Nome ?? documento.DestinatarioNome;

            if (string.IsNullOrWhiteSpace(doc))
                return null;

            MotorDestinatarioEndereco? endereco = null;
            if (d != null && !string.IsNullOrWhiteSpace(d.Uf) && !string.IsNullOrWhiteSpace(d.Logradouro))
            {
                Enum.TryParse<MotorEstado>(d.Uf, ignoreCase: true, out var ufDest);
                endereco = new MotorDestinatarioEndereco(
                    uf: ufDest,
                    logradouro: d.Logradouro!,
                    numero: d.Numero ?? string.Empty,
                    complemento: d.Complemento ?? string.Empty,
                    bairro: d.Bairro ?? string.Empty,
                    codMunicipioIbge: d.CodMunicipioIbge,
                    nomeMunicipio: d.NomeMunicipio ?? string.Empty,
                    cep: d.Cep ?? string.Empty);
            }

            return new MotorDestinatario(
                documento: doc,
                nome: nome,
                nomeFantasia: string.Empty,
                ie: d?.InscricaoEstadual ?? string.Empty,
                im: string.Empty,
                inscricaoSuframa: string.Empty,
                idIeDest: (MotorIndIeDest)(d?.IndicadorIeDestinatario ?? 9),
                email: d?.Email ?? string.Empty,
                telefone: d?.Telefone ?? string.Empty,
                consumidorFinal: (MotorConsumidorFinal)(d?.ConsumidorFinal ?? 1),
                idEstrangeiro: null,
                endereco: endereco!);
        }

        private static MotorDocumentoDfe MapearDocumento(DocumentoFiscal documento, EmitenteFiscalDto emitente)
        {
            var modelo = documento.Modelo == "65" ? MotorModeloDocumento.NFCe : MotorModeloDocumento.NFe;
            var ambiente = documento.Ambiente == 1 ? MotorTipoAmbiente.Producao : MotorTipoAmbiente.Homologacao;

            // Finalidade: 4 = Devolução -> exige chave referenciada (refNFe); demais -> Normal.
            // O caminho normal (Finalidade padrão 1) permanece exatamente como antes.
            var finalidade = documento.Finalidade == 4 ? MotorFinalidadeNFe.fnDevolucao : MotorFinalidadeNFe.fnNormal;

            System.Collections.Generic.ICollection<MotorReferenciadaNFe?>? chavesReferenciadas = null;
            if (finalidade == MotorFinalidadeNFe.fnDevolucao && !string.IsNullOrWhiteSpace(documento.ChaveReferenciada))
                chavesReferenciadas = new System.Collections.Generic.List<MotorReferenciadaNFe?>
                {
                    new MotorReferenciadaNFe(documento.ChaveReferenciada!)
                };

            // Contingência: só monta o objeto Contigencia quando tpEmis != Normal. Documento normal
            // continua com contigencia null -> o motor grava ide.tpEmis = teNormal (comportamento inalterado).
            MotorContingencia? contingencia = null;
            if (documento.EmContingencia && !string.IsNullOrWhiteSpace(documento.JustificativaContingencia))
            {
                var tpEmis = (MotorTipoEmissao)(int)documento.TipoEmissao;
                contingencia = new MotorContingencia(
                    tpEmis,
                    documento.JustificativaContingencia!,
                    documento.DataHoraContingencia ?? documento.DataEmissao);
            }

            // CertPath/CertPass são apenas validados como não-vazios pelo Validar() do modelo; a
            // transmissão real usa o certificado do ConfiguracaoServico (HerculesConfiguracaoFactory).
            return new MotorDocumentoDfe(
                tipoNFe: MotorTipoNFe.tnSaida,
                ambiente: ambiente,
                modeloDocumento: modelo,
                numeroNf: documento.Numero,
                serieNf: documento.Serie,
                certPath: "in-memory",
                certPass: "in-memory",
                csc: emitente.Csc ?? string.Empty,
                cscId: emitente.CscId ?? string.Empty,
                finalidadeNFe: finalidade,
                dataEmissao: documento.DataEmissao,
                dataHoraSaida: null,
                contigencia: contingencia!,
                chavesReferenciadasNFe: chavesReferenciadas!);
        }

        private static void MapearItem(MotorVenda venda, EmitenteFiscalDto emitente, DocumentoFiscal documento, DocumentoFiscalItem item)
        {
            var regimeSimples = emitente.RegimeTributario == 1 || emitente.RegimeTributario == 4;

            var tributacaoCfop = new MotorTributacaoCfop(item.Cfop, 0, true, false);

            venda.AdicionarItem(
                produtoId: 0,
                codigoProduto: item.Sku,
                nomeProduto: item.NomeProduto,
                codigoBarras: null,
                ncm: item.Ncm,
                tributacaoCfop: tributacaoCfop,
                unidade: "UN",
                valorUnitario: item.ValorUnitario,
                quantidade: item.Quantidade,
                origem: "0",
                csosn: regimeSimples ? item.Cst : null,
                cstIcms: regimeSimples ? null : item.Cst,
                valorAliquotaIcms: item.AliquotaIcms,
                valorReducaoIcmsPercentual: decimal.Zero,
                reducaoIcms: MotorTipoReducao.NaoUtiliza,
                valorBaseCalculoStRetidoOperacaoAnterior: decimal.Zero,
                valorAlioquotaSt: decimal.Zero,
                valorIcmsStRetidoOperacaoAnterior: decimal.Zero,
                valorIcmsProprioSubstituto: decimal.Zero,
                cstPisCofins: "07",
                valorAliquotaPis: decimal.Zero,
                valorAliquotaPisReal: decimal.Zero,
                valorAliquotaCofins: decimal.Zero,
                valorAliquotaCofinsReal: decimal.Zero,
                compoeValorTotal: MotorIndicadorTotal.ValorDoItemCompoeTotalNF,
                valorDesconto: decimal.Zero,
                enquadramentoIpi: null,
                cstIpi: null,
                valorAliquotaIpi: decimal.Zero,
                valorReducaoIpiPercentual: decimal.Zero,
                tipoReducaoIpi: MotorTipoReducao.NaoUtiliza,
                cest: null,
                valorAliquotaMva: decimal.Zero,
                valorAliquotaFcp: decimal.Zero,
                valorAliquotaFcpSt: decimal.Zero,
                valorAliquotaIcmsInterna: decimal.Zero,
                valorAliquotaIcmsInterestadual: decimal.Zero,
                valorReducaoIcmsPercentualSt: decimal.Zero,
                tipoReducaoIcmsSt: MotorTipoReducao.NaoUtiliza,
                ipiEmbutido: false,
                difalTipoCalculoPorDentro: false,
                embuteFrete: true,
                embuteSeguro: true,
                embuteAcrescimo: true,
                embuteOutro: true,
                tipoCalculoBaseIcmsSt: MotorDeterminacaoBaseIcmsSt.DbisMargemValorAgregado,
                valorUnitFixadoIcmsSt: decimal.Zero,
                cstIbsCbs: null,
                cClassTrib: null,
                qtdeBCMonoRetido: decimal.Zero,
                valorAliquotaAdRemRetido: decimal.Zero,
                informacoesAdProduto: null,
                numeroItemPedidoCompra: null,
                numeroPedidoCompra: string.Empty,
                fichaConteudoImportacao: string.Empty,
                unidadeTributavel: "UN",
                codigoBeneficioFiscal: null,
                valorCusto: decimal.Zero);
        }
    }
}
