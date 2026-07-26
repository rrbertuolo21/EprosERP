using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Conta contábil em árvore (EF FIN-CGL §5.8 / §11.6 cgl_conta_contabil).
    /// Contabilidade plena — capacidade de evolução especificada. Sobe desabilitada (ABAC nega por padrão).
    /// FKs de cadastro (cliente/fornecedor/colaborador/banco/tipo despesa) são Guid? cross-module — sem navegação.
    /// </summary>
    public class ContaContabil : EntidadeSaaSBase
    {
        public string CodigoConta { get; private set; } = string.Empty;
        public string NomeConta { get; private set; } = string.Empty;
        public Guid? ContaPaiId { get; private set; }
        public string? NomeContaPai { get; private set; }
        public int Nivel { get; private set; }
        public ETipoContaContabil TipoConta { get; private set; }
        public bool AceitaLancamento { get; private set; }
        public bool ParticipaContabilidadeGeral { get; private set; }
        public bool ParticipaOrcamento { get; private set; }
        public bool ParticipaDepreciacao { get; private set; }

        // Contas automáticas (origem cross-module, apenas por Guid FK)
        public Guid? ClienteId { get; private set; }
        public Guid? FornecedorId { get; private set; }
        public Guid? ColaboradorId { get; private set; }
        public Guid? BancoId { get; private set; }
        public Guid? TipoDespesaId { get; private set; }

        public bool Ativo { get; private set; } = true;

        public ContaContabil? ContaPai { get; private set; }

        protected ContaContabil() { } // EF Core

        public ContaContabil(
            string codigoConta,
            string nomeConta,
            Guid? contaPaiId,
            int nivel,
            ETipoContaContabil tipoConta,
            bool aceitaLancamento,
            bool participaContabilidadeGeral,
            bool participaOrcamento,
            bool participaDepreciacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            CodigoConta = codigoConta;
            NomeConta = nomeConta;
            ContaPaiId = contaPaiId;
            Nivel = nivel;
            TipoConta = tipoConta;
            AceitaLancamento = aceitaLancamento;
            ParticipaContabilidadeGeral = participaContabilidadeGeral;
            ParticipaOrcamento = participaOrcamento;
            ParticipaDepreciacao = participaDepreciacao;
            Ativo = true;
            Validar();
        }

        public void Alterar(
            string codigoConta,
            string nomeConta,
            Guid? contaPaiId,
            int nivel,
            ETipoContaContabil tipoConta,
            bool aceitaLancamento,
            bool participaContabilidadeGeral,
            bool participaOrcamento,
            bool participaDepreciacao,
            string usuario)
        {
            CodigoConta = codigoConta;
            NomeConta = nomeConta;
            ContaPaiId = contaPaiId;
            Nivel = nivel;
            TipoConta = tipoConta;
            AceitaLancamento = aceitaLancamento;
            ParticipaContabilidadeGeral = participaContabilidadeGeral;
            ParticipaOrcamento = participaOrcamento;
            ParticipaDepreciacao = participaDepreciacao;
            MarcarAlterado(usuario);
            Validar();
        }

        public void Inativar(string usuario) { Ativo = false; MarcarAlterado(usuario); }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ContaContabil>()
                .Requires()
                .IsNotNullOrEmpty(CodigoConta, nameof(CodigoConta), "O código da conta é obrigatório.")
                .IsNotNullOrEmpty(NomeConta, nameof(NomeConta), "O nome da conta é obrigatório.")
                .IsLowerOrEqualsThan(NomeConta?.Length ?? 0, 100, nameof(NomeConta), "O campo NomeConta deve ter no máximo 100 caracteres [Origem: ContaContabil]")
                .IsGreaterOrEqualsThan(Nivel, 0, nameof(Nivel), "O nível da conta deve ser maior ou igual a zero.")
            );
            // Uma conta não pode ser pai de si mesma (EF §10.3: não pode ser movida para si mesma ou descendente).
            if (ContaPaiId.HasValue && ContaPaiId.Value == Id)
                AddNotification(nameof(ContaPaiId), "A conta não pode ser pai de si mesma.");
        }
    }
}
