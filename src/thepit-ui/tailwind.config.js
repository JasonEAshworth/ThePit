/** @type {import('tailwindcss').Config} */
export default {
	content: ['./src/**/*.{html,js,svelte,ts}'],
	darkMode: 'class',
	theme: {
		extend: {
			colors: {
				pit: {
					// Semantic colors (from main)
					charcoal: '#1a1d29',
					slate: '#2d3748',
					'slate-light': '#374151',
					blue: '#3b82f6',
					'blue-light': '#60a5fa',
					cyan: '#06b6d4',
					success: '#10b981',
					warning: '#f59e0b',
					error: '#ef4444',
					'text-primary': '#ffffff',
					'text-secondary': '#e5e7eb',
					'text-disabled': '#9ca3af',
					// Amber palette for brand accent
					50: '#fffbeb',
					100: '#fef3c7',
					200: '#fde68a',
					300: '#fcd34d',
					400: '#fbbf24',
					500: '#f59e0b',
					600: '#d97706',
					700: '#b45309',
					800: '#92400e',
					900: '#78350f',
					950: '#451a03'
				}
			}
		}
	},
	plugins: []
};
