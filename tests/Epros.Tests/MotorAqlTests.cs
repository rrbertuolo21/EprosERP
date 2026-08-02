using Epros.Modules.Qualidade.Domain.Services.Aql;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes do motor de amostragem AQL (ISO 2859-1 / NBR 5426). Ancoras da skill canonica
    /// `qualidade`: forma fechada NORMAL (L@1.0=5/6, H(50)@2.5=3/4, G@1.0=0/1) + comutacao RN-01..RN-06.
    /// </summary>
    public class MotorAqlTests
    {
        private readonly MotorAql _motor = new();

        // ================= Letra-codigo (Tab. I) =================
        [Theory]
        [InlineData(2, ENivelInspecao.II, 'A')]
        [InlineData(200, ENivelInspecao.II, 'G')]   // 151-280 / II
        [InlineData(300, ENivelInspecao.II, 'H')]   // 281-500 / II  -> n=50
        [InlineData(1000, ENivelInspecao.II, 'J')]  // 501-1200 / II
        [InlineData(2000, ENivelInspecao.II, 'K')]  // 1201-3200 / II
        [InlineData(5000, ENivelInspecao.II, 'L')]  // 3201-10000 / II -> n=200
        public void ResolverLetraCodigo_Casa_Com_TabelaI(long n, ENivelInspecao nivel, char esperada)
        {
            Assert.Equal(esperada, _motor.ResolverLetraCodigo(n, nivel));
        }

        [Fact]
        public void TamanhoAmostra_H_Eh_50()
        {
            Assert.Equal(50, _motor.TamanhoAmostraPorLetra('H'));
        }

        // ================= Forma fechada NORMAL (ancoras) =================
        [Fact]
        public void Ancora_L_AQL_1_0_Da_5_6()
        {
            var p = _motor.CalcularPlano(5000, ENivelInspecao.II, 1.0m); // letra L, n=200
            Assert.Equal('L', p.LetraCodigo);
            Assert.Equal(200, p.TamanhoAmostra);
            Assert.Equal(5, p.NumeroAceitacao);
            Assert.Equal(6, p.NumeroRejeicao);
            Assert.False(p.InspecaoTotal);
        }

        [Fact]
        public void Ancora_n50_AQL_2_5_Da_3_4()
        {
            var p = _motor.CalcularPlano(300, ENivelInspecao.II, 2.5m); // letra H, n=50
            Assert.Equal(50, p.TamanhoAmostra);
            Assert.Equal(3, p.NumeroAceitacao);
            Assert.Equal(4, p.NumeroRejeicao);
        }

        [Fact]
        public void Ancora_G_AQL_1_0_Da_0_1()
        {
            var p = _motor.CalcularPlano(200, ENivelInspecao.II, 1.0m); // letra G, n=32
            Assert.Equal('G', p.LetraCodigo);
            Assert.Equal(32, p.TamanhoAmostra);
            Assert.Equal(0, p.NumeroAceitacao);
            Assert.Equal(1, p.NumeroRejeicao);
        }

        [Fact]
        public void Ancora_J_AQL_1_5_Da_3_4()
        {
            var p = _motor.CalcularPlano(1000, ENivelInspecao.II, 1.5m); // letra J, n=80
            Assert.Equal(80, p.TamanhoAmostra);
            Assert.Equal(3, p.NumeroAceitacao);
            Assert.Equal(4, p.NumeroRejeicao);
        }

        // ================= Decisao do lote =================
        [Theory]
        [InlineData(0, EDecisaoLote.Aceita)]
        [InlineData(5, EDecisaoLote.Aceita)]   // d = Ac -> aceita
        [InlineData(6, EDecisaoLote.Rejeita)]  // d = Re -> rejeita
        [InlineData(9, EDecisaoLote.Rejeita)]
        public void Decidir_Aplica_Ac_Re(int defeituosos, EDecisaoLote esperado)
        {
            var p = _motor.CalcularPlano(5000, ENivelInspecao.II, 1.0m); // Ac5 Re6
            Assert.Equal(esperado, p.Decidir(defeituosos));
        }

        // ================= Seta ↓ -> inspecao 100% =================
        [Fact]
        public void Seta_Baixo_Com_n_Maior_Que_Lote_Faz_Inspecao_100()
        {
            var p = _motor.CalcularPlano(5, ENivelInspecao.II, 0.10m); // letra C, AQL apertado -> ↓
            Assert.True(p.InspecaoTotal);
            Assert.Equal(5, p.TamanhoAmostra); // amostra = lote inteiro
            Assert.Equal(0, p.NumeroAceitacao);
        }

        // ================= Severa (mais rigorosa) =================
        [Fact]
        public void Severa_L_AQL_1_0_Reduz_Ac_Para_3_4()
        {
            var p = _motor.CalcularPlano(5000, ENivelInspecao.II, 1.0m, ESeveridadeAql.Severa);
            Assert.Equal(3, p.NumeroAceitacao); // vs 5 no normal
            Assert.Equal(4, p.NumeroRejeicao);
            Assert.Equal(200, p.TamanhoAmostra); // severa usa mesmo n do normal
        }

        // ================= Atenuada (com vao Ac<..<Re) =================
        [Fact]
        public void Atenuada_Tem_Vao_E_Amostra_Reduzida()
        {
            var p = _motor.CalcularPlano(200, ENivelInspecao.II, 1.0m, ESeveridadeAql.Atenuada); // letra G
            Assert.Equal(13, p.TamanhoAmostra);          // n reduzido (vs 32 no normal)
            Assert.True(p.NumeroRejeicao > p.NumeroAceitacao + 1); // ha vao
            // d entre Ac e Re -> aceita mas volta a normal.
            Assert.Equal(EDecisaoLote.AceitaERetornaNormal, p.Decidir(p.NumeroAceitacao + 1));
        }

        // ================= Comutacao de severidade (RN-01..RN-06) =================
        [Fact]
        public void RN01_Inicia_Em_Normal()
        {
            Assert.Equal(ESeveridadeAql.Normal, EstadoComutacaoAql.Inicial().Severidade);
        }

        [Fact]
        public void RN02_Dois_De_Cinco_Rejeitados_Vai_Para_Severa()
        {
            var motor = new MotorComutacao();
            var e = EstadoComutacaoAql.Inicial();
            motor.Registrar(e, EDecisaoLote.Rejeita, 3);
            Assert.Equal(ESeveridadeAql.Normal, e.Severidade); // 1 rejeicao ainda nao comuta
            motor.Registrar(e, EDecisaoLote.Aceita, 0);
            motor.Registrar(e, EDecisaoLote.Rejeita, 4);       // 2a rejeicao dentro de 5
            Assert.Equal(ESeveridadeAql.Severa, e.Severidade);
        }

        [Fact]
        public void RN03_Cinco_Aceitos_Em_Severa_Volta_A_Normal()
        {
            var motor = new MotorComutacao();
            var e = EstadoComutacaoAql.Restaurar(ESeveridadeAql.Severa);
            for (int k = 0; k < 4; k++) motor.Registrar(e, EDecisaoLote.Aceita, 0);
            Assert.Equal(ESeveridadeAql.Severa, e.Severidade);
            motor.Registrar(e, EDecisaoLote.Aceita, 0); // 5o aceito
            Assert.Equal(ESeveridadeAql.Normal, e.Severidade);
        }

        [Fact]
        public void RN06_Cinco_Rejeitados_Em_Severa_Suspende()
        {
            var motor = new MotorComutacao();
            var e = EstadoComutacaoAql.Restaurar(ESeveridadeAql.Severa);
            for (int k = 0; k < 5; k++) motor.Registrar(e, EDecisaoLote.Rejeita, 2);
            Assert.True(e.Suspensa);
        }

        [Fact]
        public void RN04_Dez_Aceitos_Com_Atenuada_Habilitada_Vai_A_Atenuada()
        {
            var motor = new MotorComutacao();
            var e = EstadoComutacaoAql.Inicial();
            var opc = new OpcoesComutacao { AtenuadaHabilitada = true, ProducaoEstavel = true };
            for (int k = 0; k < 10; k++) motor.Registrar(e, EDecisaoLote.Aceita, 0, opc);
            Assert.Equal(ESeveridadeAql.Atenuada, e.Severidade);
        }

        [Fact]
        public void RN04_Sem_Habilitar_Atenuada_Permanece_Normal()
        {
            var motor = new MotorComutacao();
            var e = EstadoComutacaoAql.Inicial();
            for (int k = 0; k < 20; k++) motor.Registrar(e, EDecisaoLote.Aceita, 0);
            Assert.Equal(ESeveridadeAql.Normal, e.Severidade);
        }

        [Fact]
        public void RN05_Rejeicao_Em_Atenuada_Volta_A_Normal()
        {
            var motor = new MotorComutacao();
            var e = EstadoComutacaoAql.Restaurar(ESeveridadeAql.Atenuada);
            motor.Registrar(e, EDecisaoLote.AceitaERetornaNormal, 1);
            Assert.Equal(ESeveridadeAql.Normal, e.Severidade);
        }
    }
}
