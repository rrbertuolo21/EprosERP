using System;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Qualidade.Domain.Entities
{
    /// <summary>
    /// qld_qps_registro — Registro de qualificacao/homologacao do fornecedor (parceiro de suprimento).
    /// Ciclo: Pendente -> Homologado -> (Bloqueado | ReHomologacao). Homologacao != cadastro comercial (RN-QPS-004).
    /// parceiro_id e referencia solta ao cadastro mestre (D24), sem FK cross-schema.
    /// </summary>
    public class QpsRegistro : EntidadeSaaSBase
    {
        public long? SequenciaExibicao { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public Guid ParceiroId { get; private set; }
        public string? NomeParceiro { get; private set; }
        public EStatusRegistroQualidade Status { get; private set; }
        public EQpsStatusHomologacao StatusHomologacao { get; private set; }
        public Guid ResponsavelId { get; private set; }
        public decimal? ScoreAtual { get; private set; }
        public DateTime? DataHomologacao { get; private set; }
        public DateTime? DataValidadeHomologacao { get; private set; }
        public string? MotivoBloqueio { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public int Versao { get; private set; }

        protected QpsRegistro() { }

        public QpsRegistro(string codigo, Guid parceiroId, Guid responsavelId, string? nomeParceiro,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            ParceiroId = parceiroId;
            ResponsavelId = responsavelId;
            NomeParceiro = nomeParceiro;
            Status = EStatusRegistroQualidade.Rascunho;
            StatusHomologacao = EQpsStatusHomologacao.Pendente;
            DataCriacao = DateTime.UtcNow;
            Versao = 1;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<QpsRegistro>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo do registro e obrigatorio [Origem: QpsRegistro]")
                .IsLowerOrEqualsThan(Codigo?.Length ?? 0, 30, nameof(Codigo), "O codigo deve ter no maximo 30 caracteres [Origem: QpsRegistro]")
                .AreNotEquals(ParceiroId, Guid.Empty, nameof(ParceiroId), "O parceiro e obrigatorio [Origem: QpsRegistro]")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio [Origem: QpsRegistro]"));
        }

        /// <summary>Homologa o fornecedor com validade (a validade dispara re-homologacao ao vencer).</summary>
        public void Homologar(DateTime dataValidade, string usuario)
        {
            if (StatusHomologacao == EQpsStatusHomologacao.Homologado)
            {
                AddNotification(nameof(StatusHomologacao), "Fornecedor ja homologado [Origem: QpsRegistro]");
                return;
            }
            StatusHomologacao = EQpsStatusHomologacao.Homologado;
            Status = EStatusRegistroQualidade.Ativo;
            DataHomologacao = DateTime.UtcNow;
            DataValidadeHomologacao = dataValidade;
            MotivoBloqueio = null;
            Versao++;
            MarcarAlterado(usuario);
        }

        /// <summary>Bloqueia o fornecedor (manual/automatico). Exige motivo + alcada (RN-QPS bloqueio).</summary>
        public void Bloquear(string motivo, string usuario)
        {
            AddNotifications(new Contract<QpsRegistro>()
                .Requires()
                .IsNotNullOrWhiteSpace(motivo, nameof(MotivoBloqueio), "O motivo do bloqueio e obrigatorio [Origem: QpsRegistro]"));
            if (!IsValid) return;
            StatusHomologacao = EQpsStatusHomologacao.Bloqueado;
            Status = EStatusRegistroQualidade.Suspenso;
            MotivoBloqueio = motivo;
            Versao++;
            MarcarAlterado(usuario);
        }

        /// <summary>Coloca o fornecedor em re-homologacao (documento vencido, score, revalidacao).</summary>
        public void SolicitarReHomologacao(string usuario)
        {
            StatusHomologacao = EQpsStatusHomologacao.ReHomologacao;
            Status = EStatusRegistroQualidade.EmAnalise;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void Reprovar(string motivo, string usuario)
        {
            AddNotifications(new Contract<QpsRegistro>()
                .Requires()
                .IsNotNullOrWhiteSpace(motivo, nameof(MotivoBloqueio), "O motivo da reprovacao e obrigatorio [Origem: QpsRegistro]"));
            if (!IsValid) return;
            StatusHomologacao = EQpsStatusHomologacao.Reprovado;
            Status = EStatusRegistroQualidade.Inativo;
            MotivoBloqueio = motivo;
            Versao++;
            MarcarAlterado(usuario);
        }

        /// <summary>Atualiza o score corrente (calculado pelo motor parametrizavel).</summary>
        public void AtualizarScore(decimal score, string usuario)
        {
            ScoreAtual = score;
            MarcarAlterado(usuario);
        }
    }
}
