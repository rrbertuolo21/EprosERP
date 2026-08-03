using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Regra de contabilização (DE-PARA evento→conta) — parâmetro do FIN-CGL / TEC-8 (FD-NF3).
    /// Mapeia um TIPO DE FATO/evento de integração (VendaFaturada, CompraLancada, FolhaProcessada,
    /// ProjetoFaturado…) para o par de contas (débito × crédito) do plano de contas do cliente. É a
    /// tabela que o motor de contabilização (<see cref="Epros.Modules.Financeiro.Domain.Services.MotorContabilizacao"/>)
    /// consome para gerar a partida dobrada.
    ///
    /// ⛔ Regra #0: a ESCOLHA da conta é parametrização contábil do cliente (// valida-contador). Por isso
    /// as contas são NULLABLE — enquanto o contador não configura o de-para, o motor cai no default
    /// seguro (conta transitória de contabilização, em RASCUNHO, que não impacta saldos). Nunca se
    /// inventa número de conta.
    /// </summary>
    public class RegraContabilizacao : EntidadeSaaSBase
    {
        public string TipoEvento { get; private set; } = string.Empty;
        public Guid? ContaDebitoId { get; private set; }
        public Guid? ContaCreditoId { get; private set; }
        public string? Historico { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected RegraContabilizacao() { } // EF Core

        public RegraContabilizacao(string tipoEvento, Guid? contaDebitoId, Guid? contaCreditoId, string? historico, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            TipoEvento = tipoEvento;
            ContaDebitoId = contaDebitoId;
            ContaCreditoId = contaCreditoId;
            Historico = historico;
            Ativo = true;
            Validar();
        }

        public void Alterar(Guid? contaDebitoId, Guid? contaCreditoId, string? historico, string usuario)
        {
            ContaDebitoId = contaDebitoId;
            ContaCreditoId = contaCreditoId;
            Historico = historico;
            MarcarAlterado(usuario);
            Validar();
        }

        public void DefinirAtivo(bool ativo, string usuario) { Ativo = ativo; MarcarAlterado(usuario); }

        /// <summary>True quando o de-para está completo (ambas as contas definidas pelo contador).</summary>
        public bool Completa => ContaDebitoId.HasValue && ContaCreditoId.HasValue;

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<RegraContabilizacao>()
                .Requires()
                .IsNotNullOrEmpty(TipoEvento, nameof(TipoEvento), "O tipo de evento da regra é obrigatório.")
            );
        }
    }
}
