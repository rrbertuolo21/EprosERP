using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class Servico : EntidadeSaaSBase
    {
        // FK para Empresa (outro módulo) — referenciada por Guid, sem navegação cruzada. Legado validava EmpresaId > 0.
        public Guid EmpresaId { get; private set; }
        public Guid UnidadeMedidaId { get; private set; }
        public Guid CodigoServicoSefazId { get; private set; }

        // Legado: SequenciaTenantId (somente exibição/UX).
        public long? SequenciaExibicao { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public decimal Valor { get; private set; }
        public string? InformacaoAdicional { get; private set; }
        public bool ServicoAtivo { get; private set; } = true;
        public int Cnae { get; private set; }
        public string CodigoNbs { get; private set; } = string.Empty;
        public bool IndicadorIss { get; private set; }
        public bool IndicadorIncentivo { get; private set; }
        public string? CstIbsCbs { get; private set; }
        public string? CClassTrib { get; private set; }
        public decimal AliquotaIss { get; private set; }
        public decimal AliquotaIssRetido { get; private set; }
        public decimal AliquotaIrrfRetido { get; private set; }
        public decimal AliquotaInss { get; private set; }
        public ECodigoSituacaoTributariaPisCofins CstPisCofins { get; private set; }
        public decimal AliquotaPis { get; private set; }
        public decimal AliquotaCofins { get; private set; }
        public bool CalcularRetencao { get; private set; }
        public EAnexoSimlesNacional AnexoSimplesNacional { get; private set; }

        // Navigation property for CodigoServicoSefaz (which is in the same module!)
        public CodigoServicoSefaz CodigoServicoSefaz { get; private set; } = null!;

        protected Servico() { } // EF Core

        public Servico(
            Guid empresaId,
            Guid unidadeMedidaId,
            Guid codigoServicoSefazId,
            string codigo,
            string descricao,
            decimal valor,
            string? informacaoAdicional,
            bool servicoAtivo,
            int cnae,
            string codigoNbs,
            bool indicadorIss,
            bool indicadorIncentivo,
            string? cstIbsCbs,
            string? cClassTrib,
            decimal aliquotaIss,
            decimal aliquotaIssRetido,
            decimal aliquotaIrrfRetido,
            decimal aliquotaInss,
            ECodigoSituacaoTributariaPisCofins cstPisCofins,
            decimal aliquotaPis,
            decimal aliquotaCofins,
            bool calcularRetencao,
            EAnexoSimlesNacional anexoSimplesNacional,
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            UnidadeMedidaId = unidadeMedidaId;
            CodigoServicoSefazId = codigoServicoSefazId;
            Codigo = codigo;
            Descricao = descricao;
            Valor = valor;
            InformacaoAdicional = informacaoAdicional;
            ServicoAtivo = servicoAtivo;
            Cnae = cnae;
            CodigoNbs = codigoNbs;
            IndicadorIss = indicadorIss;
            IndicadorIncentivo = indicadorIncentivo;
            CstIbsCbs = cstIbsCbs;
            CClassTrib = cClassTrib;
            AliquotaIss = aliquotaIss;
            AliquotaIssRetido = aliquotaIssRetido;
            AliquotaIrrfRetido = aliquotaIrrfRetido;
            AliquotaInss = aliquotaInss;
            CstPisCofins = cstPisCofins;
            AliquotaPis = aliquotaPis;
            AliquotaCofins = aliquotaCofins;
            CalcularRetencao = calcularRetencao;
            AnexoSimplesNacional = anexoSimplesNacional;

            Validar();
        }

        public void Alterar(
            Guid unidadeMedidaId,
            Guid codigoServicoSefazId,
            string codigo,
            string descricao,
            decimal valor,
            string? informacaoAdicional,
            bool servicoAtivo,
            int cnae,
            string codigoNbs,
            bool indicadorIss,
            bool indicadorIncentivo,
            string? cstIbsCbs,
            string? cClassTrib,
            decimal aliquotaIss,
            decimal aliquotaIssRetido,
            decimal aliquotaIrrfRetido,
            decimal aliquotaInss,
            ECodigoSituacaoTributariaPisCofins cstPisCofins,
            decimal aliquotaPis,
            decimal aliquotaCofins,
            bool calcularRetencao,
            EAnexoSimlesNacional anexoSimplesNacional,
            string alteradoPor)
        {
            UnidadeMedidaId = unidadeMedidaId;
            CodigoServicoSefazId = codigoServicoSefazId;
            Codigo = codigo;
            Descricao = descricao;
            Valor = valor;
            InformacaoAdicional = informacaoAdicional;
            ServicoAtivo = servicoAtivo;
            Cnae = cnae;
            CodigoNbs = codigoNbs;
            IndicadorIss = indicadorIss;
            IndicadorIncentivo = indicadorIncentivo;
            CstIbsCbs = cstIbsCbs;
            CClassTrib = cClassTrib;
            AliquotaIss = aliquotaIss;
            AliquotaIssRetido = aliquotaIssRetido;
            AliquotaIrrfRetido = aliquotaIrrfRetido;
            AliquotaInss = aliquotaInss;
            CstPisCofins = cstPisCofins;
            AliquotaPis = aliquotaPis;
            AliquotaCofins = aliquotaCofins;
            CalcularRetencao = calcularRetencao;
            AnexoSimplesNacional = anexoSimplesNacional;

            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Servico>()
                .Requires()
                .AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "O campo EmpresaId é obrigatório [Origem: Servico]")
                .AreNotEquals(UnidadeMedidaId, Guid.Empty, nameof(UnidadeMedidaId), "O ID da unidade de medida é obrigatório.")
                .AreNotEquals(CodigoServicoSefazId, Guid.Empty, nameof(CodigoServicoSefazId), "O ID do código de serviço SEFAZ é obrigatório.")
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O código é obrigatório.")
                .IsLowerOrEqualsThan(Codigo, 10, nameof(Codigo), "O código deve ter no máximo 10 caracteres.")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descrição é obrigatória.")
                .IsLowerOrEqualsThan(Descricao, 120, nameof(Descricao), "A descrição deve ter no máximo 120 caracteres.")
                .IsLowerOrEqualsThan(InformacaoAdicional ?? "", 120, nameof(InformacaoAdicional), "A informação adicional deve ter no máximo 120 caracteres.")
                .IsGreaterThan(Cnae, 0, nameof(Cnae), "O código CNAE é obrigatório.")
                .IsLowerOrEqualsThan(CodigoNbs, 9, nameof(CodigoNbs), "O código NBS deve ter no máximo 9 caracteres.")
                .IsLowerOrEqualsThan(CstIbsCbs ?? "", 5000, nameof(CstIbsCbs), "O CST IBS/CBS deve ter no máximo 5000 caracteres.")
                .IsLowerOrEqualsThan(CClassTrib ?? "", 5000, nameof(CClassTrib), "A Classificação Tributária deve ter no máximo 5000 caracteres.")
                .IsTrue(Enum.IsDefined(typeof(ECodigoSituacaoTributariaPisCofins), CstPisCofins), nameof(CstPisCofins), "CST PIS/COFINS inválido.")
                .IsTrue(Enum.IsDefined(typeof(EAnexoSimlesNacional), AnexoSimplesNacional), nameof(AnexoSimplesNacional), "Anexo do Simples Nacional inválido.")
            );
        }
    }
}
