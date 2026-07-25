using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>Movimentação patrimonial do ativo: baixa, alienação, depreciação, vistoria (EF FIN-AFX §10.5 afx_movimentacao).</summary>
    public class MovimentacaoAtivo : EntidadeSaaSBase
    {
        public Guid AtivoId { get; private set; }
        public ETipoMovimentacaoAtivo TipoMovimentacao { get; private set; }
        public DateTime DataMovimentacao { get; private set; }
        public decimal? Valor { get; private set; }
        public string? Observacao { get; private set; }
        public Guid? UsuarioId { get; private set; }

        protected MovimentacaoAtivo() { } // EF Core

        public MovimentacaoAtivo(Guid ativoId, ETipoMovimentacaoAtivo tipoMovimentacao, DateTime dataMovimentacao,
            decimal? valor, string? observacao, Guid? usuarioId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AtivoId = ativoId;
            TipoMovimentacao = tipoMovimentacao;
            DataMovimentacao = dataMovimentacao;
            Valor = valor;
            Observacao = observacao;
            UsuarioId = usuarioId;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<MovimentacaoAtivo>()
                .Requires()
                .IsNotEmpty(AtivoId, nameof(AtivoId), "O ativo é obrigatório [Origem: MovimentacaoAtivo]")
                .IsGreaterThan(DataMovimentacao, DateTime.MinValue, nameof(DataMovimentacao), "A data da movimentação é obrigatória [Origem: MovimentacaoAtivo]")
            );
        }
    }
}
