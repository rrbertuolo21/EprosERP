using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Domain.Entities;

namespace Epros.Modules.Fiscal.Infrastructure.Services
{
    /// <summary>
    /// Aplica os impostos reais (ICMS/ST/FCP/IPI/PIS/COFINS) em cada item de um
    /// <see cref="DocumentoFiscal"/> usando o motor legado via <see cref="ICalculoFiscalService"/>.
    ///
    /// Ponto ÚNICO de cálculo por item, reutilizado tanto na emissão manual
    /// (<c>EmitirDocumentoFiscalCommandHandler</c>) quanto na geração automática a partir da
    /// venda faturada (<c>VendaFaturadaFiscalHandler</c>), garantindo que o XML nunca saia com
    /// defaults zerados quando há emitente configurado.
    ///
    /// O contexto do emitente (UF/regime) é resolvido pelo <see cref="IEmitenteFiscalProvider"/>;
    /// na sua ausência, usa regime normal e UF neutra (degradação honesta — mantém apenas o ICMS
    /// informado e não interrompe o fluxo).
    /// </summary>
    public class CalculadoraImpostosDocumentoFiscal
    {
        private readonly ICalculoFiscalService _calculoService;
        private readonly IEmitenteFiscalProvider _emitenteProvider;

        public CalculadoraImpostosDocumentoFiscal(
            ICalculoFiscalService calculoService,
            IEmitenteFiscalProvider emitenteProvider)
        {
            _calculoService = calculoService;
            _emitenteProvider = emitenteProvider;
        }

        /// <summary>
        /// Calcula e aplica os impostos de todos os itens do documento. Retorna <c>true</c> se ao menos
        /// um item foi calculado com sucesso. Nunca lança: falha de item é ignorada (mantém ICMS informado).
        /// </summary>
        public bool CalcularEAplicar(DocumentoFiscal documento)
        {
            var contexto = _emitenteProvider.ObterContexto(documento);
            var emitente = contexto?.Emitente;

            var regime = emitente?.RegimeTributario ?? 3; // 3 = Regime Normal (default seguro)
            var ufOrigem = emitente?.Uf ?? string.Empty;
            var modelo = documento.Modelo == "65" ? 65 : 55;
            var regimeSimples = regime == 1 || regime == 4;

            var algumCalculado = false;

            foreach (var item in documento.Itens)
            {
                var req = new CalculoFiscalRequest
                {
                    RegimeTributario = regime,
                    ModeloDocumento = modelo,
                    Cfop = item.Cfop,
                    UfOrigem = ufOrigem,
                    // Sem UF de destino no documento; assume operação interna (mesma UF do emitente).
                    UfDestino = ufOrigem,
                    CodigoProduto = item.Sku,
                    NomeProduto = item.NomeProduto,
                    Ncm = item.Ncm,
                    Origem = item.Origem,
                    Quantidade = item.Quantidade,
                    ValorUnitario = item.ValorUnitario,
                    CstIcms = regimeSimples ? null : item.Cst,
                    Csosn = regimeSimples ? item.Cst : null,
                    AliquotaIcms = item.AliquotaIcms,
                    CstPisCofins = item.CstPisCofins,
                    AliquotaPis = item.AliquotaPis,
                    AliquotaCofins = item.AliquotaCofins,
                    CstIpi = item.CstIpi,
                    AliquotaIpi = item.AliquotaIpi,
                    CstIbsCbs = item.CstIbsCbs,
                    CClassTrib = item.CClassTrib
                };

                var resultado = _calculoService.Calcular(req);
                if (!resultado.Sucesso)
                    continue; // mantém o ICMS informado no construtor; não interrompe a emissão

                item.AplicarCalculoFiscal(
                    baseCalculoIcms: resultado.Icms.BaseCalculo,
                    aliquotaIcms: resultado.Icms.Aliquota,
                    valorIcms: resultado.Icms.Valor,
                    valorIcmsSt: resultado.Icms.ValorSt,
                    valorFcp: resultado.Icms.ValorFcp,
                    valorIpi: resultado.Ipi.Valor,
                    valorPis: resultado.Pis.Valor,
                    valorCofins: resultado.Cofins.Valor);

                algumCalculado = true;
            }

            return algumCalculado;
        }
    }
}
