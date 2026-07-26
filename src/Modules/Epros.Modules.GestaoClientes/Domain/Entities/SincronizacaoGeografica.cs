using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public enum StatusSincronizacao
    {
        Agendada = 1,
        EmExecucao = 2,
        Concluida = 3,
        Falha = 4
    }

    public class SincronizacaoGeografica : EntidadeSaaSBase, IGlobalEntity
    {
        public string VersaoArquivo { get; private set; } = string.Empty;
        public int Inseridos { get; private set; }
        public int Atualizados { get; private set; }
        public int Inativados { get; private set; }
        public StatusSincronizacao Status { get; private set; }
        public DateTime InicioEm { get; private set; }
        public DateTime? FimEm { get; private set; }
        public string? MensagemErro { get; private set; }

        protected SincronizacaoGeografica() { } // EF Core

        public SincronizacaoGeografica(string versaoArquivo, string criadoPor)
            : base("system", criadoPor)
        {
            AddNotifications(new Contract<SincronizacaoGeografica>()
                .Requires()
                .IsNotNullOrEmpty(versaoArquivo, nameof(VersaoArquivo), "A versão do arquivo é obrigatória.")
                .HasMaxLen(versaoArquivo, 50, nameof(VersaoArquivo), "A versão do arquivo deve ter no máximo 50 caracteres.")
            );

            VersaoArquivo = versaoArquivo;
            Status = StatusSincronizacao.Agendada;
            InicioEm = DateTime.UtcNow;
            Inseridos = 0;
            Atualizados = 0;
            Inativados = 0;
        }

        public void IniciarExecucao(string alteradoPor)
        {
            Status = StatusSincronizacao.EmExecucao;
            InicioEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Concluir(int inseridos, int atualizados, int inativados, string alteradoPor)
        {
            Status = StatusSincronizacao.Concluida;
            Inseridos = inseridos;
            Atualizados = atualizados;
            Inativados = inativados;
            FimEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Falhar(string mensagemErro, string alteradoPor)
        {
            Status = StatusSincronizacao.Falha;
            MensagemErro = mensagemErro;
            FimEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }
    }
}
