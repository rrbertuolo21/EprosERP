using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PNT). Fidelidade campo a campo.</summary>
    public partial class PntParametro : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public string MesAno { get; private set; } = string.Empty;
        public int? DiaInicialApuracao { get; private set; }
        public TimeSpan HoraNoturnaInicio { get; private set; }
        public TimeSpan HoraNoturnaFim { get; private set; }
        public string PeriodoMinimoInterjornada { get; private set; } = string.Empty;
        public decimal? PercentualHeDiurna { get; private set; }
        public decimal? PercentualHeNoturna { get; private set; }
        public string DuracaoHoraNoturna { get; private set; } = string.Empty;
        public string TratamentoHoraMais { get; private set; } = string.Empty;
        public string TratamentoHoraMenos { get; private set; } = string.Empty;

        protected PntParametro() { } // EF Core

        public PntParametro(
            Guid empresaId,
            string mesAno,
            int? diaInicialApuracao,
            TimeSpan horaNoturnaInicio,
            TimeSpan horaNoturnaFim,
            string periodoMinimoInterjornada,
            decimal? percentualHeDiurna,
            decimal? percentualHeNoturna,
            string duracaoHoraNoturna,
            string tratamentoHoraMais,
            string tratamentoHoraMenos,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            MesAno = mesAno;
            DiaInicialApuracao = diaInicialApuracao;
            HoraNoturnaInicio = horaNoturnaInicio;
            HoraNoturnaFim = horaNoturnaFim;
            PeriodoMinimoInterjornada = periodoMinimoInterjornada;
            PercentualHeDiurna = percentualHeDiurna;
            PercentualHeNoturna = percentualHeNoturna;
            DuracaoHoraNoturna = duracaoHoraNoturna;
            TratamentoHoraMais = tratamentoHoraMais;
            TratamentoHoraMenos = tratamentoHoraMenos;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PntParametro>().Requires();
            contract.AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "O campo EmpresaId e obrigatorio.");
            contract.IsNotNullOrEmpty(MesAno, nameof(MesAno), "O campo MesAno e obrigatorio.");
            contract.IsNotNullOrEmpty(PeriodoMinimoInterjornada, nameof(PeriodoMinimoInterjornada), "O campo PeriodoMinimoInterjornada e obrigatorio.");
            contract.IsNotNullOrEmpty(DuracaoHoraNoturna, nameof(DuracaoHoraNoturna), "O campo DuracaoHoraNoturna e obrigatorio.");
            contract.IsNotNullOrEmpty(TratamentoHoraMais, nameof(TratamentoHoraMais), "O campo TratamentoHoraMais e obrigatorio.");
            contract.IsNotNullOrEmpty(TratamentoHoraMenos, nameof(TratamentoHoraMenos), "O campo TratamentoHoraMenos e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
