using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Expedição / romaneio de saída (ven_expedicao). Fonte: EF §19.1. Regras §11..§15.
    /// Cabeçalho vinculado ao pedido/venda de origem. FISCAL: o local de entrega e o transporte
    /// alimentam o documento fiscal quando houver emissão (EF §15) — os grupos "entrega" (G) e
    /// "transp" da NF-e/NFC-e. A emissão/cancelamento fiscal permanece no módulo Fiscal
    /// (IHerculesFiscalService); aqui apenas registramos e referenciamos o documento_fiscal_id.
    /// [FATO fiscal: MOC 7.0 Anexo I §2 grupos G (entrega) e X (transp); NFC-e entrega a domicílio
    ///  usa indPres=4 — campo B25b. Regra da fábrica: fiscal nunca de memória.]
    /// </summary>
    public class Expedicao : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public Guid PedidoId { get; private set; }
        public Guid? DocumentoFiscalId { get; private set; }
        public Guid? RomaneioId { get; private set; }
        public EExpedicaoStatus Status { get; private set; } = EExpedicaoStatus.Rascunho;
        public DateTime? DataExpedicao { get; private set; }
        public DateTime? DataConfirmacao { get; private set; }
        public string? Observacoes { get; private set; }
        public string? TraceId { get; private set; }
        public long? SequenciaExibicao { get; private set; }

        protected Expedicao() { }

        public Expedicao(
            Guid empresaId,
            Guid pedidoId,
            Guid? documentoFiscalId,
            Guid? romaneioId,
            DateTime? dataExpedicao,
            string? observacoes,
            string? traceId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            PedidoId = pedidoId;
            DocumentoFiscalId = documentoFiscalId;
            RomaneioId = romaneioId;
            DataExpedicao = dataExpedicao;
            Observacoes = observacoes;
            TraceId = traceId;
            Status = EExpedicaoStatus.Rascunho;
            Validar();
        }

        public void Confirmar(string alteradoPor)
        {
            Status = EExpedicaoStatus.Confirmado;
            DataConfirmacao = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>EF §15/§16: expedição faturada quando vinculada a documento fiscal emitido.</summary>
        public void Faturar(Guid documentoFiscalId, string alteradoPor)
        {
            Status = EExpedicaoStatus.Faturado;
            DocumentoFiscalId = documentoFiscalId;
            MarcarAlterado(alteradoPor);
        }

        public void Cancelar(string alteradoPor)
        {
            Status = EExpedicaoStatus.Cancelado;
            MarcarAlterado(alteradoPor);
        }

        public void Estornar(string alteradoPor)
        {
            Status = EExpedicaoStatus.Estornado;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Expedicao>()
                .Requires()
                .AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "A empresa é obrigatória. [Origem: Expedicao]")
                .AreNotEquals(PedidoId, Guid.Empty, nameof(PedidoId), "O pedido de origem é obrigatório. [Origem: Expedicao]"));
        }
    }

    /// <summary>Local de entrega da expedição (ven_expedicao_local_entrega). Fonte: EF §19.2.</summary>
    public class ExpedicaoLocalEntrega : EntidadeSaaSBase
    {
        public Guid ExpedicaoId { get; private set; }
        public Guid? DocumentoFiscalId { get; private set; }
        public string CpfCnpj { get; private set; } = string.Empty;
        public string Logradouro { get; private set; } = string.Empty;
        public string Numero { get; private set; } = string.Empty;
        public string Complemento { get; private set; } = string.Empty;
        public string Bairro { get; private set; } = string.Empty;
        public string? CodigoMunicipio { get; private set; }
        public string NomeMunicipio { get; private set; } = string.Empty;
        public string Uf { get; private set; } = string.Empty;

        protected ExpedicaoLocalEntrega() { }

        public ExpedicaoLocalEntrega(
            Guid expedicaoId,
            Guid? documentoFiscalId,
            string cpfCnpj,
            string logradouro,
            string numero,
            string complemento,
            string bairro,
            string? codigoMunicipio,
            string nomeMunicipio,
            string uf,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ExpedicaoId = expedicaoId;
            DocumentoFiscalId = documentoFiscalId;
            CpfCnpj = cpfCnpj;
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            CodigoMunicipio = codigoMunicipio;
            NomeMunicipio = nomeMunicipio;
            Uf = uf;
            AddNotifications(new Contract<ExpedicaoLocalEntrega>()
                .Requires()
                .AreNotEquals(expedicaoId, Guid.Empty, nameof(ExpedicaoId), "A expedição é obrigatória. [Origem: ExpedicaoLocalEntrega]")
                .IsNotNullOrEmpty(cpfCnpj, nameof(CpfCnpj), "O CPF/CNPJ do local de entrega é obrigatório. [Origem: ExpedicaoLocalEntrega]")
                .IsNotNullOrEmpty(nomeMunicipio, nameof(NomeMunicipio), "O município do local de entrega é obrigatório. [Origem: ExpedicaoLocalEntrega]")
                .IsNotNullOrEmpty(uf, nameof(Uf), "A UF do local de entrega é obrigatória. [Origem: ExpedicaoLocalEntrega]"));
        }
    }

    /// <summary>
    /// Transporte da expedição (ven_expedicao_transporte). Fonte: EF §19.3.
    /// §13.2: quando informado, ao menos um grupo (transportadora/veículo/volumes) deve estar preenchido.
    /// </summary>
    public class ExpedicaoTransporte : EntidadeSaaSBase
    {
        public Guid ExpedicaoId { get; private set; }
        public string? ModalidadeFrete { get; private set; }
        public string? ConhecimentoTransporte { get; private set; }
        public bool PossuiTransportadora { get; private set; }
        public bool PossuiVeiculo { get; private set; }
        public bool PossuiVolumes { get; private set; }

        protected ExpedicaoTransporte() { }

        public ExpedicaoTransporte(Guid expedicaoId, string? modalidadeFrete, string? conhecimentoTransporte, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ExpedicaoId = expedicaoId;
            ModalidadeFrete = modalidadeFrete;
            ConhecimentoTransporte = conhecimentoTransporte;
            AddNotifications(new Contract<ExpedicaoTransporte>()
                .Requires()
                .AreNotEquals(expedicaoId, Guid.Empty, nameof(ExpedicaoId), "A expedição é obrigatória. [Origem: ExpedicaoTransporte]"));
        }

        public void AtualizarIndicadores(bool possuiTransportadora, bool possuiVeiculo, bool possuiVolumes, string alteradoPor)
        {
            PossuiTransportadora = possuiTransportadora;
            PossuiVeiculo = possuiVeiculo;
            PossuiVolumes = possuiVolumes;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Transportadora do transporte (ven_expedicao_transportadora). Fonte: EF §19.4.</summary>
    public class ExpedicaoTransportadora : EntidadeSaaSBase
    {
        public Guid TransporteId { get; private set; }
        public Guid? PessoaId { get; private set; }
        public string? Cnpj { get; private set; }
        public string? Cpf { get; private set; }
        public string? RazaoSocial { get; private set; }
        public string? InscricaoEstadual { get; private set; }
        public string? EnderecoCompleto { get; private set; }
        public string? NomeMunicipio { get; private set; }
        public string? Uf { get; private set; }

        protected ExpedicaoTransportadora() { }

        public ExpedicaoTransportadora(Guid transporteId, Guid? pessoaId, string? cnpj, string? cpf, string? razaoSocial, string? inscricaoEstadual, string? enderecoCompleto, string? nomeMunicipio, string? uf, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            TransporteId = transporteId;
            PessoaId = pessoaId;
            Cnpj = cnpj;
            Cpf = cpf;
            RazaoSocial = razaoSocial;
            InscricaoEstadual = inscricaoEstadual;
            EnderecoCompleto = enderecoCompleto;
            NomeMunicipio = nomeMunicipio;
            Uf = uf;
            AddNotifications(new Contract<ExpedicaoTransportadora>()
                .Requires()
                .AreNotEquals(transporteId, Guid.Empty, nameof(TransporteId), "O transporte é obrigatório. [Origem: ExpedicaoTransportadora]"));
        }
    }

    /// <summary>Veículo do transporte (ven_expedicao_veiculo). Fonte: EF §19.5.</summary>
    public class ExpedicaoVeiculo : EntidadeSaaSBase
    {
        public Guid TransporteId { get; private set; }
        public Guid? VeiculoId { get; private set; }
        public string? Placa { get; private set; }
        public string? Rntrc { get; private set; }
        public string? Uf { get; private set; }

        protected ExpedicaoVeiculo() { }

        public ExpedicaoVeiculo(Guid transporteId, Guid? veiculoId, string? placa, string? rntrc, string? uf, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            TransporteId = transporteId;
            VeiculoId = veiculoId;
            Placa = placa;
            Rntrc = rntrc;
            Uf = uf;
            AddNotifications(new Contract<ExpedicaoVeiculo>()
                .Requires()
                .AreNotEquals(transporteId, Guid.Empty, nameof(TransporteId), "O transporte é obrigatório. [Origem: ExpedicaoVeiculo]"));
        }
    }

    /// <summary>Reboque do transporte (ven_expedicao_reboque). Fonte: EF §19.6.</summary>
    public class ExpedicaoReboque : EntidadeSaaSBase
    {
        public Guid TransporteId { get; private set; }
        public Guid? VeiculoId { get; private set; }
        public string? Placa { get; private set; }
        public string? Rntrc { get; private set; }
        public string? Uf { get; private set; }

        protected ExpedicaoReboque() { }

        public ExpedicaoReboque(Guid transporteId, Guid? veiculoId, string? placa, string? rntrc, string? uf, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            TransporteId = transporteId;
            VeiculoId = veiculoId;
            Placa = placa;
            Rntrc = rntrc;
            Uf = uf;
            AddNotifications(new Contract<ExpedicaoReboque>()
                .Requires()
                .AreNotEquals(transporteId, Guid.Empty, nameof(TransporteId), "O transporte é obrigatório. [Origem: ExpedicaoReboque]"));
        }
    }

    /// <summary>Volume do transporte (ven_expedicao_volume). Fonte: EF §19.7 / §12.</summary>
    public class ExpedicaoVolume : EntidadeSaaSBase
    {
        public Guid TransporteId { get; private set; }
        public decimal? QuantidadeVolumes { get; private set; }
        public string? Especie { get; private set; }
        public string? Marca { get; private set; }
        public string? NumeroVolumes { get; private set; }
        public decimal? PesoLiquido { get; private set; }
        public decimal? PesoBruto { get; private set; }

        protected ExpedicaoVolume() { }

        public ExpedicaoVolume(Guid transporteId, decimal? quantidadeVolumes, string? especie, string? marca, string? numeroVolumes, decimal? pesoLiquido, decimal? pesoBruto, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            TransporteId = transporteId;
            QuantidadeVolumes = quantidadeVolumes;
            Especie = especie;
            Marca = marca;
            NumeroVolumes = numeroVolumes;
            PesoLiquido = pesoLiquido;
            PesoBruto = pesoBruto;
            AddNotifications(new Contract<ExpedicaoVolume>()
                .Requires()
                .AreNotEquals(transporteId, Guid.Empty, nameof(TransporteId), "O transporte é obrigatório. [Origem: ExpedicaoVolume]"));
        }
    }

    /// <summary>
    /// Item de entrega (baixa de quantidade entregue) (ven_expedicao_item_entrega). Fonte: EF §19.8 / §14.
    /// §14.5: quantidade entregue não deve ultrapassar a quantidade vendida.
    /// </summary>
    public class ExpedicaoItemEntrega : EntidadeSaaSBase
    {
        public Guid ExpedicaoId { get; private set; }
        public Guid PedidoItemId { get; private set; }
        public Guid? ProdutoId { get; private set; }
        public decimal QuantidadeVendida { get; private set; }
        public decimal QuantidadeEntregue { get; private set; }
        public decimal SaldoEntrega { get; private set; }
        public DateTime? DataEntrega { get; private set; }
        public Guid? UsuarioEntregaId { get; private set; }

        protected ExpedicaoItemEntrega() { }

        public ExpedicaoItemEntrega(
            Guid expedicaoId,
            Guid pedidoItemId,
            Guid? produtoId,
            decimal quantidadeVendida,
            decimal quantidadeEntregue,
            Guid? usuarioEntregaId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ExpedicaoId = expedicaoId;
            PedidoItemId = pedidoItemId;
            ProdutoId = produtoId;
            QuantidadeVendida = quantidadeVendida;
            QuantidadeEntregue = quantidadeEntregue;
            SaldoEntrega = quantidadeVendida - quantidadeEntregue;
            if (quantidadeEntregue > 0) DataEntrega = DateTime.UtcNow;
            UsuarioEntregaId = usuarioEntregaId;
            Validar();
        }

        /// <summary>§14.6: alteração da quantidade entregue recalcula saldo e registra usuário/data.</summary>
        public void RegistrarEntrega(decimal quantidadeEntregue, Guid? usuarioEntregaId, string alteradoPor)
        {
            QuantidadeEntregue = quantidadeEntregue;
            SaldoEntrega = QuantidadeVendida - quantidadeEntregue;
            DataEntrega = DateTime.UtcNow;
            UsuarioEntregaId = usuarioEntregaId;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ExpedicaoItemEntrega>()
                .Requires()
                .AreNotEquals(ExpedicaoId, Guid.Empty, nameof(ExpedicaoId), "A expedição é obrigatória. [Origem: ExpedicaoItemEntrega]")
                .AreNotEquals(PedidoItemId, Guid.Empty, nameof(PedidoItemId), "O item de pedido é obrigatório. [Origem: ExpedicaoItemEntrega]")
                // §14.5: entregue não ultrapassa vendida.
                .IsLowerOrEqualsThan(QuantidadeEntregue, QuantidadeVendida, nameof(QuantidadeEntregue), "A quantidade entregue não pode ultrapassar a quantidade vendida. [Origem: ExpedicaoItemEntrega]"));
        }
    }

    /// <summary>Histórico/auditoria de expedição (ven_expedicao_historico). Fonte: EF §19.9.</summary>
    public class ExpedicaoHistorico : EntidadeSaaSBase
    {
        public Guid ExpedicaoId { get; private set; }
        public DateTime DataHora { get; private set; }
        public Guid UsuarioId { get; private set; }
        public string Evento { get; private set; } = string.Empty;
        public string? StatusAnterior { get; private set; }
        public string? StatusNovo { get; private set; }
        public string? Detalhe { get; private set; }
        public string? TraceId { get; private set; }

        protected ExpedicaoHistorico() { }

        public ExpedicaoHistorico(Guid expedicaoId, Guid usuarioId, string evento, string? statusAnterior, string? statusNovo, string? detalhe, string? traceId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ExpedicaoId = expedicaoId;
            DataHora = DateTime.UtcNow;
            UsuarioId = usuarioId;
            Evento = evento;
            StatusAnterior = statusAnterior;
            StatusNovo = statusNovo;
            Detalhe = detalhe;
            TraceId = traceId;
            AddNotifications(new Contract<ExpedicaoHistorico>()
                .Requires()
                .AreNotEquals(expedicaoId, Guid.Empty, nameof(ExpedicaoId), "A expedição é obrigatória. [Origem: ExpedicaoHistorico]"));
        }
    }
}
