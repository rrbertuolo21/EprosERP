using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Endpoint genérico de enums de domínio. Devolve os membros (valor + Description) de um enum
    /// localizado por nome, via reflexão sobre os enums de <c>Epros.Shared</c> e dos módulos.
    /// Substitui os ~15 <c>*EnumsController</c> do legado por 1 controller genérico usado pelos
    /// dropdowns do frontend. Metadados públicos (sem estado / sem persistência).
    /// </summary>
    [ApiController]
    [Route("api/v1/enums")]
    [Produces("application/json")]
    public class EnumsController : ControllerBase
    {
        // Cache do índice nome-normalizado -> Type de enum, montado uma vez por reflexão.
        private static readonly Lazy<IReadOnlyDictionary<string, Type>> _indiceEnums =
            new Lazy<IReadOnlyDictionary<string, Type>>(ConstruirIndice);

        /// <summary>
        /// Retorna os membros do enum de domínio informado (ex.: <c>ETipoPagamento</c>, <c>tipo-pagamento</c>,
        /// <c>EModalidadeFrete</c>). A busca é tolerante a caixa, prefixo <c>E</c> e separadores (- _).
        /// </summary>
        [HttpGet("{dominio}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public IActionResult Obter(string dominio)
        {
            if (string.IsNullOrWhiteSpace(dominio))
                return NotFound(CommandResult.Falha("Domínio de enum não informado."));

            var chave = Normalizar(dominio);
            if (!_indiceEnums.Value.TryGetValue(chave, out var enumType))
                return NotFound(CommandResult.Falha($"Enum de domínio '{dominio}' não encontrado."));

            var membros = EnumReflectionHelper.ObterMembros(enumType);
            return Ok(CommandResult.Ok("OK", membros));
        }

        /// <summary>Lista todos os domínios de enum disponíveis (nomes canônicos).</summary>
        [HttpGet]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public IActionResult Listar()
        {
            var nomes = _indiceEnums.Value.Values
                .Select(t => t.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToList();
            return Ok(CommandResult.Ok("OK", new { Total = nomes.Count, Itens = nomes }));
        }

        private static IReadOnlyDictionary<string, Type> ConstruirIndice()
        {
            var indice = new Dictionary<string, Type>();

            // Assemblies-âncora: Shared (enums compartilhados) + um tipo marcador por módulo dono de enums.
            var ancoras = new[]
            {
                typeof(Epros.Shared.Domain.Enums.EOrigem),
                typeof(Epros.Modules.Estoque.Infrastructure.Data.ContextEstoque),
                typeof(Epros.Modules.Vendas.Infrastructure.Data.ContextVendas),
                typeof(Epros.Modules.Financeiro.Infrastructure.Data.ContextFinanceiro),
                typeof(Epros.Modules.Fiscal.Infrastructure.Data.ContextFiscal),
                typeof(Epros.Modules.GestaoClientes.Infrastructure.Data.ContextGestaoClientes),
                typeof(Epros.Modules.Aplicativo.Infrastructure.Data.ContextAplicativo),
            };

            var assemblies = ancoras.Select(t => t.Assembly).Distinct();

            foreach (var assembly in assemblies)
            {
                IEnumerable<Type> tipos;
                try
                {
                    tipos = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    tipos = ex.Types.Where(t => t != null)!;
                }

                foreach (var tipo in tipos.Where(t => t is { IsEnum: true, IsPublic: true }))
                {
                    // Chaves aceitas: nome exato normalizado e nome sem o prefixo "E".
                    Registrar(indice, tipo.Name, tipo);
                    if (tipo.Name.Length > 1 && tipo.Name[0] == 'E' && char.IsUpper(tipo.Name[1]))
                        Registrar(indice, tipo.Name.Substring(1), tipo);
                }
            }

            return indice;
        }

        private static void Registrar(Dictionary<string, Type> indice, string nome, Type tipo)
        {
            var chave = Normalizar(nome);
            // Primeiro registro vence (evita sobrescrever enum de nome exato por colisão sem prefixo).
            if (!indice.ContainsKey(chave))
                indice[chave] = tipo;
        }

        private static string Normalizar(string valor)
            => new string(valor.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
    }
}
