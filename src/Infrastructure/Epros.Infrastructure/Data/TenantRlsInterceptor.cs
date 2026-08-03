using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Epros.Shared.Application.Contracts;

namespace Epros.Infrastructure.Data
{
    public class TenantRlsInterceptor : DbCommandInterceptor
    {
        private readonly ITenantProvider _tenantProvider;

        public TenantRlsInterceptor(ITenantProvider tenantProvider)
        {
            _tenantProvider = tenantProvider;
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            SetTenantSessionVariable(command);
            return base.ReaderExecuting(command, eventData, result);
        }

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            await SetTenantSessionVariableAsync(command, cancellationToken);
            return await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<object> ScalarExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
        {
            SetTenantSessionVariable(command);
            return base.ScalarExecuting(command, eventData, result);
        }

        public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
        {
            await SetTenantSessionVariableAsync(command, cancellationToken);
            return await base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            SetTenantSessionVariable(command);
            return base.NonQueryExecuting(command, eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            await SetTenantSessionVariableAsync(command, cancellationToken);
            return await base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }

        private void SetTenantSessionVariable(DbCommand command)
        {
            if (command.Connection == null) return;

            var tenantId = _tenantProvider.GetTenantId();
            if (!string.IsNullOrEmpty(tenantId))
            {
                var sanitizedTenantId = SanitizeTenantId(tenantId);

                if (command.Connection.State != System.Data.ConnectionState.Open)
                {
                    command.Connection.Open();
                }

                using (var setCommand = command.Connection.CreateCommand())
                {
                    if (command.Transaction != null)
                    {
                        setCommand.Transaction = command.Transaction;
                    }
                    setCommand.CommandText = MontarSetCommand(command, sanitizedTenantId);
                    setCommand.ExecuteNonQuery();
                }
            }
        }

        private async Task SetTenantSessionVariableAsync(DbCommand command, CancellationToken cancellationToken)
        {
            if (command.Connection == null) return;

            var tenantId = _tenantProvider.GetTenantId();
            if (!string.IsNullOrEmpty(tenantId))
            {
                var sanitizedTenantId = SanitizeTenantId(tenantId);

                if (command.Connection.State != System.Data.ConnectionState.Open)
                {
                    await command.Connection.OpenAsync(cancellationToken);
                }

                using (var setCommand = command.Connection.CreateCommand())
                {
                    if (command.Transaction != null)
                    {
                        setCommand.Transaction = command.Transaction;
                    }
                    setCommand.CommandText = MontarSetCommand(command, sanitizedTenantId);
                    await setCommand.ExecuteNonQueryAsync(cancellationToken);
                }
            }
        }

        /// <summary>
        /// Monta o comando que propaga o tenant para a sessão do banco.
        ///
        /// Quando há uma transação ambiente, usa <c>SET LOCAL</c> (escopo transacional): o valor é
        /// automaticamente descartado no commit/rollback, evitando que uma conexão poolada carregue
        /// <c>app.current_tenant_id</c> de uma unidade de trabalho para a próxima. Sem transação,
        /// mantém o <c>SET</c> de sessão — reescrito antes de cada comando, como já era.
        /// </summary>
        private static string MontarSetCommand(DbCommand command, string sanitizedTenantId)
        {
            var escopo = command.Transaction != null ? "SET LOCAL" : "SET";
            return $"{escopo} app.current_tenant_id = '{sanitizedTenantId}';";
        }

        private static string SanitizeTenantId(string tenantId)
        {
            return tenantId.Replace("'", "''");
        }
    }
}
