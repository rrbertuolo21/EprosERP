using System;
using System.Linq;
using Epros.Modules.Qualidade.Domain.Services.Aql;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Qualidade.Domain.Entities
{
    /// <summary>
    /// Estado PERSISTIDO da comutacao de severidade (NBR 5427 / MIL-STD-105 §8-§10) por
    /// (fornecedor x produto x AQL). O <see cref="MotorComutacao"/> e puro e evolui um
    /// <see cref="EstadoComutacaoAql"/> em memoria; esta entidade guarda esse estado entre lotes para
    /// que a severidade do proximo recebimento seja recuperada do banco — nao recomeca em normal a cada lote.
    ///
    /// A janela 2-de-5 (ultimos resultados em normal) e serializada como string de 0/1 (mais antigo -> mais novo).
    /// </summary>
    public class EstadoComutacaoInspecao : EntidadeSaaSBase
    {
        public Guid FornecedorId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public string Aql { get; private set; } = string.Empty;

        public ESeveridadeAql Severidade { get; private set; } = ESeveridadeAql.Normal;
        public bool Suspensa { get; private set; }

        public int ConsecutivosAceitosNormal { get; private set; }
        public int SomaDefeituososNormal { get; private set; }
        public int ConsecutivosAceitosSevera { get; private set; }
        public int RejeitadosAcumuladosSevera { get; private set; }

        /// <summary>Janela 2-de-5 serializada (ex.: "1010" = 4 lotes, o mais novo por ultimo).</summary>
        public string JanelaNormalRejeicoes { get; private set; } = string.Empty;

        /// <summary>Total de lotes ja processados por este estado (auditoria/inspecao).</summary>
        public int LotesProcessados { get; private set; }

        public DateTime? UltimoLoteEm { get; private set; }

        protected EstadoComutacaoInspecao() { } // EF Core

        public EstadoComutacaoInspecao(Guid fornecedorId, Guid produtoId, string aql, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<EstadoComutacaoInspecao>()
                .Requires()
                .AreNotEquals(fornecedorId, Guid.Empty, nameof(FornecedorId), "O fornecedor e obrigatorio [Origem: EstadoComutacaoInspecao]")
                .AreNotEquals(produtoId, Guid.Empty, nameof(ProdutoId), "O produto e obrigatorio [Origem: EstadoComutacaoInspecao]")
                .IsNotNullOrEmpty(aql, nameof(Aql), "O AQL e obrigatorio [Origem: EstadoComutacaoInspecao]"));

            FornecedorId = fornecedorId;
            ProdutoId = produtoId;
            Aql = aql;
            Severidade = ESeveridadeAql.Normal; // RN-01: inicia em normal.
        }

        /// <summary>Reidrata o estado do motor (puro) a partir do que esta persistido.</summary>
        public EstadoComutacaoAql ParaEstadoMotor()
            => EstadoComutacaoAql.Restaurar(
                Severidade, Suspensa,
                ConsecutivosAceitosNormal, SomaDefeituososNormal,
                ConsecutivosAceitosSevera, RejeitadosAcumuladosSevera,
                DesserializarJanela(JanelaNormalRejeicoes));

        /// <summary>Grava de volta o estado evoluido pelo motor e registra o lote processado.</summary>
        public void AplicarEstadoMotor(EstadoComutacaoAql estado, string usuario)
        {
            Severidade = estado.Severidade;
            Suspensa = estado.Suspensa;
            ConsecutivosAceitosNormal = estado.ConsecutivosAceitosNormal;
            SomaDefeituososNormal = estado.SomaDefeituososNormal;
            ConsecutivosAceitosSevera = estado.ConsecutivosAceitosSevera;
            RejeitadosAcumuladosSevera = estado.RejeitadosAcumuladosSevera;
            JanelaNormalRejeicoes = SerializarJanela(estado);
            LotesProcessados++;
            UltimoLoteEm = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }

        private static string SerializarJanela(EstadoComutacaoAql estado)
            => string.Concat(estado.JanelaNormalRejeicoes.Select(r => r ? '1' : '0'));

        private static bool[] DesserializarJanela(string? serializada)
            => string.IsNullOrEmpty(serializada) ? Array.Empty<bool>() : serializada.Select(c => c == '1').ToArray();
    }
}
