using System;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Portfolio
{
    /// <summary>
    /// T-PRG (DP-PRT-hierarquia) — Programa materializa a hierarquia Portfólio &gt; Programa &gt; Projeto.
    /// Antes o <see cref="PortfolioItem.ProgramaId"/> era um Guid solto sem tabela; agora referencia esta entidade.
    /// Programa agrupa projetos sob um objetivo comum dentro de um portfólio (programa é opcional na hierarquia).
    /// Status pelo enum canônico (T3). Origem: DECISOES_IMPLANTACAO_V1 · seção 3 (T-PRG).
    /// </summary>
    public class Programa : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public Guid ResponsavelId { get; private set; }
        public Guid? PortfolioId { get; private set; }
        public EProjetoWorkflowStatus Status { get; private set; } = EProjetoWorkflowStatus.Rascunho;
        public bool Ativo { get; private set; } = true;

        protected Programa() { } // EF Core

        public Programa(
            string codigo,
            string nome,
            string? descricao,
            Guid responsavelId,
            Guid? portfolioId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Programa>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O codigo do programa e obrigatorio. [Origem: Programa]")
                .IsLowerOrEqualsThan(codigo?.Length ?? 0, 30, nameof(Codigo), "O codigo deve ter no maximo 30 caracteres. [Origem: Programa]")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do programa e obrigatorio. [Origem: Programa]")
                .AreNotEquals(responsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel do programa e obrigatorio. [Origem: Programa]"));

            Codigo = codigo ?? string.Empty;
            Nome = nome ?? string.Empty;
            Descricao = descricao;
            ResponsavelId = responsavelId;
            PortfolioId = portfolioId;
            Status = EProjetoWorkflowStatus.Rascunho;
            Ativo = true;
        }

        public void Ativar(string usuario)
        {
            if (Status == EProjetoWorkflowStatus.Encerrado || Status == EProjetoWorkflowStatus.Cancelado)
            {
                AddNotification(nameof(Status), "Programa encerrado/cancelado nao pode ser ativado. [Origem: Programa]");
                return;
            }
            Status = EProjetoWorkflowStatus.Ativo;
            MarcarAlterado(usuario);
        }

        public void Encerrar(string usuario)
        {
            Status = EProjetoWorkflowStatus.Encerrado;
            MarcarAlterado(usuario);
        }

        public void Inativar(string usuario)
        {
            Ativo = false;
            Status = EProjetoWorkflowStatus.Inativo;
            MarcarAlterado(usuario);
        }
    }
}
