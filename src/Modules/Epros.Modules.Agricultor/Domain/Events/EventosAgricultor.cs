namespace Epros.Modules.Agricultor.Domain.Events
{
    /// <summary>
    /// Catálogo de eventos de integração do módulo AGRICULTOR (namespace agr.*), publicados via Outbox
    /// pós-commit (TRANSVERSAL T2). São strings estáveis: consumidores externos dependem delas.
    /// </summary>
    public static class EventosAgricultor
    {
        // Painel do produtor
        public const string PropriedadeCadastrada = "agr.propriedade.cadastrada";
        public const string SafraAberta = "agr.safra.aberta";
        public const string SafraEncerrada = "agr.safra.encerrada";
        public const string DespesaRegistrada = "agr.despesa.registrada";
        public const string ReceitaRegistrada = "agr.receita.registrada";

        // Livro Caixa Digital (LCDPR)
        public const string EscrituracaoAberta = "agr.lcdpr.escrituracao.aberta";
        public const string EscrituracaoFechada = "agr.lcdpr.escrituracao.fechada";
        public const string LancamentoRegistrado = "agr.lcdpr.lancamento.registrado";
        public const string ArquivoExportado = "agr.lcdpr.arquivo.exportado";
    }
}
