using DFe.Classes.Flags;
using Epros.ERP.DfeCalculos.Impostos;
using Epros.ERP.DfeCalculos.Models.Vendas;
using Epros.ERP.Shared.Extensions;
using NFe.Classes;
using NFe.Classes.Informacoes;
using NFe.Classes.Informacoes.Cobranca;
using NFe.Classes.Informacoes.Destinatario;
using NFe.Classes.Informacoes.Detalhe;
using NFe.Classes.Informacoes.Detalhe.ProdEspecifico;
using NFe.Classes.Informacoes.Detalhe.Tributacao;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Federal;
using NFe.Classes.Informacoes.Emitente;
using NFe.Classes.Informacoes.Identificacao;
using NFe.Classes.Informacoes.Identificacao.Tipos;
using NFe.Classes.Informacoes.Observacoes;
using NFe.Classes.Informacoes.Pagamento;
using NFe.Classes.Informacoes.Total;
using NFe.Classes.Informacoes.Transporte;
using NFe.Utils;
using NFe.Utils.InformacoesSuplementares;
using NFe.Utils.NFe;
using Shared.NFe.Classes.Informacoes.InfRespTec;
using Shared.NFe.Classes.Informacoes.Intermediador;

namespace Epros.ERP.DfeCalculos.Models
{
    public class GeraXmlDfe
    {
        //private static Venda _venda;
        public static NFe.Classes.NFe ObterNf(Venda venda, VersaoServico versaoServico)
        {
            //_venda = venda;

            var nfe = new NFe.Classes.NFe();

            nfe.infNFe = ObterInf(venda, versaoServico);

            nfe.Assina();

            if (nfe.infNFe.ide.mod == ModeloDocumento.NFCe)
            {
                infNFeSupl supl = new infNFeSupl
                {
                    urlChave = nfe.infNFeSupl.ObterUrlConsulta(nfe, VersaoQrCode.QrCodeVersao2)
                };
                nfe.infNFeSupl = supl;
                nfe.infNFeSupl.qrCode = nfe.infNFeSupl.ObterUrlQrCode(nfe, VersaoQrCode.QrCodeVersao2, venda.Documento.CscId!.ToString(), venda.Documento.Csc);
            }

            return nfe;
        }

        public static infNFe ObterInf(Venda venda, VersaoServico versaoServico)
        {
            string InformacoesAdicionais = venda.AdicionalIbpt;

            var infNFe = new infNFe
            {
                versao = versaoServico.VersaoServicoParaString(),
                ide = ObterIdentificacao(venda, versaoServico),
                emit = ObterEmitente(venda.Emitente),
                dest = ObterDestinatario(venda.Destinatario, venda.Documento.Ambiente, versaoServico),
                autXML = ObterDocumentosAutorizacaoXml(venda.DocumentoAutorizacaoXml),
                transp = ObterTransporte(venda, venda.Emitente),
                det = ObterItens(venda, venda.Itens, venda.Documento.Ambiente),
                pag = ObterPagamentos(venda.Pagamentos),
                cobr = ObterCobranca(venda.Cobranca),
                infAdic = (string.IsNullOrEmpty(venda.InformacoesComplementares) && string.IsNullOrEmpty(venda.InformacoesAdicionaisFisco)) ? null : new infAdic { infCpl = (string.IsNullOrEmpty(venda.InformacoesComplementares) ? null : venda.InformacoesComplementares), infAdFisco = (string.IsNullOrEmpty(venda.InformacoesAdicionaisFisco) ? null : venda.InformacoesAdicionaisFisco) },
                infRespTec = new infRespTec
                {
                    CNPJ = "00028887000126",
                    email = "joao@siser.com.br",
                    xContato = "Joao Goncalves",
                    fone = "1145212524"
                },
                infIntermed = ObterInfIntermediador(venda.Intermediador),
                entrega = ObterEntrega(venda.Entrega),
                exporta = ObterExportacao(venda.Exportacao)
            };

            switch (infNFe.emit.CRT)
            {
                case CRT.SimplesNacional:
                case CRT.SimplesNacionalMei:
                    infNFe.total = ObterTotalNfeSimplesNacional(infNFe.det);
                    break;
                case CRT.SimplesNacionalExcessoSublimite:
                case CRT.RegimeNormal:
                    infNFe.total = ObterTotalNfeRegimeNormall(infNFe.det);
                    //infNFe.total.ICMSTot.vFrete = objNF.Venda.TotalFrete;
                    break;
            }

            //infNFe.infAdic.infCpl = InformacoesAdicionais;
            return infNFe;
        }

