using System;
using System.Collections.Generic;
using System.Text.Json;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class ExecucaoMassa : EntidadeSaaSBase
    {
        public string TipoOperacao { get; private set; } = string.Empty;
        public string Parametros { get; private set; } = string.Empty;
        public string Status { get; private set; } = "PendenteAprovacao"; // PendenteAprovacao, Executado, Falho, Rejeitado
        public string AprovadoresJson { get; private set; } = "[]";
        public string? ResultadoLog { get; private set; }
        public DateTime? ExecutadoEm { get; private set; }

        protected ExecucaoMassa() { } // EF Core

        public ExecucaoMassa(string tipoOperacao, string parametros, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ExecucaoMassa>()
                .Requires()
                .IsNotNullOrEmpty(tipoOperacao, nameof(TipoOperacao), "O tipo de operação é obrigatório")
                .IsNotNullOrEmpty(parametros, nameof(Parametros), "Os parâmetros da operação são obrigatórios")
            );

            TipoOperacao = tipoOperacao;
            Parametros = parametros;
            
            // Adiciona o criador como o primeiro aprovador
            var aprovadores = new List<string> { criadoPor };
            AprovadoresJson = JsonSerializer.Serialize(aprovadores);
        }

        public List<string> ObterAprovadores()
        {
            if (string.IsNullOrEmpty(AprovadoresJson)) return new List<string>();
            return JsonSerializer.Deserialize<List<string>>(AprovadoresJson) ?? new List<string>();
        }

        public void AdicionarAprovacao(string userId, string alteradoPor)
        {
            if (Status != "PendenteAprovacao")
            {
                AddNotification(nameof(Status), "A execução em lote não está pendente de aprovação.");
                return;
            }

            var aprovadores = ObterAprovadores();
            if (aprovadores.Contains(userId))
            {
                AddNotification(nameof(AprovadoresJson), "Este usuário já aprovou esta execução em lote.");
                return;
            }

            aprovadores.Add(userId);
            AprovadoresJson = JsonSerializer.Serialize(aprovadores);
            MarcarAlterado(alteradoPor);
        }

        public void Executar(string log, string status, string alteradoPor)
        {
            Status = status;
            ResultadoLog = log;
            ExecutadoEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Rejeitar(string alteradoPor)
        {
            Status = "Rejeitado";
            MarcarAlterado(alteradoPor);
        }
    }
}
