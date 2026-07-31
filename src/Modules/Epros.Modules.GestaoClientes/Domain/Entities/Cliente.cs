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
        public StatusSaaS StatusSaaS { get; private set; }

        // 1.05 / REG-021 — Instante em que o StatusSaaS atual passou a valer. Fonte da "data do
        // cancelamento" para a janela somente-leitura de 30 dias (Cancelado/Falha) aplicada no
        // InquilinoSaaSMiddleware. Preenchido a cada transição de status.
        public DateTime? StatusSaaSAtualizadoEm { get; private set; }

        public bool Ativo { get; private set; }
        public string? Telefone { get; private set; }
        public string? NomeContato { get; private set; }
        public bool IsDemo { get; private set; }
        public string? TokenAcesso { get; private set; }

        // 1.01 — Cota por cliente (snapshot). Override contratado sobre o plano base (add-on/negociação).
        // null = usa o limite do plano; valor preenchido = cota específica deste cliente (EF 5.11/6.2).
        public int? CotaUsuarios { get; private set; }
        public int? CotaEmpresas { get; private set; }
        public int? CotaPermissoes { get; private set; }

        // 1.06 — Cota (snapshot) de CLIENTES (customers do tenant) contratada por este cliente-assinante.
        // null = usa Plano.LimiteClientes; valor preenchido = override específico deste cliente.
        public int? CotaClientes { get; private set; }

        protected Cliente() { } // EF Core

        public Cliente(
            string razaoSocial, 
            string cnpj, 
            string email, 
            Guid planoId, 
            Guid? revendaId, 
            Guid? vendedorId, 
            int diaVencimento,
            StatusSaaS statusSaaS,
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
            StatusSaaSAtualizadoEm = DateTime.UtcNow;
            Ativo = true;
            Telefone = telefone;
            NomeContato = nomeContato;
            IsDemo = isDemo;
            TokenAcesso = tokenAcesso;
        }

        public Cliente(string razaoSocial, string cnpj, string email, Guid planoId, string tenantId, string criadoPor)
            : this(razaoSocial, cnpj, email, planoId, null, null, 10, Entities.StatusSaaS.Ativo, tenantId, criadoPor)
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

        public void AtualizarStatusSaaS(StatusSaaS novoStatus, string alteradoPor)
        {
            StatusSaaS = novoStatus;
            StatusSaaSAtualizadoEm = DateTime.UtcNow;
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

        /// <summary>
        /// Define a cota (snapshot) contratada por este cliente, que sobrepõe os limites do plano base.
        /// null em qualquer campo mantém o limite do plano para aquele recurso (EF 5.11/6.2).
        /// </summary>
        public void AtualizarCota(int? cotaUsuarios, int? cotaEmpresas, int? cotaPermissoes, string alteradoPor, int? cotaClientes = null)
        {
            CotaUsuarios = cotaUsuarios;
            CotaEmpresas = cotaEmpresas;
            CotaPermissoes = cotaPermissoes;
            CotaClientes = cotaClientes;
            MarcarAlterado(alteradoPor);
        }
    }
}
