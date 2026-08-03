using System;
using System.Collections.Generic;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-MES — Cabeçalho da ordem de produção (prd_mes_ordem).
    /// Fiel ao EF EXECUCAO_DE_MANUFATURA_MES §17. Workflow: Rascunho → EmAnalise → Ativo → Finalizado/Inativo/Encerrado.
    /// Ordem finalizada bloqueia edição e exclusão (MES-REG-017/018).
    /// </summary>
    public class MesOrdem : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public string? Referencia { get; private set; }
        public EStatusOrdemMes Status { get; private set; } = EStatusOrdemMes.Rascunho;
        public DateTime? Inicio { get; private set; }
        public DateTime? PrevisaoEntrega { get; private set; }
        public DateTime? Termino { get; private set; }
        public DateTime? DataTransacao { get; private set; }
        public Guid? LocalEstoqueId { get; private set; }
        public Guid? EstruturaId { get; private set; }
        /// <summary>
        /// T5 — Referência à ORDEM DE PRODUÇÃO CANÔNICA (<c>ordens_producao</c>). Convergência incremental:
        /// a ordem canônica assume a identidade única da produção; a ordem MES referencia-a (não a duplica).
        /// Null enquanto não houver vínculo (o legado segue como leitura). Ver DECISOES-PENDENTES-RAFAEL.md · T5.
        /// </summary>
        public Guid? OrdemProducaoRefId { get; private set; }
        public Guid? ProdutoAcabadoId { get; private set; }
        public Guid? VariacaoProdutoAcabadoId { get; private set; }
        public decimal CustoTotalPrevisto { get; private set; }
        public decimal CustoTotalRealizado { get; private set; }
        public decimal? PercentualVenda { get; private set; }
        public decimal? PercentualEstoque { get; private set; }
        public decimal? ValorTotalFinal { get; private set; }
        public decimal DesperdicioUnidades { get; private set; }
        public decimal? CustoProducao { get; private set; }
        public string? TipoCustoProducao { get; private set; }
        public bool Finalizada { get; private set; }
        public string? Lote { get; private set; }
        public DateTime? Validade { get; private set; }
        public string? MotivoRejeicao { get; private set; }

        public List<MesOrdemItem> Itens { get; private set; } = new();

        protected MesOrdem() { } // EF Core

        public MesOrdem(
            Guid empresaId,
            string tenantId,
            string criadoPor,
            string? referencia = null,
            DateTime? inicio = null,
            DateTime? previsaoEntrega = null,
            Guid? estruturaId = null,
            Guid? produtoAcabadoId = null,
            Guid? variacaoProdutoAcabadoId = null,
            decimal custoTotalPrevisto = 0m,
            decimal? percentualVenda = null,
            decimal? percentualEstoque = null)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            Referencia = referencia;
            Status = EStatusOrdemMes.Rascunho;
            Inicio = inicio;
            PrevisaoEntrega = previsaoEntrega;
            EstruturaId = estruturaId;
            ProdutoAcabadoId = produtoAcabadoId;
            VariacaoProdutoAcabadoId = variacaoProdutoAcabadoId;
            CustoTotalPrevisto = custoTotalPrevisto;
            PercentualVenda = percentualVenda;
            PercentualEstoque = percentualEstoque;
            Finalizada = false;

            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<MesOrdem>()
                .Requires()
                .AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "A empresa ou unidade operacional é obrigatória [Origem: MesOrdem]. (MES-REG-002)")
            );
        }

        private bool GarantirEditavel()
        {
            if (Finalizada || Status == EStatusOrdemMes.Finalizado)
            {
                AddNotification(nameof(Status), "Ordem finalizada não pode ser editada. (MES-REG-017)");
                return false;
            }
            return true;
        }

        /// <summary>MES-REG-003/004/005: item exige produto e referencia o cabeçalho.</summary>
        public void AdicionarItem(Guid produtoId, decimal quantidadeProduzir, string alteradoPor, Guid? variacaoId = null, decimal custoPrevisto = 0m)
        {
            if (!GarantirEditavel()) return;

            var item = new MesOrdemItem(Id, produtoId, quantidadeProduzir, TenantId, alteradoPor, variacaoId, custoPrevisto);
            if (!item.IsValid)
            {
                AddNotifications(item.Notifications);
                return;
            }
            Itens.Add(item);
            MarcarAlterado(alteradoPor);
        }

        /// <summary>MES-REG-033: apenas Rascunho pode ser submetida; exige ao menos um item (MES-REG-003).</summary>
        public void SubmeterParaAnalise(string alteradoPor)
        {
            if (Status != EStatusOrdemMes.Rascunho)
            {
                AddNotification(nameof(Status), "Apenas ordem em Rascunho pode ser submetida para análise. (MES-REG-033)");
                return;
            }
            if (Itens.Count == 0)
            {
                AddNotification(nameof(Itens), "A ordem deve possuir ao menos um item para ser submetida. (MES-REG-003)");
                return;
            }
            Status = EStatusOrdemMes.EmAnalise;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>MES-REG-034: ordem em análise pode ser aprovada.</summary>
        public void Aprovar(string alteradoPor)
        {
            if (Status != EStatusOrdemMes.EmAnalise)
            {
                AddNotification(nameof(Status), "Apenas ordem em análise pode ser aprovada. (MES-REG-034)");
                return;
            }
            Status = EStatusOrdemMes.Ativo;
            MotivoRejeicao = null;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>MES-REG-035: ordem rejeitada retorna para Rascunho com motivo.</summary>
        public void Rejeitar(string motivo, string alteradoPor)
        {
            if (Status != EStatusOrdemMes.EmAnalise)
            {
                AddNotification(nameof(Status), "Apenas ordem em análise pode ser rejeitada. (MES-REG-034)");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoRejeicao), "O motivo da rejeição é obrigatório. (MES-REG-035)");
                return;
            }
            Status = EStatusOrdemMes.Rascunho;
            MotivoRejeicao = motivo;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>
        /// MES-REG-009/010/011/014: finaliza a produção. Exige data de transação, local de estoque e valor total final.
        /// Calcula quantidade efetiva descontando desperdício (MES-REG-021) e bloqueia edição posterior.
        /// </summary>
        public void Finalizar(
            DateTime dataTransacao,
            Guid localEstoqueId,
            decimal valorTotalFinal,
            string alteradoPor,
            bool exigirEstruturaAtiva,
            decimal desperdicioUnidades = 0m,
            string? lote = null,
            DateTime? validade = null,
            bool exigeLote = false,
            bool exigeValidade = false)
        {
            if (Finalizada || Status == EStatusOrdemMes.Finalizado)
            {
                AddNotification(nameof(Status), "Ordem já finalizada. (MES-REG-017)");
                return;
            }
            if (Status != EStatusOrdemMes.Ativo && Status != EStatusOrdemMes.Pendente)
            {
                AddNotification(nameof(Status), "Somente ordem ativa ou pendente pode ser finalizada. (MES-REG-032)");
                return;
            }
            if (dataTransacao == default)
                AddNotification(nameof(DataTransacao), "A data de transação é obrigatória para finalizar a produção. (MES-REG-009)");
            if (localEstoqueId == Guid.Empty)
                AddNotification(nameof(LocalEstoqueId), "O local de estoque é obrigatório para finalizar a produção. (MES-REG-010)");
            if (valorTotalFinal <= 0m)
                AddNotification(nameof(ValorTotalFinal), "O valor total final é obrigatório para finalizar a produção. (MES-REG-011)");
            if (exigirEstruturaAtiva && (!EstruturaId.HasValue || EstruturaId.Value == Guid.Empty))
                AddNotification(nameof(EstruturaId), "A ordem deve possuir estrutura ativa quando o consumo automático for exigido. (MES-REG-012)");
            if (exigeLote && string.IsNullOrWhiteSpace(lote))
                AddNotification(nameof(Lote), "O lote deve ser informado quando o produto exigir controle de lote. (MES-REG-023)");
            if (exigeValidade && !validade.HasValue)
                AddNotification(nameof(Validade), "A validade deve ser informada quando o produto exigir controle de validade. (MES-REG-024)");

            if (!IsValid) return;

            DataTransacao = dataTransacao;
            LocalEstoqueId = localEstoqueId;
            ValorTotalFinal = valorTotalFinal;
            DesperdicioUnidades = desperdicioUnidades;
            Lote = lote;
            Validade = validade;
            Termino = dataTransacao;
            Finalizada = true;
            Status = EStatusOrdemMes.Finalizado;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>
        /// T5 — Vincula esta ordem MES à ordem de produção canônica (<c>ordens_producao</c>), estabelecendo a
        /// identidade única da produção sem duplicar dados. Idempotente: revincular ao mesmo id é no-op.
        /// </summary>
        public void VincularOrdemCanonica(Guid ordemProducaoRefId, string alteradoPor)
        {
            if (ordemProducaoRefId == Guid.Empty)
            {
                AddNotification(nameof(OrdemProducaoRefId), "A ordem de produção canônica é obrigatória para o vínculo. (T5)");
                return;
            }
            if (OrdemProducaoRefId == ordemProducaoRefId) return;
            OrdemProducaoRefId = ordemProducaoRefId;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>MES-REG-021: quantidade efetiva descontando desperdício informado.</summary>
        public decimal CalcularQuantidadeEfetiva(decimal quantidadeProduzida)
        {
            var efetiva = quantidadeProduzida - DesperdicioUnidades;
            return efetiva < 0m ? 0m : efetiva;
        }

        /// <summary>MES-REG-036: ordem ativa pode ser inativada por gestor.</summary>
        public void Inativar(string alteradoPor)
        {
            if (Status != EStatusOrdemMes.Ativo)
            {
                AddNotification(nameof(Status), "Apenas ordem ativa pode ser inativada. (MES-REG-036)");
                return;
            }
            Status = EStatusOrdemMes.Inativo;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>MES-REG-037: ordem inativa pode ser reativada por gestor.</summary>
        public void Reativar(string alteradoPor)
        {
            if (Status != EStatusOrdemMes.Inativo)
            {
                AddNotification(nameof(Status), "Apenas ordem inativa pode ser reativada. (MES-REG-037)");
                return;
            }
            Status = EStatusOrdemMes.Ativo;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>MES-REG-036: ordem ativa ou inativa pode ser encerrada por gestor.</summary>
        public void Encerrar(string alteradoPor)
        {
            if (Status != EStatusOrdemMes.Ativo && Status != EStatusOrdemMes.Inativo)
            {
                AddNotification(nameof(Status), "Somente ordem ativa ou inativa pode ser encerrada. (MES-REG-036)");
                return;
            }
            Status = EStatusOrdemMes.Encerrado;
            MarcarAlterado(alteradoPor);
        }
    }
}
