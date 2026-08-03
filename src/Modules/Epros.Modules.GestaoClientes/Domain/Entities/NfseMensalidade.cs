using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>
    /// 1.08J — MECANISMO/HOOK de NFS-e da mensalidade SaaS: registro da NECESSIDADE de emitir a NFS-e de
    /// uma competência mensal do serviço prestado pela Siser (landlord/prestador) ao tenant assinante
    /// (tomador). 1 registro por competência/fatura, idempotente.
    ///
    /// Base de negócio (skill Negocio-acumulado/fiscal/nfse):
    ///   • Software/SaaS é SERVIÇO tributável pelo ISS — LC 116/2003 item 1.05 ("licenciamento ou cessão de
    ///     direito de uso de programas de computação") e/ou 1.03 (processamento/hospedagem); STF ADI 1.945/5.659
    ///     (ISS, não ICMS). [RN47/RN48]
    ///   • Fato gerador = PRESTAÇÃO do serviço; em assinatura recorrente há um fato gerador POR COMPETÊNCIA
    ///     MENSAL → 1 NFS-e por competência (RN49/RN50).
    ///   • Município credor = município-sede da Siser (estabelecimento prestador, LC 116/2003 art. 3º) [RN52].
    ///
    /// ⛔ Regra #0 / GUARDA DE SEGURANÇA FISCAL: esta entidade só carrega o MECANISMO (competência, fatura,
    /// valor BASE, status, ambiente). NENHUMA alíquota, subitem, imposto ou número fiscal é calculado/inventado
    /// aqui — alíquota (2%–5%), subitem exato (1.05/1.03), regime, certificado e provedor municipal são
    /// PARÂMETRO do overlay `negocio-siser` (hoje VAZIO) + contador + infra. Enquanto isso o registro fica
    /// <see cref="NfseMensalidadeStatus.Pendente"/>. Ambiente de emissão default = Homologação (mesmo princípio
    /// da 1.07 <c>EmpresaParametrosDfe.CriarPadraoHomologacao</c>): nunca produção sem configuração deliberada.
    /// </summary>
    public class NfseMensalidade : EntidadeSaaSBase
    {
        /// <summary>Fatura de assinatura PAGA que originou a necessidade de NFS-e.</summary>
        public Guid FaturaId { get; private set; }

        /// <summary>Cliente/tenant assinante = TOMADOR do serviço (LC 116/2003; adquirente = tomador, NT 004).</summary>
        public Guid ClienteId { get; private set; }

        /// <summary>Mês de competência do serviço prestado (normalizado ao 1º dia do mês, UTC). 1 NFS-e por competência (RN50).</summary>
        public DateTime Competencia { get; private set; }

        /// <summary>Valor BASE (valor da fatura da competência). NÃO é base de cálculo fiscal com dedução — o ISS/IBS/CBS NÃO é calculado aqui.</summary>
        public decimal ValorBase { get; private set; }

        public NfseMensalidadeStatus Status { get; private set; } = NfseMensalidadeStatus.Pendente;

        /// <summary>Ambiente de emissão (default HOMOLOGAÇÃO — guarda fiscal, igual à 1.07). Produção só com config deliberada.</summary>
        public ETipoAmbiente Ambiente { get; private set; } = ETipoAmbiente.Homologacao;

        /// <summary>Motivo de o registro permanecer Pendente (ex.: "provedor municipal não configurado", "config fiscal incompleta") ou o motivo de Erro/Dispensa.</summary>
        public string? Motivo { get; private set; }

        /// <summary>Número/chave da NFS-e REAL — só preenchido quando o provedor municipal EFETIVAMENTE autoriza (nunca inventado).</summary>
        public string? NumeroNfse { get; private set; }

        /// <summary>Momento em que a NFS-e foi efetivamente emitida (null enquanto pendente/erro/dispensada).</summary>
        public DateTime? EmitidaEm { get; private set; }

        protected NfseMensalidade() { } // EF Core

        public NfseMensalidade(
            Guid faturaId,
            Guid clienteId,
            DateTime competencia,
            decimal valorBase,
            string tenantId,
            string criadoPor,
            ETipoAmbiente ambiente = ETipoAmbiente.Homologacao,
            string? motivo = null)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<NfseMensalidade>()
                .Requires()
                .AreNotEquals(faturaId, Guid.Empty, nameof(FaturaId), "FaturaId é obrigatório")
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "ClienteId é obrigatório")
                .IsGreaterThan(valorBase, 0, nameof(ValorBase), "Valor base da NFS-e deve ser maior que zero")
            );

            FaturaId = faturaId;
            ClienteId = clienteId;
            Competencia = NormalizarCompetencia(competencia);
            ValorBase = valorBase;
            Ambiente = ambiente;
            Status = NfseMensalidadeStatus.Pendente;
            Motivo = motivo;
        }

        /// <summary>Normaliza uma data para o 1º dia do seu mês (competência), em UTC.</summary>
        public static DateTime NormalizarCompetencia(DateTime data)
            => new DateTime(data.Year, data.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Registra/atualiza o motivo pelo qual o registro segue Pendente (não configurado / config incompleta).
        /// Idempotente por natureza — não muda o status (continua Pendente). NÃO emite nada.
        /// </summary>
        public void RegistrarPendencia(string motivo, string alteradoPor)
        {
            Status = NfseMensalidadeStatus.Pendente;
            Motivo = motivo;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>
        /// Marca a NFS-e como EMITIDA com o número REAL retornado pelo provedor municipal. Idempotente:
        /// se já emitida, é no-op. ⛔ SÓ deve ser chamado com um número autorizado de verdade (nunca fabricado).
        /// </summary>
        public bool MarcarEmitida(string numeroNfse, DateTime emitidaEm, string alteradoPor)
        {
            if (Status == NfseMensalidadeStatus.Emitida)
                return false;

            NumeroNfse = numeroNfse;
            EmitidaEm = emitidaEm;
            Status = NfseMensalidadeStatus.Emitida;
            Motivo = null;
            MarcarAlterado(alteradoPor);
            return true;
        }

        /// <summary>Marca uma tentativa de emissão que FALHOU no provedor (mantém o motivo para reprocessar).</summary>
        public void MarcarErro(string motivo, string alteradoPor)
        {
            Status = NfseMensalidadeStatus.Erro;
            Motivo = motivo;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Dispensa a competência de emissão (decisão do operador, ex.: fatura estornada/cancelada).</summary>
        public void Dispensar(string motivo, string alteradoPor)
        {
            Status = NfseMensalidadeStatus.Dispensada;
            Motivo = motivo;
            MarcarAlterado(alteradoPor);
        }
    }
}
