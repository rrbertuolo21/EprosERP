using System;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Transportadora do transporte da compra. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraTransporteTransportadora. Os ValueObjects CNPJ/CPF foram
    /// achatados para strings. PessoaId é FK Guid para o módulo de plataforma.
    /// </summary>
    public class CompraTransporteTransportadora : EntidadeSaaSBase
    {
        public Guid CompraTransporteId { get; private set; }
        public Guid? PessoaId { get; private set; }
        public string? Cnpj { get; private set; }  // 14
        public string? Cpf { get; private set; }  // 11
        public string? RazaoSocial { get; private set; }  // 2-60
        public string? InscricaoEstadual { get; private set; }  // 0-14
        public string? EnderecoCompleto { get; private set; }  // 0-60
        public string? NomeMunicipio { get; private set; }  // 0-60
        public EEstado Uf { get; private set; }  // 2

        // Navegação intra-módulo
        public CompraTransporte? Transporte { get; private set; }

        protected CompraTransporteTransportadora() { } // EF Core

        public CompraTransporteTransportadora(Guid compraTransporteId, Guid? pessoaId, string? cnpj, string? cpf, string? razaoSocialOuNome, string? inscricaoEstadual, string? enderecoCompleto, string? nomeMunicipio, EEstado uf, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraTransporteId = compraTransporteId;
            PessoaId = pessoaId;
            Cnpj = cnpj;
            Cpf = cpf;
            RazaoSocial = razaoSocialOuNome;
            InscricaoEstadual = inscricaoEstadual;
            EnderecoCompleto = enderecoCompleto;
            NomeMunicipio = nomeMunicipio;
            Uf = uf;
            Validar();
        }

        public void Validar()
        {
            AddNotifications(new Contract<CompraTransporteTransportadora>()
                .Requires()
                .IsBetween((RazaoSocial ?? "").Length, 2, 60, nameof(RazaoSocial), "Razão Social do emitente, deve conter entre 2 e 60 caractes")
                .IsLowerOrEqualsThan((InscricaoEstadual ?? "").Length, 20, nameof(InscricaoEstadual), "Inscrição Estadual pode conter no max 20 caracteres")
                .IsLowerOrEqualsThan((EnderecoCompleto ?? "").Length, 60, nameof(EnderecoCompleto), "Endereço completo informado na compra transporte pode conter no max 60 caracteres")
                .IsLowerOrEqualsThan((NomeMunicipio ?? "").Length, 60, nameof(NomeMunicipio), "Nome Municipio informado na compra transporte pode conter no max 60 caracteres")
            );

            if (!Enum.IsDefined(typeof(EEstado), Uf))
                AddNotification("Uf", "Uf informado na compra transporte inválido");

            if (string.IsNullOrWhiteSpace(Cnpj) && string.IsNullOrWhiteSpace(Cpf))
                AddNotification("CPF/CNPJ", "CPF/CNPJ informado na compra transportadora, pelo menos um dos dois devem ser informados válido");
        }

        public void Alterar(Guid? pessoaId, string? cnpj, string? cpf, string? razaoSocialOuNome, string? inscricaoEstadual, string? enderecoCompleto, string? nomeMunicipio, EEstado uf, string usuario)
        {
            PessoaId = pessoaId;
            Cnpj = cnpj;
            Cpf = cpf;
            RazaoSocial = razaoSocialOuNome;
            InscricaoEstadual = inscricaoEstadual;
            EnderecoCompleto = enderecoCompleto;
            NomeMunicipio = nomeMunicipio;
            Uf = uf;
            MarcarAlterado(usuario);
            Validar();
        }
    }
}
