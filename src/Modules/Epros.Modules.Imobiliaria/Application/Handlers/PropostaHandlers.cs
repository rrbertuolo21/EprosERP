using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Imobiliaria.Application.Commands;
using Epros.Modules.Imobiliaria.Domain.Entities;
using Epros.Modules.Imobiliaria.Domain.Enums;
using Epros.Modules.Imobiliaria.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Imobiliaria.Application.Handlers
{
    public class CriarPropostaCommandHandler : ICommandHandler<CriarPropostaCommand>
    {
        private readonly ContextImobiliaria _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPropostaCommandHandler(ContextImobiliaria context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(CriarPropostaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (!await _context.Imoveis.AnyAsync(i => i.Id == request.ImovelId, cancellationToken))
                return CommandResult.Falha("Imovel nao encontrado.");

            var proposta = new Proposta(request.Tipo, request.ImovelId, request.Validade, request.ValorProposto,
                request.Observacao, request.ContrapropostaDeId, tenantId, usuario);

            foreach (var p in request.Partes ?? Enumerable.Empty<PropostaParteInput>())
                proposta.AdicionarParte(new PropostaParte(p.PessoaId, p.Papel, tenantId, usuario));

            proposta.Validar();
            if (!proposta.IsValid)
                return CommandResult.Falha(proposta.Notifications.Select(n => n.Message));

            _context.Propostas.Add(proposta);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Proposta criada com sucesso!", new { PropostaId = proposta.Id, Status = proposta.Status.ToString() });
        }
    }

    /// <summary>Aprovacao/rejeicao da proposta (ID2). Compartilha a busca e a persistencia.</summary>
    public class PropostaDecisaoCommandHandler :
        ICommandHandler<AprovarPropostaCommand>,
        ICommandHandler<RejeitarPropostaCommand>
    {
        private readonly ContextImobiliaria _context;
        private readonly ICurrentUser _currentUser;

        public PropostaDecisaoCommandHandler(ContextImobiliaria context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public Task<CommandResult> Handle(AprovarPropostaCommand request, CancellationToken ct)
            => Decidir(request.PropostaId, (p, u) => p.Aprovar(u), "Proposta aprovada com sucesso!", ct);

        public Task<CommandResult> Handle(RejeitarPropostaCommand request, CancellationToken ct)
            => Decidir(request.PropostaId, (p, u) => p.Rejeitar(u), "Proposta rejeitada com sucesso!", ct);

        private async Task<CommandResult> Decidir(Guid propostaId, Action<Proposta, string> decisao, string mensagem, CancellationToken ct)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var proposta = await _context.Propostas.FirstOrDefaultAsync(p => p.Id == propostaId, ct);
            if (proposta is null)
                return CommandResult.Falha("Proposta nao encontrada.");

            decisao(proposta, usuario);
            if (!proposta.IsValid)
                return CommandResult.Falha(proposta.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok(mensagem, new { PropostaId = proposta.Id, Status = proposta.Status.ToString() });
        }
    }

    /// <summary>
    /// Converte a proposta aprovada (ID2). Locacao: gera a locacao (EmElaboracao) com o valor e os
    /// proponentes da proposta, vinculada ao imovel. Aquisicao: apenas marca convertida (VENDAS e dono
    /// do contrato de venda — fronteira). Uma unica transacao.
    /// </summary>
    public class ConverterPropostaCommandHandler : ICommandHandler<ConverterPropostaCommand>
    {
        private readonly ContextImobiliaria _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ConverterPropostaCommandHandler(ContextImobiliaria context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(ConverterPropostaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var proposta = await _context.Propostas.Include(p => p.Partes)
                .FirstOrDefaultAsync(p => p.Id == request.PropostaId, cancellationToken);
            if (proposta is null)
                return CommandResult.Falha("Proposta nao encontrada.");
            if (proposta.Status != EStatusProposta.Aprovada)
                return CommandResult.Falha("Somente proposta aprovada pode ser convertida.", block: true);

            Guid? locacaoGeradaId = null;

            if (proposta.Tipo == ETipoProposta.Locacao)
            {
                var inicio = request.PeriodoInicial ?? DateTime.UtcNow.Date;
                var fim = request.PeriodoFinal ?? inicio.AddMonths(12);

                var locacao = new Locacao(proposta.ImovelId, inicio, fim, proposta.ValorProposto,
                    request.Vencimento, tenantId, usuario);

                foreach (var parte in proposta.Partes.Where(x => x.Papel == EPapelParteProposta.Proponente))
                    locacao.AdicionarLocatario(parte.PessoaId, usuario);

                locacao.Validar();
                if (!locacao.IsValid)
                    return CommandResult.Falha(locacao.Notifications.Select(n => n.Message));

                _context.Locacoes.Add(locacao);
                locacaoGeradaId = locacao.Id;
            }

            proposta.MarcarConvertida(locacaoGeradaId, usuario);
            if (!proposta.IsValid)
                return CommandResult.Falha(proposta.Notifications.Select(n => n.Message));

            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Imobiliaria.PropostaConvertida,
                JsonSerializer.Serialize(new { propostaId = proposta.Id, tipo = proposta.Tipo.ToString(), locacaoId = locacaoGeradaId, imovelId = proposta.ImovelId, tenantId })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Proposta convertida com sucesso!",
                new { PropostaId = proposta.Id, LocacaoGeradaId = locacaoGeradaId, Status = proposta.Status.ToString() });
        }
    }
}
