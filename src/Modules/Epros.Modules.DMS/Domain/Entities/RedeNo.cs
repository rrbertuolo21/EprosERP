using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class RedeNo : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string TipoNo { get; private set; } = string.Empty; // Marca, Grupo, Filial, Ponto
        public Guid? PaiId { get; private set; }
        public Guid? PessoaEmpresaId { get; private set; }
        public Guid? LocalId { get; private set; }
        public DateTime? InicioVigencia { get; private set; }
        public DateTime? FimVigencia { get; private set; }
        public string Status { get; private set; } = "Ativo";

        protected RedeNo() { } // EF Core

        public RedeNo(
            string codigo,
            string tipoNo,
            Guid? paiId,
            Guid? pessoaEmpresaId,
            Guid? localId,
            DateTime? inicioVigencia,
            DateTime? fimVigencia,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<RedeNo>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O código do nó da rede é obrigatório.")
                .IsNotNullOrEmpty(tipoNo, nameof(TipoNo), "O tipo do nó da rede é obrigatório.")
            );

            Codigo = codigo;
            TipoNo = tipoNo;
            PaiId = paiId;
            PessoaEmpresaId = pessoaEmpresaId;
            LocalId = localId;
            InicioVigencia = inicioVigencia;
            FimVigencia = fimVigencia;
            Status = "Ativo";
        }

        public void Inativar(string usuario)
        {
            Status = "Inativo";
            MarcarAlterado(usuario);
        }
    }
}
