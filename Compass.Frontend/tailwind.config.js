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
        'fast': '100ms',
        'subtle': '200ms',
      },
      transitionTimingFunction: {
        // Curvas da Física Computacional do Compass (Cap. 7.1)
        'snap-out': 'cubic-bezier(0.16, 1, 0.3, 1)',   // Entrada rápida de modais e gavetas
        'fade-smooth': 'cubic-bezier(0.4, 0, 0.2, 1)', // Opacidade de botões e hover
        'collapse-in': 'cubic-bezier(0.4, 0, 1, 1)',   // Saída de tarefas concluídas
      },
      keyframes: {
        'draw-check': {
          '0%': { strokeDashoffset: '24', opacity: '0', transform: 'scale(0.8)' },
          '100%': { strokeDashoffset: '0', opacity: '1', transform: 'scale(1)' },
        },
        'streak-pop': {
          '0%': { transform: 'scale(1)' },
          '50%': { transform: 'scale(1.28)', filter: 'drop-shadow(0 0 12px rgba(16, 185, 129, 0.4))' },
          '100%': { transform: 'scale(1)' },
        },
        'slide-up-fade': {
          '0%': { opacity: '0', transform: 'translateY(8px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        }
      },
      animation: {
        'check': 'draw-check 150ms cubic-bezier(0.16, 1, 0.3, 1) forwards',
        'streak': 'streak-pop 200ms cubic-bezier(0.16, 1, 0.3, 1) forwards',
        'promote': 'slide-up-fade 150ms cubic-bezier(0.16, 1, 0.3, 1) forwards',
      }
    },
  },
  plugins: [],
}