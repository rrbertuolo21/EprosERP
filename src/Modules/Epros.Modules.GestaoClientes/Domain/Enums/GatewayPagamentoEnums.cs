namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Provedor de gateway de pagamento suportado pela plataforma.</summary>
    public enum EProvedorGateway
    {
        MercadoPago = 1
    }

    /// <summary>Ambiente de operação do gateway (sandbox de testes ou produção).</summary>
    public enum EAmbienteGateway
    {
        Sandbox = 1,
        Producao = 2
    }
}
