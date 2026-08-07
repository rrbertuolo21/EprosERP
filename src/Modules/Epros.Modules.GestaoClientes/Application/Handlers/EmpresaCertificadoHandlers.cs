using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Handler de aplicação (UploadCertificadoDigitalCommandHandler).</summary>
    public class UploadCertificadoDigitalCommandHandler : ICommandHandler<UploadCertificadoDigitalCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly ISegredoCofreService _cofreService;

        public UploadCertificadoDigitalCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            ISegredoCofreService cofreService)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _cofreService = cofreService;
        }

        public async Task<CommandResult> Handle(UploadCertificadoDigitalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuarioId = _currentUser.GetUserId() ?? "system";

            var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.Id == request.EmpresaId && e.TenantId == tenantId, cancellationToken);
            if (empresa == null)
                return CommandResult.Falha(new[] { "Empresa não encontrada" });

            // REG-PEM-103: Exige arquivo e senha
            if (string.IsNullOrWhiteSpace(request.ArquivoBase64) || string.IsNullOrWhiteSpace(request.Senha))
                return CommandResult.Falha(new[] { "O arquivo base64 do certificado e a senha são obrigatórios." });

            byte[] certBytes;
            try
            {
                certBytes = Convert.FromBase64String(request.ArquivoBase64);
            }
            catch (Exception)
            {
                return CommandResult.Falha(new[] { "O arquivo do certificado não está no formato base64 válido." });
            }

            // REG-PEM-104: Validar antes de gravar
            System.Security.Cryptography.X509Certificates.X509Certificate2 cert;
            try
            {
                cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(certBytes, request.Senha);
            }
            catch (Exception ex)
            {
                return CommandResult.Falha(new[] { $"Falha ao carregar e validar o certificado: {ex.Message}" });
            }

            // REG-PEM-105: Registrar serial, titular, informacao, CNPJ, validade
            var serial = cert.SerialNumber;
            var titular = cert.Subject;
            var informacao = cert.ToString();
            var validadeInicial = cert.NotBefore;
            var validadeFinal = cert.NotAfter;

            // Extrair CNPJ (14 dígitos consecutivos no Common Name ou Subject)
            var matchCnpj = System.Text.RegularExpressions.Regex.Match(titular, @"\d{14}");
            string? cnpjExtraido = matchCnpj.Success ? matchCnpj.Value : null;

            // REG-PEM-106: CNPJ deve ser conferido contra a empresa ou matriz/filial (8 primeiros dígitos coincidentes)
            if (cnpjExtraido == null)
                return CommandResult.Falha(new[] { "Não foi possível extrair o CNPJ do certificado digital." });

            var cleanEmpresaCnpj = new string(empresa.Cnpj.Where(char.IsDigit).ToArray());
            var cleanCertCnpj = new string(cnpjExtraido.Where(char.IsDigit).ToArray());

            if (cleanEmpresaCnpj.Length >= 8 && cleanCertCnpj.Length >= 8)
            {
                var baseEmpresa = cleanEmpresaCnpj.Substring(0, 8);
                var baseCert = cleanCertCnpj.Substring(0, 8);
                if (baseEmpresa != baseCert)
                    return CommandResult.Falha(new[] { $"O CNPJ do certificado ({cnpjExtraido}) não coincide com o CNPJ da empresa ({empresa.Cnpj}) ou sua matriz/filial." });
            }
            else
            {
                return CommandResult.Falha(new[] { "CNPJ da empresa ou do certificado com formato inválido." });
            }

            // Criptografar segredos no cofre
            var certSegredoId = Guid.NewGuid();
            var senhaSegredoId = Guid.NewGuid();

            var encryptedFile = await _cofreService.CriptografarAsync(request.ArquivoBase64);
            var encryptedSenha = await _cofreService.CriptografarAsync(request.Senha);

            var configCert = new ConfiguracaoGlobal($"certificado.{certSegredoId}", encryptedFile, true, $"Certificado da empresa {empresa.RazaoSocial}", tenantId, usuarioId);
            var configSenha = new ConfiguracaoGlobal($"senha_certificado.{senhaSegredoId}", encryptedSenha, true, $"Senha do certificado da empresa {empresa.RazaoSocial}", tenantId, usuarioId);

            _context.ConfiguracoesGlobais.Add(configCert);
            _context.ConfiguracoesGlobais.Add(configSenha);

            var certificado = new EmpresaCertificado(
                request.EmpresaId,
                certSegredoId,
                senhaSegredoId,
                serial,
                titular,
                informacao,
                cnpjExtraido,
                validadeInicial,
                validadeFinal,
                tenantId,
                usuarioId
            );

            _context.EmpresasCertificados.Add(certificado);
            await _context.SaveChangesAsync(cancellationToken);

            // Atualiza a empresa com o certificado ativo
            empresa.Atualizar(
                empresa.RazaoSocial,
                empresa.NomeFantasia,
                empresa.InscricaoEstadual,
                empresa.InscricaoMunicipal,
                empresa.InscricaoSuframa,
                empresa.Cnae,
                empresa.RegimeTributario,
                empresa.RegimeApuracao,
                empresa.PessoaGrupoId,
                empresa.ProdutoGrupoId,
                empresa.PlanoContasFinanceiroId,
                empresa.TributarioGrupoId,
                empresa.NcmTributacaoId,
                certificado.Id,
                empresa.EmpresaParametrosDfeId,
                empresa.LinkWebApiAppVendas,
                empresa.TokenMercadoPagoPix,
                empresa.Logo,
                empresa.Endereco,
                usuarioId,
                empresa.EhMei,
                empresa.DateFormat,
                empresa.TimeZoneId,
                empresa.CurrencyId,
                empresa.Cpf,
                empresa.EhIndustria,
                empresa.ContadorId,
                empresa.TipoConfiguracaoEstoque
            );

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Certificado digital carregado e associado com sucesso!", new { CertificadoId = certificado.Id });
        }
    }

    /// <summary>Handler de aplicação (TestarEmailEmpresaCommandHandler).</summary>
    public class TestarEmailEmpresaCommandHandler : ICommandHandler<TestarEmailEmpresaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ISegredoCofreService _cofreService;

        public TestarEmailEmpresaCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ISegredoCofreService cofreService)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _cofreService = cofreService;
        }

        public async Task<CommandResult> Handle(TestarEmailEmpresaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var emailConfig = await _context.ConfiguracoesEmail
                .FirstOrDefaultAsync(c => c.TenantId == tenantId, cancellationToken);

            if (emailConfig == null)
                return CommandResult.Falha(new[] { "Configuração de e-mail não encontrada para este inquilino." });

            if (string.IsNullOrWhiteSpace(emailConfig.Host) || !emailConfig.Port.HasValue || string.IsNullOrWhiteSpace(emailConfig.Username))
                return CommandResult.Falha(new[] { "Configuração de e-mail incompleta (Host, Porta ou Usuário ausentes)." });

            var password = emailConfig.Password;
            if (!string.IsNullOrEmpty(password) && (password.StartsWith("vault:v1:") || password.StartsWith("local:v1:")))
                password = await _cofreService.DescriptografarAsync(password);

            // REG-PEM-097: Validar host, porta, SSL, credenciais e timeout.
            // ConnectAsync com CancellationToken (não WhenAny+Dispose): DNS lento + Dispose
            // enquanto ConnectAsync pendente travava o testhost no CI (~5 min blame-hang).
            try
            {
                using (var tcpCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    tcpCts.CancelAfter(TimeSpan.FromSeconds(5));
                    try
                    {
                        using var tcpClient = new System.Net.Sockets.TcpClient();
                        await tcpClient.ConnectAsync(emailConfig.Host, emailConfig.Port.Value, tcpCts.Token);
                    }
                    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                    {
                        return CommandResult.Falha(new[] { "Tempo limite excedido ao tentar conectar ao servidor SMTP (Timeout)." });
                    }
                    catch (System.Net.Sockets.SocketException ex)
                    {
                        return CommandResult.Falha(new[] { $"Falha na conexão TCP com o servidor SMTP: {ex.Message}" });
                    }
                }

                var mailMessage = new System.Net.Mail.MailMessage
                {
                    From = new System.Net.Mail.MailAddress(emailConfig.FromEmail ?? "noreply@epros.com"),
                    Subject = "Epros ERP - Teste de Configuração SMTP",
                    Body = "Este é um e-mail de teste automático do Epros ERP para validar suas configurações SMTP.",
                    IsBodyHtml = false
                };
                mailMessage.To.Add(emailConfig.FromEmail ?? emailConfig.Username);

                using (var smtpClient = new System.Net.Mail.SmtpClient(emailConfig.Host)
                {
                    Port = emailConfig.Port.Value,
                    Credentials = new System.Net.NetworkCredential(emailConfig.Username, password),
                    EnableSsl = true,
                    Timeout = 5000
                })
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                return CommandResult.Falha(new[] { $"Falha na validação SMTP: {ex.Message}" });
            }

            return CommandResult.Ok("Configurações SMTP validadas com sucesso! E-mail de teste enviado.");
        }
    }
}
