using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;
using MediatR;

namespace Epros.Modules.Estoque.Application.Handlers
{
    // ============================ Alterar imagem ============================

    /// <summary>Altera Imagem Produto.</summary>
    public class AlterarImagemProdutoCommandHandler : IRequestHandler<AlterarImagemProdutoCommand, CommandResult>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public AlterarImagemProdutoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AlterarImagemProdutoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == request.ProdutoId, cancellationToken);
            if (produto == null)
                return CommandResult.Falha("Objeto não localizado.");

            produto.AlterarImagem(request.Imagem ?? string.Empty, usuario);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Imagem do produto atualizada com sucesso!", new { produto.Id, produto.Imagem });
        }
    }

    // ============================ Deletar imagem ============================

    /// <summary>Exclui Imagem Produto.</summary>
    public class DeletarImagemProdutoCommandHandler : IRequestHandler<DeletarImagemProdutoCommand, CommandResult>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public DeletarImagemProdutoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarImagemProdutoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == request.ProdutoId, cancellationToken);
            if (produto == null)
                return CommandResult.Falha("Objeto não localizado.");

            produto.AlterarImagem(string.Empty, usuario);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Imagem do produto removida com sucesso!", new { produto.Id });
        }
    }

    // ============================ Duplicar produto ============================

    /// <summary>Handler de aplicação (DuplicarProdutoCommandHandler).</summary>
    public class DuplicarProdutoCommandHandler : IRequestHandler<DuplicarProdutoCommand, CommandResult>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DuplicarProdutoCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DuplicarProdutoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var origem = await _context.Produtos
                .AsNoTracking()
                .Include(p => p.AdicionaisProduto)
                .Include(p => p.ProdutoEspecifico)
                    .ThenInclude(e => e!.Origens)
                .FirstOrDefaultAsync(p => p.Id == request.ProdutoId && p.DeletadoEm == null, cancellationToken);

            if (origem == null)
                return CommandResult.Falha("Id do produto não localizado.");

            var skuExistente = await _context.Produtos.AnyAsync(p => p.Codigo == request.Codigo, cancellationToken);
            if (skuExistente)
                return CommandResult.Falha("Já existe um produto cadastrado com este SKU/Código.");

            // Novo produto copiando todos os atributos, exceto Código/Descrição e imagem (legado zera a imagem).
            var novo = new Produto(
                categoriaId: origem.CategoriaId,
                marcaProdutoId: origem.MarcaProdutoId,
                unidadeMedidaComercialId: origem.UnidadeMedidaComercialId,
                codigo: request.Codigo,
                descricao: request.Descricao,
                ean: origem.Ean,
                pesoLiquido: origem.PesoLiquido,
                pesoBruto: origem.PesoBruto,
                valorVenda: origem.ValorVenda,
                valorVendaPrazo: origem.ValorVendaPrazo,
                valorCompra: origem.ValorCompra,
                tipoProduto: origem.TipoProduto,
                ativo: origem.Ativo,
                imagem: string.Empty,
                cestId: origem.CestId,
                codigoAnpId: origem.CodigoAnpId,
                balancaId: origem.BalancaId,
                utilizaBalanca: origem.UtilizaBalanca,
                codigoProdutoBalanca: origem.CodigoProdutoBalanca,
                ncmId: origem.NcmId,
                tenantId: tenantId,
                criadoPor: usuario);

            if (origem.ProdutoGrupoId.HasValue)
                novo.DefinirProdutoGrupo(origem.ProdutoGrupoId, usuario);

            foreach (var adc in origem.AdicionaisProduto)
                novo.AdicionarAdicionaisProduto(adc.AdicionaisId, usuario);

            if (origem.ProdutoEspecifico != null)
            {
                var pe = origem.ProdutoEspecifico;
                novo.AdicionarProdutoEspecifico(
                    pe.ValorPercentualGlpDerivadoPetroleo,
                    pe.ValorPercentualGasNaturalNacional,
                    pe.ValorPercentualGasNaturalImportado,
                    pe.ValorPartida,
                    pe.UfConsumo,
                    usuario);

                if (novo.ProdutoEspecifico != null && pe.Origens != null)
                {
                    foreach (var origemComb in pe.Origens)
                        novo.ProdutoEspecifico.AdicionarOrigem(origemComb.IndicadorImportacao, origemComb.UfOrigem, origemComb.ValorPercentualUf, usuario);
                }
            }

            if (!novo.IsValid)
                return CommandResult.Falha(novo.Notifications.Select(n => n.Message), "Falha na validação do produto duplicado.");

            _context.Produtos.Add(novo);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Produto duplicado com sucesso!", new { ProdutoId = novo.Id });
        }
    }

    // ============================ Importar CSV ============================

    /// <summary>Importa Produtos Csv.</summary>
    public class ImportarProdutosCsvCommandHandler : IRequestHandler<ImportarProdutosCsvCommand, CommandResult>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ImportarProdutosCsvCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ImportarProdutosCsvCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (string.IsNullOrWhiteSpace(request.ConteudoCsv))
                return CommandResult.Falha("Arquivo CSV não enviado.");

            // Trava do legado: só importa quando ainda não há produtos cadastrados.
            var existeProdutos = await _context.Produtos.AsNoTracking().AnyAsync(cancellationToken);
            if (existeProdutos)
                return CommandResult.Falha("Já existem produtos cadastrados. Arquivo não será importado.");

            var linhas = request.ConteudoCsv.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n', StringSplitOptions.None);
            if (linhas.Length == 0 || string.IsNullOrWhiteSpace(linhas[0]))
                return CommandResult.Falha("CSV sem cabeçalho.");

            var headers = ParseCsvLine(linhas[0]);
            var headerMap = headers
                .Select((name, idx) => new { Name = NormalizeHeader(name), Index = idx })
                .GroupBy(x => x.Name)
                .ToDictionary(g => g.Key, g => g.First().Index);

            var obrigatorios = new[] { "codigo", "descricao" };
            var faltantes = obrigatorios.Where(x => !headerMap.ContainsKey(x)).ToArray();
            if (faltantes.Length > 0)
                return CommandResult.Falha($"Cabeçalho inválido. Campos obrigatórios ausentes: {string.Join(", ", faltantes)}");

            var erros = new List<string>();
            var produtos = new List<Produto>();
            var codigosVistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 1; i < linhas.Length; i++)
            {
                var line = linhas[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var numeroLinha = i + 1;
                var cols = ParseCsvLine(line);

                string GetCol(string chave)
                {
                    if (headerMap.TryGetValue(chave, out var idx) && idx < cols.Length)
                        return cols[idx].Trim();
                    return string.Empty;
                }

                var codigo = GetCol("codigo");
                var descricao = GetCol("descricao");

                if (string.IsNullOrWhiteSpace(codigo)) { erros.Add($"Linha {numeroLinha}: Código vazio."); continue; }
                if (string.IsNullOrWhiteSpace(descricao)) { erros.Add($"Linha {numeroLinha}: Descrição vazia."); continue; }
                if (!codigosVistos.Add(codigo)) { erros.Add($"Linha {numeroLinha}: Código '{codigo}' duplicado no arquivo."); continue; }

                var ean = GetCol("ean");
                var valorVenda = ParseDecimal(GetCol("valorvenda"));
                var valorVendaPrazo = ParseDecimal(GetCol("valorvendaprazo"));
                var valorCompra = ParseDecimal(GetCol("valorcompra"));

                var produto = new Produto(
                    categoriaId: null,
                    marcaProdutoId: null,
                    unidadeMedidaComercialId: null,
                    codigo: codigo,
                    descricao: descricao,
                    ean: string.IsNullOrWhiteSpace(ean) ? "Sem GTIN" : ean,
                    pesoLiquido: 0m,
                    pesoBruto: 0m,
                    valorVenda: valorVenda,
                    valorVendaPrazo: valorVendaPrazo == 0 ? valorVenda : valorVendaPrazo,
                    valorCompra: valorCompra,
                    tipoProduto: ETipoProduto.MateriaPrima,
                    ativo: true,
                    imagem: string.Empty,
                    cestId: null,
                    codigoAnpId: null,
                    balancaId: null,
                    utilizaBalanca: false,
                    codigoProdutoBalanca: null,
                    ncmId: null,
                    tenantId: tenantId,
                    criadoPor: usuario);

                if (!produto.IsValid)
                {
                    erros.Add($"Linha {numeroLinha}: {string.Join("; ", produto.Notifications.Select(n => n.Message))}");
                    continue;
                }

                produtos.Add(produto);
            }

            if (produtos.Count == 0)
                return CommandResult.Falha(erros.Count > 0 ? erros : new[] { "Nenhum produto válido no CSV." }, "Nenhum produto importado.");

            _context.Produtos.AddRange(produtos);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok($"Importação concluída: {produtos.Count} produto(s) importado(s), {erros.Count} linha(s) com erro.",
                new { Importados = produtos.Count, Erros = erros });
        }

        private static string[] ParseCsvLine(string line)
        {
            // Suporta ';' (padrão BR) e ',' como separadores; sem aspas complexas (fiel ao uso simples do legado).
            var separador = line.Contains(';') ? ';' : ',';
            return line.Split(separador);
        }

        private static string NormalizeHeader(string header)
        {
            var normalized = (header ?? string.Empty).Trim().ToLowerInvariant();
            var semAcento = new string(normalized
                .Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());
            return new string(semAcento.Where(char.IsLetterOrDigit).ToArray());
        }

        private static decimal ParseDecimal(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return 0m;
            valor = valor.Replace(" ", "");
            // Aceita vírgula decimal (BR) e ponto.
            if (valor.Contains(',') && valor.Contains('.'))
                valor = valor.Replace(".", "").Replace(",", ".");
            else
                valor = valor.Replace(",", ".");
            return decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0m;
        }
    }

    // ============================ Reajustar preços (massa) ============================

    /// <summary>Handler de aplicação (ReajustarPrecosCommandHandler).</summary>
    public class ReajustarPrecosCommandHandler : IRequestHandler<ReajustarPrecosCommand, CommandResult>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public ReajustarPrecosCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ReajustarPrecosCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var query = _context.Produtos.Where(p => p.DeletadoEm == null);

            if (request.ProdutosIds != null && request.ProdutosIds.Count > 0)
            {
                var ids = request.ProdutosIds.Distinct().ToList();
                query = query.Where(p => ids.Contains(p.Id));
            }
            else
            {
                query = query.Where(p => p.Ativo);
            }

            var produtos = await query.ToListAsync(cancellationToken);
            if (produtos.Count == 0)
                return CommandResult.Falha("Nenhum produto encontrado para reajuste.");

            var tipo = (ETipoReajuste)request.Tipo;
            var fator = request.Fator == 0 ? 1 : request.Fator;

            foreach (var produto in produtos)
            {
                var valorAtual = tipo == ETipoReajuste.Valorcompra ? produto.ValorCompra : produto.ValorVenda;
                var valorNovo = (valorAtual * fator) + request.ValorFixo;

                if (tipo == ETipoReajuste.Valorcompra)
                    produto.AlterarValorCompra(valorAtual, request.Fator, request.ValorFixo, valorNovo, request.Motivo, usuario);
                else
                    produto.AlterarValorVenda(valorAtual, request.Fator, request.ValorFixo, valorNovo, request.Motivo, usuario);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok($"Reajuste aplicado a {produtos.Count} produto(s) com sucesso!", new { Reajustados = produtos.Count });
        }
    }
}
