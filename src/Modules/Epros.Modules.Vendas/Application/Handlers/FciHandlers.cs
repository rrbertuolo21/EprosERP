using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Handlers
{
    /// <summary>
    /// Handlers do submódulo Faturamento Comercial Internacional (VEN-FCI).
    /// Documento COMERCIAL internacional (FCI-001), não fiscal BR. NF-08: sinais opostos fatura/nota
    /// de crédito; câmbio pela data do documento. valida-contador nas contas fixas (FCI-038).
    /// </summary>
    internal static class FciNumeracao
    {
        /// <summary>FCI-006/007/008: número exibido = prefixo do tipo + serial (fallback documentado).</summary>
        public static string MontarNumero(EFciTipoDocumento tipo, int serial)
            => $"{FciConfiguracaoDocumento.PrefixoPadrao(tipo)}-{serial:D6}";
    }

    public class CriarFciDocumentoCommandHandler : ICommandHandler<CriarFciDocumentoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarFciDocumentoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarFciDocumentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (request.TipoDocumento == EFciTipoDocumento.Cotacao)
                return CommandResult.Falha("Cotação comercial (tipo 8) não está no fluxo funcional da V1 (FCI-062).");

            // FCI-006: serial automático por tenant/tipo = maior serial existente + 1.
            var maiorSerial = await _context.FciDocumentos
                .Where(d => d.TenantId == tenantId && d.TipoDocumento == request.TipoDocumento)
                .Select(d => (int?)d.Serial).MaxAsync(cancellationToken) ?? 0;
            var serial = maiorSerial + 1;
            var numero = string.IsNullOrWhiteSpace(request.Numero) ? FciNumeracao.MontarNumero(request.TipoDocumento, serial) : request.Numero!;

            // FCI-009: duplicidade de número por tenant/tipo.
            var duplicado = await _context.FciDocumentos.AnyAsync(
                d => d.TenantId == tenantId && d.TipoDocumento == request.TipoDocumento && d.Numero == numero, cancellationToken);
            if (duplicado) return CommandResult.Falha("Já existe documento com este número para o tenant. [FCI-009]");

            var doc = new FciDocumentoComercial(
                request.TipoDocumento, serial, numero, request.DataDocumento, request.ClienteId, request.ArmazemId,
                request.AnoFinanceiroId, request.DocumentoOrigemId, request.PercentualDesconto, request.TipoDesconto,
                request.ValorFrete, request.Moeda, request.TaxaCambio, request.Incoterm, request.Referencia,
                request.Observacao, Guid.TryParse(usuario, out var uid) ? uid : (Guid?)null, tenantId, usuario);

            foreach (var input in request.Itens ?? Enumerable.Empty<FciItemInput>())
            {
                var item = new FciDocumentoItem(input.ProdutoId, input.UnidadeId, input.Quantidade, input.ValorUnitario,
                    input.LoteId, input.Desconto, input.ImpostoId, input.AliquotaImposto, input.ContaReceitaId, input.ProjetoId,
                    tenantId, usuario);
                if (!item.IsValid) return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Linha do documento inválida.");
                item.VincularDocumento(doc.Id);
                doc.AdicionarItem(item);
            }

            doc.Recalcular();
            doc.Validar();
            if (!doc.IsValid) return CommandResult.Falha(doc.Notifications.Select(n => n.Message), "Documento comercial inválido.");

            _context.FciDocumentos.Add(doc);
            _context.FciHistoricos.Add(new FciHistorico(doc.Id, EFciEntidadeTipo.Documento, EFciEvento.Criacao, null, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Documento comercial criado como rascunho.", new { doc.Id, doc.Numero, doc.TotalGeral, Status = doc.Status.ToString() });
        }
    }

    public class AprovarFciDocumentoCommandHandler : ICommandHandler<AprovarFciDocumentoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AprovarFciDocumentoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AprovarFciDocumentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var doc = await _context.FciDocumentos.Include(d => d.Itens)
                .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == request.Id, cancellationToken);
            if (doc == null) return CommandResult.Falha("Documento comercial não encontrado.");

            doc.Recalcular();
            doc.Aprovar(usuario);
            if (!doc.IsValid) return CommandResult.Falha(doc.Notifications.Select(n => n.Message), "Não foi possível aprovar o documento.");

            // FCI-015/027..037 (NF-08 sinais opostos): estoque + razão gerados na aprovação.
            _context.FciLancamentosEstoque.AddRange(doc.GerarLancamentosEstoque());
            _context.FciLancamentosRazao.AddRange(doc.GerarLancamentosRazao(usuario));
            _context.FciHistoricos.Add(new FciHistorico(doc.Id, EFciEntidadeTipo.Documento, EFciEvento.Aprovacao, null, null, null, tenantId, usuario));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Documento comercial aprovado.", new { doc.Id, Status = doc.Status.ToString(), doc.TotalGeral, doc.TotalImposto });
        }
    }

    public class EditarFciDocumentoCommandHandler : ICommandHandler<EditarFciDocumentoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public EditarFciDocumentoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(EditarFciDocumentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var doc = await _context.FciDocumentos.Include(d => d.Itens)
                .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == request.Id, cancellationToken);
            if (doc == null) return CommandResult.Falha("Documento comercial não encontrado.");
            if (doc.Status == EFciStatus.Excluida) return CommandResult.Falha("Documento excluído não pode ser editado.");

            var eraAprovada = doc.Status == EFciStatus.Aprovada || doc.Status == EFciStatus.Paga;

            // Substitui as linhas (recálculo consistente — FCI-039..043).
            var itensAntigos = _context.FciDocumentoItens.Where(i => i.TenantId == tenantId && i.DocumentoId == doc.Id);
            _context.FciDocumentoItens.RemoveRange(itensAntigos);
            doc.LimparItens();

            foreach (var input in request.Itens ?? Enumerable.Empty<FciItemInput>())
            {
                var item = new FciDocumentoItem(input.ProdutoId, input.UnidadeId, input.Quantidade, input.ValorUnitario,
                    input.LoteId, input.Desconto, input.ImpostoId, input.AliquotaImposto, input.ContaReceitaId, input.ProjetoId,
                    tenantId, usuario);
                if (!item.IsValid) return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Linha do documento inválida.");
                item.VincularDocumento(doc.Id);
                doc.AdicionarItem(item);
            }

            doc.AlterarCabecalho(request.DataDocumento, request.PercentualDesconto, request.TipoDesconto, request.ValorFrete,
                request.Moeda, request.TaxaCambio, request.Incoterm, request.Referencia, request.Observacao, usuario);
            doc.Recalcular();
            doc.Validar();
            if (!doc.IsValid) return CommandResult.Falha(doc.Notifications.Select(n => n.Message), "Documento comercial inválido.");

            if (eraAprovada)
            {
                // FCI-039/040: remove razão/estoque anteriores e recria com os valores atuais.
                var razaoAntiga = _context.FciLancamentosRazao.Where(r => r.TenantId == tenantId && r.DocumentoId == doc.Id);
                var estoqueAntigo = _context.FciLancamentosEstoque.Where(e => e.TenantId == tenantId && e.DocumentoId == doc.Id);
                _context.FciLancamentosRazao.RemoveRange(razaoAntiga);
                _context.FciLancamentosEstoque.RemoveRange(estoqueAntigo);
                _context.FciLancamentosEstoque.AddRange(doc.GerarLancamentosEstoque());
                _context.FciLancamentosRazao.AddRange(doc.GerarLancamentosRazao(usuario));
            }

            _context.FciHistoricos.Add(new FciHistorico(doc.Id, EFciEntidadeTipo.Documento, EFciEvento.Edicao, null, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Documento comercial atualizado.", new { doc.Id, doc.TotalGeral, Status = doc.Status.ToString() });
        }
    }

    public class RegistrarPagamentoFciDocumentoCommandHandler : ICommandHandler<RegistrarPagamentoFciDocumentoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarPagamentoFciDocumentoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarPagamentoFciDocumentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var doc = await _context.FciDocumentos.FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == request.Id, cancellationToken);
            if (doc == null) return CommandResult.Falha("Documento comercial não encontrado.");
            doc.RegistrarPagamento(request.ValorPago, usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Pagamento registrado.", new { doc.Id, doc.SaldoEmAberto, Status = doc.Status.ToString() });
        }
    }

    public class ExcluirFciDocumentoCommandHandler : ICommandHandler<ExcluirFciDocumentoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ExcluirFciDocumentoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ExcluirFciDocumentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var doc = await _context.FciDocumentos.FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == request.Id, cancellationToken);
            if (doc == null) return CommandResult.Falha("Documento comercial não encontrado.");

            // FCI-047/048: remove efeitos vinculados (razão/estoque) e marca documento excluído.
            var razao = _context.FciLancamentosRazao.Where(r => r.TenantId == tenantId && r.DocumentoId == doc.Id);
            var estoque = _context.FciLancamentosEstoque.Where(e => e.TenantId == tenantId && e.DocumentoId == doc.Id);
            _context.FciLancamentosRazao.RemoveRange(razao);
            _context.FciLancamentosEstoque.RemoveRange(estoque);
            doc.MarcarExcluido(usuario);
            _context.FciHistoricos.Add(new FciHistorico(doc.Id, EFciEntidadeTipo.Documento, EFciEvento.Exclusao, null, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Documento comercial excluído.", new { doc.Id });
        }
    }

    // ----- Impostos comerciais -----
    public class CriarFciImpostoCommandHandler : ICommandHandler<CriarFciImpostoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        public CriarFciImpostoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(CriarFciImpostoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            // FCI-050: nome duplicado no tenant é rejeitado.
            if (await _context.FciImpostos.AnyAsync(i => i.TenantId == tenantId && i.Nome == request.Nome, cancellationToken))
                return CommandResult.Falha("Já existe imposto com este nome no tenant. [FCI-050]");
            var imposto = new FciImposto(request.Nome, request.Aliquota, request.Ativo, tenantId, usuario);
            if (!imposto.IsValid) return CommandResult.Falha(imposto.Notifications.Select(n => n.Message), "Imposto inválido.");
            _context.FciImpostos.Add(imposto);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Imposto comercial criado.", new { imposto.Id });
        }
    }

    public class AtualizarFciImpostoCommandHandler : ICommandHandler<AtualizarFciImpostoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        public AtualizarFciImpostoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AtualizarFciImpostoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var imposto = await _context.FciImpostos.FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Id == request.Id, cancellationToken);
            if (imposto == null) return CommandResult.Falha("Imposto não encontrado.");
            // FCI-051: permite o mesmo nome quando o registro é o próprio.
            if (await _context.FciImpostos.AnyAsync(i => i.TenantId == tenantId && i.Nome == request.Nome && i.Id != request.Id, cancellationToken))
                return CommandResult.Falha("Já existe outro imposto com este nome no tenant. [FCI-050/051]");
            imposto.Alterar(request.Nome, request.Aliquota, request.Ativo, usuario);
            if (!imposto.IsValid) return CommandResult.Falha(imposto.Notifications.Select(n => n.Message), "Imposto inválido.");
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Imposto comercial atualizado.", new { imposto.Id });
        }
    }

    public class ExcluirFciImpostoCommandHandler : ICommandHandler<ExcluirFciImpostoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        public ExcluirFciImpostoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(ExcluirFciImpostoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var imposto = await _context.FciImpostos.FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Id == request.Id, cancellationToken);
            if (imposto == null) return CommandResult.Falha("Imposto não encontrado.");
            // FCI-052: exclusão rejeitada quando há associação impeditiva com documentos.
            if (await _context.FciDocumentoItens.AnyAsync(l => l.TenantId == tenantId && l.ImpostoId == imposto.Id, cancellationToken))
                return CommandResult.Falha("Imposto associado a documentos não pode ser excluído. [FCI-052]");
            imposto.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Imposto comercial excluído.", new { imposto.Id });
        }
    }

    // ----- Preferências gerais (singleton por tenant) -----
    public class SalvarFciPreferenciaCommandHandler : ICommandHandler<SalvarFciPreferenciaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        public SalvarFciPreferenciaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(SalvarFciPreferenciaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var pref = await _context.FciPreferencias.FirstOrDefaultAsync(p => p.TenantId == tenantId, cancellationToken);
            if (pref == null)
            {
                pref = new FciPreferenciaGeral(tenantId, usuario); // FCI-054.
                _context.FciPreferencias.Add(pref);
            }
            pref.Alterar(request.MostrarMoeda, request.PermitirCaixaNegativo, request.PermitirEstoqueNegativo,
                request.ModoCalculoEstoque, request.ControlarLimiteCredito, request.PermitirDesconto,
                request.ImpostoNaCompra, request.ImpostoNaVenda, usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Preferências salvas.", new { pref.Id });
        }
    }
}
