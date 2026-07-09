/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,jsx}'],
  theme: {
    extend: {
      colors: {
        brand: {
          primary: '#FFFFFF',
          'primary-inverse': '#0E0E10',

          bg: '#F5F5F5',
          surface: '#FFFFFF',
          surfaceAlt: '#F2F2F2',
          ink: '#0E0E10',
          muted: '#6B7280',
          'muted-light': '#9CA3AF',
          divider: '#E5E7EB',
          border: '#D1D5DB',

          success: '#16A34A',
          danger: '#DC2626',
          warning: '#D97706',
          gold: '#FFB627',
        },
      },
      fontFamily: {
        sans: ['-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'Helvetica', 'Arial', 'sans-serif'],
      },
      boxShadow: {
        soft: '0 2px 8px rgba(0,0,0,0.06)',
        medium: '0 8px 30px rgba(0,0,0,0.1)',
        prominent: '0 12px 48px rgba(0,0,0,0.15)',
      },
      animation: {
        'fade-in': 'fadeIn 0.25s ease-in',
        'slide-up': 'slideUp 0.3s ease-out',
      },
      keyframes: {
        fadeIn: { '0%': { opacity: '0' }, '100%': { opacity: '1' } },
        slideUp: { '0%': { opacity: '0', transform: 'translateY(8px)' }, '100%': { opacity: '1', transform: 'translateY(0)' } },
      },
    },
  },
  plugins: [],
};
