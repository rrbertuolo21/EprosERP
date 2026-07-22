using Epros.ERP.DfeCalculos.Models.Vendas;
using NFe.Classes.Informacoes.Destinatario;
using NFe.Classes.Informacoes.Detalhe.Tributacao;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.Tipos;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Federal;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Federal.Tipos;
using NFe.Classes.Informacoes.Emitente;

namespace Epros.ERP.DfeCalculos.Impostos
{
    public class MontaImpostoHercules
    {
        public static ICMSBasico ObterICMS(Venda venda, VendaItem vendaItem)
        {
            if (vendaItem.RegimeTributario == CRT.SimplesNacional || vendaItem.RegimeTributario == CRT.SimplesNacionalMei)
            {
                var icms = vendaItem.Imposto!.Icms!; //CalculaCsosn.ObterIcmsCsosn(venda, vendaItem);

                switch (vendaItem.Csosn)
                {
                    case "61":
                        return new ICMS61
                        {
                            CST = (Csticms)Enum.Parse(typeof(Csticms), $"Cst{icms.Cst}"),
                            orig = (OrigemMercadoria)Enum.Parse(typeof(OrigemMercadoria), icms.Origem!),
                            qBCMonoRet = icms.QtdeBCMonoRetido,
                            adRemICMSRet = icms.ValorAliquotaAdRemRetido,
                            vICMSMonoRet = icms.ValorImpostoMonoRetido
                        };
                    case "101":
                        return new ICMSSN101
                        {
                            CSOSN = (Csosnicms)Enum.Parse(typeof(Csosnicms), icms.Csosn!),
                            orig = (OrigemMercadoria)Enum.Parse(typeof(OrigemMercadoria), icms.Origem!),
                            pCredSN = icms.Aliquota,
                            vCredICMSSN = icms.ValorCredito,
                        };
                    case "102":
                    case "103":
                    case "300":
                    case "400":
                        return new ICMSSN102
                        {
                            CSOSN = (Csosnicms)Enum.Parse(typeof(Csosnicms), icms.Csosn!),
                            orig = (OrigemMercadoria)Enum.Parse(typeof(OrigemMercadoria), icms.Origem!),
                        };
                    case "201":
                        return new ICMSSN201
                        {
                            CSOSN = (Csosnicms)Enum.Parse(typeof(Csosnicms), icms.Csosn!),
                            orig = (OrigemMercadoria)Enum.Parse(typeof(OrigemMercadoria), icms.Origem!),
                            modBCST = DeterminacaoBaseIcmsSt.DbisMargemValorAgregado,
                            pCredSN = icms.Aliquota,
                            vCredICMSSN = icms.ValorCredito,
                            //pRedBCST = PercentualReducaoBcICMSST                          
                            //pFCPST = decimal.Zero,
                            pICMSST = icms.AliquotaSt,
                            vBCST = icms.ValorBaseDeCalculoSt,
                            //vFCPST = decimal.Zero,
                            vICMSST = icms.ValorImpostoDevidoRecolherSt,
                            pMVAST = icms.AliquotaMva
                        };
                    case "202":
                        //var vICMS = BaseDeCalculo * (AliquotaICMS / 100);
                        //var  vBCIcmsST = (BaseDeCalculo + ValorIPI) * ((100 + MVA) / 100);
                        return new ICMSSN202
                        {
                            CSOSN = (Csosnicms)Enum.Parse(typeof(Csosnicms), icms.Csosn!),
                            orig = (OrigemMercadoria)Enum.Parse(typeof(OrigemMercadoria), icms.Origem!),
                            modBCST = DeterminacaoBaseIcmsSt.DbisMargemValorAgregado,
                            //pFCPST = decimal.Zero,
                            pICMSST = icms.AliquotaSt,
                            vBCST = icms.ValorBaseDeCalculoSt,
                            //vFCPST = decimal.Zero,
                            vICMSST = icms.ValorImpostoDevidoRecolherSt,
                            pMVAST = icms.AliquotaMva
                        };
                    case "203":
                        return new ICMSSN202
                        {
                            CSOSN = (Csosnicms)Enum.Parse(typeof(Csosnicms), icms.Csosn!),
                            orig = (OrigemMercadoria)Enum.Parse(typeof(OrigemMercadoria), icms.Origem!),
                            modBCST = DeterminacaoBaseIcmsSt.DbisMargemValorAgregado,
                            //pFCPST = decimal.Zero,
                            pICMSST = icms.AliquotaSt,
                            vBCST = icms.ValorBaseDeCalculoSt,
                            //vFCPST = decimal.Zero,
                            vICMSST = icms.ValorImpostoDevidoRecolherSt,
                            pMVAST = icms.AliquotaMva
                        };
                    case "500":
                        return new ICMSSN500
                        {
                            CSOSN = (Csosnicms)Enum.Parse(typeof(Csosnicms), icms.Csosn!),
                            orig = (OrigemMercadoria)Enum.Parse(typeof(OrigemMercadoria), icms.Origem!),
                            vBCSTRet = decimal.Zero,
                            pST = decimal.Zero,
                            vICMSSubstituto = decimal.Zero,
                            vICMSSTRet = decimal.Zero
                        };
                    case "900":
                        return new ICMSSN900
                        {
                            CSOSN = (Csosnicms)Enum.Parse(typeof(Csosnicms), icms.Csosn!),
                            orig = (OrigemMercadoria)Enum.Parse(typeof(OrigemMercadoria), icms.Origem!)
                            //pCredSN = icms.Aliquota,
                            //vCredICMSSN = icms.ValorCredito,
                            //modBC = DeterminacaoBaseIcms.DbiValorOperacao,
                            //modBCST = DeterminacaoBaseIcmsSt.DbisMargemValorAgregado,
                            ////pFCPST = decimal.Zero,
                            ////pICMSST = icms.AliquotaSt,
                            ////vBCST = icms.ValorBaseDeCalculoSt,
                            //vFCPST = decimal.Zero,
                            //vICMS = icms.ValorImpostoDevido,
                            ////vICMSST = icms.ValorImpostoDevidoRecolherSt,
                            ////pMVAST = icms.AliquotaMva
                            //vBC = decimal.Zero,
                            //pRedBC = decimal.Zero,
                            //pICMS = decimal.Zero
                        };
                    default:
                        new Exception("CSOSN ICMS inválido");
                        break;
                }
            }
            else if (vendaItem.RegimeTributario == CRT.SimplesNacionalExcessoSublimite || vendaItem.RegimeTributario == CRT.RegimeNormal)
            {
                var icms = vendaItem.Imposto!.Icms!;

                switch (vendaItem.CstIcms)
                {
                    case "00":
                        return new ICMS00
                        {
                            CST = (Csticms)Enum.Parse(typeof(Csticms), $"Cst{icms.Cst}"),
                            orig = (OrigemMercadoria)Enum.Parse(typeof(OrigemMercadoria), icms.Origem!),
                            modBC = DeterminacaoBaseIcms.DbiValorOperacao,
                            //pFCP = ,
                            pICMS = icms.Aliquota,
                            vBC = icms.ValorBaseDeCalculo,
                            //vFCP,
                            vICMS = icms.ValorImpostoDevido
                        };
                    case "10":
                        return new ICMS10
                        {
                            CST = (Csticms)Enum.Parse(typeof(Csticms), $"Cst{icms.Cst}"),
                            orig = (OrigemMercadoria)Enum.Parse(typeof(OrigemMercadoria), icms.Origem!),
                            modBC = DeterminacaoBaseIcms.DbiValorOperacao,
                            modBCST = DeterminacaoBaseIcmsSt.DbisMargemValorAgregado,
                            pICMS = icms.Aliquota,
                            vBC = icms.ValorBaseDeCalculo,
                            vICMS = icms.ValorImpostoDevido,
                            pFCP = icms.AliquotaFcp == decimal.Zero ? null : icms.AliquotaFcp,
                            pICMSST = icms.AliquotaSt,
                            vBCST = icms.ValorBaseDeCalculoSt,
                            vFCPST = icms.ValorImpostoDevidoRecolherFcpSt == decimal.Zero ? null : icms.ValorImpostoDevidoRecolherFcpSt,
                            vICMSST = icms.ValorImpostoDevidoRecolherSt,
                            pMVAST = icms.AliquotaMva
                        };
                    case "15":
                        return new ICMS15
                        {
                            //CST = (Csticms)Enum.Parse(typeof(Csticms), $"Cst{icms.Cst}"),
                            //orig = (OrigemMercadoria)Enum.Parse(typeof(OrigemMercadoria), icms.Origem!),
                            //modBC = DeterminacaoBaseIcms.DbiValorOperacao,
                            //pFCP = ,
                            //pICMS = icms.Aliquota,
                            //vBC = icms.ValorBaseDeCalculo,
                            //vFCP,
                            //vICMS = icms.ValorImpostoDevido
                        };
                    case "20":
                        return new ICMS20
                        {
                            CST = (Csticms)Enum.Parse(typeof(Csticms), $"Cst{icms.Cst}"),
                            orig = (OrigemMercadoria)Enum.Parse(typeof(OrigemMercadoria), icms.Origem!),
                            modBC = DeterminacaoBaseIcms.DbiValorOperacao,
                            //pFCP = ,
                            pICMS = icms.Aliquota,
                            vBC = icms.ValorBaseDeCalculo,
                            //vFCP,
                            vICMS = icms.ValorImpostoDevido,
                            pRedBC = icms.AliquotaReducaoPercentual
                        };
                    case "30":
                        return new ICMS30
                        {
                            CST = (Csticms)Enum.Parse(typeof(Csticms), $"Cst{icms.Cst}"),
                            orig = (OrigemMercadoria)Enum.Parse(typeof(OrigemMercadoria), icms.Origem!),
                            modBCST = DeterminacaoBaseIcmsSt.DbisMargemValorAgregado,
                            pFCPST = decimal.Zero,
                            pICMSST = icms.AliquotaSt,
                            vBCST = icms.ValorBaseDeCalculoSt,
                            vFCPST = decimal.Zero,
                            vICMSST = icms.ValorImpostoDevidoRecolherSt,
                            pMVAST = icms.AliquotaMva
                        };
                    case "40":
                    case "41":
                    case "50":
                        return new ICMS40
                        {
                            CST = (Csticms)Enum.Parse(typeof(Csticms), $"Cst{icms.Cst}"),
                            orig = (OrigemMercadoria)Enum.Parse(typeof(OrigemMercadoria), icms.Origem!),
                        };
                    case "51":
                        return new ICMS51
                        {
                            CST = (Csticms)Enum.Parse(typeof(Csticms), $"Cst{icms.Cst}"),
                            orig = (OrigemMercadoria)Enum.Parse(typeof(OrigemMercadoria), icms.Origem!),
                        };
                    case "53":
                        return new ICMS53
                        {
                            CST = (Csticms)Enum.Parse(typeof(Csticms), $"Cst{icms.Cst}"),
                            orig = (OrigemMercadoria)Enum.Parse(typeof(OrigemMercadoria), icms.Origem!),
                        };
                    case "60":
                        return new ICMS60
                        {
                            CST = (Csticms)Enum.Parse(typeof(Csticms), $"Cst{icms.Cst}"),
                            orig = (OrigemMercadoria)Enum.Parse(typeof(OrigemMercadoria), icms.Origem!),
                            vBCSTRet = decimal.Zero,
                            pST = decimal.Zero,
                            vICMSSubstituto = decimal.Zero,
                            vICMSSTRet = decimal.Zero
                        };
                    case "61":
                        return new ICMS61
                        {
                            CST = (Csticms)Enum.Parse(typeof(Csticms), $"Cst{icms.Cst}"),
                            orig = (OrigemMercadoria)Enum.Parse(typeof(OrigemMercadoria), icms.Origem!),
                            qBCMonoRet = icms.QtdeBCMonoRetido,
                            adRemICMSRet = icms.ValorAliquotaAdRemRetido,
                            vICMSMonoRet = icms.ValorImpostoMonoRetido
                        };
                    case "70":
                        return new ICMS70
                        {
                            CST = (Csticms)Enum.Parse(typeof(Csticms), $"Cst{icms.Cst}"),
                            orig = (OrigemMercadoria)Enum.Parse(typeof(OrigemMercadoria), icms.Origem!),
                            modBC = DeterminacaoBaseIcms.DbiValorOperacao,
                            modBCST = DeterminacaoBaseIcmsSt.DbisMargemValorAgregado,
                            pICMS = icms.Aliquota,
                            vBC = icms.ValorBaseDeCalculo,
                            vICMS = icms.ValorImpostoDevido,
                            pFCPST = icms.AliquotaFcpSt == decimal.Zero ? null : icms.AliquotaFcpSt,
                            pICMSST = icms.AliquotaSt,
                            vBCST = icms.ValorBaseDeCalculoSt,
                            vFCPST = icms.ValorImpostoDevidoRecolherFcpSt == decimal.Zero ? null : icms.ValorImpostoDevidoRecolherFcpSt,
                            vICMSST = icms.ValorImpostoDevidoRecolherSt,
                            pRedBC = icms.AliquotaReducaoPercentual,
                            pRedBCST = icms.AliquotaReducaoPercentualSt == decimal.Zero ? null : icms.AliquotaReducaoPercentualSt,
                            pMVAST = icms.AliquotaMva
                        };
                    case "90":
                        return new ICMS90
                        {
                            CST = (Csticms)Enum.Parse(typeof(Csticms), $"Cst{icms.Cst}"),
                            orig = (OrigemMercadoria)Enum.Parse(typeof(OrigemMercadoria), icms.Origem!),
                            modBC = DeterminacaoBaseIcms.DbiValorOperacao,
                            modBCST = DeterminacaoBaseIcmsSt.DbisMargemValorAgregado,
                            pICMS = icms.Aliquota,
                            vBC = icms.Aliquota == decimal.Zero ? decimal.Zero : icms.ValorBaseDeCalculo,
                            vICMS = icms.ValorImpostoDevido,
                            pFCP = icms.AliquotaFcp == decimal.Zero ? null : icms.AliquotaFcp,
                            //pFCPST = decimal.Zero,
                            pICMSST = icms.AliquotaSt,
                            vBCST = icms.AliquotaSt == decimal.Zero ? decimal.Zero : icms.ValorBaseDeCalculoSt,
                            //vFCP = decimal.Zero,
                            vFCPST = icms.ValorImpostoDevidoRecolherFcpSt == decimal.Zero ? null : icms.ValorImpostoDevidoRecolherFcpSt,
                            vICMSST = icms.ValorImpostoDevidoRecolherSt,
                            pMVAST = icms.AliquotaMva
                        };
                    default:
                        new Exception("CST do ICMS inválido");
                        break;
                }
            }

            return null!;
        }

