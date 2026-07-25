using System;
using System.Collections.Generic;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Qualidade.Domain.Entities
{
    /// <summary>
    /// qld_ins_plano — Plano de inspecao mestre.
    /// Porte fiel da EF QUALIDADE / PLANOS_DE_INSPECAO_E_AMOSTRAGEM (secao 11.1).
    /// </summary>
    public class PlanoInspecao : EntidadeSaaSBase
    {
        public long? SequenciaExibicao { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public EContextoPlano Contexto { get; private set; }
        public Guid? ProdutoId { get; private set; }
        public Guid? ProcessoId { get; private set; }
        public Guid? EtapaId { get; private set; }
        public EStatusRegistroQualidade Status { get; private set; }
        public Guid ResponsavelId { get; private set; }
        public DateTime? DataInicioVigencia { get; private set; }
        public DateTime? DataFimVigencia { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public int Versao { get; private set; }
        public string? MotivoStatus { get; private set; }

        private readonly List<CaracteristicaPlano> _caracteristicas = new();
        public IReadOnlyCollection<CaracteristicaPlano> Caracteristicas => _caracteristicas.AsReadOnly();

        private readonly List<RegraAmostragem> _regrasAmostragem = new();
        public IReadOnlyCollection<RegraAmostragem> RegrasAmostragem => _regrasAmostragem.AsReadOnly();

        protected PlanoInspecao() { }

        public PlanoInspecao(string codigo, string descricao, EContextoPlano contexto, Guid responsavelId,
            Guid? produtoId, Guid? processoId, Guid? etapaId, DateTime? dataInicioVigencia,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            Contexto = contexto;
            ResponsavelId = responsavelId;
            ProdutoId = produtoId;
            ProcessoId = processoId;
            EtapaId = etapaId;
            DataInicioVigencia = dataInicioVigencia;
            Status = EStatusRegistroQualidade.Rascunho;
            DataCriacao = DateTime.UtcNow;
            Versao = 1;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PlanoInspecao>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo do plano e obrigatorio [Origem: PlanoInspecao]")
                .IsLowerOrEqualsThan(Codigo?.Length ?? 0, 30, nameof(Codigo), "O codigo do plano deve ter no maximo 30 caracteres [Origem: PlanoInspecao]")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descricao do plano e obrigatoria [Origem: PlanoInspecao]")
                .IsLowerOrEqualsThan(Descricao?.Length ?? 0, 500, nameof(Descricao), "A descricao deve ter no maximo 500 caracteres [Origem: PlanoInspecao]")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel pelo plano e obrigatorio [Origem: PlanoInspecao]"));
        }

        public void Ativar(string usuario)
        {
            if (_caracteristicas.Count == 0)
            {
                AddNotification(nameof(Caracteristicas), "O plano deve ter ao menos uma caracteristica para ser ativado [Origem: PlanoInspecao]");
                return;
            }
            Status = EStatusRegistroQualidade.Ativo;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void AlterarStatus(EStatusRegistroQualidade novoStatus, string? motivo, string usuario)
        {
            if ((novoStatus == EStatusRegistroQualidade.Suspenso || novoStatus == EStatusRegistroQualidade.Encerrado)
                && string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoStatus), "O motivo e obrigatorio para suspender/encerrar o plano [Origem: PlanoInspecao]");
                return;
            }
            Status = novoStatus;
            MotivoStatus = motivo;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void AdicionarCaracteristica(CaracteristicaPlano caracteristica) => _caracteristicas.Add(caracteristica);
        public void AdicionarRegraAmostragem(RegraAmostragem regra) => _regrasAmostragem.Add(regra);
    }
}
