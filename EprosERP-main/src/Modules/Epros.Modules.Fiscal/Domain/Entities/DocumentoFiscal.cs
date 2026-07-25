using System;
using System.Collections.Generic;
using Epros.Modules.Fiscal.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class DocumentoFiscal : EntidadeSaaSBase
    {
        public string Modelo { get; private set; } = string.Empty; // "55" (NF-e) ou "65" (NFC-e)
        public string Status { get; private set; } = "Rascunho"; // Rascunho, Pendente, Autorizado, Rejeitado, Cancelado, PendenteContingencia
        public int Ambiente { get; private set; } // 1 = Produção, 2 = Homologação
        public string ChaveAcesso { get; private set; } = string.Empty;
        public string? Recibo { get; private set; }
        public string? Protocolo { get; private set; }
        public int Serie { get; private set; }
        public long Numero { get; private set; }
        public int? StatusSefaz { get; private set; }
        public string? MotivoRejeicaoSefaz { get; private set; }
        public decimal Total { get; private set; }
        public DateTime DataEmissao { get; private set; }
        public DateTime? DataAutorizacao { get; private set; }
        public DateTime? DataCancelamento { get; private set; }
        public string? XmlEnvio { get; private set; }
        public string? XmlRetorno { get; private set; }
        public string? PdfCaminho { get; private set; }
        public string? XmlCaminho { get; private set; }
        public string DestinatarioCnpjCpf { get; private set; } = string.Empty;
        public string DestinatarioNome { get; private set; } = string.Empty;
        public Guid? VendaOrigemId { get; private set; }

        /// <summary>
        /// Empresa emitente (GestaoClientes.Empresa) deste documento. Necessário para o
        /// <c>IEmitenteFiscalProvider</c> resolver certificado A1 + parâmetros DF-e (série/CSC/ambiente).
        /// Nullable porque documentos legados/rascunhos podem ter sido criados antes deste vínculo.
        /// </summary>
        public Guid? EmpresaId { get; private set; }

        // ---- Finalidade / referência (usado por devolução: finNFe=4 + refNFe) ----
        /// <summary>Finalidade da NF-e (tpNF/finNFe): 1=Normal, 2=Complementar, 3=Ajuste, 4=Devolução. Default 1.</summary>
        public int Finalidade { get; private set; } = 1;

        /// <summary>Chave (44 dígitos) da NF-e referenciada (refNFe), obrigatória quando Finalidade=4 (devolução).</summary>
        public string? ChaveReferenciada { get; private set; }

        // ---- Contingência (tpEmis) ----
        /// <summary>Tipo de emissão: Normal (online) ou uma das contingências (SVC-AN/SVC-RS/EPEC/Offline). Default Normal.</summary>
        public ETipoEmissaoFiscal TipoEmissao { get; private set; } = ETipoEmissaoFiscal.Normal;

        /// <summary>Justificativa da contingência (xJust) — obrigatória (15..256) quando em contingência.</summary>
        public string? JustificativaContingencia { get; private set; }

        /// <summary>Data/hora de entrada em contingência (dhCont).</summary>
        public DateTime? DataHoraContingencia { get; private set; }

        public List<DocumentoFiscalItem> Itens { get; private set; } = new();

        protected DocumentoFiscal() { } // EF Core

        public void VincularVendaOrigem(Guid vendaId)
        {
            VendaOrigemId = vendaId;
        }

        /// <summary>Define o documento como devolução (finNFe=4) referenciando a chave da NF de entrada/origem.</summary>
        public void DefinirDevolucao(string chaveReferenciada)
        {
            Finalidade = 4;
            ChaveReferenciada = chaveReferenciada;
        }

        /// <summary>
        /// Coloca o documento em contingência (tpEmis != Normal). Não pode ser Normal por aqui.
        /// Não altera o caminho normal: documentos sem esta chamada permanecem <see cref="ETipoEmissaoFiscal.Normal"/>.
        /// </summary>
        public void EntrarContingencia(ETipoEmissaoFiscal tipoEmissao, string justificativa)
        {
            if (tipoEmissao == ETipoEmissaoFiscal.Normal)
                return;

            TipoEmissao = tipoEmissao;
            JustificativaContingencia = justificativa;
            DataHoraContingencia = DateTime.UtcNow;
        }

        /// <summary>Retorna o documento à emissão normal (tpEmis=Normal), limpando os dados de contingência.</summary>
        public void RetornarEmissaoNormal()
        {
            TipoEmissao = ETipoEmissaoFiscal.Normal;
            JustificativaContingencia = null;
            DataHoraContingencia = null;
        }

        /// <summary>Marca o documento como emitido em contingência offline, aguardando transmissão (SEFAZ indisponível).</summary>
        public void MarcarPendenteContingencia()
        {
            Status = "PendenteContingencia";
        }

        /// <summary>True quando o documento está em algum modo de contingência (tpEmis != Normal).</summary>
        public bool EmContingencia => TipoEmissao != ETipoEmissaoFiscal.Normal;

        /// <summary>Vincula a empresa emitente (resolve certificado/params fiscais na transmissão).</summary>
        public void VincularEmpresaEmitente(Guid empresaId)
        {
            EmpresaId = empresaId;
        }

        public DocumentoFiscal(
            string modelo,
            int ambiente,
            int serie,
            long numero,
            decimal total,
            string destinatarioCnpjCpf,
            string destinatarioNome,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<DocumentoFiscal>()
                .Requires()
                .IsTrue(modelo == "55" || modelo == "65", nameof(Modelo), "Modelo deve ser 55 (NF-e) ou 65 (NFC-e).")
                .IsTrue(ambiente == 1 || ambiente == 2, nameof(Ambiente), "Ambiente deve ser 1 (Produção) ou 2 (Homologação).")
                .IsGreaterThan(serie, 0, nameof(Serie), "A série deve ser maior que zero.")
                .IsGreaterThan(numero, 0, nameof(Numero), "O número deve ser maior que zero.")
                .IsGreaterThan(total, -0.01m, nameof(Total), "O total do documento não pode ser negativo.")
                .IsNotNullOrEmpty(destinatarioCnpjCpf, nameof(DestinatarioCnpjCpf), "O CPF/CNPJ do destinatário é obrigatório.")
                .IsNotNullOrEmpty(destinatarioNome, nameof(DestinatarioNome), "O Nome do destinatário é obrigatório.")
            );

            Modelo = modelo;
            Ambiente = ambiente;
            Serie = serie;
            Numero = numero;
            Total = total;
            DestinatarioCnpjCpf = destinatarioCnpjCpf;
            DestinatarioNome = destinatarioNome;
            DataEmissao = DateTime.UtcNow;
            Status = "Rascunho";
        }

        public void AdicionarItem(string sku, string nomeProduto, string cst, int cfop, string ncm, decimal quantidade, decimal valorUnitario, decimal aliquotaIcms, string criadoPor)
        {
            var item = new DocumentoFiscalItem(Id, sku, nomeProduto, cst, cfop, ncm, quantidade, valorUnitario, aliquotaIcms, TenantId, criadoPor);
            if (!item.IsValid)
            {
                AddNotifications(item.Notifications);
                return;
            }

            Itens.Add(item);
        }

        public void Submeter()
        {
            Status = "Pendente";
        }

        public void Autorizar(string chaveAcesso, string protocolo, int statusSefaz, string xmlEnvio, string xmlRetorno, string pdfCaminho, string xmlCaminho)
        {
            Status = "Autorizado";
            ChaveAcesso = chaveAcesso;
            Protocolo = protocolo;
            StatusSefaz = statusSefaz;
            XmlEnvio = xmlEnvio;
            XmlRetorno = xmlRetorno;
            PdfCaminho = pdfCaminho;
            XmlCaminho = xmlCaminho;
            DataAutorizacao = DateTime.UtcNow;
            MotivoRejeicaoSefaz = null;
        }

        /// <summary>Grava os caminhos/URIs dos artefatos persistidos (XML autorizado e PDF do DANFE/cupom).</summary>
        public void DefinirCaminhosArquivos(string? xmlCaminho, string? pdfCaminho)
        {
            if (!string.IsNullOrWhiteSpace(xmlCaminho))
                XmlCaminho = xmlCaminho;
            if (!string.IsNullOrWhiteSpace(pdfCaminho))
                PdfCaminho = pdfCaminho;
        }

        public void Rejeitar(int statusSefaz, string motivo, string xmlRetorno)
        {
            Status = "Rejeitado";
            StatusSefaz = statusSefaz;
            MotivoRejeicaoSefaz = motivo;
            XmlRetorno = xmlRetorno;
        }

        public void Cancelar(string justificativa, string xmlRetorno, string usuario)
        {
            Status = "Cancelada";
            DataCancelamento = DateTime.UtcNow;
            XmlRetorno = xmlRetorno;
            MarcarAlterado(usuario);
        }
    }
}