        public static ICMSUFDest ObterICMSUfDestinatario(Venda venda, VendaItem vendaItem)
        {
            //if (vendaItem.RegimeTributario == CRT.SimplesNacional || vendaItem.RegimeTributario == CRT.SimplesNacionalMei)
            //{
            //    var icms = vendaItem.Imposto!.Icms!; //CalculaCsosn.ObterIcmsCsosn(venda, vendaItem);

            //    switch (vendaItem.Csosn)
            //    {
            //        case "61":
            //            return new ICMSUFDest
            //            {
            //                vBCUFDest = decimal.Zero,
            //                vBCFCPUFDest = decimal.Zero,
            //                pFCPUFDest = decimal.Zero,
            //                pICMSUFDest = decimal.Zero,
            //                //pICMSInter = decimal.Zero,
            //                pICMSInterPart = decimal.Zero,
            //                vFCPUFDest = decimal.Zero,
            //                vICMSUFDest = decimal.Zero,
            //                vICMSUFRemet = decimal.Zero
            //            };
            //        default:
            //            new Exception("CSOSN ICMS inválido");
            //            break;
            //    }
            //}
            if ((vendaItem.ModeloDocumento == DFe.Classes.Flags.ModeloDocumento.NFe && vendaItem.RegimeTributario == CRT.SimplesNacionalExcessoSublimite ||
                (vendaItem.ModeloDocumento == DFe.Classes.Flags.ModeloDocumento.NFe && vendaItem.RegimeTributario == CRT.RegimeNormal)))
            {
                var icms = vendaItem.Imposto!.Icms!;

                if (icms == null) return null!;

                if ((venda.Emitente.Endereco.Uf == venda.Destinatario.Endereco.Uf) &&
                    (venda.Destinatario.IdIeDest == indIEDest.ContribuinteICMS ||
                    venda.Destinatario.IdIeDest == indIEDest.Isento ||
                    venda.Destinatario.IdIeDest == indIEDest.NaoContribuinte))
                    return null!;

                if ((venda.Emitente.Endereco.Uf != venda.Destinatario.Endereco.Uf) &&
                   venda.Destinatario.IdIeDest == indIEDest.ContribuinteICMS)
                    return null!;

                switch (vendaItem.CstIcms)
                {
                    case "00":
                        return new ICMSUFDest
                        {
                            vBCUFDest = icms.ValorBaseDeCalculo,
                            vBCFCPUFDest = icms.ValorBaseDeCalculo,
                            pFCPUFDest = icms.AliquotaFcp,
                            pICMSUFDest = icms.AliquotaDifalInterna,
                            pICMSInter = icms.AliquotaDifalInterestadual,
                            pICMSInterPart = 100,
                            vFCPUFDest = icms.ValorImpostoDevidoFcp,
                            vICMSUFDest = icms.ValorImpostoDevidoDifal,
                            //vICMSUFRemet = icms.ValorImpostoDevido
                        };

                    default:
                        new Exception("CST do ICMS inválido");
                        break;
                }
            }

            return null!;
        }

