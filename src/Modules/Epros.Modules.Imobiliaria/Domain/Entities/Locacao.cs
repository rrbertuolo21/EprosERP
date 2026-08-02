using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Imobiliaria.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Imobiliaria.Domain.Entities
{
    /// <summary>
    /// Movimento contratual de aluguel (EF GESTAO_IMOBILIARIA 11.7, tabela imo_locacao).
    /// Agregado raiz de locatarios, fiadores, custos e documentos.
    /// RN-011: validacao funcional obrigatoria. RN-012: periodo, valor e vencimento. RN-013: partes N:N.
    /// </summary>
    public class Locacao : EntidadeSaaSBase
    {
        public Guid? ImovelId { get; private set; }
        public DateTime PeriodoInicial { get; private set; }
        public DateTime PeriodoFinal { get; private set; }
        public decimal Valor { get; private set; }
        // Vencimento pode representar dia do mes (1-31) — decisao encaminhada a MC (EF 11.7).
        public int Vencimento { get; private set; }
        public EStatusLocacao Status { get; private set; }

        private readonly List<LocacaoParte> _partes = new();
        private readonly List<LocacaoCusto> _custos = new();
        private readonly List<LocacaoDocumento> _documentos = new();

        public IReadOnlyCollection<LocacaoParte> Partes => _partes.AsReadOnly();
        public IReadOnlyCollection<LocacaoCusto> Custos => _custos.AsReadOnly();
        public IReadOnlyCollection<LocacaoDocumento> Documentos => _documentos.AsReadOnly();

        public IEnumerable<LocacaoParte> Locatarios => _partes.Where(p => p.Papel == EPapelParteLocacao.Locatario);
        public IEnumerable<LocacaoParte> Fiadores => _partes.Where(p => p.Papel == EPapelParteLocacao.Fiador);

        protected Locacao() { } // EF Core

        public Locacao(
            Guid? imovelId,
            DateTime periodoInicial,
            DateTime periodoFinal,
            decimal valor,
            int vencimento,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ImovelId = imovelId;
            PeriodoInicial = periodoInicial.Date;
            PeriodoFinal = periodoFinal.Date;
            Valor = valor;
            Vencimento = vencimento;
            Status = EStatusLocacao.EmElaboracao;
            Validar();
        }

        public void AdicionarLocatario(Guid pessoaId, string usuario)
            => AdicionarParte(new LocacaoParte(pessoaId, EPapelParteLocacao.Locatario, TenantId, usuario));

        public void AdicionarFiador(Guid pessoaId, string usuario)
            => AdicionarParte(new LocacaoParte(pessoaId, EPapelParteLocacao.Fiador, TenantId, usuario));

        public void AdicionarParte(LocacaoParte parte)
        {
            parte.VincularALocacao(Id);
            _partes.Add(parte);
        }

        public void AdicionarCusto(LocacaoCusto custo)
        {
            custo.VincularALocacao(Id);
            _custos.Add(custo);
        }

        public void AdicionarDocumento(LocacaoDocumento documento)
        {
            documento.VincularALocacao(Id);
            _documentos.Add(documento);
        }

        // RN-011 (secao 12): transicoes de estado da locacao.
        public void Formalizar(string usuario)
        {
            if (Status != EStatusLocacao.EmElaboracao)
            {
                AddNotification(nameof(Status), "Apenas locacoes em elaboracao podem ser formalizadas.");
                return;
            }
            Validar();
            if (!IsValid) return;
            Status = EStatusLocacao.Vigente;
            MarcarAlterado(usuario);
        }

        public void Encerrar(string usuario)
        {
            if (Status != EStatusLocacao.Vigente)
            {
                AddNotification(nameof(Status), "Apenas locacoes vigentes podem ser encerradas.");
                return;
            }
            Status = EStatusLocacao.Encerrada;
            MarcarAlterado(usuario);
        }

        public void Cancelar(string usuario)
        {
            if (Status == EStatusLocacao.Encerrada || Status == EStatusLocacao.Cancelada)
            {
                AddNotification(nameof(Status), "Locacoes encerradas ou canceladas nao podem ser canceladas.");
                return;
            }
            Status = EStatusLocacao.Cancelada;
            MarcarAlterado(usuario);
        }

        /// <summary>
        /// Edicao da locacao (ID3/PRD-03). Enquanto EmElaboracao a edicao e livre;
        /// Vigente so por aditivo/nova versao (reajuste/renovacao com trilha propria).
        /// </summary>
        public void AtualizarDados(
            Guid? imovelId,
            DateTime periodoInicial,
            DateTime periodoFinal,
            decimal valor,
            int vencimento,
            string usuario)
        {
            if (Status != EStatusLocacao.EmElaboracao)
            {
                AddNotification(nameof(Status),
                    "Locacao vigente/encerrada nao pode ser editada livremente; use reajuste, renovacao ou aditivo. (RN-011)");
                return;
            }
            ImovelId = imovelId;
            PeriodoInicial = periodoInicial.Date;
            PeriodoFinal = periodoFinal.Date;
            Valor = valor;
            Vencimento = vencimento;
            Validar();
            if (!IsValid) return;
            MarcarAlterado(usuario);
        }

        /// <summary>
        /// Aplica um novo valor de aluguel resultante de reajuste (ID7/NF-02). Somente Vigente.
        /// O calculo do novo valor e responsabilidade do reajuste (valida-contador); aqui so
        /// refletimos o valor ratificado no contrato, com trilha por LocacaoReajuste.
        /// </summary>
        public void AplicarNovoValor(decimal novoValor, string usuario)
        {
            if (Status != EStatusLocacao.Vigente)
            {
                AddNotification(nameof(Status), "Apenas locacoes vigentes podem ser reajustadas.");
                return;
            }
            if (novoValor <= 0)
            {
                AddNotification(nameof(Valor), "O valor reajustado deve ser positivo. (RN-012)");
                return;
            }
            Valor = novoValor;
            MarcarAlterado(usuario);
        }

        /// <summary>
        /// Renovacao (ID7): estende o periodo de uma locacao vigente (aditivo). Mantem Vigente.
        /// </summary>
        public void Renovar(DateTime novoPeriodoFinal, string usuario)
        {
            if (Status != EStatusLocacao.Vigente)
            {
                AddNotification(nameof(Status), "Apenas locacoes vigentes podem ser renovadas.");
                return;
            }
            if (novoPeriodoFinal.Date <= PeriodoFinal)
            {
                AddNotification(nameof(PeriodoFinal),
                    "O novo fim de vigencia deve ser posterior ao fim atual. (RN-012)");
                return;
            }
            PeriodoFinal = novoPeriodoFinal.Date;
            MarcarAlterado(usuario);
        }

        /// <summary>Mapa do ciclo proprio ao status canonico da plataforma (T3).</summary>
        public Epros.Shared.Domain.Enums.ESituacaoCanonica SituacaoCanonica => Status switch
        {
            EStatusLocacao.EmElaboracao => Epros.Shared.Domain.Enums.ESituacaoCanonica.Rascunho,
            EStatusLocacao.Vigente => Epros.Shared.Domain.Enums.ESituacaoCanonica.Ativo,
            EStatusLocacao.Encerrada => Epros.Shared.Domain.Enums.ESituacaoCanonica.Encerrado,
            EStatusLocacao.Cancelada => Epros.Shared.Domain.Enums.ESituacaoCanonica.Cancelado,
            _ => Epros.Shared.Domain.Enums.ESituacaoCanonica.Rascunho
        };

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Locacao>()
                .Requires()
                .IsGreaterThan(Valor, 0, nameof(Valor),
                    "O valor da locacao deve ser positivo. [Origem: Locacao] (RN-012)")
                .IsBetween(Vencimento, 1, 31, nameof(Vencimento),
                    "O vencimento deve estar entre 1 e 31. [Origem: Locacao] (RN-012)")
                .IsFalse(PeriodoFinal < PeriodoInicial, nameof(PeriodoFinal),
                    "O fim do periodo deve ser igual ou posterior ao inicio. [Origem: Locacao] (RN-012)"));

            foreach (var p in _partes) AddNotifications(p);
            foreach (var c in _custos) AddNotifications(c);
            foreach (var d in _documentos) AddNotifications(d);
        }
    }
}
