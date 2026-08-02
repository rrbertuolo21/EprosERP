using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-REG — Certificado digital (grc_reg_certificado_digital). Controla certificado,
    /// validade, empresa e origem. Fiel a EF_13_GRC_COMPLIANCE_REGULATORIO_V1 (secoes 10.2, 10.4 e 12).
    /// SEGURANCA: a senha/PFX do certificado NAO e armazenada nesta entidade (lacuna registrada
    /// na EF 10.7). Guarda-se apenas metadados de conformidade.
    /// </summary>
    public class CertificadoDigital : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public string Cnpj { get; private set; } = string.Empty;
        public string Serial { get; private set; } = string.Empty;
        public string Tipo { get; private set; } = "A1"; // A1, A3
        public string Origem { get; private set; } = "Empresa"; // Empresa, EscritorioContador
        // Compat: DataValidade permanece = ValidadeFim. D-REG-01 acrescenta validade inicial/final.
        public DateTime DataValidade { get; private set; }
        public DateTime? ValidadeInicio { get; private set; }
        public DateTime? ValidadeFim { get; private set; }
        // D-REG-01 — metadados exigidos pela EF que faltavam.
        public string? Nome { get; private set; }
        public string? ResumoCertificado { get; private set; }
        // D-REG-01 / T5 — SEGREDO NUNCA EM CLARO: apenas ponteiros para o cofre central de segredos.
        public string? SenhaReferenciaSegura { get; private set; }
        public string? CaminhoReferenciaSegura { get; private set; }
        // Rascunho, Validado, Ativo, Vencido, Revogado, Inativo
        public string Status { get; private set; } = "Rascunho";
        public string? MotivoRevogacao { get; private set; }

        protected CertificadoDigital() { } // EF Core

        public CertificadoDigital(
            Guid empresaId,
            string cnpj,
            string serial,
            string tipo,
            string origem,
            DateTime dataValidade,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<CertificadoDigital>()
                .Requires()
                .IsTrue(empresaId != Guid.Empty, nameof(EmpresaId), "A empresa do certificado e obrigatoria.")
                .IsNotNullOrEmpty(cnpj, nameof(Cnpj), "O CNPJ do certificado e obrigatorio.")
                .IsNotNullOrEmpty(serial, nameof(Serial), "O serial do certificado e obrigatorio.")
                .IsTrue(tipo == "A1" || tipo == "A3", nameof(Tipo), "O tipo do certificado deve ser 'A1' ou 'A3'.")
                .IsTrue(origem == "Empresa" || origem == "EscritorioContador", nameof(Origem),
                    "A origem deve ser 'Empresa' ou 'EscritorioContador'.")
            );

            EmpresaId = empresaId;
            Cnpj = cnpj;
            Serial = serial;
            Tipo = tipo;
            Origem = origem;
            DataValidade = dataValidade;
            ValidadeFim = dataValidade;
            Status = "Rascunho";
        }

        /// <summary>D-REG-01 — enriquece com nome/resumo e a janela de validade inicial/final.</summary>
        public void DefinirDetalhes(string? nome, string? resumo, DateTime? validadeInicio, DateTime? validadeFim, string usuario)
        {
            if (validadeInicio != null && validadeFim != null && validadeFim < validadeInicio)
            {
                AddNotification(nameof(ValidadeFim), "A validade final nao pode ser anterior a inicial.");
                return;
            }
            Nome = nome;
            ResumoCertificado = resumo;
            if (validadeInicio != null) ValidadeInicio = validadeInicio;
            if (validadeFim != null) { ValidadeFim = validadeFim; DataValidade = validadeFim.Value; }
            MarcarAlterado(usuario);
        }

        /// <summary>
        /// D-REG-01 / T5 — vincula o certificado ao cofre central de segredos guardando SÓ as
        /// referências (nunca a senha/chave em claro no módulo).
        /// </summary>
        public void VincularCofre(string senhaReferenciaSegura, string caminhoReferenciaSegura, string usuario)
        {
            if (string.IsNullOrWhiteSpace(senhaReferenciaSegura) || string.IsNullOrWhiteSpace(caminhoReferenciaSegura))
            {
                AddNotification(nameof(SenhaReferenciaSegura), "As referencias de cofre (senha e caminho) sao obrigatorias.");
                return;
            }
            SenhaReferenciaSegura = senhaReferenciaSegura;
            CaminhoReferenciaSegura = caminhoReferenciaSegura;
            MarcarAlterado(usuario);
        }

        /// <summary>
        /// D-REG-04 (default seguro) — validações locais DETERMINÍSTICAS já implementáveis, sem o
        /// serviço DFe (que fica atrás de gate D-AMB-03 até o contrato com a Siser/contador):
        /// empresa existe, CNPJ do certificado == CNPJ da empresa e hoje dentro da validade.
        /// Retorna a lista de falhas (vazia = válido). // valida-fiscal (transmissão DFe pendente).
        /// </summary>
        public IReadOnlyList<string> ValidarLocal(bool empresaExiste, string cnpjEmpresa, DateTime referencia)
        {
            var falhas = new List<string>();
            if (!empresaExiste)
                falhas.Add("A empresa vinculada ao certificado nao existe.");
            if (!string.IsNullOrWhiteSpace(cnpjEmpresa) && SomenteDigitos(cnpjEmpresa) != SomenteDigitos(Cnpj))
                falhas.Add("O CNPJ do certificado nao pertence a empresa (pertencimento).");
            var inicio = ValidadeInicio ?? CriadoEm;
            var fim = ValidadeFim ?? DataValidade;
            if (referencia < inicio)
                falhas.Add("O certificado ainda nao entrou em vigencia.");
            if (referencia > fim)
                falhas.Add("O certificado esta vencido.");
            return falhas;
        }

        private static string SomenteDigitos(string s) =>
            new string((s ?? string.Empty).Where(char.IsDigit).ToArray());

        public void Validar(string usuario)
        {
            if (Status != "Rascunho")
            {
                AddNotification(nameof(Status), "Somente certificados em rascunho podem ser validados.");
                return;
            }
            Status = "Validado";
            MarcarAlterado(usuario);
        }

        public void Ativar(string usuario)
        {
            if (Status != "Validado")
            {
                AddNotification(nameof(Status), "Somente certificados validados podem ser ativados.");
                return;
            }
            Status = "Ativo";
            MarcarAlterado(usuario);
        }

        public void MarcarVencido(string usuario)
        {
            if (Status != "Ativo")
            {
                AddNotification(nameof(Status), "Somente certificados ativos podem ser marcados como vencidos.");
                return;
            }
            Status = "Vencido";
            MarcarAlterado(usuario);
        }

        public void Revogar(string motivo, string usuario)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoRevogacao), "O motivo da revogacao e obrigatorio.");
                return;
            }
            Status = "Revogado";
            MotivoRevogacao = motivo;
            MarcarAlterado(usuario);
        }
    }
}