        public static PISBasico ObterPIS(VendaItem vendaItem)
        {
            var pis = vendaItem.Imposto!.Pis!;

            switch (pis.Cst)
            {
                case "01":
                    return new PISAliq
                    {
                        CST = (CSTPIS)Enum.Parse(typeof(CSTPIS), pis.Cst),
                        pPIS = pis.AliquotaPercetual,
                        vBC = pis.ValorBaseDeCalculo,
                        vPIS = pis.ValorImpostoDevido
                    };

                case "02":
                    return new PISAliq
                    {
                        CST = (CSTPIS)Enum.Parse(typeof(CSTPIS), pis.Cst),
                        pPIS = pis.AliquotaPercetual,
                        vBC = pis.ValorBaseDeCalculo,
                        vPIS = pis.ValorImpostoDevido
                    };
                case "03":
                    return new PISQtde
                    {
                        CST = (CSTPIS)Enum.Parse(typeof(CSTPIS), pis.Cst),
                        qBCProd = pis.QuantidadeVendida,
                        vAliqProd = pis.AliquotaReal,
                        vPIS = pis.ValorImpostoDevido
                    };
                case "04":
                case "05":
                case "06":
                case "07":
                case "08":
                case "09":
                    return new PISNT
                    {
                        CST = (CSTPIS)Enum.Parse(typeof(CSTPIS), pis.Cst)
                    };
                case "49":
                case "50":
                case "51":
                case "52":
                case "53":
                case "54":
                case "55":
                case "60":
                case "61":
                case "62":
                case "63":
                case "64":
                case "65":
                case "66":
                case "67":
                case "70":
                case "71":
                case "72":
                case "73":
                case "74":
                case "75":
                case "98":
                case "99":
                    return new PISOutr
                    {
                        CST = (CSTPIS)Enum.Parse(typeof(CSTPIS), pis.Cst),
                        vBC = pis.ValorBaseDeCalculo,
                        pPIS = pis.AliquotaPercetual,
                        vPIS = pis.ValorImpostoDevido
                    };
                default:
                    new Exception("CST PIS inválido");
                    break;
            }

            return null!;
        }

