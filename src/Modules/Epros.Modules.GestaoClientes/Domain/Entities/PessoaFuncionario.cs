using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class PessoaFuncionario : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }
        public ETipoCargo TipoCargo { get; private set; }
        public decimal ValorPercentualComissao { get; private set; }

        protected PessoaFuncionario() { } // EF Core

        public PessoaFuncionario(
            Guid pessoaId,
            ETipoCargo tipoCargo,
            decimal valorPercentualComissao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PessoaFuncionario>()
                .Requires()
                .IsTrue(Enum.IsDefined(typeof(ETipoCargo), tipoCargo), nameof(TipoCargo), "TipoCargo não consta na lista [Origem: PessoaFuncionario]")
                .IsLowerOrEqualsThan(valorPercentualComissao, 100, nameof(ValorPercentualComissao), "O campo ValorPercentualComissao deve ter o valor máximo de 100 [Origem: PessoaFuncionario]")
                .IsGreaterOrEqualsThan(valorPercentualComissao, 0, nameof(ValorPercentualComissao), "O campo ValorPercentualComissao não pode ser menor que zero")
            );

            PessoaId = pessoaId;
            TipoCargo = tipoCargo;
            ValorPercentualComissao = valorPercentualComissao;
        }
    }
}
