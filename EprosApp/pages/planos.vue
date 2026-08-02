<template>
  <div class="pricing-page">
    <header class="pricing-header">
      <h1>Planos Epros</h1>
      <p>Escolha o plano ideal para a sua empresa. Sem fidelidade — faça upgrade quando quiser.</p>
    </header>

    <div v-if="loading" class="pricing-loading">Carregando planos…</div>
    <div v-else-if="!planos.length" class="pricing-empty">Nenhum plano disponível no momento.</div>

    <div v-else class="pricing-grid">
      <div v-for="p in planos" :key="p.id" class="plan-card" :class="{ destaque: p.destaque }">
        <div v-if="p.destaque" class="plan-flag">Mais popular</div>
        <h2 class="plan-name">{{ p.nome }}</h2>
        <p v-if="p.descricaoCurta" class="plan-desc">{{ p.descricaoCurta }}</p>
        <div class="plan-price">
          <span class="price-value">{{ formatMoney(p.valor) }}</span>
          <span class="price-cycle">/ {{ cicloLabel(p.duration) }}</span>
        </div>
        <ul class="plan-features">
          <li v-if="p.limiteUsuarios">{{ p.limiteUsuarios }} usuários</li>
          <li v-if="p.limiteEmpresas">{{ p.limiteEmpresas }} empresas</li>
          <li v-if="p.moduloCrm">CRM</li>
          <li v-if="p.moduloProjetos">Projetos</li>
          <li v-if="p.moduloRh">RH</li>
          <li v-if="p.moduloFinanceiro">Financeiro</li>
          <li v-if="p.moduloPdv">PDV</li>
        </ul>
        <p v-if="p.descricaoCompleta" class="plan-longdesc">{{ p.descricaoCompleta }}</p>
        <NuxtLink :to="`/cadastro?plano=${p.id}`" class="plan-cta">Contratar</NuxtLink>
      </div>
    </div>
  </div>
</template>

<script setup>
definePageMeta({ layout: false })

const loading = ref(true)
const planos = ref([])

const formatMoney = (v) => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(Number(v || 0))
const cicloLabel = (d) => ({ Mensal: 'mês', Anual: 'ano', Vitalicia: 'única' }[d] || 'mês')

onMounted(async () => {
  try {
    const res = await useApi('/public/AreaPublica/planos')
    planos.value = Array.isArray(res) ? res : (res?.dados ?? res?.items ?? res?.data ?? [])
  } catch (e) {
    planos.value = []
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.pricing-page { max-width: 1100px; margin: 0 auto; padding: 48px 20px; }
.pricing-header { text-align: center; margin-bottom: 40px; }
.pricing-header h1 { font-size: 2.2rem; margin: 0 0 8px; }
.pricing-header p { color: #64748b; }
.pricing-loading, .pricing-empty { text-align: center; color: #64748b; padding: 40px; }
.pricing-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(260px, 1fr)); gap: 20px; }
.plan-card { position: relative; border: 1px solid #e2e8f0; border-radius: 16px; padding: 28px 22px; background: #fff; display: flex; flex-direction: column; }
.plan-card.destaque { border-color: #6366f1; box-shadow: 0 8px 30px rgba(99,102,241,.15); }
.plan-flag { position: absolute; top: -12px; left: 50%; transform: translateX(-50%); background: #6366f1; color: #fff; font-size: 12px; font-weight: 600; padding: 4px 14px; border-radius: 12px; }
.plan-name { font-size: 1.3rem; margin: 0 0 6px; }
.plan-desc { color: #64748b; font-size: 14px; min-height: 20px; }
.plan-price { margin: 18px 0; }
.price-value { font-size: 2rem; font-weight: 700; }
.price-cycle { color: #94a3b8; }
.plan-features { list-style: none; padding: 0; margin: 0 0 16px; }
.plan-features li { padding: 6px 0; border-bottom: 1px solid #f1f5f9; font-size: 14px; }
.plan-features li::before { content: '✓'; color: #16a34a; font-weight: 700; margin-right: 8px; }
.plan-longdesc { color: #64748b; font-size: 13px; margin-bottom: 16px; }
.plan-cta { margin-top: auto; display: block; text-align: center; background: #6366f1; color: #fff; padding: 12px; border-radius: 10px; text-decoration: none; font-weight: 600; }
.plan-cta:hover { background: #4f46e5; }
</style>