        public static COFINSBasico ObterCOFINS(VendaItem vendaItem)
        {
            var cofins = vendaItem.Imposto!.Cofins!;

            switch (cofins.Cst)
            {
                case "01":
                    return new COFINSAliq
                    {
                        CST = (CSTCOFINS)Enum.Parse(typeof(CSTCOFINS), cofins.Cst),
                        pCOFINS = cofins.AliquotaPercetual,
                        vBC = cofins.ValorBaseDeCalculo,
                        vCOFINS = cofins.ValorImpostoDevido
                    };

                case "02":
                    return new COFINSAliq
                    {
                        CST = (CSTCOFINS)Enum.Parse(typeof(CSTCOFINS), cofins.Cst),
                        pCOFINS = cofins.AliquotaPercetual,
                        vBC = cofins.ValorBaseDeCalculo,
                        vCOFINS = cofins.ValorImpostoDevido
                    };
                case "03":
                    return new COFINSQtde
                    {
                        CST = (CSTCOFINS)Enum.Parse(typeof(CSTCOFINS), cofins.Cst),
                        qBCProd = cofins.QuantidadeVendida,
                        vAliqProd = cofins.AliquotaReal,
                        vCOFINS = cofins.ValorImpostoDevido
                    };
                case "04":
                case "05":
                case "06":
                case "07":
                case "08":
                case "09":
                    return new COFINSNT
                    {
                        CST = (CSTCOFINS)Enum.Parse(typeof(CSTCOFINS), cofins.Cst)
                    };
                case "49":
                case "50":
                case "51":
                case "52":
                case "53":
                case "54":
                case "55":
                case "60":
                case "61":
                case "62":
                case "63":
                case "64":
                case "65":
                case "66":
                case "67":
                case "70":
                case "71":
                case "72":
                case "73":
                case "74":
                case "75":
                case "98":
                case "99":
                    return new COFINSOutr
                    {
                        CST = (CSTCOFINS)Enum.Parse(typeof(CSTCOFINS), cofins.Cst),
                        vBC = cofins.ValorBaseDeCalculo,
                        pCOFINS = cofins.AliquotaPercetual,
                        vCOFINS = cofins.ValorImpostoDevido
                    };
                default:
                    new Exception("CST Cofins inválido");
                    break;
            }

            return null!;
        }

