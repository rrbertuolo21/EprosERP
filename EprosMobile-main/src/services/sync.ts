import { getDbConnection } from './db';

export const sincronizarProdutos = async (apiUrl: string, tenantId: string) => {
  const db = await getDbConnection();
  
  // Obter o timestamp da última modificação
  const result: any = await db.getFirstAsync('SELECT MAX(alterado_em) as lastSync FROM produtos');
  const lastSync = result?.lastSync || '1970-01-01T00:00:00Z';

  // Chamar API de delta de produtos
  const response = await fetch(`${apiUrl}/api/v1/estoque/produtos/sync/delta?since=${encodeURIComponent(lastSync)}`, {
    headers: {
      'Accept': 'application/json',
      'Content-Type': 'application/json',
      'X-Tenant-Id': tenantId
    }
  });

  if (!response.ok) {
    throw new Error(`Erro ao buscar produtos da API: ${response.statusText}`);
  }

  const produtos: any[] = await response.json();

  for (const prod of produtos) {
    if (prod.deletado) {
      await db.runAsync('DELETE FROM produtos WHERE id = ?', [prod.id]);
    } else {
      const alteradoEm = prod.alteradoEm || prod.criadoEm;
      await db.runAsync(
        `INSERT OR REPLACE INTO produtos (id, sync_id, nome, preco, saldo, ean, alterado_em) 
         VALUES (?, ?, ?, ?, ?, ?, ?)`,
        [prod.id, prod.syncId, prod.nome, prod.precoVenda, prod.saldoEstoque, prod.sku, alteradoEm]
      );
    }
  }

  return produtos.length;
};

export const sincronizarVendas = async (apiUrl: string, tenantId: string) => {
  const db = await getDbConnection();

  // Buscar vendas offline pendentes de envio (enviado = 0)
  const vendasPendentes: any[] = await db.getAllAsync(
    'SELECT * FROM vendas WHERE enviado = 0'
  );

  if (vendasPendentes.length === 0) {
    return 0;
  }

  const payloadVendas = vendasPendentes.map(v => ({
    id: v.id,
    syncId: v.sync_id,
    caixaId: v.caixa_id,
    total: v.total,
    status: v.status,
    criadoEm: v.criado_em,
    itens: []
  }));

  const response = await fetch(`${apiUrl}/api/v1/vendas/sync`, {
    method: 'POST',
    headers: {
      'Accept': 'application/json',
      'Content-Type': 'application/json',
      'X-Tenant-Id': tenantId
    },
    body: JSON.stringify({ vendas: payloadVendas })
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(`Erro ao enviar vendas: ${response.status} - ${errorText}`);
  }

  // Marcar como enviadas localmente no SQLite
  for (const v of vendasPendentes) {
    await db.runAsync('UPDATE vendas SET enviado = 1, status = \'Emitida\' WHERE id = ?', [v.id]);
  }

  return vendasPendentes.length;
};

export const sincronizarCaixas = async (apiUrl: string, tenantId: string) => {
  const db = await getDbConnection();

  // Buscar caixas locais pendentes ou com movimentos pendentes
  const caixasPendentes: any[] = await db.getAllAsync(`
    SELECT DISTINCT c.* FROM caixas c
    LEFT JOIN caixa_movimentos m ON c.id = m.caixa_id
    WHERE c.enviado = 0 OR m.enviado = 0
  `);

  if (caixasPendentes.length === 0) {
    return 0;
  }

  const payloadCaixas = [];

  for (const c of caixasPendentes) {
    // Buscar movimentos pendentes deste caixa
    const movimentosPendentes: any[] = await db.getAllAsync(
      'SELECT * FROM caixa_movimentos WHERE caixa_id = ? AND enviado = 0',
      [c.id]
    );

    payloadCaixas.push({
      id: c.id,
      syncId: c.sync_id,
      operadorId: c.operador_id,
      saldoAbertura: c.saldo_abertura,
      saldoFechamento: c.saldo_fechamento,
      status: c.status,
      criadoEm: c.criado_em,
      movimentos: movimentosPendentes.map(m => ({
        id: m.id,
        syncId: m.sync_id,
        tipo: m.tipo,
        valor: m.valor,
        observacao: m.observacao,
        criadoEm: m.criado_em
      }))
    });
  }

  const response = await fetch(`${apiUrl}/api/v1/vendas/caixas/sync`, {
    method: 'POST',
    headers: {
      'Accept': 'application/json',
      'Content-Type': 'application/json',
      'X-Tenant-Id': tenantId
    },
    body: JSON.stringify({ caixas: payloadCaixas })
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(`Erro ao sincronizar caixas: ${response.status} - ${errorText}`);
  }

  // Marcar caixas e movimentos específicos como enviados no SQLite
  for (const c of caixasPendentes) {
    if (c.enviado === 0) {
      await db.runAsync('UPDATE caixas SET enviado = 1 WHERE id = ?', [c.id]);
    }
  }

  for (const c of payloadCaixas) {
    for (const m of c.movimentos) {
      await db.runAsync('UPDATE caixa_movimentos SET enviado = 1 WHERE id = ?', [m.id]);
    }
  }

  return caixasPendentes.length;
};

