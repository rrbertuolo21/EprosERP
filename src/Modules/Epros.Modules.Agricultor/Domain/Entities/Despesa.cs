using System;
using Epros.Modules.Agricultor.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Agricultor.Domain.Entities
{
    /// <summary>
    /// Despesa da atividade rural (Agris: expenses), vinculada a talhão/safra (vínculos NOVOS — AGR-D07).
    /// AGR-D04: carrega a "natureza fiscal LCDPR" que a transforma em lançamento Q100 quando escriturada:
    /// COD_IMOVEL (do 0040), COD_CONTA ({000,999}∪0050), TIPO_DOC (1..6), TIPO_LANC ({1,2,3}), ID_PARTIC.
    /// A DERIVAÇÃO concreta (plano de contas Siser → COD_CONTA/TIPO_LANC) é overlay do cliente. // valida-contador.
    /// </summary>
    public class Despesa : EntidadeSaaSBase
    {
        public Guid PropriedadeId { get; private set; }
        public Guid? TalhaoId { get; private set; }
        public Guid? SafraId { get; private set; }
        public Guid? CategoriaId { get; private set; }
        public Guid? FornecedorId { get; private set; }
        public DateTime Data { get; private set; }
        public decimal Valor { get; private set; }
        public string? Referencia { get; private set; }  // NUM_DOC candidato

        // Natureza fiscal LCDPR (AGR-D04) — preenchida pela derivação do overlay. // valida-contador.
        public int? CodImovelLcdpr { get; private set; }
        public int? CodContaLcdpr { get; private set; }
        public ETipoDocumentoLcdpr? TipoDocLcdpr { get; private set; }
        public ETipoLancamentoLcdpr? TipoLancLcdpr { get; private set; }
        public string? IdParticLcdpr { get; private set; }

        protected Despesa() { }

        public Despesa(Guid propriedadeId, Guid? talhaoId, Guid? safraId, Guid? categoriaId, Guid? fornecedorId,
            DateTime data, decimal valor, string? referencia, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PropriedadeId = propriedadeId;
            TalhaoId = talhaoId;
            SafraId = safraId;
            CategoriaId = categoriaId;
            FornecedorId = fornecedorId;
            Data = data;
            Valor = valor;
            Referencia = referencia;
            Validar();
        }

        /// <summary>
        /// Aterra a natureza fiscal LCDPR na despesa (AGR-D04). Os códigos vêm da derivação do overlay
        /// Siser (plano de contas → COD_CONTA/TIPO_LANC) — este método só valida o DOMÍNIO da norma,
        /// não inventa a classificação. // valida-contador.
        /// </summary>
        public void ClassificarLcdpr(int codImovel, int codConta, ETipoDocumentoLcdpr tipoDoc,
            ETipoLancamentoLcdpr tipoLanc, string? idPartic, string usuario)
        {
            CodImovelLcdpr = codImovel;
            CodContaLcdpr = codConta;
            TipoDocLcdpr = tipoDoc;
            TipoLancLcdpr = tipoLanc;
            IdParticLcdpr = idPartic;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            if (PropriedadeId == Guid.Empty)
                AddNotification(nameof(PropriedadeId), "A despesa exige uma propriedade. [Origem: Despesa]");
            AddNotifications(new Contract<Despesa>().Requires()
                .IsGreaterThan(Valor, 0m, nameof(Valor), "O valor da despesa deve ser maior que zero. [Origem: Despesa]"));
        }
    }

    /// <summary>
    /// Receita de produção (venda) — entidade NOVA (não existe no Agris). Vira Q100 do tipo Receita.
    /// AGR-D12 (bloqueante): receita NUNCA em COD_IMOVEL "000" — sempre pelo imóvel da NF. // valida-contador.
    /// </summary>
    public class ReceitaProducao : EntidadeSaaSBase
    {
        public Guid PropriedadeId { get; private set; }
        public Guid? TalhaoId { get; private set; }
        public Guid? SafraId { get; private set; }
        public DateTime Data { get; private set; }
        public decimal Quantidade { get; private set; }
        public decimal Preco { get; private set; }
        public decimal Valor => Math.Round(Quantidade * Preco, 2);
        public string? Comprador { get; private set; }
        public string? NumNf { get; private set; }

        // Natureza fiscal LCDPR (AGR-D04).
        public int? CodImovelLcdpr { get; private set; }
        public int? CodContaLcdpr { get; private set; }
        public string? IdParticLcdpr { get; private set; }

        protected ReceitaProducao() { }

        public ReceitaProducao(Guid propriedadeId, Guid? talhaoId, Guid? safraId, DateTime data,
            decimal quantidade, decimal preco, string? comprador, string? numNf, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PropriedadeId = propriedadeId;
            TalhaoId = talhaoId;
            SafraId = safraId;
            Data = data;
            Quantidade = quantidade;
            Preco = preco;
            Comprador = comprador;
            NumNf = numNf;
            Validar();
        }

        /// <summary>
        /// Aterra a natureza fiscal da receita. Bloqueia COD_IMOVEL "000" (AGR-D12). // valida-contador.
        /// </summary>
        public void ClassificarLcdpr(int codImovel, int codConta, string? idPartic, string usuario)
        {
            if (codImovel == 0)
            {
                AddNotification(nameof(CodImovelLcdpr),
                    "Receita não pode ser lançada em COD_IMOVEL '000' — sempre pelo imóvel da NF. [Origem: ReceitaProducao] (AGR-D12, bloqueante)");
                return;
            }
            CodImovelLcdpr = codImovel;
            CodContaLcdpr = codConta;
            IdParticLcdpr = idPartic;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            if (PropriedadeId == Guid.Empty)
                AddNotification(nameof(PropriedadeId), "A receita exige uma propriedade. [Origem: ReceitaProducao]");
            AddNotifications(new Contract<ReceitaProducao>().Requires()
                .IsGreaterThan(Quantidade, 0m, nameof(Quantidade), "A quantidade deve ser maior que zero. [Origem: ReceitaProducao]")
                .IsGreaterThan(Preco, 0m, nameof(Preco), "O preço deve ser maior que zero. [Origem: ReceitaProducao]"));
        }
    }
}
