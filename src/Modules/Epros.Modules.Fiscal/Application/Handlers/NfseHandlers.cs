using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    /// <summary>
    /// Emite um lote de RPS (NFS-e). Persiste a <see cref="NotaServicoEletronica"/> como rascunho,
    /// transmite via <see cref="INfseFiscalService"/> e atualiza o estado conforme o retorno da prefeitura.
    /// Fiel ao legado <c>NfseController.EmitirLote</c> + <c>VendaNfseService.EmitirLote</c>.
    /// </summary>
    public class EmitirLoteNfseCommandHandler : ICommandHandler<EmitirLoteNfseCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly INfseFiscalService _nfseService;

        public EmitirLoteNfseCommandHandler(
            ContextFiscal context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            INfseFiscalService nfseService)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _nfseService = nfseService;
        }

        public async Task<CommandResult> Handle(EmitirLoteNfseCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var nota = new NotaServicoEletronica(
                request.Rps.Numero,
                request.Rps.Serie,
                request.Rps.Tipo,
                request.NumeroLote,
                request.Ambiente,
                request.NaturezaOperacao,
                request.RegimeEspecialTributacao,
                request.OptanteSimplesNacional,
                request.IncentivoFiscal,
                request.Competencia,
                request.Prestador.Documento,
                request.Prestador.InscricaoMunicipal,
                request.Prestador.CodigoMunicipioIbge,
                request.Tomador.Documento,
                request.Tomador.RazaoSocial,
                request.Servico.ItemListaServico,
                request.Servico.CodigoTributacaoMunicipio,
                request.Servico.CodigoCnae,
                request.Servico.CodigoNbs,
                request.Servico.Discriminacao,
                request.Servico.CodigoMunicipioIbge,
                request.Servico.ExigibilidadeIss,
                request.Servico.MunicipioIncidencia,
                request.Servico.ValorServicos,
                request.Servico.ValorDeducoes,
                request.Servico.ValorIss,
                request.Servico.ValorIssRetido,
                request.Servico.AliquotaIss,
                request.Servico.DescontoIncondicionado,
                request.Servico.DescontoCondicionado,
                request.Servico.IssRetido,
                tenantId,
                usuario);

            if (!nota.IsValid)
                return CommandResult.Falha(nota.Notifications.Select(n => n.Message));

            var input = new NfseEmissaoInput
            {
                NumeroLote = request.NumeroLote,
                Sincrono = request.Sincrono,
                Ambiente = request.Ambiente,
                NaturezaOperacao = request.NaturezaOperacao,
                RegimeEspecialTributacao = request.RegimeEspecialTributacao,
                OptanteSimplesNacional = request.OptanteSimplesNacional,
                IncentivoFiscal = request.IncentivoFiscal,
                Competencia = request.Competencia,
                RpsNumero = request.Rps.Numero,
                RpsSerie = request.Rps.Serie,
                RpsTipo = request.Rps.Tipo,
                Prestador = new NfsePrestadorInput
                {
                    Documento = request.Prestador.Documento,
                    Crt = request.Prestador.Crt,
                    InscricaoMunicipal = request.Prestador.InscricaoMunicipal,
                    RazaoSocial = request.Prestador.RazaoSocial,
                    CodigoMunicipioIbge = request.Prestador.CodigoMunicipioIbge,
                    Uf = request.Prestador.Uf
                },
                Tomador = new NfseTomadorInput
                {
                    Documento = request.Tomador.Documento,
                    InscricaoMunicipal = request.Tomador.InscricaoMunicipal,
                    RazaoSocial = request.Tomador.RazaoSocial
                },
                Servico = new NfseServicoInput
                {
                    ItemListaServico = request.Servico.ItemListaServico,
                    CodigoCnae = request.Servico.CodigoCnae,
                    CodigoTributacaoMunicipio = request.Servico.CodigoTributacaoMunicipio,
                    CodigoNbs = request.Servico.CodigoNbs,
                    Discriminacao = request.Servico.Discriminacao,
                    CodigoMunicipioIbge = request.Servico.CodigoMunicipioIbge,
                    ExigibilidadeIss = request.Servico.ExigibilidadeIss,
                    MunicipioIncidencia = request.Servico.MunicipioIncidencia,
                    ValorServicos = request.Servico.ValorServicos,
                    ValorDeducoes = request.Servico.ValorDeducoes,
                    ValorIss = request.Servico.ValorIss,
                    ValorIssRetido = request.Servico.ValorIssRetido,
                    AliquotaIss = request.Servico.AliquotaIss,
                    DescontoIncondicionado = request.Servico.DescontoIncondicionado,
                    DescontoCondicionado = request.Servico.DescontoCondicionado,
                    IssRetido = request.Servico.IssRetido
                }
            };

            var retorno = await _nfseService.EmitirLoteAsync(input, cancellationToken);

            if (retorno.Sucesso)
            {
                nota.MarcarEnviada(retorno.Protocolo, retorno.XmlEnvio);
                nota.Autorizar(retorno.NumeroNfse ?? string.Empty, retorno.CodigoVerificacao, retorno.Protocolo, retorno.StatusPrefeitura, retorno.XmlRetorno);
            }
            else
            {
                nota.MarcarEnviada(retorno.Protocolo, retorno.XmlEnvio);
                nota.Rejeitar(retorno.StatusPrefeitura, retorno.Motivo, retorno.XmlRetorno);
            }

            _context.NotasServicoEletronicas.Add(nota);
            await _context.SaveChangesAsync(cancellationToken);

            if (!retorno.Sucesso)
                return CommandResult.Falha(new[] { retorno.Motivo }, $"Falha ao emitir a NFS-e (Código: {retorno.StatusPrefeitura}).");

            return CommandResult.Ok("NFS-e emitida com sucesso!", new
            {
                nota.Id,
                nota.NumeroNfse,
                nota.CodigoVerificacao,
                nota.Protocolo,
                nota.Status
            });
        }
    }

    /// <summary>Consulta a situação de um lote. Fiel ao legado <c>NfseController.ConsultarLote</c>.</summary>
    public class ConsultarLoteNfseCommandHandler : ICommandHandler<ConsultarLoteNfseCommand>
    {
        private readonly INfseFiscalService _nfseService;
        public ConsultarLoteNfseCommandHandler(INfseFiscalService nfseService) => _nfseService = nfseService;

        public async Task<CommandResult> Handle(ConsultarLoteNfseCommand request, CancellationToken cancellationToken)
        {
            var retorno = await _nfseService.ConsultarLoteAsync(new NfseConsultaLoteInput
            {
                NumeroLote = request.NumeroLote,
                Protocolo = request.Protocolo,
                Ambiente = request.Ambiente,
                Prestador = new NfsePrestadorInput { Documento = request.Prestador.Documento, CodigoMunicipioIbge = request.Prestador.CodigoMunicipioIbge, Uf = request.Prestador.Uf }
            }, cancellationToken);

            if (!retorno.Sucesso)
                return CommandResult.Falha(new[] { retorno.Motivo }, $"Consulta de lote sem sucesso (Código: {retorno.StatusPrefeitura}).");

            return CommandResult.Ok("Consulta de lote realizada.", new { retorno.NumeroNfse, retorno.Protocolo, retorno.StatusPrefeitura, retorno.XmlRetorno });
        }
    }

    /// <summary>Consulta a NFS-e por RPS. Fiel ao legado <c>NfseController.ConsultarPorRps</c>.</summary>
    public class ConsultarNfsePorRpsCommandHandler : ICommandHandler<ConsultarNfsePorRpsCommand>
    {
        private readonly INfseFiscalService _nfseService;
        public ConsultarNfsePorRpsCommandHandler(INfseFiscalService nfseService) => _nfseService = nfseService;

        public async Task<CommandResult> Handle(ConsultarNfsePorRpsCommand request, CancellationToken cancellationToken)
        {
            var retorno = await _nfseService.ConsultarPorRpsAsync(new NfseConsultaRpsInput
            {
                NumeroRps = request.NumeroRps,
                Serie = request.Serie,
                Tipo = request.Tipo,
                Ambiente = request.Ambiente,
                Prestador = new NfsePrestadorInput { Documento = request.Prestador.Documento, CodigoMunicipioIbge = request.Prestador.CodigoMunicipioIbge, Uf = request.Prestador.Uf }
            }, cancellationToken);

            if (!retorno.Sucesso)
                return CommandResult.Falha(new[] { retorno.Motivo }, $"Consulta por RPS sem sucesso (Código: {retorno.StatusPrefeitura}).");

            return CommandResult.Ok("Consulta por RPS realizada.", new { retorno.NumeroNfse, retorno.CodigoVerificacao, retorno.Protocolo, retorno.StatusPrefeitura, retorno.XmlRetorno });
        }
    }

    /// <summary>
    /// Cancela uma NFS-e autorizada. Fiel ao legado <c>NfseController.CancelarNfse</c>. Se houver
    /// registro local da NFS-e (por número), atualiza o estado para Cancelada.
    /// </summary>
    public class CancelarNfseCommandHandler : ICommandHandler<CancelarNfseCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;
        private readonly INfseFiscalService _nfseService;

        public CancelarNfseCommandHandler(ContextFiscal context, ICurrentUser currentUser, INfseFiscalService nfseService)
        {
            _context = context;
            _currentUser = currentUser;
            _nfseService = nfseService;
        }

        public async Task<CommandResult> Handle(CancelarNfseCommand request, CancellationToken cancellationToken)
        {
            var retorno = await _nfseService.CancelarAsync(new NfseCancelamentoInput
            {
                NumeroNfse = request.NumeroNfse,
                CodigoCancelamento = request.CodigoCancelamento,
                Motivo = request.Motivo,
                Ambiente = request.Ambiente,
                Prestador = new NfsePrestadorInput { Documento = request.Prestador.Documento, CodigoMunicipioIbge = request.Prestador.CodigoMunicipioIbge, Uf = request.Prestador.Uf }
            }, cancellationToken);

            if (!retorno.Sucesso)
                return CommandResult.Falha(new[] { retorno.Motivo }, $"Falha ao cancelar a NFS-e (Código: {retorno.StatusPrefeitura}).");

            var usuario = _currentUser.GetUserId() ?? "system";
            var nota = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                _context.NotasServicoEletronicas, n => n.NumeroNfse == request.NumeroNfse, cancellationToken);

            if (nota != null)
            {
                nota.Cancelar(retorno.XmlRetorno, usuario);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return CommandResult.Ok("NFS-e cancelada com sucesso.", new { retorno.StatusPrefeitura, retorno.Motivo });
        }
    }
}
