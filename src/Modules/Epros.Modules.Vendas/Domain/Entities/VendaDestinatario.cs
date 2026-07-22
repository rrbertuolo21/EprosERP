using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Notifications;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaDestinatario. FK long -> Guid; VOs CNPJ/CPF/Documento -> string?;
    /// herda EntidadeSaaSBase. PessoaId referencia pessoa em outro módulo por Guid FK.
    /// </summary>
    public class VendaDestinatario : EntidadeSaaSBase
    {
        public Guid VendaId { get; private set; }
        public Guid? PessoaId { get; private set; }
        public string? Cnpj { get; private set; }
        public string? Cpf { get; private set; }
        public string? RazaoSocial { get; private set; }
        public string? Telefone { get; private set; }
        public string? InscricaoEstadual { get; private set; }
        public string? IdentificadorEstrangeiro { get; private set; }
        public ETipoIndicadorIe IndicadorIE { get; private set; }
        public string? Email { get; private set; }
        public bool EhConsumidorFinal { get; private set; }
        public bool EnviarDestinatatioNaNfce { get; private set; }
        public string? DocumentoConsumidor { get; private set; }

        private readonly List<VendaDestinatarioEndereco> _enderecos = new();
        public IReadOnlyCollection<VendaDestinatarioEndereco> Enderecos => _enderecos.AsReadOnly();

        // Navegação intra-módulo
        public Venda Venda { get; private set; } = null!;

        protected VendaDestinatario() { } // EF Core

        public VendaDestinatario(Guid vendaId, Guid? pessoaId, string? cnpj, string? cpf, string? razaoSocial, string? telefone, string? inscricaoEstadual, string? identificadorEstrangeiro, ETipoIndicadorIe indicadorIE, string? email, bool ehConsumidorFinal, string? documentoConsumidor, bool enviarDestinatatioNaNfce,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            VendaId = vendaId;
            PessoaId = pessoaId;
            Cnpj = cnpj;
            Cpf = cpf;
            RazaoSocial = (razaoSocial ?? "").Trim();
            Telefone = telefone;
            InscricaoEstadual = inscricaoEstadual;
            IdentificadorEstrangeiro = identificadorEstrangeiro;
            IndicadorIE = indicadorIE;
            Email = email;
            EhConsumidorFinal = ehConsumidorFinal;
            DocumentoConsumidor = documentoConsumidor;
            EnviarDestinatatioNaNfce = enviarDestinatatioNaNfce;
            Validar();
        }

        public void Alterar(Guid? pessoaId, string? cnpj, string? cpf, string? razaoSocial, string? telefone, string? inscricaoEstadual, string? identificadorEstrangeiro, ETipoIndicadorIe indicadorIE, string? email, bool ehConsumidorFinal, string? documentoConsumidor, bool enviarDestinatatioNaNfce, string alteradoPor)
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
            EhConsumidorFinal = ehConsumidorFinal;
            DocumentoConsumidor = documentoConsumidor;
            EnviarDestinatatioNaNfce = enviarDestinatatioNaNfce;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        /// <summary>Porte fiel de VendaDestinatario.Validar.</summary>
        public void Validar()
        {
            AddNotifications(new Contract<Notification>()
                .Requires()
                .IsBetween((RazaoSocial ?? "").Length, 2, 60, "RazaoSocial", "Razão Social do destinatario, deve conter entre 2 e 60 caractes")
                .IsLowerOrEqualsThan(14, (InscricaoEstadual ?? "").Length, "InscricaoEstadual", "Inscrição Estadual pode conter no max 14 caracteres ")
                .IsLowerOrEqualsThan(20, (IdentificadorEstrangeiro ?? "").Length, "IdentificadorEstrangeiro", "Identificador Estrangeiro do destinatario pode conter entre 5-20 caracteres")
                .IsLowerOrEqualsThan(60, (Email ?? "").Length, "Email", "E-mail do destinatario pode conter entre 1-60 caracteres")
                .IsLowerOrEqualsThan(14, (Telefone ?? "").Length, "Telefone", "Telefone do destinatario pode conter no max 14 caracteres ")
            );

            if (!Enum.IsDefined(typeof(ETipoIndicadorIe), IndicadorIE))
                AddNotification("IndicadorIE", "Indicador IE informado inválido ");

            if (_enderecos.Count > 0)
                foreach (var endereco in _enderecos)
                    AddNotifications(endereco.Notifications);
        }

        public void AdicionarEndereco(ETipoEndereco tipoEndereco, EEstado uf, string logradouro, string numero, string complemento, string bairro, int municipioId, string municipioNome, string cep, int paisId, string paisNome, string criadoPor)
        {
            _enderecos.Add(new VendaDestinatarioEndereco(Id, tipoEndereco, uf, logradouro, numero, complemento, bairro, municipioId, municipioNome, cep, TenantId, criadoPor, paisId, paisNome));
        }

        public void AlterarEndereco(Guid enderecoId, EEstado uf, string logradouro, string numero, string complemento, string bairro, int municipioId, string municipioNome, string cep, int paisId, string paisNome, string alteradoPor)
        {
            var endereco = _enderecos.FirstOrDefault(x => x.Id == enderecoId);
            if (endereco == null)
            {
                AddNotification("Enderecos", "Endereço do destinatario não localizado para alteração");
                return;
            }
            endereco.Alterar(uf, logradouro, numero, complemento, bairro, municipioId, municipioNome, cep, alteradoPor, paisId, paisNome);
        }

        public void DeletarEndereco(Guid enderecoId, string userId)
        {
            var endereco = _enderecos.FirstOrDefault(x => x.Id == enderecoId);
            if (endereco == null)
            {
                AddNotification("Enderecos", "Endereço do destinatario não localizado para exclusão");
                return;
            }
            endereco.Deletar(userId);
        }

        /// <summary>Porte fiel de VendaDestinatario.Duplicar (novo Id/FK, incl. endereços).</summary>
        public VendaDestinatario Duplicar(Guid novaVendaId, string criadoPor)
        {
            var clone = new VendaDestinatario(novaVendaId, PessoaId, Cnpj, Cpf, RazaoSocial, Telefone, InscricaoEstadual,
                IdentificadorEstrangeiro, IndicadorIE, Email, EhConsumidorFinal, DocumentoConsumidor, EnviarDestinatatioNaNfce, TenantId, criadoPor);
            foreach (var endereco in _enderecos)
                clone._enderecos.Add(endereco.Duplicar(clone.Id, criadoPor));
            return clone;
        }
    }
}
