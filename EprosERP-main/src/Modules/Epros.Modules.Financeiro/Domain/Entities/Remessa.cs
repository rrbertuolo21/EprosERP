using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Controle de arquivo de remessa bancária (EF FIN-SF §7.5 / §11 sf_remessa).
    /// RSF-015: registra data, nome do arquivo e grupo. Layout CNAB 240/400.
    /// </summary>
    public class Remessa : EntidadeSaaSBase
    {
        public string NomeArquivo { get; private set; } = string.Empty;
        public DateTime DataGeracao { get; private set; }
        public int Grupo { get; private set; }
        public ELayoutCnab Layout { get; private set; }
        public Guid ContaEmissoraId { get; private set; }
        public int QuantidadeTitulos { get; private set; }
        public decimal ValorTotal { get; private set; }

        private readonly List<RemessaBoleto> _boletos = new();
        public IReadOnlyCollection<RemessaBoleto> Boletos => _boletos;

        protected Remessa() { } // EF Core

        public Remessa(string nomeArquivo, DateTime dataGeracao, int grupo, ELayoutCnab layout, Guid contaEmissoraId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            NomeArquivo = nomeArquivo;
            DataGeracao = dataGeracao;
            Grupo = grupo;
            Layout = layout;
            ContaEmissoraId = contaEmissoraId;
            Validar();
        }

        /// <summary>RSF-014: adiciona um boleto/fatura à remessa (idempotente por boleto).</summary>
        public void AdicionarBoleto(Guid boletoId, Guid faturaCobrancaId, decimal valor, string usuario)
        {
            if (_boletos.Any(b => b.BoletoId == boletoId && b.DeletadoEm == null)) return; // RSF-036 unicidade remessa-boleto
            _boletos.Add(new RemessaBoleto(Id, boletoId, faturaCobrancaId, TenantId, usuario));
            QuantidadeTitulos = _boletos.Count(b => b.DeletadoEm == null);
            ValorTotal += valor;
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Remessa>()
                .Requires()
                .IsNotNullOrEmpty(NomeArquivo, nameof(NomeArquivo), "O nome do arquivo de remessa é obrigatório.")
                .AreNotEquals(ContaEmissoraId, Guid.Empty, nameof(ContaEmissoraId), "A conta emissora da remessa é obrigatória.")
            );
        }
    }
}
