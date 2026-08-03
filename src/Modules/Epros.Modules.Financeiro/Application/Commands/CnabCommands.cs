using System;
using System.Collections.Generic;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Application.Models;
using MediatR;

namespace Epros.Modules.Financeiro.Application.Commands
{
    // ----- CNAB: geração de arquivo de remessa e processamento de retorno (FIN-SF §7.5/§7.6) -----

    /// <summary>
    /// Gera o ARQUIVO CNAB de remessa (240/400) para a conta emissora: seleciona os boletos das faturas
    /// elegíveis (Pendente, não remetida), monta header/detalhes/trailer, persiste o conteúdo na Remessa
    /// e marca as faturas como remetidas. Se FaturaIds vier vazio, considera todas as elegíveis da conta.
    /// </summary>
    public record GerarArquivoRemessaCommand(
        Guid ContaEmissoraId, ELayoutCnab Layout, int Grupo = 0,
        IReadOnlyList<Guid>? FaturaIds = null, string? NomeArquivo = null) : IRequest<CommandResult>;

    /// <summary>
    /// Processa um arquivo de RETORNO bancário: detecta o layout pela largura, interpreta as ocorrências,
    /// localiza as faturas por nosso número e baixa as liquidadas (RSF-007). Registra auditoria do
    /// processamento (RetornoBancario). Conteudo = texto do arquivo (linhas de 240/400).
    /// </summary>
    public record ProcessarRetornoBancarioCommand(string Conteudo, string? NomeArquivo = null) : IRequest<CommandResult>;
}
