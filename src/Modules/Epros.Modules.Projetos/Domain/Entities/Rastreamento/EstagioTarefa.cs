using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Rastreamento
{
    /// <summary>
    /// Coluna/fase do quadro operacional. Origem: EF PRJ-RST 4.3 (prj_rst_estagio).
    /// </summary>
    public class EstagioTarefa : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string? Cor { get; private set; }
        public bool IndicadorConclusao { get; private set; }
        public int Ordem { get; private set; }

        protected EstagioTarefa() { } // EF Core

        public EstagioTarefa(string nome, string? cor, bool indicadorConclusao, int ordem, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<EstagioTarefa>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do estagio e obrigatorio. [Origem: EstagioTarefa]"));

            Nome = nome;
            Cor = cor;
            IndicadorConclusao = indicadorConclusao;
            Ordem = ordem;
        }
    }
}
