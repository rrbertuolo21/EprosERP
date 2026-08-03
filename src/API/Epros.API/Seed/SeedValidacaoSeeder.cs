using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

using Epros.Infrastructure.Data;
using Epros.Shared.Application.Contracts;

using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.Aplicativo.Domain.Entities;

using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Domain.Entities;

namespace Epros.API.Seed
{
    /// <summary>
    /// Seed de VALIDAÇÃO ponta a ponta do fluxo multi-tenant / contador parceiro + isolamento.
    ///
    /// Roda SOMENTE em Development/Staging (gating no <c>Program.cs</c>), depois das migrations e do
    /// <c>CatalogoFiscalSeeder</c>. É FAIL-CLOSED em Production (o chamador nunca o invoca) e NÃO roda no
    /// host de teste (ambiente "Testing" também é excluído pelo chamador; e os testes usam InMemory).
    ///
    /// Idempotente: cada entidade é checada por chave natural (e-mail conhecido / TenantId) ANTES de criar,
    /// então subir o boot 2x não duplica. As senhas usam o MESMO hasher do projeto (<see cref="IPasswordHasher"/>,
    /// PBKDF2), como os <c>InstallationHandlers</c>. As leituras cross-tenant usam <see cref="AuthRlsBypass"/> +
    /// <c>IgnoreQueryFilters()</c>, como os handlers de autenticação. No ambiente alvo o usuário do Postgres é
    /// superuser (POSTGRES_USER) e ignora a RLS nas escritas — como todo o resto do seed/migrations local.
    ///
    /// O que cria:
    ///  - User Lord: <c>UsuarioInterno</c> superadmin da Siser (perfil interno, tenant "system").
    ///  - Tenant A completo: Plano (flags de módulo LIGADAS) + Cliente (StatusSaaS.Ativo) + Empresa +
    ///    Usuário admin + membership Proprietário + vínculo Usuario↔Empresa.
    ///  - Tenant B completo: idem, tenant próprio e admin próprio.
    ///  - Contador parceiro: UMA identidade global com membership <c>ContadorParceiro</c> para OS DOIS tenants,
    ///    e vínculo Usuario↔Empresa em cada um — valida a tela de cards de seleção de tenant + isolamento.
    /// </summary>
    public static class SeedValidacaoSeeder
    {
        // ---- Credenciais conhecidas (SOMENTE fora de produção — dev/staging) ----
        public const string SenhaPadrao = "Epros@Validacao#2026";

        public const string LordEmail = "lord@siser.com.br";
        public const string AdminAEmail = "admin@teste-a.com.br";
        public const string AdminBEmail = "admin@teste-b.com.br";
        public const string ContadorEmail = "contador@parceiro.com.br";

        public const string TenantA = "tenant-teste-a";
        public const string TenantB = "tenant-teste-b";

        private const string CriadoPor = "seed-validacao";

        // CNPJs válidos (dígito verificador correto) — plausíveis, só para não destoar do cadastro fiscal.
        private const string CnpjEmpresaA = "11222333000181";
        private const string CnpjEmpresaB = "44555666000181";

