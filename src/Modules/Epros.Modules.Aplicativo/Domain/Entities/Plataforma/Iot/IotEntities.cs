using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Iot
{
    /// <summary>
    /// PLT · IoT — dispositivo de campo que agrega sensores. Pode ser vinculado a um ativo de negócio
    /// (máquina/veículo) para alimentar a Manutenção preditiva via evento (condição, não ordem).
    /// </summary>
    public class DispositivoIot : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public string? Tipo { get; private set; }
        public string? Protocolo { get; private set; } // mqtt, http, coap
        public string? AtivoVinculadoTipo { get; private set; }
        public string? AtivoVinculadoId { get; private set; }
        public bool Ativo { get; private set; } = true;
        public DateTime? UltimaLeituraEm { get; private set; }

        protected DispositivoIot() { }

        public DispositivoIot(string codigo, string nome, string? tipo, string? protocolo,
            string? ativoVinculadoTipo, string? ativoVinculadoId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<DispositivoIot>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O código do dispositivo é obrigatório.")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do dispositivo é obrigatório."));

            Codigo = codigo;
            Nome = nome;
            Tipo = tipo;
            Protocolo = protocolo;
            AtivoVinculadoTipo = ativoVinculadoTipo;
            AtivoVinculadoId = ativoVinculadoId;
            Ativo = true;
        }

        public void VincularAtivo(string ativoTipo, string ativoId, string usuario)
        {
            AtivoVinculadoTipo = ativoTipo;
            AtivoVinculadoId = ativoId;
            MarcarAlterado(usuario);
        }

        public void RegistrarLeitura(DateTime medidoEm, string usuario)
        {
            UltimaLeituraEm = medidoEm;
            MarcarAlterado(usuario);
        }

        public void Ativar(string usuario) { Ativo = true; MarcarAlterado(usuario); }
        public void Desativar(string usuario) { Ativo = false; MarcarAlterado(usuario); }
    }

    /// <summary>Sensor de um dispositivo: grandeza medida + faixa operacional + retenção de série.</summary>
    public class SensorIot : EntidadeSaaSBase
    {
        public Guid DispositivoId { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Grandeza { get; private set; } = string.Empty; // temperatura, vibracao, pressao, corrente...
        public string Unidade { get; private set; } = string.Empty;
        public decimal? LimiteMin { get; private set; }
        public decimal? LimiteMax { get; private set; }
        public int RetencaoDias { get; private set; } = 90;

        protected SensorIot() { }

        public SensorIot(Guid dispositivoId, string codigo, string grandeza, string unidade,
            decimal? limiteMin, decimal? limiteMax, int retencaoDias, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<SensorIot>()
                .Requires()
                .IsTrue(dispositivoId != Guid.Empty, nameof(DispositivoId), "O dispositivo é obrigatório.")
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O código do sensor é obrigatório.")
                .IsNotNullOrEmpty(grandeza, nameof(Grandeza), "A grandeza do sensor é obrigatória.")
                .IsNotNullOrEmpty(unidade, nameof(Unidade), "A unidade do sensor é obrigatória."));

            if (limiteMin.HasValue && limiteMax.HasValue && limiteMin > limiteMax)
                AddNotification(nameof(LimiteMin), "O limite mínimo não pode ser maior que o máximo.");

            DispositivoId = dispositivoId;
            Codigo = codigo;
            Grandeza = grandeza;
            Unidade = unidade;
            LimiteMin = limiteMin;
            LimiteMax = limiteMax;
            RetencaoDias = retencaoDias <= 0 ? 90 : retencaoDias;
        }

        /// <summary>True quando o valor está fora da faixa operacional configurada.</summary>
        public bool EstaForaFaixa(decimal valor)
            => (LimiteMin.HasValue && valor < LimiteMin.Value) || (LimiteMax.HasValue && valor > LimiteMax.Value);
    }

    /// <summary>Leitura de série temporal de um sensor (ponto). Retenção governada por <see cref="SensorIot.RetencaoDias"/>.</summary>
    public class LeituraSensor : EntidadeSaaSBase
    {
        public Guid SensorId { get; private set; }
        public decimal Valor { get; private set; }
        public DateTime MedidoEm { get; private set; }
        public bool ForaFaixa { get; private set; }

        protected LeituraSensor() { }

        public LeituraSensor(Guid sensorId, decimal valor, DateTime medidoEm, bool foraFaixa, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<LeituraSensor>()
                .Requires()
                .IsTrue(sensorId != Guid.Empty, nameof(SensorId), "O sensor é obrigatório."));

            SensorId = sensorId;
            Valor = valor;
            MedidoEm = medidoEm;
            ForaFaixa = foraFaixa;
        }
    }
}
