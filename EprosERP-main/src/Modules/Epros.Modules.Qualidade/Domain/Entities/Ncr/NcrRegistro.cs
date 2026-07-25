using System;
using System.Collections.Generic;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Qualidade.Domain.Entities
{
    /// <summary>
    /// qld_ncr_registro — Registro mestre da Nao Conformidade (NCR).
    /// Porte fiel da EF QUALIDADE / NAO_CONFORMIDADES_NCR (secao 11.1).
    /// </summary>
    public class NcrRegistro : EntidadeSaaSBase
    {
        public long? SequenciaExibicao { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Titulo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public ENcrOrigem OrigemPrincipal { get; private set; }
        public string? Severidade { get; private set; } // dominio nao informado no material
        public ENcrPrioridade Prioridade { get; private set; }
        public EStatusRegistroQualidade StatusRegistro { get; private set; }
        public ENcrEtapa EtapaNcr { get; private set; }
        public Guid ResponsavelId { get; private set; }
        public Guid? AreaResponsavelId { get; private set; }
        public Guid? ProdutoId { get; private set; }
        public Guid? LoteId { get; private set; }
        public string? Serial { get; private set; }
        public Guid? ClienteId { get; private set; }
        public DateTime? DataOcorrencia { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public DateTime? DataLimiteTriagem { get; private set; }
        public DateTime? DataEncerramento { get; private set; }
        public string? Conclusao { get; private set; }
        public string? MotivoCancelamento { get; private set; }
        public int Versao { get; private set; }

        private readonly List<NcrOrigemRef> _origens = new();
        public IReadOnlyCollection<NcrOrigemRef> Origens => _origens.AsReadOnly();

        private readonly List<NcrCausaRaiz> _causasRaiz = new();
        public IReadOnlyCollection<NcrCausaRaiz> CausasRaiz => _causasRaiz.AsReadOnly();

        private readonly List<NcrAcaoCapa> _acoesCapa = new();
        public IReadOnlyCollection<NcrAcaoCapa> AcoesCapa => _acoesCapa.AsReadOnly();

        protected NcrRegistro() { } // EF Core

        public NcrRegistro(
            string codigo,
            string titulo,
            string descricao,
            ENcrOrigem origemPrincipal,
            ENcrPrioridade prioridade,
            Guid responsavelId,
            string? severidade,
            DateTime? dataOcorrencia,
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Titulo = titulo;
            Descricao = descricao;
            OrigemPrincipal = origemPrincipal;
            Prioridade = prioridade;
            ResponsavelId = responsavelId;
            Severidade = severidade;
            DataOcorrencia = dataOcorrencia;
            StatusRegistro = EStatusRegistroQualidade.Rascunho;
            EtapaNcr = ENcrEtapa.Rascunho;
            DataCriacao = DateTime.UtcNow;
            Versao = 1;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<NcrRegistro>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo da NCR e obrigatorio [Origem: NcrRegistro]")
                .IsLowerOrEqualsThan(Codigo?.Length ?? 0, 30, nameof(Codigo), "O codigo da NCR deve ter no maximo 30 caracteres [Origem: NcrRegistro]")
                .IsNotNullOrEmpty(Titulo, nameof(Titulo), "O titulo da NCR e obrigatorio [Origem: NcrRegistro]")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descricao da NCR e obrigatoria [Origem: NcrRegistro]")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel pela NCR e obrigatorio [Origem: NcrRegistro]"));
        }

        public void Alterar(string titulo, string descricao, ENcrPrioridade prioridade, string? severidade, string alteradoPor)
        {
            if (StatusRegistro == EStatusRegistroQualidade.Encerrado || StatusRegistro == EStatusRegistroQualidade.Inativo)
            {
                AddNotification(nameof(StatusRegistro), "NCR encerrada/inativa nao pode ser alterada [Origem: NcrRegistro]");
                return;
            }
            Titulo = titulo;
            Descricao = descricao;
            Prioridade = prioridade;
            Severidade = severidade;
            Versao++;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void AvancarEtapa(ENcrEtapa novaEtapa, string alteradoPor)
        {
            EtapaNcr = novaEtapa;
            if (novaEtapa != ENcrEtapa.Rascunho && StatusRegistro == EStatusRegistroQualidade.Rascunho)
                StatusRegistro = EStatusRegistroQualidade.EmAnalise;
            if (novaEtapa == ENcrEtapa.Encerrada) StatusRegistro = EStatusRegistroQualidade.Encerrado;
            if (novaEtapa == ENcrEtapa.Cancelada) StatusRegistro = EStatusRegistroQualidade.Inativo;
            Versao++;
            MarcarAlterado(alteradoPor);
        }

        public void Encerrar(string conclusao, string alteradoPor)
        {
            AddNotifications(new Contract<NcrRegistro>()
                .Requires()
                .IsNotNullOrEmpty(conclusao, nameof(Conclusao), "A conclusao e obrigatoria no encerramento [Origem: NcrRegistro]"));
            if (!IsValid) return;
            Conclusao = conclusao;
            DataEncerramento = DateTime.UtcNow;
            EtapaNcr = ENcrEtapa.Encerrada;
            StatusRegistro = EStatusRegistroQualidade.Encerrado;
            Versao++;
            MarcarAlterado(alteradoPor);
        }

        public void Cancelar(string motivoCancelamento, string alteradoPor)
        {
            AddNotifications(new Contract<NcrRegistro>()
                .Requires()
                .IsNotNullOrWhiteSpace(motivoCancelamento, nameof(MotivoCancelamento), "O motivo do cancelamento e obrigatorio [Origem: NcrRegistro]"));
            if (!IsValid) return;
            MotivoCancelamento = motivoCancelamento;
            EtapaNcr = ENcrEtapa.Cancelada;
            StatusRegistro = EStatusRegistroQualidade.Inativo;
            Versao++;
            MarcarAlterado(alteradoPor);
        }

        public void AdicionarOrigem(NcrOrigemRef origem) => _origens.Add(origem);
        public void AdicionarCausaRaiz(NcrCausaRaiz causa) => _causasRaiz.Add(causa);
        public void AdicionarAcaoCapa(NcrAcaoCapa acao) => _acoesCapa.Add(acao);
    }
}
