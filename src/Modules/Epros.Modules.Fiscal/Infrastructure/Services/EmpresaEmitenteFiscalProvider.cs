using System;
using System.Linq;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Infrastructure.Services
{
    /// <summary>
    /// Provider REAL de emitente/certificado. Resolve, a partir da <c>Empresa</c> emitente vinculada
    /// ao <see cref="DocumentoFiscal.EmpresaId"/>, o contexto de transmissão (UF, CRT, série, próximo
    /// número, CSC/idCSC, ambiente) + o certificado A1 (bytes + senha).
    ///
    /// LEITURA cross-module via Lookups (§6.4): as tabelas <c>empresas</c> / <c>empresa_certificado</c> /
    /// <c>empresas_parametros_dfe</c> / <c>configuracoes_globais</c> pertencem ao GestaoClientes (schema
    /// plataforma) e são projetadas como entidades planas (ExcludeFromMigrations) no <see cref="ContextFiscal"/>.
    ///
    /// O .pfx e a senha ficam CRIPTOGRAFADOS no cofre (<c>configuracoes_globais</c>, chaves
    /// <c>certificado.{segredoId}</c> / <c>senha_certificado.{segredoId}</c>); aqui descriptografamos via
    /// <see cref="ISegredoCofreService"/>. O .pfx é armazenado em base64, então decodificamos para bytes.
    ///
    /// FALLBACK HONESTO: se faltar EmpresaId, empresa, certificado ou parâmetros DF-e, retorna <c>null</c>
    /// (o <see cref="MotorLegadoFiscalService"/> degrada de forma controlada, sem simular chave/protocolo).
    /// </summary>
    public class EmpresaEmitenteFiscalProvider : IEmitenteFiscalProvider
    {
        private readonly ContextFiscal _context;
        private readonly ISegredoCofreService _cofreService;

        public EmpresaEmitenteFiscalProvider(ContextFiscal context, ISegredoCofreService cofreService)
        {
            _context = context;
            _cofreService = cofreService;
        }

        public ContextoTransmissaoFiscal? ObterContexto(DocumentoFiscal documento)
        {
            if (documento is null || documento.EmpresaId is null || documento.EmpresaId == Guid.Empty)
                return null;

            var empresa = _context.EmpresasLookup
                .AsNoTracking()
                .FirstOrDefault(e => e.Id == documento.EmpresaId.Value);

            return MontarContexto(empresa, documento.Modelo, documento.Ambiente);
        }

        public ContextoTransmissaoFiscal? ObterContextoPorDocumento(string documentoEmitente, string modelo, int ambiente)
        {
            var doc = SomenteDigitos(documentoEmitente);
            if (string.IsNullOrWhiteSpace(doc))
                return null;

            // Resolve a empresa emitente pelo CNPJ (ou CPF) informado.
            var empresa = _context.EmpresasLookup
                .AsNoTracking()
                .FirstOrDefault(e => e.Cnpj == doc || e.Cpf == doc);

            return MontarContexto(empresa, modelo, ambiente);
        }

        private ContextoTransmissaoFiscal? MontarContexto(EmpresaLookup? empresa, string modelo, int ambiente)
        {
            if (empresa is null)
                return null;

            var empresaId = empresa.Id;

            var certificado = ResolverCertificado(empresaId);
            if (certificado is null)
                return null;

            var parametros = _context.EmpresasParametrosDfeLookup
                .AsNoTracking()
                .FirstOrDefault(p => p.EmpresaId == empresaId);
            if (parametros is null)
                return null;

            var ehNfce = modelo == "65";
            var ambienteProducao = ambiente == 1;

            var emitente = new EmitenteFiscalDto
            {
                Documento = SomenteDigitos(!string.IsNullOrWhiteSpace(empresa.Cnpj) ? empresa.Cnpj : empresa.Cpf ?? string.Empty),
                RegimeTributario = MapearCrt(empresa.RegimeTributario, empresa.EhMei),
                RazaoSocial = empresa.RazaoSocial,
                NomeFantasia = empresa.NomeFantasia,
                InscricaoEstadual = empresa.InscricaoEstadual,
                InscricaoMunicipal = empresa.InscricaoMunicipal,
                Cnae = empresa.Cnae,

                Uf = empresa.Estado,
                Logradouro = empresa.Logradouro,
                Numero = empresa.Numero,
                Complemento = empresa.Complemento,
                Bairro = empresa.Bairro,
                NomeMunicipio = empresa.Cidade,
                Cep = SomenteDigitos(empresa.Cep),

                Csc = ehNfce ? ObterCsc(parametros, ambienteProducao) : null,
                CscId = ehNfce ? ObterCscId(parametros, ambienteProducao) : null,

                Certificado = certificado
            };

            return new ContextoTransmissaoFiscal
            {
                Emitente = emitente
                // Destinatário: o mapper usa os dados mínimos do próprio DocumentoFiscal
                // (DestinatarioCnpjCpf/DestinatarioNome) quando o DTO é nulo.
            };
        }

        private CertificadoDigitalDto? ResolverCertificado(Guid empresaId)
        {
            // Certificado mais recente (não deletado) da empresa.
            var cert = _context.EmpresasCertificadosLookup
                .AsNoTracking()
                .Where(c => c.EmpresaId == empresaId)
                .OrderByDescending(c => c.Id)
                .FirstOrDefault();
            if (cert is null)
                return null;

            // Fallback honesto: certificado A1 vencido não transmite (a SEFAZ rejeitaria a assinatura).
            // Só bloqueia quando há data de validade conhecida — certificados legados sem a coluna passam.
            if (cert.ValidadeFinal.HasValue && cert.ValidadeFinal.Value.ToUniversalTime() < DateTime.UtcNow)
                return null;

            var chaveCert = $"certificado.{cert.CertificadoSegredoId}";
            var chaveSenha = $"senha_certificado.{cert.SenhaSegredoId}";

            var configCert = _context.ConfiguracoesGlobaisLookup
                .AsNoTracking()
                .FirstOrDefault(c => c.Chave == chaveCert);
            var configSenha = _context.ConfiguracoesGlobaisLookup
                .AsNoTracking()
                .FirstOrDefault(c => c.Chave == chaveSenha);

            if (configCert is null || string.IsNullOrWhiteSpace(configCert.Valor)
                || configSenha is null || string.IsNullOrWhiteSpace(configSenha.Valor))
                return null;

            try
            {
                // Segredos ficam criptografados no cofre; descriptografa. O .pfx é armazenado em base64.
                var base64Pfx = _cofreService.DescriptografarAsync(configCert.Valor).GetAwaiter().GetResult();
                var senha = _cofreService.DescriptografarAsync(configSenha.Valor).GetAwaiter().GetResult();

                if (string.IsNullOrWhiteSpace(base64Pfx))
                    return null;

                var bytes = Convert.FromBase64String(base64Pfx);

                return new CertificadoDigitalDto
                {
                    ArrayBytes = bytes,
                    Senha = senha ?? string.Empty
                };
            }
            catch (FormatException)
            {
                // .pfx armazenado em formato inesperado (não-base64): fallback honesto.
                return null;
            }
        }

        /// <summary>
        /// Mapeia o RegimeTributario da Empresa (1=SimplesNacional, 2=LucroPresumido, 3=LucroReal)
        /// para o CRT do XML (1=SimplesNacional, 3=RegimeNormal, 4=SimplesNacionalMei).
        /// </summary>
        private static int MapearCrt(int regimeTributarioEmpresa, bool ehMei)
        {
            // 1 = SimplesNacional na Empresa
            if (regimeTributarioEmpresa == 1)
                return ehMei ? 4 /* SimplesNacionalMei */ : 1 /* SimplesNacional */;

            // LucroPresumido (2) / LucroReal (3) => Regime Normal (CRT 3)
            return 3;
        }

        private static string? ObterCsc(EmpresaParametrosDfeLookup p, bool producao)
            => producao ? p.NfceCscProducao : p.NfceCscHomologacao;

        private static string? ObterCscId(EmpresaParametrosDfeLookup p, bool producao)
            => producao ? p.NfceIdCscProducao : p.NfceIdCscHomologacao;

        private static string SomenteDigitos(string? valor)
            => string.IsNullOrEmpty(valor) ? string.Empty : new string(valor.Where(char.IsDigit).ToArray());
    }
}
