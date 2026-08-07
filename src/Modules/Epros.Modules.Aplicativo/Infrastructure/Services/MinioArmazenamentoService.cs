using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Epros.Modules.Aplicativo.Application.Contracts;

namespace Epros.Modules.Aplicativo.Infrastructure.Services
{
    /// <summary>Opções do provider MinIO/S3 (bind de Storage:Minio).</summary>
    public sealed class MinioStorageOptions
    {
        public string Endpoint { get; set; } = string.Empty; // host:porta (ex.: minio:9000)
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Bucket { get; set; } = "epros-ged";
        public bool UseSSL { get; set; }
        public bool Configurado => !string.IsNullOrWhiteSpace(Endpoint)
            && !string.IsNullOrWhiteSpace(AccessKey) && !string.IsNullOrWhiteSpace(SecretKey);
    }

    /// <summary>
    /// PLT · GED (T10) — provider REAL de armazenamento binário sobre MinIO (S3-compatível). Substitui o
    /// stub "não configurado": aqui o byte físico é de fato persistido e recuperável. A referência opaca
    /// é <c>minio://&lt;bucket&gt;/&lt;chaveLogica&gt;</c>; a chave lógica (hash do conteúdo, calculada pela
    /// camada GED) vira a object key — mesma chave = mesma key = dedupe/idempotência natural. O bucket é
    /// garantido preguiçosamente (com retry), porque a API pode subir antes de o MinIO aceitar conexão.
    /// </summary>
    public sealed class MinioArmazenamentoService : IArmazenamentoDocumentoService
    {
        private readonly IMinioClient _client;
        private readonly MinioStorageOptions _opts;
        private readonly ILogger<MinioArmazenamentoService> _logger;
        private readonly SemaphoreSlim _bucketGate = new(1, 1);
        private volatile bool _bucketPronto;

        public MinioArmazenamentoService(MinioStorageOptions opts, ILogger<MinioArmazenamentoService> logger)
        {
            _opts = opts;
            _logger = logger;
            _client = new MinioClient()
                .WithEndpoint(opts.Endpoint)
                .WithCredentials(opts.AccessKey, opts.SecretKey)
                .WithSSL(opts.UseSSL)
                .Build();
        }

        public bool ProviderConfigurado => true;

        private async Task GarantirBucketAsync(CancellationToken ct)
        {
            if (_bucketPronto) return;
            await _bucketGate.WaitAsync(ct);
            try
            {
                if (_bucketPronto) return;
                // Retry: o container da API pode iniciar antes de o MinIO aceitar conexão.
                Exception? ultimo = null;
                for (var tentativa = 1; tentativa <= 10; tentativa++)
                {
                    try
                    {
                        var existe = await _client.BucketExistsAsync(
                            new BucketExistsArgs().WithBucket(_opts.Bucket), ct);
                        if (!existe)
                        {
                            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_opts.Bucket), ct);
                            _logger.LogInformation("GED/MinIO: bucket '{Bucket}' criado.", _opts.Bucket);
                        }
                        _bucketPronto = true;
                        return;
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        ultimo = ex;
                        await Task.Delay(TimeSpan.FromSeconds(tentativa), ct); // backoff linear até ~10s
                    }
                }
                throw new InvalidOperationException(
                    $"GED/MinIO: não foi possível garantir o bucket '{_opts.Bucket}' após 10 tentativas.", ultimo);
            }
            finally
            {
                _bucketGate.Release();
            }
        }

        public async Task<string> ArmazenarAsync(byte[] conteudo, string chaveLogica, CancellationToken cancellationToken = default)
        {
            await GarantirBucketAsync(cancellationToken);
            using var stream = new MemoryStream(conteudo, writable: false);
            await _client.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_opts.Bucket)
                .WithObject(chaveLogica)
                .WithStreamData(stream)
                .WithObjectSize(conteudo.LongLength)
                .WithContentType("application/octet-stream"), cancellationToken);
            return $"minio://{_opts.Bucket}/{chaveLogica}";
        }

        public async Task<byte[]?> ObterAsync(string storageRef, CancellationToken cancellationToken = default)
        {
            var (bucket, key) = Parse(storageRef);
            if (key is null) return null;
            await GarantirBucketAsync(cancellationToken);
            try
            {
                using var ms = new MemoryStream();
                await _client.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(key)
                    .WithCallbackStream(async (s, ct) => await s.CopyToAsync(ms, ct)), cancellationToken);
                return ms.ToArray();
            }
            catch (ObjectNotFoundException) { return null; }
            catch (BucketNotFoundException) { return null; }
        }

        public async Task RemoverAsync(string storageRef, CancellationToken cancellationToken = default)
        {
            var (bucket, key) = Parse(storageRef);
            if (key is null) return;
            await GarantirBucketAsync(cancellationToken);
            try
            {
                await _client.RemoveObjectAsync(new RemoveObjectArgs()
                    .WithBucket(bucket).WithObject(key), cancellationToken);
            }
            catch (ObjectNotFoundException) { /* já removido — idempotente */ }
            catch (BucketNotFoundException) { /* nada a remover */ }
        }

        // "minio://<bucket>/<key>" -> (bucket, key). Refs antigas ("local://...") não têm byte: key=null.
        private (string bucket, string? key) Parse(string storageRef)
        {
            const string prefixo = "minio://";
            if (string.IsNullOrWhiteSpace(storageRef) || !storageRef.StartsWith(prefixo, StringComparison.Ordinal))
                return (_opts.Bucket, null);
            var resto = storageRef.Substring(prefixo.Length);
            var barra = resto.IndexOf('/');
            if (barra <= 0 || barra >= resto.Length - 1) return (_opts.Bucket, null);
            return (resto.Substring(0, barra), resto.Substring(barra + 1));
        }
    }
}
