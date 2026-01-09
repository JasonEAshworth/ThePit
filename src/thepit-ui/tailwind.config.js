/** @type {import('tailwindcss').Config} */
export default {
	content: ['./src/**/*.{html,js,svelte,ts}'],
	theme: {
		extend: {
			colors: {
				'pit': {
					'charcoal': '#1a1d29',
					'slate': '#2d3748',
					'slate-light': '#374151',
					'blue': '#3b82f6',
					'blue-light': '#60a5fa',
					'cyan': '#06b6d4',
					'success': '#10b981',
					'warning': '#f59e0b',
					'error': '#ef4444',
					'text-primary': '#ffffff',
					'text-secondary': '#e5e7eb',
					'text-disabled': '#9ca3af'
				}
			}
		}
	},
	plugins: []
};