        public static ide ObterIdentificacao(Venda venda, VersaoServico versaoServico)
        {
            long codigo = venda.Documento.NumeroNf + 55;

            var ide = new ide
            {
                verProc = versaoServico.VersaoServicoParaString(),
                cUF = venda.Emitente.Endereco.Uf,
                mod = venda.Documento.ModeloDocumento,
                serie = venda.Documento.SerieNf,
                nNF = venda.Documento.NumeroNf,
                tpNF = venda.Documento.TipoNFe,
                cMunFG = venda.Emitente.Endereco.CodMunicipioIbge,
                cMunFGIBS = (venda.Documento.ModeloDocumento == ModeloDocumento.NFCe ? null : (venda.Destinatario != null ? (venda.Destinatario.Endereco.CodMunicipioIbge > 0 ? venda.Destinatario.Endereco.CodMunicipioIbge : venda.Emitente.Endereco.CodMunicipioIbge) : venda.Emitente.Endereco.CodMunicipioIbge)),
                cNF = codigo.ToString(),
                tpAmb = venda.Documento.Ambiente,
                dhEmi = venda.Documento.DataEmissao,
                dhSaiEnt = (venda.Documento.DataHoraSaida == null ? (DateTimeOffset?)null : new DateTimeOffset((DateTime)venda.Documento.DataHoraSaida)),
                procEmi = ProcessoEmissao.peAplicativoContribuinte,
                indFinal = (venda.Documento.ModeloDocumento == ModeloDocumento.NFCe ? ConsumidorFinal.cfConsumidorFinal : venda.Destinatario!.ConsumidorFinal),
                indPres = venda.IndicadorPresenca,
                finNFe = venda.Documento.FinalidadeNFe,
                indIntermed = venda.Documento.ModeloDocumento == ModeloDocumento.NFCe ? IndicadorIntermediador.iiSemIntermediador : (venda.IndicadorPresenca == PresencaComprador.pcNao ? null : venda.IndicadorIntermediador),
                natOp = (string.IsNullOrEmpty(venda.NaturezaOperacao) ? "1" : venda.NaturezaOperacao),
                idDest = (venda.Documento.ModeloDocumento == ModeloDocumento.NFCe ? DestinoOperacao.doInterna : venda.Emitente.Endereco.Uf == venda.Destinatario?.Endereco?.Uf ? DestinoOperacao.doInterna : venda.Destinatario?.Endereco?.Uf == DFe.Classes.Entidades.Estado.EX ? DestinoOperacao.doExterior : DestinoOperacao.doInterestadual),
                NFref = ObterNFref(venda.Documento),
                tpImp = (venda.Documento.ModeloDocumento == ModeloDocumento.NFe ? TipoImpressao.tiRetrato : TipoImpressao.tiNFCe)
            };

            if (venda.Documento.Contigencia != null && venda.Documento.Contigencia.IsValid)
            {
                ide.tpEmis = venda.Documento.Contigencia.TipoEmissao;
                ide.xJust = venda.Documento.Contigencia.Justificativa;
                ide.dhCont = venda.Documento.Contigencia.DataHora;
            }
            else
                ide.tpEmis = TipoEmissao.teNormal;

            return ide;
        }

        public static List<NFref> ObterNFref(VendaDocumentoDfe vendaDocumentoDfe)
        {
            if (vendaDocumentoDfe.ChavesReferenciadasNFe == null) return null!;
            var nFrefs = new List<NFref>();
            foreach (var chave in vendaDocumentoDfe.ChavesReferenciadasNFe)
                nFrefs.Add(new NFref { refNFe = chave?.ChaveReferenciadaNFe });
            return nFrefs;
        }

        public static emit ObterEmitente(VendaEmitente emitente)
        {
            if (emitente == null || string.IsNullOrEmpty(emitente.Documento))
                return null!;

            long.TryParse(emitente.Telefone, out long telefone);

            return new emit
            {
                CRT = emitente.RegimeTributario,
                CNPJ = emitente.Documento.Length == 11 ? null : emitente.Documento,
                CPF = emitente.Documento.Length == 14 ? null : emitente.Documento,
                IE = emitente.Ie,
                xFant = emitente.NomeFantasia,
                xNome = emitente.RazaoSocial,
                CNAE = emitente.Cnae,
                IM = emitente.Im,
                IEST = null,
                enderEmit = new enderEmit
                {
                    xLgr = emitente.Endereco.Logradouro,
                    nro = emitente.Endereco.Numero,
                    xCpl = emitente.Endereco.Complemento,
                    xBairro = emitente.Endereco.Bairro,
                    cMun = emitente.Endereco.CodMunicipioIbge,
                    xMun = emitente.Endereco.NomeMunicipio,
                    CEP = emitente.Endereco.Cep,
                    cPais = 1058,
                    xPais = "BRASIL",
                    UF = emitente.Endereco.Uf,
                    fone = telefone > 0 ? telefone : null,
                }
            };
        }

        public static infIntermed ObterInfIntermediador(VendaIntermediador? entrega)
        {
            if (entrega == null || string.IsNullOrEmpty(entrega.Documento))
                return null!;

            return new infIntermed
            {
                CNPJ = entrega.Documento,
                idCadIntTran = entrega.IdCadIntTran
            };
        }

        public static entrega ObterEntrega(VendaEntrega? entrega)
        {
            if (entrega == null || string.IsNullOrEmpty(entrega.Logradouro))
                return null!;

            long.TryParse(entrega.Fone, out long fone);

            long.TryParse(entrega.Cep, out long cep);

            return new entrega
            {
                xNome = entrega.Nome,
                CNPJ = entrega.Documento!.Length == 11 ? null : entrega.Documento,
                CPF = entrega.Documento.Length == 14 ? null : entrega.Documento,
                //IE = entrega.IE,
                email = entrega.Email,
                fone = string.IsNullOrEmpty(entrega.Fone) ? null : entrega.Fone,
                xLgr = entrega.Logradouro,
                nro = entrega.Numero,
                xCpl = entrega.Complemento,
                xBairro = entrega.Bairro,
                cMun = entrega.CodMunicipioIbge,
                xMun = entrega.NomeMunicipio,
                CEP = cep,
                cPais = entrega.CodPais,
                xPais = entrega.NomePais,
                UF = entrega.Uf.ToString()
            };
        }

