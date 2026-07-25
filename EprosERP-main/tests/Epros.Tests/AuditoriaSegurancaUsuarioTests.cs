using System;
using Xunit;
using Epros.Modules.GestaoClientes.Domain.Entities;

namespace Epros.Tests
{
    /// <summary>
    /// APP-TEN-003 (Usuarios e Papeis) — regras-chave da concessão direta de capacidade
    /// (UsuarioCapacidade, grant/deny). HistoricoLogin e SessaoImpersonacao pertencem ao módulo
    /// Aplicativo (Identity) e têm cobertura própria em UsuariosPapeisTests/UsuariosPapelesGapsTests.
    /// </summary>
    public class AuditoriaSegurancaUsuarioTests
    {
        private const string Tenant = "tenant-1";
        private const string User = "user-admin";

        // ===== UsuarioCapacidade (grant/deny direto) =====

        [Fact] // REG-041: deny explícito é registrável
        public void Deve_Registrar_Deny_Direto_De_Capacidade()
        {
            var uc = new UsuarioCapacidade(Guid.NewGuid(), Guid.NewGuid(), granted: false, Tenant, User);

            Assert.True(uc.IsValid);
            Assert.False(uc.Granted);
        }

        [Fact] // REG-040: alternar concessão
        public void Deve_Alterar_Concessao_De_Capacidade()
        {
            var uc = new UsuarioCapacidade(Guid.NewGuid(), Guid.NewGuid(), granted: true, Tenant, User);

            uc.AlterarConcessao(false, User);

            Assert.True(uc.IsValid);
            Assert.False(uc.Granted);
        }

        [Fact]
        public void Nao_Deve_Criar_UsuarioCapacidade_Com_Ids_Vazios()
        {
            var uc = new UsuarioCapacidade(Guid.Empty, Guid.Empty, true, Tenant, User);

            Assert.False(uc.IsValid);
            Assert.Contains(uc.Notifications, n => n.Key == nameof(UsuarioCapacidade.UsuarioId));
            Assert.Contains(uc.Notifications, n => n.Key == nameof(UsuarioCapacidade.CapacidadeId));
        }
    }
}
