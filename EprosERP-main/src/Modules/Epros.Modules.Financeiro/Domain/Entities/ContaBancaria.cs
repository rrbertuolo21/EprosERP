using System;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    public class ContaBancaria : EntidadeSaaSBase
    {
        /// <summary>Sequência legada (SequenciaTenantId) — somente exibição/UX. Não substitui o Id (Guid).</summary>
        public long? SequenciaExibicao { get; private set; }

        /// <summary>Empresa dona da conta (legado EmpresaID — obrigatório). Referência ao módulo Plataforma por Guid FK, sem navegação cruzada.</summary>
        public Guid EmpresaId { get; private set; }
        public Guid BancoId { get; private set; }
        public ETipoContaBancaria TipoContaBancaria { get; private set; }
        public string Apelido { get; private set; } = string.Empty;
        public string Titular { get; private set; } = string.Empty;
        public string Agencia { get; private set; } = string.Empty;
        public string Conta { get; private set; } = string.Empty;
        public string? Gerente { get; private set; }
        public string? FoneGerente { get; private set; }
        public string? Detalhe { get; private set; }
        public string? DigitoAgencia { get; private set; }
        public DateTime? DataEncerramento { get; private set; }

        // Navigation
        public Banco Banco { get; private set; } = null!;

        protected ContaBancaria() { } // EF Core

        public ContaBancaria(
            Guid empresaId,
            Guid bancoId,
            ETipoContaBancaria tipoContaBancaria,
            string apelido,
            string titular,
            string agencia,
            string conta,
            string? gerente,
            string? foneGerente,
            string? detalhe,
            string? digitoAgencia,
            DateTime? dataEncerramento,
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            BancoId = bancoId;
            TipoContaBancaria = tipoContaBancaria;
            Apelido = apelido;
            Titular = titular;
            Agencia = agencia;
            Conta = conta;
            Gerente = gerente;
            FoneGerente = foneGerente;
            Detalhe = detalhe;
            DigitoAgencia = digitoAgencia;
            DataEncerramento = dataEncerramento;
            Validar();
        }

        public void Alterar(
            Guid empresaId,
            Guid bancoId,
            ETipoContaBancaria tipoContaBancaria,
            string apelido,
            string titular,
            string agencia,
            string conta,
            string? gerente,
            string? foneGerente,
            string? detalhe,
            string? digitoAgencia,
            DateTime? dataEncerramento,
            string alteradoPor)
        {
            EmpresaId = empresaId;
            BancoId = bancoId;
            TipoContaBancaria = tipoContaBancaria;
            Apelido = apelido;
            Titular = titular;
            Agencia = agencia;
            Conta = conta;
            Gerente = gerente;
            FoneGerente = foneGerente;
            Detalhe = detalhe;
            DigitoAgencia = digitoAgencia;
            DataEncerramento = dataEncerramento;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ContaBancaria>()
                .Requires()
                .AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "A empresa é obrigatória [Origem: ContaBancaria]")
                .AreNotEquals(BancoId, Guid.Empty, nameof(BancoId), "O banco é obrigatório")
                .IsNotNullOrEmpty(Apelido, nameof(Apelido), "O apelido é obrigatório")
                .IsLowerOrEqualsThan(Apelido ?? string.Empty, 150, nameof(Apelido), "O apelido deve ter no máximo 150 caracteres")
                .IsNotNullOrEmpty(Titular, nameof(Titular), "O titular é obrigatório")
                .IsLowerOrEqualsThan(Titular ?? string.Empty, 150, nameof(Titular), "O titular deve ter no máximo 150 caracteres")
                .IsNotNullOrEmpty(Agencia, nameof(Agencia), "A agência é obrigatória")
                .IsLowerOrEqualsThan(Agencia ?? string.Empty, 20, nameof(Agencia), "A agência deve ter no máximo 20 caracteres")
                .IsNotNullOrEmpty(Conta, nameof(Conta), "A conta é obrigatória")
                .IsLowerOrEqualsThan(Conta ?? string.Empty, 20, nameof(Conta), "A conta deve ter no máximo 20 caracteres")
                .IsLowerOrEqualsThan(Gerente ?? string.Empty, 150, nameof(Gerente), "O nome do gerente deve ter no máximo 150 caracteres")
                .IsLowerOrEqualsThan(FoneGerente ?? string.Empty, 150, nameof(FoneGerente), "O telefone do gerente deve ter no máximo 150 caracteres")
                .IsLowerOrEqualsThan(Detalhe ?? string.Empty, 1000, nameof(Detalhe), "O detalhe deve ter no máximo 1000 caracteres")
                .IsLowerOrEqualsThan(DigitoAgencia ?? string.Empty, 2, nameof(DigitoAgencia), "O dígito da agência deve ter no máximo 2 caracteres")
            );
        }
    }
}
