<script lang="ts">
	import { onMount } from 'svelte';
	import { invoiceApi, paymentApi } from '$lib/api';
	import { LoadingSpinner, ErrorAlert } from '$lib/components';
	import type { Invoice, Payment } from '$lib/types';

	let invoices = $state<Invoice[]>([]);
	let payments = $state<Payment[]>([]);
	let loading = $state(true);
	let error = $state<string | null>(null);

	onMount(async () => {
		await loadData();
	});

	async function loadData() {
		loading = true;
		error = null;
		try {
			const [invoiceData, paymentData] = await Promise.all([
				invoiceApi.getAll(),
				paymentApi.getAll()
			]);
			invoices = invoiceData;
			payments = paymentData;
		} catch (e) {
			error = e instanceof Error ? e.message : 'Failed to load data';
		} finally {
			loading = false;
		}
	}

	const stats = $derived.by(() => {
		const totalInvoices = invoices.length;
		const pendingInvoices = invoices.filter((i) => i.status === 'Pending').length;
		const overdueInvoices = invoices.filter((i) => i.status === 'Overdue').length;
		const paidInvoices = invoices.filter((i) => i.status === 'Paid').length;

		const totalPayments = payments.length;
		const pendingPayments = payments.filter((p) => p.status === 'Pending').length;
		const completedPayments = payments.filter((p) => p.status === 'Completed').length;

		const totalRevenue = payments
			.filter((p) => p.status === 'Completed')
			.reduce((sum, p) => sum + p.amount, 0);

		const pendingAmount = invoices
			.filter((i) => i.status === 'Pending' || i.status === 'Overdue')
			.reduce((sum, i) => sum + i.amount, 0);

		return {
			totalInvoices,
			pendingInvoices,
			overdueInvoices,
			paidInvoices,
			totalPayments,
			pendingPayments,
			completedPayments,
			totalRevenue,
			pendingAmount
		};
	});

	const recentInvoices = $derived(
		[...invoices].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()).slice(0, 5)
	);

	const recentPayments = $derived(
		[...payments].sort((a, b) => new Date(b.paymentDate).getTime() - new Date(a.paymentDate).getTime()).slice(0, 5)
	);

	function formatCurrency(amount: number): string {
		return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);
	}

	function formatDate(dateStr: string): string {
		return new Date(dateStr).toLocaleDateString();
	}

	function getInvoiceStatusClass(status: string): string {
		switch (status) {
			case 'Paid':
				return 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400';
			case 'Pending':
				return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400';
			case 'Overdue':
				return 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400';
			default:
				return 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300';
		}
	}

	function getPaymentStatusClass(status: string): string {
		switch (status) {
			case 'Completed':
				return 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400';
			case 'Pending':
				return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400';
			case 'Failed':
				return 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400';
			default:
				return 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300';
		}
	}
</script>

