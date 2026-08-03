using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    public class Compra : EntidadeSaaSBase
    {
        // Campos de compatibilidade (cabeçalho simplificado usado pelos handlers do módulo)
        public string FornecedorCnpj { get; private set; } = string.Empty;
        public string FornecedorNome { get; private set; } = string.Empty;
        public string NumeroNota { get; private set; } = string.Empty;
        public string ChaveAcesso { get; private set; } = string.Empty;
        public decimal ValorTotal { get; private set; }
        public DateTime DataEmissao { get; private set; }

        // Situação da compra: enum tipado do legado (EVendaStatus). Restaurado de string para enum.
        public EVendaStatus Status { get; private set; } = EVendaStatus.Salvar;
        // Flag operacional de cancelamento usada pelo fluxo de estorno do módulo (o legado cancela via CompraNfe).
        public bool Cancelada { get; private set; }

        // Campos Expandidos do Legado (enums tipados restaurados)
        public EModeloDocumento? ModeloFiscal { get; private set; }
        public string NaturezaOperacao { get; private set; } = "01";
        public DateTime DataCompra { get; private set; }
        public string? InformacoesComplementares { get; private set; }
        public string? InformacoesAdicionaisFisco { get; private set; }
        public EModalidadeFrete? ModalidadeFrete { get; private set; }
        public EVendaOrigem? CompraOrigem { get; private set; }
        public ETipoNFeCompra? TipoNota { get; private set; }
        public string? FormaPagamento { get; private set; }

        // ===== Comércio Exterior / Importação (CD1 / EF COMERCIO_EXTERIOR §5.3, CEX-005) =====
        // Dados factuais da operação de importação. Não alteram cálculo fiscal (valida-contador):
        // incoterm, moeda estrangeira e taxa de câmbio da compra. Default: NaoInformado/null (nacional).
        public EIncotermCompra Incoterm { get; private set; } = EIncotermCompra.NaoInformado;
        public string? Moeda { get; private set; }
        public decimal? TaxaCambio { get; private set; }

        public List<CompraItem> Itens { get; private set; } = new();

        // ================= Navegações do agregado (porte fiel do legado) =================
        // 1:1 — cada compra tem no máximo um emitente/destinatário/transporte/nfe/total/imposto/fatura.
        public CompraEmitente? Emitente { get; private set; }
        public CompraDestinatario? Destinatario { get; private set; }
        public CompraTransporte? Transporte { get; private set; }
        public CompraNfe? Nfe { get; private set; }
        public CompraTotal? Total { get; private set; }
        public CompraImposto? Imposto { get; private set; }
        public CompraFatura? Fatura { get; private set; }

        // 1:N — pagamentos da compra.
        public ICollection<CompraPagamento> Pagamentos { get; private set; } = new List<CompraPagamento>();

        protected Compra() { } // EF Core

        public Compra(
            string fornecedorCnpj,
            string fornecedorNome,
            string numeroNota,
            string chaveAcesso,
            decimal valorTotal,
            DateTime dataEmissao,
            string tenantId,
            string criadoPor,
            EModeloDocumento? modeloFiscal = null,
            string? naturezaOperacao = null,
            string? informacoesComplementares = null,
            string? informacoesAdicionaisFisco = null,
            EModalidadeFrete? modalidadeFrete = null,
            EVendaOrigem? compraOrigem = null,
            ETipoNFeCompra? tipoNota = null,
            string? formaPagamento = null)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Compra>()
                .Requires()
                .IsNotNullOrEmpty(fornecedorCnpj, nameof(FornecedorCnpj), "O CNPJ do fornecedor é obrigatório.")
                .IsNotNullOrEmpty(fornecedorNome, nameof(FornecedorNome), "O Nome do fornecedor é obrigatório.")
                .IsNotNullOrEmpty(numeroNota, nameof(NumeroNota), "O Número da nota é obrigatório.")
                .IsTrue(chaveAcesso != null && chaveAcesso.Length == 44, nameof(ChaveAcesso), "A Chave de Acesso da NF-e deve ter exatamente 44 dígitos.")
                .IsGreaterThan(valorTotal, -0.01m, nameof(ValorTotal), "O Valor Total da nota não pode ser negativo.")
            );

            FornecedorCnpj = fornecedorCnpj ?? string.Empty;
            FornecedorNome = fornecedorNome ?? string.Empty;
            NumeroNota = numeroNota ?? string.Empty;
            ChaveAcesso = chaveAcesso ?? string.Empty;
            ValorTotal = valorTotal;
            DataEmissao = dataEmissao;
            Status = EVendaStatus.Transmitido; // compra lançada/efetivada

            ModeloFiscal = modeloFiscal;
            NaturezaOperacao = naturezaOperacao ?? "01";
            DataCompra = dataEmissao;
            InformacoesComplementares = informacoesComplementares;
            InformacoesAdicionaisFisco = informacoesAdicionaisFisco;
            ModalidadeFrete = modalidadeFrete;
            CompraOrigem = compraOrigem;
            TipoNota = tipoNota;
            FormaPagamento = formaPagamento;
        }

        public void AdicionarItem(
            Guid produtoId,
            decimal quantidade,
            decimal precoUnitario,
            decimal valorIms,
            decimal valorIpi,
            string criadoPor,
            string? codigoProduto = null,
            string? codigoEan = null,
            string? descricaoProduto = null,
            string? ncm = null,
            Guid? cestId = null,
            string? cest = null,
            Guid? codigoAnpId = null,
            string? codigoAnp = null,
            int cfop = 0,
            string? unidadeComercial = null,
            decimal valorDesconto = 0m,
            decimal valorFreteRateado = 0m,
            decimal valorCusto = 0m)
        {
            var item = new CompraItem(
                Id,
                produtoId,
                quantidade,
                precoUnitario,
                valorIms,
                valorIpi,
                TenantId,
                criadoPor,
                codigoProduto,
                codigoEan,
                descricaoProduto,
                ncm,
                cestId,
                cest,
                codigoAnpId,
                codigoAnp,
                cfop,
                unidadeComercial,
                valorDesconto,
                valorFreteRateado,
                valorCusto
            );

            if (!item.IsValid)
            {
                AddNotifications(item.Notifications);
                return;
            }

            Itens.Add(item);
        }

        // ================= Composição do agregado (Definir/Adicionar sub-entidades) =================
        // Cada método recebe a sub-entidade já construída (com CompraId = this.Id), valida e anexa.
        // Notificações inválidas da sub-entidade são propagadas para o agregado raiz.

        private bool PropagarSeInvalido(EntidadeSaaSBase entidade)
        {
            if (entidade == null)
            {
                AddNotification("Compra", "Sub-entidade da compra não pode ser nula.");
                return true;
            }
            if (!entidade.IsValid)
            {
                AddNotifications(entidade.Notifications);
                return true;
            }
            return false;
        }

        public void DefinirEmitente(CompraEmitente emitente)
        {
            if (PropagarSeInvalido(emitente)) return;
            Emitente = emitente;
        }

        public void DefinirDestinatario(CompraDestinatario destinatario)
        {
            if (PropagarSeInvalido(destinatario)) return;
            Destinatario = destinatario;
        }

        public void DefinirTransporte(CompraTransporte transporte)
        {
            if (PropagarSeInvalido(transporte)) return;
            Transporte = transporte;
        }

        public void DefinirNfe(CompraNfe nfe)
        {
            if (PropagarSeInvalido(nfe)) return;
            Nfe = nfe;
        }

        public void DefinirTotal(CompraTotal total)
        {
            if (PropagarSeInvalido(total)) return;
            Total = total;
        }

        public void DefinirImposto(CompraImposto imposto)
        {
            if (PropagarSeInvalido(imposto)) return;
            Imposto = imposto;
        }

        public void DefinirFatura(CompraFatura fatura)
        {
            if (PropagarSeInvalido(fatura)) return;
            Fatura = fatura;
        }

        public void AdicionarPagamento(CompraPagamento pagamento)
        {
            if (PropagarSeInvalido(pagamento)) return;
            Pagamentos.Add(pagamento);
        }

        /// <summary>Valida as invariantes do agregado completo (raiz + sub-entidades já anexadas).</summary>
        public bool AgregadoValido()
        {
            foreach (var item in Itens.Where(i => !i.IsValid))
                AddNotifications(item.Notifications);
            foreach (var pag in Pagamentos.Where(p => !p.IsValid))
                AddNotifications(pag.Notifications);
            return IsValid;
        }

        /// <summary>Porte fiel de Compra.AtualizarStatus(): atualiza a situação fiscal da compra.</summary>
        public void AtualizarStatus(EVendaStatus status, string usuario)
        {
            Status = status;
            MarcarAlterado(usuario);
        }

        public void Cancelar(string usuario)
        {
            Cancelada = true;
            MarcarAlterado(usuario);
        }

        /// <summary>
        /// Define os dados de comércio exterior da compra (CD1 / CEX-005): incoterm, moeda estrangeira e
        /// taxa de câmbio. Dados factuais — não disparam cálculo fiscal (valida-contador). Câmbio, quando
        /// informado, deve ser positivo.
        /// </summary>
        public bool DefinirComercioExterior(EIncotermCompra incoterm, string? moeda, decimal? taxaCambio, string usuario)
        {
            if (taxaCambio.HasValue && taxaCambio.Value <= 0)
            {
                AddNotification("TaxaCambio", "A taxa de câmbio, quando informada, deve ser maior que zero [CEX-005] [Origem: Compra]");
                return false;
            }
            Incoterm = incoterm;
            Moeda = string.IsNullOrWhiteSpace(moeda) ? null : moeda.Trim().ToUpperInvariant();
            TaxaCambio = taxaCambio;
            MarcarAlterado(usuario);
            return true;
        }
    }
}
