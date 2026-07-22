namespace Epros.Modules.Aplicativo.Domain.Enums
{
    /// <summary>Origem do upload (upl_execucao_upload / upl_arquivo).</summary>
    public enum EUplOrigemUpload
    {
        Direct = 0,
        Remote = 1,
        Api = 2,
        Offline = 3
    }

    /// <summary>Estado da execução de upload (upl_execucao_upload).</summary>
    public enum EUplStatusUpload
    {
        Recebido = 0,
        Validando = 1,
        Armazenando = 2,
        Concluido = 3,
        Erro = 4,
        Cancelado = 5
    }

    /// <summary>Situação de um arquivo consolidado (upl_arquivo).</summary>
    public enum EUplStatusArquivo
    {
        Ativo = 0,
        Removido = 1,
        Erro = 2
    }

    /// <summary>Status preservado do material para a fila de download remoto (upl_fila_url_remota).</summary>
    public enum EUplStatusUrlRemota
    {
        Pending = 0,
        Processing = 1,
        Downloading = 2,
        Complete = 3,
        Failed = 4,
        Cancelled = 5
    }

    /// <summary>Estado de uma execução de importação tabular/XML (upl_execucao_importacao).</summary>
    public enum EUplStatusImportacao
    {
        Criada = 0,
        Validando = 1,
        Processando = 2,
        Parcial = 3,
        Finalizada = 4,
        Erro = 5,
        Desfeita = 6
    }

    /// <summary>Resultado de tela da importação (upl_execucao_importacao.resultado).</summary>
    public enum EUplResultadoImportacao
    {
        Nothing = 0,
        Passed = 1,
        Partial = 2,
        Failed = 3
    }

    /// <summary>Resultado por linha processada (upl_importacao_linha).</summary>
    public enum EUplStatusLinha
    {
        Importada = 0,
        Ignorada = 1,
        Erro = 2
    }

    /// <summary>Tipo fiscal do XML importado (upl_importacao_xml).</summary>
    public enum EUplTipoXml
    {
        NaoAplicavel = 0,
        NotaFiscalEntrada = 1,
        NotaFiscalSaida = 2,
        NotaFiscalEntradaPropria = 3,
        NotaFiscalCancelamento = 4
    }

    /// <summary>Status de cada etapa de processamento de XML fiscal (importação/cadastro/PDF).</summary>
    public enum EUplStatusProcessamentoXml
    {
        NaoProcessado = 0,
        Processando = 1,
        Finalizado = 2,
        Erro = 3
    }

    /// <summary>Estado de um lote de XML de saída (upl_arquivo_xml_saida).</summary>
    public enum EUplStatusArquivoXmlSaida
    {
        Verificando = 0,
        Processando = 1,
        Finalizado = 2,
        Erro = 3
    }

    /// <summary>Estado de uma execução de exportação (upl_execucao_exportacao).</summary>
    public enum EUplStatusExportacao
    {
        Solicitada = 0,
        Gerando = 1,
        Disponivel = 2,
        Erro = 3,
        Expirada = 4
    }

    /// <summary>Origem do campo selecionado na exportação (upl_exportacao_campo).</summary>
    public enum EUplOrigemCampo
    {
        StandardField = 0,
        CustomField = 1
    }

    /// <summary>Estado de uma atualização incremental (upl_atualizacao_versao).</summary>
    public enum EUplStatusAtualizacao
    {
        Novo = 0,
        Processando = 1,
        Concluido = 2,
        Falho = 3
    }

    /// <summary>Estado de um bloco de atualização idempotente (upl_atualizacao_bloco).</summary>
    public enum EUplStatusBloco
    {
        Pendente = 0,
        Aplicado = 1,
        Ignorado = 2,
        Falho = 3
    }

    /// <summary>Status preservado do material para o job técnico de atualização (upl_atualizacao_job).</summary>
    public enum EUplStatusJobAtualizacao
    {
        New = 0,
        Processing = 1,
        Failed = 2,
        Completed = 3
    }

    /// <summary>Estado de uma execução de migração offline de arquivos (upl_migracao_offline).</summary>
    public enum EUplStatusMigracaoOffline
    {
        Criada = 0,
        Processando = 1,
        Concluida = 2,
        Erro = 3,
        Cancelada = 4
    }
}
