using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Ativo fixo / bem patrimonial (EF FIN-AFX §10.3 afx_ativo — porte fiel campo a campo).
    /// Submódulo de evolução — sobe desabilitado (ABAC nega por padrão). FKs de cadastro cross-module por Guid.
    /// </summary>
    public class AtivoFixo : EntidadeSaaSBase
    {
        // Classificação e relações (cross-module por Guid FK)
        public Guid? TipoAquisicaoId { get; private set; }
        public Guid? EstadoConservacaoId { get; private set; }
        public Guid? GrupoBemId { get; private set; }
        public Guid? SetorId { get; private set; }
        public Guid? FornecedorId { get; private set; }
        public Guid? ColaboradorId { get; private set; }

        // Identificação patrimonial
        public string? NumeroNb { get; private set; }
        public string? Nome { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public string? NumeroSerie { get; private set; }
        public string? Funcao { get; private set; }
        public string? NumeroNotaFiscal { get; private set; }
        public string? ChaveNfe { get; private set; }

        // Datas
        public DateTime DataAquisicao { get; private set; }
        public DateTime? DataAceite { get; private set; }
        public DateTime? DataCadastro { get; private set; }
        public DateTime? DataContabilizado { get; private set; }
        public DateTime? DataVistoria { get; private set; }
        public DateTime? DataMarcacao { get; private set; }
        public DateTime? DataBaixa { get; private set; }
        public DateTime? VencimentoGarantia { get; private set; }

        // Valores
        public decimal? ValorOriginal { get; private set; }
        public decimal ValorCompra { get; private set; }
        public decimal? ValorAtualizado { get; private set; }
        public decimal? ValorBaixa { get; private set; }

        // Depreciação
        public bool Deprecia { get; private set; }
        public string? MetodoDepreciacao { get; private set; }
        public ETipoDepreciacaoAtivo? TipoDepreciacao { get; private set; }
        public DateTime? InicioDepreciacao { get; private set; }
        public DateTime? UltimaDepreciacao { get; private set; }
        public decimal? TaxaAnual { get; private set; }
        public decimal? TaxaMensal { get; private set; }
        public decimal? TaxaAcelerada { get; private set; }
        public decimal? TaxaIncentivada { get; private set; }

        public EStatusAtivoFixo Status { get; private set; } = EStatusAtivoFixo.Ativo;

        protected AtivoFixo() { } // EF Core

        public AtivoFixo(
            string descricao, decimal valorCompra, DateTime dataAquisicao,
            Guid? grupoBemId, Guid? tipoAquisicaoId, Guid? estadoConservacaoId, Guid? setorId, Guid? fornecedorId, Guid? colaboradorId,
            string? numeroNb, string? nome, string? numeroSerie, string? funcao, string? numeroNotaFiscal, string? chaveNfe,
            decimal? valorOriginal, DateTime? vencimentoGarantia,
            bool deprecia, ETipoDepreciacaoAtivo? tipoDepreciacao, decimal? taxaAnual, decimal? taxaMensal,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Descricao = descricao;
            ValorCompra = valorCompra;
            ValorAtualizado = valorOriginal ?? valorCompra;
            ValorOriginal = valorOriginal;
            DataAquisicao = dataAquisicao;
            DataCadastro = DateTime.UtcNow;
            GrupoBemId = grupoBemId;
            TipoAquisicaoId = tipoAquisicaoId;
            EstadoConservacaoId = estadoConservacaoId;
            SetorId = setorId;
            FornecedorId = fornecedorId;
            ColaboradorId = colaboradorId;
            NumeroNb = numeroNb;
            Nome = nome;
            NumeroSerie = numeroSerie;
            Funcao = funcao;
            NumeroNotaFiscal = numeroNotaFiscal;
            ChaveNfe = chaveNfe;
            VencimentoGarantia = vencimentoGarantia;
            Deprecia = deprecia;
            TipoDepreciacao = tipoDepreciacao;
            TaxaAnual = taxaAnual;
            TaxaMensal = taxaMensal;
            Status = EStatusAtivoFixo.Ativo;
            Validar();
        }

        public void Alterar(string descricao, decimal valorCompra, DateTime dataAquisicao,
            Guid? grupoBemId, Guid? setorId, Guid? fornecedorId, Guid? colaboradorId,
            string? numeroNb, string? nome, string? numeroSerie, string? funcao,
            bool deprecia, ETipoDepreciacaoAtivo? tipoDepreciacao, decimal? taxaAnual, decimal? taxaMensal, string usuario)
        {
            if (Status == EStatusAtivoFixo.Baixado)
            {
                AddNotification(nameof(Status), "Ativo baixado não pode ser alterado.");
                return;
            }
            Descricao = descricao;
            ValorCompra = valorCompra;
            DataAquisicao = dataAquisicao;
            GrupoBemId = grupoBemId;
            SetorId = setorId;
            FornecedorId = fornecedorId;
            ColaboradorId = colaboradorId;
            NumeroNb = numeroNb;
            Nome = nome;
            NumeroSerie = numeroSerie;
            Funcao = funcao;
            Deprecia = deprecia;
            TipoDepreciacao = tipoDepreciacao;
            TaxaAnual = taxaAnual;
            TaxaMensal = taxaMensal;
            MarcarAlterado(usuario);
            Validar();
        }

        /// <summary>Registra depreciação do mês (atualiza valor contábil e última competência).</summary>
        public void AplicarDepreciacao(decimal valorDepreciacao, DateTime competencia, string usuario)
        {
            if (Status != EStatusAtivoFixo.Ativo)
            {
                AddNotification(nameof(Status), "Somente ativo em situação Ativo pode ser depreciado.");
                return;
            }
            ValorAtualizado = (ValorAtualizado ?? ValorCompra) - valorDepreciacao;
            UltimaDepreciacao = competencia;
            if (ValorAtualizado <= 0)
            {
                ValorAtualizado = 0;
                Status = EStatusAtivoFixo.TotalmenteDepreciado;
            }
            MarcarAlterado(usuario);
        }

        /// <summary>Baixa/alienação do ativo (EF §7.4).</summary>
        public void Baixar(DateTime dataBaixa, decimal? valorBaixa, string usuario)
        {
            if (Status == EStatusAtivoFixo.Baixado)
            {
                AddNotification(nameof(Status), "Ativo já está baixado.");
                return;
            }
            Status = EStatusAtivoFixo.Baixado;
            DataBaixa = dataBaixa;
            ValorBaixa = valorBaixa;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<AtivoFixo>()
                .Requires()
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descrição do ativo é obrigatória [Origem: AtivoFixo]")
                .IsGreaterThan(ValorCompra, 0, nameof(ValorCompra), "O valor de compra deve ser maior que zero [Origem: AtivoFixo]")
                .IsGreaterThan(DataAquisicao, DateTime.MinValue, nameof(DataAquisicao), "A data de aquisição é obrigatória [Origem: AtivoFixo]")
            );
        }
    }
}
