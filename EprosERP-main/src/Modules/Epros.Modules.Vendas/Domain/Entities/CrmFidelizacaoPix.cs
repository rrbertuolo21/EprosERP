using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>Cliente fidelizado (crm_cliente_fidelizado). Fonte: EF §10.21. CRM-066.</summary>
    public class CrmClienteFidelizado : EntidadeSaaSBase
    {
        public Guid? ClienteId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string? Email { get; private set; }
        public string? Celular { get; private set; }
        public decimal? TotalPontos { get; private set; }
        public DateTime? DataAniversario { get; private set; }
        public DateTime? UltimoPonto { get; private set; }
        public DateTime DataCadastro { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected CrmClienteFidelizado() { }

        public CrmClienteFidelizado(Guid? clienteId, string nome, string? email, string? celular, DateTime? dataAniversario, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ClienteId = clienteId;
            Nome = nome;
            Email = email;
            Celular = celular;
            DataAniversario = dataAniversario;
            TotalPontos = 0;
            DataCadastro = DateTime.UtcNow;
            Ativo = true;
            AddNotifications(new Contract<CrmClienteFidelizado>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O cliente fidelizado deve possuir nome. [Origem: CrmClienteFidelizado]"));
        }

        /// <summary>CRM-067: acumula pontuação, atualizando total e data do último ponto.</summary>
        public void AcumularPontos(decimal pontos, DateTime data, string alteradoPor)
        {
            TotalPontos = (TotalPontos ?? 0) + pontos;
            UltimoPonto = data;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Pontuação de fidelização (crm_pontuacao_fidelizacao). Fonte: EF §10.22. CRM-067.</summary>
    public class CrmPontuacaoFidelizacao : EntidadeSaaSBase
    {
        public Guid ClienteFidelizadoId { get; private set; }
        public DateTime? Data { get; private set; }
        public decimal? Pontos { get; private set; }
        public string? Observacao { get; private set; }

        protected CrmPontuacaoFidelizacao() { }

        public CrmPontuacaoFidelizacao(Guid clienteFidelizadoId, DateTime? data, decimal? pontos, string? observacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ClienteFidelizadoId = clienteFidelizadoId;
            Data = data ?? DateTime.UtcNow;
            Pontos = pontos;
            Observacao = observacao;
            AddNotifications(new Contract<CrmPontuacaoFidelizacao>()
                .Requires()
                .AreNotEquals(clienteFidelizadoId, Guid.Empty, nameof(ClienteFidelizadoId), "O cliente fidelizado é obrigatório. [Origem: CrmPontuacaoFidelizacao]"));
        }
    }

    /// <summary>
    /// Configuração de Pix relacional (crm_configuracao_pix_relacional). Fonte: EF §10.23.
    /// CRM-071: token/credenciais são segredo e nunca devem ser exibidos ao usuário final.
    /// </summary>
    public class CrmConfiguracaoPixRelacional : EntidadeSaaSBase
    {
        public string? LinkApiAppVendas { get; private set; }
        public string? TokenPix { get; private set; }
        public string? CnpjEmpresa { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected CrmConfiguracaoPixRelacional() { }

        public CrmConfiguracaoPixRelacional(string? linkApiAppVendas, string? tokenPix, string? cnpjEmpresa, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            LinkApiAppVendas = linkApiAppVendas;
            TokenPix = tokenPix;
            CnpjEmpresa = cnpjEmpresa;
            Ativo = true;
        }

        public void Alterar(string? linkApiAppVendas, string? tokenPix, string? cnpjEmpresa, bool ativo, string alteradoPor)
        {
            LinkApiAppVendas = linkApiAppVendas;
            if (!string.IsNullOrWhiteSpace(tokenPix)) TokenPix = tokenPix;
            CnpjEmpresa = cnpjEmpresa;
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Pagamento Pix relacional (crm_pagamento_pix_relacional). Fonte: EF §10.24. CRM-069/CRM-070.</summary>
    public class CrmPagamentoPixRelacional : EntidadeSaaSBase
    {
        public string? EntidadeOrigemTipo { get; private set; }
        public Guid? EntidadeOrigemId { get; private set; }
        public decimal Valor { get; private set; }
        public string? ProvedorPagamentoId { get; private set; }
        public string? QrCode { get; private set; }
        public string? QrCodeImagem { get; private set; }
        public ECrmStatusPagamentoPix Status { get; private set; } = ECrmStatusPagamentoPix.Pendente;
        public DateTime? DataAprovacao { get; private set; }

        protected CrmPagamentoPixRelacional() { }

        public CrmPagamentoPixRelacional(string? entidadeOrigemTipo, Guid? entidadeOrigemId, decimal valor, string? qrCode, string? qrCodeImagem, string? provedorPagamentoId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            EntidadeOrigemTipo = entidadeOrigemTipo;
            EntidadeOrigemId = entidadeOrigemId;
            Valor = valor;
            QrCode = qrCode;
            QrCodeImagem = qrCodeImagem;
            ProvedorPagamentoId = provedorPagamentoId;
            Status = ECrmStatusPagamentoPix.Pendente;
            AddNotifications(new Contract<CrmPagamentoPixRelacional>()
                .Requires()
                .IsGreaterThan(valor, 0m, nameof(Valor), "O valor do pagamento Pix deve ser maior que zero. [Origem: CrmPagamentoPixRelacional]"));
        }

        /// <summary>CRM-070: atualiza status consultado no provedor (aprovado/rejeitado/outro).</summary>
        public void AtualizarStatus(ECrmStatusPagamentoPix status, string? provedorPagamentoId, string alteradoPor)
        {
            Status = status;
            if (provedorPagamentoId != null) ProvedorPagamentoId = provedorPagamentoId;
            if (status == ECrmStatusPagamentoPix.Aprovado) DataAprovacao = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Histórico imutável do CRM (crm_historico). Fonte: EF §10.25. CRM-072.</summary>
    public class CrmHistorico : EntidadeSaaSBase
    {
        public string EntidadeTipo { get; private set; } = string.Empty;
        public Guid EntidadeId { get; private set; }
        public string Evento { get; private set; } = string.Empty;
        public Guid? UsuarioId { get; private set; }
        public string? DadosAnterioresJson { get; private set; }
        public string? DadosNovosJson { get; private set; }
        public string? Observacao { get; private set; }
        public DateTime DataEvento { get; private set; }

        protected CrmHistorico() { }

        public CrmHistorico(string entidadeTipo, Guid entidadeId, string evento, Guid? usuarioId, string? dadosAnterioresJson, string? dadosNovosJson, string? observacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            EntidadeTipo = entidadeTipo;
            EntidadeId = entidadeId;
            Evento = evento;
            UsuarioId = usuarioId;
            DadosAnterioresJson = dadosAnterioresJson;
            DadosNovosJson = dadosNovosJson;
            Observacao = observacao;
            DataEvento = DateTime.UtcNow;
            AddNotifications(new Contract<CrmHistorico>()
                .Requires()
                .IsNotNullOrEmpty(entidadeTipo, nameof(EntidadeTipo), "O tipo da entidade é obrigatório. [Origem: CrmHistorico]")
                .AreNotEquals(entidadeId, Guid.Empty, nameof(EntidadeId), "O identificador da entidade é obrigatório. [Origem: CrmHistorico]")
                .IsNotNullOrEmpty(evento, nameof(Evento), "O evento do histórico é obrigatório. [Origem: CrmHistorico]"));
        }
    }
}
