using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Producao.Application.Commands;
using Epros.Modules.Producao.Domain.Entities;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Modules.Producao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Application.Handlers
{
    /// <summary>PRD-MES — Criação da ordem de produção (MES-EF §7.1).</summary>
    public class CriarMesOrdemCommandHandler : ICommandHandler<CriarMesOrdemCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarMesOrdemCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarMesOrdemCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // MES-REG-025: prefixo de referência quando configurado.
            var referencia = request.Referencia;
            if (string.IsNullOrWhiteSpace(referencia))
            {
                var parametro = await _context.MesParametros.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
                if (parametro != null && !string.IsNullOrWhiteSpace(parametro.PrefixoReferencia))
                    referencia = $"{parametro.PrefixoReferencia}-{DateTime.UtcNow:yyyyMMddHHmmss}";
            }

            var ordem = new MesOrdem(
                request.EmpresaId, tenantId, usuario, referencia, request.Inicio, request.PrevisaoEntrega,
                request.EstruturaId, request.ProdutoAcabadoId, request.VariacaoProdutoAcabadoId,
                request.CustoTotalPrevisto, request.PercentualVenda, request.PercentualEstoque);

            if (request.Itens != null)
            {
                foreach (var i in request.Itens)
                    ordem.AdicionarItem(i.ProdutoId, i.QuantidadeProduzir, usuario, i.VariacaoId, i.CustoPrevisto);
            }

            if (!ordem.IsValid)
                return CommandResult.Falha(ordem.Notifications.Select(n => n.Message));

            _context.MesOrdens.Add(ordem);
            _context.MesHistoricos.Add(new MesHistorico(ordem.Id, "Criacao", usuario, "{}", tenantId, usuario, null, EStatusOrdemMes.Rascunho));
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Ordem de produção criada em Rascunho.", new { ordem.Id, ordem.Referencia });
        }
    }

    /// <summary>PRD-MES — Inclusão de item na ordem (MES-EF §7.1).</summary>
    public class AdicionarMesOrdemItemCommandHandler : ICommandHandler<AdicionarMesOrdemItemCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarMesOrdemItemCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarMesOrdemItemCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var ordem = await _context.MesOrdens.Include(o => o.Itens).FirstOrDefaultAsync(o => o.Id == request.OrdemId, cancellationToken);
            if (ordem == null) return CommandResult.Falha("Ordem não encontrada.");

            ordem.AdicionarItem(request.ProdutoId, request.QuantidadeProduzir, usuario, request.VariacaoId, request.CustoPrevisto);
            if (!ordem.IsValid) return CommandResult.Falha(ordem.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Item incluído na ordem.", new { ordem.Id });
        }
    }

    /// <summary>PRD-MES — Registro de produção realizada por item (MES-EF §7.5).</summary>
    public class RegistrarMesProducaoItemCommandHandler : ICommandHandler<RegistrarMesProducaoItemCommand>
    {
        private readonly ContextProducao _context;
        private readonly ICurrentUser _currentUser;

        public RegistrarMesProducaoItemCommandHandler(ContextProducao context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarMesProducaoItemCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var item = await _context.MesOrdemItens.FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);
            if (item == null) return CommandResult.Falha("Item da ordem não encontrado.");

            item.RegistrarProducao(request.QuantidadeProduzida, request.QuantidadeEntregue, usuario, request.CustoRealizado);
            if (!item.IsValid) return CommandResult.Falha(item.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Produção registrada.", new { item.Id, Status = item.Status.ToString() });
        }
    }

    /// <summary>PRD-MES — Apontamento de serviço (MES-EF §7.3).</summary>
    public class ApontarMesServicoCommandHandler : ICommandHandler<ApontarMesServicoCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ApontarMesServicoCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ApontarMesServicoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var itemExiste = await _context.MesOrdemItens.AnyAsync(i => i.Id == request.ItemOrdemId, cancellationToken);
            if (!itemExiste) return CommandResult.Falha("Item da ordem não encontrado.");

            var servico = new MesServico(
                request.ItemOrdemId, tenantId, usuario,
                request.InicioPrevisto, request.TerminoPrevisto,
                request.HorasPrevisto, request.MinutosPrevisto, request.SegundosPrevisto, request.CustoPrevisto);

            if (request.TerminoRealizado.HasValue || request.InicioRealizado.HasValue || request.HorasRealizado > 0)
            {
                servico.RegistrarRealizado(request.InicioRealizado, request.TerminoRealizado,
                    request.HorasRealizado, request.MinutosRealizado, request.SegundosRealizado, usuario, request.CustoRealizado);
            }

            if (!servico.IsValid) return CommandResult.Falha(servico.Notifications.Select(n => n.Message));

            _context.MesServicos.Add(servico);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Serviço apontado.", new { servico.Id });
        }
    }

    /// <summary>PRD-MES — Vínculo de equipamento ao serviço (MES-EF §7.4).</summary>
    public class VincularMesEquipamentoCommandHandler : ICommandHandler<VincularMesEquipamentoCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public VincularMesEquipamentoCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(VincularMesEquipamentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var servicoExiste = await _context.MesServicos.AnyAsync(s => s.Id == request.ServicoId, cancellationToken);
            if (!servicoExiste) return CommandResult.Falha("Serviço não encontrado.");

            var vinculo = new MesServicoEquipamento(request.ServicoId, request.EquipamentoId, tenantId, usuario);
            if (!vinculo.IsValid) return CommandResult.Falha(vinculo.Notifications.Select(n => n.Message));

            _context.MesServicoEquipamentos.Add(vinculo);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Equipamento vinculado ao serviço.", new { vinculo.Id });
        }
    }

    /// <summary>
    /// PRD-MES — Finalização da produção (MES-EF §7.6). Gera consumo de materiais a partir da estrutura ativa
    /// e o par coordenado entrada de produto acabado + consumo (MES-REG-014/015/016).
    /// </summary>
    public class FinalizarMesOrdemCommandHandler : ICommandHandler<FinalizarMesOrdemCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public FinalizarMesOrdemCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(FinalizarMesOrdemCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var ordem = await _context.MesOrdens.Include(o => o.Itens).FirstOrDefaultAsync(o => o.Id == request.OrdemId, cancellationToken);
            if (ordem == null) return CommandResult.Falha("Ordem não encontrada.");

            var parametro = await _context.MesParametros.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
            var exigirEstruturaAtiva = parametro?.ExigirEstruturaAtiva ?? false;

            ordem.Finalizar(request.DataTransacao, request.LocalEstoqueId, request.ValorTotalFinal, usuario,
                exigirEstruturaAtiva, request.DesperdicioUnidades, request.Lote, request.Validade);

            if (!ordem.IsValid)
                return CommandResult.Falha(ordem.Notifications.Select(n => n.Message));

            var produtoAcabadoId = ordem.ProdutoAcabadoId ?? ordem.Itens.FirstOrDefault()?.ProdutoId ?? Guid.Empty;
            var quantidadeProduzida = ordem.Itens.Sum(i => i.QuantidadeProduzida);
            var quantidadeEfetiva = ordem.CalcularQuantidadeEfetiva(quantidadeProduzida);

            // MES-REG-014: entrada de produto acabado (movimento pai).
            var entrada = new MesMovimentoProducao(
                ordem.Id, ETipoMovimentoMes.EntradaProdutoAcabado, produtoAcabadoId,
                quantidadeEfetiva > 0m ? quantidadeEfetiva : 1m, request.LocalEstoqueId, tenantId, usuario,
                null, ordem.VariacaoProdutoAcabadoId, null,
                request.ValorTotalFinal);
            _context.MesMovimentos.Add(entrada);

            // MES-REG-013/015/016: consumo de materiais da estrutura ativa, vinculado à entrada (movimento pai).
            if (ordem.EstruturaId.HasValue && ordem.EstruturaId.Value != Guid.Empty)
            {
                var componentes = await _context.BomComponentes.AsNoTracking()
                    .Where(c => c.EstruturaId == ordem.EstruturaId.Value)
                    .ToListAsync(cancellationToken);

                foreach (var comp in componentes)
                {
                    var quantidadePrevista = comp.QuantidadeFinal ?? comp.Quantidade;

                    var consumo = new MesConsumoMaterial(
                        ordem.Id, ordem.EstruturaId.Value, comp.Id, comp.VariacaoComponenteId, tenantId, usuario,
                        quantidadePrevista, quantidadePrevista, comp.VariacaoComponenteId != Guid.Empty ? comp.VariacaoComponenteId : (Guid?)null,
                        comp.PercentualDesperdicio, comp.SubUnidadeId, comp.CustoLinha);
                    consumo.Confirmar(quantidadePrevista, usuario);
                    _context.MesConsumos.Add(consumo);

                    var movConsumo = new MesMovimentoProducao(
                        ordem.Id, ETipoMovimentoMes.ConsumoMaterial, comp.VariacaoComponenteId,
                        quantidadePrevista > 0m ? quantidadePrevista : 1m, request.LocalEstoqueId, tenantId, usuario,
                        entrada.Id, null, null, comp.CustoLinha);
                    movConsumo.Confirmar(usuario);
                    _context.MesMovimentos.Add(movConsumo);
                }
            }

            entrada.Confirmar(usuario);
            _context.MesHistoricos.Add(new MesHistorico(ordem.Id, "Finalizacao", usuario, "{}", tenantId, usuario, EStatusOrdemMes.Ativo, EStatusOrdemMes.Finalizado));
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Produção finalizada com entrada de produto acabado e consumo de materiais.", new { ordem.Id, ordem.Status });
        }
    }

    /// <summary>PRD-MES — Salva parâmetros de produção por tenant (MES-REG-025/026/027).</summary>
    public class SalvarMesParametroCommandHandler : ICommandHandler<SalvarMesParametroCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public SalvarMesParametroCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(SalvarMesParametroCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var parametro = await _context.MesParametros.FirstOrDefaultAsync(cancellationToken);
            if (parametro == null)
            {
                parametro = new MesParametro(tenantId, usuario, request.PrefixoReferencia,
                    request.BloquearEdicaoQuantidadeInsumo, request.AtualizarPrecoProdutoFinal,
                    request.ExigirEstruturaAtiva, request.VersaoParametro);
                _context.MesParametros.Add(parametro);
            }
            else
            {
                parametro.Alterar(request.PrefixoReferencia, request.BloquearEdicaoQuantidadeInsumo,
                    request.AtualizarPrecoProdutoFinal, request.ExigirEstruturaAtiva, request.VersaoParametro, usuario);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Parâmetros de produção salvos.", new { parametro.Id });
        }
    }

    // ===================== Transições de workflow da ordem =====================

    public abstract class MesOrdemTransicaoHandlerBase
    {
        protected readonly ContextProducao _context;
        protected readonly ITenantProvider _tenantProvider;
        protected readonly ICurrentUser _currentUser;

        protected MesOrdemTransicaoHandlerBase(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        protected async Task<(MesOrdem? ordem, string usuario, string tenantId)> CarregarAsync(Guid id, CancellationToken ct)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var tenantId = _tenantProvider.GetTenantId();
            var ordem = await _context.MesOrdens.Include(o => o.Itens).FirstOrDefaultAsync(o => o.Id == id, ct);
            return (ordem, usuario, tenantId);
        }

        protected async Task<CommandResult> FinalizarAsync(MesOrdem ordem, string acao, string usuario, string tenantId, CancellationToken ct)
        {
            if (!ordem.IsValid)
                return CommandResult.Falha(ordem.Notifications.Select(n => n.Message));

            _context.MesHistoricos.Add(new MesHistorico(ordem.Id, acao, usuario, "{}", tenantId, usuario, null, ordem.Status));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok($"Ordem {acao} com sucesso.", new { ordem.Id, Status = ordem.Status.ToString() });
        }
    }

    public class SubmeterMesOrdemCommandHandler : MesOrdemTransicaoHandlerBase, ICommandHandler<SubmeterMesOrdemCommand>
    {
        public SubmeterMesOrdemCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(SubmeterMesOrdemCommand request, CancellationToken ct)
        {
            var (ordem, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (ordem == null) return CommandResult.Falha("Ordem não encontrada.");
            ordem.SubmeterParaAnalise(usuario);
            return await FinalizarAsync(ordem, "Submissao", usuario, tenantId, ct);
        }
    }

    public class AprovarMesOrdemCommandHandler : MesOrdemTransicaoHandlerBase, ICommandHandler<AprovarMesOrdemCommand>
    {
        public AprovarMesOrdemCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(AprovarMesOrdemCommand request, CancellationToken ct)
        {
            var (ordem, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (ordem == null) return CommandResult.Falha("Ordem não encontrada.");
            ordem.Aprovar(usuario);
            return await FinalizarAsync(ordem, "Aprovacao", usuario, tenantId, ct);
        }
    }

    public class RejeitarMesOrdemCommandHandler : MesOrdemTransicaoHandlerBase, ICommandHandler<RejeitarMesOrdemCommand>
    {
        public RejeitarMesOrdemCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(RejeitarMesOrdemCommand request, CancellationToken ct)
        {
            var (ordem, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (ordem == null) return CommandResult.Falha("Ordem não encontrada.");
            ordem.Rejeitar(request.Motivo, usuario);
            return await FinalizarAsync(ordem, "Rejeicao", usuario, tenantId, ct);
        }
    }

    public class InativarMesOrdemCommandHandler : MesOrdemTransicaoHandlerBase, ICommandHandler<InativarMesOrdemCommand>
    {
        public InativarMesOrdemCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(InativarMesOrdemCommand request, CancellationToken ct)
        {
            var (ordem, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (ordem == null) return CommandResult.Falha("Ordem não encontrada.");
            ordem.Inativar(usuario);
            return await FinalizarAsync(ordem, "Inativacao", usuario, tenantId, ct);
        }
    }

    public class ReativarMesOrdemCommandHandler : MesOrdemTransicaoHandlerBase, ICommandHandler<ReativarMesOrdemCommand>
    {
        public ReativarMesOrdemCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(ReativarMesOrdemCommand request, CancellationToken ct)
        {
            var (ordem, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (ordem == null) return CommandResult.Falha("Ordem não encontrada.");
            ordem.Reativar(usuario);
            return await FinalizarAsync(ordem, "Reativacao", usuario, tenantId, ct);
        }
    }

    public class EncerrarMesOrdemCommandHandler : MesOrdemTransicaoHandlerBase, ICommandHandler<EncerrarMesOrdemCommand>
    {
        public EncerrarMesOrdemCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(EncerrarMesOrdemCommand request, CancellationToken ct)
        {
            var (ordem, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (ordem == null) return CommandResult.Falha("Ordem não encontrada.");
            ordem.Encerrar(usuario);
            return await FinalizarAsync(ordem, "Encerramento", usuario, tenantId, ct);
        }
    }
}
