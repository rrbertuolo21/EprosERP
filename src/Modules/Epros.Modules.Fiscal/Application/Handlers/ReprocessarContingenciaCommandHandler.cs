using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    /// <summary>
    /// Reenvia à SEFAZ os documentos que ficaram pendentes em contingência offline
    /// (Status = "PendenteContingencia"), tipicamente quando a SEFAZ volta a operar. A transmissão
    /// mantém o tpEmis original com que a nota foi assinada (regra SEFAZ para offline). Reusa a mesma
    /// <see cref="IHerculesFiscalService.EmitirAsync"/> da emissão — nenhum caminho normal é alterado.
    /// </summary>
    public class ReprocessarContingenciaCommandHandler : ICommandHandler<ReprocessarContingenciaCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;
        private readonly IHerculesFiscalService _fiscalService;

        public ReprocessarContingenciaCommandHandler(
            ContextFiscal context,
            ICurrentUser currentUser,
            IHerculesFiscalService fiscalService)
        {
            _context = context;
            _currentUser = currentUser;
            _fiscalService = fiscalService;
        }

        public async Task<CommandResult> Handle(ReprocessarContingenciaCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var query = _context.DocumentosFiscais
                .Include(d => d.Itens)
                .Where(d => d.Status == "PendenteContingencia");

            if (request.IncluirRejeitados)
                query = _context.DocumentosFiscais
                    .Include(d => d.Itens)
                    .Where(d => d.Status == "PendenteContingencia" || d.Status == "Rejeitado");

            if (request.EmpresaId is not null && request.EmpresaId != Guid.Empty)
                query = query.Where(d => d.EmpresaId == request.EmpresaId);

            var pendentes = await query.OrderBy(d => d.DataEmissao).ToListAsync(cancellationToken);

            if (!pendentes.Any())
                return CommandResult.Ok("Não há documentos de contingência pendentes de reenvio.", new { Reenviados = 0, Autorizados = 0, Rejeitados = 0 });

            // Verificação opcional do status do serviço SEFAZ antes de reenviar (evita reenvio em massa
            // com a SEFAZ ainda fora). Best-effort: resolve o CNPJ do emitente pelo primeiro documento.
            if (request.VerificarStatusServico)
            {
                var primeiro = pendentes.First();
                var cnpj = await ResolverCnpjEmitenteAsync(primeiro.EmpresaId, cancellationToken);
                if (!string.IsNullOrWhiteSpace(cnpj))
                {
                    var status = await _fiscalService.VerificarStatusServicoAsync(new ConsultaStatusServicoRequest
                    {
                        Documento = cnpj!,
                        Modelo = primeiro.Modelo,
                        Ambiente = primeiro.Ambiente
                    });

                    if (!status.Sucesso)
                        return CommandResult.Falha(new[] { status.Motivo }, "SEFAZ ainda indisponível — reenvio de contingência adiado.");
                }
            }

            int autorizados = 0, rejeitados = 0;
            var falhas = new List<string>();

            foreach (var documento in pendentes)
            {
                var resultado = await _fiscalService.EmitirAsync(documento);

                if (resultado.Sucesso && resultado.StatusSefaz == 100)
                {
                    documento.Autorizar(resultado.ChaveAcesso, resultado.Protocolo, resultado.StatusSefaz,
                        resultado.XmlEnvio, resultado.XmlRetorno, resultado.PdfCaminho, resultado.XmlCaminho);
                    autorizados++;
                }
                else
                {
                    documento.Rejeitar(resultado.StatusSefaz, resultado.Motivo, resultado.XmlRetorno);
                    rejeitados++;
                    falhas.Add($"Doc {documento.Modelo}-{documento.Serie}-{documento.Numero}: [{resultado.StatusSefaz}] {resultado.Motivo}");
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok(
                $"Reenvio de contingência concluído: {autorizados} autorizado(s), {rejeitados} rejeitado(s).",
                new { Reenviados = pendentes.Count, Autorizados = autorizados, Rejeitados = rejeitados, Falhas = falhas });
        }

        private async Task<string?> ResolverCnpjEmitenteAsync(Guid? empresaId, CancellationToken ct)
        {
            if (empresaId is null || empresaId == Guid.Empty)
                return null;

            var empresa = await _context.EmpresasLookup
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == empresaId, ct);

            return string.IsNullOrWhiteSpace(empresa?.Cnpj) ? empresa?.Cpf : empresa.Cnpj;
        }
    }
}
