using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    public class Colaborador : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string Cpf { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Cargo { get; private set; } = string.Empty;
        public string Departamento { get; private set; } = string.Empty;
        public decimal SalarioBase { get; private set; }
        public DateTime DataAdmissao { get; private set; }
        public DateTime? DataDemissao { get; private set; }
        public string Status { get; private set; } = "Ativo"; // Ativo, Desligado

        protected Colaborador() { } // EF Core

        public Colaborador(
            string nome,
            string cpf,
            string email,
            string cargo,
            string departamento,
            decimal salarioBase,
            DateTime dataAdmissao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Colaborador>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O Nome do colaborador é obrigatório.")
                .IsNotNullOrEmpty(cpf, nameof(Cpf), "O CPF do colaborador é obrigatório.")
                .IsNotNullOrEmpty(email, nameof(Email), "O E-mail é obrigatório.")
                .IsEmail(email, nameof(Email), "E-mail em formato inválido.")
                .IsNotNullOrEmpty(cargo, nameof(Cargo), "O Cargo é obrigatório.")
                .IsNotNullOrEmpty(departamento, nameof(Departamento), "O Departamento é obrigatório.")
                .IsGreaterThan(salarioBase, 0, nameof(SalarioBase), "O Salário Base deve ser maior que zero.")
            );

            Nome = nome;
            Cpf = cpf;
            Email = email;
            Cargo = cargo;
            Departamento = departamento;
            SalarioBase = salarioBase;
            DataAdmissao = dataAdmissao;
            Status = "Ativo";
        }

        public void Desligar(DateTime dataDemissao, string usuario)
        {
            if (Status != "Ativo")
            {
                AddNotification(nameof(Status), "Este colaborador já está desligado.");
                return;
            }

            if (dataDemissao < DataAdmissao)
            {
                AddNotification(nameof(DataDemissao), "A data de demissão não pode ser anterior à data de admissão.");
                return;
            }

            Status = "Desligado";
            DataDemissao = dataDemissao;
            MarcarAlterado(usuario);
        }
    }
}
