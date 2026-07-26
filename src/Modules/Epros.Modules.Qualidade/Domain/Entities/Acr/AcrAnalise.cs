using System;
using System.Collections.Generic;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Qualidade.Domain.Entities
{
    /// <summary>
    /// qld_acr_analise — Registro central da analise de aceite/rejeicao.
    /// Porte fiel da EF QUALIDADE / ANALISE_DE_ACEITACAO_E_REJEICAO (secao 12.1).
    /// </summary>
    public class AcrAnalise : EntidadeSaaSBase
    {
        public long? SequenciaExibicao { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public ETipoAnaliseAcr TipoAnalise { get; private set; }
        public EStatusRegistroQualidade Status { get; private set; }
        public Guid ResponsavelId { get; private set; }
        public Guid? LocalId { get; private set; }
        public Guid? DocumentoFiscalId { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public int Versao { get; private set; }

        private readonly List<AcrItem> _itens = new();
        public IReadOnlyCollection<AcrItem> Itens => _itens.AsReadOnly();

        private readonly List<AcrResultado> _resultados = new();
        public IReadOnlyCollection<AcrResultado> Resultados => _resultados.AsReadOnly();

        protected AcrAnalise() { }

        public AcrAnalise(string codigo, string descricao, ETipoAnaliseAcr tipoAnalise, Guid responsavelId,
            Guid? localId, Guid? documentoFiscalId, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            TipoAnalise = tipoAnalise;
            ResponsavelId = responsavelId;
            LocalId = localId;
            DocumentoFiscalId = documentoFiscalId;
            Status = EStatusRegistroQualidade.Rascunho;
            DataCriacao = DateTime.UtcNow;
            Versao = 1;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<AcrAnalise>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo da analise e obrigatorio [Origem: AcrAnalise]")
                .IsLowerOrEqualsThan(Codigo?.Length ?? 0, 30, nameof(Codigo), "O codigo deve ter no maximo 30 caracteres [Origem: AcrAnalise]")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descricao da analise e obrigatoria [Origem: AcrAnalise]")
                .IsLowerOrEqualsThan(Descricao?.Length ?? 0, 500, nameof(Descricao), "A descricao deve ter no maximo 500 caracteres [Origem: AcrAnalise]")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel pela analise e obrigatorio [Origem: AcrAnalise]"));
        }

        public void Submeter(string usuario)
        {
            if (_itens.Count == 0)
            {
                AddNotification(nameof(Itens), "A analise deve ter ao menos um item para ser submetida [Origem: AcrAnalise]");
                return;
            }
            Status = EStatusRegistroQualidade.EmAnalise;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void Encerrar(string usuario)
        {
            Status = EStatusRegistroQualidade.Encerrado;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void AdicionarItem(AcrItem item) => _itens.Add(item);
        public void AdicionarResultado(AcrResultado resultado) => _resultados.Add(resultado);
    }
}
