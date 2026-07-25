using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Definicao
{
    /// <summary>
    /// Arquivo/anexo do projeto. Origem: EF PRJ-DEF 11.4 (prj_def_projeto_arquivo).
    /// RN-DEF-020: arquivo deve estar vinculado ao projeto. GED e contrato pendente (ArquivoId).
    /// </summary>
    public class ProjetoArquivo : EntidadeSaaSBase
    {
        public Guid ProjetoId { get; private set; }
        public string NomeArquivo { get; private set; } = string.Empty;
        public string? CaminhoArquivo { get; private set; }
        public Guid? ArquivoId { get; private set; }

        protected ProjetoArquivo() { } // EF Core

        public ProjetoArquivo(Guid projetoId, string nomeArquivo, string? caminhoArquivo, Guid? arquivoId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ProjetoArquivo>()
                .Requires()
                .AreNotEquals(projetoId, Guid.Empty, nameof(ProjetoId), "O projeto e obrigatorio. [Origem: ProjetoArquivo]")
                .IsNotNullOrEmpty(nomeArquivo, nameof(NomeArquivo), "O nome do arquivo e obrigatorio. [Origem: ProjetoArquivo]"));

            ProjetoId = projetoId;
            NomeArquivo = nomeArquivo;
            CaminhoArquivo = caminhoArquivo;
            ArquivoId = arquivoId;
        }
    }
}
