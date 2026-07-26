using System;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public enum ETipoPessoa
    {
        PessoaFisica = 1,
        PessoaJuridica = 2,
        PessoaEstrangeira = 3
    }

    public enum EStatusPessoa
    {
        Ativo = 1,
        Inativo = 2,
        Todos = 3
    }

    public enum ETipoIndicadorIe
    {
        ContribuinteICMS = 1,
        Isento = 2,
        NaoContribuinte = 3
    }

    public enum ETipoContribuinte
    {
        NaoInformado = 1,
        SimplesNacional = 2,
        RPA = 3,
        MEI = 4
    }

    public enum ETipoGenero
    {
        NaoUtiliza = 1,
        Feminino = 2,
        Masculino = 3,
        Outros = 4
    }

    public enum ETipoPix
    {
        ChaveAleatoria = 1,
        CpfCnpj = 2,
        Email = 3,
        Telefone = 4
    }

    public enum ETipoContatoTelefonico
    {
        NaoUtiliza = 1,
        Residencial = 2,
        Comercial = 3,
        Recado = 4,
        Emergencia = 5,
        Outros = 6
    }

    public enum ETipoContatoEmail
    {
        NaoUtiliza = 1,
        EnvioNFe = 2,
        EnvioNfse = 3,
        Contador = 4
    }

    public enum ETipoCargo
    {
        Operador = 1,
        Vendedor = 2,
        Supervisor = 3,
        Gerente = 4
    }

    public enum ETipoCategoriaCnh
    {
        NaoUtiliza = 1,
        A = 2,
        B = 3,
        C = 4,
        D = 5,
        E = 6,
        AB = 7,
        AC = 8,
        AD = 9,
        AE = 10
    }

    public enum ETipoVeiculo
    {
        Veiculo = 1,
        Reboque = 2
    }

    public enum ETipoVinculoMotorista
    {
        Proprio = 1,
        Terceiros = 2
    }

    public enum EEstado
    {
        AC, AL, AP, AM, BA, CE, DF, ES, GO, MA, MT, MS, MG, PA, PB, PR, PE, PI, RJ, RN, RS, RO, RR, SC, SP, SE, TO
    }

    public enum RegimeTributario
    {
        SimplesNacional = 1,
        LucroPresumido = 2,
        LucroReal = 3
    }

    public enum ETipoEndereco
    {
        Principal = 1,
        Entrega = 2,
        Obra = 3
    }

    public enum RegimeApuracao
    {
        Cumulativo = 1,
        NaoCumulativo = 2,
        Misto = 3
    }

    public enum StockCalculationMode
    {
        CustoMedio = 1,
        FIFO = 2,
        Ultimo = 3
    }

    // Enums portados fielmente do legado (Epros.ERP.Shared.Enums) para os parâmetros
    // fiscais e configurações da Empresa. Mantêm os nomes e valores originais.
    public enum ERegimeTributario
    {
        NaoUtiliza = -1,
        SimplesNacional = 1,
        SimplesNacionalExcessoSublimite = 2,
        RegimeNormal = 3,
        SimplesNacionalMei = 4
    }

    public enum ERegimeApuracao
    {
        NaoUtiliza = 0,
        LucroReal = 1,
        LucroPresumido = 2
    }

    public enum ETipoConfiguracaoEstoque
    {
        NaoBloquear = 0,
        BloquearVenda = 1,
        BloquearVendaEPedido = 2,
        PermitirVendaComAviso = 3
    }

    public enum ETipoAmbiente
    {
        Producao = 1,
        Homologacao = 2
    }
}
