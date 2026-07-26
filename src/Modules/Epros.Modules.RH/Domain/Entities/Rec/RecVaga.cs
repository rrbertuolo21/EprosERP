using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-REC). Fidelidade campo a campo.</summary>
    public partial class RecVaga : EntidadeSaaSBase
    {
        public string CodigoInterno { get; private set; } = string.Empty;
        public string CodigoPublico { get; private set; } = string.Empty;
        public string Titulo { get; private set; } = string.Empty;
        public int Posicoes { get; private set; }
        public int Prioridade { get; private set; }
        public decimal? ExperienciaMinima { get; private set; }
        public decimal? ExperienciaMaxima { get; private set; }
        public decimal? SalarioMinimo { get; private set; }
        public decimal? SalarioMaximo { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public string? Requisitos { get; private set; }
        public string Habilidades { get; private set; } = string.Empty;
        public string? Beneficios { get; private set; }
        public string? TermosCondicoes { get; private set; }
        public bool? ExibirTermosCondicoes { get; private set; }
        public DateTime? PrazoCandidatura { get; private set; }
        public bool Publicada { get; private set; }
        public DateTime? DataPublicacao { get; private set; }
        public bool? Destaque { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public string TipoCandidatura { get; private set; } = string.Empty;
        public string? UrlCandidatura { get; private set; }
        public string? CamposCandidatoJson { get; private set; }
        public string? VisibilidadeJson { get; private set; }
        public Guid FilialId { get; private set; }
        public Guid TipoVagaId { get; private set; }
        public Guid LocalVagaId { get; private set; }
        public string? PerguntasCustomizadasJson { get; private set; }
        public Guid CriadoPorUsuarioId { get; private set; }
        public Guid DonoFuncionalId { get; private set; }

        protected RecVaga() { } // EF Core

        public RecVaga(
            string codigoInterno,
            string codigoPublico,
            string titulo,
            int posicoes,
            int prioridade,
            decimal? experienciaMinima,
            decimal? experienciaMaxima,
            decimal? salarioMinimo,
            decimal? salarioMaximo,
            string descricao,
            string? requisitos,
            string habilidades,
            string? beneficios,
            string? termosCondicoes,
            bool? exibirTermosCondicoes,
            DateTime? prazoCandidatura,
            bool publicada,
            DateTime? dataPublicacao,
            bool? destaque,
            string status,
            string tipoCandidatura,
            string? urlCandidatura,
            string? camposCandidatoJson,
            string? visibilidadeJson,
            Guid filialId,
            Guid tipoVagaId,
            Guid localVagaId,
            string? perguntasCustomizadasJson,
            Guid criadoPorUsuarioId,
            Guid donoFuncionalId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            CodigoInterno = codigoInterno;
            CodigoPublico = codigoPublico;
            Titulo = titulo;
            Posicoes = posicoes;
            Prioridade = prioridade;
            ExperienciaMinima = experienciaMinima;
            ExperienciaMaxima = experienciaMaxima;
            SalarioMinimo = salarioMinimo;
            SalarioMaximo = salarioMaximo;
            Descricao = descricao;
            Requisitos = requisitos;
            Habilidades = habilidades;
            Beneficios = beneficios;
            TermosCondicoes = termosCondicoes;
            ExibirTermosCondicoes = exibirTermosCondicoes;
            PrazoCandidatura = prazoCandidatura;
            Publicada = publicada;
            DataPublicacao = dataPublicacao;
            Destaque = destaque;
            Status = status;
            TipoCandidatura = tipoCandidatura;
            UrlCandidatura = urlCandidatura;
            CamposCandidatoJson = camposCandidatoJson;
            VisibilidadeJson = visibilidadeJson;
            FilialId = filialId;
            TipoVagaId = tipoVagaId;
            LocalVagaId = localVagaId;
            PerguntasCustomizadasJson = perguntasCustomizadasJson;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            DonoFuncionalId = donoFuncionalId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<RecVaga>().Requires();
            contract.IsNotNullOrEmpty(CodigoInterno, nameof(CodigoInterno), "O campo CodigoInterno e obrigatorio.");
            contract.IsNotNullOrEmpty(CodigoPublico, nameof(CodigoPublico), "O campo CodigoPublico e obrigatorio.");
            contract.IsNotNullOrEmpty(Titulo, nameof(Titulo), "O campo Titulo e obrigatorio.");
            contract.IsNotNullOrEmpty(Descricao, nameof(Descricao), "O campo Descricao e obrigatorio.");
            contract.IsNotNullOrEmpty(Habilidades, nameof(Habilidades), "O campo Habilidades e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            contract.IsNotNullOrEmpty(TipoCandidatura, nameof(TipoCandidatura), "O campo TipoCandidatura e obrigatorio.");
            contract.AreNotEquals(FilialId, Guid.Empty, nameof(FilialId), "O campo FilialId e obrigatorio.");
            contract.AreNotEquals(TipoVagaId, Guid.Empty, nameof(TipoVagaId), "O campo TipoVagaId e obrigatorio.");
            contract.AreNotEquals(LocalVagaId, Guid.Empty, nameof(LocalVagaId), "O campo LocalVagaId e obrigatorio.");
            contract.AreNotEquals(CriadoPorUsuarioId, Guid.Empty, nameof(CriadoPorUsuarioId), "O campo CriadoPorUsuarioId e obrigatorio.");
            contract.AreNotEquals(DonoFuncionalId, Guid.Empty, nameof(DonoFuncionalId), "O campo DonoFuncionalId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
