using System;
using System.Collections.Generic;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Qualidade.Domain.Entities
{
    /// <summary>
    /// qld_atr_atributo — Catalogo governado de atributos/caracteristicas.
    /// Porte fiel da EF QUALIDADE / GESTAO_DE_ATRIBUTOS (secao 12.1).
    /// </summary>
    public class AtrAtributo : EntidadeSaaSBase
    {
        public long? SequenciaExibicao { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string NomeInterno { get; private set; } = string.Empty;
        public string Rotulo { get; private set; } = string.Empty;
        public ETipoAtributo TipoAtributo { get; private set; }
        public ETipoCaracteristica? TipoCaracteristica { get; private set; }
        public ETipoDadoAtributo TipoDado { get; private set; }
        public EEscopoAtributo Escopo { get; private set; }
        public int? Posicao { get; private set; }
        public EStatusRegistroQualidade Status { get; private set; }
        public bool ExibirFormularioPadrao { get; private set; }
        public bool Obrigatorio { get; private set; }
        public bool SensivelLgpd { get; private set; }
        public Guid? ResponsavelId { get; private set; }
        public int Versao { get; private set; }

        private readonly List<AtrOpcao> _opcoes = new();
        public IReadOnlyCollection<AtrOpcao> Opcoes => _opcoes.AsReadOnly();

        protected AtrAtributo() { }

        public AtrAtributo(string codigo, string nomeInterno, string rotulo, ETipoAtributo tipoAtributo,
            ETipoDadoAtributo tipoDado, EEscopoAtributo escopo, bool exibirFormularioPadrao, bool obrigatorio,
            ETipoCaracteristica? tipoCaracteristica, bool sensivelLgpd, int? posicao, Guid? responsavelId,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            NomeInterno = nomeInterno;
            Rotulo = rotulo;
            TipoAtributo = tipoAtributo;
            TipoDado = tipoDado;
            Escopo = escopo;
            ExibirFormularioPadrao = exibirFormularioPadrao;
            Obrigatorio = obrigatorio;
            TipoCaracteristica = tipoCaracteristica;
            SensivelLgpd = sensivelLgpd;
            Posicao = posicao;
            ResponsavelId = responsavelId;
            Status = EStatusRegistroQualidade.Rascunho;
            Versao = 1;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<AtrAtributo>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo do atributo e obrigatorio [Origem: AtrAtributo]")
                .IsLowerOrEqualsThan(Codigo?.Length ?? 0, 30, nameof(Codigo), "O codigo deve ter no maximo 30 caracteres [Origem: AtrAtributo]")
                .IsNotNullOrEmpty(NomeInterno, nameof(NomeInterno), "O nome interno e obrigatorio [Origem: AtrAtributo]")
                .IsNotNullOrEmpty(Rotulo, nameof(Rotulo), "O rotulo e obrigatorio [Origem: AtrAtributo]"));

            // QLD-ATR-RN-020: atributo de qualidade usado em inspecao deve ter tipo de caracteristica.
            if (TipoAtributo == ETipoAtributo.Qualidade && TipoCaracteristica == null)
                AddNotification(nameof(TipoCaracteristica), "Atributo de qualidade deve possuir tipo de caracteristica [Origem: AtrAtributo]");
        }

        public void Alterar(string rotulo, ETipoDadoAtributo tipoDado, bool obrigatorio, bool exibirFormularioPadrao,
            int? posicao, string usuario)
        {
            Rotulo = rotulo;
            TipoDado = tipoDado;
            Obrigatorio = obrigatorio;
            ExibirFormularioPadrao = exibirFormularioPadrao;
            Posicao = posicao;
            Versao++;
            MarcarAlterado(usuario);
            Validar();
        }

        // Maquina de estados fiel a secao 13.
        public void Submeter(string usuario) => Transicionar(EStatusRegistroQualidade.Rascunho, EStatusRegistroQualidade.EmAnalise, usuario);
        public void Aprovar(string usuario) => Transicionar(EStatusRegistroQualidade.EmAnalise, EStatusRegistroQualidade.Ativo, usuario);
        public void Rejeitar(string usuario) => Transicionar(EStatusRegistroQualidade.EmAnalise, EStatusRegistroQualidade.Rascunho, usuario);
        public void Suspender(string usuario) => Transicionar(EStatusRegistroQualidade.Ativo, EStatusRegistroQualidade.Suspenso, usuario);
        public void Inativar(string usuario) => Transicionar(EStatusRegistroQualidade.Ativo, EStatusRegistroQualidade.Inativo, usuario);
        public void Reativar(string usuario) => Transicionar(EStatusRegistroQualidade.Inativo, EStatusRegistroQualidade.Ativo, usuario);
        public void Retomar(string usuario) => Transicionar(EStatusRegistroQualidade.Suspenso, EStatusRegistroQualidade.Ativo, usuario);

        private void Transicionar(EStatusRegistroQualidade de, EStatusRegistroQualidade para, string usuario)
        {
            if (Status != de)
            {
                AddNotification(nameof(Status), $"Transicao invalida de {Status} para {para} [Origem: AtrAtributo]");
                return;
            }
            Status = para;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void AdicionarOpcao(AtrOpcao opcao) => _opcoes.Add(opcao);
    }
}
