using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class PessoaMotorista : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }
        public ETipoVinculoMotorista TipoVinculoMotorista { get; private set; }
        public ETipoCategoriaCnh TipoCategoriaCnh { get; private set; }
        public DateTime? DataEmissaoCnh { get; private set; }
        public DateTime? DataVencimentoCnh { get; private set; }
        public string? Rntrc { get; private set; }

        protected PessoaMotorista() { } // EF Core

        public PessoaMotorista(
            Guid pessoaId,
            ETipoVinculoMotorista tipoVinculoMotorista,
            ETipoCategoriaCnh tipoCategoriaCnh,
            DateTime? dataEmissaoCnh,
            DateTime? dataVencimentoCnh,
            string? rntrc,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PessoaMotorista>()
                .Requires()
                .IsTrue(Enum.IsDefined(typeof(ETipoVinculoMotorista), tipoVinculoMotorista), nameof(TipoVinculoMotorista), "TipoVinculoMotorista não consta na lista [Origem: PessoaMotorista]")
                .IsTrue(Enum.IsDefined(typeof(ETipoCategoriaCnh), tipoCategoriaCnh), nameof(TipoCategoriaCnh), "TipoCategoriaCnh não consta na lista [Origem: PessoaMotorista]")
                .HasMaxLen(rntrc ?? string.Empty, 14, nameof(Rntrc), "O campo Rntrc deve ter no máximo 14 caracteres [Origem: PessoaMotorista]")
            );

            PessoaId = pessoaId;
            TipoVinculoMotorista = tipoVinculoMotorista;
            TipoCategoriaCnh = tipoCategoriaCnh;
            DataEmissaoCnh = dataEmissaoCnh;
            DataVencimentoCnh = dataVencimentoCnh;
            Rntrc = rntrc;
        }
    }
}
