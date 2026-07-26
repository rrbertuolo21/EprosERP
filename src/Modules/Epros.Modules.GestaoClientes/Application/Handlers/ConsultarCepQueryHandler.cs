using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Consulta Cep.</summary>
    public class ConsultarCepQueryHandler : IQueryHandler<ConsultarCepQuery, CepResultadoDto?>
    {
        private readonly ContextGestaoClientes _context;
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        public ConsultarCepQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<CepResultadoDto?> Handle(ConsultarCepQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Cep))
                return null;

            var cepNormalizado = request.Cep.Replace("-", "").Replace(" ", "").Trim();

            // 1. Obter Brasil
            var brasil = await _context.Paises.FirstOrDefaultAsync(p => p.CodigoIsoAlpha2 == "BR", cancellationToken);
            if (brasil == null)
            {
                brasil = new Pais("Brasil", "BR", "BRA", "076", "Brasília", "+55", "system");
                _context.Paises.Add(brasil);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // 2. Verificar cache
            var cache = await _context.CodigosPostaisCache
                .Include(c => c.Municipio)
                .FirstOrDefaultAsync(c => c.PaisId == brasil.Id && c.CodigoPostal == cepNormalizado, cancellationToken);

            if (cache != null)
            {
                if (cache.Falhou)
                {
                    // Se falhou no cache, retornamos null para permitir preenchimento manual
                    return null;
                }

                return new CepResultadoDto
                {
                    Cep = cache.CodigoPostal,
                    Logradouro = cache.Logradouro ?? string.Empty,
                    Complemento = string.Empty,
                    Bairro = cache.Bairro ?? string.Empty,
                    Localidade = cache.Municipio?.Nome ?? string.Empty,
                    Uf = cache.Uf ?? string.Empty,
                    Ibge = cache.Municipio?.CodigoIbge.ToString() ?? string.Empty,
                    MunicipioId = cache.MunicipioId,
                    PaisId = cache.PaisId
                };
            }

            // 3. Consultar via API externa (ViaCEP)
            try
            {
                var url = $"https://viacep.com.br/ws/{cepNormalizado}/json/";
                var response = await _httpClient.GetAsync(url, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    using var doc = JsonDocument.Parse(content);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("erro", out var erroProp) && 
                        ((erroProp.ValueKind == JsonValueKind.True) || 
                         (erroProp.ValueKind == JsonValueKind.String && string.Equals(erroProp.GetString(), "true", StringComparison.OrdinalIgnoreCase))))
                    {
                        // CEP não existe
                        var cacheFalha = CodigoPostalCache.CriarFalha(brasil.Id, cepNormalizado, "ViaCEP", "CEP não encontrado na base do ViaCEP", "system");
                        _context.CodigosPostaisCache.Add(cacheFalha);
                        await _context.SaveChangesAsync(cancellationToken);
                        return null;
                    }

                    var logradouro = root.TryGetProperty("logradouro", out var logProp) ? logProp.GetString() : "";
                    var bairro = root.TryGetProperty("bairro", out var baiProp) ? baiProp.GetString() : "";
                    var localidade = root.TryGetProperty("localidade", out var locProp) ? locProp.GetString() : "";
                    var uf = root.TryGetProperty("uf", out var ufProp) ? ufProp.GetString() : "";
                    var ibge = root.TryGetProperty("ibge", out var ibgeProp) ? ibgeProp.GetString() : "";

                    long ibgeLong = 0;
                    Guid? municipioId = null;
                    if (long.TryParse(ibge, out ibgeLong))
                    {
                        var mun = await _context.Municipios.FirstOrDefaultAsync(m => m.CodigoIbge == ibgeLong, cancellationToken);
                        if (mun != null)
                        {
                            municipioId = mun.Id;
                        }
                        else
                        {
                            var subdivisao = await _context.Subdivisoes.FirstOrDefaultAsync(s => s.CodigoISO31662 == $"BR-{(uf ?? string.Empty).ToUpper()}", cancellationToken);
                            if (subdivisao != null)
                            {
                                var novoMun = new Municipio(brasil.Id, subdivisao.Id, localidade ?? "", ibgeLong, null, null, "system");
                                _context.Municipios.Add(novoMun);
                                await _context.SaveChangesAsync(cancellationToken);
                                municipioId = novoMun.Id;
                            }
                        }
                    }

                    // Salvar no cache com sucesso
                    var cacheSucesso = new CodigoPostalCache(
                        brasil.Id,
                        cepNormalizado,
                        logradouro,
                        bairro,
                        municipioId,
                        uf,
                        "ViaCEP",
                        "system"
                    );

                    _context.CodigosPostaisCache.Add(cacheSucesso);
                    await _context.SaveChangesAsync(cancellationToken);

                    return new CepResultadoDto
                    {
                        Cep = cepNormalizado,
                        Logradouro = logradouro ?? string.Empty,
                        Bairro = bairro ?? string.Empty,
                        Localidade = localidade ?? string.Empty,
                        Uf = uf ?? string.Empty,
                        Ibge = ibge ?? string.Empty,
                        MunicipioId = municipioId,
                        PaisId = brasil.Id
                    };
                }
                else
                {
                    // Falha no request
                    var cacheFalha = CodigoPostalCache.CriarFalha(brasil.Id, cepNormalizado, "ViaCEP", $"HTTP Status Code: {response.StatusCode}", "system");
                    _context.CodigosPostaisCache.Add(cacheFalha);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                // Registrar falha no cache para auditoria
                var cacheFalha = CodigoPostalCache.CriarFalha(brasil.Id, cepNormalizado, "ViaCEP", ex.Message, "system");
                _context.CodigosPostaisCache.Add(cacheFalha);
                try
                {
                    await _context.SaveChangesAsync(cancellationToken);
                }
                catch
                {
                    // Ignora erro ao salvar falha se houver problema de concorrência/DB
                }
            }

            return null;
        }
    }
}
