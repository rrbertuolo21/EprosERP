using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Configuração do cedente e parâmetros de boleto (EF FIN-SF §7.2 / §11.3 sf_configuracao_cedente).
    /// Dados do beneficiário, logo, parâmetros de vencimento, multa, juros e instruções de boleto.
    /// </summary>
    public class ConfiguracaoCedente : EntidadeSaaSBase
    {
        /// <summary>Empresa dona da configuração (FK ao módulo Plataforma por Guid).</summary>
        public Guid EmpresaId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string? Email { get; private set; }
        public string? Documento { get; private set; }
        public string? Endereco { get; private set; }
        public string? Numero { get; private set; }
        public string? Bairro { get; private set; }
        public string? Cidade { get; private set; }
        public string? Cep { get; private set; }
        public string? UF { get; private set; }
        public string? Logo { get; private set; }
        public int ReceberAteDias { get; private set; }
        public int DiasAntecedencia { get; private set; }
        public decimal MultaAtraso { get; private set; }
        public decimal Juro { get; private set; }
        public string? Instrucao1 { get; private set; }
        public string? Instrucao2 { get; private set; }
        public string? Instrucao3 { get; private set; }
        public string? Instrucao4 { get; private set; }

        protected ConfiguracaoCedente() { } // EF Core

        public ConfiguracaoCedente(
            Guid empresaId, string nome, string? email, string? documento, string? endereco, string? numero,
            string? bairro, string? cidade, string? cep, string? uf, string? logo,
            int receberAteDias, int diasAntecedencia, decimal multaAtraso, decimal juro,
            string? instrucao1, string? instrucao2, string? instrucao3, string? instrucao4,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            Nome = nome; Email = email; Documento = documento; Endereco = endereco; Numero = numero;
            Bairro = bairro; Cidade = cidade; Cep = cep; UF = uf; Logo = logo;
            ReceberAteDias = receberAteDias; DiasAntecedencia = diasAntecedencia; MultaAtraso = multaAtraso; Juro = juro;
            Instrucao1 = instrucao1; Instrucao2 = instrucao2; Instrucao3 = instrucao3; Instrucao4 = instrucao4;
            Validar();
        }

        public void Alterar(
            string nome, string? email, string? documento, string? endereco, string? numero,
            string? bairro, string? cidade, string? cep, string? uf, string? logo,
            int receberAteDias, int diasAntecedencia, decimal multaAtraso, decimal juro,
            string? instrucao1, string? instrucao2, string? instrucao3, string? instrucao4, string usuario)
        {
            Nome = nome; Email = email; Documento = documento; Endereco = endereco; Numero = numero;
            Bairro = bairro; Cidade = cidade; Cep = cep; UF = uf; Logo = logo;
            ReceberAteDias = receberAteDias; DiasAntecedencia = diasAntecedencia; MultaAtraso = multaAtraso; Juro = juro;
            Instrucao1 = instrucao1; Instrucao2 = instrucao2; Instrucao3 = instrucao3; Instrucao4 = instrucao4;
            MarcarAlterado(usuario);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ConfiguracaoCedente>()
                .Requires()
                .AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "A empresa do cedente é obrigatória.")
                .IsNotNullOrEmpty(Nome, nameof(Nome), "O nome do cedente é obrigatório.")
            );
        }
    }
}
