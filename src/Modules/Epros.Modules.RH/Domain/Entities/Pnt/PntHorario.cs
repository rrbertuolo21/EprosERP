using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PNT). Fidelidade campo a campo.</summary>
    public partial class PntHorario : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public string Tipo { get; private set; } = string.Empty;
        public string Codigo { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public string TipoTrabalho { get; private set; } = string.Empty;
        public string CargaHoraria { get; private set; } = string.Empty;
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

        protected PntHorario() { } // EF Core

        public PntHorario(
            Guid empresaId,
            string tipo,
            string codigo,
            string nome,
            string tipoTrabalho,
            string cargaHoraria,
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
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            Tipo = tipo;
            Codigo = codigo;
            Nome = nome;
            TipoTrabalho = tipoTrabalho;
            CargaHoraria = cargaHoraria;
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
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PntHorario>().Requires();
            contract.AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "O campo EmpresaId e obrigatorio.");
            contract.IsNotNullOrEmpty(Tipo, nameof(Tipo), "O campo Tipo e obrigatorio.");
            contract.IsNotNullOrEmpty(Codigo, nameof(Codigo), "O campo Codigo e obrigatorio.");
            contract.IsNotNullOrEmpty(Nome, nameof(Nome), "O campo Nome e obrigatorio.");
            contract.IsNotNullOrEmpty(TipoTrabalho, nameof(TipoTrabalho), "O campo TipoTrabalho e obrigatorio.");
            contract.IsNotNullOrEmpty(CargaHoraria, nameof(CargaHoraria), "O campo CargaHoraria e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
