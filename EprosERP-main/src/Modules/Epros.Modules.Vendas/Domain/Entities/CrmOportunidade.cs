using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Oportunidade/deal comercial (crm_oportunidade). Fonte: EF §10.7. Regras CRM-025..CRM-028.
    /// </summary>
    public class CrmOportunidade : EntidadeSaaSBase
    {
        public Guid PipelineId { get; private set; }
        public Guid EtapaId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public decimal? Valor { get; private set; }
        public Guid? MoedaId { get; private set; }
        public decimal? ValorConvertido { get; private set; }
        public decimal? Probabilidade { get; private set; }
        public DateTime? DataFechamentoPrevista { get; private set; }
        public ECrmOportunidadeStatus Status { get; private set; } = ECrmOportunidadeStatus.Ativa;
        public Guid? OrigemId { get; private set; }
        public string? FontesJson { get; private set; }
        public string? ProdutosJson { get; private set; }
        public string? EtiquetasJson { get; private set; }
        public string? Telefone { get; private set; }
        public string? Notas { get; private set; }
        public int? PosicaoKanban { get; private set; }
        public Guid? LeadOrigemId { get; private set; }
        public Guid? ClientePrincipalId { get; private set; }
        public string? MotivoPerda { get; private set; }
        public Guid CriadoPorUsuarioId { get; private set; }

        protected CrmOportunidade() { }

        public CrmOportunidade(
            Guid pipelineId,
            Guid etapaId,
            string nome,
            decimal? valor,
            Guid? origemId,
            Guid? clientePrincipalId,
            Guid? leadOrigemId,
            string? notas,
            DateTime? dataFechamentoPrevista,
            Guid criadoPorUsuarioId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PipelineId = pipelineId;
            EtapaId = etapaId;
            Nome = nome;
            Valor = valor;
            OrigemId = origemId;
            ClientePrincipalId = clientePrincipalId;
            LeadOrigemId = leadOrigemId;
            Notas = notas;
            DataFechamentoPrevista = dataFechamentoPrevista;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            Status = ECrmOportunidadeStatus.Ativa;

            AddNotifications(new Contract<CrmOportunidade>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "A oportunidade deve possuir nome. [Origem: CrmOportunidade]")
                .AreNotEquals(pipelineId, Guid.Empty, nameof(PipelineId), "A oportunidade deve possuir pipeline. [Origem: CrmOportunidade]")
                .AreNotEquals(etapaId, Guid.Empty, nameof(EtapaId), "A oportunidade deve possuir etapa. [Origem: CrmOportunidade]"));
        }

        public void Alterar(string nome, decimal? valor, Guid? origemId, Guid? clientePrincipalId, string? notas, DateTime? dataFechamentoPrevista, string alteradoPor)
        {
            Nome = nome;
            Valor = valor;
            OrigemId = origemId;
            ClientePrincipalId = clientePrincipalId;
            Notas = notas;
            DataFechamentoPrevista = dataFechamentoPrevista;
            MarcarAlterado(alteradoPor);
        }

        public void MoverEtapa(Guid etapaId, int? posicaoKanban, string alteradoPor)
        {
            EtapaId = etapaId;
            PosicaoKanban = posicaoKanban;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>CRM-026: oportunidade ganha; pode acionar proposta/pedido/venda (EF §6.6).</summary>
        public void Ganhar(string alteradoPor)
        {
            Status = ECrmOportunidadeStatus.Ganha;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>CRM-026: oportunidade perdida preserva histórico e motivo de perda.</summary>
        public void Perder(string? motivoPerda, string alteradoPor)
        {
            Status = ECrmOportunidadeStatus.Perdida;
            MotivoPerda = motivoPerda;
            MarcarAlterado(alteradoPor);
        }

        public void Arquivar(string alteradoPor)
        {
            Status = ECrmOportunidadeStatus.Arquivada;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Participante da oportunidade (crm_oportunidade_participante). Fonte: EF §10.8. CRM-028.</summary>
    public class CrmOportunidadeParticipante : EntidadeSaaSBase
    {
        public Guid OportunidadeId { get; private set; }
        public ECrmTipoParticipante TipoParticipante { get; private set; }
        public Guid? ClienteId { get; private set; }
        public Guid? UsuarioId { get; private set; }
        public string? PermissoesJson { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected CrmOportunidadeParticipante() { }

        public CrmOportunidadeParticipante(Guid oportunidadeId, ECrmTipoParticipante tipo, Guid? clienteId, Guid? usuarioId, string? permissoesJson, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            OportunidadeId = oportunidadeId;
            TipoParticipante = tipo;
            ClienteId = clienteId;
            UsuarioId = usuarioId;
            PermissoesJson = permissoesJson;
            Ativo = true;

            AddNotifications(new Contract<CrmOportunidadeParticipante>()
                .Requires()
                .AreNotEquals(oportunidadeId, Guid.Empty, nameof(OportunidadeId), "A oportunidade é obrigatória. [Origem: CrmOportunidadeParticipante]"));

            // Obrigatório clienteId quando tipo Cliente; usuarioId quando tipo Usuário (EF §10.8).
            if (tipo == ECrmTipoParticipante.Cliente && (clienteId == null || clienteId == Guid.Empty))
                AddNotification(nameof(ClienteId), "Participante do tipo Cliente exige cliente. [Origem: CrmOportunidadeParticipante]");
            if (tipo == ECrmTipoParticipante.Usuario && (usuarioId == null || usuarioId == Guid.Empty))
                AddNotification(nameof(UsuarioId), "Participante do tipo Usuário exige usuário. [Origem: CrmOportunidadeParticipante]");
        }
    }
}
