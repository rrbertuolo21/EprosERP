using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Fiscal.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    /// <summary>
    /// Documento fiscal de DEVOLUÇÃO (NF-e de devolução, finalidade fnDevolucao=4). Fiel à estrutura
    /// <c>devolucaos</c> comprovada no material (EF_DEVOLUCAO_FISCAL): guarda estado, chave da NF de
    /// entrada (referência fiscal obrigatória), chave gerada e número gerado após autorização.
    ///
    /// Regras portadas (seção 9 da EF):
    /// - REG-DEV-001: exige referência fiscal (chave da NF de entrada) para criar/transmitir.
    /// - REG-DEV-003: inicia em NOVO. REG-DEV-004/005/006: APROVADO/REJEITADO/CANCELADO.
    /// - REG-DEV-012: aprovado não retransmite. REG-DEV-018: sem itens não transmite.
    /// - REG-DEV-007/008/009: preserva chave de entrada, chave gerada e número gerado.
    /// </summary>
    public class DevolucaoFiscal : EntidadeSaaSBase
    {
        public EEstadoDevolucaoFiscal Estado { get; private set; } = EEstadoDevolucaoFiscal.Novo;

        /// <summary>Modelo do documento gerado na devolução: "55" (NF-e) ou "65" (NFC-e). Devolução usa NF-e (55).</summary>
        public string Modelo { get; private set; } = "55";
        public int Ambiente { get; private set; } // 1=Produção, 2=Homologação
        public int Serie { get; private set; }

        /// <summary>Chave (44 dígitos) da NF de entrada/origem referenciada. Referência fiscal obrigatória.</summary>
        public string ChaveNfEntrada { get; private set; } = string.Empty;

        /// <summary>Chave gerada (44 dígitos) da devolução quando transmitida/autorizada.</summary>
        public string? ChaveGerada { get; private set; }

        /// <summary>Número fiscal gerado da devolução; participa da sequência fiscal compartilhada.</summary>
        public long? NumeroGerado { get; private set; }

        public string? Protocolo { get; private set; }

        /// <summary>Motivo/justificativa da devolução (informado na criação).</summary>
        public string Motivo { get; private set; } = string.Empty;

        /// <summary>Mensagem de retorno fiscal (rejeição/cancelamento/correção) quando houver.</summary>
        public string? MensagemRetorno { get; private set; }

        /// <summary>XML de entrada carregado para iniciar a devolução (quando origem por upload de XML).</summary>
        public string? XmlEntrada { get; private set; }
        public string? XmlRetorno { get; private set; }

        /// <summary>Empresa emitente (GestaoClientes). Resolve certificado/parâmetros DF-e na transmissão.</summary>
        public Guid? EmpresaId { get; private set; }

        /// <summary>Vínculo opcional com o documento fiscal de origem interno (quando a origem é uma saída própria).</summary>
        public Guid? DocumentoOrigemId { get; private set; }

        /// <summary>Destinatário da devolução (normalmente o fornecedor da NF de entrada).</summary>
        public string DestinatarioCnpjCpf { get; private set; } = string.Empty;
        public string DestinatarioNome { get; private set; } = string.Empty;

        public decimal Total { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public DateTime? DataTransmissao { get; private set; }

        public List<DevolucaoFiscalItem> Itens { get; private set; } = new();

        protected DevolucaoFiscal() { } // EF Core

        public DevolucaoFiscal(
            string modelo,
            int ambiente,
            int serie,
            string chaveNfEntrada,
            string motivo,
            string destinatarioCnpjCpf,
            string destinatarioNome,
            decimal total,
            Guid? empresaId,
            Guid? documentoOrigemId,
            string? xmlEntrada,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Modelo = string.IsNullOrWhiteSpace(modelo) ? "55" : modelo;
            Ambiente = ambiente;
            Serie = serie;
            ChaveNfEntrada = chaveNfEntrada?.Trim() ?? string.Empty;
            Motivo = motivo ?? string.Empty;
            DestinatarioCnpjCpf = destinatarioCnpjCpf ?? string.Empty;
            DestinatarioNome = destinatarioNome ?? string.Empty;
            Total = total;
            EmpresaId = empresaId;
            DocumentoOrigemId = documentoOrigemId;
            XmlEntrada = xmlEntrada;
            Estado = EEstadoDevolucaoFiscal.Novo; // REG-DEV-003
            DataCriacao = DateTime.UtcNow;
            Validar();
        }

        public void AdicionarItem(
            Guid? produtoId,
            string sku,
            string nomeProduto,
            string ncm,
            int cfop,
            string cst,
            decimal quantidade,
            decimal valorUnitario,
            decimal aliquotaIcms,
            string criadoPor)
        {
            var item = new DevolucaoFiscalItem(Id, produtoId, sku, nomeProduto, ncm, cfop, cst, quantidade, valorUnitario, aliquotaIcms, TenantId, criadoPor);
            if (!item.IsValid)
            {
                AddNotifications(item.Notifications);
                return;
            }

            Itens.Add(item);
        }

        /// <summary>Marca a transmissão realizada (para consistência de auditoria de data).</summary>
        public void RegistrarTransmissao()
        {
            DataTransmissao = DateTime.UtcNow;
        }

        /// <summary>REG-DEV-004/008/009: registra aprovação com chave gerada, número gerado e protocolo.</summary>
        public void Aprovar(string chaveGerada, long numeroGerado, string protocolo, string? xmlRetorno, string alteradoPor)
        {
            Estado = EEstadoDevolucaoFiscal.Aprovado;
            ChaveGerada = chaveGerada;
            NumeroGerado = numeroGerado;
            Protocolo = protocolo;
            XmlRetorno = xmlRetorno;
            MensagemRetorno = null;
            DataTransmissao = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>REG-DEV-005/014: registra rejeição (permite correção/retransmissão posterior).</summary>
        public void Rejeitar(string mensagemRetorno, string? xmlRetorno, string alteradoPor)
        {
            Estado = EEstadoDevolucaoFiscal.Rejeitado;
            MensagemRetorno = mensagemRetorno;
            XmlRetorno = xmlRetorno;
            DataTransmissao = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>REG-DEV-006/015/019: cancela preservando chaves de entrada/gerada rastreáveis.</summary>
        public void Cancelar(string mensagemRetorno, string? xmlRetorno, string alteradoPor)
        {
            Estado = EEstadoDevolucaoFiscal.Cancelado;
            MensagemRetorno = mensagemRetorno;
            if (!string.IsNullOrWhiteSpace(xmlRetorno))
                XmlRetorno = xmlRetorno;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>REG-DEV-016: registra correção mantendo a rastreabilidade do documento original.</summary>
        public void RegistrarCorrecao(string textoCorrecao, string? xmlRetorno, string alteradoPor)
        {
            MensagemRetorno = textoCorrecao;
            if (!string.IsNullOrWhiteSpace(xmlRetorno))
                XmlRetorno = xmlRetorno;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>REG-DEV-012/013/018: a devolução pode ser transmitida (NOVO ou REJEITADO, com itens e referência).</summary>
        public bool PodeTransmitir(out string motivoBloqueio)
        {
            if (Estado == EEstadoDevolucaoFiscal.Aprovado)
            {
                motivoBloqueio = "Devolução já aprovada não pode ser retransmitida pelo fluxo normal.";
                return false;
            }

            if (Estado == EEstadoDevolucaoFiscal.Cancelado)
            {
                motivoBloqueio = "Devolução cancelada não pode ser transmitida.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ChaveNfEntrada))
            {
                motivoBloqueio = "Referência fiscal da devolução não localizada.";
                return false;
            }

            if (!Itens.Any())
            {
                motivoBloqueio = "Devolução sem itens.";
                return false;
            }

            motivoBloqueio = string.Empty;
            return true;
        }

        public bool PodeCancelar() => Estado == EEstadoDevolucaoFiscal.Aprovado || Estado == EEstadoDevolucaoFiscal.Novo;

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<DevolucaoFiscal>()
                .Requires()
                .IsTrue(Modelo == "55" || Modelo == "65", nameof(Modelo), "O modelo deve ser 55 (NF-e) ou 65 (NFC-e) [Origem: DevolucaoFiscal]")
                .IsTrue(Ambiente == 1 || Ambiente == 2, nameof(Ambiente), "O ambiente deve ser 1 (Produção) ou 2 (Homologação) [Origem: DevolucaoFiscal]")
                .IsGreaterThan(Serie, 0, nameof(Serie), "A série deve ser maior que zero [Origem: DevolucaoFiscal]")
                // REG-DEV-001/007: devolução exige a chave da NF de entrada como referência fiscal.
                .IsNotNullOrEmpty(ChaveNfEntrada, nameof(ChaveNfEntrada), "A devolução exige a chave da NF de entrada (referência fiscal) [Origem: DevolucaoFiscal]")
                .IsNotNullOrEmpty(DestinatarioCnpjCpf, nameof(DestinatarioCnpjCpf), "O CPF/CNPJ do destinatário da devolução é obrigatório [Origem: DevolucaoFiscal]")
                .IsNotNullOrEmpty(DestinatarioNome, nameof(DestinatarioNome), "O nome do destinatário da devolução é obrigatório [Origem: DevolucaoFiscal]")
                .IsGreaterThan(Total, -0.01m, nameof(Total), "O total da devolução não pode ser negativo [Origem: DevolucaoFiscal]"));

            if (!string.IsNullOrWhiteSpace(ChaveNfEntrada) && ChaveNfEntrada.Length != 44)
                AddNotification(nameof(ChaveNfEntrada), "A chave da NF de entrada deve ter 44 dígitos [Origem: DevolucaoFiscal]");
        }
    }
}
