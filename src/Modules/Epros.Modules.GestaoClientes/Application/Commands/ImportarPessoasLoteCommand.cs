using System.Collections.Generic;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    // T-02 / P1-3 (auditoria CADASTROS): importação em lote de pessoas OPERÁVEL. As entidades
    // PessoaImportacaoLote/Linha existiam mapeadas mas sem command/handler/controller — este contrato
    // liga o fluxo (mapeamento → validação → processamento) reutilizando a criação de Pessoa canônica.
    //
    // O LAYOUT OFICIAL do arquivo (colunas/ordem/versão) é uma definição de produto ainda não publicada:
    // o command recebe as linhas JÁ PARSEADAS (o parsing CSV/planilha do layout oficial fica no
    // adaptador de upload). // valida — layout oficial (colunas e versão) pendente de definição da Siser.

    /// <summary>Uma linha do lote já mapeada para os campos essenciais de Pessoa.</summary>
    public record PessoaImportacaoLinhaInput(
        int NumeroLinha,
        ETipoPessoa TipoPessoa,
        string? Documento,          // CPF (física) ou CNPJ (jurídica); identificação (estrangeira)
        string? Nome,               // nome (física) ou razão social (jurídica/estrangeira)
        string? Sobrenome,          // apenas física
        string? NomeFantasia,       // apenas jurídica
        string? Email,
        string? Telefone,
        bool EhCliente = true);

    /// <summary>
    /// Importa um lote de pessoas. <paramref name="AbortarSeHouverErro"/> = política tudo-ou-nada:
    /// quando true, se QUALQUER linha falhar na validação de mapeamento o lote inteiro é rejeitado e
    /// nenhuma Pessoa é criada; quando false, aceita as linhas válidas e rejeita as inválidas (parcial).
    /// </summary>
    public record ImportarPessoasLoteCommand(
        string NomeArquivo,
        string LayoutVersao,
        bool AbortarSeHouverErro,
        List<PessoaImportacaoLinhaInput> Linhas) : ICommand;
}
