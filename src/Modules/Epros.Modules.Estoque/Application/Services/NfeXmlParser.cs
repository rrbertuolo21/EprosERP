using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Enums;

namespace Epros.Modules.Estoque.Application.Services
{
    /// <summary>
    /// Parser leve de XML de NF-e de entrada (padrão nacional Portal Fiscal), baseado em XDocument.
    /// Reaproveita a lógica de mapeamento do serviço legado (ImportacaoXmlService) mas sem depender das
    /// bibliotecas OpenAC/NFe.Classes — extrai emitente, destinatário, itens, total, transporte e
    /// pagamentos diretamente das tags do XML e monta um <see cref="LancarCompraFiscalCommand"/>.
    /// </summary>
    public static class NfeXmlParser
    {
        private static readonly XNamespace Nfe = "http://www.portalfiscal.inf.br/nfe";

        /// <summary>
        /// Converte o conteúdo de um XML de NF-e em um comando de lançamento fiscal de compra.
        /// Aceita tanto o envelope &lt;nfeProc&gt; quanto a &lt;NFe&gt; direta.
        /// </summary>
        /// <exception cref="ArgumentException">XML vazio ou sem a estrutura mínima de NF-e.</exception>
        public static LancarCompraFiscalCommand Parse(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                throw new ArgumentException("O XML da NF-e está vazio.");

            XDocument doc;
            try
            {
                doc = XDocument.Parse(xml);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"XML da NF-e inválido: {ex.Message}", ex);
            }

            var infNFe = doc.Descendants(Nfe + "infNFe").FirstOrDefault();
            if (infNFe == null)
                throw new ArgumentException("Estrutura da NF-e não encontrada (tag infNFe ausente).");

            var ide = infNFe.Element(Nfe + "ide");
            var emit = infNFe.Element(Nfe + "emit");
            var dest = infNFe.Element(Nfe + "dest");
            var total = infNFe.Element(Nfe + "total")?.Element(Nfe + "ICMSTot");
            var transp = infNFe.Element(Nfe + "transp");
            var pag = infNFe.Element(Nfe + "pag");

            // Chave de acesso: atributo Id ("NFe" + 44 dígitos).
            var idAttr = (string?)infNFe.Attribute("Id") ?? string.Empty;
            var chaveAcesso = new string(idAttr.Where(char.IsDigit).ToArray());

            var numeroNota = Str(ide?.Element(Nfe + "nNF")) ?? string.Empty;
            var dataEmissao = ParseData(Str(ide?.Element(Nfe + "dhEmi")) ?? Str(ide?.Element(Nfe + "dEmi")));

            // Emitente (fornecedor).
            var fornecedorCnpj = Str(emit?.Element(Nfe + "CNPJ"));
            var fornecedorCpf = Str(emit?.Element(Nfe + "CPF"));
            var fornecedorDoc = fornecedorCnpj ?? fornecedorCpf ?? string.Empty;
            var fornecedorNome = Str(emit?.Element(Nfe + "xNome")) ?? string.Empty;

            var emitenteInput = new EmitenteInput(
                Cnpj: fornecedorCnpj,
                Cpf: fornecedorCpf,
                RazaoSocial: fornecedorNome,
                NomeFantasia: Str(emit?.Element(Nfe + "xFant")),
                Telefone: Str(emit?.Element(Nfe + "enderEmit")?.Element(Nfe + "fone")),
                InscricaoEstadual: Str(emit?.Element(Nfe + "IE")) ?? string.Empty,
                Cnae: ToInt(Str(emit?.Element(Nfe + "CNAE"))),
                RegimeTributario: ParseRegime(Str(emit?.Element(Nfe + "CRT"))));

