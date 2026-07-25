using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Usuário externo do portal vinculado a um cliente (ven_portal_usuario_cliente). Fonte: EF §16.1.
    /// §13/§18: todo acesso é filtrado pelo cliente vinculado — nunca consulta sem critério de cliente/tenant.
    /// SEGURANÇA: senha/SSO não é modelada aqui (autenticação fica na plataforma); apenas o vínculo e o estado.
    /// </summary>
    public class PortalUsuarioCliente : EntidadeSaaSBase
    {
        public Guid ClienteId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string? Telefone { get; private set; }
        public EPortalUsuarioStatus Status { get; private set; } = EPortalUsuarioStatus.Ativo;
        public bool AdministradorCliente { get; private set; }
        public DateTime? UltimoAcessoEm { get; private set; }
        public Guid? CriadoPorUsuarioId { get; private set; }

        protected PortalUsuarioCliente() { }

        public PortalUsuarioCliente(
            Guid clienteId,
            string nome,
            string email,
            string? telefone,
            bool administradorCliente,
            Guid? criadoPorUsuarioId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ClienteId = clienteId;
            Nome = nome;
            Email = email;
            Telefone = telefone;
            AdministradorCliente = administradorCliente;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            Status = EPortalUsuarioStatus.Ativo;
            AddNotifications(new Contract<PortalUsuarioCliente>()
                .Requires()
                // §18: tenant, cliente, nome, email e status.
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "O cliente é obrigatório. [Origem: PortalUsuarioCliente]")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do usuário é obrigatório. [Origem: PortalUsuarioCliente]")
                .IsEmail(email ?? string.Empty, nameof(Email), "E-mail inválido. [Origem: PortalUsuarioCliente]"));
        }

        public void Alterar(string nome, string? telefone, bool administradorCliente, string alteradoPor)
        {
            Nome = nome;
            Telefone = telefone;
            AdministradorCliente = administradorCliente;
            MarcarAlterado(alteradoPor);
        }

        public void Bloquear(string alteradoPor)
        {
            Status = EPortalUsuarioStatus.Bloqueado;
            MarcarAlterado(alteradoPor);
        }

        public void Inativar(string alteradoPor)
        {
            Status = EPortalUsuarioStatus.Inativo;
            MarcarAlterado(alteradoPor);
        }

        public void Ativar(string alteradoPor)
        {
            Status = EPortalUsuarioStatus.Ativo;
            MarcarAlterado(alteradoPor);
        }

        public void RegistrarAcesso(string alteradoPor)
        {
            UltimoAcessoEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>§18: só usuário ativo pode acessar recurso.</summary>
        public bool PodeAcessar() => Status == EPortalUsuarioStatus.Ativo;
    }

    /// <summary>Formulário web publicável (ven_portal_formulario). Fonte: EF §16.2.</summary>
    public class PortalFormulario : EntidadeSaaSBase
    {
        public string? Codigo { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public EPortalFormularioStatus Status { get; private set; } = EPortalFormularioStatus.Rascunho;
        public bool Publico { get; private set; }
        public string? ConfiguracaoCampos { get; private set; }

        protected PortalFormulario() { }

        public PortalFormulario(string? codigo, string nome, string? descricao, bool publico, string? configuracaoCampos, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Nome = nome;
            Descricao = descricao;
            Publico = publico;
            ConfiguracaoCampos = configuracaoCampos;
            Status = EPortalFormularioStatus.Rascunho;
            AddNotifications(new Contract<PortalFormulario>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do formulário é obrigatório. [Origem: PortalFormulario]"));
        }

        public void Alterar(string nome, string? descricao, bool publico, string? configuracaoCampos, string alteradoPor)
        {
            Nome = nome;
            Descricao = descricao;
            Publico = publico;
            ConfiguracaoCampos = configuracaoCampos;
            MarcarAlterado(alteradoPor);
        }

        public void Publicar(string alteradoPor)
        {
            Status = EPortalFormularioStatus.Publicado;
            MarcarAlterado(alteradoPor);
        }

        public void Inativar(string alteradoPor)
        {
            Status = EPortalFormularioStatus.Inativo;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Responsável interno de formulário (ven_portal_formulario_responsavel). Fonte: EF §16.3.</summary>
    public class PortalFormularioResponsavel : EntidadeSaaSBase
    {
        public Guid FormularioId { get; private set; }
        public Guid UsuarioInternoId { get; private set; }
        public string? Papel { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected PortalFormularioResponsavel() { }

        public PortalFormularioResponsavel(Guid formularioId, Guid usuarioInternoId, string? papel, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            FormularioId = formularioId;
            UsuarioInternoId = usuarioInternoId;
            Papel = papel;
            Ativo = true;
            AddNotifications(new Contract<PortalFormularioResponsavel>()
                .Requires()
                .AreNotEquals(formularioId, Guid.Empty, nameof(FormularioId), "O formulário é obrigatório. [Origem: PortalFormularioResponsavel]")
                .AreNotEquals(usuarioInternoId, Guid.Empty, nameof(UsuarioInternoId), "O usuário responsável é obrigatório. [Origem: PortalFormularioResponsavel]"));
        }
    }

    /// <summary>Solicitação de atendimento do cliente (ven_portal_solicitacao). Fonte: EF §16.4.</summary>
    public class PortalSolicitacao : EntidadeSaaSBase
    {
        public Guid? ClienteId { get; private set; }
        public Guid? UsuarioClienteId { get; private set; }
        public Guid? FormularioId { get; private set; }
        public Guid? ResponsavelId { get; private set; }
        public string? Assunto { get; private set; }
        public string? Descricao { get; private set; }
        public string? DadosFormulario { get; private set; }
        public EPortalSolicitacaoStatus Status { get; private set; } = EPortalSolicitacaoStatus.Aberta;
        public DateTime AbertaEm { get; private set; }
        public DateTime? RespondidaEm { get; private set; }
        public DateTime? EncerradaEm { get; private set; }

        protected PortalSolicitacao() { }

        public PortalSolicitacao(
            Guid? clienteId,
            Guid? usuarioClienteId,
            Guid? formularioId,
            string? assunto,
            string? descricao,
            string? dadosFormulario,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ClienteId = clienteId;
            UsuarioClienteId = usuarioClienteId;
            FormularioId = formularioId;
            Assunto = assunto;
            Descricao = descricao;
            DadosFormulario = dadosFormulario;
            Status = EPortalSolicitacaoStatus.Aberta;
            AbertaEm = DateTime.UtcNow;
            AddNotifications(new Contract<PortalSolicitacao>()
                .Requires()
                // §18: origem (cliente/usuário) e descrição/assunto quando exigidos.
                .IsTrue(clienteId.HasValue || usuarioClienteId.HasValue, nameof(ClienteId), "A solicitação deve estar vinculada a cliente ou usuário externo. [Origem: PortalSolicitacao]")
                .IsTrue(!string.IsNullOrWhiteSpace(assunto) || !string.IsNullOrWhiteSpace(descricao), nameof(Assunto), "A solicitação deve possuir assunto ou descrição. [Origem: PortalSolicitacao]"));
        }

        public void AtribuirResponsavel(Guid responsavelId, string alteradoPor)
        {
            ResponsavelId = responsavelId;
            Status = EPortalSolicitacaoStatus.EmAtendimento;
            MarcarAlterado(alteradoPor);
        }

        public void Responder(string alteradoPor)
        {
            Status = EPortalSolicitacaoStatus.Respondida;
            RespondidaEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Encerrar(string alteradoPor)
        {
            Status = EPortalSolicitacaoStatus.Encerrada;
            EncerradaEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Cancelar(string alteradoPor)
        {
            Status = EPortalSolicitacaoStatus.Cancelada;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Permissão de recurso do usuário externo (ven_portal_permissao). Fonte: EF §16.5.</summary>
    public class PortalPermissao : EntidadeSaaSBase
    {
        public Guid UsuarioClienteId { get; private set; }
        public EPortalRecurso Recurso { get; private set; }
        public bool PodeVisualizar { get; private set; }
        public bool PodeCriar { get; private set; }
        public bool PodeBaixar { get; private set; }
        public bool PodeAdministrar { get; private set; }

        protected PortalPermissao() { }

        public PortalPermissao(Guid usuarioClienteId, EPortalRecurso recurso, bool podeVisualizar, bool podeCriar, bool podeBaixar, bool podeAdministrar, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            UsuarioClienteId = usuarioClienteId;
            Recurso = recurso;
            PodeVisualizar = podeVisualizar;
            PodeCriar = podeCriar;
            PodeBaixar = podeBaixar;
            PodeAdministrar = podeAdministrar;
            AddNotifications(new Contract<PortalPermissao>()
                .Requires()
                .AreNotEquals(usuarioClienteId, Guid.Empty, nameof(UsuarioClienteId), "O usuário externo é obrigatório. [Origem: PortalPermissao]"));
        }

        public void Alterar(bool podeVisualizar, bool podeCriar, bool podeBaixar, bool podeAdministrar, string alteradoPor)
        {
            PodeVisualizar = podeVisualizar;
            PodeCriar = podeCriar;
            PodeBaixar = podeBaixar;
            PodeAdministrar = podeAdministrar;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>
    /// Auditoria de acesso do portal (ven_portal_auditoria). Fonte: EF §16.6 / §19.
    /// Downloads sensíveis e acessos devem ser registrados.
    /// </summary>
    public class PortalAuditoria : EntidadeSaaSBase
    {
        public Guid? UsuarioClienteId { get; private set; }
        public Guid? UsuarioInternoId { get; private set; }
        public Guid? ClienteId { get; private set; }
        public string Recurso { get; private set; } = string.Empty;
        public string Acao { get; private set; } = string.Empty;
        public Guid? EntidadeId { get; private set; }
        public DateTime DataHora { get; private set; }
        public string? Detalhe { get; private set; }

        protected PortalAuditoria() { }

        public PortalAuditoria(
            Guid? usuarioClienteId,
            Guid? usuarioInternoId,
            Guid? clienteId,
            string recurso,
            string acao,
            Guid? entidadeId,
            string? detalhe,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            UsuarioClienteId = usuarioClienteId;
            UsuarioInternoId = usuarioInternoId;
            ClienteId = clienteId;
            Recurso = recurso;
            Acao = acao;
            EntidadeId = entidadeId;
            Detalhe = detalhe;
            DataHora = DateTime.UtcNow;
            AddNotifications(new Contract<PortalAuditoria>()
                .Requires()
                .IsNotNullOrEmpty(recurso, nameof(Recurso), "O recurso é obrigatório. [Origem: PortalAuditoria]")
                .IsNotNullOrEmpty(acao, nameof(Acao), "A ação é obrigatória. [Origem: PortalAuditoria]"));
        }
    }
}
