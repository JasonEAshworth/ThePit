<script lang="ts">
	import '../app.css';
	import { page } from '$app/stores';
	import { ThemeToggle } from '$lib/components';

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

<div class="min-h-screen bg-gray-100 dark:bg-slate-900">
	<!-- Navigation -->
	<nav class="bg-white shadow dark:bg-slate-800">
		<div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
			<div class="flex h-16 justify-between">
				<!-- Logo and Desktop Nav -->
				<div class="flex">
					<a href="/" class="flex flex-shrink-0 items-center gap-2">
						<!-- Pit/Flame icon -->
						<svg class="h-8 w-8 text-pit-500" viewBox="0 0 24 24" fill="currentColor">
							<path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8zm-1-13h2v6h-2zm0 8h2v2h-2z"/>
							<path d="M12 5.5c-1.5 0-2.5 1.5-2.5 3 0 1 .5 2 1.5 3 .5.5 1 1.5 1 2.5s-.5 2-1 2.5c1.5 0 2.5-1.5 2.5-3 0-1-.5-2-1.5-3-.5-.5-1-1.5-1-2.5s.5-2 1-2.5z" fill="currentColor"/>
						</svg>
						<span class="text-xl font-bold text-gray-900 dark:text-white">
							The<span class="text-pit-500">Pit</span>
						</span>
					</a>
					<div class="hidden sm:ml-8 sm:flex sm:space-x-4">
						{#each navItems as item}
							<a
								href={item.href}
								class="inline-flex items-center border-b-2 px-1 pt-1 text-sm font-medium transition-colors
									{isActive(item.href)
									? 'border-pit-500 text-gray-900 dark:text-white'
									: 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700 dark:text-gray-400 dark:hover:border-slate-600 dark:hover:text-gray-300'}"
							>
								{item.label}
							</a>
						{/each}
					</div>
				</div>

				<!-- Theme toggle and mobile menu button -->
				<div class="flex items-center gap-2">
					<ThemeToggle />

					<!-- Mobile menu button -->
					<div class="flex items-center sm:hidden">
						<button
							type="button"
							onclick={() => (mobileMenuOpen = !mobileMenuOpen)}
							class="inline-flex items-center justify-center rounded-md p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-500 dark:hover:bg-slate-700 dark:hover:text-gray-300"
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
								? 'border-pit-500 bg-pit-50 text-pit-700 dark:bg-pit-900/20 dark:text-pit-400'
								: 'border-transparent text-gray-500 hover:border-gray-300 hover:bg-gray-50 hover:text-gray-700 dark:text-gray-400 dark:hover:border-slate-600 dark:hover:bg-slate-700 dark:hover:text-gray-300'}"
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
