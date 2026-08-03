using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-REG — Calendario regulatorio (grc_reg_calendario). Controla vencimentos, alertas
    /// e SLA de obrigacoes/certificados. Fiel a EF_13_GRC_COMPLIANCE_REGULATORIO_V1 (secao 10.2/10.4).
    /// </summary>
    public class CalendarioRegulatorio : EntidadeSaaSBase
    {
        public Guid? CertificadoId { get; private set; }
        public Guid? RegistroId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public DateTime DataVencimento { get; private set; }
        // D-REG-02 — janelas de alerta (default 30/15/7), responsável e tratamento.
        public string DiasAlerta { get; private set; } = "30,15,7";
        public Guid? ResponsavelId { get; private set; }
        public string? Tratamento { get; private set; }
        // Pendente, Alertado, Vencido, Encerrado, Cancelado
        public string Status { get; private set; } = "Pendente";

        protected CalendarioRegulatorio() { } // EF Core

        public CalendarioRegulatorio(
            Guid? certificadoId,
            Guid? registroId,
            string descricao,
            DateTime dataVencimento,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<CalendarioRegulatorio>()
                .Requires()
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descricao do vencimento e obrigatoria.")
                .IsTrue(certificadoId != null || registroId != null, nameof(CertificadoId),
                    "O vencimento deve estar vinculado a um certificado ou registro regulatorio.")
            );

            CertificadoId = certificadoId;
            RegistroId = registroId;
            Descricao = descricao;
            DataVencimento = dataVencimento;
            Status = "Pendente";
        }

        /// <summary>D-REG-02 — configura janelas de alerta (csv de dias), responsável e tratamento.</summary>
        public void ConfigurarAlertas(string? diasAlerta, Guid? responsavelId, string? tratamento, string usuario)
        {
            if (!string.IsNullOrWhiteSpace(diasAlerta)) DiasAlerta = diasAlerta;
            ResponsavelId = responsavelId;
            Tratamento = tratamento;
            MarcarAlterado(usuario);
        }

        /// <summary>
        /// D-REG-02 — indica se, na data de referência, o vencimento cai dentro de alguma janela de
        /// alerta configurada (ex.: faltam 30/15/7 dias). Base para a rotina de alerta parametrizada.
        /// </summary>
        public bool DeveAlertar(DateTime referencia)
        {
            if (Status != "Pendente") return false;
            var diasRestantes = (DataVencimento.Date - referencia.Date).Days;
            if (diasRestantes < 0) return false;
            foreach (var parte in DiasAlerta.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(parte.Trim(), out var janela) && diasRestantes <= janela)
                    return true;
            }
            return false;
        }

        public void Alertar(string usuario)
        {
            if (Status != "Pendente")
            {
                AddNotification(nameof(Status), "Somente vencimentos pendentes podem ser alertados.");
                return;
            }
            Status = "Alertado";
            MarcarAlterado(usuario);
        }

        public void MarcarVencido(string usuario)
        {
            if (Status == "Encerrado" || Status == "Cancelado")
            {
                AddNotification(nameof(Status), "Vencimento ja encerrado ou cancelado.");
                return;
            }
            Status = "Vencido";
            MarcarAlterado(usuario);
        }

        public void Encerrar(string usuario)
        {
            Status = "Encerrado";
            MarcarAlterado(usuario);
        }
    }
}
