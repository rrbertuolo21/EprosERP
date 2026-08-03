using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Domain.ValueObjects;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Handler de aplicação (UpdateEmpresaCommandHandler).</summary>
    public class UpdateEmpresaCommandHandler : ICommandHandler<AtualizarEmpresaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly ISegredoCofreService _cofreService;

        public UpdateEmpresaCommandHandler(
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

        public async Task<CommandResult> Handle(AtualizarEmpresaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var empresa = await _context.Empresas
                .FirstOrDefaultAsync(e => e.Id == request.Id && e.TenantId == tenantId, cancellationToken);

            if (empresa == null)
            {
                return CommandResult.Falha(new[] { "Empresa não encontrada" }, "Erro de validação");
            }

            // Uniqueness validation on update
            if (string.IsNullOrWhiteSpace(request.Cnpj))
            {
                return CommandResult.Falha(new[] { "CNPJ inválido" }, "Erro de validação");
            }
            var tempCnpj = new Cnpj(request.Cnpj);
            if (!tempCnpj.IsValid)
            {
                return CommandResult.Falha(new[] { "CNPJ inválido" }, "Erro de validação");
            }

            var existeCnpj = await _context.Empresas.AnyAsync(e => e.Cnpj == tempCnpj.Valor && e.Id != empresa.Id && e.TenantId == tenantId, cancellationToken);
            if (existeCnpj)
            {
                return CommandResult.Falha(new[] { "CNPJ já cadastrado!" }, "Erro de validação");
            }

            // Mapeia Endereco VO
            var endereco = new Epros.Modules.GestaoClientes.Domain.ValueObjects.Endereco(
                request.Endereco.Logradouro,
                request.Endereco.Numero,
                request.Endereco.Complemento,
                request.Endereco.Bairro,
                request.Endereco.Cep,
                request.Endereco.Cidade,
                request.Endereco.Estado
            );

            // T-01/P1-2: preserva o segredo cifrado quando a UI devolve a máscara (ou vazio); só cifra e
            // substitui quando um NOVO token é informado. Empresa.Atualizar sempre sobrescreve o campo, então
            // aqui garantimos que o valor persistido seja o ciphertext atual ou o novo ciphertext — nunca claro.
            var tokenAlterado = !string.IsNullOrEmpty(request.TokenMercadoPagoPix) && request.TokenMercadoPagoPix != Empresa.MascaraTokenPix;
            var tokenParaPersistir = tokenAlterado
                ? await _cofreService.CriptografarAsync(request.TokenMercadoPagoPix!)
                : empresa.TokenMercadoPagoPix;

            // Atualiza Empresa
            empresa.Atualizar(
                request.RazaoSocial,
                request.NomeFantasia,
                request.InscricaoEstadual,
                request.InscricaoMunicipal,
                request.InscricaoSuframa,
                request.Cnae,
                request.RegimeTributario,
                request.RegimeApuracao,
                request.PessoaGrupoId,
                request.ProdutoGrupoId,
                request.PlanoContasFinanceiroId,
                request.TributarioGrupoId,
                request.NcmTributacaoId,
                request.CertificadoDigitalId,
                request.EmpresaParametrosDfeId,
                request.LinkWebApiAppVendas,
                tokenParaPersistir,
                request.Logo,
                endereco,
                alteradoPor,
                false,
                request.DateFormat ?? "MM-DD-YYYY",
                request.TimeZoneId,
                request.CurrencyId
            );

            // Valida invariantes de domínio
            if (!empresa.IsValid)
            {
                var erros = empresa.Notifications.Select(n => n.Message).Distinct();
                return CommandResult.Falha(erros, "Invariantes de domínio da Empresa não foram atendidas.");
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Empresa atualizada com sucesso!", new { EmpresaId = empresa.Id });
        }
    }
}
