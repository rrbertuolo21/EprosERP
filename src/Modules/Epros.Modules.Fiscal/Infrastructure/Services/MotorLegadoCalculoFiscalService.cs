using Epros.Modules.Fiscal.Application.Services;

// ISOLAMENTO DO NAMESPACE LEGADO: todos os tipos do motor Epros.ERP.DfeCalculos
// entram por alias/nomes qualificados, evitando colisão de enums entre
// Epros.ERP.Shared (legado) e Epros.Shared (novo).
using MotorCalculo = Epros.ERP.DfeCalculos.Impostos.Calculo;
using MotorVenda = Epros.ERP.DfeCalculos.Models.Vendas.Venda;
using MotorVendaItem = Epros.ERP.DfeCalculos.Models.Vendas.VendaItem;
using MotorTributacaoCfop = Epros.ERP.DfeCalculos.Models.Vendas.VendaItemTributacaoCfop;
using MotorCrt = NFe.Classes.Informacoes.Emitente.CRT;
using MotorModeloDocumento = DFe.Classes.Flags.ModeloDocumento;
using MotorIndicadorTotal = NFe.Classes.Informacoes.Detalhe.IndicadorTotal;
using MotorDeterminacaoBaseIcmsSt = NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.Tipos.DeterminacaoBaseIcmsSt;
using MotorTipoReducao = Epros.ERP.Shared.Enums.ETipoReducaoBaseDeCalculo;

