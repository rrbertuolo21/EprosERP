using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class Ncm : EntidadeSaaSBase, IGlobalEntity
    {
        public string CodigoNcm { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public DateTime DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public string? TipoAtoIni { get; private set; }
        public string? NumeroAtoIni { get; private set; }
        public string? AnoAtoIni { get; private set; }

        protected Ncm() { } // EF Core

        public Ncm(
            string codigoNcm,
            string descricao,
            DateTime dataInicio,
            DateTime? dataFim,
            string? tipoAtoIni,
            string? numeroAtoIni,
            string? anoAtoIni,
            string criadoPor) : base("system", criadoPor)
        {
            CodigoNcm = codigoNcm;
            Descricao = descricao;
            DataInicio = dataInicio;
            DataFim = dataFim;
            TipoAtoIni = tipoAtoIni;
            NumeroAtoIni = numeroAtoIni;
            AnoAtoIni = anoAtoIni;
            Validar();
        }

        public void Alterar(
            string codigoNcm,
            string descricao,
            DateTime dataInicio,
            DateTime? dataFim,
            string? tipoAtoIni,
            string? numeroAtoIni,
            string? anoAtoIni,
            string alteradoPor)
        {
            CodigoNcm = codigoNcm;
            Descricao = descricao;
            DataInicio = dataInicio;
            DataFim = dataFim;
            TipoAtoIni = tipoAtoIni;
            NumeroAtoIni = numeroAtoIni;
            AnoAtoIni = anoAtoIni;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Ncm>()
                .Requires()
                .IsLowerOrEqualsThan((CodigoNcm ?? "").Length, 8, nameof(CodigoNcm), "O campo CodigoNcm deve ter no máximo 8 caracteres [Origem: Ncm]")
                .IsLowerOrEqualsThan((Descricao ?? "").Length, 1500, nameof(Descricao), "O campo Descricao deve ter no máximo 1500 caracteres [Origem: Ncm]")
            );
        }
    }
}
