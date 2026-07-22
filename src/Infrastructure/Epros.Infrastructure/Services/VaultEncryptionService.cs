using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Epros.Infrastructure.Services
{
    /// <summary>
    /// Implementação de ISegredoCofreService integrada ao Vault Transit Engine,
    /// com mecanismo de fallback local resiliente com AES-256-GCM.
    /// </summary>
    public class VaultEncryptionService : ISegredoCofreService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<VaultEncryptionService> _logger;
        private readonly string _chaveNome;
        private readonly byte[] _kekLocal;
        private bool _usarCofreLocal;
        private bool _inicializado;
        private readonly SemaphoreSlim _inicializacaoSemaphore = new(1, 1);

        public VaultEncryptionService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<VaultEncryptionService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Define URL base a partir da configuração ou usa padrão local
            var vaultUrl = configuration["Cofre:VaultUrl"] ?? "http://localhost:8200";
            _httpClient.BaseAddress = new Uri(vaultUrl);
            
            // Define o token padrão de desenvolvimento se não houver um configurado
            var token = configuration["Cofre:VaultToken"] ?? "epros-dev-token";
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Vault-Token", token);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _chaveNome = configuration["Cofre:ChaveNome"] ?? "epros-kek";

            var kekLocalString = configuration["Cofre:KekLocal"] ?? "epros-local-default-fallback-kek-key-32bytes";
            // Gera a chave de 32 bytes de forma robusta e determinística via SHA256 da string configurada
            _kekLocal = SHA256.HashData(Encoding.UTF8.GetBytes(kekLocalString));
        }

        /// <summary>
        /// Inicializa programaticamente o Transit Secret Engine no Vault e garante a criação da chave.
        /// Se houver falha de conexão com o Vault, entra silenciosamente em modo de fallback local.
        /// </summary>
        public async Task InicializarCofreAsync()
        {
            if (_inicializado) return;

            await _inicializacaoSemaphore.WaitAsync();
            try
            {
                if (_inicializado) return;

                _logger.LogInformation("Tentando inicializar o Vault Transit Secret Engine...");

                try
                {
                    // Tenta habilitar o transit engine via POST /v1/sys/mounts/transit
                    var responseTransit = await _httpClient.PostAsJsonAsync("v1/sys/mounts/transit", new { type = "transit" });
                    
                    // Se for 200/204 ou se for 400 (porque já está montado/em uso), consideramos sucesso
                    if (responseTransit.IsSuccessStatusCode || responseTransit.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        _logger.LogInformation("Vault Transit Secret Engine está habilitado.");
                        
                        // Tenta verificar se a chave existe fazendo GET
                        var responseGetChave = await _httpClient.GetAsync($"v1/transit/keys/{_chaveNome}");
                        if (responseGetChave.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            _logger.LogInformation("Chave {ChaveNome} não encontrada. Criando chave no Vault...", _chaveNome);
                            
                            // Cria a chave aes256-gcm96
                            var responseCreateChave = await _httpClient.PostAsJsonAsync($"v1/transit/keys/{_chaveNome}", new { type = "aes256-gcm96" });
                            
                            if (responseCreateChave.IsSuccessStatusCode)
                            {
                                _logger.LogInformation("Chave {ChaveNome} criada com sucesso no Vault.", _chaveNome);
                            }
                            else
                            {
                                var content = await responseCreateChave.Content.ReadAsStringAsync();
                                _logger.LogWarning("Não foi possível criar a chave no Vault: {Erro}. Usando fallback local.", content);
                                _usarCofreLocal = true;
                            }
                        }
                        else if (responseGetChave.IsSuccessStatusCode)
                        {
                            _logger.LogInformation("Chave {ChaveNome} já existe e está pronta para uso no Vault.", _chaveNome);
                        }
                        else
                        {
                            _logger.LogWarning("Erro ao verificar chave no Vault. Usando fallback local.");
                            _usarCofreLocal = true;
                        }
                    }
                    else
                    {
                        var content = await responseTransit.Content.ReadAsStringAsync();
                        _logger.LogWarning("Não foi possível habilitar o Transit Engine: {Erro}. Usando fallback local.", content);
                        _usarCofreLocal = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao conectar ou configurar o Vault. Ativando silenciosamente o modo de Criptografia Local (fallback offline).");
                    _usarCofreLocal = true;
                }

                _inicializado = true;
            }
            finally
            {
                _inicializacaoSemaphore.Release();
            }
        }

        public async Task<string> CriptografarAsync(string valor)
        {
            if (string.IsNullOrEmpty(valor)) return valor;

            if (!_inicializado)
            {
                await InicializarCofreAsync();
            }

            if (_usarCofreLocal)
            {
                return CriptografarLocal(valor);
            }

            try
            {
                var base64Plaintext = Convert.ToBase64String(Encoding.UTF8.GetBytes(valor));
                var payload = new { plaintext = base64Plaintext };

                var response = await _httpClient.PostAsJsonAsync($"v1/transit/encrypt/{_chaveNome}", payload);
                if (response.IsSuccessStatusCode)
                {
                    var resultJson = await response.Content.ReadFromJsonAsync<JsonElement>();
                    var ciphertext = resultJson.GetProperty("data").GetProperty("ciphertext").GetString();
                    if (!string.IsNullOrEmpty(ciphertext))
                    {
                        return ciphertext; // O próprio Vault retorna com prefixo vault:v1:
                    }
                }

                _logger.LogWarning("Erro na API do Vault ao criptografar. Usando criptografia local.");
                return CriptografarLocal(valor);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exceção ao criptografar usando Vault. Usando criptografia local.");
                return CriptografarLocal(valor);
            }
        }

        public async Task<string> DescriptografarAsync(string ciphertext)
        {
            if (string.IsNullOrEmpty(ciphertext)) return ciphertext;

            // Se o ciphertext tiver prefixo local
            if (ciphertext.StartsWith("local:v1:"))
            {
                return DescriptografarLocal(ciphertext);
            }

            if (!_inicializado)
            {
                await InicializarCofreAsync();
            }

            // Se estivermos usando fallback local e o ciphertext não for do Vault
            if (_usarCofreLocal && !ciphertext.StartsWith("vault:"))
            {
                return DescriptografarLocal(ciphertext);
            }

            try
            {
                var payload = new { ciphertext = ciphertext };
                var response = await _httpClient.PostAsJsonAsync($"v1/transit/decrypt/{_chaveNome}", payload);
                if (response.IsSuccessStatusCode)
                {
                    var resultJson = await response.Content.ReadFromJsonAsync<JsonElement>();
                    var base64Plaintext = resultJson.GetProperty("data").GetProperty("plaintext").GetString();
                    if (!string.IsNullOrEmpty(base64Plaintext))
                    {
                        return Encoding.UTF8.GetString(Convert.FromBase64String(base64Plaintext));
                    }
                }

                throw new InvalidOperationException($"Não foi possível descriptografar usando o Vault. Status Code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao descriptografar segredo com Vault: {Ciphertext}", ciphertext);
                throw;
            }
        }

        private string CriptografarLocal(string valor)
        {
            var plaintextBytes = Encoding.UTF8.GetBytes(valor);
            
            // Gerar Nonce aleatório de 12 bytes
            var nonce = new byte[12];
            RandomNumberGenerator.Fill(nonce);

            // Reservar Tag de 16 bytes e Ciphertext correspondente
            var tag = new byte[16];
            var ciphertextBytes = new byte[plaintextBytes.Length];

            using var aesGcm = new AesGcm(_kekLocal, tagSizeInBytes: 16);
            aesGcm.Encrypt(nonce, plaintextBytes, ciphertextBytes, tag);

            // Concatenar nonce (12) + tag (16) + ciphertext
            var totalBytes = new byte[nonce.Length + tag.Length + ciphertextBytes.Length];
            Buffer.BlockCopy(nonce, 0, totalBytes, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, totalBytes, nonce.Length, tag.Length);
            Buffer.BlockCopy(ciphertextBytes, 0, totalBytes, nonce.Length + tag.Length, ciphertextBytes.Length);

            var base64 = Convert.ToBase64String(totalBytes);
            return $"local:v1:{base64}";
        }

        private string DescriptografarLocal(string ciphertext)
        {
            var base64Data = ciphertext;
            if (ciphertext.StartsWith("local:v1:"))
            {
                base64Data = ciphertext.Substring("local:v1:".Length);
            }

            var totalBytes = Convert.FromBase64String(base64Data);
            if (totalBytes.Length < 28) // 12 (nonce) + 16 (tag)
            {
                throw new CryptographicException("Ciphertext local inválido (tamanho insuficiente).");
            }

            var nonce = new byte[12];
            var tag = new byte[16];
            var ciphertextBytes = new byte[totalBytes.Length - 28];

            Buffer.BlockCopy(totalBytes, 0, nonce, 0, 12);
            Buffer.BlockCopy(totalBytes, 12, tag, 0, 16);
            Buffer.BlockCopy(totalBytes, 28, ciphertextBytes, 0, ciphertextBytes.Length);

            var plaintextBytes = new byte[ciphertextBytes.Length];
            using var aesGcm = new AesGcm(_kekLocal, tagSizeInBytes: 16);
            aesGcm.Decrypt(nonce, ciphertextBytes, tag, plaintextBytes);

            return Encoding.UTF8.GetString(plaintextBytes);
        }
    }
}
