using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Destinatário (comprador) da compra. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraDestinatario. Os ValueObjects CNPJ/CPF foram achatados
    /// para strings. PessoaId é FK Guid para o módulo de plataforma (sem navegação cruzada).
    /// </summary>
    public class CompraDestinatario : EntidadeSaaSBase
    {
        public Guid CompraId { get; private set; }
        public Guid? PessoaId { get; private set; }
        public string? Cnpj { get; private set; }  // 14
        public string? Cpf { get; private set; }  // 11
        public string? RazaoSocial { get; private set; }  // 2-60
        public string? Telefone { get; private set; }  // 6-14
        public string? InscricaoEstadual { get; private set; }  // 0-14
        public string? IdentificadorEstrangeiro { get; private set; }  // 5-20
        public ETipoIndicadorIe IndicadorIE { get; private set; }
        public string? Email { get; private set; }  // 1-60
        public bool EhConsumidorFinal { get; private set; }

        // Navegação intra-módulo
        public ICollection<CompraDestinatarioEndereco> Enderecos { get; private set; } = new List<CompraDestinatarioEndereco>();
        public Compra? Compra { get; private set; }

        protected CompraDestinatario() { } // EF Core

        public CompraDestinatario(Guid compraId, Guid? pessoaId, string? cnpj, string? cpf, string? razaoSocial, string? telefone, string? inscricaoEstadual, string? identificadorEstrangeiro, ETipoIndicadorIe indicadorIE, string? email, bool ehConsumidorFinal, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraId = compraId;
            PessoaId = pessoaId;
            Cnpj = cnpj;
            Cpf = cpf;
            RazaoSocial = razaoSocial;
            Telefone = telefone;
            InscricaoEstadual = inscricaoEstadual;
            IdentificadorEstrangeiro = identificadorEstrangeiro;
            IndicadorIE = indicadorIE;
            Email = email;
            EhConsumidorFinal = ehConsumidorFinal;
            Validar();
        }

        public void Validar()
        {
            AddNotifications(new Contract<CompraDestinatario>()
                .Requires()
                .IsBetween((RazaoSocial ?? "").Length, 2, 60, nameof(RazaoSocial), "Razão Social do destinatario, deve conter entre 2 e 60 caractes")
                .IsLowerOrEqualsThan((InscricaoEstadual ?? "").Length, 14, nameof(InscricaoEstadual), "Inscrição Estadual pode conter no max 14 caracteres")
                .IsLowerOrEqualsThan((IdentificadorEstrangeiro ?? "").Length, 20, nameof(IdentificadorEstrangeiro), "Identificador Estrangeiro do destinatario pode conter entre 5-20 caracteres")
                .IsLowerOrEqualsThan((Email ?? "").Length, 60, nameof(Email), "E-mail do destinatario pode conter entre 1-60 caracteres")
                .IsLowerOrEqualsThan((Telefone ?? "").Length, 14, nameof(Telefone), "Telefone do destinatario pode conter no max 14 caracteres")
            );
        }

        public void Alterar(Guid? pessoaId, string? cnpj, string? cpf, string? razaoSocial, string? telefone, string? inscricaoEstadual, string? identificadorEstrangeiro, ETipoIndicadorIe indicadorIE, string? email, string usuario)
        {
            PessoaId = pessoaId;
            Cnpj = cnpj;
            Cpf = cpf;
            RazaoSocial = razaoSocial;
            Telefone = telefone;
            InscricaoEstadual = inscricaoEstadual;
            IdentificadorEstrangeiro = identificadorEstrangeiro;
            IndicadorIE = indicadorIE;
            Email = email;
            MarcarAlterado(usuario);
            Validar();
        }

        public void AdicionarEndereco(ETipoEndereco tipoEndereco, EEstado uf, string? logradouro, string? numero, string? complemento, string? bairro, int municipioId, string? municipioNome, string? cep, int paisId, string? paisNome, string usuario)
        {
            Enderecos.Add(new CompraDestinatarioEndereco(Id, tipoEndereco, uf, logradouro, numero, complemento, bairro, municipioId, municipioNome, cep, TenantId, usuario, paisId, paisNome));
        }

        public void AlterarEndereco(Guid enderecoId, EEstado uf, string? logradouro, string? numero, string? complemento, string? bairro, int municipioId, string? municipioNome, string? cep, int paisId, string? paisNome, string usuario)
        {
            var endereco = Enderecos.FirstOrDefault(x => x.Id == enderecoId);
            if (endereco == null)
            {
                AddNotification("Enderecos", "Endereço do destinatario não localizado para alteração");
                return;
            }
            endereco.Alterar(uf, logradouro, numero, complemento, bairro, municipioId, municipioNome, cep, usuario, paisId, paisNome);
        }

        public void DeletarEndereco(Guid enderecoId, string usuario)
        {
            var endereco = Enderecos.FirstOrDefault(x => x.Id == enderecoId);
            if (endereco == null)
            {
                AddNotification("Enderecos", "Endereço do destinatario não localizado para exclusão");
                return;
            }
            endereco.Deletar(usuario);
        }
    }
}
