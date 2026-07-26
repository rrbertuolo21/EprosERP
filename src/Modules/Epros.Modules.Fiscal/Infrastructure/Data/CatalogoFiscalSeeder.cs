using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Infrastructure.Data
{
    /// <summary>
    /// Semeia os catálogos fiscais nacionais que são pré-requisito para emissão de nota:
    /// CFOP (621 códigos, portados do arquivo legado <c>CFO_LISTA_FIN.csv</c>) e
    /// CST IBS/CBS (Reforma Tributária: 000, 200, 410, 510).
    ///
    /// Catálogos NACIONAIS (<see cref="Epros.Shared.Domain.Entities.IGlobalEntity"/>): gravados com
    /// tenant "system" e visíveis a todos os tenants (sem filtro por tenant nas queries globais).
    ///
    /// Idempotente: só insere o que ainda não existe (checa por código). Pode rodar em toda inicialização.
    ///
    /// FORA DE ESCOPO (dependem de fonte externa — NÃO inventados aqui):
    /// - NCM: no legado vem de JSON externo (IBGE/Receita) via <c>NcmService.ImportarDadosJson</c>.
    /// - CEST: sem seed no legado; importado externamente.
    /// Reportar ao usuário: NCM/CEST precisam do arquivo de dados (vem na migração de dados dos clientes).
    /// </summary>
    public static class CatalogoFiscalSeeder
    {
        private const string TenantSistema = "system";
        private const string UsuarioSeed = "seed-fiscal";

        public static async Task SeedAsync(ContextFiscal context, CancellationToken ct = default)
        {
            await SeedCfopAsync(context, ct);
            await SeedCstIbsCbsAsync(context, ct);
        }

        // ----------------------------------------------------------------------------------------
        // CFOP
        // ----------------------------------------------------------------------------------------
        private static async Task SeedCfopAsync(ContextFiscal context, CancellationToken ct)
        {
            // Cfop é IGlobalEntity: query sem filtro de tenant já enxerga todos os códigos existentes.
            var existentes = await context.Cfops
                .AsNoTracking()
                .Select(c => c.CfopCodigo)
                .ToListAsync(ct);

            var jaExiste = new HashSet<int>(existentes);

            var novos = new List<Cfop>();
            foreach (var (codigo, natureza) in LerCfopDoRecursoEmbutido())
            {
                if (jaExiste.Contains(codigo))
                    continue;

                novos.Add(ConstruirCfop(codigo, natureza));
                jaExiste.Add(codigo);
            }

            if (novos.Count == 0)
                return;

            await context.Cfops.AddRangeAsync(novos, ct);
            await context.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Constrói um <see cref="Cfop"/> derivando os indicadores a partir do código.
        /// As faixas do CFOP são padronizadas nacionalmente: o 1º dígito indica a esfera da operação
        /// (1/2/3 = entrada; 5/6/7 = saída) e os 3 últimos, a natureza. Os indicadores derivados aqui
        /// habilitam o uso em NF-e/NFC-e e sinalizam devolução/transferência/remessa por prefixo,
        /// suficiente para emissão. Ajustes finos (ST/CIAP/combustível por código) podem ser feitos
        /// depois na tela de manutenção de CFOP; o objetivo do seed é destravar a emissão.
        /// </summary>
        private static Cfop ConstruirCfop(int codigo, string natureza)
        {
            var cod = codigo.ToString();
            var primeiro = cod.Length > 0 ? cod[0] : '5';
            var sufixo = cod.Length >= 3 ? cod[^3..] : cod;
            var descLower = (natureza ?? string.Empty).ToLowerInvariant();

            bool ehSaida = primeiro is '5' or '6' or '7';
            bool ehEntrada = primeiro is '1' or '2' or '3';

            bool devolucao = descLower.Contains("devolu") || descLower.Contains("retorno");
            bool transferencia = descLower.Contains("transfer");
            bool remessa = descLower.Contains("remessa");
            bool combustivel = descLower.Contains("combust") || descLower.Contains("energia el");
            bool comunicacao = descLower.Contains("comunica");
            bool transporte = descLower.Contains("transporte");
            bool usoConsumo = descLower.Contains("uso") && descLower.Contains("consumo");

            // Incidência no Simples: revenda/venda de mercadoria como padrão (ajustável na UI).
            var incidencia = ehSaida ? EIncidenciaSimples.RevendaMercadorias : EIncidenciaSimples.NaoAplica;

            return new Cfop(
                cfopCodigo: codigo,
                descricao: natureza ?? string.Empty,
                naturezaOperacao: natureza ?? string.Empty,
                cfopCorrelacao: string.Empty,
                integraFaturamento: ehSaida && !remessa && !transferencia,
                indicadorNfe: true,
                indicadorComunicacao: comunicacao,
                indicadorTransporte: transporte,
                indicadorDevolucao: devolucao,
                indicadorRetorno: descLower.Contains("retorno"),
                indicadorAnulacao: descLower.Contains("anula"),
                indicadorRemessa: remessa,
                indicadorCombustivel: combustivel,
                indicadorTransferencia: transferencia,
                // NFC-e (modelo 65) só admite CFOP de saída internos (5xxx). Habilita nesse caso.
                indicadorNfce: primeiro == '5',
                indicadorCiap: false,
                indicadorUsoConsumo: usoConsumo,
                indicadorUsoSemOperacao: false,
                indicadorSt: descLower.Contains("substitui"),
                indicadorMei: false,
                incidenciaSimples: incidencia,
                cfopDevolucao: null,
                tenantId: TenantSistema,
                criadoPor: UsuarioSeed);
        }

        private static IEnumerable<(int Codigo, string Natureza)> LerCfopDoRecursoEmbutido()
        {
            var assembly = typeof(CatalogoFiscalSeeder).Assembly;
            var recurso = assembly
                .GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("cfop.csv", StringComparison.OrdinalIgnoreCase));

            if (recurso is null)
                yield break;

            using var stream = assembly.GetManifestResourceStream(recurso);
            if (stream is null)
                yield break;

            using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);

            bool primeiraLinha = true;
            string? linha;
            while ((linha = reader.ReadLine()) != null)
            {
                if (primeiraLinha) { primeiraLinha = false; continue; } // cabeçalho
                if (string.IsNullOrWhiteSpace(linha)) continue;

                var partes = linha.Split(';', 2);
                if (partes.Length < 2) continue;
                if (!int.TryParse(partes[0].Trim(), out var codigo)) continue;

                yield return (codigo, partes[1].Trim());
            }
        }

        // ----------------------------------------------------------------------------------------
        // CST IBS/CBS (Reforma Tributária) — códigos suportados pelo motor legado (CalculaIbsCbs).
        // ----------------------------------------------------------------------------------------
        private static async Task SeedCstIbsCbsAsync(ContextFiscal context, CancellationToken ct)
        {
            var catalogo = new (string Cst, string Descricao)[]
            {
                ("000", "Tributação integral (IBS/CBS)"),
                ("200", "Alíquota reduzida (IBS/CBS)"),
                ("410", "Imunidade / não incidência (IBS/CBS)"),
                ("510", "Diferimento (IBS/CBS)"),
            };

            var existentes = await context.CstsIbsCbs
                .AsNoTracking()
                .Select(c => c.Cst)
                .ToListAsync(ct);

            var jaExiste = new HashSet<string>(existentes, StringComparer.OrdinalIgnoreCase);

            var novos = new List<CstIbsCbs>();
            foreach (var (cst, descricao) in catalogo)
            {
                if (jaExiste.Contains(cst))
                    continue;

                novos.Add(new CstIbsCbs(
                    cst: cst,
                    descricao: descricao,
                    dataInicioVigencia: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    dataFimVigencia: null,
                    tenantId: TenantSistema,
                    criadoPor: UsuarioSeed));
            }

            if (novos.Count == 0)
                return;

            await context.CstsIbsCbs.AddRangeAsync(novos, ct);
            await context.SaveChangesAsync(ct);
        }
    }
}