            // Destinatário (comprador).
            DestinatarioInput? destinatarioInput = null;
            if (dest != null)
            {
                destinatarioInput = new DestinatarioInput(
                    PessoaId: null,
                    Cnpj: Str(dest.Element(Nfe + "CNPJ")),
                    Cpf: Str(dest.Element(Nfe + "CPF")),
                    RazaoSocial: Str(dest.Element(Nfe + "xNome")) ?? string.Empty,
                    Telefone: Str(dest.Element(Nfe + "enderDest")?.Element(Nfe + "fone")),
                    InscricaoEstadual: Str(dest.Element(Nfe + "IE")),
                    Email: Str(dest.Element(Nfe + "email")),
                    IndicadorIE: ParseIndicadorIe(Str(dest.Element(Nfe + "indIEDest"))),
                    EhConsumidorFinal: false);
            }

            // Itens.
            var itens = new List<ItemCompraFiscalInput>();
            foreach (var det in infNFe.Elements(Nfe + "det"))
            {
                var prod = det.Element(Nfe + "prod");
                if (prod == null) continue;

                var sku = Str(prod.Element(Nfe + "cProd")) ?? string.Empty;
                var nome = Str(prod.Element(Nfe + "xProd")) ?? string.Empty;
                var quantidade = ToDecimal(Str(prod.Element(Nfe + "qCom")));
                var precoUnitario = ToDecimal(Str(prod.Element(Nfe + "vUnCom")));

                var imposto = det.Element(Nfe + "imposto");
                var valorIcms = SomarValores(imposto?.Element(Nfe + "ICMS"), "vICMS");
                var valorIpi = SomarValores(imposto?.Element(Nfe + "IPI"), "vIPI");

                itens.Add(new ItemCompraFiscalInput(
                    Sku: sku,
                    NomeProduto: nome,
                    Quantidade: quantidade,
                    PrecoUnitario: precoUnitario,
                    ValorIms: valorIcms,
                    ValorIpi: valorIpi,
                    Ncm: Str(prod.Element(Nfe + "NCM")),
                    Cfop: ToInt(Str(prod.Element(Nfe + "CFOP"))),
                    UnidadeComercial: Str(prod.Element(Nfe + "uCom"))));
            }

            // Total.
            var valorNota = total != null ? ToDecimal(Str(total.Element(Nfe + "vNF"))) : itens.Sum(i => i.Quantidade * i.PrecoUnitario);
            TotalCompraInput? totalInput = total != null
                ? new TotalCompraInput(
                    ValorProduto: ToDecimal(Str(total.Element(Nfe + "vProd"))),
                    ValorFrete: ToDecimal(Str(total.Element(Nfe + "vFrete"))),
                    ValorSeguro: ToDecimal(Str(total.Element(Nfe + "vSeg"))),
                    ValorDesconto: ToDecimal(Str(total.Element(Nfe + "vDesc"))),
                    ValorIpi: ToDecimal(Str(total.Element(Nfe + "vIPI"))),
                    ValorIcms: ToDecimal(Str(total.Element(Nfe + "vICMS"))),
                    ValorNotaFiscal: valorNota)
                : null;

            // Transporte (transportadora).
            TransporteCompraInput? transporteInput = null;
            var transporta = transp?.Element(Nfe + "transporta");
            if (transporta != null)
            {
                transporteInput = new TransporteCompraInput(
                    TransportadoraPessoaId: null,
                    TransportadoraCnpj: Str(transporta.Element(Nfe + "CNPJ")),
                    TransportadoraCpf: Str(transporta.Element(Nfe + "CPF")),
                    TransportadoraRazaoSocialOuNome: Str(transporta.Element(Nfe + "xNome")),
                    TransportadoraInscricaoEstadual: Str(transporta.Element(Nfe + "IE")),
                    TransportadoraUf: ParseEstado(Str(transporta.Element(Nfe + "UF"))));
            }

