using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class Contador : EntidadeSaaSBase
    {
        public string? RazaoSocial { get; private set; }
        public string? NomeContador { get; private set; }
        public string? Cpf { get; private set; }
        public string? Cnpj { get; private set; }
        public string NumeroCrc { get; private set; } = string.Empty;
        public EEstado UfCrc { get; private set; }
        public DateTime DataVencimentoCrc { get; private set; }
        public string? Qualificacao { get; private set; }
        public string? Funcao { get; private set; }
        public string? Telefone { get; private set; }
        public string? Email { get; private set; }
        public EPermissaoTransmissao PermissaoTransmissao { get; private set; }
        public bool Ativo { get; private set; } = true;

        // Flat address fields for boundary isolation
        public string Logradouro { get; private set; } = string.Empty;
        public string Numero { get; private set; } = string.Empty;
        public string? Complemento { get; private set; }
        public string Bairro { get; private set; } = string.Empty;
        public string Cep { get; private set; } = string.Empty;
        public int MunicipioId { get; private set; }
        public EEstado Uf { get; private set; }

        protected Contador() { } // EF Core

        public Contador(
            string? razaoSocial,
            string? nomeContador,
            string? cpf,
            string? cnpj,
            string numeroCrc,
            EEstado ufCrc,
            DateTime dataVencimentoCrc,
            string? qualificacao,
            string? funcao,
            string? telefone,
            string? email,
            EPermissaoTransmissao permissaoTransmissao,
            bool ativo,
            string logradouro,
            string numero,
            string? complemento,
            string bairro,
            string cep,
            int municipioId,
            EEstado uf,
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            RazaoSocial = razaoSocial;
            NomeContador = nomeContador;
            Cpf = cpf;
            Cnpj = cnpj;
            NumeroCrc = numeroCrc;
            UfCrc = ufCrc;
            DataVencimentoCrc = dataVencimentoCrc;
            Qualificacao = qualificacao;
            Funcao = funcao;
            Telefone = telefone;
            Email = email;
            PermissaoTransmissao = permissaoTransmissao;
            Ativo = ativo;
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            Cep = cep;
            MunicipioId = municipioId;
            Uf = uf;

            Validar();
        }

        public void Alterar(
            string? razaoSocial,
            string? nomeContador,
            string? cpf,
            string? cnpj,
            string numeroCrc,
            EEstado ufCrc,
            DateTime dataVencimentoCrc,
            string? qualificacao,
            string? funcao,
            string? telefone,
            string? email,
            EPermissaoTransmissao permissaoTransmissao,
            bool ativo,
            string logradouro,
            string numero,
            string? complemento,
            string bairro,
            string cep,
            int municipioId,
            EEstado uf,
            string alteradoPor)
        {
            RazaoSocial = razaoSocial;
            NomeContador = nomeContador;
            Cpf = cpf;
            Cnpj = cnpj;
            NumeroCrc = numeroCrc;
            UfCrc = ufCrc;
            DataVencimentoCrc = dataVencimentoCrc;
            Qualificacao = qualificacao;
            Funcao = funcao;
            Telefone = telefone;
            Email = email;
            PermissaoTransmissao = permissaoTransmissao;
            Ativo = ativo;
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            Cep = cep;
            MunicipioId = municipioId;
            Uf = uf;

            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Contador>()
                .Requires()
                .IsTrue(Enum.IsDefined(typeof(EEstado), UfCrc), nameof(UfCrc), "UF do CRC inválida.")
                .IsTrue(Enum.IsDefined(typeof(EEstado), Uf), nameof(Uf), "UF do endereço inválida.")
                .IsLowerOrEqualsThan(RazaoSocial ?? "", 100, nameof(RazaoSocial), "A razão social deve ter no máximo 100 caracteres.")
                .IsLowerOrEqualsThan(NomeContador ?? "", 100, nameof(NomeContador), "O nome do contador deve ter no máximo 100 caracteres.")
                .IsLowerOrEqualsThan(NumeroCrc ?? "", 15, nameof(NumeroCrc), "O número do CRC deve ter no máximo 15 caracteres.")
                .IsLowerOrEqualsThan(Qualificacao ?? "", 60, nameof(Qualificacao), "A qualificação deve ter no máximo 60 caracteres.")
                .IsLowerOrEqualsThan(Funcao ?? "", 60, nameof(Funcao), "A função deve ter no máximo 60 caracteres.")
                .IsTrue(string.IsNullOrEmpty(Telefone) || Telefone.Length == 10 || Telefone.Length == 11, nameof(Telefone), "Telefone inválido.")
                .IsLowerOrEqualsThan(Email ?? "", 150, nameof(Email), "O email deve ter no máximo 150 caracteres.")
                .IsTrue(Enum.IsDefined(typeof(EPermissaoTransmissao), PermissaoTransmissao), nameof(PermissaoTransmissao), "Permissão de transmissão inválida.")
                .IsNotNullOrEmpty(Logradouro, nameof(Logradouro), "O logradouro é obrigatório.")
                .IsLowerOrEqualsThan(Logradouro, 100, nameof(Logradouro), "O logradouro deve ter no máximo 100 caracteres.")
                .IsNotNullOrEmpty(Numero, nameof(Numero), "O número é obrigatório.")
                .IsLowerOrEqualsThan(Numero, 10, nameof(Numero), "O número deve ter no máximo 10 caracteres.")
                .IsLowerOrEqualsThan(Complemento ?? "", 60, nameof(Complemento), "O complemento deve ter no máximo 60 caracteres.")
                .IsNotNullOrEmpty(Bairro, nameof(Bairro), "O bairro é obrigatório.")
                .IsLowerOrEqualsThan(Bairro, 60, nameof(Bairro), "O bairro deve ter no máximo 60 caracteres.")
                .IsNotNullOrEmpty(Cep, nameof(Cep), "O CEP é obrigatório.")
                .IsLowerOrEqualsThan(Cep, 8, nameof(Cep), "O CEP deve ter no máximo 8 caracteres.")
                .IsGreaterThan(MunicipioId, 0, nameof(MunicipioId), "O município é obrigatório.")
            );
        }
    }
}
