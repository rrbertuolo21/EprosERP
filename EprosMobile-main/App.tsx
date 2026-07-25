import React, { useEffect, useState } from 'react';
import { StatusBar } from 'expo-status-bar';
import { 
  StyleSheet, 
  Text, 
  View, 
  TouchableOpacity, 
  ActivityIndicator,
  TextInput,
  ScrollView,
  SafeAreaView
} from 'react-native';
import { 
  inicializarBancoLocal, 
  registrarVendaOffline, 
  getDbConnection,
  abrirCaixaLocal,
  fecharCaixaLocal,
  registrarMovimentoCaixaLocal
} from './src/services/db';
import { 
  sincronizarProdutos, 
  sincronizarVendas,
  sincronizarCaixas
} from './src/services/sync';

export default function App() {
  const [dbReady, setDbReady] = useState(false);
  const [loading, setLoading] = useState(false);
  const [vendasCount, setVendasCount] = useState(0);
  const [percentualDesconto, setPercentualDesconto] = useState('5');
  const [mensagemDesconto, setMensagemDesconto] = useState('');

  // Estados de Sincronização
  const [apiUrl, setApiUrl] = useState('http://localhost:5000'); // Porta padrão do Kestrel local ou gateway Caddy
  const [tenantId, setTenantId] = useState('tenant-abac');
  const [syncing, setSyncing] = useState(false);
  const [syncMessage, setSyncMessage] = useState('');
  const [produtos, setProdutos] = useState<any[]>([]);

  // Estados de Lançamento de Venda Offline
  const [selectedProdutoId, setSelectedProdutoId] = useState<string | null>(null);
  const [quantidadeVenda, setQuantidadeVenda] = useState('1');
  const [vendaFeedback, setVendaFeedback] = useState('');

  // Estados do Caixa / Frente de Caixa (Bloco 15)
  const [caixaAtivo, setCaixaAtivo] = useState<any>(null);
  const [saldoAberturaInput, setSaldoAberturaInput] = useState('100');
  const [saldoFechamentoInput, setSaldoFechamentoInput] = useState('');
  const [tipoMovimento, setTipoMovimento] = useState<'Suprimento' | 'Sangria'>('Suprimento');
  const [valorMovimento, setValorMovimento] = useState('');
  const [obsMovimento, setObsMovimento] = useState('');
  const [caixaFeedback, setCaixaFeedback] = useState('');
  const [saldoAtual, setSaldoAtual] = useState(0);

  // Perfil mockado do usuário logado (ABAC)
  const userPerfil = {
    nome: "Rafael Santos",
    email: "rafael.santos@epros.com.br",
    cargo: "Vendedor",
    departamento: "Comercial",
    limiteDesconto: 10.00, // 10%
    tenant: "Epros Matriz (ID: tenant-abac)"
  };

  useEffect(() => {
    const setupDb = async () => {
      try {
        await inicializarBancoLocal();
        setDbReady(true);
        await atualizarContadorVendas();
        await carregarProdutosLocais();
        await carregarCaixaAtivo();
      } catch (error) {
        console.error("Falha ao inicializar o SQLite:", error);
      }
    };

    setupDb();
  }, []);

  const carregarCaixaAtivo = async () => {
    try {
      const db = await getDbConnection();
      // Obter o caixa aberto mais recente
      const caixa: any = await db.getFirstAsync("SELECT * FROM caixas WHERE status = 'Aberto' ORDER BY criado_em DESC LIMIT 1");
      if (caixa) {
        setCaixaAtivo(caixa);
        await calcularSaldoAtual(caixa.id, caixa.saldo_abertura);
      } else {
        setCaixaAtivo(null);
        setSaldoAtual(0);
      }
    } catch (e) {
      console.error("Erro ao carregar caixa ativo:", e);
    }
  };

  const calcularSaldoAtual = async (caixaId: string, saldoAbertura: number) => {
    try {
      const db = await getDbConnection();
      
      // Soma das vendas
      const vendasResult: any = await db.getFirstAsync(
        "SELECT SUM(total) as totalVendas FROM vendas WHERE caixa_id = ? AND status != 'Cancelada'",
        [caixaId]
      );
      const totalVendas = vendasResult?.totalVendas || 0;

      // Soma dos suprimentos
      const suprimentosResult: any = await db.getFirstAsync(
        "SELECT SUM(valor) as totalSuprimentos FROM caixa_movimentos WHERE caixa_id = ? AND tipo = 'Suprimento'",
        [caixaId]
      );
      const totalSuprimentos = suprimentosResult?.totalSuprimentos || 0;

      // Soma das sangrias
      const sangriasResult: any = await db.getFirstAsync(
        "SELECT SUM(valor) as totalSangrias FROM caixa_movimentos WHERE caixa_id = ? AND tipo = 'Sangria'",
        [caixaId]
      );
      const totalSangrias = sangriasResult?.totalSangrias || 0;

      const saldo = saldoAbertura + totalVendas + totalSuprimentos - totalSangrias;
      setSaldoAtual(Math.round(saldo * 100) / 100);
    } catch (e) {
      console.error("Erro ao calcular saldo atual:", e);
    }
  };

  const handleAbrirCaixa = async () => {
    const saldo = parseFloat(saldoAberturaInput);
    if (isNaN(saldo) || saldo < 0) {
      setCaixaFeedback("⛔ Saldo de abertura inválido.");
      return;
    }
    setLoading(true);
    setCaixaFeedback("");
    try {
      const id = Math.random().toString(36).substring(7);
      await abrirCaixaLocal(id, userPerfil.email, saldo);
      await carregarCaixaAtivo();
      setCaixaFeedback("✅ Caixa aberto com sucesso!");
    } catch (err) {
      console.error(err);
      setCaixaFeedback("⛔ Erro ao abrir caixa local.");
    } finally {
      setLoading(false);
    }
  };

  const handleFecharCaixa = async () => {
    if (!caixaAtivo) return;
    const saldo = parseFloat(saldoFechamentoInput);
    if (isNaN(saldo) || saldo < 0) {
      setCaixaFeedback("⛔ Saldo de fechamento inválido.");
      return;
    }
    setLoading(true);
    setCaixaFeedback("");
    try {
      await fecharCaixaLocal(caixaAtivo.id, saldo);
      await carregarCaixaAtivo();
      setCaixaFeedback("✅ Caixa fechado com sucesso!");
      setSaldoFechamentoInput('');
    } catch (err) {
      console.error(err);
      setCaixaFeedback("⛔ Erro ao fechar caixa local.");
    } finally {
      setLoading(false);
    }
  };

  const handleRegistrarMovimento = async () => {
    if (!caixaAtivo) return;
    const valor = parseFloat(valorMovimento);
    if (isNaN(valor) || valor <= 0) {
      setCaixaFeedback("⛔ Valor do movimento inválido.");
      return;
    }
    if (tipoMovimento === 'Sangria' && valor > saldoAtual) {
      setCaixaFeedback("⛔ Sangria não pode ser maior que o saldo atual.");
      return;
    }
    setLoading(true);
    setCaixaFeedback("");
    try {
      const id = Math.random().toString(36).substring(7);
      const syncId = Math.random().toString(36).substring(7);
      await registrarMovimentoCaixaLocal(id, syncId, caixaAtivo.id, tipoMovimento, valor, obsMovimento);
      await calcularSaldoAtual(caixaAtivo.id, caixaAtivo.saldo_abertura);
      setCaixaFeedback(`✅ ${tipoMovimento} de R$ ${valor.toFixed(2)} registrado com sucesso!`);
      setValorMovimento('');
      setObsMovimento('');
    } catch (err) {
      console.error(err);
      setCaixaFeedback("⛔ Erro ao registrar movimentação.");
    } finally {
      setLoading(false);
    }
  };

  const atualizarContadorVendas = async () => {
    try {
      const db = await getDbConnection();
      const result: any = await db.getFirstAsync('SELECT COUNT(*) as total FROM vendas WHERE enviado = 0');
      setVendasCount(result?.total || 0);
    } catch (e) {
      console.error(e);
    }
  };

  const carregarProdutosLocais = async () => {
    try {
      const db = await getDbConnection();
      const list: any[] = await db.getAllAsync('SELECT * FROM produtos');
      setProdutos(list);
      if (list.length > 0 && !selectedProdutoId) {
        setSelectedProdutoId(list[0].id);
      }
    } catch (e) {
      console.error("Erro ao carregar produtos locais:", e);
    }
  };

  const handleSync = async () => {
    if (!apiUrl) {
      setSyncMessage("⛔ Por favor, insira a URL da API.");
      return;
    }
    setSyncing(true);
    setSyncMessage("⏳ Sincronizando dados com o servidor...");
    try {
      // 1. Enviar caixas e movimentos locais pendentes
      const caixasSync = await sincronizarCaixas(apiUrl, tenantId);

      // 2. Enviar vendas locais pendentes
      const enviadas = await sincronizarVendas(apiUrl, tenantId);
      
      // 3. Buscar novos produtos do servidor
      const baixados = await sincronizarProdutos(apiUrl, tenantId);

      // 4. Atualizar estados locais
      await atualizarContadorVendas();
      await carregarProdutosLocais();
      await carregarCaixaAtivo();

      setSyncMessage(`✅ Sincronizado! Caixas: ${caixasSync}. Vendas enviadas: ${enviadas}. Novos produtos/atualizações: ${baixados}.`);
    } catch (err: any) {
      console.error(err);
      setSyncMessage(`⛔ Falha na sincronização: ${err.message || err}`);
    } finally {
      setSyncing(false);
    }
  };

  const handleNovaVendaOffline = async () => {
    if (!caixaAtivo) {
      setVendaFeedback("⛔ Não há nenhum caixa aberto. Por favor, abra um caixa primeiro.");
      return;
    }

    if (!selectedProdutoId) {
      setVendaFeedback("⛔ Por favor, selecione um produto.");
      return;
    }

    const qtd = parseFloat(quantidadeVenda);
    if (isNaN(qtd) || qtd <= 0) {
      setVendaFeedback("⛔ Quantidade inválida.");
      return;
    }

    setLoading(true);
    setVendaFeedback("");
    try {
      const db = await getDbConnection();
      
      // Buscar produto selecionado no SQLite local
      const prod: any = await db.getFirstAsync('SELECT * FROM produtos WHERE id = ?', [selectedProdutoId]);
      if (!prod) {
        setVendaFeedback("⛔ Produto não encontrado localmente.");
        return;
      }

      if (prod.saldo < qtd) {
        setVendaFeedback(`⛔ Estoque insuficiente offline. Disponível: ${prod.saldo}`);
        return;
      }

      const totalVenda = prod.preco * qtd;
      const vendaId = Math.random().toString(36).substring(7);
      const syncId = Math.random().toString(36).substring(7);
      const caixaId = caixaAtivo.id;

      // Registrar a venda offline
      await registrarVendaOffline(vendaId, syncId, caixaId, totalVenda);

      // Reservar o estoque deduzindo localmente
      await db.runAsync('UPDATE produtos SET saldo = saldo - ? WHERE id = ?', [qtd, selectedProdutoId]);

      // Atualizar interface
      await atualizarContadorVendas();
      await carregarProdutosLocais();
      await calcularSaldoAtual(caixaId, caixaAtivo.saldo_abertura);

      setVendaFeedback(`✅ Venda offline de R$ ${totalVenda.toFixed(2)} registrada!`);
    } catch (err) {
      console.error("Erro ao registrar venda offline:", err);
      setVendaFeedback("⛔ Erro interno ao gravar venda local.");
    } finally {
      setLoading(false);
    }
  };

  const handleTestarDesconto = () => {
    const descValue = parseFloat(percentualDesconto);
    if (isNaN(descValue)) {
      setMensagemDesconto("Por favor, digite um percentual válido.");
      return;
    }

    if (descValue > userPerfil.limiteDesconto) {
      setMensagemDesconto(`⛔ Acesso Negado: O desconto solicitado (${descValue}%) excede o limite do seu perfil (${userPerfil.limiteDesconto}%).`);
    } else {
      setMensagemDesconto(`✅ Acesso Autorizado: Desconto de ${descValue}% aplicado com sucesso!`);
    }
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <StatusBar style="light" />
      <ScrollView contentContainerStyle={styles.scrollContainer}>
        
        {/* Glow Orbs decorativos para efeito premium (Dark mode) */}
        <View style={[styles.glowOrb, styles.glowPurple]} />
        <View style={[styles.glowOrb, styles.glowCyan]} />

        {/* Header da Aplicação */}
        <View style={styles.header}>
          <Text style={styles.brandTitle}>Ep<Text style={styles.brandItalic}>ros</Text></Text>
          <Text style={styles.brandSubtitle}>MOBILE ENGINE</Text>
        </View>

        {/* Card do Tenant e Perfil de Usuário (ABAC) */}
        <View style={styles.card}>
          <Text style={styles.cardTitle}>👤 Perfil de Acesso (ABAC)</Text>
          
          <View style={styles.profileRow}>
            <Text style={styles.profileLabel}>Usuário:</Text>
            <Text style={styles.profileValue}>{userPerfil.nome}</Text>
          </View>
          <View style={styles.profileRow}>
            <Text style={styles.profileLabel}>Cargo/Dep:</Text>
            <Text style={styles.profileValue}>{userPerfil.cargo} ({userPerfil.departamento})</Text>
          </View>
          <View style={styles.profileRow}>
            <Text style={styles.profileLabel}>Inquilino:</Text>
            <Text style={styles.profileValue}>{userPerfil.tenant}</Text>
          </View>
          <View style={styles.profileRow}>
            <Text style={styles.profileLabel}>Lim. Desconto:</Text>
            <Text style={styles.profileValueHighlight}>{userPerfil.limiteDesconto}%</Text>
          </View>
        </View>

        {/* Card da Validação Dinâmica do Limite de Desconto (ABAC) */}
        <View style={styles.card}>
          <Text style={styles.cardTitle}>🔒 Testar Regra de Desconto</Text>
          <Text style={styles.cardText}>
            Digite um valor de desconto para simular o comportamento da política ABAC baseada no seu limite permitido:
          </Text>
          
          <View style={styles.inputContainer}>
            <TextInput 
              style={styles.textInput}
              value={percentualDesconto}
              onChangeText={setPercentualDesconto}
              keyboardType="numeric"
              placeholderTextColor="rgba(255,255,255,0.3)"
              placeholder="Ex: 5"
            />
            <TouchableOpacity style={styles.btnValidate} onPress={handleTestarDesconto}>
              <Text style={styles.btnValidateText}>Testar</Text>
            </TouchableOpacity>
          </View>

          {mensagemDesconto ? (
            <Text style={[
              styles.messageText,
              mensagemDesconto.startsWith('⛔') ? styles.messageDenied : styles.messageAllowed
            ]}>
              {mensagemDesconto}
            </Text>
          ) : null}
        </View>

        {/* Card do Motor de Sincronização */}
        <View style={styles.card}>
          <Text style={styles.cardTitle}>⚡ Sincronização & Rede</Text>
          
          <View style={styles.fieldGroup}>
            <Text style={styles.fieldLabel}>URL da API Backend:</Text>
            <TextInput 
              style={styles.textInput}
              value={apiUrl}
              onChangeText={setApiUrl}
              placeholder="http://192.168.1.50:5000"
              placeholderTextColor="rgba(255,255,255,0.3)"
            />
          </View>

          <View style={styles.fieldGroup}>
            <Text style={styles.fieldLabel}>Tenant ID:</Text>
            <TextInput 
              style={styles.textInput}
              value={tenantId}
              onChangeText={setTenantId}
              placeholder="tenantId"
              placeholderTextColor="rgba(255,255,255,0.3)"
            />
          </View>

          <TouchableOpacity 
            style={[styles.btnSync, syncing && styles.btnDisabled]} 
            onPress={handleSync}
            disabled={syncing}
          >
            {syncing ? (
              <ActivityIndicator color="#FFF" />
            ) : (
              <Text style={styles.btnActionText}>🔄 Sincronizar com o Servidor</Text>
            )}
          </TouchableOpacity>

          {syncMessage ? (
            <Text style={[
              styles.messageText,
              syncMessage.startsWith('⛔') ? styles.messageDenied : styles.messageAllowed
            ]}>
              {syncMessage}
            </Text>
          ) : null}
        </View>

        {/* Card do Status SQLite e Sync Offline */}
        <View style={styles.card}>
          <Text style={styles.cardTitle}>💾 Motor SQLite Offline</Text>
          
          <View style={styles.statusContainer}>
            <Text style={styles.statusLabel}>Status do Banco:</Text>
            <Text style={[styles.statusValue, dbReady ? styles.statusSuccess : styles.statusPending]}>
              {dbReady ? "🟢 Conectado" : "🟡 Inicializando..."}
            </Text>
          </View>

          <View style={styles.syncContainer}>
            <Text style={styles.syncLabel}>Vendas Pendentes de Sync:</Text>
            <Text style={styles.syncValue}>{vendasCount}</Text>
          </View>
        </View>

        {/* Card de Turno de Caixa (Frente de Caixa) */}
        <View style={styles.card}>
          <Text style={styles.cardTitle}>🏪 Turno de Caixa (Frente de Caixa)</Text>
          
          {!caixaAtivo ? (
            <View>
              <Text style={styles.cardText}>
                O caixa está atualmente fechado. Informe o saldo inicial para abrir um novo turno e liberar as vendas:
              </Text>
              <View style={styles.fieldGroup}>
                <Text style={styles.fieldLabel}>Saldo de Abertura (R$):</Text>
                <TextInput 
                  style={styles.textInput}
                  value={saldoAberturaInput}
                  onChangeText={setSaldoAberturaInput}
                  keyboardType="numeric"
                  placeholderTextColor="rgba(255,255,255,0.3)"
                />
              </View>
              <TouchableOpacity 
                style={[styles.btnAction, styles.btnOpenCaixa, !dbReady && styles.btnDisabled]} 
                onPress={handleAbrirCaixa}
                disabled={!dbReady || loading}
              >
                <Text style={styles.btnActionText}>🔓 Abrir Turno de Caixa</Text>
              </TouchableOpacity>
            </View>
          ) : (
            <View>
              {/* Painel do Saldo Atualizado */}
              <View style={styles.balancePanel}>
                <Text style={styles.balanceLabel}>Saldo em Caixa Atual:</Text>
                <Text style={styles.balanceValue}>R$ {saldoAtual.toFixed(2)}</Text>
                <View style={styles.balanceBreakdown}>
                  <Text style={styles.balanceSubtext}>Abertura: R$ {caixaAtivo.saldo_abertura.toFixed(2)}</Text>
                  <Text style={styles.balanceSubtext}>Operador: {caixaAtivo.operador_id}</Text>
                </View>
              </View>

              {/* Seção de Sangria/Suprimento */}
              <Text style={styles.sectionDivider}>💸 Movimentação Avulsa (Sangria / Suprimento)</Text>
              
              <View style={styles.tabContainer}>
                <TouchableOpacity 
                  style={[styles.tabButton, tipoMovimento === 'Suprimento' && styles.tabButtonActive]}
                  onPress={() => setTipoMovimento('Suprimento')}
                >
                  <Text style={[styles.tabText, tipoMovimento === 'Suprimento' && styles.tabTextActive]}>
                    📥 Suprimento
                  </Text>
                </TouchableOpacity>
                <TouchableOpacity 
                  style={[styles.tabButton, tipoMovimento === 'Sangria' && styles.tabButtonActive]}
                  onPress={() => setTipoMovimento('Sangria')}
                >
                  <Text style={[styles.tabText, tipoMovimento === 'Sangria' && styles.tabTextActive]}>
                    📤 Sangria
                  </Text>
                </TouchableOpacity>
              </View>

              <View style={styles.fieldGroup}>
                <Text style={styles.fieldLabel}>Valor (R$):</Text>
                <TextInput 
                  style={styles.textInput}
                  value={valorMovimento}
                  onChangeText={setValorMovimento}
                  keyboardType="numeric"
                  placeholder="Ex: 50.00"
                  placeholderTextColor="rgba(255,255,255,0.3)"
                />
              </View>

              <View style={styles.fieldGroup}>
                <Text style={styles.fieldLabel}>Observação / Descrição:</Text>
                <TextInput 
                  style={styles.textInput}
                  value={obsMovimento}
                  onChangeText={setObsMovimento}
                  placeholder="Ex: Troco inicial extra, retirada p/ banco"
                  placeholderTextColor="rgba(255,255,255,0.3)"
                />
              </View>

              <TouchableOpacity 
                style={[styles.btnAction, styles.btnMovement, loading && styles.btnDisabled]} 
                onPress={handleRegistrarMovimento}
                disabled={loading}
              >
                <Text style={styles.btnActionText}>⚡ Registrar Movimentação</Text>
              </TouchableOpacity>

              {/* Seção de Fechamento de Caixa */}
              <Text style={styles.sectionDivider}>🔒 Fechamento de Caixa</Text>

              <View style={styles.fieldGroup}>
                <Text style={styles.fieldLabel}>Saldo Final Informado (R$):</Text>
                <TextInput 
                  style={styles.textInput}
                  value={saldoFechamentoInput}
                  onChangeText={setSaldoFechamentoInput}
                  keyboardType="numeric"
                  placeholder="Ex: 150.00"
                  placeholderTextColor="rgba(255,255,255,0.3)"
                />
              </View>

              <TouchableOpacity 
                style={[styles.btnAction, styles.btnCloseCaixa, loading && styles.btnDisabled]} 
                onPress={handleFecharCaixa}
                disabled={loading}
              >
                <Text style={styles.btnActionText}>🔒 Fechar Turno de Caixa</Text>
              </TouchableOpacity>
            </View>
          )}

          {caixaFeedback ? (
            <Text style={[
              styles.messageText,
              caixaFeedback.startsWith('⛔') ? styles.messageDenied : styles.messageAllowed
            ]}>
              {caixaFeedback}
            </Text>
          ) : null}
        </View>

        {/* Card de Nova Venda Offline */}
        <View style={styles.card}>
          <Text style={styles.cardTitle}>🛒 Lançar Venda Offline (PDV)</Text>

          {produtos.length === 0 ? (
            <Text style={styles.emptyText}>⚠️ Nenhum produto disponível localmente. Por favor, sincronize com o servidor primeiro.</Text>
          ) : (
            <View>
              <Text style={styles.fieldLabel}>Selecione o Produto:</Text>
              <View style={styles.productList}>
                {produtos.map(p => (
                  <TouchableOpacity 
                    key={p.id} 
                    style={[styles.productItem, selectedProdutoId === p.id && styles.productItemActive]}
                    onPress={() => setSelectedProdutoId(p.id)}
                  >
                    <Text style={styles.productName}>{p.nome}</Text>
                    <Text style={styles.productDetails}>
                      Preço: R$ {p.preco.toFixed(2)} | EAN/SKU: {p.ean || 'N/D'}
                    </Text>
                    <Text style={styles.productStock}>
                      Estoque Local: {p.saldo} un
                    </Text>
                  </TouchableOpacity>
                ))}
              </View>

              <View style={styles.fieldGroup}>
                <Text style={styles.fieldLabel}>Quantidade:</Text>
                <TextInput 
                  style={styles.textInput}
                  value={quantidadeVenda}
                  onChangeText={setQuantidadeVenda}
                  keyboardType="numeric"
                  placeholderTextColor="rgba(255,255,255,0.3)"
                />
              </View>

              <TouchableOpacity 
                style={[styles.btnAction, !dbReady && styles.btnDisabled]} 
                onPress={handleNovaVendaOffline}
                disabled={!dbReady || loading}
              >
                {loading ? (
                  <ActivityIndicator color="#FFF" />
                ) : (
                  <Text style={styles.btnActionText}>💸 Registrar Venda Offline</Text>
                )}
              </TouchableOpacity>
            </View>
          )}

          {vendaFeedback ? (
            <Text style={[
              styles.messageText,
              vendaFeedback.startsWith('⛔') ? styles.messageDenied : styles.messageAllowed
            ]}>
              {vendaFeedback}
            </Text>
          ) : null}
        </View>

      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: '#0D0F12',
  },
  scrollContainer: {
    padding: 20,
    alignItems: 'center',
  },
  glowOrb: {
    position: 'absolute',
    width: 250,
    height: 250,
    borderRadius: 125,
    opacity: 0.15,
  },
  glowPurple: {
    top: -50,
    left: -50,
    backgroundColor: '#9F55FF',
  },
  glowCyan: {
    bottom: 50,
    right: -50,
    backgroundColor: '#00F2FE',
  },
  header: {
    marginTop: 40,
    marginBottom: 30,
    alignItems: 'center',
  },
  brandTitle: {
    fontFamily: 'System',
    fontSize: 48,
    fontWeight: 'bold',
    color: '#FFFFFF',
    letterSpacing: 2,
  },
  brandItalic: {
    fontStyle: 'italic',
    color: '#00F2FE',
  },
  brandSubtitle: {
    fontSize: 10,
    fontWeight: 'bold',
    color: 'rgba(255, 255, 255, 0.4)',
    letterSpacing: 4,
    marginTop: 5,
  },
  card: {
    width: '100%',
    backgroundColor: 'rgba(255, 255, 255, 0.04)',
    borderColor: 'rgba(255, 255, 255, 0.08)',
    borderWidth: 1,
    borderRadius: 16,
    padding: 20,
    marginBottom: 20,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 10 },
    shadowOpacity: 0.3,
    shadowRadius: 20,
  },
  cardTitle: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#FFFFFF',
    marginBottom: 15,
  },
  cardText: {
    fontSize: 13,
    color: 'rgba(255,255,255,0.7)',
    lineHeight: 18,
    marginBottom: 15,
  },
  profileRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    paddingVertical: 8,
    borderBottomWidth: 1,
    borderBottomColor: 'rgba(255,255,255,0.05)',
  },
  profileLabel: {
    fontSize: 13,
    color: 'rgba(255, 255, 255, 0.5)',
  },
  profileValue: {
    fontSize: 13,
    color: '#FFFFFF',
    fontWeight: '500',
  },
  profileValueHighlight: {
    fontSize: 13,
    color: '#FFB900',
    fontWeight: 'bold',
  },
  inputContainer: {
    flexDirection: 'row',
    gap: 10,
  },
  fieldGroup: {
    width: '100%',
    marginBottom: 15,
  },
  fieldLabel: {
    fontSize: 12,
    color: 'rgba(255,255,255,0.6)',
    marginBottom: 6,
  },
  textInput: {
    width: '100%',
    height: 48,
    backgroundColor: 'rgba(255, 255, 255, 0.05)',
    borderColor: 'rgba(255, 255, 255, 0.1)',
    borderWidth: 1,
    borderRadius: 8,
    color: '#FFFFFF',
    paddingHorizontal: 16,
    fontSize: 16,
  },
  btnValidate: {
    width: 90,
    height: 48,
    backgroundColor: '#3B82F6',
    borderRadius: 8,
    justifyContent: 'center',
    alignItems: 'center',
  },
  btnValidateText: {
    color: '#FFFFFF',
    fontWeight: 'bold',
    fontSize: 14,
  },
  btnSync: {
    width: '100%',
    height: 48,
    backgroundColor: '#8B5CF6',
    borderRadius: 8,
    justifyContent: 'center',
    alignItems: 'center',
    marginTop: 5,
  },
  messageText: {
    marginTop: 15,
    fontSize: 13,
    fontWeight: '600',
    lineHeight: 18,
    padding: 12,
    borderRadius: 8,
    overflow: 'hidden',
  },
  messageDenied: {
    backgroundColor: 'rgba(239, 68, 68, 0.1)',
    color: '#EF4444',
  },
  messageAllowed: {
    backgroundColor: 'rgba(16, 185, 129, 0.1)',
    color: '#10B981',
  },
  statusContainer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: 15,
  },
  statusLabel: {
    fontSize: 13,
    color: 'rgba(255, 255, 255, 0.5)',
  },
  statusValue: {
    fontSize: 13,
    fontWeight: 'bold',
  },
  statusSuccess: {
    color: '#10B981',
  },
  statusPending: {
    color: '#FFB900',
  },
  syncContainer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    backgroundColor: 'rgba(255, 255, 255, 0.02)',
    padding: 12,
    borderRadius: 8,
  },
  syncLabel: {
    fontSize: 13,
    color: '#FFFFFF',
  },
  syncValue: {
    fontSize: 22,
    fontWeight: 'bold',
    color: '#00F2FE',
  },
  emptyText: {
    fontSize: 13,
    color: '#FFB900',
    lineHeight: 18,
    textAlign: 'center',
    paddingVertical: 10,
  },
  productList: {
    width: '100%',
    marginBottom: 15,
    gap: 8,
  },
  productItem: {
    width: '100%',
    backgroundColor: 'rgba(255, 255, 255, 0.02)',
    borderColor: 'rgba(255, 255, 255, 0.05)',
    borderWidth: 1,
    borderRadius: 8,
    padding: 12,
  },
  productItemActive: {
    backgroundColor: 'rgba(0, 242, 254, 0.05)',
    borderColor: '#00F2FE',
  },
  productName: {
    fontSize: 14,
    fontWeight: 'bold',
    color: '#FFFFFF',
  },
  productDetails: {
    fontSize: 11,
    color: 'rgba(255,255,255,0.6)',
    marginTop: 4,
  },
  productStock: {
    fontSize: 11,
    color: '#10B981',
    fontWeight: '600',
    marginTop: 2,
  },
  btnAction: {
    width: '100%',
    height: 48,
    backgroundColor: '#10B981',
    borderRadius: 8,
    justifyContent: 'center',
    alignItems: 'center',
    marginTop: 5,
  },
  btnActionText: {
    color: '#FFFFFF',
    fontWeight: 'bold',
    fontSize: 15,
  },
  btnDisabled: {
    opacity: 0.5,
  },
  btnOpenCaixa: {
    backgroundColor: '#3B82F6',
  },
  btnCloseCaixa: {
    backgroundColor: '#EF4444',
  },
  btnMovement: {
    backgroundColor: '#10B981',
  },
  balancePanel: {
    backgroundColor: 'rgba(0, 242, 254, 0.05)',
    borderColor: 'rgba(0, 242, 254, 0.2)',
    borderWidth: 1,
    borderRadius: 12,
    padding: 16,
    alignItems: 'center',
    marginBottom: 20,
  },
  balanceLabel: {
    fontSize: 12,
    color: 'rgba(255,255,255,0.6)',
    textTransform: 'uppercase',
    letterSpacing: 1,
    marginBottom: 4,
  },
  balanceValue: {
    fontSize: 32,
    fontWeight: 'bold',
    color: '#00F2FE',
  },
  balanceBreakdown: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    width: '100%',
    marginTop: 12,
    borderTopWidth: 1,
    borderTopColor: 'rgba(255,255,255,0.05)',
    paddingTop: 8,
  },
  balanceSubtext: {
    fontSize: 10,
    color: 'rgba(255,255,255,0.4)',
  },
  sectionDivider: {
    fontSize: 13,
    fontWeight: 'bold',
    color: 'rgba(255,255,255,0.7)',
    marginTop: 20,
    marginBottom: 12,
    borderBottomWidth: 1,
    borderBottomColor: 'rgba(255,255,255,0.1)',
    paddingBottom: 6,
  },
  tabContainer: {
    flexDirection: 'row',
    backgroundColor: 'rgba(255,255,255,0.03)',
    borderRadius: 8,
    padding: 4,
    marginBottom: 15,
    gap: 4,
  },
  tabButton: {
    flex: 1,
    paddingVertical: 10,
    alignItems: 'center',
    borderRadius: 6,
  },
  tabButtonActive: {
    backgroundColor: 'rgba(255,255,255,0.07)',
  },
  tabText: {
    fontSize: 12,
    color: 'rgba(255,255,255,0.5)',
    fontWeight: '600',
  },
  tabTextActive: {
    color: '#FFFFFF',
  },
});
