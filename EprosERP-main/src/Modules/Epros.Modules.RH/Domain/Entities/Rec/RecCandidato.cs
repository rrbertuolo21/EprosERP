using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-REC). Fidelidade campo a campo.</summary>
    public partial class RecCandidato : EntidadeSaaSBase
    {
        public string PrimeiroNome { get; private set; } = string.Empty;
        public string? Sobrenome { get; private set; }
        public string Email { get; private set; } = string.Empty;
        public string? Telefone { get; private set; }
        public string? Genero { get; private set; }
        public DateTime? DataNascimento { get; private set; }
        public string? Pais { get; private set; }
        public string? Estado { get; private set; }
        public string? Cidade { get; private set; }
        public string? EmpresaAtual { get; private set; }
        public string? CargoAtual { get; private set; }
        public decimal AnosExperiencia { get; private set; }
        public decimal? SalarioAtual { get; private set; }
        public decimal? SalarioPretendido { get; private set; }
        public string? AvisoPrevio { get; private set; }
        public string? Habilidades { get; private set; }
        public string? Escolaridade { get; private set; }
        public string? PortfolioUrl { get; private set; }
        public string? LinkedinUrl { get; private set; }
        public string? CaminhoFoto { get; private set; }
        public string? CaminhoCurriculo { get; private set; }
        public string? CaminhoCartaApresentacao { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public DateTime DataCandidatura { get; private set; }
        public string? RespostasCustomizadasJson { get; private set; }
        public string ProtocoloRastreio { get; private set; } = string.Empty;
        public Guid VagaId { get; private set; }
        public Guid? UsuarioId { get; private set; }
        public Guid FonteCandidatoId { get; private set; }
        public Guid CriadoPorUsuarioId { get; private set; }
        public Guid DonoFuncionalId { get; private set; }

        protected RecCandidato() { } // EF Core

        public RecCandidato(
            string primeiroNome,
            string? sobrenome,
            string email,
            string? telefone,
            string? genero,
            DateTime? dataNascimento,
            string? pais,
            string? estado,
            string? cidade,
            string? empresaAtual,
            string? cargoAtual,
            decimal anosExperiencia,
            decimal? salarioAtual,
            decimal? salarioPretendido,
            string? avisoPrevio,
            string? habilidades,
            string? escolaridade,
            string? portfolioUrl,
            string? linkedinUrl,
            string? caminhoFoto,
            string? caminhoCurriculo,
            string? caminhoCartaApresentacao,
            string status,
            DateTime dataCandidatura,
            string? respostasCustomizadasJson,
            string protocoloRastreio,
            Guid vagaId,
            Guid? usuarioId,
            Guid fonteCandidatoId,
            Guid criadoPorUsuarioId,
            Guid donoFuncionalId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PrimeiroNome = primeiroNome;
            Sobrenome = sobrenome;
            Email = email;
            Telefone = telefone;
            Genero = genero;
            DataNascimento = dataNascimento;
            Pais = pais;
            Estado = estado;
            Cidade = cidade;
            EmpresaAtual = empresaAtual;
            CargoAtual = cargoAtual;
            AnosExperiencia = anosExperiencia;
            SalarioAtual = salarioAtual;
            SalarioPretendido = salarioPretendido;
            AvisoPrevio = avisoPrevio;
            Habilidades = habilidades;
            Escolaridade = escolaridade;
            PortfolioUrl = portfolioUrl;
            LinkedinUrl = linkedinUrl;
            CaminhoFoto = caminhoFoto;
            CaminhoCurriculo = caminhoCurriculo;
            CaminhoCartaApresentacao = caminhoCartaApresentacao;
            Status = status;
            DataCandidatura = dataCandidatura;
            RespostasCustomizadasJson = respostasCustomizadasJson;
            ProtocoloRastreio = protocoloRastreio;
            VagaId = vagaId;
            UsuarioId = usuarioId;
            FonteCandidatoId = fonteCandidatoId;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            DonoFuncionalId = donoFuncionalId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<RecCandidato>().Requires();
            contract.IsNotNullOrEmpty(PrimeiroNome, nameof(PrimeiroNome), "O campo PrimeiroNome e obrigatorio.");
            contract.IsNotNullOrEmpty(Email, nameof(Email), "O campo Email e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            contract.IsNotNullOrEmpty(ProtocoloRastreio, nameof(ProtocoloRastreio), "O campo ProtocoloRastreio e obrigatorio.");
            contract.AreNotEquals(VagaId, Guid.Empty, nameof(VagaId), "O campo VagaId e obrigatorio.");
            contract.AreNotEquals(FonteCandidatoId, Guid.Empty, nameof(FonteCandidatoId), "O campo FonteCandidatoId e obrigatorio.");
            contract.AreNotEquals(CriadoPorUsuarioId, Guid.Empty, nameof(CriadoPorUsuarioId), "O campo CriadoPorUsuarioId e obrigatorio.");
            contract.AreNotEquals(DonoFuncionalId, Guid.Empty, nameof(DonoFuncionalId), "O campo DonoFuncionalId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