        public static transp ObterTransporte(Venda venda, VendaEmitente emitente)
        {
            if (emitente == null || string.IsNullOrEmpty(emitente.Documento))
                return null!;

            transp? transp = null;
            if (emitente.RegimeTributario == CRT.SimplesNacionalMei)
            {
                transp = new transp { modFrete = venda.Tansporte == null && venda.ValorFrete == decimal.Zero ? ModalidadeFrete.mfSemFrete : ModalidadeFrete.mfContaDestinatario };
            }
            else if (venda.Tansporte == null)
            {
                transp = new transp { modFrete = ModalidadeFrete.mfSemFrete };
            }
            else if (venda.Tansporte != null && emitente.RegimeTributario != CRT.SimplesNacionalMei)
            {
                transp = new transp
                {
                    modFrete = venda.ModalidadeFrete,
                    transporta = (venda.Tansporte.Transportadora == null ? null : new transporta
                    {
                        CNPJ = venda.Tansporte.Transportadora.Cnpj,
                        xNome = venda.Tansporte.Transportadora.Nome,
                        CPF = venda.Tansporte.Transportadora.Cpf,
                        IE = venda.Tansporte.Transportadora.Ie,
                        UF = venda.Tansporte.Transportadora.Uf.ToString(),
                        xEnder = venda.Tansporte.Transportadora.Endereco,
                        xMun = venda.Tansporte.Transportadora.Municipio
                    }),
                    veicTransp = (venda.Tansporte.Veiculo == null ? null : new veicTransp
                    {
                        placa = venda.Tansporte.Veiculo.Placa,
                        UF = venda.Tansporte.Veiculo.Uf.ToString(),
                        RNTC = venda.Tansporte.Veiculo.Rntc
                    }),
                    vol = ObterVolumesTransporte(venda.Tansporte.Volumes),
                    reboque = ObterReboquesTransporte(venda.Tansporte.Reboques)
                };
            }

            //else if (vendaTransporte == null || vendaTransporte.ValorFrete == decimal.Zero && emitente.RegimeTributario != CRT.SimplesNacionalMei)
            //{
            //    transp = new transp { modFrete = ModalidadeFrete.mfSemFrete };
            //}
            //else if (vendaTransporte != null && vendaTransporte.ValorFrete > decimal.Zero && emitente.RegimeTributario != CRT.SimplesNacionalMei)
            //{
            //    transp = new transp
            //    {
            //        modFrete = vendaTransporte.ModalidadeFrete,
            //        transporta = (vendaTransporte.Transportadora == null ? null : new transporta
            //        {
            //            CNPJ = vendaTransporte.Transportadora.Cnpj,
            //            xNome = vendaTransporte.Transportadora.Nome,
            //            CPF = vendaTransporte.Transportadora.Cpf,
            //            IE = vendaTransporte.Transportadora.Ie,
            //            UF = vendaTransporte.Transportadora.Uf.ToString(),
            //            xEnder = vendaTransporte.Transportadora.Endereco,
            //            xMun = vendaTransporte.Transportadora.Municipio
            //        }),
            //        veicTransp = (vendaTransporte.Veiculo == null ? null : new veicTransp
            //        {
            //            placa = vendaTransporte.Veiculo.Placa,
            //            UF = vendaTransporte.Veiculo.Uf.ToString(),
            //            RNTC = vendaTransporte.Veiculo.Rntc
            //        }),
            //        vol = ObterVolumesTransporte(vendaTransporte.Volumes)                    
            //    };
            //}

            return transp!;
        }

        public static dest ObterDestinatario(VendaDestinatario destinatario, TipoAmbiente tipoAmbiente, VersaoServico versaoServico)
        {
            if (destinatario == null)
                return null!;

            long.TryParse(destinatario.Telefone, out long telefone);

            return new dest(versaoServico)
            {
                CNPJ = destinatario.Endereco != null && destinatario.Endereco.Uf == DFe.Classes.Entidades.Estado.EX ? "" : destinatario.Documento!.Length == 11 ? null : destinatario.Documento,
                CPF = destinatario.Endereco != null && destinatario.Endereco.Uf == DFe.Classes.Entidades.Estado.EX ? "" : destinatario.Documento!.Length == 14 ? null : destinatario.Documento,
                IE = destinatario.Ie,
                xNome = tipoAmbiente == TipoAmbiente.Homologacao ? "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL" : destinatario.Nome,
                indIEDest = destinatario.IdIeDest,
                email = destinatario.Email,
                idEstrangeiro = string.IsNullOrEmpty(destinatario.IdEstrangeiro) ? (destinatario.Endereco != null && destinatario.Endereco.Uf == DFe.Classes.Entidades.Estado.EX ? "" : null) : destinatario.IdEstrangeiro,
                IM = destinatario.Im,
                ISUF = destinatario.InscricaoSuframa,
                enderDest = destinatario.Endereco == null ? null : new enderDest
                {
                    xLgr = destinatario.Endereco.Logradouro,
                    nro = destinatario.Endereco.Numero,
                    xCpl = destinatario.Endereco.Complemento,
                    xBairro = destinatario.Endereco.Bairro,
                    cMun = destinatario.Endereco.CodMunicipioIbge,
                    xMun = destinatario.Endereco.NomeMunicipio,
                    CEP = destinatario.Endereco.Cep,
                    cPais = destinatario.Endereco.CodPais,
                    xPais = destinatario.Endereco.NomePais,
                    UF = destinatario?.Endereco.Uf.ToString(),
                    fone = telefone > 0 ? telefone : null,
                }
            };
        }


