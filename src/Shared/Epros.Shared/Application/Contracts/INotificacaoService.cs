using System.Threading.Tasks;

namespace Epros.Shared.Application.Contracts
{
    public interface INotificacaoService
    {
        Task EnviarEmailAsync(string destinatario, string assunto, string corpoHtml);
        Task EnviarSmsAsync(string telefone, string mensagem);
        Task EnviarWhatsAppAsync(string telefone, string mensagem);
    }
}
