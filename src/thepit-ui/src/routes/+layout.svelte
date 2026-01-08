<script lang="ts">
	import '../app.css';
	import { page } from '$app/stores';

	let { children } = $props();
	let mobileMenuOpen = $state(false);

	const navItems = [
		{ href: '/', label: 'Home' },
		{ href: '/invoices', label: 'Invoices' },
		{ href: '/payments', label: 'Payments' }
	];

	function isActive(href: string): boolean {
		if (href === '/') return $page.url.pathname === '/';
		return $page.url.pathname.startsWith(href);
	}
</script>

<div class="min-h-screen bg-gray-100">
	<!-- Navigation -->
	<nav class="bg-white shadow">
		<div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
			<div class="flex h-16 justify-between">
				<!-- Logo and Desktop Nav -->
				<div class="flex">
					<a href="/" class="flex flex-shrink-0 items-center">
						<span class="text-xl font-bold text-gray-900">The Pit</span>
					</a>
					<div class="hidden sm:ml-8 sm:flex sm:space-x-4">
						{#each navItems as item}
							<a
								href={item.href}
								class="inline-flex items-center border-b-2 px-1 pt-1 text-sm font-medium transition-colors
									{isActive(item.href)
									? 'border-blue-500 text-gray-900'
									: 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700'}"
							>
								{item.label}
							</a>
						{/each}
					</div>
				</div>

				<!-- Mobile menu button -->
				<div class="flex items-center sm:hidden">
					<button
						type="button"
						onclick={() => (mobileMenuOpen = !mobileMenuOpen)}
						class="inline-flex items-center justify-center rounded-md p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-500"
						aria-expanded={mobileMenuOpen}
					>
						<span class="sr-only">Open main menu</span>
						{#if mobileMenuOpen}
							<svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
								<path
									stroke-linecap="round"
									stroke-linejoin="round"
									stroke-width="2"
									d="M6 18L18 6M6 6l12 12"
								/>
							</svg>
						{:else}
							<svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
								<path
									stroke-linecap="round"
									stroke-linejoin="round"
									stroke-width="2"
									d="M4 6h16M4 12h16M4 18h16"
								/>
							</svg>
						{/if}
					</button>
				</div>
			</div>
		</div>

		<!-- Mobile menu -->
		{#if mobileMenuOpen}
			<div class="sm:hidden">
				<div class="space-y-1 pb-3 pt-2">
					{#each navItems as item}
						<a
							href={item.href}
							onclick={() => (mobileMenuOpen = false)}
							class="block border-l-4 py-2 pl-3 pr-4 text-base font-medium
								{isActive(item.href)
								? 'border-blue-500 bg-blue-50 text-blue-700'
								: 'border-transparent text-gray-500 hover:border-gray-300 hover:bg-gray-50 hover:text-gray-700'}"
						>
							{item.label}
						</a>
					{/each}
				</div>
			</div>
		{/if}
	</nav>

	<!-- Main content -->
	<main class="mx-auto max-w-7xl">
		{@render children()}
	</main>
</div>