        public static exporta ObterExportacao(VendaExportacao? exportacao)
        {
            if (exportacao == null) return null!;

            return new exporta
            {
                UFSaidaPais = exportacao.UfSaidaPais,
                xLocExporta = exportacao.LocalExportacao,
                xLocDespacho = string.IsNullOrEmpty(exportacao.LocalDespacho) ? null : exportacao.LocalDespacho
            };
        }


        public static List<det> ObterItens(Venda venda, IEnumerable<VendaItem> itens, TipoAmbiente tipoAmbiente)
        {
            if (itens == null || itens.Count() == 0)
                return null!;

            var dets = new List<det>();
            int seq = 0;
            foreach (var item in itens)
            {
                var impostoIcms = MontaImpostoHercules.ObterICMS(venda, item);
                var impostoIcmsUfDest = MontaImpostoHercules.ObterICMSUfDestinatario(venda, item);
                var impostoPis = MontaImpostoHercules.ObterPIS(item);
                var impostoCofins = MontaImpostoHercules.ObterCOFINS(item);
                var impostoIpi = MontaImpostoHercules.ObterIPI(item);
                var impostoIbsCbs = MontaImpostoHercules.ObterIBSCBS(item);

                var itemDet = new det
                {
                    nItem = ++seq,
                    prod = new prod
                    {
                        cProd = item.CodigoProduto,
                        cEAN = string.IsNullOrEmpty(item.CodigoBarras) ? "SEM GTIN" : item.CodigoBarras,
                        xProd = tipoAmbiente == TipoAmbiente.Homologacao && seq == 1 ? "NOTA FISCAL EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL" : item.NomeProduto,
                        NCM = item.Ncm,
                        CFOP = item.TributacaoCfop.Cfop,
                        uCom = item.Unidade,
                        qCom = item.Quantidade,
                        vUnCom = item.ValorUnitario,
                        vProd = item.ValorItem,
                        cEANTrib = string.IsNullOrEmpty(item.CodigoBarras) ? "SEM GTIN" : item.CodigoBarras,
                        uTrib = item.UnidadeTributavel,
                        qTrib = item.Quantidade,
                        vUnTrib = item.ValorUnitario,
                        indTot = item.CompoeValorTotal,
                        vFrete = item.ValorFreteRateado,
                        vDesc = item.ValorDescontoRateado + item.ValorDesconto,
                        vOutro = item.ValorOutroRateado + item.ValorAcrescimoRateado,
                        vSeg = item.ValorSeguroRateado,
                        CEST = item.Cest,
                        nItemPed = item.NumeroItemPedidoCompra,
                        xPed = item.NumeroPedidoCompra,
                        nFCI = item.FichaConteudoImportacao,
                        cBenef = item.CodigoBeneficioFiscal,
                        ProdutoEspecifico = item.TributacaoCfop.Cfop == 5656 && item.Combustivel != null ?
                        [
                            new comb
                            {
                                cProdANP = item.Combustivel.CodigoAnp,
                                descANP = item.Combustivel.DescricaoAnp,
                                UFCons = item.Combustivel.UFCons,
                                pGLP = item.Combustivel.PGLP,
                                pGNn = item.Combustivel.PGNn,
                                pGNi = item.Combustivel.PGNi,
                                vPart = item.Combustivel.VPart,
                                pBio = item.Combustivel.PBio,
                                origComb = item.Combustivel.Origens
                                    .Select(oc => new origComb
                                    {
                                        indImport = oc.IndImport,
                                        cUFOrig = oc.CUFOrig,
                                        pOrig = oc.POrig
                                    }).ToList()
                            }
                        ] : null
                    },
                    infAdProd = item.InformacoesAdProduto,
                    vItem = item.ValorItem +
                         item.ValorFreteRateado +
                         item.ValorSeguroRateado -
                         (item.ValorDesconto + item.ValorDescontoRateado) +
                         item.ValorOutroRateado +
                         decimal.Zero + // vIBS
                         (item.Imposto.IbsCbs == null ? decimal.Zero : item.Imposto.IbsCbs.ValorImpostoDevidoCbs) + // vCBS
                         decimal.Zero + // vIS
                         decimal.Zero + // vMono
                         decimal.Zero // vTribCompraGov
                };

                itemDet.imposto = new imposto
                {
                    vTotTrib = item.ValorAproximadoIbpt,
                    ICMS = new ICMS
                    {
                        TipoICMS = impostoIcms,
                    },
                    IPI = (impostoIpi == null ? null : impostoIpi),
                    PIS = new PIS
                    {
                        TipoPIS = impostoPis
                    },

                    COFINS = new COFINS
                    {
                        TipoCOFINS = impostoCofins
                    },

                    ICMSUFDest = impostoIcmsUfDest == null ? null : impostoIcmsUfDest,

                    IBSCBS = (impostoIbsCbs == null ? null : impostoIbsCbs)
                };

                dets.Add(itemDet);
            }

            return dets;
        }

