using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Manutencao.Domain.Entities
{
    public class Equipamento : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string Codigo { get; private set; } = string.Empty;
        public string Setor { get; private set; } = string.Empty;
        public string Status { get; private set; } = "Ativo"; // Ativo, EmManutencao, Inativo
        public DateTime DataAquisicao { get; private set; }
        public string Criticidade { get; private set; } = "Media"; // Alta, Media, Baixa

        // MAN-IND (Inducao e Configuracao de Equipamentos) — campos tecnico-operacionais aditivos (EF 11.1).
        // Nullable e opcionais para nao quebrar o construtor legado; preenchidos via EnriquecerConfiguracao.
        public string? Descricao { get; private set; }
        public Guid? TipoEquipamentoId { get; private set; }
        public Guid? MarcaId { get; private set; }
        public string? NumeroSerie { get; private set; }
        public string? FuncaoOperacional { get; private set; }
        public Guid? EstadoConservacaoId { get; private set; }
        public Guid? ResponsavelId { get; private set; }
        public Guid? LocalId { get; private set; }
        public int Versao { get; private set; } = 1;

        protected Equipamento() { } // EF Core

        public Equipamento(
            string nome,
            string codigo,
            string setor,
            DateTime dataAquisicao,
            string criticidade,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Equipamento>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O Nome do equipamento é obrigatório.")
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O Código do equipamento é obrigatório.")
                .IsNotNullOrEmpty(setor, nameof(Setor), "O Setor é obrigatório.")
                .IsTrue(criticidade == "Alta" || criticidade == "Media" || criticidade == "Baixa", nameof(Criticidade), "A Criticidade deve ser 'Alta', 'Media' ou 'Baixa'.")
            );

            Nome = nome;
            Codigo = codigo;
            Setor = setor;
            DataAquisicao = dataAquisicao.Date;
            Criticidade = criticidade;
            Status = "Ativo";
        }

        // MAN-IND: enriquece o cadastro tecnico-operacional do equipamento (EF 11.1).
        public void EnriquecerConfiguracao(
            string? descricao,
            Guid? tipoEquipamentoId,
            Guid? marcaId,
            string? numeroSerie,
            string? funcaoOperacional,
            Guid? estadoConservacaoId,
            Guid? responsavelId,
            Guid? localId,
            string usuario)
        {
            Descricao = descricao;
            TipoEquipamentoId = tipoEquipamentoId;
            MarcaId = marcaId;
            NumeroSerie = numeroSerie;
            FuncaoOperacional = funcaoOperacional;
            EstadoConservacaoId = estadoConservacaoId;
            ResponsavelId = responsavelId;
            LocalId = localId;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void AlterarStatus(string novoStatus, string usuario)
        {
            if (novoStatus != "Ativo" && novoStatus != "EmManutencao" && novoStatus != "Inativo")
            {
                AddNotification(nameof(Status), "Status inválido.");
                return;
            }
            Status = novoStatus;
            MarcarAlterado(usuario);
        }
    }
}
