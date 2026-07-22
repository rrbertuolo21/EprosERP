using System;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Upload
{
    /// <summary>
    /// upl_execucao_importacao — execução de importação tabular (CSV/XLSX) ou XML, com contadores de
    /// linhas e resultado. Reutilizável pela migração dos clientes. [Origem: EF UPLOAD 12.8]
    /// </summary>
    public class UplExecucaoImportacao : EntidadeSaaSBase
    {
        public Guid UsuarioId { get; private set; }
        public string ImportRef { get; private set; } = string.Empty;
        public string TipoImportacao { get; private set; } = string.Empty; // leads, clients, projects, items, xml...
        public Guid? ArquivoId { get; private set; }
        public string? ArquivoTemporarioChave { get; private set; }
        public string? ArquivoTemporarioNome { get; private set; }
        public EUplStatusImportacao Status { get; private set; }
        public int? TotalLinhas { get; private set; }
        public int? LinhasSucesso { get; private set; }
        public int? LinhasIgnoradas { get; private set; }
        public int? QuantidadeErros { get; private set; }
        public string? ReferenciaErro { get; private set; }
        public EUplResultadoImportacao? Resultado { get; private set; }
        public DateTime? FinalizadoEm { get; private set; }

        protected UplExecucaoImportacao() { }

        public UplExecucaoImportacao(
            Guid usuarioId,
            string importRef,
            string tipoImportacao,
            Guid? arquivoId,
            string? arquivoTemporarioChave,
            string? arquivoTemporarioNome,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            UsuarioId = usuarioId;
            ImportRef = importRef;
            TipoImportacao = tipoImportacao;
            ArquivoId = arquivoId;
            ArquivoTemporarioChave = arquivoTemporarioChave;
            ArquivoTemporarioNome = arquivoTemporarioNome;
            Status = EUplStatusImportacao.Criada;
            ReferenciaErro = importRef;
            Validar();
        }

        public void IniciarProcessamento(string alteradoPor)
        {
            Status = EUplStatusImportacao.Processando;
            MarcarAlterado(alteradoPor);
        }

        public void Finalizar(int totalLinhas, int linhasSucesso, int linhasIgnoradas, int quantidadeErros, string alteradoPor)
        {
            TotalLinhas = totalLinhas;
            LinhasSucesso = linhasSucesso;
            LinhasIgnoradas = linhasIgnoradas;
            QuantidadeErros = quantidadeErros;
            FinalizadoEm = DateTime.UtcNow;

            // Resultado de tela: passed, partial, failed ou nothing (cap. 7.6.17)
            if (totalLinhas == 0)
            {
                Resultado = EUplResultadoImportacao.Nothing;
                Status = EUplStatusImportacao.Finalizada;
            }
            else if (quantidadeErros == 0 && linhasIgnoradas == 0)
            {
                Resultado = EUplResultadoImportacao.Passed;
                Status = EUplStatusImportacao.Finalizada;
            }
            else if (linhasSucesso == 0)
            {
                Resultado = EUplResultadoImportacao.Failed;
                Status = EUplStatusImportacao.Erro;
            }
            else
            {
                Resultado = EUplResultadoImportacao.Partial;
                Status = EUplStatusImportacao.Parcial;
            }

            MarcarAlterado(alteradoPor);
        }

        public void RegistrarErroGeral(string referenciaErro, string alteradoPor)
        {
            Status = EUplStatusImportacao.Erro;
            ReferenciaErro = referenciaErro;
            Resultado = EUplResultadoImportacao.Failed;
            FinalizadoEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Desfazer(string alteradoPor)
        {
            Status = EUplStatusImportacao.Desfeita;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<UplExecucaoImportacao>()
                .Requires()
                .AreNotEquals(UsuarioId, Guid.Empty, nameof(UsuarioId), "O usuário importador é obrigatório [Origem: UplExecucaoImportacao]")
                .IsNotNullOrEmpty(ImportRef, nameof(ImportRef), "O identificador único da transação (import_ref) é obrigatório [Origem: UplExecucaoImportacao]")
                .IsNotNullOrEmpty(TipoImportacao, nameof(TipoImportacao), "O tipo de importação é obrigatório [Origem: UplExecucaoImportacao]"));
        }
    }

    /// <summary>
    /// upl_importacao_linha — resultado por linha processada. [Origem: EF UPLOAD 12.9]
    /// </summary>
    public class UplImportacaoLinha : EntidadeSaaSBase
    {
        public Guid ExecucaoImportacaoId { get; private set; }
        public int NumeroLinha { get; private set; }
        public EUplStatusLinha Status { get; private set; }
        public string? EntidadeDestino { get; private set; }
        public string? EntidadeDestinoId { get; private set; }
        public string? PayloadLinha { get; private set; }

        protected UplImportacaoLinha() { }

        public UplImportacaoLinha(
            Guid execucaoImportacaoId,
            int numeroLinha,
            EUplStatusLinha status,
            string? entidadeDestino,
            string? entidadeDestinoId,
            string? payloadLinha,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ExecucaoImportacaoId = execucaoImportacaoId;
            NumeroLinha = numeroLinha;
            Status = status;
            EntidadeDestino = entidadeDestino;
            EntidadeDestinoId = entidadeDestinoId;
            PayloadLinha = payloadLinha;

            AddNotifications(new Contract<UplImportacaoLinha>()
                .Requires()
                .AreNotEquals(execucaoImportacaoId, Guid.Empty, nameof(ExecucaoImportacaoId), "A execução da linha é obrigatória [Origem: UplImportacaoLinha]")
                .IsGreaterThan(numeroLinha, 1, nameof(NumeroLinha), "A linha de dados deve iniciar em 2 (cabeçalho na linha 1) [Origem: UplImportacaoLinha]"));
        }
    }

    /// <summary>
    /// upl_importacao_erro — log de erro por execução, linha e atributo, consultável pela referência. [Origem: EF UPLOAD 12.10]
    /// </summary>
    public class UplImportacaoErro : EntidadeSaaSBase
    {
        public Guid ExecucaoImportacaoId { get; private set; }
        public string ReferenciaErro { get; private set; } = string.Empty;
        public int? NumeroLinha { get; private set; }
        public string? Atributo { get; private set; }
        public string Mensagem { get; private set; } = string.Empty;
        public string? FormatoExibicao { get; private set; }

        protected UplImportacaoErro() { }

        public UplImportacaoErro(
            Guid execucaoImportacaoId,
            string referenciaErro,
            int? numeroLinha,
            string? atributo,
            string mensagem,
            string? formatoExibicao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ExecucaoImportacaoId = execucaoImportacaoId;
            ReferenciaErro = referenciaErro;
            NumeroLinha = numeroLinha;
            Atributo = atributo;
            Mensagem = mensagem;
            FormatoExibicao = formatoExibicao;

            AddNotifications(new Contract<UplImportacaoErro>()
                .Requires()
                .AreNotEquals(execucaoImportacaoId, Guid.Empty, nameof(ExecucaoImportacaoId), "A execução do erro é obrigatória [Origem: UplImportacaoErro]")
                .IsNotNullOrEmpty(referenciaErro, nameof(ReferenciaErro), "A referência do erro é obrigatória [Origem: UplImportacaoErro]")
                .IsNotNullOrEmpty(mensagem, nameof(Mensagem), "A mensagem funcional do erro é obrigatória [Origem: UplImportacaoErro]"));
        }
    }

    /// <summary>
    /// upl_mapeamento_importacao — mapeamentos coluna→campo salvos e reaproveitáveis. [Origem: EF UPLOAD 12.11]
    /// </summary>
    public class UplMapeamentoImportacao : EntidadeSaaSBase
    {
        public Guid UsuarioId { get; private set; }
        public string TipoImportacao { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public string MapaColunas { get; private set; } = string.Empty; // JSON coluna -> campo
        public bool Ativo { get; private set; }

        protected UplMapeamentoImportacao() { }

        public UplMapeamentoImportacao(
            Guid usuarioId,
            string tipoImportacao,
            string nome,
            string mapaColunas,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            UsuarioId = usuarioId;
            TipoImportacao = tipoImportacao;
            Nome = nome;
            MapaColunas = mapaColunas;
            Ativo = true;

            AddNotifications(new Contract<UplMapeamentoImportacao>()
                .Requires()
                .AreNotEquals(usuarioId, Guid.Empty, nameof(UsuarioId), "O dono do mapeamento é obrigatório [Origem: UplMapeamentoImportacao]")
                .IsNotNullOrEmpty(tipoImportacao, nameof(TipoImportacao), "O tipo de importação do mapeamento é obrigatório [Origem: UplMapeamentoImportacao]")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do mapeamento é obrigatório [Origem: UplMapeamentoImportacao]")
                .IsNotNullOrEmpty(mapaColunas, nameof(MapaColunas), "O mapa de colunas é obrigatório [Origem: UplMapeamentoImportacao]"));
        }

        public void Alterar(string nome, string mapaColunas, bool ativo, string alteradoPor)
        {
            Nome = nome;
            MapaColunas = mapaColunas;
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
        }
    }
}
