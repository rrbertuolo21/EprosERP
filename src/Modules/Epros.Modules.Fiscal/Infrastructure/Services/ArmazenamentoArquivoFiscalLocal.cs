using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Fiscal.Application.Services;

namespace Epros.Modules.Fiscal.Infrastructure.Services
{
    /// <summary>
    /// Implementação de <see cref="IArmazenamentoArquivoFiscal"/> em sistema de arquivos local.
    ///
    /// Raiz padrão: <c>{ContentRoot}/dfe-arquivos</c> (ou variável de ambiente <c>EPROS_DFE_STORAGE</c>).
    /// Os artefatos ficam em <c>{raiz}/{chaveLogica}/{nomeArquivo}</c>. O caminho absoluto é o que
    /// se persiste no <c>DocumentoFiscal</c>.
    ///
    /// NOTA (MinIO): não há cliente MinIO/S3 neste repositório hoje. Quando houver, criar
    /// <c>ArmazenamentoArquivoFiscalMinio : IArmazenamentoArquivoFiscal</c> e trocar o registro no DI —
    /// nada mais muda (o restante do fluxo só conhece a interface).
    /// </summary>
    public class ArmazenamentoArquivoFiscalLocal : IArmazenamentoArquivoFiscal
    {
        private readonly string _raiz;

        public ArmazenamentoArquivoFiscalLocal(string? raiz = null)
        {
            _raiz = raiz
                ?? Environment.GetEnvironmentVariable("EPROS_DFE_STORAGE")
                ?? Path.Combine(AppContext.BaseDirectory, "dfe-arquivos");
        }

        public async Task<string> SalvarAsync(string chaveLogica, string nomeArquivo, byte[] conteudo, string contentType, CancellationToken ct = default)
        {
            var pasta = Path.Combine(_raiz, Sanitizar(chaveLogica));
            Directory.CreateDirectory(pasta);

            var caminho = Path.Combine(pasta, Sanitizar(nomeArquivo));
            await File.WriteAllBytesAsync(caminho, conteudo, ct);
            return caminho;
        }

        public async Task<byte[]?> LerAsync(string caminho, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(caminho) || !File.Exists(caminho))
                return null;

            return await File.ReadAllBytesAsync(caminho, ct);
        }

        private static string Sanitizar(string nome)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                nome = nome.Replace(c, '_');
            return nome;
        }
    }
}