        public static async Task SeedAsync(IServiceProvider services, string environmentName, CancellationToken cancellationToken = default)
        {
            var contextAplicativo = services.GetRequiredService<ContextAplicativo>();
            var contextGestao = services.GetRequiredService<ContextGestaoClientes>();
            var passwordHasher = services.GetRequiredService<IPasswordHasher>();

            // Guarda extra: só faz sentido em base relacional (Postgres). Em InMemory (testes) não roda.
            if (!contextAplicativo.Database.IsRelational() || !contextGestao.Database.IsRelational())
            {
                Log.Information("[SeedValidacao] Base não-relacional — seed ignorado.");
                return;
            }

            Log.Information("[SeedValidacao] Iniciando seed de validação (ambiente {Env})...", environmentName);

            var senhaHash = passwordHasher.Hash(SenhaPadrao);

            await AuthRlsBypass.EnableAsync(contextAplicativo, cancellationToken);
            await AuthRlsBypass.EnableAsync(contextGestao, cancellationToken);
            try
            {
                // 1.09 — garante o catálogo RBAC (Capacidades system + papel Administrador) antes de
                // atribuí-lo aos admins de validação. Idempotente; no boot normal já rodou no Program.cs.
                await CapacidadeCatalogoSeeder.SeedAsync(contextGestao, ct: cancellationToken);

                await SemearLordAsync(contextAplicativo, senhaHash, cancellationToken);

                await SemearTenantAsync(
                    contextAplicativo, contextGestao, senhaHash,
                    tenantId: TenantA,
                    nome: "A",
                    cnpj: CnpjEmpresaA,
                    adminEmail: AdminAEmail,
                    cancellationToken);

                await SemearTenantAsync(
                    contextAplicativo, contextGestao, senhaHash,
                    tenantId: TenantB,
                    nome: "B",
                    cnpj: CnpjEmpresaB,
                    adminEmail: AdminBEmail,
                    cancellationToken);

                await SemearContadorParceiroAsync(contextAplicativo, contextGestao, senhaHash, cancellationToken);

                Log.Information("[SeedValidacao] Seed de validação concluído. Lord={Lord} | Tenants={A},{B} | Contador={Contador}",
                    LordEmail, TenantA, TenantB, ContadorEmail);
            }
            finally
            {
                await AuthRlsBypass.DisableAsync(contextGestao, cancellationToken);
                await AuthRlsBypass.DisableAsync(contextAplicativo, cancellationToken);
            }
        }

        // ---------------------------------------------------------------------
        // 1. User Lord (superadmin interno da Siser)
        // ---------------------------------------------------------------------
        private static async Task SemearLordAsync(ContextAplicativo ctx, string senhaHash, CancellationToken ct)
        {
            var existe = await ctx.UsuariosInternos
                .IgnoreQueryFilters()
                .AnyAsync(u => u.Email == LordEmail && u.DeletadoEm == null, ct);

            if (existe)
            {
                Log.Information("[SeedValidacao] Lord já existe ({Email}) — mantido.", LordEmail);
                return;
            }

            var lord = new UsuarioInterno(
                nome: "Lord Siser (Validação)",
                email: LordEmail,
                senha: senhaHash,
                creatorId: null,
                uniqueId: "lord-validacao",
                timezone: "America/Sao_Paulo",
                primaryAdmin: true,
                tenantId: "system",
                criadoPor: CriadoPor,
                perfilSuporte: PerfilSuporteSiser.SuporteTecnico);

            if (!lord.IsValid)
            {
                Log.Warning("[SeedValidacao] Lord inválido: {Erros}", string.Join("; ", lord.Notifications.Select(n => n.Message)));
                return;
            }

            ctx.UsuariosInternos.Add(lord);
            await ctx.SaveChangesAsync(ct);
            Log.Information("[SeedValidacao] Lord criado ({Email}).", LordEmail);
        }

        // ---------------------------------------------------------------------
        // 2/3. Tenant completo (Plano + Cliente + Empresa + Admin + memberships)
        // ---------------------------------------------------------------------
        private static async Task SemearTenantAsync(
            ContextAplicativo ctxApp,
            ContextGestaoClientes ctxGestao,
            string senhaHash,
            string tenantId,
            string nome,
            string cnpj,
            string adminEmail,
            CancellationToken ct)
        {
            // Idempotência do tenant: se já existe Cliente para este TenantId, considera semeado.
            var clienteExiste = await ctxGestao.Clientes
                .IgnoreQueryFilters()
                .AnyAsync(c => c.TenantId == tenantId && c.DeletadoEm == null, ct);

            // Plano (flags de módulo LIGADAS), pertencente ao próprio tenant.
            var plano = await ctxGestao.Planos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.DeletadoEm == null, ct);

