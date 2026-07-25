using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel do agregado raiz Venda. FK long -> Guid; herda EntidadeSaaSBase.
    /// Preserva o construtor/superfície usados pelos handlers já existentes (RegistrarVenda,
    /// CancelarVenda, Sincronizar) e adiciona a totalidade dos agregados filhos do legado.
    /// O documento fiscal transmitido em si (DocumentoFiscal) vive no módulo Fiscal e é
    /// referenciado por Guid FK (DocumentoFiscalId) — não escrevemos no Fiscal.
    /// </summary>
    public class Venda : EntidadeSaaSBase
    {
        public string CaixaId { get; private set; } = string.Empty;
        public decimal Total { get; private set; }

        // Situação fiscal da venda: enum tipado do legado (EVendaStatus). Restaurado de string para enum.
        public EVendaStatus Status { get; private set; } = EVendaStatus.Salvar;
        // Flag operacional de cancelamento do PDV (o legado cancela via NFC-e/NF-e e soft-delete; o enum
        // do legado não possui membro "Cancelada", então o cancelamento operacional é rastreado aqui).
        public bool Cancelada { get; private set; }

        // Campos do legado — enums fiscais tipados (restaurados de int? na migração).
        // Armazenados como int pela coluna (EF: HasConversion<int>), valores 1:1 com o legado.
        public EModeloDocumento? ModeloFiscal { get; private set; }
        public string NaturezaOperacao { get; private set; } = "01";
        public DateTime DataVenda { get; private set; }
        public string? InformacoesComplementares { get; private set; }
        public string? InformacoesAdicionaisFisco { get; private set; }
        public EModalidadeFrete? ModalidadeFrete { get; private set; }
        public EVendaOrigem? VendaOrigem { get; private set; }
        public bool IncluirFreteNoTotal { get; private set; }
        public Guid? ClienteId { get; private set; }
        public decimal ValorDesconto { get; private set; }
        public decimal ValorFrete { get; private set; }
        public string? FormaPagamento { get; private set; }
        public string? CaminhoPdfCupomNaoFiscal { get; private set; }
        public long? NumeroCupomNaoFiscal { get; private set; }
        public DateTime DataUltimoProcessamento { get; private set; }

        /// <summary>FK para o documento fiscal transmitido (módulo Fiscal). Sem navegação cruzada.</summary>
        public Guid? DocumentoFiscalId { get; private set; }

        // Itens
        private readonly List<VendaItem> _itens = new();
        public IReadOnlyCollection<VendaItem> Itens => _itens.AsReadOnly();

        // Pagamentos
        private readonly List<VendaPagamento> _pagamentos = new();
        public IReadOnlyCollection<VendaPagamento> Pagamentos => _pagamentos.AsReadOnly();

        // Autorizações XML
        private readonly List<VendaAutorizacaoXml> _autorizacoesXml = new();
        public IReadOnlyCollection<VendaAutorizacaoXml> AutorizacoesXml => _autorizacoesXml.AsReadOnly();

        // NF-e referenciadas
        private readonly List<VendaNfeReferenciada> _referenciadas = new();
        public IReadOnlyCollection<VendaNfeReferenciada> Referenciadas => _referenciadas.AsReadOnly();

        // Histórico de retornos SEFAZ
        private readonly List<VendaNfHistorico> _nfHistoricos = new();
        public IReadOnlyCollection<VendaNfHistorico> NfHistoricos => _nfHistoricos.AsReadOnly();

        // Agregados filhos 1:1
        public VendaEmitente? Emitente { get; private set; }
        public VendaDestinatario? Destinatario { get; private set; }
        public VendaTransporte? Transporte { get; private set; }
        public VendaTotal? Total_ { get; private set; }
        public VendaConfiguracao? Configuracao { get; private set; }
        public VendaNfce? Nfce { get; private set; }
        public VendaNfe? Nfe { get; private set; }
        public VendaFatura? Fatura { get; private set; }
        public VendaImposto? Imposto { get; private set; }
        public VendaTotalIbsCbs? TotalIbsCbs { get; private set; }
        public VendaEntrega? VendaEntrega { get; private set; }
        public VendaCobrancaEndereco? VendaCobrancaEndereco { get; private set; }

        protected Venda() { } // EF Core

        /// <summary>Construtor compatível com os handlers já existentes.</summary>
        public Venda(
            Guid id,
            Guid syncId,
            string caixaId,
            decimal total,
            EVendaStatus status,
            string tenantId,
            string criadoPor,
            DateTime criadoEm,
            EModeloDocumento? modeloFiscal = null,
            string? naturezaOperacao = null,
            string? informacoesComplementares = null,
            string? informacoesAdicionaisFisco = null,
            EModalidadeFrete? modalidadeFrete = null,
            EVendaOrigem? vendaOrigem = null,
            bool incluirFreteNoTotal = false,
            Guid? clienteId = null,
            decimal valorDesconto = 0m,
            decimal valorFrete = 0m,
            string? formaPagamento = null)
            : base(id, syncId, tenantId, criadoPor, criadoEm)
        {
            AddNotifications(new Contract<Venda>()
                .Requires()
                .IsNotNullOrEmpty(caixaId, nameof(CaixaId), "O Caixa da venda é obrigatório.")
                .IsGreaterThan(total, -0.01m, nameof(Total), "O Total da venda não pode ser negativo.")
            );

            CaixaId = caixaId;
            Total = total;
            Status = status;

            ModeloFiscal = modeloFiscal;
            NaturezaOperacao = naturezaOperacao ?? "01";
            DataVenda = criadoEm;
            DataUltimoProcessamento = criadoEm;
            InformacoesComplementares = informacoesComplementares;
            InformacoesAdicionaisFisco = informacoesAdicionaisFisco;
            ModalidadeFrete = modalidadeFrete;
            VendaOrigem = vendaOrigem;
            IncluirFreteNoTotal = incluirFreteNoTotal;
            ClienteId = clienteId;
            ValorDesconto = valorDesconto;
            ValorFrete = valorFrete;
            FormaPagamento = formaPagamento;
        }

        // ---------------------------------------------------------------------
        // Itens
        // ---------------------------------------------------------------------
        public void AdicionarItem(VendaItem item)
        {
            if (item == null)
            {
                AddNotification(nameof(Itens), "O item não pode ser nulo.");
                return;
            }
            _itens.Add(item);
            AddNotifications(item.Notifications);
        }

        /// <summary>Porte fiel de Venda.DeletarItem.</summary>
        public void DeletarItem(Guid itemId, string userId)
        {
            var localizado = _itens.FirstOrDefault(x => x.Id == itemId);
            localizado?.Deletar(userId);
        }

        // ---------------------------------------------------------------------
        // Status / cancelamento
        // ---------------------------------------------------------------------
        public void Cancelar(string alteradoPor)
        {
            if (Cancelada)
            {
                AddNotification(nameof(Cancelada), "A venda já está cancelada.");
                return;
            }
            Cancelada = true;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Porte fiel de Venda.AtualizarStatus(): atualiza a situação fiscal da venda.</summary>
        public void AtualizarStatus(EVendaStatus status, string alteradoPor)
        {
            Status = status;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Porte fiel de Venda.RedefinirIncluirFreteNoTotal.</summary>
        public void RedefinirIncluirFreteNoTotal(bool incluirFreteNoTotal) => IncluirFreteNoTotal = incluirFreteNoTotal;

        /// <summary>Porte fiel de Venda.RedefinirNaturezaOperacao (trunca em 60).</summary>
        public void RedefinirNaturezaOperacao(string naturezaOperacao)
            => NaturezaOperacao = (naturezaOperacao ?? "").Length > 60 ? naturezaOperacao![..60] : (naturezaOperacao ?? "");

        /// <summary>Porte fiel de Venda.AtualizarDataUltimoProcessamento.</summary>
        public void AtualizarDataUltimoProcessamento() => DataUltimoProcessamento = DateTime.UtcNow;

        /// <summary>Porte fiel de Venda.Alterar (dados cabeçalho).</summary>
        public void Alterar(EModeloDocumento? modeloFiscal, string naturezaOperacao, DateTime dataVenda, string? informacoesComplementares, string? informacoesAdicionaisFisco, EModalidadeFrete? modalidadeFrete, decimal valorDesconto, decimal valorFrete, string alteradoPor)
        {
            ModeloFiscal = modeloFiscal;
            NaturezaOperacao = naturezaOperacao;
            DataVenda = dataVenda;
            InformacoesComplementares = informacoesComplementares;
            InformacoesAdicionaisFisco = informacoesAdicionaisFisco;
            ModalidadeFrete = modalidadeFrete;
            ValorDesconto = valorDesconto;
            ValorFrete = valorFrete;
            MarcarAlterado(alteradoPor);
        }

        // ---------------------------------------------------------------------
        // Agregados filhos (associação/atualização)
        // ---------------------------------------------------------------------
        public void DefinirEmitente(VendaEmitente emitente)
        {
            Emitente = emitente;
            AddNotifications(emitente.Notifications);
        }

        public void DefinirDestinatario(VendaDestinatario destinatario)
        {
            Destinatario = destinatario;
            AddNotifications(destinatario.Notifications);
        }

        public void DefinirTransporte(VendaTransporte transporte)
        {
            Transporte = transporte;
            AddNotifications(transporte.Notifications);
        }

        public void DefinirTotal(VendaTotal total)
        {
            Total_ = total;
            AddNotifications(total.Notifications);
        }

        public void DefinirTotalIbsCbs(VendaTotalIbsCbs totalIbsCbs)
        {
            TotalIbsCbs = totalIbsCbs;
            AddNotifications(totalIbsCbs.Notifications);
        }

        public void DefinirConfiguracao(VendaConfiguracao configuracao)
        {
            Configuracao = configuracao;
            AddNotifications(configuracao.Notifications);
        }

        public void DefinirImposto(VendaImposto imposto)
        {
            Imposto = imposto;
            AddNotifications(imposto.Notifications);
        }

        public void DefinirFatura(VendaFatura fatura)
        {
            Fatura = fatura;
            AddNotifications(fatura.Notifications);
        }

        public void DeletarFatura(string userId) => Fatura?.DeletarEmCascata(userId);

        public void DefinirNfce(VendaNfce nfce) => Nfce = nfce;
        public void DefinirNfe(VendaNfe nfe) => Nfe = nfe;
        public void DefinirVendaEntrega(VendaEntrega entrega)
        {
            VendaEntrega = entrega;
            AddNotifications(entrega.Notifications);
        }
        public void DefinirVendaCobrancaEndereco(VendaCobrancaEndereco cobranca)
        {
            VendaCobrancaEndereco = cobranca;
            AddNotifications(cobranca.Notifications);
        }

        // ---------------------------------------------------------------------
        // Pagamentos
        // ---------------------------------------------------------------------
        public void AdicionarPagamento(VendaPagamento pagamento)
        {
            AddNotifications(pagamento.Notifications);
            if (pagamento.IsValid)
                _pagamentos.Add(pagamento);
        }

        public void DeletarPagamento(Guid pagamentoId, string userId)
        {
            var localizado = _pagamentos.FirstOrDefault(x => x.Id == pagamentoId);
            localizado?.Deletar(userId);
        }

        /// <summary>Porte fiel de Venda.ObterSomaDosPagamentos.</summary>
        public decimal ObterSomaDosPagamentos()
        {
            if (_pagamentos.Count == 0) return decimal.Zero;
            return _pagamentos.Sum(x => x.ValorPagamento - x.ValorTroco);
        }

        // ---------------------------------------------------------------------
        // Autorizações XML / referenciadas / histórico
        // ---------------------------------------------------------------------
        public void AdicionarAutorizacaoXml(VendaAutorizacaoXml autorizacao)
        {
            AddNotifications(autorizacao.Notifications);
            if (autorizacao.IsValid)
                _autorizacoesXml.Add(autorizacao);
        }

        public void AdicionarNfeReferenciada(string chaveAcesso, string criadoPor)
            => _referenciadas.Add(new VendaNfeReferenciada(Id, chaveAcesso, TenantId, criadoPor));

        public void AdicionarNfHistorico(string descricao, string criadoPor)
            => _nfHistoricos.Add(new VendaNfHistorico(Id, descricao, TenantId, criadoPor));

        /// <summary>Porte fiel de Venda.GerarNumeroCupomNaoFiscal.</summary>
        public void GerarNumeroCupomNaoFiscal()
        {
            var idVenda = Id.ToString("N").PadLeft(12, '0');
            string idFinal = idVenda[^12..];
            var numero = DataVenda.ToString("ddMMyy") + new string(idFinal.Where(char.IsDigit).ToArray()).PadLeft(12, '0')[^12..];
            NumeroCupomNaoFiscal = long.TryParse(numero, out var n) ? n : null;
        }

        public void IncluirCaminhoPdfCupomNaoFiscal(string caminho) => CaminhoPdfCupomNaoFiscal = caminho;

        /// <summary>Vincula o documento fiscal transmitido (módulo Fiscal) por Guid FK.</summary>
        public void VincularDocumentoFiscal(Guid documentoFiscalId) => DocumentoFiscalId = documentoFiscalId;

        // ---------------------------------------------------------------------
        // Duplicação (porte fiel de Venda.DuplicarVenda)
        // ---------------------------------------------------------------------
        /// <summary>
        /// Porte fiel de Venda.DuplicarVenda: cria uma nova venda (novo Id/SyncId) em rascunho
        /// (Status = Salvar), clonando emitente, destinatário, transporte, totais, configuração,
        /// fatura, imposto, itens, pagamentos, autorizações e NF-e referenciadas.
        /// NFC-e/NF-e NÃO são duplicadas (a venda nova fica sem documento até nova emissão) —
        /// idêntico ao legado, que reinicia venda_nfce/venda_nfe.
        /// </summary>
        public Venda DuplicarVenda(string criadoPor)
        {
            var clone = new Venda(
                id: Guid.NewGuid(),
                syncId: Guid.NewGuid(),
                caixaId: CaixaId,
                total: Total,
                status: EVendaStatus.Salvar,
                tenantId: TenantId,
                criadoPor: criadoPor,
                criadoEm: DateTime.UtcNow,
                modeloFiscal: ModeloFiscal,
                naturezaOperacao: NaturezaOperacao,
                informacoesComplementares: InformacoesComplementares,
                informacoesAdicionaisFisco: InformacoesAdicionaisFisco,
                modalidadeFrete: ModalidadeFrete,
                vendaOrigem: VendaOrigem,
                incluirFreteNoTotal: IncluirFreteNoTotal,
                clienteId: ClienteId,
                valorDesconto: ValorDesconto,
                valorFrete: ValorFrete,
                formaPagamento: FormaPagamento);

            if (Emitente is not null) clone.Emitente = Emitente.Duplicar(clone.Id, criadoPor);
            if (Destinatario is not null) clone.Destinatario = Destinatario.Duplicar(clone.Id, criadoPor);
            if (Transporte is not null) clone.Transporte = Transporte.Duplicar(clone.Id, criadoPor);
            if (Total_ is not null) clone.Total_ = Total_.Duplicar(clone.Id, criadoPor);
            if (Configuracao is not null) clone.Configuracao = Configuracao.Duplicar(clone.Id, criadoPor);
            if (Fatura is not null) clone.Fatura = Fatura.Duplicar(clone.Id, criadoPor);
            if (Imposto is not null) clone.Imposto = Imposto.Duplicar(clone.Id, criadoPor);
            if (TotalIbsCbs is not null) clone.TotalIbsCbs = TotalIbsCbs.Duplicar(clone.Id, criadoPor);

            foreach (var item in _itens) clone._itens.Add(item.Duplicar(clone.Id, criadoPor));
            foreach (var pg in _pagamentos) clone._pagamentos.Add(pg.Duplicar(clone.Id, criadoPor));
            foreach (var aut in _autorizacoesXml) clone._autorizacoesXml.Add(aut.Duplicar(clone.Id, criadoPor));
            foreach (var refr in _referenciadas) clone._referenciadas.Add(refr.Duplicar(clone.Id, criadoPor));

            return clone;
        }

        // ---------------------------------------------------------------------
        // Soft delete em cascata (porte fiel de Venda.Deletar)
        // ---------------------------------------------------------------------
        public void DeletarEmCascata(string userId)
        {
            Deletar(userId);
            foreach (var item in _itens) item.Deletar(userId);
            Fatura?.DeletarEmCascata(userId);
            foreach (var pg in _pagamentos) pg.Deletar(userId);
            Emitente?.Deletar(userId);
            Destinatario?.Deletar(userId);
            Transporte?.DeletarEmCascata(userId);
            Total_?.Deletar(userId);
            Configuracao?.Deletar(userId);
            Nfce?.Deletar(userId);
        }
    }
}
