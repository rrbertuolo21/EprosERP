using DFe.Classes.Flags;
using Epros.ERP.DfeCalculos.Models.Vendas.VendaItemProdutosEspecificos;
using Epros.ERP.DfeCalculos.Validations;
using Epros.ERP.DfeCalculos.Validations.Nfces.CfopCsosns;
using Epros.ERP.Shared.Enums;
using Epros.ERP.Shared.Extensions;
using Flunt.Notifications;
using Flunt.Validations;
using NFe.Classes.Informacoes.Detalhe;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.Tipos;
using NFe.Classes.Informacoes.Emitente;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaItem : Notifiable<Notification>
    {
        protected VendaItem() { }

        public VendaItem(CRT regimeTributario, ModeloDocumento modeloDocumento, string? emitenteCnae, long produtoId, string? codigoProduto, string? nomeProduto, string? codigoBarras, string? ncm, VendaItemTributacaoCfop tributacaoCfop, string? unidade, decimal valorUnitario, decimal quantidade, string? origem, string? csosn, string? cstIcms, decimal valorAliquotaIcms, decimal valorReducaoIcmsPercentual, ETipoReducaoBaseDeCalculo reducaoIcms, decimal valorBaseCalculoStRetidoOperacaoAnterior, decimal valorAlioquotaSt, decimal valorIcmsStRetidoOperacaoAnterior, decimal valorIcmsProprioSubstituto, string cstPisCofins, decimal valorAliquotaPis, decimal valorAliquotaPisReal, decimal valorAliquotaCofins, decimal valorAliquotaCofinsReal, IndicadorTotal compoeValorTotal, decimal valorDesconto, string? enquadramentoIpi, string? cstIpi, decimal valorAliquotaIpi, decimal valorReducaoIpiPercentual, ETipoReducaoBaseDeCalculo tipoReducaoIpi, string? cest, decimal valorAliquotaMva, decimal valorAliquotaFcp, decimal valorAliquotaFcpSt, decimal valorAliquotaIcmsInterna, decimal valorAliquotaIcmsInterestadual, decimal valorReducaoIcmsPercentualSt, ETipoReducaoBaseDeCalculo tipoReducaoIcmsSt, bool ipiEmbutido, bool difalTipoCalculoPorDentro, bool embuteFrete, bool embuteSeguro, bool embuteAcrescimo, bool embuteOutro, DeterminacaoBaseIcmsSt tipoCalculoBaseIcmsSt, decimal valorUnitFixadoIcmsSt, string? cstIbsCbs, string? cClassTrib, decimal qtdeBCMonoRetido, decimal valorAliquotaAdRemRetido, string? informacoesAdProduto, int? numeroItemPedidoCompra, string numeroPedidoCompra, string fichaConteudoImportacao, string? unidadeTributavel, string? codigoBeneficioFiscal, decimal valorCusto, string? externoId = null)
        {

            RegimeTributario = regimeTributario;
            ModeloDocumento = modeloDocumento;
            EmitenteCnae = emitenteCnae;
            ProdutoId = produtoId;
            CodigoProduto = string.IsNullOrEmpty(codigoProduto) ? null : codigoProduto;
            NomeProduto = string.IsNullOrEmpty(nomeProduto) ? null : nomeProduto;
            CodigoBarras = string.IsNullOrEmpty(codigoBarras) ? null : codigoBarras;
            Ncm = string.IsNullOrEmpty(ncm) ? null : ncm;
            TributacaoCfop = tributacaoCfop;
            Unidade = string.IsNullOrEmpty(unidade) ? null : unidade;
            ValorUnitario = valorUnitario;
            Quantidade = quantidade;
            Origem = string.IsNullOrEmpty(origem) ? null : origem;
            Csosn = string.IsNullOrEmpty(csosn) ? null : csosn;
            CstIcms = string.IsNullOrEmpty(cstIcms) ? null : cstIcms.PadLeft(2, '0');
            ValorAliquotaIcms = valorAliquotaIcms;
            ValorReducaoIcmsPercentual = valorReducaoIcmsPercentual;
            TipoReducaoIcms = reducaoIcms;
            ValorBaseCalculoStRetidoOperacaoAnterior = valorBaseCalculoStRetidoOperacaoAnterior;
            ValorAliquotaSt = valorAlioquotaSt;
            ValorIcmsStRetidoOperacaoAnterior = valorIcmsStRetidoOperacaoAnterior;
            ValorIcmsProprioSubstituto = valorIcmsProprioSubstituto;
            CstPisCofins = string.IsNullOrEmpty(cstPisCofins) ? null! : cstPisCofins;
            ValorAliquotaPis = valorAliquotaPis;
            ValorAliquotaPisReal = valorAliquotaPisReal;
            ValorAliquotaCofins = valorAliquotaCofins;
            ValorAliquotaCofinsReal = valorAliquotaCofinsReal;
            CompoeValorTotal = compoeValorTotal;
            ValorDesconto = valorDesconto;
            EnquadramentoIpi = enquadramentoIpi;
            CstIpi = cstIpi;
            ValorAliquotaIpi = valorAliquotaIpi;
            ValorReducaoIpiPercentual = valorReducaoIpiPercentual;
            TipoReducaoIpi = tipoReducaoIpi;
            Cest = (string.IsNullOrEmpty(cest) ? null : cest);
            ValorAliquotaMva = valorAliquotaMva;
            ValorAliquotaFcp = valorAliquotaFcp;
            ValorAliquotaFcpSt = valorAliquotaFcpSt;
            ValorAliquotaIcmsInterna = valorAliquotaIcmsInterna;
            ValorAliquotaIcmsInterestadual = valorAliquotaIcmsInterestadual;
            ValorReducaoIcmsPercentualSt = valorReducaoIcmsPercentualSt;
            TipoReducaoIcmsSt = tipoReducaoIcmsSt;
            IpiEmbutido = ipiEmbutido;
            DifalTipoCalculoPorDentro = difalTipoCalculoPorDentro;
            EmbuteFrete = embuteFrete;
            EmbuteSeguro = embuteSeguro;
            EmbuteAcrescimo = embuteAcrescimo;
            EmbuteOutro = embuteOutro;
            TipoCalculoBaseIcmsSt = tipoCalculoBaseIcmsSt;
            ValorUnitFixadoIcmsSt = valorUnitFixadoIcmsSt;
            CstIbsCbs = string.IsNullOrEmpty(cstIbsCbs) ? "000" : cstIbsCbs;
            CClassTrib = string.IsNullOrEmpty(cstIbsCbs) ? "000001" : cClassTrib;
            QtdeBCMonoRetido = qtdeBCMonoRetido;
            ValorAliquotaAdRemRetido = valorAliquotaAdRemRetido;
            InformacoesAdProduto = informacoesAdProduto;
            NumeroItemPedidoCompra = numeroItemPedidoCompra;
            NumeroPedidoCompra = string.IsNullOrEmpty(numeroPedidoCompra) ? null! : numeroPedidoCompra;
            FichaConteudoImportacao = string.IsNullOrEmpty(fichaConteudoImportacao) ? null! : fichaConteudoImportacao;
            UnidadeTributavel = string.IsNullOrEmpty(unidadeTributavel) ? Unidade : unidadeTributavel;
            AplicarRegraCodigoBeneficioFiscal(codigoBeneficioFiscal, CstIcms);
            ValorCompra = valorCusto;
            ExternoId = externoId;
            Validar();
            Imposto = new VendaItemImposto();
        }

        public CRT RegimeTributario { get; private set; }
        public ModeloDocumento ModeloDocumento { get; private set; }
        public string? EmitenteCnae { get; private set; }
        public long ProdutoId { get; private set; }
        public string? CodigoProduto { get; private set; }
        public string? NomeProduto { get; private set; }
        public string? CodigoBarras { get; private set; }
        public string? Ncm { get; private set; }
        public VendaItemTributacaoCfop TributacaoCfop { get; private set; } = null!;
        public string? Unidade { get; private set; }
        public decimal ValorUnitario { get; private set; }
        public decimal Quantidade { get; private set; }
        public string? Origem { get; private set; }
        public string? Csosn { get; private set; }
        public string? CstIcms { get; private set; }
        public decimal ValorAliquotaIcms { get; private set; }
        public decimal ValorReducaoIcmsPercentual { get; private set; }
        public ETipoReducaoBaseDeCalculo TipoReducaoIcms { get; private set; }
        public decimal ValorReducaoIcmsPercentualSt { get; private set; }
        public ETipoReducaoBaseDeCalculo TipoReducaoIcmsSt { get; private set; }
        public decimal ValorBaseCalculoStRetidoOperacaoAnterior { get; private set; }
        public decimal ValorAliquotaSt { get; private set; }
        public decimal ValorIcmsStRetidoOperacaoAnterior { get; private set; }
        public decimal ValorIcmsProprioSubstituto { get; private set; }
        public string CstPisCofins { get; private set; } = null!;
        public decimal ValorAliquotaPis { get; private set; }
        public decimal ValorAliquotaPisReal { get; private set; }
        public decimal ValorAliquotaCofins { get; private set; }
        public decimal ValorAliquotaCofinsReal { get; private set; }
        public IndicadorTotal CompoeValorTotal { get; private set; }
        public decimal ValorDesconto { get; private set; }
        public decimal ValorDescontoRateado { get; private set; }
        public decimal ValorFreteRateado { get; private set; }
        public decimal ValorFreteBaseCalculoRateado { get; private set; }
        public decimal ValorSeguroRateado { get; private set; }
        public decimal ValorAcrescimoRateado { get; private set; }
        public decimal ValorOutroRateado { get; private set; }
        public decimal ValorAproximadoIbpt { get; private set; }
        public decimal ValorAliquotaMva { get; private set; }
        public decimal ValorAliquotaFcp { get; private set; }
        public decimal ValorAliquotaFcpSt { get; private set; }
        public decimal ValorAliquotaIcmsInterna { get; private set; }
        public decimal ValorAliquotaIcmsInterestadual { get; private set; }

        public string? EnquadramentoIpi { get; private set; }
        public string? CstIpi { get; private set; }
        public decimal ValorAliquotaIpi { get; private set; }
        public decimal ValorReducaoIpiPercentual { get; private set; }
        public ETipoReducaoBaseDeCalculo TipoReducaoIpi { get; private set; }
        public string? Cest { get; private set; }
        public bool IpiEmbutido { get; private set; }
        public bool DifalTipoCalculoPorDentro { get; private set; }

        public bool EmbuteFrete { get; private set; } = true;
        public bool EmbuteSeguro { get; private set; } = true;
        public bool EmbuteAcrescimo { get; private set; } = true;
        public bool EmbuteOutro { get; private set; } = true;
        public DeterminacaoBaseIcmsSt TipoCalculoBaseIcmsSt { get; private set; }
        public decimal ValorUnitFixadoIcmsSt { get; private set; }

        public string? CstIbsCbs { get; set; }
        public string? CClassTrib { get; set; }

        public decimal QtdeBCMonoRetido { get; private set; }
        public decimal ValorAliquotaAdRemRetido { get; private set; }
        public string? InformacoesAdProduto { get; private set; }
        public int? NumeroItemPedidoCompra { get; private set; }
        public string NumeroPedidoCompra { get; private set; } = null!;
        public string FichaConteudoImportacao { get; private set; } = null!;
        public string? UnidadeTributavel { get; private set; }
        public string? CodigoBeneficioFiscal { get; set; } = "SEM CBENEF";
        public decimal ValorCompra { get; private set; }
        public string? ExternoId { get; private set; }

        public VendaItemImposto Imposto { get; private set; } = null!;
        public VendaItemIbpt Ibpt { get; private set; } = null!;
        public VendaItemProdutoCombustivel? Combustivel { get; private set; }

        public void Alterar(long produtoId, string? codigoProduto, string? nomeProduto, string? codigoBarras, string? ncm, VendaItemTributacaoCfop tributacaoCfop, string? unidade, decimal valorUnitario, decimal quantidade, string? origem, string? csosn, string? cstIcms, decimal valorAliquotaIcms, decimal valorReducaoIcmsPercentual, ETipoReducaoBaseDeCalculo reducaoIcms, decimal valorBaseCalculoStRetidoOperacaoAnterior, decimal valorAlioquotaSt, decimal valorIcmsStRetidoOperacaoAnterior, decimal valorIcmsProprioSubstituto, string cstPisCofins, decimal valorAliquotaPis, decimal valorAliquotaPisReal, decimal valorAliquotaCofins, decimal valorAliquotaCofinsReal, IndicadorTotal compoeValorTotal, decimal valorDesconto, string? enquadramentoIpi, string? cstIpi, decimal valorAliquotaIpi, decimal valorReducaoIpiPercentual, ETipoReducaoBaseDeCalculo tipoReducaoIpi, string? cest, decimal valorAliquotaMva, decimal valorAliquotaFcp, decimal valorAliquotaFcpSt, decimal valorAliquotaIcmsInterna, decimal valorAliquotaIcmsInterestadual, decimal valorReducaoIcmsPercentualSt, ETipoReducaoBaseDeCalculo tipoReducaoIcmsSt, bool ipiEmbutido, bool difalTipoCalculoPorDentro, bool embuteFrete, bool embuteSeguro, bool embuteAcrescimo, bool embuteOutro, DeterminacaoBaseIcmsSt tipoCalculoBaseIcmsSt, decimal valorUnitFixadoIcmsSt, string? cstIbsCbs, string? cClassTrib, string? unidadeTributavel, string? codigoBeneficioFiscal, decimal valorCusto)
        {
            ProdutoId = produtoId;
            CodigoProduto = string.IsNullOrEmpty(codigoProduto) ? null : codigoProduto;
            NomeProduto = string.IsNullOrEmpty(nomeProduto) ? null : nomeProduto;
            CodigoBarras = string.IsNullOrEmpty(codigoBarras) ? null : codigoBarras;
            Ncm = string.IsNullOrEmpty(ncm) ? null : ncm;
            TributacaoCfop = tributacaoCfop;
            Unidade = string.IsNullOrEmpty(unidade) ? null : unidade;
            UnidadeTributavel = string.IsNullOrEmpty(unidadeTributavel) ? Unidade : unidadeTributavel;
            ValorUnitario = valorUnitario;
            Quantidade = quantidade;
            Origem = string.IsNullOrEmpty(origem) ? null : origem;
            Csosn = string.IsNullOrEmpty(csosn) ? null : csosn;
            CstIcms = string.IsNullOrEmpty(cstIcms) ? null : cstIcms.PadLeft(2, '0');
            ValorAliquotaIcms = valorAliquotaIcms;
            ValorReducaoIcmsPercentual = valorReducaoIcmsPercentual;
            TipoReducaoIcms = reducaoIcms;
            ValorBaseCalculoStRetidoOperacaoAnterior = valorBaseCalculoStRetidoOperacaoAnterior;
            ValorAliquotaSt = valorAlioquotaSt;
            ValorIcmsStRetidoOperacaoAnterior = valorIcmsStRetidoOperacaoAnterior;
            ValorIcmsProprioSubstituto = valorIcmsProprioSubstituto;
            CstPisCofins = string.IsNullOrEmpty(cstPisCofins) ? null! : cstPisCofins;
            ValorAliquotaPis = valorAliquotaPis;
            ValorAliquotaPisReal = valorAliquotaPisReal;
            ValorAliquotaCofins = valorAliquotaCofins;
            ValorAliquotaCofinsReal = valorAliquotaCofinsReal;
            CompoeValorTotal = compoeValorTotal;
            ValorDesconto = valorDesconto;
            EnquadramentoIpi = enquadramentoIpi;
            CstIpi = cstIpi;
            ValorAliquotaIpi = valorAliquotaIpi;
            ValorReducaoIpiPercentual = valorReducaoIpiPercentual;
            TipoReducaoIpi = tipoReducaoIpi;
            Cest = (string.IsNullOrEmpty(cest) ? null : cest);
            ValorAliquotaMva = valorAliquotaMva;
            ValorAliquotaFcp = valorAliquotaFcp;
            ValorAliquotaFcpSt = valorAliquotaFcpSt;
            ValorAliquotaIcmsInterna = valorAliquotaIcmsInterna;
            ValorAliquotaIcmsInterestadual = valorAliquotaIcmsInterestadual;
            ValorReducaoIcmsPercentualSt = valorReducaoIcmsPercentualSt;
            TipoReducaoIcmsSt = tipoReducaoIcmsSt;
            IpiEmbutido = ipiEmbutido;
            DifalTipoCalculoPorDentro = difalTipoCalculoPorDentro;
            EmbuteFrete = embuteFrete;
            EmbuteSeguro = embuteSeguro;
            EmbuteAcrescimo = embuteAcrescimo;
            EmbuteOutro = embuteOutro;
            TipoCalculoBaseIcmsSt = tipoCalculoBaseIcmsSt;
            ValorUnitFixadoIcmsSt = valorUnitFixadoIcmsSt;
            CstIbsCbs = string.IsNullOrEmpty(cstIbsCbs) ? "000" : cstIbsCbs;
            CClassTrib = string.IsNullOrEmpty(cstIbsCbs) ? "000001" : cClassTrib;
            ValorCompra = valorCusto;
            AplicarRegraCodigoBeneficioFiscal(codigoBeneficioFiscal, CstIcms);

            Validar();
        }

        public decimal ObterAdicionaisAEmbutirBaseCalculoIcms()
        {
            return ((EmbuteFrete ? (ValorFreteRateado > decimal.Zero ? ValorFreteRateado : ValorFreteBaseCalculoRateado) : decimal.Zero) +
                    (EmbuteSeguro ? ValorSeguroRateado : decimal.Zero) +
                    (EmbuteAcrescimo ? ValorAcrescimoRateado : decimal.Zero) +
                    (EmbuteOutro ? ValorOutroRateado : decimal.Zero));
        }

        public decimal ObterDescontosAEmbutirBaseCalculoIcms(bool embuteDesconto = true)
        {
            return (embuteDesconto ? (ValorDesconto + ValorDescontoRateado) : decimal.Zero);
        }

        public decimal ValorItem
        {
            get
            {
                var valorCalculado = (Quantidade * ValorUnitario).Arredonda2();
                return valorCalculado;
            }
        }

        public decimal ValorTotal
        {
            get
            {
                var valorCalculado = (ValorItem - ValorDesconto - ValorDescontoRateado);
                return valorCalculado;
            }
        }

        public void Validar()
        {
            AddNotifications(new Contract<Notification>()
            .Requires()
            .IsTrue(Ncm!.Length == 8, "Ncm", "NCM precisa ter 8 caracteres")
            );

            if (!Enum.IsDefined(typeof(ETipoReducaoBaseDeCalculo), TipoReducaoIcms))
                AddNotification("TipoReducaoIcms", $"Tipo redução icms do item inválido");

            if (ModeloDocumento == ModeloDocumento.NFCe)
            {
                if (!CfopNfceValidation.Validar(TributacaoCfop.Cfop.ToString()))
                    AddNotification("Cfop", $"CFOP [{TributacaoCfop.Cfop}] inváldio para NFC-e");

                if (RegimeTributario == CRT.SimplesNacional || RegimeTributario == CRT.SimplesNacionalMei)
                {
                    if (!CsosnNfceValidation.Validar(Csosn!))
                        AddNotification("Csosn", $"CSOSN [{Csosn}] inváldio para NFC-e");

                    var cfopNfceCsosnValidation = CfopNfceCsosnValidation.Validar(TributacaoCfop.Cfop.ToString(), Csosn!);
                    if (!string.IsNullOrEmpty(cfopNfceCsosnValidation))
                        AddNotification("Cfop", cfopNfceCsosnValidation);
                }
                else if (RegimeTributario == CRT.SimplesNacionalExcessoSublimite || RegimeTributario == CRT.RegimeNormal)
                {
                    if (!CstNfceValidation.Validar(CstIcms!))
                        AddNotification("CstIcms", $"CST ICMS [{CstIcms}] inválido para NFC-e");

                    if (CstIcms == "20" && ValorReducaoIcmsPercentual == decimal.Zero)
                        AddNotification("CstIcms", $"CST ICMS [{CstIcms}] é necessário informar o percentual de redução da base de calculo");

                    if (!CstNfceValCstPisCofinsValidationidation.Validar(CstPisCofins!))
                        AddNotification("CstPisCofins", $"CST PIS/COFINS [{CstPisCofins}] inválido para NF-e/NFC-e");

                    var cfopNfceCstValidation = CfopNfceCstValidation.Validar(TributacaoCfop.Cfop.ToString(), CstIcms!);
                    if (!string.IsNullOrEmpty(cfopNfceCstValidation))
                        AddNotification("Cfop", cfopNfceCstValidation);

                    if (CstIcms == "10" && ValorAliquotaIcms <= decimal.Zero)
                        AddNotification("CstIcms", "Produto com CST 10, obrigatório informar AliquotaICMS");

                    if (CstPisCofins == "01" && ValorAliquotaPis <= decimal.Zero)
                        AddNotification("AliquotaPis", "Produto com CST_PIS 01, obrigatório informar AliquotaPIS");

                    if (CstPisCofins == "01" && ValorAliquotaCofins <= decimal.Zero)
                        AddNotification("CstPisCofins", "Produto com CST_COFINS 01, obrigatório informar AliquotaCOFINS");
                }
                else
                {
                    AddNotification("RegimeTributario", $"Regime tributario do item inválido");
                }
            }
            else if (ModeloDocumento == ModeloDocumento.NFe)
            {
                if (RegimeTributario == CRT.SimplesNacional || RegimeTributario == CRT.SimplesNacionalMei)
                {
                    // Regra





                }
                else if (RegimeTributario == CRT.SimplesNacionalExcessoSublimite || RegimeTributario == CRT.RegimeNormal)
                {
                    // Regra
                }
                else
                {
                    AddNotification("RegimeTributario", $"Regime tributario do item inválido");
                }
            }
            else
            {
                AddNotification("ModeloDocumento", $"Modelo Documento inválido");
            }

        }

        public void DefinirValorFreteRatiado(decimal valorFreteRatiado)
        {
            ValorFreteRateado = valorFreteRatiado;
        }

        public void DefinirValorFreteBaseCalculoRateado(decimal valorFreteBaseCalculoRateado)
        {
            ValorFreteBaseCalculoRateado = valorFreteBaseCalculoRateado;
        }

        public void DefinirValorDescontoRatiado(decimal valorDescontoRatiado)
        {
            ValorDescontoRateado = valorDescontoRatiado;
        }

        public void DefinirValorOutro(decimal valorOutro)
        {
            ValorOutroRateado = valorOutro;
        }

        public void DefinirValorAcrescimoRatiado(decimal valorAcrescimoRatiado)
        {
            ValorAcrescimoRateado = valorAcrescimoRatiado;
        }

        public void DefinirValorSeguroRatiado(decimal valorSeguroRateado)
        {
            ValorSeguroRateado = valorSeguroRateado;
        }

        public void DefinirValorAproximadoIbpt(decimal aliqNacionalFederal, decimal aliqImportadoFederal, decimal aliqEstadual, decimal aliqMunicipal, string versao)
        {
            decimal soma = decimal.Zero;

            if (Origem == "0")
            {
                Ibpt = new VendaItemIbpt(((ValorItem + ObterAdicionaisAEmbutirBaseCalculoIcms() - ObterDescontosAEmbutirBaseCalculoIcms()) * (aliqNacionalFederal / 100)).Arredonda2(),
                    decimal.Zero,
                    ((ValorItem + ObterAdicionaisAEmbutirBaseCalculoIcms() - ObterDescontosAEmbutirBaseCalculoIcms()) * (aliqEstadual / 100)).Arredonda2(),
                    ((ValorItem + ObterAdicionaisAEmbutirBaseCalculoIcms() - ObterDescontosAEmbutirBaseCalculoIcms()) * (aliqMunicipal / 100)).Arredonda2(),
                    versao);

                soma = ValorItem * ((aliqNacionalFederal + aliqEstadual + aliqMunicipal) / 100);
            }
            else
            {
                Ibpt = new VendaItemIbpt(decimal.Zero, ((ValorItem + ObterAdicionaisAEmbutirBaseCalculoIcms() - ObterDescontosAEmbutirBaseCalculoIcms()) * (aliqImportadoFederal / 100)).Arredonda2(), decimal.Zero, decimal.Zero, versao);
                soma = ValorItem * ((aliqImportadoFederal) / 100);
            }

            ValorAproximadoIbpt = soma.Arredonda2();
        }

        public void DefinirValoresEmbutir(bool embuteFrete, bool embuteSeguro, bool embuteAcrescimo, bool embuteOutro)
        {
            EmbuteFrete = embuteFrete;
            EmbuteSeguro = embuteSeguro;
            EmbuteAcrescimo = embuteAcrescimo;
            EmbuteOutro = embuteOutro;
        }

        public void DefinirImposto(VendaItemImposto imposto)
        {
            Imposto = imposto;
        }

        public void DefinirProdutoEspecificoCombustivel(string codigoAnp, string descricaoAnp, decimal? pMixGN, decimal? pGLP, decimal? pGNn, decimal? pGNi, decimal? vPart, string cODIF, decimal? qTemp, string uFCons, decimal? pBio, ICollection<VendaItemProdutoCombustivelOrigemCombustivel> origComb)
        {
            Combustivel = new VendaItemProdutoCombustivel
            (
                codigoAnp,
                descricaoAnp,
                pMixGN,
                pGLP,
                pGNn,
                pGNi,
                vPart,
                cODIF,
                qTemp,
                uFCons,
                pBio
            );

            foreach (var origem in origComb)
                Combustivel.AdicionarOrigemCombustivel(origem.IndImport, origem.CUFOrig, origem.POrig);
        }

        public void AplicarRegraCodigoBeneficioFiscal(string? codigoBeneficioFiscal, string? cst)
        {
            if (RegimeTributario == CRT.SimplesNacional || RegimeTributario == CRT.SimplesNacionalMei)
            {
                if (codigoBeneficioFiscal == "PR")
                {
                    CodigoBeneficioFiscal = null;
                    return;
                }
            }

            string[] cstCodNeficios = ["00", "02", "10", "15", "60", "61"];

            if (string.IsNullOrEmpty(codigoBeneficioFiscal))
            {
                CodigoBeneficioFiscal = cstCodNeficios.Contains(CstIcms) ? null : "SEM CBENEF";
            }
            else
            {
                CodigoBeneficioFiscal = cstCodNeficios.Contains(CstIcms) ? null : codigoBeneficioFiscal;
            }
        }
    }
}
