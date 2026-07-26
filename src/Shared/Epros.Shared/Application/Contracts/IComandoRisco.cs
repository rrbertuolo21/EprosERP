namespace Epros.Shared.Application.Contracts
{
    public interface IComandoRisco : ICommand
    {
        bool Aprovado { get; }
        bool Simular { get; }
        string ObterDescricao();
    }
}
