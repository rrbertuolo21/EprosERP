using System;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    /// <summary>
    /// Alíquota IBPT (Lei da Transparência / De Olho no Imposto) por NCM + UF. É a tabela nacional
    /// distribuída pelo IBPT (arquivo TabelaIBPTaxSSSSSSSS.csv por UF), que informa o percentual
    /// aproximado dos tributos federais (nacional/importado), estaduais e municipais de cada NCM.
    ///
    /// O motor <c>IbptCalculo</c> apenas aplica os percentuais sobre o valor; esta tabela é o
    /// LOOKUP que fornece esses percentuais a partir do NCM/UF — no legado vinha do banco + do
    /// endpoint <c>obter-aliquotas-por-ncm-uf</c> e do loader <c>POST /ibpts/atualizar</c>.
    ///
    /// Dado NACIONAL (não por tenant): implementa <see cref="IGlobalEntity"/>.
    /// </summary>
    public class Ibpt : EntidadeSaaSBase, IGlobalEntity
    {
        /// <summary>Código NCM (8 dígitos) — ou NBS para serviços — sem máscara.</summary>
        public string Codigo { get; private set; } = string.Empty;

        /// <summary>UF a que se referem as alíquotas estadual/municipal.</summary>
        public EEstado Uf { get; private set; }

        /// <summary>EX-TIPI (0 = sem exceção). Mantém a granularidade da tabela oficial.</summary>
        public int Ex { get; private set; }

        /// <summary>Tipo do item na tabela (0=NCM, 1=NBS/serviço). Preserva a coluna "tipo" do CSV oficial.</summary>
        public int Tipo { get; private set; }

        public string? Descricao { get; private set; }

        /// <summary>Alíquota federal para produtos nacionais (%).</summary>
        public decimal AliquotaNacionalFederal { get; private set; }

        /// <summary>Alíquota federal para produtos importados (%).</summary>
        public decimal AliquotaImportadosFederal { get; private set; }

        /// <summary>Alíquota estadual (%).</summary>
        public decimal AliquotaEstadual { get; private set; }

        /// <summary>Alíquota municipal (%).</summary>
        public decimal AliquotaMunicipal { get; private set; }

        /// <summary>Versão da tabela IBPT (coluna "versao" do CSV, ex.: "25.1.A").</summary>
        public string Versao { get; private set; } = string.Empty;

        /// <summary>Chave do lançamento na Receita (coluna "chave" do CSV — token de autenticidade).</summary>
        public string? Chave { get; private set; }

        public DateTime VigenciaInicio { get; private set; }
        public DateTime VigenciaFim { get; private set; }

        protected Ibpt() { } // EF Core

        public Ibpt(
            string codigo,
            EEstado uf,
            int ex,
            int tipo,
            string? descricao,
            decimal aliquotaNacionalFederal,
            decimal aliquotaImportadosFederal,
            decimal aliquotaEstadual,
            decimal aliquotaMunicipal,
            string versao,
            string? chave,
            DateTime vigenciaInicio,
            DateTime vigenciaFim,
            string criadoPor)
            : base("system", criadoPor)
        {
            Codigo = (codigo ?? string.Empty).Trim();
            Uf = uf;
            Ex = ex;
            Tipo = tipo;
            Descricao = descricao;
            AliquotaNacionalFederal = aliquotaNacionalFederal;
            AliquotaImportadosFederal = aliquotaImportadosFederal;
            AliquotaEstadual = aliquotaEstadual;
            AliquotaMunicipal = aliquotaMunicipal;
            Versao = versao ?? string.Empty;
            Chave = chave;
            VigenciaInicio = vigenciaInicio;
            VigenciaFim = vigenciaFim;
            Validar();
        }

        public void Alterar(
            int ex,
            int tipo,
            string? descricao,
            decimal aliquotaNacionalFederal,
            decimal aliquotaImportadosFederal,
            decimal aliquotaEstadual,
            decimal aliquotaMunicipal,
            string versao,
            string? chave,
            DateTime vigenciaInicio,
            DateTime vigenciaFim,
            string alteradoPor)
        {
            Ex = ex;
            Tipo = tipo;
            Descricao = descricao;
            AliquotaNacionalFederal = aliquotaNacionalFederal;
            AliquotaImportadosFederal = aliquotaImportadosFederal;
            AliquotaEstadual = aliquotaEstadual;
            AliquotaMunicipal = aliquotaMunicipal;
            Versao = versao ?? string.Empty;
            Chave = chave;
            VigenciaInicio = vigenciaInicio;
            VigenciaFim = vigenciaFim;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Ibpt>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O código NCM/NBS é obrigatório [Origem: Ibpt]")
                .IsLowerOrEqualsThan(Codigo, 8, nameof(Codigo), "O código deve ter no máximo 8 dígitos [Origem: Ibpt]")
                .IsTrue(Enum.IsDefined(typeof(EEstado), Uf), nameof(Uf), "UF inválida [Origem: Ibpt]")
                .IsGreaterOrEqualsThan(AliquotaNacionalFederal, 0m, nameof(AliquotaNacionalFederal), "Alíquota federal nacional não pode ser negativa [Origem: Ibpt]")
                .IsGreaterOrEqualsThan(AliquotaImportadosFederal, 0m, nameof(AliquotaImportadosFederal), "Alíquota federal importado não pode ser negativa [Origem: Ibpt]")
                .IsGreaterOrEqualsThan(AliquotaEstadual, 0m, nameof(AliquotaEstadual), "Alíquota estadual não pode ser negativa [Origem: Ibpt]")
                .IsGreaterOrEqualsThan(AliquotaMunicipal, 0m, nameof(AliquotaMunicipal), "Alíquota municipal não pode ser negativa [Origem: Ibpt]")
            );
        }
    }
}
