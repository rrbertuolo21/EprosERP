using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Transporte da compra (transportadora, veículo, volumes, reboques). Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraTransporte, expandido conforme a EF Transporte e Frete TMS
    /// (EST-TMS): estados funcionais §8 (Situacao) e regra de consistência TMS-001 (§7).
    /// </summary>
    public class CompraTransporte : EntidadeSaaSBase
    {
        public Guid CompraId { get; private set; }

        /// <summary>Estado funcional do transporte da compra — EF TMS §8/§9.1.</summary>
        public ETmsStatusTransporte Situacao { get; private set; } = ETmsStatusTransporte.Rascunho;

        // Navegação intra-módulo
        public CompraTransporteTransportadora? Transportadora { get; private set; }
        public CompraTransporteVeiculo? Veiculo { get; private set; }
        public ICollection<CompraTransporteVolume> Volumes { get; private set; } = new List<CompraTransporteVolume>();
        public ICollection<CompraTransporteReboque> Reboques { get; private set; } = new List<CompraTransporteReboque>();
        public Compra? Compra { get; private set; }

        protected CompraTransporte() { } // EF Core

        public CompraTransporte(Guid compraId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraId = compraId;
        }

        public void Validar()
        {
            if (Veiculo == null && Transportadora == null && (Volumes == null || !Volumes.Any()))
                AddNotification("CompraTransporte", "Em CompraTransporte, pelo menos Transportadora, Veiculo ou Volumes, precisam ser preenchidos");

            if (Transportadora != null && Transportadora.Notifications.Any())
                AddNotifications(Transportadora.Notifications);

            if (Veiculo != null && Veiculo.Notifications.Any())
                AddNotifications(Veiculo.Notifications);

            if (Volumes != null)
                foreach (var v in Volumes)
                    if (v.Notifications.Count > 0) AddNotifications(v.Notifications);
        }

        public void IncluirTransportadora(Guid? pessoaId, string? cnpj, string? cpf, string? razaoSocialOuNome, string? inscricaoEstadual, string? enderecoCompleto, string? nomeMunicipio, EEstado uf, string usuario)
        {
            Transportadora = new CompraTransporteTransportadora(Id, pessoaId, cnpj, cpf, razaoSocialOuNome, inscricaoEstadual, enderecoCompleto, nomeMunicipio, uf, TenantId, usuario);
            Validar();
        }

        public void AlterarTransportadora(Guid? pessoaId, string? cnpj, string? cpf, string? razaoSocialOuNome, string? inscricaoEstadual, string? enderecoCompleto, string? nomeMunicipio, EEstado uf, string usuario)
        {
            Transportadora?.Alterar(pessoaId, cnpj, cpf, razaoSocialOuNome, inscricaoEstadual, enderecoCompleto, nomeMunicipio, uf, usuario);
            Validar();
        }

        public void ExcluirTransportadora(string usuario)
        {
            Transportadora?.Deletar(usuario);
            Transportadora = null;
            Validar();
        }

        public void IncluirVeiculo(Guid? veiculoId, string placa, EEstado uf, string? rntrc, string usuario)
        {
            Veiculo = new CompraTransporteVeiculo(Id, veiculoId, placa, uf, rntrc, TenantId, usuario);
            Validar();
        }

        public void AlterarVeiculo(Guid? veiculoId, string placa, EEstado uf, string? rntrc, string usuario)
        {
            Veiculo?.Alterar(veiculoId, placa, uf, rntrc, usuario);
            Validar();
        }

        public void ExcluirVeiculo(string usuario)
        {
            Veiculo?.Deletar(usuario);
            Veiculo = null;
            Validar();
        }

        public void IncluirVolume(int quantidadeVolumes, string? especie, string? numeroVolumes, decimal pesoLiquido, decimal pesoBruto, string? marca, string usuario)
        {
            var volume = new CompraTransporteVolume(Id, quantidadeVolumes, especie, numeroVolumes, pesoLiquido, pesoBruto, marca, TenantId, usuario);
            Volumes.Add(volume);
            Validar();
        }

        public void AlterarVolume(Guid volumeId, int quantidadeVolumes, string? especie, string? numeroVolumes, decimal pesoLiquido, decimal pesoBruto, string? marca, string usuario)
        {
            var localizado = Volumes.FirstOrDefault(x => x.Id == volumeId);
            localizado?.Alterar(quantidadeVolumes, especie, numeroVolumes, pesoLiquido, pesoBruto, marca, usuario);
            Validar();
        }

        public void ExcluirVolume(Guid volumeId, string usuario)
        {
            var localizado = Volumes.FirstOrDefault(x => x.Id == volumeId);
            localizado?.Deletar(usuario);
        }

        public void IncluirReboque(Guid? veiculoId, string placa, EEstado uf, string? rntrc, string usuario)
        {
            var reboque = new CompraTransporteReboque(Id, veiculoId, placa, uf, rntrc, TenantId, usuario);
            Reboques.Add(reboque);
            Validar();
        }

        public void AlterarReboque(Guid reboqueId, Guid? veiculoId, string placa, EEstado uf, string? rntrc, string usuario)
        {
            var localizado = Reboques.FirstOrDefault(x => x.Id == reboqueId);
            localizado?.Alterar(veiculoId, placa, uf, rntrc, usuario);
            Validar();
        }

        public void ExcluirReboque(Guid reboqueId, string usuario)
        {
            var localizado = Reboques.FirstOrDefault(x => x.Id == reboqueId);
            localizado?.Deletar(usuario);
        }

        /// <summary>Confirma os dados de transporte para uso fiscal/operacional — EF TMS §8.</summary>
        public void Confirmar(string usuario)
        {
            Situacao = ETmsStatusTransporte.Confirmado;
            MarcarAlterado(usuario);
        }

        /// <summary>Marca o transporte como faturado (documento fiscal emitido/registrado) — EF TMS §8.</summary>
        public void Faturar(string usuario)
        {
            Situacao = ETmsStatusTransporte.Faturado;
            MarcarAlterado(usuario);
        }

        /// <summary>Cancela os dados de transporte por reflexo do cancelamento do documento principal — EF TMS §8.</summary>
        public void Cancelar(string usuario)
        {
            Situacao = ETmsStatusTransporte.Cancelado;
            MarcarAlterado(usuario);
        }

        /// <summary>Reverte os dados de transporte por estorno do processo principal — EF TMS §8.</summary>
        public void Estornar(string usuario)
        {
            Situacao = ETmsStatusTransporte.Estornado;
            MarcarAlterado(usuario);
        }
    }
}
