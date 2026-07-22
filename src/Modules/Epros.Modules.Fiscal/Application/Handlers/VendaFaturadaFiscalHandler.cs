using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Epros.Modules.Fiscal.Infrastructure.Services;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    public class VendaFaturadaFiscalHandler : INotificationHandler<VendaFaturadaEventNotification>
    {
        private readonly ContextFiscal _context;
        private readonly CalculadoraImpostosDocumentoFiscal _calculadora;

        public VendaFaturadaFiscalHandler(
            ContextFiscal context,
            CalculadoraImpostosDocumentoFiscal calculadora)
        {
            _context = context;
            _calculadora = calculadora;
        }

        public async Task Handle(VendaFaturadaEventNotification notification, CancellationToken cancellationToken)
        {
            // 1. Evitar emissão em duplicidade utilizando o VendaOrigemId
            var existe = await _context.DocumentosFiscais
                .IgnoreQueryFilters()
                .AnyAsync(d => d.TenantId == notification.TenantId && d.VendaOrigemId == notification.VendaId, cancellationToken);

            if (existe)
            {
                return; // Já processado
            }

            // 2. Determinar o número sequencial correto para a NFC-e (modelo 65, série 1)
            const string modeloNfce = "65";
            const int serieNfce = 1;
            const int ambienteHomologacao = 2; // Default para segurança em homologação

            var ultimoNumero = await _context.DocumentosFiscais
                .IgnoreQueryFilters()
                .Where(d => d.TenantId == notification.TenantId && d.Modelo == modeloNfce && d.Serie == serieNfce)
                .MaxAsync(d => (long?)d.Numero, cancellationToken) ?? 0;

            var proximoNumero = ultimoNumero + 1;

            // 3. Instanciar agregado raiz do Documento Fiscal
            var documento = new DocumentoFiscal(
                modelo: modeloNfce,
                ambiente: ambienteHomologacao,
                serie: serieNfce,
                numero: proximoNumero,
                total: notification.Total,
                destinatarioCnpjCpf: "00000000000", // Consumidor Final
                destinatarioNome: "Consumidor Final",
                tenantId: notification.TenantId,
                criadoPor: notification.UserId
            );

            documento.VincularVendaOrigem(notification.VendaId);

            // 4. Adicionar os itens ao documento fiscal buscando SKU/nome no ProdutoLookup
            foreach (var item in notification.Itens)
            {
                var produto = await _context.ProdutosLookup
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.TenantId == notification.TenantId && p.Id == item.ProdutoId, cancellationToken);

                var sku = produto?.Sku ?? "GENERICO";
                var nome = produto?.Nome ?? "Produto não identificado";

                // Valores padrão fiscais brasileiros para NFC-e Simples Nacional
                const string csosnDefault = "102"; // Simples Nacional sem permissão de crédito (CSOSN)
                const int cfopDefault = 5102;      // Venda de mercadoria de terceiros
                const string ncmDefault = "99999999"; // NCM Genérico
                const decimal aliquotaIcmsDefault = 0.00m;

                documento.AdicionarItem(
                    sku: sku,
                    nomeProduto: nome,
                    cst: csosnDefault,
                    cfop: cfopDefault,
                    ncm: ncmDefault,
                    quantidade: item.Quantidade,
                    valorUnitario: item.PrecoUnitario,
                    aliquotaIcms: aliquotaIcmsDefault,
                    criadoPor: notification.UserId
                );

                // Classificação fiscal do item recém-adicionado (CSOSN/origem/PIS-COFINS) para o cálculo.
                var itemAdicionado = documento.Itens[^1];
                itemAdicionado.DefinirClassificacaoFiscal(
                    csosn: csosnDefault,
                    origem: "0",
                    cstIpi: null,
                    aliquotaIpi: 0m,
                    cstPisCofins: "07", // Operação isenta da contribuição
                    aliquotaPis: 0m,
                    aliquotaCofins: 0m,
                    cstIbsCbs: null,
                    cClassTrib: null);
            }

            if (!documento.IsValid)
            {
                Console.WriteLine($"[Fiscal] Documento Fiscal gerado para a venda {notification.VendaId} é inválido.");
                return;
            }

            // 4.1. Calcular e aplicar os impostos reais por item (ICMS/ST/FCP/IPI/PIS/COFINS) já no rascunho,
            //      para que a NFC-e nasça com os tributos apurados (não com zeros). Reusa o mesmo motor da
            //      emissão manual. Se não houver emitente configurado, degrada mantendo o ICMS informado.
            _calculadora.CalcularEAplicar(documento);

            // 5. Salvar o documento fiscal no status rascunho
            _context.DocumentosFiscais.Add(documento);
            await _context.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"[Fiscal] Rascunho de NFC-e Número {proximoNumero} gerado automaticamente para a Venda {notification.VendaId}.");
        }
    }
}