        public static IPI ObterIPI(VendaItem vendaItem)
        {
            if ((!string.IsNullOrEmpty(vendaItem.CstIpi)) && vendaItem.RegimeTributario != CRT.SimplesNacional && vendaItem.RegimeTributario != CRT.SimplesNacionalMei)
            {
                var ipi = vendaItem.Imposto!.Ipi!;

                IPITrib? iPITrib = null;
                IPINT? iPINT = null;

                switch (vendaItem.CstIpi)
                {
                    case "00":
                    case "49":
                    case "50":
                        iPITrib = new IPITrib
                        {
                            CST = (CSTIPI)Enum.Parse(typeof(CSTIPI), $"ipi{ipi.Cst}"),
                            pIPI = ipi.Aliquota,
                            //qUnid = vendaItem.Quantidade,
                            vBC = ipi.ValorBaseDeCalculo,
                            vIPI = ipi.ValorImpostoDevido,
                            //vUnid = vendaItem.ValorUnitario                            
                        };
                        break;
                    case "01":
                    case "02":
                    case "03":
                    case "04":
                    case "05":
                    case "51":
                    case "52":
                    case "53":
                    case "54":
                    case "55":
                        iPINT = new IPINT
                        {
                            CST = (CSTIPI)Enum.Parse(typeof(CSTIPI), $"ipi{ipi.Cst}")
                        };
                        break;
                    case "99":
                        iPITrib = new IPITrib
                        {
                            CST = (CSTIPI)Enum.Parse(typeof(CSTIPI), $"ipi{ipi.Cst}"),
                            pIPI = ipi.Aliquota,
                            //qUnid = decimal.Zero,
                            vBC = ipi.ValorBaseDeCalculo,
                            vIPI = ipi.ValorImpostoDevido,
                            //vUnid = vendaItem.ValorUnitario
                        };
                        break;
                    default:
                        new Exception("CST IPI inválido");
                        break;
                }

                int.TryParse(ipi.Enquadramento, out int enquadramento);

                return new IPI
                {
                    cEnq = enquadramento,
                    TipoIPI = (iPINT != null ? iPINT : iPITrib)
                };
            }

            return null!;
        }

