using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Fiscal.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class NcmTributacao : EntidadeSaaSBase
    {
        // Legado: SequenciaTenantId (somente exibição/UX).
        public long? SequenciaExibicao { get; private set; }
        public Guid TributarioGrupoId { get; private set; }
        public Guid? CodigoBeneficioFiscalId { get; private set; }
        public int CodRegra { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public int CfopNotaConsumidor { get; private set; }
        public int CfopNotaFiscal { get; private set; }               // saída 5102
        public int CfopNotaFiscalInterestadual { get; private set; }   // saída interestadual 6102
        public EOrigemMercadoria Origem { get; private set; }
        public ECodigoSituacaoOperacaoSimplesNacional CsosnNotaConsumidor { get; private set; }
        public ECodigoSituacaoTributariaIcms CstIcmsNotaConsumidor { get; private set; }
        public ECodigoSituacaoOperacaoSimplesNacional CsosnNotaFiscal { get; private set; }
        public ECodigoSituacaoTributariaIcms CstIcmsNotaFiscalInterna { get; private set; }
        public ECodigoSituacaoTributariaIcms CstIcmsNotaFiscalInterstadual { get; private set; }
        public ECodigoSituacaoTributariaPisCofins CstPis { get; private set; }
        public ECodigoSituacaoTributariaPisCofins CstCofins { get; private set; }
        public decimal ValorUnitFixoPis { get; private set; }
        public decimal ValorUnitFixoCofins { get; private set; }
        public decimal ValorAliquotaPis { get; private set; }
        public decimal ValorAliquotaCofins { get; private set; }
        public ECodigoSituacaoTributariaPisCofins CstPisCofinsEntrada { get; private set; }
        public ECodigoSituacaoTributariaIpi CstIpiSaida { get; private set; }
        public ECodigoSituacaoTributariaIpi CstIpiEntrada { get; private set; }
        public decimal ValorAliquotaIpi { get; private set; }
        public decimal ValorPercentualReducacaoBcIpi { get; private set; }
        public ETipoReducaoBaseDeCalculo TipoReducaoIpi { get; private set; }
        public EDestinoReducao DestinoReducaoIpi { get; private set; }
        public bool IpiEmbutido { get; private set; }
        public string? EnquadramentoIpi { get; private set; }
        public ECodigoValorFiscalIcms CodigoValorFiscalIcmsInterna { get; private set; }
        public ECodigoValorFiscalIcms CodigoValorFiscalcmsInterstadual { get; private set; }
        public decimal ValorAliquotaIcmsInterna { get; private set; }
        public decimal ValorPercentualReducacaoBcIcmsInterna { get; private set; }
        public ETipoReducaoBaseDeCalculo TipoReducaoIcmsInterna { get; private set; }
        public EDestinoReducao DestinoReducaoIcmsInterna { get; private set; }
        public decimal ValorAliquotaIcmsInterstadual { get; private set; } // DIFAL ???
        public decimal ValorPercentualReducacaoBcIcmsInterstadual { get; private set; }
        public ETipoReducaoBaseDeCalculo TipoReducaoIcmsInterstadual { get; private set; }
        public EDestinoReducao DestinoReducaoIcmsInterstadual { get; private set; }
        public string? CodigoBeneficioFiscalIcms { get; private set; }
        public int MotivoDesoneracaoIcms { get; private set; }
        public string? InformacoesComplementares { get; private set; }
        public string? InformacoesAdicionaisAoFisco { get; private set; }

        public string? CstIbsCbsNfe { get; private set; }
        public string? CClassTribNfe { get; private set; }
        public string? CstIbsCbsNfce { get; private set; }
        public string? CClassTribNfce { get; private set; }

        // Navegações intra-módulo
        public TributarioGrupo TributarioGrupo { get; private set; } = null!;
        public CodigoBeneficioFiscal? CodigoBeneficioFiscal { get; private set; }
        public ICollection<NcmConfiguracao> NcmConfiguracoes { get; private set; } = new List<NcmConfiguracao>();
        public ICollection<NcmTributacaoSt> NcmTributacaoSts { get; private set; } = new List<NcmTributacaoSt>();
        public ICollection<NcmTributacaoFundoCombatePobreza> NcmTributacaoFundoCombatePobrezas { get; private set; } = new List<NcmTributacaoFundoCombatePobreza>();

        // N:N com Empresa (outro módulo) — legado tinha ICollection<Empresa> Empresas.
        // Restaurado por coleção de vínculos com Guid EmpresaId (sem navegação cross-module).
        public ICollection<NcmTributacaoEmpresa> Empresas { get; private set; } = new List<NcmTributacaoEmpresa>();

        protected NcmTributacao() { } // EF Core

        public NcmTributacao(
            Guid tributarioGrupoId,
            Guid? codigoBeneficioFiscalId,
            int codRegra,
            string descricao,
            int cfopNotaConsumidor,
            int cfopNotaFiscal,
            int cfopNotaFiscalInterestadual,
            EOrigemMercadoria origem,
            ECodigoSituacaoOperacaoSimplesNacional csosnNotaConsumidor,
            ECodigoSituacaoTributariaIcms cstIcmsNotaConsumidor,
            ECodigoSituacaoOperacaoSimplesNacional csosnNotaFiscal,
            ECodigoSituacaoTributariaIcms cstIcmsNotaFiscalInterna,
            ECodigoSituacaoTributariaIcms cstIcmsNotaFiscalInterstadual,
            ECodigoSituacaoTributariaPisCofins cstPis,
            ECodigoSituacaoTributariaPisCofins cstCofins,
            decimal valorUnitFixoPis,
            decimal valorUnitFixoCofins,
            decimal valorAliquotaPis,
            decimal valorAliquotaCofins,
            ECodigoSituacaoTributariaPisCofins cstPisCofinsEntrada,
            ECodigoSituacaoTributariaIpi cstIpiSaida,
            ECodigoSituacaoTributariaIpi cstIpiEntrada,
            decimal valorAliquotaIpi,
            decimal valorPercentualReducacaoBcIpi,
            ETipoReducaoBaseDeCalculo tipoReducaoIpi,
            EDestinoReducao destinoReducaoIpi,
            bool ipiEmbutido,
            string? enquadramentoIpi,
            ECodigoValorFiscalIcms codigoValorFiscalIcmsInterna,
            ECodigoValorFiscalIcms codigoValorFiscalcmsInterstadual,
            decimal valorAliquotaIcmsInterna,
            decimal valorPercentualReducacaoBcIcmsInterna,
            ETipoReducaoBaseDeCalculo tipoReducaoIcmsInterna,
            EDestinoReducao destinoReducaoIcmsInterna,
            decimal valorAliquotaIcmsInterstadual,
            decimal valorPercentualReducacaoBcIcmsInterstadual,
            ETipoReducaoBaseDeCalculo tipoReducaoIcmsInterstadual,
            EDestinoReducao destinoReducaoIcmsInterstadual,
            string? codigoBeneficioFiscalIcms,
            int motivoDesoneracaoIcms,
            string? informacoesComplementares,
            string? informacoesAdicionaisAoFisco,
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            TributarioGrupoId = tributarioGrupoId;
            CodigoBeneficioFiscalId = codigoBeneficioFiscalId;
            CodRegra = codRegra;
            Descricao = descricao;
            CfopNotaConsumidor = cfopNotaConsumidor;
            CfopNotaFiscal = cfopNotaFiscal;
            CfopNotaFiscalInterestadual = cfopNotaFiscalInterestadual;
            Origem = origem;
            CsosnNotaConsumidor = csosnNotaConsumidor;
            CstIcmsNotaConsumidor = cstIcmsNotaConsumidor;
            CsosnNotaFiscal = csosnNotaFiscal;
            CstIcmsNotaFiscalInterna = cstIcmsNotaFiscalInterna;
            CstIcmsNotaFiscalInterstadual = cstIcmsNotaFiscalInterstadual;
            CstPis = cstPis;
            CstCofins = cstCofins;
            ValorUnitFixoPis = valorUnitFixoPis;
            ValorUnitFixoCofins = valorUnitFixoCofins;
            ValorAliquotaPis = valorAliquotaPis;
            ValorAliquotaCofins = valorAliquotaCofins;
            CstPisCofinsEntrada = cstPisCofinsEntrada;
            CstIpiSaida = cstIpiSaida;
            CstIpiEntrada = cstIpiEntrada;
            ValorAliquotaIpi = valorAliquotaIpi;
            ValorPercentualReducacaoBcIpi = valorPercentualReducacaoBcIpi;
            TipoReducaoIpi = tipoReducaoIpi;
            DestinoReducaoIpi = destinoReducaoIpi;
            IpiEmbutido = ipiEmbutido;
            EnquadramentoIpi = enquadramentoIpi;
            CodigoValorFiscalIcmsInterna = codigoValorFiscalIcmsInterna;
            CodigoValorFiscalcmsInterstadual = codigoValorFiscalcmsInterstadual;
            ValorAliquotaIcmsInterna = valorAliquotaIcmsInterna;
            ValorPercentualReducacaoBcIcmsInterna = valorPercentualReducacaoBcIcmsInterna;
            TipoReducaoIcmsInterna = tipoReducaoIcmsInterna;
            DestinoReducaoIcmsInterna = destinoReducaoIcmsInterna;
            ValorAliquotaIcmsInterstadual = valorAliquotaIcmsInterstadual;
            ValorPercentualReducacaoBcIcmsInterstadual = valorPercentualReducacaoBcIcmsInterstadual;
            TipoReducaoIcmsInterstadual = tipoReducaoIcmsInterstadual;
            DestinoReducaoIcmsInterstadual = destinoReducaoIcmsInterstadual;
            CodigoBeneficioFiscalIcms = codigoBeneficioFiscalIcms;
            MotivoDesoneracaoIcms = motivoDesoneracaoIcms;
            InformacoesComplementares = informacoesComplementares;
            InformacoesAdicionaisAoFisco = informacoesAdicionaisAoFisco;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<NcmTributacao>()
                .Requires()
                .AreNotEquals(TributarioGrupoId, Guid.Empty, nameof(TributarioGrupoId), "O campo TributarioGrupoId é obrigatório [Origem: NcmTributacao]")
                .IsGreaterThan(CodRegra, 0, nameof(CodRegra), "O campo CodRegra deve ser maior que Zero [Origem: NcmTributacao]")
                .IsLowerOrEqualsThan((Descricao ?? "").Length, 200, nameof(Descricao), "O campo Descricao deve ter no máximo 200 caracteres [Origem: NcmTributacao]")
                .IsTrue(Enum.IsDefined(typeof(EOrigemMercadoria), Origem), nameof(Origem), "Origem não consta na lista [Origem: NcmTributacao]")
                .IsLowerOrEqualsThan((EnquadramentoIpi ?? "").Length, 3, nameof(EnquadramentoIpi), "O campo EnquadramentoIpi deve ter no máximo 3 caracteres [Origem: NcmTributacao]")
                .IsTrue(Enum.IsDefined(typeof(ECodigoValorFiscalIcms), CodigoValorFiscalIcmsInterna), nameof(CodigoValorFiscalIcmsInterna), "CodigoValorFiscalIcmsInterna não consta na lista [Origem: NcmTributacao]")
                .IsTrue(Enum.IsDefined(typeof(ECodigoValorFiscalIcms), CodigoValorFiscalcmsInterstadual), nameof(CodigoValorFiscalcmsInterstadual), "CodigoValorFiscalcmsInterstadual não consta na lista [Origem: NcmTributacao]")
                .IsTrue(Enum.IsDefined(typeof(ETipoReducaoBaseDeCalculo), TipoReducaoIcmsInterstadual), nameof(TipoReducaoIcmsInterstadual), "TipoReducaoIcmsInterstadual não consta na lista [Origem: NcmTributacao]")
                .IsLowerOrEqualsThan((CodigoBeneficioFiscalIcms ?? "").Length, 10, nameof(CodigoBeneficioFiscalIcms), "O campo CodigoBeneficioFiscalIcms deve ter no máximo 10 caracteres [Origem: NcmTributacao]")
                .IsLowerOrEqualsThan((InformacoesComplementares ?? "").Length, 5000, nameof(InformacoesComplementares), "O campo InformacoesComplementares deve ter no máximo 5000 caracteres [Origem: NcmTributacao]")
                .IsLowerOrEqualsThan((InformacoesAdicionaisAoFisco ?? "").Length, 2000, nameof(InformacoesAdicionaisAoFisco), "O campo InformacoesAdicionaisAoFisco deve ter no máximo 2000 caracteres [Origem: NcmTributacao]")
            );

            if (CstIpiSaida.GetHashCode() < 50)
                AddNotification(nameof(CstIpiSaida), "Não pode ser selecionado um CST de Entrada no campo de CST de Saída");

            if (CstIpiEntrada.GetHashCode() > 49)
                AddNotification(nameof(CstIpiEntrada), "Não pode ser selecionado um CST de Saída no campo de CST Entrada");

            if (NcmTributacaoSts != null)
                foreach (var item in NcmTributacaoSts)
                    AddNotifications(item.Notifications);

            if (NcmTributacaoFundoCombatePobrezas != null)
                foreach (var item in NcmTributacaoFundoCombatePobrezas)
                    AddNotifications(item.Notifications);

            if (NcmConfiguracoes != null)
                foreach (var item in NcmConfiguracoes)
                    AddNotifications(item.Notifications);
        }

        public void Alterar(
            Guid tributarioGrupoId,
            Guid? codigoBeneficioFiscalId,
            int codRegra,
            string descricao,
            int cfopNotaConsumidor,
            int cfopNotaFiscal,
            int cfopNotaFiscalInterestadual,
            EOrigemMercadoria origem,
            ECodigoSituacaoOperacaoSimplesNacional csosnNotaConsumidor,
            ECodigoSituacaoTributariaIcms cstIcmsNotaConsumidor,
            ECodigoSituacaoOperacaoSimplesNacional csosnNotaFiscal,
            ECodigoSituacaoTributariaIcms cstIcmsNotaFiscalInterna,
            ECodigoSituacaoTributariaIcms cstIcmsNotaFiscalInterstadual,
            ECodigoSituacaoTributariaPisCofins cstPis,
            ECodigoSituacaoTributariaPisCofins cstCofins,
            decimal valorUnitFixoPis,
            decimal valorUnitFixoCofins,
            decimal valorAliquotaPis,
            decimal valorAliquotaCofins,
            ECodigoSituacaoTributariaPisCofins cstPisCofinsEntrada,
            ECodigoSituacaoTributariaIpi cstIpiSaida,
            ECodigoSituacaoTributariaIpi cstIpiEntrada,
            decimal valorAliquotaIpi,
            decimal valorPercentualReducacaoBcIpi,
            ETipoReducaoBaseDeCalculo tipoReducaoIpi,
            EDestinoReducao destinoReducaoIpi,
            bool ipiEmbutido,
            string? enquadramentoIpi,
            ECodigoValorFiscalIcms codigoValorFiscalIcmsInterna,
            ECodigoValorFiscalIcms codigoValorFiscalcmsInterstadual,
            decimal valorAliquotaIcmsInterna,
            decimal valorPercentualReducacaoBcIcmsInterna,
            ETipoReducaoBaseDeCalculo tipoReducaoIcmsInterna,
            EDestinoReducao destinoReducaoIcmsInterna,
            decimal valorAliquotaIcmsInterstadual,
            decimal valorPercentualReducacaoBcIcmsInterstadual,
            ETipoReducaoBaseDeCalculo tipoReducaoIcmsInterstadual,
            EDestinoReducao destinoReducaoIcmsInterstadual,
            string? codigoBeneficioFiscalIcms,
            int motivoDesoneracaoIcms,
            string? informacoesComplementares,
            string? informacoesAdicionaisAoFisco,
            string alteradoPor)
        {
            TributarioGrupoId = tributarioGrupoId;
            CodigoBeneficioFiscalId = codigoBeneficioFiscalId;
            CodRegra = codRegra;
            Descricao = descricao;
            CfopNotaConsumidor = cfopNotaConsumidor;
            CfopNotaFiscal = cfopNotaFiscal;
            CfopNotaFiscalInterestadual = cfopNotaFiscalInterestadual;
            Origem = origem;
            CsosnNotaConsumidor = csosnNotaConsumidor;
            CstIcmsNotaConsumidor = cstIcmsNotaConsumidor;
            CsosnNotaFiscal = csosnNotaFiscal;
            CstIcmsNotaFiscalInterna = cstIcmsNotaFiscalInterna;
            CstIcmsNotaFiscalInterstadual = cstIcmsNotaFiscalInterstadual;
            CstPis = cstPis;
            CstCofins = cstCofins;
            ValorUnitFixoPis = valorUnitFixoPis;
            ValorUnitFixoCofins = valorUnitFixoCofins;
            ValorAliquotaPis = valorAliquotaPis;
            ValorAliquotaCofins = valorAliquotaCofins;
            CstPisCofinsEntrada = cstPisCofinsEntrada;
            CstIpiSaida = cstIpiSaida;
            CstIpiEntrada = cstIpiEntrada;
            ValorAliquotaIpi = valorAliquotaIpi;
            ValorPercentualReducacaoBcIpi = valorPercentualReducacaoBcIpi;
            TipoReducaoIpi = tipoReducaoIpi;
            DestinoReducaoIpi = destinoReducaoIpi;
            IpiEmbutido = ipiEmbutido;
            EnquadramentoIpi = enquadramentoIpi;
            CodigoValorFiscalIcmsInterna = codigoValorFiscalIcmsInterna;
            CodigoValorFiscalcmsInterstadual = codigoValorFiscalcmsInterstadual;
            ValorAliquotaIcmsInterna = valorAliquotaIcmsInterna;
            ValorPercentualReducacaoBcIcmsInterna = valorPercentualReducacaoBcIcmsInterna;
            TipoReducaoIcmsInterna = tipoReducaoIcmsInterna;
            DestinoReducaoIcmsInterna = destinoReducaoIcmsInterna;
            ValorAliquotaIcmsInterstadual = valorAliquotaIcmsInterstadual;
            ValorPercentualReducacaoBcIcmsInterstadual = valorPercentualReducacaoBcIcmsInterstadual;
            TipoReducaoIcmsInterstadual = tipoReducaoIcmsInterstadual;
            DestinoReducaoIcmsInterstadual = destinoReducaoIcmsInterstadual;
            CodigoBeneficioFiscalIcms = codigoBeneficioFiscalIcms;
            MotivoDesoneracaoIcms = motivoDesoneracaoIcms;
            InformacoesComplementares = informacoesComplementares;
            InformacoesAdicionaisAoFisco = informacoesAdicionaisAoFisco;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        // Soft delete em cascata (equivalente ao override Deletar() do legado)
        public void DeletarCascata(string deletadoPor)
        {
            Deletar(deletadoPor);

            foreach (var n in NcmTributacaoSts)
                n.Deletar(deletadoPor);

            foreach (var n in NcmTributacaoFundoCombatePobrezas)
                n.Deletar(deletadoPor);

            foreach (var n in NcmConfiguracoes)
                n.Deletar(deletadoPor);
        }

        public void ValidarIbsCbs(string? cstIbsCbsNfe, string? cClassTribNfe, string? cstIbsCbsNfce, string? cClassTribNfce)
        {
            if (string.IsNullOrEmpty(cstIbsCbsNfe) && string.IsNullOrEmpty(cstIbsCbsNfce))
                AddNotification("cstIbsCbs", "Deve ser informado ao menos um IBS/CBS.");

            if (!string.IsNullOrEmpty(cstIbsCbsNfe))
            {
                if (string.IsNullOrEmpty(cClassTribNfe))
                    AddNotification("cClassTribNfe", "CClassTrib Nfe deve ser informado.");
            }

            if (!string.IsNullOrEmpty(cstIbsCbsNfce))
            {
                if (string.IsNullOrEmpty(cClassTribNfce))
                    AddNotification("cClassTribNfe", "CClassTrib Nfce deve ser informado.");
            }
        }

        public void AdicionarIbsCbs(string? cstIbsCbsNfe, string? cClassTribNfe, string? cstIbsCbsNfce, string? cClassTribNfce)
        {
            ValidarIbsCbs(cstIbsCbsNfe, cClassTribNfe, cstIbsCbsNfce, cClassTribNfce);

            CstIbsCbsNfe = cstIbsCbsNfe;
            CClassTribNfe = cClassTribNfe;
            CstIbsCbsNfce = cstIbsCbsNfce;
            CClassTribNfce = cClassTribNfce;
        }

        public void AlterarIbsCbs(string? cstIbsCbsNfe, string? cClassTribNfe, string? cstIbsCbsNfce, string? cClassTribNfce, string alteradoPor)
        {
            ValidarIbsCbs(cstIbsCbsNfe, cClassTribNfe, cstIbsCbsNfce, cClassTribNfce);

            CstIbsCbsNfe = cstIbsCbsNfe;
            CClassTribNfe = cClassTribNfe;
            CstIbsCbsNfce = cstIbsCbsNfce;
            CClassTribNfce = cClassTribNfce;
            MarcarAlterado(alteradoPor);
        }

        public void AdicionarNcmConfiguracao(Guid ncmId, string criadoPor)
        {
            var ncmConfiguracoes = new NcmConfiguracao(ncmId, Id, TenantId, criadoPor);
            NcmConfiguracoes.Add(ncmConfiguracoes);

            if (ncmConfiguracoes.Notifications.Any())
                AddNotifications(ncmConfiguracoes.Notifications);
        }

        public void AlterarNcmConfiguracao(Guid ncmConfiguracaoId, Guid ncmId, string alteradoPor)
        {
            var localizado = NcmConfiguracoes.FirstOrDefault(x => x.Id == ncmConfiguracaoId);

            if (localizado != null)
            {
                localizado.Alterar(ncmId, alteradoPor);

                if (localizado.Notifications.Any())
                    AddNotifications(localizado.Notifications);
            }
        }

        public void DeletarNcmConfiguracao(Guid ncmConfiguracaoId, string deletadoPor)
        {
            var localizado = NcmConfiguracoes.FirstOrDefault(x => x.Id == ncmConfiguracaoId);
            localizado?.Deletar(deletadoPor);
        }

        public void AdicionarNcmTributacaoSt(string uf, ETipoCalculo tipoCalculo, decimal valorAliquotaIcmsSt, decimal valorMva, decimal valorReducaoBcIcmsSt, int tipoReducaoIcmsSt, decimal valorUnitarioSt, decimal valorPercentualFcpSt, string criadoPor)
        {
            var ncmTributacaoSts = new NcmTributacaoSt(Id, uf, tipoCalculo, valorAliquotaIcmsSt, valorMva, valorReducaoBcIcmsSt, tipoReducaoIcmsSt, valorUnitarioSt, valorPercentualFcpSt, TenantId, criadoPor);
            NcmTributacaoSts.Add(ncmTributacaoSts);

            if (ncmTributacaoSts.Notifications.Any())
                AddNotifications(ncmTributacaoSts.Notifications);
        }

        public void AlterarNcmTributacaoSt(Guid ncmTributacaoStId, string uf, ETipoCalculo tipoCalculo, decimal valorAliquotaIcmsSt, decimal valorMva, decimal valorReducaoBcIcmsSt, int tipoReducaoIcmsSt, decimal valorUnitarioSt, decimal valorPercentualFcpSt, string alteradoPor)
        {
            var localizado = NcmTributacaoSts.FirstOrDefault(x => x.Id == ncmTributacaoStId);

            if (localizado != null)
            {
                localizado.Alterar(uf, tipoCalculo, valorAliquotaIcmsSt, valorMva, valorReducaoBcIcmsSt, tipoReducaoIcmsSt, valorUnitarioSt, valorPercentualFcpSt, alteradoPor);

                if (localizado.Notifications.Any())
                    AddNotifications(localizado.Notifications);
            }
        }

        public void DeletarNcmTributacaoSt(Guid ncmTributacaoStId, string deletadoPor)
        {
            var localizado = NcmTributacaoSts.FirstOrDefault(x => x.Id == ncmTributacaoStId);
            localizado?.Deletar(deletadoPor);
        }

        public void AdicionarNcmTributacaoFundoCombatePobreza(string uf, decimal valorPercentual, string criadoPor)
        {
            var ncmTributacaoFundoCombatePobrezas = new NcmTributacaoFundoCombatePobreza(Id, uf, valorPercentual, TenantId, criadoPor);
            NcmTributacaoFundoCombatePobrezas.Add(ncmTributacaoFundoCombatePobrezas);

            if (ncmTributacaoFundoCombatePobrezas.Notifications.Any())
                AddNotifications(ncmTributacaoFundoCombatePobrezas.Notifications);
        }

        public void AlterarNcmTributacaoFundoCombatePobreza(Guid ncmTributacaoFundoCombatePobrezaId, string uf, decimal valorPercentual, string alteradoPor)
        {
            var localizado = NcmTributacaoFundoCombatePobrezas.FirstOrDefault(x => x.Id == ncmTributacaoFundoCombatePobrezaId);

            if (localizado != null)
            {
                localizado.Alterar(uf, valorPercentual, alteradoPor);

                if (localizado.Notifications.Any())
                    AddNotifications(localizado.Notifications);
            }
        }

        public void DeletarNcmTributacaoFundoCombatePobreza(Guid ncmTributacaoFundoCombatePobrezaId, string deletadoPor)
        {
            var localizado = NcmTributacaoFundoCombatePobrezas.FirstOrDefault(x => x.Id == ncmTributacaoFundoCombatePobrezaId);
            localizado?.Deletar(deletadoPor);
        }

        public void AdicionarEmpresa(Guid empresaId, string criadoPor)
        {
            if (Empresas.Any(e => e.EmpresaId == empresaId && e.DeletadoEm == null))
                return;

            var vinculo = new NcmTributacaoEmpresa(Id, empresaId, TenantId, criadoPor);
            Empresas.Add(vinculo);

            if (vinculo.Notifications.Any())
                AddNotifications(vinculo.Notifications);
        }

        public void RemoverEmpresa(Guid empresaId, string deletadoPor)
        {
            var vinculo = Empresas.FirstOrDefault(e => e.EmpresaId == empresaId && e.DeletadoEm == null);
            vinculo?.Deletar(deletadoPor);
        }

        public string? ObterCstIbsCbsNfe() => string.IsNullOrEmpty(CstIbsCbsNfe) ? null : CstIbsCbsNfe.Split("-")[0];
        public string? ObterCstIbsCbsNfeDescricao() => string.IsNullOrEmpty(CstIbsCbsNfe) ? null : CstIbsCbsNfe.Split("-")[1];

        public string? ObterCClassTribNfe() => string.IsNullOrEmpty(CClassTribNfe) ? null : CClassTribNfe.Split("-")[0];
        public string? ObterCClassTribNfeDescricao() => string.IsNullOrEmpty(CClassTribNfe) ? null : CClassTribNfe.Split("-")[1];

        public string? ObterCstIbsCbsNfce() => string.IsNullOrEmpty(CstIbsCbsNfce) ? null : CstIbsCbsNfce.Split("-")[0];
        public string? ObterCstIbsCbsNfceDescricao() => string.IsNullOrEmpty(CstIbsCbsNfce) ? null : CstIbsCbsNfce.Split("-")[1];

        public string? ObterCClassTribNfce() => string.IsNullOrEmpty(CClassTribNfce) ? null : CClassTribNfce.Split("-")[0];
        public string? ObterCClassTribNfceDescricao() => string.IsNullOrEmpty(CClassTribNfce) ? null : CClassTribNfce.Split("-")[1];
    }
}
