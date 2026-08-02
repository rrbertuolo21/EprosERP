using System;
using System.Collections.Generic;
using System.Linq;

namespace Epros.Modules.DMS.Domain.Financiamento
{
    /// <summary>Tipo de mutuário para efeito de IOF (alíquota diária difere PF × PJ).</summary>
    public enum TipoMutuario
    {
        PessoaFisica = 0,
        PessoaJuridica = 1
    }

    /// <summary>Alíquotas de IOF de crédito vigentes num intervalo de datas.</summary>
    public sealed record VigenciaIof(
        DateTime Inicio,
        DateTime? Fim,
        decimal AliquotaDiariaPf,
        decimal AliquotaDiariaPj,
        decimal AliquotaAdicional);

    /// <summary>
    /// IOF de crédito — estrutura do Decreto 6.306/2007 (RIOF), art. 7º e §§ (skill Receita C).
    ///   IOF = IOF_diário + IOF_adicional
    ///   IOF_diário    = PV · aliq_dia · min(prazo_dias, 365)   (teto de 365 dias)
    ///   IOF_adicional = PV · aliq_adicional                    (fixo, independe do prazo)
    ///
    /// ⚠️ // valida-contador — as ALÍQUOTAS mudam por decreto (majoração em 2025) e são
    /// [VIGÊNCIA] + [REQUER VALIDAÇÃO antes do go-live]. NÃO hardcode no cálculo: a tabela abaixo é
    /// um DEFAULT parametrizado por data de vigência + tipo de mutuário, montado com os valores de
    /// REFERÊNCIA pré-2025 citados na skill. Confirmar a redação vigente na data da operação com
    /// contador / instituição financeira antes do go-live. O overlay do cliente (negocio-siser)
    /// substitui/estende estas vigências.
    /// </summary>
    public static class TabelaIof
    {
        // valida-contador: alíquotas de REFERÊNCIA pré-2025 (skill Receita C). Estrutura correta,
        // valores a confirmar na vigência. Uma vigência 2025+ deve ser ACRESCENTADA aqui após
        // validação da norma (majoração por decreto) — não editar a linha histórica.
        private static readonly IReadOnlyList<VigenciaIof> Vigencias = new List<VigenciaIof>
        {
            new VigenciaIof(
                Inicio: new DateTime(2008, 1, 1),
                Fim: null,
                AliquotaDiariaPf: 0.000082m,   // 0,0082% ao dia — PF  [VIGÊNCIA] valida-contador
                AliquotaDiariaPj: 0.000041m,   // 0,0041% ao dia — PJ  [VIGÊNCIA] valida-contador
                AliquotaAdicional: 0.0038m)    // 0,38% fixo           [VIGÊNCIA] valida-contador
        };

        /// <summary>Vigência aplicável a uma data de operação (a mais recente cujo início &lt;= data).</summary>
        public static VigenciaIof ObterVigencia(DateTime dataOperacao)
        {
            var vigente = Vigencias
                .Where(v => v.Inicio <= dataOperacao && (v.Fim == null || v.Fim >= dataOperacao))
                .OrderByDescending(v => v.Inicio)
                .FirstOrDefault();

            return vigente ?? Vigencias.OrderByDescending(v => v.Inicio).First();
        }

        /// <summary>
        /// IOF total da operação = diário (com teto de 365 dias) + adicional fixo.
        /// // valida-contador: para operações PARCELADAS a base do diário é apurada por parcela de
        /// principal × dias até o vencimento (§ 1º), com o total limitado ao equivalente a 365 dias
        /// sobre o principal. Aqui aplicamos a forma simplificada sobre o principal com teto de 365
        /// dias — suficiente para o CET; refino por parcela é decisão de negócio/contador.
        /// </summary>
        public static decimal Calcular(decimal principal, int prazoDias, TipoMutuario mutuario, DateTime dataOperacao)
        {
            if (principal <= 0m || prazoDias <= 0)
                return 0m;

            var v = ObterVigencia(dataOperacao);
            var aliquotaDia = mutuario == TipoMutuario.PessoaFisica ? v.AliquotaDiariaPf : v.AliquotaDiariaPj;

            var diasComTeto = Math.Min(prazoDias, 365);
            var iofDiario = principal * aliquotaDia * diasComTeto;
            var iofAdicional = principal * v.AliquotaAdicional;

            return Math.Round(iofDiario + iofAdicional, 2, MidpointRounding.AwayFromZero);
        }
    }
}
