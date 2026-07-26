using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Domain.ValueObjects;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Security;
using Microsoft.AspNetCore.Http;
// Desambiguação: HistoricoLogin existe em Aplicativo e GestaoClientes; aqui persiste em ContextAplicativo.HistoricosLogin.
using HistoricoLogin = Epros.Modules.Aplicativo.Domain.Entities.HistoricoLogin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    public class AutenticarUsuarioCommandHandler : ICommandHandler<AutenticarUsuarioCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ContextGestaoClientes _contextGestao;
        private readonly ICurrentUser _currentUser;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEprosTokenService _tokenService;

        public AutenticarUsuarioCommandHandler(
            ContextAplicativo context,
            ContextGestaoClientes contextGestao,
            ICurrentUser currentUser,
            IPasswordHasher passwordHasher,
            IHttpContextAccessor httpContextAccessor,
            IEprosTokenService tokenService)
        {
            _context = context;
            _contextGestao = contextGestao;
            _currentUser = currentUser;
            _passwordHasher = passwordHasher;
            _httpContextAccessor = httpContextAccessor;
            _tokenService = tokenService;
        }

        public async Task<CommandResult> Handle(AutenticarUsuarioCommand request, CancellationToken cancellationToken)
        {
            var emailLower = request.Email.ToLowerInvariant().Trim();

            // 1. Verificar se o IP está banido
            var ipBanido = await _context.BannedIps
                .FirstOrDefaultAsync(b => b.IpAddress == request.IpAddress && b.DeletadoEm == null, cancellationToken);

            if (ipBanido != null && !ipBanido.EstaExpirado())
            {
                return CommandResult.Falha(new[] { "Acesso temporariamente bloqueado." });
            }

            // 2. Tentar localizar o usuário (cross-tenant: login anônimo não conhece o inquilino)
            await AuthRlsBypass.EnableAsync(_context, cancellationToken);
            var usuario = await _context.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == emailLower && u.DeletadoEm == null, cancellationToken);
            await AuthRlsBypass.DisableAsync(_context, cancellationToken);

            if (usuario != null)
            {
                DefinirTenantDaRequisicao(usuario.TenantId);
            }

            if (usuario == null)
            {
                // Grava falha anonimizada na auditoria
                var historicoFalha = new HistoricoLogin("system", null, emailLower, request.IpAddress, request.UserAgent, false, "Usuário não encontrado.", "system");
                _context.HistoricosLogin.Add(historicoFalha);
                await _context.SaveChangesAsync(cancellationToken);

                return CommandResult.Falha(new[] { "E-mail ou senha incorretos." });
            }

            // 3. Verificar bloqueio temporário (Lockout)
            if (usuario.LockoutEnd.HasValue && usuario.LockoutEnd.Value > DateTime.UtcNow)
            {
                return CommandResult.Falha(new[] { "Conta temporariamente bloqueada." });
            }

            // 4. Validar senha (hash PBKDF2 — comparação em tempo constante)
            if (!_passwordHasher.Verify(request.Senha, usuario.PasswordHash))
            {
                // Registra tentativa falha e lockout se atingir limite
                usuario.RegistrarFalhaLogin(5, TimeSpan.FromMinutes(15));
                
                var historicoFalha = new HistoricoLogin(usuario.TenantId, usuario.Id, emailLower, request.IpAddress, request.UserAgent, false, "Senha incorreta.", "system");
                _context.HistoricosLogin.Add(historicoFalha);
                
                await _context.SaveChangesAsync(cancellationToken);

                return CommandResult.Falha(new[] { "E-mail ou senha incorretos." });
            }

            // 5. Sucesso
            usuario.ResetarFalhasLogin();

            // Gerar token básico e sessão
            var tokenSessao = _tokenService.GerarBasico(usuario.TenantId, usuario.Id.ToString());
            var expiracaoSessao = DateTime.UtcNow.AddHours(10); // Expiracao do token basico do material

            var sessao = new SessaoUsuario(
                usuario.TenantId,
                usuario.Id,
                tokenSessao,
                request.IpAddress,
                request.UserAgent,
                expiracaoSessao,
                usuario.Id.ToString()
            );

            var historicoSucesso = new HistoricoLogin(usuario.TenantId, usuario.Id, emailLower, request.IpAddress, request.UserAgent, true, "Login realizado com sucesso.", usuario.Id.ToString());
            
            _context.SessoesUsuarios.Add(sessao);
            _context.HistoricosLogin.Add(historicoSucesso);
            await _context.SaveChangesAsync(cancellationToken);

            // Verifica as empresas do usuário
            var empresasVinculadas = await _context.UsuariosEmpresas
                .Where(ue => ue.UsuarioId == usuario.Id && ue.DeletadoEm == null)
                .ToListAsync(cancellationToken);

            // Buscar informações de Razão Social da Gestão de Clientes para as empresas do usuário
            var empresaIds = empresasVinculadas.Select(ue => ue.EmpresaId).ToList();
            var empresasDetalhadas = await _contextGestao.Empresas
                .Where(e => e.TenantId == usuario.TenantId && empresaIds.Contains(e.Id) && e.DeletadoEm == null)
                .ToListAsync(cancellationToken);

            var empresasDto = empresasVinculadas.Select(ue =>
            {
                var emp = empresasDetalhadas.FirstOrDefault(e => e.Id == ue.EmpresaId);
                return new UsuarioEmpresaDto(
                    ue.EmpresaId,
                    emp?.RazaoSocial ?? "Empresa Desconhecida",
                    ue.EhAdmin,
                    ue.PerfilAcessoId
                );
            }).ToList();

            // Verificar se o tenant está inadimplente (Bloqueio SaaS)
            var cliente = await _contextGestao.Clientes
                .FirstOrDefaultAsync(c => c.TenantId == usuario.TenantId && c.DeletadoEm == null, cancellationToken);

            var block = false;
            if (cliente != null && cliente.Ativo)
            {
                var dataLimite = DateTime.UtcNow.AddDays(-15);
                block = await _contextGestao.Faturas
                    .AnyAsync(f => f.ClienteId == cliente.Id &&
                                   f.Status != FaturaStatus.Paga &&
                                   f.Status != FaturaStatus.Cancelada &&
                                   f.DataVencimento < dataLimite &&
                                   f.DeletadoEm == null, cancellationToken);
            }

            var exigeSelecao = empresasVinculadas.Count > 1;
            var tokenRetorno = tokenSessao;

            // Se o usuário tem apenas uma empresa vinculada, emite token completo direto
            if (empresasVinculadas.Count == 1)
            {
                var vinculo = empresasVinculadas.First();
                tokenRetorno = _tokenService.GerarCompleto(usuario.TenantId, usuario.Id.ToString(), vinculo.EmpresaId.ToString(), vinculo.PerfilAcessoId?.ToString() ?? "null");
            }

            var dto = new AuthResponseDto(
                Token: tokenRetorno,
                Expiracao: expiracaoSessao,
                UsuarioId: usuario.Id,
                Nome: usuario.Nome,
                Email: usuario.Email,
                ExigeSelecaoEmpresa: exigeSelecao,
                TenantId: usuario.TenantId,
                Block: block,
                Empresas: empresasDto
            );

            return CommandResult.Ok("Autenticação realizada com sucesso!", dto);
        }

        private void DefinirTenantDaRequisicao(string tenantId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Items["TenantId"] = tenantId;
            }
        }
    }

    public class AutenticarUsuarioInternoCommandHandler : ICommandHandler<AutenticarUsuarioInternoCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEprosTokenService _tokenService;

        public AutenticarUsuarioInternoCommandHandler(
            ContextAplicativo context,
            IPasswordHasher passwordHasher,
            IEprosTokenService tokenService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<CommandResult> Handle(AutenticarUsuarioInternoCommand request, CancellationToken cancellationToken)
        {
            var emailLower = request.Email.ToLowerInvariant().Trim();

            // Login de operador interno é anônimo: não conhece o inquilino. O UsuarioInterno vive no
            // tenant fixo "system"; buscamos cross-tenant driblando o filtro de tenant do EF Core
            // (IgnoreQueryFilters) e a RLS do Postgres (AuthRlsBypass), como no login de tenant.
            // A comparação de e-mail é case-insensitive porque o seed não normaliza o e-mail.
            await AuthRlsBypass.EnableAsync(_context, cancellationToken);
            var usuario = await _context.UsuariosInternos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower && u.DeletadoEm == null, cancellationToken);
            await AuthRlsBypass.DisableAsync(_context, cancellationToken);

            // Mesma mensagem para usuário inexistente ou senha incorreta (evita enumeração de contas).
            if (usuario == null || !_passwordHasher.Verify(request.Senha, usuario.Senha))
            {
                return CommandResult.Falha(new[] { "E-mail ou senha incorretos." });
            }

            // Token marca o operador como interno: tenantId="system" (curto-circuito do AbacFilter),
            // empresaId="system" e perfilId="interno".
            var token = _tokenService.GerarCompleto("system", usuario.Id.ToString(), "system", "interno");

            return CommandResult.Ok("Autenticação de operador interno realizada com sucesso!", new
            {
                token,
                usuarioId = usuario.Id,
                nome = usuario.Nome,
                email = usuario.Email
            });
        }
    }

    public class SelecionarEmpresaCommandHandler : ICommandHandler<SelecionarEmpresaCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ContextGestaoClientes _contextGestao;
        private readonly IEprosTokenService _tokenService;

        public SelecionarEmpresaCommandHandler(ContextAplicativo context, ContextGestaoClientes contextGestao, IEprosTokenService tokenService)
        {
            _context = context;
            _contextGestao = contextGestao;
            _tokenService = tokenService;
        }

        public async Task<CommandResult> Handle(SelecionarEmpresaCommand request, CancellationToken cancellationToken)
        {
            var vinculo = await _context.UsuariosEmpresas
                .FirstOrDefaultAsync(ue => ue.UsuarioId == request.UsuarioId && ue.EmpresaId == request.EmpresaId && ue.DeletadoEm == null, cancellationToken);

            if (vinculo == null)
            {
                return CommandResult.Falha(new[] { "O usuário não possui permissão de acesso à empresa informada." });
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioId && u.DeletadoEm == null, cancellationToken);

            if (usuario == null)
            {
                return CommandResult.Falha(new[] { "Usuário inválido ou inativo." });
            }

            // Verificar se o tenant está inadimplente (Bloqueio SaaS)
            var cliente = await _contextGestao.Clientes
                .FirstOrDefaultAsync(c => c.TenantId == usuario.TenantId && c.DeletadoEm == null, cancellationToken);

            var block = false;
            if (cliente != null && cliente.Ativo)
            {
                var dataLimite = DateTime.UtcNow.AddDays(-15);
                block = await _contextGestao.Faturas
                    .AnyAsync(f => f.ClienteId == cliente.Id &&
                                   f.Status != FaturaStatus.Paga &&
                                   f.Status != FaturaStatus.Cancelada &&
                                   f.DataVencimento < dataLimite &&
                                   f.DeletadoEm == null, cancellationToken);
            }

            // Emite token completo com empresa e perfil
            var tokenCompleto = _tokenService.GerarCompleto(usuario.TenantId, usuario.Id.ToString(), vinculo.EmpresaId.ToString(), vinculo.PerfilAcessoId?.ToString() ?? "null");
            var expiracao = DateTime.UtcNow.AddHours(10);

            var dto = new AuthResponseDto(
                Token: tokenCompleto,
                Expiracao: expiracao,
                UsuarioId: usuario.Id,
                Nome: usuario.Nome,
                Email: usuario.Email,
                ExigeSelecaoEmpresa: false,
                TenantId: usuario.TenantId,
                Block: block
            );

            return CommandResult.Ok("Empresa selecionada e contexto completo emitido com sucesso!", dto);
        }
    }

    public class SolicitarRecuperacaoSenhaCommandHandler : ICommandHandler<SolicitarRecuperacaoSenhaCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SolicitarRecuperacaoSenhaCommandHandler(
            ContextAplicativo context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CommandResult> Handle(SolicitarRecuperacaoSenhaCommand request, CancellationToken cancellationToken)
        {
            var emailLower = request.Email.ToLowerInvariant().Trim();

            await AuthRlsBypass.EnableAsync(_context, cancellationToken);
            var usuario = await _context.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == emailLower && u.DeletadoEm == null, cancellationToken);
            await AuthRlsBypass.DisableAsync(_context, cancellationToken);

            if (usuario != null && _httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Items["TenantId"] = usuario.TenantId;
            }

            if (usuario != null)
            {
                // Gera token temporário de redefinição com validade de 3 horas
                var token = Guid.NewGuid().ToString("N");
                usuario.GerarTokenRecuperacaoSenha(token, TimeSpan.FromHours(3));
                await _context.SaveChangesAsync(cancellationToken);

                // Em ambiente real, aqui despacharíamos o e-mail de redefinição.
                // Registramos o sucesso da solicitação.
            }

            // Para evitar enumeração de contas, retornamos Ok independente de encontrar o usuário
            return CommandResult.Ok("Se o e-mail informado existir na plataforma, você receberá as instruções de redefinição.");
        }
    }

    public class ResetarSenhaCommandHandler : ICommandHandler<ResetarSenhaCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResetarSenhaCommandHandler(
            ContextAplicativo context,
            IPasswordHasher passwordHasher,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CommandResult> Handle(ResetarSenhaCommand request, CancellationToken cancellationToken)
        {
            var emailLower = request.Email.ToLowerInvariant().Trim();

            await AuthRlsBypass.EnableAsync(_context, cancellationToken);
            var usuario = await _context.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == emailLower && u.DeletadoEm == null, cancellationToken);
            await AuthRlsBypass.DisableAsync(_context, cancellationToken);

            if (usuario != null && _httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Items["TenantId"] = usuario.TenantId;
            }

            if (usuario == null || usuario.ForgotPasswordToken != request.Token)
            {
                return CommandResult.Falha(new[] { "O e-mail informado ou o token de recuperação é inválido." });
            }

            if (usuario.ForgotPasswordTokenExpiry.HasValue && usuario.ForgotPasswordTokenExpiry.Value < DateTime.UtcNow)
            {
                return CommandResult.Falha(new[] { "O token de recuperação de senha expirou." });
            }

            // Altera a senha (hash PBKDF2) e limpa o token
            usuario.AlterarSenha(_passwordHasher.Hash(request.NovaSenha), "system-reset");
            
            if (!usuario.IsValid)
            {
                var erros = usuario.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Erro ao validar a nova senha.");
            }

            usuario.LimparTokenRecuperacaoSenha();
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Senha redefinida com sucesso!");
        }
    }

    public class AlterarSenhaUsuarioCommandHandler : ICommandHandler<AlterarSenhaUsuarioCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly IPasswordHasher _passwordHasher;

        public AlterarSenhaUsuarioCommandHandler(ContextAplicativo context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<CommandResult> Handle(AlterarSenhaUsuarioCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioId && u.DeletadoEm == null, cancellationToken);

            if (usuario == null)
            {
                return CommandResult.Falha(new[] { "Usuário não encontrado." });
            }

            if (!_passwordHasher.Verify(request.SenhaAtual, usuario.PasswordHash))
            {
                return CommandResult.Falha(new[] { "A senha atual informada é inválida." });
            }

            usuario.AlterarSenha(_passwordHasher.Hash(request.NovaSenha), request.UsuarioId.ToString());

            if (!usuario.IsValid)
            {
                var erros = usuario.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Erro ao validar a nova senha.");
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Senha alterada com sucesso!");
        }
    }

    public class RegistrarNovoTenantCommandHandler : ICommandHandler<RegistrarNovoTenantCommand>
    {
        private readonly ContextAplicativo _contextAplicativo;
        private readonly ContextGestaoClientes _contextGestaoClientes;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RegistrarNovoTenantCommandHandler(
            ContextAplicativo contextAplicativo,
            ContextGestaoClientes contextGestaoClientes,
            IPasswordHasher passwordHasher,
            IHttpContextAccessor httpContextAccessor)
        {
            _contextAplicativo = contextAplicativo;
            _contextGestaoClientes = contextGestaoClientes;
            _passwordHasher = passwordHasher;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CommandResult> Handle(RegistrarNovoTenantCommand request, CancellationToken cancellationToken)
        {
            var emailLower = request.EmailAdmin.ToLowerInvariant().Trim();

            // 1. Validar duplicidade de e-mail na base de usuários (cross-tenant)
            await AuthRlsBypass.EnableAsync(_contextAplicativo, cancellationToken);
            await AuthRlsBypass.EnableAsync(_contextGestaoClientes, cancellationToken);

            var emailExiste = await _contextAplicativo.Usuarios
                .IgnoreQueryFilters()
                .AnyAsync(u => u.Email == emailLower && u.DeletadoEm == null, cancellationToken);

            if (emailExiste)
            {
                await AuthRlsBypass.DisableAsync(_contextGestaoClientes, cancellationToken);
                await AuthRlsBypass.DisableAsync(_contextAplicativo, cancellationToken);
                return CommandResult.Falha(new[] { "Já existe um usuário cadastrado com este e-mail." });
            }

            // 2. Validar duplicidade de CNPJ
            var cnpjFormatado = request.Cnpj.Trim();
            var cnpjExiste = await _contextGestaoClientes.Empresas
                .IgnoreQueryFilters()
                .AnyAsync(e => e.Cnpj == cnpjFormatado && e.DeletadoEm == null, cancellationToken);

            await AuthRlsBypass.DisableAsync(_contextGestaoClientes, cancellationToken);
            await AuthRlsBypass.DisableAsync(_contextAplicativo, cancellationToken);

            if (cnpjExiste)
            {
                return CommandResult.Falha(new[] { "Já existe uma empresa cadastrada com este CNPJ." });
            }

            // 3. Transação compartilhada — fechar conexões abertas nas validações acima
            var isRelational = _contextAplicativo.Database.IsRelational() && _contextGestaoClientes.Database.IsRelational();
            IDbContextTransaction? transaction = null;
            if (isRelational)
            {
                await _contextAplicativo.Database.CloseConnectionAsync();
                await _contextGestaoClientes.Database.CloseConnectionAsync();

                // Uma transação do PostgreSQL/Npgsql pertence à conexão que a abriu; ela só pode ser
                // reaproveitada por outro DbContext se ambos compartilharem a MESMA conexão física.
                // Como os dois contextos apontam para o mesmo banco (DefaultConnection), passamos a
                // conexão do ContextAplicativo para o ContextGestaoClientes antes de iniciar a
                // transação compartilhada. Sem isto, UseTransactionAsync lança
                // "The specified transaction is not associated with the current connection".
                var conexaoCompartilhada = _contextAplicativo.Database.GetDbConnection();
                _contextGestaoClientes.Database.SetDbConnection(conexaoCompartilhada, contextOwnsConnection: false);

                transaction = await _contextAplicativo.Database.BeginTransactionAsync(cancellationToken);
                await _contextGestaoClientes.Database.UseTransactionAsync(transaction.GetDbTransaction(), cancellationToken);
            }

            try
            {
                var tenantId = $"tenant-{Guid.NewGuid().ToString("N").Substring(0, 12)}";
                var criadoPor = "self-register";

                if (_httpContextAccessor.HttpContext != null)
                {
                    _httpContextAccessor.HttpContext.Items["TenantId"] = tenantId;
                }

                // Criar o endereço padrão para a empresa (valores padrões aceitáveis no self-register)
                var endereco = new Epros.Modules.GestaoClientes.Domain.ValueObjects.Endereco("Logradouro Padrão", "S/N", "Self Register", "Bairro Padrão", "00000000", "Cidade Padrão", "SP");

                // 3.5. Criar PessoaGrupo para o tenant
                var pessoaGrupo = new PessoaGrupo(
                    descricao: $"Grupo Geral - {request.NomeEmpresa}",
                    tenantId: tenantId,
                    criadoPor: criadoPor
                );
                _contextGestaoClientes.PessoaGrupos.Add(pessoaGrupo);
                await _contextGestaoClientes.SaveChangesAsync(cancellationToken);

                // 4. Criar Empresa no schema plataforma (ContextGestaoClientes)
                var empresa = new Empresa(
                    razaoSocial: request.NomeEmpresa,
                    nomeFantasia: request.NomeEmpresa,
                    cnpj: cnpjFormatado,
                    inscricaoEstadual: null,
                    inscricaoMunicipal: null,
                    inscricaoSuframa: null,
                    cnae: null,
                    regimeTributario: RegimeTributario.SimplesNacional,
                    regimeApuracao: RegimeApuracao.Cumulativo,
                    pessoaGrupoId: pessoaGrupo.Id,
                    produtoGrupoId: Guid.NewGuid(),
                    planoContasFinanceiroId: Guid.NewGuid(),
                    tributarioGrupoId: Guid.NewGuid(),
                    ncmTributacaoId: null,
                    certificadoDigitalId: null,
                    empresaParametrosDfeId: null,
                    linkWebApiAppVendas: null,
                    tokenMercadoPagoPix: null,
                    logo: null,
                    endereco: endereco,
                    tenantId: tenantId,
                    criadoPor: criadoPor
                );

                if (!empresa.IsValid)
                {
                    var erros = empresa.Notifications.Select(n => n.Message);
                    return CommandResult.Falha(erros, "Erro ao validar os dados cadastrais da empresa.");
                }

                _contextGestaoClientes.Empresas.Add(empresa);
                await _contextGestaoClientes.SaveChangesAsync(cancellationToken);

                // 5. Criar Usuário Admin no schema aplicativo (ContextAplicativo)
                var usuarioAdmin = new Usuario(
                    tenantId: tenantId,
                    nome: request.NomeAdmin,
                    email: emailLower,
                    passwordHash: _passwordHasher.Hash(request.SenhaAdmin), // Senha derivada via PBKDF2 — nunca em texto puro
                    tipo: UsuarioTipo.Company,
                    criadoPor: criadoPor
                );

                if (!usuarioAdmin.IsValid)
                {
                    var erros = usuarioAdmin.Notifications.Select(n => n.Message);
                    return CommandResult.Falha(erros, "Erro ao validar os dados cadastrais do usuário administrador.");
                }

                _contextAplicativo.Usuarios.Add(usuarioAdmin);
                await _contextAplicativo.SaveChangesAsync(cancellationToken);

                // 6. Criar vínculo UsuarioEmpresa
                var vinculo = new UsuarioEmpresa(
                    tenantId: tenantId,
                    usuarioId: usuarioAdmin.Id,
                    empresaId: empresa.Id,
                    perfilUsuarioId: null, // Admin primário dispensa perfil inicial
                    ehAdmin: true,
                    criadoPor: criadoPor
                );

                _contextAplicativo.UsuariosEmpresas.Add(vinculo);
                await _contextAplicativo.SaveChangesAsync(cancellationToken);

                // 6.5. Criar ConfiguracaoEmpresa padrão
                var configEmpresa = new ConfiguracaoEmpresa(
                    tenantId: tenantId,
                    empresaId: empresa.Id,
                    nome: request.NomeEmpresa,
                    email: emailLower,
                    telefone: null,
                    endereco: "Logradouro Padrão, S/N",
                    timeZoneId: 1,
                    dateFormat: "DD-MM-YYYY",
                    currencyId: 1,
                    vatPercentage: 0,
                    vatType: 1,
                    currencyPosition: 1,
                    footerText: null,
                    logo: null,
                    favicon: null,
                    criadoPor: criadoPor
                );
                _contextAplicativo.ConfiguracoesEmpresas.Add(configEmpresa);

                // 6.6. Criar primeiro AnoFinanceiro ativo
                var anoFinanceiro = new AnoFinanceiro(
                    tenantId: tenantId,
                    fromDate: DateTime.UtcNow,
                    toDate: DateTime.UtcNow.AddDays(365),
                    isActive: true,
                    criadoPor: criadoPor
                );
                _contextAplicativo.AnosFinanceiros.Add(anoFinanceiro);

                // 6.7. Criar Idiomas padrão (en, pt-BR)
                var langEn = new Idioma(tenantId, "en", "English", "US", true, criadoPor);
                var langPt = new Idioma(tenantId, "pt-BR", "Português", "BR", true, criadoPor);
                _contextAplicativo.Idiomas.Add(langEn);
                _contextAplicativo.Idiomas.Add(langPt);

                await _contextAplicativo.SaveChangesAsync(cancellationToken);

                // Commit transação
                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                return CommandResult.Ok("Tenant e administrador registrados com sucesso!", new Dictionary<string, object>
                {
                    { "TenantId", tenantId },
                    { "UsuarioAdminId", usuarioAdmin.Id },
                    { "EmpresaId", empresa.Id }
                });
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                return CommandResult.Falha(new[] { $"Erro interno ao realizar registro de tenant: {ex.Message}" });
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }
    }
}
