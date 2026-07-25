using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Risco
{
    /// <summary>
    /// Estagio kanban do risco/issue. Origem: EF PRJ-RSK 11.2 (prj_risco_estagio / BugStage).
    /// RN-RSK-008 (nome obrigatorio, max 255), RN-RSK-009 (cor obrigatoria, max 7).
    /// </summary>
    public class EstagioRisco : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string Cor { get; private set; } = string.Empty;
        public bool Completo { get; private set; }
        public int Ordem { get; private set; }
        public Guid? CriadorId { get; private set; }

        protected EstagioRisco() { } // EF Core

        public EstagioRisco(
            string nome,
            string cor,
            bool completo,
            int ordem,
            Guid? criadorId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<EstagioRisco>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do estagio e obrigatorio. [Origem: EstagioRisco]")
                .IsLowerOrEqualsThan(nome?.Length ?? 0, 255, nameof(Nome), "O nome do estagio deve ter no maximo 255 caracteres. [Origem: EstagioRisco]")
                .IsNotNullOrEmpty(cor, nameof(Cor), "A cor do estagio e obrigatoria. [Origem: EstagioRisco]")
                .IsLowerOrEqualsThan(cor?.Length ?? 0, 7, nameof(Cor), "A cor do estagio deve ter no maximo 7 caracteres. [Origem: EstagioRisco]"));

            Nome = nome ?? string.Empty;
            Cor = cor ?? string.Empty;
            Completo = completo;
            Ordem = ordem;
            CriadorId = criadorId;
        }

        public void Alterar(string nome, string cor, bool completo, int ordem, string usuario)
        {
            if (string.IsNullOrWhiteSpace(nome) || nome.Length > 255)
            {
                AddNotification(nameof(Nome), "O nome do estagio e obrigatorio e limitado a 255 caracteres. [Origem: EstagioRisco]");
                return;
            }
            if (string.IsNullOrWhiteSpace(cor) || cor.Length > 7)
            {
                AddNotification(nameof(Cor), "A cor do estagio e obrigatoria e limitada a 7 caracteres. [Origem: EstagioRisco]");
                return;
            }
            Nome = nome;
            Cor = cor;
            Completo = completo;
            Ordem = ordem;
            MarcarAlterado(usuario);
        }
    }
}
