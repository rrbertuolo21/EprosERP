// Seed transacional (fundação, passo 2) — cria 1–3 registros por recurso via os POSTs REAIS da API.
// Idempotente-ish: pula quando a lista já tem item. Descobre FKs (produto/fornecedor/cliente/local)
// em runtime a partir dos endpoints de lista já semeados. Re-executável.
// Uso: BASE=http://localhost:8080 node scripts/seed-demo.mjs
const BASE = process.env.BASE || 'http://localhost:8080'
const EMAIL = process.env.EMAIL || 'admin@teste-a.com.br'
const SENHA = process.env.SENHA || 'Epros@Validacao#2026'

let TOKEN = ''
const H = () => ({ 'Content-Type': 'application/json', Authorization: `Bearer ${TOKEN}` })
const log = (...a) => console.error(...a)

async function api(method, path, body) {
  const r = await fetch(BASE + path, { method, headers: H(), body: body ? JSON.stringify(body) : undefined })
  let j = null
  try { j = await r.json() } catch {}
  return { status: r.status, body: j }
}
// extrai lista tolerando {dados:{itens}} | {dados:[]} | []
function lista(resp) {
  const d = resp?.body?.dados ?? resp?.body
  if (Array.isArray(d)) return d
  if (Array.isArray(d?.itens)) return d.itens
  if (Array.isArray(d?.Itens)) return d.Itens
  return []
}
const idDe = (o) => o?.id || o?.Id || o?.pessoaId || null

async function login() {
  const r = await fetch(BASE + '/api/v1/public/auth/login', {
    method: 'POST', headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email: EMAIL, senha: SENHA })
  })
  const j = await r.json()
  TOKEN = j?.dados?.token || j?.dados?.Token
  if (!TOKEN) throw new Error('login falhou')
}

// ensure: se a lista já tem >=1, pula; senão cria e reporta
const resultados = []
async function ensure(label, listPath, createPath, payload) {
  try {
    if (listPath) {
      const jl = await api('GET', listPath)
      if (lista(jl).length > 0) { resultados.push([label, 'já-tem', '']); return lista(jl)[0] }
    }
    const r = await api('POST', createPath, payload)
    if (r.status >= 200 && r.status < 300) {
      resultados.push([label, 'CRIADO', ''])
      return r.body?.dados ?? r.body
    }
    const err = JSON.stringify(r.body?.errors || r.body?.erros || r.body?.mensagem || r.body || '').slice(0, 160)
    resultados.push([label, `falhou ${r.status}`, err])
    return null
  } catch (e) {
    resultados.push([label, 'erro', String(e.message).slice(0, 120)])
    return null
  }
}

async function main() {
  await login()
  log('login OK; descobrindo FKs...')

  // FKs de entidades já semeadas
  const produtos = lista(await api('GET', '/api/v1/estoque/produtos?pagina=1&tamanhoPagina=5'))
  const pessoas = lista(await api('GET', '/api/v1/cadastros/pessoas?pagina=1&tamanhoPagina=10'))
  const produtoId = idDe(produtos[0])
  const cliente = pessoas.find((p) => p.ehCliente) || pessoas[0]
  const fornecedor = pessoas.find((p) => p.ehFornecedor) || pessoas[1]
  const clienteId = idDe(cliente), fornecedorId = idDe(fornecedor)
  log(`  produtoId=${produtoId} clienteId=${clienteId} fornecedorId=${fornecedorId}`)

  const hoje = '2026-08-05'
  const em30 = '2026-09-05'

  // --- Qualidade / Recall (produto existe; responsavel = usuário admin) ---
  await ensure('qualidade/recall', '/api/v1/qualidade/recall?pagina=1', '/api/v1/qualidade/recall', {
    codigo: 'RCL-0001', titulo: 'Recall demonstração lote piloto', gravidade: 'Media',
    responsavelId: null, descricao: 'Campanha de recall de demonstração.', produtoId, ncrId: null
  })

  // --- Estoque / Portal Fornecedor (convite; fornecedor existe) ---
  await ensure('portal-fornecedor/convite', '/api/v1/estoque-portal-fornecedor/convites', '/api/v1/estoque-portal-fornecedor/convites', {
    fornecedorId, emailConvite: 'fornecedor.demo@teste.com.br', dataExpiracao: em30
  })

  // --- Estoque / Rastreabilidade / Lote (produto existe; local descoberto) ---
  const locais = lista(await api('GET', '/api/v1/estoque-wms-armazens'))
  const localId = idDe(locais[0])
  await ensure('rastreabilidade/lote', '/api/v1/estoque-rastreabilidade/lotes', '/api/v1/estoque-rastreabilidade/lotes', {
    empresaId: null, produtoId, codigoLote: 'LOTE-0001', quantidadeRecebida: 100,
    origem: 'Compra', localId, fichaEntradaId: null, dataFabricacao: hoje, dataValidade: '2027-08-05',
    observacao: 'Lote de demonstração.'
  })

  // --- Imobiliária / Proposta (precisa imóvel) ---
  const imoveis = lista(await api('GET', '/api/v1/imobiliaria/imoveis?pagina=1'))
  const imovelId = idDe(imoveis[0])
  if (imovelId) {
    await ensure('imobiliaria/proposta', '/api/v1/imobiliaria/propostas?pagina=1', '/api/v1/imobiliaria/propostas', {
      tipo: 'Locacao', imovelId, validade: em30, valorProposto: 2500, observacao: 'Proposta demo', contrapropostaDeId: null, partes: []
    })
  } else resultados.push(['imobiliaria/proposta', 'skip', 'sem imóvel semeado'])

  // --- Financeiro / Contas a Receber (cliente existe; natureza descoberta) ---
  const naturezas = lista(await api('GET', '/api/v1/configuracao-codigo-naturezas-financeiras?pagina=1'))
  const naturezaId = idDe(naturezas[0])
  await ensure('financeiro/conta-receber', '/api/v1/financeiro/contas-a-receber?pagina=1', '/api/v1/financeiro/contas-a-receber', {
    clienteId, naturezaFinanceiraId: naturezaId, documento: 'DOC-0001', numeroParcela: 1,
    valorTitulo: 1500, dataEmissao: hoje, dataVencimento: em30, detalhamento: 'Título demo'
  })

  // ---- relatório ----
  log('\n=== SEED — resultado ===')
  for (const [l, s, e] of resultados) log(`  ${s.padEnd(12)} ${l}${e ? '  :: ' + e : ''}`)
  const criados = resultados.filter((r) => r[1] === 'CRIADO').length
  const jatem = resultados.filter((r) => r[1] === 'já-tem').length
  const falhas = resultados.filter((r) => /falhou|erro/.test(r[1])).length
  log(`\ncriados:${criados}  já-tinham:${jatem}  falhas:${falhas}`)
}
main().catch((e) => { log('FATAL', e); process.exit(1) })
