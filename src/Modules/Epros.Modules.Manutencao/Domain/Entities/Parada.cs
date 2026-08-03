using System;
using System.Collections.Generic;
using Epros.Modules.Manutencao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Manutencao.Domain.Entities
{
    /// <summary>MAN-PAR — Parada (agregado raiz). EF 11.1.</summary>
    public class Parada : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public EStatusRegistroManutencao Status { get; private set; } = EStatusRegistroManutencao.Rascunho;
        public Guid ResponsavelId { get; private set; }
        public Guid? EquipamentoId { get; private set; }
        public Guid? LinhaId { get; private set; }
        public Guid? CelulaId { get; private set; }
        public DateTime DataHoraInicio { get; private set; }
        public DateTime? DataHoraFim { get; private set; }
        public decimal? DuracaoMinutos { get; private set; }
        public ETipoParada TipoParada { get; private set; }
        public Guid? MotivoParadaId { get; private set; }
        public Guid? OsGeradaId { get; private set; }
        public string? Observacao { get; private set; }
        public Guid UsuarioRegistroId { get; private set; }
        public int Versao { get; private set; } = 1;

        public List<ParadaItem> Itens { get; private set; } = new();
        public List<ParadaVinculoOs> VinculosOs { get; private set; } = new();
        public List<ParadaIndicador> Indicadores { get; private set; } = new();

        protected Parada() { }

        public Parada(
            string codigo,
            string descricao,
            Guid responsavelId,
            ETipoParada tipoParada,
            DateTime dataHoraInicio,
            Guid usuarioRegistroId,
            Guid? equipamentoId,
            Guid? linhaId,
            Guid? celulaId,
            Guid? motivoParadaId,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            ResponsavelId = responsavelId;
            TipoParada = tipoParada;
            DataHoraInicio = dataHoraInicio;
            UsuarioRegistroId = usuarioRegistroId;
            EquipamentoId = equipamentoId;
            LinhaId = linhaId;
            CelulaId = celulaId;
            MotivoParadaId = motivoParadaId;
            Observacao = observacao;
            Status = EStatusRegistroManutencao.Rascunho;
            Versao = 1;
            Validar();
        }

        public void AdicionarItem(ParadaItem item, string usuario)
        {
            if (!item.IsValid) { AddNotifications(item.Notifications); return; }
            Itens.Add(item);
            MarcarAlterado(usuario);
        }

        // Finaliza a parada calculando a duracao em minutos (fim - inicio).
        public void Finalizar(DateTime dataHoraFim, string usuario)
        {
            if (DataHoraFim.HasValue)
            {
                AddNotification(nameof(DataHoraFim), "A parada ja foi finalizada.");
                return;
            }
            if (dataHoraFim < DataHoraInicio)
            {
                AddNotification(nameof(DataHoraFim), "A data/hora de fim nao pode ser anterior ao inicio.");
                return;
            }
            DataHoraFim = dataHoraFim;
            DuracaoMinutos = (decimal)(dataHoraFim - DataHoraInicio).TotalMinutes;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void Classificar(Guid motivoParadaId, string usuario)
        {
            if (motivoParadaId == Guid.Empty)
            {
                AddNotification(nameof(MotivoParadaId), "Informe um motivo de parada valido.");
                return;
            }
            MotivoParadaId = motivoParadaId;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void VincularOsGerada(Guid osId, string usuario)
        {
            OsGeradaId = osId;
            Versao++;
            MarcarAlterado(usuario);
        }

        // MAN-PAR D16: registra snapshot de indicador calculado pelo motor.
        public void RegistrarIndicador(ParadaIndicador indicador, string usuario)
        {
            if (!indicador.IsValid) { AddNotifications(indicador.Notifications); return; }
            Indicadores.Add(indicador);
            MarcarAlterado(usuario);
        }

        // MAN-PAR D17: registra vinculo de OS (corretiva gerada ou vinculo manual) a parada.
        public void AdicionarVinculoOs(ParadaVinculoOs vinculo, string usuario)
        {
            if (!vinculo.IsValid) { AddNotifications(vinculo.Notifications); return; }
            VinculosOs.Add(vinculo);
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Parada>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo da parada e obrigatorio [Origem: Parada].")
                .IsLowerOrEqualsThan(Codigo?.Length ?? 0, 30, nameof(Codigo), "O codigo deve ter no maximo 30 caracteres.")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descricao da parada e obrigatoria [Origem: Parada].")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio.")
                .AreNotEquals(UsuarioRegistroId, Guid.Empty, nameof(UsuarioRegistroId), "O usuario de registro e obrigatorio."));
        }
    }

    /// <summary>MAN-PAR — Item/impacto de parada. EF 11.2.</summary>
    public class ParadaItem : EntidadeSaaSBase
    {
        public Guid ParadaId { get; private set; }
        public int Sequencia { get; private set; }
        public string? TipoImpacto { get; private set; }
        public decimal? Quantidade { get; private set; }
        public string? Unidade { get; private set; }
        public string? Observacao { get; private set; }

        protected ParadaItem() { }

        public ParadaItem(Guid paradaId, int sequencia, string? tipoImpacto, decimal? quantidade, string? unidade, string? observacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ParadaId = paradaId;
            Sequencia = sequencia;
            TipoImpacto = tipoImpacto;
            Quantidade = quantidade;
            Unidade = unidade;
            Observacao = observacao;

            AddNotifications(new Contract<ParadaItem>()
                .Requires()
                .AreNotEquals(paradaId, Guid.Empty, nameof(ParadaId), "A parada e obrigatoria."));
        }
    }

    /// <summary>MAN-PAR — Motivo de parada (arvore simplificada). EF 11.3.</summary>
    public class MotivoParada : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public Guid? MotivoPaiId { get; private set; }
        public string? TipoParadaAplicavel { get; private set; }
        public bool Ativo { get; private set; } = true;
        public bool ExigeObservacao { get; private set; }
        public bool ExigeAnexo { get; private set; }

        protected MotivoParada() { }

        public MotivoParada(string codigo, string descricao, Guid? motivoPaiId, string? tipoParadaAplicavel, bool exigeObservacao, bool exigeAnexo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            MotivoPaiId = motivoPaiId;
            TipoParadaAplicavel = tipoParadaAplicavel;
            ExigeObservacao = exigeObservacao;
            ExigeAnexo = exigeAnexo;
            Ativo = true;

            AddNotifications(new Contract<MotivoParada>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O codigo do motivo e obrigatorio.")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descricao do motivo e obrigatoria."));
        }
    }

    /// <summary>MAN-PAR — Vinculo de OS gerada/vinculada a parada. EF 11.4.</summary>
    public class ParadaVinculoOs : EntidadeSaaSBase
    {
        public Guid ParadaId { get; private set; }
        public Guid? OrdemServicoId { get; private set; }
        public ETipoVinculoOsParada TipoVinculo { get; private set; }
        public string? RegraAcionadora { get; private set; }
        public EStatusVinculoOsParada StatusVinculo { get; private set; } = EStatusVinculoOsParada.Pendente;
        public DateTime DataHoraAcionamento { get; private set; }
        public string? MensagemErro { get; private set; }

        protected ParadaVinculoOs() { }

        public ParadaVinculoOs(Guid paradaId, ETipoVinculoOsParada tipoVinculo, Guid? ordemServicoId, string? regraAcionadora, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ParadaId = paradaId;
            TipoVinculo = tipoVinculo;
            OrdemServicoId = ordemServicoId;
            RegraAcionadora = regraAcionadora;
            StatusVinculo = EStatusVinculoOsParada.Pendente;
            DataHoraAcionamento = DateTime.UtcNow;

            AddNotifications(new Contract<ParadaVinculoOs>()
                .Requires()
                .AreNotEquals(paradaId, Guid.Empty, nameof(ParadaId), "A parada e obrigatoria."));
        }

        public void RegistrarGeracao(Guid osId, string usuario)
        {
            OrdemServicoId = osId;
            StatusVinculo = EStatusVinculoOsParada.Gerada;
            MarcarAlterado(usuario);
        }

        public void RegistrarFalha(string erro, string usuario)
        {
            StatusVinculo = EStatusVinculoOsParada.Falhou;
            MensagemErro = erro;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>MAN-PAR — Indicador derivado de parada. EF 11.5.</summary>
    public class ParadaIndicador : EntidadeSaaSBase
    {
        public Guid ParadaId { get; private set; }
        public ETipoIndicadorParada TipoIndicador { get; private set; }
        public DateTime? PeriodoInicio { get; private set; }
        public DateTime? PeriodoFim { get; private set; }
        public decimal Valor { get; private set; }
        public string? Unidade { get; private set; }
        public string? FormulaAplicada { get; private set; }
        public string? OrigemDados { get; private set; }
        public DateTime DataCalculo { get; private set; }

        protected ParadaIndicador() { }

        public ParadaIndicador(Guid paradaId, ETipoIndicadorParada tipoIndicador, decimal valor, string? unidade, string? formulaAplicada, DateTime? periodoInicio, DateTime? periodoFim, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ParadaId = paradaId;
            TipoIndicador = tipoIndicador;
            Valor = valor;
            Unidade = unidade;
            FormulaAplicada = formulaAplicada;
            PeriodoInicio = periodoInicio;
            PeriodoFim = periodoFim;
            DataCalculo = DateTime.UtcNow;

            AddNotifications(new Contract<ParadaIndicador>()
                .Requires()
                .AreNotEquals(paradaId, Guid.Empty, nameof(ParadaId), "A parada e obrigatoria."));
        }
    }
}
