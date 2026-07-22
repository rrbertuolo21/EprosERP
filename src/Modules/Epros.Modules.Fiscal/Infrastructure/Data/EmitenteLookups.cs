using System;

namespace Epros.Modules.Fiscal.Infrastructure.Data
{
    /// <summary>
    /// Lookups de LEITURA cross-module (padrão §6.4). O módulo Fiscal NÃO referencia o projeto
    /// GestaoClientes; para resolver o emitente/certificado/parâmetros DF-e da Empresa emitente,
    /// projetamos as tabelas do schema <c>plataforma</c> (pertencentes ao GestaoClientes) como
    /// entidades keyless/flat mapeadas com <c>ExcludeFromMigrations</c> no <see cref="ContextFiscal"/>.
    ///
    /// As colunas seguem o snake_case aplicado automaticamente pelo ContextBase, coincidindo com as
    /// colunas geradas pelo ContextGestaoClientes (empresas / empresa_certificado /
    /// empresas_parametros_dfe / configuracoes_globais).
    /// </summary>
    public class EmpresaLookup
    {
        public Guid Id { get; set; }
        public string RazaoSocial { get; set; } = string.Empty;
        public string? NomeFantasia { get; set; }
        public string Cnpj { get; set; } = string.Empty;
        public string? Cpf { get; set; }
        public string? InscricaoEstadual { get; set; }
        public string? InscricaoMunicipal { get; set; }
        public string? Cnae { get; set; }

        /// <summary>RegimeTributario (GestaoClientes): 1=SimplesNacional, 2=LucroPresumido, 3=LucroReal.</summary>
        public int RegimeTributario { get; set; }
        public bool EhMei { get; set; }

        public Guid? CertificadoDigitalId { get; set; }
        public Guid? EmpresaParametrosDfeId { get; set; }

        // Endereço (owned type embutido na própria tabela empresas)
        public string Logradouro { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string? Complemento { get; set; }
        public string Bairro { get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;

        public string TenantId { get; set; } = string.Empty;
        public DateTime? DeletadoEm { get; set; }
    }

    public class EmpresaCertificadoLookup
    {
        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public Guid CertificadoSegredoId { get; set; }
        public Guid SenhaSegredoId { get; set; }
        public string Serial { get; set; } = string.Empty;

        /// <summary>Titular (Subject) do certificado — informativo para a tela de gestão.</summary>
        public string? Titular { get; set; }

        /// <summary>Início da validade do A1 (NotBefore do X509), gravado pelo GestaoClientes.</summary>
        public DateTime? ValidadeInicial { get; set; }

        /// <summary>
        /// Fim da validade do A1 (NotAfter do X509). Usado pelo provider fiscal para rejeitar
        /// transmissão com certificado vencido (fallback honesto — não simula autorização).
        /// </summary>
        public DateTime? ValidadeFinal { get; set; }

        public string TenantId { get; set; } = string.Empty;
        public DateTime? DeletadoEm { get; set; }
    }

    /// <summary>
    /// Projeção plana de <c>empresas_parametros_dfe</c> — inclui as colunas dos owned types
    /// (Nfe / NfceProducao / NfceHomologacao) que no banco ficam achatadas na mesma tabela.
    /// </summary>
    public class EmpresaParametrosDfeLookup
    {
        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }

        /// <summary>TipoAmbiente (string): "Producao" ou "Homologacao".</summary>
        public string TipoAmbienteNfe { get; set; } = string.Empty;
        public string TipoAmbienteNfce { get; set; } = string.Empty;

        // ---- Bloco NF-e (owned type Nfe) ----
        public int NfeSerieProducao { get; set; }
        public long NfeProximoNrProducao { get; set; }
        public int NfeSerieHomologacao { get; set; }
        public long NfeProximoNrHomologacao { get; set; }

        // ---- Bloco NFC-e produção (owned type NfceProducao) ----
        public string? NfceCscProducao { get; set; }
        public string? NfceIdCscProducao { get; set; }
        public int NfceSerieProducao { get; set; }
        public long NfceProximoNrProducao { get; set; }

        // ---- Bloco NFC-e homologação (owned type NfceHomologacao) ----
        public string? NfceCscHomologacao { get; set; }
        public string? NfceIdCscHomologacao { get; set; }
        public int NfceSerieHomologacao { get; set; }
        public long NfceProximoNrHomologacao { get; set; }

        public string TenantId { get; set; } = string.Empty;
        public DateTime? DeletadoEm { get; set; }
    }

    /// <summary>
    /// Projeção de <c>configuracoes_globais</c> — onde ficam os segredos criptografados do certificado
    /// (chave <c>certificado.{segredoId}</c>) e da senha (chave <c>senha_certificado.{segredoId}</c>),
    /// gravados pelo EmpresasController. O valor é ciphertext descriptografado via ISegredoCofreService.
    /// </summary>
    public class ConfiguracaoGlobalLookup
    {
        public Guid Id { get; set; }
        public string Chave { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public DateTime? DeletadoEm { get; set; }
    }
}
