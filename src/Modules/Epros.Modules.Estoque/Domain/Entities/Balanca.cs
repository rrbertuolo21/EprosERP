using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Configuração de balança para leitura de código de produto. Porte fiel do legado Epros.ERP.Domain.Entities.Cadastros.Produtos.Balanca.
    /// </summary>
    public class Balanca : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public int QntDigitoIdentificador { get; private set; }
        public int QntDigitoCodigoProduto { get; private set; }
        public int QntDigitoValorProduto { get; private set; }
        public int QntCasaDecimal { get; private set; }
        public ETipoValorBalanca TipoValor { get; private set; }

        // Navegação intra-módulo
        public ICollection<Produto> Produtos { get; private set; } = new List<Produto>();

        protected Balanca() { } // EF Core

        public Balanca(string nome, int qntDigitoIdentificador, int qntDigitoCodigoProduto, int qntDigitoValorProduto, int qntCasaDecimal, ETipoValorBalanca tipoValor, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            QntDigitoIdentificador = qntDigitoIdentificador;
            QntDigitoCodigoProduto = qntDigitoCodigoProduto;
            QntDigitoValorProduto = qntDigitoValorProduto;
            QntCasaDecimal = qntCasaDecimal;
            TipoValor = tipoValor;
            Validar();
        }

        public void Validar()
        {
            AddNotifications(new Contract<Balanca>()
                .Requires()
                .IsLowerOrEqualsThan((Nome ?? "").Length, 60, nameof(Nome), "O campo Nome deve ter no máximo 60 caracteres [Origem: Balanca]")
                .IsGreaterThan(QntDigitoIdentificador, 0, nameof(QntDigitoIdentificador), "A Quantidade de Digitos do Identificador deve ser maior que Zero. [Origem: Balanca]")
                .IsGreaterThan(QntDigitoCodigoProduto, 0, nameof(QntDigitoCodigoProduto), "A Quantidade de Digitos do Código do Produto deve ser maior que Zero. [Origem: Balanca]")
                .IsGreaterThan(QntDigitoValorProduto, 0, nameof(QntDigitoValorProduto), "A Quantidade de Digitos do Valor do Produto deve ser maior que Zero. [Origem: Balanca]")
                .IsGreaterThan(QntCasaDecimal, 0, nameof(QntCasaDecimal), "A Quantidade de Casas Decimais deve ser maior que Zero. [Origem: Balanca]")
                .IsTrue(Enum.IsDefined(typeof(ETipoValorBalanca), TipoValor), nameof(TipoValor), "O Tipo de Valor não consta na lista [Origem: Balanca]")
            );
        }

        public void Alterar(string nome, int qntDigitoIdentificador, int qntDigitoCodigoProduto, int qntDigitoValorProduto, int qntCasaDecimal, ETipoValorBalanca tipoValor, string usuario)
        {
            Nome = nome;
            QntDigitoIdentificador = qntDigitoIdentificador;
            QntDigitoCodigoProduto = qntDigitoCodigoProduto;
            QntDigitoValorProduto = qntDigitoValorProduto;
            QntCasaDecimal = qntCasaDecimal;
            TipoValor = tipoValor;
            MarcarAlterado(usuario);
            Validar();
        }

        /// <summary>
        /// Deleção condicionada: só permite excluir se nenhum produto estiver vinculado a esta balança.
        /// Porte fiel da regra do legado Balanca.Deletar().
        /// </summary>
        public void DeletarSeNaoVinculada(string usuario)
        {
            var produtoLocalizado = Produtos?.FirstOrDefault(x => x.BalancaId == Id);

            if (produtoLocalizado == null)
                Deletar(usuario);
            else
                AddNotification("Delecao", $"Deleção não permitida. Este ID {Id} já está vinculado ao Produto ID {produtoLocalizado.Id} [Origem: Balanca]");
        }
    }
}
