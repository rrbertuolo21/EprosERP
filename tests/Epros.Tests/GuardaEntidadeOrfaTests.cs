using System;
using System.Collections.Generic;
using Epros.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Modules.Producao.Infrastructure.Data;
using Epros.Modules.RH.Infrastructure.Data;
using Epros.Modules.Projetos.Infrastructure.Data;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Epros.Modules.GRC.Infrastructure.Data;
using Epros.Modules.ESG.Infrastructure.Data;
using Epros.Modules.DMS.Infrastructure.Data;
using Epros.Modules.Imobiliaria.Infrastructure.Data;

namespace Epros.Tests
{
    /// <summary>
    /// Valida a BARREIRA de entidade órfã (REG-001) sobre o modelo RELACIONAL real (Npgsql) de TODOS
    /// os módulos — o mesmo modelo do boot de produção. Não precisa de banco: só constrói o modelo
    /// EF offline. Se algum módulo introduzir uma entidade persistente sem classificação de tenant
    /// (não herda EntidadeSaaSBase, não é IGlobalEntity, não tem tenant_id e não é read-model de
    /// tabela tenantizada), este teste — e o startup da API — falham.
    /// </summary>
    public class GuardaEntidadeOrfaTests
    {
        private sealed class NoopTenantProvider : ITenantProvider
        {
            public string GetTenantId() => "system";
        }

        private sealed class NoopCurrentUser : ICurrentUser
        {
            public string? GetUserId() => "system-test";
            public string? GetUserName() => "System Test";
            public string? GetUserEmail() => "system@epros.com";
        }

        private static DbContextOptions<T> Opts<T>() where T : DbContext =>
            new DbContextOptionsBuilder<T>()
                .UseNpgsql("Host=localhost;Port=5433;Database=modelo_offline;Username=epros;Password=epros")
                .Options;

        [Fact]
        public void Nenhuma_Entidade_Persistente_Fica_Sem_Classificacao_De_Tenant()
        {
            var tp = new NoopTenantProvider();
            var cu = new NoopCurrentUser();

            var contextos = new List<DbContext>
            {
                new ContextGestaoClientes(Opts<ContextGestaoClientes>(), tp, cu),
                new ContextAplicativo(Opts<ContextAplicativo>(), tp, cu),
                new ContextEstoque(Opts<ContextEstoque>(), tp, cu),
                new ContextFiscal(Opts<ContextFiscal>(), tp, cu),
                new ContextFinanceiro(Opts<ContextFinanceiro>(), tp, cu),
                new ContextVendas(Opts<ContextVendas>(), tp, cu),
                new ContextQualidade(Opts<ContextQualidade>(), tp, cu),
                new ContextProducao(Opts<ContextProducao>(), tp, cu),
                new ContextRH(Opts<ContextRH>(), tp, cu),
                new ContextProjetos(Opts<ContextProjetos>(), tp, cu),
                new ContextManutencao(Opts<ContextManutencao>(), tp, cu),
                new ContextGRC(Opts<ContextGRC>(), tp, cu),
                new ContextESG(Opts<ContextESG>(), tp, cu),
                new ContextDMS(Opts<ContextDMS>(), tp, cu),
                new ContextImobiliaria(Opts<ContextImobiliaria>(), tp, cu),
            };

            try
            {
                // Não deve lançar. Se lançar, a mensagem lista as entidades órfãs.
                GuardaEntidadeOrfa.ValidarModelos(contextos);
            }
            finally
            {
                foreach (var c in contextos) c.Dispose();
            }
        }
    }
}
