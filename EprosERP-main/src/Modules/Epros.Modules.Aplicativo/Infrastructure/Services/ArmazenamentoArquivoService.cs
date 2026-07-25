using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Epros.Modules.Aplicativo.Infrastructure.Services
{
    /// <summary>
    /// Contrato de armazenamento de arquivos com deduplicação por hash (PLT-UPL, EF UPLOAD 7.3).
    /// </summary>
    public interface IArmazenamentoArquivoService
    {
        string CalcularHash(byte[] conteudo);
        string SanitizarNome(string nomeOriginal);
        string GerarNomeArmazenado(string extensao);
        Task<string> SalvarAsync(byte[] conteudo, string nomeArmazenado, string? pastaDestino, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Implementação local do armazenamento. Usada diretamente pelos handlers (instanciada sem DI, no
    /// Modo Porte). O hash SHA-256 habilita reaproveitamento de arquivo ativo com mesmo hash e tamanho.
    /// </summary>
    public class ArmazenamentoLocalArquivoService : IArmazenamentoArquivoService
    {
        private readonly string _baseDir;

        public ArmazenamentoLocalArquivoService(string? baseDir = null)
        {
            _baseDir = baseDir ?? Path.Combine(Directory.GetCurrentDirectory(), "storage", "uploads");
        }

        public string CalcularHash(byte[] conteudo)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(conteudo);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        /// <summary>
        /// Sanitiza nome de arquivo: remove ponto inicial (evita arquivo oculto) e caracteres inválidos.
        /// Se vazio, atribui nome funcional baseado em data/hora (EF UPLOAD 7.1.11-12).
        /// </summary>
        public string SanitizarNome(string nomeOriginal)
        {
            if (string.IsNullOrWhiteSpace(nomeOriginal))
            {
                return $"arquivo_{DateTime.UtcNow:yyyyMMddHHmmssfff}";
            }

            var nome = nomeOriginal.Trim();
            while (nome.StartsWith("."))
            {
                nome = nome.Substring(1);
            }

            foreach (var c in Path.GetInvalidFileNameChars())
            {
                nome = nome.Replace(c, '_');
            }

            return string.IsNullOrWhiteSpace(nome) ? $"arquivo_{DateTime.UtcNow:yyyyMMddHHmmssfff}" : nome;
        }

        /// <summary>Gera identificador técnico opaco para armazenamento (EF UPLOAD 7.3.5).</summary>
        public string GerarNomeArmazenado(string extensao)
        {
            var ext = string.IsNullOrWhiteSpace(extensao) ? string.Empty : (extensao.StartsWith(".") ? extensao : "." + extensao);
            return $"{Guid.NewGuid():N}{ext}";
        }

        public async Task<string> SalvarAsync(byte[] conteudo, string nomeArmazenado, string? pastaDestino, CancellationToken cancellationToken)
        {
            var dir = string.IsNullOrWhiteSpace(pastaDestino) ? _baseDir : Path.Combine(_baseDir, pastaDestino);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var caminho = Path.Combine(dir, nomeArmazenado);
            await File.WriteAllBytesAsync(caminho, conteudo, cancellationToken);
            return caminho;
        }
    }
}
