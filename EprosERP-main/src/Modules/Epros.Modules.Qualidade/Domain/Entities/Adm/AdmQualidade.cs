using System;
using System.Collections.Generic;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Qualidade.Domain.Entities
{
    /// <summary>
    /// qld_adm_qualidade — Registro central da administracao da qualidade (SGQ).
    /// Porte fiel da EF QUALIDADE / ADMINISTRACAO_DA_QUALIDADE (secao 12.1).
    /// </summary>
    public class AdmQualidade : EntidadeSaaSBase
    {
        public long? SequenciaExibicao { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public EStatusRegistroQualidade Status { get; private set; }
        public Guid ResponsavelId { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public int Versao { get; private set; }

        private readonly List<AdmDocumentoQms> _documentos = new();
        public IReadOnlyCollection<AdmDocumentoQms> Documentos => _documentos.AsReadOnly();

        private readonly List<AdmObjetivo> _objetivos = new();
        public IReadOnlyCollection<AdmObjetivo> Objetivos => _objetivos.AsReadOnly();

        private readonly List<AdmProgramaAuditoria> _programasAuditoria = new();
        public IReadOnlyCollection<AdmProgramaAuditoria> ProgramasAuditoria => _programasAuditoria.AsReadOnly();

        protected AdmQualidade() { }

        public AdmQualidade(string codigo, string descricao, Guid responsavelId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            ResponsavelId = responsavelId;
            Status = EStatusRegistroQualidade.Rascunho;
            DataCriacao = DateTime.UtcNow;
            Versao = 1;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<AdmQualidade>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo e obrigatorio [Origem: AdmQualidade]")
                .IsLowerOrEqualsThan(Codigo?.Length ?? 0, 30, nameof(Codigo), "O codigo deve ter no maximo 30 caracteres [Origem: AdmQualidade]")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descricao e obrigatoria [Origem: AdmQualidade]")
                .IsLowerOrEqualsThan(Descricao?.Length ?? 0, 500, nameof(Descricao), "A descricao deve ter no maximo 500 caracteres [Origem: AdmQualidade]")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio [Origem: AdmQualidade]"));
        }

        // Maquina de estados: Rascunho->EmAnalise->Ativo; Ativo->Suspenso/Encerrado/Inativo; etc.
        public void Submeter(string usuario) => Transicionar(EStatusRegistroQualidade.Rascunho, EStatusRegistroQualidade.EmAnalise, usuario);
        public void Aprovar(string usuario) => Transicionar(EStatusRegistroQualidade.EmAnalise, EStatusRegistroQualidade.Ativo, usuario);
        public void Rejeitar(string usuario) => Transicionar(EStatusRegistroQualidade.EmAnalise, EStatusRegistroQualidade.Rascunho, usuario);
        public void Suspender(string usuario) => Transicionar(EStatusRegistroQualidade.Ativo, EStatusRegistroQualidade.Suspenso, usuario);
        public void Encerrar(string usuario) => Transicionar(EStatusRegistroQualidade.Ativo, EStatusRegistroQualidade.Encerrado, usuario);
        public void Retomar(string usuario) => Transicionar(EStatusRegistroQualidade.Suspenso, EStatusRegistroQualidade.Ativo, usuario);

        private void Transicionar(EStatusRegistroQualidade de, EStatusRegistroQualidade para, string usuario)
        {
            if (Status != de)
            {
                AddNotification(nameof(Status), $"Transicao invalida de {Status} para {para} [Origem: AdmQualidade]");
                return;
            }
            Status = para;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void AdicionarDocumento(AdmDocumentoQms documento) => _documentos.Add(documento);
        public void AdicionarObjetivo(AdmObjetivo objetivo) => _objetivos.Add(objetivo);
        public void AdicionarProgramaAuditoria(AdmProgramaAuditoria programa) => _programasAuditoria.Add(programa);
    }
}