        public static List<pag> ObterPagamentos(IEnumerable<VendaPagamento> pagamentos)
        {
            if (pagamentos == null || pagamentos.Count() == 0)
                return null!;

            var pags = new List<pag>();

            pag pag = new pag();

            pag.detPag = new List<detPag>();

            foreach (var pg in pagamentos)
            {
                pag.detPag.Add(new detPag
                {
                    indPag = pg.IndicacaoPagamento,
                    tPag = pg.FormaPagamento,
                    vPag = pg.ValorPago,
                    xPag = pg.Descricao,
                    card = (pg.Cartao == null ? null : new card
                    {
                        tpIntegra = pg.Cartao.TipoIntegracaoPagamento,
                        tBand = pg.Cartao.BandeiraCartao,
                        cAut = pg.Cartao.NumeroAutorizacaoOperaCartao
                    })
                });

                if (pg.ValorTroco > decimal.Zero)
                {
                    pag.vTroco = pag.vTroco == null ? decimal.Zero : pag.vTroco;
                    pag.vTroco += pg.ValorTroco;
                }
            }

            pags.Add(pag);
            return pags;
        }

        private static total ObterTotalNfeSimplesNacional(List<det> produtos)
        {
            decimal vBCST = 0;
            decimal vST = 0;
            decimal vIPI = 0;
            decimal vPIS = 0;
            decimal vCOFINS = 0;
            decimal qBCMonoRet = decimal.Zero;
            decimal vICMSMonoRet = decimal.Zero;

            // IBS/CBS
            decimal vBCIBSCBSSoma = 0;
            // gIBSUF
            decimal pIBSUFSoma = 0;
            decimal vIBSUFSoma = 0;
            // gIBSMun
            decimal pIBSMunSoma = 0;
            decimal vIBSMunSoma = 0;
            // gCBS
            decimal pCBSSoma = 0;
            decimal vCBSSoma = 0;

            foreach (var prodx in produtos)
            {
                if (prodx.imposto.ICMS.TipoICMS.GetType() == typeof(ICMS61))
                {
                    qBCMonoRet += (prodx.imposto.ICMS.TipoICMS as ICMS61)!.qBCMonoRet ?? decimal.Zero;
                    vICMSMonoRet += (prodx.imposto.ICMS.TipoICMS as ICMS61)!.vICMSMonoRet ?? decimal.Zero;
                }

                if (prodx.imposto.ICMS.TipoICMS.GetType() == typeof(ICMSSN201))
                {
                    vBCST += (prodx.imposto.ICMS.TipoICMS as ICMSSN201)!.vBCST;
                    vST += (prodx.imposto.ICMS.TipoICMS as ICMSSN201)!.vICMSST;
                }

                if (prodx.imposto.ICMS.TipoICMS.GetType() == typeof(ICMSSN202))
                {
                    vBCST += (prodx.imposto.ICMS.TipoICMS as ICMSSN202)!.vBCST;
                    vST += (prodx.imposto.ICMS.TipoICMS as ICMSSN202)!.vICMSST;
                }

                if (prodx.imposto.IPI != null && prodx.imposto.IPI.TipoIPI.GetType() == typeof(IPITrib))
                    vIPI = vIPI + (((IPITrib)prodx.imposto.IPI.TipoIPI).vIPI ?? 0);

                if (prodx.imposto.PIS != null && prodx.imposto.PIS.TipoPIS.GetType() == typeof(PISAliq))
                    vPIS = vPIS + ((PISAliq)prodx.imposto.PIS.TipoPIS).vPIS;

                if (prodx.imposto.COFINS != null && prodx.imposto.COFINS.TipoCOFINS.GetType() == typeof(COFINSAliq))
                    vCOFINS = vCOFINS + ((COFINSAliq)prodx.imposto.COFINS.TipoCOFINS).vCOFINS;

                // IBS/CBS
                if (prodx.imposto.IBSCBS != null && prodx.imposto.IBSCBS.GetType() == typeof(IBSCBS))
                {
                    if (prodx.imposto.IBSCBS.gIBSCBS != null)
                    {
                        // IBS/CBS
                        vBCIBSCBSSoma += prodx.imposto.IBSCBS.gIBSCBS.vBC;
                        // gIBSUF
                        pIBSUFSoma += prodx.imposto.IBSCBS.gIBSCBS.gIBSUF.pIBSUF;
                        vIBSUFSoma += prodx.imposto.IBSCBS.gIBSCBS.gIBSUF.vIBSUF;
                        // gIBSMun
                        pIBSMunSoma += prodx.imposto.IBSCBS.gIBSCBS.gIBSMun.pIBSMun;
                        vIBSMunSoma += prodx.imposto.IBSCBS.gIBSCBS.gIBSMun.vIBSMun;
                        // gCBS
                        pCBSSoma += prodx.imposto.IBSCBS.gIBSCBS.gCBS.pCBS;
                        vCBSSoma += prodx.imposto.IBSCBS.gIBSCBS.gCBS.vCBS;
                    }
                }
            }

            return new total
            {
                ICMSTot = new ICMSTot
                {
                    vProd = produtos.Sum(p => p.prod.vProd),
                    vNF = produtos.Sum(p => p.prod.vProd) + vST + produtos.Sum(p => p.prod.vFrete ?? 0) + produtos.Sum(p => p.prod.vOutro ?? 0) + produtos.Sum(p => p.prod.vSeg ?? 0) - produtos.Sum(p => p.prod.vDesc ?? 0),
                    vBCST = vBCST,
                    vST = vST,
                    vDesc = produtos.Sum(p => p.prod.vDesc ?? 0),
                    vTotTrib = produtos.Sum(p => p.imposto.vTotTrib ?? 0),
                    vFrete = produtos.Sum(p => p.prod.vFrete ?? 0),
                    vOutro = produtos.Sum(p => p.prod.vOutro ?? 0),
                    vSeg = produtos.Sum(p => p.prod.vSeg ?? 0),
                    vBC = 0,
                    vICMSDeson = 0,
                    vICMS = 0,
                    vCOFINS = vCOFINS,
                    vPIS = vPIS,
                    vIPI = vIPI,
                    vFCPUFDest = 0,
                    vICMSUFDest = 0,
                    vICMSUFRemet = 0,
                    vFCP = 0,
                    vFCPST = 0,
                    vFCPSTRet = 0,
                    vIPIDevol = 0,
                    qBCMonoRet = qBCMonoRet,
                    vICMSMonoRet = vICMSMonoRet
                },
                IBSCBSTot = vBCIBSCBSSoma == decimal.Zero ? null : new IBSCBSTot
                {
                    vBCIBSCBS = vBCIBSCBSSoma,
                    gIBS = new gIBSTotal
                    {
                        gIBSUF = new gIBSUFTotal
                        {
                            vDif = decimal.Zero, //Math.Round(pIBSUFSoma, 2),
                            vDevTrib = decimal.Zero,
                            vIBSUF = vIBSUFSoma.Arredonda2()
                        },
                        gIBSMun = new gIBSMunTotal
                        {
                            vDif = decimal.Zero,
                            vDevTrib = decimal.Zero,
                            vIBSMun = decimal.Zero
                        },
                        vIBS = vIBSUFSoma.Arredonda2(),
                        vCredPres = decimal.Zero,
                        vCredPresCondSus = decimal.Zero
                    },
                    gCBS = new gCBSTotal
                    {
                        vDif = decimal.Zero,
                        vDevTrib = decimal.Zero,
                        vCBS = vCBSSoma,
                        vCredPres = decimal.Zero,
                        vCredPresCondSus = decimal.Zero
                    }
                }
            };
        }

