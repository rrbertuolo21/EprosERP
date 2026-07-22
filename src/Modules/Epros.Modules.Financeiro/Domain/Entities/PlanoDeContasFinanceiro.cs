using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    public class PlanoDeContasFinanceiro : EntidadeSaaSBase
    {
        /// <summary>Sequência legada (SequenciaTenantId) — somente exibição/UX. Não substitui o Id (Guid).</summary>
        public long? SequenciaExibicao { get; private set; }

        public Guid? ConfiguracaoCodigoNaturezaFinanceiraRecebimentoId { get; private set; }
        public Guid? ConfiguracaoCodigoNaturezaFinanceiraPagamentoId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public string Mascara { get; private set; } = string.Empty;
        public bool EhPadrao { get; private set; }

        public ICollection<PlanoDeContasFinanceiroItem> Itens { get; private set; } = new List<PlanoDeContasFinanceiroItem>();

        /// <summary>
        /// Relação N:N legada com Empresa (legado: ICollection&lt;Empresa&gt; Empresas), restaurada via
        /// coleção de EmpresaId (entidade de junção PlanoDeContasFinanceiroEmpresa) — sem navegação cross-module.
        /// </summary>
        private readonly List<PlanoDeContasFinanceiroEmpresa> _empresas = new();
        public IReadOnlyCollection<PlanoDeContasFinanceiroEmpresa> Empresas => _empresas;

        public ConfiguracaoCodigoNaturezaFinanceira? ConfiguracaoCodigoNaturezaFinanceiraRecebimento { get; private set; }
        public ConfiguracaoCodigoNaturezaFinanceira? ConfiguracaoCodigoNaturezaFinanceiraPagamento { get; private set; }

        protected PlanoDeContasFinanceiro() { } // EF Core

        public PlanoDeContasFinanceiro(Guid? configuracaoCodigoNaturezaFinanceiraRecebimentoId, Guid? configuracaoCodigoNaturezaFinanceiraPagamentoId, string descricao, string mascara, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ConfiguracaoCodigoNaturezaFinanceiraRecebimentoId = configuracaoCodigoNaturezaFinanceiraRecebimentoId;
            ConfiguracaoCodigoNaturezaFinanceiraPagamentoId = configuracaoCodigoNaturezaFinanceiraPagamentoId;
            Descricao = descricao;
            Mascara = mascara;
            Validar();
        }

        public void Validar()
        {
            AddNotifications(new Contract<PlanoDeContasFinanceiro>()
                .Requires()
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descrição é obrigatória.")
                .IsLowerOrEqualsThan(Descricao?.Length ?? 0, 50, nameof(Descricao), "O campo Descricao deve ter no máximo 50 caracteres [Origem: PlanoDeContasFinanceiro]")
                .IsNotNullOrEmpty(Mascara, nameof(Mascara), "A máscara é obrigatória.")
                .IsLowerOrEqualsThan(Mascara?.Length ?? 0, 100, nameof(Mascara), "O campo Mascara deve ter no máximo 100 caracteres [Origem: PlanoDeContasFinanceiro]")
            );
        }

        public void Alterar(Guid? configuracaoCodigoNaturezaFinanceiraRecebimentoId, Guid? configuracaoCodigoNaturezaFinanceiraPagamentoId, string descricao, string mascara, string usuario)
        {
            ConfiguracaoCodigoNaturezaFinanceiraRecebimentoId = configuracaoCodigoNaturezaFinanceiraRecebimentoId;
            ConfiguracaoCodigoNaturezaFinanceiraPagamentoId = configuracaoCodigoNaturezaFinanceiraPagamentoId;
            Descricao = descricao;
            Mascara = mascara;
            MarcarAlterado(usuario);
            Validar();
        }

        public void MarcarPlanoPadrao() => EhPadrao = true;
        public void DesmarcarPlanoPadrao() => EhPadrao = false;

        /// <summary>Vincula uma empresa ao plano (N:N legado). Idempotente.</summary>
        public void VincularEmpresa(Guid empresaId, string usuario)
        {
            if (empresaId == Guid.Empty) return;
            if (_empresas.Any(e => e.EmpresaId == empresaId && e.DeletadoEm == null)) return;
            _empresas.Add(new PlanoDeContasFinanceiroEmpresa(Id, empresaId, TenantId, usuario));
        }

        /// <summary>Desvincula (soft delete) uma empresa do plano.</summary>
        public void DesvincularEmpresa(Guid empresaId, string usuario)
        {
            var vinculo = _empresas.FirstOrDefault(e => e.EmpresaId == empresaId && e.DeletadoEm == null);
            vinculo?.Deletar(usuario);
        }

        /// <summary>Substitui todo o conjunto de empresas vinculadas pelo informado.</summary>
        public void SubstituirEmpresas(IEnumerable<Guid> empresaIds, string usuario)
        {
            var novos = (empresaIds ?? Enumerable.Empty<Guid>()).Where(id => id != Guid.Empty).Distinct().ToList();

            foreach (var vinculo in _empresas.Where(e => e.DeletadoEm == null && !novos.Contains(e.EmpresaId)).ToList())
                vinculo.Deletar(usuario);

            foreach (var id in novos)
                VincularEmpresa(id, usuario);
        }
    }
}
