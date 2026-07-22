using System;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    // ---------- Upload em partes / direto ----------

    /// <summary>
    /// Recebe uma parte (faixa de bytes) de um upload fracionado. A primeira parte cria a execução;
    /// ao atingir o total esperado, o arquivo é consolidado e enviado ao storage com deduplicação.
    /// [PLT-UPL, EF UPLOAD 7.2/7.3]
    /// </summary>
    public record ReceberParteUploadCommand(
        Guid? ExecucaoUploadId,
        Guid UsuarioId,
        string NomeOriginal,
        string Extensao,
        byte[] Conteudo,
        long ByteInicio,
        long ByteFim,
        long TotalBytes,
        string? PastaDestino) : ICommand;

    public class ReceberParteUploadCommandValidator : AbstractValidator<ReceberParteUploadCommand>
    {
        public ReceberParteUploadCommandValidator()
        {
            RuleFor(x => x.UsuarioId).NotEmpty().WithMessage("O usuário do upload é obrigatório.");
            RuleFor(x => x.NomeOriginal).NotEmpty().WithMessage("O nome do arquivo é obrigatório.");
            RuleFor(x => x.TotalBytes).GreaterThan(0).WithMessage("O total de bytes deve ser maior que zero.");
            RuleFor(x => x.Conteudo).NotNull().WithMessage("O conteúdo da parte é obrigatório.");
        }
    }

    /// <summary>Upload direto de arquivo completo, com deduplicação por hash. [PLT-UPL, EF UPLOAD 7.1/7.3]</summary>
    public record RegistrarArquivoUploadCommand(
        Guid UsuarioId,
        Guid? UsuarioUploadId,
        EUplOrigemUpload Origem,
        string NomeOriginal,
        string Extensao,
        byte[] Conteudo,
        string? PastaDestino) : ICommand;

    public class RegistrarArquivoUploadCommandValidator : AbstractValidator<RegistrarArquivoUploadCommand>
    {
        public RegistrarArquivoUploadCommandValidator()
        {
            RuleFor(x => x.UsuarioId).NotEmpty().WithMessage("O usuário do upload é obrigatório.");
            RuleFor(x => x.NomeOriginal).NotEmpty().WithMessage("O nome do arquivo é obrigatório.");
            RuleFor(x => x.Conteudo).NotNull().WithMessage("O conteúdo do arquivo é obrigatório.");
        }
    }

    // ---------- Importação tabular (CSV/XLSX) ----------

    /// <summary>
    /// Cria a execução de importação (import_ref único) a partir de um arquivo já enviado (por chave
    /// temporária ou arquivo consolidado) e processa CSV/XLSX linha a linha. [PLT-UPL, EF UPLOAD 7.6]
    /// </summary>
    public record ImportarArquivoTabularCommand(
        Guid UsuarioId,
        string TipoImportacao,
        Guid? ArquivoId,
        string NomeArquivo,
        string Extensao,
        byte[] Conteudo,
        bool IgnorarLinhasInvalidas) : ICommand;

    public class ImportarArquivoTabularCommandValidator : AbstractValidator<ImportarArquivoTabularCommand>
    {
        public ImportarArquivoTabularCommandValidator()
        {
            RuleFor(x => x.UsuarioId).NotEmpty().WithMessage("O usuário importador é obrigatório.");
            RuleFor(x => x.TipoImportacao).NotEmpty().WithMessage("O tipo de importação é obrigatório.");
            RuleFor(x => x.NomeArquivo).NotEmpty().WithMessage("O nome do arquivo é obrigatório.");
            RuleFor(x => x.Extensao).NotEmpty().Must(e => !string.IsNullOrWhiteSpace(e) && e.TrimStart('.').ToLowerInvariant() is "csv" or "xlsx")
                .WithMessage("Somente arquivos CSV ou XLSX são aceitos para importação tabular.");
            RuleFor(x => x.Conteudo).NotNull().WithMessage("O conteúdo do arquivo é obrigatório.");
        }
    }

    /// <summary>Salva um mapeamento coluna→campo reaproveitável para importação. [PLT-UPL, EF UPLOAD 7.9.3]</summary>
    public record SalvarMapeamentoImportacaoCommand(Guid UsuarioId, string TipoImportacao, string Nome, string MapaColunasJson) : ICommand;

    public class SalvarMapeamentoImportacaoCommandValidator : AbstractValidator<SalvarMapeamentoImportacaoCommand>
    {
        public SalvarMapeamentoImportacaoCommandValidator()
        {
            RuleFor(x => x.UsuarioId).NotEmpty().WithMessage("O dono do mapeamento é obrigatório.");
            RuleFor(x => x.TipoImportacao).NotEmpty().WithMessage("O tipo de importação é obrigatório.");
            RuleFor(x => x.Nome).NotEmpty().WithMessage("O nome do mapeamento é obrigatório.");
            RuleFor(x => x.MapaColunasJson).NotEmpty().WithMessage("O mapa de colunas é obrigatório.");
        }
    }

    /// <summary>Desfaz (undo) os registros de uma execução de importação quando suportado. [PLT-UPL, EF UPLOAD 7.9.7]</summary>
    public record DesfazerImportacaoCommand(Guid ExecucaoImportacaoId) : ICommand;
}
