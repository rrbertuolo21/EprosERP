using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Qualidade.Domain.Entities;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Qualidade.Application.EventHandlers
{
    /// <summary>
    /// QLD-ACR inbound: recebimento fisico de mercadoria (Estoque LDE) abre uma analise de aceite de
    /// entrada com os itens recebidos (D2/D3, fronteira do dossie). Idempotente por entrada: o codigo
    /// derivado do EntradaId evita analises duplicadas em reentrega do evento.
    /// </summary>
    public class MercadoriaRecebidaQualidadeHandler : INotificationHandler<MercadoriaRecebidaEventNotification>
    {
        private readonly ContextQualidade _context;
        public MercadoriaRecebidaQualidadeHandler(ContextQualidade context) => _context = context;

        public async Task Handle(MercadoriaRecebidaEventNotification n, CancellationToken cancellationToken)
        {
            var usuario = "system_event";
            var codigo = $"ACR-{n.EntradaId.ToString("N").Substring(0, 12)}";

            // Idempotencia: se ja existe analise para esta entrada, nada a fazer.
            if (await _context.AcrAnalises.AnyAsync(a => a.Codigo == codigo, cancellationToken))
                return;

            var descricao = $"Recebimento entrada {n.EntradaId}"
                            + (string.IsNullOrWhiteSpace(n.Numero) ? "" : $" — NF {n.Numero}");
            var analise = new AcrAnalise(codigo, Truncar(descricao, 500), ETipoAnaliseAcr.Recebimento,
                n.FornecedorId ?? Guid.Empty, null, n.DocumentoId, n.TenantId, usuario);
            if (!analise.IsValid) return;
            _context.AcrAnalises.Add(analise);

            var seq = 1;
            foreach (var item in n.Itens ?? Enumerable.Empty<MercadoriaRecebidaItemNotification>())
            {
                var quantidade = item.Quantidade > 0 ? item.Quantidade : 1m;
                var acrItem = new AcrItem(analise.Id, seq++, quantidade, "UN", false, item.ProdutoId,
                    null, null, null, item.Quantidade, null, null, item.ValorItem, null, n.TenantId, usuario);
                if (acrItem.IsValid) _context.AcrItens.Add(acrItem);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        private static string Truncar(string valor, int max)
            => string.IsNullOrEmpty(valor) || valor.Length <= max ? valor : valor.Substring(0, max);
    }
}
