using System;
using Epros.Shared.Domain.Entities;
using Epros.Modules.GestaoClientes.Domain.ValueObjects;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class Endereco : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }
        // Legado: PessoaEndereco.EmpresaId — dimensão Empresa do vínculo de endereço (escopo por empresa).
        public Guid? EmpresaId { get; private set; }
        // Legado: Endereco.Contador (navegação) -> FK por Guid (cross-module: Contador vive no Fiscal).
        public Guid? ContadorId { get; private set; }
        public ETipoEndereco TipoEndereco { get; private set; }
        public Guid PaisId { get; private set; }
        public Guid MunicipioId { get; private set; }
        public Guid? SubdivisaoId { get; private set; }
        public string Uf { get; private set; } = string.Empty;
        public string? Cep { get; private set; }
        public string Logradouro { get; private set; } = string.Empty;
        public string? Numero { get; private set; }
        public string? Complemento { get; private set; }
        public string Bairro { get; private set; } = string.Empty;
        public string? Referencia { get; private set; }
        public string? CodigoPostalInternacional { get; private set; }
        public string? LinhaEndereco1 { get; private set; }
        public string? LinhaEndereco2 { get; private set; }
        public decimal? Latitude { get; private set; }
        public decimal? Longitude { get; private set; }
        public string? NomeDoRecebedor { get; private set; } = string.Empty;
        public string? DocumentoDoRecebedor { get; private set; } = string.Empty;

        // Navigation Properties
        public Pais Pais { get; private set; } = null!;
        public Municipio Municipio { get; private set; } = null!;
        public Subdivisao? Subdivisao { get; private set; }

        protected Endereco() { } // EF Core

        public Endereco(
            Guid pessoaId,
            ETipoEndereco tipoEndereco,
            Guid paisId,
            Guid municipioId,
            Guid? subdivisaoId,
            string uf,
            string? cep,
            string logradouro,
            string? numero,
            string? complemento,
            string bairro,
            string? referencia,
            string? codigoPostalInternacional,
            string? linhaEndereco1,
            string? linhaEndereco2,
            decimal? latitude,
            decimal? longitude,
            string tenantId,
            string criadoPor,
            string? nomeDoRecebedor = null,
            string? documentoDoRecebedor = null,
            Guid? empresaId = null,
            Guid? contadorId = null)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Endereco>()
                .Requires()
                .AreNotEquals(paisId, Guid.Empty, nameof(PaisId), "O ID do país é obrigatório.")
                .AreNotEquals(municipioId, Guid.Empty, nameof(MunicipioId), "O ID do município é obrigatório.")
                .IsTrue(string.IsNullOrEmpty(uf) || Enum.TryParse<EEstado>(uf, out _) || subdivisaoId.HasValue, nameof(Uf), "Uf não consta na lista [Origem: Endereco]")
                .IsNotNullOrEmpty(logradouro, nameof(Logradouro), "O campo Logradouro é obrigatório.")
                .IsMaxLength(logradouro, 150, nameof(Logradouro), "O campo Logradouro deve ter no máximo 150 caracteres [Origem: Endereco]")
                .IsMaxLength(complemento ?? string.Empty, 100, nameof(Complemento), "O campo Complemento deve ter no máximo 100 caracteres [Origem: Endereco]")
                .IsMaxLength(numero ?? string.Empty, 20, nameof(Numero), "O campo Numero deve ter no máximo 20 caracteres [Origem: Endereco]")
                .IsMaxLength(bairro, 100, nameof(Bairro), "O campo Bairro deve ter no máximo 100 caracteres [Origem: Endereco]")
                .IsMaxLength(referencia ?? string.Empty, 250, nameof(Referencia), "O campo Referencia deve ter no máximo 250 caracteres [Origem: Endereco]")
                .IsTrue(Enum.IsDefined(typeof(ETipoEndereco), tipoEndereco), nameof(TipoEndereco), "TipoEndereco não consta na lista [Origem: Endereco]")
            );

            // Regra de negócio portada do legado: para endereços de entrega,
            // NomeDoRecebedor e DocumentoDoRecebedor são obrigatórios.
            if (ETipoEndereco.Entrega.Equals(tipoEndereco))
            {
                AddNotifications(new Contract<Endereco>()
                    .Requires()
                    .IsNotNullOrEmpty(nomeDoRecebedor, nameof(NomeDoRecebedor), "O campo NomeDoRecebedor é obrigatório para endereços de entrega [Origem: Endereco]")
                    .IsNotNullOrEmpty(documentoDoRecebedor, nameof(DocumentoDoRecebedor), "O campo DocumentoDoRecebedor é obrigatório para endereços de entrega [Origem: Endereco]")
                    .IsMaxLength(nomeDoRecebedor ?? string.Empty, 100, nameof(NomeDoRecebedor), "O campo NomeDoRecebedor deve ter no máximo 100 caracteres [Origem: Endereco]")
                    .IsMaxLength(documentoDoRecebedor ?? string.Empty, 20, nameof(DocumentoDoRecebedor), "O campo DocumentoDoRecebedor deve ter no máximo 20 caracteres [Origem: Endereco]")
                );
            }

            if (!string.IsNullOrEmpty(cep))
            {
                var cepVo = new ValueObjects.Cep(cep);
                if (!cepVo.IsValid)
                {
                    AddNotifications(cepVo.Notifications);
                }
                Cep = cepVo.Valor;
            }

            PessoaId = pessoaId;
            EmpresaId = empresaId;
            ContadorId = contadorId;
            TipoEndereco = tipoEndereco;
            PaisId = paisId;
            MunicipioId = municipioId;
            SubdivisaoId = subdivisaoId;
            Uf = uf;
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            Referencia = referencia;
            CodigoPostalInternacional = codigoPostalInternacional;
            LinhaEndereco1 = linhaEndereco1;
            LinhaEndereco2 = linhaEndereco2;
            Latitude = latitude;
            Longitude = longitude;
            NomeDoRecebedor = nomeDoRecebedor;
            DocumentoDoRecebedor = documentoDoRecebedor;
        }

        public string ObterEnderecoCompletoTransportadora()
        {
            string enderecoCompleto = $"{Logradouro}, {(string.IsNullOrEmpty(Numero) ? "S/N" : Numero)}";

            if (!string.IsNullOrWhiteSpace(Bairro))
            {
                enderecoCompleto += $", {Bairro}";
            }

            if (!string.IsNullOrWhiteSpace(Complemento))
            {
                enderecoCompleto += $", {Complemento}";
            }

            return enderecoCompleto.Length > 60 ? enderecoCompleto.Substring(0, 60) : enderecoCompleto;
        }
    }
}

