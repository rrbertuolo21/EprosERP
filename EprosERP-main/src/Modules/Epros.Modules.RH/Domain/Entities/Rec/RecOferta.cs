using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-REC). Fidelidade campo a campo.</summary>
    public partial class RecOferta : EntidadeSaaSBase
    {
        public Guid CandidatoId { get; private set; }
        public Guid VagaId { get; private set; }
        public DateTime DataOferta { get; private set; }
        public string Cargo { get; private set; } = string.Empty;
        public Guid DepartamentoId { get; private set; }
        public decimal Salario { get; private set; }
        public string? Bonus { get; private set; }
        public string? ParticipacaoSocietaria { get; private set; }
        public string? Beneficios { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime? DataExpiracao { get; private set; }
        public string? CaminhoCartaOferta { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public DateTime? DataResposta { get; private set; }
        public string? MotivoRecusa { get; private set; }
        public bool ConvertidaColaborador { get; private set; }
        public Guid? ColaboradorId { get; private set; }
        public Guid? AprovadoPorUsuarioId { get; private set; }
        public string StatusAprovacao { get; private set; } = string.Empty;
        public Guid CriadoPorUsuarioId { get; private set; }
        public Guid DonoFuncionalId { get; private set; }

        protected RecOferta() { } // EF Core

        public RecOferta(
            Guid candidatoId,
            Guid vagaId,
            DateTime dataOferta,
            string cargo,
            Guid departamentoId,
            decimal salario,
            string? bonus,
            string? participacaoSocietaria,
            string? beneficios,
            DateTime dataInicio,
            DateTime? dataExpiracao,
            string? caminhoCartaOferta,
            string status,
            DateTime? dataResposta,
            string? motivoRecusa,
            bool convertidaColaborador,
            Guid? colaboradorId,
            Guid? aprovadoPorUsuarioId,
            string statusAprovacao,
            Guid criadoPorUsuarioId,
            Guid donoFuncionalId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            CandidatoId = candidatoId;
            VagaId = vagaId;
            DataOferta = dataOferta;
            Cargo = cargo;
            DepartamentoId = departamentoId;
            Salario = salario;
            Bonus = bonus;
            ParticipacaoSocietaria = participacaoSocietaria;
            Beneficios = beneficios;
            DataInicio = dataInicio;
            DataExpiracao = dataExpiracao;
            CaminhoCartaOferta = caminhoCartaOferta;
            Status = status;
            DataResposta = dataResposta;
            MotivoRecusa = motivoRecusa;
            ConvertidaColaborador = convertidaColaborador;
            ColaboradorId = colaboradorId;
            AprovadoPorUsuarioId = aprovadoPorUsuarioId;
            StatusAprovacao = statusAprovacao;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            DonoFuncionalId = donoFuncionalId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<RecOferta>().Requires();
            contract.AreNotEquals(CandidatoId, Guid.Empty, nameof(CandidatoId), "O campo CandidatoId e obrigatorio.");
            contract.AreNotEquals(VagaId, Guid.Empty, nameof(VagaId), "O campo VagaId e obrigatorio.");
            contract.IsNotNullOrEmpty(Cargo, nameof(Cargo), "O campo Cargo e obrigatorio.");
            contract.AreNotEquals(DepartamentoId, Guid.Empty, nameof(DepartamentoId), "O campo DepartamentoId e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            contract.IsNotNullOrEmpty(StatusAprovacao, nameof(StatusAprovacao), "O campo StatusAprovacao e obrigatorio.");
            contract.AreNotEquals(CriadoPorUsuarioId, Guid.Empty, nameof(CriadoPorUsuarioId), "O campo CriadoPorUsuarioId e obrigatorio.");
            contract.AreNotEquals(DonoFuncionalId, Guid.Empty, nameof(DonoFuncionalId), "O campo DonoFuncionalId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
