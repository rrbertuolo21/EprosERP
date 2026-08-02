using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Modules.Financeiro.Domain.Services.Cnab;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Handlers
{
    // ===== CNAB: geração de remessa =====
    public class GerarArquivoRemessaCommandHandler : IRequestHandler<GerarArquivoRemessaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ITenantProvider _tenant; private readonly ICurrentUser _user;
        public GerarArquivoRemessaCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(GerarArquivoRemessaCommand r, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";

            var conta = await _context.ContasEmissoras.FirstOrDefaultAsync(c => c.Id == r.ContaEmissoraId, ct);
            if (conta == null) return CommandResult.Falha("Conta emissora não encontrada.");

            var banco = await _context.Bancos.FirstOrDefaultAsync(b => b.Id == conta.BancoId, ct);

            ConfiguracaoCedente? cedente = null;
            if (conta.ConfiguracaoCedenteId.HasValue)
                cedente = await _context.ConfiguracoesCedente.FirstOrDefaultAsync(c => c.Id == conta.ConfiguracaoCedenteId.Value, ct);

            // Boletos emitidos nesta conta emissora, de faturas elegíveis à remessa (Pendente, não remetida).
            var boletos = await _context.Boletos
                .Where(b => b.ContaEmissoraId == conta.Id)
                .Join(_context.FaturasCobranca, b => b.FaturaCobrancaId, f => f.Id, (b, f) => new { b, f })
                .Where(x => !x.f.Remetida && x.f.Situacao == ESituacaoFaturaCobranca.Pendente)
                .ToListAsync(ct);

            if (r.FaturaIds is { Count: > 0 })
            {
                var set = r.FaturaIds.ToHashSet();
                boletos = boletos.Where(x => set.Contains(x.f.Id)).ToList();
            }

            if (boletos.Count == 0) return CommandResult.Falha("Nenhuma fatura elegível para remessa nesta conta emissora.");

            // Resolve sacados para os detalhes (dados do sacado no arquivo).
            var sacadoIds = boletos.Select(x => x.f.SacadoId).Distinct().ToList();
            var sacados = await _context.Sacados.Where(s => sacadoIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, ct);

            var dadosCedente = new DadosCedenteCnab(
                CodigoBanco: banco?.Codigo ?? "000",
                NomeBanco: banco?.Descricao ?? conta.NomeBanco ?? string.Empty,
                NomeCedente: cedente?.Nome ?? "CEDENTE",
                DocumentoCedente: cedente?.Documento ?? string.Empty,
                Agencia: conta.Agencia ?? string.Empty,
                DigitoAgencia: conta.DigitoAgencia ?? string.Empty,
                Conta: conta.Conta ?? string.Empty,
                DigitoConta: conta.DigitoConta ?? string.Empty,
                Carteira: conta.Carteira ?? string.Empty,
                Convenio: conta.Convenio ?? string.Empty);

            var titulos = boletos.Select(x =>
            {
                sacados.TryGetValue(x.f.SacadoId, out var s);
                return new TituloRemessaCnab(
                    NossoNumero: x.b.NossoNumero,
                    NumeroDocumento: x.b.NumeroDocumento ?? x.f.NumeroDocumento ?? x.f.Id.ToString("N").Substring(0, 10),
                    Valor: x.b.Valor,
                    DataVencimento: x.b.DataVencimento,
                    DataEmissao: x.f.Data,
                    SacadoNome: s?.Nome ?? "SACADO",
                    SacadoDocumento: s?.Documento ?? string.Empty,
                    SacadoEndereco: s?.Endereco ?? string.Empty,
                    SacadoBairro: s?.Bairro ?? string.Empty,
                    SacadoCidade: s?.Cidade ?? string.Empty,
                    SacadoUF: s?.UF ?? string.Empty,
                    SacadoCep: s?.CEP ?? string.Empty);
            }).ToList();

            var sequencial = await _context.Remessas.IgnoreQueryFilters()
                .CountAsync(x => x.TenantId == _tenant.GetTenantId(), ct) + 1;
            var dataGeracao = DateTime.UtcNow;

            var conteudo = r.Layout == ELayoutCnab.Cnab240
                ? CnabWriter.GerarRemessa240(dadosCedente, titulos, sequencial, dataGeracao)
                : CnabWriter.GerarRemessa400(dadosCedente, titulos, sequencial, dataGeracao);

            var nomeArquivo = r.NomeArquivo ?? $"REMESSA_{dataGeracao:yyyyMMddHHmmss}_{(int)r.Layout}.REM";
            var remessa = new Remessa(nomeArquivo, dataGeracao, r.Grupo, r.Layout, conta.Id, _tenant.GetTenantId(), userId);

            foreach (var x in boletos)
            {
                remessa.AdicionarBoleto(x.b.Id, x.f.Id, x.b.Valor, userId); // RSF-036 idempotência
                x.f.MarcarRemetida(userId); // RSF-014
            }
            remessa.DefinirConteudo(conteudo, userId);
            if (!remessa.IsValid) return CommandResult.Falha(remessa.Notifications.Select(n => n.Message));

            _context.Remessas.Add(remessa);
            await _context.SaveChangesAsync(ct);

            return CommandResult.Ok("Arquivo de remessa CNAB gerado.", new
            {
                remessa.Id, remessa.NomeArquivo, remessa.Layout, remessa.QuantidadeTitulos, remessa.ValorTotal, Conteudo = conteudo
            });
        }
    }

    // ===== CNAB: processamento de retorno =====
    public class ProcessarRetornoBancarioCommandHandler : IRequestHandler<ProcessarRetornoBancarioCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ITenantProvider _tenant; private readonly ICurrentUser _user;
        public ProcessarRetornoBancarioCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(ProcessarRetornoBancarioCommand r, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            if (string.IsNullOrWhiteSpace(r.Conteudo)) return CommandResult.Falha("Arquivo de retorno vazio."); // RSF-008

            RetornoCnab retorno;
            try
            {
                retorno = CnabRetornoParser.ParsearRetorno(r.Conteudo); // detecta layout (RSF-009/010)
            }
            catch (ArgumentException ex)
            {
                return CommandResult.Falha(ex.Message); // RSF-008 (vazio/ilegível/layout desconhecido)
            }

            int baixadas = 0, rejeitadas = 0;
            decimal totalBaixado = 0m;
            var detalhes = new List<object>();

            foreach (var oc in retorno.Ocorrencias)
            {
                if (!oc.Liquidado) { detalhes.Add(new { oc.NossoNumero, oc.CodigoOcorrencia, status = "ignorada" }); continue; }

                // RSF-007: localiza fatura por nosso número.
                var fatura = await _context.FaturasCobranca
                    .FirstOrDefaultAsync(f => f.NossoNumero == oc.NossoNumero, ct);

                if (fatura == null)
                {
                    rejeitadas++;
                    detalhes.Add(new { oc.NossoNumero, oc.CodigoOcorrencia, status = "nao_localizada" });
                    continue;
                }
                if (fatura.Situacao == ESituacaoFaturaCobranca.Baixada)
                {
                    detalhes.Add(new { oc.NossoNumero, oc.CodigoOcorrencia, status = "ja_baixada" });
                    continue;
                }

                var valorPago = oc.ValorPago > 0m ? oc.ValorPago : oc.ValorTitulo;
                fatura.Baixar(oc.DataOcorrencia ?? DateTime.UtcNow, valorPago, userId); // RSF-006/007
                if (fatura.IsValid)
                {
                    baixadas++;
                    totalBaixado += valorPago;
                    detalhes.Add(new { oc.NossoNumero, oc.CodigoOcorrencia, valorPago, status = "baixada" });
                }
                else
                {
                    rejeitadas++;
                    detalhes.Add(new { oc.NossoNumero, oc.CodigoOcorrencia, status = "erro_baixa" });
                }
            }

            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(r.Conteudo)));
            var audit = new RetornoBancario(
                r.NomeArquivo ?? $"RETORNO_{DateTime.UtcNow:yyyyMMddHHmmss}",
                retorno.Layout, hash, retorno.Ocorrencias.Count, baixadas, rejeitadas, totalBaixado,
                _tenant.GetTenantId(), userId);
            _context.RetornosBancarios.Add(audit);

            await _context.SaveChangesAsync(ct);

            return CommandResult.Ok("Retorno bancário processado.", new
            {
                layout = retorno.Layout.ToString(),
                ocorrencias = retorno.Ocorrencias.Count,
                baixadas, rejeitadas, totalBaixado, retornoId = audit.Id, detalhes
            });
        }
    }
}