        public static total ObterTotalNfeRegimeNormall(List<det> produtos)
        {
            decimal vBC = decimal.Zero;
            decimal vBCST = decimal.Zero;
            decimal vICMS = decimal.Zero;
            decimal vST = decimal.Zero;
            decimal vFCPST = decimal.Zero;
            decimal vIPI = decimal.Zero;
            decimal vPIS = decimal.Zero;
            decimal vCOFINS = decimal.Zero;
            decimal qBCMonoRet = decimal.Zero;
            decimal vICMSMonoRet = decimal.Zero;

            // IBS/CBS
            decimal vBCIBSCBSSoma = 0;
            // gIBSUF
            decimal pIBSUFSoma = 0;
            decimal vIBSUFSoma = 0;
            // gIBSMun
            decimal pIBSMunSoma = 0;
            decimal vIBSMunSoma = 0;
            // gCBS
            decimal pCBSSoma = 0;
            decimal vCBSSoma = 0;

            foreach (var prodx in produtos)
            {
                // ICMS
                if (prodx.imposto.ICMS.TipoICMS.GetType() == typeof(ICMS00))
                {
                    vBC += prodx.imposto.ICMS.TipoICMS.GetIcmsBcValue();
                    vICMS += (prodx.imposto.ICMS.TipoICMS as ICMS00)!.vICMS;
                }
                if (prodx.imposto.ICMS.TipoICMS.GetType() == typeof(ICMS10)) // ICMS E ICMS ST
                {
                    vBC += prodx.imposto.ICMS.TipoICMS.GetIcmsBcValue();
                    vICMS += (prodx.imposto.ICMS.TipoICMS as ICMS10)!.vICMS;
                    vST += (prodx.imposto.ICMS.TipoICMS as ICMS10)!.vICMSST;
                    vBCST += (prodx.imposto.ICMS.TipoICMS as ICMS10)!.vBCST;
                    vFCPST += (prodx.imposto.ICMS.TipoICMS as ICMS10)!.vFCPST ?? decimal.Zero;
                }
                if (prodx.imposto.ICMS.TipoICMS.GetType() == typeof(ICMS20))
                {
                    vBC += prodx.imposto.ICMS.TipoICMS.GetIcmsBcValue();
                    vICMS += (prodx.imposto.ICMS.TipoICMS as ICMS20)!.vICMS;
                }
                if (prodx.imposto.ICMS.TipoICMS.GetType() == typeof(ICMS51))
                {
                    vBC += prodx.imposto.ICMS.TipoICMS.GetIcmsBcValue();
                    vICMS += (prodx.imposto.ICMS.TipoICMS as ICMS51)!.vICMS ?? decimal.Zero;
                }
                if (prodx.imposto.ICMS.TipoICMS.GetType() == typeof(ICMS61))
                {
                    qBCMonoRet += (prodx.imposto.ICMS.TipoICMS as ICMS61)!.qBCMonoRet ?? decimal.Zero;
                    vICMSMonoRet += (prodx.imposto.ICMS.TipoICMS as ICMS61)!.vICMSMonoRet ?? decimal.Zero;
                }

                //ICMS ST
                if (prodx.imposto.ICMS.TipoICMS.GetType() == typeof(ICMS30))
                {
                    vBC += prodx.imposto.ICMS.TipoICMS.GetIcmsBcValue();
                    vST += (prodx.imposto.ICMS.TipoICMS as ICMS30)!.vICMSST;
                    vBCST += (prodx.imposto.ICMS.TipoICMS as ICMS30)!.vBCST;
                }
                if (prodx.imposto.ICMS.TipoICMS.GetType() == typeof(ICMS70))
                {
                    vBC += prodx.imposto.ICMS.TipoICMS.GetIcmsBcValue();
                    vST += (prodx.imposto.ICMS.TipoICMS as ICMS70)!.vICMSST;
                    vBCST += (prodx.imposto.ICMS.TipoICMS as ICMS70)!.vBCST;
                    vICMS += (prodx.imposto.ICMS.TipoICMS as ICMS70)!.vICMS;
                }

                if (prodx.imposto.IPI != null && prodx.imposto.IPI.TipoIPI.GetType() == typeof(IPITrib))
                    vIPI = vIPI + (((IPITrib)prodx.imposto.IPI.TipoIPI).vIPI ?? 0);

                //COFINS                
                if (prodx.imposto.COFINS != null && prodx.imposto.COFINS.TipoCOFINS.GetType() == typeof(COFINSAliq))
                    vCOFINS = vCOFINS + ((COFINSAliq)prodx.imposto.COFINS.TipoCOFINS).vCOFINS;

                //PIS
                if (prodx.imposto.PIS != null && prodx.imposto.PIS.TipoPIS.GetType() == typeof(PISAliq))
                    vPIS = vPIS + ((PISAliq)prodx.imposto.PIS.TipoPIS).vPIS;

                // IBS/CBS
                if (prodx.imposto.IBSCBS != null && prodx.imposto.IBSCBS.GetType() == typeof(IBSCBS))
                {
                    if (prodx.imposto.IBSCBS.gIBSCBS != null)
                    {
                        // IBS/CBS
                        vBCIBSCBSSoma += prodx.imposto.IBSCBS.gIBSCBS.vBC;
                        // gIBSUF
                        pIBSUFSoma += prodx.imposto.IBSCBS.gIBSCBS.gIBSUF.pIBSUF;
                        vIBSUFSoma += prodx.imposto.IBSCBS.gIBSCBS.gIBSUF.vIBSUF;
                        // gIBSMun
                        pIBSMunSoma += prodx.imposto.IBSCBS.gIBSCBS.gIBSMun.pIBSMun;
                        vIBSMunSoma += prodx.imposto.IBSCBS.gIBSCBS.gIBSMun.vIBSMun;
                        // gCBS
                        pCBSSoma += prodx.imposto.IBSCBS.gIBSCBS.gCBS.pCBS;
                        vCBSSoma += prodx.imposto.IBSCBS.gIBSCBS.gCBS.vCBS;
                    }
                }
            }
            return new total
            {
                ICMSTot = new ICMSTot
                {
                    vProd = produtos.Sum(p => p.prod.vProd),
                    vNF = produtos.Sum(p => p.prod.vProd) + vST + vIPI + vFCPST + produtos.Sum(p => p.prod.vFrete ?? 0) + produtos.Sum(p => p.prod.vOutro ?? 0) + produtos.Sum(p => p.prod.vSeg ?? 0) - produtos.Sum(p => p.prod.vDesc ?? 0),
                    vDesc = produtos.Sum(p => p.prod.vDesc ?? 0),
                    vTotTrib = produtos.Sum(p => p.imposto.vTotTrib ?? 0),
                    vBC = vBC,
                    vICMS = vICMS,
                    vST = vST,
                    vFrete = produtos.Sum(p => p.prod.vFrete ?? 0),
                    vOutro = produtos.Sum(p => p.prod.vOutro ?? 0),
                    vSeg = produtos.Sum(p => p.prod.vSeg ?? 0),
                    vICMSDeson = 0,
                    vBCST = vBCST,
                    vCOFINS = vCOFINS,
                    vIPI = vIPI,
                    vPIS = vPIS,
                    vFCPUFDest = produtos.Sum(p => p.imposto.ICMSUFDest == null ? 0 : p.imposto.ICMSUFDest.vFCPUFDest),
                    vICMSUFDest = produtos.Sum(p => p.imposto.ICMSUFDest == null ? 0 : p.imposto.ICMSUFDest.vICMSUFDest),
                    vICMSUFRemet = produtos.Sum(p => p.imposto.ICMSUFDest == null ? 0 : p.imposto.ICMSUFDest.vICMSUFRemet),
                    vFCP = 0,
                    vFCPST = 0,
                    vFCPSTRet = 0,
                    vIPIDevol = 0,
                    qBCMonoRet = qBCMonoRet,
                    vICMSMonoRet = vICMSMonoRet
                },
                IBSCBSTot = vBCIBSCBSSoma == decimal.Zero ? null : new IBSCBSTot
                {
                    vBCIBSCBS = vBCIBSCBSSoma,
                    gIBS = new gIBSTotal
                    {
                        gIBSUF = new gIBSUFTotal
                        {
                            vDif = decimal.Zero, //Math.Round(pIBSUFSoma, 2),
                            vDevTrib = decimal.Zero,
                            vIBSUF = vIBSUFSoma.Arredonda2()
                        },
                        gIBSMun = new gIBSMunTotal
                        {
                            vDif = decimal.Zero,
                            vDevTrib = decimal.Zero,
                            vIBSMun = decimal.Zero
                        },
                        vIBS = vIBSUFSoma.Arredonda2(),
                        vCredPres = decimal.Zero,
                        vCredPresCondSus = decimal.Zero
                    },
                    gCBS = new gCBSTotal
                    {
                        vDif = decimal.Zero,
                        vDevTrib = decimal.Zero,
                        vCBS = vCBSSoma,
                        vCredPres = decimal.Zero,
                        vCredPresCondSus = decimal.Zero
                    }
                },
                vNFTot = vBCIBSCBSSoma == decimal.Zero ? null : produtos.Sum(p => p.prod.vProd) +
                         produtos.Sum(p => p.prod.vFrete ?? 0) +
                         produtos.Sum(p => p.prod.vSeg ?? 0) -
                         produtos.Sum(p => p.prod.vDesc ?? 0) +
                         produtos.Sum(p => p.prod.vOutro ?? 0) +
                         decimal.Zero + // vIBS
                         vCBSSoma + // vCBS
                         decimal.Zero + // vIS
                         decimal.Zero + // vMono
                         decimal.Zero // vTribCompraGov
            };
        }

