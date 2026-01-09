<script lang="ts">
	import { onMount } from 'svelte';
	import { invoiceApi } from '$lib/api';
	import { LoadingSpinner, ErrorAlert } from '$lib/components';
	import type { Invoice, InvoiceStatus } from '$lib/types';

	let invoices = $state<Invoice[]>([]);
	let loading = $state(true);
	let error = $state<string | null>(null);

	// Filters
	let searchQuery = $state('');
	let statusFilter = $state<InvoiceStatus | ''>('');

	// Sorting
	let sortColumn = $state<keyof Invoice>('createdAt');
	let sortDirection = $state<'asc' | 'desc'>('desc');

	const statuses: InvoiceStatus[] = ['Pending', 'Paid', 'Overdue', 'Cancelled'];

	onMount(async () => {
		await loadInvoices();
	});

	async function loadInvoices() {
		loading = true;
		error = null;
		try {
			invoices = await invoiceApi.getAll();
		} catch (e) {
			error = e instanceof Error ? e.message : 'Failed to load invoices';
		} finally {
			loading = false;
		}
	}

	function toggleSort(column: keyof Invoice) {
		if (sortColumn === column) {
			sortDirection = sortDirection === 'asc' ? 'desc' : 'asc';
		} else {
			sortColumn = column;
			sortDirection = 'asc';
		}
	}

	function getSortIcon(column: keyof Invoice): string {
		if (sortColumn !== column) return '↕';
		return sortDirection === 'asc' ? '↑' : '↓';
	}

	const filteredInvoices = $derived.by(() => {
		let result = invoices;

		// Filter by status
		if (statusFilter) {
			result = result.filter((inv) => inv.status === statusFilter);
		}

		// Search filter
		if (searchQuery.trim()) {
			const query = searchQuery.toLowerCase();
			result = result.filter(
				(inv) =>
					inv.invoiceNumber.toLowerCase().includes(query) ||
					inv.status.toLowerCase().includes(query)
			);
		}

		// Sort
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

	function formatDate(dateStr: string): string {
		return new Date(dateStr).toLocaleDateString();
	}

	function formatCurrency(amount: number): string {
		return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);
	}

	function getStatusClass(status: InvoiceStatus): string {
		switch (status) {
			case 'Paid':
				return 'bg-green-100 text-green-800';
			case 'Pending':
				return 'bg-yellow-100 text-yellow-800';
			case 'Overdue':
				return 'bg-red-100 text-red-800';
			case 'Cancelled':
				return 'bg-gray-100 text-gray-800';
			default:
				return 'bg-gray-100 text-gray-800';
		}
	}
</script>

<div class="p-6">
	<div class="mb-6 flex items-center justify-between">
		<h1 class="text-2xl font-bold text-gray-900">Invoices</h1>
		<a
			href="/invoices/new"
			class="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
		>
			Create Invoice
		</a>
	</div>

	<!-- Filters -->
	<div class="mb-4 flex flex-wrap gap-4">
		<input
			type="text"
			placeholder="Search invoices..."
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
	</div>

	{#if loading}
		<LoadingSpinner message="Loading invoices..." />
	{:else if error}
		<ErrorAlert message={error} onRetry={loadInvoices} />
	{:else if filteredInvoices.length === 0}
		<div class="py-12 text-center text-gray-500">
			{invoices.length === 0 ? 'No invoices found' : 'No invoices match your filters'}
		</div>
	{:else}
		<div class="overflow-x-auto rounded-lg border border-gray-200">
			<table class="min-w-full divide-y divide-gray-200">
				<thead class="bg-gray-50">
					<tr>
						<th
							class="cursor-pointer px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 hover:bg-gray-100"
							onclick={() => toggleSort('invoiceNumber')}
						>
							Invoice # {getSortIcon('invoiceNumber')}
						</th>
						<th
							class="cursor-pointer px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 hover:bg-gray-100"
							onclick={() => toggleSort('amount')}
						>
							Amount {getSortIcon('amount')}
						</th>
						<th
							class="cursor-pointer px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 hover:bg-gray-100"
							onclick={() => toggleSort('dueDate')}
						>
							Due Date {getSortIcon('dueDate')}
						</th>
						<th
							class="cursor-pointer px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 hover:bg-gray-100"
							onclick={() => toggleSort('status')}
						>
							Status {getSortIcon('status')}
						</th>
						<th
							class="cursor-pointer px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 hover:bg-gray-100"
							onclick={() => toggleSort('createdAt')}
						>
							Created {getSortIcon('createdAt')}
						</th>
						<th class="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500">
							Actions
						</th>
					</tr>
				</thead>
				<tbody class="divide-y divide-gray-200 bg-white">
					{#each filteredInvoices as invoice (invoice.id)}
						<tr class="hover:bg-gray-50">
							<td class="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">
								<a href="/invoices/{invoice.id}" class="text-blue-600 hover:underline">
									{invoice.invoiceNumber}
								</a>
							</td>
							<td class="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
								{formatCurrency(invoice.amount)}
							</td>
							<td class="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
								{formatDate(invoice.dueDate)}
							</td>
							<td class="whitespace-nowrap px-6 py-4 text-sm">
								<span
									class="inline-flex rounded-full px-2 text-xs font-semibold leading-5 {getStatusClass(invoice.status)}"
								>
									{invoice.status}
								</span>
							</td>
							<td class="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
								{formatDate(invoice.createdAt)}
							</td>
							<td class="whitespace-nowrap px-6 py-4 text-right text-sm">
								<a
									href="/invoices/{invoice.id}/edit"
									class="text-blue-600 hover:text-blue-900 hover:underline"
								>
									Edit
								</a>
							</td>
						</tr>
					{/each}
				</tbody>
			</table>
		</div>

		<div class="mt-4 text-sm text-gray-500">
			Showing {filteredInvoices.length} of {invoices.length} invoices
		</div>
	{/if}
</div>
