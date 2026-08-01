using System;
using System.Text.Json;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Contracts;
using Epros.Modules.GestaoClientes.Application.Dtos;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Epros.Modules.GestaoClientes.Infrastructure.Services
{
    public class ConfiguracaoGlobalCache : IConfiguracaoGlobalCache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<ConfiguracaoGlobalCache> _logger;
        private readonly IConnectionMultiplexer? _redis;
        private readonly IDatabase? _redisDb;
        private readonly ISubscriber? _pubSub;
        private static readonly RedisChannel ChannelName = RedisChannel.Literal("configuracaoglobal:invalida");
        private static readonly TimeSpan L1Duration = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan L2Duration = TimeSpan.FromHours(1);

        public ConfiguracaoGlobalCache(
            IMemoryCache memoryCache,
            IConfiguration configuration,
            ILogger<ConfiguracaoGlobalCache> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;

            var connectionString = configuration.GetConnectionString("RedisConnection") ??
                                   configuration.GetValue<string>("RedisConnection") ??
                                   "localhost:6379";

            try
            {
                var options = ConfigurationOptions.Parse(connectionString);
                options.ConnectTimeout = 2000; // 2 segundos para timeout de conexão
                options.SyncTimeout = 2000;
                options.AbortOnConnectFail = false; // Não lança exceção em caso de erro ao conectar

                _redis = ConnectionMultiplexer.Connect(options);

                if (_redis.IsConnected)
                {
                    _redisDb = _redis.GetDatabase();
                    _pubSub = _redis.GetSubscriber();

                    // Se inscreve no canal de Pub/Sub para escutar invalidações de outras instâncias
                    _pubSub.Subscribe(ChannelName, (channel, message) =>
                    {
                        var chave = message.ToString();
                        var cacheKey = ObterL1Chave(chave);
                        _memoryCache.Remove(cacheKey);
                        _logger.LogInformation("L1 Cache invalidado via Pub/Sub para a chave global: {Chave}", chave);
                    });

                    _logger.LogInformation("Redis/Valkey L2 Cache inicializado com sucesso para Configuração Global.");
                }
                else
                {
                    _logger.LogWarning("Redis/Valkey offline. O cache L2 de configurações globais está inativo (fallback automático para L1).");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao inicializar conexão com o Redis/Valkey para cache L2. Operando apenas com L1.");
            }
        }

        public async Task<ConfiguracaoGlobalCacheDto?> ObterAsync(string chave, Func<Task<ConfiguracaoGlobal?>> factory)
        {
            var l1Key = ObterL1Chave(chave);

            // 1. Tenta obter do Cache L1 (InMemory)
            if (_memoryCache.TryGetValue(l1Key, out ConfiguracaoGlobalCacheDto? cachedDto))
            {
                _logger.LogDebug("Configuração global obtida do cache L1 (InMemory): {Chave}", chave);
                return cachedDto;
            }

            // 2. Se falhar, tenta obter do Cache L2 (Redis/Valkey)
            if (_redisDb != null && _redis != null && _redis.IsConnected)
            {
                try
                {
                    var l2Key = ObterL2Chave(chave);
                    var l2Value = await _redisDb.StringGetAsync(l2Key);

                    if (l2Value.HasValue)
                    {
                        var dto = JsonSerializer.Deserialize<ConfiguracaoGlobalCacheDto>(l2Value!);
                        if (dto != null)
                        {
                            // Salva de volta no L1
                            _memoryCache.Set(l1Key, dto, L1Duration);
                            _logger.LogDebug("Configuração global obtida do cache L2 (Redis) e salva no L1: {Chave}", chave);
                            return dto;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao tentar ler do cache L2 (Redis) para a chave: {Chave}", chave);
                }
            }

            // 3. Se falhar, lê da base de dados (factory)
            _logger.LogInformation("Cache miss para configuração global: {Chave}. Buscando do banco de dados.", chave);
            var config = await factory();

            if (config == null)
            {
                return null;
            }

            // Transforma no DTO plano
            var resultDto = new ConfiguracaoGlobalCacheDto(
                config.Id,
                config.Chave,
                config.Valor,
                config.EhSegredo,
                config.Descricao,
                config.TenantId
            );

            // Grava de volta nos caches
            _memoryCache.Set(l1Key, resultDto, L1Duration);

            if (_redisDb != null && _redis != null && _redis.IsConnected)
            {
                try
                {
                    var l2Key = ObterL2Chave(chave);
                    var l2Value = JsonSerializer.Serialize(resultDto);
                    await _redisDb.StringSetAsync(l2Key, l2Value, L2Duration);
                    _logger.LogDebug("Configuração global salva no cache L2 (Redis): {Chave}", chave);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao tentar gravar no cache L2 (Redis) para a chave: {Chave}", chave);
                }
            }

            return resultDto;
        }

        public async Task InvalidarAsync(string chave)
        {
            var l1Key = ObterL1Chave(chave);

            // 1. Limpa L1 local
            _memoryCache.Remove(l1Key);
            _logger.LogInformation("Cache L1 local invalidado manualmente para a chave: {Chave}", chave);

            // 2. Limpa L2 e notifica outras instâncias via Pub/Sub
            if (_redisDb != null && _redis != null && _redis.IsConnected)
            {
                try
                {
                    var l2Key = ObterL2Chave(chave);
                    await _redisDb.KeyDeleteAsync(l2Key);
                    _logger.LogInformation("Cache L2 (Redis) invalidado manualmente para a chave: {Chave}", chave);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao tentar excluir chave L2 (Redis): {Chave}", chave);
                }

                if (_pubSub != null)
                {
                    try
                    {
                        await _pubSub.PublishAsync(ChannelName, chave);
                        _logger.LogInformation("Mensagem de invalidação Pub/Sub publicada para a chave: {Chave}", chave);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao tentar publicar mensagem Pub/Sub de invalidação para a chave: {Chave}", chave);
                    }
                }
            }
        }

        private string ObterL1Chave(string chave) => $"configuracaoglobal:l1:{chave}";
        private string ObterL2Chave(string chave) => $"configuracaoglobal:l2:{chave}";
    }
}
