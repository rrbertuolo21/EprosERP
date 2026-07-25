using System;
using System.Security.Cryptography;
using Epros.Shared.Application.Contracts;

namespace Epros.Infrastructure.Services
{
    /// <summary>
    /// Implementação de <see cref="IPasswordHasher"/> baseada em PBKDF2 (Rfc2898DeriveBytes)
    /// com HMAC-SHA256, salt aleatório e comparação em tempo constante.
    ///
    /// Formato armazenado (autocontido, sem dependências externas):
    ///   pbkdf2.sha256.&lt;iteracoes&gt;.&lt;saltBase64&gt;.&lt;hashBase64&gt;
    ///
    /// Não utiliza pacotes de terceiros — apenas System.Security.Cryptography da BCL.
    /// </summary>
    public sealed class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const string Prefixo = "pbkdf2";
        private const string Algoritmo = "sha256";
        private const int Iteracoes = 100_000;
        private const int TamanhoSaltBytes = 16;
        private const int TamanhoSubchaveBytes = 32;

        private static readonly HashAlgorithmName HashName = HashAlgorithmName.SHA256;

        public string Hash(string senha)
        {
            if (senha is null)
                throw new ArgumentNullException(nameof(senha));

            var salt = RandomNumberGenerator.GetBytes(TamanhoSaltBytes);

            var subchave = Rfc2898DeriveBytes.Pbkdf2(
                password: senha,
                salt: salt,
                iterations: Iteracoes,
                hashAlgorithm: HashName,
                outputLength: TamanhoSubchaveBytes);

            return string.Join('.',
                Prefixo,
                Algoritmo,
                Iteracoes.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Convert.ToBase64String(salt),
                Convert.ToBase64String(subchave));
        }

        public bool Verify(string senha, string hashArmazenado)
        {
            if (senha is null || string.IsNullOrWhiteSpace(hashArmazenado))
                return false;

            // Formato esperado: pbkdf2.sha256.<iteracoes>.<saltBase64>.<hashBase64>
            var partes = hashArmazenado.Split('.');
            if (partes.Length != 5)
                return false;

            if (!string.Equals(partes[0], Prefixo, StringComparison.Ordinal))
                return false;

            if (!string.Equals(partes[1], Algoritmo, StringComparison.Ordinal))
                return false;

            if (!int.TryParse(partes[2], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var iteracoes)
                || iteracoes <= 0)
            {
                return false;
            }

            byte[] salt;
            byte[] hashEsperado;
            try
            {
                salt = Convert.FromBase64String(partes[3]);
                hashEsperado = Convert.FromBase64String(partes[4]);
            }
            catch (FormatException)
            {
                return false;
            }

            if (salt.Length == 0 || hashEsperado.Length == 0)
                return false;

            var subchaveInformada = Rfc2898DeriveBytes.Pbkdf2(
                password: senha,
                salt: salt,
                iterations: iteracoes,
                hashAlgorithm: HashName,
                outputLength: hashEsperado.Length);

            return CryptographicOperations.FixedTimeEquals(subchaveInformada, hashEsperado);
        }
    }
}
