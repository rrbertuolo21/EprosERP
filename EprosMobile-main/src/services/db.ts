import * as SQLite from 'expo-sqlite';

// Abre ou cria o banco de dados SQLite local
export const getDbConnection = async () => {
  return SQLite.openDatabaseAsync('epros_local.db');
};

// Inicializa a estrutura de tabelas offline para o modo contingência/PDV
export const inicializarBancoLocal = async () => {
  const db = await getDbConnection();
  
  // Criação das tabelas essenciais para o funcionamento offline
  await db.execAsync(`
    PRAGMA foreign_keys = ON;

    CREATE TABLE IF NOT EXISTS produtos (
      id TEXT PRIMARY KEY NOT NULL,
      sync_id TEXT UNIQUE NOT NULL,
      nome TEXT NOT NULL,
      preco REAL NOT NULL,
      saldo INTEGER NOT NULL,
      ean TEXT,
      alterado_em TEXT NOT NULL
    );

    CREATE TABLE IF NOT EXISTS caixas (
      id TEXT PRIMARY KEY NOT NULL,
      sync_id TEXT UNIQUE NOT NULL,
      operador_id TEXT NOT NULL,
      status TEXT NOT NULL, -- 'Aberto', 'Fechado'
      saldo_abertura REAL NOT NULL,
      saldo_fechamento REAL,
      criado_em TEXT NOT NULL,
      enviado INTEGER DEFAULT 0
    );

    CREATE TABLE IF NOT EXISTS vendas (
      id TEXT PRIMARY KEY NOT NULL,
      sync_id TEXT UNIQUE NOT NULL,
      caixa_id TEXT NOT NULL,
      total REAL NOT NULL,
      status TEXT NOT NULL, -- 'Emitida', 'Contingencia', 'Cancelada'
      enviado INTEGER DEFAULT 0, -- 0 = Não enviado ao servidor, 1 = Enviado
      criado_em TEXT NOT NULL,
      FOREIGN KEY (caixa_id) REFERENCES caixas(id) ON DELETE CASCADE
    );

    CREATE TABLE IF NOT EXISTS caixa_movimentos (
      id TEXT PRIMARY KEY NOT NULL,
      sync_id TEXT UNIQUE NOT NULL,
      caixa_id TEXT NOT NULL,
      tipo TEXT NOT NULL, -- 'Suprimento', 'Sangria'
      valor REAL NOT NULL,
      observacao TEXT,
      criado_em TEXT NOT NULL,
      enviado INTEGER DEFAULT 0,
      FOREIGN KEY (caixa_id) REFERENCES caixas(id) ON DELETE CASCADE
    );
  `);

  // Migração segura para adicionar coluna 'enviado' à tabela 'caixas' se ela já existir sem ela
  try {
    await db.execAsync('ALTER TABLE caixas ADD COLUMN enviado INTEGER DEFAULT 0;');
  } catch (e) {
    // A coluna já existe ou o banco está sendo criado do zero, ignora o erro
  }

  console.log('Banco de dados SQLite offline inicializado com sucesso.');
};

// Salvar produto sincronizado
export const salvarProdutosOffline = async (produtos: any[]) => {
  const db = await getDbConnection();
  
  for (const produto of produtos) {
    await db.runAsync(
      `INSERT OR REPLACE INTO produtos (id, sync_id, nome, preco, saldo, ean, alterado_em) 
       VALUES (?, ?, ?, ?, ?, ?, ?)`,
      [produto.id, produto.syncId, produto.nome, Math.round(produto.preco * 100) / 100, produto.saldo, produto.ean, produto.alteradoEm]
    );
  }
};

// Registrar venda offline
export const registrarVendaOffline = async (vendaId: string, syncId: string, caixaId: string, total: number) => {
  const db = await getDbConnection();
  const criadoEm = new Date().toISOString();
  
  await db.runAsync(
    `INSERT INTO vendas (id, sync_id, caixa_id, total, status, enviado, criado_em) 
     VALUES (?, ?, ?, ?, 'Contingencia', 0, ?)`,
    [vendaId, syncId, caixaId, Math.round(total * 100) / 100, criadoEm]
  );
};

// Abrir Caixa local
export const abrirCaixaLocal = async (id: string, operadorId: string, saldoAbertura: number) => {
  const db = await getDbConnection();
  const criadoEm = new Date().toISOString();

  await db.runAsync(
    `INSERT INTO caixas (id, sync_id, operador_id, status, saldo_abertura, criado_em, enviado)
     VALUES (?, ?, ?, 'Aberto', ?, ?, 0)`,
    [id, id, operadorId, saldoAbertura, criadoEm]
  );
};

// Fechar Caixa local
export const fecharCaixaLocal = async (id: string, saldoFechamento: number) => {
  const db = await getDbConnection();

  await db.runAsync(
    `UPDATE caixas 
     SET status = 'Fechado', saldo_fechamento = ?, enviado = 0
     WHERE id = ?`,
    [saldoFechamento, id]
  );
};

// Registrar Movimento de Caixa local
export const registrarMovimentoCaixaLocal = async (id: string, syncId: string, caixaId: string, tipo: string, valor: number, observacao: string) => {
  const db = await getDbConnection();
  const criadoEm = new Date().toISOString();

  await db.runAsync(
    `INSERT INTO caixa_movimentos (id, sync_id, caixa_id, tipo, valor, observacao, criado_em, enviado)
     VALUES (?, ?, ?, ?, ?, ?, ?, 0)`,
    [id, syncId, caixaId, tipo, valor, observacao, criadoEm]
  );
};
