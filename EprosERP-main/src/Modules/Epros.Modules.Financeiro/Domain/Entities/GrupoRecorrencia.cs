using System;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Grupo de recorrência de cobrança (EF FIN-SF §7.3 / §11 sf_grupo_recorrencia).
    /// Regras RSF-017 (intervalos 1,2,3,6,12,24,36 meses) e RSF-018 (dia de vencimento 1..31).
    /// </summary>
    public class GrupoRecorrencia : EntidadeSaaSBase
    {
        public static readonly int[] MesesPermitidos = { 1, 2, 3, 6, 12, 24, 36 };

        public string Descricao { get; private set; } = string.Empty;
        public int Meses { get; private set; }
        public int DiaVencimento { get; private set; }
        public decimal Valor { get; private set; }

        protected GrupoRecorrencia() { } // EF Core

        public GrupoRecorrencia(string descricao, int meses, int diaVencimento, decimal valor, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Descricao = descricao;
            Meses = meses;
            DiaVencimento = diaVencimento;
            Valor = valor;
            Validar();
        }

        public void Alterar(string descricao, int meses, int diaVencimento, decimal valor, string usuario)
        {
            Descricao = descricao;
            Meses = meses;
            DiaVencimento = diaVencimento;
            Valor = valor;
            MarcarAlterado(usuario);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<GrupoRecorrencia>()
                .Requires()
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descrição do grupo é obrigatória.")
            );
            if (!MesesPermitidos.Contains(Meses))
                AddNotification(nameof(Meses), "O intervalo em meses deve ser 1, 2, 3, 6, 12, 24 ou 36.");
            if (DiaVencimento < 1 || DiaVencimento > 31)
                AddNotification(nameof(DiaVencimento), "O dia de vencimento deve estar entre 1 e 31.");
        }
    }
}
