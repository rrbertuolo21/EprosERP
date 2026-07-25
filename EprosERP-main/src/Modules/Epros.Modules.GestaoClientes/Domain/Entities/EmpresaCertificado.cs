using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class EmpresaCertificado : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public Guid CertificadoSegredoId { get; private set; }
        public Guid SenhaSegredoId { get; private set; }
        public string Serial { get; private set; } = string.Empty;
        public string? Titular { get; private set; }
        public string? Informacao { get; private set; }
        public string? Cnpj { get; private set; }
        public DateTime? ValidadeInicial { get; private set; }
        public DateTime? ValidadeFinal { get; private set; }

        protected EmpresaCertificado() { } // EF Core

        public EmpresaCertificado(
            Guid empresaId,
            Guid certificadoSegredoId,
            Guid senhaSegredoId,
            string serial,
            string? titular,
            string? informacao,
            string? cnpj,
            DateTime? validadeInicial,
            DateTime? validadeFinal,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<EmpresaCertificado>()
                .Requires()
                .AreNotEquals(empresaId, Guid.Empty, nameof(EmpresaId), "EmpresaId é obrigatório")
                .AreNotEquals(certificadoSegredoId, Guid.Empty, nameof(CertificadoSegredoId), "CertificadoSegredoId é obrigatório")
                .AreNotEquals(senhaSegredoId, Guid.Empty, nameof(SenhaSegredoId), "SenhaSegredoId é obrigatório")
                .IsNotNullOrWhiteSpace(serial, nameof(Serial), "Serial é obrigatório")
            );

            EmpresaId = empresaId;
            CertificadoSegredoId = certificadoSegredoId;
            SenhaSegredoId = senhaSegredoId;
            Serial = serial;
            Titular = titular;
            Informacao = informacao;
            Cnpj = cnpj;
            ValidadeInicial = validadeInicial;
            ValidadeFinal = validadeFinal;
        }
    }
}