        public static List<autXML> ObterDocumentosAutorizacaoXml(string[]? documentoAutorizacaoXml)
        {
            if (documentoAutorizacaoXml == null || documentoAutorizacaoXml.Count() == 0)
                return null!;

            var docs = new List<autXML>();

            foreach (var doc in documentoAutorizacaoXml)
            {
                docs.Add(new autXML
                {
                    CNPJ = doc.Length == 14 ? doc : null,
                    CPF = doc.Length == 11 ? doc : null
                });
            }

            return docs;
        }

        public static cobr ObterCobranca(VendaCobranca cobranca)
        {
            if (cobranca == null)
                return null!;

            var cobr = new cobr();

            cobr.fat = new fat();
            cobr.fat.nFat = cobranca.Fatura.NumeroFatura;
            cobr.fat.vOrig = cobranca.Fatura.ValorOriginal;
            cobr.fat.vDesc = cobranca.Fatura.ValorDesconto;
            cobr.fat.vLiq = cobranca.Fatura.ValorLiquido;

            cobr.dup = new List<dup>();
            foreach (var dupl in cobranca.Duplicatas)
            {
                cobr.dup.Add(new dup
                {
                    nDup = dupl.NumeroDuplicata,
                    dVenc = dupl.DataVencimento,
                    vDup = dupl.ValorDuplicata
                });
            }

            return cobr;
        }

