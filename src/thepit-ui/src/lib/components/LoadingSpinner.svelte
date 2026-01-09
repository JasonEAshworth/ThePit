<script lang="ts">
	interface Props {
		size?: 'sm' | 'md' | 'lg';
		message?: string;
	}

	let { size = 'md', message = 'Loading...' }: Props = $props();

	const sizeClasses = {
		sm: 'h-4 w-4',
		md: 'h-8 w-8',
		lg: 'h-12 w-12'
	};
</script>

<div class="flex flex-col items-center justify-center py-8">
	<div
		class="animate-spin rounded-full border-b-2 border-pit-500 {sizeClasses[size]}"
		role="status"
		aria-label="Loading"
	></div>
	{#if message}
		<p class="mt-3 text-sm text-gray-500 dark:text-gray-400">{message}</p>
	{/if}
</div>
