<script setup lang="ts">
/**
 * EmpresaDfePanel — abas "NFCe" e "NFe" do formulário de empresa.
 *
 * Porta o comportamento do legado (bloco de produção vs. homologação conforme
 * `tipoAmbienteNfce`/`tipoAmbienteNfe`, séries/numeração, CSC, contingência,
 * substituição tributária, alíquota de crédito ICMS quando Simples Nacional).
 */
import { computed } from 'vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import type { EmpresaParametrosDfe } from './types'

const props = defineProps<{
  modelValue: EmpresaParametrosDfe
  regimeSimplesNacional: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: EmpresaParametrosDfe]
}>()

const dfe = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v)
})

const ambienteOpcoes = [
  { label: 'Produção', value: 1 },
  { label: 'Homologação', value: 2 }
]

function togglePropriedade<K extends keyof EmpresaParametrosDfe>(chave: K) {
  const atual = dfe.value[chave] as unknown as boolean
  dfe.value = { ...dfe.value, [chave]: !atual }
}
</script>

<template>
  <div class="dfe-panel">
    <div class="dfe-secao">
      <h4 class="dfe-titulo">NFC-e</h4>
      <div class="form-grid">
        <div class="col-4">
          <SelectField v-model="dfe.tipoAmbienteNfce" label="Ambiente" :options="ambienteOpcoes" :clearable="false" />
        </div>
        <div v-if="dfe.tipoAmbienteNfce === 2" class="col-4 dfe-switch">
          <label class="switch-label">
            <input
              type="checkbox"
              :checked="dfe.nfceHomologacao.nfceGerarContingenciaEmHomologacao"
              @change="dfe = { ...dfe, nfceHomologacao: { ...dfe.nfceHomologacao, nfceGerarContingenciaEmHomologacao: !dfe.nfceHomologacao.nfceGerarContingenciaEmHomologacao } }"
            />
            <span>Em contingência? {{ dfe.nfceHomologacao.nfceGerarContingenciaEmHomologacao ? 'SIM' : 'NÃO' }}</span>
          </label>
        </div>
      </div>

      <div v-if="dfe.tipoAmbienteNfce === 1" class="form-grid">
        <div class="col-4">
          <TextField v-model="dfe.nfceProducao.nfceCscProducao" label="CSC Produção" />
        </div>
        <div class="col-4">
          <TextField v-model="dfe.nfceProducao.nfceIdCscProducao" label="IDCSC Produção" />
        </div>
        <div class="col-4">
          <QuantityInput v-model="dfe.nfceProducao.nfceSerieProducao" label="Série NFC-e Produção" :decimais="0" />
        </div>
        <div class="col-4">
          <QuantityInput v-model="dfe.nfceProducao.nfceProximoNrProducao" label="N° da próxima NFC-e (produção)" :decimais="0" />
        </div>
      </div>

      <div v-if="dfe.tipoAmbienteNfce === 2" class="form-grid">
        <div class="col-4">
          <TextField v-model="dfe.nfceHomologacao.nfceCscHomologacao" label="CSC Homologação" />
        </div>
        <div class="col-4">
          <TextField v-model="dfe.nfceHomologacao.nfceIdCscHomologacao" label="IDCSC Homologação" />
        </div>
        <div class="col-4">
          <QuantityInput v-model="dfe.nfceHomologacao.nfceSerieHomologacao" label="Série NFC-e Homologação" :decimais="0" />
        </div>
        <div class="col-4">
          <QuantityInput v-model="dfe.nfceHomologacao.nfceProximoNrHomologacao" label="N° da próxima NFC-e (homologação)" :decimais="0" />
        </div>
      </div>
    </div>

    <div class="dfe-secao">
      <h4 class="dfe-titulo">NF-e</h4>
      <div class="form-grid">
        <div class="col-4">
          <SelectField v-model="dfe.tipoAmbienteNfe" label="Ambiente" :options="ambienteOpcoes" :clearable="false" />
        </div>
        <div class="col-4 dfe-switch">
          <label class="switch-label">
            <input type="checkbox" :checked="dfe.destacarIcmsSt" @change="togglePropriedade('destacarIcmsSt')" />
            <span>Destacar ICMS-ST? {{ dfe.destacarIcmsSt ? 'SIM' : 'NÃO' }}</span>
          </label>
        </div>
        <div class="col-4 dfe-switch">
          <label class="switch-label">
            <input
              type="checkbox"
              :checked="dfe.nfe.indicadorSt"
              @change="dfe = { ...dfe, nfe: { ...dfe.nfe, indicadorSt: !dfe.nfe.indicadorSt } }"
            />
            <span>Calcular Substituição Tributária? {{ dfe.nfe.indicadorSt ? 'SIM' : 'NÃO' }}</span>
          </label>
        </div>
        <div v-if="dfe.tipoAmbienteNfe === 2" class="col-4 dfe-switch">
          <label class="switch-label">
            <input
              type="checkbox"
              :checked="dfe.nfe.nfeGerarContingenciaEmHomologacao"
              @change="dfe = { ...dfe, nfe: { ...dfe.nfe, nfeGerarContingenciaEmHomologacao: !dfe.nfe.nfeGerarContingenciaEmHomologacao } }"
            />
            <span>Em contingência? {{ dfe.nfe.nfeGerarContingenciaEmHomologacao ? 'SIM' : 'NÃO' }}</span>
          </label>
        </div>
        <div class="col-4 dfe-switch">
          <label class="switch-label">
            <input
              type="checkbox"
              :checked="dfe.nfe.emitirNfeConjugada"
              @change="dfe = { ...dfe, nfe: { ...dfe.nfe, emitirNfeConjugada: !dfe.nfe.emitirNfeConjugada } }"
            />
            <span>Emitir NF-e conjugada? {{ dfe.nfe.emitirNfeConjugada ? 'SIM' : 'NÃO' }}</span>
          </label>
        </div>
      </div>

      <div v-if="dfe.tipoAmbienteNfe === 1" class="form-grid">
        <div class="col-4">
          <QuantityInput v-model="dfe.nfe.nfeSerieProducao" label="Série NF-e Produção" :decimais="0" />
        </div>
        <div class="col-4">
          <QuantityInput v-model="dfe.nfe.nfeProximoNrProducao" label="N° da próxima NF-e (produção)" :decimais="0" />
        </div>
      </div>
      <div v-if="dfe.tipoAmbienteNfe === 2" class="form-grid">
        <div class="col-4">
          <QuantityInput v-model="dfe.nfe.nfeSerieHomologacao" label="Série NF-e Homologação" :decimais="0" />
        </div>
        <div class="col-4">
          <QuantityInput v-model="dfe.nfe.nfeProximoNrHomologacao" label="N° da próxima NF-e (homologação)" :decimais="0" />
        </div>
      </div>

      <div v-if="regimeSimplesNacional" class="form-grid">
        <div class="col-4">
          <PercentInput v-model="dfe.nfe.valorAliquotaCreditoIcms" label="Alíquota Crédito ICMS" />
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.dfe-secao { margin-bottom: 20px; }
.dfe-titulo { font-size: 14px; color: var(--text-secondary); text-transform: uppercase; letter-spacing: 0.04em; margin: 0 0 12px; }
.dfe-switch { display: flex; align-items: flex-end; padding-bottom: 8px; }
.switch-label { display: flex; align-items: center; gap: 8px; font-size: 13px; color: var(--text-secondary); }
.switch-label input { accent-color: var(--primary); width: 16px; height: 16px; }
</style>
