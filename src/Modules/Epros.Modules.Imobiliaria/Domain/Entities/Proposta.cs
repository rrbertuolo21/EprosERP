using System;
using System.Collections.Generic;
using Epros.Modules.Imobiliaria.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Imobiliaria.Domain.Entities
{
    /// <summary>
    /// Proposta imobiliaria (ID2/PRD-02). Dono = GESTAO_IMOBILIARIA (VENDAS consome, nao e dono).
    /// Dois tipos (Locacao/Aquisicao), com estados (Rascunho→Aprovada/Rejeitada/Expirada),
    /// validade, contraproposta (auto-referencia) e conversao (aceite gera locacao/contrato).
    /// Mapeada ao status canonico T3.
    /// </summary>
    public class Proposta : EntidadeSaaSBase
    {
        public ETipoProposta Tipo { get; private set; }
        public Guid ImovelId { get; private set; }
        public EStatusProposta Status { get; private set; }
        public DateTime Validade { get; private set; }
        public decimal ValorProposto { get; private set; }
        public string? Observacao { get; private set; }
        /// <summary>Proposta de origem quando esta e uma contraproposta (auto-referencia).</summary>
        public Guid? ContrapropostaDeId { get; private set; }
        /// <summary>Locacao gerada na conversao (aceite) — quando Tipo = Locacao.</summary>
        public Guid? LocacaoGeradaId { get; private set; }

        private readonly List<PropostaParte> _partes = new();
        public IReadOnlyCollection<PropostaParte> Partes => _partes.AsReadOnly();

        protected Proposta() { } // EF Core

        public Proposta(
            ETipoProposta tipo,
            Guid imovelId,
            DateTime validade,
            decimal valorProposto,
            string? observacao,
            Guid? contrapropostaDeId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Tipo = tipo;
            ImovelId = imovelId;
            Validade = validade.Date;
            ValorProposto = valorProposto;
            Observacao = observacao;
            ContrapropostaDeId = contrapropostaDeId;
            Status = EStatusProposta.Rascunho;
            Validar();
        }

        public void AdicionarParte(PropostaParte parte)
        {
            parte.VincularAProposta(Id);
            _partes.Add(parte);
        }

        public void Aprovar(string usuario)
        {
            if (Status != EStatusProposta.Rascunho)
            {
                AddNotification(nameof(Status), "Apenas propostas em rascunho podem ser aprovadas.");
                return;
            }
            if (Validade < DateTime.UtcNow.Date)
            {
                AddNotification(nameof(Validade), "Proposta expirada nao pode ser aprovada.");
                return;
            }
            Status = EStatusProposta.Aprovada;
            MarcarAlterado(usuario);
        }

        public void Rejeitar(string usuario)
        {
            if (Status != EStatusProposta.Rascunho)
            {
                AddNotification(nameof(Status), "Apenas propostas em rascunho podem ser rejeitadas.");
                return;
            }
            Status = EStatusProposta.Rejeitada;
            MarcarAlterado(usuario);
        }

        public void Expirar(string usuario)
        {
            if (Status != EStatusProposta.Rascunho) return; // so rascunho expira
            Status = EStatusProposta.Expirada;
            MarcarAlterado(usuario);
        }

        /// <summary>Marca a proposta como convertida, guardando a locacao gerada (ID2).</summary>
        public void MarcarConvertida(Guid? locacaoGeradaId, string usuario)
        {
            if (Status != EStatusProposta.Aprovada)
            {
                AddNotification(nameof(Status), "Somente proposta aprovada pode ser convertida.");
                return;
            }
            LocacaoGeradaId = locacaoGeradaId;
            Status = EStatusProposta.Convertida;
            MarcarAlterado(usuario);
        }

        public Epros.Shared.Domain.Enums.ESituacaoCanonica SituacaoCanonica => Status switch
        {
            EStatusProposta.Rascunho => Epros.Shared.Domain.Enums.ESituacaoCanonica.Rascunho,
            EStatusProposta.Aprovada => Epros.Shared.Domain.Enums.ESituacaoCanonica.Ativo,
            EStatusProposta.Convertida => Epros.Shared.Domain.Enums.ESituacaoCanonica.Encerrado,
            EStatusProposta.Rejeitada => Epros.Shared.Domain.Enums.ESituacaoCanonica.Cancelado,
            EStatusProposta.Expirada => Epros.Shared.Domain.Enums.ESituacaoCanonica.Inativo,
            _ => Epros.Shared.Domain.Enums.ESituacaoCanonica.Rascunho
        };

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Proposta>()
                .Requires()
                .AreNotEquals(ImovelId, Guid.Empty, nameof(ImovelId),
                    "A proposta exige imovel. [Origem: Proposta] (ID2)")
                .IsGreaterThan(ValorProposto, 0, nameof(ValorProposto),
                    "O valor proposto deve ser positivo. [Origem: Proposta] (ID2)"));

            foreach (var p in _partes) AddNotifications(p);
        }
    }
}
