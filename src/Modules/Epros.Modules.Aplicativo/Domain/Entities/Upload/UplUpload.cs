using System;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Upload
{
    /// <summary>
    /// upl_execucao_upload — controle do recebimento de um arquivo por origem direta, remota, API ou offline.
    /// [Origem: EF UPLOAD 12.4]
    /// </summary>
    public class UplExecucaoUpload : EntidadeSaaSBase
    {
        public Guid UsuarioId { get; private set; }
        public Guid? UsuarioUploadId { get; private set; }
        public EUplOrigemUpload Origem { get; private set; }
        public string NomeOriginal { get; private set; } = string.Empty;
        public string? Extensao { get; private set; }
        public long? TamanhoBytes { get; private set; }
        public string? MimeType { get; private set; }
        public EUplStatusUpload Status { get; private set; }
        public string? MensagemErro { get; private set; }
        public string? PastaDestinoId { get; private set; }
        public Guid? ArquivoId { get; private set; }

        protected UplExecucaoUpload() { }

        public UplExecucaoUpload(
            Guid usuarioId,
            Guid? usuarioUploadId,
            EUplOrigemUpload origem,
            string nomeOriginal,
            string? extensao,
            long? tamanhoBytes,
            string? mimeType,
            string? pastaDestinoId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            UsuarioId = usuarioId;
            UsuarioUploadId = usuarioUploadId;
            Origem = origem;
            NomeOriginal = nomeOriginal;
            Extensao = extensao;
            TamanhoBytes = tamanhoBytes;
            MimeType = mimeType;
            PastaDestinoId = pastaDestinoId;
            Status = EUplStatusUpload.Recebido;
            Validar();
        }

        public void AlterarStatus(EUplStatusUpload status, string alteradoPor)
        {
            Status = status;
            MarcarAlterado(alteradoPor);
        }

        public void Consolidar(Guid arquivoId, long tamanhoBytes, string alteradoPor)
        {
            ArquivoId = arquivoId;
            TamanhoBytes = tamanhoBytes;
            Status = EUplStatusUpload.Concluido;
            MarcarAlterado(alteradoPor);
        }

        public void RegistrarErro(string mensagemErro, string alteradoPor)
        {
            Status = EUplStatusUpload.Erro;
            MensagemErro = mensagemErro;
            MarcarAlterado(alteradoPor);
        }

        public void Cancelar(string alteradoPor)
        {
            Status = EUplStatusUpload.Cancelado;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<UplExecucaoUpload>()
                .Requires()
                .AreNotEquals(UsuarioId, Guid.Empty, nameof(UsuarioId), "O usuário dono da execução é obrigatório [Origem: UplExecucaoUpload]")
                .IsNotNullOrEmpty(NomeOriginal, nameof(NomeOriginal), "O nome original do arquivo é obrigatório [Origem: UplExecucaoUpload]"));
        }
    }

    /// <summary>
    /// upl_upload_parte — partes recebidas de um upload fracionado (faixa de bytes). Partes antigas
    /// (mais de 3 dias) devem ser removidas por rotina. [Origem: EF UPLOAD 12.5, cap. 7.2]
    /// </summary>
    public class UplUploadParte : EntidadeSaaSBase
    {
        public Guid ExecucaoUploadId { get; private set; }
        public long ByteInicio { get; private set; }
        public long ByteFim { get; private set; }
        public long TotalBytes { get; private set; }
        public string CaminhoTemporario { get; private set; } = string.Empty;
        public bool Completa { get; private set; }

        protected UplUploadParte() { }

        public UplUploadParte(
            Guid execucaoUploadId,
            long byteInicio,
            long byteFim,
            long totalBytes,
            string caminhoTemporario,
            bool completa,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ExecucaoUploadId = execucaoUploadId;
            ByteInicio = byteInicio;
            ByteFim = byteFim;
            TotalBytes = totalBytes;
            CaminhoTemporario = caminhoTemporario;
            Completa = completa;

            AddNotifications(new Contract<UplUploadParte>()
                .Requires()
                .AreNotEquals(execucaoUploadId, Guid.Empty, nameof(ExecucaoUploadId), "O upload de origem é obrigatório [Origem: UplUploadParte]")
                .IsGreaterThan(totalBytes, 0, nameof(TotalBytes), "O total de bytes esperado é obrigatório [Origem: UplUploadParte]")
                .IsNotNullOrEmpty(caminhoTemporario, nameof(CaminhoTemporario), "O caminho temporário da parte é obrigatório [Origem: UplUploadParte]"));
        }

        public void MarcarCompleta(string alteradoPor)
        {
            Completa = true;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>
    /// upl_arquivo — registro funcional do arquivo recebido ou migrado, com hash para deduplicação e
    /// nome armazenado opaco. [Origem: EF UPLOAD 12.6, cap. 7.3]
    /// </summary>
    public class UplArquivo : EntidadeSaaSBase
    {
        public Guid OwnerUsuarioId { get; private set; }
        public Guid? UploadedUsuarioId { get; private set; }
        public string NomeOriginal { get; private set; } = string.Empty;
        public string NomeArmazenado { get; private set; } = string.Empty;
        public string? Extensao { get; private set; }
        public long TamanhoBytes { get; private set; }
        public string? HashArquivo { get; private set; }
        public string? PastaId { get; private set; }
        public string? ServidorStorageId { get; private set; }
        public EUplOrigemUpload OrigemUpload { get; private set; }
        public EUplStatusArquivo Status { get; private set; }

        protected UplArquivo() { }

        public UplArquivo(
            Guid ownerUsuarioId,
            Guid? uploadedUsuarioId,
            string nomeOriginal,
            string nomeArmazenado,
            string? extensao,
            long tamanhoBytes,
            string? hashArquivo,
            string? pastaId,
            string? servidorStorageId,
            EUplOrigemUpload origemUpload,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            OwnerUsuarioId = ownerUsuarioId;
            UploadedUsuarioId = uploadedUsuarioId;
            NomeOriginal = nomeOriginal;
            NomeArmazenado = nomeArmazenado;
            Extensao = extensao;
            TamanhoBytes = tamanhoBytes;
            HashArquivo = hashArquivo;
            PastaId = pastaId;
            ServidorStorageId = servidorStorageId;
            OrigemUpload = origemUpload;
            Status = EUplStatusArquivo.Ativo;

            AddNotifications(new Contract<UplArquivo>()
                .Requires()
                .AreNotEquals(ownerUsuarioId, Guid.Empty, nameof(OwnerUsuarioId), "O dono do arquivo é obrigatório [Origem: UplArquivo]")
                .IsNotNullOrEmpty(nomeOriginal, nameof(NomeOriginal), "O nome original é obrigatório [Origem: UplArquivo]")
                .IsNotNullOrEmpty(nomeArmazenado, nameof(NomeArmazenado), "O nome armazenado (opaco) é obrigatório [Origem: UplArquivo]"));
        }

        public void MarcarRemovido(string alteradoPor)
        {
            Status = EUplStatusArquivo.Removido;
            MarcarAlterado(alteradoPor);
        }
    }
}
