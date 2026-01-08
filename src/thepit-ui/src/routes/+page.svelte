<script lang="ts">
	import { onMount } from 'svelte';
	import { invoiceApi, paymentApi } from '$lib/api';
	import type { Invoice, Payment } from '$lib/types';

	let invoices = $state<Invoice[]>([]);
	let payments = $state<Payment[]>([]);
	let loading = $state(true);

	onMount(async () => {
		try {
			const [invs, pays] = await Promise.all([invoiceApi.getAll(), paymentApi.getAll()]);
			invoices = invs;
			payments = pays;
		} catch {
			// Silently fail - stats will show 0
		} finally {
			loading = false;
		}
	});

	const stats = $derived({
		totalInvoices: invoices.length,
		pendingInvoices: invoices.filter((i) => i.status === 'Pending').length,
		overdueInvoices: invoices.filter((i) => i.status === 'Overdue').length,
		totalPayments: payments.length,
		pendingPayments: payments.filter((p) => p.status === 'Pending').length,
		totalRevenue: payments
			.filter((p) => p.status === 'Completed')
			.reduce((sum, p) => sum + p.amount, 0)
	});

	function formatCurrency(amount: number): string {
		return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);
	}
</script>

<div class="p-4 sm:p-6">
	<div class="mb-6">
		<h1 class="text-2xl font-bold text-gray-900 sm:text-3xl">Dashboard</h1>
		<p class="mt-1 text-gray-600">Invoice & Payment Management</p>
	</div>

	{#if loading}
		<div class="flex items-center justify-center py-12">
			<div class="text-gray-500">Loading dashboard...</div>
		</div>
	{:else}
		<!-- Stats Grid -->
		<div class="mb-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
			<div class="rounded-lg bg-white p-4 shadow sm:p-6">
				<div class="text-sm font-medium text-gray-500">Total Invoices</div>
				<div class="mt-1 text-2xl font-semibold text-gray-900 sm:text-3xl">
					{stats.totalInvoices}
				</div>
				<div class="mt-2 text-sm">
					<span class="text-yellow-600">{stats.pendingInvoices} pending</span>
					{#if stats.overdueInvoices > 0}
						<span class="ml-2 text-red-600">{stats.overdueInvoices} overdue</span>
					{/if}
				</div>
			</div>

			<div class="rounded-lg bg-white p-4 shadow sm:p-6">
				<div class="text-sm font-medium text-gray-500">Total Payments</div>
				<div class="mt-1 text-2xl font-semibold text-gray-900 sm:text-3xl">
					{stats.totalPayments}
				</div>
				<div class="mt-2 text-sm text-yellow-600">
					{stats.pendingPayments} pending
				</div>
			</div>

			<div class="rounded-lg bg-white p-4 shadow sm:col-span-2 sm:p-6 lg:col-span-1">
				<div class="text-sm font-medium text-gray-500">Total Revenue</div>
				<div class="mt-1 text-2xl font-semibold text-green-600 sm:text-3xl">
					{formatCurrency(stats.totalRevenue)}
				</div>
				<div class="mt-2 text-sm text-gray-500">From completed payments</div>
			</div>
		</div>

		<!-- Quick Actions -->
		<div class="mb-8">
			<h2 class="mb-4 text-lg font-semibold text-gray-900">Quick Actions</h2>
			<div class="flex flex-wrap gap-3">
				<a
					href="/invoices"
					class="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
				>
					View Invoices
				</a>
				<a
					href="/payments"
					class="inline-flex items-center rounded-md bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700"
				>
					View Payments
				</a>
			</div>
		</div>

		<!-- Recent Activity -->
		<div class="grid gap-6 lg:grid-cols-2">
			<!-- Recent Invoices -->
			<div class="rounded-lg bg-white p-4 shadow sm:p-6">
				<div class="mb-4 flex items-center justify-between">
					<h2 class="text-lg font-semibold text-gray-900">Recent Invoices</h2>
					<a href="/invoices" class="text-sm text-blue-600 hover:underline">View all</a>
				</div>
				{#if invoices.length === 0}
					<p class="text-gray-500">No invoices yet</p>
				{:else}
					<ul class="divide-y divide-gray-200">
						{#each invoices.slice(0, 5) as invoice (invoice.id)}
							<li class="py-3">
								<a href="/invoices/{invoice.id}" class="block hover:bg-gray-50">
									<div class="flex items-center justify-between">
										<span class="font-medium text-gray-900">{invoice.invoiceNumber}</span>
										<span class="text-gray-500">{formatCurrency(invoice.amount)}</span>
									</div>
									<div class="mt-1 text-sm text-gray-500">{invoice.status}</div>
								</a>
							</li>
						{/each}
					</ul>
				{/if}
			</div>

			<!-- Recent Payments -->
			<div class="rounded-lg bg-white p-4 shadow sm:p-6">
				<div class="mb-4 flex items-center justify-between">
					<h2 class="text-lg font-semibold text-gray-900">Recent Payments</h2>
					<a href="/payments" class="text-sm text-blue-600 hover:underline">View all</a>
				</div>
				{#if payments.length === 0}
					<p class="text-gray-500">No payments yet</p>
				{:else}
					<ul class="divide-y divide-gray-200">
						{#each payments.slice(0, 5) as payment (payment.id)}
							<li class="py-3">
								<a href="/payments/{payment.id}" class="block hover:bg-gray-50">
									<div class="flex items-center justify-between">
										<span class="font-medium text-gray-900">{payment.transactionId}</span>
										<span class="text-gray-500">{formatCurrency(payment.amount)}</span>
									</div>
									<div class="mt-1 text-sm text-gray-500">{payment.status}</div>
								</a>
							</li>
						{/each}
					</ul>
				{/if}
			</div>
		</div>
	{/if}
</div>
