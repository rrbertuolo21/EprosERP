using System;
using Epros.Shared.Domain.Entities;
using Epros.Modules.GestaoClientes.Domain.ValueObjects;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class PessoaVeiculo : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }
        public Guid PaisId { get; private set; }
        public ETipoVeiculo TipoVeiculo { get; private set; }
        public string Uf { get; private set; } = string.Empty;
        public string Placa { get; private set; } = string.Empty;
        public string? Rntrc { get; private set; }

        protected PessoaVeiculo() { } // EF Core

        public PessoaVeiculo(
            Guid pessoaId,
            Guid paisId,
            ETipoVeiculo tipoVeiculo,
            string uf,
            string placa,
            string? rntrc,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PessoaVeiculo>()
                .Requires()
                .AreNotEquals(paisId, Guid.Empty, nameof(PaisId), "O campo Pais é obrigatório [Origem: Veiculo]")
                .IsTrue(Enum.IsDefined(typeof(ETipoVeiculo), tipoVeiculo), nameof(TipoVeiculo), "TipoVeiculo não consta na lista [Origem: Veiculo]")
                .IsTrue(Enum.TryParse<EEstado>(uf, out _), nameof(Uf), "Uf não consta na lista [Origem: Veiculo]")
                .HasMaxLen(placa ?? string.Empty, 8, nameof(Placa), "O campo Placa deve ter no máximo 8 caracteres [Origem: Veiculo]")
                .HasMaxLen(rntrc ?? string.Empty, 14, nameof(Rntrc), "O campo Rntrc deve ter no máximo 14 caracteres [Origem: Veiculo]")
            );

            if (!string.IsNullOrEmpty(placa))
            {
                var placaVo = new ValueObjects.Placa(placa);
                if (!placaVo.IsValid)
                {
                    AddNotifications(placaVo.Notifications);
                }
                Placa = placaVo.Valor;
            }

            PessoaId = pessoaId;
            PaisId = paisId;
            TipoVeiculo = tipoVeiculo;
            Uf = uf;
            Rntrc = rntrc;
        }
    }
}
