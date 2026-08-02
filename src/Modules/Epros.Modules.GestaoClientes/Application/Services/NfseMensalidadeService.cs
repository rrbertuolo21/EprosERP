using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Services
{
    /// <summary>
    /// 1.08J — MECANISMO/HOOK de NFS-e da mensalidade SaaS. Quando uma <see cref="Fatura"/> de assinatura é
    /// PAGA, registra a NECESSIDADE de NFS-e da COMPETÊNCIA correspondente (<see cref="NfseMensalidade"/>),
    /// idempotente por (fatura, competência) — 1 NFS-e por competência mensal (RN49/RN50 da skill
    /// Negocio-acumulado/fiscal/nfse). NÃO chama SaveChanges (o orquestrador decide o commit), igual ao
    /// <see cref="FaturaLiquidacaoService"/>.
    ///
    /// ⛔ GUARDA DE SEGURANÇA FISCAL (Regra #0): o mecanismo PREPARA os dados e PARA no provedor. A emissão
    /// REAL só é tentada se a <c>ConfiguracaoFiscal</c> da Siser (parâmetros do overlay `negocio-siser`, hoje
    /// VAZIO) tiver alíquota + subitem (1.05/1.03) + município-sede + certificado. Faltando qualquer um, o
    /// registro fica <see cref="NfseMensalidadeStatus.Pendente"/> — NUNCA emite com dado fabricado. Mesmo com a
    /// config completa, a emissão passa pelo <see cref="INfseProvider"/>, cujo default
    /// (<see cref="NfseProviderNaoConfigurado"/>) devolve "não configurado". NENHUMA alíquota/subitem/base
    /// fiscal é inventada aqui. [LC 116/2003 item 1.05/1.03; STF ADI 1.945/5.659]
    /// </summary>
    public class NfseMensalidadeService
    {
        // Chaves de PARÂMETRO fiscal (ConfiguracaoGlobal). Enquanto o overlay `negocio-siser` estiver vazio,
        // NENHUMA existe → a guarda barra a emissão e o registro fica Pendente. NUNCA hardcodar valor aqui.
        /// <summary>Alíquota efetiva do ISS (2%–5%) — PARÂMETRO do município-sede da Siser (contador). LC 116/2003 arts. 8º/8º-A.</summary>
        public const string ChaveAliquotaIss = "fiscal.nfse.aliquota";
        /// <summary>Subitem da lista da LC 116/2003 adotado (1.05 e/ou 1.03) — PARÂMETRO (decisão do município-sede + contador).</summary>
        public const string ChaveSubitemLc116 = "fiscal.nfse.subitem";
        /// <summary>Código IBGE do município credor (estabelecimento prestador, LC 116/2003 art. 3º) — PARÂMETRO.</summary>
        public const string ChaveMunicipioIncidencia = "fiscal.nfse.municipio";
        /// <summary>Indica que há certificado digital configurado para transmitir a NFS-e — PARÂMETRO/infra.</summary>
        public const string ChaveCertificadoConfigurado = "fiscal.nfse.certificado";

        private readonly ContextGestaoClientes _context;
        private readonly INfseProvider _provider;

        public NfseMensalidadeService(ContextGestaoClientes context, INfseProvider? provider = null)
        {
            _context = context;
            // Default seguro: sem provedor municipal configurado → "não configurado" (registro segue Pendente).
            _provider = provider ?? new NfseProviderNaoConfigurado();
        }

        /// <summary>
        /// MARCAÇÃO POR COMPETÊNCIA: registra (SEM persistir) a necessidade de NFS-e da competência da fatura
        /// paga. Idempotente por (fatura, competência): se já existir o registro, retorna o existente sem
        /// duplicar. O registro nasce <see cref="NfseMensalidadeStatus.Pendente"/> em ambiente HOMOLOGAÇÃO,
        /// com o motivo derivado da guarda fiscal — NÃO emite nada.
        /// </summary>
        public async Task<NfseMensalidade> RegistrarPendenteAsync(
            Fatura fatura,
            DateTime competencia,
            string criadoPor,
            CancellationToken cancellationToken)
        {
            var comp = NfseMensalidade.NormalizarCompetencia(competencia);

            var existente = await _context.NfseMensalidades
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(n => n.FaturaId == fatura.Id
                                          && n.Competencia == comp
                                          && n.DeletadoEm == null, cancellationToken);
            if (existente != null)
                return existente;

            var (podeEmitir, motivo) = await AvaliarGuardaFiscalAsync(cancellationToken);
            var registro = new NfseMensalidade(
                faturaId: fatura.Id,
                clienteId: fatura.ClienteId,
                competencia: comp,
                valorBase: fatura.Valor,
                tenantId: fatura.TenantId,
                criadoPor: criadoPor,
                ambiente: ETipoAmbiente.Homologacao,
                motivo: podeEmitir ? null : motivo);

            await _context.NfseMensalidades.AddAsync(registro, cancellationToken);
            return registro;
        }

        /// <summary>
        /// GUARDA FISCAL: a NFS-e só pode ser tentada se TODOS os parâmetros do overlay `negocio-siser`
        /// existirem (alíquota + subitem + município + certificado). Enquanto o overlay estiver VAZIO,
        /// retorna (false, motivo). NÃO lê alíquota para calcular imposto — só verifica presença.
        /// </summary>
        public async Task<(bool PodeEmitir, string Motivo)> AvaliarGuardaFiscalAsync(CancellationToken cancellationToken)
        {
            var faltantes = new List<string>();
            if (string.IsNullOrWhiteSpace(await LerConfigAsync(ChaveAliquotaIss, cancellationToken))) faltantes.Add("alíquota ISS");
            if (string.IsNullOrWhiteSpace(await LerConfigAsync(ChaveSubitemLc116, cancellationToken))) faltantes.Add("subitem LC 116 (1.05/1.03)");
            if (string.IsNullOrWhiteSpace(await LerConfigAsync(ChaveMunicipioIncidencia, cancellationToken))) faltantes.Add("município-sede");
            if (string.IsNullOrWhiteSpace(await LerConfigAsync(ChaveCertificadoConfigurado, cancellationToken))) faltantes.Add("certificado digital");

            if (faltantes.Count == 0)
                return (true, string.Empty);

            return (false,
                "Config fiscal incompleta (overlay negocio-siser): falta " + string.Join(", ", faltantes) +
                ". Emissão bloqueada; registro Pendente. [Regra #0 — negócio vem da skill; LC 116/2003 item 1.05/1.03]");
        }

        private async Task<string?> LerConfigAsync(string chave, CancellationToken cancellationToken)
        {
            var cfg = await _context.ConfiguracoesGlobais
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Chave == chave && c.DeletadoEm == null, cancellationToken);
            return cfg?.Valor;
        }

        /// <summary>
        /// TENTATIVA DE EMISSÃO (passo futuro, fora do hook de pagamento): só age em registros Pendente/Erro.
        /// (1) Passa pela GUARDA FISCAL — faltando parâmetro, mantém Pendente com o motivo (não chama provedor,
        /// não inventa dado). (2) Com a config completa, delega ao <see cref="INfseProvider"/>; o resultado
        /// move o registro para Emitida (com número REAL), Erro (com motivo) ou mantém Pendente (não configurado).
        /// NÃO persiste (o orquestrador commita). Retorna o resultado do provedor para observabilidade.
        /// </summary>
        public async Task<NfseEmissaoResultado> TentarEmitirAsync(
            NfseMensalidade registro,
            string alteradoPor,
            CancellationToken cancellationToken)
        {
            if (registro.Status == NfseMensalidadeStatus.Emitida || registro.Status == NfseMensalidadeStatus.Dispensada)
                return NfseEmissaoResultado.NaoConfigurado("Registro já finalizado; nada a emitir.");

            var (podeEmitir, motivo) = await AvaliarGuardaFiscalAsync(cancellationToken);
            if (!podeEmitir)
            {
                // ⛔ Guarda de segurança fiscal: sem alíquota/subitem/município/certificado, NÃO emite.
                registro.RegistrarPendencia(motivo, alteradoPor);
                return NfseEmissaoResultado.NaoConfigurado(motivo);
            }

            var dados = new NfseEmissaoDados(
                registro.Id, registro.FaturaId, registro.ClienteId,
                registro.Competencia, registro.ValorBase, registro.TenantId, registro.Ambiente.ToString());

            var resultado = await _provider.EmitirAsync(dados, cancellationToken);
            switch (resultado.Situacao)
            {
                case NfseEmissaoSituacao.Emitida when !string.IsNullOrWhiteSpace(resultado.NumeroNfse):
                    registro.MarcarEmitida(resultado.NumeroNfse!, DateTime.UtcNow, alteradoPor);
                    break;
                case NfseEmissaoSituacao.Erro:
                    registro.MarcarErro(resultado.Motivo ?? "Falha do provedor de NFS-e.", alteradoPor);
                    break;
                default: // NaoConfigurado (ou Emitida sem número → trata como não configurado, nunca inventa)
                    registro.RegistrarPendencia(resultado.Motivo ?? NfseProviderNaoConfigurado.MotivoPadrao, alteradoPor);
                    break;
            }
            return resultado;
        }
    }
}
