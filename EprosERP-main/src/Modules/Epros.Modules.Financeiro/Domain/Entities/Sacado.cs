using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Sacado de cobrança (EF FIN-SF §7.3 / §11.4 sf_sacado).
    /// Dados de cobrança usados em fatura, boleto, e-mail e portal do sacado.
    /// PessoaId referencia o cadastro base (módulo Plataforma) por Guid, quando definido.
    /// </summary>
    public class Sacado : EntidadeSaaSBase
    {
        public Guid? PessoaId { get; private set; }
        public Guid? GrupoRecorrenciaId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string? Documento { get; private set; }
        public string? RG { get; private set; }
        public string? Inscricao { get; private set; }
        public string? Endereco { get; private set; }
        public string? Numero { get; private set; }
        public string? Complemento { get; private set; }
        public string? Bairro { get; private set; }
        public string? Cidade { get; private set; }
        public string? UF { get; private set; }
        public string? CEP { get; private set; }
        public string? Telefone { get; private set; }
        public string? Email { get; private set; }
        public string? Observacao { get; private set; }
        public decimal Valor { get; private set; }
        public bool Bloqueado { get; private set; }

        public GrupoRecorrencia? GrupoRecorrencia { get; private set; }

        protected Sacado() { } // EF Core

        public Sacado(
            Guid? pessoaId, Guid? grupoRecorrenciaId, string nome, string? documento, string? rg, string? inscricao,
            string? endereco, string? numero, string? complemento, string? bairro, string? cidade, string? uf,
            string? cep, string? telefone, string? email, string? observacao, decimal valor,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PessoaId = pessoaId; GrupoRecorrenciaId = grupoRecorrenciaId; Nome = nome; Documento = documento;
            RG = rg; Inscricao = inscricao; Endereco = endereco; Numero = numero; Complemento = complemento;
            Bairro = bairro; Cidade = cidade; UF = uf; CEP = cep; Telefone = telefone; Email = email;
            Observacao = observacao; Valor = valor;
            Validar();
        }

        public void Alterar(
            Guid? pessoaId, Guid? grupoRecorrenciaId, string nome, string? documento, string? rg, string? inscricao,
            string? endereco, string? numero, string? complemento, string? bairro, string? cidade, string? uf,
            string? cep, string? telefone, string? email, string? observacao, decimal valor, string usuario)
        {
            PessoaId = pessoaId; GrupoRecorrenciaId = grupoRecorrenciaId; Nome = nome; Documento = documento;
            RG = rg; Inscricao = inscricao; Endereco = endereco; Numero = numero; Complemento = complemento;
            Bairro = bairro; Cidade = cidade; UF = uf; CEP = cep; Telefone = telefone; Email = email;
            Observacao = observacao; Valor = valor;
            MarcarAlterado(usuario);
            Validar();
        }

        public void Bloquear(string usuario) { Bloqueado = true; MarcarAlterado(usuario); }
        public void Desbloquear(string usuario) { Bloqueado = false; MarcarAlterado(usuario); }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Sacado>()
                .Requires()
                .IsNotNullOrEmpty(Nome, nameof(Nome), "O nome do sacado é obrigatório.")
            );
        }
    }
}
