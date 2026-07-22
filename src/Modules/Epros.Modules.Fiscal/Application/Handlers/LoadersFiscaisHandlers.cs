using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Enums;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    /// <summary>Utilitários de parse compartilhados pelos loaders de tabelas fiscais.</summary>
    internal static class LoaderParse
    {
        private static readonly string[] DateFormats =
        {
            "dd/MM/yyyy", "yyyy-MM-dd", "d/M/yyyy", "yyyy-MM-ddTHH:mm:ss", "MM/dd/yyyy"
        };

        public static DateTime? ParseData(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;
            if (DateTime.TryParseExact(valor.Trim(), DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d;
            if (DateTime.TryParse(valor.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
                return d;
            return null;
        }

        public static decimal ParseDecimal(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return 0m;
            var v = valor.Trim().Replace("%", "");
            // Aceita tanto "1,23" (pt-BR) quanto "1.23" (invariante).
            if (v.Contains(',') && !v.Contains('.'))
                v = v.Replace(',', '.');
            return decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var r) ? r : 0m;
        }

        public static int ParseInt(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return 0;
            return int.TryParse(new string(valor.Trim().Where(char.IsDigit).ToArray()), out var r) ? r : 0;
        }

        public static bool ParseBool(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return false;
            var v = valor.Trim().ToLowerInvariant();
            return v is "1" or "true" or "s" or "sim" or "t" or "y" or "yes";
        }

        /// <summary>Detecta o separador de um CSV (";" ou ",") pela primeira linha não vazia.</summary>
        public static char DetectarSeparador(string primeiraLinha)
        {
            var pontoVirgula = primeiraLinha.Count(c => c == ';');
            var virgula = primeiraLinha.Count(c => c == ',');
            var tab = primeiraLinha.Count(c => c == '\t');
            if (tab >= pontoVirgula && tab >= virgula && tab > 0) return '\t';
            return pontoVirgula >= virgula ? ';' : ',';
        }

        /// <summary>Lê linhas de um CSV/TXT já como matriz de campos, ignorando linhas vazias.</summary>
        public static List<string[]> LerLinhas(string conteudo, out char separador)
        {
            var linhas = conteudo
                .Replace("\r\n", "\n").Replace('\r', '\n')
                .Split('\n')
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();

            separador = linhas.Count > 0 ? DetectarSeparador(linhas[0]) : ';';
            var sep = separador;
            return linhas.Select(l => l.Split(sep).Select(c => c.Trim().Trim('"')).ToArray()).ToList();
        }

        /// <summary>Heurística para identificar (e pular) uma linha de cabeçalho textual.</summary>
        public static bool PareceCabecalho(string[] campos, params string[] tokens)
        {
            var join = string.Join(" ", campos).ToLowerInvariant();
            return tokens.Any(t => join.Contains(t.ToLowerInvariant()));
        }
    }

    /// <summary>DTO do JSON oficial de NCM (Tabela de NCM Vigentes / Siscomex).</summary>
    internal sealed class NcmJsonDto
    {
        public List<NcmNomenclaturaDto> Nomenclaturas { get; set; } = new();
    }

    internal sealed class NcmNomenclaturaDto
    {
        public string? Codigo { get; set; }
        public string? Descricao { get; set; }
        public string? Data_Inicio { get; set; }
        public string? Data_Fim { get; set; }
        public string? Tipo_Ato_Ini { get; set; }
        public string? Numero_Ato_Ini { get; set; }
        public string? Ano_Ato_Ini { get; set; }
    }

    /// <summary>
    /// Loader da tabela de NCM. Aceita o JSON oficial (<c>{ "Nomenclaturas": [...] }</c>) do legado.
    /// Insere apenas NCMs de 8 dígitos ainda não presentes (mesmo código + mesma data de início),
    /// preservando a idempotência do carregamento. Fiel a <c>NcmService.Atualizar</c>.
    /// </summary>
    public class AtualizarTabelaNcmCommandHandler : ICommandHandler<AtualizarTabelaNcmCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarTabelaNcmCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarTabelaNcmCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            string conteudo;
            using (var reader = new StreamReader(request.Conteudo))
                conteudo = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(conteudo))
                return CommandResult.Falha("Arquivo vazio.");

            NcmJsonDto? dados;
            try
            {
                dados = JsonSerializer.Deserialize<NcmJsonDto>(conteudo, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                return CommandResult.Falha($"Arquivo JSON de NCM inválido: {ex.Message}");
            }

            if (dados?.Nomenclaturas is null || dados.Nomenclaturas.Count == 0)
                return CommandResult.Falha("Nenhuma nomenclatura encontrada no arquivo.");

            var existentes = await _context.Ncms
                .Select(n => new { n.CodigoNcm, n.DataInicio })
                .ToListAsync(cancellationToken);
            var chaves = new HashSet<string>(existentes.Select(e => $"{e.CodigoNcm}|{e.DataInicio:yyyy-MM-dd}"));

            var novos = new List<Ncm>();
            foreach (var nom in dados.Nomenclaturas)
            {
                var codigo = (nom.Codigo ?? string.Empty).Replace(".", string.Empty).Trim();
                if (codigo.Length != 8) continue;

                var dataInicio = LoaderParse.ParseData(nom.Data_Inicio) ?? DateTime.UtcNow.Date;
                var chave = $"{codigo}|{dataInicio:yyyy-MM-dd}";
                if (chaves.Contains(chave)) continue;
                chaves.Add(chave);

                novos.Add(new Ncm(
                    codigo,
                    nom.Descricao ?? string.Empty,
                    dataInicio,
                    LoaderParse.ParseData(nom.Data_Fim),
                    nom.Tipo_Ato_Ini,
                    nom.Numero_Ato_Ini,
                    nom.Ano_Ato_Ini,
                    usuario));
            }

            if (novos.Count > 0)
            {
                _context.Ncms.AddRange(novos);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return CommandResult.Ok($"Foram atualizados com sucesso {novos.Count} registros.", new { Registros = novos.Count });
        }
    }

    /// <summary>
    /// Loader da tabela de CFOP-padrão (CSV/TXT). Layout esperado por posição:
    /// <c>codigo;descricao;naturezaOperacao;dataInicio;dataFim;cfopCorrelacao;integraFaturamento;
    /// indNfe;indNfce;incidenciaSimples;cfopDevolucao</c> — colunas ausentes assumem default.
    /// Insere apenas códigos ainda não presentes (idempotente por CfopCodigo).
    /// </summary>
    public class AtualizarTabelaCfopPadraoCommandHandler : ICommandHandler<AtualizarTabelaCfopPadraoCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarTabelaCfopPadraoCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarTabelaCfopPadraoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            string conteudo;
            using (var reader = new StreamReader(request.Conteudo))
                conteudo = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(conteudo))
                return CommandResult.Falha("Arquivo vazio.");

            var linhas = LoaderParse.LerLinhas(conteudo, out _);
            if (linhas.Count == 0)
                return CommandResult.Falha("Arquivo sem linhas de dados.");

            var existentes = new HashSet<int>(await _context.CfopPadroes.Select(c => c.CfopCodigo).ToListAsync(cancellationToken));

            var novos = new List<CfopPadrao>();
            var erros = new List<string>();
            foreach (var campos in linhas)
            {
                if (campos.Length == 0) continue;
                if (LoaderParse.PareceCabecalho(campos, "codigo", "cfop", "descricao", "natureza")) continue;

                var codigo = LoaderParse.ParseInt(campos.ElementAtOrDefault(0));
                if (codigo < 1000 || codigo > 9999) continue;
                if (existentes.Contains(codigo)) continue;

                // IncidenciaSimples é obrigatória (enum sem 0). Se o arquivo não trouxer um valor
                // definido, assume "Revenda de Mercadorias" (2) como default neutro.
                var incidencia = (EIncidenciaSimples)LoaderParse.ParseInt(campos.ElementAtOrDefault(9));
                if (!Enum.IsDefined(typeof(EIncidenciaSimples), incidencia))
                    incidencia = EIncidenciaSimples.RevendaMercadorias;

                var cfop = new CfopPadrao(
                    cfopCodigo: codigo,
                    dataInicioVigencia: LoaderParse.ParseData(campos.ElementAtOrDefault(3)) ?? DateTime.UtcNow.Date,
                    dataFimVigencia: LoaderParse.ParseData(campos.ElementAtOrDefault(4)),
                    descricao: campos.ElementAtOrDefault(1) ?? string.Empty,
                    naturezaOperacao: campos.ElementAtOrDefault(2) ?? string.Empty,
                    cfopCorrelacao: NuloSeVazio(campos.ElementAtOrDefault(5)),
                    integraFaturamento: LoaderParse.ParseBool(campos.ElementAtOrDefault(6)),
                    indicadorNfe: LoaderParse.ParseBool(campos.ElementAtOrDefault(7)),
                    indicadorComunicacao: false,
                    indicadorTransporte: false,
                    indicadorDevolucao: false,
                    indicadorRetorno: false,
                    indicadorAnulacao: false,
                    indicadorRemessa: false,
                    indicadorCombustivel: false,
                    indicadorTransferencia: false,
                    indicadorNfce: LoaderParse.ParseBool(campos.ElementAtOrDefault(8)),
                    indicadorCiap: false,
                    indicadorUsoConsumo: false,
                    indicadorUsoSemOperacao: false,
                    indicadorSt: false,
                    indicadorMei: false,
                    incidenciaSimples: incidencia,
                    cfopDevolucao: NuloSeVazio(campos.ElementAtOrDefault(10)),
                    criadoPor: usuario);

                if (!cfop.IsValid)
                {
                    erros.Add($"CFOP {codigo}: {string.Join("; ", cfop.Notifications.Select(n => n.Message))}");
                    continue;
                }

                existentes.Add(codigo);
                novos.Add(cfop);
            }

            if (novos.Count > 0)
            {
                _context.CfopPadroes.AddRange(novos);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return CommandResult.Ok($"Foram atualizados com sucesso {novos.Count} registros.", new { Registros = novos.Count, Erros = erros });
        }

        private static string? NuloSeVazio(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
    }

    /// <summary>
    /// Loader dos Códigos de Serviço da SEFAZ (CSV/TXT). Layout esperado por posição:
    /// <c>codigo;descricao</c>. Insere apenas códigos ainda não presentes (idempotente por Codigo).
    /// </summary>
    public class AtualizarTabelaCodigoServicoSefazCommandHandler : ICommandHandler<AtualizarTabelaCodigoServicoSefazCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarTabelaCodigoServicoSefazCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarTabelaCodigoServicoSefazCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            string conteudo;
            using (var reader = new StreamReader(request.Conteudo))
                conteudo = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(conteudo))
                return CommandResult.Falha("Arquivo vazio.");

            var linhas = LoaderParse.LerLinhas(conteudo, out _);
            if (linhas.Count == 0)
                return CommandResult.Falha("Arquivo sem linhas de dados.");

            var existentes = new HashSet<string>(await _context.CodigosServicosSefaz.Select(c => c.Codigo).ToListAsync(cancellationToken));

            var novos = new List<CodigoServicoSefaz>();
            var erros = new List<string>();
            foreach (var campos in linhas)
            {
                if (campos.Length == 0) continue;
                if (LoaderParse.PareceCabecalho(campos, "codigo", "descricao")) continue;

                var codigo = (campos.ElementAtOrDefault(0) ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(codigo)) continue;
                if (codigo.Length > 5) codigo = codigo[..5];
                if (existentes.Contains(codigo)) continue;

                // Descrição pode conter o separador; reagrega o resto da linha.
                var descricao = campos.Length > 1 ? string.Join(" ", campos.Skip(1)).Trim() : string.Empty;

                var item = new CodigoServicoSefaz(codigo, descricao, usuario);
                if (!item.IsValid)
                {
                    erros.Add($"Código {codigo}: {string.Join("; ", item.Notifications.Select(n => n.Message))}");
                    continue;
                }

                existentes.Add(codigo);
                novos.Add(item);
            }

            if (novos.Count > 0)
            {
                _context.CodigosServicosSefaz.AddRange(novos);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return CommandResult.Ok($"Foram atualizados com sucesso {novos.Count} registros.", new { Registros = novos.Count, Erros = erros });
        }
    }

    /// <summary>
    /// Loader das alíquotas de FCP por UF (CSV/TXT). Layout esperado por posição:
    /// <c>uf;aliquota;observacao</c> (uf como sigla, ex.: "SP"). Idempotente por UF: se já existe,
    /// atualiza a alíquota/observação; caso contrário, insere.
    /// </summary>
    public class AtualizarTabelaFcpAliquotaUfCommandHandler : ICommandHandler<AtualizarTabelaFcpAliquotaUfCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarTabelaFcpAliquotaUfCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarTabelaFcpAliquotaUfCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            string conteudo;
            using (var reader = new StreamReader(request.Conteudo))
                conteudo = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(conteudo))
                return CommandResult.Falha("Arquivo vazio.");

            var linhas = LoaderParse.LerLinhas(conteudo, out _);
            if (linhas.Count == 0)
                return CommandResult.Falha("Arquivo sem linhas de dados.");

            var existentes = await _context.FcpAliquotasUf.ToListAsync(cancellationToken);

            var afetados = 0;
            var erros = new List<string>();
            foreach (var campos in linhas)
            {
                if (campos.Length == 0) continue;
                if (LoaderParse.PareceCabecalho(campos, "uf", "aliquota", "estado")) continue;

                var ufTexto = (campos.ElementAtOrDefault(0) ?? string.Empty).Trim().ToUpperInvariant();
                if (!Enum.TryParse<EEstado>(ufTexto, out var uf))
                {
                    erros.Add($"UF inválida: '{ufTexto}'.");
                    continue;
                }

                var aliquota = LoaderParse.ParseDecimal(campos.ElementAtOrDefault(1));
                var observacao = campos.ElementAtOrDefault(2);

                var atual = existentes.FirstOrDefault(f => f.Uf == uf && f.DeletadoEm == null);
                if (atual != null)
                {
                    atual.Alterar(uf, aliquota, string.IsNullOrWhiteSpace(observacao) ? null : observacao, usuario);
                    if (!atual.IsValid) { erros.Add($"UF {ufTexto}: {string.Join("; ", atual.Notifications.Select(n => n.Message))}"); continue; }
                }
                else
                {
                    var novo = new FcpAliquotaUf(uf, aliquota, string.IsNullOrWhiteSpace(observacao) ? null : observacao, usuario);
                    if (!novo.IsValid) { erros.Add($"UF {ufTexto}: {string.Join("; ", novo.Notifications.Select(n => n.Message))}"); continue; }
                    _context.FcpAliquotasUf.Add(novo);
                    existentes.Add(novo);
                }
                afetados++;
            }

            if (afetados > 0)
                await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok($"Foram atualizados com sucesso {afetados} registros.", new { Registros = afetados, Erros = erros });
        }
    }

    /// <summary>
    /// Loader da tabela nacional IBPT (CSV/TXT — <c>TabelaIBPTax*.csv</c>). Layout oficial por posição:
    /// <c>codigo;ex;tipo;descricao;nacionalfederal;importadosfederal;estadual;municipal;vigenciainicio;
    /// vigenciafim;chave;versao</c>. A UF vem do parâmetro do loader (o arquivo IBPT é por UF).
    /// Substitui os registros da UF informada (limpa e recarrega) para refletir a nova versão.
    /// </summary>
    public class AtualizarTabelaIbptCommandHandler : ICommandHandler<AtualizarTabelaIbptCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarTabelaIbptCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarTabelaIbptCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            string conteudo;
            using (var reader = new StreamReader(request.Conteudo))
                conteudo = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(conteudo))
                return CommandResult.Falha("Arquivo vazio.");

            // Tenta inferir a UF pelo nome do arquivo (ex.: "TabelaIBPTaxSP...csv"); default via coluna.
            EEstado? ufArquivo = InferirUfPeloNome(request.NomeArquivo);

            var linhas = LoaderParse.LerLinhas(conteudo, out _);
            if (linhas.Count == 0)
                return CommandResult.Falha("Arquivo sem linhas de dados.");

            var novos = new List<Ibpt>();
            var erros = new List<string>();
            var ufsCarregadas = new HashSet<EEstado>();

            foreach (var campos in linhas)
            {
                if (campos.Length == 0) continue;
                if (LoaderParse.PareceCabecalho(campos, "codigo", "nacionalfederal", "aliquota", "descricao")) continue;

                var codigo = (campos.ElementAtOrDefault(0) ?? string.Empty).Replace(".", string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(codigo)) continue;
                if (codigo.Length > 8) codigo = codigo[..8];

                // UF: coluna "uf" quando o layout a traz; senão a UF do arquivo.
                EEstado uf;
                var ufCampo = (campos.ElementAtOrDefault(12) ?? string.Empty).Trim().ToUpperInvariant();
                if (Enum.TryParse<EEstado>(ufCampo, out var ufParse))
                    uf = ufParse;
                else if (ufArquivo.HasValue)
                    uf = ufArquivo.Value;
                else
                {
                    erros.Add($"Código {codigo}: UF não identificada (informe no nome do arquivo, ex.: TabelaIBPTaxSP.csv).");
                    continue;
                }
                ufsCarregadas.Add(uf);

                var ibpt = new Ibpt(
                    codigo: codigo,
                    uf: uf,
                    ex: LoaderParse.ParseInt(campos.ElementAtOrDefault(1)),
                    tipo: LoaderParse.ParseInt(campos.ElementAtOrDefault(2)),
                    descricao: campos.ElementAtOrDefault(3),
                    aliquotaNacionalFederal: LoaderParse.ParseDecimal(campos.ElementAtOrDefault(4)),
                    aliquotaImportadosFederal: LoaderParse.ParseDecimal(campos.ElementAtOrDefault(5)),
                    aliquotaEstadual: LoaderParse.ParseDecimal(campos.ElementAtOrDefault(6)),
                    aliquotaMunicipal: LoaderParse.ParseDecimal(campos.ElementAtOrDefault(7)),
                    versao: campos.ElementAtOrDefault(11) ?? string.Empty,
                    chave: campos.ElementAtOrDefault(10),
                    vigenciaInicio: LoaderParse.ParseData(campos.ElementAtOrDefault(8)) ?? DateTime.UtcNow.Date,
                    vigenciaFim: LoaderParse.ParseData(campos.ElementAtOrDefault(9)) ?? DateTime.UtcNow.Date.AddYears(1),
                    criadoPor: usuario);

                if (!ibpt.IsValid)
                {
                    erros.Add($"Código {codigo}: {string.Join("; ", ibpt.Notifications.Select(n => n.Message))}");
                    continue;
                }

                novos.Add(ibpt);
            }

            if (novos.Count > 0)
            {
                // Recarga por UF: remove os registros antigos das UFs presentes no arquivo antes de inserir.
                var ufs = ufsCarregadas.ToList();
                var antigos = await _context.Ibpts.Where(i => ufs.Contains(i.Uf)).ToListAsync(cancellationToken);
                if (antigos.Count > 0)
                    _context.Ibpts.RemoveRange(antigos);

                _context.Ibpts.AddRange(novos);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return CommandResult.Ok($"Foram atualizados com sucesso {novos.Count} registros.", new { Registros = novos.Count, Erros = erros });
        }

        private static EEstado? InferirUfPeloNome(string? nomeArquivo)
        {
            if (string.IsNullOrWhiteSpace(nomeArquivo)) return null;
            var nome = nomeArquivo.ToUpperInvariant();
            foreach (var uf in Enum.GetNames(typeof(EEstado)))
            {
                // "TabelaIBPTaxSP25.1.A.csv" -> contém "SP".
                if (nome.Contains(uf))
                    return Enum.Parse<EEstado>(uf);
            }
            return null;
        }
    }
}
