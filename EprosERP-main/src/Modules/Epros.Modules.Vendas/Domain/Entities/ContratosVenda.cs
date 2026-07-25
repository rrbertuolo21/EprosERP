using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>Tipo/catálogo de contrato (ven_contrato_tipo). Fonte: EF §10.1. GCV-017/GCV-018.</summary>
    public class ContratoTipo : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public bool Ativo { get; private set; } = true;
        public Guid? CriadoPorUsuarioId { get; private set; }

        protected ContratoTipo() { }

        public ContratoTipo(string nome, bool ativo, Guid? criadoPorUsuarioId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Ativo = ativo;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            AddNotifications(new Contract<ContratoTipo>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do tipo de contrato é obrigatório. [Origem: ContratoTipo]"));
        }

        public void Alterar(string nome, bool ativo, string alteradoPor)
        {
            Nome = nome;
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>
    /// Contrato de venda (ven_contrato). Fonte: EF §10.2. Regras GCV-005..GCV-042.
    /// GCV-006: data_fim posterior a data_inicio. GCV-008: origem vazia → Contrato.
    /// GCV-009/GCV-011: número gerado por tenant/prefixo quando vazio.
    /// </summary>
    public class Contrato : EntidadeSaaSBase
    {
        public string? IdentificadorPublico { get; private set; }
        public string Assunto { get; private set; } = string.Empty;
        public string? NumeroContrato { get; private set; }
        public EContratoTipoOrigem TipoOrigem { get; private set; } = EContratoTipoOrigem.Contrato;
        public string? NumeroModelo { get; private set; }
        public Guid ClienteId { get; private set; }
        public Guid UsuarioResponsavelId { get; private set; }
        public Guid TipoId { get; private set; }
        public decimal Valor { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime DataFim { get; private set; }
        public string? Descricao { get; private set; }
        public string? CorpoDocumento { get; private set; }
        public EContratoStatus Status { get; private set; } = EContratoStatus.Rascunho;
        public DateTime? PublicadoEm { get; private set; }
        public DateTime? EnviadoEm { get; private set; }
        public DateTime? PublicacaoAgendadaEm { get; private set; }
        public bool EmpresaAssinou { get; private set; }
        public bool ClienteAssinou { get; private set; }
        public bool Visualizado { get; private set; }
        public Guid? ProjetoId { get; private set; }
        public Guid? LeadId { get; private set; }
        public Guid? PropostaId { get; private set; }
        public Guid? PedidoId { get; private set; }
        public Guid? CategoriaId { get; private set; }
        public bool AutomacaoHabilitada { get; private set; }
        public string? AutomacaoConfigJson { get; private set; }
        public string? AutomacaoResultadoJson { get; private set; }
        public Guid CriadoPorUsuarioId { get; private set; }
        public Guid OwnerUsuarioId { get; private set; }
        public long? SequenciaExibicao { get; private set; }

        protected Contrato() { }

        public Contrato(
            string assunto,
            string? numeroContrato,
            EContratoTipoOrigem? tipoOrigem,
            string? numeroModelo,
            Guid clienteId,
            Guid usuarioResponsavelId,
            Guid tipoId,
            decimal valor,
            DateTime dataInicio,
            DateTime dataFim,
            string? descricao,
            string? corpoDocumento,
            Guid? projetoId,
            Guid? leadId,
            Guid? propostaId,
            Guid? pedidoId,
            Guid? categoriaId,
            bool automacaoHabilitada,
            string? automacaoConfigJson,
            Guid criadoPorUsuarioId,
            Guid ownerUsuarioId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Assunto = assunto;
            NumeroContrato = numeroContrato;
            TipoOrigem = tipoOrigem ?? EContratoTipoOrigem.Contrato; // GCV-008
            NumeroModelo = numeroModelo;
            ClienteId = clienteId;
            UsuarioResponsavelId = usuarioResponsavelId;
            TipoId = tipoId;
            Valor = valor;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Descricao = descricao;
            CorpoDocumento = corpoDocumento;
            ProjetoId = projetoId;
            LeadId = leadId;
            PropostaId = propostaId;
            PedidoId = pedidoId;
            CategoriaId = categoriaId;
            AutomacaoHabilitada = automacaoHabilitada;
            AutomacaoConfigJson = automacaoConfigJson;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            OwnerUsuarioId = ownerUsuarioId;
            Status = EContratoStatus.Rascunho;
            IdentificadorPublico = Guid.NewGuid().ToString("N"); // GCV-039: identificador único para visualização pública
            Validar();
        }

        /// <summary>GCV-009/GCV-011: aplica o número gerado por tenant/prefixo/origem quando vazio.</summary>
        public void DefinirNumero(string numeroContrato, string alteradoPor)
        {
            if (!string.IsNullOrWhiteSpace(NumeroContrato)) return;
            NumeroContrato = numeroContrato;
            MarcarAlterado(alteradoPor);
        }

        public void Alterar(string assunto, decimal valor, DateTime dataInicio, DateTime dataFim, string? descricao, string? corpoDocumento, string alteradoPor)
        {
            Assunto = assunto;
            Valor = valor;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Descricao = descricao;
            CorpoDocumento = corpoDocumento;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        /// <summary>GCV-036/GCV-037/GCV-038: publicação coloca em aguardando assinaturas.</summary>
        public void Publicar(DateTime? publicacaoAgendadaEm, bool enviar, string alteradoPor)
        {
            if (publicacaoAgendadaEm.HasValue && publicacaoAgendadaEm.Value < DateTime.UtcNow)
            {
                AddNotification(nameof(PublicacaoAgendadaEm), "A publicação agendada não pode estar no passado. [Origem: Contrato]");
                return;
            }
            if (publicacaoAgendadaEm.HasValue)
            {
                PublicacaoAgendadaEm = publicacaoAgendadaEm;
            }
            else
            {
                Status = EContratoStatus.AguardandoAssinaturas;
                PublicadoEm = DateTime.UtcNow;
                if (enviar) EnviadoEm = DateTime.UtcNow;
            }
            MarcarAlterado(alteradoPor);
        }

        /// <summary>GCV-030/GCV-034: registra assinatura de uma parte; contrato com assinaturas completas evolui para Ativo.</summary>
        public void RegistrarAssinatura(EContratoParteAssinatura parte, string alteradoPor)
        {
            if (parte == EContratoParteAssinatura.Empresa) EmpresaAssinou = true;
            if (parte == EContratoParteAssinatura.Cliente || parte == EContratoParteAssinatura.Convidado) ClienteAssinou = true;
            if (EmpresaAssinou && ClienteAssinou) Status = EContratoStatus.Ativo;
            MarcarAlterado(alteradoPor);
        }

        public void MarcarVisualizado(string alteradoPor)
        {
            Visualizado = true;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>GCV-041: contrato ativo com data final passada deve expirar.</summary>
        public void AvaliarVencimento(string alteradoPor)
        {
            if (Status == EContratoStatus.Ativo && DataFim.Date < DateTime.UtcNow.Date)
            {
                Status = EContratoStatus.Expirado;
                MarcarAlterado(alteradoPor);
            }
            else if (Status == EContratoStatus.Expirado && DataFim.Date >= DateTime.UtcNow.Date)
            {
                // GCV-042: expirado volta a ativo se a data final voltar a ser futura.
                Status = EContratoStatus.Ativo;
                MarcarAlterado(alteradoPor);
            }
        }

        /// <summary>GCV-044: grava IDs gerados pela automação pós-assinatura.</summary>
        public void RegistrarResultadoAutomacao(string automacaoResultadoJson, string alteradoPor)
        {
            AutomacaoResultadoJson = automacaoResultadoJson;
            MarcarAlterado(alteradoPor);
        }

        public void Cancelar(string alteradoPor)
        {
            Status = EContratoStatus.Cancelado;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>GCV-043: automação só executa quando habilitada e assinaturas requeridas completas.</summary>
        public bool PodeExecutarAutomacao() => AutomacaoHabilitada && EmpresaAssinou && ClienteAssinou;

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Contrato>()
                .Requires()
                .IsNotNullOrEmpty(Assunto, nameof(Assunto), "O assunto do contrato é obrigatório. [Origem: Contrato]")
                .AreNotEquals(ClienteId, Guid.Empty, nameof(ClienteId), "O cliente do contrato é obrigatório. [Origem: Contrato]")
                .AreNotEquals(UsuarioResponsavelId, Guid.Empty, nameof(UsuarioResponsavelId), "O usuário responsável é obrigatório. [Origem: Contrato]")
                .AreNotEquals(TipoId, Guid.Empty, nameof(TipoId), "O tipo de contrato é obrigatório. [Origem: Contrato]")
                // GCV-006: data final posterior à inicial.
                .IsGreaterThan(DataFim, DataInicio, nameof(DataFim), "A data final deve ser posterior à data inicial. [Origem: Contrato]"));
        }
    }

    /// <summary>Modelo de contrato (ven_contrato_modelo). Fonte: EF §10.3. GCV-045/GCV-046.</summary>
    public class ContratoModelo : EntidadeSaaSBase
    {
        public string Titulo { get; private set; } = string.Empty;
        public string Corpo { get; private set; } = string.Empty;
        public string? CorCabecalho { get; private set; }
        public bool Sistema { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected ContratoModelo() { }

        public ContratoModelo(string titulo, string corpo, string? corCabecalho, bool sistema, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Titulo = titulo;
            Corpo = corpo;
            CorCabecalho = corCabecalho ?? "#7493a9"; // default observado
            Sistema = sistema;
            Ativo = true;
            AddNotifications(new Contract<ContratoModelo>()
                .Requires()
                .IsNotNullOrEmpty(titulo, nameof(Titulo), "O título do modelo é obrigatório. [Origem: ContratoModelo]")
                .IsNotNullOrEmpty(corpo, nameof(Corpo), "O corpo do modelo é obrigatório. [Origem: ContratoModelo]"));
        }
    }

    /// <summary>Anexo de contrato (ven_contrato_anexo). Fonte: EF §10.4. GCV-020.</summary>
    public class ContratoAnexo : EntidadeSaaSBase
    {
        public Guid ContratoId { get; private set; }
        public string NomeArquivo { get; private set; } = string.Empty;
        public string ReferenciaArquivo { get; private set; } = string.Empty;
        public Guid UsuarioUploadId { get; private set; }
        public Guid? CriadoPorUsuarioId { get; private set; }

        protected ContratoAnexo() { }

        public ContratoAnexo(Guid contratoId, string nomeArquivo, string referenciaArquivo, Guid usuarioUploadId, Guid? criadoPorUsuarioId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ContratoId = contratoId;
            NomeArquivo = nomeArquivo;
            ReferenciaArquivo = referenciaArquivo;
            UsuarioUploadId = usuarioUploadId;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            AddNotifications(new Contract<ContratoAnexo>()
                .Requires()
                .AreNotEquals(contratoId, Guid.Empty, nameof(ContratoId), "O contrato é obrigatório. [Origem: ContratoAnexo]")
                .IsNotNullOrEmpty(nomeArquivo, nameof(NomeArquivo), "O nome do arquivo é obrigatório. [Origem: ContratoAnexo]")
                .IsNotNullOrEmpty(referenciaArquivo, nameof(ReferenciaArquivo), "A referência do arquivo é obrigatória. [Origem: ContratoAnexo]"));
        }
    }

    /// <summary>Comentário de contrato (ven_contrato_comentario). Fonte: EF §10.5. GCV-022/GCV-024.</summary>
    public class ContratoComentario : EntidadeSaaSBase
    {
        public Guid ContratoId { get; private set; }
        public string Comentario { get; private set; } = string.Empty;
        public Guid UsuarioId { get; private set; }
        public bool Editado { get; private set; }

        protected ContratoComentario() { }

        public ContratoComentario(Guid contratoId, string comentario, Guid usuarioId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ContratoId = contratoId;
            Comentario = comentario;
            UsuarioId = usuarioId;
            Editado = false;
            AddNotifications(new Contract<ContratoComentario>()
                .Requires()
                .AreNotEquals(contratoId, Guid.Empty, nameof(ContratoId), "O contrato é obrigatório. [Origem: ContratoComentario]")
                .IsNotNullOrEmpty(comentario, nameof(Comentario), "O comentário é obrigatório. [Origem: ContratoComentario]")
                .IsLowerOrEqualsThan(comentario?.Length ?? 0, 1000, nameof(Comentario), "O comentário deve ter no máximo 1000 caracteres. [Origem: ContratoComentario]"));
        }

        public void Editar(string comentario, string alteradoPor)
        {
            Comentario = comentario;
            Editado = true; // GCV-024
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Nota de contrato (ven_contrato_nota). Fonte: EF §10.6. GCV-023/GCV-024.</summary>
    public class ContratoNota : EntidadeSaaSBase
    {
        public Guid ContratoId { get; private set; }
        public string Nota { get; private set; } = string.Empty;
        public Guid UsuarioId { get; private set; }
        public bool Editado { get; private set; }

        protected ContratoNota() { }

        public ContratoNota(Guid contratoId, string nota, Guid usuarioId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ContratoId = contratoId;
            Nota = nota;
            UsuarioId = usuarioId;
            Editado = false;
            AddNotifications(new Contract<ContratoNota>()
                .Requires()
                .AreNotEquals(contratoId, Guid.Empty, nameof(ContratoId), "O contrato é obrigatório. [Origem: ContratoNota]")
                .IsNotNullOrEmpty(nota, nameof(Nota), "A nota é obrigatória. [Origem: ContratoNota]")
                .IsLowerOrEqualsThan(nota?.Length ?? 0, 1000, nameof(Nota), "A nota deve ter no máximo 1000 caracteres. [Origem: ContratoNota]"));
        }

        public void Editar(string nota, string alteradoPor)
        {
            Nota = nota;
            Editado = true;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Renovação de contrato (ven_contrato_renovacao). Fonte: EF §10.7. GCV-026..GCV-029.</summary>
    public class ContratoRenovacao : EntidadeSaaSBase
    {
        public Guid ContratoId { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime DataFim { get; private set; }
        public decimal? Valor { get; private set; }
        public string? Notas { get; private set; }
        public EContratoRenovacaoStatus Status { get; private set; } = EContratoRenovacaoStatus.Rascunho;
        public Guid CriadoPorUsuarioId { get; private set; }

        protected ContratoRenovacao() { }

        public ContratoRenovacao(Guid contratoId, DateTime dataInicio, DateTime dataFim, decimal? valor, string? notas, EContratoRenovacaoStatus status, Guid criadoPorUsuarioId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ContratoId = contratoId;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Valor = valor;
            Notas = notas;
            Status = status;
            CriadoPorUsuarioId = criadoPorUsuarioId;

            var contract = new Contract<ContratoRenovacao>()
                .Requires()
                .AreNotEquals(contratoId, Guid.Empty, nameof(ContratoId), "O contrato é obrigatório. [Origem: ContratoRenovacao]")
                // GCV-026: data inicial >= hoje.
                .IsGreaterOrEqualsThan(dataInicio.Date, DateTime.UtcNow.Date, nameof(DataInicio), "A data inicial deve ser igual ou posterior a hoje. [Origem: ContratoRenovacao]")
                // GCV-027: data final > data inicial.
                .IsGreaterThan(dataFim, dataInicio, nameof(DataFim), "A data final deve ser posterior à data inicial. [Origem: ContratoRenovacao]");
            // GCV-028: valor >= 0 quando informado.
            if (valor.HasValue)
                contract.IsGreaterOrEqualsThan(valor.Value, 0m, nameof(Valor), "O valor da renovação deve ser maior ou igual a zero. [Origem: ContratoRenovacao]");
            AddNotifications(contract);
        }
    }

    /// <summary>Assinatura de contrato (ven_contrato_assinatura). Fonte: EF §10.8. GCV-030..GCV-033.</summary>
    public class ContratoAssinatura : EntidadeSaaSBase
    {
        public Guid ContratoId { get; private set; }
        public Guid? UsuarioId { get; private set; }
        public EContratoParteAssinatura Parte { get; private set; }
        public EContratoTipoAssinatura TipoAssinatura { get; private set; }
        public string DadosAssinatura { get; private set; } = string.Empty;
        public DateTime AssinadoEm { get; private set; }
        public Guid? CriadoPorUsuarioId { get; private set; }

        protected ContratoAssinatura() { }

        public ContratoAssinatura(Guid contratoId, Guid? usuarioId, EContratoParteAssinatura parte, EContratoTipoAssinatura tipoAssinatura, string dadosAssinatura, Guid? criadoPorUsuarioId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ContratoId = contratoId;
            UsuarioId = usuarioId;
            Parte = parte;
            TipoAssinatura = tipoAssinatura;
            DadosAssinatura = dadosAssinatura;
            AssinadoEm = DateTime.UtcNow;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            AddNotifications(new Contract<ContratoAssinatura>()
                .Requires()
                .AreNotEquals(contratoId, Guid.Empty, nameof(ContratoId), "O contrato é obrigatório. [Origem: ContratoAssinatura]")
                // GCV-032: conteúdo mínimo de 10 caracteres.
                .IsGreaterOrEqualsThan(dadosAssinatura?.Length ?? 0, 10, nameof(DadosAssinatura), "A assinatura deve possuir conteúdo mínimo de 10 caracteres. [Origem: ContratoAssinatura]"));
        }
    }

    /// <summary>Configuração de contratos por tenant (ven_contrato_configuracao). Fonte: EF §10.9. GCV-010.</summary>
    public class ContratoConfiguracao : EntidadeSaaSBase
    {
        public string PrefixoContrato { get; private set; } = string.Empty;
        public string? AutomacaoPadraoJson { get; private set; }
        public string? UsuariosPadraoJson { get; private set; }

        protected ContratoConfiguracao() { }

        public ContratoConfiguracao(string prefixoContrato, string? automacaoPadraoJson, string? usuariosPadraoJson, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PrefixoContrato = prefixoContrato;
            AutomacaoPadraoJson = automacaoPadraoJson;
            UsuariosPadraoJson = usuariosPadraoJson;
            AddNotifications(new Contract<ContratoConfiguracao>()
                .Requires()
                // GCV-010: prefixo obrigatório, máx 10 caracteres.
                .IsNotNullOrEmpty(prefixoContrato, nameof(PrefixoContrato), "O prefixo de contrato é obrigatório. [Origem: ContratoConfiguracao]")
                .IsLowerOrEqualsThan(prefixoContrato?.Length ?? 0, 10, nameof(PrefixoContrato), "O prefixo deve ter no máximo 10 caracteres. [Origem: ContratoConfiguracao]"));
        }

        public void Alterar(string prefixoContrato, string? automacaoPadraoJson, string? usuariosPadraoJson, string alteradoPor)
        {
            PrefixoContrato = prefixoContrato;
            AutomacaoPadraoJson = automacaoPadraoJson;
            UsuariosPadraoJson = usuariosPadraoJson;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Histórico/auditoria de contrato (ven_contrato_historico). Fonte: EF §10.10. GCV-016.</summary>
    public class ContratoHistorico : EntidadeSaaSBase
    {
        public Guid ContratoId { get; private set; }
        public EContratoEvento Evento { get; private set; }
        public Guid? UsuarioId { get; private set; }
        public string? DadosAnterioresJson { get; private set; }
        public string? DadosNovosJson { get; private set; }
        public DateTime DataEvento { get; private set; }

        protected ContratoHistorico() { }

        public ContratoHistorico(Guid contratoId, EContratoEvento evento, Guid? usuarioId, string? dadosAnterioresJson, string? dadosNovosJson, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ContratoId = contratoId;
            Evento = evento;
            UsuarioId = usuarioId;
            DadosAnterioresJson = dadosAnterioresJson;
            DadosNovosJson = dadosNovosJson;
            DataEvento = DateTime.UtcNow;
            AddNotifications(new Contract<ContratoHistorico>()
                .Requires()
                .AreNotEquals(contratoId, Guid.Empty, nameof(ContratoId), "O contrato é obrigatório. [Origem: ContratoHistorico]"));
        }
    }
}