        public static IBSCBS ObterIBSCBS(VendaItem vendaItem)
        {
            if (vendaItem.Imposto.IbsCbs == null || string.IsNullOrEmpty(vendaItem.Imposto.IbsCbs.Cst)) return null!;

            Enum.TryParse<CSTIBSCBS>($"cst{vendaItem.Imposto.IbsCbs.Cst}", out var cst);
            //Enum.TryParse<cClassTrib>($"ct{vendaItem.Imposto.IbsCbs.CClassTrib}", out var cClassTrib);

            Enum.TryParse<CSTIBSCBS>($"cst{vendaItem.Imposto.IbsCbs.TributacaoRegular?.Cst}", out var cstReg);

            var ibsCbs = vendaItem.Imposto.IbsCbs;

            return new IBSCBS
            {
                CST = cst,
                cClassTrib = vendaItem.Imposto.IbsCbs.CClassTrib,
                gIBSCBS = vendaItem.Imposto.IbsCbs.ValorBaseDeCalculo > decimal.Zero ? new gIBSCBS
                {
                    vBC = vendaItem.Imposto.IbsCbs.ValorBaseDeCalculo,
                    gIBSUF = new gIBSUF
                    {
                        pIBSUF = vendaItem.Imposto.IbsCbs.AliquotaEstadual,
                        vIBSUF = vendaItem.Imposto.IbsCbs.ValorImpostoDevidoEstadual,
                        gRed = vendaItem.Imposto.IbsCbs.AliquotaEstadualReducao > decimal.Zero ? new gRed
                        {
                            pRedAliq = vendaItem.Imposto.IbsCbs.AliquotaEstadualReducao,
                            pAliqEfet = vendaItem.Imposto.IbsCbs.AliquotaEfetivaEstadual
                        } : null,
                        gDif = vendaItem.Imposto.IbsCbs.AliquotaEstadualDiferimento > 0 ? new gDif
                        {
                            pDif = vendaItem.Imposto.IbsCbs.AliquotaEstadualDiferimento,
                            vDif = decimal.Zero
                        } : null

                    },
                    gIBSMun = new gIBSMun
                    {
                        pIBSMun = vendaItem.Imposto.IbsCbs.AliquotaMunicipal,
                        vIBSMun = vendaItem.Imposto.IbsCbs.ValorImpostoDevidoMunicipal,
                        gRed = vendaItem.Imposto.IbsCbs.AliquotaMunicipalReducao > decimal.Zero ? new gRed
                        {
                            pRedAliq = vendaItem.Imposto.IbsCbs.AliquotaMunicipalReducao,
                            pAliqEfet = vendaItem.Imposto.IbsCbs.AliquotaEfetivaMunicipal
                        } : null,
                        gDif = vendaItem.Imposto.IbsCbs.AliquotaMunicipalDiferimento > 0 ? new gDif
                        {
                            pDif = vendaItem.Imposto.IbsCbs.AliquotaMunicipalDiferimento,
                            vDif = decimal.Zero
                        } : null
                    },
                    gCBS = new gCBS
                    {
                        pCBS = vendaItem.Imposto.IbsCbs.AliquotaCbs,
                        vCBS = vendaItem.Imposto.IbsCbs.ValorImpostoDevidoCbs,
                        gRed = vendaItem.Imposto.IbsCbs.AliquotaCbsReducao > decimal.Zero ? new gRed
                        {
                            pRedAliq = vendaItem.Imposto.IbsCbs.AliquotaCbsReducao,
                            pAliqEfet = vendaItem.Imposto.IbsCbs.AliquotaEfetivaCbs
                        } : null,
                        gDif = vendaItem.Imposto.IbsCbs.AliquotaCbsDiferimento > 0 ? new gDif
                        {
                            pDif = vendaItem.Imposto.IbsCbs.AliquotaCbsDiferimento,
                            vDif = decimal.Zero
                        } : null
                    },
                    vIBS = vendaItem.Imposto.IbsCbs.ValorImpostoDevidoEstadual + vendaItem.Imposto.IbsCbs.ValorImpostoDevidoMunicipal,
                    gTribRegular = vendaItem.Imposto.IbsCbs.TributacaoRegular != null ? new gTribRegular
                    {
                        CSTReg = cstReg,
                        cClassTribReg = vendaItem.Imposto.IbsCbs.TributacaoRegular.CClassTrib,
                        pAliqEfetRegIBSUF = vendaItem.Imposto.IbsCbs.TributacaoRegular.AliquotaEfetivaIbsEstadual,
                        vTribRegIBSUF = vendaItem.Imposto.IbsCbs.TributacaoRegular.ValorIbsEstadual,
                        pAliqEfetRegIBSMun = vendaItem.Imposto.IbsCbs.TributacaoRegular.AliquotaEfetivaIbsMunicipal,
                        vTribRegIBSMun = vendaItem.Imposto.IbsCbs.TributacaoRegular.ValorIbsMunicipal,
                        pAliqEfetRegCBS = vendaItem.Imposto.IbsCbs.TributacaoRegular.AliquotaEfetivaCbs,
                        vTribRegCBS = vendaItem.Imposto.IbsCbs.TributacaoRegular.ValorCbs
                    } : null
                } : null
            };
        }
    }
}


