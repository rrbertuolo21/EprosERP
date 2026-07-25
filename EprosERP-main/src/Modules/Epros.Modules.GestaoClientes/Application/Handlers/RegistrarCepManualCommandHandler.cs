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
    /// <summary>Registra Cep Manual.</summary>
    public class RegistrarCepManualCommandHandler : ICommandHandler<RegistrarCepManualCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;

        public RegistrarCepManualCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarCepManualCommand request, CancellationToken cancellationToken)
        {
            var criadoPor = _currentUser.GetUserId() ?? "system";

            if (string.IsNullOrWhiteSpace(request.Cep))
                return CommandResult.Falha(new[] { "O CEP é obrigatório." }, "Erro de validação");
            if (string.IsNullOrWhiteSpace(request.Logradouro))
                return CommandResult.Falha(new[] { "O logradouro é obrigatório." }, "Erro de validação");
            if (string.IsNullOrWhiteSpace(request.Justificativa))
                return CommandResult.Falha(new[] { "A justificativa é obrigatória." }, "Erro de validação");

            var cepNormalizado = request.Cep.Replace("-", "").Replace(" ", "").Trim();
            if (cepNormalizado.Length != 8)
                return CommandResult.Falha(new[] { "O CEP brasileiro deve conter exatamente 8 dígitos." }, "Erro de validação");

            // 1. Obter Brasil
            var brasil = await _context.Paises.FirstOrDefaultAsync(p => p.CodigoIsoAlpha2 == "BR", cancellationToken);
            if (brasil == null)
                return CommandResult.Falha(new[] { "País Brasil não cadastrado." }, "Erro de configuração");

            // 2. Verificar se o município informado é válido e ativo
            var mun = await _context.Municipios.FirstOrDefaultAsync(m => m.Id == request.MunicipioId, cancellationToken);
            if (mun == null)
                return CommandResult.Falha(new[] { "Município informado não encontrado." }, "Erro de validação");
            if (!mun.Ativo)
                return CommandResult.Falha(new[] { "Município informado está inativo." }, "Erro de validação");

            // 3. Excluir cache anterior se houver
            var cacheExistente = await _context.CodigosPostaisCache
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.PaisId == brasil.Id && c.CodigoPostal == cepNormalizado, cancellationToken);

            if (cacheExistente != null)
            {
                _context.CodigosPostaisCache.Remove(cacheExistente);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // 4. Criar e persistir o registro manual de cache com justificativa
            var cacheManual = CodigoPostalCache.CriarManual(
                brasil.Id,
                cepNormalizado,
                request.Logradouro,
                request.Bairro,
                request.MunicipioId,
                request.Uf.ToUpperInvariant(),
                request.Justificativa,
                criadoPor
            );

            if (!cacheManual.IsValid)
            {
                var erros = cacheManual.Notifications.Select(n => n.Message).Distinct();
                return CommandResult.Falha(erros, "Invariantes de domínio do CEP não atendidas.");
            }

            _context.CodigosPostaisCache.Add(cacheManual);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("CEP registrado manualmente com sucesso!", new
            {
                Cep = cepNormalizado,
                Logradouro = request.Logradouro,
                Bairro = request.Bairro,
                Localidade = mun.Nome,
                Uf = request.Uf,
                MunicipioId = mun.Id,
                PaisId = brasil.Id
            });
        }
    }
}
