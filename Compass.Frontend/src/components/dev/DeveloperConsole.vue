<script setup lang="ts">
import { ref, computed } from 'vue';
import { useDevStore, type HttpLogEntry } from '@/stores/devStore';
import { useCommitmentsStore } from '@/stores/commitmentsStore';
import { useDecisionStore } from '@/stores/decisionStore';
import { Terminal, X, Trash2, Activity, Database, Radio, ShieldAlert, Gauge } from 'lucide-vue-next';

const devStore = useDevStore();
const commitmentsStore = useCommitmentsStore();
const decisionStore = useDecisionStore();

const selectedHttpLog = ref<HttpLogEntry | null>(null);

const selectLog = (log: HttpLogEntry) => {
  selectedHttpLog.value = log;
};
</script>

<template>
  <!-- Guarda arquitetural estrita: este bloco não deve existir em builds de produção -->
  <div v-if="devStore.isConsoleOpen" class="fixed inset-x-0 bottom-0 z-50 h-[420px] bg-app border-t border-borderfocus text-content-muted font-mono text-xs flex flex-col shadow-2xl select-none">
    
    <!-- Cabeçalho do Console -->
    <div class="flex items-center justify-between px-4 py-2 bg-surface border-b border-borderbase flex-shrink-0">
      <div class="flex items-center gap-4">
        <span class="flex items-center gap-1.5 font-bold text-content">
          <Terminal class="w-4 h-4 text-content-accent" />
          <span>COMPASS DEV CONSOLE v1.0</span>
        </span>

        <!-- Abas de Navegação -->
        <div class="flex items-center gap-1 bg-app p-0.5 rounded border border-borderbase">
          <button 
            @click="devStore.activeTab = 'http'"
            class="px-2.5 py-1 rounded transition-colors cursor-pointer flex items-center gap-1.5"
            :class="devStore.activeTab === 'http' ? 'bg-surface-active text-content font-semibold' : 'text-content-muted hover:text-content'"
          >
            <Activity class="w-3 h-3 text-content-accent" />
            <span>HTTP & RFC 7807 ({{ devStore.httpLogs.length }})</span>
          </button>

          <button 
            @click="devStore.activeTab = 'pinia'"
            class="px-2.5 py-1 rounded transition-colors cursor-pointer flex items-center gap-1.5"
            :class="devStore.activeTab === 'pinia' ? 'bg-surface-active text-content font-semibold' : 'text-content-muted hover:text-content'"
          >
            <Database class="w-3 h-3 text-status-success-text" />
            <span>Pinia RAM ({{ commitmentsStore.items.length }})</span>
          </button>

          <button 
            @click="devStore.activeTab = 'events'"
            class="px-2.5 py-1 rounded transition-colors cursor-pointer flex items-center gap-1.5"
            :class="devStore.activeTab === 'events' ? 'bg-surface-active text-content font-semibold' : 'text-content-muted hover:text-content'"
          >
            <Radio class="w-3 h-3 text-status-warning" />
            <span>Event Bus ({{ devStore.domainEvents.length }})</span>
          </button>
          <button 
            @click="devStore.activeTab = 'metrics'"
            class="px-2.5 py-1 rounded transition-colors cursor-pointer flex items-center gap-1.5"
            :class="devStore.activeTab === 'metrics'
              ? 'bg-surface-active text-content font-semibold'
              : 'text-content-muted hover:text-content'"
          >
            <Gauge class="w-3 h-3 text-content-accent" />
            <span>Métricas ({{ devStore.metrics.length }})</span>
          </button>
        </div>
      </div>

      <!-- Ações de Fechamento e Limpeza -->
      <div class="flex items-center gap-2">
        <button 
          @click="devStore.clearAllLogs"
          class="inline-flex items-center gap-1 px-2 py-1 rounded bg-surface-active hover:bg-surface-hover text-content-muted transition-colors cursor-pointer"
          title="Limpar Histórico do Console"
        >
          <Trash2 class="w-3 h-3 text-status-danger-text" />
          <span>Purge</span>
        </button>

        <button 
          @click="devStore.isConsoleOpen = false"
          class="p-1 rounded hover:bg-surface-hover text-content-muted hover:text-content transition-colors cursor-pointer"
        >
          <X class="w-4 h-4" />
        </button>
      </div>
    </div>

    <!-- ÁREA DE CONTEÚDO PRINCIPAL (SPLIT VIEW) -->
    <div class="flex-1 flex overflow-hidden min-h-0">
      
      <!-- ABA 1: HTTP & RFC 7807 PROBLEM DETAILS INSPECTOR -->
      <template v-if="devStore.activeTab === 'http'">
        <!-- Lista de Requisições -->
        <div class="w-1/2 border-r border-borderbase overflow-y-auto divide-y divide-borderbase">
          <div v-if="devStore.httpLogs.length === 0" class="p-8 text-center text-content-muted">
            Nenhuma transação HTTP capturada nesta sessão.
          </div>

          <div 
            v-for="log in devStore.httpLogs" 
            :key="log.id"
            @click="selectLog(log)"
            class="px-3 py-2 hover:bg-surface-hover transition-colors cursor-pointer flex items-center justify-between gap-3"
            :class="selectedHttpLog?.id === log.id ? 'bg-surface-active border-l-2 border-borderfocus text-content' : ''"
          >
            <div class="flex items-center gap-2.5 min-w-0 flex-1">
              <span 
                class="px-1.5 py-0.5 rounded text-[10px] font-bold"
                :class="
                  devStore.backendHealth?.status === 'Healthy'
                    ? 'bg-status-success-bg text-status-success-text border border-status-success-border'
                    : 'bg-status-danger-bg text-status-danger-text border border-status-danger-border'
                "
              >
                {{ log.status }}
              </span>
              <span class="font-bold text-content w-12">{{ log.method }}</span>
              <span class="truncate text-content-muted" :title="log.url">{{ log.url }}</span>
            </div>

            <div class="flex items-center gap-2 flex-shrink-0">
              <span v-if="log.isProblemDetails" class="px-1 py-0.5 rounded bg-status-warning-bg text-status-warning border border-status-warning-border text-[9px] font-bold flex items-center gap-0.5">
                <ShieldAlert class="w-2.5 h-2.5" /> RFC7807
              </span>
              <span class="text-content-muted w-12 text-right">{{ log.latencyMs }}ms</span>
              <span class="text-[10px] text-content-muted w-16 text-right">{{ log.timestamp }}</span>
            </div>
          </div>
        </div>

        <!-- Painel de Detalhes (JSON Viewer / Problem Details Decoder) -->
        <div class="w-1/2 bg-app overflow-y-auto p-4 space-y-4">
          <div v-if="!selectedHttpLog" class="h-full flex items-center justify-center text-content-muted">
            Selecione uma transação à esquerda para inspecionar o payload e Correlation ID.
          </div>

          <template v-else>
            <div class="space-y-1 pb-3 border-b border-borderbase">
              <div class="flex items-center justify-between">
                <span class="text-sm font-bold text-content">{{ selectedHttpLog.method }} {{ selectedHttpLog.url }}</span>
                <span class="text-xs font-mono text-content-muted">Status: {{ selectedHttpLog.status }} ({{ selectedHttpLog.latencyMs }}ms)</span>
              </div>
              <div class="text-[11px] text-content-muted">Correlation ID: <span class="text-content-accent select-all">{{ selectedHttpLog.id }}</span></div>
            </div>

            <!-- Destaque Especial para Retornos RFC 7807 -->
            <div v-if="selectedHttpLog.isProblemDetails" class="p-3 rounded bg-status-warning-bg border border-status-warning-border space-y-2">
              <div class="flex items-center gap-1.5 text-status-warning font-bold text-xs">
                <ShieldAlert class="w-4 h-4" />
                <span>Problem Details Decodificado (RFC 7807)</span>
              </div>
              <div class="space-y-1 text-content-muted">
                <div><strong class="text-content-muted">Título:</strong> {{ selectedHttpLog.payload.title || 'N/A' }}</div>
                <div><strong class="text-content-muted">Detalhe:</strong> {{ selectedHttpLog.payload.detail || 'N/A' }}</div>
                <div v-if="selectedHttpLog.payload.errors" class="pt-2">
                  <strong class="text-status-danger-text font-bold block mb-1">Erros de Validação (FluentValidation):</strong>
                  <ul class="list-disc list-inside space-y-0.5 pl-2 text-content-muted">
                    <li v-for="(msgs, field) in selectedHttpLog.payload.errors" :key="field">
                      <span class="font-bold text-content">{{ field }}:</span> {{ Array.isArray(msgs) ? msgs.join(', ') : msgs }}
                    </li>
                  </ul>
                </div>
              </div>
            </div>

            <!-- Dump de Payload RAW -->
            <div class="space-y-1">
              <span class="text-[10px] text-content-muted uppercase tracking-wider font-bold">Payload / Response Body:</span>
              <pre class="p-3 rounded bg-surface border border-borderbase overflow-x-auto text-[11px] text-content-muted leading-relaxed">{{ JSON.stringify(selectedHttpLog.payload, null, 2) }}</pre>
            </div>
          </template>
        </div>
      </template>

      <!-- ABA 2: PINIA RAM STATE INSPECTOR -->
      <template v-else-if="devStore.activeTab === 'pinia'">
        <div class="w-full p-4 overflow-y-auto space-y-6">
          <div class="grid grid-cols-3 gap-4">
            <div class="p-3 rounded bg-surface border border-borderbase">
              <span class="text-content-muted block">Compromissos em RAM:</span>
              <span class="text-lg font-bold text-content">{{ commitmentsStore.items.length }} itens</span>
            </div>
            <div class="p-3 rounded bg-surface border border-borderbase">
              <span class="text-content-muted block">Tempo Livre (M tempo):</span>
              <span class="text-lg font-bold text-status-success-text">{{ decisionStore.availableMinutes }} min</span>
            </div>
            <div class="p-3 rounded bg-surface border border-borderbase">
              <span class="text-content-muted block">Itens em Sincronização:</span>
              <span class="text-lg font-bold text-status-warning">{{ commitmentsStore.items.filter(i => i._isSyncing).length }} ativos</span>
            </div>
          </div>

          <div class="space-y-2">
            <span class="text-xs font-bold text-content-muted">Dump de Memória Otimista (items.value slice):</span>
            <pre class="p-3 rounded bg-surface border border-borderbase overflow-x-auto text-[11px] text-content-muted max-h-64">{{ JSON.stringify(commitmentsStore.items.slice(0, 10), null, 2) }}</pre>
          </div>
        </div>
      </template>

      <!-- ABA 3: DOMAIN EVENT BUS TIMELINE -->
      <template v-else-if="devStore.activeTab === 'events'">
        <div class="w-full p-4 overflow-y-auto space-y-2">
          <div v-if="devStore.domainEvents.length === 0" class="p-8 text-center text-content-muted">
            Nenhum evento de domínio intercetado até o momento.
          </div>

          <div 
            v-for="evt in devStore.domainEvents" 
            :key="evt.id"
            class="p-3 rounded bg-surface border border-borderbase flex items-center justify-between gap-4"
          >
            <div class="flex items-center gap-3">
              <span class="text-status-warning font-bold">⚡ {{ evt.eventType }}</span>
              <span class="text-content-muted">{{ evt.message }}</span>
            </div>
            <div class="flex items-center gap-3 font-mono text-[10px] text-content-muted">
              <span>Entity: {{ evt.entityId }}</span>
              <span>{{ evt.timestamp }}</span>
            </div>
          </div>
        </div>
      </template>

      <!-- ABA 4: TELEMETRY & HEALTH METRICS -->
      <template v-else-if="devStore.activeTab === 'metrics'">
        <div class="w-full p-4 overflow-y-auto space-y-6 font-mono text-xs">
          
          <!-- Painel de Saúde da Infraestrutura -->
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div class="p-3 rounded bg-surface border border-borderbase flex flex-col justify-between">
              <span class="text-content-muted">PostgreSQL (.NET 10 Check):</span>
              <div class="flex items-center justify-between mt-2">
                <span 
                  class="px-2 py-0.5 rounded text-xs font-bold"
                  :class="
                    devStore.backendHealth?.status === 'Healthy'
                      ? 'bg-status-success-bg text-status-success-text border border-status-success-border'
                      : 'bg-status-danger-bg text-status-danger-text border border-status-danger-border'
                  "
                >
                  {{ devStore.backendHealth?.status || 'Checando...' }}
                </span>
                <button @click="devStore.pingBackendHealth()" class="text-[10px] underline text-content-accent cursor-pointer">Ping Now</button>
              </div>
            </div>

            <div class="p-3 rounded bg-surface border border-borderbase flex flex-col justify-between">
              <span class="text-content-muted">Latência do Banco (Ping):</span>
              <span class="text-lg font-bold text-content mt-2">{{ devStore.backendHealth?.latencyMs ?? 0 }} ms</span>
            </div>

            <div class="p-3 rounded bg-surface border border-borderbase flex flex-col justify-between">
              <span class="text-content-muted">JS Heap Memory (Browser):</span>
              <span class="text-lg font-bold text-status-warning mt-2">
                {{ typeof performance !== 'undefined' && (performance as any).memory ? Math.round((performance as any).memory.usedJSHeapSize / 1024 / 1024) + ' MB' : 'N/A' }}
              </span>
            </div>
          </div>

          <!-- Log de Long Tasks e Gargalos de Renderização -->
          <div class="space-y-2">
            <div class="flex items-center justify-between border-b border-borderbase pb-1">
              <span class="font-bold text-content">Radar de Concorrência e Long Tasks (> 50ms):</span>
              <span class="text-[10px] text-content-muted">PerformanceObserver API</span>
            </div>

            <div v-if="devStore.metrics.length === 0" class="p-6 text-center text-content-muted bg-surface rounded border border-dashed border-borderbase">
              Nenhum gargalo de thread ou engasgo de renderização capturado até o momento. A UI está operando a 60 FPS.
            </div>

            <div 
              v-for="met in devStore.metrics" 
              :key="met.id"
              class="p-3 rounded bg-surface border border-borderbase flex items-center justify-between gap-4"
            >
              <div class="flex items-center gap-3">
                <span class="px-1.5 py-0.5 rounded bg-status-danger-bg text-status-danger-text border border-status-danger-border text-[10px] font-bold">
                  {{ met.type }}
                </span>
                <span class="text-content font-bold">{{ met.name }}</span>
                <span class="text-content-muted text-[11px] hidden sm:inline">{{ met.detail }}</span>
              </div>

              <div class="flex items-center gap-3 flex-shrink-0">
                <span class="text-status-danger-text font-bold">{{ met.durationMs }} ms</span>
                <span class="text-content-muted text-[10px]">{{ met.timestamp }}</span>
              </div>
            </div>
          </div>

        </div>
      </template>

    </div>
  </div>
</template>