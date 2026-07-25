using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Cabeçalho funcional do documento de entrada usado pela logística
    /// (EF Logística de Entrada §15.3 `lde_documento_entrada`).
    /// O documento fiscal completo pertence ao domínio fiscal; aqui é usado como documento de entrada.
    /// LDE-013: emitente é obrigatório para confirmação (checado no handler de confirmação).
    /// </summary>
    public class LdeDocumentoEntrada : EntidadeSaaSBase
    {
        public string? ChaveAcesso { get; private set; }
        public string? Numero { get; private set; }
        public string? Serie { get; private set; }
        public DateTime? DataEmissao { get; private set; }
        public string? NaturezaOperacao { get; private set; }
        public decimal? ValorTotal { get; private set; }
        public Guid? FornecedorId { get; private set; }
        public Guid? DestinatarioId { get; private set; }
        public Guid? EmitenteId { get; private set; }
        public Guid? TransporteId { get; private set; }
        public Guid? FaturaId { get; private set; }
        public string? Situacao { get; private set; }

        // Navegação intra-módulo
        public ICollection<LdeDocumentoEntradaItem> Itens { get; private set; } = new List<LdeDocumentoEntradaItem>();
        public ICollection<LdeDocumentoEntradaDuplicata> Duplicatas { get; private set; } = new List<LdeDocumentoEntradaDuplicata>();

        protected LdeDocumentoEntrada() { } // EF Core

        public LdeDocumentoEntrada(
            string? chaveAcesso, string? numero, string? serie, DateTime? dataEmissao, string? naturezaOperacao,
            decimal? valorTotal, Guid? fornecedorId, Guid? destinatarioId, Guid? emitenteId,
            Guid? transporteId, Guid? faturaId, string? situacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ChaveAcesso = chaveAcesso;
            Numero = numero;
            Serie = serie;
            DataEmissao = dataEmissao;
            NaturezaOperacao = naturezaOperacao;
            ValorTotal = valorTotal;
            FornecedorId = fornecedorId;
            DestinatarioId = destinatarioId;
            EmitenteId = emitenteId;
            TransporteId = transporteId;
            FaturaId = faturaId;
            Situacao = situacao;
        }

        public void AdicionarItem(LdeDocumentoEntradaItem item) => Itens.Add(item);
        public void AdicionarDuplicata(LdeDocumentoEntradaDuplicata duplicata) => Duplicatas.Add(duplicata);

        public void Validar() { }
    }
}