<div class="min-h-screen p-6">
	<div class="mx-auto max-w-7xl">
		<div class="mb-8">
			<h1 class="text-3xl font-bold text-gray-900 dark:text-white">Dashboard</h1>
			<p class="mt-1 text-gray-600 dark:text-gray-400">Invoice & Payment Management Overview</p>
		</div>

		{#if loading}
			<LoadingSpinner message="Loading dashboard..." />
		{:else if error}
			<ErrorAlert message={error} onRetry={loadData} />
		{:else}
			<!-- Statistics Cards -->
			<div class="mb-8 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
				<div class="rounded-lg bg-white p-6 shadow dark:bg-slate-800">
					<div class="text-sm font-medium text-gray-500 dark:text-gray-400">Total Invoices</div>
					<div class="mt-2 text-3xl font-bold text-gray-900 dark:text-white">{stats.totalInvoices}</div>
					<div class="mt-2 text-sm text-gray-600 dark:text-gray-400">
						<span class="text-yellow-600 dark:text-yellow-400">{stats.pendingInvoices} pending</span> ·
						<span class="text-red-600 dark:text-red-400">{stats.overdueInvoices} overdue</span>
					</div>
				</div>

				<div class="rounded-lg bg-white p-6 shadow dark:bg-slate-800">
					<div class="text-sm font-medium text-gray-500 dark:text-gray-400">Total Payments</div>
					<div class="mt-2 text-3xl font-bold text-gray-900 dark:text-white">{stats.totalPayments}</div>
					<div class="mt-2 text-sm text-gray-600 dark:text-gray-400">
						<span class="text-green-600 dark:text-green-400">{stats.completedPayments} completed</span> ·
						<span class="text-yellow-600 dark:text-yellow-400">{stats.pendingPayments} pending</span>
					</div>
				</div>

				<div class="rounded-lg bg-white p-6 shadow dark:bg-slate-800">
					<div class="text-sm font-medium text-gray-500 dark:text-gray-400">Total Revenue</div>
					<div class="mt-2 text-3xl font-bold text-green-600 dark:text-green-400">{formatCurrency(stats.totalRevenue)}</div>
					<div class="mt-2 text-sm text-gray-600 dark:text-gray-400">From completed payments</div>
				</div>

				<div class="rounded-lg bg-white p-6 shadow dark:bg-slate-800">
					<div class="text-sm font-medium text-gray-500 dark:text-gray-400">Pending Amount</div>
					<div class="mt-2 text-3xl font-bold text-pit-500">{formatCurrency(stats.pendingAmount)}</div>
					<div class="mt-2 text-sm text-gray-600 dark:text-gray-400">Outstanding invoices</div>
				</div>
			</div>

			<!-- Quick Actions -->
			<div class="mb-8">
				<h2 class="mb-4 text-lg font-semibold text-gray-900 dark:text-white">Quick Actions</h2>
				<div class="flex flex-wrap gap-4">
					<a
						href="/invoices/new"
						class="rounded-md bg-pit-500 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-pit-600"
					>
						Create Invoice
					</a>
					<a
						href="/invoices"
						class="rounded-md bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm ring-1 ring-gray-300 hover:bg-gray-50 dark:bg-slate-700 dark:text-gray-200 dark:ring-slate-600 dark:hover:bg-slate-600"
					>
						View All Invoices
					</a>
					<a
						href="/payments"
						class="rounded-md bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm ring-1 ring-gray-300 hover:bg-gray-50 dark:bg-slate-700 dark:text-gray-200 dark:ring-slate-600 dark:hover:bg-slate-600"
					>
						View All Payments
					</a>
				</div>
			</div>

			<!-- Recent Activity -->
			<div class="grid grid-cols-1 gap-6 lg:grid-cols-2">
				<!-- Recent Invoices -->
				<div class="rounded-lg bg-white p-6 shadow dark:bg-slate-800">
					<div class="mb-4 flex items-center justify-between">
						<h2 class="text-lg font-semibold text-gray-900 dark:text-white">Recent Invoices</h2>
						<a href="/invoices" class="text-sm text-pit-500 hover:underline">View all</a>
					</div>
					{#if recentInvoices.length === 0}
						<p class="text-gray-500 dark:text-gray-400">No invoices yet</p>
					{:else}
						<div class="space-y-3">
							{#each recentInvoices as invoice (invoice.id)}
								<div class="flex items-center justify-between rounded-md border border-gray-100 p-3 dark:border-slate-700">
									<div>
										<a href="/invoices/{invoice.id}" class="font-medium text-pit-500 hover:underline">
											{invoice.invoiceNumber}
										</a>
										<div class="text-sm text-gray-500 dark:text-gray-400">{formatDate(invoice.createdAt)}</div>
									</div>
									<div class="text-right">
										<div class="font-medium text-gray-900 dark:text-white">{formatCurrency(invoice.amount)}</div>
										<span
											class="inline-flex rounded-full px-2 text-xs font-semibold {getInvoiceStatusClass(invoice.status)}"
										>
											{invoice.status}
										</span>
									</div>
								</div>
							{/each}
						</div>
					{/if}
				</div>

				<!-- Recent Payments -->
				<div class="rounded-lg bg-white p-6 shadow dark:bg-slate-800">
					<div class="mb-4 flex items-center justify-between">
						<h2 class="text-lg font-semibold text-gray-900 dark:text-white">Recent Payments</h2>
						<a href="/payments" class="text-sm text-pit-500 hover:underline">View all</a>
					</div>
					{#if recentPayments.length === 0}
						<p class="text-gray-500 dark:text-gray-400">No payments yet</p>
					{:else}
						<div class="space-y-3">
							{#each recentPayments as payment (payment.id)}
								<div class="flex items-center justify-between rounded-md border border-gray-100 p-3 dark:border-slate-700">
									<div>
										<div class="font-medium text-gray-900 dark:text-white">{payment.transactionId}</div>
										<div class="text-sm text-gray-500 dark:text-gray-400">{formatDate(payment.paymentDate)}</div>
									</div>
									<div class="text-right">
										<div class="font-medium text-gray-900 dark:text-white">{formatCurrency(payment.amount)}</div>
										<span
											class="inline-flex rounded-full px-2 text-xs font-semibold {getPaymentStatusClass(payment.status)}"
										>
											{payment.status}
										</span>
									</div>
								</div>
							{/each}
						</div>
					{/if}
				</div>
			</div>
		{/if}
	</div>
</div>
