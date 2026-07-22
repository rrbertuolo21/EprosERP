using DFe.Classes.Flags;
using Epros.ERP.DfeCalculos.Impostos;
using Epros.ERP.DfeCalculos.Impostos.Ibpts;
using Epros.ERP.DfeCalculos.Impostos.Rateios;
using Epros.ERP.Shared.Enums;
using Flunt.Notifications;
using Flunt.Validations;
using NFe.Classes.Informacoes.Detalhe;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.Tipos;
using NFe.Classes.Informacoes.Identificacao.Tipos;
using NFe.Classes.Informacoes.Pagamento;
using NFe.Classes.Informacoes.Transporte;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class Venda : Notifiable<Notification>
    {
        public Venda() { }

        public Venda(VendaEmitente emitente, string localizadorExternoId)
        {
            NaturezaOperacao = "";
            Emitente = emitente;
            LocalizadorExternoId = localizadorExternoId;

            IndicadorPresenca = PresencaComprador.pcPresencial;
            IndicadorIntermediador = IndicadorIntermediador.iiSemIntermediador;

            Pagamentos = new List<VendaPagamento>();
            Itens = new List<VendaItem>();

        }

        public Venda(string naturezaOperacao, decimal valorDesconto, decimal valorAcrescimo, decimal valorSeguro, decimal valorOutro, decimal valorTotal, VendaDocumentoDfe documento, VendaEmitente emitente, VendaDestinatario destinatario, VendaTransporte transporte, VendaCobranca cobranca, ModalidadeFrete modalidadeFrete, decimal valorFrete, string? informacoesComplementares, string? informacoesAdicionaisFisco, string? localizadorExternoId, string[]? documentoAutorizacaoXml, PresencaComprador indicadorPresenca, IndicadorIntermediador indicadorIntermediador, VendaEntrega entrega, VendaIntermediador? intermediador, VendaImposto imposto)
        {
            CobraFrete = true;
            NaturezaOperacao = string.IsNullOrEmpty(naturezaOperacao) ? null! : naturezaOperacao;
            ValorDesconto = valorDesconto;
            ValorAcrescimo = valorAcrescimo;
            ValorSeguro = valorSeguro;
            ValorOutro = valorOutro;
            ValorTotal = valorTotal;
            Documento = documento;
            Emitente = emitente;
            Destinatario = destinatario;
            Tansporte = transporte;
            Cobranca = cobranca;
            ModalidadeFrete = modalidadeFrete;
            ValorFrete = valorFrete;
            InformacoesComplementares = informacoesComplementares;
            InformacoesAdicionaisFisco = informacoesAdicionaisFisco;
            LocalizadorExternoId = localizadorExternoId;
            DocumentoAutorizacaoXml = documentoAutorizacaoXml;
            IndicadorPresenca = indicadorPresenca;
            IndicadorIntermediador = indicadorIntermediador;
            Entrega = entrega;
            Intermediador = intermediador;
            Imposto = imposto;

            Pagamentos = new List<VendaPagamento>();
            Itens = new List<VendaItem>();
            Validar();
        }

        public string NaturezaOperacao { get; private set; } = null!;
        public decimal ValorDesconto { get; private set; }
        public decimal ValorAcrescimo { get; private set; }
        public decimal ValorSeguro { get; private set; }
        public decimal ValorOutro { get; private set; }
        public decimal ValorTotal { get; private set; }
        public decimal ValorTotalRecebimento { get; private set; }
        public string AdicionalIbpt { get; private set; } = null!;
        public decimal ValorCalculoIbpt { get; private set; }
        public ModalidadeFrete ModalidadeFrete { get; private set; }
        public decimal ValorFrete { get; private set; }
        public bool CobraFrete { get; private set; }

        public string? InformacoesComplementares { get; private set; }
        public string? InformacoesAdicionaisFisco { get; private set; }
        public string? LocalizadorExternoId { get; private set; }
        public string[]? DocumentoAutorizacaoXml { get; private set; }
        public PresencaComprador IndicadorPresenca { get; private set; }
        public IndicadorIntermediador IndicadorIntermediador { get; private set; }
        public VendaDocumentoDfe Documento { get; private set; } = null!;
        public VendaEmitente Emitente { get; private set; } = null!;
        public VendaDestinatario Destinatario { get; private set; } = null!;
        public VendaTransporte Tansporte { get; private set; } = null!;
        public VendaCobranca Cobranca { get; private set; } = null!;
        public VendaEntrega? Entrega { get; private set; }
        public VendaIntermediador? Intermediador { get; private set; }
        public VendaExportacao? Exportacao { get; private set; }

        public VendaImposto Imposto { get; private set; } = null!;

        // Listas
        public ICollection<VendaPagamento> Pagamentos { get; private set; } = null!;
        public ICollection<VendaItem> Itens { get; private set; } = null!;

        public void Validar()
        {
            AddNotifications(new Contract<Notification>()

                //.IsGreaterThan(ValorTotal, decimal.Zero, "Total da NF precisa ser maior que zero")

                .IsGreaterOrEqualsThan(ValorDesconto, decimal.Zero, "Desconto da NF deve ser maior ou igual a zero")

                .IsGreaterOrEqualsThan(ValorSeguro, decimal.Zero, "Seguro da NF deve ser maior ou igual a zero")

                .IsGreaterOrEqualsThan(ValorOutro, decimal.Zero, "Taxa / Valor Outro da NF deve ser maior ou igual a zero")

                .IsLowerOrEqualsThan(60, (NaturezaOperacao ?? "").Length, "NaturezaOperacao", "Natureza Operação Complementares pode conter no max 60 caracteres")

                .IsLowerOrEqualsThan(5000, (InformacoesComplementares ?? "").Length, "InformacoesComplementares", "Informaçõees Complementares pode conter no max 5000 caracteres")

                .IsLowerOrEqualsThan(2000, (InformacoesAdicionaisFisco ?? "").Length, "InformacoesAdicionaisFisco", "Informações AdicionaisFisco pode conter no max 2000 caracteres")

                );

            if (Documento != null && Documento.Notifications.Count > 0)
                AddNotifications(Documento.Notifications);

            if (Emitente.Notifications.Count > 0)
                AddNotifications(Emitente.Notifications);

            if (Destinatario != null && Destinatario?.Notifications.Count > 0)
                AddNotifications(Destinatario.Notifications);

            if (Tansporte != null && Tansporte?.Notifications.Count > 0)
                AddNotifications(Tansporte.Notifications);
        }

        public void AdicionarPagamento(decimal valorPago, decimal valorTroco, FormaPagamento formaPagamento, IndicadorPagamentoDetalhePagamento indicacaoPagamento, string descricao, VendaPagamentoCartao? cartao)
        {
            var pag = new VendaPagamento(valorPago, valorTroco, formaPagamento, indicacaoPagamento, descricao, cartao);
            if (pag.Notifications.Count > 0)
                AddNotifications(pag.Notifications);
            Pagamentos.Add(pag);

            if (valorTroco == decimal.Zero && pag.FormaPagamento == FormaPagamento.fpDinheiro)
                pag.RedefinirTroco(ObterValorTroco());
        }

        public void RemoverPagamento(int index)
        {
            var pagamento = Pagamentos.ElementAtOrDefault(index);

            if (pagamento == null) return;

            Pagamentos.Remove(pagamento);
        }

        public void AdicionarItem(VendaItem item)
        {
            if (item.Notifications.Count > 0)
                AddNotifications(item.Notifications);

            Itens.Add(item);
        }

        public VendaItem AdicionarItem(long produtoId, string? codigoProduto, string? nomeProduto, string? codigoBarras, string? ncm, VendaItemTributacaoCfop tributacaoCfop, string? unidade, decimal valorUnitario, decimal quantidade, string? origem, string? csosn, string? cstIcms, decimal valorAliquotaIcms, decimal valorReducaoIcmsPercentual, ETipoReducaoBaseDeCalculo reducaoIcms, decimal valorBaseCalculoStRetidoOperacaoAnterior, decimal valorAlioquotaSt, decimal valorIcmsStRetidoOperacaoAnterior, decimal valorIcmsProprioSubstituto, string cstPisCofins, decimal valorAliquotaPis, decimal valorAliquotaPisReal, decimal valorAliquotaCofins, decimal valorAliquotaCofinsReal, IndicadorTotal compoeValorTotal, decimal valorDesconto, string? enquadramentoIpi, string? cstIpi, decimal valorAliquotaIpi, decimal valorReducaoIpiPercentual, ETipoReducaoBaseDeCalculo tipoReducaoIpi, string? cest, decimal valorAliquotaMva, decimal valorAliquotaFcp, decimal valorAliquotaFcpSt, decimal valorAliquotaIcmsInterna, decimal valorAliquotaIcmsInterestadual, decimal valorReducaoIcmsPercentualSt, ETipoReducaoBaseDeCalculo tipoReducaoIcmsSt, bool ipiEmbutido, bool difalTipoCalculoPorDentro, bool embuteFrete, bool embuteSeguro, bool embuteAcrescimo, bool embuteOutro, DeterminacaoBaseIcmsSt tipoCalculoBaseIcmsSt, decimal valorUnitFixadoIcmsSt, string? cstIbsCbs, string? cClassTrib, decimal qtdeBCMonoRetido, decimal valorAliquotaAdRemRetido, string? informacoesAdProduto, int? numeroItemPedidoCompra, string numeroPedidoCompra, string fichaConteudoImportacao, string? unidadeTributavel, string? codigoBeneficioFiscal, decimal valorCusto, string? externoId = null)
        {
            var item = new VendaItem(Emitente.RegimeTributario, Documento.ModeloDocumento, Emitente.Cnae, produtoId, codigoProduto, nomeProduto, codigoBarras, ncm, tributacaoCfop, unidade, valorUnitario, quantidade, origem, csosn, cstIcms, valorAliquotaIcms, valorReducaoIcmsPercentual, reducaoIcms, valorBaseCalculoStRetidoOperacaoAnterior, valorAlioquotaSt, valorIcmsStRetidoOperacaoAnterior, valorIcmsProprioSubstituto, cstPisCofins, valorAliquotaPis, valorAliquotaPisReal, valorAliquotaCofins, valorAliquotaCofinsReal, compoeValorTotal, valorDesconto, enquadramentoIpi, cstIpi, valorAliquotaIpi, valorReducaoIpiPercentual, tipoReducaoIpi, cest, valorAliquotaMva, valorAliquotaFcp, valorAliquotaFcpSt, valorAliquotaIcmsInterna, valorAliquotaIcmsInterestadual, valorReducaoIcmsPercentualSt, tipoReducaoIcmsSt, ipiEmbutido, difalTipoCalculoPorDentro, embuteFrete, embuteSeguro, embuteAcrescimo, embuteOutro, tipoCalculoBaseIcmsSt, valorUnitFixadoIcmsSt, cstIbsCbs, cClassTrib, qtdeBCMonoRetido, valorAliquotaAdRemRetido, informacoesAdProduto, numeroItemPedidoCompra, numeroPedidoCompra, fichaConteudoImportacao, unidadeTributavel, codigoBeneficioFiscal, valorCusto);
            if (item.Notifications.Count > 0)
                AddNotifications(item.Notifications);

            Itens.Add(item);

            if (item.Notifications.Count == 0)
                Calcular();

            if (item.Notifications.Count > 0)
                AddNotifications(item.Notifications);

            return item;
        }

        public void AlterarItem(int index, long produtoId, string? codigoProduto, string? nomeProduto, string? codigoBarras, string? ncm, VendaItemTributacaoCfop tributacaoCfop, string? unidade, decimal valorUnitario, decimal quantidade, string? origem, string? csosn, string? cstIcms, decimal valorAliquotaIcms, decimal valorReducaoIcmsPercentual, ETipoReducaoBaseDeCalculo reducaoIcms, decimal valorBaseCalculoStRetidoOperacaoAnterior, decimal valorAlioquotaSt, decimal valorIcmsStRetidoOperacaoAnterior, decimal valorIcmsProprioSubstituto, string cstPisCofins, decimal valorAliquotaPis, decimal valorAliquotaPisReal, decimal valorAliquotaCofins, decimal valorAliquotaCofinsReal, IndicadorTotal compoeValorTotal, decimal valorDesconto, string? enquadramentoIpi, string? cstIpi, decimal valorAliquotaIpi, decimal valorReducaoIpiPercentual, ETipoReducaoBaseDeCalculo tipoReducaoIpi, string? cest, decimal valorAliquotaMva, decimal valorAliquotaFcp, decimal valorAliquotaFcpSt, decimal valorAliquotaIcmsInterna, decimal valorAliquotaIcmsInterestadual, decimal valorReducaoIcmsPercentualSt, ETipoReducaoBaseDeCalculo tipoReducaoIcmsSt, bool ipiEmbutido, bool difalTipoCalculoPorDentro, bool embuteFrete, bool embuteSeguro, bool embuteAcrescimo, bool embuteOutro, DeterminacaoBaseIcmsSt tipoCalculoBaseIcmsSt, decimal valorUnitFixadoIcmsSt, string? cstIbsCbs, string? cClassTrib, string? unidadeTributavel, string? codigoBeneficioFiscal, decimal valorCusto)
        {
            var item = Itens.ElementAtOrDefault(index);

            if (item == null) return;

            item.Alterar(produtoId, codigoProduto, nomeProduto, codigoBarras, ncm, tributacaoCfop, unidade, valorUnitario, quantidade, origem, csosn, cstIcms, valorAliquotaIcms, valorReducaoIcmsPercentual, reducaoIcms, valorBaseCalculoStRetidoOperacaoAnterior, valorAlioquotaSt, valorIcmsStRetidoOperacaoAnterior, valorIcmsProprioSubstituto, cstPisCofins, valorAliquotaPis, valorAliquotaPisReal, valorAliquotaCofins, valorAliquotaCofinsReal, compoeValorTotal, valorDesconto, enquadramentoIpi, cstIpi, valorAliquotaIpi, valorReducaoIpiPercentual, tipoReducaoIpi, cest, valorAliquotaMva, valorAliquotaFcp, valorAliquotaFcpSt, valorAliquotaIcmsInterna, valorAliquotaIcmsInterestadual, valorReducaoIcmsPercentualSt, tipoReducaoIcmsSt, ipiEmbutido, difalTipoCalculoPorDentro, embuteFrete, embuteSeguro, embuteAcrescimo, embuteOutro, tipoCalculoBaseIcmsSt, valorUnitFixadoIcmsSt, cstIbsCbs, cClassTrib, unidadeTributavel, codigoBeneficioFiscal, valorCusto);

            if (item.Notifications.Count > 0)
                AddNotifications(item.Notifications);

            if (item.Notifications.Count == 0)
                Calcular();
        }

        public void RemoverItem(int index)
        {
            var item = Itens.ElementAtOrDefault(index);

            if (item == null) return;

            Itens.Remove(item);
            Calcular();
        }

        public void RemoverTodosItens()
        {
            Itens.Clear();
            Calcular();
        }

        public void CalcularValorAproximadoIbpt()
        {
            AdicionalIbpt = IbptCalculo.GerarObservacaoIbptValores(Itens.Sum(x => x.Ibpt?.AliqFederal ?? decimal.Zero), Itens.Sum(x => x.Ibpt?.AliqImportado ?? decimal.Zero), Itens.Sum(x => x.Ibpt?.AliqEstadual ?? decimal.Zero), Itens.Sum(x => x.Ibpt?.AliqMunicipal ?? decimal.Zero));
            ValorCalculoIbpt = Itens.Sum(x => x.ValorAproximadoIbpt);
        }

        public void AdicionarCobranca(VendaCobrancaFatura fatura, ICollection<VendaCobrancaDuplicata> duplicatas)
        {
            var cobr = new VendaCobranca(fatura, duplicatas);
            if (cobr.Notifications.Count > 0)
                AddNotifications(cobr.Notifications);
            Cobranca = cobr;
        }

        public void DefinirDestinatario(VendaDestinatario destinatario)
        {
            if (destinatario.Notifications.Count > 0)
                AddNotifications(destinatario.Notifications);
            Destinatario = destinatario;
            Calcular();
        }

        public void DefinirTransporte(VendaTransporte transporte)
        {
            if (transporte.Notifications.Count > 0)
                AddNotifications(transporte.Notifications);
            Tansporte = transporte;
        }

        public void DefinirDocumento(VendaDocumentoDfe documento)
        {
            if (documento.Notifications.Count > 0)
                AddNotifications(documento.Notifications);
            Documento = documento;
        }

        public void DefinirCobranca(VendaCobranca cobranca)
        {
            if (cobranca.Notifications.Count > 0)
                AddNotifications(cobranca.Notifications);
            Cobranca = cobranca;
        }

        public void DefinirExportacao(string ufSaidaPais, string localExportacao, string localDespacho)
        {
            Exportacao = new VendaExportacao(ufSaidaPais, localExportacao, localDespacho);
        }

        public void DefinirIndicadorPresencaIntermediador(PresencaComprador indicadorPresenca, IndicadorIntermediador indicadorIntermediador)
        {
            IndicadorPresenca = indicadorPresenca;
            IndicadorIntermediador = indicadorIntermediador;
        }

        public void DefinirEntrega(VendaEntrega entrega)
        {
            if (entrega.Notifications.Count > 0)
                AddNotifications(entrega.Notifications);
            Entrega = entrega;
        }

        public void DefinirIntermediador(VendaIntermediador intermediador)
        {
            if (intermediador.Notifications.Count > 0)
                AddNotifications(intermediador.Notifications);
            Intermediador = intermediador;
        }

        public void DefinirImposto(VendaImposto imposto)
        {
            if (imposto.Notifications.Count > 0)
                AddNotifications(imposto.Notifications);
            Imposto = imposto;
        }

        public void DefinirValoresVenda(decimal valorDesconto, decimal valorAcrescimo, decimal valorSeguro, decimal valorOutro)
        {
            ValorDesconto = valorDesconto;
            ValorAcrescimo = valorAcrescimo;
            ValorSeguro = valorSeguro;
            ValorOutro = valorOutro;
            Calcular();
        }

        public void DefinirValoresEmbutir(bool embuteFrete, bool embuteSeguro, bool embuteAcrescimo, bool embuteOutro)
        {
            if (Itens == null || Itens.Count() == 0) return;
            foreach (var item in Itens)
                item.DefinirValoresEmbutir(embuteFrete, embuteSeguro, embuteAcrescimo, embuteOutro);
            Calcular();
        }

        public void DefinirFrete(ModalidadeFrete modalidadeFrete, decimal valorFrete, bool cobraFrete)
        {
            ModalidadeFrete = modalidadeFrete;
            ValorFrete = valorFrete;
            CobraFrete = cobraFrete;
            Calcular();
        }

        public void DefinirValorTotal(decimal valorTotal)
        {
            ValorTotal = valorTotal;
        }

        public void DefinirDadosInformacoesAdicionaisFisco(string informacoesComplementares, string informacoesAdicionaisFisco)
        {
            InformacoesComplementares = informacoesComplementares;
            InformacoesAdicionaisFisco = informacoesAdicionaisFisco;
        }

        // ICMS Totais

        public decimal ObterValorTotalIcmsValorImpostoDevidoRecolherSt()
        {
            if (Itens.Any(x => x.Imposto?.Icms != null))
            {
                return Itens.Where(x => x.Imposto?.Icms != null)
                             .Sum(x => x.Imposto!.Icms!.ValorImpostoDevidoRecolherSt);
            }
            return decimal.Zero;
        }

        public decimal ObterValorTotalIcmsValorImpostoDevido()
        {
            if (Itens.Any(x => x.Imposto?.Icms != null))
            {
                return Itens.Where(x => x.Imposto?.Icms != null)
                             .Sum(x => x.Imposto!.Icms!.ValorImpostoDevido);
            }
            return decimal.Zero;
        }

        public decimal ObterValorTotalIcmsValorImpostoDevidoFcp()
        {
            if (Itens.Any(x => x.Imposto?.Icms != null))
            {
                return Itens.Where(x => x.Imposto?.Icms != null)
                             .Sum(x => x.Imposto!.Icms!.ValorImpostoDevidoFcp);
            }
            return decimal.Zero;
        }

        public decimal ObterValorTotalIcmsValorImpostoDevidoFcpSt()
        {
            if (Itens.Any(x => x.Imposto?.Icms != null))
            {
                return Itens.Where(x => x.Imposto?.Icms != null)
                             .Sum(x => x.Imposto!.Icms!.ValorImpostoDevidoFcpSt);
            }
            return decimal.Zero;
        }

        public decimal ObterValorTotalIcmsValorImpostoDevidoRecolherFcpSt()
        {
            if (Itens.Any(x => x.Imposto?.Icms != null))
            {
                return Itens.Where(x => x.Imposto?.Icms != null)
                             .Sum(x => x.Imposto!.Icms!.ValorImpostoDevidoRecolherFcpSt);
            }
            return decimal.Zero;
        }

        public decimal ObterValorTotalIcmsValorBaseDeCalculo()
        {
            if (Itens.Any(x => x.Imposto?.Icms != null))
            {
                return Itens.Where(x => x.Imposto?.Icms != null)
                             .Sum(x => x.Imposto!.Icms!.ValorBaseDeCalculo);
            }
            return decimal.Zero;
        }

        public decimal ObterValorTotalIcmsValorBaseDeCalculoSt()
        {
            if (Itens.Any(x => x.Imposto?.Icms != null))
            {
                return Itens.Where(x => x.Imposto?.Icms != null)
                             .Sum(x => x.Imposto!.Icms!.ValorBaseDeCalculoSt);
            }
            return decimal.Zero;
        }

        public decimal ObterValorTotalIcmsValorImpostoDevidoDifal()
        {
            if (Itens.Any(x => x.Imposto?.Icms != null))
            {
                return Itens.Where(x => x.Imposto?.Icms != null)
                             .Sum(x => x.Imposto!.Icms!.ValorImpostoDevidoDifal);
            }
            return decimal.Zero;
        }

        // IPI Totais
        public decimal ObterValorTotalIpiValorImpostoDevido()
        {
            if (Itens.Any(x => x.Imposto?.Ipi != null))
            {
                return Itens.Where(x => x.Imposto?.Ipi != null)
                             .Sum(x => x.Imposto!.Ipi!.ValorImpostoDevido);
            }
            return decimal.Zero;
        }

        // PIS Totais
        public decimal ObterValorTotalPisValorImpostoDevido()
        {
            if (Itens.Any(x => x.Imposto?.Pis != null))
            {
                return Itens.Where(x => x.Imposto?.Pis != null)
                             .Sum(x => x.Imposto!.Pis!.ValorImpostoDevido);
            }
            return decimal.Zero;
        }

        // COFINS Totais
        public decimal ObterValorTotalCofinsValorImpostoDevido()
        {
            if (Itens.Any(x => x.Imposto?.Cofins != null))
            {
                return Itens.Where(x => x.Imposto?.Cofins != null)
                             .Sum(x => x.Imposto!.Cofins!.ValorImpostoDevido);
            }
            return decimal.Zero;
        }

        // IbsCbs Totais   
        public decimal ObterValorTotalIbsCbsValorBaseDeCalculo()
        {
            if (Itens.Any(x => x.Imposto?.IbsCbs != null))
            {
                return Itens.Where(x => x.Imposto?.IbsCbs != null)
                             .Sum(x => x.Imposto!.IbsCbs!.ValorBaseDeCalculo);
            }
            return decimal.Zero;
        }

        public decimal ObterValorTotalIbsCbsValorImpostoDevidoMunicipal()
        {
            if (Itens.Any(x => x.Imposto?.IbsCbs != null))
            {
                return Itens.Where(x => x.Imposto?.IbsCbs != null)
                             .Sum(x => x.Imposto!.IbsCbs!.ValorImpostoDevidoMunicipal);
            }
            return decimal.Zero;
        }

        public decimal ObterValorTotalIbsCbsValorImpostoDevidoEstadual()
        {
            if (Itens.Any(x => x.Imposto?.IbsCbs != null))
            {
                return Itens.Where(x => x.Imposto?.IbsCbs != null)
                             .Sum(x => x.Imposto!.IbsCbs!.ValorImpostoDevidoEstadual);
            }
            return decimal.Zero;
        }

        public decimal ObterValorTotalIbsCbsValorImpostoDevidoCbs()
        {
            if (Itens.Any(x => x.Imposto?.IbsCbs != null))
            {
                return Itens.Where(x => x.Imposto?.IbsCbs != null)
                             .Sum(x => x.Imposto!.IbsCbs!.ValorImpostoDevidoCbs);
            }
            return decimal.Zero;
        }

        public decimal ObterValorTotalItens()
        {
            if (Itens != null && Itens.Count() > 0)
            {
                return Itens.Sum(x => x.ValorItem);
            }
            return decimal.Zero;
        }

        public decimal ObterValorTotalItensSemIntegraFaturamento()
        {
            if (Itens != null && Itens.Count() > 0)
            {
                return Itens.Where(x => x.TributacaoCfop?.IntegraFaturamento == false)
                             .Sum(x => x.ValorItem);
            }
            return decimal.Zero;
        }

        public decimal ObterValorTotalItensBruto()
        {
            if (Itens != null && Itens.Count() > 0)
            {
                return Itens.Sum(x => x.Quantidade * x.ValorUnitario);
            }
            return decimal.Zero;
        }

        public decimal ObterValorTotalDesconto()
        {
            if (Itens != null && Itens.Count() > 0)
            {
                return Itens.Sum(x => x.ValorDescontoRateado + x.ValorDesconto);
            }
            return decimal.Zero;
        }

        public decimal ObterValorTotalPagamentos() => Pagamentos.Sum(x => x.ValorPago - x.ValorTroco);

        public decimal ObterValorTroco()
        {
            var valorPagoTotal = Pagamentos.Sum(x => x.ValorPago);
            var valorTrocoCalculado = valorPagoTotal - ValorTotal;
            return (valorTrocoCalculado > decimal.Zero ? valorTrocoCalculado : decimal.Zero);
        }

        public decimal ObterValorTotalRecebimento()
        {
            return ValorTotalRecebimento;
        }

        private void Calcular()
        {
            if (Itens == null || Itens.Count() == 0) return;

            RateioDescontoItens.Ratear(ValorDesconto, Itens);
            RateioAcrescimoItens.Ratear(ValorAcrescimo, Itens);
            RateioFreteItens.Ratear(CobraFrete ? ValorFrete : decimal.Zero, Itens);
            RateioFreteItens.RatearParaBaseDeCalculo(ValorFrete, Itens);
            RateioValorOutroItens.Ratear(ValorOutro, Itens);
            RateioSeguroItens.Ratear(ValorSeguro, Itens);

            foreach (var item in Itens)
            {
                if (item.Notifications.Count > 0)
                    continue;

                item.Imposto.Ipi = (Documento.ModeloDocumento == ModeloDocumento.NFe ? Calculo.CalcularIPI(item) : null);
                item.Imposto.Icms = Calculo.CalcularICMS(this, item);
                item.Imposto.Pis = Calculo.CalcularPIS(item);
                item.Imposto.Cofins = Calculo.CalcularCOFINS(item);
                item.Imposto.IbsCbs = Calculo.CalcularIBSCBS(item);
            }

            AtualizarValorTotal();
        }

        private void AtualizarValorTotal()
        {
            // Verificar se há itens antes de calcular
            if (Itens == null || !Itens.Any())
            {
                ValorTotal = (CobraFrete ? ValorFrete : decimal.Zero) + ValorSeguro + ValorOutro + ValorAcrescimo - ObterValorTotalDesconto();
                return;
            }

            // Calcular ICMS ST com verificação de null
            var icmsSt = ObterValorTotalIcmsValorImpostoDevidoRecolherSt();

            // Calcular ICMS FCP ST com verificação de null
            var icmsFcpSt = ObterValorTotalIcmsValorImpostoDevidoRecolherFcpSt();

            // Calcular IPI com verificação de null
            var ipi = ObterValorTotalIpiValorImpostoDevido();

            // Calcular valor total dos itens que integram faturamento
            var valorItens = ObterValorTotalItens();
            var valorItensNaoIntegraFaturamento = ObterValorTotalItensSemIntegraFaturamento();

            // Calcular valor total final
            ValorTotal = valorItens + icmsSt + icmsFcpSt + ipi;

            decimal descontoRatiado = ObterValorTotalDesconto();

            ValorTotal += ((CobraFrete ? ValorFrete : decimal.Zero) + ValorSeguro + ValorOutro + ValorAcrescimo) - descontoRatiado;

            ValorTotalRecebimento = ValorTotal - valorItensNaoIntegraFaturamento;
        }

    }
}
