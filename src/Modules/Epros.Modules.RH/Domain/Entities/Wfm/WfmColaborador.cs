using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-WFM, tabela rh_wfm_colaborador). Fidelidade campo a campo.</summary>
    public partial class WfmColaborador : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }
        public Guid? UsuarioId { get; private set; }
        public string Matricula { get; private set; } = string.Empty;
        public string? CodigoColaborador { get; private set; }
        public string? FotoReferencia { get; private set; }
        public DateTime? DataCadastro { get; private set; }
        public DateTime? DataAdmissao { get; private set; }
        public DateTime? DataNascimento { get; private set; }
        public string? Genero { get; private set; }
        public string? TipoEmprego { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public Guid CargoId { get; private set; }
        public Guid DepartamentoId { get; private set; }
        public Guid? FilialId { get; private set; }
        public Guid? TurnoId { get; private set; }
        public Guid? ContaContabilId { get; private set; }
        public decimal? SalarioBase { get; private set; }
        public string? TipoRemuneracao { get; private set; }
        public decimal? ValorHora { get; private set; }
        public decimal? HorasPorDia { get; private set; }
        public decimal? DiasPorSemana { get; private set; }
        public string? Observacao { get; private set; }
        public bool Ativo { get; private set; }

        protected WfmColaborador() { } // EF Core

        public WfmColaborador(
            Guid pessoaId,
            Guid? usuarioId,
            string matricula,
            string? codigoColaborador,
            string? fotoReferencia,
            DateTime? dataCadastro,
            DateTime? dataAdmissao,
            DateTime? dataNascimento,
            string? genero,
            string? tipoEmprego,
            string status,
            Guid cargoId,
            Guid departamentoId,
            Guid? filialId,
            Guid? turnoId,
            Guid? contaContabilId,
            decimal? salarioBase,
            string? tipoRemuneracao,
            decimal? valorHora,
            decimal? horasPorDia,
            decimal? diasPorSemana,
            string? observacao,
            bool ativo,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            PessoaId = pessoaId;
            UsuarioId = usuarioId;
            Matricula = matricula;
            CodigoColaborador = codigoColaborador;
            FotoReferencia = fotoReferencia;
            DataCadastro = dataCadastro;
            DataAdmissao = dataAdmissao;
            DataNascimento = dataNascimento;
            Genero = genero;
            TipoEmprego = tipoEmprego;
            Status = status;
            CargoId = cargoId;
            DepartamentoId = departamentoId;
            FilialId = filialId;
            TurnoId = turnoId;
            ContaContabilId = contaContabilId;
            SalarioBase = salarioBase;
            TipoRemuneracao = tipoRemuneracao;
            ValorHora = valorHora;
            HorasPorDia = horasPorDia;
            DiasPorSemana = diasPorSemana;
            Observacao = observacao;
            Ativo = ativo;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<WfmColaborador>().Requires();
            contract.AreNotEquals(PessoaId, Guid.Empty, nameof(PessoaId), "O campo PessoaId e obrigatorio.");
            contract.IsNotNullOrEmpty(Matricula, nameof(Matricula), "O campo Matricula e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            contract.AreNotEquals(CargoId, Guid.Empty, nameof(CargoId), "O campo CargoId e obrigatorio.");
            contract.AreNotEquals(DepartamentoId, Guid.Empty, nameof(DepartamentoId), "O campo DepartamentoId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
