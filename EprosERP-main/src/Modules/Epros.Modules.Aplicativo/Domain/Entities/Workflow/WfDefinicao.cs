using System;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Workflow
{
    /// <summary>
    /// wf_definicao — configuração reutilizável de workflow por tenant e módulo (estados, transições,
    /// permissões e eventos). Motor transversal do Epros para aprovações e ciclos de vida. [Origem: EF WORKFLOW 10.3]
    /// </summary>
    public class WfDefinicao : EntidadeSaaSBase
    {
        public string Modulo { get; private set; } = string.Empty;
        public string Entidade { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public int Versao { get; private set; }
        public EWfDefinicaoStatus Status { get; private set; }
        public Guid? CriadoPorUsuarioId { get; private set; }

        protected WfDefinicao() { } // EF Core

        public WfDefinicao(
            string modulo,
            string entidade,
            string nome,
            int versao,
            Guid? criadoPorUsuarioId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Modulo = modulo;
            Entidade = entidade;
            Nome = nome;
            Versao = versao <= 0 ? 1 : versao;
            Status = EWfDefinicaoStatus.Rascunho;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            Validar();
        }

        public void Alterar(string modulo, string entidade, string nome, int versao, string alteradoPor)
        {
            Modulo = modulo;
            Entidade = entidade;
            Nome = nome;
            Versao = versao <= 0 ? 1 : versao;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Ativar(string alteradoPor)
        {
            Status = EWfDefinicaoStatus.Ativo;
            MarcarAlterado(alteradoPor);
        }

        public void Inativar(string alteradoPor)
        {
            Status = EWfDefinicaoStatus.Inativo;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<WfDefinicao>()
                .Requires()
                .IsNotNullOrEmpty(Modulo, nameof(Modulo), "O módulo dono da definição é obrigatório [Origem: WfDefinicao]")
                .IsNotNullOrEmpty(Entidade, nameof(Entidade), "A entidade controlada é obrigatória [Origem: WfDefinicao]")
                .IsNotNullOrEmpty(Nome, nameof(Nome), "O nome funcional da definição é obrigatório [Origem: WfDefinicao]")
                .IsGreaterThan(Versao, 0, nameof(Versao), "A versão da definição deve ser maior que zero [Origem: WfDefinicao]"));
        }
    }
}
