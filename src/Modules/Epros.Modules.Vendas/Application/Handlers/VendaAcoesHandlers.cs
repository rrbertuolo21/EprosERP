using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Application.Models;
using Epros.Modules.Vendas.Application.Services;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Handlers
{
    // ============================================================================
    // Handlers das AÇÕES de venda (Onda 7 - Vendas-Acoes). Auto-registrados por
    // assembly scan (MediatR). Reutilizam VendaStatusMapper (VendaFiscalHandlers.cs).
    // ============================================================================

    // ---------- Baixar cupom não fiscal (MEI) ----------

    public class BaixarCupomNaoFiscalCommandHandler : ICommandHandler<BaixarCupomNaoFiscalCommand>
    {
        private readonly ContextVendas _context;

        public BaixarCupomNaoFiscalCommandHandler(ContextVendas context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(BaixarCupomNaoFiscalCommand r, CancellationToken ct)
        {
            var venda = await _context.Vendas
                .AsNoTracking()
                .Include(v => v.Emitente)
                .FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);

            if (venda == null) return CommandResult.Falha("Id da venda não encontrado.");

            // Porte fiel: só MEI + Status SalvarImprimirMei geram/baixam o cupom não fiscal.
            if (venda.Status != EVendaStatus.SalvarImprimirMei
                || venda.Emitente?.RegimeTributario != ERegimeTributario.SimplesNacionalMei)
            {
                return CommandResult.Falha("Cupom não pode ser gerado. O Status da transmissão e o Regime da empresa devem estar registrados como MEI.");
            }

            if (string.IsNullOrEmpty(venda.CaminhoPdfCupomNaoFiscal))
                return CommandResult.Falha("Caminho do Cupom não cadastrado na venda.");

            if (!File.Exists(venda.CaminhoPdfCupomNaoFiscal))
                return CommandResult.Falha("Arquivo do Cupom não localizado no armazenamento.");

            var bytes = await File.ReadAllBytesAsync(venda.CaminhoPdfCupomNaoFiscal, ct);
            var nomeArquivo = Path.GetFileName(venda.CaminhoPdfCupomNaoFiscal);
            return CommandResult.Ok("Cupom não fiscal recuperado com sucesso.", new ArquivoPdfResult(bytes, nomeArquivo));
        }
    }

    // ---------- Duplicar venda ----------

    public class DuplicarVendaCommandHandler : ICommandHandler<DuplicarVendaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ICurrentUser _user;

        public DuplicarVendaCommandHandler(ContextVendas context, ICurrentUser user)
        {
            _context = context;
            _user = user;
        }

        public async Task<CommandResult> Handle(DuplicarVendaCommand r, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";

            // Carrega o agregado completo para clonar fielmente (porte de ObterVendaCompletaPorId + DuplicarVenda).
            var venda = await _context.Vendas
                .Include(v => v.Emitente).ThenInclude(e => e!.Endereco)
                .Include(v => v.Destinatario).ThenInclude(d => d!.Enderecos)
                .Include(v => v.Transporte).ThenInclude(t => t!.Transportadora)
                .Include(v => v.Transporte).ThenInclude(t => t!.Veiculo)
                .Include(v => v.Transporte).ThenInclude(t => t!.Volumes)
                .Include(v => v.Transporte).ThenInclude(t => t!.Reboques)
                .Include(v => v.Total_)
                .Include(v => v.TotalIbsCbs)
                .Include(v => v.Configuracao)
                .Include(v => v.Imposto)
                .Include(v => v.Fatura).ThenInclude(f => f!.Duplicatas)
                .Include(v => v.Pagamentos)
                .Include(v => v.AutorizacoesXml)
                .Include(v => v.Referenciadas)
                .Include(v => v.Itens).ThenInclude(i => i.Imposto)
                .Include(v => v.Itens).ThenInclude(i => i.ImpostoIbsCbs).ThenInclude(ic => ic!.VendaItemImpostoIbsCbsTributacaoRegular)
                .Include(v => v.Itens).ThenInclude(i => i.ImpostoValorAproximado)
                .Include(v => v.Itens).ThenInclude(i => i.Combustivel).ThenInclude(c => c!.Origens)
                .FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);

            if (venda == null) return CommandResult.Falha("Venda não localizada para duplicar.");

            var nova = venda.DuplicarVenda(userId);
            if (!nova.IsValid) return CommandResult.Falha(nova.Notifications.Select(n => n.Message));

            _context.Vendas.Add(nova);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Venda duplicada com sucesso.", new { nova.Id });
        }
    }

    // ---------- Gerar DANFE sem autorização (pré-visualização) ----------

    public class GerarDanfeSemAutorizacaoCommandHandler : ICommandHandler<GerarDanfeSemAutorizacaoCommand>
    {
        private readonly ContextVendas _context;
        private readonly IDanfeVendaService _danfeVendaService;

        public GerarDanfeSemAutorizacaoCommandHandler(ContextVendas context, IDanfeVendaService danfeVendaService)
        {
            _context = context;
            _danfeVendaService = danfeVendaService;
        }

        public async Task<CommandResult> Handle(GerarDanfeSemAutorizacaoCommand r, CancellationToken ct)
        {
            var venda = await _context.Vendas
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);

            if (venda == null) return CommandResult.Falha("Venda não encontrada.");
            if (venda.DocumentoFiscalId is not { } documentoFiscalId)
                return CommandResult.Falha("DANFE indisponível: a venda ainda não possui documento fiscal vinculado.");

            var pdf = await _danfeVendaService.GerarPreviewPorDocumentoFiscalIdAsync(documentoFiscalId, ct);
            if (pdf is null)
                return CommandResult.Falha("DANFE indisponível: serviço de geração do documento fiscal não configurado.");

            return CommandResult.Ok("DANFE (pré-visualização) gerado com sucesso.",
                new ArquivoPdfResult(pdf.Conteudo, pdf.NomeArquivo));
        }
    }

    // ---------- NF-e simplificado ----------

    public class CriarVendaSimplificadaNfeCommandHandler : ICommandHandler<CriarVendaSimplificadaNfeCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public CriarVendaSimplificadaNfeCommandHandler(ContextVendas context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(CriarVendaSimplificadaNfeCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";

            var venda = new Venda(
                id: Guid.NewGuid(),
                syncId: Guid.NewGuid(),
                caixaId: r.CaixaId,
                total: r.Total,
                status: VendaStatusMapper.Mapear(r.Status),
                tenantId: tenantId,
                criadoPor: userId,
                criadoEm: r.DataVenda == default ? DateTime.UtcNow : r.DataVenda,
                modeloFiscal: EModeloDocumento.NFe,
                naturezaOperacao: r.NaturezaOperacao,
                informacoesComplementares: r.InformacoesComplementares,
                informacoesAdicionaisFisco: r.InformacoesAdicionaisFisco,
                modalidadeFrete: r.ModalidadeFrete,
                vendaOrigem: EVendaOrigem.NfeSimplificada,
                incluirFreteNoTotal: r.IncluirFreteNoTotal,
                clienteId: r.ClienteId,
                valorDesconto: r.ValorDesconto,
                valorFrete: r.ValorFrete,
                formaPagamento: r.FormaPagamento);

            if (!venda.IsValid) return CommandResult.Falha(venda.Notifications.Select(n => n.Message));

            _context.Vendas.Add(venda);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Venda NF-e simplificada criada com sucesso.", new { venda.Id });
        }
    }

    public class AtualizarVendaSimplificadaNfeCommandHandler : ICommandHandler<AtualizarVendaSimplificadaNfeCommand>
    {
        private readonly ContextVendas _context;
        private readonly ICurrentUser _user;

        public AtualizarVendaSimplificadaNfeCommandHandler(ContextVendas context, ICurrentUser user)
        {
            _context = context;
            _user = user;
        }

        public async Task<CommandResult> Handle(AtualizarVendaSimplificadaNfeCommand r, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            var venda = await _context.Vendas.FirstOrDefaultAsync(v => v.Id == r.Id && v.DeletadoEm == null, ct);
            if (venda == null) return CommandResult.Falha($"Venda não localizada: {r.Id}");

            venda.Alterar(EModeloDocumento.NFe, r.NaturezaOperacao, r.DataVenda, r.InformacoesComplementares,
                r.InformacoesAdicionaisFisco, r.ModalidadeFrete, r.ValorDesconto, r.ValorFrete, userId);
            if (!venda.IsValid) return CommandResult.Falha(venda.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Venda NF-e simplificada atualizada com sucesso.", new { venda.Id });
        }
    }

    public class TransmitirVendaSimplificadaNfeCommandHandler : ICommandHandler<TransmitirVendaSimplificadaNfeCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public TransmitirVendaSimplificadaNfeCommandHandler(ContextVendas context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(TransmitirVendaSimplificadaNfeCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";

            var venda = await _context.Vendas
                .Include(v => v.Nfe)
                .Include(v => v.Itens)
                .Include(v => v.Pagamentos)
                .FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda == null) return CommandResult.Falha($"Venda não localizada: {r.VendaId}");

            // Porte fiel de nfe-simplificado-transmitir: inclui a NF-e (série/número) e marca Transmitido.
            // A comunicação/autorização SEFAZ é do motor fiscal (fora deste módulo).
            if (venda.Nfe == null)
            {
                var nfe = new VendaNfe(venda.Id, r.Numero, r.Serie, r.DataHoraSaida, tenantId, userId);
                venda.DefinirNfe(nfe);
                // A NF-e é uma entidade NOVA de um agregado JÁ persistido: como a PK (GUID) é
                // client-generated (ValueGeneratedNever), a fixação por navegação a marca como Modified
                // (UPDATE em linha inexistente → DbUpdateConcurrencyException). Força o estado Added.
                _context.Entry(nfe).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            }
            else
            {
                venda.Nfe.AtualizarNumeroSerie(r.Serie, r.Numero, r.DataHoraSaida, userId);
            }

            venda.AtualizarStatus(EVendaStatus.Transmitido, userId);
            venda.AtualizarDataUltimoProcessamento();
            if (!venda.IsValid) return CommandResult.Falha(venda.Notifications.Select(n => n.Message));

            // T3 — LIGA OS EFEITOS REAIS do faturamento no PDV (antes esta transmissão fiscal-interativa
            // não creditava caixa, nem gerava financeiro, nem baixava estoque): credita CaixaMovimento por
            // forma de pagamento e enfileira VendaFaturada no Outbox (→ Financeiro + Estoque pelo motor único).
            // Idempotente e transacional (mesmo SaveChanges da venda).
            await new EfeitosFaturamentoPdvService(_context).AplicarAsync(venda, tenantId, userId, ct);

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("NF-e simplificada marcada para transmissão com sucesso.", new { venda.Id });
        }
    }
}