            if (plano == null)
            {
                plano = new Plano(
                    nome: $"Plano Validação {nome}",
                    preco: 199.90m,
                    grupoPlanoId: null,
                    limiteUsuarios: 0,   // 0 = ilimitado
                    limiteEmpresas: 0,
                    recursosInclusos: "Todos os módulos (validação)",
                    tenantId: tenantId,
                    criadoPor: CriadoPor,
                    descricaoCurta: "Plano de validação com todos os módulos ligados",
                    descricaoCompleta: null,
                    dataInicio: DateTime.UtcNow,
                    dataFim: null,
                    duration: PlanoDuration.Mensal,
                    moduloCrm: true,
                    moduloProjetos: true,
                    moduloRh: true,
                    moduloFinanceiro: true,
                    moduloPdv: true,
                    limiteClientes: 0,
                    diasToleranciaInadimplencia: 15);

                if (!plano.IsValid)
                {
                    Log.Warning("[SeedValidacao] Plano {Tenant} inválido: {Erros}", tenantId, string.Join("; ", plano.Notifications.Select(n => n.Message)));
                    return;
                }
                ctxGestao.Planos.Add(plano);
                await ctxGestao.SaveChangesAsync(ct);
            }

            // Empresa emitente do tenant.
            var empresa = await ctxGestao.Empresas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.DeletadoEm == null, ct);

            if (empresa == null)
            {
                var endereco = new Epros.Modules.GestaoClientes.Domain.ValueObjects.Endereco(
                    logradouro: $"Rua da Validação {nome}",
                    numero: "100",
                    complemento: null,
                    bairro: "Centro",
                    cep: "01001000",
                    cidade: "São Paulo",
                    estado: "SP");

                empresa = new Empresa(
                    razaoSocial: $"Empresa Teste {nome} Ltda",
                    nomeFantasia: $"Teste {nome}",
                    cnpj: cnpj,
                    inscricaoEstadual: null,
                    inscricaoMunicipal: null,
                    inscricaoSuframa: null,
                    cnae: null,
                    regimeTributario: RegimeTributario.SimplesNacional,
                    regimeApuracao: RegimeApuracao.Cumulativo,
                    pessoaGrupoId: null,
                    produtoGrupoId: null,
                    planoContasFinanceiroId: null,
                    tributarioGrupoId: null,
                    ncmTributacaoId: null,
                    certificadoDigitalId: null,
                    empresaParametrosDfeId: null,
                    linkWebApiAppVendas: null,
                    tokenMercadoPagoPix: null,
                    logo: null,
                    endereco: endereco,
                    tenantId: tenantId,
                    criadoPor: CriadoPor);

                if (!empresa.IsValid)
                {
                    Log.Warning("[SeedValidacao] Empresa {Tenant} inválida: {Erros}", tenantId, string.Join("; ", empresa.Notifications.Select(n => n.Message)));
                    return;
                }
                ctxGestao.Empresas.Add(empresa);
                await ctxGestao.SaveChangesAsync(ct);
            }

            // Cliente (assinante SaaS) do tenant — StatusSaaS.Ativo.
            if (!clienteExiste)
            {
                var cliente = new Cliente(
                    razaoSocial: $"Tenant Teste {nome} Ltda",
                    cnpj: cnpj,
                    email: adminEmail,
                    planoId: plano.Id,
                    revendaId: null,
                    vendedorId: null,
                    diaVencimento: 10,
                    statusSaaS: StatusSaaS.Ativo,
                    tenantId: tenantId,
                    criadoPor: CriadoPor,
                    telefone: "(11) 4000-0000",
                    nomeContato: $"Admin Teste {nome}");

                if (!cliente.IsValid)
                {
                    Log.Warning("[SeedValidacao] Cliente {Tenant} inválido: {Erros}", tenantId, string.Join("; ", cliente.Notifications.Select(n => n.Message)));
                    return;
                }
                ctxGestao.Clientes.Add(cliente);
                await ctxGestao.SaveChangesAsync(ct);
            }

