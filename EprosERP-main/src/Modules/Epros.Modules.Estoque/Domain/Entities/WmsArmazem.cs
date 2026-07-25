using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Armazém operacional (EF Gestão de Armazém WMS §16.1 `wms_armazem`).
    /// WMS-014: novo armazém nasce ativo. WMS-015: grava usuário dono e tenant.
    /// WMS-019: nome, endereço, cidade e CEP obrigatórios. WMS-020/021: telefone/email opcionais
    /// validados quando informados. WMS-022: status ativo booleano.
    /// </summary>
    public class WmsArmazem : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string Endereco { get; private set; } = string.Empty;
        public string Cidade { get; private set; } = string.Empty;
        public string Cep { get; private set; } = string.Empty;
        public string? Telefone { get; private set; }
        public string? Email { get; private set; }
        public bool Ativo { get; private set; } = true;
        public Guid UsuarioDonoId { get; private set; }

        // Navegação intra-módulo
        public ICollection<WmsEnderecoOperacional> Enderecos { get; private set; } = new List<WmsEnderecoOperacional>();

        protected WmsArmazem() { } // EF Core

        public WmsArmazem(string nome, string endereco, string cidade, string cep, string? telefone, string? email, bool? ativo, Guid usuarioDonoId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Endereco = endereco;
            Cidade = cidade;
            Cep = cep;
            Telefone = telefone;
            Email = email;
            Ativo = ativo ?? true; // WMS-014
            UsuarioDonoId = usuarioDonoId; // WMS-015
            Validar();
        }

        public void Alterar(string nome, string endereco, string cidade, string cep, string? telefone, string? email, bool ativo, string alteradoPor)
        {
            Nome = nome;
            Endereco = endereco;
            Cidade = cidade;
            Cep = cep;
            Telefone = telefone;
            Email = email;
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void AdicionarEndereco(WmsEnderecoOperacional endereco) => Enderecos.Add(endereco);

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<WmsArmazem>()
                .Requires()
                .IsNotNullOrEmpty(Nome, nameof(Nome), "O nome do armazém é obrigatório [WMS-019] [Origem: WmsArmazem]")
                .IsNotNullOrEmpty(Endereco, nameof(Endereco), "O endereço do armazém é obrigatório [WMS-019] [Origem: WmsArmazem]")
                .IsNotNullOrEmpty(Cidade, nameof(Cidade), "A cidade do armazém é obrigatória [WMS-019] [Origem: WmsArmazem]")
                .IsNotNullOrEmpty(Cep, nameof(Cep), "O CEP do armazém é obrigatório [WMS-019] [Origem: WmsArmazem]"));

            if (!string.IsNullOrWhiteSpace(Email))
                AddNotifications(new Contract<WmsArmazem>().Requires().IsEmail(Email, nameof(Email), "O e-mail do armazém deve ter formato válido [WMS-021] [Origem: WmsArmazem]"));
        }
    }
}
