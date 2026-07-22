import { defineStore } from 'pinia';
import { ref } from 'vue';

export type ThemeId = 'dark' | 'oled' | 'light' | 'paper' | 'slate' | 'nordic' | 'solarized' | 'matrix';

export interface ThemeOption {
  id: ThemeId;
  name: string;
  description: string;
  preview: {
    bg: string;
    surface: string;
    accent: string;
  };
}

export const THEME_OPTIONS: ThemeOption[] = [
  {
    id: 'dark',
    name: 'Grafite Escuro',
    description: 'Composição cromática fundamentada em tonalidades Zinc, concebida para proporcionar elevado contraste, sobriedade estética e reduzida fadiga visual durante extensos períodos de utilização.',
    preview: { bg: '#09090b', surface: '#121215', accent: '#ffffff' }
  },
  {
    id: 'oled',
    name: 'Preto Absoluto',
    description: 'Configuração alicerçada sobre o preto integral (#000000), destinada à minimização da emissão luminosa e à otimização do consumo energético em painéis OLED e AMOLED.',
    preview: { bg: '#000000', surface: '#09090b', accent: '#f4f4f5' }
  },
  {
    id: 'light',
    name: 'Claro Técnico',
    description: 'Arranjo de elevada luminosidade composto por superfícies neutras e contraste rigorosamente equilibrado, apropriado para ambientes sujeitos à intensa iluminação.',
    preview: { bg: '#f4f4f5', surface: '#ffffff', accent: '#09090b' }
  },
  {
    id: 'paper',
    name: 'Papel Clássico',
    description: 'Paleta desenvolvida a partir de tonalidades cálidas inspiradas em suportes editoriais tradicionais, favorecendo a leitura contínua mediante reduzida agressividade luminosa.',
    preview: { bg: '#faf8f5', surface: '#ffffff', accent: '#1c1917' }
  },
  {
    id: 'slate',
    name: 'Ardósia Técnica',
    description: 'Conjunto cromático edificado sobre matizes Slate, destinado a preservar a distinção hierárquica dos elementos visuais mediante equilíbrio tonal e clareza estrutural.',
    preview: { bg: '#0f172a', surface: '#1e293b', accent: '#38bdf8' }
  },
  {
    id: 'nordic',
    name: 'Nórdico',
    description: 'Composição baseada em tonalidades frias de cinza e azul, concebida para assegurar elevada legibilidade, discrição cromática e notável estabilidade perceptiva.',
    preview: { bg: '#2e3440', surface: '#3b4252', accent: '#88c0d0' }
  },
  {
    id: 'solarized',
    name: 'Solarizado',
    description: 'Paleta derivada do consagrado esquema Solarized, distinguindo-se pela harmonia entre luminosidade e contraste, preservando a consistência visual em extensas jornadas de trabalho.',
    preview: { bg: '#002b36', surface: '#073642', accent: '#2aa198' }
  },
  {
    id: 'matrix',
    name: 'Terminal Clássico',
    description: 'Configuração inspirada nos históricos terminais de caracteres, empregando superfícies escuras e acentos cromáticos em verde para realçar informações de natureza operacional.',
    preview: { bg: '#030704', surface: '#08120a', accent: '#22c55e' }
  }
];
export const useThemeStore = defineStore('theme', () => {
  const currentTheme = ref<ThemeId>('dark');

  const applyThemeToDOM = (themeId: ThemeId) => {
    const root = document.documentElement;

    if (themeId === 'dark') {
      root.removeAttribute('data-theme');
    } else {
      root.setAttribute('data-theme', themeId);
    }
  };

  const setTheme = (themeId: ThemeId) => {
    currentTheme.value = themeId;
    applyThemeToDOM(themeId);

    try {
      localStorage.setItem('compass_theme', themeId);
    } catch (e) {
      console.warn('[ThemeStore]: Falha ao persistir tema.', e);
    }
  };

  const initTheme = () => {
    try {
      const saved = localStorage.getItem('compass_theme') as ThemeId;

      if (saved && THEME_OPTIONS.some(t => t.id === saved)) {
        currentTheme.value = saved;
        applyThemeToDOM(saved);
      } else {
        applyThemeToDOM('dark');
      }
    } catch {
      applyThemeToDOM('dark');
    }
  };

  return {
    currentTheme,
    setTheme,
    initTheme
  };
});