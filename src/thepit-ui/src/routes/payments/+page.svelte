<script lang="ts">
	import { onMount } from 'svelte';
	import { paymentApi } from '$lib/api';
	import type { Payment, PaymentStatus } from '$lib/types';

	let payments = $state<Payment[]>([]);
	let loading = $state(true);
	let error = $state<string | null>(null);

	// Filters
	let searchQuery = $state('');
	let statusFilter = $state<PaymentStatus | ''>('');
	let methodFilter = $state('');

	// Sorting
	let sortColumn = $state<keyof Payment>('paymentDate');
	let sortDirection = $state<'asc' | 'desc'>('desc');

	const statuses: PaymentStatus[] = ['Pending', 'Completed', 'Failed', 'Refunded'];
	const methods = ['Credit Card', 'Debit Card', 'Bank Transfer', 'Cash', 'Check'];

	onMount(async () => {
		await loadPayments();
	});

	async function loadPayments() {
		loading = true;
		error = null;
		try {
			payments = await paymentApi.getAll();
		} catch (e) {
			error = e instanceof Error ? e.message : 'Failed to load payments';
		} finally {
			loading = false;
		}
	}

	function toggleSort(column: keyof Payment) {
		if (sortColumn === column) {
			sortDirection = sortDirection === 'asc' ? 'desc' : 'asc';
		} else {
			sortColumn = column;
			sortDirection = 'asc';
		}
	}

	function getSortIcon(column: keyof Payment): string {
		if (sortColumn !== column) return '↕';
		return sortDirection === 'asc' ? '↑' : '↓';
	}

	const filteredPayments = $derived.by(() => {
		let result = payments;

		if (statusFilter) {
			result = result.filter((p) => p.status === statusFilter);
		}

		if (methodFilter) {
			result = result.filter((p) => p.paymentMethod === methodFilter);
		}

		if (searchQuery.trim()) {
			const query = searchQuery.toLowerCase();
			result = result.filter(
				(p) =>
					p.transactionId.toLowerCase().includes(query) ||
					p.paymentMethod.toLowerCase().includes(query)
			);
		}

		result = [...result].sort((a, b) => {
			const aVal = a[sortColumn];
			const bVal = b[sortColumn];

			if (typeof aVal === 'number' && typeof bVal === 'number') {
				return sortDirection === 'asc' ? aVal - bVal : bVal - aVal;
			}

			const aStr = String(aVal);
			const bStr = String(bVal);
			return sortDirection === 'asc' ? aStr.localeCompare(bStr) : bStr.localeCompare(aStr);
		});

		return result;
	});

	function formatDateTime(dateStr: string): string {
		return new Date(dateStr).toLocaleString();
	}

	function formatCurrency(amount: number): string {
		return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);
	}

	function getStatusClass(status: PaymentStatus): string {
		switch (status) {
			case 'Completed':
				return 'bg-green-100 text-green-800';
			case 'Pending':
				return 'bg-yellow-100 text-yellow-800';
			case 'Failed':
				return 'bg-red-100 text-red-800';
			case 'Refunded':
				return 'bg-gray-100 text-gray-800';
			default:
				return 'bg-gray-100 text-gray-800';
		}
	}
</script>

<div class="p-6">
	<div class="mb-6">
		<h1 class="text-2xl font-bold text-gray-900">Payments</h1>
	</div>

	<!-- Filters -->
	<div class="mb-4 flex flex-wrap gap-4">
		<input
			type="text"
			placeholder="Search by transaction ID..."
			bind:value={searchQuery}
			class="rounded-md border border-gray-300 px-4 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
		/>

		<select
			bind:value={statusFilter}
			class="rounded-md border border-gray-300 px-4 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
		>
			<option value="">All Statuses</option>
			{#each statuses as status}
				<option value={status}>{status}</option>
			{/each}
		</select>

		<select
			bind:value={methodFilter}
			class="rounded-md border border-gray-300 px-4 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
		>
			<option value="">All Methods</option>
			{#each methods as method}
				<option value={method}>{method}</option>
			{/each}
		</select>
	</div>

	{#if loading}
		<div class="flex items-center justify-center py-12">
			<div class="text-gray-500">Loading payments...</div>
		</div>
	{:else if error}
		<div class="rounded-md bg-red-50 p-4 text-red-700">
			{error}
			<button onclick={loadPayments} class="ml-4 underline">Retry</button>
		</div>
	{:else if filteredPayments.length === 0}
		<div class="py-12 text-center text-gray-500">
			{payments.length === 0 ? 'No payments found' : 'No payments match your filters'}
		</div>
	{:else}
		<div class="overflow-x-auto rounded-lg border border-gray-200">
			<table class="min-w-full divide-y divide-gray-200">
				<thead class="bg-gray-50">
					<tr>
						<th
							class="cursor-pointer px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 hover:bg-gray-100"
							onclick={() => toggleSort('transactionId')}
						>
							Transaction ID {getSortIcon('transactionId')}
						</th>
						<th
							class="cursor-pointer px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 hover:bg-gray-100"
							onclick={() => toggleSort('invoiceId')}
						>
							Invoice {getSortIcon('invoiceId')}
						</th>
						<th
							class="cursor-pointer px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 hover:bg-gray-100"
							onclick={() => toggleSort('amount')}
						>
							Amount {getSortIcon('amount')}
						</th>
						<th
							class="cursor-pointer px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 hover:bg-gray-100"
							onclick={() => toggleSort('paymentDate')}
						>
							Date {getSortIcon('paymentDate')}
						</th>
						<th
							class="cursor-pointer px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 hover:bg-gray-100"
							onclick={() => toggleSort('paymentMethod')}
						>
							Method {getSortIcon('paymentMethod')}
						</th>
						<th
							class="cursor-pointer px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 hover:bg-gray-100"
							onclick={() => toggleSort('status')}
						>
							Status {getSortIcon('status')}
						</th>
					</tr>
				</thead>
				<tbody class="divide-y divide-gray-200 bg-white">
					{#each filteredPayments as payment (payment.id)}
						<tr class="hover:bg-gray-50">
							<td class="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">
								<a href="/payments/{payment.id}" class="text-blue-600 hover:underline">
									{payment.transactionId}
								</a>
							</td>
							<td class="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
								<a href="/invoices/{payment.invoiceId}" class="text-blue-600 hover:underline">
									#{payment.invoiceId}
								</a>
							</td>
							<td class="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
								{formatCurrency(payment.amount)}
							</td>
							<td class="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
								{formatDateTime(payment.paymentDate)}
							</td>
							<td class="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
								{payment.paymentMethod}
							</td>
							<td class="whitespace-nowrap px-6 py-4 text-sm">
								<span
									class="inline-flex rounded-full px-2 text-xs font-semibold leading-5 {getStatusClass(payment.status)}"
								>
									{payment.status}
								</span>
							</td>
						</tr>
					{/each}
				</tbody>
			</table>
		</div>

		<div class="mt-4 text-sm text-gray-500">
			Showing {filteredPayments.length} of {payments.length} payments
		</div>
	{/if}
</div>
