using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Domain.Entities.Upload;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.Aplicativo.Infrastructure.Services;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    /// <summary>Handlers de Upload e Migração de Dados genéricos (PLT-UPL).</summary>
    public class UploadHandlers :
        ICommandHandler<ReceberParteUploadCommand>,
        ICommandHandler<RegistrarArquivoUploadCommand>,
        ICommandHandler<ImportarArquivoTabularCommand>,
        ICommandHandler<SalvarMapeamentoImportacaoCommand>,
        ICommandHandler<DesfazerImportacaoCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IArmazenamentoArquivoService _armazenamento;

        public UploadHandlers(ContextAplicativo context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            // Instanciado sem DI (Modo Porte): storage local com deduplicação por hash.
            _armazenamento = new ArmazenamentoLocalArquivoService();
        }

        private string Tenant() => _tenantProvider.GetTenantId();
        private string User() => _currentUser.GetUserId() ?? "system";

        public async Task<CommandResult> Handle(ReceberParteUploadCommand request, CancellationToken ct)
        {
            var tenant = Tenant();
            var user = User();

            UplExecucaoUpload execucao;
            if (request.ExecucaoUploadId.HasValue)
            {
                execucao = (await _context.UplExecucoesUpload.FirstOrDefaultAsync(e => e.Id == request.ExecucaoUploadId && e.DeletadoEm == null, ct))!;
                if (execucao == null) return CommandResult.Falha("Execução de upload não encontrada.");
            }
            else
            {
                execucao = new UplExecucaoUpload(request.UsuarioId, null, EUplOrigemUpload.Direct, request.NomeOriginal, request.Extensao, request.TotalBytes, null, request.PastaDestino, tenant, user);
                if (!execucao.IsValid) return CommandResult.Falha(execucao.Notifications.Select(n => n.Message));
                _context.UplExecucoesUpload.Add(execucao);
                await _context.SaveChangesAsync(ct);
            }

            // Primeira parte (byte_inicio == 0) limpa partes temporárias anteriores do mesmo upload (EF 7.2.2).
            if (request.ByteInicio == 0)
            {
                var antigas = await _context.UplUploadPartes.Where(p => p.ExecucaoUploadId == execucao.Id && p.DeletadoEm == null).ToListAsync(ct);
                foreach (var a in antigas) a.Deletar(user);
            }

            var caminhoTemp = Path.Combine(Path.GetTempPath(), "epros_upl_partes", $"{execucao.Id}_{request.ByteInicio}_{request.ByteFim}.part");
            Directory.CreateDirectory(Path.GetDirectoryName(caminhoTemp)!);
            await File.WriteAllBytesAsync(caminhoTemp, request.Conteudo, ct);

            var parte = new UplUploadParte(execucao.Id, request.ByteInicio, request.ByteFim, request.TotalBytes, caminhoTemp, false, tenant, user);
            _context.UplUploadPartes.Add(parte);
            await _context.SaveChangesAsync(ct);

            // Consolida quando o total esperado foi atingido (EF 7.2.6).
            var partes = await _context.UplUploadPartes.Where(p => p.ExecucaoUploadId == execucao.Id && p.DeletadoEm == null).OrderBy(p => p.ByteInicio).ToListAsync(ct);
            long acumulado = partes.Sum(p => (p.ByteFim - p.ByteInicio) + 1);

            if (acumulado < request.TotalBytes)
            {
                execucao.AlterarStatus(EUplStatusUpload.Recebido, user);
                await _context.SaveChangesAsync(ct);
                return CommandResult.Ok("Parte recebida. Aguardando próximas partes.", new { execucao.Id, Consolidado = false });
            }

            // Monta o arquivo completo a partir das partes.
            using var ms = new MemoryStream();
            foreach (var p in partes)
            {
                if (File.Exists(p.CaminhoTemporario))
                {
                    ms.Write(await File.ReadAllBytesAsync(p.CaminhoTemporario, ct));
                    p.MarcarCompleta(user);
                }
            }
            var conteudoFinal = ms.ToArray();

            var arquivoId = await ConsolidarArquivoAsync(conteudoFinal, request.UsuarioId, null, EUplOrigemUpload.Direct, request.NomeOriginal, request.Extensao, request.PastaDestino, tenant, user, ct);
            execucao.Consolidar(arquivoId, conteudoFinal.LongLength, user);
            await _context.SaveChangesAsync(ct);

            return CommandResult.Ok("Upload consolidado com sucesso.", new { execucao.Id, ArquivoId = arquivoId, Consolidado = true });
        }

        public async Task<CommandResult> Handle(RegistrarArquivoUploadCommand request, CancellationToken ct)
        {
            var tenant = Tenant();
            var user = User();

            var execucao = new UplExecucaoUpload(request.UsuarioId, request.UsuarioUploadId, request.Origem, request.NomeOriginal, request.Extensao, request.Conteudo.LongLength, null, request.PastaDestino, tenant, user);
            if (!execucao.IsValid) return CommandResult.Falha(execucao.Notifications.Select(n => n.Message));
            _context.UplExecucoesUpload.Add(execucao);

            var arquivoId = await ConsolidarArquivoAsync(request.Conteudo, request.UsuarioId, request.UsuarioUploadId, request.Origem, request.NomeOriginal, request.Extensao, request.PastaDestino, tenant, user, ct);
            execucao.Consolidar(arquivoId, request.Conteudo.LongLength, user);
            await _context.SaveChangesAsync(ct);

            return CommandResult.Ok("Arquivo registrado com sucesso.", new { ArquivoId = arquivoId });
        }

        /// <summary>
        /// Consolida um arquivo aplicando deduplicação por hash+tamanho e unicidade de nome no destino.
        /// Retorna o Id do arquivo (existente reaproveitado ou novo). [EF UPLOAD 7.3]
        /// </summary>
        private async Task<Guid> ConsolidarArquivoAsync(byte[] conteudo, Guid ownerId, Guid? uploadedId, EUplOrigemUpload origem, string nomeOriginal, string extensao, string? pastaDestino, string tenant, string user, CancellationToken ct)
        {
            var hash = _armazenamento.CalcularHash(conteudo);
            long tamanho = conteudo.LongLength;

            // Deduplicação: reaproveita arquivo ativo com mesmo hash e tamanho (EF 7.3.3).
            var existente = await _context.UplArquivos
                .FirstOrDefaultAsync(a => a.HashArquivo == hash && a.TamanhoBytes == tamanho && a.Status == EUplStatusArquivo.Ativo && a.DeletadoEm == null, ct);
            if (existente != null) return existente.Id;

            var nomeSanitizado = _armazenamento.SanitizarNome(nomeOriginal);
            // Torna o nome original único quando já existir arquivo ativo com mesmo nome (EF 7.3.4).
            var nomeConflita = await _context.UplArquivos.AnyAsync(a => a.NomeOriginal == nomeSanitizado && a.Status == EUplStatusArquivo.Ativo && a.DeletadoEm == null, ct);
            if (nomeConflita)
            {
                var semExt = Path.GetFileNameWithoutExtension(nomeSanitizado);
                var ext = Path.GetExtension(nomeSanitizado);
                nomeSanitizado = $"{semExt}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}";
            }

            var nomeArmazenado = _armazenamento.GerarNomeArmazenado(extensao);
            await _armazenamento.SalvarAsync(conteudo, nomeArmazenado, pastaDestino, ct);

            var arquivo = new UplArquivo(ownerId, uploadedId, nomeSanitizado, nomeArmazenado, extensao?.TrimStart('.'), tamanho, hash, pastaDestino, null, origem, tenant, user);
            _context.UplArquivos.Add(arquivo);
            await _context.SaveChangesAsync(ct);
            return arquivo.Id;
        }

        public async Task<CommandResult> Handle(ImportarArquivoTabularCommand request, CancellationToken ct)
        {
            var tenant = Tenant();
            var user = User();

            var importRef = $"IMP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}".Substring(0, 40);
            var execucao = new UplExecucaoImportacao(request.UsuarioId, importRef, request.TipoImportacao, request.ArquivoId, null, request.NomeArquivo, tenant, user);
            if (!execucao.IsValid) return CommandResult.Falha(execucao.Notifications.Select(n => n.Message));
            _context.UplExecucoesImportacao.Add(execucao);
            execucao.IniciarProcessamento(user);
            await _context.SaveChangesAsync(ct);

            int sucesso = 0, ignoradas = 0, erros = 0, total = 0;
            try
            {
                var linhas = LeitorTabularService.Ler(request.Conteudo, request.Extensao);
                total = linhas.Count;

                foreach (var linha in linhas)
                {
                    // Linha totalmente vazia é ignorada sem abortar o lote (EF 7.6.12).
                    if (linha.Valores.Values.All(string.IsNullOrWhiteSpace))
                    {
                        ignoradas++;
                        _context.UplImportacaoLinhas.Add(new UplImportacaoLinha(execucao.Id, linha.NumeroLinha, EUplStatusLinha.Ignorada, request.TipoImportacao, null, null, tenant, user));
                        continue;
                    }

                    // O motor genérico registra a linha normalizada; o módulo dono aplica o CRUD específico.
                    var payload = JsonSerializer.Serialize(linha.Valores);
                    _context.UplImportacaoLinhas.Add(new UplImportacaoLinha(execucao.Id, linha.NumeroLinha, EUplStatusLinha.Importada, request.TipoImportacao, null, payload, tenant, user));
                    sucesso++;
                }
            }
            catch (Exception ex)
            {
                erros++;
                _context.UplImportacaoErros.Add(new UplImportacaoErro(execucao.Id, execucao.ReferenciaErro!, null, null, $"Falha ao processar arquivo: {ex.Message}", "tabela", tenant, user));
                execucao.RegistrarErroGeral(execucao.ReferenciaErro!, user);
                await _context.SaveChangesAsync(ct);
                return CommandResult.Falha($"Falha na importação: {ex.Message}", dados: new { execucao.Id, execucao.ImportRef });
            }

            execucao.Finalizar(total, sucesso, ignoradas, erros, user);
            await _context.SaveChangesAsync(ct);

            return CommandResult.Ok("Importação processada.", new
            {
                execucao.Id,
                execucao.ImportRef,
                Total = total,
                Sucesso = sucesso,
                Ignoradas = ignoradas,
                Erros = erros,
                Resultado = execucao.Resultado?.ToString()
            });
        }

        public async Task<CommandResult> Handle(SalvarMapeamentoImportacaoCommand request, CancellationToken ct)
        {
            var mapeamento = new UplMapeamentoImportacao(request.UsuarioId, request.TipoImportacao, request.Nome, request.MapaColunasJson, Tenant(), User());
            if (!mapeamento.IsValid) return CommandResult.Falha(mapeamento.Notifications.Select(n => n.Message));
            _context.UplMapeamentosImportacao.Add(mapeamento);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Mapeamento salvo com sucesso.", new { mapeamento.Id });
        }

        public async Task<CommandResult> Handle(DesfazerImportacaoCommand request, CancellationToken ct)
        {
            var user = User();
            var execucao = await _context.UplExecucoesImportacao.FirstOrDefaultAsync(e => e.Id == request.ExecucaoImportacaoId && e.DeletadoEm == null, ct);
            if (execucao == null) return CommandResult.Falha("Execução de importação não encontrada.");

            var linhas = await _context.UplImportacaoLinhas.Where(l => l.ExecucaoImportacaoId == execucao.Id && l.DeletadoEm == null).ToListAsync(ct);
            foreach (var l in linhas) l.Deletar(user);

            execucao.Desfazer(user);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Importação desfeita.");
        }
    }
}
