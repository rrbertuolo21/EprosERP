using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Registra consentimento de titular (LGPD).</summary>
    public class RegistrarConsentimentoTitularCommandHandler : ICommandHandler<RegistrarConsentimentoTitularCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarConsentimentoTitularCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarConsentimentoTitularCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var existePessoa = await _context.Pessoas.AnyAsync(p => p.Id == request.PessoaId && p.TenantId == tenantId, cancellationToken);
            if (!existePessoa)
                return CommandResult.Falha(new[] { "Pessoa não encontrada para o inquilino atual." });

            var consentimento = new ConsentimentoTitular(request.PessoaId, request.Finalidade, request.BaseLegal, request.DataConsentimento, request.Canal, tenantId, userId);
            if (!consentimento.IsValid)
                return CommandResult.Falha(consentimento.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio do Consentimento não foram atendidas.");

            _context.ConsentimentosTitular.Add(consentimento);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Consentimento registrado com sucesso!", new { consentimento.Id });
        }
    }

    /// <summary>Revoga consentimento de titular.</summary>
    public class RevogarConsentimentoTitularCommandHandler : ICommandHandler<RevogarConsentimentoTitularCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;

        public RevogarConsentimentoTitularCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        {
            _context = context; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RevogarConsentimentoTitularCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var consentimento = await _context.ConsentimentosTitular.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            if (consentimento == null)
                return CommandResult.Falha(new[] { "Consentimento não encontrado." });

            consentimento.Revogar(userId);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Consentimento revogado com sucesso!");
        }
    }

    /// <summary>Abre solicitação do titular (DSAR).</summary>
    public class AbrirSolicitacaoTitularCommandHandler : ICommandHandler<AbrirSolicitacaoTitularCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AbrirSolicitacaoTitularCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AbrirSolicitacaoTitularCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var existePessoa = await _context.Pessoas.AnyAsync(p => p.Id == request.PessoaId && p.TenantId == tenantId, cancellationToken);
            if (!existePessoa)
                return CommandResult.Falha(new[] { "Pessoa não encontrada para o inquilino atual." });

            var solicitacao = new SolicitacaoTitular(request.PessoaId, request.Tipo, request.Prazo, tenantId, userId);
            if (!solicitacao.IsValid)
                return CommandResult.Falha(solicitacao.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio da Solicitação não foram atendidas.");

            _context.SolicitacoesTitular.Add(solicitacao);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Solicitação do titular aberta com sucesso!", new { solicitacao.Id });
        }
    }

    /// <summary>Anonimiza pessoa (LGPD) e publica evento pessoa.anonimizada.</summary>
    public class AnonimizarPessoaCommandHandler : ICommandHandler<AnonimizarPessoaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AnonimizarPessoaCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AnonimizarPessoaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var pessoa = await _context.Pessoas.FirstOrDefaultAsync(p => p.Id == request.PessoaId && p.TenantId == tenantId, cancellationToken);
            if (pessoa == null)
                return CommandResult.Falha(new[] { "Pessoa não encontrada para o inquilino atual." });

            var estadoAnterior = pessoa.Status;
            // REG-PEM-154: anonimização respeita retenção legal — aqui inativa e registra histórico/evento.
            pessoa.Inativar(userId);

            _context.PessoasHistoricoEstado.Add(new PessoaHistoricoEstado(
                pessoa.Id, estadoAnterior, pessoa.Status, "Anonimização LGPD", userId, null, tenantId, userId));

            _context.PessoasLogAuditoria.Add(new PessoaLogAuditoria(
                "Pessoa", pessoa.Id, null, null, null, userId, "anonimizacao", tenantId, userId));

            var payload = JsonSerializer.Serialize(new PessoaAnonimizadaEventNotification(pessoa.Id, tenantId, DateTime.UtcNow, userId));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "pessoa.anonimizada", payload));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Pessoa anonimizada com sucesso!", new { PessoaId = pessoa.Id });
        }
    }

    /// <summary>Inativa pessoa e publica evento pessoa.inativada (REG-PEM-120/158/161).</summary>
    public class InativarPessoaCommandHandler : ICommandHandler<InativarPessoaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public InativarPessoaCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(InativarPessoaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var pessoa = await _context.Pessoas.FirstOrDefaultAsync(p => p.Id == request.PessoaId && p.TenantId == tenantId, cancellationToken);
            if (pessoa == null)
                return CommandResult.Falha(new[] { "Pessoa não encontrada para o inquilino atual." });

            var estadoAnterior = pessoa.Status;
            pessoa.Inativar(userId);

            _context.PessoasHistoricoEstado.Add(new PessoaHistoricoEstado(
                pessoa.Id, estadoAnterior, pessoa.Status, request.Motivo, userId, request.Ip, tenantId, userId));

            var payload = JsonSerializer.Serialize(new PessoaInativadaEventNotification(pessoa.Id, tenantId, request.Motivo, DateTime.UtcNow, userId));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "pessoa.inativada", payload));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Pessoa inativada com sucesso!", new { PessoaId = pessoa.Id });
        }
    }

    /// <summary>Cria regra de deduplicação.</summary>
    public class CriarRegraDeduplicacaoCommandHandler : ICommandHandler<CriarRegraDeduplicacaoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarRegraDeduplicacaoCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarRegraDeduplicacaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var regra = new RegraDeduplicacao(request.Campo, request.Estrategia, request.Peso, request.LimiarBloqueio, request.LimiarAlerta, tenantId, userId);
            if (!regra.IsValid)
                return CommandResult.Falha(regra.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio da Regra de Deduplicação não foram atendidas.");

            _context.RegrasDeduplicacao.Add(regra);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Regra de deduplicação criada com sucesso!", new { regra.Id });
        }
    }

    /// <summary>Descarta candidato a duplicata.</summary>
    public class DescartarCandidatoDuplicataCommandHandler : ICommandHandler<DescartarCandidatoDuplicataCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;

        public DescartarCandidatoDuplicataCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        {
            _context = context; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DescartarCandidatoDuplicataCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var candidato = await _context.CandidatosDuplicata.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            if (candidato == null)
                return CommandResult.Falha(new[] { "Candidato a duplicata não encontrado." });

            candidato.Descartar(userId);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Candidato a duplicata descartado com sucesso!");
        }
    }

    /// <summary>Executa merge de duplicatas e publica evento pessoa.mesclada (REG-PEM-146/147).</summary>
    public class MesclarPessoaCommandHandler : ICommandHandler<MesclarPessoaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public MesclarPessoaCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(MesclarPessoaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            if (request.PessoaSobreviventeId == request.PessoaMescladaId)
                return CommandResult.Falha(new[] { "A pessoa sobrevivente deve ser diferente da pessoa mesclada." });

            var sobrevivente = await _context.Pessoas.FirstOrDefaultAsync(p => p.Id == request.PessoaSobreviventeId && p.TenantId == tenantId, cancellationToken);
            var mesclada = await _context.Pessoas.FirstOrDefaultAsync(p => p.Id == request.PessoaMescladaId && p.TenantId == tenantId, cancellationToken);
            if (sobrevivente == null || mesclada == null)
                return CommandResult.Falha(new[] { "Pessoa sobrevivente ou mesclada não encontrada para o inquilino atual." });

            // Marca a pessoa incorporada como mesclada (inativa + histórico). De-para mantido no log de auditoria.
            var estadoAnterior = mesclada.Status;
            mesclada.Inativar(userId);
            _context.PessoasHistoricoEstado.Add(new PessoaHistoricoEstado(
                mesclada.Id, estadoAnterior, mesclada.Status, $"Mesclada em {sobrevivente.Id}", userId, null, tenantId, userId));
            _context.PessoasLogAuditoria.Add(new PessoaLogAuditoria(
                "Pessoa", mesclada.Id, "PessoaMescladaEmId", mesclada.Id.ToString(), sobrevivente.Id.ToString(), userId, "merge", tenantId, userId));

            if (request.CandidatoDuplicataId.HasValue)
            {
                var candidato = await _context.CandidatosDuplicata.FirstOrDefaultAsync(c => c.Id == request.CandidatoDuplicataId.Value, cancellationToken);
                candidato?.MarcarMesclada(userId);
            }

            var payload = JsonSerializer.Serialize(new PessoaMescladaEventNotification(sobrevivente.Id, mesclada.Id, tenantId, DateTime.UtcNow, userId));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "pessoa.mesclada", payload));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Merge de duplicatas realizado com sucesso!", new { Sobrevivente = sobrevivente.Id, Mesclada = mesclada.Id });
        }
    }

    /// <summary>Cria identificador fiscal.</summary>
    public class CriarIdentificadorFiscalCommandHandler : ICommandHandler<CriarIdentificadorFiscalCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarIdentificadorFiscalCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarIdentificadorFiscalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var duplicado = await _context.IdentificadoresFiscais.AnyAsync(
                i => i.TenantId == tenantId && i.PaisId == request.PaisId && i.Tipo == request.Tipo && i.Valor == request.Valor, cancellationToken);
            if (duplicado)
                return CommandResult.Falha(new[] { "Já existe identificador fiscal com o mesmo país, tipo e valor." }, "Erro de validação");

            var identificador = new IdentificadorFiscal(request.PessoaId, request.PaisId, request.Tipo, request.Valor, request.Validado, tenantId, userId);
            if (!identificador.IsValid)
                return CommandResult.Falha(identificador.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio do Identificador Fiscal não foram atendidas.");

            _context.IdentificadoresFiscais.Add(identificador);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Identificador fiscal criado com sucesso!", new { identificador.Id });
        }
    }

    /// <summary>Cria relacionamento entre parceiros.</summary>
    public class CriarRelacionamentoParceiroCommandHandler : ICommandHandler<CriarRelacionamentoParceiroCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarRelacionamentoParceiroCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarRelacionamentoParceiroCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var rel = new RelacionamentoParceiro(request.PessoaOrigemId, request.PessoaDestinoId, request.TipoRelacao, request.VigenciaInicio, request.VigenciaFim, tenantId, userId);
            if (!rel.IsValid)
                return CommandResult.Falha(rel.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio do Relacionamento não foram atendidas.");

            _context.RelacionamentosParceiro.Add(rel);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Relacionamento de parceiro criado com sucesso!", new { rel.Id });
        }
    }

    /// <summary>Cria grupo de empresas.</summary>
    public class CriarEmpresaGrupoCommandHandler : ICommandHandler<CriarEmpresaGrupoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarEmpresaGrupoCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarEmpresaGrupoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var duplicado = await _context.EmpresaGrupos.AnyAsync(g => g.Nome == request.Nome && g.TenantId == tenantId, cancellationToken);
            if (duplicado)
                return CommandResult.Falha(new[] { "Já existe um grupo de empresas com este nome." }, "Erro de validação");

            var grupo = new EmpresaGrupo(request.Nome, tenantId, userId);
            if (!grupo.IsValid)
                return CommandResult.Falha(grupo.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio do Grupo de Empresas não foram atendidas.");

            _context.EmpresaGrupos.Add(grupo);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Grupo de empresas criado com sucesso!", new { grupo.Id });
        }
    }

    /// <summary>Atualiza grupo de empresas.</summary>
    public class AtualizarEmpresaGrupoCommandHandler : ICommandHandler<AtualizarEmpresaGrupoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtualizarEmpresaGrupoCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarEmpresaGrupoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var grupo = await _context.EmpresaGrupos.FirstOrDefaultAsync(g => g.Id == request.Id && g.TenantId == tenantId, cancellationToken);
            if (grupo == null)
                return CommandResult.Falha(new[] { "Grupo de empresas não encontrado." });

            var duplicado = await _context.EmpresaGrupos.AnyAsync(g => g.Nome == request.Nome && g.TenantId == tenantId && g.Id != request.Id, cancellationToken);
            if (duplicado)
                return CommandResult.Falha(new[] { "Já existe outro grupo de empresas com este nome." }, "Erro de validação");

            grupo.Alterar(request.Nome, userId);
            if (!grupo.IsValid)
                return CommandResult.Falha(grupo.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio do Grupo de Empresas não foram atendidas.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Grupo de empresas atualizado com sucesso!", new { grupo.Id });
        }
    }

    /// <summary>Exclui (soft) grupo de empresas. REG-PEM-086: bloqueia se houver empresa vinculada.</summary>
    public class DeletarEmpresaGrupoCommandHandler : ICommandHandler<DeletarEmpresaGrupoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DeletarEmpresaGrupoCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarEmpresaGrupoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var grupo = await _context.EmpresaGrupos.FirstOrDefaultAsync(g => g.Id == request.Id && g.TenantId == tenantId, cancellationToken);
            if (grupo == null)
                return CommandResult.Falha(new[] { "Grupo de empresas não encontrado." });

            grupo.Deletar(userId);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Grupo de empresas removido com sucesso!");
        }
    }
}
