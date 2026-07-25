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
    // ============================================================================
    // Handlers do agregado fiscal Venda. Auto-registrados por assembly scan (MediatR).
    // ============================================================================

    /// <summary>
    /// Tradução do status textual (contrato fiscal) para o enum tipado do legado (EVendaStatus).
    /// Centraliza o mapeamento usado pelos handlers fiscais.
    /// </summary>
    internal static class VendaStatusMapper
    {
        public static EVendaStatus Mapear(string? status)
        {
            var s = (status ?? string.Empty).Trim();
            if (Enum.TryParse<EVendaStatus>(s, ignoreCase: true, out var parsed))
                return parsed;

            return s.ToLowerInvariant() switch
            {
                "emitida" or "contingencia" or "contingência" or "transmitido" or "transmitida" => EVendaStatus.Transmitido,
                "errado" or "rejeitada" or "rejeitado" => EVendaStatus.Errado,
                _ => EVendaStatus.Salvar,
            };
        }
    }

    // ---------- Criar / Atualizar / Deletar Venda ----------

    public class CriarVendaFiscalCommandHandler : ICommandHandler<CriarVendaFiscalCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public CriarVendaFiscalCommandHandler(ContextVendas context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarVendaFiscalCommand r, CancellationToken ct)
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
                criadoEm: DateTime.UtcNow,
                modeloFiscal: r.ModeloFiscal,
                naturezaOperacao: r.NaturezaOperacao,
                informacoesComplementares: r.InformacoesComplementares,
                informacoesAdicionaisFisco: r.InformacoesAdicionaisFisco,
                modalidadeFrete: r.ModalidadeFrete,
                vendaOrigem: r.VendaOrigem,
                incluirFreteNoTotal: r.IncluirFreteNoTotal,
                clienteId: r.ClienteId,
                valorDesconto: r.ValorDesconto,
                valorFrete: r.ValorFrete,
                formaPagamento: r.FormaPagamento);

            if (!venda.IsValid) return CommandResult.Falha(venda.Notifications.Select(n => n.Message));

            _context.Vendas.Add(venda);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Venda fiscal criada com sucesso.", new { venda.Id });
        }
    }

    public class AtualizarVendaFiscalCommandHandler : ICommandHandler<AtualizarVendaFiscalCommand>
    {
        private readonly ContextVendas _context;
        private readonly ICurrentUser _user;

        public AtualizarVendaFiscalCommandHandler(ContextVendas context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(AtualizarVendaFiscalCommand r, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            var venda = await _context.Vendas.FirstOrDefaultAsync(v => v.Id == r.Id && v.DeletadoEm == null, ct);
            if (venda == null) return CommandResult.Falha("Venda não encontrada.");

            venda.Alterar(r.ModeloFiscal, r.NaturezaOperacao, r.DataVenda, r.InformacoesComplementares, r.InformacoesAdicionaisFisco, r.ModalidadeFrete, r.ValorDesconto, r.ValorFrete, userId);
            if (!venda.IsValid) return CommandResult.Falha(venda.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Venda atualizada com sucesso.", new { venda.Id });
        }
    }

    public class DeletarVendaFiscalCommandHandler : ICommandHandler<DeletarVendaFiscalCommand>
    {
        private readonly ContextVendas _context;
        private readonly ICurrentUser _user;

        public DeletarVendaFiscalCommandHandler(ContextVendas context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(DeletarVendaFiscalCommand r, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            var venda = await _context.Vendas
                .Include(v => v.Itens)
                .Include(v => v.Pagamentos)
                .Include(v => v.Fatura).ThenInclude(f => f!.Duplicatas)
                .Include(v => v.Emitente)
                .Include(v => v.Destinatario)
                .Include(v => v.Transporte).ThenInclude(t => t!.Volumes)
                .Include(v => v.Transporte).ThenInclude(t => t!.Reboques)
                .Include(v => v.Transporte).ThenInclude(t => t!.Transportadora)
                .Include(v => v.Transporte).ThenInclude(t => t!.Veiculo)
                .Include(v => v.Total_)
                .Include(v => v.Configuracao)
                .Include(v => v.Nfce)
                .FirstOrDefaultAsync(v => v.Id == r.Id && v.DeletadoEm == null, ct);

            if (venda == null) return CommandResult.Falha("Venda não encontrada.");

            venda.DeletarEmCascata(userId);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Venda excluída com sucesso.");
        }
    }

    // ---------- Itens ----------

    public class AdicionarItemFiscalCommandHandler : ICommandHandler<AdicionarItemFiscalCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public AdicionarItemFiscalCommandHandler(ContextVendas context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(AdicionarItemFiscalCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";

            var venda = await _context.Vendas.FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda == null) return CommandResult.Falha("Venda não encontrada.");

            var item = new VendaItem(
                r.VendaId, r.ProdutoId, r.CodigoProduto, r.CodigoEan, r.DescricaoProduto, r.Ncm, r.ExcecaoNcmTipi,
                r.CestId, r.Cest, r.CodigoAnpId, r.CodigoAnp, r.Cfop, r.UnidadeComercial, r.QuantidadeComercial,
                r.ValorUnitarioComercial, r.ValorTotalBrutoProdutos, r.CodigoEanTributavel, r.UnidadeTributavel,
                r.QuantidadeTributavel, r.ValorUnitarioTributavel, r.ValorDesconto, r.ValorDescontoRateado,
                r.ValorFreteRateado, r.ValorSeguroRateado, r.ValorOutrasDepesasAcessoriasRateado, r.CompoeValorTotal,
                r.InformacoesAdicionaisDoProduto, r.CfopCorrelacao, r.IntegraFaturamento, r.NumeroItemPedidoCompra,
                r.NumeroPedidoCompra, r.FichaConteudoImportacao, r.CodigoBeneficioFiscal, r.ValorCusto, tenantId, userId);

            var imp = r.Imposto;
            var imposto = new VendaItemImposto(
                item.Id, imp.Origem, imp.CstIcms, imp.Csosn, imp.ModalidadeDeterminacaoBaseCalculoIcms, imp.ValorBaseDeCalculoIcms,
                imp.PercentualReducaoBaseDeCalculoIcms, imp.AliquotaIcms, imp.ValorImpostoIcms, imp.ModalidadeBaseDeCalculosST,
                imp.PercentualMvaBaseDeCalculoST, imp.PercentualReducaoBaseDeCalculoST, imp.ValorBaseDeCalculoSt, imp.AliquotaSt,
                imp.ValorImpostoSt, imp.MotivoDesoneracaoIcms, imp.ValorBaseDeCalculoStRetido, imp.ValorImpostoStRetido,
                imp.PercentualCreditoSimplesNacionalIcms, imp.ValorImpostoCreditoSimplesNacionalIcms, imp.ValorBaseDeCalculoFcp,
                imp.PercentualFcp, imp.ValorImpostoFcp, imp.ValorOperacaoDiferimentoIcms, imp.PercentualDiferimentoIcms,
                imp.ValorImpostoDiferimentoIcms, imp.CstIpiSaida, imp.ValorBaseDeCalculoIpi, imp.AliquotaIpi,
                imp.ValorImpostoDiferimentoIpi, imp.ValorQuantidadeTotalParaTributacaoIpi, imp.ValorPorUnidadeTributavelIpi,
                imp.CstPis, imp.ValorBaseDeCalculoPis, imp.AliquotaPis, imp.ValorQuantidadeVendidaProdutoPis,
                imp.AliquotaPorUnidadeVendidaPis, imp.ValorImpostoDiferimentoPis, imp.CstCofins, imp.ValorBaseDeCalculoCofins,
                imp.AliquotaCofins, imp.ValorQuantidadeVendidaProdutoCofins, imp.AliquotaPorUnidadeVendidaCofins,
                imp.ValorImpostoDiferimentoCofins, imp.TipoReducaoIcms, imp.TipoReducaoIcmsSt, imp.ValorBaseDeCalculoFcpSt,
                imp.PercentualFcpSt, imp.ValorImpostoFcpSt, imp.ValorIcmsProprioSubistituto, imp.ValorAliquotaIcmsInterna,
                imp.ValorAliquotaIcmsInternaEstadual, imp.EnquadramentoIpi, imp.ValorReducaoIpiPercentual, imp.IpiEmbutido,
                imp.DifalTipoCalculoPorDentro, imp.TipoReducaoIpi, imp.TipoCalculoBaseIcmsSt, imp.ValorUnitFixadoIcmsSt,
                imp.ValorBaseDeCalculoDifal, imp.ValorImpostoDevidoDifal, imp.ValorImpostoDevidoRecolherSt, imp.ValorImpostoDevidoFcp,
                imp.ValorIcmsIsento, imp.ValorIcmsOutros, imp.IcmsObservacao, imp.ValorIpiIsento, imp.ValorIpiOutros,
                imp.IpiObservacao, tenantId, userId);
            item.DefinirImposto(imposto);

            if (r.ImpostoIbsCbs is { } ibs)
            {
                var impostoIbsCbs = new VendaItemImpostoIbsCbs(
                    item.Id, ibs.Cst, ibs.CClassTrib, ibs.AliquotaEstadual, ibs.AliquotaMunicipal, ibs.AliquotaCbs,
                    ibs.AliquotaEstadualReducao, ibs.AliquotaMunicipalReducao, ibs.AliquotaCbsReducao, ibs.AliquotaEstadualDiferimento,
                    ibs.AliquotaMunicipalDiferimento, ibs.AliquotaCbsDiferimento, ibs.AliquotaEfetivaEstadual, ibs.AliquotaEfetivaMunicipal,
                    ibs.AliquotaEfetivaCbs, ibs.ValorBaseDeCalculo, ibs.ValorImpostoDevidoEstadual, ibs.ValorImpostoDevidoMunicipal,
                    ibs.ValorImpostoDevidoCbs, null, tenantId, userId);

                if (ibs.TributacaoRegular is { } tr)
                {
                    var tribReg = new VendaItemImpostoIbsCbsTributacaoRegular(
                        impostoIbsCbs.Id, tr.Cst, tr.CClassTrib, tr.AliquotaEfetivaIbsEstadual, tr.ValorIbsEstadual,
                        tr.AliquotaEfetivaIbsMunicipal, tr.ValorIbsMunicipal, tr.AliquotaEfetivaCbs, tr.ValorCbs, tenantId, userId);
                    impostoIbsCbs.DefinirTributacaoRegular(tribReg);
                }

                item.DefinirImpostoIbsCbs(impostoIbsCbs);
            }

            // item.DefinirImposto agrega as notificações do imposto ao próprio item.
            if (!item.IsValid) return CommandResult.Falha(item.Notifications.Select(n => n.Message));

            // Persiste via AGREGADO (porte fiel de Venda.AdicionarItem): a coleção 1:N fica no agregado
            // raiz e o EF cascateia p/ imposto/IBS-CBS/tributação regular a partir de Venda.
            venda.AdicionarItem(item);
            if (!venda.IsValid) return CommandResult.Falha(venda.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Item adicionado com sucesso.", new { item.Id });
        }
    }

    public class DeletarItemFiscalCommandHandler : ICommandHandler<DeletarItemFiscalCommand>
    {
        private readonly ContextVendas _context;
        private readonly ICurrentUser _user;

        public DeletarItemFiscalCommandHandler(ContextVendas context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(DeletarItemFiscalCommand r, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            var venda = await _context.Vendas.Include(v => v.Itens).FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda == null) return CommandResult.Falha("Venda não encontrada.");

            venda.DeletarItem(r.ItemId, userId);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Item excluído com sucesso.");
        }
    }

    // ---------- Emitente / Destinatário ----------

    public class DefinirEmitenteCommandHandler : ICommandHandler<DefinirEmitenteCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public DefinirEmitenteCommandHandler(ContextVendas context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(DefinirEmitenteCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";

            var venda = await _context.Vendas.Include(v => v.Emitente).ThenInclude(e => e!.Endereco)
                .FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda == null) return CommandResult.Falha("Venda não encontrada.");

            var e = r.Endereco;
            var emitente = new VendaEmitente(r.VendaId, r.EmpresaId, r.Cnpj, r.Cpf, r.RazaoSocial, r.NomeFantasia, r.Telefone,
                r.InscricaoEstadual, r.InscricaoEstadualST, r.InscricaoMunicipal, r.Cnae, r.RegimeTributario, null, tenantId, userId);
            var endereco = new VendaEmitenteEndereco(emitente.Id, e.Uf, e.Logradouro, e.Numero, e.Complemento, e.Bairro,
                e.MunicipioId, e.MunicipioNome, e.Cep, tenantId, userId, e.PaisId, e.PaisNome);

            if (!emitente.IsValid || !endereco.IsValid)
                return CommandResult.Falha(emitente.Notifications.Select(n => n.Message).Concat(endereco.Notifications.Select(n => n.Message)));

            // Persiste via AGREGADO (porte fiel de Venda.IncluirEmitente/AlterarEmitente): a navegação
            // filha fica no agregado raiz e o EF persiste em cascata a partir de Venda.
            if (venda.Emitente != null) _context.VendaEmitentes.Remove(venda.Emitente);
            emitente.DefinirEndereco(endereco);
            venda.DefinirEmitente(emitente);
            if (!venda.IsValid) return CommandResult.Falha(venda.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Emitente definido com sucesso.", new { emitente.Id });
        }
    }

    public class DefinirDestinatarioCommandHandler : ICommandHandler<DefinirDestinatarioCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public DefinirDestinatarioCommandHandler(ContextVendas context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(DefinirDestinatarioCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";

            var venda = await _context.Vendas.Include(v => v.Destinatario)
                .FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda == null) return CommandResult.Falha("Venda não encontrada.");

            var destinatario = new VendaDestinatario(r.VendaId, r.PessoaId, r.Cnpj, r.Cpf, r.RazaoSocial, r.Telefone,
                r.InscricaoEstadual, r.IdentificadorEstrangeiro, r.IndicadorIE, r.Email, r.EhConsumidorFinal,
                r.DocumentoConsumidor, r.EnviarDestinatatioNaNfce, tenantId, userId);

            if (!destinatario.IsValid) return CommandResult.Falha(destinatario.Notifications.Select(n => n.Message));

            // Persiste via AGREGADO (porte fiel de Venda.IncluirDestinatario/AlterarDestinatario).
            if (venda.Destinatario != null) _context.VendaDestinatarios.Remove(venda.Destinatario);
            venda.DefinirDestinatario(destinatario);
            if (!venda.IsValid) return CommandResult.Falha(venda.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Destinatário definido com sucesso.", new { destinatario.Id });
        }
    }

    // ---------- Totais / Configuração / Imposto ----------

    public class DefinirTotalCommandHandler : ICommandHandler<DefinirTotalCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public DefinirTotalCommandHandler(ContextVendas context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(DefinirTotalCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";

            var venda = await _context.Vendas.Include(v => v.Total_).FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda == null) return CommandResult.Falha("Venda não encontrada.");

            if (venda.Total_ != null)
            {
                venda.Total_.Alterar(r.ValorBaseDeCalculoIcms, r.ValorIcms, r.ValorIcmsDesonerado, r.ValorFcp, r.ValorBaseDeCalculoSt,
                    r.ValorSt, r.ValorFcpSt, r.ValorFcpRet, r.ValorProduto, r.ValorFrete, r.ValorSeguro, r.ValorDesconto,
                    r.ValorImpostoImportacao, r.ValorIpi, r.ValorIpiDevolucao, r.ValorPis, r.ValorCofins, r.ValorOutro, r.ValorNotaFiscal, userId);
            }
            else
            {
                var total = new VendaTotal(r.VendaId, r.ValorBaseDeCalculoIcms, r.ValorIcms, r.ValorIcmsDesonerado, r.ValorFcp,
                    r.ValorBaseDeCalculoSt, r.ValorSt, r.ValorFcpSt, r.ValorFcpRet, r.ValorProduto, r.ValorFrete, r.ValorSeguro,
                    r.ValorDesconto, r.ValorImpostoImportacao, r.ValorIpi, r.ValorIpiDevolucao, r.ValorPis, r.ValorCofins, r.ValorOutro,
                    r.ValorNotaFiscal, tenantId, userId);
                if (!total.IsValid) return CommandResult.Falha(total.Notifications.Select(n => n.Message));
                // Persiste via AGREGADO (porte fiel de Venda.IncluirTotal).
                venda.DefinirTotal(total);
                if (!venda.IsValid) return CommandResult.Falha(venda.Notifications.Select(n => n.Message));
            }

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Totais definidos com sucesso.");
        }
    }

    public class DefinirTotalIbsCbsCommandHandler : ICommandHandler<DefinirTotalIbsCbsCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public DefinirTotalIbsCbsCommandHandler(ContextVendas context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(DefinirTotalIbsCbsCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";

            var venda = await _context.Vendas.Include(v => v.TotalIbsCbs).FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda == null) return CommandResult.Falha("Venda não encontrada.");

            if (venda.TotalIbsCbs != null)
                venda.TotalIbsCbs.Alterar(r.ValorBaseDeCalculo, r.ValorImpostoDevidoEstadual, r.ValorImpostoDevidoMunicipal, r.ValorImpostoDevidoCbs, userId);
            else
            {
                var t = new VendaTotalIbsCbs(r.VendaId, r.ValorBaseDeCalculo, r.ValorImpostoDevidoEstadual, r.ValorImpostoDevidoMunicipal, r.ValorImpostoDevidoCbs, tenantId, userId);
                // Persiste via AGREGADO (porte fiel de Venda.IncluirTotalIbsCbs).
                venda.DefinirTotalIbsCbs(t);
                if (!venda.IsValid) return CommandResult.Falha(venda.Notifications.Select(n => n.Message));
            }

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Totais IBS/CBS definidos com sucesso.");
        }
    }

    public class DefinirConfiguracaoCommandHandler : ICommandHandler<DefinirConfiguracaoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public DefinirConfiguracaoCommandHandler(ContextVendas context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(DefinirConfiguracaoCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";

            var venda = await _context.Vendas.Include(v => v.Configuracao).FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda == null) return CommandResult.Falha("Venda não encontrada.");

            if (venda.Configuracao != null)
                venda.Configuracao.Alterar(r.TipoOperacao, r.TipoFormatoImpressaoDanfe, r.TipoEmissao, r.TipoAmbiente, r.FinalidadeEmissao, r.IndicadorFinalidadeOperacao, r.TipoAtendimento, r.IndicadorIntermediadorMarketplace, userId);
            else
            {
                var cfg = new VendaConfiguracao(r.VendaId, r.TipoOperacao, r.TipoFormatoImpressaoDanfe, r.TipoEmissao, r.TipoAmbiente, r.FinalidadeEmissao, r.IndicadorFinalidadeOperacao, r.TipoAtendimento, r.IndicadorIntermediadorMarketplace, tenantId, userId);
                // Persiste via AGREGADO (porte fiel de Venda.IncluirConfiguracao).
                venda.DefinirConfiguracao(cfg);
                if (!venda.IsValid) return CommandResult.Falha(venda.Notifications.Select(n => n.Message));
            }

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Configuração definida com sucesso.");
        }
    }

    public class DefinirVendaImpostoCommandHandler : ICommandHandler<DefinirVendaImpostoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public DefinirVendaImpostoCommandHandler(ContextVendas context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(DefinirVendaImpostoCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";

            var venda = await _context.Vendas.Include(v => v.Imposto).FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda == null) return CommandResult.Falha("Venda não encontrada.");

            if (venda.Imposto != null)
                venda.Imposto.Alterar(r.ValorAliquotaCreditoIcms, userId);
            else
            {
                var imp = new VendaImposto(r.VendaId, r.ValorAliquotaCreditoIcms, tenantId, userId);
                // Persiste via AGREGADO (porte fiel de Venda.AdicionarImposto).
                venda.DefinirImposto(imp);
                if (!venda.IsValid) return CommandResult.Falha(venda.Notifications.Select(n => n.Message));
            }

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Imposto da venda definido com sucesso.");
        }
    }

    // ---------- Pagamentos ----------

    public class AdicionarPagamentoCommandHandler : ICommandHandler<AdicionarPagamentoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public AdicionarPagamentoCommandHandler(ContextVendas context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(AdicionarPagamentoCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";

            var venda = await _context.Vendas.FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda == null) return CommandResult.Falha("Venda não encontrada.");

            var pag = new VendaPagamento(r.VendaId, r.ValorTroco, r.TipoPagamento, r.ValorPagamento, r.CartaoTipoIntegracao,
                r.CartaoCnpjIntermediadorFinanceira, r.CartaoBandeira, r.CartaoCodigoAutorizacaoOperacao, tenantId, userId);

            if (!pag.IsValid) return CommandResult.Falha(pag.Notifications.Select(n => n.Message));

            // Persiste via AGREGADO (porte fiel de Venda.DefinirPagamento/AdicionarPagamento): a coleção
            // 1:N fica no agregado raiz e cascateia a partir de Venda.
            venda.AdicionarPagamento(pag);
            if (!venda.IsValid) return CommandResult.Falha(venda.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Pagamento adicionado com sucesso.", new { pag.Id });
        }
    }

    public class DeletarPagamentoCommandHandler : ICommandHandler<DeletarPagamentoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ICurrentUser _user;

        public DeletarPagamentoCommandHandler(ContextVendas context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(DeletarPagamentoCommand r, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            var venda = await _context.Vendas.Include(v => v.Pagamentos).FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda == null) return CommandResult.Falha("Venda não encontrada.");

            venda.DeletarPagamento(r.PagamentoId, userId);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Pagamento excluído com sucesso.");
        }
    }

    // ---------- Fatura ----------

    public class DefinirFaturaCommandHandler : ICommandHandler<DefinirFaturaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public DefinirFaturaCommandHandler(ContextVendas context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(DefinirFaturaCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";

            var venda = await _context.Vendas.Include(v => v.Fatura).FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda == null) return CommandResult.Falha("Venda não encontrada.");

            if (venda.Fatura != null)
            {
                venda.Fatura.Alterar(r.NumeroFatura, r.ValorOriginal, r.ValorDesconto, r.ValorLiquido, userId);
                if (!venda.Fatura.IsValid) return CommandResult.Falha(venda.Fatura.Notifications.Select(n => n.Message));
            }
            else
            {
                var fatura = new VendaFatura(r.VendaId, r.NumeroFatura, r.ValorOriginal, r.ValorDesconto, r.ValorLiquido, tenantId, userId);
                if (!fatura.IsValid) return CommandResult.Falha(fatura.Notifications.Select(n => n.Message));
                // Persiste via AGREGADO (porte fiel de Venda.AdicionarFatura).
                venda.DefinirFatura(fatura);
                if (!venda.IsValid) return CommandResult.Falha(venda.Notifications.Select(n => n.Message));
            }

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Fatura definida com sucesso.");
        }
    }

    public class DeletarFaturaCommandHandler : ICommandHandler<DeletarFaturaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ICurrentUser _user;

        public DeletarFaturaCommandHandler(ContextVendas context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(DeletarFaturaCommand r, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            var venda = await _context.Vendas.Include(v => v.Fatura).ThenInclude(f => f!.Duplicatas).FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda == null) return CommandResult.Falha("Venda não encontrada.");

            venda.DeletarFatura(userId);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Fatura excluída com sucesso.");
        }
    }

    public class IncluirDuplicataCommandHandler : ICommandHandler<IncluirDuplicataCommand>
    {
        private readonly ContextVendas _context;
        private readonly ICurrentUser _user;

        public IncluirDuplicataCommandHandler(ContextVendas context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(IncluirDuplicataCommand r, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            var venda = await _context.Vendas.Include(v => v.Fatura).ThenInclude(f => f!.Duplicatas).FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda?.Fatura == null) return CommandResult.Falha("Fatura da venda não encontrada.");

            venda.Fatura.IncluirDuplicata(r.NumeroDuplicata, r.DataVencimento, r.ValorDuplicata, userId);
            if (!venda.Fatura.IsValid) return CommandResult.Falha(venda.Fatura.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Duplicata incluída com sucesso.");
        }
    }

    // ---------- Transporte ----------

    public class DefinirTransporteCommandHandler : ICommandHandler<DefinirTransporteCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public DefinirTransporteCommandHandler(ContextVendas context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(DefinirTransporteCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";

            var venda = await _context.Vendas
                .Include(v => v.Transporte).ThenInclude(t => t!.Volumes)
                .Include(v => v.Transporte).ThenInclude(t => t!.Reboques)
                .Include(v => v.Transporte).ThenInclude(t => t!.Transportadora)
                .Include(v => v.Transporte).ThenInclude(t => t!.Veiculo)
                .FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda == null) return CommandResult.Falha("Venda não encontrada.");

            if (venda.Transporte != null) _context.VendaTransportes.Remove(venda.Transporte);

            var transporte = new VendaTransporte(r.VendaId, tenantId, userId);

            if (r.TransportadoraRazaoSocial != null || r.TransportadoraPessoaId != null)
                transporte.IncluirTransportadora(r.TransportadoraPessoaId, r.TransportadoraCnpj, r.TransportadoraCpf, r.TransportadoraRazaoSocial, r.TransportadoraInscricaoEstadual, r.TransportadoraLogradouro, r.TransportadoraNumero, r.TransportadoraComplemento, r.TransportadoraBairro, r.TransportadoraMunicipio, r.TransportadoraUf ?? default, userId);

            if (r.VeiculoPlaca != null)
                transporte.IncluirVeiculo(r.VeiculoId, r.VeiculoPlaca, r.VeiculoUf ?? default, r.VeiculoRntrc, userId);

            if (r.Volumes != null)
                foreach (var vol in r.Volumes)
                    transporte.IncluirVolume(vol.QuantidadeVolumes, vol.Especie, vol.NumeroVolumes, vol.PesoLiquido, vol.PesoBruto, vol.Marca, userId);

            if (r.Reboques != null)
                foreach (var reb in r.Reboques)
                    transporte.IncluirReboque(reb.VeiculoId, reb.Placa, reb.Uf, reb.Rntrc, userId);

            transporte.Validar();
            if (!transporte.IsValid) return CommandResult.Falha(transporte.Notifications.Select(n => n.Message));

            // Persiste via AGREGADO (porte fiel de Venda.IncluirTransporte): a navegação 1:1 fica no
            // agregado raiz e cascateia p/ transportadora/veículo/volumes/reboques a partir de Venda.
            venda.DefinirTransporte(transporte);
            if (!venda.IsValid) return CommandResult.Falha(venda.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Transporte definido com sucesso.", new { transporte.Id });
        }
    }

    // ---------- NF-e / NFC-e ----------

    public class IncluirNfceCommandHandler : ICommandHandler<IncluirNfceCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public IncluirNfceCommandHandler(ContextVendas context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(IncluirNfceCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";

            var venda = await _context.Vendas.Include(v => v.Nfce).FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda == null) return CommandResult.Falha("Venda não encontrada.");
            if (venda.Nfce != null) return CommandResult.Falha("NFC-e já incluída para esta venda.");

            var nfce = new VendaNfce(r.VendaId, r.Serie, r.Numero, r.IdCsc, r.Csc, tenantId, userId);
            // Persiste via AGREGADO (porte fiel de Venda.IncluirNfce).
            venda.DefinirNfce(nfce);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("NFC-e incluída com sucesso.", new { nfce.Id });
        }
    }

    public class TransmitirNfceCommandHandler : ICommandHandler<TransmitirNfceCommand>
    {
        private readonly ContextVendas _context;
        private readonly ICurrentUser _user;

        public TransmitirNfceCommandHandler(ContextVendas context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(TransmitirNfceCommand r, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            var venda = await _context.Vendas.Include(v => v.Nfce).FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda?.Nfce == null) return CommandResult.Falha("NFC-e da venda não encontrada.");

            // Porte fiel de Venda.TransmitirNfce: Status = EVendaStatus.Transmitido.
            venda.AtualizarStatus(EVendaStatus.Transmitido, userId);
            venda.Nfce.Transmitir(r.Chave, r.DataHoraEmissao, r.StatusSefaz, r.Protocolo, userId);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("NFC-e transmitida com sucesso.");
        }
    }

    public class CancelarNfceCommandHandler : ICommandHandler<CancelarNfceCommand>
    {
        private readonly ContextVendas _context;
        private readonly ICurrentUser _user;

        public CancelarNfceCommandHandler(ContextVendas context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(CancelarNfceCommand r, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            var venda = await _context.Vendas.Include(v => v.Nfce).FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda?.Nfce == null) return CommandResult.Falha("NFC-e da venda não encontrada.");

            venda.Nfce.Cancelar(r.DataHoraCancelamento, r.ProtocoloCancelamento, r.StatusSefazCancelamento, r.MotivoCancelamento, userId);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("NFC-e cancelada com sucesso.");
        }
    }

    public class IncluirNfeCommandHandler : ICommandHandler<IncluirNfeCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public IncluirNfeCommandHandler(ContextVendas context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(IncluirNfeCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";

            var venda = await _context.Vendas.Include(v => v.Nfe).FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda == null) return CommandResult.Falha("Venda não encontrada.");

            if (venda.Nfe != null)
            {
                venda.Nfe.AtualizarNumeroSerie(r.Serie, r.Numero, r.DataHoraSaida, userId);
            }
            else
            {
                var nfe = new VendaNfe(r.VendaId, r.Numero, r.Serie, r.DataHoraSaida, tenantId, userId, r.EmbuteFrete, r.EmbuteSeguro, r.EmbuteAcrescimo, r.EmbuteOutro);
                // Persiste via AGREGADO (porte fiel de Venda.IncluirNfe).
                venda.DefinirNfe(nfe);
            }

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("NF-e incluída com sucesso.");
        }
    }

    public class TransmitirNfeCommandHandler : ICommandHandler<TransmitirNfeCommand>
    {
        private readonly ContextVendas _context;
        private readonly ICurrentUser _user;

        public TransmitirNfeCommandHandler(ContextVendas context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(TransmitirNfeCommand r, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            var venda = await _context.Vendas.Include(v => v.Nfe).FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda?.Nfe == null) return CommandResult.Falha("NF-e da venda não encontrada.");

            // Porte fiel de Venda.TransmitirNfe: Status = EVendaStatus.Transmitido.
            venda.AtualizarStatus(EVendaStatus.Transmitido, userId);
            venda.RedefinirNaturezaOperacao(r.NaturezaOperacao);
            venda.Nfe.Transmitir(r.Chave, r.DataHoraEmissao, r.StatusSefaz, r.Protocolo, userId);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("NF-e transmitida com sucesso.");
        }
    }

    public class CancelarNfeCommandHandler : ICommandHandler<CancelarNfeCommand>
    {
        private readonly ContextVendas _context;
        private readonly ICurrentUser _user;

        public CancelarNfeCommandHandler(ContextVendas context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(CancelarNfeCommand r, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            var venda = await _context.Vendas.Include(v => v.Nfe).FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda?.Nfe == null) return CommandResult.Falha("NF-e da venda não encontrada.");

            venda.Nfe.Cancelar(r.DataHoraCancelamento, r.ProtocoloCancelamento, r.StatusSefazCancelamento, r.MotivoCancelamento, userId);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("NF-e cancelada com sucesso.");
        }
    }

    public class AdicionarNfeReferenciadaCommandHandler : ICommandHandler<AdicionarNfeReferenciadaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ICurrentUser _user;

        public AdicionarNfeReferenciadaCommandHandler(ContextVendas context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(AdicionarNfeReferenciadaCommand r, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            var venda = await _context.Vendas.Include(v => v.Referenciadas).FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda == null) return CommandResult.Falha("Venda não encontrada.");

            venda.AdicionarNfeReferenciada(r.ChaveAcesso, userId);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("NF-e referenciada adicionada com sucesso.");
        }
    }

    public class AdicionarCartaCorrecaoCommandHandler : ICommandHandler<AdicionarCartaCorrecaoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ICurrentUser _user;

        public AdicionarCartaCorrecaoCommandHandler(ContextVendas context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(AdicionarCartaCorrecaoCommand r, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            var venda = await _context.Vendas
                .Include(v => v.Nfe).ThenInclude(n => n!.CartasCorrecoes)
                .FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda?.Nfe == null) return CommandResult.Falha("NF-e da venda não encontrada.");

            venda.Nfe.AdicionarCartaCorrecao(r.Ambiente, r.TextoCorrecao, userId);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Carta de correção adicionada com sucesso.");
        }
    }

    public class VincularDocumentoFiscalCommandHandler : ICommandHandler<VincularDocumentoFiscalCommand>
    {
        private readonly ContextVendas _context;
        private readonly ICurrentUser _user;

        public VincularDocumentoFiscalCommandHandler(ContextVendas context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(VincularDocumentoFiscalCommand r, CancellationToken ct)
        {
            var venda = await _context.Vendas.FirstOrDefaultAsync(v => v.Id == r.VendaId && v.DeletadoEm == null, ct);
            if (venda == null) return CommandResult.Falha("Venda não encontrada.");

            venda.VincularDocumentoFiscal(r.DocumentoFiscalId);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Documento fiscal vinculado com sucesso.");
        }
    }
}