namespace Epros.Modules.Fiscal.Infrastructure.Services
{
    /// <summary>
    /// Adapter que faz o motor legado (Epros.ERP.DfeCalculos) calcular impostos
    /// a partir de um <see cref="CalculoFiscalRequest"/> neutro do módulo Fiscal.
    /// Mapeia request -> DTOs do motor (Venda/VendaItem), chama os métodos estáticos de
    /// Calculo (ICMS/IPI/PIS/COFINS/IBS-CBS/IBPT) e mapeia o retorno de volta ao Fiscal.
    /// ESCOPO: só cálculo — nada de transmissão SEFAZ.
    /// </summary>
    public class MotorLegadoCalculoFiscalService : ICalculoFiscalService
    {
        public CalculoFiscalResultado Calcular(CalculoFiscalRequest request)
        {
            var resultado = new CalculoFiscalResultado();

            if (request is null)
            {
                resultado.Sucesso = false;
                resultado.Erros.Add("Requisição de cálculo fiscal não informada.");
                return resultado;
            }

            MotorVendaItem item;
            try
            {
                item = MontarItemMotor(request);
            }
            catch (Exception ex)
            {
                resultado.Sucesso = false;
                resultado.Erros.Add($"Falha ao montar o item para o motor de cálculo: {ex.Message}");
                return resultado;
            }

            // O item legado valida no construtor (NCM, CFOP para NFC-e, etc.).
            if (item.Notifications.Count > 0)
            {
                resultado.Sucesso = false;
                foreach (var n in item.Notifications)
                    resultado.Erros.Add(n.Message);
                return resultado;
            }

            // Venda mínima só para satisfazer a assinatura do motor de ICMS.
            var venda = new MotorVenda();

            // --- ICMS (+ ST + FCP + DIFAL) ---
            var icms = MotorCalculo.CalcularICMS(venda, item);
            if (icms != null)
            {
                item.Imposto.Icms = icms;
                resultado.Icms = new CalculoImpostoIcms
                {
                    Cst = icms.Cst,
                    Csosn = icms.Csosn,
                    Origem = icms.Origem,
                    BaseCalculo = icms.ValorBaseDeCalculo,
                    Aliquota = icms.Aliquota,
                    Valor = icms.ValorImpostoDevido,
                    BaseCalculoSt = icms.ValorBaseDeCalculoSt,
                    AliquotaSt = icms.AliquotaSt,
                    ValorSt = icms.ValorImpostoDevidoRecolherSt,
                    BaseCalculoFcp = icms.ValorBaseDeCalculoFcp,
                    AliquotaFcp = icms.AliquotaFcp,
                    ValorFcp = icms.ValorImpostoDevidoFcp,
                    ValorFcpSt = icms.ValorImpostoDevidoRecolherFcpSt,
                    BaseCalculoDifal = icms.ValorBaseDeCalculoDifal,
                    ValorDifal = icms.ValorImpostoDevidoDifal,
                    ValorCredito = icms.ValorCredito
                };
            }

            // --- IPI (só regime normal, e apenas para NF-e no fluxo legado) ---
            if (request.ModeloDocumento == 55)
            {
                var ipi = MotorCalculo.CalcularIPI(item);
                if (ipi != null)
                {
                    item.Imposto.Ipi = ipi;
                    resultado.Ipi = new CalculoImpostoIpi
                    {
                        Cst = ipi.Cst,
                        BaseCalculo = ipi.ValorBaseDeCalculo,
                        Aliquota = ipi.Aliquota,
                        Valor = ipi.ValorImpostoDevido
                    };
                }
            }

            // --- PIS ---
            var pis = MotorCalculo.CalcularPIS(item);
            if (pis != null)
            {
                item.Imposto.Pis = pis;
                resultado.Pis = new CalculoImpostoPisCofins
                {
                    Cst = pis.Cst,
                    BaseCalculo = pis.ValorBaseDeCalculo,
                    Aliquota = pis.AliquotaPercetual,
                    Valor = pis.ValorImpostoDevido
                };
            }

            // --- COFINS ---
            var cofins = MotorCalculo.CalcularCOFINS(item);
            if (cofins != null)
            {
                item.Imposto.Cofins = cofins;
                resultado.Cofins = new CalculoImpostoPisCofins
                {
                    Cst = cofins.Cst,
                    BaseCalculo = cofins.ValorBaseDeCalculo,
                    Aliquota = cofins.AliquotaPercetual,
                    Valor = cofins.ValorImpostoDevido
                };
            }

            // --- IBS / CBS (Reforma Tributária) ---
            var ibsCbs = MotorCalculo.CalcularIBSCBS(item);
            if (ibsCbs != null)
            {
                item.Imposto.IbsCbs = ibsCbs;
                resultado.IbsCbs = new CalculoImpostoIbsCbs
                {
                    Cst = ibsCbs.Cst,
                    CClassTrib = ibsCbs.CClassTrib,
                    BaseCalculo = ibsCbs.ValorBaseDeCalculo,
                    AliquotaEstadual = ibsCbs.AliquotaEstadual,
                    ValorEstadual = ibsCbs.ValorImpostoDevidoEstadual,
                    AliquotaMunicipal = ibsCbs.AliquotaMunicipal,
                    ValorMunicipal = ibsCbs.ValorImpostoDevidoMunicipal,
                    AliquotaCbs = ibsCbs.AliquotaCbs,
                    ValorCbs = ibsCbs.ValorImpostoDevidoCbs
                };
            }

            // --- IBPT (carga tributária aproximada) ---
            if (request.AliquotaIbptFederalNacional > 0 || request.AliquotaIbptFederalImportado > 0 ||
                request.AliquotaIbptEstadual > 0 || request.AliquotaIbptMunicipal > 0)
            {
                item.DefinirValorAproximadoIbpt(
                    request.AliquotaIbptFederalNacional,
                    request.AliquotaIbptFederalImportado,
                    request.AliquotaIbptEstadual,
                    request.AliquotaIbptMunicipal,
                    request.VersaoIbpt ?? string.Empty);

                if (item.Ibpt != null)
                {
                    resultado.Ibpt = new CalculoImpostoIbpt
                    {
                        AliquotaFederal = item.Ibpt.AliqFederal,
                        AliquotaImportado = item.Ibpt.AliqImportado,
                        AliquotaEstadual = item.Ibpt.AliqEstadual,
                        AliquotaMunicipal = item.Ibpt.AliqMunicipal,
                        ValorAproximado = item.ValorAproximadoIbpt
                    };
                }
            }

            return resultado;
        }

