using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-SOD — Funcao SoD (grc_sod_funcao). Normaliza a funcao avaliavel pela matriz de
    /// segregacao de funcoes. Fiel a EF_13_GRC_SEGREGACAO_DE_FUNCOES_SOD_V1 (secao 10.2).
    /// Observacao: RBAC (Perfil/Usuario/Menu) e propriedade do modulo GestaoClientes;
    /// aqui a funcao referencia acesso/perfil por Id, sem duplicar o cadastro (ver pendencia SOD-DEP-001).
    /// </summary>
    public class FuncaoSoD : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public string Status { get; private set; } = "Ativo"; // Ativo, Inativo

        protected FuncaoSoD() { } // EF Core

        public FuncaoSoD(
            string codigo,
            string nome,
            string? descricao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<FuncaoSoD>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O codigo da funcao SoD e obrigatorio.")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome da funcao SoD e obrigatorio.")
            );

            Codigo = codigo;
            Nome = nome;
            Descricao = descricao;
            Status = "Ativo";
        }

        public void Inativar(string usuario)
        {
            Status = "Inativo";
            MarcarAlterado(usuario);
        }
    }
}
