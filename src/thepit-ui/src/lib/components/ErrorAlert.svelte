<script lang="ts">
	interface Props {
		message: string;
		onRetry?: () => void;
		onDismiss?: () => void;
	}

	let { message, onRetry, onDismiss }: Props = $props();
</script>

<div class="rounded-md bg-red-50 p-4 dark:bg-red-900/20" role="alert">
	<div class="flex">
		<div class="flex-shrink-0">
			<svg class="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
				<path
					fill-rule="evenodd"
					d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.28 7.22a.75.75 0 00-1.06 1.06L8.94 10l-1.72 1.72a.75.75 0 101.06 1.06L10 11.06l1.72 1.72a.75.75 0 101.06-1.06L11.06 10l1.72-1.72a.75.75 0 00-1.06-1.06L10 8.94 8.28 7.22z"
					clip-rule="evenodd"
				/>
			</svg>
		</div>
		<div class="ml-3 flex-1">
			<p class="text-sm text-red-700 dark:text-red-400">{message}</p>
		</div>
		<div class="ml-3 flex gap-2">
			{#if onRetry}
				<button
					type="button"
					onclick={onRetry}
					class="text-sm font-medium text-red-700 underline hover:text-red-600 dark:text-red-400 dark:hover:text-red-300"
				>
					Retry
				</button>
			{/if}
			{#if onDismiss}
				<button
					type="button"
					onclick={onDismiss}
					class="text-red-500 hover:text-red-400"
					aria-label="Dismiss"
				>
					<svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
						<path
							d="M6.28 5.22a.75.75 0 00-1.06 1.06L8.94 10l-3.72 3.72a.75.75 0 101.06 1.06L10 11.06l3.72 3.72a.75.75 0 101.06-1.06L11.06 10l3.72-3.72a.75.75 0 00-1.06-1.06L10 8.94 6.28 5.22z"
						/>
					</svg>
				</button>
			{/if}
		</div>
	</div>
</div>
