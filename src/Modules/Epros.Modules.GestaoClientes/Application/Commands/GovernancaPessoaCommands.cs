using System;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    // ===== Privacidade / LGPD =====
    public record RegistrarConsentimentoTitularCommand(
        Guid PessoaId,
        EFinalidadeDados Finalidade,
        EBaseLegalPrivacidade BaseLegal,
        DateTime? DataConsentimento,
        string? Canal
    ) : ICommand;

    public record RevogarConsentimentoTitularCommand(Guid Id) : ICommand;

    public record AbrirSolicitacaoTitularCommand(
        Guid PessoaId,
        ETipoSolicitacaoTitular Tipo,
        DateTime Prazo
    ) : ICommand;

    public record AnonimizarPessoaCommand(Guid PessoaId) : ICommand;

    public record InativarPessoaCommand(Guid PessoaId, string? Motivo, string? Ip) : ICommand;

    // ===== Deduplicação / Merge =====
    public record CriarRegraDeduplicacaoCommand(
        string Campo,
        EEstrategiaMatch Estrategia,
        decimal Peso,
        decimal LimiarBloqueio,
        decimal LimiarAlerta
    ) : ICommand;

    public record DescartarCandidatoDuplicataCommand(Guid Id) : ICommand;

    public record MesclarPessoaCommand(
        Guid PessoaSobreviventeId,
        Guid PessoaMescladaId,
        Guid? CandidatoDuplicataId
    ) : ICommand;

    // ===== Identificador fiscal / Relacionamento =====
    public record CriarIdentificadorFiscalCommand(
        Guid PessoaId,
        Guid PaisId,
        ETipoIdFiscal Tipo,
        string Valor,
        bool Validado
    ) : ICommand;

    public record CriarRelacionamentoParceiroCommand(
        Guid PessoaOrigemId,
        Guid PessoaDestinoId,
        ETipoRelacaoParceiro TipoRelacao,
        DateTime? VigenciaInicio,
        DateTime? VigenciaFim
    ) : ICommand;

    // ===== Grupo de empresas =====
    public record CriarEmpresaGrupoCommand(string Nome) : ICommand;
    public record AtualizarEmpresaGrupoCommand(Guid Id, string Nome) : ICommand;
    public record DeletarEmpresaGrupoCommand(Guid Id) : ICommand;
}
