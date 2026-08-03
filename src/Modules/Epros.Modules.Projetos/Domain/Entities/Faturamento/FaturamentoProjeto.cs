using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Faturamento
{
    /// <summary>
    /// Agregado raiz do faturamento de projeto. Origem: EF PRJ-FAT 11.1 (prj_faturamento_projeto).
    /// RN-FAT-002 (projeto obrigatorio), RN-FAT-003 (codigo unico por tenant),
    /// RN-FAT-006 (aprovar so de EmAnalise), RN-FAT-008 (evento financeiro so quando Ativo),
    /// RN-FAT-014 (exclusao fisica bloqueada). O titulo de Contas a Receber pertence ao Financeiro (RN-FAT-009).
    /// </summary>
    public class FaturamentoProjeto : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public EProjetoWorkflowStatus Status { get; private set; } = EProjetoWorkflowStatus.Rascunho;
        public Guid ResponsavelId { get; private set; }
        public Guid ProjetoId { get; private set; }
        public Guid? ClienteId { get; private set; }
        public EModalidadeFaturamento? ModalidadeFaturamento { get; private set; }
        public decimal ValorTotal { get; private set; }
        public string? Moeda { get; private set; }
        public DateTime? DataVencimento { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public int Versao { get; private set; }
        public string? MotivoRejeicao { get; private set; }

        // ===================== FISCAL (DP-FAT-004/008) — // valida-contador =====================
        // Modelagem dos tributos/retenções sobre serviço de projeto (ISS/IRRF/INSS/PIS/COFINS/CSLL).
        // ⚠️ valida-contador: quais tributos incidem, bases e alíquotas por serviço de projeto e o
        // contrato de NFS-e vão ao contador (Negocio-acumulado/fiscal + validação humana). NENHUM valor
        // ou alíquota é inventado aqui: os campos guardam valores informados e o líquido = bruto - retenções.
        public decimal? ValorIss { get; private set; }
        public decimal? ValorIrrf { get; private set; }
        public decimal? ValorInss { get; private set; }
        public decimal? ValorPis { get; private set; }
        public decimal? ValorCofins { get; private set; }
        public decimal? ValorCsll { get; private set; }
        /// <summary>Soma das retenções aplicadas (derivado). // valida-contador para quais tributos são retidos.</summary>
        public decimal ValorRetencoes { get; private set; }
        /// <summary>Valor líquido a receber = ValorTotal (bruto) - ValorRetencoes. Regra de dado, não fiscal.</summary>
        public decimal ValorLiquido { get; private set; }

        public List<ItemFaturamentoProjeto> Itens { get; private set; } = new();

        protected FaturamentoProjeto() { } // EF Core

        public FaturamentoProjeto(
            string codigo,
            string descricao,
            Guid projetoId,
            Guid responsavelId,
            Guid? clienteId,
            EModalidadeFaturamento? modalidade,
            string? moeda,
            DateTime? dataVencimento,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<FaturamentoProjeto>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O codigo do faturamento e obrigatorio. [Origem: FaturamentoProjeto]")
                .IsLowerOrEqualsThan(codigo?.Length ?? 0, 30, nameof(Codigo), "O codigo deve ter no maximo 30 caracteres. [Origem: FaturamentoProjeto]")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descricao do faturamento e obrigatoria. [Origem: FaturamentoProjeto]")
                .IsLowerOrEqualsThan(descricao?.Length ?? 0, 500, nameof(Descricao), "A descricao deve ter no maximo 500 caracteres. [Origem: FaturamentoProjeto]")
                .AreNotEquals(projetoId, Guid.Empty, nameof(ProjetoId), "O projeto e obrigatorio. [Origem: FaturamentoProjeto]")
                .AreNotEquals(responsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio. [Origem: FaturamentoProjeto]"));

            // DP-FAT-002/003: Cliente e Moeda passam a ser obrigatorios no agregado de faturamento.
            if (clienteId is null || clienteId == Guid.Empty)
                AddNotification(nameof(ClienteId), "O cliente do faturamento e obrigatorio. [Origem: FaturamentoProjeto / DP-FAT-002]");
            if (string.IsNullOrWhiteSpace(moeda))
                AddNotification(nameof(Moeda), "A moeda do faturamento e obrigatoria. [Origem: FaturamentoProjeto / DP-FAT-003]");

            Codigo = codigo ?? string.Empty;
            Descricao = descricao ?? string.Empty;
            ProjetoId = projetoId;
            ResponsavelId = responsavelId;
            ClienteId = clienteId;
            ModalidadeFaturamento = modalidade;
            Moeda = moeda;
            DataVencimento = dataVencimento;
            Status = EProjetoWorkflowStatus.Rascunho;
            DataCriacao = DateTime.UtcNow;
            Versao = 1;
            ValorTotal = 0;
            ValorRetencoes = 0;
            ValorLiquido = 0;
        }

        public void AdicionarItem(
            int sequencia,
            decimal? quantidade,
            string? observacao,
            ETipoItemFaturamento? tipoItem,
            decimal? valorUnitario,
            decimal? valorTotalItem,
            string? origemTipo,
            Guid? origemId,
            string usuario,
            bool reembolsavel = false)
        {
            // RN-FAT-004: rascunho editavel por perfil autorizado.
            if (Status != EProjetoWorkflowStatus.Rascunho)
            {
                AddNotification(nameof(Status), "Itens so podem ser adicionados enquanto o faturamento esta em Rascunho. [Origem: FaturamentoProjeto]");
                return;
            }

            var item = new ItemFaturamentoProjeto(Id, sequencia, quantidade, observacao, tipoItem, valorUnitario, valorTotalItem, origemTipo, origemId, TenantId, usuario, reembolsavel);
            if (!item.IsValid)
            {
                AddNotifications(item.Notifications);
                return;
            }
            Itens.Add(item);
            RecalcularTotal();
            MarcarAlterado(usuario);
        }

        private void RecalcularTotal()
        {
            ValorTotal = Itens.Sum(i => i.ValorTotal ?? 0);
            RecalcularLiquido();
        }

        private void RecalcularLiquido()
        {
            ValorRetencoes = (ValorIss ?? 0) + (ValorIrrf ?? 0) + (ValorInss ?? 0)
                           + (ValorPis ?? 0) + (ValorCofins ?? 0) + (ValorCsll ?? 0);
            ValorLiquido = ValorTotal - ValorRetencoes;
        }

        /// <summary>
        /// DP-FAT-004/008 — aplica os valores de tributos/retenções ao faturamento. // valida-contador:
        /// os VALORES/ALÍQUOTAS e quais tributos incidem/são retidos sobre serviço de projeto são decididos
        /// pelo contador (Negocio-acumulado/fiscal). Aqui só persistimos os valores informados e derivamos
        /// o líquido (bruto - retenções). Só é permitido em Rascunho/EmAnalise (antes da geração do evento
        /// financeiro), preservando a imutabilidade fiscal do faturamento já ativo (RN-FAT-008).
        /// </summary>
        public void AplicarTributacao(
            decimal? valorIss,
            decimal? valorIrrf,
            decimal? valorInss,
            decimal? valorPis,
            decimal? valorCofins,
            decimal? valorCsll,
            string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Rascunho && Status != EProjetoWorkflowStatus.EmAnalise)
            {
                AddNotification(nameof(Status), "A tributacao so pode ser ajustada antes da aprovacao do faturamento. [Origem: FaturamentoProjeto]");
                return;
            }

            // Nenhum valor negativo (regra de dado, não fiscal).
            foreach (var (valor, campo) in new[]
            {
                (valorIss, nameof(ValorIss)), (valorIrrf, nameof(ValorIrrf)), (valorInss, nameof(ValorInss)),
                (valorPis, nameof(ValorPis)), (valorCofins, nameof(ValorCofins)), (valorCsll, nameof(ValorCsll))
            })
            {
                if (valor.HasValue && valor.Value < 0)
                {
                    AddNotification(campo, "O valor de tributo/retencao nao pode ser negativo. [Origem: FaturamentoProjeto]");
                    return;
                }
            }

            ValorIss = valorIss;
            ValorIrrf = valorIrrf;
            ValorInss = valorInss;
            ValorPis = valorPis;
            ValorCofins = valorCofins;
            ValorCsll = valorCsll;
            RecalcularLiquido();
            MarcarAlterado(usuario);
        }

        /// <summary>RN-FAT-005: submissao exige itens validos.</summary>
        public void Submeter(string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Rascunho)
            {
                AddNotification(nameof(Status), "So e possivel submeter faturamento em Rascunho. [Origem: FaturamentoProjeto]");
                return;
            }
            if (!Itens.Any())
            {
                AddNotification(nameof(Itens), "Informe ao menos um item para submeter o faturamento. [Origem: FaturamentoProjeto]");
                return;
            }
            Status = EProjetoWorkflowStatus.EmAnalise;
            MarcarAlterado(usuario);
        }

        /// <summary>RN-FAT-006: aprovar somente a partir de EmAnalise. Habilita evento financeiro (RN-FAT-008).</summary>
        public void Aprovar(string usuario)
        {
            if (Status != EProjetoWorkflowStatus.EmAnalise)
            {
                AddNotification(nameof(Status), "A aprovacao somente ocorre a partir de EmAnalise. [Origem: FaturamentoProjeto]");
                return;
            }
            Status = EProjetoWorkflowStatus.Ativo;
            MarcarAlterado(usuario);
        }

        /// <summary>RN-FAT-007: rejeicao exige motivo; retorna a Rascunho.</summary>
        public void Rejeitar(string motivo, string usuario)
        {
            if (Status != EProjetoWorkflowStatus.EmAnalise)
            {
                AddNotification(nameof(Status), "A rejeicao somente ocorre a partir de EmAnalise. [Origem: FaturamentoProjeto]");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoRejeicao), "A rejeicao exige motivo. [Origem: FaturamentoProjeto]");
                return;
            }
            MotivoRejeicao = motivo;
            Status = EProjetoWorkflowStatus.Rascunho;
            MarcarAlterado(usuario);
        }

        /// <summary>RN-FAT-018: inativacao/encerramento exige motivo auditavel.</summary>
        public void Encerrar(string motivo, string usuario)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(Status), "O encerramento exige motivo. [Origem: FaturamentoProjeto]");
                return;
            }
            Status = EProjetoWorkflowStatus.Encerrado;
            MarcarAlterado(usuario);
        }

        public bool PodePublicarEventoFinanceiro() => Status == EProjetoWorkflowStatus.Ativo;
    }
}
