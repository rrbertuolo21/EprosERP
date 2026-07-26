using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PNT). Fidelidade campo a campo.</summary>
    public partial class PntFechamentoJornada : EntidadeSaaSBase
    {
        public Guid ClassificacaoJornadaId { get; private set; }
        public Guid ColaboradorId { get; private set; }
        public DateTime? DataFechamento { get; private set; }
        public string DiaSemana { get; private set; } = string.Empty;
        public string CodigoHorario { get; private set; } = string.Empty;
        public string CargaHorariaEsperada { get; private set; } = string.Empty;
        public string CargaHorariaDiurna { get; private set; } = string.Empty;
        public string CargaHorariaNoturna { get; private set; } = string.Empty;
        public string CargaHorariaTotal { get; private set; } = string.Empty;
        public TimeSpan Entrada01 { get; private set; }
        public TimeSpan Saida01 { get; private set; }
        public TimeSpan Entrada02 { get; private set; }
        public TimeSpan Saida02 { get; private set; }
        public TimeSpan Entrada03 { get; private set; }
        public TimeSpan Saida03 { get; private set; }
        public TimeSpan Entrada04 { get; private set; }
        public TimeSpan Saida04 { get; private set; }
        public TimeSpan Entrada05 { get; private set; }
        public TimeSpan Saida05 { get; private set; }
        public TimeSpan HoraInicioJornada { get; private set; }
        public TimeSpan HoraFimJornada { get; private set; }
        public string HoraExtra01 { get; private set; } = string.Empty;
        public decimal? PercentualHoraExtra01 { get; private set; }
        public string ModalidadeHoraExtra01 { get; private set; } = string.Empty;
        public string HoraExtra02 { get; private set; } = string.Empty;
        public decimal? PercentualHoraExtra02 { get; private set; }
        public string ModalidadeHoraExtra02 { get; private set; } = string.Empty;
        public string HoraExtra03 { get; private set; } = string.Empty;
        public decimal? PercentualHoraExtra03 { get; private set; }
        public string ModalidadeHoraExtra03 { get; private set; } = string.Empty;
        public string HoraExtra04 { get; private set; } = string.Empty;
        public decimal? PercentualHoraExtra04 { get; private set; }
        public string ModalidadeHoraExtra04 { get; private set; } = string.Empty;
        public string FaltaAtraso { get; private set; } = string.Empty;
        public string Compensar { get; private set; } = string.Empty;
        public string BancoHoras { get; private set; } = string.Empty;
        public string Observacao { get; private set; } = string.Empty;

        protected PntFechamentoJornada() { } // EF Core

        public PntFechamentoJornada(
            Guid classificacaoJornadaId,
            Guid colaboradorId,
            DateTime? dataFechamento,
            string diaSemana,
            string codigoHorario,
            string cargaHorariaEsperada,
            string cargaHorariaDiurna,
            string cargaHorariaNoturna,
            string cargaHorariaTotal,
            TimeSpan entrada01,
            TimeSpan saida01,
            TimeSpan entrada02,
            TimeSpan saida02,
            TimeSpan entrada03,
            TimeSpan saida03,
            TimeSpan entrada04,
            TimeSpan saida04,
            TimeSpan entrada05,
            TimeSpan saida05,
            TimeSpan horaInicioJornada,
            TimeSpan horaFimJornada,
            string horaExtra01,
            decimal? percentualHoraExtra01,
            string modalidadeHoraExtra01,
            string horaExtra02,
            decimal? percentualHoraExtra02,
            string modalidadeHoraExtra02,
            string horaExtra03,
            decimal? percentualHoraExtra03,
            string modalidadeHoraExtra03,
            string horaExtra04,
            decimal? percentualHoraExtra04,
            string modalidadeHoraExtra04,
            string faltaAtraso,
            string compensar,
            string bancoHoras,
            string observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ClassificacaoJornadaId = classificacaoJornadaId;
            ColaboradorId = colaboradorId;
            DataFechamento = dataFechamento;
            DiaSemana = diaSemana;
            CodigoHorario = codigoHorario;
            CargaHorariaEsperada = cargaHorariaEsperada;
            CargaHorariaDiurna = cargaHorariaDiurna;
            CargaHorariaNoturna = cargaHorariaNoturna;
            CargaHorariaTotal = cargaHorariaTotal;
            Entrada01 = entrada01;
            Saida01 = saida01;
            Entrada02 = entrada02;
            Saida02 = saida02;
            Entrada03 = entrada03;
            Saida03 = saida03;
            Entrada04 = entrada04;
            Saida04 = saida04;
            Entrada05 = entrada05;
            Saida05 = saida05;
            HoraInicioJornada = horaInicioJornada;
            HoraFimJornada = horaFimJornada;
            HoraExtra01 = horaExtra01;
            PercentualHoraExtra01 = percentualHoraExtra01;
            ModalidadeHoraExtra01 = modalidadeHoraExtra01;
            HoraExtra02 = horaExtra02;
            PercentualHoraExtra02 = percentualHoraExtra02;
            ModalidadeHoraExtra02 = modalidadeHoraExtra02;
            HoraExtra03 = horaExtra03;
            PercentualHoraExtra03 = percentualHoraExtra03;
            ModalidadeHoraExtra03 = modalidadeHoraExtra03;
            HoraExtra04 = horaExtra04;
            PercentualHoraExtra04 = percentualHoraExtra04;
            ModalidadeHoraExtra04 = modalidadeHoraExtra04;
            FaltaAtraso = faltaAtraso;
            Compensar = compensar;
            BancoHoras = bancoHoras;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PntFechamentoJornada>().Requires();
            contract.AreNotEquals(ClassificacaoJornadaId, Guid.Empty, nameof(ClassificacaoJornadaId), "O campo ClassificacaoJornadaId e obrigatorio.");
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(DiaSemana, nameof(DiaSemana), "O campo DiaSemana e obrigatorio.");
            contract.IsNotNullOrEmpty(CodigoHorario, nameof(CodigoHorario), "O campo CodigoHorario e obrigatorio.");
            contract.IsNotNullOrEmpty(CargaHorariaEsperada, nameof(CargaHorariaEsperada), "O campo CargaHorariaEsperada e obrigatorio.");
            contract.IsNotNullOrEmpty(CargaHorariaDiurna, nameof(CargaHorariaDiurna), "O campo CargaHorariaDiurna e obrigatorio.");
            contract.IsNotNullOrEmpty(CargaHorariaNoturna, nameof(CargaHorariaNoturna), "O campo CargaHorariaNoturna e obrigatorio.");
            contract.IsNotNullOrEmpty(CargaHorariaTotal, nameof(CargaHorariaTotal), "O campo CargaHorariaTotal e obrigatorio.");
            contract.IsNotNullOrEmpty(HoraExtra01, nameof(HoraExtra01), "O campo HoraExtra01 e obrigatorio.");
            contract.IsNotNullOrEmpty(ModalidadeHoraExtra01, nameof(ModalidadeHoraExtra01), "O campo ModalidadeHoraExtra01 e obrigatorio.");
            contract.IsNotNullOrEmpty(HoraExtra02, nameof(HoraExtra02), "O campo HoraExtra02 e obrigatorio.");
            contract.IsNotNullOrEmpty(ModalidadeHoraExtra02, nameof(ModalidadeHoraExtra02), "O campo ModalidadeHoraExtra02 e obrigatorio.");
            contract.IsNotNullOrEmpty(HoraExtra03, nameof(HoraExtra03), "O campo HoraExtra03 e obrigatorio.");
            contract.IsNotNullOrEmpty(ModalidadeHoraExtra03, nameof(ModalidadeHoraExtra03), "O campo ModalidadeHoraExtra03 e obrigatorio.");
            contract.IsNotNullOrEmpty(HoraExtra04, nameof(HoraExtra04), "O campo HoraExtra04 e obrigatorio.");
            contract.IsNotNullOrEmpty(ModalidadeHoraExtra04, nameof(ModalidadeHoraExtra04), "O campo ModalidadeHoraExtra04 e obrigatorio.");
            contract.IsNotNullOrEmpty(FaltaAtraso, nameof(FaltaAtraso), "O campo FaltaAtraso e obrigatorio.");
            contract.IsNotNullOrEmpty(Compensar, nameof(Compensar), "O campo Compensar e obrigatorio.");
            contract.IsNotNullOrEmpty(BancoHoras, nameof(BancoHoras), "O campo BancoHoras e obrigatorio.");
            contract.IsNotNullOrEmpty(Observacao, nameof(Observacao), "O campo Observacao e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
