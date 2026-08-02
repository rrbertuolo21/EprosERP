<template>
  <div>
    <header class="page-header">
      <h1 class="glow-text">Moedas</h1>
      <p class="tagline">Catálogo global de moedas (código ISO único).</p>
    </header>
    <section class="admin-section glass-panel mt-4">
      <header class="section-header">
        <h3>Moedas</h3>
        <button class="btn btn-primary" @click="abrirNovo">Nova moeda</button>
      </header>
      <table class="data-table">
        <thead><tr><th>ISO</th><th>Nome</th><th>Símbolo</th><th>Decimais</th><th></th></tr></thead>
        <tbody>
          <tr v-if="!moedas.length"><td colspan="5" class="empty">Nenhuma moeda.</td></tr>
          <tr v-for="m in moedas" :key="m.id">
            <td><code>{{ m.codigoISO }}</code></td>
            <td>{{ m.nome }}</td>
            <td>{{ m.simbolo }}</td>
            <td>{{ m.casasDecimais }}</td>
            <td class="align-right">
              <button class="btn btn-secondary btn-table-action" @click="editar(m)">Editar</button>
              <button class="btn btn-secondary btn-table-action btn-danger-action" @click="excluir(m)">Excluir</button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>

    <div v-if="modal.open" class="modal-overlay" @click.self="modal.open = false">
      <div class="modal-card glass-panel">
        <h3>{{ modal.form.id ? 'Editar' : 'Nova' }} moeda</h3>
        <form @submit.prevent="salvar" class="vertical-form mt-2">
          <div class="form-row">
            <div class="form-group col-4"><label>Código ISO *</label><input v-model="modal.form.codigoISO" maxlength="3" required /></div>
            <div class="form-group col-8"><label>Nome *</label><input v-model="modal.form.nome" required /></div>
          </div>
          <div class="form-row">
            <div class="form-group col-6"><label>Símbolo *</label><input v-model="modal.form.simbolo" maxlength="5" required /></div>
            <div class="form-group col-6"><label>Casas decimais</label><input type="number" min="0" max="8" v-model.number="modal.form.casasDecimais" /></div>
          </div>
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

const moedas = ref([])
const saving = ref(false)
const modal = reactive({ open: false, form: {} })
const extrair = (r) => Array.isArray(r) ? r : (r?.items ?? r?.dados ?? r?.data ?? [])

const carregar = async () => { try { moedas.value = extrair(await useApi('/plataforma/moedas')) } catch { moedas.value = [] } }
onMounted(carregar)

const abrirNovo = () => { modal.form = { codigoISO: '', nome: '', simbolo: '', casasDecimais: 2 }; modal.open = true }
const editar = (m) => { modal.form = { id: m.id, codigoISO: m.codigoISO, nome: m.nome, simbolo: m.simbolo, casasDecimais: m.casasDecimais }; modal.open = true }

const salvar = async () => {
  saving.value = true
  try {
    const body = { CodigoISO: modal.form.codigoISO, Nome: modal.form.nome, Simbolo: modal.form.simbolo, CasasDecimais: modal.form.casasDecimais }
    if (modal.form.id) await useApi(`/plataforma/moedas/${modal.form.id}`, { method: 'PUT', body: { Id: modal.form.id, ...body } })
    else await useApi('/plataforma/moedas', { method: 'POST', body })
    modal.open = false; await carregar()
  } catch { alert('Falha ao salvar moeda.') } finally { saving.value = false }
}
const excluir = async (m) => { if (!confirm(`Excluir a moeda ${m.codigoISO}?`)) return; try { await useApi(`/plataforma/moedas/${m.id}`, { method: 'DELETE' }); await carregar() } catch { alert('Falha ao excluir.') } }
</script>

<style scoped>
.empty { text-align: center; color: #94a3b8; padding: 20px; }
.align-right { text-align: right; }
.modal-overlay { position: fixed; inset: 0; background: rgba(15,23,42,.5); display: flex; align-items: center; justify-content: center; z-index: 50; }
.modal-card { max-width: 520px; width: 90%; padding: 24px; border-radius: 16px; }
.modal-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 16px; }
.mt-2 { margin-top: 12px; } .mt-4 { margin-top: 24px; }
</style>