            // Usuário admin do tenant (identidade global).
            var admin = await ctxApp.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == adminEmail && u.DeletadoEm == null, ct);

            if (admin == null)
            {
                admin = new Usuario(
                    tenantId: tenantId,
                    nome: $"Admin Teste {nome}",
                    email: adminEmail,
                    passwordHash: senhaHash,
                    tipo: UsuarioTipo.Company,
                    criadoPor: CriadoPor);

                if (!admin.IsValid)
                {
                    Log.Warning("[SeedValidacao] Admin {Tenant} inválido: {Erros}", tenantId, string.Join("; ", admin.Notifications.Select(n => n.Message)));
                    return;
                }
                ctxApp.Usuarios.Add(admin);
                await ctxApp.SaveChangesAsync(ct);
            }

            // Vínculo Usuario↔Empresa (admin do tenant).
            await GarantirUsuarioEmpresaAsync(ctxApp, tenantId, admin.Id, empresa.Id, ehAdmin: true, ct);

            // Membership no tenant — Proprietário.
            await GarantirAcessoTenantAsync(ctxApp, tenantId, admin.Id, PapelAcessoTenant.Proprietario, ct);

            // 1.09 — RBAC: atribui o papel de sistema Administrador ao admin (senão fica travado no AbacFilter).
            await GarantirPapelAdministradorAsync(ctxGestao, tenantId, admin.Id, ct);

            Log.Information("[SeedValidacao] Tenant {Tenant} pronto (plano+cliente+empresa+admin+memberships+papel).", tenantId);
        }

        // ---------------------------------------------------------------------
        // 4. Contador parceiro — 1 identidade, membership nos DOIS tenants
        // ---------------------------------------------------------------------
        private static async Task SemearContadorParceiroAsync(
            ContextAplicativo ctxApp,
            ContextGestaoClientes ctxGestao,
            string senhaHash,
            CancellationToken ct)
        {
            var contador = await ctxApp.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == ContadorEmail && u.DeletadoEm == null, ct);

            if (contador == null)
            {
                // A identidade global "mora" no tenant A, mas o login é cross-tenant por e-mail.
                contador = new Usuario(
                    tenantId: TenantA,
                    nome: "Contador Parceiro (Validação)",
                    email: ContadorEmail,
                    passwordHash: senhaHash,
                    tipo: UsuarioTipo.Team,
                    criadoPor: CriadoPor);

                if (!contador.IsValid)
                {
                    Log.Warning("[SeedValidacao] Contador inválido: {Erros}", string.Join("; ", contador.Notifications.Select(n => n.Message)));
                    return;
                }
                ctxApp.Usuarios.Add(contador);
                await ctxApp.SaveChangesAsync(ct);
            }

            // Membership ContadorParceiro nos DOIS tenants (base da tela de cards + isolamento).
            await GarantirAcessoTenantAsync(ctxApp, TenantA, contador.Id, PapelAcessoTenant.ContadorParceiro, ct);
            await GarantirAcessoTenantAsync(ctxApp, TenantB, contador.Id, PapelAcessoTenant.ContadorParceiro, ct);

            // Vínculo Usuario↔Empresa em cada tenant para o contexto de empresa funcionar após escolher o card.
            var empresaA = await ctxGestao.Empresas.IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.TenantId == TenantA && e.DeletadoEm == null, ct);
            var empresaB = await ctxGestao.Empresas.IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.TenantId == TenantB && e.DeletadoEm == null, ct);

            if (empresaA != null)
                await GarantirUsuarioEmpresaAsync(ctxApp, TenantA, contador.Id, empresaA.Id, ehAdmin: false, ct);
            if (empresaB != null)
                await GarantirUsuarioEmpresaAsync(ctxApp, TenantB, contador.Id, empresaB.Id, ehAdmin: false, ct);

            Log.Information("[SeedValidacao] Contador parceiro pronto ({Email}) — membership em {A} e {B}.", ContadorEmail, TenantA, TenantB);
        }

        // ---------------------------------------------------------------------
        // Helpers idempotentes
        // ---------------------------------------------------------------------
        private static async Task GarantirAcessoTenantAsync(
            ContextAplicativo ctx, string tenantId, Guid usuarioId, PapelAcessoTenant papel, CancellationToken ct)
        {
            var existe = await ctx.AcessosUsuarioTenant
                .IgnoreQueryFilters()
                .AnyAsync(a => a.TenantId == tenantId && a.UsuarioId == usuarioId && a.DeletadoEm == null, ct);
            if (existe) return;

            var acesso = new AcessoUsuarioTenant(tenantId, usuarioId, papel, CriadoPor);
            if (!acesso.IsValid)
            {
                Log.Warning("[SeedValidacao] AcessoUsuarioTenant inválido ({Tenant}/{Usuario}): {Erros}",
                    tenantId, usuarioId, string.Join("; ", acesso.Notifications.Select(n => n.Message)));
                return;
            }
            ctx.AcessosUsuarioTenant.Add(acesso);
            await ctx.SaveChangesAsync(ct);
        }

        // 1.09 — atribui o papel de sistema Administrador (todas as capacidades) ao usuário, por tenant,
        // com EmpresaId nulo (vale p/ todas as empresas). Idempotente.
        private static async Task GarantirPapelAdministradorAsync(
            ContextGestaoClientes ctx, string tenantId, Guid usuarioId, CancellationToken ct)
        {
            var papelId = await CapacidadeCatalogoSeeder.ObterPapelAdministradorIdAsync(ctx, ct);
            if (papelId == null)
            {
                Log.Warning("[SeedValidacao] Papel Administrador ausente — usuário {Usuario} sem papel RBAC.", usuarioId);
                return;
            }

            var existe = await ctx.UsuariosPapeis
                .IgnoreQueryFilters()
                .AnyAsync(up => up.TenantId == tenantId && up.UsuarioId == usuarioId && up.PapelId == papelId.Value && up.DeletadoEm == null, ct);
            if (existe) return;

            var vinculo = new UsuarioPapel(usuarioId, papelId.Value, "Usuario", tenantId, CriadoPor, empresaId: null);
            if (!vinculo.IsValid)
            {
                Log.Warning("[SeedValidacao] UsuarioPapel inválido ({Tenant}/{Usuario}): {Erros}",
                    tenantId, usuarioId, string.Join("; ", vinculo.Notifications.Select(n => n.Message)));
                return;
            }
            ctx.UsuariosPapeis.Add(vinculo);
            await ctx.SaveChangesAsync(ct);
        }

        private static async Task GarantirUsuarioEmpresaAsync(
            ContextAplicativo ctx, string tenantId, Guid usuarioId, Guid empresaId, bool ehAdmin, CancellationToken ct)
        {
            var existe = await ctx.UsuariosEmpresas
                .IgnoreQueryFilters()
                .AnyAsync(v => v.TenantId == tenantId && v.UsuarioId == usuarioId && v.EmpresaId == empresaId && v.DeletadoEm == null, ct);
            if (existe) return;

            var vinculo = new UsuarioEmpresa(tenantId, usuarioId, empresaId, perfilUsuarioId: null, ehAdmin: ehAdmin, criadoPor: CriadoPor);
            if (!vinculo.IsValid)
            {
                Log.Warning("[SeedValidacao] UsuarioEmpresa inválido ({Tenant}/{Usuario}): {Erros}",
                    tenantId, usuarioId, string.Join("; ", vinculo.Notifications.Select(n => n.Message)));
                return;
            }
            ctx.UsuariosEmpresas.Add(vinculo);
            await ctx.SaveChangesAsync(ct);
        }
    }
}