        private static MotorVendaItem MontarItemMotor(CalculoFiscalRequest r)
        {
            var regime = (MotorCrt)r.RegimeTributario;
            var modelo = (MotorModeloDocumento)r.ModeloDocumento;
            var tributacaoCfop = new MotorTributacaoCfop(r.Cfop, 0, true, false);

            // O construtor do VendaItem legado carrega ~60 parâmetros; abaixo mapeamos os
            // usados no cálculo e zeramos/neutralizamos o restante (produtos específicos,
            // combustível, pedido de compra, etc.) que não afetam esta leva.
            return new MotorVendaItem(
                regimeTributario: regime,
                modeloDocumento: modelo,
                emitenteCnae: null,
                produtoId: 0,
                codigoProduto: r.CodigoProduto,
                nomeProduto: r.NomeProduto,
                codigoBarras: null,
                ncm: r.Ncm,
                tributacaoCfop: tributacaoCfop,
                unidade: r.Unidade,
                valorUnitario: r.ValorUnitario,
                quantidade: r.Quantidade,
                origem: r.Origem,
                csosn: r.Csosn,
                cstIcms: r.CstIcms,
                valorAliquotaIcms: r.AliquotaIcms,
                valorReducaoIcmsPercentual: r.ReducaoIcmsPercentual,
                reducaoIcms: MotorTipoReducao.NaoUtiliza,
                valorBaseCalculoStRetidoOperacaoAnterior: decimal.Zero,
                valorAlioquotaSt: r.AliquotaSt,
                valorIcmsStRetidoOperacaoAnterior: decimal.Zero,
                valorIcmsProprioSubstituto: decimal.Zero,
                cstPisCofins: r.CstPisCofins ?? string.Empty,
                valorAliquotaPis: r.AliquotaPis,
                valorAliquotaPisReal: r.AliquotaPisReal,
                valorAliquotaCofins: r.AliquotaCofins,
                valorAliquotaCofinsReal: r.AliquotaCofinsReal,
                compoeValorTotal: default(MotorIndicadorTotal),
                valorDesconto: r.ValorDesconto,
                enquadramentoIpi: r.EnquadramentoIpi,
                cstIpi: r.CstIpi,
                valorAliquotaIpi: r.AliquotaIpi,
                valorReducaoIpiPercentual: decimal.Zero,
                tipoReducaoIpi: MotorTipoReducao.NaoUtiliza,
                cest: null,
                valorAliquotaMva: r.AliquotaMva,
                valorAliquotaFcp: r.AliquotaFcp,
                valorAliquotaFcpSt: r.AliquotaFcpSt,
                valorAliquotaIcmsInterna: r.AliquotaIcmsInterna,
                valorAliquotaIcmsInterestadual: r.AliquotaIcmsInterestadual,
                valorReducaoIcmsPercentualSt: r.ReducaoIcmsPercentualSt,
                tipoReducaoIcmsSt: MotorTipoReducao.NaoUtiliza,
                ipiEmbutido: r.IpiEmbutido,
                difalTipoCalculoPorDentro: r.DifalTipoCalculoPorDentro,
                embuteFrete: true,
                embuteSeguro: true,
                embuteAcrescimo: true,
                embuteOutro: true,
                tipoCalculoBaseIcmsSt: MotorDeterminacaoBaseIcmsSt.DbisMargemValorAgregado,
                valorUnitFixadoIcmsSt: decimal.Zero,
                cstIbsCbs: r.CstIbsCbs,
                cClassTrib: r.CClassTrib,
                qtdeBCMonoRetido: decimal.Zero,
                valorAliquotaAdRemRetido: decimal.Zero,
                informacoesAdProduto: null,
                numeroItemPedidoCompra: null,
                numeroPedidoCompra: string.Empty,
                fichaConteudoImportacao: string.Empty,
                unidadeTributavel: r.Unidade,
                codigoBeneficioFiscal: null,
                valorCusto: decimal.Zero,
                externoId: null!);
        }
    }
}
