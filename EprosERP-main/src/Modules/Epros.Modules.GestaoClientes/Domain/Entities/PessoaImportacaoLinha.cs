using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Linha de um lote de importação de pessoas (CAD-PEM 12.15).</summary>
    public class PessoaImportacaoLinha : EntidadeSaaSBase
    {
        public Guid LoteId { get; private set; }
        public int NumeroLinha { get; private set; }
        public string DadosOriginais { get; private set; } = string.Empty;
        public EStatusLinhaImportacao Status { get; private set; }
        public string? MensagemErro { get; private set; }
        public Guid? PessoaIdGerada { get; private set; }

        protected PessoaImportacaoLinha() { } // EF Core

        public PessoaImportacaoLinha(
            Guid loteId,
            int numeroLinha,
            string dadosOriginais,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            LoteId = loteId;
            NumeroLinha = numeroLinha;
            DadosOriginais = dadosOriginais;
            Status = EStatusLinhaImportacao.Pendente;
            Validar();
        }

        public void Aceitar(Guid pessoaIdGerada, string alteradoPor)
        {
            Status = EStatusLinhaImportacao.Aceita;
            PessoaIdGerada = pessoaIdGerada;
            MensagemErro = null;
            MarcarAlterado(alteradoPor);
        }

        public void Rejeitar(string mensagemErro, string alteradoPor)
        {
            Status = EStatusLinhaImportacao.Rejeitada;
            MensagemErro = mensagemErro;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PessoaImportacaoLinha>()
                .Requires()
                .AreNotEquals(LoteId, Guid.Empty, nameof(LoteId), "LoteId é obrigatório [Origem: PessoaImportacaoLinha]")
                .IsGreaterThan(NumeroLinha, 0, nameof(NumeroLinha), "NumeroLinha deve ser maior que zero [Origem: PessoaImportacaoLinha]")
                .IsNotNullOrEmpty(DadosOriginais, nameof(DadosOriginais), "DadosOriginais é obrigatório [Origem: PessoaImportacaoLinha]")
            );
        }
    }
}
