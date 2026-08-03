using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    // ============================================================================
    // Submódulo Faturamento Comercial Internacional (VEN-FCI).
    // Fonte: EF_7_VENDAS_FATURAMENTO_COMERCIAL_INTERNACIONAL_V1. Regras FCI-001..064.
    // FCI-001: documento COMERCIAL internacional, NÃO documento fiscal brasileiro (não é NF-e).
    // NF-08: fatura = saída / nota de crédito = entrada (sinais OPOSTOS); câmbio pela data do documento.
    // valida-contador (FCI-038): contas fixas de contas-a-receber/imposto/desconto pendentes de Siser.
    // INV-08 (pendente Siser): ano financeiro default 1; tipos de documento 8/9/10.
    // ============================================================================

    /// <summary>
    /// Documento comercial internacional (fci_documento_comercial) — consolida fatura (9),
    /// nota de crédito (10) e cotação (8). Fonte: EF §10.1. Agregado raiz do submódulo.
    /// </summary>
    public class FciDocumentoComercial : EntidadeSaaSBase
    {
        private readonly List<FciDocumentoItem> _itens = new();
        public IReadOnlyCollection<FciDocumentoItem> Itens => _itens.AsReadOnly();

        public EFciTipoDocumento TipoDocumento { get; private set; }
        public int Serial { get; private set; }
        public string Numero { get; private set; } = string.Empty;
        public DateTime DataDocumento { get; private set; }
        public Guid ClienteId { get; private set; }
        public Guid? DocumentoOrigemId { get; private set; }
        public Guid ArmazemId { get; private set; }
        /// <summary>Ano financeiro. INV-08: default observado 1 (pendente Siser).</summary>
        public int AnoFinanceiroId { get; private set; } = 1;

        // --- Campos comerciais internacionais (o que diferencia de uma NF-e BR). ---
        /// <summary>Moeda do documento (ISO 4217). valida-Siser: moeda operacional padrão.</summary>
        public string? Moeda { get; private set; }
        /// <summary>Taxa de câmbio na data do documento (NF-08). Integra FIN-CAM quando disponível.</summary>
        public decimal? TaxaCambio { get; private set; }
        /// <summary>Incoterm comercial (EXW/FOB/CIF...). Campo comercial internacional (FCI-059).</summary>
        public string? Incoterm { get; private set; }

        public Guid? ImpostoId { get; private set; }
        public decimal? AliquotaImposto { get; private set; }
        public decimal TotalImposto { get; private set; }
        public decimal? PercentualDesconto { get; private set; }
        public EFciTipoDesconto TipoDesconto { get; private set; } = EFciTipoDesconto.Percentual;
        public decimal DescontoDocumento { get; private set; }
        public decimal ValorFrete { get; private set; }
        public decimal TotalBruto { get; private set; }
        public decimal TotalLiquido { get; private set; }
        public decimal TotalGeral { get; private set; }
        public decimal ValorPago { get; private set; }
        public decimal SaldoAnterior { get; private set; }
        public decimal SaldoEmAberto { get; private set; }
        public string? Referencia { get; private set; }
        public string? Observacao { get; private set; }
        public EFciStatus Status { get; private set; } = EFciStatus.Rascunho;
        public Guid? UsuarioId { get; private set; }

        public bool EhFatura => TipoDocumento == EFciTipoDocumento.Fatura;
        public bool EhNotaCredito => TipoDocumento == EFciTipoDocumento.NotaCredito;

        protected FciDocumentoComercial() { }

        public FciDocumentoComercial(
            EFciTipoDocumento tipoDocumento, int serial, string numero, DateTime dataDocumento,
            Guid clienteId, Guid armazemId, int anoFinanceiroId, Guid? documentoOrigemId,
            decimal percentualDesconto, EFciTipoDesconto tipoDesconto, decimal valorFrete,
            string? moeda, decimal? taxaCambio, string? incoterm, string? referencia, string? observacao,
            Guid? usuarioId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            TipoDocumento = tipoDocumento;
            Serial = serial;
            Numero = numero ?? string.Empty;
            DataDocumento = dataDocumento;
            ClienteId = clienteId;
            ArmazemId = armazemId;
            AnoFinanceiroId = anoFinanceiroId <= 0 ? 1 : anoFinanceiroId; // INV-08 default seguro.
            DocumentoOrigemId = documentoOrigemId;
            PercentualDesconto = percentualDesconto;
            TipoDesconto = tipoDesconto;
            ValorFrete = valorFrete;
            Moeda = moeda;
            TaxaCambio = taxaCambio;
            Incoterm = incoterm;
            Referencia = referencia;
            Observacao = observacao;
            UsuarioId = usuarioId;
            Status = EFciStatus.Rascunho;
        }

        public void AdicionarItem(FciDocumentoItem item)
        {
            _itens.Add(item);
        }

        /// <summary>Altera o cabeçalho na edição (FCI-039..043). Não muda tipo/serial/número/cliente.</summary>
        public void AlterarCabecalho(DateTime dataDocumento, decimal percentualDesconto, EFciTipoDesconto tipoDesconto,
            decimal valorFrete, string? moeda, decimal? taxaCambio, string? incoterm, string? referencia,
            string? observacao, string alteradoPor)
        {
            DataDocumento = dataDocumento;
            PercentualDesconto = percentualDesconto;
            TipoDesconto = tipoDesconto;
            ValorFrete = valorFrete;
            Moeda = moeda;
            TaxaCambio = taxaCambio;
            Incoterm = incoterm;
            Referencia = referencia;
            Observacao = observacao;
            MarcarAlterado(alteradoPor);
        }

        public void LimparItens() => _itens.Clear();

        /// <summary>
        /// Recalcula totais do cabeçalho a partir das linhas (EF §11 análogo).
        /// TotalBruto = Σ valor_bruto; TotalImposto = Σ valor_imposto; DescontoDocumento por tipo;
        /// TotalLiquido = bruto - desconto; TotalGeral = líquido + imposto + frete.
        /// </summary>
        public void Recalcular()
        {
            TotalBruto = _itens.Sum(i => i.ValorBruto);
            TotalImposto = _itens.Sum(i => i.ValorImposto ?? 0m);

            DescontoDocumento = TipoDesconto == EFciTipoDesconto.Percentual
                ? Math.Round(TotalBruto * ((PercentualDesconto ?? 0m) / 100m), 2)
                : (PercentualDesconto ?? 0m); // no modo Valor, o campo carrega o valor absoluto.

            TotalLiquido = TotalBruto - DescontoDocumento;
            TotalGeral = TotalLiquido + TotalImposto + ValorFrete;
            AtualizarSaldo();
        }

        private void AtualizarSaldo()
        {
            SaldoEmAberto = SaldoAnterior + TotalGeral - ValorPago;
        }

        public void RegistrarPagamento(decimal valorPago, string alteradoPor)
        {
            ValorPago = valorPago;
            AtualizarSaldo();
            // FCI-044: nota de crédito com saldo zero vira Paga; caso contrário permanece Aprovada.
            if (Status == EFciStatus.Aprovada && SaldoEmAberto <= 0m)
                Status = EFciStatus.Paga;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Valida FCI-017..021 antes de salvar. Rascunho e Aprovada exigem os mesmos mínimos.</summary>
        public void Validar()
        {
            Clear();
            var c = new Contract<FciDocumentoComercial>()
                .Requires()
                .AreNotEquals(ClienteId, Guid.Empty, nameof(ClienteId), "Documento sem cliente deve ser rejeitado. [FCI-019]")
                .IsNotNullOrEmpty(Numero, nameof(Numero), "Documento sem número deve ser rejeitado. [FCI-020]")
                .IsGreaterThan(_itens.Count, 0, nameof(Itens), "Documento sem itens deve ser rejeitado. [FCI-017/018]")
                .IsGreaterThan(TotalGeral, 0m, nameof(TotalGeral), "Documento com total geral igual a zero deve ser rejeitado. [FCI-021]");
            AddNotifications(c);
        }

        /// <summary>Aprova o documento (Rascunho → Aprovada). EF §5/§6.2/§6.6. Efeitos gerados pelo handler.</summary>
        public void Aprovar(string alteradoPor)
        {
            Clear();
            if (Status != EFciStatus.Rascunho)
            {
                AddNotification(nameof(Status), "Somente documento em rascunho pode ser aprovado. [Origem: FciDocumentoComercial]");
                return;
            }
            Validar();
            if (!IsValid) return;
            Status = EFciStatus.Aprovada;
            MarcarAlterado(alteradoPor);
        }

        public void MarcarExcluido(string alteradoPor)
        {
            Status = EFciStatus.Excluida;
            Deletar(alteradoPor);
        }

        /// <summary>
        /// FCI-030..037 (NF-08 sinais OPOSTOS): gera os lançamentos de razão do documento aprovado.
        /// Fatura: debita contas-a-receber (total geral), credita receita por item, credita imposto,
        /// debita desconto. Nota de crédito: exatamente o inverso.
        /// valida-contador (FCI-038): contaId de AR/imposto/desconto fica Empty até parametrização Siser.
        /// </summary>
        public IEnumerable<FciLancamentoRazao> GerarLancamentosRazao(string usuario)
        {
            var lancamentos = new List<FciLancamentoRazao>();
            var fatura = EhFatura;

            // Item: fatura credita receita; nota de crédito debita a conta da linha.
            foreach (var item in _itens)
            {
                lancamentos.Add(new FciLancamentoRazao(
                    Id, item.Id, TipoDocumento, Numero, item.ContaReceitaId,
                    debito: fatura ? 0m : item.ValorTotal,
                    credito: fatura ? item.ValorTotal : 0m,
                    EFciRazaoCategoria.Item, TenantId, usuario));
            }

            // Contas a receber pelo total geral (fatura debita, nota de crédito credita).
            lancamentos.Add(new FciLancamentoRazao(
                Id, null, TipoDocumento, Numero, Guid.Empty, // valida-contador: conta AR fixa pendente Siser.
                debito: fatura ? TotalGeral : 0m,
                credito: fatura ? 0m : TotalGeral,
                EFciRazaoCategoria.ContasReceber, TenantId, usuario));

            // Imposto pelo total de imposto (fatura credita, nota de crédito debita).
            if (TotalImposto > 0m)
            {
                lancamentos.Add(new FciLancamentoRazao(
                    Id, null, TipoDocumento, Numero, Guid.Empty, // valida-contador.
                    debito: fatura ? 0m : TotalImposto,
                    credito: fatura ? TotalImposto : 0m,
                    EFciRazaoCategoria.Imposto, TenantId, usuario));
            }

            // Desconto pelo desconto do documento (fatura debita, nota de crédito credita).
            if (DescontoDocumento > 0m)
            {
                lancamentos.Add(new FciLancamentoRazao(
                    Id, null, TipoDocumento, Numero, Guid.Empty, // valida-contador.
                    debito: fatura ? DescontoDocumento : 0m,
                    credito: fatura ? 0m : DescontoDocumento,
                    EFciRazaoCategoria.Desconto, TenantId, usuario));
            }

            return lancamentos;
        }

        /// <summary>
        /// FCI-027..029 (NF-08 sinais OPOSTOS): fatura gera SAÍDA de estoque por linha;
        /// nota de crédito gera ENTRADA (devolução). Delega a baixa efetiva ao motor único do Estoque
        /// via evento — aqui só o registro documental.
        /// </summary>
        public IEnumerable<FciLancamentoEstoque> GerarLancamentosEstoque()
        {
            var fatura = EhFatura;
            return _itens.Select(item => new FciLancamentoEstoque(
                Id, item.Id, TipoDocumento, Numero, item.ProdutoId, ArmazemId,
                quantidadeEntrada: fatura ? 0m : item.Quantidade,
                quantidadeSaida: fatura ? item.Quantidade : 0m,
                TenantId, CriadoPor ?? "system"));
        }
    }

    /// <summary>Linha do documento comercial (fci_documento_item). Fonte: EF §10.2. FCI-024.</summary>
    public class FciDocumentoItem : EntidadeSaaSBase
    {
        public Guid DocumentoId { get; private set; }
        public Guid? ItemOrigemId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public Guid UnidadeId { get; private set; }
        public decimal Quantidade { get; private set; }
        public decimal ValorUnitario { get; private set; }
        public Guid? LoteId { get; private set; }
        public decimal Desconto { get; private set; }
        public decimal ValorDesconto { get; private set; }
        public Guid? ImpostoId { get; private set; }
        public decimal? AliquotaImposto { get; private set; }
        public decimal? ValorImposto { get; private set; }
        public decimal ValorBruto { get; private set; }
        public decimal ValorLiquido { get; private set; }
        public decimal ValorTotal { get; private set; }
        public Guid ContaReceitaId { get; private set; }
        public Guid? ProjetoId { get; private set; }

        protected FciDocumentoItem() { }

        public FciDocumentoItem(
            Guid produtoId, Guid unidadeId, decimal quantidade, decimal valorUnitario, Guid? loteId,
            decimal desconto, Guid? impostoId, decimal? aliquotaImposto, Guid contaReceitaId, Guid? projetoId,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ProdutoId = produtoId;
            UnidadeId = unidadeId;
            Quantidade = quantidade <= 0 ? 1 : quantidade;
            ValorUnitario = valorUnitario;
            LoteId = loteId;
            Desconto = desconto;
            ImpostoId = impostoId;
            AliquotaImposto = aliquotaImposto;
            ContaReceitaId = contaReceitaId;
            ProjetoId = projetoId;
            Recalcular();

            AddNotifications(new Contract<FciDocumentoItem>()
                .Requires()
                .AreNotEquals(produtoId, Guid.Empty, nameof(ProdutoId), "A linha deve ter produto. [FCI-024]")
                .IsGreaterThan(Quantidade, 0m, nameof(Quantidade), "A quantidade deve ser maior que zero. [FCI-024]"));
        }

        public void VincularDocumento(Guid documentoId) => DocumentoId = documentoId;

        /// <summary>Calcula bruto/desconto/imposto/líquido/total da linha. FCI-024/025/026.</summary>
        public void Recalcular()
        {
            ValorBruto = Math.Round(Quantidade * ValorUnitario, 2);
            ValorDesconto = Math.Round(ValorBruto * (Desconto / 100m), 2);
            ValorLiquido = ValorBruto - ValorDesconto;
            ValorImposto = AliquotaImposto.HasValue
                ? Math.Round(ValorLiquido * (AliquotaImposto.Value / 100m), 2)
                : (decimal?)null;
            ValorTotal = ValorLiquido + (ValorImposto ?? 0m);
        }
    }

    /// <summary>Imposto comercial (fci_imposto). Fonte: EF §10.3. FCI-049..052.</summary>
    public class FciImposto : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public decimal Aliquota { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected FciImposto() { }

        public FciImposto(string nome, decimal aliquota, bool ativo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Aliquota = aliquota;
            Ativo = ativo;
            Validar();
        }

        public void Alterar(string nome, decimal aliquota, bool ativo, string alteradoPor)
        {
            Nome = nome;
            Aliquota = aliquota;
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<FciImposto>()
                .Requires()
                .IsNotNullOrEmpty(Nome, nameof(Nome), "O imposto deve possuir nome. [FCI-049]")
                .IsFalse(Aliquota < 0m, nameof(Aliquota), "A alíquota não pode ser negativa. [FCI-049]"));
        }
    }

    /// <summary>Preferências gerais singleton por tenant (fci_preferencia_geral). EF §10.4. FCI-053..055.</summary>
    public class FciPreferenciaGeral : EntidadeSaaSBase
    {
        public bool MostrarMoeda { get; private set; }
        public bool PermitirCaixaNegativo { get; private set; }
        public bool PermitirEstoqueNegativo { get; private set; }
        public string? ModoCalculoEstoque { get; private set; }
        public bool ControlarLimiteCredito { get; private set; }
        public bool PermitirDesconto { get; private set; }
        public bool ImpostoNaCompra { get; private set; }
        public bool ImpostoNaVenda { get; private set; }

        protected FciPreferenciaGeral() { }

        public FciPreferenciaGeral(string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            // FCI-054: configuração padrão quando não existe registro para o tenant.
            MostrarMoeda = true;
            PermitirDesconto = true;
        }

        public void Alterar(bool mostrarMoeda, bool permitirCaixaNegativo, bool permitirEstoqueNegativo,
            string? modoCalculoEstoque, bool controlarLimiteCredito, bool permitirDesconto,
            bool impostoNaCompra, bool impostoNaVenda, string alteradoPor)
        {
            MostrarMoeda = mostrarMoeda;
            PermitirCaixaNegativo = permitirCaixaNegativo;
            PermitirEstoqueNegativo = permitirEstoqueNegativo;
            ModoCalculoEstoque = modoCalculoEstoque;
            ControlarLimiteCredito = controlarLimiteCredito;
            PermitirDesconto = permitirDesconto;
            ImpostoNaCompra = impostoNaCompra;
            ImpostoNaVenda = impostoNaVenda;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Configuração por tipo de documento (fci_configuracao_documento). EF §10.5. FCI-056..058.</summary>
    public class FciConfiguracaoDocumento : EntidadeSaaSBase
    {
        public EFciTipoDocumento TipoDocumento { get; private set; }
        public string? NomeTipoDocumento { get; private set; }
        public int IndiceInicial { get; private set; }
        public string? Prefixo { get; private set; }
        public string? Sufixo { get; private set; }
        public Guid? EmpresaId { get; private set; }
        public string? NumeroPedido { get; private set; }
        public string? NumeroTransporte { get; private set; }
        public string? Veiculo { get; private set; }
        public bool MostrarDesconto { get; private set; } = true;
        public bool MostrarImposto { get; private set; } = true;
        public bool MostrarCodigoBarras { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected FciConfiguracaoDocumento() { }

        public FciConfiguracaoDocumento(EFciTipoDocumento tipoDocumento, string? nomeTipoDocumento,
            int indiceInicial, string? prefixo, string? sufixo, Guid? empresaId, bool mostrarDesconto,
            bool mostrarImposto, bool mostrarCodigoBarras, bool ativo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            TipoDocumento = tipoDocumento;
            NomeTipoDocumento = nomeTipoDocumento;
            IndiceInicial = indiceInicial;
            Prefixo = prefixo;
            Sufixo = sufixo;
            EmpresaId = empresaId;
            MostrarDesconto = mostrarDesconto;
            MostrarImposto = mostrarImposto;
            MostrarCodigoBarras = mostrarCodigoBarras;
            Ativo = ativo;
        }

        /// <summary>Prefixo por tenant (FCI-007/008/058). Fallback documentado quando não configurado.</summary>
        public static string PrefixoPadrao(EFciTipoDocumento tipo) => tipo switch
        {
            EFciTipoDocumento.Fatura => "INV",         // fallback; prefixo real é configurável (FCI-058).
            EFciTipoDocumento.NotaCredito => "CN",     // credit note.
            _ => "QT"
        };
    }

    /// <summary>Lançamento de estoque documental (fci_lancamento_estoque). EF §10.6. FCI-029.</summary>
    public class FciLancamentoEstoque : EntidadeSaaSBase
    {
        public Guid DocumentoId { get; private set; }
        public Guid DocumentoItemId { get; private set; }
        public EFciTipoDocumento TipoDocumento { get; private set; }
        public string NumeroDocumento { get; private set; } = string.Empty;
        public Guid ProdutoId { get; private set; }
        public Guid ArmazemId { get; private set; }
        public decimal QuantidadeEntrada { get; private set; }
        public decimal QuantidadeSaida { get; private set; }
        public string? ModoCalculo { get; private set; }
        public DateTime DataLancamento { get; private set; }

        protected FciLancamentoEstoque() { }

        public FciLancamentoEstoque(Guid documentoId, Guid documentoItemId, EFciTipoDocumento tipoDocumento,
            string numeroDocumento, Guid produtoId, Guid armazemId, decimal quantidadeEntrada,
            decimal quantidadeSaida, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            DocumentoId = documentoId;
            DocumentoItemId = documentoItemId;
            TipoDocumento = tipoDocumento;
            NumeroDocumento = numeroDocumento;
            ProdutoId = produtoId;
            ArmazemId = armazemId;
            QuantidadeEntrada = quantidadeEntrada;
            QuantidadeSaida = quantidadeSaida;
            DataLancamento = DateTime.UtcNow;
        }
    }

    /// <summary>Lançamento de razão documental (fci_lancamento_razao). EF §10.7. FCI-030..037.</summary>
    public class FciLancamentoRazao : EntidadeSaaSBase
    {
        public Guid DocumentoId { get; private set; }
        public Guid? DocumentoItemId { get; private set; }
        public EFciTipoDocumento TipoDocumento { get; private set; }
        public string NumeroDocumento { get; private set; } = string.Empty;
        public Guid ContaId { get; private set; }
        public decimal Debito { get; private set; }
        public decimal Credito { get; private set; }
        public EFciRazaoCategoria Categoria { get; private set; }
        public DateTime DataLancamento { get; private set; }

        protected FciLancamentoRazao() { }

        public FciLancamentoRazao(Guid documentoId, Guid? documentoItemId, EFciTipoDocumento tipoDocumento,
            string numeroDocumento, Guid contaId, decimal debito, decimal credito, EFciRazaoCategoria categoria,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            DocumentoId = documentoId;
            DocumentoItemId = documentoItemId;
            TipoDocumento = tipoDocumento;
            NumeroDocumento = numeroDocumento;
            ContaId = contaId;
            Debito = debito;
            Credito = credito;
            Categoria = categoria;
            DataLancamento = DateTime.UtcNow;
        }
    }

    /// <summary>Registro de geração de PDF/impressão (fci_pdf_documento). EF §10.8. FCI-060/061.</summary>
    public class FciPdfDocumento : EntidadeSaaSBase
    {
        public Guid DocumentoId { get; private set; }
        public EFciTipoSaidaPdf TipoSaida { get; private set; }
        public string? PaginaOrigem { get; private set; }
        public string? ArquivoReferencia { get; private set; }
        public DateTime DataGeracao { get; private set; }
        public Guid? UsuarioId { get; private set; }

        protected FciPdfDocumento() { }

        public FciPdfDocumento(Guid documentoId, EFciTipoSaidaPdf tipoSaida, string? paginaOrigem,
            string? arquivoReferencia, Guid? usuarioId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            DocumentoId = documentoId;
            TipoSaida = tipoSaida;
            PaginaOrigem = paginaOrigem;
            ArquivoReferencia = arquivoReferencia;
            UsuarioId = usuarioId;
            DataGeracao = DateTime.UtcNow;
        }
    }

    /// <summary>Histórico/auditoria (fci_historico). EF §10.9. FCI-064.</summary>
    public class FciHistorico : EntidadeSaaSBase
    {
        public Guid? DocumentoId { get; private set; }
        public EFciEntidadeTipo EntidadeTipo { get; private set; }
        public EFciEvento Evento { get; private set; }
        public Guid? UsuarioId { get; private set; }
        public string? DadosAnterioresJson { get; private set; }
        public string? DadosNovosJson { get; private set; }
        public DateTime DataEvento { get; private set; }

        protected FciHistorico() { }

        public FciHistorico(Guid? documentoId, EFciEntidadeTipo entidadeTipo, EFciEvento evento,
            Guid? usuarioId, string? dadosAnterioresJson, string? dadosNovosJson, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            DocumentoId = documentoId;
            EntidadeTipo = entidadeTipo;
            Evento = evento;
            UsuarioId = usuarioId;
            DadosAnterioresJson = dadosAnterioresJson;
            DadosNovosJson = dadosNovosJson;
            DataEvento = DateTime.UtcNow;
        }
    }
}
