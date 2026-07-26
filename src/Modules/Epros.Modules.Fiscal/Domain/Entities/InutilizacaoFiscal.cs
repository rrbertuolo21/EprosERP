using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    /// <summary>
    /// Registro de uma inutilização de faixa de numeração de DF-e (NF-e/NFC-e) na SEFAZ.
    /// No legado a inutilização era delegada à API de DFe sem persistência local; aqui persistimos
    /// o evento para permitir a listagem/histórico (<c>GET api/v1/inutilizacao-dfe</c>).
    /// </summary>
    public class InutilizacaoFiscal : EntidadeSaaSBase
    {
        /// <summary>Modelo do documento: 55 (NF-e) ou 65 (NFC-e).</summary>
        public int ModeloDocumento { get; private set; }
        public int Serie { get; private set; }
        public long NrNfInicial { get; private set; }
        public long NrNfFinal { get; private set; }
        public int Ano { get; private set; }
        /// <summary>Ambiente SEFAZ: 1 = Produção, 2 = Homologação.</summary>
        public int Ambiente { get; private set; }
        public string Justificativa { get; private set; } = string.Empty;

        /// <summary>Status (cStat) retornado pela SEFAZ (102 = homologada).</summary>
        public int StatusSefaz { get; private set; }
        public string? Motivo { get; private set; }
        public string? Protocolo { get; private set; }
        public DateTime DataInutilizacao { get; private set; }

        protected InutilizacaoFiscal() { } // EF Core

        public InutilizacaoFiscal(
            int modeloDocumento,
            int serie,
            long nrNfInicial,
            long nrNfFinal,
            int ano,
            int ambiente,
            string justificativa,
            int statusSefaz,
            string? motivo,
            string? protocolo,
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            ModeloDocumento = modeloDocumento;
            Serie = serie;
            NrNfInicial = nrNfInicial;
            NrNfFinal = nrNfFinal;
            Ano = ano;
            Ambiente = ambiente;
            Justificativa = justificativa;
            StatusSefaz = statusSefaz;
            Motivo = motivo;
            Protocolo = protocolo;
            DataInutilizacao = DateTime.UtcNow;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<InutilizacaoFiscal>()
                .Requires()
                .IsTrue(ModeloDocumento == 55 || ModeloDocumento == 65, nameof(ModeloDocumento), "O modelo do documento deve ser 55 (NF-e) ou 65 (NFC-e) [Origem: InutilizacaoFiscal]")
                .IsGreaterThan(NrNfInicial, 0, nameof(NrNfInicial), "O número inicial da faixa deve ser maior que zero [Origem: InutilizacaoFiscal]")
                .IsGreaterOrEqualsThan(NrNfFinal, NrNfInicial, nameof(NrNfFinal), "O número final deve ser maior ou igual ao inicial [Origem: InutilizacaoFiscal]")
                .IsNotNullOrEmpty(Justificativa, nameof(Justificativa), "A justificativa é obrigatória [Origem: InutilizacaoFiscal]"));
        }
    }
}
