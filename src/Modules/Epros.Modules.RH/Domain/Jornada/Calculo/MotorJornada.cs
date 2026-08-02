using System;

namespace Epros.Modules.RH.Domain.Jornada.Calculo
{
    // ============================================================================================
    //  MOTOR DE JORNADA (RH-PNT) — valor-hora, horas extras e adicional noturno.
    //  Skill: departamento-pessoal/jornada-clt — RN-02 (HE), RN-03 (adicional noturno).
    //  Percentuais aqui são MÍNIMOS LEGAIS; a CCT do cliente pode ampliar (overlay do projeto).
    //  // valida-contador — divisor de horas mensais (220/200/180) depende da jornada/CCT.
    // ============================================================================================

    public static class MotorJornada
    {
        public const decimal AdicionalHeMinimo = 0.50m;          // CF art. 7º XVI — mínimo 50%
        public const decimal AdicionalHeDomingoFeriado = 1.00m;  // Súmula 146 TST — 100%
        public const decimal AdicionalNoturnoUrbanoMinimo = 0.20m; // CLT art. 73 — mínimo 20%
        public const decimal MinutosHoraNoturnaUrbana = 52.5m;   // hora noturna reduzida = 52min30s

        /// <summary>Valor da hora normal = salário mensal ÷ divisor de horas (220 = 44h/semana).</summary>
        public static decimal ValorHora(decimal salarioMensal, decimal divisorHoras = 220m)
        {
            if (divisorHoras <= 0) throw new ArgumentOutOfRangeException(nameof(divisorHoras));
            if (salarioMensal <= 0) return 0m;
            return Math.Round(salarioMensal / divisorHoras, 6, MidpointRounding.AwayFromZero);
        }

        /// <summary>Valor total das horas extras = valorHora × (1 + adicional) × qtde.</summary>
        public static decimal HorasExtras(decimal valorHora, decimal qtdHoras, decimal adicional)
        {
            if (valorHora <= 0 || qtdHoras <= 0) return 0m;
            if (adicional < AdicionalHeMinimo) adicional = AdicionalHeMinimo; // nunca abaixo do mínimo legal
            return Math.Round(valorHora * (1m + adicional) * qtdHoras, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>Horas-relógio convertidas em horas noturnas reduzidas (urbano: 7 contam como 8).</summary>
        public static decimal HorasNoturnasReduzidas(decimal horasRelogio)
        {
            if (horasRelogio <= 0) return 0m;
            return Math.Round(horasRelogio * 60m / MinutosHoraNoturnaUrbana, 6, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Adicional noturno (a parcela extra, sobre as horas noturnas REDUZIDAS).
        /// Retorna só o adicional; o pagamento das horas noturnas em si é hora normal.
        /// </summary>
        public static decimal AdicionalNoturno(decimal valorHora, decimal horasNoturnasRelogio, decimal percentual = 0.20m)
        {
            if (valorHora <= 0 || horasNoturnasRelogio <= 0) return 0m;
            if (percentual < AdicionalNoturnoUrbanoMinimo) percentual = AdicionalNoturnoUrbanoMinimo;
            var reduzidas = HorasNoturnasReduzidas(horasNoturnasRelogio);
            return Math.Round(valorHora * reduzidas * percentual, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>Valor das horas noturnas com o adicional (hora reduzida × valorHora × (1 + percentual)).</summary>
        public static decimal PagamentoNoturno(decimal valorHora, decimal horasNoturnasRelogio, decimal percentual = 0.20m)
        {
            if (valorHora <= 0 || horasNoturnasRelogio <= 0) return 0m;
            if (percentual < AdicionalNoturnoUrbanoMinimo) percentual = AdicionalNoturnoUrbanoMinimo;
            var reduzidas = HorasNoturnasReduzidas(horasNoturnasRelogio);
            return Math.Round(valorHora * reduzidas * (1m + percentual), 2, MidpointRounding.AwayFromZero);
        }
    }
}
