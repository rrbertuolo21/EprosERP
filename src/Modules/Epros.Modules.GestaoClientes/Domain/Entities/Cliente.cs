using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class Cliente : EntidadeSaaSBase
    {
        public string RazaoSocial { get; private set; } = string.Empty;
        public string Cnpj { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public Guid PlanoId { get; private set; }
        public Guid? RevendaId { get; private set; }
        public Guid? VendedorId { get; private set; }
        public int DiaVencimento { get; private set; }
        public string StatusSaaS { get; private set; } = string.Empty;
        public bool Ativo { get; private set; }
        public string? Telefone { get; private set; }
        public string? NomeContato { get; private set; }
        public bool IsDemo { get; private set; }
        public string? TokenAcesso { get; private set; }

        protected Cliente() { } // EF Core

        public Cliente(
            string razaoSocial, 
            string cnpj, 
            string email, 
            Guid planoId, 
            Guid? revendaId, 
            Guid? vendedorId, 
            int diaVencimento, 
            string statusSaaS, 
            string tenantId, 
            string criadoPor,
            string? telefone = null,
            string? nomeContato = null,
            bool isDemo = false,
            string? tokenAcesso = null)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Cliente>()
                .Requires()
                .IsNotNullOrEmpty(razaoSocial, nameof(RazaoSocial), "Razão social é obrigatória")
                .IsNotNullOrEmpty(cnpj, nameof(Cnpj), "CNPJ é obrigatório")
                .IsEmail(email, nameof(Email), "E-mail inválido")
                .AreNotEquals(planoId, Guid.Empty, nameof(PlanoId), "PlanoId é obrigatório")
                .IsBetween(diaVencimento, 1, 31, nameof(DiaVencimento), "Dia de vencimento deve ser entre 1 e 31")
            );

            RazaoSocial = razaoSocial;
            Cnpj = cnpj;
            Email = email;
            PlanoId = planoId;
            RevendaId = revendaId;
            VendedorId = vendedorId;
            DiaVencimento = diaVencimento;
            StatusSaaS = statusSaaS;
            Ativo = true;
            Telefone = telefone;
            NomeContato = nomeContato;
            IsDemo = isDemo;
            TokenAcesso = tokenAcesso;
        }

        public Cliente(string razaoSocial, string cnpj, string email, Guid planoId, string tenantId, string criadoPor)
            : this(razaoSocial, cnpj, email, planoId, null, null, 10, "Active", tenantId, criadoPor)
        {
        }

        public void Alterar(
            string razaoSocial,
            string cnpj,
            string email,
            Guid planoId,
            Guid? revendaId,
            Guid? vendedorId,
            int diaVencimento,
            bool ativo,
            string? telefone,
            string? nomeContato,
            bool isDemo,
            string? tokenAcesso,
            string alteradoPor)
        {
            AddNotifications(new Contract<Cliente>()
                .Requires()
                .IsNotNullOrEmpty(razaoSocial, nameof(RazaoSocial), "Razão social é obrigatória")
                .IsNotNullOrEmpty(cnpj, nameof(Cnpj), "CNPJ é obrigatório")
                .IsEmail(email, nameof(Email), "E-mail inválido")
                .AreNotEquals(planoId, Guid.Empty, nameof(PlanoId), "PlanoId é obrigatório")
                .IsBetween(diaVencimento, 1, 31, nameof(DiaVencimento), "Dia de vencimento deve ser entre 1 e 31")
            );

            if (IsValid)
            {
                RazaoSocial = razaoSocial;
                Cnpj = cnpj;
                Email = email;
                PlanoId = planoId;
                RevendaId = revendaId;
                VendedorId = vendedorId;
                DiaVencimento = diaVencimento;
                Ativo = ativo;
                Telefone = telefone;
                NomeContato = nomeContato;
                IsDemo = isDemo;
                TokenAcesso = tokenAcesso;
                MarcarAlterado(alteradoPor);
            }
        }

        public void AlterarPlano(Guid novoPlanoId, string alteradoPor)
        {
            AddNotifications(new Contract<Cliente>()
                .Requires()
                .AreNotEquals(novoPlanoId, Guid.Empty, nameof(PlanoId), "Novo PlanoId é obrigatório")
            );

            if (IsValid)
            {
                PlanoId = novoPlanoId;
                MarcarAlterado(alteradoPor);
            }
        }

        public void AtualizarStatusSaaS(string novoStatus, string alteradoPor)
        {
            StatusSaaS = novoStatus;
            MarcarAlterado(alteradoPor);
        }

        public void Inativar(string alteradoPor)
        {
            Ativo = false;
            MarcarAlterado(alteradoPor);
        }

        public void Ativar(string alteradoPor)
        {
            Ativo = true;
            MarcarAlterado(alteradoPor);
        }

        public void AtualizarDadosContato(string? nomeContato, string? telefone, string alteradoPor)
        {
            NomeContato = nomeContato;
            Telefone = telefone;
            MarcarAlterado(alteradoPor);
        }

        public void AlterarIsDemo(bool isDemo, string alteradoPor)
        {
            IsDemo = isDemo;
            MarcarAlterado(alteradoPor);
        }

        public void AlterarTokenAcesso(string? tokenAcesso, string alteradoPor)
        {
            TokenAcesso = tokenAcesso;
            MarcarAlterado(alteradoPor);
        }
    }
}
