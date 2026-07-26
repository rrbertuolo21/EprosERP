using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GRC.Application.Commands;
using Epros.Modules.GRC.Domain.Entities;
using Epros.Modules.GRC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GRC.Application.Handlers
{
    // ===================== Categoria =====================

    public class CriarCategoriaDenunciaCommandHandler : ICommandHandler<CriarCategoriaDenunciaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCategoriaDenunciaCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCategoriaDenunciaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // Unicidade funcional do nome por tenant (secao 10.4).
            var duplicado = await _context.DenunciaCategorias
                .AnyAsync(c => c.Nome == request.Nome, cancellationToken);
            if (duplicado)
            {
                return CommandResult.Falha("Já existe categoria de denúncia com este nome para o tenant.");
            }

            var categoria = new DenunciaCategoria(request.Nome, request.Descricao, request.Cor, request.CriadorId, tenantId, usuario);
            if (!categoria.IsValid)
            {
                return CommandResult.Falha(categoria.Notifications.Select(n => n.Message));
            }

            _context.DenunciaCategorias.Add(categoria);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Categoria de denúncia criada com sucesso!", new { CategoriaId = categoria.Id });
        }
    }

    public class InativarCategoriaDenunciaCommandHandler : ICommandHandler<InativarCategoriaDenunciaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ICurrentUser _currentUser;

        public InativarCategoriaDenunciaCommandHandler(ContextGRC context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(InativarCategoriaDenunciaCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var categoria = await _context.DenunciaCategorias.FirstOrDefaultAsync(c => c.Id == request.CategoriaId, cancellationToken);
            if (categoria == null)
            {
                return CommandResult.Falha("Categoria não encontrada.");
            }

            categoria.Inativar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Categoria inativada com sucesso!", new { CategoriaId = categoria.Id });
        }
    }

    // ===================== Registro detalhado =====================

    public class RegistrarDenunciaDetalhadaCommandHandler : ICommandHandler<RegistrarDenunciaDetalhadaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarDenunciaDetalhadaCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarDenunciaDetalhadaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            // Denuncia anonima nao identifica autor (RN-DEN-007).
            var usuario = request.Anonima ? "anonymous" : (_currentUser.GetUserId() ?? "system");

            // RN-DEN-004: categoria inativa nao pode ser usada em nova denuncia.
            if (request.CategoriaId.HasValue)
            {
                var categoria = await _context.DenunciaCategorias
                    .FirstOrDefaultAsync(c => c.Id == request.CategoriaId.Value, cancellationToken);
                if (categoria == null)
                {
                    return CommandResult.Falha("Categoria informada não encontrada.");
                }
                if (!categoria.Ativa)
                {
                    return CommandResult.Falha("A categoria informada está inativa e não pode ser usada.");
                }
            }

            var denuncia = new Denuncia(request.Titulo, request.Relato, request.CategoriaId, request.Prioridade, request.Anonima, tenantId, usuario);
            if (!denuncia.IsValid)
            {
                return CommandResult.Falha(denuncia.Notifications.Select(n => n.Message));
            }

            // Token de acompanhamento para canal anonimo (RN-DEN-007).
            if (request.Anonima)
            {
                var token = Guid.NewGuid().ToString("N");
                denuncia.DefinirTokenAcompanhamento(Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token))));
            }

            _context.Denuncias.Add(denuncia);
            RegistrarHistorico(denuncia.Id, "Registro", usuario, tenantId, $"{{\"protocolo\":\"{denuncia.CodigoAcompanhamento}\"}}");
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Denúncia registrada com sucesso!",
                new { DenunciaId = denuncia.Id, denuncia.CodigoAcompanhamento });
        }

        private void RegistrarHistorico(Guid denunciaId, string acao, string usuario, string tenantId, string payload)
        {
            var hist = new DenunciaHistorico(denunciaId, acao, null, null, payload, null, tenantId, usuario);
            if (hist.IsValid) _context.DenunciaHistoricos.Add(hist);
        }
    }

    // ===================== Triagem =====================

    public class TriarDenunciaCommandHandler : ICommandHandler<TriarDenunciaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public TriarDenunciaCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(TriarDenunciaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var denuncia = await _context.Denuncias.FirstOrDefaultAsync(d => d.Id == request.DenunciaId, cancellationToken);
            if (denuncia == null)
            {
                return CommandResult.Falha("Denúncia não encontrada.");
            }

            // RN-DEN-004: categoria inativa nao pode ser usada na reclassificacao.
            if (request.CategoriaId.HasValue)
            {
                var categoria = await _context.DenunciaCategorias
                    .FirstOrDefaultAsync(c => c.Id == request.CategoriaId.Value, cancellationToken);
                if (categoria == null || !categoria.Ativa)
                {
                    return CommandResult.Falha("A categoria informada está inativa ou não existe.");
                }
            }

            denuncia.Triar(request.CategoriaId, request.Prioridade, usuario);
            if (!denuncia.IsValid)
            {
                return CommandResult.Falha(denuncia.Notifications.Select(n => n.Message));
            }

            _context.DenunciaHistoricos.Add(new DenunciaHistorico(denuncia.Id, "Triagem", null, null, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Denúncia triada com sucesso!", new { DenunciaId = denuncia.Id, denuncia.Status });
        }
    }

    // ===================== Participantes =====================

    public class AdicionarParticipanteDenunciaCommandHandler : ICommandHandler<AdicionarParticipanteDenunciaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarParticipanteDenunciaCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarParticipanteDenunciaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var denunciaExiste = await _context.Denuncias.AnyAsync(d => d.Id == request.DenunciaId, cancellationToken);
            if (!denunciaExiste)
            {
                return CommandResult.Falha("Denúncia não encontrada.");
            }

            // Evita duplicidade do mesmo papel/pessoa (secao 10.4).
            if (request.PessoaId.HasValue)
            {
                var jaExiste = await _context.DenunciaParticipantes.AnyAsync(
                    p => p.DenunciaId == request.DenunciaId && p.PessoaId == request.PessoaId && p.Papel == request.Papel,
                    cancellationToken);
                if (jaExiste)
                {
                    return CommandResult.Falha("Este participante já está registrado com este papel.");
                }
            }

            var participante = new DenunciaParticipante(request.DenunciaId, request.PessoaId, request.Papel, request.NomeDeclarado, request.Sigiloso, tenantId, usuario);
            if (!participante.IsValid)
            {
                return CommandResult.Falha(participante.Notifications.Select(n => n.Message));
            }

            _context.DenunciaParticipantes.Add(participante);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Participante adicionado com sucesso!", new { ParticipanteId = participante.Id });
        }
    }

    // ===================== Investigacao segregada =====================

    public class AtribuirInvestigacaoCommandHandler : ICommandHandler<AtribuirInvestigacaoCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtribuirInvestigacaoCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtribuirInvestigacaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var denuncia = await _context.Denuncias.FirstOrDefaultAsync(d => d.Id == request.DenunciaId, cancellationToken);
            if (denuncia == null)
            {
                return CommandResult.Falha("Denúncia não encontrada.");
            }

            // RN-DEN-006 / DEN-SOD-001: investigador nao pode ser denunciado nem beneficiario do caso.
            var conflito = await _context.DenunciaParticipantes.AnyAsync(
                p => p.DenunciaId == request.DenunciaId &&
                     p.PessoaId == request.InvestigadorId &&
                     (p.Papel == "Denunciado" || p.Papel == "Beneficiario"),
                cancellationToken);
            if (conflito)
            {
                return CommandResult.Falha("Conflito de interesse: o investigador é denunciado ou beneficiário do caso (RN-DEN-006).");
            }

            var investigacao = new DenunciaInvestigacao(request.DenunciaId, request.InvestigadorId, request.PrazoSla, tenantId, usuario);
            if (!investigacao.IsValid)
            {
                return CommandResult.Falha(investigacao.Notifications.Select(n => n.Message));
            }

            // Registra o investigador como participante para trilha e futuras checagens de conflito.
            var investigadorParticipante = new DenunciaParticipante(request.DenunciaId, request.InvestigadorId, "Investigador", null, true, tenantId, usuario);

            denuncia.ColocarEmInvestigacao(usuario);
            if (!denuncia.IsValid)
            {
                return CommandResult.Falha(denuncia.Notifications.Select(n => n.Message));
            }

            _context.DenunciaInvestigacoes.Add(investigacao);
            _context.DenunciaParticipantes.Add(investigadorParticipante);
            _context.DenunciaHistoricos.Add(new DenunciaHistorico(denuncia.Id, "AtribuicaoInvestigacao", null, null, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Investigação atribuída com sucesso!", new { InvestigacaoId = investigacao.Id, denuncia.Status });
        }
    }

    public class ConcluirInvestigacaoCommandHandler : ICommandHandler<ConcluirInvestigacaoCommand>
    {
        private readonly ContextGRC _context;
        private readonly ICurrentUser _currentUser;

        public ConcluirInvestigacaoCommandHandler(ContextGRC context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ConcluirInvestigacaoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var investigacao = await _context.DenunciaInvestigacoes.FirstOrDefaultAsync(i => i.Id == request.InvestigacaoId, cancellationToken);
            if (investigacao == null)
            {
                return CommandResult.Falha("Investigação não encontrada.");
            }

            investigacao.Concluir(request.ConclusaoProposta, request.DataConclusao, usuario);
            if (!investigacao.IsValid)
            {
                return CommandResult.Falha(investigacao.Notifications.Select(n => n.Message));
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Investigação concluída com sucesso!", new { InvestigacaoId = investigacao.Id, investigacao.Status });
        }
    }

    // ===================== Respostas e anexos =====================

    public class ResponderDenunciaCommandHandler : ICommandHandler<ResponderDenunciaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ResponderDenunciaCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ResponderDenunciaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var denunciaExiste = await _context.Denuncias.AnyAsync(d => d.Id == request.DenunciaId, cancellationToken);
            if (!denunciaExiste)
            {
                return CommandResult.Falha("Denúncia não encontrada.");
            }

            var resposta = new DenunciaResposta(request.DenunciaId, request.Mensagem, request.Interna, null, tenantId, usuario);
            if (!resposta.IsValid)
            {
                return CommandResult.Falha(resposta.Notifications.Select(n => n.Message));
            }

            _context.DenunciaRespostas.Add(resposta);
            _context.DenunciaHistoricos.Add(new DenunciaHistorico(request.DenunciaId, request.Interna ? "RespostaInterna" : "RespostaExterna", null, null, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Resposta registrada com sucesso!", new { RespostaId = resposta.Id, resposta.Interna });
        }
    }

    public class AnexarEvidenciaDenunciaCommandHandler : ICommandHandler<AnexarEvidenciaDenunciaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AnexarEvidenciaDenunciaCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AnexarEvidenciaDenunciaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var denunciaExiste = await _context.Denuncias.AnyAsync(d => d.Id == request.DenunciaId, cancellationToken);
            if (!denunciaExiste)
            {
                return CommandResult.Falha("Denúncia não encontrada.");
            }

            var anexo = new DenunciaAnexo(request.DenunciaId, request.RespostaId, request.ArquivoId, request.Sigiloso, null, tenantId, usuario);
            if (!anexo.IsValid)
            {
                return CommandResult.Falha(anexo.Notifications.Select(n => n.Message));
            }

            _context.DenunciaAnexos.Add(anexo);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Evidência anexada com sucesso!", new { AnexoId = anexo.Id, anexo.Sigiloso });
        }
    }

    // ===================== Conclusao =====================

    public class ConcluirDenunciaCommandHandler : ICommandHandler<ConcluirDenunciaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ConcluirDenunciaCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ConcluirDenunciaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var denuncia = await _context.Denuncias.FirstOrDefaultAsync(d => d.Id == request.DenunciaId, cancellationToken);
            if (denuncia == null)
            {
                return CommandResult.Falha("Denúncia não encontrada.");
            }

            denuncia.Concluir(request.ResolvedAt, request.ParecerFinal, usuario);
            if (!denuncia.IsValid)
            {
                return CommandResult.Falha(denuncia.Notifications.Select(n => n.Message));
            }

            _context.DenunciaHistoricos.Add(new DenunciaHistorico(denuncia.Id, "Conclusao", null, null, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Denúncia concluída com sucesso!", new { DenunciaId = denuncia.Id, denuncia.Status, denuncia.ResolvedAt });
        }
    }

    // ===================== Parametros =====================

    public class DefinirParametroDenunciaCommandHandler : ICommandHandler<DefinirParametroDenunciaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DefinirParametroDenunciaCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DefinirParametroDenunciaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // RN unicidade: parametro unico por tenant/chave — atualiza se existir.
            var existente = await _context.DenunciaParametros.FirstOrDefaultAsync(p => p.Chave == request.Chave, cancellationToken);
            if (existente != null)
            {
                existente.Alterar(request.ValorJson, true, usuario);
                await _context.SaveChangesAsync(cancellationToken);
                return CommandResult.Ok("Parâmetro atualizado com sucesso!", new { ParametroId = existente.Id });
            }

            var parametro = new DenunciaParametro(request.Chave, request.ValorJson, tenantId, usuario);
            if (!parametro.IsValid)
            {
                return CommandResult.Falha(parametro.Notifications.Select(n => n.Message));
            }

            _context.DenunciaParametros.Add(parametro);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Parâmetro criado com sucesso!", new { ParametroId = parametro.Id });
        }
    }
}
