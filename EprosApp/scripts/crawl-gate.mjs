// Portão de navegação — crawl automatizado do EprosApp rodando.
// Loga, descobre as rotas do filesystem (pages/**), visita cada uma e coleta
// erro de console / pageerror / rede >=400 / página de erro do Nuxt.
// Uso:  node scripts/crawl-gate.mjs            (crawl completo, gera relatório JSON)
//       BASE=http://localhost:3000 EMAIL=... SENHA=... node scripts/crawl-gate.mjs
// Saída: escreve crawl-report.json e imprime resumo; exit code = nº de rotas quebradas (0 = verde).
import { chromium } from 'playwright'
import { readdirSync, statSync, writeFileSync } from 'node:fs'
import { join } from 'node:path'

const BASE = process.env.BASE || 'http://localhost:3000'
const EMAIL = process.env.EMAIL || 'admin@teste-a.com.br'
const SENHA = process.env.SENHA || 'Epros@Validacao#2026'
const OUT = process.env.OUT || 'crawl-report.json'
const ONLY = process.env.ONLY // substring p/ filtrar rotas (debug)
const PAGES = 'pages'

// --- descobre rotas estáticas a partir de pages/**.vue (pula dinâmicas [id]) ---
function walk(dir, acc = []) {
  for (const name of readdirSync(dir)) {
    const p = join(dir, name)
    if (statSync(p).isDirectory()) walk(p, acc)
    else if (name.endsWith('.vue')) acc.push(p)
  }
  return acc
}
function fileToRoute(f) {
  let r = f.slice(PAGES.length).replace(/\.vue$/, '')
  if (r.endsWith('/index')) r = r.slice(0, -('/index'.length)) || '/'
  return r
}
const allFiles = walk(PAGES)
let routes = [...new Set(allFiles.map(fileToRoute))]
  .filter((r) => !r.includes('[')) // dinâmicas precisam de id/seed — fora do baseline
  .filter((r) => r.startsWith('/erp') || r === '/') // foca no ERP autenticado
  .sort()
if (ONLY) routes = routes.filter((r) => r.includes(ONLY))

const IGNORE_NET = [/\/api\/v1\/menu$/] // ruído conhecido: menu é refetch em cada nav
const isErrorConsole = (m) =>
  m.type() === 'error' &&
  !/Failed to load resource/.test(m.text()) && // ruído de 404 de asset já contado na rede
  !/\[nuxt\] error caught during app initialization Error: Page not found/.test(m.text()) // resíduo de nav anterior

async function login(page) {
  await page.goto(BASE + '/', { waitUntil: 'networkidle', timeout: 30000 })
  await page.fill('input[type=email]', EMAIL)
  await page.fill('input[type=password]', SENHA)
  await Promise.all([
    page.waitForURL((u) => !/\/(login)?$/.test(u.pathname) || u.pathname.startsWith('/erp'), { timeout: 30000 }).catch(() => {}),
    page.click('button[type=submit]')
  ])
  await page.waitForTimeout(1500)
  const url = page.url()
  if (/\/$/.test(new URL(url).pathname) && !/erp/.test(url)) {
    // ainda na home de login? tenta detectar sessão via localStorage
    const hasToken = await page.evaluate(() =>
      Object.keys(localStorage).some((k) => /token|auth/i.test(k) && (localStorage.getItem(k) || '').length > 20)
    )
    if (!hasToken) throw new Error('login falhou — sem token após submit')
  }
}

const results = []
const browser = await chromium.launch({ headless: true })
const ctx = await browser.newContext({ ignoreHTTPSErrors: true })
const page = await ctx.newPage()

try {
  await login(page)
  console.error(`login OK · ${routes.length} rotas a visitar`)
} catch (e) {
  console.error('ERRO no login:', e.message)
  await browser.close()
  process.exit(2)
}

for (const route of routes) {
  const consoleErrors = []
  const pageErrors = []
  const netErrors = []
  const onConsole = (m) => { if (isErrorConsole(m)) consoleErrors.push(m.text().slice(0, 300)) }
  const onPageError = (e) => pageErrors.push(String(e).slice(0, 300))
  const onResponse = (r) => {
    const s = r.status()
    const u = r.url()
    if (s >= 400 && u.includes('/api/') && !IGNORE_NET.some((rx) => rx.test(u)))
      netErrors.push(`${r.request().method()} ${u.replace(/https?:\/\/[^/]+/, '')} → ${s}`)
  }
  page.on('console', onConsole)
  page.on('pageerror', onPageError)
  page.on('response', onResponse)
  let errorPage = false
  try {
    await page.goto(BASE + route, { waitUntil: 'networkidle', timeout: 25000 })
    await page.waitForTimeout(600)
    const title = await page.title().catch(() => '')
    const bodyTxt = await page.evaluate(() => document.body?.innerText?.slice(0, 400) || '').catch(() => '')
    errorPage = /Page not found|error-500|Algo deu errado|Não foi possível carregar/i.test(title + '\n' + bodyTxt)
  } catch (e) {
    pageErrors.push('goto: ' + String(e.message).slice(0, 200))
  }
  page.off('console', onConsole)
  page.off('pageerror', onPageError)
  page.off('response', onResponse)
  const broken = consoleErrors.length || pageErrors.length || netErrors.length || errorPage
  results.push({ route, broken: !!broken, errorPage, consoleErrors, pageErrors, netErrors })
  if (broken) console.error(`  ✗ ${route}  console:${consoleErrors.length} pageerr:${pageErrors.length} net:${netErrors.length}${errorPage ? ' [errorPage]' : ''}`)
}

await browser.close()

const brokenList = results.filter((r) => r.broken)
const summary = {
  base: BASE,
  total: results.length,
  ok: results.length - brokenList.length,
  broken: brokenList.length,
  brokenRoutes: brokenList.map((r) => r.route)
}
writeFileSync(OUT, JSON.stringify({ summary, results }, null, 2))
console.error(`\n=== RESUMO === total:${summary.total} ok:${summary.ok} quebradas:${summary.broken}`)
console.error(`relatório: ${OUT}`)
process.exit(brokenList.length ? 1 : 0)
