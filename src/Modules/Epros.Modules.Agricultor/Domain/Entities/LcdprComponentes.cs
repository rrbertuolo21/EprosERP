using System;
using System.Linq;
using Epros.Modules.Agricultor.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Agricultor.Domain.Entities
{
    /// <summary>
    /// Registro 0030 — dados cadastrais do declarante (1:1 com a escrituração). Domínios UF/COD_MUN/CEP
    /// validados contra a tabela SPED (AGR-D16).
    /// </summary>
    public class LcdprDadosCadastrais : EntidadeSaaSBase
    {
        public Guid EscrituracaoId { get; private set; }
        public string? Endereco { get; private set; }
        public string? Uf { get; private set; }
        public string? CodMunicipio { get; private set; }
        public string? Cep { get; private set; }
        public string? Email { get; private set; }

        protected LcdprDadosCadastrais() { }

        public LcdprDadosCadastrais(string? endereco, string? uf, string? codMunicipio, string? cep, string? email,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            Endereco = endereco; Uf = uf; CodMunicipio = codMunicipio; Cep = cep; Email = email;
        }

        public void Vincular(Guid escrituracaoId) => EscrituracaoId = escrituracaoId;
    }

    /// <summary>
    /// Registro 0040 — imóvel rural da escrituração. COD_IMOVEL é o sequencial usado por Q100.
    /// CAD_ITR/CAFIR N8 c/ DV; CAEPF N14 condicional; TIPO_EXPLORACAO 1..6; PARTICIPACAO N5,2 (AGR-D17).
    /// </summary>
    public class LcdprImovel : EntidadeSaaSBase
    {
        public Guid EscrituracaoId { get; private set; }
        public int CodImovel { get; private set; }
        public string? CadItrCafir { get; private set; }
        public string? Caepf { get; private set; }
        public string NomeImovel { get; private set; } = string.Empty;
        // AGR-D17 / leiaute 1.3 — bloco de endereço do 0040 (ENDERECO/NUM/COMPL/BAIRRO/CEP).
        // Sem estes campos o 0040 saía com 14 colunas onde o leiaute exige 17 (registro rejeitado pelo validador).
        public string? Endereco { get; private set; }
        public string? Num { get; private set; }
        public string? Compl { get; private set; }
        public string? Bairro { get; private set; }
        public string? Cep { get; private set; }
        public string? Uf { get; private set; }
        public string? CodMunicipio { get; private set; }
        public ETipoExploracao TipoExploracao { get; private set; }
        public decimal Participacao { get; private set; }

        private readonly System.Collections.Generic.List<LcdprTerceiro> _terceiros = new();
        public System.Collections.Generic.IReadOnlyCollection<LcdprTerceiro> Terceiros => _terceiros.AsReadOnly();

        protected LcdprImovel() { }

        public LcdprImovel(int codImovel, string nomeImovel, string? cadItrCafir, string? caepf,
            string? uf, string? codMunicipio, ETipoExploracao tipoExploracao, decimal participacao,
            string tenantId, string criadoPor,
            string? endereco = null, string? num = null, string? compl = null, string? bairro = null, string? cep = null)
            : base(tenantId, criadoPor)
        {
            CodImovel = codImovel;
            NomeImovel = nomeImovel;
            CadItrCafir = cadItrCafir;
            Caepf = caepf;
            Endereco = endereco;
            Num = num;
            Compl = compl;
            Bairro = bairro;
            Cep = cep;
            Uf = uf;
            CodMunicipio = codMunicipio;
            TipoExploracao = tipoExploracao;
            Participacao = participacao;
            Validar();
        }

        /// <summary>Define/atualiza o bloco de endereço do 0040 (ENDERECO/NUM/COMPL/BAIRRO/CEP).</summary>
        public void DefinirEndereco(string? endereco, string? num, string? compl, string? bairro, string? cep)
        {
            Endereco = endereco;
            Num = num;
            Compl = compl;
            Bairro = bairro;
            Cep = cep;
        }

        public void Vincular(Guid escrituracaoId) => EscrituracaoId = escrituracaoId;

        public void AdicionarTerceiro(LcdprTerceiro terceiro)
        {
            terceiro.Vincular(Id, CodImovel);
            _terceiros.Add(terceiro);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<LcdprImovel>().Requires()
                .IsNotNullOrEmpty(NomeImovel, nameof(NomeImovel), "0040: NOME_IMOVEL é obrigatório. [Origem: LcdprImovel]")
                .IsGreaterOrEqualsThan(CodImovel, 1, nameof(CodImovel), "0040: COD_IMOVEL deve ser >= 1. [Origem: LcdprImovel]"));
            if (!string.IsNullOrWhiteSpace(CadItrCafir) && (CadItrCafir!.Length > 8 || !CadItrCafir.All(char.IsDigit)))
                AddNotification(nameof(CadItrCafir), "0040: CAD_ITR/CAFIR N8 c/ DV (numérico, <=8). [Origem: LcdprImovel] (AGR-D17)");
        }
    }

    /// <summary>
    /// Registro 0045 — condômino/parceiro do imóvel (condicional). A soma das participações dos 0045
    /// mais a do titular deve fechar 100% quando exigido (validação bloqueante do gerador). // valida-contador.
    /// </summary>
    public class LcdprTerceiro : EntidadeSaaSBase
    {
        public Guid ImovelId { get; private set; }
        public int CodImovel { get; private set; }
        public int TipoContraparte { get; private set; }
        public string IdContraparte { get; private set; } = string.Empty; // CPF/CNPJ
        public string NomeContraparte { get; private set; } = string.Empty;
        public decimal PercContraparte { get; private set; } // N5,2

        protected LcdprTerceiro() { }

        public LcdprTerceiro(int tipoContraparte, string idContraparte, string nomeContraparte, decimal percContraparte,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            TipoContraparte = tipoContraparte;
            IdContraparte = idContraparte;
            NomeContraparte = nomeContraparte;
            PercContraparte = percContraparte;
            AddNotifications(new Contract<LcdprTerceiro>().Requires()
                .IsNotNullOrEmpty(idContraparte, nameof(IdContraparte), "0045: ID_CONTRAPARTE é obrigatório. [Origem: LcdprTerceiro]"));
        }

        public void Vincular(Guid imovelId, int codImovel) { ImovelId = imovelId; CodImovel = codImovel; }
    }

    /// <summary>
    /// Registro 0050 — conta bancária/caixa. NUM_CONTA N16 com DV; AGENCIA N4 sem DV; BANCO = cód. de
    /// compensação Bacen (AGR-D17). COD_CONTA é o sequencial referenciado por Q100.
    /// </summary>
    public class LcdprConta : EntidadeSaaSBase
    {
        public Guid EscrituracaoId { get; private set; }
        public int CodConta { get; private set; }
        public int? Banco { get; private set; }
        public int? Agencia { get; private set; }  // N4 sem DV
        public string? NumConta { get; private set; } // N16 com DV

        protected LcdprConta() { }

        public LcdprConta(int codConta, int? banco, int? agencia, string? numConta, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CodConta = codConta;
            Banco = banco;
            Agencia = agencia;
            NumConta = numConta;
            Validar();
        }

        public void Vincular(Guid escrituracaoId) => EscrituracaoId = escrituracaoId;

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<LcdprConta>().Requires()
                .IsGreaterOrEqualsThan(CodConta, 1, nameof(CodConta), "0050: COD_CONTA deve ser >= 1. [Origem: LcdprConta]"));
            if (Agencia.HasValue && Agencia.Value.ToString().Length > 4)
                AddNotification(nameof(Agencia), "0050: AGENCIA N4 sem DV (<=4 dígitos). [Origem: LcdprConta] (AGR-D17)");
            if (!string.IsNullOrWhiteSpace(NumConta) && NumConta!.Replace("-", "").Length > 16)
                AddNotification(nameof(NumConta), "0050: NUM_CONTA N16 com DV (<=16 dígitos). [Origem: LcdprConta] (AGR-D17)");
        }
    }

    /// <summary>
    /// Registro Q100 — lançamento do livro caixa. Regras de domínio (RN-05/06/07 da EF; RN38/39 da skill):
    ///  - COD_IMOVEL deve existir no 0040 (validado no gerador contra a coleção de imóveis);
    ///  - COD_CONTA ∈ {000 espécie, 999 numerário em trânsito} ∪ contas do 0050;
    ///  - TIPO_LANC ∈ {1,2,3} (AGR-D08 — "código 6" bloqueado até RFB);
    ///  - receita (VL_ENTRADA) nunca em COD_IMOVEL "000" (AGR-D12, bloqueante).
    /// SLD_FIN/NAT_SLD_FIN são DERIVADOS na geração (saldo corrido), não entrada manual.
    /// </summary>
    public class LcdprLancamento : EntidadeSaaSBase
    {
        public const int CodContaEspecie = 0;   // "000"
        public const int CodContaTransito = 999; // "999"

        public Guid EscrituracaoId { get; private set; }
        public int CodImovel { get; private set; }
        public int CodConta { get; private set; }
        public DateTime Data { get; private set; }
        public ETipoDocumentoLcdpr TipoDoc { get; private set; }
        public string? NumDoc { get; private set; }
        public string? Historico { get; private set; }
        public string? IdPartic { get; private set; }
        public ETipoLancamentoLcdpr TipoLanc { get; private set; }
        public decimal VlEntrada { get; private set; }
        public decimal VlSaida { get; private set; }

        protected LcdprLancamento() { }

        public LcdprLancamento(int codImovel, int codConta, DateTime data, ETipoDocumentoLcdpr tipoDoc,
            string? numDoc, string? historico, string? idPartic, ETipoLancamentoLcdpr tipoLanc,
            decimal vlEntrada, decimal vlSaida, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            CodImovel = codImovel;
            CodConta = codConta;
            Data = data;
            TipoDoc = tipoDoc;
            NumDoc = numDoc;
            Historico = historico;
            IdPartic = idPartic;
            TipoLanc = tipoLanc;
            VlEntrada = vlEntrada;
            VlSaida = vlSaida;
            Validar();
        }

        public void Vincular(Guid escrituracaoId) => EscrituracaoId = escrituracaoId;

        public void Validar()
        {
            Clear();

            if (!Enum.IsDefined(typeof(ETipoLancamentoLcdpr), TipoLanc))
                AddNotification(nameof(TipoLanc), "Q100: TIPO_LANC fora do domínio {1,2,3}. [Origem: LcdprLancamento] (AGR-D08)");

            if (VlEntrada < 0 || VlSaida < 0)
                AddNotification(nameof(VlEntrada), "Q100: valores não podem ser negativos. [Origem: LcdprLancamento]");

            if (VlEntrada == 0 && VlSaida == 0)
                AddNotification(nameof(VlEntrada), "Q100: informe VL_ENTRADA ou VL_SAIDA. [Origem: LcdprLancamento]");

            if (VlEntrada > 0 && VlSaida > 0)
                AddNotification(nameof(VlEntrada), "Q100: um lançamento não pode ter entrada e saída simultâneas. [Origem: LcdprLancamento]");

            // AGR-D12 (bloqueante): receita (entrada) nunca em COD_IMOVEL "000".
            if (VlEntrada > 0 && CodImovel == 0)
                AddNotification(nameof(CodImovel), "Q100: receita não pode usar COD_IMOVEL '000'. [Origem: LcdprLancamento] (AGR-D12, bloqueante)");
        }
    }
}
