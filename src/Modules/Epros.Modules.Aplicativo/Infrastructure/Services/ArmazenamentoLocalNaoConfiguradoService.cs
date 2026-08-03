using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Contracts;

namespace Epros.Modules.Aplicativo.Infrastructure.Services
{
    /// <summary>
    /// PLT · GED (T10) — implementação PADRÃO da abstração de storage quando NÃO há provider real
    /// configurado no ambiente (// valida-ambiente). Gera uma referência determinística a partir do
    /// hash/chave para que o fluxo de registro/dedupe/versionamento funcione ponta-a-ponta em
    /// desenvolvimento e testes, SEM persistir o byte físico. Em produção, registra-se um provider
    /// real (MinIO/S3/servidor de storage) que substitui esta implementação.
    /// </summary>
    public sealed class ArmazenamentoLocalNaoConfiguradoService : IArmazenamentoDocumentoService
    {
        public bool ProviderConfigurado => false;

        public Task<string> ArmazenarAsync(byte[] conteudo, string chaveLogica, CancellationToken cancellationToken = default)
        {
            // Referência opaca determinística: "local://<sha256-da-chave>". Não há gravação física.
            using var sha = SHA256.Create();
            var hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(chaveLogica))).ToLowerInvariant();
            return Task.FromResult($"local://{hash}");
        }

        public Task<byte[]?> ObterAsync(string storageRef, CancellationToken cancellationToken = default)
            // Sem provider real, o conteúdo não é recuperável — retorna null (dependência de ambiente).
            => Task.FromResult<byte[]?>(null);

        public Task RemoverAsync(string storageRef, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