            // Pagamentos.
            var pagamentos = new List<PagamentoCompraInput>();
            if (pag != null)
            {
                foreach (var detPag in pag.Elements(Nfe + "detPag"))
                {
                    var valor = ToDecimal(Str(detPag.Element(Nfe + "vPag")));
                    if (valor <= 0m) continue;
                    pagamentos.Add(new PagamentoCompraInput(
                        TipoPagamento: ParseTipoPagamento(Str(detPag.Element(Nfe + "tPag"))),
                        ValorPagamento: valor));
                }
            }

            return new LancarCompraFiscalCommand(
                FornecedorCnpj: fornecedorDoc,
                FornecedorNome: fornecedorNome,
                NumeroNota: numeroNota,
                ChaveAcesso: chaveAcesso,
                ValorTotal: valorNota,
                DataEmissao: dataEmissao,
                Itens: itens,
                Emitente: emitenteInput,
                Destinatario: destinatarioInput,
                Imposto: null,
                Total: totalInput,
                Transporte: transporteInput,
                Pagamentos: pagamentos.Count > 0 ? pagamentos : null);
        }

        private static string? Str(XElement? el)
        {
            var v = el?.Value;
            return string.IsNullOrWhiteSpace(v) ? null : v.Trim();
        }

        private static decimal ToDecimal(string? v) =>
            decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0m;

        private static int ToInt(string? v) =>
            int.TryParse(new string((v ?? string.Empty).Where(char.IsDigit).ToArray()), out var i) ? i : 0;

        private static DateTime ParseData(string? v) =>
            DateTimeOffset.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dto)
                ? dto.UtcDateTime
                : DateTime.UtcNow;

        /// <summary>Soma o valor de uma tag (ex.: vICMS) em qualquer sub-grupo de ICMS/IPI da NF-e.</summary>
        private static decimal SomarValores(XElement? grupo, string tag)
        {
            if (grupo == null) return 0m;
            return grupo.Descendants(Nfe + tag).Sum(e => ToDecimal(e.Value));
        }

        private static ERegimeTributario ParseRegime(string? crt) => crt switch
        {
            "1" => ERegimeTributario.SimplesNacional,
            "2" => ERegimeTributario.SimplesNacionalExcessoSublimite,
            "3" => ERegimeTributario.RegimeNormal,
            _ => ERegimeTributario.SimplesNacional
        };

        private static ETipoIndicadorIe ParseIndicadorIe(string? ind) => ind switch
        {
            "1" => ETipoIndicadorIe.ContribuinteICMS,
            "2" => ETipoIndicadorIe.Isento,
            "9" => ETipoIndicadorIe.NaoContribuinte,
            _ => ETipoIndicadorIe.NaoContribuinte
        };

        private static ETipoPagamento ParseTipoPagamento(string? tPag) => tPag switch
        {
            "01" => ETipoPagamento.Dinheiro,
            "02" => ETipoPagamento.Cheque,
            "03" => ETipoPagamento.CartaoCredito,
            "04" => ETipoPagamento.CartaoDebito,
            "05" => ETipoPagamento.CartaoDaLoja,
            "10" => ETipoPagamento.ValeAlimentacao,
            "11" => ETipoPagamento.ValeRefeicao,
            "12" => ETipoPagamento.ValePresente,
            "13" => ETipoPagamento.ValeCombustivel,
            "14" => ETipoPagamento.DuplicataMercantil,
            "15" => ETipoPagamento.BoletoBancario,
            "16" => ETipoPagamento.DepositoBancario,
            "17" => ETipoPagamento.PagamentoInstantaneoPixDinamico,
            "18" => ETipoPagamento.TransferenciaBancaria,
            "19" => ETipoPagamento.ProgramaDeFidelidade,
            "20" => ETipoPagamento.PagamentoInstantaneoPixEstatico,
            "90" => ETipoPagamento.SemPagamento,
            "99" => ETipoPagamento.Outros,
            _ => ETipoPagamento.Outros
        };

        private static EEstado ParseEstado(string? uf) =>
            Enum.TryParse<EEstado>(uf, true, out var e) ? e : EEstado.SP;
    }
}
