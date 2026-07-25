using Flunt.Notifications;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.ValueObjects
{
    public class Endereco : Notifiable<Notification>
    {
        public string Logradouro { get; private set; } = string.Empty;
        public string Numero { get; private set; } = string.Empty;
        public string? Complemento { get; private set; }
        public string Bairro { get; private set; } = string.Empty;
        public string Cep { get; private set; } = string.Empty;
        public string Cidade { get; private set; } = string.Empty;
        public string Estado { get; private set; } = string.Empty;

        protected Endereco() { } // EF Core

        public Endereco(string logradouro, string numero, string? complemento, string bairro, string cep, string cidade, string estado)
        {
            AddNotifications(new Contract<Endereco>()
                .Requires()
                .IsNotNullOrEmpty(logradouro, nameof(Logradouro), "Logradouro é obrigatório")
                .IsNotNullOrEmpty(numero, nameof(Numero), "Número é obrigatório")
                .IsNotNullOrEmpty(bairro, nameof(Bairro), "Bairro é obrigatório")
                .IsNotNullOrEmpty(cep, nameof(Cep), "CEP é obrigatório")
                .IsNotNullOrEmpty(cidade, nameof(Cidade), "Cidade é obrigatório")
                .IsNotNullOrEmpty(estado, nameof(Estado), "Estado é obrigatório")
            );

            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            Cep = cep;
            Cidade = cidade;
            Estado = estado;
        }
    }
}
