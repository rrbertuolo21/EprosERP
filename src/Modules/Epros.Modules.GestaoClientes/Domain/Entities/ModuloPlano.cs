using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class ModuloPlano : EntidadeSaaSBase
    {
        public string NomeModulo { get; private set; } = string.Empty;
        public Guid PlanoId { get; private set; }
        public string? ModuloGeralId { get; private set; }
        public string? Descricao { get; private set; }
        public decimal Valor { get; private set; }
        public bool Ativo { get; private set; }

        protected ModuloPlano() { } // EF Core

        public ModuloPlano(
            string nomeModulo,
            Guid planoId,
            string tenantId,
            string criadoPor,
            string? moduloGeralId = null,
            string? descricao = null,
            decimal valor = 0m,
            bool ativo = true)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ModuloPlano>()
                .Requires()
                .IsNotNullOrEmpty(nomeModulo, nameof(NomeModulo), "Nome do módulo é obrigatório")
                .AreNotEquals(planoId, Guid.Empty, nameof(PlanoId), "PlanoId inválido")
            );

            NomeModulo = nomeModulo;
            PlanoId = planoId;
            ModuloGeralId = moduloGeralId;
            Descricao = descricao;
            Valor = valor;
            Ativo = ativo;
        }

        /// <summary>Atualiza os dados escalares do módulo (mantém NomeModulo como chave de reconciliação).</summary>
        public void Atualizar(string? moduloGeralId, string? descricao, decimal valor, bool ativo, string alteradoPor)
        {
            ModuloGeralId = moduloGeralId;
            Descricao = descricao;
            Valor = valor;
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
        }
    }
}
