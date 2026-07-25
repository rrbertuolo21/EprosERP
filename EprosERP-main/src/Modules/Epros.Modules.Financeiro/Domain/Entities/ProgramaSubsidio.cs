using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Programa de subsídio / fundo / incentivo controlado financeiramente (EF FIN-SBF §11.1 ProgramaSubsidio).
    /// Submódulo de evolução — sobe desabilitado (ABAC nega por padrão). Isolamento por tenant via ContextBase.
    /// </summary>
    public class ProgramaSubsidio : EntidadeSaaSBase
    {
        public long? SequenciaExibicao { get; private set; }
        public string Orgao { get; private set; } = string.Empty;
        public decimal ValorTotal { get; private set; }
        public DateTime VigenciaInicio { get; private set; }
        public DateTime? VigenciaFim { get; private set; }
        public EEstadoProgramaSubsidio Estado { get; private set; } = EEstadoProgramaSubsidio.Vigente;

        protected ProgramaSubsidio() { } // EF Core

        public ProgramaSubsidio(string orgao, decimal valorTotal, DateTime vigenciaInicio, DateTime? vigenciaFim,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Orgao = orgao;
            ValorTotal = valorTotal;
            VigenciaInicio = vigenciaInicio;
            VigenciaFim = vigenciaFim;
            Estado = EEstadoProgramaSubsidio.Vigente;
            Validar();
        }

        public void Alterar(string orgao, decimal valorTotal, DateTime vigenciaInicio, DateTime? vigenciaFim, string usuario)
        {
            Orgao = orgao;
            ValorTotal = valorTotal;
            VigenciaInicio = vigenciaInicio;
            VigenciaFim = vigenciaFim;
            MarcarAlterado(usuario);
            Validar();
        }

        public void IniciarPrestacaoContas(string usuario)
        {
            if (Estado != EEstadoProgramaSubsidio.Vigente)
            {
                AddNotification(nameof(Estado), "Somente programa vigente pode iniciar prestação de contas.");
                return;
            }
            Estado = EEstadoProgramaSubsidio.PrestacaoContas;
            MarcarAlterado(usuario);
        }

        public void Encerrar(string usuario)
        {
            if (Estado == EEstadoProgramaSubsidio.Encerrado)
            {
                AddNotification(nameof(Estado), "Programa já encerrado.");
                return;
            }
            Estado = EEstadoProgramaSubsidio.Encerrado;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ProgramaSubsidio>()
                .Requires()
                .IsNotNullOrEmpty(Orgao, nameof(Orgao), "O órgão do programa é obrigatório [Origem: ProgramaSubsidio]")
                .IsGreaterThan(ValorTotal, 0, nameof(ValorTotal), "O valor total do programa deve ser maior que zero [Origem: ProgramaSubsidio]")
                .IsGreaterThan(VigenciaInicio, DateTime.MinValue, nameof(VigenciaInicio), "A vigência do programa é obrigatória [Origem: ProgramaSubsidio]")
            );
        }
    }
}
