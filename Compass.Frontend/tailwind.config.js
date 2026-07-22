/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        // Abstração semântica apontando para os Tokens CSS
        app: 'var(--bg-app)',
        surface: {
          DEFAULT: 'var(--bg-surface)',
          hover: 'var(--bg-surface-hover)',
          active: 'var(--bg-surface-active)',
        },
        borderbase: 'var(--border-base)',
        borderfocus: 'var(--border-focus)',
        borderhighlight: 'var(--border-highlight)',
        content: {
          DEFAULT: 'var(--text-main)',
          muted: 'var(--text-muted)',
          accent: 'var(--text-accent)',
          invert: 'var(--text-invert)',
        },
        status: {
          success: {
            bg: 'var(--status-success-bg)',
            text: 'var(--status-success-text)',
            border: 'var(--status-success-border)',
          },
          warning: 'var(--status-warning-text)',
          danger: {
            bg: 'var(--status-danger-bg)',
            text: 'var(--status-danger-text)',
            border: 'var(--status-danger-border)',
          }
        }
      }
    },
  },
  plugins: [],
}