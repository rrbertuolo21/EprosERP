using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Contrato de compra (EF Gestão de Contratos de Compra §16.1 `gcc_contrato_compra`).
    /// GCC-001: exige fornecedor. GCC-002: exige vigência. GCC-009: alteração com impacto financeiro
    /// passa por workflow (integração externa; ver pendências). Modelo proposto por autoria (§22).
    /// FornecedorId/CompraId são referências externas por FK Guid (sem navegação cruzada).
    /// </summary>
    public class GccContratoCompra : EntidadeSaaSBase
    {
        public Guid FornecedorId { get; private set; }
        public string? NumeroContrato { get; private set; }
        public DateTime? VigenciaInicio { get; private set; }
        public DateTime? VigenciaFim { get; private set; }
        public decimal? ValorTotal { get; private set; }
        public ESituacaoContratoCompra Situacao { get; private set; } = ESituacaoContratoCompra.Rascunho;
        public string? Observacao { get; private set; }

        // Navegação intra-módulo
        public ICollection<GccContratoCompraItem> Itens { get; private set; } = new List<GccContratoCompraItem>();
        public ICollection<GccContratoCompraAditivo> Aditivos { get; private set; } = new List<GccContratoCompraAditivo>();
        public ICollection<GccConsumoContrato> Consumos { get; private set; } = new List<GccConsumoContrato>();

        protected GccContratoCompra() { } // EF Core

        public GccContratoCompra(Guid fornecedorId, string? numeroContrato, DateTime? vigenciaInicio, DateTime? vigenciaFim, decimal? valorTotal, string? observacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            FornecedorId = fornecedorId;
            NumeroContrato = numeroContrato;
            VigenciaInicio = vigenciaInicio;
            VigenciaFim = vigenciaFim;
            ValorTotal = valorTotal;
            Observacao = observacao;
            Situacao = ESituacaoContratoCompra.Rascunho;
            Validar();
        }

        public void Alterar(Guid fornecedorId, string? numeroContrato, DateTime? vigenciaInicio, DateTime? vigenciaFim, decimal? valorTotal, string? observacao, string alteradoPor)
        {
            FornecedorId = fornecedorId;
            NumeroContrato = numeroContrato;
            VigenciaInicio = vigenciaInicio;
            VigenciaFim = vigenciaFim;
            ValorTotal = valorTotal;
            Observacao = observacao;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void AdicionarItem(GccContratoCompraItem item) => Itens.Add(item);

        /// <summary>Envia o contrato para aprovação (workflow) — EF §10.1 / GCC-009.</summary>
        public void EnviarParaAprovacao(string usuario)
        {
            Situacao = ESituacaoContratoCompra.EmAprovacao;
            MarcarAlterado(usuario);
        }

        /// <summary>Aprova o contrato, tornando-o válido para consumo — EF §10.1.</summary>
        public void Aprovar(string usuario)
        {
            Situacao = ESituacaoContratoCompra.Aprovado;
            MarcarAlterado(usuario);
        }

        public void Suspender(string usuario)
        {
            Situacao = ESituacaoContratoCompra.Suspenso;
            MarcarAlterado(usuario);
        }

        public void Encerrar(string usuario)
        {
            Situacao = ESituacaoContratoCompra.Encerrado;
            MarcarAlterado(usuario);
        }

        public bool PodeConsumir() => Situacao == ESituacaoContratoCompra.Aprovado; // regra §18: apenas contrato aprovado e vigente

        /// <summary>
        /// Aditivo contratual (CD5): só se aplica a contrato APROVADO (contrato vigente é o que se adita).
        /// Aplica o efeito conforme o tipo — Vigencia (nova data-fim), Valor (novo valor total), Condicoes
        /// (observação). Aditivos de Preco/Quantidade aplicam-se por ITEM (ver GccContratoCompraItem).
        /// Registra o <paramref name="aditivo"/> na coleção. Retorna false com notificação se não pode aditar.
        /// </summary>
        public bool AplicarAditivo(GccContratoCompraAditivo aditivo, DateTime? novaVigenciaFim, decimal? novoValorTotal, string? novaObservacao, string usuario)
        {
            if (Situacao != ESituacaoContratoCompra.Aprovado)
            {
                AddNotification("Situacao", "Só é possível aditar um contrato aprovado (vigente) [GCC-022] [Origem: GccContratoCompra]");
                return false;
            }
            switch (aditivo.TipoAditivo)
            {
                case ETipoAditivoContrato.Vigencia:
                    if (novaVigenciaFim.HasValue && VigenciaInicio.HasValue && novaVigenciaFim.Value < VigenciaInicio.Value)
                    {
                        AddNotification("VigenciaFim", "A nova vigência não pode ser anterior ao início [GCC-023] [Origem: GccContratoCompra]");
                        return false;
                    }
                    if (novaVigenciaFim.HasValue) VigenciaFim = novaVigenciaFim.Value;
                    break;
                case ETipoAditivoContrato.Preco:
                    if (novoValorTotal.HasValue) ValorTotal = novoValorTotal.Value;
                    break;
                case ETipoAditivoContrato.Quantidade:
                    if (novoValorTotal.HasValue) ValorTotal = novoValorTotal.Value;
                    break;
                case ETipoAditivoContrato.Condicoes:
                    if (!string.IsNullOrWhiteSpace(novaObservacao)) Observacao = novaObservacao.Trim();
                    break;
            }
            Aditivos.Add(aditivo);
            MarcarAlterado(usuario);
            return true;
        }

        public void Validar()
        {
            Clear();
            // GCC-001 fornecedor obrigatório; GCC-002 vigência obrigatória (mínimo início).
            if (FornecedorId == Guid.Empty)
                AddNotification("FornecedorId", "O fornecedor é obrigatório [GCC-001] [Origem: GccContratoCompra]");
            if (VigenciaInicio == null)
                AddNotification("VigenciaInicio", "A vigência do contrato é obrigatória [GCC-002] [Origem: GccContratoCompra]");
            if (VigenciaInicio != null && VigenciaFim != null && VigenciaFim < VigenciaInicio)
                AddNotification("VigenciaFim", "A vigência final não pode ser anterior à inicial [CA-002] [Origem: GccContratoCompra]");
        }
    }
}
