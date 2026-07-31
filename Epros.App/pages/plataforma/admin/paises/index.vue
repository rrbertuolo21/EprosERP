<template>
  <div>
    <header class="page-header">
      <h1 class="glow-text">Países</h1>
      <p class="tagline">Catálogo global de países (nome único).</p>
    </header>
    <section class="admin-section glass-panel mt-4">
      <header class="section-header"><h3>Países</h3></header>
      <table class="data-table">
        <thead><tr><th>Nome</th><th>ISO-2</th><th>ISO-3</th><th>Capital</th><th>DDI</th><th>Ativo</th><th></th></tr></thead>
        <tbody>
          <tr v-if="!paises.length"><td colspan="7" class="empty">Nenhum país.</td></tr>
          <tr v-for="p in paises" :key="p.id">
            <td>{{ p.nome }}</td>
            <td><code>{{ p.codigoIsoAlpha2 }}</code></td>
            <td><code>{{ p.codigoIsoAlpha3 }}</code></td>
            <td>{{ p.capital || '—' }}</td>
            <td>{{ p.codigoDiscagem || '—' }}</td>
            <td><span class="badge" :class="p.ativo ? 'badge-ok' : 'badge-off'">{{ p.ativo ? 'Ativo' : 'Inativo' }}</span></td>
            <td class="align-right">
              <button class="btn btn-secondary btn-table-action" @click="editar(p)">Editar</button>
              <button class="btn btn-secondary btn-table-action" @click="toggleAtivo(p)">{{ p.ativo ? 'Inativar' : 'Reativar' }}</button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>

    <div v-if="modal.open" class="modal-overlay" @click.self="modal.open = false">
      <div class="modal-card glass-panel">
        <h3>Editar país</h3>
        <form @submit.prevent="salvar" class="vertical-form mt-2">
          <div class="form-row"><div class="form-group col-12"><label>Nome *</label><input v-model="modal.form.nome" required /></div></div>
          <div class="form-row">
            <div class="form-group col-6"><label>Capital</label><input v-model="modal.form.capital" /></div>
            <div class="form-group col-6"><label>DDI</label><input v-model="modal.form.codigoDiscagem" placeholder="Ex: +55" /></div>
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

const paises = ref([])
const saving = ref(false)
const modal = reactive({ open: false, form: {} })
const extrair = (r) => Array.isArray(r) ? r : (r?.items ?? r?.dados ?? r?.data ?? [])

const carregar = async () => { try { paises.value = extrair(await useApi('/cadastros/geografia/paises')) } catch { paises.value = [] } }
onMounted(carregar)

const editar = (p) => { modal.form = { id: p.id, nome: p.nome, capital: p.capital, codigoDiscagem: p.codigoDiscagem }; modal.open = true }
const salvar = async () => {
  saving.value = true
  try {
    await useApi(`/cadastros/geografia/paises/${modal.form.id}`, { method: 'PUT', body: { Id: modal.form.id, Nome: modal.form.nome, Capital: modal.form.capital || null, CodigoDiscagem: modal.form.codigoDiscagem || null } })
    modal.open = false; await carregar()
  } catch { alert('Falha ao salvar país.') } finally { saving.value = false }
}
const toggleAtivo = async (p) => { try { await useApi(`/cadastros/geografia/paises/${p.id}/ativo?ativo=${!p.ativo}`, { method: 'PATCH' }); await carregar() } catch { alert('Falha ao alterar status.') } }
</script>

<style scoped>
.empty { text-align: center; color: #94a3b8; padding: 20px; }
.align-right { text-align: right; }
.badge { padding: 3px 9px; border-radius: 10px; font-size: 12px; font-weight: 600; }
.badge-ok { background: #dcfce7; color: #15803d; } .badge-off { background: #fee2e2; color: #b91c1c; }
.modal-overlay { position: fixed; inset: 0; background: rgba(15,23,42,.5); display: flex; align-items: center; justify-content: center; z-index: 50; }
.modal-card { max-width: 520px; width: 90%; padding: 24px; border-radius: 16px; }
.modal-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 16px; }
.mt-2 { margin-top: 12px; } .mt-4 { margin-top: 24px; }
</style>
