using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Declaração de importação de um item de compra. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraItemImportacao. Os ValueObjects CNPJ/CPF foram achatados para strings.
    /// </summary>
    public class CompraItemImportacao : EntidadeSaaSBase
    {
        public Guid CompraItemId { get; private set; }
        public string NumeroDeclaracaoImportacao { get; private set; } = string.Empty;
        public DateTime DataDeclaracaoImportacao { get; private set; }
        public string LocalDesembaraco { get; private set; } = string.Empty;
        public string UfDesembaraco { get; private set; } = string.Empty;
        public DateTime DataDesembaraco { get; private set; }
        public ETipoViaTransporte TipoViaTransporte { get; private set; }
        public decimal ValorAFRMM { get; private set; }
        public ETipoIntermedioImportacao TipoIntermedio { get; private set; }
        public string? Cnpj { get; private set; }
        public string? Cpf { get; private set; }
        public string? UfTerceiro { get; private set; }
        public string CodigoExportador { get; private set; } = string.Empty;

        // Navegação intra-módulo
        public ICollection<CompraItemImportacaoAdicao> Adicoes { get; private set; } = new List<CompraItemImportacaoAdicao>();
        public CompraItem? CompraItem { get; private set; }

        protected CompraItemImportacao() { } // EF Core

        public CompraItemImportacao(Guid compraItemId, string numeroDeclaracaoImportacao, DateTime dataDeclaracaoImportacao, string localDesembaraco, string ufDesembaraco, DateTime dataDesembaraco, ETipoViaTransporte tipoViaTransporte, decimal valorAFRMM, ETipoIntermedioImportacao tipoIntermedio, string? cnpj, string? cpf, string? ufTerceiro, string codigoExportador, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraItemId = compraItemId;
            NumeroDeclaracaoImportacao = numeroDeclaracaoImportacao ?? string.Empty;
            DataDeclaracaoImportacao = dataDeclaracaoImportacao;
            LocalDesembaraco = localDesembaraco ?? string.Empty;
            UfDesembaraco = ufDesembaraco ?? string.Empty;
            DataDesembaraco = dataDesembaraco;
            TipoViaTransporte = tipoViaTransporte;
            ValorAFRMM = valorAFRMM;
            TipoIntermedio = tipoIntermedio;
            Cnpj = cnpj;
            Cpf = cpf;
            UfTerceiro = ufTerceiro;
            CodigoExportador = codigoExportador ?? string.Empty;
        }

        public void Alterar(string numeroDeclaracaoImportacao, DateTime dataDeclaracaoImportacao, string localDesembaraco, string ufDesembaraco, DateTime dataDesembaraco, ETipoViaTransporte tipoViaTransporte, decimal valorAFRMM, ETipoIntermedioImportacao tipoIntermedio, string? cnpj, string? cpf, string? ufTerceiro, string codigoExportador, string usuario)
        {
            NumeroDeclaracaoImportacao = numeroDeclaracaoImportacao ?? string.Empty;
            DataDeclaracaoImportacao = dataDeclaracaoImportacao;
            LocalDesembaraco = localDesembaraco ?? string.Empty;
            UfDesembaraco = ufDesembaraco ?? string.Empty;
            DataDesembaraco = dataDesembaraco;
            TipoViaTransporte = tipoViaTransporte;
            ValorAFRMM = valorAFRMM;
            TipoIntermedio = tipoIntermedio;
            Cnpj = cnpj;
            Cpf = cpf;
            UfTerceiro = ufTerceiro;
            CodigoExportador = codigoExportador ?? string.Empty;
            MarcarAlterado(usuario);
        }

        public void DeletarComAdicoes(string usuario)
        {
            Deletar(usuario);
            foreach (var adicao in Adicoes)
                adicao.Deletar(usuario);
        }

        public void AdicionarAdicao(int numeroAdicao, int numeroSequencialAdicao, string codigoFabricante, decimal valorDesconto, string? numeroAtoConcessorio, string usuario)
        {
            Adicoes.Add(new CompraItemImportacaoAdicao(Id, numeroAdicao, numeroSequencialAdicao, codigoFabricante, valorDesconto, numeroAtoConcessorio, TenantId, usuario));
        }

        public void AlterarAdicao(Guid compraItemImportacaoAdicaoId, int numeroAdicao, int numeroSequencialAdicao, string codigoFabricante, decimal valorDesconto, string? numeroAtoConcessorio, string usuario)
        {
            var localizado = Adicoes.FirstOrDefault(a => a.Id == compraItemImportacaoAdicaoId);
            localizado?.Alterar(numeroAdicao, numeroSequencialAdicao, codigoFabricante, valorDesconto, numeroAtoConcessorio, usuario);
        }

        public void ExcluirAdicao(Guid compraItemImportacaoAdicaoId, string usuario)
        {
            var localizado = Adicoes.FirstOrDefault(a => a.Id == compraItemImportacaoAdicaoId);
            localizado?.Deletar(usuario);
        }
    }
}
