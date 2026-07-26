using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    public record ExecutarAjustePlanosLoteCommand(
        decimal PercentualAjuste,
        bool Aprovado = false,
        bool Simular = false
    ) : IComandoRisco
    {
        public string ObterDescricao() => 
            $"Ajuste em lote dos preços dos planos comerciais em {PercentualAjuste}%.";
    }
}