        private static List<vol> ObterVolumesTransporte(IEnumerable<VendaTransporteVolume>? volumes)
        {
            if (volumes == null || volumes.Count() == 0) return null!;

            var vols = new List<vol>();
            foreach (var volume in volumes)
            {
                vols.Add(new vol
                {
                    qVol = volume.QuantidadeVolumes == 0 ? null : volume.QuantidadeVolumes,
                    nVol = string.IsNullOrEmpty(volume.NumeroVolumes) ? null : volume.NumeroVolumes,
                    esp = string.IsNullOrEmpty(volume.Especie) ? null : volume.Especie,
                    pesoB = volume.PesoBruto == 0 ? null : volume.PesoBruto,
                    pesoL = volume.PesoLiquido == 0 ? null : volume.PesoLiquido,
                    marca = string.IsNullOrEmpty(volume.Marca) ? null : volume.Marca
                });
            }

            return vols;
        }

        private static List<reboque> ObterReboquesTransporte(IEnumerable<VendaTransporteReboque>? reboques)
        {
            if (reboques == null || reboques.Count() == 0) return null!;

            var rebs = new List<reboque>();
            foreach (var reb in reboques)
            {
                rebs.Add(new reboque
                {
                    placa = string.IsNullOrEmpty(reb.Placa) ? null : reb.Placa,
                    UF = reb.Uf == null ? null : reb.Uf.ToString(),
                    RNTC = string.IsNullOrEmpty(reb.Rntc) ? null : reb.Rntc
                });
            }

            return rebs;
        }
    }
}
