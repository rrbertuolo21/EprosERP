using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Controle de lote de importação de pessoas (CAD-PEM 12.15).</summary>
    public class PessoaImportacaoLote : EntidadeSaaSBase
    {
        public string NomeArquivo { get; private set; } = string.Empty;
        public string LayoutVersao { get; private set; } = string.Empty;
        public EStatusImportacao Status { get; private set; }
        public int TotalLinhas { get; private set; }
        public int LinhasAceitas { get; private set; }
        public int LinhasRejeitadas { get; private set; }

        public List<PessoaImportacaoLinha> Linhas { get; private set; } = new();

        protected PessoaImportacaoLote() { } // EF Core

        public PessoaImportacaoLote(
            string nomeArquivo,
            string layoutVersao,
            int totalLinhas,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            NomeArquivo = nomeArquivo;
            LayoutVersao = layoutVersao;
            Status = EStatusImportacao.Pendente;
            TotalLinhas = totalLinhas;
            LinhasAceitas = 0;
            LinhasRejeitadas = 0;
            Validar();
        }

        public void RegistrarResultado(int linhasAceitas, int linhasRejeitadas, EStatusImportacao status, string alteradoPor)
        {
            LinhasAceitas = linhasAceitas;
            LinhasRejeitadas = linhasRejeitadas;
            Status = status;
            MarcarAlterado(alteradoPor);
        }

        public void AdicionarLinha(PessoaImportacaoLinha linha)
        {
            if (linha == null) return;
            Linhas.Add(linha);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PessoaImportacaoLote>()
                .Requires()
                .IsNotNullOrEmpty(NomeArquivo, nameof(NomeArquivo), "NomeArquivo é obrigatório [Origem: PessoaImportacaoLote]")
                .HasMaxLen(NomeArquivo ?? string.Empty, 260, nameof(NomeArquivo), "NomeArquivo deve ter no máximo 260 caracteres [Origem: PessoaImportacaoLote]")
                .IsNotNullOrEmpty(LayoutVersao, nameof(LayoutVersao), "LayoutVersao é obrigatório [Origem: PessoaImportacaoLote]")
                .HasMaxLen(LayoutVersao ?? string.Empty, 50, nameof(LayoutVersao), "LayoutVersao deve ter no máximo 50 caracteres [Origem: PessoaImportacaoLote]")
                .IsGreaterOrEqualsThan(TotalLinhas, 0, nameof(TotalLinhas), "TotalLinhas deve ser maior ou igual a zero [Origem: PessoaImportacaoLote]")
            );
        }
    }
}
