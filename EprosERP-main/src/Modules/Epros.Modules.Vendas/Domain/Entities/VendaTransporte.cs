using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaTransporte (agregado de transporte da venda: transportadora,
    /// veículo, volumes e reboques). FK long -> Guid; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaTransporte : EntidadeSaaSBase
    {
        public Guid VendaId { get; private set; }

        // Navegações intra-módulo
        public Venda Venda { get; private set; } = null!;
        public VendaTransporteTransportadora? Transportadora { get; private set; }
        public VendaTransporteVeiculo? Veiculo { get; private set; }

        private readonly List<VendaTransporteVolume> _volumes = new();
        public IReadOnlyCollection<VendaTransporteVolume> Volumes => _volumes.AsReadOnly();

        private readonly List<VendaTransporteReboque> _reboques = new();
        public IReadOnlyCollection<VendaTransporteReboque> Reboques => _reboques.AsReadOnly();

        protected VendaTransporte() { } // EF Core

        public VendaTransporte(Guid vendaId, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            VendaId = vendaId;
        }

        /// <summary>Porte fiel de VendaTransporte.Validar.</summary>
        public void Validar()
        {
            if (Veiculo == null && Transportadora == null && _volumes.Count == 0)
                AddNotification("VendaTransporte", "Em VendaTransporte, pelo menos Transportadora, Veiculo ou Volumes, precisam ser preenchidos");

            if (Transportadora != null && Transportadora.Notifications.Any())
                AddNotifications(Transportadora.Notifications);

            if (Veiculo != null && Veiculo.Notifications.Any())
                AddNotifications(Veiculo.Notifications);

            foreach (var v in _volumes)
                if (v.Notifications.Count > 0) AddNotifications(v.Notifications);
        }

        public void IncluirTransportadora(Guid? pessoaId, string? cnpj, string? cpf, string? razaoSocialOuNome, string? inscricaoEstadual, string? logradouro, string? numero, string? complemento, string? bairro, string? municipio, EEstado uf, string criadoPor)
        {
            Transportadora = new VendaTransporteTransportadora(Id, pessoaId, cnpj, cpf, razaoSocialOuNome, inscricaoEstadual, logradouro, numero, complemento, bairro, municipio, uf, TenantId, criadoPor);
            Validar();
        }

        public void AlterarTransportadora(Guid? pessoaId, string? cnpj, string? cpf, string? razaoSocialOuNome, string? inscricaoEstadual, string? logradouro, string? numero, string? complemento, string? bairro, string? municipio, EEstado uf, string alteradoPor)
        {
            Transportadora?.Alterar(pessoaId, cnpj, cpf, razaoSocialOuNome, inscricaoEstadual, logradouro, numero, complemento, bairro, municipio, uf, alteradoPor);
            Validar();
        }

        public void ExcluirTransportadora(string userId)
        {
            Transportadora?.Deletar(userId);
            Validar();
        }

        public void IncluirVeiculo(Guid? veiculoId, string placa, EEstado uf, string? rntrc, string criadoPor)
        {
            Veiculo = new VendaTransporteVeiculo(Id, veiculoId, placa, uf, rntrc, TenantId, criadoPor);
            Validar();
        }

        public void AlterarVeiculo(Guid? veiculoId, string placa, EEstado uf, string? rntrc, string alteradoPor)
        {
            Veiculo?.Alterar(veiculoId, placa, uf, rntrc, alteradoPor);
            Validar();
        }

        public void ExcluirVeiculo(string userId)
        {
            Veiculo?.Deletar(userId);
            Validar();
        }

        public void IncluirVolume(int quantidadeVolumes, string? especie, string? numeroVolumes, decimal pesoLiquido, decimal pesoBruto, string? marca, string criadoPor)
        {
            _volumes.Add(new VendaTransporteVolume(Id, quantidadeVolumes, especie, numeroVolumes, pesoLiquido, pesoBruto, marca, TenantId, criadoPor));
            Validar();
        }

        public void AlterarVolume(Guid volumeId, int quantidadeVolumes, string? especie, string? numeroVolumes, decimal pesoLiquido, decimal pesoBruto, string? marca, string alteradoPor)
        {
            var localizado = _volumes.FirstOrDefault(x => x.Id == volumeId);
            localizado?.Alterar(quantidadeVolumes, especie, numeroVolumes, pesoLiquido, pesoBruto, marca, alteradoPor);
            Validar();
        }

        public void ExcluirVolume(Guid volumeId, string userId)
        {
            var localizado = _volumes.FirstOrDefault(x => x.Id == volumeId);
            localizado?.Deletar(userId);
        }

        public void IncluirReboque(Guid? veiculoId, string placa, EEstado uf, string? rntrc, string criadoPor)
        {
            _reboques.Add(new VendaTransporteReboque(Id, veiculoId, placa, uf, rntrc, TenantId, criadoPor));
            Validar();
        }

        public void AlterarReboque(Guid reboqueId, Guid? veiculoId, string placa, EEstado uf, string? rntrc, string alteradoPor)
        {
            var localizado = _reboques.FirstOrDefault(x => x.Id == reboqueId);
            localizado?.Alterar(veiculoId, placa, uf, rntrc, alteradoPor);
            Validar();
        }

        public void ExcluirReboque(Guid reboqueId, string userId)
        {
            var localizado = _reboques.FirstOrDefault(x => x.Id == reboqueId);
            localizado?.Deletar(userId);
        }

        /// <summary>Porte fiel de VendaTransporte.Deletar (soft delete em cascata).</summary>
        public void DeletarEmCascata(string userId)
        {
            Deletar(userId);
            ExcluirTransportadora(userId);
            ExcluirVeiculo(userId);
            _volumes.ForEach(x => x.Deletar(userId));
            _reboques.ForEach(x => x.Deletar(userId));
        }

        /// <summary>Porte fiel de VendaTransporte.Duplicar (novo Id/FK, incl. transportadora/veículo/volumes/reboques).</summary>
        public VendaTransporte Duplicar(Guid novaVendaId, string criadoPor)
        {
            var clone = new VendaTransporte(novaVendaId, TenantId, criadoPor);
            if (Transportadora is not null)
                clone.IncluirTransportadora(Transportadora.PessoaId, Transportadora.Cnpj, Transportadora.Cpf, Transportadora.RazaoSocial,
                    Transportadora.InscricaoEstadual, Transportadora.Logradouro, Transportadora.Numero, Transportadora.Complemento,
                    Transportadora.Bairro, Transportadora.Municipio, Transportadora.Uf, criadoPor);
            if (Veiculo is not null)
                clone.IncluirVeiculo(Veiculo.VeiculoId, Veiculo.Placa, Veiculo.Uf, Veiculo.Rntrc, criadoPor);
            foreach (var v in _volumes)
                clone.IncluirVolume(v.QuantidadeVolumes, v.Especie, v.NumeroVolumes, v.PesoLiquido, v.PesoBruto, v.Marca, criadoPor);
            foreach (var r in _reboques)
                clone.IncluirReboque(r.VeiculoId, r.Placa, r.Uf, r.Rntrc, criadoPor);
            return clone;
        }
    }
}
