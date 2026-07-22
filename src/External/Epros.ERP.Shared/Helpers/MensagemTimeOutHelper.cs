namespace Epros.ERP.Shared.Helpers
{
    public static class MensagemTimeOutHelper
    {
        public static bool VerificarSeMensagemIndicaTimeOut(string mensagem)
        {
            if (string.IsNullOrWhiteSpace(mensagem))
                return false;

            var msg = mensagem.AsSpan().Trim().ToString().ToLowerInvariant();

            // fast-path (mais comum e barato)
            if (msg.Contains("timeout") || msg.Contains("timed out"))
                return true;

            // checks mais específicos (evita falso positivo)
            return msg.Contains("read timed out")
                || msg.Contains("write timed out")
                || msg.Contains("connection timed out")
                || msg.Contains("task was canceled")
                || msg.Contains("operation was canceled")
                || msg.Contains("underlying connection was closed")
                || msg.Contains("a conexão subjacente foi fechada")
                || msg.Contains("forcibly closed by the remote host")
                || msg.Contains("connection reset by peer")
                || msg.Contains("unexpected eof")
                || msg.Contains("unexpected end of file")
                || msg.Contains("the server did not respond")
                || msg.Contains("no response received");
        }
    }
}