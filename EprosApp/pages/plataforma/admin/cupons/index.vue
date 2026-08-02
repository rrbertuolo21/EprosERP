<template>
  <div>
    <header class="page-header">
      <h1 class="glow-text">Cupons de Desconto</h1>
      <p class="tagline">Cupons globais do Siser e cupons custom por tenant.</p>
    </header>

    <section class="admin-section glass-panel mt-4">
      <header class="section-header">
        <h3>Cupons</h3>
        <button class="btn btn-primary" @click="abrirNovo">Novo cupom</button>
      </header>
      <table class="data-table">
        <thead><tr><th>Nome</th><th>Código</th><th>Tipo</th><th>Desconto</th><th>Usos</th><th>Escopo</th><th>Válido até</th><th></th></tr></thead>
        <tbody>
          <tr v-if="!cupons.length"><td colspan="8" class="empty">Nenhum cupom.</td></tr>
          <tr v-for="c in cupons" :key="c.id">
            <td>{{ c.nome }}</td>
            <td><code>{{ c.codigo }}</code></td>
            <td>{{ c.tipo }}</td>
            <td>{{ c.tipo === 'Percentual' ? c.valorDesconto + '%' : formatMoney(c.valorDesconto) }}</td>
            <td>{{ c.quantidadeUsos }}{{ c.limiteUso ? '/' + c.limiteUso : '' }}</td>
            <td><span class="badge" :class="c.global ? 'badge-global' : 'badge-custom'">{{ c.global ? 'Global' : 'Tenant' }}</span></td>
            <td>{{ c.validoAte ? formatDate(c.validoAte) : '—' }}</td>
            <td class="align-right">
              <button class="btn btn-secondary btn-table-action" @click="editar(c)">Editar</button>
              <button class="btn btn-secondary btn-table-action btn-danger-action" @click="excluir(c)">Excluir</button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>

    <div v-if="modal.open" class="modal-overlay" @click.self="modal.open = false">
      <div class="modal-card glass-panel">
        <h3>{{ modal.form.id ? 'Editar' : 'Novo' }} cupom</h3>
        <form @submit.prevent="salvar" class="vertical-form mt-2">
          <div class="form-row">
            <div class="form-group col-6"><label>Nome *</label><input v-model="modal.form.nome" required /></div>
            <div class="form-group col-6"><label>Código *</label><input v-model="modal.form.codigo" :disabled="!!modal.form.id" required /></div>
          </div>
          <div class="form-row">
            <div class="form-group col-4"><label>Tipo</label><select v-model="modal.form.tipo"><option>Fixo</option><option>Percentual</option></select></div>
            <div class="form-group col-4"><label>Valor *</label><input type="number" step="0.01" min="0" v-model.number="modal.form.valorDesconto" required /></div>
            <div class="form-group col-4"><label>Limite de uso</label><input type="number" min="0" v-model.number="modal.form.limiteUso" placeholder="ilimitado" /></div>
          </div>
          <div class="form-row"><div class="form-group col-6"><label>Válido até</label><input type="date" v-model="modal.form.validoAte" /></div></div>
          <div class="modal-actions">
            <button type="button" class="btn btn-secondary" @click="modal.open = false">Cancelar</button>
            <button type="submit" class="btn btn-primary" :disabled="saving">Salvar</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
definePageMeta({ layout: 'admin' })

const cupons = ref([])
const saving = ref(false)
const modal = reactive({ open: false, form: {} })

const formatMoney = (v) => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(Number(v || 0))
const formatDate = (d) => d ? new Date(d).toLocaleDateString('pt-BR') : '—'
const extrair = (r) => Array.isArray(r) ? r : (r?.items ?? r?.dados?.items ?? r?.dados ?? r?.data ?? [])

const carregar = async () => { try { cupons.value = extrair(await useApi('/aplicativo/cupons')) } catch { cupons.value = [] } }
onMounted(carregar)

const abrirNovo = () => { modal.form = { nome: '', codigo: '', tipo: 'Fixo', valorDesconto: 0, limiteUso: null, validoAte: null }; modal.open = true }
const editar = (c) => { modal.form = { id: c.id, nome: c.nome, codigo: c.codigo, tipo: c.tipo, valorDesconto: c.valorDesconto, limiteUso: c.limiteUso, validoAte: c.validoAte ? c.validoAte.substring(0,10) : null }; modal.open = true }

const salvar = async () => {
  saving.value = true
  try {
    const body = { Nome: modal.form.nome, Codigo: modal.form.codigo, Tipo: modal.form.tipo, ValorDesconto: modal.form.valorDesconto, LimiteUso: modal.form.limiteUso || null, ValidoAte: modal.form.validoAte || null }
    if (modal.form.id) await useApi(`/aplicativo/cupons/${modal.form.id}`, { method: 'PUT', body: { Id: modal.form.id, ...body } })
    else await useApi('/aplicativo/cupons', { method: 'POST', body })
    modal.open = false; await carregar()
  } catch (e) { alert('Falha ao salvar cupom.') } finally { saving.value = false }
}
const excluir = async (c) => { if (!confirm(`Excluir o cupom ${c.codigo}?`)) return; try { await useApi(`/aplicativo/cupons/${c.id}`, { method: 'DELETE' }); await carregar() } catch { alert('Falha ao excluir.') } }
</script>

<style scoped>
.badge { padding: 3px 9px; border-radius: 10px; font-size: 12px; font-weight: 600; }
.badge-global { background: #e0f2fe; color: #0369a1; }
.badge-custom { background: #f1f5f9; color: #475569; }
.empty { text-align: center; color: #94a3b8; padding: 20px; }
.align-right { text-align: right; }
.modal-overlay { position: fixed; inset: 0; background: rgba(15,23,42,.5); display: flex; align-items: center; justify-content: center; z-index: 50; }
.modal-card { max-width: 560px; width: 90%; padding: 24px; border-radius: 16px; }
.modal-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 16px; }
.mt-2 { margin-top: 12px; } .mt-4 { margin-top: 24px; }
</style>
