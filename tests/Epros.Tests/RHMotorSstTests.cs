using Epros.Modules.RH.Domain.Folha.Calculo;
using Epros.Modules.RH.Domain.Sst.Calculo;
using Xunit;

namespace Epros.Tests
{
    // Motor de adicionais de SST. Skill departamento-pessoal/sst (RN-01 insalubridade, RN-02 periculosidade).
    public class RHMotorSstTests
    {
        private static readonly TabelasFolha T = TabelasFolha.Vigente(2026); // SM 2026 = 1621,00

        [Fact]
        public void Insalubridade_Grau_Medio_20_Do_Salario_Minimo()
        {
            // 20% × 1621,00 = 324,20
            Assert.Equal(324.20m, MotorAdicionalSst.Insalubridade(GrauInsalubridade.Medio, T.SalarioMinimo));
        }

        [Fact]
        public void Insalubridade_Graus_Minimo_E_Maximo()
        {
            Assert.Equal(162.10m, MotorAdicionalSst.Insalubridade(GrauInsalubridade.Minimo, 1621m)); // 10%
            Assert.Equal(648.40m, MotorAdicionalSst.Insalubridade(GrauInsalubridade.Maximo, 1621m)); // 40%
        }

        [Fact]
        public void Periculosidade_30_Do_Salario_Base()
        {
            // 30% × 3000 = 900,00
            Assert.Equal(900.00m, MotorAdicionalSst.Periculosidade(3000m));
        }

        [Fact]
        public void Nao_Cumulativo_Escolhe_O_Mais_Benefico()
        {
            // Insalub máximo 40%×1621 = 648,40 vs periculosidade 30%×3000 = 900 → escolhe periculosidade.
            var r = MotorAdicionalSst.Calcular(GrauInsalubridade.Maximo, 3000m, T, temPericulosidade: true);
            Assert.Equal(900.00m, r.AdicionalDevido);
            Assert.Equal("Periculosidade", r.Escolhido);
            Assert.Equal(648.40m, r.Insalubridade);
            Assert.Equal(900.00m, r.Periculosidade);
        }

        [Fact]
        public void So_Insalubridade_Quando_Nao_Ha_Periculosidade()
        {
            var r = MotorAdicionalSst.Calcular(GrauInsalubridade.Medio, 3000m, T, temPericulosidade: false);
            Assert.Equal("Insalubridade", r.Escolhido);
            Assert.Equal(324.20m, r.AdicionalDevido);
        }

        [Fact]
        public void Sem_Enquadramento_Nao_Ha_Adicional()
        {
            var r = MotorAdicionalSst.Calcular(GrauInsalubridade.Nenhum, 3000m, T, temPericulosidade: false);
            Assert.Equal(0m, r.AdicionalDevido);
            Assert.Equal("Nenhum", r.Escolhido);
        }
    }
}
