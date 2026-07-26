using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class Plano : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public decimal Preco { get; private set; }
        public bool Ativo { get; private set; }
        public Guid? GrupoPlanoId { get; private set; }
        public int LimiteUsuarios { get; private set; }
        public int LimiteEmpresas { get; private set; }
        public string? RecursosInclusos { get; private set; }
        public string? DescricaoCurta { get; private set; }
        public string? DescricaoCompleta { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public List<ModuloPlano> Modulos { get; private set; } = new();

        protected Plano() { } // EF Core

        public Plano(
            string nome,
            decimal preco,
            Guid? grupoPlanoId,
            int limiteUsuarios,
            int limiteEmpresas,
            string? recursosInclusos,
            string tenantId,
            string criadoPor,
            string? descricaoCurta = null,
            string? descricaoCompleta = null,
            DateTime? dataInicio = null,
            DateTime? dataFim = null)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Plano>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome do plano é obrigatório")
                .IsGreaterThan(preco, -0.01m, nameof(Preco), "Preço do plano deve ser maior ou igual a zero")
                .IsGreaterThan(limiteUsuarios, 0, nameof(LimiteUsuarios), "Limite de usuários deve ser maior que zero")
                .IsGreaterThan(limiteEmpresas, 0, nameof(LimiteEmpresas), "Limite de empresas deve ser maior que zero")
            );

            Nome = nome;
            Preco = preco;
            GrupoPlanoId = grupoPlanoId;
            LimiteUsuarios = limiteUsuarios;
            LimiteEmpresas = limiteEmpresas;
            RecursosInclusos = recursosInclusos;
            DescricaoCurta = descricaoCurta;
            DescricaoCompleta = descricaoCompleta;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Ativo = true;
        }

        public Plano(string nome, decimal preco, string tenantId, string criadoPor)
            : this(nome, preco, null, 999, 99, null, tenantId, criadoPor)
        {
        }

        public void AdicionarModulo(
            string nomeModulo,
            string criadoPor,
            string? moduloGeralId = null,
            string? descricao = null,
            decimal valor = 0m,
            bool ativo = true)
        {
            var modulo = new ModuloPlano(nomeModulo, Id, TenantId, criadoPor, moduloGeralId, descricao, valor, ativo);
            Modulos.Add(modulo);
        }

        public void AtualizarPreco(decimal novoPreco, string alteradoPor)
        {
            AddNotifications(new Contract<Plano>()
                .Requires()
                .IsGreaterThan(novoPreco, 0, nameof(Preco), "Preço do plano deve ser maior que zero")
            );

            if (IsValid)
            {
                Preco = novoPreco;
                MarcarAlterado(alteradoPor);
            }
        }

        /// <summary>Atualiza os dados escalares do plano (não mexe na coleção de módulos).</summary>
        public void Atualizar(
            string nome,
            decimal preco,
            Guid? grupoPlanoId,
            int limiteUsuarios,
            int limiteEmpresas,
            string? recursosInclusos,
            string alteradoPor,
            string? descricaoCurta = null,
            string? descricaoCompleta = null,
            DateTime? dataInicio = null,
            DateTime? dataFim = null)
        {
            AddNotifications(new Contract<Plano>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome do plano é obrigatório")
                .IsGreaterThan(preco, -0.01m, nameof(Preco), "Preço do plano deve ser maior ou igual a zero")
                .IsGreaterThan(limiteUsuarios, 0, nameof(LimiteUsuarios), "Limite de usuários deve ser maior que zero")
                .IsGreaterThan(limiteEmpresas, 0, nameof(LimiteEmpresas), "Limite de empresas deve ser maior que zero")
            );

            if (!IsValid) return;

            Nome = nome;
            Preco = preco;
            GrupoPlanoId = grupoPlanoId;
            LimiteUsuarios = limiteUsuarios;
            LimiteEmpresas = limiteEmpresas;
            RecursosInclusos = recursosInclusos;
            DescricaoCurta = descricaoCurta;
            DescricaoCompleta = descricaoCompleta;
            DataInicio = dataInicio;
            DataFim = dataFim;
            MarcarAlterado(alteradoPor);
        }

        public void DefinirAtivo(bool ativo, string alteradoPor)
        {
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
        }

        public void RemoverModulo(ModuloPlano modulo)
        {
            Modulos.Remove(modulo);
        }

        public void LimparModulos()
        {
            Modulos.Clear();
        }
    }
}
