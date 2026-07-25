using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    /// <summary>
    /// Importa uma NF-e/NFC-e autorizada a partir do XML (nfeProc/procNFe), persistindo como
    /// <c>DocumentoFiscal</c> Autorizado. Parse namespace-agnóstico (por LocalName) para tolerar o
    /// namespace do portal ou XMLs sem prefixo. Não transmite nada — apenas registra o que já existe.
    /// </summary>
    public class SalvarDocumentoFiscalComXmlCommandHandler : ICommandHandler<SalvarDocumentoFiscalComXmlCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IArmazenamentoArquivoFiscal _armazenamento;
        private readonly IDanfeService _danfeService;

        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public SalvarDocumentoFiscalComXmlCommandHandler(
            ContextFiscal context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IArmazenamentoArquivoFiscal armazenamento,
            IDanfeService danfeService)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _armazenamento = armazenamento;
            _danfeService = danfeService;
        }

        public async Task<CommandResult> Handle(SalvarDocumentoFiscalComXmlCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (string.IsNullOrWhiteSpace(request.Xml))
                return CommandResult.Falha("O XML é obrigatório.");

            XDocument doc;
            try
            {
                doc = XDocument.Parse(request.Xml);
            }
            catch (XmlException ex)
            {
                return CommandResult.Falha($"XML inválido: {ex.Message}");
            }

            var infNFe = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "infNFe");
            if (infNFe is null)
                return CommandResult.Falha("XML não contém uma NF-e/NFC-e (elemento infNFe ausente).");

            // Chave: atributo Id da infNFe = "NFe" + 44 dígitos.
            var idAttr = infNFe.Attribute("Id")?.Value ?? string.Empty;
            var chave = new string(idAttr.Where(char.IsDigit).ToArray());
            if (chave.Length != 44)
                return CommandResult.Falha("Não foi possível extrair a chave de acesso (44 dígitos) da infNFe.");

            // Protocolo de autorização (protNFe/infProt). Sem protocolo => não é documento autorizado.
            var infProt = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "infProt");
            var protocolo = Valor(infProt, "nProt");
            var cStat = Valor(infProt, "cStat");
            if (string.IsNullOrWhiteSpace(protocolo) || cStat != "100")
                return CommandResult.Falha("O XML não representa um documento AUTORIZADO (protNFe/cStat=100 ausente). Importe apenas notas autorizadas.");

            // Idempotência: já importado?
            var jaExiste = await _context.DocumentosFiscais.AnyAsync(d => d.ChaveAcesso == chave, cancellationToken);
            if (jaExiste)
                return CommandResult.Falha($"Já existe um documento fiscal com a chave {chave}.");

            var ide = infNFe.Elements().FirstOrDefault(e => e.Name.LocalName == "ide");
            var modelo = Valor(ide, "mod");
            if (modelo != "55" && modelo != "65")
                return CommandResult.Falha($"Modelo de documento não suportado: '{modelo}'. Apenas 55 (NF-e) e 65 (NFC-e).");

            var serie = ParseInt(Valor(ide, "serie"), 1);
            var numero = ParseLong(Valor(ide, "nNF"), 0);
            var ambiente = Valor(ide, "tpAmb") == "1" ? 1 : 2;

            var dest = infNFe.Elements().FirstOrDefault(e => e.Name.LocalName == "dest");
            var destDoc = Valor(dest, "CNPJ");
            if (string.IsNullOrWhiteSpace(destDoc)) destDoc = Valor(dest, "CPF");
            if (string.IsNullOrWhiteSpace(destDoc)) destDoc = "00000000000";
            var destNome = Valor(dest, "xNome");
            if (string.IsNullOrWhiteSpace(destNome)) destNome = "CONSUMIDOR NAO IDENTIFICADO";

            var totalEl = infNFe.Descendants().FirstOrDefault(e => e.Name.LocalName == "ICMSTot");
            var total = ParseDecimal(Valor(totalEl, "vNF"), 0m);

            var documento = new DocumentoFiscal(modelo, ambiente, serie, numero, total, destDoc, destNome, tenantId, usuario);
            if (!documento.IsValid)
                return CommandResult.Falha(documento.Notifications.Select(n => n.Message), "Dados extraídos do XML são inválidos.");

            if (request.EmpresaId is not null && request.EmpresaId != Guid.Empty)
                documento.VincularEmpresaEmitente(request.EmpresaId.Value);
            if (request.VendaOrigemId is not null && request.VendaOrigemId != Guid.Empty)
                documento.VincularVendaOrigem(request.VendaOrigemId.Value);

            // Itens (det)
            foreach (var det in infNFe.Elements().Where(e => e.Name.LocalName == "det"))
            {
                var prod = det.Elements().FirstOrDefault(e => e.Name.LocalName == "prod");
                if (prod is null) continue;

                var sku = Valor(prod, "cProd");
                var nome = Valor(prod, "xProd");
                var ncm = Valor(prod, "NCM");
                var cfop = ParseInt(Valor(prod, "CFOP"), 0);
                var qtd = ParseDecimal(Valor(prod, "qCom"), 0m);
                var vUn = ParseDecimal(Valor(prod, "vUnCom"), 0m);

                var (cst, aliqIcms, vIcms, bcIcms) = ExtrairIcms(det);

                if (string.IsNullOrWhiteSpace(sku)) sku = "SEM-COD";
                if (string.IsNullOrWhiteSpace(nome)) nome = "ITEM";
                if (string.IsNullOrWhiteSpace(ncm)) ncm = "00000000";
                if (string.IsNullOrWhiteSpace(cst)) cst = "00";
                if (cfop <= 0) cfop = 5102;
                if (qtd <= 0) qtd = 1;

                documento.AdicionarItem(sku, nome, cst, cfop, ncm, qtd, vUn, aliqIcms, usuario);
            }

            if (!documento.IsValid)
                return CommandResult.Falha(documento.Notifications.Select(n => n.Message), "Erro ao montar itens a partir do XML.");

            // Marca como autorizado com os dados reais do XML (o XML já É o retorno autorizado).
            documento.Autorizar(chave, protocolo!, 100, request.Xml, request.Xml, string.Empty, string.Empty);

            _context.DocumentosFiscais.Add(documento);
            await PersistirArtefatosAsync(documento, request.Xml, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Documento fiscal importado do XML e registrado como autorizado.", new
            {
                DocumentoFiscalId = documento.Id,
                ChaveAcesso = documento.ChaveAcesso,
                Protocolo = documento.Protocolo,
                Modelo = documento.Modelo,
                Status = documento.Status
            });
        }

        /// <summary>Extrai CST/CSOSN, alíquota, valor e BC do ICMS do item (qualquer grupo ICMSxx / ICMSSN).</summary>
        private static (string cst, decimal aliq, decimal valor, decimal bc) ExtrairIcms(XElement det)
        {
            var icms = det.Descendants().FirstOrDefault(e => e.Name.LocalName == "ICMS");
            var grupo = icms?.Elements().FirstOrDefault();
            if (grupo is null)
                return ("00", 0m, 0m, 0m);

            var cst = Valor(grupo, "CST");
            if (string.IsNullOrWhiteSpace(cst)) cst = Valor(grupo, "CSOSN");
            var aliq = ParseDecimal(Valor(grupo, "pICMS"), 0m);
            var valor = ParseDecimal(Valor(grupo, "vICMS"), 0m);
            var bc = ParseDecimal(Valor(grupo, "vBC"), 0m);
            return (string.IsNullOrWhiteSpace(cst) ? "00" : cst, aliq, valor, bc);
        }

        private async Task PersistirArtefatosAsync(DocumentoFiscal documento, string xml, CancellationToken ct)
        {
            try
            {
                var chaveLogica = string.IsNullOrWhiteSpace(documento.ChaveAcesso) ? documento.Id.ToString() : documento.ChaveAcesso;
                var xmlBytes = System.Text.Encoding.UTF8.GetBytes(xml);
                var xmlCaminho = await _armazenamento.SalvarAsync(chaveLogica, $"{chaveLogica}.xml", xmlBytes, "application/xml", ct);

                var pdfBytes = _danfeService.GerarPdf(documento);
                var pdfCaminho = await _armazenamento.SalvarAsync(chaveLogica, $"{chaveLogica}.pdf", pdfBytes, "application/pdf", ct);

                documento.DefinirCaminhosArquivos(xmlCaminho, pdfCaminho);
            }
            catch
            {
                // best-effort: DANFE pode ser regerado sob demanda.
            }
        }

        private static string Valor(XElement? parent, string localName)
            => parent?.Elements().FirstOrDefault(e => e.Name.LocalName == localName)?.Value?.Trim() ?? string.Empty;

        private static int ParseInt(string s, int fallback)
            => int.TryParse(s, NumberStyles.Integer, Inv, out var v) ? v : fallback;

        private static long ParseLong(string s, long fallback)
            => long.TryParse(s, NumberStyles.Integer, Inv, out var v) ? v : fallback;

        private static decimal ParseDecimal(string s, decimal fallback)
            => decimal.TryParse(s, NumberStyles.Any, Inv, out var v) ? v : fallback;
    }

    /// <summary>
    /// Registra o cancelamento de um documento a partir de um XML de evento externo
    /// (procEventoNFe/retEnvEvento). Vincula o evento ao documento pela chave e exige homologação
    /// (cStat 135/155). Fallback honesto: sem chave/documento/cStat válido, falha sem alterar estado.
    /// </summary>
    public class RegistrarCancelamentoPorXmlCommandHandler : ICommandHandler<RegistrarCancelamentoPorXmlCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarCancelamentoPorXmlCommandHandler(
            ContextFiscal context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarCancelamentoPorXmlCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (string.IsNullOrWhiteSpace(request.XmlEvento))
                return CommandResult.Falha("O XML do evento é obrigatório.");

            XDocument doc;
            try
            {
                doc = XDocument.Parse(request.XmlEvento);
            }
            catch (XmlException ex)
            {
                return CommandResult.Falha($"XML de evento inválido: {ex.Message}");
            }

            // Um procEventoNFe tem dois infEvento: o do <evento> (pedido, com tpEvento) e o do
            // <retEvento> (resposta, com cStat/nProt/xMotivo). Extraímos cada campo de onde ele existir,
            // varrendo o documento inteiro (namespace-agnóstico) e pegando a primeira ocorrência não vazia.
            var temInfEvento = doc.Descendants().Any(e => e.Name.LocalName == "infEvento");
            if (!temInfEvento)
                return CommandResult.Falha("XML não contém um evento (infEvento ausente).");

            var chave = new string(PrimeiroValor(doc, "chNFe").Where(char.IsDigit).ToArray());
            if (chave.Length != 44)
                return CommandResult.Falha("Não foi possível extrair a chave (chNFe) de 44 dígitos do evento.");

            var tpEvento = PrimeiroValor(doc, "tpEvento");
            var cStat = PrimeiroValor(doc, "cStat");
            var nProt = PrimeiroValor(doc, "nProt");
            var xMotivo = PrimeiroValor(doc, "xMotivo");

            // 110111 = Cancelamento. cStat 135/155 = homologado / homologado fora de prazo.
            var ehCancelamento = string.IsNullOrWhiteSpace(tpEvento) || tpEvento == "110111";
            if (!ehCancelamento)
                return CommandResult.Falha($"O evento (tpEvento={tpEvento}) não é um cancelamento.");
            if (cStat != "135" && cStat != "155")
                return CommandResult.Falha($"O evento de cancelamento não está homologado (cStat={cStat}). Não será registrado.");

            var documento = await _context.DocumentosFiscais.FirstOrDefaultAsync(d => d.ChaveAcesso == chave, cancellationToken);
            if (documento is null)
                return CommandResult.Falha($"Nenhum documento fiscal encontrado para a chave {chave}.");

            if (documento.Status == "Cancelada")
                return CommandResult.Falha("O documento já está cancelado.");

            // REG-CANC-014 (bloqueante): importação de XML de cancelamento sem documento AUTORIZADO
            // relacionado deve ser rejeitada. Não basta existir a chave — o documento tem de estar
            // autorizado para ser elegível ao cancelamento (não cancelar rascunho/rejeitado/pendente).
            if (documento.Status != "Autorizado")
                return CommandResult.Falha($"O documento da chave {chave} não está autorizado (status atual: {documento.Status}); cancelamento por XML rejeitado (REG-CANC-014).");

            var justificativaCanc = request.Justificativa ?? "Cancelamento registrado a partir de evento externo (XML).";
            var evento = new EventoDocumentoFiscal(
                documento.Id,
                "Cancelamento",
                int.TryParse(cStat, out var cs) ? cs : 135,
                string.IsNullOrWhiteSpace(xMotivo) ? "Cancelamento homologado (evento importado por XML)." : xMotivo,
                nProt,
                1,
                null, // xCorrecao (exclusivo da CC-e)
                request.XmlEvento,
                tenantId,
                usuario,
                justificativa: justificativaCanc);

            _context.EventosDocumentosFiscais.Add(evento);
            documento.Cancelar(request.Justificativa ?? "Cancelamento por evento externo (XML).", request.XmlEvento, usuario);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Cancelamento registrado a partir do XML de evento.", new
            {
                DocumentoFiscalId = documento.Id,
                ChaveAcesso = documento.ChaveAcesso,
                Protocolo = nProt,
                Status = documento.Status
            });
        }

        /// <summary>Primeira ocorrência não vazia de um elemento (por LocalName) em todo o documento.</summary>
        private static string PrimeiroValor(XDocument doc, string localName)
            => doc.Descendants()
                  .Where(e => e.Name.LocalName == localName)
                  .Select(e => e.Value?.Trim() ?? string.Empty)
                  .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? string.Empty;
    }
}
