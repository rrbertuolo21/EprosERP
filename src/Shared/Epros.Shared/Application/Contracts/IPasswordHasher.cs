namespace Epros.Shared.Application.Contracts
{
    /// <summary>
    /// Serviço de hashing de senhas. Nunca armazene ou compare senhas em texto puro.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Gera um hash autocontido (salt + parâmetros embutidos) a partir da senha em texto puro.
        /// </summary>
        string Hash(string senha);

        /// <summary>
        /// Verifica, em tempo constante, se a senha informada corresponde ao hash armazenado.
        /// Retorna false (sem lançar) quando o hash armazenado é nulo, vazio ou está em formato
        /// inválido/legado — evitando quebras em dados antigos.
        /// </summary>
        bool Verify(string senha, string hashArmazenado);
    }
}
