using System;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Upload
{
    /// <summary>
    /// upl_atualizacao_versao — versão aplicada por empresa ou por ambiente central. [Origem: EF UPLOAD 12.16]
    /// </summary>
    public class UplAtualizacaoVersao : EntidadeSaaSBase
    {
        public string VersaoAtual { get; private set; } = string.Empty;
        public string VersaoAlvo { get; private set; } = string.Empty;
        public EUplStatusAtualizacao Status { get; private set; }
        public string? Log { get; private set; }
        public DateTime? IniciadoEm { get; private set; }
        public DateTime? FinalizadoEm { get; private set; }

        protected UplAtualizacaoVersao() { }

        public UplAtualizacaoVersao(string versaoAtual, string versaoAlvo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            VersaoAtual = versaoAtual;
            VersaoAlvo = versaoAlvo;
            Status = EUplStatusAtualizacao.Novo;

            AddNotifications(new Contract<UplAtualizacaoVersao>()
                .Requires()
                .IsNotNullOrEmpty(versaoAtual, nameof(VersaoAtual), "A versão atual é obrigatória [Origem: UplAtualizacaoVersao]")
                .IsNotNullOrEmpty(versaoAlvo, nameof(VersaoAlvo), "A versão alvo é obrigatória [Origem: UplAtualizacaoVersao]"));
        }

        public void Iniciar(string alteradoPor)
        {
            Status = EUplStatusAtualizacao.Processando;
            IniciadoEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Concluir(string versaoFinal, string? log, string alteradoPor)
        {
            VersaoAtual = versaoFinal;
            Status = EUplStatusAtualizacao.Concluido;
            Log = log;
            FinalizadoEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Falhar(string? log, string alteradoPor)
        {
            Status = EUplStatusAtualizacao.Falho;
            Log = log;
            FinalizadoEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>
    /// upl_atualizacao_bloco — blocos/arquivos idempotentes aplicados; um bloco já registrado não é
    /// reaplicado. [Origem: EF UPLOAD 12.17]
    /// </summary>
    public class UplAtualizacaoBloco : EntidadeSaaSBase
    {
        public Guid AtualizacaoVersaoId { get; private set; }
        public string NomeArquivo { get; private set; } = string.Empty;
        public string? IdentificadorBloco { get; private set; }
        public EUplStatusBloco Status { get; private set; }
        public string? Log { get; private set; }
        public DateTime? AplicadoEm { get; private set; }

        protected UplAtualizacaoBloco() { }

        public UplAtualizacaoBloco(Guid atualizacaoVersaoId, string nomeArquivo, string? identificadorBloco, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AtualizacaoVersaoId = atualizacaoVersaoId;
            NomeArquivo = nomeArquivo;
            IdentificadorBloco = identificadorBloco;
            Status = EUplStatusBloco.Pendente;

            AddNotifications(new Contract<UplAtualizacaoBloco>()
                .Requires()
                .AreNotEquals(atualizacaoVersaoId, Guid.Empty, nameof(AtualizacaoVersaoId), "A atualização do bloco é obrigatória [Origem: UplAtualizacaoBloco]")
                .IsNotNullOrEmpty(nomeArquivo, nameof(NomeArquivo), "O nome do arquivo do bloco é obrigatório (idempotência) [Origem: UplAtualizacaoBloco]"));
        }

        public void Aplicar(string? log, string alteradoPor)
        {
            Status = EUplStatusBloco.Aplicado;
            Log = log;
            AplicadoEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Ignorar(string alteradoPor)
        {
            Status = EUplStatusBloco.Ignorado;
            MarcarAlterado(alteradoPor);
        }

        public void Falhar(string? log, string alteradoPor)
        {
            Status = EUplStatusBloco.Falho;
            Log = log;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>
    /// upl_atualizacao_job — jobs técnicos de atualização. Job sem função ou com função inexistente
    /// deve falhar. [Origem: EF UPLOAD 12.18]
    /// </summary>
    public class UplAtualizacaoJob : EntidadeSaaSBase
    {
        public string Tipo { get; private set; } = string.Empty; // modal, cronjob, url
        public string Nome { get; private set; } = string.Empty;
        public string? FuncaoNome { get; private set; }
        public EUplStatusJobAtualizacao Status { get; private set; }
        public string? PayloadJson { get; private set; }
        public string? Log { get; private set; }
        public DateTime? FinalizadoEm { get; private set; }

        protected UplAtualizacaoJob() { }

        public UplAtualizacaoJob(string tipo, string nome, string? funcaoNome, string? payloadJson, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Tipo = tipo;
            Nome = nome;
            FuncaoNome = funcaoNome;
            PayloadJson = payloadJson;
            Status = EUplStatusJobAtualizacao.New;

            AddNotifications(new Contract<UplAtualizacaoJob>()
                .Requires()
                .IsNotNullOrEmpty(tipo, nameof(Tipo), "O tipo de disparo do job é obrigatório [Origem: UplAtualizacaoJob]")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do job é obrigatório [Origem: UplAtualizacaoJob]"));
        }

        public void Processar(string alteradoPor)
        {
            Status = EUplStatusJobAtualizacao.Processing;
            MarcarAlterado(alteradoPor);
        }

        public void Concluir(string? log, string alteradoPor)
        {
            Status = EUplStatusJobAtualizacao.Completed;
            Log = log;
            FinalizadoEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Falhar(string? log, string alteradoPor)
        {
            Status = EUplStatusJobAtualizacao.Failed;
            Log = log;
            FinalizadoEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }
    }
}
