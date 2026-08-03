using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Wizards
{
    /// <summary>
    /// PLT · WIZARDS (PD-04) — definição de um assistente: form dinâmico + wizard multi-etapa (builder).
    /// Canal público opcional (form embutível) com sanitização. A execução converte, de forma
    /// idempotente, para N destinos (os módulos donos consomem o evento de conclusão).
    /// </summary>
    public class DefinicaoWizard : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public bool Publico { get; private set; }
        public bool Ativo { get; private set; }

        protected DefinicaoWizard() { }

        public DefinicaoWizard(string codigo, string nome, string? descricao, bool publico, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<DefinicaoWizard>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O código do wizard é obrigatório.")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do wizard é obrigatório."));

            Codigo = codigo;
            Nome = nome;
            Descricao = descricao;
            Publico = publico;
            Ativo = false; // sobe inativo; publica-se após montar etapas/campos
        }

        public void Publicar(string usuario) { Ativo = true; MarcarAlterado(usuario); }
        public void Despublicar(string usuario) { Ativo = false; MarcarAlterado(usuario); }
    }

    public class EtapaWizard : EntidadeSaaSBase
    {
        public Guid DefinicaoId { get; private set; }
        public int Ordem { get; private set; }
        public string Titulo { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }

        protected EtapaWizard() { }

        public EtapaWizard(Guid definicaoId, int ordem, string titulo, string? descricao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<EtapaWizard>()
                .Requires()
                .IsTrue(definicaoId != Guid.Empty, nameof(DefinicaoId), "A definição é obrigatória.")
                .IsGreaterOrEqualsThan(ordem, 1, nameof(Ordem), "A ordem deve ser >= 1.")
                .IsNotNullOrEmpty(titulo, nameof(Titulo), "O título da etapa é obrigatório."));

            DefinicaoId = definicaoId;
            Ordem = ordem;
            Titulo = titulo;
            Descricao = descricao;
        }
    }

    /// <summary>Campo configurável de uma etapa. O tipo pertence ao catálogo mínimo (V1).</summary>
    public class CampoWizard : EntidadeSaaSBase
    {
        /// <summary>Catálogo mínimo de tipos de campo (PD-04, V1).</summary>
        public static readonly HashSet<string> TiposValidos = new(StringComparer.OrdinalIgnoreCase)
        {
            "texto", "textolongo", "numero", "data", "booleano", "selecao", "email", "telefone", "cpf_cnpj"
        };

        public Guid EtapaId { get; private set; }
        public string Chave { get; private set; } = string.Empty;
        public string Rotulo { get; private set; } = string.Empty;
        public string Tipo { get; private set; } = "texto";
        public bool Obrigatorio { get; private set; }
        public string? OpcoesJson { get; private set; } // para "selecao"
        public int Ordem { get; private set; }

        protected CampoWizard() { }

        public CampoWizard(Guid etapaId, string chave, string rotulo, string tipo, bool obrigatorio,
            string? opcoesJson, int ordem, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<CampoWizard>()
                .Requires()
                .IsTrue(etapaId != Guid.Empty, nameof(EtapaId), "A etapa é obrigatória.")
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do campo é obrigatória.")
                .IsNotNullOrEmpty(rotulo, nameof(Rotulo), "O rótulo do campo é obrigatório.")
                .IsTrue(TiposValidos.Contains(tipo), nameof(Tipo), $"Tipo de campo inválido. Válidos: {string.Join(", ", TiposValidos)}."));

            EtapaId = etapaId;
            Chave = chave;
            Rotulo = rotulo;
            Tipo = tipo;
            Obrigatorio = obrigatorio;
            OpcoesJson = opcoesJson;
            Ordem = ordem;
        }
    }

    /// <summary>Estado de execução de um wizard por tenant (respostas acumuladas + etapa atual).</summary>
    public class ExecucaoWizard : EntidadeSaaSBase
    {
        public Guid DefinicaoId { get; private set; }
        /// <summary>EmAndamento, Concluida, Cancelada.</summary>
        public string Status { get; private set; } = "EmAndamento";
        public int EtapaAtualOrdem { get; private set; }
        public string RespostasJson { get; private set; } = "{}";
        public bool CanalPublico { get; private set; }
        public string? TokenPublico { get; private set; }

        protected ExecucaoWizard() { }

        public ExecucaoWizard(Guid definicaoId, bool canalPublico, string? tokenPublico, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ExecucaoWizard>()
                .Requires()
                .IsTrue(definicaoId != Guid.Empty, nameof(DefinicaoId), "A definição é obrigatória."));

            DefinicaoId = definicaoId;
            Status = "EmAndamento";
            EtapaAtualOrdem = 1;
            RespostasJson = "{}";
            CanalPublico = canalPublico;
            TokenPublico = tokenPublico;
        }

        public bool EmAndamento => Status == "EmAndamento";

        public void RegistrarRespostas(string respostasJson, int proximaEtapa, string usuario)
        {
            RespostasJson = respostasJson;
            EtapaAtualOrdem = proximaEtapa;
            MarcarAlterado(usuario);
        }

        public void Concluir(string respostasJson, string usuario)
        {
            RespostasJson = respostasJson;
            Status = "Concluida";
            MarcarAlterado(usuario);
        }

        public void Cancelar(string usuario)
        {
            Status = "Cancelada";
            MarcarAlterado(usuario);
        }
    }
}
