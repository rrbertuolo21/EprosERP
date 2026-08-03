using System;

namespace Epros.Modules.Aplicativo.Application.Dtos
{
    public record UsuarioInternoDto(
        Guid Id,
        string Nome,
        string Email,
        string Timezone,
        bool PrimaryAdmin
    );

    public record SystemSettingDto(
        Guid Id,
        string Chave,
        string Valor,
        string Escopo,
        bool IsSecret
    );

    public record ExecucaoMassaGlobalDto(
        Guid Id,
        string Descricao,
        string ActionPayload,
        string Status,
        Guid? AprovadoPor,
        string CriadoPor,
        DateTime CriadoEm
    );

    public record CustomPageDto(
        Guid Id,
        string Slug,
        string Status,
        string Conteudo
    );

    public record NewsletterSubscriberDto(
        Guid Id,
        string Email,
        bool Ativo,
        DateTime CriadoEm
    );

    // 1.08G — Dashboard SuperAdmin com métricas SaaS REBASEADAS na AssinaturaCliente (fonte da verdade
    // do ciclo de assinatura), estendendo — sem quebrar — os campos legados. Todos os valores são
    // FACTUAIS/caixa (agregação sobre o estado atual da base). Reconhecimento de receita por competência
    // e apuração fiscal de comissão são Onda 2 (skill de negócio + contador), NÃO calculados aqui.
    public record DashboardGlobalDto(
        int TotalTenants,
        int TotalAssinaturasAtivas,
        decimal ReceitaEstimadaMRR,
        int NovasAssinaturasMes,
        decimal ChurnRate,
        decimal ReceitaTotal,
        // 1.08G — novas métricas SaaS (extensão aditiva; default 0 preserva contratos existentes).
        // MRR normalizado p/ mês: Mensal = Preço; Anual = Preço/12; Vitalícia = 0 (cobrança única, NÃO
        // recorrente — tratamento documentado; não entra no MRR).
        decimal Arpu = 0m,                       // receita recorrente (MRR) / nº de clientes ativos
        decimal ConversaoTrialParaPago = 0m,     // % de trials (TrialAte != null) que converteram (TrialConvertidoEm != null)
                                                 // ⚠️ MÉTODO DE LTV = PARÂMETRO (há variantes). Aqui: LTV = ARPU / churn (fração). NÃO é verdade contábil.
        decimal Ltv = 0m,
        decimal InadimplenciaValorTotal = 0m,    // soma do valor das faturas vencidas e não pagas na base
        int InadimplenciaQtdFaturas = 0          // nº de faturas vencidas e não pagas na base
    );

    public record SuperAdminClienteDto(
        Guid Id,
        string TenantId,
        string RazaoSocial,
        string Cnpj,
        string Email,
        string PlanoNome,
        string StatusSaaS,
        bool Ativo
    );
}
