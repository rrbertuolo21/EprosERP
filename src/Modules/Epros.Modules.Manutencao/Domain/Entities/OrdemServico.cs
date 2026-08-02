using System;
using System.Collections.Generic;
using Epros.Modules.Manutencao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Manutencao.Domain.Entities
{
    /// <summary>
    /// MAN-TRB — Ordem de servico de manutencao (agregado raiz). Fiel a EF secao 11.1.
    /// Distinta da OrdemManutencao legada simples; cobre o ciclo Oficina/Campo completo.
    /// </summary>
    public class OrdemServico : EntidadeSaaSBase
    {
        public string? Numero { get; private set; }
        public EPerfilOrdem PerfilOrdem { get; private set; }
        public Guid? EmpresaId { get; private set; }
        public int TipoPessoa { get; private set; }
        // T5 — nullable: ordem INTERNA (origem != Manual) nao exige cliente/pessoa.
        public Guid? PessoaId { get; private set; }
        // T5 — origem canonica da OS (Manual/Preventiva/Preditiva/Corretiva/Confiabilidade)
        // e id do registro de origem (execucao PRV, alarme PDT, parada PAR, recomendacao CRV).
        public EOrigemOrdemServico OrigemTipo { get; private set; } = EOrigemOrdemServico.Manual;
        public Guid? OrigemId { get; private set; }
        public Guid? ColaboradorId { get; private set; }
        public DateTime Data { get; private set; }
        public DateTime? DataAbertura { get; private set; }
        public DateTime? DataOrcamento { get; private set; }
        public DateTime? DataAprovacao { get; private set; }
        public DateTime? DataMontagem { get; private set; }
        public DateTime? DataPronta { get; private set; }
        public DateTime? DataEntrega { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public string? HoraInicio { get; private set; }
        public DateTime? DataPrevisao { get; private set; }
        public string? HoraPrevisao { get; private set; }
        public DateTime? DataFim { get; private set; }
        public string? HoraFim { get; private set; }
        public string? NomeContato { get; private set; }
        public string? FoneContato { get; private set; }
        public string? ObservacaoCliente { get; private set; }
        public string? ObservacaoAbertura { get; private set; }
        public string? Reclamacao { get; private set; }
        public string? Observacao { get; private set; }
        public bool Garantia { get; private set; }
        public Guid? TipoAtendimentoId { get; private set; }
        public Guid? TipoEquipamentoId { get; private set; }
        public Guid? MarcaId { get; private set; }
        public string? InfoEquip1 { get; private set; }
        public string? InfoEquip2 { get; private set; }
        public string? InfoEquip3 { get; private set; }
        public string? ObsEquip1 { get; private set; }
        public string? ObsEquip2 { get; private set; }
        public EStatusOrdemServico StatusCodigo { get; private set; } = EStatusOrdemServico.Aberta;
        public Guid? UsuarioComissaoOrcamentoId { get; private set; }
        public Guid? UsuarioComissaoMontagemId { get; private set; }
        public bool Faturado { get; private set; }
        public bool DocumentoFiscalEmitido { get; private set; }
        public Guid? DocumentoFiscalId { get; private set; }
        public bool Cancelado { get; private set; }
        public string? DocumentoPessoa { get; private set; }
        public Guid? PagamentoId { get; private set; }
        public Guid? ParcelamentoId { get; private set; }
        public Guid? UFId { get; private set; }
        public int? TipoNF { get; private set; }
        public int Versao { get; private set; } = 1;

        public List<OrdemServicoItem> Itens { get; private set; } = new();
        public List<OrdemServicoEvolucao> Evolucoes { get; private set; } = new();
        public List<OrdemServicoEquipamento> Equipamentos { get; private set; } = new();
        public List<OrdemServicoFinanceiroFiscal> VinculosFinanceiros { get; private set; } = new();

        protected OrdemServico() { } // EF Core

        public OrdemServico(
            EPerfilOrdem perfilOrdem,
            int tipoPessoa,
            Guid pessoaId,
            DateTime data,
            bool garantia,
            Guid? tipoAtendimentoId,
            Guid? tipoEquipamentoId,
            Guid? marcaId,
            Guid? colaboradorId,
            string? numero,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PerfilOrdem = perfilOrdem;
            TipoPessoa = tipoPessoa;
            PessoaId = pessoaId;
            Data = data;
            Garantia = garantia;
            TipoAtendimentoId = tipoAtendimentoId;
            TipoEquipamentoId = tipoEquipamentoId;
            MarcaId = marcaId;
            ColaboradorId = colaboradorId;
            Numero = numero;
            OrigemTipo = EOrigemOrdemServico.Manual;
            StatusCodigo = EStatusOrdemServico.Aberta;
            DataAbertura = DateTime.UtcNow;
            Versao = 1;
            Validar();
        }

        // T5 — construtor de ordem INTERNA (sem cliente/pessoa). Usado pela factory CriarInterna.
        private OrdemServico(
            EOrigemOrdemServico origemTipo,
            Guid origemId,
            EPerfilOrdem perfilOrdem,
            DateTime data,
            Guid? colaboradorId,
            Guid? tipoEquipamentoId,
            string? numero,
            string? observacaoAbertura,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PerfilOrdem = perfilOrdem;
            TipoPessoa = 0;
            PessoaId = null; // ordem interna nao tem cliente
            OrigemTipo = origemTipo;
            OrigemId = origemId;
            Data = data;
            ColaboradorId = colaboradorId;
            TipoEquipamentoId = tipoEquipamentoId;
            Numero = numero;
            ObservacaoAbertura = observacaoAbertura;
            StatusCodigo = EStatusOrdemServico.Aberta;
            DataAbertura = DateTime.UtcNow;
            Versao = 1;
            Validar();
        }

        /// <summary>
        /// T5 — Cria a OS canonica INTERNA a partir de outro submodulo (PRV/PDT/PAR/CRV),
        /// sem exigir cliente/pessoa. Perfil default Oficina; a origem (tipo+id) rastreia o
        /// registro gerador (execucao preventiva, alarme preditivo, parada, recomendacao).
        /// </summary>
        public static OrdemServico CriarInterna(
            EOrigemOrdemServico origemTipo,
            Guid origemId,
            DateTime data,
            string tenantId,
            string criadoPor,
            Guid? colaboradorId = null,
            Guid? tipoEquipamentoId = null,
            string? numero = null,
            string? observacaoAbertura = null,
            EPerfilOrdem perfilOrdem = EPerfilOrdem.Oficina)
            => new OrdemServico(origemTipo, origemId, perfilOrdem, data, colaboradorId, tipoEquipamentoId, numero, observacaoAbertura, tenantId, criadoPor);

        public void AdicionarItem(OrdemServicoItem item, string usuario)
        {
            if (Cancelado || StatusCodigo == EStatusOrdemServico.Entregue)
            {
                AddNotification(nameof(StatusCodigo), "Nao e possivel adicionar itens a uma OS cancelada ou entregue.");
                return;
            }
            if (!item.IsValid) { AddNotifications(item.Notifications); return; }
            Itens.Add(item);
            Versao++;
            MarcarAlterado(usuario);
        }

        public void RegistrarEvolucao(OrdemServicoEvolucao evolucao, string usuario)
        {
            if (!evolucao.IsValid) { AddNotifications(evolucao.Notifications); return; }
            Evolucoes.Add(evolucao);
            MarcarAlterado(usuario);
        }

        // D19 — maquina de estado da OS (gating). Transicoes validas por status atual.
        // Cancelamento se da por Cancelar() (exige motivo), nao por TransicionarStatus.
        private static readonly System.Collections.Generic.Dictionary<EStatusOrdemServico, EStatusOrdemServico[]> TransicoesValidas = new()
        {
            [EStatusOrdemServico.Aberta] = new[] { EStatusOrdemServico.EmOrcamento, EStatusOrdemServico.Aprovada },
            [EStatusOrdemServico.EmOrcamento] = new[] { EStatusOrdemServico.Aprovada, EStatusOrdemServico.Aberta },
            [EStatusOrdemServico.Aprovada] = new[] { EStatusOrdemServico.Montagem },
            [EStatusOrdemServico.Montagem] = new[] { EStatusOrdemServico.Pronta },
            [EStatusOrdemServico.Pronta] = new[] { EStatusOrdemServico.Entregue },
            [EStatusOrdemServico.Entregue] = System.Array.Empty<EStatusOrdemServico>(),
            [EStatusOrdemServico.Cancelada] = System.Array.Empty<EStatusOrdemServico>()
        };

        public static bool PodeTransicionar(EStatusOrdemServico de, EStatusOrdemServico para) =>
            TransicoesValidas.TryGetValue(de, out var destinos) && System.Array.IndexOf(destinos, para) >= 0;

        // Transicoes de status espelhando o campo de data associado.
        public void TransicionarStatus(EStatusOrdemServico novoStatus, string usuario)
        {
            if (Cancelado)
            {
                AddNotification(nameof(StatusCodigo), "OS cancelada nao pode mudar de status.");
                return;
            }
            if (novoStatus == EStatusOrdemServico.Cancelada)
            {
                AddNotification(nameof(StatusCodigo), "Use o cancelamento (com motivo) para cancelar a OS.");
                return;
            }
            if (!PodeTransicionar(StatusCodigo, novoStatus))
            {
                AddNotification(nameof(StatusCodigo), $"Transicao de status invalida: {StatusCodigo} -> {novoStatus}.");
                return;
            }
            StatusCodigo = novoStatus;
            var agora = DateTime.UtcNow;
            switch (novoStatus)
            {
                case EStatusOrdemServico.EmOrcamento: DataOrcamento = agora; break;
                case EStatusOrdemServico.Aprovada: DataAprovacao = agora; break;
                case EStatusOrdemServico.Montagem: DataMontagem = agora; break;
                case EStatusOrdemServico.Pronta: DataPronta = agora; break;
                case EStatusOrdemServico.Entregue: DataEntrega = agora; break;
            }
            Versao++;
            MarcarAlterado(usuario);
        }

        public void Cancelar(string motivo, string usuario)
        {
            if (Faturado)
            {
                AddNotification(nameof(Faturado), "OS faturada nao pode ser cancelada.");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(Observacao), "Informe o motivo do cancelamento.");
                return;
            }
            Cancelado = true;
            StatusCodigo = EStatusOrdemServico.Cancelada;
            Observacao = motivo;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void MarcarFaturado(Guid? documentoFiscalId, string usuario)
        {
            Faturado = true;
            if (documentoFiscalId.HasValue)
            {
                DocumentoFiscalEmitido = true;
                DocumentoFiscalId = documentoFiscalId;
            }
            Versao++;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<OrdemServico>()
                .Requires()
                // T5 — cliente/pessoa so e obrigatorio na ordem EXTERNA (origem Manual);
                // ordens internas (PRV/PDT/PAR/CRV) nao tem pessoa.
                .IsTrue(
                    OrigemTipo != EOrigemOrdemServico.Manual || (PessoaId.HasValue && PessoaId.Value != Guid.Empty),
                    nameof(PessoaId),
                    "A pessoa/cliente e obrigatoria [Origem: OrdemServico].")
                .IsTrue(
                    PerfilOrdem != EPerfilOrdem.Campo || (ColaboradorId.HasValue && ColaboradorId.Value != Guid.Empty),
                    nameof(ColaboradorId),
                    "No perfil Campo o colaborador responsavel e obrigatorio."));
        }
    }

    /// <summary>MAN-TRB — Status configuravel da OS por tenant. EF 11.2.</summary>
    public class OrdemServicoStatus : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public int? Ordem { get; private set; }
        public string? CampoDataAssociado { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected OrdemServicoStatus() { }

        public OrdemServicoStatus(string codigo, string nome, int? ordem, string? campoDataAssociado, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Nome = nome;
            Ordem = ordem;
            CampoDataAssociado = campoDataAssociado;
            Ativo = true;

            AddNotifications(new Contract<OrdemServicoStatus>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O codigo do status e obrigatorio.")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do status e obrigatorio."));
        }
    }

    /// <summary>MAN-TRB — Equipamento vinculado a OS. EF 11.3.</summary>
    public class OrdemServicoEquipamento : EntidadeSaaSBase
    {
        public Guid OrdemServicoId { get; private set; }
        public Guid EquipamentoId { get; private set; }
        public string NumeroSerie { get; private set; } = string.Empty;
        public int? TipoCobertura { get; private set; }
        public string? Observacao { get; private set; }

        protected OrdemServicoEquipamento() { }

        public OrdemServicoEquipamento(Guid ordemServicoId, Guid equipamentoId, string numeroSerie, int? tipoCobertura, string? observacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            OrdemServicoId = ordemServicoId;
            EquipamentoId = equipamentoId;
            NumeroSerie = numeroSerie;
            TipoCobertura = tipoCobertura;
            Observacao = observacao;

            AddNotifications(new Contract<OrdemServicoEquipamento>()
                .Requires()
                .AreNotEquals(ordemServicoId, Guid.Empty, nameof(OrdemServicoId), "A OS e obrigatoria.")
                .AreNotEquals(equipamentoId, Guid.Empty, nameof(EquipamentoId), "O equipamento e obrigatorio."));
        }
    }

    /// <summary>MAN-TRB — Evolucao/apontamento da OS. EF 11.4.</summary>
    public class OrdemServicoEvolucao : EntidadeSaaSBase
    {
        public Guid OrdemServicoId { get; private set; }
        public DateTime? DataRegistro { get; private set; }
        public string HoraRegistro { get; private set; } = string.Empty;
        public string Observacao { get; private set; } = string.Empty;
        public bool EnviarEmail { get; private set; }
        public Guid? UsuarioId { get; private set; }

        protected OrdemServicoEvolucao() { }

        public OrdemServicoEvolucao(Guid ordemServicoId, DateTime? dataRegistro, string horaRegistro, string observacao, bool enviarEmail, Guid? usuarioId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            OrdemServicoId = ordemServicoId;
            DataRegistro = dataRegistro ?? DateTime.UtcNow;
            HoraRegistro = horaRegistro;
            Observacao = observacao;
            EnviarEmail = enviarEmail;
            UsuarioId = usuarioId;

            AddNotifications(new Contract<OrdemServicoEvolucao>()
                .Requires()
                .AreNotEquals(ordemServicoId, Guid.Empty, nameof(OrdemServicoId), "A OS e obrigatoria.")
                .IsNotNullOrEmpty(observacao, nameof(Observacao), "O texto da evolucao e obrigatorio."));
        }
    }

    /// <summary>MAN-TRB — Item (produto/peca/servico) da OS. EF 11.5.</summary>
    public class OrdemServicoItem : EntidadeSaaSBase
    {
        public Guid OrdemServicoId { get; private set; }
        public Guid? ProdutoId { get; private set; }
        public ETipoItemOrdemServico? Tipo { get; private set; }
        public string? Complemento { get; private set; }
        public decimal Quantidade { get; private set; }
        public decimal QuantidadeEntregue { get; private set; }
        public decimal ValorUnitario { get; private set; }
        public decimal ValorSubtotal { get; private set; }
        public decimal TaxaDesconto { get; private set; }
        public decimal ValorDesconto { get; private set; }
        public decimal ValorTotal { get; private set; }
        public decimal ValorCusto { get; private set; }
        public decimal ValorVenda { get; private set; }
        public ETipoSaidaItem? TipoSaida { get; private set; }
        public Guid? GradeId { get; private set; }
        public string? Informacao { get; private set; }

        protected OrdemServicoItem() { }

        public OrdemServicoItem(
            Guid ordemServicoId,
            Guid? produtoId,
            ETipoItemOrdemServico? tipo,
            string? complemento,
            decimal quantidade,
            decimal valorUnitario,
            decimal taxaDesconto,
            ETipoSaidaItem? tipoSaida,
            Guid? gradeId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            OrdemServicoId = ordemServicoId;
            ProdutoId = produtoId;
            Tipo = tipo;
            Complemento = complemento;
            Quantidade = quantidade;
            QuantidadeEntregue = 0m;
            ValorUnitario = valorUnitario;
            TaxaDesconto = taxaDesconto;
            TipoSaida = tipoSaida;
            GradeId = gradeId;
            RecalcularTotais();

            AddNotifications(new Contract<OrdemServicoItem>()
                .Requires()
                .AreNotEquals(ordemServicoId, Guid.Empty, nameof(OrdemServicoId), "A OS e obrigatoria.")
                .IsTrue(
                    tipo != ETipoItemOrdemServico.Produto || (produtoId.HasValue && produtoId.Value != Guid.Empty),
                    nameof(ProdutoId),
                    "Item do tipo Produto exige um produto.")
                .IsGreaterThan(quantidade, 0, nameof(Quantidade), "A quantidade do item deve ser maior que zero."));
        }

        private void RecalcularTotais()
        {
            ValorSubtotal = Quantidade * ValorUnitario;
            ValorDesconto = ValorSubtotal * (TaxaDesconto / 100m);
            ValorTotal = ValorSubtotal - ValorDesconto;
        }
    }

    /// <summary>MAN-TRB — Vinculo financeiro/fiscal da OS. EF 11.6.</summary>
    public class OrdemServicoFinanceiroFiscal : EntidadeSaaSBase
    {
        public Guid OrdemServicoId { get; private set; }
        public ETipoVinculoFinanceiroOs TipoVinculo { get; private set; }
        public Guid ReferenciaId { get; private set; }
        public EStatusVinculoFinanceiroOs StatusVinculo { get; private set; } = EStatusVinculoFinanceiroOs.Pendente;
        public decimal? Valor { get; private set; }
        public DateTime DataVinculo { get; private set; }
        public string? Erro { get; private set; }

        protected OrdemServicoFinanceiroFiscal() { }

        public OrdemServicoFinanceiroFiscal(Guid ordemServicoId, ETipoVinculoFinanceiroOs tipoVinculo, Guid referenciaId, decimal? valor, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            OrdemServicoId = ordemServicoId;
            TipoVinculo = tipoVinculo;
            ReferenciaId = referenciaId;
            Valor = valor;
            StatusVinculo = EStatusVinculoFinanceiroOs.Pendente;
            DataVinculo = DateTime.UtcNow;

            AddNotifications(new Contract<OrdemServicoFinanceiroFiscal>()
                .Requires()
                .AreNotEquals(ordemServicoId, Guid.Empty, nameof(OrdemServicoId), "A OS e obrigatoria.")
                .AreNotEquals(referenciaId, Guid.Empty, nameof(ReferenciaId), "A referencia do vinculo e obrigatoria."));
        }

        public void Confirmar(string usuario)
        {
            StatusVinculo = EStatusVinculoFinanceiroOs.Confirmado;
            MarcarAlterado(usuario);
        }

        public void RegistrarFalha(string erro, string usuario)
        {
            StatusVinculo = EStatusVinculoFinanceiroOs.Falhou;
            Erro = erro;
            MarcarAlterado(usuario);
        }
    }
}
