/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  darkMode: 'class', // Dark Mode obrigatório e estrutural
  theme: {
    extend: {
      fontFamily: {
        sans: ['Geist', 'Inter', 'system-ui', 'sans-serif'],
        mono: ['Geist Mono', 'JetBrains Mono', 'monospace'],
      },
      colors: {
        // Paleta Semântica e Funcional (UI_SPECIFICATION.md - Cap. 2.3)
        compass: {
          bg: '#09090b',         // zinc-950 - Fundo primário do viewport
          surface: '#18181b',    // zinc-900 - Cards e modais
          border: '#27272a',     // zinc-800 - Divisórias e bordas de 1px
          text: '#f4f4f5',       // zinc-100 - Texto primário de alto contraste (AAA)
          muted: '#71717a',      // zinc-500 - Metadata e placeholders
          urgent: '#f59e0b',     // amber-500 - Prazos < 24h ou adiamentos crônicos
          strategic: '#6366f1',  // indigo-500 - Alinhado a metas estratégicas ativas
          execution: '#10b981',  // emerald-500 - Botões de foco, streaks de hábitos
          blocked: '#f43f5e',    // rose-500 - Erros de validação, bloqueios de dependência
        }
      },
      borderRadius: {
        'tactic': '6px',         // Arredondamento padrão para elementos interativos (Cap. 2.4)
      },
      transitionDuration: {
        'tactic': '150ms',       // Orçamento de movimento padrão (Cap. 7.1)
      }
    },
  },
  plugins: [],
}